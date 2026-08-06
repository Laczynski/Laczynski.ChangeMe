# L2 — Conventions

> Default product behavior. Functional specifications inherit via `inherits_conventions: [STD-…]` in frontmatter.

| Document                                     | Role                                                            |
| -------------------------------------------- | --------------------------------------------------------------- |
| [product-standards.md](product-standards.md) | Canonical `STD-*` rules and **implementation review checklist** |

Visual implementation (Fluent 2 tokens, Laczynski components, Tailwind layout) is defined in L5 — [`frontend-guidelines.md`](../../../guides/frontend-guidelines.md). L2 states _what_ the UI must do; L5 states _how_ to build it in this repository.

## STD index

| ID                                                                                  | Topic                                           |
| ----------------------------------------------------------------------------------- | ----------------------------------------------- |
| [STD-MSG-001](product-standards.md#std-msg-001--feedback-channels)                  | Feedback channels (toast, inline, dialog)       |
| [STD-ACC-001](product-standards.md#std-acc-001--access-and-visibility)              | Access, visibility, permission denial           |
| [STD-VAL-001](product-standards.md#std-val-001--validation-behavior)                | Validation presentation and timing              |
| [STD-LST-001](product-standards.md#std-lst-001--list-screens)                       | List screens (search, filter, sort, pagination) |
| [STD-LST-002](product-standards.md#std-lst-002--embedded-lists-and-show-more)       | Embedded lists, Show more                       |
| [STD-FRM-001](product-standards.md#std-frm-001--create-and-edit-forms)              | Create and edit forms                           |
| [STD-DTL-001](product-standards.md#std-dtl-001--detail-views)                       | Detail views                                    |
| [STD-NAV-001](product-standards.md#std-nav-001--back-navigation)                    | Back navigation                                 |
| [STD-OP-001](product-standards.md#std-op-001--destructive-and-irreversible-actions) | Destructive confirmations                       |
| [STD-FMT-001](product-standards.md#std-fmt-001--date-time-and-number-display)       | Date, time, number display                      |
| [STD-RPT-001](product-standards.md#std-rpt-001--reporting-and-export)               | Reporting and export                            |

When this file grows large, split into `msg.md`, `lists.md`, etc. — still **L2**, same `STD-*` ids.
