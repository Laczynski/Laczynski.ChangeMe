# Persistence

> Type: operations
> Scope: backend
> Status: implemented
> Canonical for: PostgreSQL configuration, EF Core migrations, and test databases

## Summary

- `ApplicationDbContext` uses PostgreSQL.
- EF configuration and migrations live under `ChangeMe.Backend.Infrastructure/Persistence/`.
- Development startup applies pending migrations; production deployment uses the backend's migration-only mode.
- Integration tests create disposable PostgreSQL databases with Testcontainers.

## Configuration

The ignored root `.env`, copied from `.env.example`, owns the locally editable PostgreSQL values. It derives `ConnectionStrings__DefaultConnection` with `Host=localhost` for API, EF Core, and data-generator processes running on the host. Docker Compose builds its backend connection string from the same database/user/password values with `Host=postgres`.

Real environment variables override `.env`; production does not load the file. Cross-module setup and precedence: [local full-stack environment](../../../system/operations/local-stack.md).

`InitialCreate` is included under `Infrastructure/Persistence/Migrations/`.

## Migrations

Run from the repository root:

| Task | Command |
| --- | --- |
| Add | `npm run ef:migrations:add -- <Name>` |
| Remove latest | `npm run ef:migrations:remove` |
| Apply without starting API | `npm run ef:database:update` |

In Development, `DatabaseOptions:ApplyMigrationsOnStartup` is enabled. `dotnet run`, `npm run start:backend`, and the Compose backend therefore apply pending migrations.

Keep startup migration disabled in production. The native deployment runs the selected release once as `ChangeMe.Backend.Web --migrate-only`; it applies pending EF Core migrations plus the idempotent bootstrap seed, then exits without starting HTTP. The application release is activated only after this command succeeds.

The migration-only mode uses the same production environment variables and startup validation as the normal service. Database migrations are not reversed during application rollback, so new schema changes must remain compatible with the previous retained release or have an explicit database-restore plan. Cross-module sequencing and rollback: [deployment](../../../system/operations/deployment.md).

## PostgreSQL container

The local stack uses PostgreSQL 18. Official PostgreSQL 18+ images store data under a versioned directory, so the Compose volume is mounted at `/var/lib/postgresql`, not `/var/lib/postgresql/data`.

When upgrading a disposable local volume from PostgreSQL 16/17, recreate it with `npm run docker:down:volumes`. Preserve non-disposable data with `pg_dump` or `pg_upgrade` instead.

## Seeds and tests

- `ApplicationDataSeeder` creates system roles and the optional initial administrator when database initialization runs.
- Optional sample users and issues are owned by the [demo data tool](../demo-data.md).
- `BackendWebApplicationFactory` creates a disposable PostgreSQL container, overrides the connection string, and calls `MigrateAsync()`.
- Backend integration tests require a running Docker engine.

## Verification

Run `npm run test:backend:integration` after endpoint or persistence changes. For local schema verification, apply the migration to a disposable database and start the API.

Cross-module Compose configuration: [local full-stack environment](../../../system/operations/local-stack.md).
