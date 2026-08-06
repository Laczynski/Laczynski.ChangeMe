---
id: NFR-A11Y-001
title: Accessibility
type: quality
status: active
---

## Accessibility

Unless a REQ states otherwise, all screens must meet these minimum expectations:

### Keyboard and focus

- Every interactive control is reachable and operable with **keyboard only** (Tab, Shift+Tab, Enter, Space, Escape).
- **Escape** closes dropdown panels, dialogs, and overlays without navigating away (for example notification bell — FR-ISS-005).
- Focus moves into opened dialogs and returns to the triggering control on close.
- No keyboard trap outside intentional modal flows.

### Labels and semantics

- Form fields have visible **labels** matching the functional specification field names.
- Icon-only buttons have an accessible name via **aria-label** or visible tooltip text defined in the functional specification.
- Data tables use column headers that match REQ column names.
- Status badges convey status text, not color alone.

### Visual and motion

- The application supports **light** and **dark** themes. Contrast follows **Fluent 2** semantic tokens exposed by **@laczynski/ui** (`--color-neutral-foreground-*`, `--color-neutral-background-*`, `--color-brand-*`) — specification authors do not specify hex colors.
- Interactive states (hover, pressed, selected, disabled) use Laczynski component defaults so focus rings and disabled styling stay consistent across the product.
- Respect **prefers-reduced-motion** for non-essential animations when the UI kit provides a hook (`app-reduced-motion` on `<html>`); compliance-gate sticky toasts and required policy dialogs are never suppressed.

### specification author checklist

- When a REQ introduces an icon-only control, specify tooltip or aria-label text.
- When a REQ introduces a custom keyboard shortcut, document the key combination explicitly.
- If a screen is intentionally below full accessibility standard, state the gap and reason under **Out of scope** in the functional specification.
