# Continuous integration

> Type: operations
> Scope: system
> Status: implemented
> Canonical for: human-readable GitHub and GitLab CI job ownership and local reproduction

## Template repository CI

Source of truth: [`.github/workflows/ci.yml`](../../../.github/workflows/ci.yml).

### Triggers

Runs on **push** and **pull_request** to `main` or `master`.

Concurrent runs for the same branch are cancelled (`cancel-in-progress: true`) when a newer commit is pushed.

### Jobs

Five jobs run **in parallel** (no job depends on another):

| Job              | What it runs                                                             | Working directory       |
| ---------------- | ------------------------------------------------------------------------ | ----------------------- |
| **Documentation** | `npm ci` → docs validation → requirements validation                    | Repository root         |
| **Frontend**     | `npm ci` → `npm test -- --watch=false` → `npm run build`                 | `src/Template.Frontend` |
| **Backend**      | `dotnet restore` → `dotnet test` → `dotnet build`                        | Repository root         |
| **Deployment automation** | Inventory/generator tests → Ansible syntax/lint → package contract | Repository root |
| **E2E**          | PostgreSQL service → `npm ci` → Playwright → smoke tests (`npm run e2e`) | `src/Template.Frontend` |

#### Documentation

`npm run docs:validate` is the only public documentation check. It validates common links, ownership, type contracts, names, and index reachability, then runs the internal requirements validator for `FR-*` / `NFR-*` frontmatter, cross-references, change records, and generated indexes. It fails when a generated requirements index was stale, after updating that file locally, so CI also verifies that generated output is committed. See `docs/requirements/requirements-change-process.md`.

Run locally after documentation or specification changes:

```powershell
npm run docs:validate
```

#### Frontend

- Node.js **24.15** (Angular 22 CLI minimum; see `engines` in `src/Template.Frontend/package.json`)
- ESLint and Prettier checks, then tests run once (no watch), then production **build**
- Local equivalents: `npm run lint:frontend`, `npm run format:check:frontend`

#### Backend

- .NET **10**
- `dotnet format --verify-no-changes` (migrations excluded), then `dotnet test` and `dotnet build` on `Template.Backend.slnx` in **Release**
- **Integration tests** use Testcontainers (Docker). GitHub-hosted `ubuntu-latest` runners provide Docker; local runs need a running Docker engine too.

#### Deployment automation

Python 3.13 installs the pinned `ansible-core` and `ansible-lint` versions from `deploy/ansible/requirements-ci.txt`. The job validates the full inventory, generated GitLab child pipeline, GitLab YAML structure, Python generator tests, every playbook, and deterministic archive contract. It does not connect to an external VPS.

#### E2E

- Node.js **24.15** (Angular 22 CLI minimum; see `engines` in `src/Template.Frontend/package.json`) and .NET **10** (same as Frontend / Backend jobs).
- **PostgreSQL 18** service container on the runner (`localhost:5432`).
- Playwright starts the backend and frontend dev servers, then runs the smoke suite in `src/Template.Frontend/e2e/features/`. The E2E job also starts a **MailHog** service on port `1025` (SMTP) so user-invitation flows can send mail.
- The job supplies explicit test-only connection, JWT, SMTP, and bootstrap-administrator environment variables; CI does not read a local `.env`.
- Reproduce locally: run `npm run install:frontend` once (Chromium), PostgreSQL on `localhost`, Docker available for MailHog, then `npm run test:e2e` from the repository root (see `AGENTS.md`).

### What template repository CI does not cover

| Check                         | Local command / workflow                                                     |
| ----------------------------- | ---------------------------------------------------------------------------- |
| Full stack in Docker          | `npm run docker:up`                                                          |
| Backend tests only in Compose | `npm run docker:test:backend`                                                |
| Real GitLab pipeline lint and protected environments | Validate in the target GitLab project before first production deploy |
| VPS bootstrap/deploy/recovery | Exercise on a disposable supported Linux VPS; see [deployment](deployment.md) |
| **Template publishing**      | Source repository only: push a `v*` tag → see [publishing](publishing.md)     |
| **Dependency updates**        | Dependabot opens weekly PRs — [dependabot.yml](../../../.github/dependabot.yml) |

For test scope and project layout, see [testing strategy](../development/testing-strategy.md).

### Publish workflow

Separate from CI — runs on **tag push** `v*`. Tests, packs the `Template` template, publishes NuGet (nuget.org + GitHub Packages), and creates a GitHub Release.

Details: [publishing](publishing.md).

### Reproduce template repository CI locally

From the repository root after `npm install`:

```powershell
npm run install:frontend
npm run docs:validate
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

(`test:all` does not include documentation validation, requirements validation, or frontend build.)

## Generated application GitLab CI

Source of truth: [`.gitlab-ci.yml`](../../../.gitlab-ci.yml) with local includes under [`.gitlab/ci/`](../../../.gitlab/ci/).

Merge requests and the default branch run documentation, deployment-definition, package, frontend, backend, and E2E verification. A protected stable `vX.Y.Z` tag additionally publishes one immutable `linux-x64` application archive to GitLab's Generic Package Registry and generates independent manual deployment jobs from enabled Ansible inventory hosts.

Running a default-branch pipeline with the `application-version` input selects an existing registry package and skips rebuilding the application. The exact pipeline commit supplies the non-secret configuration revision. Manual jobs use environment-scoped SSH file variables and a per-instance `resource_group`; no environment deploys automatically.

The backend verification job uses Docker-in-Docker for Testcontainers and therefore needs an appropriately isolated privileged runner. Deployment jobs need outbound SSH access to their selected hosts. GitLab credentials, environment protection, approvals, runner networking, and the GitLab instance's own CI lint must be validated in the generated project's infrastructure.

Release and VPS procedures: [deployment](deployment.md).
