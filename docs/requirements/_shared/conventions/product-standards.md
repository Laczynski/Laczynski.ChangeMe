---
id: CONV-001
title: Product Standards
type: conventions
status: active
depends_on: []
---

# Product standards

> **Layer L2 — Conventions.** Default product behavior inherited by functional specifications (`FR-*`) unless they state an explicit override.
>
> **Agent insert:** load this file for any UI or interaction work. Functional specifications define _what_ a capability does; this document defines _how the product usually behaves_.

Out of scope here: email templates, push notification payloads, and API error shapes — those belong in the relevant `FR-*` or quality layer.

---

## STD-MSG-001 — Feedback channels

Use the correct channel so behavior stays consistent across modules.

| Channel                                      | When to use                                                                                  | Persistence                              |
| -------------------------------------------- | -------------------------------------------------------------------------------------------- | ---------------------------------------- |
| **Inline field validation**                  | Field-level rule failures on forms                                                           | Until the field becomes valid            |
| **Inline screen message** (`ui-message-bar`) | Load failures, form-level errors not tied to one field                                       | Until the user retries or navigates away |
| **Toast**                                    | Successful mutations; action failures not tied to a single field; background policy warnings | Auto-dismiss; see below                  |
| **Confirmation dialog**                      | Destructive or irreversible actions before execution                                         | Until confirm or cancel                  |
| **Modal dialog**                             | Short secondary flows that need input (for example reject reason)                            | Until submit, cancel, or close           |

### Toast conventions

- Success after save/create/delete: exact message from the functional specification (for example **`Role saved.`**, **`User deactivated.`**).
- Default toast lifetime: **3000** ms for success; **5000** ms for errors, unless the functional specification defines **sticky** behavior.
- Sticky toasts include an **action** button when the functional specification names one.
- Do not use a toast for field validation errors.

---

## STD-ACC-001 — Access and visibility

- Authenticated screens live inside the application shell (sidebar, header, content area). Guest screens (login) follow the same **feedback** and **validation** rules where applicable.
- A guest who attempts a protected action is redirected to sign-in.
- Sidebar and navigation entries are visible only when the user has the permission named in the functional specification.
- Actions the user **lacks permission** for are **not shown** (not disabled), unless the functional specification explicitly requires a disabled state with explanation.
- Protected actions attempted without permission are rejected with message **`You do not have permission to perform this action.`** (see `docs/requirements/_shared/domain/permissions.md`).

---

## STD-VAL-001 — Validation behavior

- Validation runs on submit and on field blur when the functional specification does not state otherwise.
- Errors are **inline** next to the relevant field, or **form-level** for group rules (for example permission checkboxes).
- The form **stays open** on validation failure; entered values are preserved.
- Server-side validation errors map to the same inline positions when possible.
- Do not use a toast for field validation errors.

---

## STD-LST-001 — List screens

Full-page server-paginated tables and grids. Examples: **Issues list**, **Users list**, **Roles list** — not limited to administration areas.

Unless a functional specification specifies otherwise:

- Primary **create** or **add** action is available when the user has the required permission.
- **Global search** when the functional specification defines matched fields; search is **case-insensitive**.
- **Column-header filters** on columns named in the functional specification. Default control by column type unless the functional specification overrides:
  - **Text**: contains.
  - **Status / enum**: multi-select; empty selection means no restriction.
  - **Date / time**: range per column.
  - **Number**: comparison.
- Active search and column filters combine with **AND** logic.
- **Clear** resets search, column filters, and sort to defaults and reloads from the first page.
- **Multi-sort**: click column headers to add or toggle sort levels (no Shift key).
- **Sortable** attributes behave as stated in the functional specification; unspecified attributes are not sortable.
- **Default sort** is defined in the functional specification.
- Primary identifier links to details when the functional specification defines one.
- Missing optional text values display **`—`** (em dash), unless the functional specification defines a different placeholder (for example **`Unassigned`**, **`Never`**).
- **Status** and categorical values use compact **tint badges** (`ui-tag`, `appearance="tint"`) with exact labels from the functional specification; color is supplementary — the label text must remain readable in light and dark themes.
- Secondary row actions use an **overflow menu** unless the functional specification defines inline controls.
- Lists are **server-paginated** unless the functional specification defines a different control.
- Default page size: **10** rows per page.
- Search, filter, and sort changes reset to **page 1**.
- While loading, a loading indicator appears **in the table area**; page chrome stays visible.
- When the functional specification does not define empty-state copy, use **`No items match the filters.`** for filtered lists and **`No items yet.`** for unfiltered lists.
- Success after a row action shows a **toast** and refreshes the current list in place unless the functional specification defines navigation away.

---

## STD-LST-002 — Embedded lists and Show more

Examples: comments, history, attachments, sessions, assigned users, notifications.

Unless a functional specification specifies otherwise:

- Lists inside detail views are **server-paginated** with default page size **10** unless the functional specification defines **Show more** append loading.
- **Show more** loads the next page of **older** items and **appends** them without leaving the screen; the control is hidden when all items are loaded.
- Paginated embedded tables (for example **Active sessions** on **User details**) use a paginator below the table instead of **Show more**.
- Section empty states use exact copy from the functional specification when provided.
- While loading, a loading indicator appears in the list area; primary controls above the list (upload, add comment) stay visible.

---

## STD-FRM-001 — Create and edit forms

Examples: **Create issue**, **Edit user**, **Create role**.

Unless a functional specification specifies otherwise:

- Fields, defaults, and validation limits are defined in the functional specification.
- **Required** fields are marked in the functional specification; optional fields use **not required** wording.
- Read-only system metadata on edit appears in a separate summary block when the functional specification lists them.
- **Back** / **Cancel** leave without saving; destination is defined in the functional specification or STD-NAV-001 defaults.
- **Create** / **Save changes** submits the form. On success: toast with exact message from the functional specification, then navigate when the functional specification defines a destination. On failure: keep the form open.
- Primary submit button label: **`Create {entity}`** on create, **`Save changes`** on edit, unless the functional specification names a different label.
- Before the first load on edit, a loading state covers the **form area** until initial data arrives.

---

## STD-DTL-001 — Detail views

Examples: **Issue details**, **User details**, **Role details**.

Unless a functional specification specifies otherwise:

- Content is grouped into named sections (for example **Comments**, **Sessions**, **Permissions**).
- Section-level **Add** actions appear only when the user has the required permission.
- Primary actions (edit, delete) appear in the page header toolbar or actions area — at most **one** primary-styled control per view (Fluent 2 emphasis rule).
- **Back** navigates to the parent list named in the functional specification.

---

## STD-NAV-001 — Back navigation

- **Back** uses a **fixed label** and **fixed route** — not browser history.
- Label format: **`Back to {destination}`** (for example **`Back to issues list`**, **`Back to issue details`**, **`Back to my account`**).
- When the functional specification omits back navigation:

| Context            | Back label                   | Destination                        |
| ------------------ | ---------------------------- | ---------------------------------- |
| Create from a list | **`Back to {list name}`**    | Parent list                        |
| Edit from details  | **`Back to {details name}`** | Parent details for the same entity |

---

## STD-OP-001 — Destructive and irreversible actions

Unless the functional specification defines custom copy:

- Confirmation dialog title: action name (for example **`Delete issue`**).
- Confirmation body format: **`{Action} "{entity label}"? {Consequence.}`**
  - Example: **`Delete "{issue title}"? This action cannot be undone.`**
  - Example: **`Deactivate "{full name}"? The user will be signed out and cannot sign in until reactivated.`**
- **Cancel** closes without changes.
- **Confirm** executes the action, shows the success toast from the functional specification, and follows the functional specification navigation rule (stay in place vs navigate away).

---

## STD-FMT-001 — Date, time, and number display

Unless a functional specification specifies otherwise:

- Dates and times use the user's browser locale for formatting (see `NFR-I18N-001` in the quality layer).
- Functional specifications document exact **labels** and **placeholder tokens** in backticks; they are English canonical strings.
- Durations and counts use plain numbers in UI copy as defined in the functional specification (for example **`{n} permissions`**, **`{n} users`**).

---

## STD-RPT-001 — Reporting and export

Unless a functional specification specifies otherwise:

- Filters follow STD-LST-001 rules.
- Result tables are **server-paginated** with default **10** rows per page when showing row-level detail.
- **Export** actions show progress or loading on the control; success toast: **`Export started.`** or exact copy from the functional specification.

---

## How to reference in a functional specification

Frontmatter:

```yaml
inherits_conventions: [STD-LST-001, STD-VAL-001]
```

In **Functional requirements**:

```markdown
- Inherits STD-LST-001 unless stated below.
- Default sort: **Last activity** descending.
```

Do **not** copy pagination, loading, or filter control types when inheritance suffices. State only overrides and feature-specific data rules.

---

## Implementation review checklist

Use after implementing or reviewing a feature. Check only the `STD-*` sections listed in the target `FR-*` `inherits_conventions` (plus any section cited in the spec body). **L4 overrides L2** — if the `FR-*` states an exception, the exception wins.

| Check                    | STD         | Pass when…                                                                                                                                                  |
| ------------------------ | ----------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Access denied copy       | STD-ACC-001 | Rejection uses **`You do not have permission to perform this action.`**; unauthorized actions are hidden, not disabled                                      |
| Guest / sign-in          | STD-ACC-001 | Protected actions redirect unauthenticated users to sign-in                                                                                                 |
| Field validation UX      | STD-VAL-001 | Errors inline at the field; form stays open; values preserved; server errors map to the same positions                                                      |
| Success / error feedback | STD-MSG-001 | Mutations use toast; field errors do not use toast; destructive actions use confirmation dialog first                                                       |
| List screen              | STD-LST-001 | Server pagination (default 10); AND filters; clear resets; sort only on FR-defined columns; loading in table area; status uses tint badges with text labels |
| Embedded list / tabs     | STD-LST-002 | Show more or paginator per FR; upload/add controls stay visible while list loads                                                                            |
| Create / edit form       | STD-FRM-001 | Back/cancel leave without save; submit shows toast on success; form stays open on failure                                                                   |
| Detail view              | STD-DTL-001 | Sections grouped; header actions permission-gated; one primary action emphasis per view                                                                     |
| Back navigation          | STD-NAV-001 | Fixed label and route — not browser history                                                                                                                 |
| Delete / deactivate      | STD-OP-001  | Confirmation before irreversible action; success toast after confirm                                                                                        |
| Dates and counts         | STD-FMT-001 | Locale formatting; FR-defined labels in backticks                                                                                                           |
| Export                   | STD-RPT-001 | Only when the feature includes export                                                                                                                       |

### Review order

1. **L4** — every bullet in **Functional requirements** (authorization, data, operations, validation, business rules).
2. **L2** — rows above for inherited `STD-*` sections.
3. **L3** — quality docs in `inherits_quality` when the change touches perf, a11y, or i18n.
4. **L5** — code follows `docs/guides/` (Tailwind layout + `@laczynski/ui` components per `frontend-guidelines.md`); no product rules invented in implementation guides.

If a check fails and the `FR-*` does not document an override, fix the implementation or update the `FR-*` / `STD-*` deliberately in a requirements change — do not leave silent drift.

For **which automated test layer** should prove each `STD-*` row, see [testing-guidelines.md](../../../guides/testing-guidelines.md#mapping-std--to-test-layers) (L5).
