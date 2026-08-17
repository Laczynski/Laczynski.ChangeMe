# Requirements authoring guide

> **Audience:** analysts and authors of functional specifications (`FR-*`).
> **Layers:** `docs/requirements/_shared/README.md` (L1 Domain · L2 Conventions · L3 Quality · L4 Capabilities · L5 Implementation).
> **Workflow:** `docs/requirements/requirements-change-process.md`.
> **Skeleton:** `docs/requirements/_functional-specification-skeleton.md`.

## New vs updated specification

| Situation          | What to do                                                                                                                                                                                                            |
| ------------------ | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| **New `FR-*`**     | Copy `_functional-specification-skeleton.md` to `docs/requirements/functional/<domain>/fr-<area>-<nnn>-<slug>.md`. Assign the next free `FR-<AREA>-NNN` in that domain. Fill frontmatter and sections per this guide. |
| **Updated `FR-*`** | Edit the existing file only. Describe what changed in the pending change record (**Behavior delta**), not only in the specification body.                                                                             |

After either path, add or update a pending record in `docs/requirements/changes/` and run `npm run docs:validate`.

## Before you write

1. Read relevant `docs/requirements/_shared/domain/` docs when the feature touches glossary terms, account model, or permissions.
2. Check whether behavior belongs in `docs/requirements/_shared/conventions/product-standards.md` (L2) — link `STD-*` sections, do not duplicate.
3. Search `docs/requirements/README.md` for related `FR-*`; set `depends_on` and cite identifiers when behavior builds on another specification.

## Scope (L4 — Capabilities)

- Describe **what the system must do** to deliver business value: capabilities, data rules, validations, permissions, and observable outcomes.
- One file = one coherent **business capability**. Do not organize specifications around screens, tabs, or layout sections.
- Cross-cutting defaults live in L2 Conventions and L3 Quality — link, do not duplicate.
- Document **main** behavior only. Omit detail already covered by inherited `STD-*` sections.

## Structure

- Keep identifiers in the `FR-<AREA>-NNN` format.
- **Goal** — business outcome in one or two sentences.
- **Functional requirements** — grouped by concern:
  - **Authorization** — permissions required for each capability.
  - **Data** — entities, fields, constraints, and defaults.
  - **Operations** — what the user or system can do, triggers, and results.
  - **Validation** — field rules and rejection messages.
  - **Business rules** — state transitions, side effects, consistency, and evaluation order.
- **Quality requirements** — inherits L3 documents via `inherits_quality`; override only when this specification differs.
- **Out of scope** — deliberate exclusions.

Frontmatter:

```yaml
depends_on: [FR-YYY-MMM]
inherits_conventions: [STD-LST-001, STD-VAL-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
```

## What to include in L4

- Permissions and who can perform each action.
- Field constraints, allowed values, uniqueness.
- Business logic: state changes, side effects.
- Rejection messages that encode business rules or validation failures.
- Default values and evaluation order when behavior depends on them.
- Explicit overrides of L2 standards (for example `Inherits STD-LST-001 except: default sort …`).

## What to omit (defer to L2 Conventions)

- Column tables, badge styles, icons, tooltips.
- Button placement, sidebar entries, back-link labels.
- Generic pagination, loading, and empty-state mechanics.
- Form control types unless they imply distinct behavior.

## Clarity and precision (mandatory)

- **Do not use** vague qualifiers: _optional_, _may_, _might_, _TBD_, _etc._
- **State exact rejection messages** when wording matters.
- **Use "not required"** for empty-allowed fields; **use "must not" / "cannot"** for prohibitions.
- **One behavior, one bullet.**

## No implementation details (mandatory)

Do not include source code, API endpoints, HTTP details, database tables, or migrations. Describe business entities, permission names, field constraints, operations, and observable outcomes.

## Layer reference

| Layer             | Location               | Question                             |
| ----------------- | ---------------------- | ------------------------------------ |
| L1 Domain         | `_shared/domain/`      | What exists?                         |
| L2 Conventions    | `_shared/conventions/` | How does the product usually behave? |
| L3 Quality        | `_shared/quality/`     | How good must it be?                 |
| L4 Capabilities   | `functional/<domain>/fr-*.md` | What must this feature do?    |
| L5 Implementation | `docs/modules/*/development.md`, `docs/system/development/` | How do we build it in this repo? |

**Override rule:** L4 overrides L2; L2 overrides implicit habit; L3 applies unless L4 scopes it out; L5 never defines product behavior.

## Checklist before opening a PR

1. Testable outcomes are in **Functional requirements** or in the correct shared layer.
2. `inherits_conventions` lists only valid `STD-*` ids from `product-standards.md`.
3. Pending change record lists every touched document with a clear **Behavior delta**.
4. `npm run docs:validate` passes.
