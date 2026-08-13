# Functional specification skeleton

> Copy the block below to `docs/requirements/functional/<domain>/fr-<area>-<nnn>-<slug>.md`.
> **Layers:** `docs/requirements/_shared/README.md`.
> **How to write:** `docs/requirements/requirements-authoring-guide.md`.
> **Workflow:** `docs/requirements/requirements-change-process.md`.

```markdown
---
id: FR-XXX-NNN
title: Specification Name
domain: identity | users | invitations | access | passkeys | issues
type: functional
status: active
depends_on: [FR-YYY-MMM]
inherits_conventions: [STD-MSG-001, STD-VAL-001, STD-ACC-001]
inherits_quality:
  [NFR-QUAL-001, NFR-A11Y-001, NFR-I18N-001, NFR-PERF-001, NFR-RSP-001]
---

## Goal

The system must `<main business outcome>`.

## Functional requirements

### Authorization

- `<permission and access rules>`

### Data

- `<entities, fields, constraints, defaults>`

### Operations

- `<what the user or system can do and the result>`

### Validation

- **`<Field>`**: `<validation rule>`; rejection message: **`<exact message>`** when applicable.

### Business rules

- `<state transitions, side effects, consistency rules>`

## Quality requirements

- Inherits `docs/requirements/_shared/quality/product-quality.md` (`NFR-QUAL-001`) and linked quality documents.
- Inherits `docs/requirements/_shared/conventions/product-standards.md` (`CONV-001`) via `inherits_conventions` unless stated above.
- Document only overrides below.

## Out of scope

- `<deliberate exclusion>`
```
