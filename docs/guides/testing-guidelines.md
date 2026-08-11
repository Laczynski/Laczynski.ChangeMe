# Testing Guidelines

> Scope: **default is no new test** unless a lower layer cannot catch the failure. Which layer owns what, anti-patterns, scenario pickers, and what to run before a PR.
>
> Commands, CI, paths, and integration-test setup: [`AGENTS.md`](../../AGENTS.md), [`docs/technical/ci.md`](../technical/ci.md), [`docs/technical/database-and-docker.md`](../technical/database-and-docker.md), and [repo-map.md](repo-map.md#test-map).

## Core rule

**Default: do not add a test** unless you can name a failure class that **no lower layer already covers**.

When you add one: ground it in touched `FR-*` bullets and inherited `STD-*` / `_shared/` docs — not ad-hoc acceptance tables. Use the **lowest** layer that can prove the requirement; extend existing tests before adding a higher layer.

See [Mapping STD-\* to test layers](#mapping-std--to-test-layers) when a change inherits L2 conventions.

Automated tests do **not** use `Template.Backend.DataGenerator` — they seed via `IssueTestHelper`, `TestAuthHelper`, and Testcontainers ([repo-map.md](repo-map.md#test-map), [`data-generator.md`](../technical/data-generator.md)).

## Layer ownership

| Layer                     | Owns                                                                                   | Does not own                                         |
| ------------------------- | -------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Backend unit              | Domain invariants, aggregate behavior, small helpers without app startup               | HTTP, auth middleware, persistence                   |
| Backend integration       | Routes, status codes, auth, server validation, persistence side effects, API contracts | Angular routing, templates, browser session          |
| Frontend unit / component | Client logic, forms, guards, UI state, service orchestration (mocked `ApiService`)     | Server rules already proven through HTTP             |
| E2E                       | Multi-screen user journeys, session/cookies, SignalR                                   | Per-field API validation, exhaustive CRUD per screen |

Colocate frontend specs as `*.spec.ts` next to the source. Integration tests: `src/Template.Backend/tests/Template.Backend.IntegrationTests/Endpoints/<Feature>/` (use sub-slices for nested routes, for example `Issues/Attachments/`).

## Mapping STD-\* to test layers

Use the target `FR-*` `inherits_conventions` to see which rows apply. **L4 business rules** (field limits, side effects, rejection messages with business meaning) are proven from `FR-*` bullets — the table below is for **L2 conventions** only. Full pass/fail criteria: [product-standards.md](../requirements/_shared/conventions/product-standards.md#implementation-review-checklist).

| STD             | What to prove                          | Lowest layer                         | Typical assertion                                                                            |
| --------------- | -------------------------------------- | ------------------------------------ | -------------------------------------------------------------------------------------------- |
| **STD-ACC-001** | Permission denial message              | **Integration**                      | `403` + body contains `You do not have permission to perform this action.`                   |
| **STD-ACC-001** | Unauthorized actions hidden in UI      | **Frontend unit**                    | Action button/menu item absent when permission mock omits grant; not merely `disabled`       |
| **STD-ACC-001** | Guest redirected from protected route  | **Frontend unit** (guard) or **E2E** | Guard redirects to login; E2E only when guard wiring or cookie journey is what changed       |
| **STD-VAL-001** | Inline field errors, form stays open   | **Frontend unit**                    | Invalid submit shows control-level error; values preserved; no navigation                    |
| **STD-VAL-001** | Server validation mapped to fields     | **Integration** + **Frontend unit**  | API returns validation problem; component maps to same field (integration first for shape)   |
| **STD-MSG-001** | Success toast after mutation           | **Frontend unit**                    | `ToastService` / `MessageService` spy called with FR success copy on mocked success response |
| **STD-MSG-001** | No toast for field validation          | **Frontend unit**                    | Invalid submit does not call toast                                                           |
| **STD-MSG-001** | Destructive confirmation before action | **Frontend unit**                    | Confirm dialog opens; cancel does not call API; confirm calls API                            |
| **STD-LST-001** | Server pagination, filters, sort query | **Integration**                      | `grid` query param or filter/sort params; response page size and total count                 |
| **STD-LST-001** | Client sends correct grid state        | **Frontend unit**                    | Filter/sort/pagination change updates service call args (mocked `ApiService`)                |
| **STD-LST-001** | Default sort / filter from FR          | **Integration** or **Frontend unit** | Whichever owns the default — do not duplicate in E2E                                         |
| **STD-LST-002** | Show more / embedded list paging       | **Integration**                      | Second page append; correct skip/take or page index                                          |
| **STD-FRM-001** | Submit success navigation              | **Frontend unit**                    | Router navigates to FR destination on mocked success                                         |
| **STD-FRM-001** | Back/cancel without save               | **Frontend unit**                    | Navigation without API submit call                                                           |
| **STD-NAV-001** | Fixed back label and route             | **Frontend unit**                    | `app-back-button` (or equivalent) `label` + `route` inputs match FR                          |
| **STD-OP-001**  | Delete/deactivate confirmation         | **Frontend unit**                    | Same as STD-MSG-001 destructive row                                                          |
| **STD-DTL-001** | Section/actions permission-gated       | **Frontend unit**                    | Section or header action absent without permission                                           |
| **STD-FMT-001** | Locale date/number display             | **Frontend unit** or manual          | Pipe/formatter output for fixture date; skip automated if copy-only                          |
| **STD-RPT-001** | Export flow                            | **Integration**                      | Export endpoint auth + response; frontend unit for button loading state                      |

**Skip E2E for:** toast copy, inline validation, hidden vs disabled actions, pagination query shape — unless the change is a multi-screen journey none of the above layers can reach.

**NFR (L3):** map separately — performance targets in integration/load tests when applicable; a11y often manual or dedicated tooling (see `NFR-A11Y-001`); i18n copy in frontend unit when pipes or labels change.

## Anti-patterns

**Do not** add or extend automated tests when:

| Layer               | Skip                                                                                                                                                                                                                                               |
| ------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Any                 | A lower layer already covers the failure; scenario is outside `FR-*`, `STD-*`, `_shared/`, or an explicit regression                                                                                                                               |
| Backend unit        | Behavior exists only at the HTTP boundary; the type has no domain rules                                                                                                                                                                            |
| Backend integration | No API or persistence change                                                                                                                                                                                                                       |
| Frontend unit       | Markup, layout, or styling only; duplicating server validation; smoke with no real assertion (`should create`, widget presence); real HTTP; full-template snapshots; asserting private fields instead of observable UI                             |
| E2E                 | API-only change, single form, or one list screen; re-checking status codes, field validation, or “save shows toast” without unique routing; mirroring integration test matrices in the browser; proving a single `STD-*` row already covered below |

**E2E only when** unit and integration cannot prove the journey: multi-step auth flows, cookies or browser APIs, SignalR (see FR-AUTH-001 for sign-in behavior). Record `required` / `optional` / `skip` (with reason) in `docs/requirements/changes/` when a user journey changes.

When frontend unit tests are warranted: Vitest + TestBed; mock `ApiService`; stub heavy children and layout shell; assert copy, disabled controls, and navigation.

## Scenario templates

Pick the lowest layers that satisfy the `FR-*` bullets. Dash (`—`) means **skip that layer**.

| Change                      | Backend unit        | Integration                          | Frontend unit                                                       | E2E                                                       | Minimum before PR                                                                  |
| --------------------------- | ------------------- | ------------------------------------ | ------------------------------------------------------------------- | --------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| List screen                 | If new domain rules | GET, filters, pagination, auth       | Filter → query mapping, chips, client empty state                   | Optional: login → row → details for new domain or routing | Skip E2E unless routing or domain is new                                           |
| Create / edit form          | If new domain rules | POST/PUT, server validation, auth    | Client validators, submit state, success navigation (mocked router) | Only multi-step or compliance workflows                   | Skip E2E for single-screen CRUD                                                    |
| Auth / session / compliance | If new domain rules | Flags, middleware, token endpoints   | Guards, toasts, redirects                                           | Required when user-visible gate journey changes           | Integration: anonymous, authenticated, forbidden as applicable                     |
| Visual / layout only        | —                   | —                                    | —                                                                   | —                                                         | `npm run lint:frontend` only — no new automated tests                              |
| API contract only           | If domain changed   | **First** — contract source of truth | Models/services if mapping changed                                  | Only if user flow changes                                 | Skip frontend/E2E when only server contract changed and mapping is already covered |

CI does **not** run ESLint or formatting — run `npm run lint:frontend` and `npm run format:check:all` locally when you touched matching code ([`ci.md`](../technical/ci.md)).

## What to run

Prefer the **smallest** relevant check. Command details: [`AGENTS.md`](../../AGENTS.md).

| Change                          | Run                                                                                                                             |
| ------------------------------- | ------------------------------------------------------------------------------------------------------------------------------- |
| Frontend logic or services      | `npm run lint:frontend` plus `npm run test:frontend:ci` (or affected specs)                                                     |
| Backend domain or helpers only  | `npm run test:backend:unit`                                                                                                     |
| Backend endpoint or persistence | `npm run test:backend:integration` — Docker required                                                                            |
| User journey or wide regression | `npm run test:all`; add `npm run test:e2e` when the journey or compliance gate changed ([e2e-guidelines.md](e2e-guidelines.md)) |
| No Docker available             | `npm run test:frontend:ci` and `npm run test:backend:unit` first; integration when Docker is up                                 |

## Guardrails for AI agents

- Name the failure class before adding a test; if a lower layer covers it, **stop**.
- Do not repeat HTTP contracts in frontend or E2E tests.
- Do not invent scenarios outside `FR-*`, inherited `STD-*`, or an explicit regression.
- Markup-only frontend change: lint — not component smoke tests.
- Cross-stack change: follow **What to run**; integration tests need Docker (Testcontainers).
