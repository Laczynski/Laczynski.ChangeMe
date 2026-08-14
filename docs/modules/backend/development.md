# Backend development

> Type: development
> Scope: backend
> Status: implemented
> Canonical for: .NET layer ownership, endpoint, handler, persistence, and configuration-options implementation rules
>
> Product behaviour: target `FR-*` (L4) and [`product-standards.md`](../../requirements/_shared/conventions/product-standards.md) (L2) when API responses affect UX. This document owns implementation only.

## Stack summary

ASP.NET Core on .NET 10 with FastEndpoints, Mediator source generator request handling, EF Core for persistence, FluentValidation validators declared near endpoints, and xUnit v3 tests. The backend is split into `Web`, `UseCases`, `Domain`, and `Infrastructure`.

For build, run, and test commands from `src/Template.Backend` or from the repository root (`npm run build:backend`, `npm run test:backend`, and scoped scripts), see `AGENTS.md`.

## Layer ownership

### Web

- Owns application startup in `Program.cs`.
- Startup registrations and middleware for one concern should be grouped in `Web/Configurations/*Config.cs` extension methods rather than added inline in `Program.cs`.
- Owns HTTP endpoint declarations under `Web/Endpoints/<Feature>/`.
- For nested routes (child resources under a parent ID), add a sub-slice folder: `Web/Endpoints/<Feature>/<ChildResource>/` — for example `Issues/Attachments/`, `Users/Sessions/`, `Roles/Users/`.
- SignalR hubs and other non-REST transport stay outside `Endpoints/` (for example `Web/Notifications/NotificationHub.cs`).
- Owns API-facing validation classes placed next to endpoint definitions.
- Owns endpoint base behavior: JWT auth defaults, `Result<T>` serialization, and HTTP status mapping through `Web/Common/` (`BaseEndpoint.cs`, `BaseEndpointWithoutRequest.cs`, `ResultHttpMapper.cs`, `HttpContextResultExtensions.cs`).

### UseCases

- Owns commands, queries, handlers, API DTOs, and feature-scoped orchestration services.
- Orchestrates domain operations and infrastructure dependencies.
- Should not duplicate domain invariants that already belong in aggregates.
- Keep only `*Command.cs` and `*Query.cs` files at the top level of each feature folder (root resource operations).
- For nested routes, mirror the Web sub-slice under `UseCases/<Feature>/<ChildResource>/` — for example `Issues/Attachments/`, `Users/Sessions/`, `Roles/Users/`.
- Place DTOs shared by the root resource and multiple sub-slices under `UseCases/<Feature>/Dtos/`.
- Place DTOs used only by one sub-slice under `UseCases/<Feature>/<ChildResource>/Dtos/`.
- Place shared handler helpers under `UseCases/<Feature>/Utils/` as `*Utils.cs` static classes — mirror the frontend `utils/<feature>.utils.ts` convention.
- Place helpers used only by one sub-slice under `UseCases/<Feature>/<ChildResource>/Utils/`.
- Place feature services under `UseCases/<Feature>/Services/` for orchestration shared across the feature; sub-slice-only services go under `UseCases/<Feature>/<ChildResource>/Services/`.

### Domain

- Owns aggregates, entities, enums, interfaces, and business rules.
- Persisted domain types (including child entities and join rows) inherit `Entity` so `CreatedAt`, `UpdatedAt`, `CreatedBy`, and `UpdatedBy` are set consistently via `ApplicationDbContext`.
- Should be the place for invariants like issue title/description constraints.

### Infrastructure

- Owns EF Core `ApplicationDbContext`, entity configuration, migrations, auth adapters, email service (`Infrastructure/Email/`), and application service registrations.

## Standard path for a new endpoint

1. Add or extend the endpoint in `Web/Endpoints/<Feature>/` (or `Web/Endpoints/<Feature>/<ChildResource>/` for nested routes).
2. Add or update the validator in the same file if the request shape changes.
3. Add or update the command/query and handler in the matching `UseCases/<Feature>/` folder (use a sub-slice when the route is nested).
4. Add or update DTOs in `UseCases/<Feature>/Dtos/` when shared, or in `UseCases/<Feature>/<ChildResource>/Dtos/` when sub-slice-specific.
5. Add or update services in `UseCases/<Feature>/Services/` when shared, or in `UseCases/<Feature>/<ChildResource>/Services/` when sub-slice-specific.
6. Reuse or extend domain methods in `Domain/`.
7. Update EF configuration or migrations in `Infrastructure/` if persistence changes.
8. Add or update integration tests when the endpoint or persistence changes ([testing strategy](../../system/development/testing-strategy.md)).

## Feature and sub-slice layout

```
Web/Endpoints/<Feature>/              UseCases/<Feature>/
├── CreateX.cs                        ├── CreateXCommand.cs
├── GetXById.cs                       ├── GetXByIdQuery.cs
├── Dtos/                             ├── Dtos/              ← shared across feature
├── Utils/                            ├── Utils/
└── <ChildResource>/                  └── <ChildResource>/
    ├── GetXChildItems.cs                 ├── GetXChildItemsQuery.cs
    ├── DeleteXChildItem.cs               ├── DeleteXChildItemCommand.cs
    ├── Dtos/                             ├── Dtos/          ← sub-slice only
    └── Utils/                            └── Utils/
```

Use a sub-slice when the route nests under a parent resource — for example `/issues/{issueId}/attachments/{attachmentId}` maps to `Issues/Attachments/`. Root CRUD for the aggregate stays at the feature root. Namespace follows the folder path (for example `Template.Backend.Web.Endpoints.Issues.Attachments`, `Template.Backend.UseCases.Issues.Attachments.Dtos`).

Keep `Dtos/`, `Utils/`, and `Services/` at the feature root only when they are shared by the root resource and sub-slices. When a type or helper belongs to a single child resource, colocate it under that sub-slice folder.

Reference implementations: `Issues/Attachments/` (list, upload, download, two-ID delete), `Issues/Comments/`, `Users/Sessions/`, `Roles/Users/`, `Auth/Sessions/`.

## Endpoint conventions

Choose the endpoint base type by how the HTTP request is shaped:

| Shape                                                          | Base type                                                               | Example                                                       |
| -------------------------------------------------------------- | ----------------------------------------------------------------------- | ------------------------------------------------------------- |
| Body, query string, and/or route params bound to a request DTO | `BaseEndpoint<TRequest, TResponse>`                                     | `UpdateIssue`, `DeleteIssueAttachment`, `GetIssueAttachments` |
| No request payload (GET/POST/PUT with empty body)              | `BaseEndpointWithoutRequest<TRequest, TResponse>`                       | `GetMyAccount`, `Logout`, `MarkAllNotificationsAsRead`        |
| Multipart upload or other custom request handling              | `Endpoint<TRequest, Result<TResponse>>` + `HttpContext.SendResultAsync` | `UploadIssueAttachment`                                       |
| Binary stream or non-JSON response                             | `EndpointWithoutRequest` (custom `HandleAsync`)                         | `DownloadIssueAttachment`                                     |

Shared rules:

- Configure route, permissions, and summary in `ConfigureEndpoint()`.
- Let the base types handle `Result<T>` serialization and status codes; custom endpoints call `HttpContext.SendResultAsync` for JSON `Result<T>` responses.
- Validators stay close to the endpoint they protect (`Validator<TRequest>` in the same file). `BaseEndpoint` runs them automatically via FastEndpoints; custom endpoints must validate explicitly when needed.
- Use **parameterless** command/query records for no-payload endpoints — e.g. `record LogoutCommand() : ICommand<bool>`. Do **not** add dummy properties such as `doNothing` to satisfy binding.
- When an endpoint has **route parameters only** (no body), keep using `BaseEndpoint<TRequest, TResponse>` — FastEndpoints binds route segments to matching property names on the request DTO.
- When an endpoint has **no payload but needs route values** and you prefer `BaseEndpointWithoutRequest`, override `CreateRequest()` to read `Route<T>(...)`.

## API versioning

REST routes are versioned in the URL (`/api/v1/...`). Version and prefix live in `Web/Configurations/ApiVersionConfig.cs`; wiring in `FastEndpointsConfig.cs`. `BaseEndpoint` classes apply the current version automatically; custom `Endpoint<...>` classes set `Version(...)` themselves. `/health`, `/hubs`, and Hangfire stay unversioned.

## Handler conventions

- Handlers live in the same file as their request contract in `UseCases/<Feature>/`.
- Return `Result<T>` consistently.
- **Grid list endpoints** (Issues, Users, Roles, and embedded lists such as comments, history, attachments, notifications, sessions, assigned users) accept a `GridQuery` on the request (query parameter `grid`, JSON) and return `GridResult<T>` from `Laczynski.DataGrid.Abstractions`. Project to a list DTO, then call `ToGridResultAsync` from `Laczynski.DataGrid.EntityFrameworkCore`. Invalid grid input is handled by the global exception handler — do not catch `GridValidationException` in handlers. Mark searchable DTO properties with `[GridSearchable]`; use `[GridIgnore]` for display-only or non-queryable fields. See `UseCases/Issues/GetAllIssuesQuery.cs` as the reference handler.
- Use `ApplicationDbContext` for persistence from the handler layer.
- After `SaveChangesAsync`, return API DTOs through an existing query via `mediator.Send` — do not instantiate query handlers with `new`.
- For create commands that return a resource body, wrap the query result in `Result.Created(dto, "/resource/{id}")` so `BaseEndpoint` responds with `201 Created`.
- For update or state-change commands that return the same details DTO, return the query `Result` directly (`200 OK`).

## Persistence conventions

- EF Core configuration lives under `Infrastructure/Persistence/Config`.
- Migrations live under `Infrastructure/Persistence/Migrations`.
- Database startup and migration behavior are configured through `Database` options and startup configuration.

### Unit of work

Infrastructure services (e.g. `UserAuthTokenService`) **stage** EF changes only — **no** `SaveChangesAsync`. The handler or feature orchestrator commits **once** per operation, after the full flow succeeds (including email send when a token must not outlive a failed delivery).

`IUserAuthTokenService`: `IssueTokenAsync`, `MarkTokenUsedAsync`, and `InvalidateUnusedTokensAsync` never save; `ValidateTokenAsync` and previews are read-only. Do not save in both the token service and the caller for the same change.

### Optimistic concurrency

- Persisted roots inherit `Entity.Version` (`long`, EF concurrency token via `IsConcurrencyToken()`).
- `ApplicationDbContext` bumps `Version` on modified entities; `DbUpdateConcurrencyException` propagates from `SaveChangesAsync`.
- `ExceptionHandlerConfig` maps `DbUpdateConcurrencyException` to HTTP 409 with `Result.Conflict` in all environments.
- Update commands for form-edited aggregates accept `Version` in the request body; after load, call `ConcurrencyGuard.CheckExpectedVersion(entity, command.Version)` before applying changes.
- Details DTOs expose `version` so the client can send it back on `PUT`.
- Message constant: `ConcurrencyMessages.StaleVersion` in `Domain/Common/`.

## Auth and cross-cutting concerns

- JWT configuration lives in `Web/Configurations/AuthConfig.cs` and environment settings.
- The notifications SignalR hub (push notifications only) and its DI registration should be configured through dedicated `Web/Configurations/*Config.cs` files, not inline in `Program.cs`.
- Endpoint auth defaults come from `BaseEndpoint` and `BaseEndpointWithoutRequest`.
- Email is abstracted behind `IEmailService`.

## Configuration options

Rationale and environment delivery: [runtime configuration hardening](../../system/designs/runtime-configuration-hardening-design.md).

- Place each `*Options` type and its `*OptionsValidator` as separate `sealed` classes in the owning folder (`Infrastructure/Auth/`, `Infrastructure/Email/`, `Web/Configurations/`, `UseCases/<Feature>/Services/`, …). Do not fold options into `*Config.cs` or service classes.
- Register every validated section in `Web/Configurations/RuntimeConfigurationConfig.cs` through `AddValidatedOptions` (`IValidateOptions<T>` + bind + `ValidateOnStart()`).
- Call `OptionsValidation.GetValidated` only when an `Add*Config` method needs the values during service registration (JWT, CORS, Hangfire, connection string). Design-time and demo-data hosts reuse `ConnectionStringsOptionsValidator.GetValidatedDefaultConnection`.
- `Program` calls `ValidateRuntimeConfiguration()` after `Build()` so invalid values fail before middleware. Hosts that only call `AddRuntimeConfiguration` still fail on `Start` via `ValidateOnStart()`.
- Simple flags such as `DatabaseOptions` and `SwaggerOptions` may stay without a validator.

Reference implementations: `AuthOptions` / `AuthOptionsValidator`, `EmailOptions` / `EmailOptionsValidator`.

## Tests

Layer ownership, anti-patterns, and when to skip: [testing strategy](../../system/development/testing-strategy.md). Integration tests: `src/Template.Backend/tests/Template.Backend.IntegrationTests/Endpoints/<Feature>/` (sub-slices for nested routes).

## Guardrails for AI agents

- Do not bypass the existing layered structure by placing domain logic directly in `Web`.
- Do not access the database directly from endpoint classes.
- Do not return ad hoc response envelopes; reuse the existing `Result<T>` flow.
- Before adding a new abstraction, inspect the closest feature in `Issues` or `Auth` and extend that pattern first.
- Infrastructure helpers stage EF changes; handlers/orchestrators own `SaveChangesAsync` (see **Unit of work** above).
