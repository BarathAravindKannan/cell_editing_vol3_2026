---
name: fixed-column-skill
description: Expert knowledge for the Fixed Column feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to `GridColumn.FixedColumn`, fixed-first column ordering, DOM rendering flow, stacked-header partitioning, reorder/group prevention, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Fixed Column in the Syncfusion Blazor DataGrid. Covers render-tree flow, DOM output, JS/C# coordination, blocked interactions, and safe behavior across stacked headers, grouping, reordering, filtering, sorting, export, and virtualization.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Supported modes: `feature-implementation` | `bug-fix` | `rendering-analysis`
- Load this skill ONLY for work scoped to `GridColumn.FixedColumn`, fixed-first ordering, fixed-column DOM analysis, stacked-header fixed children, reorder/group prevention, or fixed-column regression debugging.
- Do NOT use this skill for frozen pane rendering, `FreezeDirection.Fixed`, freeze-line moving, or frozen row bugs unless the issue explicitly overlaps `FixedColumn` compatibility.
- Always finish with regression verification using `docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## Knowledge References
<!-- token-budget: 60 words -->

All content derived from reading these files — do NOT reproduce their full content elsewhere:

- `docs/ai-agents/skills/_skill-template-standard.md`
- `.github/skills/frozen-column-skill/SKILL.md`
- `.github/skills/searching-skill/SKILL.md`
- `.github/skills/virtualization-dom-structure-skill/SKILL.md`
- `src/GridColumn.cs`
- `src/SfGrid.razor.cs`
- `src/Internal/SfGrid.razor`
- `src/Internal/Base/InternalClass.cs`
- `src/Internal/Actions/Group.cs`
- `src/Internal/Renderer/GridHeader.razor`
- `src/Internal/Renderer/GridHeaderBase.cs`
- `src/Internal/Renderer/GridHeaderCell.razor`
- `src/Internal/Renderer/GridContent.razor`
- `src/Internal/Renderer/Filter/FilterBarRenderer.razor`
- `src/Internal/Renderer/ColumnChooser.razor`
- `src/Internal/Renderer/ColumnMenu.razor`
- `src/Internal/Generators/RowModelGenerator.cs`
- `tests/Actions/Columns.razor`
- `scripts/interfaces.ts`
- `scripts/sf-grid-fn.ts`
- `scripts/reorder.ts`
- `scripts/header-drag-drop.ts`
- `src/GridEvents.cs`
- `src/EventModels/Grids.cs`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **`FixedColumn` is order-based, not pane-based**: unlike frozen columns, it does not create left/right table panes; it pushes matching columns to the front of the render order.
- **Public source of truth is `GridColumn.FixedColumn`**: the feature starts as a column parameter, is serialized as `fixedColumn`, then is re-applied in both C# and JS ordering paths.
- **Fixed columns preserve declaration order among themselves**: flat columns are split into `[fixed..., normal...]`; stacked headers are partitioned recursively into fixed and unfixed subtrees.
- **Reordering is blocked, not post-corrected**: drag helper creation, drop validation, destination validation, and programmatic reorder paths all return early for fixed columns.
- **Grouping is intentionally blocked at action entry points**: `Ctrl+Space` keyboard grouping and `Grouping.GroupColumn(...)` both refuse fixed columns; the column stays visible and anchored.
- **DOM identity is subtle**: there is no dedicated `e-fixed-col` CSS class; the main visible header difference is omission of `e-mousepointer` on fixed headers.
- **Do not confuse `FixedColumn` with `FreezeDirection.Fixed`**: those are separate features. `FreezeDirection.Fixed` belongs to frozen-column pane logic, not fixed-first ordering.
- **Header/content must consume the same reordered column list**: fixed-column correctness depends on `RearrangeColumns(...)` being used consistently by header, rows, filter bar, group rows, summary rows, chooser/menu surfaces, and export.
- **JS still needs `isFixedColumnPresent`**: header drag logic uses this flag to resolve the correct header target before deciding whether drag is allowed.
- **Stacked-header handling is regression-sensitive**: recursive fixed-child extraction happens in both `SfGrid.razor.cs` and `reorder.ts`; either side drifting breaks DOM order and drag behavior.
- **No dedicated fixed-column event exists**: behavior is expressed by suppressing reorder/group operations before existing event callbacks fire.
- **Known limitation to keep visible**: some auxiliary surfaces still gate reorder normalization on frozen-column checks, so pure fixed-column scenarios must be validated carefully.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `src/GridColumn.cs` | Declares `FixedColumn` (line 588), tracks runtime changes, and bubbles `PropertyChanges["FixedColumn"]` to the parent grid |
| `src/SfGrid.razor.cs` | `IsFixedColumnPresent()` (line 1219), `RearrangeLockedColumns(...)` (line 1236), `RearrangeColumns(...)` (line 1292), stacked fixed-child partitioning, JS option serialization (line 1851) |
| `src/Internal/SfGrid.razor` | Root render tree choosing `GridHeader`/`GridContent` or virtual variants; fixed column rides these standard paths |
| `src/Internal/Base/InternalClass.cs` | JS payload property `isFixedColumnPresent` (line 184) |
| `src/Internal/Renderer/GridHeader.razor` | Always routes header columns through `RearrangeColumns(...)` (line 190); includes `ShouldRender` guard for frozen or fixed columns |
| `src/Internal/Renderer/GridHeaderCell.razor` | Omits `e-mousepointer` on fixed headers (lines 392, 404); stacked-header header-cell class resolution |
| `src/Internal/Renderer/GridHeaderBase.cs` | Keyboard grouping guard: `Ctrl+Space` refuses fixed columns |
| `src/Internal/Renderer/GridContent.razor` | Standard body table rendering; rows consume reordered cells through row models |
| `src/Internal/Renderer/Filter/FilterBarRenderer.razor` | Filter-bar cell order follows `RearrangeColumns(...)` (line 113) |
| `src/Internal/Renderer/ColumnChooser.razor` | Chooser list source; validate pure fixed-column scenarios because this path is gated by frozen-count checks |
| `src/Internal/Renderer/ColumnMenu.razor` | Column-menu surface that must reflect effective column order |
| `src/Internal/Actions/Group.cs` | Programmatic grouping refuses `Column.FixedColumn` (line 78) |
| `src/Internal/Generators/RowModelGenerator.cs` | Reorders row-model columns when fixed columns exist (line 166 uses `RearrangeColumns(...)`) |
| `scripts/interfaces.ts` | Declares `GridOptions.isFixedColumnPresent` (line 87) and `Column.fixedColumn` (line 640) |
| `scripts/sf-grid-fn.ts` | Flattens column model, then applies `updateFixedcolumns()` (line 1985); recursive `checkFixedColumns(...)` helper (line 1999) |
| `scripts/reorder.ts` | Primary drag/drop/order guards for fixed columns (20+ checks), including stacked-header fixed-child processing (line 631 `setStackedFixedColumns`) |
| `scripts/header-drag-drop.ts` | Cancels drag-clone creation for fixed columns or stacked parents containing fixed children (line 207) |
| `tests/Actions/Columns.razor` | Baseline fixed-column rendering tests for flat and stacked-header scenarios |

### Rendering Surface Map

| Surface | Owner | Fixed-column responsibility |
|--------|-------|-----------------------------|
| Public API/config | `GridColumn.FixedColumn` | Declares which columns are locked to the front |
| Grid render root | `src/Internal/SfGrid.razor` | Keeps fixed columns on the normal header/content render path |
| Header ordering | `GridHeader.razor` + `SfGrid.razor.cs` | Reorders visible columns before header-cell creation |
| Body row ordering | `RowModelGenerator.cs` | Produces cells in fixed-first order for each row |
| Filter bar | `FilterBarRenderer.razor` | Mirrors header ordering for filter inputs |
| JS column model | `sf-grid-fn.ts` | Keeps client-side flattened column model fixed-first |
| Drag / grouping protection | `reorder.ts`, `header-drag-drop.ts`, `GridHeaderBase.cs`, `Group.cs` | Prevents reorder and grouping on fixed columns |
| Export surfaces | `GridExcelExport.cs`, `GridPdfExport.cs` | Preserve fixed-first order in exported columns |

### Event Surface Map

| Event / trigger | Fixed-column behavior |
|-----------------|----------------------|
| `PropertyChanges["FixedColumn"]` | Raised internally when a column parameter changes at runtime |
| `GridEvents.Grouping` / `Grouped` | Not reached for blocked fixed-column grouping paths; action returns early |
| `GridEvents.ColumnReordered` | Not raised for blocked fixed-column drag/drop or reorder attempts |
| `Ctrl+Space` on header | Handled by `GridHeaderBase`; fixed column cancels grouping toggle |
| Drag clone creation | `header-drag-drop.ts` returns `false`; no drag visual or downstream reorder event |

---

## Rendering Approach
<!-- token-budget: 220 words -->

Fixed Column uses a **hybrid C# + JS ordering model**:

1. **C# decides semantic order**
   - `GridColumn.FixedColumn` is the public marker.
   - `SfGrid.IsFixedColumnPresent()` detects whether fixed behavior is active.
   - `SfGrid.RearrangeColumns(...)` and `RearrangeLockedColumns(...)` build the effective column order before header rows, filter bar cells, body rows, group rows, summary rows, chooser/menu lists, and export models are generated.

2. **JS decides interactive order**
   - `sf-grid-fn.ts` flattens the column tree into `columnModel`.
   - `updateFixedcolumns()` rewrites that client model as `[fixed..., normal...]`.
   - `reorder.ts` and `header-drag-drop.ts` use `fixedColumn` + `isFixedColumnPresent` to stop drag, drop, and programmatic reorder paths before they mutate order.

3. **DOM output stays single-table**
   - Fixed columns do **not** render separate left/right panes.
   - The same `<table>` is used; only the column order changes.
   - The main header-level visual cue is that fixed headers do not receive `e-mousepointer` when grouping/reordering would otherwise enable it.

### Fixed-column render flow

```text
<GridColumn FixedColumn="true">
          │
          ▼
GridColumn.FixedColumn parameter
          │
          ├─ serialized to JS as column.fixedColumn
          ├─ runtime change tracked via PropertyChanges["FixedColumn"]
          ▼
SfGrid.IsFixedColumnPresent()
          │
          ▼
SfGrid.RearrangeColumns(...)
          │
          ├─ Header → GridHeader.razor
          ├─ Filter bar → FilterBarRenderer.razor
          ├─ Rows → RowModelGenerator.cs
          ├─ Group/Summary rows → model generators
          ├─ Aux UI → ColumnMenu / ColumnChooser
          └─ Export → Excel / PDF
          │
          ▼
JS getColumns() / updateColumnModel()
          │
          ▼
updateFixedcolumns()
          │
          ▼
reorder.ts + header-drag-drop.ts block reorder/group drag
```

### Flat-column ordering algorithm

```text
Declared columns: [A, B*, C, D*]
(* = FixedColumn=true)

C# effective order: [B*, D*, A, C]
JS columnModel:     [B*, D*, A, C]
Rendered DOM:       header cells, filter cells, row cells, export columns all follow this order
```

### Stacked-header ordering algorithm

```text
Parent group: [Order Details[A, B], Ship Details[C*, D], Employee*]

Partition result:
- fixed subtree  : [Ship Details[C*], Employee*]
- normal subtree : [Order Details[A, B], Ship Details[D]]

Rendered top-level order:
[Ship Details(fixed child subtree), Employee, Order Details, Ship Details(unfixed subtree)]
```

---

## DOM Rendering Structure
<!-- token-budget: 400 words -->

### 1. Root render tree

```text
SfGrid<TValue>   (#GridId .e-grid)
├─ GridToolbar?                         [unrelated UI; fixed columns do not render here]
├─ div.e-gridheader
│  └─ div.e-headercontent
│     └─ GridHeader OR GridVirtualHeader
│        └─ table.e-table#<GridId>_header_table
│           ├─ WidthController
│           └─ thead
│              └─ tr.e-columnheader
│                 └─ GridHeaderCell × effective ordered columns
├─ div.e-gridcontent / div.e-content
│  └─ GridContent OR GridVirtualContent
│     └─ table.e-table#<GridId>_content_table
│        ├─ WidthController
│        └─ tbody
│           └─ GridRow × data rows
│              └─ td.e-rowcell × effective ordered cells
├─ ColumnChooser?
├─ ColumnMenu?
└─ Pager?
```

### 2. Flat fixed-column header DOM

```text
Declared columns: [OrderID, CustomerID, OrderDate, Freight, ShipCountry*]

Rendered header DOM:
.e-gridheader
└─ .e-headercontent
   └─ table.e-table
      └─ thead
         └─ tr.e-columnheader
            ├─ th.e-headercell                 [ShipCountry*  ← moved to index 0]
            │  └─ div.e-headercelldiv
            │     └─ "ShipCountry"
            ├─ th.e-headercell.e-mousepointer [Order ID]
            ├─ th.e-headercell.e-mousepointer [Customer Name]
            ├─ th.e-headercell.e-mousepointer [Order Date]
            └─ th.e-headercell.e-mousepointer [Freight]
```

**Important DOM cue**
- Fixed header cell: `th.e-headercell` with **no** `e-mousepointer` when the fixed guard applies.
- Normal reorderable/groupable header: `th.e-headercell.e-mousepointer`.
- There is **no** dedicated `.e-fixed-col` class.

### 3. Flat fixed-column body DOM

```text
tbody
└─ tr.e-row
   ├─ td.e-rowcell   [ShipCountry* value]
   ├─ td.e-rowcell   [OrderID value]
   ├─ td.e-rowcell   [CustomerID value]
   ├─ td.e-rowcell   [OrderDate value]
   └─ td.e-rowcell   [Freight value]
```

The body remains a normal row/cell table. Fixed columns are front-loaded by cell order only.

### 4. Stacked-header DOM split

```text
Declared:
Top row    : [Order Details] [Ship Details] [Customer ID] [Employee ID*]
Child row  : [OrderDate] [Freight] [ShipCity*] [ShipCountry]

Effective DOM concept:
Top row    : [Ship Details(fixed child subtree)] [Employee ID*] [Order Details] [Ship Details(unfixed subtree)] [Customer ID]
Child row  : [ShipCity*] [OrderDate] [Freight] [ShipCountry]
```

This is why stacked fixed columns can duplicate the same parent header text into separate fixed and unfixed subtrees.

### 5. Interaction-block DOM path

```text
Header mousedown / drag start
        │
        ▼
header-drag-drop.ts helper()
        │
        ├─ fixed column?                → return false
        ├─ stacked parent with fixed child? → return false
        └─ otherwise create .e-dragclone
```

### 6. Virtualization note

```text
FixedColumn + virtualization

Header path : GridVirtualHeader → still consumes effective ordered columns
Body path   : GridVirtualContent → still consumes effective ordered cells
DOM model   : single virtual table, not frozen panes
```

---

## JavaScript Interop
<!-- token-budget: 100 words -->

Fixed Column uses a **serialization-based coordination pattern** between C# and JavaScript, not explicit InvokeMethod calls.

### C# → JS Coordination

| C# Component | JS Target | Mechanism |
|--------------|-----------|-----------|
| `SfGrid.razor.cs` line 1851 | `GridOptions.isFixedColumnPresent` | Serialized to JS options during grid initialization |
| `GridColumn.FixedColumn` | `Column.fixedColumn` | Serialized via JSON column model to TypeScript |

### TypeScript Functions

| Function | Location | Responsibility |
|----------|----------|----------------|
| `updateFixedcolumns()` | `sf-grid-fn.ts:1985` | Reorders `columnModel` array to `[fixed..., normal...]` order |
| `checkFixedColumns(columns)` | `sf-grid-fn.ts:1999` | Recursively detects if any column (including nested stacked children) has `fixedColumn=true` |

### Responsibility Split

- **C#**: Decides semantic order via `RearrangeColumns(...)`, renders DOM, serializes state to JS
- **JavaScript**: Maintains client-side column model order, blocks drag/drop interactions, validates drop targets
- **No direct interop calls**: State flows one-way (C# → JS) via serialization; JS never calls back to C# for fixed-column operations

### CRITICAL Rules

1. **Never invoke JS methods for fixed-column ordering** — serialization handles sync automatically
2. **JS `columnModel` must match C# effective order** — both apply fixed-first logic independently
3. **Drag/drop blocks happen in JS** — C# never receives reorder events for fixed columns
4. **Stacked-header partitioning must stay synchronized** — both C# (`RearrangeLockedColumns`) and JS (`setStackedFixedColumns`) implement recursive fixed-child extraction

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live code paths and cross-surface rendering reads.
> Omit pairs with no meaningful risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Fixed Column + Frozen Column | Never combine `FixedColumn` with `IsFrozen` / `FreezeDirection.*`; frozen panes and fixed-first ordering are different engines | Critical |
| Fixed Column + Reordering | Fixed headers must not drag, accept drops, or fire reorder completion callbacks | Critical |
| Fixed Column + Grouping | Fixed columns must not group through keyboard or group action entry points; grouped layouts must still honor fixed-first order for remaining columns | Critical |
| Fixed Column + Stacked Headers | Recursive partitioning must preserve fixed-child-first DOM order without losing unfixed siblings | Critical |
| Fixed Column + Column Virtualization | Virtual header/content windows must still use effective fixed-first order; never create frozen-pane assumptions | High |
| Fixed Column + Filter Bar | Filter input cells must match header/body order exactly | High |
| Fixed Column + Column Menu | Menu actions must target the correct effective column after fixed-first reordering | High |
| Fixed Column + Column Chooser | Chooser ordering/visibility logic must be validated because some paths are still frozen-count gated | High |
| Fixed Column + Sorting | Sorting must remain allowed; only reorder/group affordances are blocked | Medium |
| Fixed Column + Editing | Editors and command flow must operate on the reordered visible cell index without assuming declaration order | Medium |
| Fixed Column + Selection | Focus/selection movement must use rendered order, not original declaration order | Medium |
| Fixed Column + Export | Excel/PDF output must preserve fixed-first effective order | Medium |
| Fixed Column + Paging | Page changes must not reset fixed-column ordering | Medium |
| Fixed Column + Foreign Key / Templates | Complex cells must render normally after fixed-first relocation in the DOM | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

Mode: `feature-implementation` | `bug-fix` | `rendering-analysis`

Skill: Fixed Column

Use this skill when the task involves `GridColumn.FixedColumn`, fixed-first ordering, fixed-column DOM structure, stacked-header fixed children, reorder/group blocking, or any bug where rendered column order differs from declared order.

Mandatory reasoning path:
1. Read `src/GridColumn.cs`, `src/SfGrid.razor.cs`, `src/Internal/SfGrid.razor`, `src/Internal/Renderer/GridHeader.razor`, `src/Internal/Renderer/GridHeaderCell.razor`, `src/Internal/Renderer/GridContent.razor`, `scripts/sf-grid-fn.ts`, `scripts/reorder.ts`, and `scripts/header-drag-drop.ts`.
2. Treat Fixed Column as a **single-table ordering feature**, not a frozen-pane feature.
3. Verify both C# and JS order pipelines stay aligned: `RearrangeColumns(...)` and `updateFixedcolumns()`.
4. If stacked headers exist, validate recursive fixed-child partitioning on both C# and JS sides.
5. Preserve the DOM contract:
   - fixed headers render first
   - fixed headers do not get `e-mousepointer` in blocked scenarios
   - no new frozen-pane markup is introduced
6. Check blocked interactions: drag helper creation, drop validation, programmatic reorder, `Ctrl+Space`, and `Grouping.GroupColumn(...)`.
7. Validate auxiliary surfaces: filter bar, chooser/menu, row models, virtualization path, and export order.
8. Do not confuse `FixedColumn` with `FreezeDirection.Fixed`.
9. Finish with regression verification using `docs/ai-agents/prompts/regression-verification-prompt.md` and targeted checks from `tests/Actions/Columns.razor` fixed-column fixtures.
