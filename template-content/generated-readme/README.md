# Template

Full-stack starter generated from the **Template** template: Angular frontend, layered ASP.NET backend, PostgreSQL, Docker Compose, and automated tests.

The included **issue-tracking sample** shows how features are structured — login, RBAC, CRUD, attachments, notifications. Replace or remove it; keep the **architecture, tooling, guidelines, and doc workflow**.

## What you get

### Architecture and patterns

- **Layered backend** — Web → UseCases → Domain → Infrastructure, with feature folders and handler conventions
- **FastEndpoints** + **Mediator** source generator — command/query handlers, validation, and endpoint base types
- **API versioning** (`/api/v1`) — pattern for versioned routes and Swagger
- **JWT sessions** — access + refresh tokens, session list/revoke; **RBAC** with permission catalog, guards, and backend checks (reference implementation)
- **EF Core** + PostgreSQL — configurations, migrations (`InitialCreate`), `ApplicationDbContext` usage
- **Cross-cutting infrastructure** — Hangfire jobs, Serilog, email abstraction (MailHog locally), local file storage pattern
- **Angular feature slices** — `features/<name>/`, shared `ApiService`, interceptors, guards, PrimeNG + Tailwind setup
- **Production frontend config** — `runtime-config.js`, nginx same-origin proxy for `/api/` and `/hubs/` in Docker
- **Sample domain** — issues, users, roles illustrate end-to-end flows; copy the pattern or delete the feature

### Tooling (ready to run)

- Root **`package.json`** — start/build, lint/format, tests, E2E, EF migrations, Docker Compose, demo data, requirements validation
- **Docker Compose** — full stack (frontend, backend, PostgreSQL, MailHog)
- **Playwright E2E** — project layout, fixtures, smoke pattern (`docs/system/development/e2e-testing.md`)
- **Testcontainers** — integration tests against real PostgreSQL
- **Data generator** — Development seed data CLI (`npm run data:generate`)

### Documentation workflow

| Location | Purpose |
| --- | --- |
| **`docs/requirements/`** | What to build — domain, conventions, quality, and `FR-*` specifications |
| **`docs/system/`** | Cross-module development, designs, local stack, deployment, and CI |
| **`docs/modules/`** | Frontend/backend implementation and operations; movable with future repositories |

- **`AGENTS.md`** — fast-start for AI agents and contributors
- **`npm run docs:validate`** — check all documentation, including requirement specs, cross-references, and generated indexes

### Testing approach

- **Frontend** — specs where useful; Playwright smoke tests for main flows
- **Backend unit** — domain and infrastructure helpers
- **Backend integration** — API behaviour with Testcontainers
- **`docs/system/development/testing-strategy.md`** — layer ownership and when to skip redundant tests

## Purpose

This codebase gives you:

- a reproducible full-stack skeleton with conventions already chosen
- separation between frontend, API, domain, and infrastructure
- reference implementations you can extend or replace with your own domain
- a documentation and requirements workflow as the product grows
- structure that is easy for developers and AI assistants to follow

## Tech Stack

- Frontend: Angular 22, TypeScript, RxJS
- Backend: ASP.NET Core, FastEndpoints, Mediator source generator use case flow
- Database: PostgreSQL
- Background jobs: Hangfire
- Local email testing: MailHog
- Testing: Angular test runner, .NET unit tests, .NET integration tests with Testcontainers
- Local orchestration: Docker Compose
- UI: PrimeNG + Tailwind CSS

## Repository Structure

- `src/Template.Frontend` - Angular application
- `src/Template.Backend` - .NET solution with source projects and tests
- `docs/` - implementation and testing guidance
- `docker-compose.yml` - local full-stack environment (frontend, backend, PostgreSQL, MailHog)
- `AGENTS.md` - working guide for AI agents and contributors

## Getting Started

Run the repository setup once:

```powershell
npm run setup
```

Setup creates the ignored `.env` from `.env.example` when missing and never overwrites an existing file. Replace the placeholder values before starting the backend or Docker Compose. The same file supplies local .NET tools and Compose; real environment variables retain precedence.

### Frontend

From the **repository root** (recommended — includes Playwright Chromium for E2E):

```powershell
npm install
npm run install:frontend
npm run start:frontend
```

Or from `src/Template.Frontend` (npm packages only):

```powershell
npm install
npm start
```

Useful commands:

```powershell
npm run lint
npm run format
npm test
npm run test:e2e
```

See `docs/system/development/e2e-testing.md` for Playwright setup (Chromium from `npm run install:frontend`).

### Backend

Includes `InitialCreate` — in Development, migrations apply on API startup (`DatabaseOptions:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`; see `docs/modules/backend/operations/persistence.md`).

From `src/Template.Backend`:

```powershell
dotnet build Template.Backend.slnx
dotnet run --project src/Template.Backend.Web
```

### Sample data (optional)

After migrations are applied, from the **repository root**:

```powershell
npm run data:generate
```

Creates demo users (`user1@demo.local`, password in `DataGenerator:DefaultPassword`), issues, comments, and notifications. Use `npm run data:generate -- --reset` to refresh. See `docs/modules/backend/demo-data.md`.

Useful commands:

```powershell
dotnet test tests/Template.Backend.UnitTests
dotnet test tests/Template.Backend.IntegrationTests
```

### Full Stack with Docker

From the repository root:

```powershell
docker compose up --build
```

This starts the frontend, backend, MailHog, and the database service defined in Compose.

## Documentation

See [`docs/README.md`](docs/README.md) for the full index.

- `docs/system/` - cross-module development, designs, and operations
- `docs/modules/` - frontend/backend implementation and module-owned operations
- `docs/requirements/` - product specs and change workflow (start at `docs/requirements/requirements-change-process.md`; templates `_functional-specification-template.md`, `_changes-template.md`)

## About `AGENTS.md`

[`AGENTS.md`](AGENTS.md) is the fast-start guide for AI agents and contributors: which docs to open first, repo layout, commands, folder navigation, and cross-stack coupling checks.

## Development Notes

- Frontend routes: `src/app/app.routes.ts`.
- Frontend features: `src/app/features/<feature>/`.
- Backend endpoints: `src/Template.Backend.Web`.
- Use cases: `src/Template.Backend.UseCases`.
- Domain: `src/Template.Backend.Domain`.
- Persistence and integrations: `src/Template.Backend.Infrastructure`.

## Testing

Use the smallest relevant test scope for the change:

- frontend UI or service change: `npm run lint` and `npm test`
- backend domain change: unit tests
- backend endpoint or auth change: integration tests

See `docs/system/development/testing-strategy.md`.

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE) for details.
