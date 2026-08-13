---
id: NFR-RSP-001
title: Responsiveness and Layout
type: quality
status: active
---

## Responsiveness and layout

### Target viewports

- Primary design target: **desktop** and **large tablet** (viewport width **≥ 1024** px).
- Layout uses the existing shell: sidebar, top bar, scrollable content (`docs/modules/frontend/development.md`).

### Smaller viewports

- Screens **reflow** without horizontal clipping of primary actions at widths down to **768** px.
- Tables may scroll horizontally inside their container when columns do not fit.
- Sidebar collapses to the shell drawer pattern already used in the application.

### Out of scope (current product version)

Unless a REQ explicitly targets mobile:

- Dedicated **mobile-native** layouts are **not required**.
- **Touch-optimized** calendar gestures, swipe actions, and offline mode are **not required**.
- Complex grids (team availability month view, wide report tables) may require horizontal scroll on narrow viewports rather than column hiding.

### REQ overrides

- Calendar, kanban, or density-toggle screens must state viewport behavior when it differs from the defaults above (for example sticky user column in team availability).
