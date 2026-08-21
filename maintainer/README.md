# Template repository maintainers

> **Not shipped** in the `dotnet new` NuGet payload. Generated solutions receive application code, `docs/`, `.gitlab-ci.yml`, and `deploy/` — not this folder or `.github/`.

Use this directory when working on **Laczynski/Laczynski.ChangeMe** on GitHub: packaging the **`ChangeMe`** template, running GitHub Actions, and publishing to NuGet.

## What stays in the source repo only

| Path | Purpose |
| --- | --- |
| **`maintainer/`** (this folder) | Maintainer docs — publishing, GitHub CI, template authoring |
| **`.github/`** | GitHub Actions (`ci.yml`, `publish.yml`) and Dependabot |
| **`.claude/skills/template-publish/`** | Agent skill for NuGet template publishing (maintainer repo only) |
| **`template-pack/`**, **`.template.config/`**, **`template-content/`** | NuGet packaging and `dotnet new` manifest |
| Root **`README.md`**, **`CHANGELOG.md`**, root **`CONTRIBUTING.md`** stub | GitHub-facing maintainer entry points (excluded from the template payload) |

## What ships in `dotnet new`

| Path | Purpose |
| --- | --- |
| **`template-content/generated-readme/README.md`** → root **`README.md`** | Product-focused readme for generated apps |
| **`docs/`** | Requirements and implementation guidance |
| **`.gitlab-ci.yml`**, **`.gitlab/ci/`**, **`deploy/`** | Application delivery on GitLab + Ansible |
| **`AGENTS.md`**, **`SECURITY.md`**, **`CODE_OF_CONDUCT.md`** | Agent guide and community policies |
| **`.claude/skills/release/`** | Agent skill for GitLab application releases |

Exclusions are enforced in [`.template.config/template.json`](../.template.config/template.json) and [template-pack/ChangeMe.Templates.csproj](../template-pack/ChangeMe.Templates.csproj).

## Maintainer documents

| Task | Read |
| --- | --- |
| Author, validate, or change the template | [CONTRIBUTING](CONTRIBUTING.md) |
| Publish the **`ChangeMe`** NuGet package | [publishing](publishing.md) · agent **`/template-publish`** |
| GitHub Actions CI on this repository | [ci-github](ci-github.md) |
| GitLab CI in generated applications | [Continuous integration](../docs/system/operations/ci.md) (shipped payload) |
| VPS deployment of generated apps | [Deployment](../docs/system/operations/deployment.md) (shipped payload) |
