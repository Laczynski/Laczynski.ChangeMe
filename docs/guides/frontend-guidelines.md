# Frontend Guidelines

> **L5 — Implementation.** Scope: current conventions for writing Angular code in this frontend.
>
> **Product behaviour** (lists, forms, validation UX, toasts): [`product-standards.md`](../requirements/_shared/conventions/product-standards.md) (L2). **Feature rules**: target `FR-*` (L4). This file covers _how_ to implement in Angular with **@laczynski/ui** and **Tailwind CSS**.

## Stack summary

Angular 21 standalone application with strict TypeScript settings, ESLint, and Prettier. UI components come from [**@laczynski/ui**](https://ui.laczynski.dev/) — a Fluent 2–inspired Angular component library. **Layout, spacing, and page chrome use Tailwind CSS v4 utilities in templates**, backed by Laczynski design tokens (`--color-*`) bridged into Tailwind via `@theme` in `src/tailwind.css`. State uses a mix of Angular signals and RxJS Observables. HTTP calls go through a shared `ApiService`. Feature code is grouped under `src/app/features`.

For dev server, lint, format, and test commands from `src/ChangeMe.Frontend` or from the repository root (`npm run start:frontend`, `npm run lint:frontend`, and related scripts), see `AGENTS.md`.

## Project structure

- Routes are declared centrally in `src/app/app.routes.ts`.
- Use path aliases from `tsconfig.json` instead of long relative imports when an alias exists.
- Put feature-specific models in `features/<feature>/models`.
- Put validation limits, select options, labels, and other UI-oriented constants in a single `features/<feature>/utils/<feature>.utils.ts` file (for example `issue.utils.ts`, `auth.utils.ts`). Keep DTOs, enums, and request/response shapes in `models/`.
- Put shared transport or utility contracts in `shared/`.
- Put cross-cutting app services in `core/` or `features/auth/` depending on ownership.
- Use `app-back-button` with a fixed **label** and **route** for in-app back navigation (for example **`Back to issues list`** → **`/issues`**, **`Back to issue details`** → **`/issues/:id`**). Do not use browser history stacks or `sessionStorage` navigation stacks.

## Component rules

### Standalone components

- New components should remain standalone.
- Declare Angular and Laczynski UI dependencies in the component `imports` array.
- Keep route components under `features/<feature>/components/<component-name>/`.

### State and data loading

- Services return `Observable<T>`.
- Components may use signals for local UI state and derived values.
- Avoid scattering raw `HttpClient` usage through components; route all HTTP through feature services and the shared `ApiService`.
- When loading data in response to changing state, prefer an explicit RxJS pipeline over ad hoc nested subscriptions.

### Dependency injection

- Prefer `inject()` field initializers, matching the current codebase pattern.

### Naming

- Use descriptive names for signals, request objects, and callback parameters.
- Keep identifiers in English.
- Keep route paths and selectors consistent with existing feature naming.

## Service rules

- Feature services live in `features/<feature>/services`.
- Feature services own HTTP calls and orchestration only. Do not put validation limits or static select-option lists in services; import them from `features/<feature>/utils`.
- Shared request/response handling belongs in `shared/api/services/api.service.ts`.
- Keep endpoint strings centralized per service through a `baseEndpoint` field when the service owns one API area.

## API base URL

- **Development (`ng serve`):** explicit in `environment.development.ts` (`http://localhost:5000/api/v1`).
- **Production / Docker:** explicit in `public/runtime-config.js` (`apiUrl: '/api/v1'`) or `CHANGE_ME_API_URL` — no value in `environment.ts`.
- HTTP and SignalR services use `getApiUrl()` / `getNotificationsHubUrl()` from `src/environments/runtime-config.ts`.
- Deployment patterns (nginx `/api` proxy, CORS, split hosts): [deployment.md](../technical/deployment.md).

## Forms and templates

- Follow the existing Angular standalone template style already used in the repo.
- Use typed reactive forms with explicit control binding in templates: `[formControl]="form.controls.field"` (or `filtersForm.controls.field`, `criterion.controls.content`, and so on). Do not use `formControlName`, `formGroupName`, or `formArrayName`.
- Keep `[formGroup]="form"` on the `<form>` element for submit handling and group-level validators.
- For `FormArray` rows, iterate `form.controls.arrayName.controls` and bind nested fields with `[formControl]="row.controls.field"` from the loop variable.
- Keep user-facing text consistent within a feature. If a feature is already English-only in UI text, do not partially localize one screen.
- Prefer moving formatting or mapping logic out of templates when it starts to obscure the markup.

## @laczynski/ui

### Global setup

- Laczynski component styles are registered once in `angular.json` via `src/laczynski-vendor.scss` (`@use '@laczynski/ui/src/lib/scss/main'`).
- Tailwind is registered in `angular.json` via `src/tailwind.css` (with PostCSS `@tailwindcss/postcss`).
- Icon sprite assets are copied from `node_modules/@laczynski/ui/assets/icons` to `/assets/icons` in `angular.json`.
- Root shell renders `<ui-toast-container />` and `<app-confirm-dialog />` in `app.component.ts`.
- Do not add a second UI kit beside `@laczynski/ui` for application screens.

### Fluent 2 alignment

Laczynski UI follows [Fluent 2](https://fluent2.microsoft.design/) design principles. When choosing variants, spacing, and hierarchy:

- **One primary action per view** — use `variant="primary"` on a single main button (Create, Save); secondary actions use `variant="secondary"`.
- **Semantic surfaces** — page background uses neutral background 2; cards and form panels use background 1 with stroke 1 borders (`ui-card`, bordered form workspaces).
- **Typography hierarchy** — page titles are semibold; subtitles and helper text use foreground 2 / foreground 3 tokens, not ad hoc gray hex values.
- **Status and badges** — use `ui-tag` with `appearance="tint"` and semantic `variant` (`success`, `warning`, `danger`, `info`); never rely on color alone (see `NFR-A11Y-001`).
- **Icons** — use Fluent icon names on `ui-button`, `ui-nav`, and field components; icons are decorative only when adjacent text already conveys meaning.
- **Density** — default Laczynski `size="medium"`; do not mix sizes within one form row unless the functional specification requires compact layout.

See [Laczynski UI docs](https://ui.laczynski.dev/) for per-component API, workspace compositions (tabs, nav, drawer), and validation patterns.

### Component usage

- Import Laczynski components from `@laczynski/ui` in the standalone `imports` array of the component that uses them.
- Prefer Laczynski field components (`ui-text`, `ui-textarea`, `ui-email`, `ui-password`, `ui-select`, `ui-checkbox`, `ui-file`) with reactive forms and `[formControl]` binding.
- Bind validation messages with `[errorText]` and `fieldError()` from `@shared/forms/field-error.ts` (presentational fields; no `autoValidation`).
- Reserve `ui-card` for content blocks (metadata panels, permission groups, tab sections), not whole-page wrappers — page chrome is plain layout markup with Tailwind utilities.
- Use `ui-nav` for sidebar navigation (see `app-sidebar-nav`). Do not build primary navigation from `ui-button` modifiers.
- Use `ui-accordion` for grouped form sections. Accordions should be open by default — add the `appDefaultExpanded` attribute and import `DefaultExpandedAccordionDirective` from `@shared/directives/default-expanded-accordion.directive`.
- Use `ui-message-bar` for screen-level request errors.
- Use the app `ToastService` facade in features for mutation feedback; do not inject Laczynski `ToastService` directly in feature code.
- Use `ui-tag` for compact status labels such as issue status or priority (`variant`: `primary` | `secondary` | `success` | `warning` | `danger` | `info`).
- Use `ui-button` with `text`, `icon` (Fluent icon name), `variant`, `appearance`, and `(click)`.
- Use `ui-spinner` or grid loading state for in-flight data.
- Use `uiTooltip` for icon-only or compact action hints.
- Use `ConfirmService` (`@core/confirm/services/confirm.service.ts`) for destructive confirmations; do not open ad hoc dialogs for the same pattern.
- **List screens** (Issues, Users, Roles) use `<qg-ui-data-grid>` from `@query-grid/ui` with `createAppGridResource` from `@shared/data/utils/grid.utils.ts`. Column templates use the `qgColumn` directive; feature services pass `GridQuery` to the API as a `grid` query parameter and return `GridResult<T>`. See `features/issues/components/issues-list/` as the reference implementation.
- **Embedded lists** (issue tabs, sessions, notifications, role assigned users) call the same API shape with `GridQuery`/`GridResult`; use `shared/data/utils/grid.utils.ts` for `createGridQuery`, `hasMoreGridItems`, and `createIssueTabGridQuery`.
- Keep business logic in feature services and component TypeScript. Laczynski UI should handle presentation only.

### Tailwind and layout

**Prefer Tailwind utilities in templates for one-off layout.** Reuse the **shared CSS classes** in `src/tailwind.css` for repeated detail/list/table patterns — do not duplicate their rules in feature SCSS or long utility chains.

Global CSS is limited to:

1. Laczynski library import (`src/laczynski-vendor.scss`)
2. Brand token overrides (`src/changeme-theme.css`) — edit here for primary/hover/pressed brand colours; light primary is `#d81e04`
3. Tailwind `@theme` token bridge (map Laczynski `--color-*` CSS variables to Tailwind semantic colors)
4. Base `html` / `body` rules (font, page background)
5. **Shared layout classes** (`detail-*`, shell, list grids, notifications) and rare Laczynski internal overrides that cannot be reached from templates

#### Token bridge (`src/tailwind.css`)

Expose Laczynski Fluent tokens to Tailwind so utilities stay semantic:

```css
@import "tailwindcss" important;

@theme {
  --color-foreground: var(--color-neutral-foreground-rest);
  --color-foreground-2: var(--color-neutral-foreground2-rest);
  --color-foreground-3: var(--color-neutral-foreground3-rest);
  --color-surface-1: var(--color-neutral-background-rest);
  --color-surface-2: var(--color-neutral-background2-rest);
  --color-surface-3: var(--color-neutral-background3-rest);
  --color-stroke: var(--color-neutral-stroke-rest);
  --color-brand: var(--color-brand-primary);
}

@custom-variant dark (&:where(.dark, .dark *));
```

Use `text-foreground`, `text-foreground-2`, `bg-surface-1`, `bg-surface-2`, `bg-surface-3`, `border-stroke`, `text-brand`, and standard Tailwind spacing in templates. When a token has no Tailwind alias yet, use arbitrary values: `text-[var(--color-neutral-foreground2-rest)]`.

`@import 'tailwindcss' important` is required so Tailwind utilities beat Laczynski unlayered component CSS.

#### Shared CSS classes (`tailwind.css`)

Reuse these instead of inventing new patterns. Reference: `features/issues/components/issue-details/`, `issues-list/`, `notifications-panel/`.

| Class                                                        | Use for                                                                     |
| ------------------------------------------------------------ | --------------------------------------------------------------------------- |
| `detail-meta`                                                | Read-only metadata grid (`<dl>`) on detail pages — `surface-3`, 2/3 columns |
| `detail-meta__term`                                          | Label (12px, `foreground-3`)                                                |
| `detail-meta__value`                                         | Value (14px semibold); add `--wrap` or `--date` modifiers                   |
| `detail-section-title`                                       | Section heading (14px semibold)                                             |
| `detail-inset`                                               | Inset panel (`surface-3`) — tab content, tables, permission lists           |
| `detail-tab-panel`                                           | Tab root wrapper                                                            |
| `detail-tab-list` / `detail-tab-item`                        | Vertical lists (comments, attachments, notifications)                       |
| `detail-tab-item__title` / `__meta` / `__body` / `__submeta` | Item typography                                                             |
| `detail-tab-empty`                                           | Empty state copy                                                            |
| `detail-tab-actions`                                         | Centred “Show more” row                                                     |
| `detail-table` + `detail-table-wrap`                         | Embedded HTML tables (sessions, assigned users)                             |
| `detail-table__link` / `__text` / `__date` / `__muted`       | Table cell content in grids and detail tables                               |

**List grids (`qg-ui-data-grid`):** wrap the page section in `app-page-fill app-page-grid`. Grid chrome (toolbar, table, pagination, column-filter popovers) is styled under `.app-page-grid qg-ui-data-grid` in `tailwind.css` — extend there, not in feature CSS. In column templates use `detail-table__link`, `detail-table__date`, etc. Set `[hoverable]="true"` on the grid. Empty state: `detail-tab-empty`.

**Do not** use `:has(form)` or other implicit selectors on `.app-shell-page` for layout — only `app-page-fill` triggers fill-height behaviour (see below).

#### Shell (`app-shell`)

Authenticated layout:

- **Sidebar** — full viewport height; brand “ChangeMe” in sidebar on `md+`
- **Toolbar** — only above main content (not over sidebar); actions (notifications, theme, logout)
- **Main** — `surface-2` padding; inner **`app-shell-page`** card (`surface-1`, `rounded-xl`)

Detail pages: `app-shell-page` grows with content; `app-shell-main` scrolls (`overflow-y-auto`).

List / create-edit workspace pages: add **`app-page-fill`** on the route root `<section>`. That makes `.app-shell-page` flex-fill so the grid or form scrolls inside the card. **Do not** put `app-page-fill` on detail pages (comments/forms there must not trap overflow).

#### Page layout patterns

Authenticated pages use the full workspace width (`max-w-none`). Do not reintroduce a narrow content column.

**Standard list page** — add `app-page-fill app-page-grid` on the root section:

```html
<section
  class="app-page-fill app-page-grid mx-auto flex min-h-0 w-full max-w-none flex-1 flex-col gap-3 overflow-hidden"
  aria-label="Issues list"
>
  <header class="grid shrink-0 gap-1.5 pb-2">…</header>
  <div class="flex shrink-0 flex-wrap items-center gap-2">…</div>
  <div class="flex min-h-0 flex-1 flex-col gap-3 overflow-hidden">
    <qg-ui-data-grid
      class="flex min-h-0 flex-1 flex-col overflow-hidden"
      [hoverable]="true"
      …
    />
  </div>
</section>
```

**Create / edit workspace** — same `app-page-fill` on the root section; sticky footer form pattern unchanged.

**Detail screens** — metadata `detail-meta`; sections with `detail-section-title` + `detail-inset`; tabs with `ui-tabs` `appearance="transparent"` and tab body in `detail-inset`. See `issue-details` (comments / attachments / history tabs).

**Embedded tables** — `detail-table-wrap` > `detail-table`; links via `detail-table__link`.

**Notifications panel** — `app-notifications-panel` classes in `tailwind.css`; template uses `detail-tab-*` for list rows.

**Field grid** — two columns from `sm`, three from `lg`:

```html
<div class="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">…</div>
```

**Permission checklists** — `ui-accordion` + `detail-tab-list` / `detail-tab-item` (see `effective-permissions`).

#### Styling rules

- Put layout utilities in the template. Use component `host: { class: 'flex min-h-0 flex-1 flex-col' }` when the host element needs layout classes.
- Do **not** use inline `style` attributes.
- Do **not** add feature-level `*.component.css` / `*.component.scss` files unless a rule cannot be expressed with Tailwind or shared classes.
- Do **not** add new global SCSS partials — extend `tailwind.css` (`@theme`, shared classes, or scoped `.app-page-grid` / `.app-notifications-panel` blocks).
- Restyle Laczynski components through documented inputs (`variant`, `appearance`, `size`, `class`) when possible; grid/shell/popover overrides live in `tailwind.css`.
- Application font is **Inter** via `@fontsource-variable/inter` in `src/tailwind.css`.
- Dark mode: `LayoutService` sets `data-theme="dark"` and `.dark` on `<html>`. `public/theme-init.js` restores the saved theme before Angular boots. Brand tokens for dark mode are in `changeme-theme.css`.
- **Reduced motion** (`NFR-A11Y-001`): `LayoutService` toggles `app-reduced-motion` on `<html>`; global styles shorten non-essential transitions.

### When adding a new screen

- Look at `features/auth` for form patterns and `features/issues` for grids, filters, and detail layouts.
- **New list page:** copy `issues-list` (`app-page-fill`, `app-page-grid`, grid column classes).
- **New detail page:** copy `issue-details` (`detail-meta`, `detail-inset`, tabs).
- **New global look (tables, shell, filters):** add to `tailwind.css` under the existing `detail-*` / `.app-page-grid` / shell sections — update this guide if you introduce a new shared class.
- Issues routes are behind `authGuard`. Do not gate issues UI with `isAuthenticated`; keep auth checks in guards, `app.component` navigation, and `NotificationsRealtimeConnectionService` (push notifications only).

## Existing repo patterns worth preserving

- Auth session state lives in `features/auth/services/auth.service.ts`.
- API response unwrapping and error conversion live in `shared/api/services/api.service.ts`.
- Route guarding stays in `features/auth/guards`.

## When changing frontend contracts

- If a backend DTO changes, update the matching frontend model first.
- Then update the feature service.
- Then update affected components and routes.
- Re-check auth-sensitive flows if the endpoint requires a token.

## Guardrails for AI agents

- Do not introduce a second HTTP abstraction beside `ApiService`.
- Do not create a new top-level frontend folder unless the existing `core` / `features` / `shared` split cannot fit the change.
- Do not hardcode backend URLs outside `environment.*` and the shared API layer.
- Before adding a new pattern, look for the nearest example in `features/issues` or `features/auth`.
- Follow Laczynski UI defaults: `variant="secondary"`, `appearance="filled"`, `size="medium"` unless the screen needs emphasis (`variant="primary"` for the main action only).
- Prefer Tailwind utilities and **shared `detail-*` / `app-page-*` classes** over new global CSS. If you reach for SCSS, stop and check whether utilities or `tailwind.css` shared classes can cover the case.
- New shared visual patterns belong in `src/tailwind.css` with a short note in this guide — see **Shared CSS classes** above.
