# Contributing to ChangeMe (template maintainers)

> Scope: authors packaging **[ChangeMe](https://www.nuget.org/packages/ChangeMe)** from **this GitHub repository** (`Laczynski/Laczynski.ChangeMe`). If you only consume **`dotnet new changeme`** or fork a generated app, use the root **`README.md`**, **`AGENTS.md`**, and **`docs/`** in your solution — not this folder.

## Payload split

| Audience | Location |
| --- | --- |
| Generated solution / fork | `README.md`, `AGENTS.md`, `docs/*`, `.gitlab-ci.yml`, `deploy/` |
| Template source maintainers | **`maintainer/`** (this folder), **`.github/`**, `template-pack/*`, `.template.config/*`, `template-content/*` |

The `maintainer/` folder and `.github/` are **excluded** from the NuGet template payload. `SECURITY.md` and `CODE_OF_CONDUCT.md` ship with generated solutions.

## Security

Report vulnerabilities through [`SECURITY.md`](../SECURITY.md) — use GitHub private security advisories, not public issues.

## Install and validate template locally

```powershell
dotnet pack template-pack/ChangeMe.Templates.csproj -c Release
dotnet new install .\template-pack\bin\Release\ChangeMe.<version>.nupkg
dotnet new changeme -n Smoke -o %TEMP%\ChangeMeSmoke --force
```

Avoid **`dotnet new -o`** under **`artifacts/`** without cleaning stale folders.

After changing exclusions in `.template.config/template.json`, confirm generated output:

```powershell
dotnet new changeme -n Smoke -o $env:TEMP\ChangeMeSmoke --force
# expect: no maintainer/, no .github/, no .claude/skills/template-publish/, no template-pack/
```

## Publishing

See [publishing.md](publishing.md). Summary:

1. Bump **`Version`** in **`template-pack/ChangeMe.Templates.csproj`** and update **`CHANGELOG.md`**.
2. Push a git tag (`v2.1.0`) — [publish.yml](../.github/workflows/publish.yml) tests, packs, publishes to nuget.org + GitHub Packages, and creates a GitHub Release.

## Template authoring

- `.template.config/template.json` — symbols, sources, **exclude list** for maintainer-only paths.
- `template-content/generated-readme/README.md` — becomes the generated solution root **`README.md`**.
- `maintainer/` — docs that must never ship in the NuGet payload.
- **`src/ChangeMe.Backend/.../Persistence/Migrations/*.cs`** — shipped (`InitialCreate` included). Add migrations with `npm run ef:migrations:add -- <Name>`.

When adjusting shipped docs (`docs/`, generated readme, `AGENTS.md`), keep them oriented toward the **generated product**, not this GitHub repository layout.
