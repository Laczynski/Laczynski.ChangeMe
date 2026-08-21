# ChangeMe.Backend.DataGenerator

Demo/test data generator for local ChangeMe development.

## Purpose

This tool fills the database with sample users, issues, comments, acceptance criteria, watchers, and notifications so you can exercise the frontend and API without entering data manually.

**Run only in Development.** Do not use in production.

## Prerequisites

1. A running PostgreSQL database (Docker Compose or local instance).
2. EF Core migrations applied (automatic on API startup in Development, or `npm run ef:database:update` from the repository root).
3. A root `.env` copied from `.env.example`; the generator loads the same local connection string as the API and EF Core tooling.

## Running

From the repository root:

```powershell
npm run data:generate
```

Refresh demo data (removes existing `@demo.local` accounts and related records, then regenerates):

```powershell
npm run data:generate -- --reset
```

Directly via `dotnet`:

```powershell
dotnet run --project src/ChangeMe.Backend/tools/ChangeMe.Backend.DataGenerator/ChangeMe.Backend.DataGenerator.csproj
```

## Behavior

1. Applies pending migrations and system seed (roles, optional administrator from `InitialAdministrator`).
2. If demo users (`*@demo.local`) already exist — exits without changes (unless you pass `--reset`).
3. Generates data according to the `DataGenerator` section in the Web `appsettings.Development.json`.

## Demo accounts

By default, accounts `user1@demo.local` through `user8@demo.local` are created with the password from `DataGenerator:DefaultPassword` (default: `Demo123!`).

The optional system administrator configured in the root `.env` is left unchanged.

## Configuration

`DataGenerator` section in `src/ChangeMe.Backend/src/ChangeMe.Backend.Web/appsettings.Development.json`:

| Key                               | Description                            |
| --------------------------------- | -------------------------------------- |
| `Seed`                            | Bogus seed (repeatable data)           |
| `Users`                           | Number of demo accounts                |
| `Issues`                          | Number of issues                       |
| `CommentsPerIssueMin` / `Max`     | Comment count range per issue          |
| `NotificationsPerUserMin` / `Max` | Notification count range per demo user |
| `DefaultPassword`                 | Shared password for demo accounts      |
| `EmailDomain`                     | Email domain (e.g. `demo.local`)       |

## More information

Full documentation: `docs/modules/backend/demo-data.md` at the repository root.
