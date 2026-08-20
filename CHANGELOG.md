# Changelog

All notable changes to the **ChangeMe** NuGet template package (`dotnet new changeme`) are documented here.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/), and this project follows [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [4.0.0] - 2026-08-20

### Changed

- **Restore ChangeMe template tokens** — NuGet package `ChangeMe`; `dotnet new changeme`; `sourceName` is `ChangeMe` again (folders/namespaces `ChangeMe.*`).
- **Database schema** — EF migrations default schema `changeme_backend` (derived from `ChangeMe.Backend.Infrastructure` assembly naming).

### Migration notes for consumers upgrading from 3.x

1. **Install** — `dotnet new uninstall Laczynski.Template` (if installed), then `dotnet new install ChangeMe`.
2. **Scaffold** — `dotnet new changeme -n YourApp -o YourApp` (replaces `dotnet new laczynski-fullstack`).
3. **Merge** — if syncing an existing app generated from 3.x, rename `YourApp.*` projects that used the `Template` token back to your app name manually; JWT issuer/audience and schema names follow the new project name.

## [3.0.0] - 2026-08-10

### Changed

- **Rebrand to Laczynski ecosystem** — NuGet package `Laczynski.Template`; `dotnet new laczynski-fullstack`; template `sourceName` was `Template` (folders/namespaces `Template.*`).
- **DataGrid** — migrated from QueryGrid / `@query-grid/*` to `Laczynski.DataGrid.*` and `@laczynski/datagrid` / `@laczynski/datagrid-primeng` (`0.1.0-preview.19`); PrimeNG grid selectors `dg-prime-data-grid`, `dgColumn`, `dgEmpty`.
- **Repository** — `https://github.com/Laczynski/Template`; JWT sample issuer/audience used `Template`.
- **Database schema** — EF migrations default schema `changeme_backend` (aligned with `ChangeMe.Backend.Infrastructure` assembly naming).

### Migration notes for consumers upgrading from 2.x

1. **Install** — `dotnet new uninstall ChangeMe` (if installed), then `dotnet new install ChangeMe`.
2. **Scaffold** — `dotnet new changeme -n YourApp -o YourApp` (replaces `dotnet new changeme`).
3. **DataGrid** — update NuGet to `Laczynski.DataGrid.*` and npm to `@laczynski/datagrid` / `@laczynski/datagrid-primeng`; rename `qg-*` selectors to `dg-*`.
4. **Merge** — if syncing an existing generated app, rename `ChangeMe.*` projects/namespaces to your app name and update package references manually.

## [2.2.0] - 2026-07-20

### Added

- **DataGrid** — shared list/search/sort/pagination for issues, users, and roles (`@laczynski/datagrid`, `@laczynski/datagrid-primeng`).
- **Production-safe ops defaults** — `SwaggerOptions:Enabled` and `HangfireOptions:DashboardEnabled` default to `false`; `HangfireOptions:ServerEnabled` gates background job processing per host.
- **Auth integration tests** — refresh, logout, sessions, revoke, logout-all, and my-account flows.
- **CI quality gates** — ESLint, Prettier, and `dotnet format --verify-no-changes` on every PR and push to `main`.
- **Dependabot** — weekly updates for npm (root and frontend), NuGet, and GitHub Actions.

### Changed

- **Requirements docs** — five-layer structure under `docs/requirements/_shared/`; regenerated index and validation manifest.
- **Code style** — `dotnet format` enforced in CI; `.editorconfig` UTF-8 without BOM; unused usings cleanup across the backend.
- **Hangfire recurring jobs** — register via `IRecurringJobManager` (DI) so jobs work when the dashboard middleware is disabled.

### Fixed

- **Integration tests** — fixture startup after Swagger/Hangfire gating (static `RecurringJob` required dashboard initialization).

### Migration notes for consumers upgrading from 2.1.x

If you generated a project from **2.1.x** and merge template updates (not an automatic upgrade):

1. **Swagger / Hangfire** — review `SwaggerOptions` and `HangfireOptions` in `appsettings.json`. Enable dashboard and Swagger in `appsettings.Development.json` for local dev; keep them disabled in production unless intentionally exposed.
2. **Multi-instance API** — set `HangfireOptions:ServerEnabled` to `true` on exactly one host (or dedicated workers); API replicas can set it to `false`.
3. **Lists** — adopt DataGrid patterns when extending issues, users, or roles list screens.
4. **CI / format** — run `npm run format:check:all` and `npm run lint:frontend` before pushing if you sync CI workflows from the template.

Fresh installs: `dotnet new install ChangeMe --force`, then `dotnet new changeme -n YourApp -o YourApp`.

## [2.1.1] - 2026-07-19

### Changed

- CI: tag `v*` triggers NuGet publish to nuget.org and GitHub Packages via trusted publishing (OIDC)

## [2.1.0] - 2026-06-24

### Added

- **Optimistic concurrency** — `Entity.Version` on persisted roots; update commands and detail DTOs carry `version`; HTTP 409 on stale writes; EF migration `AddEntityVersionConcurrency`.
- **API rate limiting** — per-IP fixed-window limits (`RateLimitingOptions`); stricter auth policy on login and refresh; 429 with `Retry-After`; `/health` bypass; disabled in Development by default.
- **Security headers** — backend middleware (X-Frame-Options, X-Content-Type-Options, Referrer-Policy, Permissions-Policy, COOP/CORP); nginx CSP with per-request nonce; dynamic `connect-src` from `CHANGE_ME_API_URL` in the frontend Docker entrypoint.
- **Integration tests** — login rate limit and issue update concurrency scenarios.

### Changed

- **Frontend API client** — maps HTTP 409 conflict responses for stale-version handling.
- **Issue, role, user, and my-account edits** — send `version` on update; detail views expose it for round-trips.
- **Docker images** — non-root frontend container user; Dockerfile build improvements.
- **Validation and logging** — refactored error handling and `LoggingBehavior`.

### Fixed

- **Confirmation dialog** — message rendering in the shared confirmation component.

### Migration notes for consumers upgrading from 2.0.0

If you generated a project from **2.0.0** and want to align with **2.1.0** patterns (not an automatic upgrade):

1. **Database** — apply EF migration `AddEntityVersionConcurrency` (`npm run ef:database:update`).
2. **API clients** — include `version` from detail DTOs in PUT bodies for issues, roles, users, and my-account updates.
3. **Rate limiting** — review `RateLimitingOptions` for production (`Enabled` is `true` in `appsettings.json`; tune limits as needed). Forward `X-Forwarded-For` at the reverse proxy so limits apply to clients.
4. **Split-host Docker** — set `CHANGE_ME_API_URL` to the full API base URL so CSP `connect-src` allows browser calls to the API origin.

Fresh installs: `dotnet new install ChangeMe --force`, then `dotnet new changeme -n YourApp -o YourApp`.

## [2.0.0] - 2026-06-21

### Added

- **API versioning** — HTTP endpoints under `/api/v1`; Swagger and frontend updated accordingly.
- **Same-origin API proxy (Docker / production)** — nginx proxies `/api/` and `/hubs/`; `runtime-config.js` written at container start from `CHANGE_ME_API_URL` (default `/api/v1`); `docker-entrypoint.sh` for the frontend image.
- **Root `package.json`** — unified scripts for install, start, build, lint/format, test, EF Core, Docker Compose, E2E, data generation, and requirements validation.
- **GitHub Actions CI** — parallel jobs for requirements, frontend, backend, and Playwright E2E.
- **Playwright E2E** — smoke suite under `src/ChangeMe.Frontend/e2e/`; `npm run test:e2e` / `test:e2e:ui`; `npm run install:frontend` installs Chromium.
- **Roles and permissions (RBAC)** — permission catalog, role CRUD, role–user assignments, effective-permissions preview, and permission-gated issue operations.
- **Users administration** — user list, create/edit, details, deactivate/activate, and admin session management.
- **Session auth** — refresh tokens, my-account profile, session list, per-session revoke, and logout-all (replacing single long-lived JWT from 1.0.0).
- **Password policy** — server-side validation and frontend helpers.
- **Initial administrator seed** — bootstrap admin user on first empty database.
- **Issue file attachments** — upload, download, and delete with local file storage and validation.
- **Issue details tabs** — separate comments, change history, and attachments tabs.
- **Data generator** — `npm run data:generate` for Development demo data (`docs/modules/backend/demo-data.md`).
- **Template symbols** — derived `changeMe` (camelCase) and `CHANGE_ME` (SCREAMING_SNAKE) from the `-n` project name.
- **Documentation split** — product requirements plus system- and module-owned documentation with validation via `npm run docs:validate`.
- **Generated-project readme** — consumer-focused root `README.md` from `template-content/generated-readme/` (maintainer readme stays on GitHub only).
- **Frontend UI stack** — PrimeNG + Tailwind CSS; shared applied-filters chips and back-button components.

### Changed

- **Authentication** — admin-provisioned users instead of public self-registration; login returns refreshable sessions instead of a single access token.
- **Notifications UI** — notifications center replaced with header bell and dropdown panel (SignalR push for new notifications retained).
- **Mediator** — MediatR replaced with the Mediator source generator across UseCases and tests.
- **Solution format** — backend solution file is `ChangeMe.Backend.slnx` (was `.sln`).
- **Database migrations** — consolidated `InitialCreate` for the current PostgreSQL schema (roles, sessions, attachments, and related tables).
- **Configuration sections** — options classes bound with consistent `nameof` section names.
- **Docker Compose** — frontend service sets `CHANGE_ME_API_URL`; nginx serves SPA and proxies API/SignalR on one origin.
- **Maintainer docs** — `CONTRIBUTING.md` and this changelog excluded from `dotnet new` output.

### Removed

- **Public self-registration** — `Register` endpoint and UI; users are created by administrators.
- **Live issues-list refresh via SignalR** — `IssueRealtimeService` and list auto-reload on issue events (notification bell still uses SignalR).

### Migration notes for consumers upgrading from 1.0.0

If you generated a project from **1.0.0** and want to align with **2.0.0** patterns (not an automatic upgrade):

1. **API clients** — prefix routes with `/api/v1`.
2. **Auth** — replace public registration with admin user creation; adopt refresh/session endpoints if you extend auth.
3. **Production frontend** — serve SPA and API from one host with nginx proxy, or set `CHANGE_ME_API_URL` and `CorsOptions:AllowedOrigins` for split-host deployment (`docs/system/operations/deployment.md`).
4. **Scripts** — use root `package.json` for common tasks; run `npm run install:frontend` once before E2E.
5. **Permissions** — gate new endpoints and UI with the permission catalog pattern if you add features.

Fresh installs: `dotnet new install ChangeMe --force`, then `dotnet new changeme -n YourApp -o YourApp`.

## [1.0.0] - 2026-05-02

### Added

- Initial NuGet template package **`ChangeMe`** (`dotnet new changeme`).
- Full-stack starter: Angular frontend, layered ASP.NET backend (Web → UseCases → Domain → Infrastructure).
- Email/password login and public self-registration.
- Issue listing, details, comments, history, acceptance criteria, watch, and in-app notifications with SignalR.
- PostgreSQL, Docker Compose, MailHog, and Hangfire.
- Backend unit and integration tests (Testcontainers).
- Template token replacement for `ChangeMe` across solution, Docker, and docs.

[4.0.0]: https://github.com/Laczynski/Template/compare/v3.0.0...v4.0.0
[3.0.0]: https://github.com/Laczynski/Template/compare/v2.2.0...v3.0.0
[2.2.0]: https://github.com/Laczynski/Template/compare/v2.1.1...v2.2.0
[2.1.1]: https://github.com/Laczynski/Template/compare/v2.1.0...v2.1.1
[2.1.0]: https://github.com/Laczynski/Template/compare/v2.0.0...v2.1.0
[2.0.0]: https://github.com/Laczynski/Template/compare/v1.0.0...v2.0.0
[1.0.0]: https://github.com/Laczynski/Template/releases/tag/v1.0.0
