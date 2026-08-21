---
name: template-publish
description: Publish the ChangeMe dotnet new NuGet template from the GitHub source repository. Use when the user invokes /template-publish, asks to release the ChangeMe template package, bump the template version, or push a maintainer v* tag for NuGet.
---

# Template publish

Publish the **`ChangeMe`** NuGet template package from **Laczynski/Laczynski.ChangeMe** on GitHub. This skill does **not** release a generated application — use `/release` for GitLab application releases in a scaffolded product repository.

This skill and `maintainer/` are **excluded** from `dotnet new` output. Generated applications ship only `.claude/skills/release/`.

## Before starting

1. Confirm `template-pack/ChangeMe.Templates.csproj` exists in the current repository.
2. If the user asked for `/release` or a GitLab application tag, stop and use `/release` instead.
3. Read [maintainer/publishing.md](../../maintainer/publishing.md) completely — it is the canonical operator document.

## Interpret the request

- Accept `patch`, `minor`, `major`, or an exact stable `vX.Y.Z` target for the **template package** version.
- Default to `patch` only when the user asked for the next template release without choosing a level.
- The git tag **`vX.Y.Z`** must match `<Version>` in `template-pack/ChangeMe.Templates.csproj` and the matching section in root `CHANGELOG.md`.

## Prepare the release

1. Require a clean working tree unless the user explicitly allows otherwise.
2. Fetch remotes and list existing `v*` tags. Reject the target when the tag or GitHub Release already exists.
3. Bump `<Version>` in `template-pack/ChangeMe.Templates.csproj`.
4. Add `## [X.Y.Z] - YYYY-MM-DD` to root `CHANGELOG.md` under the template-package sections.
5. Verify locally:

   ```powershell
   npm run docs:validate
   npm run test:all
   npm run build:all
   npm run pack:backend
   ```

6. Optionally smoke-test the packed template:

   ```powershell
   dotnet new install .\template-pack\bin\Release\ChangeMe.<version>.nupkg
   dotnet new changeme -n Smoke -o $env:TEMP\ChangeMeSmoke --force
   ```

   Confirm the output contains **no** `maintainer/`, `.github/`, or `.claude/skills/template-publish/`.

7. Commit version and changelog updates. Push to the default branch when the repository workflow requires it before tagging.

## Publish

1. Create and push the annotated tag on the intended commit:

   ```powershell
   git tag vX.Y.Z
   git push origin vX.Y.Z
   ```

2. Monitor [`.github/workflows/publish.yml`](../../.github/workflows/publish.yml): tests, `dotnet pack`, NuGet publish (nuget.org + GitHub Packages), GitHub Release from `CHANGELOG.md`.
3. Return the tag, commit SHA, workflow URL, NuGet package version, and GitHub Release URL.

## Failure rules

- Never publish a template tag from a generated application repository.
- Never confuse template `CHANGELOG.md` (NuGet package history) with an application changelog in a fork.
- Do not force-push tags. If publish fails after the tag exists, diagnose the workflow; do not republish different bytes under the same version.
- Trusted publishing to nuget.org works in GitHub Actions only; local push requires an API key (see [maintainer/publishing.md](../../maintainer/publishing.md)).
