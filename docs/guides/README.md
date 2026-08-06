# Implementation guides

> **L5 — Implementation.** How to write and verify code in this repository — not product behaviour or deployment configuration.

## Requirements layers (read before implementing)

| Layer           | Location                                                                           | When to read                               |
| --------------- | ---------------------------------------------------------------------------------- | ------------------------------------------ |
| L4 Capabilities | [`docs/requirements/functional/`](../requirements/functional/)                     | Always — the feature you are building      |
| L2 Conventions  | [`product-standards.md`](../requirements/_shared/conventions/product-standards.md) | UI — lists, forms, validation UX, feedback |
| L1 Domain       | [`docs/requirements/_shared/domain/`](../requirements/_shared/domain/)             | Terms, account model, permissions          |
| L3 Quality      | [`docs/requirements/_shared/quality/`](../requirements/_shared/quality/)           | Performance, a11y, i18n when relevant      |

Full layer index: [`docs/requirements/_shared/README.md`](../requirements/_shared/README.md). **L4 overrides L2; this folder (L5) never defines product behaviour.**

## Documents

| Document                                         | Read when you need to…                                             |
| ------------------------------------------------ | ------------------------------------------------------------------ |
| [repo-map.md](repo-map.md)                       | Find where code lives and which layer owns what                    |
| [frontend-guidelines.md](frontend-guidelines.md) | Angular components, forms, routing, @laczynski/ui, Tailwind layout |
| [backend-guidelines.md](backend-guidelines.md)   | Layers, endpoints, use cases, validation, EF                       |
| [testing-guidelines.md](testing-guidelines.md)   | Test layer ownership, anti-patterns, when to skip tests            |
| [e2e-guidelines.md](e2e-guidelines.md)           | Playwright layout, locators, conventions, config, commands         |
| [feature-recipes.md](feature-recipes.md)         | Step-by-step recipes for common feature work                       |

## Start here by task

| Task                              | Documents                                                                                                                                                                                                                                          |
| --------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| First orientation in the codebase | [repo-map.md](repo-map.md)                                                                                                                                                                                                                         |
| Frontend change                   | Target `FR-*` + [product-standards.md](../requirements/_shared/conventions/product-standards.md) + [frontend-guidelines.md](frontend-guidelines.md) + [testing-guidelines.md](testing-guidelines.md#mapping-std--to-test-layers) when adding tests |
| Backend change                    | Target `FR-*` + [backend-guidelines.md](backend-guidelines.md)                                                                                                                                                                                     |
| New endpoint or screen end-to-end | Target `FR-*` + [feature-recipes.md](feature-recipes.md) + relevant guidelines + [testing-guidelines.md](testing-guidelines.md)                                                                                                                    |
| Verify before PR                  | [testing-guidelines.md](testing-guidelines.md) + [`AGENTS.md`](../../AGENTS.md) (commands)                                                                                                                                                         |

## Relationship to other docs

| Folder                       | Layer | Use for                              |
| ---------------------------- | ----- | ------------------------------------ |
| `docs/guides/` (this folder) | L5    | How to _implement_ in this repo      |
| `docs/requirements/`         | L1–L4 | What the product _must do_           |
| `docs/technical/`            | —     | How to _run and configure_ the stack |

Commands shared across areas: [`AGENTS.md`](../../AGENTS.md).
