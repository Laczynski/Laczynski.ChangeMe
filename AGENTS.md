# AI Working Guide

> Scope: fast-start context for AI agents and contributors in **this solution**. Load this file first, then open the focused docs under `docs/` for the area you are changing.

## Repository shape

- `src/ChangeMe.Frontend` - Angular frontend.
- `src/ChangeMe.Backend` - .NET backend solution.
- `docker-compose.yml` - local full-stack environment with frontend, backend, PostgreSQL, and MailHog.
- `docs/` - product requirements plus system- and module-owned documentation (`docs/README.md` for the task index).
- `docs/documentation-rules.md` - ownership, naming, diagram, and no-duplication rules for documentation.
- `docs/system/` - cross-module development, designs, and operations owned by this repository.
- `docs/modules/` - module documentation designed to move with future repositories.
- Root `package.json` - optional npm scripts that run frontend and backend tasks from the repository root (see Commands).

## Start here by task

Use [`docs/README.md`](docs/README.md) as the task router. It links to the canonical document for orientation, implementation, testing, operations, and requirements workflow.

Requirement structure and layer precedence: [`docs/requirements/_shared/README.md`](docs/requirements/_shared/README.md) (**L4 overrides L2; L5 never defines product behavior**).

After documentation or specification edits, run `npm run docs:validate`.

## Commands

### Repository root (npm)

From the repository root, run `npm run setup` once after clone (creates `.env` from `.env.example` when missing, installs npm packages and Playwright Chromium, restores .NET, and installs Lefthook pre-commit hooks). Existing `.env` files are never overwritten. Frontend packages still live under `src/ChangeMe.Frontend`; use `npm run install:frontend` when only frontend dependencies change.

- First-time setup: `npm run setup`
- Install frontend dependencies and Playwright Chromium: `npm run install:frontend`
- Start dev servers: `npm run start:frontend`, `npm run start:backend`, or both in parallel with `npm run start:all`
- Build: `npm run build:frontend`, `npm run build:backend`, or `npm run build:all`
- Frontend quality: `npm run lint:frontend`, `npm run format:frontend`, `npm run test:frontend` (interactive watch when TTY), or `npm run test:frontend:ci` (single run, `--watch=false`)
- Backend tests: `npm run test:backend` (entire solution — unit and integration projects), `npm run test:backend:unit`, or `npm run test:backend:integration`
- Full automated check (frontend CI tests + full backend solution tests, parallel): `npm run test:all` — backend integration tests use Testcontainers and need a running Docker engine
- E2E (Playwright): `npm run test:e2e` (needs Chromium from `npm run install:frontend`, PostgreSQL on `localhost`; Playwright starts the stack); `npm run test:e2e:ui` for interactive debugging — E2E is local/manual and is not a CI or release gate (`docs/system/operations/ci.md`)
- CI workflow (GitLab): see `docs/system/operations/ci.md`
- EF Core (from repo root): `npm run ef:migrations:add -- <Name>`, `npm run ef:migrations:remove`, `npm run ef:database:update`
- Demo data (after migrations; Development only): `npm run data:generate`, or `npm run data:generate -- --reset` — see `docs/modules/backend/demo-data.md`
- Documentation: `npm run docs:validate` — checks links, system/module contracts and index reachability, then validates `FR-*` / `NFR-*` cross-references and regenerates the requirements index

### Native VPS delivery (Linux/WSL)

- Set up the isolated Ansible controller and all local prerequisites: `npm run setup:deployment` (native Linux or the default WSL distribution on Windows)
- Validate inventory, generator, GitLab YAML, tests, playbooks, lint, and the package contract: `npm run validate:deployment`
- Setup troubleshooting, bootstrap, deploy, verify, configuration-only deploy, and rollback procedures: `docs/system/operations/deployment.md`
- GitLab creates packages and manual deployment choices; Docker and Compose remain local-only

Generated-application release automation (`/release`) is documented in `docs/system/operations/deployment.md`.

### Frontend (in `src/ChangeMe.Frontend`)

- Install dependencies: prefer `npm run install:frontend` from the repository root (includes Playwright Chromium); `npm install` here installs npm packages only
- Run dev server: `npm start`
- Lint: `npm run lint`
- Format: `npm run format`
- Tests: `npm test`
- E2E: `npm run e2e` (from repo root: `npm run test:e2e`, or `npm run test:e2e:ui` for Playwright UI)

### Backend (in `src/ChangeMe.Backend`)

- Includes `InitialCreate` — in Development, migrations apply on API startup (`DatabaseOptions:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`; see `docs/modules/backend/operations/persistence.md`).
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

Configuration in containers: `appsettings.json` + `appsettings.Development.json` (image build) with overrides from `docker-compose.yml` environment variables — see `docs/system/operations/local-stack.md`.

## Repo navigation rules

Structural patterns only — feature-specific routes, endpoints, and requirements live in code and `docs/requirements/functional/`.

### Frontend

- Routes live in `src/app/app.routes.ts`.
- Feature code lives under `src/app/features/<feature>/`.
- Shared HTTP wrapper lives in `src/app/shared/api/services/api.service.ts`.
- Cross-cutting concerns live under `src/app/core/`; feature-specific guards, interceptors, and services live under the matching feature folder.
- Shared data models live under `src/app/shared/`.

Module conventions: [`docs/modules/frontend/development.md`](docs/modules/frontend/development.md).

### Backend

- HTTP endpoints live in `src/ChangeMe.Backend.Web/Endpoints/<Feature>/` (nested routes in sub-slices).
- Query and command contracts live in `src/ChangeMe.Backend.UseCases/<Feature>/` (matching sub-slice layout).
- Keep only `*Query.cs` and `*Command.cs` files at the top level of each `UseCases` feature folder (root resource operations).
- Place shared DTOs, utils, and services at the feature root; sub-slice-only types live under the matching child folder.
- Handlers stay in the same files as their commands and queries.
- Domain rules and aggregates live in `src/ChangeMe.Backend.Domain`.
- EF Core, persistence, auth, and adapters live in `src/ChangeMe.Backend.Infrastructure`.
- Integration tests mirror API behavior under `tests/ChangeMe.Backend.IntegrationTests`.
- Unit tests cover domain/infrastructure helpers under `tests/ChangeMe.Backend.UnitTests`.

Module conventions: [`docs/modules/backend/development.md`](docs/modules/backend/development.md).

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
