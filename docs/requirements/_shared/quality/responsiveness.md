---
id: NFR-RSP-001
title: Responsiveness and Layout
type: quality
status: active
---

## Responsiveness and layout

### Target viewports

- Primary design target: **desktop** and **large tablet** (viewport width **≥ 1024** px — Tailwind `lg` breakpoint).
- Layout uses the existing shell: sidebar (`ui-nav` / drawer), top bar, scrollable content (`docs/guides/frontend-guidelines.md`).

### Breakpoints (implementation)

The frontend uses Tailwind CSS default breakpoints aligned with Fluent 2 responsive guidance:

| Breakpoint | Min width | Typical use                                        |
| ---------- | --------- | -------------------------------------------------- |
| `sm`       | 640 px    | Two-column form grids; show compact labels         |
| `md`       | 768 px    | Shell padding; sidebar drawer; reflow without clip |
| `lg`       | 1024 px   | Primary desktop target; three-column form grids    |
| `xl`       | 1280 px   | Wide permission card grids; dense detail layouts   |

Prefer responsive Tailwind utilities (`sm:`, `md:`, `lg:`) in templates over custom media queries in CSS files.

### Smaller viewports

- Screens **reflow** without horizontal clipping of primary actions at widths down to **768** px (`md`).
- Tables may scroll horizontally inside their container when columns do not fit.
- Sidebar collapses to the shell drawer pattern already used in the application.

### Content width

- Authenticated workspace content uses available shell width.
- Readable page content may cap at **96rem** (~1536px) on narrow informational screens; list, detail, and form screens use full workspace width (`max-w-none`).

### Out of scope (current product version)

Unless a REQ explicitly targets mobile:

- Dedicated **mobile-native** layouts are **not required**.
- **Touch-optimized** calendar gestures, swipe actions, and offline mode are **not required**.
- Complex grids (team availability month view, wide report tables) may require horizontal scroll on narrow viewports rather than column hiding.

### REQ overrides

- Calendar, kanban, or density-toggle screens must state viewport behavior when it differs from the defaults above (for example sticky user column in team availability).
