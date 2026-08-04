# Continuous integration

> Scope: GitHub Actions workflow for this repository.
> **Source of truth:** [`.github/workflows/ci.yml`](../../.github/workflows/ci.yml) — update this file when CI changes; keep `ci.md` in sync.

## Triggers

Runs on **push** and **pull_request** to `main` or `master`.

Concurrent runs for the same branch are cancelled (`cancel-in-progress: true`) when a newer commit is pushed.

## Jobs

Four jobs run **in parallel** (no job depends on another):

| Job              | What it runs                                                             | Working directory       |
| ---------------- | ------------------------------------------------------------------------ | ----------------------- |
| **Requirements** | `npm ci` → `npm run requirements:validate`                               | Repository root         |
| **Frontend**     | `npm ci` → `npm test -- --watch=false` → `npm run build`                 | `src/ChangeMe.Frontend` |
| **Backend**      | `dotnet restore` → `dotnet test` → `dotnet build`                        | Repository root         |
| **E2E**          | PostgreSQL service → `npm ci` → Playwright → smoke tests (`npm run e2e`) | `src/ChangeMe.Frontend` |

### Requirements

Validates `docs/requirements/` structure: `FR-*` / `NFR-*` frontmatter, cross-references, change records, and regenerates `docs/requirements/README.md`. See `docs/requirements/requirements-change-process.md`.

Run locally before pushing specification changes:

```powershell
npm run requirements:validate
```

### Frontend

- Node.js **24.15** (Angular 22 CLI minimum; see `engines` in `src/ChangeMe.Frontend/package.json`)
- ESLint and Prettier checks, then tests run once (no watch), then production **build**
- Local equivalents: `npm run lint:frontend`, `npm run format:check:frontend`

### Backend

- .NET **10**
- `dotnet format --verify-no-changes` (migrations excluded), then `dotnet test` and `dotnet build` on `ChangeMe.Backend.slnx` in **Release**
- **Integration tests** use Testcontainers (Docker). GitHub-hosted `ubuntu-latest` runners provide Docker; local runs need a running Docker engine too.

### E2E

- Node.js **24.15** (Angular 22 CLI minimum; see `engines` in `src/ChangeMe.Frontend/package.json`) and .NET **10** (same as Frontend / Backend jobs).
- **PostgreSQL 18** service container on the runner (`localhost:5432`).
- Playwright starts the backend and frontend dev servers, then runs the smoke suite in `src/ChangeMe.Frontend/e2e/features/`. The E2E job also starts a **MailHog** service on port `1025` (SMTP) so user-invitation flows can send mail.
- Reproduce locally: run `npm run install:frontend` once (Chromium), PostgreSQL on `localhost`, Docker available for MailHog, then `npm run test:e2e` from the repository root (see `AGENTS.md`).

## What CI does not cover

| Check                         | Local command / workflow                                                     |
| ----------------------------- | ---------------------------------------------------------------------------- |
| Full stack in Docker          | `npm run docker:up`                                                          |
| Backend tests only in Compose | `npm run docker:test:backend`                                                |
| **Publishing**                | Push a `v*` tag → [publish.yml](../../.github/workflows/publish.yml)         |
| **Dependency updates**        | Dependabot opens weekly PRs — [dependabot.yml](../../.github/dependabot.yml) |

For test scope and project layout, see `docs/guides/testing-guidelines.md`.

## Publish workflow

Separate from CI — runs on **tag push** `v*`. Tests, packs the `ChangeMe` template, publishes NuGet (nuget.org + GitHub Packages), and creates a GitHub Release.

Details: [publishing.md](publishing.md).

## Reproduce CI locally

From the repository root after `npm install`:

```powershell
npm run install:frontend
npm run requirements:validate
npm run lint:frontend
npm run format:check:all
npm run test:frontend:ci
npm run build:frontend
npm run test:backend
npm run build:backend
npm run test:e2e
```

(`install:frontend` installs Playwright Chromium; `test:e2e` also needs PostgreSQL on `localhost` — same as local backend Development settings.)

Or approximate the full automated check:

```powershell
npm run test:all
npm run build:all
```

(`test:all` does not include `requirements:validate` or frontend build.)
