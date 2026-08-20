# Contributing to Laczynski.ChangeMe (maintainers)

> Scope: authors packaging **[Laczynski.ChangeMe](https://www.nuget.org/packages/Laczynski.ChangeMe)** from **this GitHub repository** (`Laczynski/Laczynski-ChangeMe`). If you only consume **`dotnet new changeme`** or fork a generated app, you do **not** need this file; see **`README.md`**, **`AGENTS.md`**, and **`docs/`** in your generated solution.

## Documentation split

| Audience                  | Files                                                                                                    |
| ------------------------- | -------------------------------------------------------------------------------------------------------- |
| Generated solution / fork | `README.md`, `AGENTS.md`, `docs/*`                                                                       |
| Template repo maintainers | `CONTRIBUTING.md` (this file), `.template.config/template.json`, `template-pack/*`, `template-content/*` |

`CONTRIBUTING.md` is excluded from the NuGet template payload (`dotnet new` output).

## Install and validate template locally

```powershell
dotnet new install .
dotnet new changeme -n Smoke -o %TEMP%\TemplateSmoke --force
```

Avoid **`dotnet new -o`** under **`artifacts/`** (or other nested outputs inside this repo) without cleaning stale folders; `artifacts/**` is excluded from packaged content but local installs can still recurse oddly.

## Publish NuGet package

Tag-based CI publish is documented in **`docs/system/operations/publishing.md`**. Summary:

1. Bump **`Version`** in **`template-pack/Laczynski.ChangeMe.csproj`** and update **`CHANGELOG.md`**.
2. Push a git tag (`v2.1.0`) — [publish.yml](.github/workflows/publish.yml) tests, packs, publishes to nuget.org + GitHub Packages, and creates a GitHub Release.

Local pack only:

```powershell
npm run pack:backend
```

Optional local push with API key — see **`docs/system/operations/publishing.md`**.

Package readme: **`template-pack/NuGetPackageREADME.md`**.

## Layout hints for editors

- `.template.config/template.json` – symbols, sources.
- `template-content/generated-readme/README.md` – becomes the generated solution’s root **`README.md`** (root **`README.md`** is excluded from the template payload and stays maintainer-facing on GitHub).
- **`src/ChangeMe.Backend/.../Persistence/Migrations/*.cs`** – shipped with the template (`InitialCreate` included). After an EF model change, add a migration: `npm run ef:migrations:add -- <Name>` from the repo root.

When adjusting persistence, keep **`README.md` / `AGENTS.md` / `docs/`** oriented toward the **generated product**, not this repo.
