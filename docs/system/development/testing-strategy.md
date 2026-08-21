# Testing strategy

> Type: development
> Scope: system
> Status: implemented
> Canonical for: test-layer ownership, intentional test omissions, and pre-PR test selection
>
> Commands: [`AGENTS.md`](../../../AGENTS.md). CI: [continuous integration](../operations/ci.md). Integration database: [backend persistence](../../modules/backend/operations/persistence.md). Paths: [repository map](repository-map.md#test-map).

## Core rule

**Default: do not add a test** unless you can name a failure class that **no lower layer already covers**.

When you add one: ground it in the touched `FR-*` bullets and any inherited L2/L3 documents referenced from that specification — not ad-hoc acceptance tables. Use the **lowest** layer that can prove the requirement; extend existing tests before adding a higher layer.

For UI work that inherits L2 conventions, use the **Implementation review checklist** in [product-standards.md](../../requirements/_shared/conventions/product-standards.md#implementation-review-checklist) as the canonical pass/fail criteria. Do not duplicate STD rows or exact assertion text in this document.

Automated tests do **not** use `ChangeMe.Backend.DataGenerator` — they seed via `IssueTestHelper`, `TestAuthHelper`, and Testcontainers ([repository map](repository-map.md#test-map), [demo data](../../modules/backend/demo-data.md)).

## Layer ownership

| Layer                     | Owns                                                                                              | Does not own                                         |
| ------------------------- | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------- |
| Backend unit              | Domain invariants, aggregate behavior, small helpers without app startup                          | HTTP, auth middleware, persistence                   |
| Backend integration       | Routes, status codes, auth, server validation, persistence side effects, API contracts            | Angular routing, templates, browser session          |
| Frontend unit / component | Client logic, forms, guards, UI state, service orchestration (mocked `ApiService`)                | Server rules already proven through HTTP             |
| E2E                       | Multi-screen user journeys, session/cookies, real-time transport the lower layers cannot exercise | Per-field API validation, exhaustive CRUD per screen |

Colocate frontend specs as `*.spec.ts` next to the source. Integration tests: `src/ChangeMe.Backend/tests/ChangeMe.Backend.IntegrationTests/Endpoints/<Feature>/` (use sub-slices for nested routes).

## Choosing a layer

1. Read the target `FR-*` and note `depends_on`, `inherits_conventions`, and `inherits_quality`.
2. Prove **L4 business rules** from the functional specification bullets at the lowest layer that can fail independently.
3. Prove **L2 conventions** only where the UI owns the behavior; use [product-standards.md](../../requirements/_shared/conventions/product-standards.md#implementation-review-checklist) for criteria and skip layers already covered below.
4. Treat **L3 quality** separately — performance, accessibility, and i18n expectations live in `docs/requirements/_shared/quality/`; automated coverage is optional unless the FR or quality doc makes it mandatory.

**Skip E2E for:** toast copy, inline validation, hidden vs disabled actions, pagination query shape — unless the change is a multi-screen journey none of the lower layers can reach.

## Anti-patterns

**Do not** add or extend automated tests when:

| Layer               | Skip                                                                                                                                                                                                                                            |
| ------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Any                 | A lower layer already covers the failure; scenario is outside the target requirements, inherited conventions/quality docs, or an explicit regression                                                                                            |
| Backend unit        | Behavior exists only at the HTTP boundary; the type has no domain rules                                                                                                                                                                         |
| Backend integration | No API or persistence change                                                                                                                                                                                                                    |
| Frontend unit       | Markup, layout, or styling only; duplicating server validation; smoke with no real assertion (`should create`, widget presence); real HTTP; full-template snapshots; asserting private fields instead of observable UI                          |
| E2E                 | API-only change, single form, or one list screen; re-checking status codes, field validation, or success feedback without unique routing; mirroring integration test matrices in the browser; proving behavior already covered at a lower layer |

**E2E only when** unit and integration cannot prove the journey: multi-step auth flows, cookies or browser APIs, or real-time transport that requires a browser session. Record `required` / `optional` / `skip` (with reason) in `docs/requirements/changes/` when a user journey changes.

When frontend unit tests are warranted: Vitest + TestBed; mock `ApiService`; stub heavy children and layout shell; assert observable UI behavior from the functional specification and inherited conventions.

## Scenario templates

Pick the lowest layers that satisfy the `FR-*` bullets. Dash (`—`) means **skip that layer**.

| Change                      | Backend unit        | Integration                          | Frontend unit                                                       | E2E                                                       | Minimum before PR                                                                  |
| --------------------------- | ------------------- | ------------------------------------ | ------------------------------------------------------------------- | --------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| List screen                 | If new domain rules | GET, filters, pagination, auth       | Filter → query mapping, chips, client empty state                   | Optional: login → row → details for new domain or routing | Skip E2E unless routing or domain is new                                           |
| Create / edit form          | If new domain rules | POST/PUT, server validation, auth    | Client validators, submit state, success navigation (mocked router) | Only multi-step or compliance workflows                   | Skip E2E for single-screen CRUD                                                    |
| Auth / session / compliance | If new domain rules | Flags, middleware, token endpoints   | Guards, toasts, redirects                                           | Required when user-visible gate journey changes           | Integration: anonymous, authenticated, forbidden as applicable                     |
| Visual / layout only        | —                   | —                                    | —                                                                   | —                                                         | `npm run lint:frontend` only — no new automated tests                              |
| API contract only           | If domain changed   | **First** — contract source of truth | Models/services if mapping changed                                  | Only if user flow changes                                 | Skip frontend/E2E when only server contract changed and mapping is already covered |

CI and local quality commands: [continuous integration](../operations/ci.md).

## What to run

Prefer the **smallest** relevant check. Command details: [`AGENTS.md`](../../../AGENTS.md).

| Change                          | Run                                                                                                                    |
| ------------------------------- | ---------------------------------------------------------------------------------------------------------------------- |
| Frontend logic or services      | `npm run lint:frontend` plus `npm run test:frontend:ci` (or affected specs)                                            |
| Backend domain or helpers only  | `npm run test:backend:unit`                                                                                            |
| Backend endpoint or persistence | `npm run test:backend:integration` — Docker required                                                                   |
| User journey or wide regression | `npm run test:all`; add `npm run test:e2e` when the journey or compliance gate changed ([E2E testing](e2e-testing.md)) |
| No Docker available             | `npm run test:frontend:ci` and `npm run test:backend:unit` first; integration when Docker is up                        |

## Guardrails for AI agents

- Name the failure class before adding a test; if a lower layer covers it, **stop**.
- Do not repeat HTTP contracts in frontend or E2E tests.
- Do not invent scenarios outside the target `FR-*`, its inherited L2/L3 references, or an explicit regression.
- Markup-only frontend change: lint — not component smoke tests.
- Cross-stack change: follow **What to run**; integration tests need Docker (Testcontainers).
