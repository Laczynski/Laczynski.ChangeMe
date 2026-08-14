# CI and deployment lab

Maintainer-only tooling to exercise **GitLab CI** and **Ansible deployment** locally. This folder is excluded from `dotnet new` output (see `.template.config/template.json`).

It does **not** run GitHub Actions. Reproduce template-repository CI with the commands in `docs/system/operations/ci.md`, or use [`act`](https://github.com/nektos/act) for workflow dry-runs.

## What this lab covers

| Goal                                   | Tooling                                                                          |
| -------------------------------------- | -------------------------------------------------------------------------------- |
| GitLab verify / release / registry     | GitLab CE + Docker executor runner in Compose                                    |
| VPS bootstrap / deploy / rollback      | Ubuntu 24.04 VM via [Multipass](https://multipass.run/)                          |
| Static pipeline validation (no GitLab) | `deploy/scripts/validate-gitlab-ci.py` and related checks (already in GitHub CI) |

## Prerequisites

- Docker Desktop (≥ 8 GB RAM allocated to Docker)
- [Multipass](https://multipass.run/install) for the deployment target VM
- **Windows Home:** install [Oracle VirtualBox](https://www.virtualbox.org/) (Multipass uses the `virtualbox` driver; Hyper-V is unavailable)
- PowerShell 7+ on Windows

## Quick start

One-shot setup (GitLab + VM):

```powershell
cd ci-lab
Copy-Item .env.example .env
docker compose up -d
.\scripts\setup-lab.ps1
```

Or step by step:

### 1. Start local GitLab

```powershell
cd ci-lab
Copy-Item .env.example .env
.\scripts\bootstrap-gitlab.ps1
```

First boot can take several minutes. Sign in at `http://gitlab.local:8929` (hosts entry is added automatically). Read the initial `root` password:

```powershell
docker compose exec gitlab grep 'Password:' /etc/gitlab/initial_root_password
```

Change the root password after first login.

### 2. Create a project and register a runner

1. In GitLab: **New project** → **Create blank project** (for example `template/ci-lab`).
2. **Admin** → **CI/CD** → **Runners** → **New instance runner** → copy the runner authentication token.
3. Register the Docker executor (privileged, for Testcontainers / DinD jobs):

```powershell
.\scripts\register-runner.ps1 -RunnerToken "<token>"
```

### 3. Mirror this repository

```powershell
git remote add ci-lab ssh://git@gitlab.local:2224/template/ci-lab.git
.\scripts\mirror-to-gitlab.ps1
```

Push tags when testing release jobs, for example `git push ci-lab v0.0.1`.

### 4. Create the deployment target VM

```powershell
.\scripts\new-vm.ps1
.\scripts\prepare-vm.ps1
```

Copy `inventory/host_vars/ci-lab.yml.example` to `deploy/ansible/inventory/host_vars/ci-lab.yml`, set `ansible_host` to the Multipass IPv4, and merge `inventory/hosts.snippet.yml` into `deploy/ansible/inventory/hosts.yml`.

Generate deployment SSH material:

```powershell
.\scripts\new-deployment-key.ps1
ssh-keyscan -p 22 <vm-ipv4> > keys/known_hosts
```

### 5. Bootstrap the VM

On **Windows Home + VirtualBox**, the VM usually gets a NAT address (`10.0.2.15`) that the host cannot SSH to directly. Use the local bootstrap helper instead of WSL:

```powershell
.\scripts\setup-vm.ps1
.\scripts\bootstrap-vm-local.ps1
```

On Linux/WSL with a routable VM IP:

```bash
ansible-playbook --inventory deploy/ansible/inventory/hosts.yml --limit ci-lab --user ubuntu deploy/ansible/playbooks/bootstrap.yml
```

### 6. Configure GitLab deployment variables

For environment `development/ci-lab` (must match `gitlab_environment` in host vars):

| Variable                 | Type | Content                   |
| ------------------------ | ---- | ------------------------- |
| `DEPLOY_SSH_PRIVATE_KEY` | File | `ci-lab/keys/deployment`  |
| `DEPLOY_KNOWN_HOSTS`     | File | `ci-lab/keys/known_hosts` |

Protect the environment if you want to mirror production controls.

## Typical test flows

### Verify pipeline (MR / default branch)

Push a branch or open a merge request in the GitLab project. Expect `documentation:verify`, `deployment:verify`, `package:verify`, `frontend:verify`, and `backend:verify` to run. E2E is intentionally not part of CI.

### Release pipeline (tag)

```powershell
git tag v0.0.1
git push ci-lab v0.0.1
```

Expect `release:package`, `release:publish`, `release:create`, generated manual deploy jobs, registry assets under **Deploy** → **Package Registry**, and release notes under **Deploy** → **Releases**.

### Manual deploy job

Run the `ci-lab` manual job from the tag pipeline after bootstrap completes.

## Windows networking notes

- **Verify jobs** run inside the GitLab runner container and use Docker-in-Docker; they do not need the Multipass VM. Register the runner with the provided scripts so the host Docker socket is not mounted into DinD service containers.
- **Deploy jobs** SSH from the runner container to the VM. On Windows, Docker Desktop routing to the Multipass network is often unreliable.
  - **Recommended:** run deploy/rollback playbooks manually from WSL against the Multipass IP while iterating on Ansible.
  - **Alternative:** register a [shell executor](https://docs.gitlab.com/runner/executors/shell.html) runner on WSL for deploy jobs only, or run GitLab + runner on a Linux host.

## Tear down

```powershell
cd ci-lab
docker compose down
multipass delete --purge template-ci-lab
Remove-Item -Recurse -Force data, runner-config, keys
```

Remove the `gitlab.local` line from `C:\Windows\System32\drivers\etc\hosts` if you no longer need it.

## Troubleshooting

### Multipass `launch failed` right after install

On **Windows Home**, set the VirtualBox driver and install VirtualBox:

```powershell
multipass set local.driver=virtualbox
winget install Oracle.VirtualBox
```

Avoid `--cloud-init` on the first launch (it can time out). `scripts/prepare-vm.ps1` applies packages and secrets through `multipass exec` instead.

### Multipass shows `IPv4: N/A`

This is normal with the VirtualBox backend. Scripts read the internal address through `multipass exec hostname -I` (typically `10.0.2.15`).

### Pipeline jobs cannot clone (`Could not resolve host: gitlab.local`)

Re-register the runner with host mapping for job containers:

```powershell
.\scripts\register-runner.ps1 -RunnerToken "<token>"
```

The script passes `--docker-extra-hosts gitlab.local:host-gateway`.

## Related documentation

- [Continuous integration](../docs/system/operations/ci.md)
- [Deployment](../docs/system/operations/deployment.md)
