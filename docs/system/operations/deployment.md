# Deployment

> Type: operations
> Scope: system
> Status: implemented
> Canonical for: GitLab application releases and native Linux VPS bootstrap, deployment, verification, and rollback
>
> Local Compose: [local full-stack environment](local-stack.md). Migrations: [backend persistence](../../modules/backend/operations/persistence.md).

## Summary

- Docker is used locally only. Deployed instances run a self-contained ASP.NET Core backend under systemd and serve the Angular frontend through nginx.
- A protected `vX.Y.Z` Git tag verifies and publishes one immutable `linux-x64` package plus a GitLab Release. It does not deploy automatically.
- GitLab generates an independent manual action for every enabled inventory instance. The same package can be selected for development or production environments.
- Git versions all non-secret instance configuration. Each VPS owns its secret values in `/etc/<application>/secrets.env`.
- The supported baseline is one application instance per Ubuntu 24.04 or Debian 12 x86-64 VPS with externally managed PostgreSQL.

## Runtime topology and paths

nginx serves `/opt/<application>/current/frontend`, exposes the active server-rendered `/runtime-config.js`, and proxies `/api/` plus `/hubs/` to the backend on `127.0.0.1:5000`. systemd runs `/opt/<application>/current/backend/<backend-executable>` as the unprivileged `appsvc` account.

```text
/opt/<application>/releases/vX.Y.Z/        immutable application releases
/opt/<application>/current                 active application symlink
/opt/<application>/previous                previous application symlink
/etc/<application>/config-revisions/<sha>/ immutable non-secret configuration
/etc/<application>/current-config          active configuration symlink
/etc/<application>/previous-config         previous configuration symlink
/etc/<application>/secrets.env             server-owned secrets, root:root 0600
/var/lib/<application>/storage/             persistent files
/var/lib/<application>/deployment.json      active deployment record
/var/log/<application>/                     service-writable logs
```

Application packages never contain host configuration or secrets. Releases and configuration revisions are retained separately; active and previous revisions are protected from automatic retention cleanup.

## Set up local deployment tooling

The Ansible control node is supported on native Linux or Windows through a default WSL distribution. Native Windows Python and Git Bash are not supported. According to the [ansible-core support matrix](https://docs.ansible.com/projects/ansible-core/devel/reference_appendices/release_and_maintenance.html), the pinned version requires control-node Python 3.12-3.14; local validation additionally uses Node.js, OpenSSH, `tar`, and `sha256sum`.

On Ubuntu or Debian, install the system prerequisites first. Install a Node.js version allowed by `src/Template.Frontend/package.json` separately when it is not already available inside WSL:

```bash
sudo apt update
sudo apt install python3 python3-venv openssh-client tar coreutils
```

From the repository root, in PowerShell, Linux, or WSL, run:

```powershell
npm run setup:deployment
```

On Windows the npm wrapper translates the repository path and runs the setup inside the default WSL distribution. On Linux it runs directly. Setup checks the platform and prerequisites, creates the ignored `.venv-deploy`, and installs `requirements-ci.txt`; that file includes both runtime Ansible and validation tooling. It is safe to rerun and does not modify the system Python environment.

The npm validation command does not require manual activation. For direct Ansible commands in an interactive Linux/WSL shell, activate and configure the environment:

```bash
. .venv-deploy/bin/activate
export ANSIBLE_CONFIG="$PWD/deploy/ansible/ansible.cfg"
```

## Configure an instance in Git

Run `npm run setup:deployment` before editing or validating the inventory.

To add an instance:

1. Copy `deploy/ansible/examples/new-instance.yml` to `deploy/ansible/inventory/host_vars/<instance-id>.yml`.
2. Add the same ID under `application_instances.hosts` in `deploy/ansible/inventory/hosts.yml`.
3. Replace host names, URLs, tier, GitLab environment name, and `backend_config` values.
4. Add at least one SSH public key as `deployment_public_keys`. This is the public half of the protected GitLab deployment key.
5. Leave `deployment_enabled: false` until the server secret file and one-time bootstrap are ready. Enable it through a reviewed merge request.

Only scalar keys listed by `allowed_backend_config_keys` may appear in `backend_config`. Connection strings, JWT signing keys, SMTP credentials, and bootstrap-administrator values are rejected there and belong on the server. Validate the complete inventory before merge:

```powershell
npm run validate:deployment
```

The generator rejects unsafe paths, enabled `.invalid` examples, duplicate environments, unrecognized config keys, and secrets committed as configuration.

## Provision server secrets

The application deliberately does not load a production repository `.env`. Standard ASP.NET Core environment variables are supplied through systemd environment files.

Before bootstrap, an administrator creates the configuration directory and transfers the secret file through the server's controlled administration path:

```bash
sudo install -d -o root -g root -m 0755 /etc/<application>
sudo install -o root -g root -m 0600 /secure-transfer/secrets.env /etc/<application>/secrets.env
```

The required variables are:

```dotenv
ConnectionStrings__DefaultConnection=Host=database.internal;Database=Application;Username=application;Password=replace-on-vps
AuthOptions__Jwt__SigningKey=replace-with-a-unique-key-of-at-least-32-bytes
```

Optional allowed secrets include SMTP username/password and initial-administrator fields. The deployment automation validates file ownership, mode, syntax, allowlisted names, and required non-empty keys without returning values to Ansible or CI logs. It never creates or updates this file.

After a controlled secret rotation, restart and verify the existing service:

```bash
sudo systemctl restart <application>-backend
sudo systemctl is-active --quiet <application>-backend
curl --fail --silent --show-error https://application.example/health
```

## Bootstrap a VPS once

Set the instance's real deployment public key and temporarily set `deployment_enabled: true` in the checked-out inventory used for bootstrap. Connect as the initial administrative account; the playbook creates the long-lived `deploy` account and application service account:

```bash
ansible-playbook \
  --inventory deploy/ansible/inventory/hosts.yml \
  --limit <instance-id> \
  --user <initial-admin> \
  --private-key <initial-admin-key> \
  --ask-become-pass \
  deploy/ansible/playbooks/bootstrap.yml
```

Bootstrap installs nginx and njs, creates the managed directories, installs systemd/nginx definitions, installs versioned deployment public keys, checks the secret file, and validates nginx. The `deploy` account receives passwordless root sudo because routine Ansible manages root-owned services and paths. Treat its private key as a root-equivalent credential.

Bootstrap does not configure a firewall, provision PostgreSQL, create backups, issue certificates, change server secrets, or install Docker. When nginx terminates TLS, provision the certificate and key separately and set `nginx_tls_enabled`, `nginx_tls_certificate_path`, and `nginx_tls_certificate_key_path`. Leave TLS disabled when HTTPS terminates at an upstream proxy.

Verify access with the deployment key:

```bash
ansible-playbook \
  --inventory deploy/ansible/inventory/hosts.yml \
  --limit <instance-id> \
  --user deploy \
  --private-key <deployment-key> \
  deploy/ansible/playbooks/verify.yml
```

The first `verify.yml` succeeds only after an application release is active; immediately after bootstrap, use `nginx -t` and proceed with the first deployment.

## Configure GitLab protection and credentials

For every enabled value of `gitlab_environment`, configure two **File** variables scoped to that exact environment:

| Variable | Content |
| --- | --- |
| `DEPLOY_SSH_PRIVATE_KEY` | Private key matching a versioned `deployment_public_keys` entry |
| `DEPLOY_KNOWN_HOSTS` | Pre-verified `known_hosts` line for the inventory host and port |

Protect both variables, the `vX.Y.Z` tag pattern, and the production environment. Restrict who may run manual production jobs and require deployment approvals where available. The GitLab runner needs outbound SSH access to every selected VPS. Backend integration tests also require a privileged runner capable of Docker-in-Docker/Testcontainers.

Do not use `ssh-keyscan` inside a deployment job as the trust decision. Capture and verify the server host key through an administrative channel, then store it in `DEPLOY_KNOWN_HOSTS`.

## Prepare a release through a merge request

The stable Git tag is the canonical application version; application releases do not maintain a second version in `package.json` or .NET project files. Every release is prepared on a `release/vX.Y.Z` branch with a dated, non-empty version section in `CHANGELOG.md`, reviewed through an MR, and tagged only after that MR and its required pipeline succeed.

Cursor and Claude Code expose the same repository-owned `/release` workflow:

```text
/release patch              # calculate the next version, write notes, verify, and open the MR
/release publish v1.4.1     # after merge, verify default branch and push the protected tag
```

The Claude Code implementation is [the release skill](../../../.claude/skills/release/SKILL.md); Cursor's [release command](../../../.cursor/commands/release.md) delegates to that same procedure. The workflow never merges an open MR, publishes from an unmerged branch, bypasses failed checks, or runs a manual environment deployment without explicit authorization.

## Release and deploy

A stable tag pipeline performs documentation, deployment-definition, package, frontend, backend, and integration verification. E2E is not a CI or release gate; future post-deployment execution is deferred until target access and configuration are defined.

The tag version must have a non-empty `## [X.Y.Z]` or `## X.Y.Z` section in `CHANGELOG.md`. CI uses that section as the GitLab Release description and publishes:

```text
<project-slug>-vX.Y.Z-linux-x64.tar.gz
<project-slug>-vX.Y.Z-linux-x64.tar.gz.sha256
<project-slug>-vX.Y.Z-linux-x64.tar.gz.manifest.json
```

The archive contains the self-contained backend, production frontend, internal checksums, and manifest. Publication is idempotent only when existing registry bytes are identical; a tag cannot silently replace a different package. After package publication, GitLab creates a Release for the tag with the versioned notes and direct links to all three registry assets.

After publication, a generated child pipeline exposes one blocking manual job per enabled instance. Each job shows the package version, target GitLab environment, and configuration commit and has its own `resource_group`. Running one job does not start any other environment.

To reuse a published package with configuration from a newer commit, run a default-branch pipeline in GitLab and set the pipeline input `application-version` to an existing stable version such as `v1.4.0`. Verification/build jobs are skipped, the current full commit becomes the configuration revision, and the same manual environment choices are generated.

Deployment verifies both package checksums, stages immutable package and configuration revisions, runs the new backend with `--migrate-only`, switches symlinks, restarts systemd, and waits for health. The deployment record contains application version/checksum, package commit, configuration commit, deployment ID, and pipeline URL.

Normal startup keeps `DatabaseOptions:ApplyMigrationsOnStartup=false`. The migration-only command applies pending EF Core migrations and the idempotent bootstrap seed, then exits without starting the HTTP host.

## Configuration-only operation

The standard GitLab path may redeploy the selected existing package with a newer configuration. For an explicit controller-side configuration-only activation, check out the desired commit and run:

```bash
ansible-playbook \
  --inventory deploy/ansible/inventory/hosts.yml \
  --limit <instance-id> \
  --user deploy \
  --private-key <deployment-key> \
  --extra-vars "application_config_revision=$(git rev-parse HEAD)" \
  deploy/ansible/playbooks/deploy-config.yml
```

This stages the revision, switches only the configuration symlink, restarts the existing backend, health-checks it, and restores the prior configuration on failure.

## Rollback and failure recovery

Automatic recovery restores previous application and configuration symlinks if service startup or health verification fails. It does not reverse migrations. A release must keep its schema compatible with the previous retained version or document a database restore procedure before deployment.

To select explicit retained revisions:

```bash
ansible-playbook \
  --inventory deploy/ansible/inventory/hosts.yml \
  --limit <instance-id> \
  --user deploy \
  --private-key <deployment-key> \
  --extra-vars "rollback_application_version=v1.3.0" \
  --extra-vars "rollback_config_revision=<full-commit-sha>" \
  deploy/ansible/playbooks/rollback.yml
```

Rollback verifies the retained package's internal checksums, switches both links, restarts, and health-checks. If rollback health fails, it restores the revisions that were active when rollback began.

Both GitLab and the server serialize deployments. A server lock is an atomic directory at `/run/lock/<application>-deployment`; a second run fails instead of racing. If a process is interrupted and leaves a stale lock, first verify that no GitLab deployment, Ansible process, or migration one-shot unit is active. Remove the exact lock directory only after that administrative inspection—automation intentionally does not guess that a lock is stale.

Partial immutable release/configuration directories are never overwritten. Inspect and remove an incomplete exact revision manually before retrying. Retention keeps the newest configured number of revisions and never removes active or previous targets. Persistent storage is outside releases and is never removed by deployment or rollback.

## Production configuration checklist

- Keep a unique `AuthOptions__Jwt__SigningKey` of at least 32 bytes and a production PostgreSQL connection string in `secrets.env`.
- Configure real SMTP settings; MailHog is local only. Remove initial-administrator secret fields after bootstrap.
- Keep rate limiting enabled in production. Forward `X-Forwarded-For` and `X-Forwarded-Proto` at every upstream proxy.
- Keep Swagger and the Hangfire dashboard disabled unless access is explicitly restricted.
- At least one application instance must run the Hangfire server when background jobs are required.
- Back up PostgreSQL and persistent application storage independently of releases. Exercise restore before a migration that cannot be rolled back safely.
- Use same-origin `/api/v1` unless a split frontend/API origin is intentional. Split origins also require the matching `CorsOptions__AllowedOrigins` configuration.

Production frontend configuration is never baked into the package. The active configuration revision renders:

```javascript
window.__CHANGE_ME_CONFIG__ = {
  apiUrl: "/api/v1",
};
```

## Local tooling troubleshooting

| Symptom | Cause and recovery |
| --- | --- |
| `A working default WSL distribution is required` | Follow the [Microsoft WSL installation instructions](https://learn.microsoft.com/en-us/windows/wsl/install), restart if requested, launch the installed distribution once to finish initialization, and rerun setup. Use `wsl --set-default <Distribution>` when several distributions exist. |
| `Deployment tooling must run on Linux or WSL` | The shell script was started through Git Bash or native Windows Python. Run the npm command from PowerShell so it can hand off to WSL, or run it directly inside WSL. |
| `Missing prerequisite: python3` or unsupported Python | Install a Python version from the supported 3.12-3.14 range inside Linux/WSL. The Windows Python installation is unrelated. |
| Virtual environment creation fails or reports missing `ensurepip` | On Ubuntu/Debian install `python3-venv`. If the repository moved and the existing venv contains old absolute paths, move the exact `.venv-deploy` aside and rerun setup. |
| `Missing prerequisite: node`, `ssh`, `ssh-keygen`, `tar`, or `sha256sum` | Install the named program inside the same Linux/WSL distribution. `ssh` and `ssh-keygen` come from `openssh-client`; `sha256sum` comes from `coreutils`. |
| Validation reports a missing `.venv-deploy/bin/...` executable | Setup did not finish or used a different checkout. Run `npm run setup:deployment` from the current repository root. |
| Inventory, roles, or configuration cannot be found during a manual command | Start at the repository root, activate `.venv-deploy`, and export the repository's `ANSIBLE_CONFIG` as shown above. The npm validation command sets it automatically. |
| SSH returns `Host key verification failed` | Verify the server fingerprint through an administrative channel and update the applicable `known_hosts` file or GitLab file variable. Do not disable host-key checking and do not use `ssh-keyscan` as the trust decision. |
| SSH returns `Permission denied (publickey)` | Confirm the target inventory host/user, private-key permissions, and that its public half is present in `deployment_public_keys` and installed on the server. Do not print private keys while diagnosing. |
| `pip` cannot reach the registry or rejects a certificate | Fix WSL/Linux DNS, proxy, or CA trust. Do not disable TLS verification. Rerun setup after connectivity is restored. |

## Verification

Before enabling a real environment, run the repository deployment checks:

```powershell
npm run validate:deployment
```

The command resolves the complete inventory, exercises the child-pipeline generator, validates GitLab YAML, runs deployment unit tests, syntax-checks and lints every playbook, and verifies the deterministic package contract. GitLab separates the package contract into its parallel `package:verify` job but uses the same underlying validation scripts for the Ansible portion.

Repository checks do not validate project-specific GitLab permissions, environment scoping, runner networking, DNS, TLS, firewall rules, database reachability, or backups. Exercise bootstrap, first deployment, forced health failure, retained rollback, and restore on a disposable target before production.

## Related docs

| Topic | Document |
| --- | --- |
| Delivery rationale and contracts | [multi-environment application delivery](../designs/multi-environment-application-delivery-design.md) |
| Local Docker Compose | [local full-stack environment](local-stack.md) |
| PostgreSQL and migrations | [backend persistence](../../modules/backend/operations/persistence.md) |
| Hangfire | [backend background jobs](../../modules/backend/operations/background-jobs.md) |
| File storage | [backend file storage](../../modules/backend/operations/file-storage.md) |
| CI | [continuous integration](ci.md) |
| Template package publishing | [publishing](publishing.md) |
