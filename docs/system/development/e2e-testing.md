# E2E testing

> Type: development
> Scope: system
> Status: implemented
> Canonical for: Playwright suite layout, fixtures, locators, and execution
>
> When to add E2E: [testing strategy](testing-strategy.md). Commands: [`AGENTS.md`](../../../AGENTS.md). Automation policy: [continuous integration](../operations/ci.md).

E2E is currently a local/manual suite and does not block CI, package publication, or GitLab Release creation. A future post-deployment execution model is intentionally deferred until target environments, credentials, test data, and runner network access are defined.

## Layout

```
e2e/
├── playwright.config.ts
├── tsconfig.json              # extends frontend tsconfig → @features/*, @shared/*
├── shared/
│   ├── global-setup.ts        # admin login → auth-storage.json
│   ├── env.ts                 # URLs, credentials, e2eTitle / e2eEmail
│   ├── test.ts                # test.extend({ apiClient })
│   ├── auth.fixture.ts        # loginViaUi (globalSetup + auth specs)
│   └── api/client.ts          # authenticated API client
└── features/<feature>/
    ├── *.api.ts               # HTTP arrange only (optional)
    ├── *.fixture.ts           # shared UI navigation/actions
    └── *.smoke.spec.ts
```

| Pattern           | Purpose                               |
| ----------------- | ------------------------------------- |
| `*.api.ts`        | HTTP arrange — not Playwright tests   |
| `*.fixture.ts`    | Reusable UI steps for one feature     |
| `*.smoke.spec.ts` | Test files                            |
| `shared/`         | Cross-cutting infra, not domain logic |

## Session

- **`globalSetup`** logs in seed admin → writes `shared/auth-storage.json` (gitignored).
- **`app` project** — most specs; uses saved `storageState`.
- **`auth` project** — `features/auth/` only; empty storage (redirect, login, logout).

Default credentials match `.env.example`: `admin@example.local` / `replace-for-local-development-123A` (`InitialAdministratorOptions`). When local `.env` uses different administrator credentials, expose matching `E2E_USER_EMAIL` and `E2E_USER_PASSWORD` values to the Playwright process.

| File                            | Role                                                  |
| ------------------------------- | ----------------------------------------------------- |
| `shared/auth.fixture.ts`        | `loginViaUi` — shared by `globalSetup` and auth specs |
| `features/auth/auth.fixture.ts` | Re-exports `login`; adds `logout` for auth-only tests |

## Locators

Prefer, in order:

1. `getByRole` (buttons, links, checkboxes, options, textboxes)
2. `getByLabel` (inputs, multiselects linked with `for` / `inputId`)
3. `getByPlaceholder` (search fields)

Avoid Tailwind layout classes as locators except where noted below.

```typescript
await page.getByRole("button", { name: "Create user" }).click();
await page.getByRole("textbox", { name: "Name" }).fill(title);
await page.getByRole("combobox", { name: "Roles" }).click();
await page.getByRole("option", { name: "User" }).click();
await page.keyboard.press("Escape");
await page.getByRole("checkbox", { name: "View users" }).click();
await expect(page.getByRole("main")).toContainText(title);
```

**PrimeNG selects (`p-select`):** scope with `getByRole('main').getByRole('region', { name: '…' })` and `.p-select`, or `getByRole('combobox', { name: '…' })` when the control exposes it. Pick `getByRole('option')`, press `Escape` to close. Permission checkboxes can make `getByLabel` ambiguous — prefer `getByRole('textbox', …)` for inputs. Use `expectDetailsTitle` in `*.fixture.ts` when the page title includes extra context (e.g. user name + email).

## Test data

| Helper                    | Use                                              |
| ------------------------- | ------------------------------------------------ |
| `e2eTitle('issues-list')` | Issues, roles — `E2E-<feature>-<timestamp>`      |
| `e2eEmail('users')`       | User emails — `E2E-<feature>-<uuid>@example.com` |
| `e2eTestPassword`         | Create-user password (`StrongPass123!`)          |

**No automated cleanup** — the suite does not delete issues, users, or roles after tests.

| Reason     | Detail                                                                                        |
| ---------- | --------------------------------------------------------------------------------------------- |
| Isolation  | `e2eTitle()` and `e2eEmail()` produce unique names — tests do not depend on an empty database |
| Simplicity | Less infrastructure (`afterAll`, registries, per-entity delete helpers)                       |

Reset a noisy local database with `npm run data:generate -- --reset`, or recreate the dev database. Revisit cleanup only if orphaned rows affect performance or assertions.

## API arrange

1. Arrange via API only — not for Act/Assert of UI behaviour.
2. Import payload types from frontend models (`IssueStatus`, `IssueDetailsDto`, …).
3. Never modify the seed administrator.

## Multi-step specs

Use `test.step()` in journeys that span several screens (users create→edit, roles create→edit). Single-purpose smoke tests can stay flat.

```typescript
await test.step("create user", async () => {
  /* … */
});
await test.step("edit profile", async () => {
  /* … */
});
```

## Playwright config

| Option                 | Value                                   |
| ---------------------- | --------------------------------------- |
| `workers`              | `1` — unique test data replaces cleanup |
| `fullyParallel`        | `false`                                 |
| `screenshot` / `trace` | `only-on-failure` / `retain-on-failure` |
| CI reporter            | `github`, `html`, `list`                |

Locally Playwright starts backend + frontend (`webServer`). MailHog starts via Docker when `CI` is unset.

## Smoke coverage

| Spec     | Scenarios                                |
| -------- | ---------------------------------------- |
| `auth`   | unauthenticated redirect; login + logout |
| `issues` | list → details; search; create via form  |
| `users`  | list → create → edit profile             |
| `roles`  | list → details; create → edit            |

## Out of scope (deferred)

- Restricted user (no `usersView` / `rolesView`)
- `data-testid` — only if accessible locators break repeatedly

## Run

```powershell
npm run install:frontend   # once — includes Chromium
npm run test:e2e           # PostgreSQL on localhost
npm run test:e2e:ui        # interactive debugging
```

## Related documents

- [Testing strategy](testing-strategy.md) — layer ownership
- [Repository map](repository-map.md) — where E2E lives in the repo
