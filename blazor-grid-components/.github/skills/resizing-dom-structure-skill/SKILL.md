---
name: resizing-dom-structure-skill
description: Expert knowledge for the Column Resizing and AutoFit rendered DOM structure in the Syncfusion Blazor DataGrid. Use this skill when implementing or debugging resize handles, helper lines, header-cell rendering, stacked headers, frozen/virtualized resize behavior, and width propagation across header, content, footer, and edit tables.
---

# Resizing DOM Structure Skill
<!-- token-budget: 20 words -->

**Purpose**  
Authoritative reference for Column Resizing and AutoFit DOM rendering in `SfGrid<TValue>`. Covers resize-handle markup, helper-line rendering, width propagation through `<colgroup>`, and all critical combinations with frozen columns, stacked headers, column virtualization, grouping, filtering, editing, paging, and export.

---

**Agent Invocation**  
Load this skill when implementing resize features, debugging resize-handle rendering, analyzing AutoFit measurement flow, or fixing regressions where column width changes break frozen panes, stacked headers, virtual columns, filter rows, inline edit tables, or persisted widths.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md`
- `/docs/training/01-getting-started/architecture-overview.md`
- `/docs/training/03-llm-best-practices/working-with-llms.md`
- `/docs/training/04-code-processing/optimal-chunking-strategies.md`
- `/docs/training/05-practical-examples/feature-implementation-walkthrough.md`
- `/docs/training/06-reference/quick-reference-guides.md`
- `/docs/architecture/system-architecture.md`
- `/docs/architecture/component-architecture.md`
- `/docs/code-guidelines/coding-standards.md`
- `/docs/tech-stack/tech-stack.md`
- `/docs/ai-agents/prompts/regression-verification-prompt.md`
- `/.github/skills/frozen-column-skill/SKILL.md`
- `/.github/skills/filtering-skill/SKILL.md`
- `/.github/skills/virtualization-dom-structure-skill/SKILL.md`
- `/.github/agents/skill_generator.md`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Resize is a hybrid concern** — Razor renders the handle DOM, but all pointer tracking, helper-line movement, and content measurement stay in TypeScript.
- **Never bypass `GridJSInteropAdaptor<T>`** — resize start/stop, width change, and cancellation flow through JS-invokable callbacks.
- **Do not mutate DOM widths from C#** — C# updates column state; JS applies actual `<col>` and `<table>` widths.
- **`PropertyChanges` is the source of incremental refresh** — width changes bubble as `ColumnWidth`, then grid refresh logic decides whether header/content/client refresh is needed.
- **Preserve cross-feature behavior** — quick-reference docs explicitly mark Resize + Frozen Columns as a regression-risk pair.
- **Use render-path awareness** — standard content, virtual content, frozen panes, stacked headers, and filter bar all render different width consumers.
- **No direct `JSRuntime.InvokeAsync` outside the bridge** — resize changes must remain in the established JS/C# contract.
- **Regression verification is mandatory** — always apply `/docs/ai-agents/prompts/regression-verification-prompt.md` after resize-related changes.

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**TypeScript → .NET Communication**

| TypeScript Source | .NET JSInvokable | Location | Purpose |
|-------------------|------------------|----------|---------|
| `resize.ts` line 437 | `ResizeStarted(object args)` | `GridJSInteropAdaptor.cs` line 219 | Fires `OnResizeStart` event; user can cancel via `preventResizeAction()` |
| `resize.ts` line 911 | `ColumnWidthChanged(object args, bool)` | `GridJSInteropAdaptor.cs` line 144 | Updates column state; fires `ResizeStopped` event; persists widths; triggers refresh |

**C# → TypeScript Invocations**

| C# Source | TypeScript Target | Location | Purpose |
|-----------|-------------------|----------|---------|
| `GridJSInteropAdaptor.cs` line 215 | `preventResizeAction(dataId, isCancel)` | `sf-grid.ts` line 113 | Cancels active resize gesture when `OnResizeStart` returns `Cancel = true` |
| `SfGrid.Methods.cs` line 121-188 | `autoFitColumns(dataId, columns, fieldNames, ...)` | `sf-grid.ts` line 317 | Triggers AutoFit measurement and width application |
| Grid refresh pipeline | `updateResizeCursor(dataId)` | `sf-grid.ts` line 1116 | Updates frozen resize cursor classes after width changes |

**Responsibility Split**

| Layer | Owns |
|-------|------|
| **C# (Razor/Model)** | Column state (`Width`, `MinWidth`, `MaxWidth`), event dispatch (`OnResizeStart`, `ResizeStopped`), persistence logic, refresh coordination |
| **TypeScript (resize.ts)** | Pointer tracking, helper line rendering, DOM measurement (`getBoundingClientRect`), `<col>` width application, stacked/frozen width propagation |

**CRITICAL: JavaScript Interop Rules**

1. **Never skip `ResizeStarted` callback** — user cancellation via `OnResizeStart` must invoke `preventResizeAction()` before live resize begins.
2. **`ColumnWidthChanged` is the single source of truth** — all width updates flow through this JSInvokable; direct DOM mutations outside this flow break frozen panes and persistence.
3. **Preserve bidirectional contract** — TypeScript owns measurement; C# owns state; neither bypasses the interop bridge.
4. **AutoFit must use current DOM** — `autoFitColumns()` clones and measures rendered cells; if cells aren't rendered (virtualization), measurement fails.

---

## Code Location Map
<!-- token-budget: 120 words -->

| File | Responsibility |
|------|----------------|
| `scripts/resize.ts` | Core resize engine: handle events, helper line, live width updates, AutoFit measurement, stacked-column propagation, double-click/double-tap AutoFit |
| `scripts/width-controller.ts` | Applies widths to header/content/footer/edit `<colgroup>` and table elements; frozen/movable width partitioning |
| `scripts/sf-grid-fn.ts` | Initializes `resizeModule`; triggers `render()`, `autoFit()`, persisted-width refresh, frozen ordering helpers |
| `scripts/sf-grid.ts` | Public JS API: `preventResizeAction`, `autoFitColumns`, `autoFit`, `updateResizeCursor`, frozen resize refresh hooks |
| `scripts/freeze.ts` | `updateResizeHandler()`, `setFrozenHeight()`, stacked-header/frozen row-height sync after width changes |
| `scripts/virtual-scroll.ts` | Column-virtualized AutoFit path; offset refresh after width updates |
| `scripts/interfaces.ts` | Grid/client options and `Column` width, resize, freeze, autofit contracts |
| `src/Internal/SfGrid.razor` | Chooses header/content renderer path: standard vs virtual; establishes DOM regions resize works against |
| `src/Internal/Renderer/GridHeader.razor` | Renders header `<table>`, `<thead>`, filter bar, frozen-row header tbody, and `WidthController` |
| `src/Internal/Renderer/GridHeaderCell.razor` | Renders `.e-rhandler`, `.e-rsuppress`, frozen-line cursors, stacked header cell divs |
| `src/Internal/Renderer/GridContent.razor` | Standard content table, masked rows, inline edit table consumers of resized widths |
| `src/Internal/Renderer/GridVirtualHeader.razor` | Virtual header wrapper with `.e-virtualtable` and `.e-virtualtrack` |
| `src/Internal/Renderer/GridVirtualContent.razor` | Virtual content wrapper; `translate(X,Y)` and virtual track width/height |
| `src/Internal/Renderer/GroupedHeader.razor` | Group-drop header region that coexists above resizable header tables |
| `src/Internal/Renderer/GridRow.CellRenderer.cs` | Frozen resize cursor rendering on content cells and summary cells |
| `src/Internal/Models/Cell.cs` | Internal flags like `EnableFrozenResizeCursor` |
| `src/SfGrid.Properties.cs` | Public `AllowResizing` and grid-level `AutoFit` parameters |
| `src/GridColumn.cs` | Column-level `AllowResizing`, `AutoFit`, `Width`, `MinWidth`, `MaxWidth`, persisted table widths |
| `src/SfGrid.Methods.cs` | Public API: `AutoFitColumnsAsync()` and `AutoFitColumnAsync()` |
| `src/SfGrid.Lifecycle.cs` | Persistence restore, `AllowResizing`/`AutoFit` property change tracking |
| `src/SfGrid.razor.cs` | Client option serialization, resize flags, view refresh triggers, persisted-width state |
| `src/Internal/Base/GridJSInteropAdaptor.cs` | JS→.NET callbacks: `ResizeStarted`, `ColumnWidthChanged`, `ResizeStopped` event dispatch |
| `src/GridEvents.cs` | Public `OnResizeStart` and `ResizeStopped` callbacks |
| `src/EventModels/Grids.cs` | `ResizeArgs` event model |
| `src/Internal/Base/InternalClass.cs` | Client option payload flags such as `isColumnResized` and `isColumnWidthChanged` |

---

## DOM Structure Map
<!-- token-budget: 500 words -->

### 1. Standard non-virtual, non-frozen resize path

```text
SfGrid<TValue>
├── .e-gridheader
│   └── .e-headercontent
│       └── GridHeader
│           └── <table .e-table #ID_header_table>
│               ├── <WidthController> → <colgroup><col>…</colgroup>
│               └── <thead>
│                   └── <tr .e-columnheader>
│                       └── GridHeaderCell × visible columns
│                           └── <th .e-headercell>
│                               └── <div .e-headercell-container>
│                                   ├── <div .e-headercelldiv | .e-stackedheadercelldiv e-mappinguid>
│                                   │   └── header text / sort / filter / menu icons
│                                   └── <div .e-rhandler .e-rcursor>
│                                       OR <div .e-rsuppress>
│
└── .e-gridcontent > .e-content
    └── GridContent
        └── <table .e-table #ID_content_table>
            ├── <WidthController> → matching <colgroup>
            └── <tbody>
                └── GridRow × N
```

**Key rule**: resize does **not** mutate `<th>` width directly. It updates column state, then `ColumnWidthService` writes width to matching `<col>` elements in header, content, footer, and inline-edit tables.

### 2. Resize gesture flow

```text
User presses .e-rhandler
    ↓
GridHeaderCell.razor sets Parent.IsResizing = true
    ↓
resize.ts::resizeStart()
    ├── resolves target column from e-mappinguid
    ├── refreshes stacked widths if needed
    ├── appends <div .e-rhelper>
    ├── computes minMove / minWidth / maxWidth bounds
    └── invokes .NET ResizeStarted(columnUid, optional columnList)
            ↓
GridJSInteropAdaptor.ResizeStarted()
    ├── fires GridEvents.OnResizeStart
    ├── user may cancel
    └── calls sfBlazor.Grid.preventResizeAction(cancel)
            ↓
JS either cancels or wires touchMove/pointerMove
            ↓
resize.ts::resizing()
    ├── computes new width
    ├── updates leaf columns for stacked headers
    ├── ColumnWidthService.setColumnWidth(..., module='resize')
    ├── moves .e-rhelper left position
    └── refreshes frozen positions if active
            ↓
resize.ts::resizeEnd()
    ├── removes .e-rhelper
    ├── calls freezeModule.setFrozenHeight() when frozen
    ├── sends ColumnWidthChanged(... allowStopEvent=true)
    └── adds .e-resized to header cell
```

### 3. AutoFit flow

```text
Trigger sources
├── GridColumn.AutoFit = true at initial render
├── Grid.AutoFit = true
├── AutoFitColumnsAsync(...) from C#
├── Double-click on .e-rhandler
└── iOS double-tap fallback

Measurement path
Resize.autoFitColumns()
    ↓
Resize.resizeColumn(field)
    ├── clone target header cell
    ├── clone matching content/footer cells
    ├── build hidden measurement table (.e-resizetable)
    ├── read getBoundingClientRect().width
    ├── clamp by MinWidth / MaxWidth
    ├── set column.width
    └── ColumnWidthService.setColumnWidth()
```

### 4. Frozen-column DOM topology

```text
.e-gridheader
├── .e-frozenheader            ← left frozen header table
├── .e-movableheader           ← movable header table
└── .e-frozen-right-header     ← right frozen header table (LeftRight mode)

.e-gridcontent
├── .e-frozencontent           ← left frozen content table
├── .e-movablecontent          ← movable content table
└── .e-frozen-right-content    ← right frozen content table
```

Resize impact in frozen mode:
- `.e-rhandler` still renders in header cells.
- `freeze.ts::updateResizeHandler()` equalizes handle height across pane headers.
- `width-controller.ts` partitions widths into `leftFrozenTableWidth`, `tableWidth`, and `rightFrozenTableWidth`.
- `resize.ts` repositions sticky left/right cells using `applyStickyLeftRightPosition()`.
- C# content cells may render `.e-frozen-resize-cursor` using `GridRow.CellRenderer.cs` and `Cell.EnableFrozenResizeCursor`.

### 5. Stacked-header resize model

```text
Stacked Header Tree
Parent Column
├── Child A (leaf)
├── Child B (leaf)
└── Child C
    ├── Grandchild C1 (leaf)
    └── Grandchild C2 (leaf)
```

Rendered behavior:
- Header markup uses `.e-stackedheadercelldiv` for stacked cells.
- Dragging a stacked parent does **not** store width on the parent only.
- `resize.ts::getSubColumns()` flattens visible resizable leaves.
- `calulateColumnsWidth()` distributes delta across leaf columns.
- `refreshStackedColumnWidth()` recomputes parent widths as sum of visible leaves.
- `GridHeader.razor` uses `RowSpan`/`ColSpan`; resize must preserve these spans.

### 6. Column virtualization path

```text
.e-gridheader > .e-headercontent
└── GridVirtualHeader
    ├── <div .e-virtualtable style="transform: translate(X,0)">
    │   └── inner GridHeader (virtualized slice)
    └── <div .e-virtualtrack style="width: all-columns-width">

.e-gridcontent > .e-content
└── GridVirtualContent
    ├── <div .e-virtualtable style="transform: translate(X,Y)">
    │   └── <table .e-table>
    └── <div .e-virtualtrack>
```

Resize rules under column virtualization:
- Header/content only render `VirtualizedColumns` plus frozen columns.
- Live resize must refresh virtual offsets and virtual table width.
- `width-controller.ts` updates `virtualizedColumns`, then `virtualContentModule.refreshOffsets()`.
- `scripts/virtual-scroll.ts` can call `resizeModule.autoFitColumns(column.field)` when a virtualized column requires AutoFit.

---

## Rendering Decision Flow (ASCII)
<!-- token-budget: 180 words -->

```text
SfGrid.razor chooses the resize DOM path:

HEADER
┌────────────────────────────────────────────────────────────┐
│ EnableColumnVirtualization = false                        │
│   → GridHeader                                            │
│     → GridHeaderCell renders .e-rhandler / .e-rsuppress   │
│                                                            │
│ EnableColumnVirtualization = true                         │
│   → GridVirtualHeader                                     │
│     → .e-virtualtable                                     │
│       → inner GridHeader                                  │
│         → GridHeaderCell renders resize handles           │
└────────────────────────────────────────────────────────────┘

CONTENT
┌────────────────────────────────────────────────────────────┐
│ No virtualization                                         │
│   → GridContent                                           │
│     → WidthController + content table                     │
│                                                            │
│ Row and/or Column virtualization                          │
│   → GridVirtualContent                                    │
│     → .e-virtualtable + .e-virtualtrack                   │
│     → WidthController uses VirtualizedColumns             │
└────────────────────────────────────────────────────────────┘

FROZEN
┌────────────────────────────────────────────────────────────┐
│ FrozenColumns / IsFrozen / Freeze != None                 │
│   → multiple pane tables                                  │
│   → freeze.ts synchronizes handle height and pane width   │
│   → content/header cells may render .e-frozen-resize-cursor│
└────────────────────────────────────────────────────────────┘
```

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 170 words -->

> Built from live codebase + training docs cross-reference. Omit pairs with no real resize risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| Resizing + AutoFit | Double-click and API-triggered AutoFit must measure rendered header/content/footer cells, then clamp by `MinWidth` / `MaxWidth` | Critical |
| Resizing + Frozen Columns | Left/movable/right tables stay width-synchronized; `setFrozenHeight()` and `updateResizeHandler()` must run after width changes | Critical |
| Resizing + Column Virtualization | Virtual offsets and `VirtualizedColumns` widths refresh correctly; `.e-virtualtrack` width stays aligned with full column width sum | Critical |
| Resizing + Stacked Headers | Parent stacked header width remains sum of visible leaf widths; `RowSpan` / `ColSpan` structure stays valid | Critical |
| Resizing + Reordering | Reordered columns retain correct resize handles, `e-mappinguid` mapping, and width ownership after drag-drop | High |
| Resizing + Filtering | FilterBar cells and filter icon headers stay width-aligned; frozen filter cells keep resize cursor state | High |
| Resizing + Grouping | Group indent/header layout and grouped hidden-column rules must not break AutoFit or handle placement | High |
| Resizing + Editing | Inline edit tables, add-new-row, and batch cells consume the same `<colgroup>` widths as the display table | High |
| Resizing + Selection | Checkbox/header selection cells and focused cell borders align to resized colgroups | Medium |
| Resizing + Paging | Page changes must not drop persisted or user-resized widths | Medium |
| Resizing + Infinite Scroll | Newly loaded blocks inherit resized widths without stale colgroup state | Medium |
| Resizing + Export | Export modules must use current resized column widths, including persisted frozen table widths | Medium |
| Resizing + Text Wrap | Wrapped rows/header heights recalc after width change; resize handler height must match new wrapped header height | High |
| Resizing + Aggregates/Footer | Footer summary cells stay width-aligned with header/content colgroups | Medium |
| Resizing + Persistence | `isColumnResized` / persisted AutoFit widths restore through JS on next initialization | High |

---

## Prompt Template
<!-- token-budget: 300 words -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Resizing DOM Structure  

You are working on **column resizing / autofit** in `SfGrid<TValue>`. Consult `/docs/training/` and `/docs/ai-agents/prompts/regression-verification-prompt.md` before changing code. Treat resizing as a **hybrid Razor + JS** feature: Razor renders the handles and colgroups; TypeScript owns pointer tracking, helper-line movement, DOM measurement, and table-width application.

**Core paths**
- Handle DOM: `src/Internal/Renderer/GridHeaderCell.razor`
- JS engine: `scripts/resize.ts`
- Width application: `scripts/width-controller.ts`
- JS bridge: `src/Internal/Base/GridJSInteropAdaptor.cs`
- Public API: `src/SfGrid.Methods.cs`, `src/GridColumn.cs`, `src/SfGrid.Properties.cs`
- Render-path selectors: `src/Internal/SfGrid.razor`, `GridHeader.razor`, `GridVirtualHeader.razor`, `GridContent.razor`, `GridVirtualContent.razor`

**Non-negotiable rules**
- Do not update DOM widths from C#; update column state and let JS `ColumnWidthService` apply widths.
- Keep `ResizeStarted` → cancel check → `preventResizeAction()` → live resize → `ColumnWidthChanged` flow intact.
- Preserve `.e-rhandler`, `.e-rsuppress`, `.e-rhelper`, `.e-frozen-resize-cursor`, `.e-virtualtable`, and `<colgroup>` contracts.
- If frozen columns are active, verify `setFrozenHeight()` and `updateResizeHandler()` still run.
- If stacked headers are active, resize leaf columns and recompute parent width.
- If column virtualization is active, refresh offsets and virtual table width.
- Keep footer, filter row, inline edit row, and persisted widths aligned.

**Regression checklist**
- Standard resize drag
- Double-click AutoFit
- Frozen left/right/fixed resize
- Stacked header resize
- Column virtualization resize/autofit
- FilterBar + resize alignment
- Inline edit row width sync
- Persistence restore after reload
- Export width consumption

---

## Quick Reference: DOM Landmarks
<!-- token-budget: 120 words -->

| Element / Class | Meaning |
|---|---|
| `.e-rhandler` | Active resize handle rendered in header cell |
| `.e-rsuppress` | Non-resizable placeholder handle to preserve geometry |
| `.e-rcursor` | Cursor class added on active resizable handles |
| `.e-rhelper` | Vertical helper line rendered during live resize |
| `.e-resized` | Marker added to resized header cells |
| `.e-headercelldiv` | Standard leaf header content wrapper carrying `e-mappinguid` |
| `.e-stackedheadercelldiv` | Stacked header content wrapper |
| `.e-frozen-resize-cursor` | Extra frozen-line cursor class on header/content/filter cells |
| `<WidthController>` | Emits `<colgroup>` widths that all tables consume |
| `.e-virtualtable` | Translated container for virtual header/content |
| `.e-virtualtrack` | Spacer defining total virtual width/height |
