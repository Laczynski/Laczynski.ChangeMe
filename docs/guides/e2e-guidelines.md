# E2E testing guidelines

> Scope: Playwright smoke suite in `src/ChangeMe.Frontend/e2e/`.
>
> When to add E2E: [testing-guidelines.md](testing-guidelines.md). Commands: [`AGENTS.md`](../../AGENTS.md). CI: [ci.md](../technical/ci.md).

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

Default credentials: `admin@example.local` / `admin123` (`InitialAdministratorOptions`), overridable via `E2E_USER_EMAIL` and `E2E_USER_PASSWORD`.

| File                            | Role                                                  |
| ------------------------------- | ----------------------------------------------------- |
| `shared/auth.fixture.ts`        | `loginViaUi` — shared by `globalSetup` and auth specs |
| `features/auth/auth.fixture.ts` | Re-exports `login`; adds `logout` for auth-only tests |

## Locators

Prefer, in order:

1. `getByRole` (buttons, links, checkboxes, options, textboxes)
2. `getByLabel` (inputs, multiselects linked with `for` / `inputId`)
3. `getByPlaceholder` (search fields)

Avoid Laczynski internal class selectors and Tailwind layout classes except where noted below.

```typescript
await page.getByRole("button", { name: "Create user" }).click();
await page.getByRole("textbox", { name: "Name" }).fill(title);
await page
  .getByRole("main")
  .getByRole("region", { name: "Roles" })
  .getByRole("combobox")
  .click();
await page.getByRole("option", { name: "Administrator" }).click();
await page.getByRole("checkbox", { name: "View users" }).click();
await expect(page.getByRole("main")).toContainText(title);
```

**@laczynski/ui:** prefer `getByRole` for fields and buttons. For `ui-select` / multiselect panels, scope to `getByRole("main").getByRole("region", { name: "…" })`, open the combobox, pick `getByRole("option")`, press `Escape`. Collapsible `ui-accordion` sections and permission checkboxes can make `getByLabel` ambiguous — prefer `getByRole("textbox", …)` for inputs. Use `expectDetailsTitle` in `*.fixture.ts` when the page title includes extra context (e.g. user name + email).

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
| CI         | Each GitHub Actions job uses a fresh PostgreSQL service                                       |
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

- [testing-guidelines.md](testing-guidelines.md) — layer ownership
- [repo-map.md](repo-map.md) — where E2E lives in the repo
