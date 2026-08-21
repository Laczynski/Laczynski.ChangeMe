# Template publishing

> Scope: **template source repository maintainers** only — not shipped in `dotnet new` output.

This process releases the reusable template package. It does not version or deploy an application created from the template; generated-application releases use GitLab CI documented in [deployment](../docs/system/operations/deployment.md). The rationale for keeping those lifecycles separate is in [multi-environment application delivery](../docs/system/designs/multi-environment-application-delivery-design.md).

## Registry

| Package    | Primary registry                   | Secondary       |
| ---------- | ---------------------------------- | --------------- |
| `ChangeMe` | [nuget.org](https://www.nuget.org) | GitHub Packages |

Publish on tag push `v*` via [`.github/workflows/publish.yml`](../.github/workflows/publish.yml) (trusted publishing / OIDC). Agent-assisted flow: **`.claude/skills/template-publish/`** (`/template-publish`). This workflow, `maintainer/`, `template-pack/`, `.github/`, and `template-publish` are excluded from generated applications.

## Where the version lives

| Location                                  | Field       |
| ----------------------------------------- | ----------- |
| `template-pack/ChangeMe.Templates.csproj` | `<Version>` |

## Release checklist

1. Bump `<Version>` in `template-pack/ChangeMe.Templates.csproj`.
2. Update `CHANGELOG.md` (`## [x.y.z]` section).
3. Verify locally:

   ```powershell
   npm run test:all
   npm run build:all
   npm run pack:backend
   ```

4. Tag and push:

   ```powershell
   git tag v2.1.0
   git push origin v2.1.0
   ```

   `publish.yml` runs tests, packs the template, publishes NuGet (nuget.org + GitHub Packages), and creates a GitHub Release from `CHANGELOG.md`.

## One-time setup

### GitHub secret

Settings → Secrets and variables → Actions:

| Secret       | Value                              |
| ------------ | ---------------------------------- |
| `NUGET_USER` | nuget.org profile name (not email) |

### nuget.org trusted publishing

1. [nuget.org](https://www.nuget.org) → profile → **Trusted Publishing** → **Add**.
2. Policy fields:

   | Field            | Value                  |
   | ---------------- | ---------------------- |
   | Package Owner    | your nuget.org account |
   | Repository Owner | `Laczynski`            |
   | Repository       | `Laczynski.ChangeMe`   |
   | Workflow File    | `publish.yml`          |
   | Environment      | _(leave empty)_        |

### GitHub repository settings

Actions enabled; workflow permissions allow `packages: write` and OIDC (`id-token: write` is set in the workflow).

## Publish workflow (tag `v*`)

1. Test (requirements, frontend, backend)
2. `dotnet pack template-pack/ChangeMe.Templates.csproj`
3. Publish NuGet to **nuget.org** and **GitHub Packages**
4. GitHub Release from `CHANGELOG.md`

## Consumer setup

### nuget.org

```powershell
dotnet new install ChangeMe
```

### GitHub Packages

```powershell
dotnet nuget add source --username YOUR_GITHUB_USERNAME --password YOUR_PAT --store-password-in-clear-text --name github "https://nuget.pkg.github.com/Laczynski/index.json"
dotnet new install ChangeMe --nuget-source github
```

## Optional: local push (API key)

Trusted publishing works in CI only:

```powershell
dotnet pack template-pack/ChangeMe.Templates.csproj -c Release
dotnet nuget push template-pack/bin/Release/ChangeMe.*.nupkg --api-key <nuget.org-api-key> --source https://api.nuget.org/v3/index.json
```
