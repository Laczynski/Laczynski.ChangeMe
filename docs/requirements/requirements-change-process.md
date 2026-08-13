# Requirements change process

> Workflow for analysts and developers: pending deltas, validation, and handoff. Trimmed after delivery.
> **Five layers:** see `docs/requirements/_shared/README.md`.

## Start here

| Role                                   | Read first                                                         | Then                                                                                                                                       |
| -------------------------------------- | ------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------------------------------------------------ |
| **Analyst** (new or updated `FR-*`)    | [requirements-authoring-guide.md](requirements-authoring-guide.md) | Edit specs → [\_changes-template.md](_changes-template.md) → `npm run docs:validate`                                                       |
| **Developer** (implement pending work) | Pending files in [changes/](changes/)                              | Linked `FR-*` + `_shared/` → [feature workflow](../system/development/feature-workflow.md) + [testing strategy](../system/development/testing-strategy.md) |
| **Find a specification**               | [README.md](README.md) (auto-generated index)                      | Open the linked `FR-*` file                                                                                                                |

## Five layers

| Layer                 | Location               | Role                                                                  |
| --------------------- | ---------------------- | --------------------------------------------------------------------- |
| **L1 Domain**         | `_shared/domain/`      | Glossary, account model, permissions catalog — _what exists_          |
| **L2 Conventions**    | `_shared/conventions/` | Product standards (`CONV-001`, `STD-*` sections) — _default behavior_ |
| **L3 Quality**        | `_shared/quality/`     | `NFR-*` — accessibility, i18n, performance, responsiveness            |
| **L4 Capabilities**   | `functional/<domain>/fr-*.md` | Business capabilities — _what each feature must do_            |
| **L5 Implementation** | `docs/modules/*/development.md`, `docs/system/development/` | Module patterns and cross-module workflows — _how we code in this repo_ |

Meta (not a product layer): `requirements-authoring-guide.md`, `requirements-change-process.md`, `changes/`, `README.md`.

## Roles

### Analyst (or author of the change)

1. Read `docs/requirements/requirements-authoring-guide.md` when creating or substantially updating `FR-*` files.
2. Decide whether the change touches **L1–L3** shared docs. If yes, edit the matching file under `_shared/` **once** instead of duplicating across `FR-*`.
3. Edit only the affected `FR-*` files under `functional/<domain>/`.
4. Create **one** file in `changes/` from `_changes-template.md`.
5. List every touched document; describe the **behavior delta**.
6. Run `npm run docs:validate` before opening a PR.

### Developer

1. Read pending files in `changes/` (**Status:** `pending`).
2. Read linked `FR-*` and referenced `_shared/` docs (`inherits_conventions`, `inherits_quality`, `depends_on`).
3. Implement and verify against **Functional requirements**, inherited `STD-*`, and quality docs.
4. When merged, set **Status:** `done (YYYY-MM-DD)` or delete the change file.

## Sections in a change record

| Section                               | Purpose                                                |
| ------------------------------------- | ------------------------------------------------------ |
| **Why**                               | Brief intent. Optional.                                |
| **Functional specifications touched** | Which `FR-*` files are new or updated.                 |
| **Shared documents touched**          | Which L1–L3 files changed, if any.                     |
| **Behavior delta**                    | What is different from the old product (Before/After). |
| **Implementation scope**              | Optional hint for backend/frontend/test areas.         |

## Validation

```bash
npm run docs:validate
```

Checks: `FR-*` / `NFR-*` / `STD-*` references, required FR sections, broken paths, change records. Regenerates `README.md` and `.requirements-manifest.json`.

## For AI agents

1. Follow `docs/requirements/requirements-authoring-guide.md` when editing `FR-*`.
2. Read pending `changes/` files with **Status:** `pending` plus referenced specs.
3. **Load order for implementation:**
   - Target `FR-*` (L4) — always
   - `docs/requirements/_shared/conventions/product-standards.md` (L2) — for any UI or interaction work
   - Relevant `docs/requirements/_shared/domain/*` (L1) — when access, users, or permissions matter
   - Relevant `docs/requirements/_shared/quality/*` (L3) — when NFR applies
   - Matching module development document and `docs/system/development/*` (L5) — for code patterns
4. Verify: L4 rules met; L2 conventions applied unless L4 overrides; L5 used only for implementation, not product behavior.
