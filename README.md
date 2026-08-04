# ChangeMe

ChangeMe is a **full-stack starter template** for `dotnet new` — not a product you deploy as-is. It ships a small sample app (issue tracking) so you can see patterns in context, but the value is the **architecture, tooling, documentation workflow, and test setup** you reuse in your own domain.

Use it to bootstrap Angular + ASP.NET projects with layered backend, JWT sessions, PostgreSQL, Docker Compose, CI, and docs already wired — then replace or extend the sample features.

## What the template gives you

### Architecture and patterns

- **Layered backend** — Web → UseCases → Domain → Infrastructure, with feature folders and handler conventions
- **FastEndpoints** + **Mediator** source generator — command/query handlers, validation, and endpoint base types
- **API versioning** (`/api/v1`) — pattern for versioned routes and Swagger
- **JWT sessions** — access + refresh tokens, session list/revoke; **RBAC** with permission catalog, guards, and backend checks (reference implementation)
- **EF Core** + PostgreSQL — configurations, migrations (`InitialCreate`), repository-style `ApplicationDbContext` usage
- **Cross-cutting infrastructure** — Hangfire jobs, Serilog, email abstraction (MailHog locally), local file storage pattern
- **Angular feature slices** — `features/<name>/`, shared `ApiService`, interceptors, guards, PrimeNG + Tailwind setup
- **Production frontend config** — `runtime-config.js`, nginx same-origin proxy for `/api/` and `/hubs/` in Docker
- **Sample domain** — issues CRUD, comments, attachments, notifications illustrate end-to-end flows; treat as examples to copy or remove

### Tooling (ready to run)

- Root **`package.json`** — one entry point for start/build, lint/format, unit & integration tests, E2E, EF migrations, Docker Compose, demo data, requirements validation
- **Docker Compose** — full stack (frontend, backend, PostgreSQL, MailHog) plus optional test profile
- **GitHub Actions CI** — requirements, frontend, backend, and Playwright E2E in parallel
- **Playwright E2E** — project layout, fixtures, smoke pattern (`docs/guides/e2e-guidelines.md`)
- **Testcontainers** — integration tests against real PostgreSQL
- **Data generator** — CLI pattern for Development seed data (`npm run data:generate`)
- **`dotnet new` token replacement** — `ChangeMe` plus derived `changeMe` / `CHANGE_ME` symbols from your project name

### Documentation workflow

Three doc layers, each with an entry point:

| Layer                    | Purpose                                                                                             |
| ------------------------ | --------------------------------------------------------------------------------------------------- |
| **`docs/guides/`**       | How to implement — repo map, frontend/backend guidelines, testing & E2E guidelines, feature recipes |
| **`docs/technical/`**    | How to run & deploy — Docker, database, CI, deployment checklist, data generator                    |
| **`docs/requirements/`** | What to build — `FR-*` / `NFR-*` specs, authoring guide, change process, validation script          |

- **`AGENTS.md`** — fast-start for AI agents and contributors (task → which doc to open, commands, coupling checks)
- **`npm run requirements:validate`** — lint specs, cross-references, regenerate requirements index
- Templates for new specs and change deltas (`_functional-specification-template.md`, `_changes-template.md`)

### Testing approach

- **Frontend** — component/service specs where they add value; primary confidence from Playwright smoke tests
- **Backend unit** — domain and infrastructure helpers
- **Backend integration** — API behaviour with Testcontainers; endpoint tests mirror Web layer
- **`docs/guides/testing-guidelines.md`** — layer ownership, when to skip redundant tests, anti-patterns

## Purpose

This repository is meant to provide:

- a reproducible full-stack skeleton with conventions already chosen
- a clear separation between frontend, API, domain, and infrastructure concerns
- reference implementations of auth, permissions, CRUD, validation, background jobs, and integration testing
- a documentation and requirements workflow you can keep as the product grows
- a structure that is easy for both developers and AI agents to navigate

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

- `src/ChangeMe.Frontend` - Angular application
- `src/ChangeMe.Backend` - .NET solution with source projects and tests
- `docs/` - implementation and testing guidance
- `docker-compose.yml` - local full-stack environment (frontend, backend, PostgreSQL, MailHog)
- `AGENTS.md` - working guide for AI agents and contributors
- `.template.config/` - `dotnet new` template manifest (`changeme`, `sourceName` token `ChangeMe`)
- `template-pack/` - NuGet packaging project for the template
- `template-content/` - overlays and **generated-project** `README.md` (consumer readme for `dotnet new` output)

## Getting Started

### Quick start (this repository)

From the repository root after clone:

```powershell
npm run setup
docker compose up postgres mailhog -d
npm run start:all
```

- **`npm run setup`** — installs root and frontend npm packages (including Playwright Chromium), restores the .NET solution, and installs Git pre-commit hooks.
- **Pre-commit hooks** — [Lefthook](https://github.com/evilmartians/lefthook) runs ESLint, Prettier, and `dotnet format` on staged files. Re-run `npm run setup` or `npx lefthook install` after clone if hooks are missing.

See `docs/technical/database-and-docker.md` for Docker Compose, migrations, and secrets.

### Install as a `dotnet new` template

Install the template from NuGet:

```powershell
dotnet new install ChangeMe
```

Or install from this repository root during local development:

```powershell
dotnet new install .
```

Create a new solution from the installed template:

```powershell
dotnet new changeme -n IssuesDemo -o IssuesDemo
```

PostgreSQL persistence, Hangfire storage, Docker Compose, and integration tests.

The installed short name appears in the `Short Name` column of `dotnet new list`.

This replaces `ChangeMe` across the solution, project names, folders, Docker configuration, docs, and frontend package metadata. Use a .NET-friendly project name such as `IssuesDemo` so generated solution and namespace names stay valid. Avoid embedding the substring `ChangeMe` in secrets you expect to stay literal after generation (the template renames that token everywhere).

**Generated projects** receive a different root `README.md` (product-focused, no template packaging steps); that file is maintained under `template-content/generated-readme/README.md`.

### Publish the NuGet package

```powershell
dotnet pack template-pack/ChangeMe.Templates.csproj -c Release
```

```powershell
dotnet nuget push template-pack/bin/Release/ChangeMe.<version>.nupkg --source https://api.nuget.org/v3/index.json --api-key <your-api-key>
```

The packaging project targets `net10.0` only as a carrier for NuGet metadata and `dotnet pack`. It does not affect the generated solution structure or the target frameworks used by the projects created from the template.

### Frontend (this repository)

From the **repository root** (installs npm packages and Playwright Chromium for E2E):

```powershell
npm install
npm run install:frontend
npm run start:frontend
```

Or from `src/ChangeMe.Frontend` (npm packages only; E2E needs Chromium from `npm run install:frontend` at the root):

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

### Backend (this repository)

Includes `InitialCreate` — in Development, migrations apply on API startup (`DatabaseOptions:ApplyMigrationsOnStartup` is `true` in `appsettings.Development.json`; see `docs/technical/database-and-docker.md`).

From `src/ChangeMe.Backend`:

```powershell
dotnet build ChangeMe.Backend.slnx
dotnet run --project src/ChangeMe.Backend.Web
```

Useful commands:

```powershell
dotnet test tests/ChangeMe.Backend.UnitTests
dotnet test tests/ChangeMe.Backend.IntegrationTests
```

### Full stack with Docker

From the repository root:

```powershell
docker compose up --build
```

This starts the frontend, backend, MailHog, and the database service defined in this solution's Compose file.

## Documentation

The `docs/` directory contains guidance that is also shipped into generated solutions. See [`docs/README.md`](docs/README.md) for the full index.

- `docs/guides/` - implementation conventions (start at `docs/guides/README.md`)
- `docs/technical/` - run and configure the stack (start at `docs/technical/README.md`)
- `docs/requirements/` - product specs and change workflow (start at `docs/requirements/requirements-change-process.md`; templates `_functional-specification-template.md`, `_changes-template.md`)

Maintainers of the template package: see [`CONTRIBUTING.md`](CONTRIBUTING.md).

## About `AGENTS.md`

[`AGENTS.md`](AGENTS.md) is the fast-start guide for AI agents and contributors working in this repository. It explains:

- which docs to read first depending on the task
- the main repo structure
- standard frontend, backend, and Docker commands
- navigation rules for key folders
- change-coupling checks across frontend and backend
- working agreements for keeping changes aligned with the existing architecture

If you are making code changes, `AGENTS.md` should be treated as the first orientation document before opening area-specific docs in `docs/`.

## Development Notes

- Frontend routes are defined in `src/app/app.routes.ts`.
- Frontend feature code lives under `src/app/features/<feature>/`.
- Backend endpoints live in `src/ChangeMe.Backend.Web`.
- Use case contracts and handlers live in `src/ChangeMe.Backend.UseCases`.
- Domain rules live in `src/ChangeMe.Backend.Domain`.
- Persistence and integrations live in `src/ChangeMe.Backend.Infrastructure`.

## Testing

Use the smallest relevant test scope for the change:

- frontend UI or service change: `npm run lint` and `npm test`
- backend domain change: unit tests
- backend endpoint or auth change: integration tests

For more detail, see `docs/guides/testing-guidelines.md`.

## License

This project is licensed under the MIT License. See [`LICENSE`](LICENSE) for details.
