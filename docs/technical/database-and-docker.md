# Database and Docker Compose

> Scope: how persistence and `docker-compose.yml` fit **this solution**.

## Docker Compose and configuration

`npm run docker:up` runs `docker compose up --build` from the repository root.

The **backend** container sets `ASPNETCORE_ENVIRONMENT=Development`, so ASP.NET Core loads:

1. `appsettings.json` (baked into the image at publish time)
2. `appsettings.Development.json` (same)
3. **Environment variables** from `docker-compose.yml` (override JSON for matching keys)

Today Compose overrides only what the container must differ from local `dotnet run` — for example `ConnectionStrings__DefaultConnection` (database host `postgres` instead of `localhost`) and `FileStorageOptions__RootPath=/app/storage`. Everything else (Auth, email, CORS, Serilog, etc.) comes from **appsettings** unless you add more `ServiceName__Property` entries under `backend.environment` in Compose.

To change Docker-only settings, prefer `docker-compose.yml` environment entries over editing appsettings committed for local dev.

For sensitive local overrides (JWT signing key, SMTP credentials, database password), see `src/Template.Backend/src/Template.Backend.Web/secrets.json.example` and set values via [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) or environment variables — never commit real secrets.

Deployed stacks (runtime API URL, TLS, CORS, production checklist): [deployment.md](deployment.md).

## EF Core migrations

`InitialCreate` is included in `Infrastructure/Persistence/Migrations/`. In Development, **`DatabaseOptions:ApplyMigrationsOnStartup` is `true`** in `appsettings.Development.json` — pending migrations apply when the API starts (`dotnet run`, `npm run start:backend`, or Docker Compose).

When you change the EF model, add a migration: `npm run ef:migrations:add -- <Name>`.

For a one-off apply without starting the API: `npm run ef:database:update`.

**Production:** keep `ApplyMigrationsOnStartup` false (default in `appsettings.json`) and apply migrations from CI/CD rather than at app startup on many instances.

## PostgreSQL

- **Docker Compose** runs `postgres` (image `postgres:18`) and wires the API to that host.
- PostgreSQL **18+** official images store data under a versioned path; mount the named volume at **`/var/lib/postgresql`** (not `/var/lib/postgresql/data`). After upgrading from PostgreSQL 16/17 Compose volumes, run `npm run docker:down:volumes` once and recreate the stack, or migrate data with `pg_dump` / `pg_upgrade`.
- Default connection string for local dev: `src/Template.Backend/src/Template.Backend.Web/appsettings.Development.json`.

- **Integration tests** use disposable databases via Testcontainers (`BackendWebApplicationFactory`). The factory calls `MigrateAsync()`. A running Docker engine is required.

## Demo data (optional)

**System seed** (`ApplicationDataSeeder`) runs when migrations are applied at startup or via the DataGenerator tool: system roles and optional `InitialAdministratorOptions` from configuration.

**Demo dataset** (sample users, issues, comments, notifications) is separate. Use the DataGenerator console tool after migrations:

```powershell
npm run data:generate
npm run data:generate -- --reset
```

See [data-generator.md](data-generator.md) for architecture, configuration, and troubleshooting.

## Hangfire and background jobs

**Hangfire** stores job state in the **same database** as the application (`ConnectionStrings:DefaultConnection`). The API host runs a Hangfire server and registers recurring jobs at startup.

### Dashboard

| Setting                            | Default     | Purpose                                             |
| ---------------------------------- | ----------- | --------------------------------------------------- |
| `HangfireOptions:ServerEnabled`    | `true`      | Process background and recurring jobs on this host  |
| `HangfireOptions:DashboardPath`    | `/hangfire` | Browser UI path when the dashboard is on            |
| `HangfireOptions:DashboardEnabled` | `false`     | Expose the Hangfire dashboard (Development: `true`) |

Open `https://<api-host><DashboardPath>` when `DashboardEnabled` is `true` (for local dev: `http://localhost:<backend-port>/hangfire`).

The template ships **without dashboard authentication**. Restrict or disable the dashboard in production (`DashboardEnabled: false` is the default in `appsettings.json`).

### Recurring jobs

| Hangfire job id                   | Configuration                                        | Default schedule     | Purpose                                  |
| --------------------------------- | ---------------------------------------------------- | -------------------- | ---------------------------------------- |
| `attachment-storage-cleanup`      | `FileStorageOptions:CleanupCronExpression`           | `0 * * * *` (hourly) | Delete orphaned attachment files on disk |
| `notifications-retention-cleanup` | `NotificationRetentionOptions:CleanupCronExpression` | `0 3 * * *`          | Purge old in-app notifications           |

Notification retention days: `NotificationRetentionOptions` (`UnreadRetentionDays`, `ReadRetentionDays`, `AbsoluteRetentionDays`) in `appsettings.json`.

Cron expressions use standard five-field syntax (minute hour day month weekday). Changing a schedule requires an API restart so `RecurringJob.AddOrUpdate` runs again.

### Production notes

- Hangfire tables live in the application database — include them in backup/restore with the rest of the schema.
- Run **at least one** API instance with `HangfireOptions:ServerEnabled` `true` so recurring jobs execute. On additional HTTP-only replicas, set `ServerEnabled` to `false` to avoid duplicate job processing.
- Failed jobs appear in the dashboard; fix the underlying issue and retry or delete stale jobs there.

## File storage (issue attachments)

Issue attachment **file bytes** are stored on disk under **`FileStorage:RootPath`**, not in the database. **Metadata** (file name, size, content type, opaque storage key) lives in the shared **`attachments`** table using **TPH** inheritance: the **`Type`** column (EF discriminator, `AttachmentType` enum stored as string) distinguishes types; **`IssueAttachment`** (`Type` = `Issue`) is the current derived type. New attachment owners add another enum value, subclass, and storage container name.

### Layout

```
{FileStorage:RootPath}/
  {container}/
    {ownerId}/
      {storageKey}
```

Example for issues (`StorageContainer` = `"Issue"` from `IssueConstraints.STORAGE_CONTAINER`):

```
{FileStorage:RootPath}/Issue/{issueId}/{storageKey}
```

- **`storageKey`** is server-generated (GUID); user file names are never used as paths.
- Default local dev path: **`../../storage`** (relative to the Web project working directory).
- **Docker Compose** mounts a named volume at **`/app/storage`** and sets **`FileStorageOptions__RootPath=/app/storage`** on the `backend` service so attachments survive container restarts.

### Configuration

Settings live under **`FileStorage`** in `appsettings.json` / environment variables:

| Setting                                    | Default         | Purpose                                               |
| ------------------------------------------ | --------------- | ----------------------------------------------------- |
| `RootPath`                                 | `../../storage` | Root directory for all stored files                   |
| `CleanupCronExpression`                    | `0 * * * *`     | Schedule for orphaned-file reconciliation             |
| `CleanupConcurrentExecutionTimeoutSeconds` | `3600`          | Hangfire lock timeout for cleanup job (avoid overlap) |

Per-feature upload limits (count, size, allowed extensions) live in domain constraints such as **`IssueConstraints`** in `Domain/Aggregates/Issue/Issue.cs`, not in `FileStorage` options. Content inspection uses **`IFileContentValidator`** (Mime-Detective).

### Retention and cleanup

- Attachment metadata and stored files are kept until the attachment is deleted or the owning issue is deleted.
- **`AttachmentStorageCleanupJob`** (Hangfire) deletes **orphaned files** on disk that have no matching row in **`attachments`** (for example after a failed upload or process crash between storage write and DB commit). Schedule and dashboard: [Hangfire and background jobs](#hangfire-and-background-jobs).
- Deleting an issue cascades attachment metadata and deletes all stored files for that issue.

### Backup (production)

Include **`{FileStorage:RootPath}/Issue/`** in your backup plan **together with the application database**. Restoring only the database without files leaves broken download links; restoring files without matching metadata leaves orphaned blobs (cleaned up eventually by the cleanup job, but downloads will fail until then).

For cloud deployments, consider moving **`IFileStorageService`** to object storage (S3, Azure Blob) with server-side encryption and lifecycle policies; the Issues slice is the reference pattern for metadata + opaque keys.
