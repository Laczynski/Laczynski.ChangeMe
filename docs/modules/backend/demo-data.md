# Demo data

> Type: development
> Scope: backend
> Status: implemented
> Canonical for: optional demo dataset generation and reset

## Overview

`Template.Backend.DataGenerator` is a console project under `src/Template.Backend/tools/Template.Backend.DataGenerator/`. It is **not** part of the production API host and is **not** used by automated tests.

| Mechanism               | Purpose                                                                  |
| ----------------------- | ------------------------------------------------------------------------ |
| `ApplicationDataSeeder` | System roles and optional initial administrator (always-on product seed) |
| **DataGenerator**       | Optional demo dataset for UI and manual API exploration                  |

## Prerequisites

1. Database is reachable (see [backend persistence](operations/persistence.md)).
2. Migrations applied (automatic on API startup in Development, or `npm run ef:database:update` from the repository root).

3. `ConnectionStrings:DefaultConnection` in `src/Template.Backend/src/Template.Backend.Web/appsettings.Development.json` points at your database.

## Commands

From the repository root:

```powershell
npm run data:generate
npm run data:generate -- --reset
```

`--reset` removes existing demo users (emails ending with `@<EmailDomain>`) and related issues/notifications, then regenerates.

If demo data already exists and you omit `--reset`, the tool exits successfully without changes.

## What gets generated

- **Users** — `user1@demo.local`, `user2@demo.local`, … with the default `User` role
- **Issues** — varied title, description, status, priority, optional assignee
- **Issue children** — acceptance criteria, comments, watchers (via domain methods)
- **Notifications** — linked to issues and demo users

All inserts go through domain factories (`User.Create`, `Issue.Create`, etc.) and `ApplicationDbContext`, matching the integration-test pattern.

## Configuration

Settings live in the Web project `appsettings.Development.json` under `DataGenerator` (also documented in the tool [README](../../../src/Template.Backend/tools/Template.Backend.DataGenerator/README.md)).

The generator copies `appsettings.json` and `appsettings.Development.json` from `Template.Backend.Web` at build output time.

## Architecture

```text
npm run data:generate
  → Template.Backend.DataGenerator (console)
  → DatabaseConfig.InitializeDatabaseAsync (migrate + ApplicationDataSeeder)
  → DemoDataExistsChecker (skip or DemoDataCleaner on --reset)
  → UsersGenerator → IssuesGenerator → NotificationsGenerator
  → ApplicationDbContext.SaveChanges
```

## Troubleshooting

| Problem                    | Action                                                                                                     |
| -------------------------- | ---------------------------------------------------------------------------------------------------------- |
| No migrations found        | Start the API in Development or run `npm run ef:database:update` (see [persistence](operations/persistence.md)) |
| Connection refused         | Start Docker Compose or local DB; verify connection string                                                 |
| Demo data already exists   | Run with `--reset` or delete demo users manually                                                           |
| Migration history mismatch | Regenerate migrations with PostgreSQL; do not reuse migration snapshots from other database providers      |

## Tests

Integration tests use their own helpers (`IssueTestHelper`, `TestAuthHelper`) and Testcontainers — they do **not** invoke DataGenerator. Test layout and helpers: [repository map](../../system/development/repository-map.md#test-map).
