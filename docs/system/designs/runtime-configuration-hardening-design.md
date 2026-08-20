# Runtime configuration hardening

> Type: design
> Scope: system
> Status: implemented
> Canonical for: rationale behind startup validation and environment-variable delivery for local and VPS backend configuration

## Goal

Make configuration errors fail early with actionable messages and keep locally editable configuration in one ignored `.env` file shared by Docker Compose and .NET tooling. Use standard environment variables loaded by systemd as the sufficient production configuration mechanism for a native Linux VPS, without requiring containers, a secret manager, or mounted-secret integration before the deployment has such a need.

## Target

### Backend startup validation

Register backend option sections through the .NET options builder, bind them from configuration, add section-specific validation, and call `ValidateOnStart()`. Invalid explicit values should stop application startup instead of being silently replaced with defaults or failing during the first request or background job.

The initial validation scope should be:

| Section | Candidate rules |
| --- | --- |
| `ConnectionStrings:DefaultConnection` | Required and parseable as a PostgreSQL connection string |
| `AuthOptions` | Non-empty issuer and audience; sufficiently long signing key; positive token and session lifetimes; valid frontend absolute URL; coherent password length limits |
| `EmailOptions` | Non-empty host and sender; port in `1-65535`; valid sender email; credentials may remain empty when the SMTP server permits anonymous access |
| `RateLimitingOptions` | Positive permit limits and window lengths, including when limiting is temporarily disabled |
| `FileStorageOptions` | Non-empty root path; valid cleanup cron; positive concurrent-execution timeout |
| `NotificationRetentionOptions` | Positive retention periods with coherent ordering; valid cleanup cron |
| `HangfireOptions` | Dashboard path starts with `/`; at least one deployed host must enable the server, verified operationally rather than by a single host |
| `CorsOptions` | Every configured origin is an absolute HTTP or HTTPS origin without a path |
| `InitialAdministratorOptions` | Either all bootstrap fields are empty, or all are present and valid |

Small validators may use inline `Validate(...)` rules. Sections with multiple or conditional rules should use dedicated `IValidateOptions<T>` implementations so error messages name the invalid section and property.

Configuration validation checks shape and consistency only. PostgreSQL, SMTP, and filesystem reachability remain operational checks; startup connection attempts and health checks should not be replaced with static option validation.

### Local configuration and secrets

Add a tracked root `.env.example` and ignore the root `.env`. The ignored `.env` becomes the only standard source of locally editable values for Docker Compose, direct `dotnet run`, IDE launches, EF Core tooling, and the demo-data generator. Local .NET development no longer requires a second copy in .NET User Secrets.

Use the `DotNetEnv` package to load the root `.env` before .NET builds its normal configuration. Load it only for local Development, traverse parent directories so commands may start from the repository or a backend project, and use `NoClobber` behavior so variables already supplied by the shell, Docker, CI, or an IDE override values from the file. The standard ASP.NET Core environment-variable provider then maps `__` to configuration section separators.

Keep this bootstrap in one infrastructure helper and call it before configuration is built by every local entry point:

- the backend API host;
- the EF Core design-time `ApplicationDbContext` factory;
- the demo-data generator host.

The root `.env` may define shared PostgreSQL primitives once and derive the host-run connection string through interpolation:

```dotenv
POSTGRES_DB=Template
POSTGRES_USER=postgres
POSTGRES_PASSWORD=replace-for-local-development

ConnectionStrings__DefaultConnection="Host=localhost;Database=${POSTGRES_DB};Username=${POSTGRES_USER};Password=${POSTGRES_PASSWORD}"

AuthOptions__Jwt__SigningKey=replace-with-at-least-32-random-characters
InitialAdministratorOptions__Email=admin@example.local
InitialAdministratorOptions__Password=replace-for-local-development
```

Docker Compose uses the same root `.env` for explicit `${VARIABLE}` interpolation. It constructs the backend connection string with `Host=postgres` from the same database name, user, and password, and passes only the settings required by each container. Sensitive required variables should use required interpolation such as `${POSTGRES_PASSWORD:?POSTGRES_PASSWORD is required}` rather than silent defaults. Do not use `env_file` to inject every local variable into every container.

Candidate values for local `.env` ownership:

- PostgreSQL database, user, and password;
- JWT signing key;
- initial administrator credentials;
- SMTP username and password when authentication is used.

Mode-specific, non-sensitive topology remains tracked and is not duplicated in `.env`:

| Concern | Local .NET | Docker Compose |
| --- | --- | --- |
| PostgreSQL host | `localhost` in the derived local connection string | `postgres` in the Compose backend override |
| SMTP host | `localhost` in Development settings | `mailhog` in Compose |
| File storage root | Local Development path | `/app/storage` in Compose |
| Service names, ports, mounts, and browser API path | Relevant tracked Development settings | `docker-compose.yml` |

The resulting local configuration flow is:

| Runtime or tool | Source | Adapter |
| --- | --- | --- |
| Docker Compose | Ignored root `.env`, based on `.env.example` | Compose interpolates explicitly referenced variables |
| Local `dotnet run` and IDE launch | The same ignored root `.env` | `DotNetEnv` loads values before ASP.NET Core configuration |
| EF Core commands | The same ignored root `.env` | The design-time factory invokes the shared loader |
| Demo-data generator | The same ignored root `.env` | The generator host invokes the shared loader |

The `.env` file is a developer convenience, not encrypted storage. Its values can still be read from the filesystem, the process or container environment, and Docker inspection output. It must not be copied into backend or frontend images.

### Linux VPS deployment configuration

Standard environment variables are the initial production delivery mechanism. ASP.NET Core already reads them and maps `__` to the `:` section separator, so the application needs no production `.env` provider or platform-specific configuration integration.

Ansible renders versioned non-secret variables into `/etc/<application>/current-config/backend.config.env`. Keep sensitive variables in the untracked server-owned `/etc/<application>/secrets.env`:

```dotenv
ConnectionStrings__DefaultConnection=Host=database.internal;Database=Template;Username=template;Password=replace-on-vps
AuthOptions__Jwt__SigningKey=replace-with-a-production-signing-key
EmailOptions__Password=replace-when-smtp-authentication-is-used
```

The file must:

- remain outside source control, application packages, and release directories;
- be owned by `root` and readable only by `root`, normally mode `0600`;
- be provisioned or updated through a controlled server-administration process without printing its contents in CI/CD logs;
- never be copied, replaced, or displayed by the application deployment pipeline;
- cause the backend service to be restarted and health-checked after a value changes.

The native systemd service loads the protected file:

```ini
[Service]
WorkingDirectory=/opt/<application>/current/backend
ExecStart=/opt/<application>/current/backend/ChangeMe.Backend.Web
Environment=ASPNETCORE_ENVIRONMENT=Production
EnvironmentFile=/etc/<application>/current-config/backend.config.env
EnvironmentFile=/etc/<application>/secrets.env
Restart=always
```

The deployment pipeline creates and activates the non-secret revision but never reads, copies, or replaces `secrets.env`. The exact administrative mechanism that creates or updates the secret file is deployment-specific. A secret manager, automated rotation, and Kubernetes-specific delivery may be introduced later if scale, platform capabilities, or security requirements justify them. Kubernetes can use the same environment-variable contract through `ConfigMap` and `Secret` references without changing the backend.

## Implemented migration

1. Added the centrally versioned `DotNetEnv` dependency and a shared Development-only, no-clobber local environment loader.
2. Invoked the loader from the API host, EF Core design-time factory, and demo-data generator before each builds configuration.
3. Added `.env.example`, ignored `.env`, protected Docker/template artifacts, and updated Compose to interpolate only selected values.
4. Removed JWT signing keys, database passwords, SMTP passwords, and initial-administrator passwords from tracked development settings; retired User Secrets as the standard local workflow.
5. Added section-specific validators and startup-validation tests, then removed silent fallback behavior for invalid configured values.
6. Updated [local stack operations](../operations/local-stack.md) with the implemented workflow and precedence.
7. Added versioned non-secret Ansible configuration and a separate protected VPS secret file, with bootstrap, activation, validation, and restart procedures in [deployment](../operations/deployment.md).

## Alternatives considered

| Alternative | Trade-off |
| --- | --- |
| Keep fixed development credentials in tracked files | Preserves zero-setup startup, but normalizes committed credentials and makes accidental reuse more likely |
| Give Compose substitutions defaults such as `${POSTGRES_PASSWORD:-postgres}` | Keeps onboarding simple, but missing local configuration no longer fails fast and moving the value to `.env` provides little benefit |
| Use `env_file` to inject the complete `.env` | Shorter Compose configuration, but exposes unrelated variables to containers and hides the effective configuration contract |
| Use `.env` for Compose and .NET User Secrets for host runs | Uses built-in .NET behavior, but makes developers maintain the same values in two local stores that can drift |
| Wrap all .NET commands in an npm or shell launcher | Keeps `.env` handling outside application code, but direct `dotnet run`, IDE launches, and design-time tooling require separate adapters or documented exceptions |
| Load `.env` as an `IConfiguration` source after creating the builder | Avoids modifying process variables, but provider ordering must be rebuilt carefully so real environment variables and command-line arguments retain precedence |
| Require mounted secrets or an external secret manager for the first VPS deployment | Reduces some environment-variable exposure and may improve centralized auditing, but adds infrastructure and operational work before the current deployment requires it |
| Validate options only when first resolved | Requires less startup work, but leaves failures dependent on which endpoint or job happens to resolve an option first |

## Completion criteria

- Invalid values in every selected option section prevent backend startup and identify the failing property.
- Valid development, integration-test, and production-shaped configurations still build the service provider successfully.
- One local `.env` supplies the editable values for Docker Compose, direct `dotnet run`, IDE launches, EF Core commands, and the demo-data generator.
- Real process or container environment variables take precedence over `.env` values.
- Docker Compose starts from a documented `.env.example` workflow, passes only explicitly selected values, and fails clearly when a required sensitive value is absent.
- No real or reusable JWT key, database password, SMTP password, or initial administrator password is committed.
- Local and VPS environment files are absent from source control, application packages, and CI artifacts.
- A production-shaped VPS configuration reaches ASP.NET Core through environment variables and passes startup validation.
- VPS documentation covers file ownership, restrictive permissions, non-logging, and systemd service restart after changes.
- Local API startup, an EF Core design-time command, and demo-data generation are verified against the same `.env` fixture.
- `docker compose config --quiet`, backend tests, and `npm run docs:validate` pass.

## Related documents

- [Backend development](../../modules/backend/development.md)
- [Local full-stack environment](../operations/local-stack.md)
- [Deployment](../operations/deployment.md)
