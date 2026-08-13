# Repository map

> Type: reference
> Scope: system
> Status: implemented
> Canonical for: current repository layout and code ownership

## Top level

- `docker-compose.yml` starts the local stack: Angular frontend, ASP.NET backend, PostgreSQL, and MailHog. The same file defines `backend-tests` (Compose profile `test`): an SDK container that runs `dotnet test` on the backend solution with the repository mounted from the host.
- Root `package.json` defines optional npm scripts (`start:*`, `build:*`, `test:*`, `install:frontend`, and frontend `lint`/`format`) so you can run common frontend and `dotnet` backend tasks from the repository root. Run `npm install` in the repository root to install root devDependencies such as `concurrently` (used by `start:all` and `test:all`). Frontend `node_modules` still live under `src/Template.Frontend` — refresh them with `npm run install:frontend` from the root (also installs Playwright Chromium for E2E) or `npm install` inside that folder (npm packages only).
- `src/Template.Frontend` contains the Angular application.
- `src/Template.Backend` contains the .NET solution and tests.
- `docs/requirements/` — product rules in five layers (`docs/requirements/_shared/README.md`): **L1** domain · **L2** conventions · **L3** quality · **L4** `FR-*` · **L5** module and system development docs.

## Frontend map

### Tooling and entry points

- `package.json` defines `start`, `build`, `lint`, `format`, and `test`.
- `src/main.ts` bootstraps the Angular app.
- `src/app/app.config.ts` configures providers.
- `src/app/app.routes.ts` defines route-to-component mapping.
- `tsconfig.json` defines strict TypeScript settings and path aliases:
  - `@core/*`
  - `@features/*`
  - `@shared/*`
  - `@styles/*`
  - `@environments/*`

### Runtime structure

- `src/app/core/` holds app-wide services and models that are not specific to one feature (layout shell, toasts).
- `src/app/features/` holds feature slices such as `auth` and `issues`.
- `src/app/shared/` holds reusable API wrappers and shared data contracts.

### Feature layout

Each current feature follows a simple slice structure:

- `components/` - standalone Angular components bound directly from routes or nested views.
- `models/` - feature-specific TypeScript contracts (DTOs, enums, request/response shapes).
- `utils/<feature>.utils.ts` - one file per feature for validation limits, labeled select options, and other UI constants.
- `services/` - feature-specific data access and orchestration.
- `guards/` or `interceptors/` - feature-specific Angular infrastructure where needed.

## Backend map

### Solution shape

- `Template.Backend.slnx` is the backend solution entry point.
- `Directory.Packages.props` manages package versions centrally.
- `Directory.Build.props` enables central package version management.

### Layer responsibilities

- `src/Template.Backend.Web`
  - ASP.NET host startup in `Program.cs`
  - REST endpoint definitions under `Endpoints/<Feature>/` (sub-slices for nested routes, for example `Issues/Attachments/`)
  - SignalR hubs and other non-REST transport outside `Endpoints/`
  - transport-level configuration
  - common endpoint base types and pipeline behavior (`BaseEndpoint`, `BaseEndpointWithoutRequest`, `ResultHttpMapper`, `HttpContextResultExtensions`)
- `src/Template.Backend.UseCases`
  - top level of each feature folder contains only root-resource command and query files
  - nested routes use sub-slice folders (for example `Issues/Attachments/`, `Users/Sessions/`)
  - handlers stay in the same files as their commands and queries
  - `Dtos/`, `Utils/`, and `Services/` at the feature root are for types and helpers shared across the root resource and sub-slices
  - sub-slice-only `Dtos/`, `Utils/`, and `Services/` live under the matching `<ChildResource>/` folder
  - request/response orchestration
- `src/Template.Backend.Domain`
  - aggregates and entities
  - invariants and domain rules
  - domain interfaces and shared primitives
- `src/Template.Backend.Infrastructure`
  - EF Core `ApplicationDbContext`
  - entity configuration and migrations
  - auth and email adapters
  - persistence and infrastructure registrations
- `tools/Template.Backend.DataGenerator`
  - dev-only console tool for demo/test data (`npm run data:generate`)
  - see [backend demo data](../../modules/backend/demo-data.md)

### Endpoint flow

Current issue endpoints illustrate the standard flow:

1. HTTP endpoint class in `Web/Endpoints/Issues/*.cs` (nested routes in `Web/Endpoints/Issues/<ChildResource>/`)
2. validation class near the endpoint
3. command/query contract in `UseCases/Issues/*.cs` (or `UseCases/Issues/<ChildResource>/`)
4. handler in the same command/query file
5. domain calls in `Domain/Aggregates/*`
6. persistence through `ApplicationDbContext`

## Test map

- `src/Template.Backend/tests/Template.Backend.UnitTests` — domain and infrastructure helper tests
- `src/Template.Backend/tests/Template.Backend.IntegrationTests` — endpoint-level tests through real HTTP
  - `Endpoints/<Feature>/` — one test class per endpoint area (use sub-slices for nested routes, for example `Issues/Attachments/`)
  - `Fixtures/` — `BackendWebApplicationFactory` and feature-specific factories
  - `Support/` — `TestAuthHelper` (register + authenticate via real API calls), `IssueTestHelper`, and other feature helpers
- `src/Template.Frontend` — Vitest unit/component specs colocated as `*.spec.ts`; E2E suite in `e2e/features/` (see [E2E testing](e2e-testing.md))

Which layer to test and when to skip: [testing strategy](testing-strategy.md).

`BackendWebApplicationFactory` starts disposable PostgreSQL via Testcontainers, applies test environment overrides (connection string, JWT, email settings), and replaces `IEmailService` with `FakeEmailService`.

## Documentation ownership

| Scope | Location | Contains |
| --- | --- | --- |
| Product | `docs/requirements/` | Domain, conventions, quality, functional requirements |
| System | `docs/system/` | Cross-module development, designs, and operations |
| Frontend | `docs/modules/frontend/` | Angular implementation and architecture |
| Backend | `docs/modules/backend/` | .NET implementation, development tools, and operations |

The classification and naming rules are canonical in [`docs/documentation-rules.md`](../../documentation-rules.md).
