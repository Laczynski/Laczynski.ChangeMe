# Frontend Guidelines

> **L5 — Implementation.** Scope: current conventions for writing Angular code in this frontend.
>
> **Product behaviour** (lists, forms, validation UX, toasts): [`product-standards.md`](../requirements/_shared/conventions/product-standards.md) (L2). **Feature rules**: target `FR-*` (L4). This file covers _how_ to implement in Angular with **spartan/ui**.

## Stack summary

Angular 22 standalone application with strict TypeScript settings, ESLint, and Prettier. UI components come from **[spartan/ui](https://www.spartan.ng)** (`@spartan-ng/brain` primitives + Helm styles copied into `src/app/shared/ui`). Layout and spacing use **Tailwind CSS v4** with Spartan theme tokens (`--background`, `--foreground`, `--border`, …). Icons use **Lucide** via `@ng-icons/lucide`. State uses a mix of Angular signals and RxJS Observables. HTTP calls go through a shared `ApiService`. Feature code is grouped under `src/app/features`.

For dev server, lint, format, and test commands from `src/Template.Frontend` or from the repository root (`npm run start:frontend`, `npm run lint:frontend`, and related scripts), see `AGENTS.md`.

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
- Declare Angular and router dependencies in the component `imports` array.
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

## Spartan UI

### Global setup

- Spartan is initialized in `src/tailwind.css` (`@import '@spartan-ng/brain/hlm-tailwind-preset.css'`) with theme variables in `:root` / `:root.dark`.
- `provideSpartanHlm()` is registered once in `src/app/app.config.ts`.
- Helm component sources live under `src/app/shared/ui/<component>/` and are imported through the `@spartan/ui/<component>` path alias.
- Add new primitives with `ng g @spartan-ng/cli:ui <name> --directory src/app/shared/ui` (requires `components.json`).
- Root overlays: `<hlm-toaster>` and `<app-confirm-dialog-host>` in `app.component.ts`.

### Component usage

- Import Helm `*Imports` arrays in the standalone `imports` array of the component that uses them. Do not create a global `SharedModule`.
- **Buttons:** `<button hlmBtn variant="default|outline|secondary|ghost|destructive|link" size="default|sm|lg|icon">`. Use `<ng-icon name="lucide…">` for icons; register icons with `provideIcons` in the component.
- **Inputs / textarea / label:** `hlmInput`, `hlmTextarea`, `hlmLabel` with `[formControl]` binding. Set `[forceInvalid]="control.touched && control.errors"` on inputs and select triggers.
- **Select:** `hlm-select` with `hlm-select-trigger`, `hlm-select-value`, `hlm-select-content *hlmSelectPortal`, and `hlm-select-item [value]`. Bind `[value]` / `(valueChange)` to the form control.
- **Checkbox:** `<hlm-checkbox [formControl]="…">`.
- **Cards:** `section hlmCard` with `hlmCardHeader`, `hlmCardTitle`, `hlmCardDescription`, `hlmCardContent`, `hlmCardFooter`.
- **Alerts:** `hlm-alert variant="destructive"` for screen-level errors; field errors use `<p hlmAlertDescription class="text-destructive text-sm">…</p>`.
- **Badges:** `span hlmBadge [variant]="mapBadgeSeverity(severity)"` — map domain severities through `shared/ui/utils/badge.utils.ts`.
- **Loading:** `hlm-spinner` in the target content area.
- **Tabs:** `hlm-tabs` / `hlm-tabs-list` / `hlm-tabs-trigger` / `hlm-tabs-content`.
- **Tooltips:** `hlmTooltip` on the host element.
- **Mobile drawer:** `hlm-sheet` with `[state]` / `(stateChanged)`.
- **Toasts:** inject `ToastService` in features; do not call `@spartan-ng/brain/sonner` `toast` directly.
- **Confirmations:** inject `ConfirmDialogService` and call `confirm({ header, message, acceptLabel, rejectLabel, acceptVariant, accept })`.
- Keep business logic in feature services and component TypeScript. Spartan handles presentation only.

### Tables

- **List screens** (Issues, Users, Roles) use **DataGrid** via `@laczynski/datagrid-spartan` (`<dg-spartan-data-grid>`).
- **Embedded tables** (sessions, assigned users) use native `<table>` markup with Spartan/Tailwind styling and `app-grid-paginator` for skip/take pagination.

### Theming and layout

- Global styles live in `src/tailwind.css`. Register that file in `angular.json` `styles`.
- Prefer Spartan semantic Tailwind tokens (`bg-background`, `text-foreground`, `text-muted-foreground`, `border-border`, `bg-card`, `bg-primary`) instead of custom colors.
- Use Tailwind utility classes in templates for layout (`flex`, `grid`, `gap-*`, `p-*`, `max-w-*`, `rounded-*`, `dark:` variants). Do not add feature-level `*.component.css` files for layout.
- Put layout classes on elements you control in the template, or on the component `host` metadata (`host: { class: 'flex flex-1 flex-col' }`).
- Application font is **Inter** (`@fontsource-variable/inter` in `tailwind.css`).
- Dark mode: `LayoutService` toggles `dark` (and `app-dark` for compatibility) on `<html>`. Tailwind uses `@custom-variant dark` in `tailwind.css`. `theme-init.js` restores the class before bootstrap.
- Toggle light/dark through `LayoutService`; the shell header theme button calls `layoutService.toggleTheme()`.
- **Reduced motion** (`NFR-A11Y-001`): `LayoutService` toggles `app-reduced-motion` on `<html>`; global styles shorten non-essential transitions.

### When adding a new screen

- Look at `features/auth` for Spartan form patterns and `features/issues/components/create-issue` for card/section layout.
- Issues routes are behind `authGuard`. Do not gate issues UI with `isAuthenticated`; keep auth checks in guards, `app.component` navigation, and `NotificationsRealtimeConnectionService` (push notifications only).
- Match existing Tailwind layout patterns (`flex flex-col gap-1.5`, `flex flex-wrap items-center gap-3`, `grid gap-4 sm:grid-cols-2`) before introducing new one-off utilities.

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
