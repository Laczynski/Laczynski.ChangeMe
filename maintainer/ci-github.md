# GitHub Actions CI (template source repository)

> Scope: **Laczynski/Laczynski.ChangeMe** on GitHub only — **not shipped** in `dotnet new` output. Generated applications use [GitLab CI](../docs/system/operations/ci.md).

## Pipeline ownership

| Pipeline | Files | Runs where |
| --- | --- | --- |
| **Template maintainer CI** | [`.github/workflows/ci.yml`](../.github/workflows/ci.yml), [`.github/workflows/publish.yml`](../.github/workflows/publish.yml) | This GitHub repository |
| **Generated application CI** | [`.gitlab-ci.yml`](../.gitlab-ci.yml), [`.gitlab/ci/`](../.gitlab/ci/) | Consumer GitLab project after `dotnet new` |

GitHub Actions runs **`npm run validate:deployment`** to lint-check GitLab YAML and Ansible assets without executing GitLab jobs on GitHub.

| Release artifact | Action | Documentation |
| --- | --- | --- |
| NuGet template **`ChangeMe`** | Push **`v*`** tag on this repo | [publishing](publishing.md) |
| Application **`vX.Y.Z`** on GitLab | MR + protected tag in generated repo | [deployment](../docs/system/operations/deployment.md) |

## CI workflow

Source of truth: [`.github/workflows/ci.yml`](../.github/workflows/ci.yml).

### Triggers

Runs on **push** and **pull_request** to `main` or `master`. Concurrent runs for the same branch are cancelled when a newer commit is pushed.

### Jobs

Four jobs run **in parallel**:

| Job | What it runs |
| --- | --- |
| **Documentation** | `npm ci` → `npm run docs:validate` |
| **Frontend** | lint/format → unit tests → production build |
| **Backend** | restore → format → unit and integration tests → build |
| **Deployment automation** | `npm run setup:deployment` → `npm run validate:deployment` |

Backend integration tests use Testcontainers (Docker). The deployment job validates the **GitLab** child pipeline generator and Ansible assets that ship in the template payload.

### What GitHub CI does not cover

| Check | Local alternative |
| --- | --- |
| Full stack in Docker | `npm run docker:up` |
| Browser E2E | `npm run test:e2e` (local/manual) |
| Real GitLab protected environments | Validate in the consumer GitLab project |
| VPS bootstrap/deploy | [deployment](../docs/system/operations/deployment.md) on a disposable VPS |
| **Template NuGet publish** | Push **`v*`** tag → [publishing](publishing.md) |
| Dependency updates | [dependabot.yml](../.github/dependabot.yml) |

### Reproduce locally

```powershell
npm run install:frontend
npm run docs:validate
npm run lint:frontend
npm run format:check:all
npm run test:frontend:ci
npm run build:frontend
npm run test:backend
npm run build:backend
npm run setup:deployment
npm run validate:deployment
```

Or approximate checks with `npm run test:all` and `npm run build:all` (skips docs validation, frontend build, and deployment validation).

Test scope: [testing strategy](../docs/system/development/testing-strategy.md).
