# Deployment

> Type: operations
> Scope: system
> Status: implemented
> Canonical for: runtime API routing and the production deployment checklist
>
> Local Compose: [local full-stack environment](local-stack.md). Migrations: [backend persistence](../../modules/backend/operations/persistence.md).

## How the frontend reaches the API

| Mode                          | Where `apiUrl` is defined      | Typical use                                                                                |
| ----------------------------- | ------------------------------ | ------------------------------------------------------------------------------------------ |
| **`ng serve`** (Development)  | `environment.development.ts`   | Backend on port 5000, frontend on 4200                                                     |
| **Production build / Docker** | `runtime-config.js` (required) | Default `/api/v1` — nginx proxies `/api/` and `/hubs/` to the backend                      |
| **Split API host**            | `CHANGE_ME_API_URL` env var    | SPA and API on different origins — see [Runtime config](#runtime-config) and [CORS](#cors) |

SignalR hub URL is derived from `apiUrl`. In Docker, nginx proxies `/hubs/` to the backend with WebSocket headers.

Development uses `environment.development.ts` only. Production reads **`window.__CHANGE_ME_CONFIG__.apiUrl`** from `public/runtime-config.js` (loaded before the Angular bundle) — there is no silent fallback to build-time environment files.

## Default deployment topology

The Compose reference topology serves the Angular build through nginx and proxies `/api/` and `/hubs/` to `backend:8080`. The browser therefore uses one public origin. Commands, ports, services, and local configuration precedence are canonical in [local full-stack environment](local-stack.md).

## Runtime config

`public/runtime-config.js` defines the production API URL before bootstrap:

```javascript
window.__CHANGE_ME_CONFIG__ = {
  apiUrl: "/api/v1",
};
```

The frontend Docker entrypoint **always** writes this file from **`CHANGE_ME_API_URL`** (default `/api/v1` when the variable is unset). `docker-compose.yml` sets it explicitly for the default stack.

Split API host — change the env var:

```yaml
frontend:
  environment:
    - CHANGE_ME_API_URL=https://api.example.com/api/v1
```

Do **not** commit real production URLs or secrets in tracked files — set `CHANGE_ME_API_URL` in your orchestrator or secret store.

## Production checklist

### Secrets and configuration

- Supply a unique **`AuthOptions:Jwt:SigningKey`** of at least 32 bytes; no signing key is shipped in tracked settings.
- Set **`ConnectionStrings:DefaultConnection`** for your PostgreSQL instance.
- Configure **`EmailOptions`** for real SMTP (MailHog is for local dev only).
- Set all **`InitialAdministratorOptions`** fields only for first bootstrap, then remove or empty the entire section.
- Keep **`RateLimitingOptions:Enabled`** `true` in production (see [Rate limiting](#rate-limiting)); tune `AuthPermitLimit` and `ApiPermitLimit` for your traffic.

See [local full-stack environment](local-stack.md) for Compose overrides and sensitive local values.

### Protected VPS environment file

Production uses standard ASP.NET Core environment variables; the application does not load a repository `.env` outside `Development`. On a Compose-based Linux VPS, keep the production file outside the checkout, for example `/etc/template/backend.env`, owned by the deployment account with mode `0600`:

```bash
sudo install -o template -g template -m 600 /secure-transfer/backend.env /etc/template/backend.env
```

A minimal production-shaped file contains the values absent from tracked settings:

```dotenv
ConnectionStrings__DefaultConnection=Host=postgres;Database=Template;Username=template;Password=replace-on-vps
AuthOptions__Jwt__SigningKey=replace-with-a-unique-production-signing-key
EmailOptions__Host=smtp.example.com
EmailOptions__Port=587
EmailOptions__EnableSsl=true
EmailOptions__Username=template
EmailOptions__Password=replace-when-smtp-authentication-is-used
EmailOptions__FromEmail=no-reply@example.com
EmailOptions__FromName=Template
```

Interpolate only the variables required by the backend container in the production Compose file or override:

```yaml
services:
  backend:
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      ConnectionStrings__DefaultConnection: ${ConnectionStrings__DefaultConnection:?Default connection string is required}
      AuthOptions__Jwt__SigningKey: ${AuthOptions__Jwt__SigningKey:?JWT signing key is required}
      EmailOptions__Host: ${EmailOptions__Host:?SMTP host is required}
      EmailOptions__Port: ${EmailOptions__Port:?SMTP port is required}
      EmailOptions__EnableSsl: ${EmailOptions__EnableSsl:-true}
      EmailOptions__Username: ${EmailOptions__Username:-}
      EmailOptions__Password: ${EmailOptions__Password:-}
      EmailOptions__FromEmail: ${EmailOptions__FromEmail:?Sender email is required}
      EmailOptions__FromName: ${EmailOptions__FromName:-Template}
```

Validate structure without emitting resolved values, then recreate the affected container after every configuration change:

```bash
docker compose --env-file /etc/template/backend.env config --quiet
docker compose --env-file /etc/template/backend.env up -d --force-recreate backend
```

Do not print the file or resolved `docker compose config` in CI/CD logs. Transfer/update it through a masked channel, keep it out of source control and image layers, and restrict backup access like any other credential store.

For a backend running directly under systemd, use the same variable names:

```ini
[Service]
EnvironmentFile=/etc/template/backend.env
ExecStart=/opt/template/dotnet/Template.Backend.Web.dll
```

After updating the file, run `systemctl daemon-reload` when the unit changed and restart the backend service. Verify startup and `/health` without logging environment contents.

### Migrations

- **`InitialCreate`** is included — apply with `npm run ef:database:update` or your pipeline (`dotnet ef database update`); see [backend persistence](../../modules/backend/operations/persistence.md).
- Prefer applying migrations from **CI/CD** rather than `Database:ApplyMigrationsOnStartup` on many concurrent app instances.

### CORS

Required only when the browser talks to the API on a **different origin** than the SPA (split host with `CHANGE_ME_API_URL`).

Set **`CorsOptions:AllowedOrigins`** in `appsettings.json` or environment variables to your frontend origin(s), for example:

```json
"CorsOptions": {
  "AllowedOrigins": ["https://app.example.com"]
}
```

Same-origin Docker Compose (default) does not need CORS changes for browser API calls through nginx.

### Hangfire

- Keep **`HangfireOptions:DashboardEnabled`** `false` in production (`appsettings.json` default). Enable only in Development or staging when you need the dashboard.
- On multi-instance deployments, keep **`HangfireOptions:ServerEnabled`** `true` on at least one instance (job worker) and `false` on stateless HTTP replicas.
- Restrict **`/hangfire`** when enabled (reverse proxy auth, network policy, or Hangfire authorization filters). The template ships without dashboard authentication.
- Recurring jobs still register on every instance (`RecurringJob.AddOrUpdate` at startup); only hosts with `ServerEnabled: true` execute them.

Details: [backend background jobs](../../modules/backend/operations/background-jobs.md).

### Swagger / OpenAPI

- Keep **`SwaggerOptions:Enabled`** `false` in production (`appsettings.json` default). Enable in Development (`appsettings.Development.json`) or via environment override for staging.
- When enabled locally, Swagger UI is available at `/swagger` on the API host.

### Rate limiting

- **Production:** per-IP fixed-window limits on all API traffic; login and refresh use a stricter auth limit on top. Exceeded requests return **429** with **`Retry-After`**. **`/health`** is excluded from the global limit.
- **Development / default Compose:** off (`RateLimitingOptions:Enabled: false` in `appsettings.Development.json`).
- **Deploy:** keep `Enabled` true; tune `AuthPermitLimit` and `ApiPermitLimit` via `RateLimitingOptions` in `appsettings.json` or `RateLimitingOptions__*` environment variables. Defaults and option names: `appsettings.json`, `RateLimitingOptions.cs`, `RateLimitingConfig.cs`.
- Forward **`X-Forwarded-For`** at the reverse proxy (see [TLS and reverse proxy](#tls-and-reverse-proxy)) so limits apply to clients, not the load balancer.

### TLS and reverse proxy

Terminate HTTPS at your load balancer or ingress. Forward `X-Forwarded-Proto` and `X-Forwarded-For` so the API generates correct links when needed.

For same-origin deployment, proxy **`/api/`**, **`/hubs/`**, and static SPA assets from one public host — the template’s `nginx.conf` is the reference for `/api` and `/hubs`.

### Observability

- Serilog writes to console and rolling files under `logs/` by default — redirect to your log aggregator in production.
- Health checks: backend exposes standard ASP.NET Core health endpoints configured in the Web project.

## Related docs

| Topic                                     | Document                                                             |
| ----------------------------------------- | -------------------------------------------------------------------- |
| Local Compose | [local full-stack environment](local-stack.md) |
| PostgreSQL and migrations | [backend persistence](../../modules/backend/operations/persistence.md) |
| Hangfire | [backend background jobs](../../modules/backend/operations/background-jobs.md) |
| File storage | [backend file storage](../../modules/backend/operations/file-storage.md) |
| CI pipeline | [continuous integration](ci.md) |
| Frontend implementation | [frontend development](../../modules/frontend/development.md) |
