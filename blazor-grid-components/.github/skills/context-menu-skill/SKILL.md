---
name: context-menu-skill
description: Expert knowledge for the Context Menu feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to context menu behaviour, including DOM structure, JS-to-Blazor event flow, default/custom item rendering, target-based item filtering, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for DataGrid Context Menu behaviour, DOM item structure, render flow, event contracts, and cross-feature safety.

---

**Agent Invocation**  
Load this skill when implementing or debugging grid context menu rendering, right-click target detection, custom/default item composition, open/click/close events, or regressions involving selection, editing, sorting, grouping, paging, export, virtualization, or frozen headers.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `src/Internal/Renderer/ContextMenu.razor` — main renderer, default item creation, target filtering, open/close/click handlers
- `src/Internal/SfGrid.razor` — host render location for `<ContextMenu>`
- `scripts/sf-grid-fn.ts` — `mousedown`/`contextmenu` wiring and target resolution
- `src/Internal/Base/GridJSInteropAdaptor.cs` — JS interop bridge to `GridMouseDown`
- `src/EventModels/Grids.cs` — `ContextMenuOpenEventArgs<T>`, `ContextMenuClickEventArgs<T>`, `ContextMenuItemModel`
- `src/GridEvents.cs` — public `ContextMenuOpen` and `ContextMenuItemClicked` callbacks
- `src/SfGrid.Properties.cs` — public `ContextMenuItems` API
- `src/Enumeration/GridsEnumerations.cs` — `ContextMenuTarget`
- `scripts/sf-grid.ts` — popup coexistence check with `.e-sfcontextmenu` nodes

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- Derived from live implementation: the JS layer owns right-click detection; Blazor owns item computation and action execution.
- Context menu exists only when both `ContextMenuItems` and `CurrentViewData` are non-null.
- Four effective targets drive menu state: `Header`, `Content`, `Pager`, `EditForm`; any other target cancels opening.
- Content right-click can update grid selection before the menu opens.
- Item hidden/disabled state is transient and must be reset on close.
- Default item text is localization-driven, so visibility rules should compare resolved localized text carefully.

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`src/Internal/SfGrid.razor`** (line 159) — mounts `<ContextMenu TContent="@TValue">` after pager/column UI and before dialog edit rendering.
- **`src/Internal/Renderer/ContextMenu.razor`** — renders `SfContextMenu`, builds default/custom items, partitions zones (`HeaderItems`, `ContenItems`, `PagerItems`, `EditItems`), raises grid events, and executes default commands.
  - Line 109: `MouseDownHandler(object args)` - internal EventAggregator handler
  - Line 183: `ContextMenuBeforeOpenEvent()` - opens menu, applies rules
  - Line 604: `ContextMenuItemClick()` - handles item clicks
  - Line 747: `EventAggregator.Add("MouseDown", ...)` - subscribes to mouse events
- **`scripts/sf-grid-fn.ts`** — wires DOM events, recognizes right-click / long press / macOS Ctrl+Click, resolves target zone, and forwards row/header identifiers.
  - Line 1006: `EventHandler.add(element, 'mousedown', ...)` - event wiring
  - Line 1018: `EventHandler.add(element, 'contextmenu', ...)` - event wiring
  - Line 1838: `mouseDownHandler()` - main right-click detection
- **`src/Internal/Base/GridJSInteropAdaptor.cs`** (line 556) — converts DOM identifiers into row/cell/column models and triggers EventAggregator `MouseDown`.
- **`src/EventModels/Grids.cs`** — event payloads and custom item model (`Target`, row info, item list, submenu parent, target enum).
  - Line 1824: `ContextMenuClickEventArgs<T>`
  - Line 1875: `ContextMenuItemModel`
  - Line 1887: `ContextMenuOpenEventArgs<T>`
- **`src/GridEvents.cs`** — public event surface for `ContextMenuOpen` and `ContextMenuItemClicked`.
  - Line 1440: `ContextMenuItemClicked` callback
  - Line 1468: `ContextMenuOpen` callback
- **`src/SfGrid.Properties.cs`** (line 510) — public `ContextMenuItems` definition and XML docs for default items.
- **`src/Enumeration/GridsEnumerations.cs`** (line 1998) — `ContextMenuTarget` values: `None`, `Header`, `Content`, `Pager`, `EditForm`.
- **`scripts/sf-grid.ts`** (line 65) — shared popup inspection using `.e-sfcontextmenu`; relevant when changing popup coexistence.

---

## DOM Rendering Structure
<!-- token-budget: 80 words -->

**Context Menu DOM Hierarchy**
```
SfContextMenu (renders to .e-sfcontextmenu.e-grid-menu)
└── <ul class="e-ul">
    ├── <li class="e-menu-item"> (AutoFit, SortAscending, etc.)
    ├── <li class="e-separator"> (optional dividers)
    └── <li class="e-menu-item e-menu-caret-icon"> (parent with submenu)
        └── <ul class="e-ul"> (submenu items)
```

**Key Classes**
- `.e-sfcontextmenu` - Base context menu element
- `.e-grid-menu` - Indicates grid-specific menu styling
- `.e-menu-item` - Individual menu item
- `.e-separator` - Horizontal divider
- `.e-hide-menu` - Hidden column menu state
- `.e-grid-column-menu` - Column menu variant

**Target Zone Detection**
- Header: `parentsUntil(target, 'e-headercell')` or `.e-headercontent`
- Content: `parentsUntil(target, 'e-row')` or `.e-content`
- Pager: `parentsUntil(target, 'e-pager')`
- EditForm: `parentsUntil(target, 'e-editedrow')` or `.e-editform`

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live context-menu implementation and adjacent integration points.  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Context Menu + Selection | Content right-click updates selection before open; both-mode selection uses row/cell path; multi-selected active row is not unnecessarily cleared | Critical |
| Context Menu + Editing | `Edit`, `Delete`, `Save`/`Update`, `Cancel` must respect edit mode and permissions; batch mode edits cell, normal mode starts/ends row edit, edit-form target shows edit-only items | Critical |
| Context Menu + Sorting | Header target exposes `SortAscending`/`SortDescending`; disable when column has no `Field`, sorting is disallowed, or requested direction already exists | High |
| Context Menu + Grouping | `Group`/`Ungroup` only valid for header target; preserve grouped-column detection and `AllowGrouping` guard | High |
| Context Menu + Paging | Pager target exposes only page navigation items; boundary pages and empty data disable navigation actions | High |
| Context Menu + Export | Content menu may collapse export items into `Export` parent; preserve `PdfExport`, `ExcelExport`, `CsvExport` enable rules | High |
| Context Menu + Frozen Columns | Header target lookup must search normal, frozen, and frozen-right header rows when resolving the clicked column | High |
| Context Menu + Virtualization | Content target resolution depends on current rendered row uid and `aria-colindex`; do not assume stable DOM row positions | High |
| Context Menu + Filtering/Popup UI | Grid popup coexistence must not break filter popup, filter menu, or other `.e-sfcontextmenu` consumers | Medium |
| Context Menu + Detail / Group UI | Non-target regions such as detail cells, expand/collapse icons, summary cells, group captions, and resize handlers should not produce stale target state | Medium |
| Context Menu + Custom Items | `ContextMenuItemModel.Target`, `Hide`, `Separator`, submenu items, and custom ids must continue to work beside default items | Critical |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

## JavaScript Interop
<!-- token-budget: 100 words -->

**JS → C# Call Flow**
1. Right-click/touch/ctrl+click detected in `scripts/sf-grid-fn.ts` line 1838 (`mouseDownHandler()`)
2. Calls `GridJSInteropAdaptor.MouseDownHandler()` (C#) via `dotNetRef.invokeMethodAsync()`
3. Resolves row/cell/column from DOM identifiers (line 556)
4. Triggers EventAggregator `"MouseDown"` (line 581)
5. `ContextMenu.razor` receives via `EventAggregator.Add("MouseDown", MouseDownHandler)` (line 747)
6. `MouseDownHandler(object args)` processes target and stores state (line 109)

**Responsibility Split**
| Layer | Responsibility |
|-------|---------------|
| `sf-grid-fn.ts` | Right-click detection, target zone resolution, DOM id extraction |
| `GridJSInteropAdaptor.cs` | DOM id to row/cell/column model conversion, event triggering |
| `ContextMenu.razor` | Menu item computation, open/close, feature rule enforcement |

**Critical Rules**
- Never call `MouseDownHandler()` directly; always go through JS interop
- DOM ids must be stable across renders (use `Uid`, not indices)
- EventAggregator `"MouseDown"` is internal; don't trigger externally
- `.e-sfcontextmenu` class identifies grid context menus vs other popups

---

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Context Menu  

Work on the Syncfusion Blazor DataGrid context menu. Use this skill when changing DOM structure, item generation, right-click handling, or context-menu events.

**Core flow**
1. Grid host renders `<ContextMenu>` only when `ContextMenuItems` and `CurrentViewData` exist.
2. JS in `scripts/sf-grid-fn.ts` listens to `mousedown` and `contextmenu`, detects right-click / touch long press / macOS Ctrl+Click, resolves target (`Header`, `Content`, `Pager`, `Edit`), then calls `MouseDownHandler` on .NET.
3. `GridJSInteropAdaptor.MouseDownHandler()` resolves row/cell/column from DOM ids and triggers EventAggregator `MouseDown`.
4. `ContextMenu.razor` stores target state, builds localized default/custom `MenuItem` objects, opens `SfContextMenu`, then filters/disable items by target and feature state.
5. On open, it may update selection for content cells, raises public `ContextMenuOpen`, then applies hide/disable rules.
6. On click, default items dispatch to grid APIs (`AutoFitColumnsAsync`, `GroupColumnAsync`, `EditCellAsync`, `StartEdit`, `DeleteRecordAsync`, `CopyAsync`, export, paging, sort), then raises `ContextMenuItemClicked`.
7. On close, hidden/disabled state is reset.

**Must preserve**
- DOM classes: `.e-sfcontextmenu`, `.e-grid-menu`
- Targets: `Header`, `Content`, `Pager`, `EditForm`
- Events: internal open/close/item-selected pipeline plus public `ContextMenuOpen` and `ContextMenuItemClicked`
- Custom item support: `ContextMenuItemModel.Target`, `Hide`, `Separator`, submenu items
- Cross-feature safety with selection, editing, sorting, grouping, paging, export, virtualization, and frozen headers.

Before changes, inspect the files in the Code Location Map and verify no target-state regression is introduced.
