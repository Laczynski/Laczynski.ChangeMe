# AI Working Guide

> Scope: fast-start context for AI agents and contributors in **this solution**. Load this file first, then open the focused docs under `docs/` for the area you are changing.

## Repository shape

- `src/ChangeMe.Frontend` - Angular 22 frontend.
- `src/ChangeMe.Backend` - .NET backend solution.
- `docker-compose.yml` - local full-stack environment with frontend, backend, PostgreSQL, and MailHog.
- `docs/` - guides, technical, and requirements (`docs/README.md` for the full index).
- `docs/guides/README.md`, `docs/technical/README.md`, `docs/requirements/requirements-change-process.md` - entry points per area.
- Root `package.json` - optional npm scripts that run frontend and backend tasks from the repository root (see Commands).

## Start here by task

- Frontend change: read `docs/guides/README.md`, then `repo-map.md` and `frontend-guidelines.md` under `docs/guides/`.
- Backend change: read `docs/guides/README.md`, then `repo-map.md` and `backend-guidelines.md` under `docs/guides/`.
- Test work or bugfix verification: read `docs/guides/testing-guidelines.md`.
- Cross-stack feature: read all four docs above before editing.
- Auth deployment, Docker, CI, or local stack: read `docs/technical/README.md`, then the linked technical doc (`deployment.md` for production runtime URL and checklist).
- Requirement changes: read `docs/requirements/requirements-change-process.md`; authoring in `docs/requirements/requirements-authoring-guide.md`; **five layers** in `docs/requirements/_shared/README.md`; product behavior defaults in `docs/requirements/_shared/conventions/product-standards.md`; pending deltas in `docs/requirements/changes/`; validate with `npm run requirements:validate`.

## Commands

### Repository root (npm)

From the repository root, run `npm run setup` once after clone (installs npm packages, Playwright Chromium, restores .NET, and installs Lefthook pre-commit hooks). Frontend packages still live under `src/ChangeMe.Frontend`; use `npm run install:frontend` when only frontend dependencies change.

- First-time setup: `npm run setup`
- Install frontend dependencies and Playwright Chromium: `npm run install:frontend`
- Start dev servers: `npm run start:frontend`, `npm run start:backend`, or both in parallel with `npm run start:all`
- Build: `npm run build:frontend`, `npm run build:backend`, or `npm run build:all`
- Frontend quality: `npm run lint:frontend`, `npm run format:frontend`, `npm run test:frontend` (interactive watch when TTY), or `npm run test:frontend:ci` (single run, `--watch=false`)
- Backend tests: `npm run test:backend` (entire solution — unit and integration projects), `npm run test:backend:unit`, or `npm run test:backend:integration`
- Full automated check (frontend CI tests + full backend solution tests, parallel): `npm run test:all` — backend integration tests use Testcontainers and need a running Docker engine
- E2E (Playwright): `npm run test:e2e` (needs Chromium from `npm run install:frontend`, PostgreSQL on `localhost`; Playwright starts the stack); `npm run test:e2e:ui` for interactive debugging — also runs in CI on every PR (`docs/technical/ci.md`)
- CI workflow (GitHub Actions): see `docs/technical/ci.md`
- EF Core (from repo root): `npm run ef:migrations:add -- <Name>`, `npm run ef:migrations:remove`, `npm run ef:database:update`
- Demo data (after migrations; Development only): `npm run data:generate`, or `npm run data:generate -- --reset` — see `docs/technical/data-generator.md`
- Requirements structure: `npm run requirements:validate` — checks `FR-*` / `NFR-*` specs, cross-references, and regenerates `docs/requirements/README.md`

### Frontend (in `src/ChangeMe.Frontend`)

- Install dependencies: prefer `npm run install:frontend` from the repository root (includes Playwright Chromium); `npm install` here installs npm packages only
- Run dev server: `npm start`
- Lint: `npm run lint`
- Format: `npm run format`
- Tests: `npm test`
- E2E: `npm run e2e` (from repo root: `npm run test:e2e`, or `npm run test:e2e:ui` for Playwright UI)

### Backend (in `src/ChangeMe.Backend`)

- Includes `InitialCreate` — in Development, migrations apply on API startup (`DatabaseOptions:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`; see `docs/technical/database-and-docker.md`).
- Restore/build solution: `dotnet build ChangeMe.Backend.slnx`
- Run web app: `dotnet run --project src/ChangeMe.Backend.Web`
- All tests in the solution: `dotnet test ChangeMe.Backend.slnx`
- Unit tests only: `dotnet test tests/ChangeMe.Backend.UnitTests`
- Integration tests only: `dotnet test tests/ChangeMe.Backend.IntegrationTests`

### Full stack (Docker Compose)

- Start stack (foreground): `npm run docker:up`
- Start stack (background): `npm run docker:up:detached`
- Stop stack: `npm run docker:down`
- Stop stack and remove volumes: `npm run docker:down:volumes`
- Rebuild images only: `npm run docker:build`
- Follow logs: `npm run docker:logs`
- Backend tests in container (bind-mounts the repo; integration tests need the host Docker socket): `npm run docker:test:backend`

Configuration in containers: `appsettings.json` + `appsettings.Development.json` (image build) with overrides from `docker-compose.yml` environment variables — see `docs/technical/database-and-docker.md`.

## Repo navigation rules

### Frontend

- Routes live in `src/app/app.routes.ts`.
- Feature code lives under `src/app/features/<feature>/`.
- Shared HTTP wrapper lives in `src/app/shared/api/services/api.service.ts`.
- Cross-cutting user/session concerns live under `src/app/core/` and `features/auth/`.
- Transient toast feedback uses `src/app/core/toast/services/toast.service.ts` with global `<p-toast>` in `app.component.ts`.
- Shared data models live under `src/app/shared/`.

### Backend

- HTTP endpoints live in `src/ChangeMe.Backend.Web`.
- Query and command contracts live in `src/ChangeMe.Backend.UseCases/<Feature>/`.
- Keep only `*Query.cs` and `*Command.cs` files at the top level of each `UseCases` feature folder.
- Place feature DTOs in `src/ChangeMe.Backend.UseCases/<Feature>/Dtos/`.
- Place shared handler helpers in `src/ChangeMe.Backend.UseCases/<Feature>/Utils/` (for example `RolesUtils.cs`).
- Place feature services in `src/ChangeMe.Backend.UseCases/<Feature>/Services/`.
- Handlers still live with their matching command or query file.
- Domain rules and aggregates live in `src/ChangeMe.Backend.Domain`.
- EF Core, persistence, auth, and adapters live in `src/ChangeMe.Backend.Infrastructure`.
- Integration tests mirror API behavior under `tests/ChangeMe.Backend.IntegrationTests`.
- Unit tests cover domain/infrastructure helpers under `tests/ChangeMe.Backend.UnitTests`.

## Change coupling checklist

- If you change a backend request/response contract, check matching frontend models and services.
- If you add or change an endpoint, check validator, handler, and integration tests.
- If you change persistence shape, check EF configuration, migrations, and tests.
- If you change auth behavior, check backend auth config, frontend auth service, guards, and integration tests.
- If you change pagination or shared API result handling, check both backend shared models and frontend `ApiService`.

## Working agreements

- Follow the current code structure instead of inventing a new layer or folder layout.
- Prefer extending an existing feature slice over creating a parallel pattern.
- Keep docs current when introducing a new enforced convention.
- Do not assume files visible in the IDE are committed; verify against the filesystem first.

## Requirements layers (product analysis)

Five layers — see `docs/requirements/_shared/README.md`:

| Layer             | Path                                                         | When to read                                   |
| ----------------- | ------------------------------------------------------------ | ---------------------------------------------- |
| L1 Domain         | `docs/requirements/_shared/domain/`                          | Terms, account model, permissions              |
| L2 Conventions    | `docs/requirements/_shared/conventions/product-standards.md` | Any UI — lists, forms, validation UX, feedback |
| L3 Quality        | `docs/requirements/_shared/quality/`                         | Performance, a11y, i18n                        |
| L4 Capabilities   | `docs/requirements/functional/FR-*`                          | The feature you are implementing               |
| L5 Implementation | `docs/guides/`                                               | Code patterns in this repo                     |

**Override rule:** L4 overrides L2; L5 never defines product behavior.

**After implementing UI:** run the **Implementation review checklist** at the end of `product-standards.md` for each `STD-*` in the target `FR-*` `inherits_conventions`.
