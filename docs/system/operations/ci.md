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

Four jobs run **in parallel** (no job depends on another):

| Job                       | What it runs                                                       | Working directory       |
| ------------------------- | ------------------------------------------------------------------ | ----------------------- |
| **Documentation**         | `npm ci` → docs validation → requirements validation               | Repository root         |
| **Frontend**              | lint/format → unit tests → production build                        | `src/ChangeMe.Frontend` |
| **Backend**               | restore → format → unit and integration tests → build              | Repository root         |
| **Deployment automation** | inventory/generator tests → Ansible syntax/lint → package contract | Repository root         |

#### Documentation

`npm run docs:validate` is the only public documentation check. It validates common links, ownership, type contracts, names, and index reachability, then runs the internal requirements validator for `FR-*` / `NFR-*` frontmatter, cross-references, change records, and generated indexes. It fails when a generated requirements index was stale, after updating that file locally, so CI also verifies that generated output is committed. See `docs/requirements/requirements-change-process.md`.

Run locally after documentation or specification changes:

```powershell
npm run docs:validate
```

#### Frontend

- Node.js **24.15** (Angular 22 CLI minimum; see `engines` in `src/ChangeMe.Frontend/package.json`)
- ESLint and Prettier checks, then tests run once (no watch), then production **build**
- Local equivalents: `npm run lint:frontend`, `npm run format:check:frontend`

#### Backend

- .NET **10**
- `dotnet format --verify-no-changes` (migrations excluded), then `dotnet test` and `dotnet build` on `ChangeMe.Backend.slnx` in **Release**
- **Integration tests** use Testcontainers (Docker). GitHub-hosted `ubuntu-latest` runners provide Docker; local runs need a running Docker engine too.

#### Deployment automation

The job runs the same `npm run setup:deployment` and `npm run validate:deployment` entry points used locally. Python 3.13 installs the pinned `ansible-core` and `ansible-lint` versions into `.venv-deploy`; validation covers the full inventory, generated GitLab child pipeline, GitLab YAML structure, Python generator tests, every playbook, and deterministic archive contract. It does not connect to an external VPS.

### What template repository CI does not cover

| Check                                                | Local command / workflow                                                                                          |
| ---------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| Full stack in Docker                                 | `npm run docker:up`                                                                                               |
| Backend tests only in Compose                        | `npm run docker:test:backend`                                                                                     |
| Browser E2E journeys                                 | Local/manual `npm run test:e2e`; post-deployment automation is deferred until environments and access are defined |
| Real GitLab pipeline lint and protected environments | Validate in the target GitLab project before the first production deployment                                      |
| VPS bootstrap/deploy/recovery                        | Exercise on a disposable supported Linux VPS; see [deployment](deployment.md)                                     |
| **Template publishing**                              | Source repository only: push a `v*` tag → see [publishing](publishing.md)                                         |
| **Dependency updates**                               | Dependabot opens weekly PRs — [dependabot.yml](../../../.github/dependabot.yml)                                   |

For test scope and project layout, see [testing strategy](../development/testing-strategy.md).

### Publish workflow

Separate from CI — runs on **tag push** `v*`. Tests, packs the `Laczynski.ChangeMe` template, publishes NuGet (nuget.org + GitHub Packages), and creates a GitHub Release.

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
npm run setup:deployment
npm run validate:deployment
```

Or approximate the full automated check:

```powershell
npm run test:all
npm run build:all
```

(`test:all` does not include documentation validation, requirements validation, or frontend build.)

## Generated application GitLab CI

Source of truth: [`.gitlab-ci.yml`](../../../.gitlab-ci.yml) with local includes under [`.gitlab/ci/`](../../../.gitlab/ci/).

The main pipeline is static. Only the child pipeline containing environment-specific deployment jobs is generated dynamically.

### Pipeline modes

| Trigger                                          | Verification               | Package                                                       | Deployment plan                                                                                   |
| ------------------------------------------------ | -------------------------- | ------------------------------------------------------------- | ------------------------------------------------------------------------------------------------- |
| Merge request                                    | Runs all verification jobs | Not created                                                   | Not created                                                                                       |
| Default branch without `application-version`     | Runs all verification jobs | Not created                                                   | Not created                                                                                       |
| Protected stable `vX.Y.Z` tag                    | Runs all verification jobs | Builds and publishes the tagged package plus a GitLab Release | Generates manual jobs for the package and configuration from the tagged commit                    |
| Default branch with `application-version=vX.Y.Z` | Skipped intentionally      | Reuses that existing registry package                         | Generates manual jobs for the selected package and configuration from the current pipeline commit |

The last mode supports a configuration-only deployment without rebuilding the application. In every deployment mode, the exact pipeline commit is the non-secret configuration revision. No environment deploys automatically.

### Stages and jobs

Jobs inside a stage can run in parallel unless a `needs` dependency states otherwise. A later stage starts only after the required work from earlier stages succeeds.

| Stage          | Job                      | Responsibility and output                                                                                                                                                                                                         |
| -------------- | ------------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| `validate`     | `documentation:verify`   | Installs the root npm dependencies and runs `docs:validate`, including documentation links, ownership/index contracts, and requirement references.                                                                                |
| `validate`     | `deployment:verify`      | Resolves the Ansible inventory, exercises the deployment-pipeline generator, compiles its Python source, validates the GitLab CI definitions, runs generator unit tests, checks every playbook's syntax, and runs `ansible-lint`. |
| `validate`     | `package:verify`         | Runs the shell contract test that proves the application package can be built deterministically and has the expected archive, manifest, and checksum structure.                                                                   |
| `test`         | `frontend:verify`        | Installs frontend dependencies, checks linting and formatting, runs unit tests, performs the production build, and publishes `artifacts/frontend/browser/` for release packaging.                                                 |
| `test`         | `backend:verify`         | Restores the .NET solution, verifies formatting, and runs all backend unit and integration tests. Testcontainers use the job's Docker-in-Docker service.                                                                          |
| `build`        | `release:backend`        | For a stable tag, publishes the backend as a self-contained `linux-x64` application and exposes it as a short-lived job artifact.                                                                                                 |
| `build`        | `release:package`        | For a stable tag, combines the verified frontend and published backend into one deterministic archive, creates its manifest and SHA-256 file, and extracts that version's notes from `CHANGELOG.md`.                              |
| `release`      | `release:publish`        | Uploads the archive, manifest, and SHA-256 file to GitLab's Generic Package Registry. Re-publishing identical bytes succeeds; different bytes under the same version fail as an immutable-package conflict.                       |
| `release`      | `release:create`         | After package publication, creates the GitLab Release from the versioned changelog section and links its archive, manifest, and SHA-256 asset. Missing release notes fail the tag pipeline.                                       |
| `plan`         | `deploy:generate`        | Chooses the stable tag or `application-version`, pairs it with `CI_COMMIT_SHA`, validates the Ansible inventory, and saves the generated child pipeline as an artifact. For a new tag it waits for `release:publish`.             |
| `deploy`       | `deploy:environments`    | Starts the dynamic child pipeline from the artifact produced by `deploy:generate`. This is a bridge job, not a server deployment.                                                                                                 |
| Child `deploy` | `deploy:<instance>`      | One manual job per enabled inventory host. It downloads the immutable package and checksum, verifies SHA-256, configures strict SSH host checking, and runs the Ansible deployment playbook for only that instance.               |
| Child `deploy` | `deploy:no-environments` | Replaces the per-instance jobs when no inventory host is enabled and reports that there is nothing to deploy. It does not contact a server.                                                                                       |

### Deployment pipeline generator

[`generate-deployment-pipeline.py`](../../../deploy/scripts/generate-deployment-pipeline.py) is called by `deploy:generate`; it does not generate the verification or release stages. It accepts:

- the Ansible inventory path;
- an application version in the stable `vX.Y.Z` form;
- a full lowercase 40-character configuration commit SHA;
- the destination path for the generated child-pipeline artifact.

The generator loads the inventory through `ansible-inventory --list`, so Ansible group variables, host variables, and inheritance are resolved before validation. It validates every application instance, including disabled ones. The checks cover identifiers, URLs and ports, deployment tier and users, the managed application directory layout, the fixed backend loopback endpoint, allowed non-secret backend configuration, and other values passed into deployment. An enabled instance additionally needs a non-example host and at least one versioned SSH public key. Enabled GitLab environment names must be unique.

After validation, the generator emits one `deploy:<instance>` job for every host whose `deployment_enabled` value is `true`. Each job extends [`.deploy-application`](../../../.gitlab/ci/deploy-job.yml) and receives only the selected application version, configuration commit, instance, environment, public URL, and deployment user. If no host is enabled, it emits `deploy:no-environments` instead. Any validation error stops `deploy:generate`, so GitLab never creates manual deployment buttons from an invalid inventory.

The generated job template supplies the remaining deployment behavior: environment-scoped SSH file variables, strict `known_hosts` verification, an immutable package download using `CI_JOB_TOKEN`, checksum verification, an instance-specific `resource_group`, and the Ansible playbook invocation. The inventory remains the source of non-secret environment configuration; GitLab variables remain the source of secrets.

The backend verification job uses Docker-in-Docker for Testcontainers and therefore needs an appropriately isolated privileged runner. Deployment jobs need outbound SSH access to their selected hosts. GitLab credentials, environment protection, approvals, runner networking, and the GitLab instance's own CI lint must be validated in the generated project's infrastructure.

Release and VPS procedures: [deployment](deployment.md).
