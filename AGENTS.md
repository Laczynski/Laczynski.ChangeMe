# AI Working Guide

> Scope: fast-start context for AI agents and contributors in **this solution**. Load this file first, then open the focused docs under `docs/` for the area you are changing.

## Repository shape

- `src/Template.Frontend` - Angular 22 frontend.
- `src/Template.Backend` - .NET backend solution.
- `docker-compose.yml` - local full-stack environment with frontend, backend, PostgreSQL, and MailHog.
- `docs/` - product requirements plus system- and module-owned documentation (`docs/README.md` for the task index).
- `docs/documentation-rules.md` - ownership, naming, diagram, and no-duplication rules for documentation.
- `docs/system/` - cross-module development, designs, and operations owned by this repository.
- `docs/modules/frontend/`, `docs/modules/backend/` - module documentation designed to move with future repositories.
- Root `package.json` - optional npm scripts that run frontend and backend tasks from the repository root (see Commands).

## Start here by task

- First codebase orientation: read `docs/system/development/repository-map.md`.
- Frontend change: read `docs/modules/frontend/development.md`; for session renewal also read `docs/modules/frontend/architecture/auth-session-lifecycle.md`.
- Backend change: read `docs/modules/backend/development.md`; add the focused document under `docs/modules/backend/operations/` only when persistence, jobs, or file storage changes.
- Test work or bugfix verification: read `docs/system/development/testing-strategy.md`; read `e2e-testing.md` only for browser journeys.
- Cross-stack feature: read the target requirements, `docs/system/development/feature-workflow.md`, and the affected module development documents.
- Docker or local stack: read `docs/system/operations/local-stack.md`; production deployment: `docs/system/operations/deployment.md`; CI: `docs/system/operations/ci.md`.
- Application release: read `docs/system/operations/deployment.md` and `docs/system/operations/ci.md`; use `/release patch|minor|major` in Cursor or Claude Code to prepare the reviewed release MR, then `/release publish vX.Y.Z` after merge.
- Documentation additions or updates: read `docs/documentation-rules.md`.
- Repository extraction, documentation ownership changes, or `docs/` reorganization: also read `docs/system/designs/modular-documentation-migration.md`.
- Requirement changes: read `docs/requirements/requirements-change-process.md`; authoring in `docs/requirements/requirements-authoring-guide.md`; **five layers** in `docs/requirements/_shared/README.md`; product behavior defaults in `docs/requirements/_shared/conventions/product-standards.md`; pending deltas in `docs/requirements/changes/`; validate with `npm run docs:validate`.

## Commands

### Repository root (npm)

From the repository root, run `npm run setup` once after clone (creates `.env` from `.env.example` when missing, installs npm packages and Playwright Chromium, restores .NET, and installs Lefthook pre-commit hooks). Existing `.env` files are never overwritten. Frontend packages still live under `src/Template.Frontend`; use `npm run install:frontend` when only frontend dependencies change.

- First-time setup: `npm run setup`
- Install frontend dependencies and Playwright Chromium: `npm run install:frontend`
- Start dev servers: `npm run start:frontend`, `npm run start:backend`, or both in parallel with `npm run start:all`
- Build: `npm run build:frontend`, `npm run build:backend`, or `npm run build:all`
- Frontend quality: `npm run lint:frontend`, `npm run format:frontend`, `npm run test:frontend` (interactive watch when TTY), or `npm run test:frontend:ci` (single run, `--watch=false`)
- Backend tests: `npm run test:backend` (entire solution — unit and integration projects), `npm run test:backend:unit`, or `npm run test:backend:integration`
- Full automated check (frontend CI tests + full backend solution tests, parallel): `npm run test:all` — backend integration tests use Testcontainers and need a running Docker engine
- E2E (Playwright): `npm run test:e2e` (needs Chromium from `npm run install:frontend`, PostgreSQL on `localhost`; Playwright starts the stack); `npm run test:e2e:ui` for interactive debugging — E2E is local/manual and is not a CI or release gate (`docs/system/operations/ci.md`)
- CI workflow (GitHub Actions): see `docs/system/operations/ci.md`
- EF Core (from repo root): `npm run ef:migrations:add -- <Name>`, `npm run ef:migrations:remove`, `npm run ef:database:update`
- Demo data (after migrations; Development only): `npm run data:generate`, or `npm run data:generate -- --reset` — see `docs/modules/backend/demo-data.md`
- Documentation: `npm run docs:validate` — checks links, system/module contracts and index reachability, then validates `FR-*` / `NFR-*` cross-references and regenerates the requirements index

### Native VPS delivery (Linux/WSL)

- Set up the isolated Ansible controller and all local prerequisites: `npm run setup:deployment` (native Linux or the default WSL distribution on Windows)
- Validate inventory, generator, GitLab YAML, tests, playbooks, lint, and the package contract: `npm run validate:deployment`
- Agent-assisted release: `/release patch|minor|major` prepares release notes and an MR; `/release publish vX.Y.Z` verifies the merge and pushes the protected tag
- Setup troubleshooting, bootstrap, deploy, verify, configuration-only deploy, and rollback procedures: `docs/system/operations/deployment.md`
- GitLab creates packages and manual deployment choices; Docker and Compose remain local-only

### Frontend (in `src/Template.Frontend`)

- Install dependencies: prefer `npm run install:frontend` from the repository root (includes Playwright Chromium); `npm install` here installs npm packages only
- Run dev server: `npm start`
- Lint: `npm run lint`
- Format: `npm run format`
- Tests: `npm test`
- E2E: `npm run e2e` (from repo root: `npm run test:e2e`, or `npm run test:e2e:ui` for Playwright UI)

### Backend (in `src/Template.Backend`)

- Includes `InitialCreate` — in Development, migrations apply on API startup (`DatabaseOptions:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`; see `docs/modules/backend/operations/persistence.md`).
- Restore/build solution: `dotnet build Template.Backend.slnx`
- Run web app: `dotnet run --project src/Template.Backend.Web`
- All tests in the solution: `dotnet test Template.Backend.slnx`
- Unit tests only: `dotnet test tests/Template.Backend.UnitTests`
- Integration tests only: `dotnet test tests/Template.Backend.IntegrationTests`

### Full stack (Docker Compose)

- Start stack (foreground): `npm run docker:up`
- Start stack (background): `npm run docker:up:detached`
- Stop stack: `npm run docker:down`
- Stop stack and remove volumes: `npm run docker:down:volumes`
- Rebuild images only: `npm run docker:build`
- Follow logs: `npm run docker:logs`
- Backend tests in container (bind-mounts the repo; integration tests need the host Docker socket): `npm run docker:test:backend`

Configuration in containers: `appsettings.json` + `appsettings.Development.json` (image build) with overrides from `docker-compose.yml` environment variables — see `docs/system/operations/local-stack.md`.

## Repo navigation rules

### Frontend

- Routes live in `src/app/app.routes.ts`.
- Feature code lives under `src/app/features/<feature>/`.
- Shared HTTP wrapper lives in `src/app/shared/api/services/api.service.ts`.
- Cross-cutting user/session concerns live under `src/app/core/` and `features/auth/`.
- Transient toast feedback uses `src/app/core/toast/services/toast.service.ts` with global `<p-toast>` in `app.component.ts`.
- Shared data models live under `src/app/shared/`.

### Backend

- HTTP endpoints live in `src/Template.Backend.Web/Endpoints/<Feature>/` (nested routes in sub-slices, for example `Endpoints/Issues/Attachments/`).
- Query and command contracts live in `src/Template.Backend.UseCases/<Feature>/` (nested routes in sub-slices, for example `UseCases/Issues/Attachments/`).
- Keep only `*Query.cs` and `*Command.cs` files at the top level of each `UseCases` feature folder (root resource operations).
- Place shared DTOs in `src/Template.Backend.UseCases/<Feature>/Dtos/`; sub-slice-only DTOs in `UseCases/<Feature>/<ChildResource>/Dtos/`.
- Place shared handler helpers in `src/Template.Backend.UseCases/<Feature>/Utils/`; sub-slice-only helpers in `UseCases/<Feature>/<ChildResource>/Utils/`.
- Place shared feature services in `src/Template.Backend.UseCases/<Feature>/Services/`; sub-slice-only services in `UseCases/<Feature>/<ChildResource>/Services/`.
- Handlers still live with their matching command or query file.
- Domain rules and aggregates live in `src/Template.Backend.Domain`.
- EF Core, persistence, auth, and adapters live in `src/Template.Backend.Infrastructure`.
- Integration tests mirror API behavior under `tests/Template.Backend.IntegrationTests`.
- Unit tests cover domain/infrastructure helpers under `tests/Template.Backend.UnitTests`.

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
- Start every executable or repository-automation script with a one- or two-line purpose comment as the first content after the shebang, or at the top when there is no shebang. Describe its outcome and main responsibility; declarative configuration and data files are exempt.
- Do not assume files visible in the IDE are committed; verify against the filesystem first.

## Requirements layers (product analysis)

Five layers — see `docs/requirements/_shared/README.md`:

| Layer             | Path                                                         | When to read                                   |
| ----------------- | ------------------------------------------------------------ | ---------------------------------------------- |
| L1 Domain         | `docs/requirements/_shared/domain/`                          | Terms, account model, permissions              |
| L2 Conventions    | `docs/requirements/_shared/conventions/product-standards.md` | Any UI — lists, forms, validation UX, feedback |
| L3 Quality        | `docs/requirements/_shared/quality/`                         | Performance, a11y, i18n                        |
| L4 Capabilities   | `docs/requirements/functional/<domain>/fr-*.md`              | The feature you are implementing               |
| L5 Implementation | `docs/modules/*/development.md`, `docs/system/development/`  | Module patterns and cross-module workflows     |

**Override rule:** L4 overrides L2; L5 never defines product behavior.

**After implementing UI:** run the **Implementation review checklist** at the end of `product-standards.md` for each `STD-*` in the target `FR-*` `inherits_conventions`.
