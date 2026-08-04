# Frontend Guidelines

> **L5 — Implementation.** Scope: current conventions for writing Angular code in this frontend.
>
> **Product behaviour** (lists, forms, validation UX, toasts): [`product-standards.md`](../requirements/_shared/conventions/product-standards.md) (L2). **Feature rules**: target `FR-*` (L4). This file covers _how_ to implement in Angular/PrimeNG.

## Stack summary

Angular 22 standalone application with strict TypeScript settings, ESLint, and Prettier. UI components come from **PrimeNG** with the **Aura**-based `AppPreset` from `@primeuix/themes`. Layout and spacing use **Tailwind CSS v4** with the official `tailwindcss-primeui` plugin. State currently uses a mix of Angular signals and RxJS Observables. HTTP calls go through a shared `ApiService`. Feature code is grouped under `src/app/features`.

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

## PrimeNG

### License (PrimeUI)

> **Warning:** From PrimeNG v22, the library is part of **PrimeUI** and requires a valid license key for production use. Without a key, PrimeNG may display a license notice in the UI.

- **Community License (free)** — for eligible small teams, students, non-profits, and non-commercial OSS. See [Community License](https://primeui.dev/licenses/community).
- **Commercial License (paid)** — for organizations that do not qualify for the community tier. See [Commercial License](https://primeui.dev/licenses/commercial).
- Full terms are also in `node_modules/primeng/LICENSE.md`.

Set the key once in `providePrimeNG()` in `src/app/app.config.ts`:

```typescript
providePrimeNG({
  license: "<your-primeui-license-key>",
  // ...
});
```

Do not commit license keys to the repository. For local development, use a personal/community key or accept the in-app notice until a project key is provisioned.

### Global setup

- Theme and PrimeNG providers are configured once in `src/app/app.config.ts` through `providePrimeNG()`. PrimeNG v22 uses CSS-based animations; do not add deprecated `provideAnimationsAsync()`.
- Ripple is enabled globally via `ripple: true` in `providePrimeNG()`.
- Shared icon styles are imported globally from `src/tailwind.css` (`primeicons`).
- Do not add a second UI kit beside PrimeNG for application screens.

### Component usage

- Import PrimeNG modules in the standalone `imports` array of the component that uses them. Do not create a global `SharedModule`.
- Prefer PrimeNG form controls (`pInputText`, `pTextarea`, `p-select`, `p-multiselect`, `p-checkbox`, `p-password`) with reactive forms and `[formControl]` binding instead of native HTML inputs.
- For validated fields, bind invalid state to PrimeNG: `[invalid]="form.controls.field.touched && form.controls.field.errors"`.
- Wrap page content in `p-card` when a screen needs a clear content frame; use `p-fluid` on forms that should stretch inputs to the container width.
- Use `p-message` for inline validation and request errors. Put the message copy inside the tag (`<p-message>…</p-message>`); do not use the deprecated `text` input.
- PrimeNG exposes toast through `MessageService` (`add` / `clear`) plus `<p-toast>` in the root template. Use the app `ToastService` facade in features so `key`, `life`, and severity helpers stay consistent; do not inject `MessageService` in feature code.
- Use `p-message` for inline field validation and screen-level load errors; use toasts for successful mutations and action failures that are not tied to a single form field.
- Use `p-tag` for compact status labels such as issue status or priority.
- Use `p-table` for tabular data, `p-paginator` for server-driven paging, and `p-progressSpinner` or table `[loading]` for in-flight data.
- **List screens** (full-page tables and grids — for example Issues, Users, Roles) use **QueryGrid** via `@query-grid/primeng` (`<qg-prime-data-grid>`, `QgColumnDirective`, `GridResourceFactory`). Column-header filters, multi-sort, search, and pagination are driven by a `GridResource` created in the component; the feature service passes `GridQuery` to the API as a `grid` query parameter and returns `GridResult<T>` from `@query-grid/core`. See `features/issues/components/issues-list/` as the reference implementation.
- **Embedded lists** (issue tabs, sessions, notifications, role assigned users) call the same API shape with `GridQuery`/`GridResult`; use `shared/data/utils/grid.utils.ts` for `createGridQuery`, `hasMoreGridItems`, and `createIssueTabGridQuery`.
- Keep business logic in feature services and component TypeScript. PrimeNG should handle presentation only.

### Theming and layout

- Global styles live in `src/tailwind.css` (Tailwind, `tailwindcss-primeui`, and `primeicons`). Register that file in `angular.json` `styles`.
- Prefer PrimeNG semantic Tailwind utilities from the plugin (`bg-surface-0`, `text-color`, `text-muted-color`, `bg-primary`, `border-surface-200`) instead of custom colors.
- Use Tailwind utility classes in templates for layout and surface styling (`flex`, `grid`, `gap-*`, `p-*`, `max-w-*`, `rounded-*`, `border-surface-200`, `dark:` variants). Do not restyle PrimeNG components with custom CSS unless there is no built-in option.
- Do **not** add feature-level `*.component.css` files that `@reference` `tailwind.css` and use `@apply` or custom class names for layout or surface styling. Put utilities in the template; use the component `host` metadata (for example `host: { class: 'flex flex-1 flex-col' }`) when the host element needs layout classes.
- Put PrimeNG semantic color and surface utilities (`bg-surface-0`, `border-surface-200`, `text-color`, `dark:` variants) on elements you control directly in the template. Do not wrap them in custom CSS classes unless you must target PrimeNG internal markup.
- Omit `styleUrl` on feature components unless a screen has a rare rule that cannot be expressed with template utilities or PrimeNG inputs (`class`, `pt`, and so on).
- For host-enabled PrimeNG components (`<p-* />` whose visible root is the custom element, e.g. `p-progress-spinner`, `p-timeline`), use the native `class` attribute instead of deprecated `styleClass`.
- Overlay components that render their panel inside the template (`p-drawer`, `p-dialog`) still need `styleClass` to style the visible surface — `class` on the host does not reach the inner panel.
- Use named `ng-template` references (`#marker`, `#content`, …) instead of deprecated `pTemplate`.
- Use `ButtonDirective` (`pButton`) on native `<button>` or `<a>` elements. Do not use deprecated `<p-button>`. Put icons (`<i class="pi …">`) and label text as direct children; use `[iconOnly]="true"` with `aria-label` for icon-only actions. For loading, set `[disabled]` and render `<i class="pi pi-spin pi-spinner">` instead of the regular icon. Import `ButtonDirective` from `primeng/button`.
- On `p-drawer`, use `closable` instead of deprecated `showCloseIcon`.
- Theme preset extensions belong in `src/app/theme/app-preset.ts`. To switch the base look, start from another preset (`Lara`, `Nora`, `Material`) in that file.
- Application font is **Inter** (Google Fonts in `index.html`, mirrored in `AppPreset` and `@theme` in `tailwind.css`).
- Dark mode follows PrimeNG styled mode: set `darkModeSelector: '.app-dark'` in `providePrimeNG()`, toggle that class on `<html>` in `LayoutService`, and mirror it for Tailwind with `@custom-variant dark` in `tailwind.css`. Page background and text color live in `tailwind.css` on `html` / `html.app-dark` (PrimeNG tokens); do not duplicate them on the app shell.
- The small inline script in `index.html` only restores `app-dark` from `localStorage` before Angular boots to avoid a light flash on reload. It is optional if you accept that flash.
- Toggle light/dark through `LayoutService`; the shell header theme button calls `layoutService.toggleTheme()`.
- **Reduced motion** (`NFR-A11Y-001`): `LayoutService` listens for `prefers-reduced-motion: reduce`, toggles `app-reduced-motion` on `<html>`, and global styles in `tailwind.css` shorten non-essential transitions and hide PrimeNG ripples. Do not suppress compliance toasts or required policy dialogs.

### When adding a new screen

- Look at `features/auth` for form patterns and `features/issues` for tables, filters, and detail layouts.
- Issues routes are behind `authGuard`. Do not gate issues UI with `isAuthenticated`; keep auth checks in guards, `app.component` navigation, and `NotificationsRealtimeConnectionService` (push notifications only).
- Match existing Tailwind layout patterns (`flex flex-col gap-1.5 mb-4` for labeled fields, `flex flex-wrap items-center gap-3 mt-4` for action rows, `grid gap-4 sm:grid-cols-2 xl:grid-cols-3` for filter grids) before introducing new one-off utilities.

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
