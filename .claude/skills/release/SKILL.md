---
name: release
description: Prepare and publish a reviewed stable GitLab application release. Use when the user invokes /release, asks to bump the application version, prepare release notes or a release merge request, publish a stable tag, or verify the resulting GitLab Release.
---

# Release

Prepare application releases in two reviewed phases: a release merge request, then a stable tag after that merge. The tag is the canonical application version; do not add or update package, frontend, or assembly version fields unless repository documentation explicitly assigns them that role.

This workflow is for the generated application's GitLab package. If `template-pack/ChangeMe.Templates.csproj` exists, first confirm that the user wants an application release rather than the separately owned template NuGet release in `maintainer/publishing.md`.

## Interpret the request

- Accept `patch`, `minor`, `major`, or an exact stable `vX.Y.Z` target. Default to `patch` only when the user asked for the next release without choosing a level.
- Treat `/release patch` as the prepare phase. If its release MR is already merged, resume at the publish checks instead of creating another MR.
- Treat `/release publish vX.Y.Z` as explicit authorization to create and push that exact tag after all publish checks pass.
- Never infer a breaking `major` bump. Surface possible breaking changes and ask the user to select the version level.

## Load the release contract

1. Read `AGENTS.md`, `docs/system/operations/deployment.md`, and `docs/system/operations/ci.md` completely.
2. Inspect `.gitlab-ci.yml`, `.gitlab/ci/release.yml`, `CHANGELOG.md`, and `scripts/extract-changelog.mjs` before editing.
3. Use the GitLab remote and `glab` when available. Verify authentication and project identity before creating a branch, merge request, tag, or release.

## Prepare the release merge request

1. Require a clean working tree. Do not stash, reset, discard, or absorb unrelated changes.
2. Fetch the default branch and tags. Determine the latest stable tag using semantic-version ordering; use `v0.0.0` as the base when none exists.
3. Calculate the requested target and reject it when the tag or GitLab Release already exists.
4. Review commits and merged MRs since the previous stable tag. Check their diffs when titles do not explain user or operator impact.
5. Create `release/vX.Y.Z` from the current remote default branch.
6. Add `## [X.Y.Z] - YYYY-MM-DD` immediately below `## [Unreleased]` in `CHANGELOG.md`. Include only non-empty Keep a Changelog categories such as `Added`, `Changed`, `Fixed`, `Security`, `Deprecated`, and `Removed`.
7. Write concise user- and operator-facing notes. Call out breaking changes, migrations, configuration changes, and manual actions. Do not fabricate a change from a commit title alone.
8. Verify extraction with `node scripts/extract-changelog.mjs X.Y.Z`. Run the non-E2E checks required by `AGENTS.md` and the touched areas. E2E is not a release gate.
9. Commit as `chore(release): prepare vX.Y.Z`, push the release branch, and create a GitLab MR summarizing the notes and verification.
10. Return the MR URL, target tag, previous tag, and checks run. Do not merge or tag while the MR is open.

Pause for the required review and merge. Merge only when the user explicitly authorizes it and project approvals allow it.

## Publish after merge

1. Fetch the remote without rewriting local work. Verify the release MR is merged and its required pipeline succeeded.
2. Verify the remote default-branch commit contains the exact changelog section and passes `node scripts/extract-changelog.mjs X.Y.Z`.
3. Confirm again that neither `vX.Y.Z` nor its GitLab Release exists and that the target commit belongs to the remote default branch.
4. Create an annotated `vX.Y.Z` tag directly on the verified remote default-branch commit and push only that tag.
5. Monitor the tag pipeline through `release:package`, `release:publish`, and `release:create`. Do not start any manual `deploy:<instance>` job.
6. Verify the GitLab Release notes and its archive, manifest, and SHA-256 links. Return the tag, commit, pipeline URL, Release URL, and package version.

## Failure rules

- Stop before any write when the repository, remote, default branch, version, or release mode is ambiguous.
- Never force-push the default branch or a release tag. Never move, recreate, or delete a published tag as an automatic recovery step.
- Retry an external runner or network failure only when retrying cannot change release bytes. If committed release content is wrong, prepare a new version through another MR.
- Do not bypass failed verification, protected-tag rules, MR approvals, or GitLab environment protection.
- Never publish a tag from an unmerged release branch or from a local commit absent from the remote default branch.
