# Requirements shared layers

> Five layers for product analysis and development. **Layers are concepts; folders may contain many files** as the product grows. Agents load only the files relevant to the task.

| Layer                 | Folder           | Question                               | Load when…                                                                                                           |
| --------------------- | ---------------- | -------------------------------------- | -------------------------------------------------------------------------------------------------------------------- |
| **L1 Domain**         | `domain/`        | _What exists?_                         | Feature touches users, roles, sessions, permissions, glossary terms                                                  |
| **L2 Conventions**    | `conventions/`   | _How does the product usually behave?_ | Any UI, forms, lists, feedback, confirmations                                                                        |
| **L3 Quality**        | `quality/`       | _How good must it be?_                 | Performance, a11y, i18n, responsiveness matter                                                                       |
| **L4 Capabilities**   | `../functional/` | _What must this feature do?_           | **Always** — unit of work (`FR-*`)                                                                                   |
| **L5 Implementation** | `../../modules/*/development.md`, `../../system/development/` | _How do we build it in this repo?_ | Module patterns; [STD → test layers](../../system/development/testing-strategy.md#mapping-std--to-test-layers) when adding tests |

## Override rule

**L4 overrides L2; L2 overrides implicit habit; L3 applies unless L4 scopes it out; L5 never defines product behavior.**

## Agent load order (minimal)

1. Target `FR-*` (L4)
2. `conventions/product-standards.md` (L2) for UI work
3. Relevant `domain/*` (L1) when access or entities matter
4. Relevant `quality/*` (L3) when NFR applies
5. Matching module development document and `docs/system/development/*` (L5) for implementation patterns

## Entry documents

| Layer | Start here                                                                                                  |
| ----- | ----------------------------------------------------------------------------------------------------------- |
| L1    | [domain/README.md](domain/README.md)                                                                        |
| L2    | [conventions/product-standards.md](conventions/product-standards.md) (+ [STD index](conventions/README.md)) |
| L3    | [quality/README.md](quality/README.md)                                                                      |
