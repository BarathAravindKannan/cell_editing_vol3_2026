---
name: frozen-rows-columns-dom-structure-skill
description: Expert knowledge for the rendered DOM structure and Blazor component rendering flow for Frozen Rows, Frozen Columns, stacked-header combinations, and their interaction with row cell ordering in the Syncfusion Blazor DataGrid. Use this skill when implementing or debugging pinned-row/header-body split, column rearrangement, stacked header projection, and frozen-zone render output.
---

# Frozen Rows and Frozen Columns DOM Structure Skill
<!-- token-budget: 24 words -->

**Purpose**  
Authoritative reference for the Blazor render tree and DOM layout produced by `FrozenRows`, frozen left/right/fixed columns, and stacked-header combinations. Covers `GridHeader`, `GridContent`, `RearrangeColumns()`, stacked-header projection, and where row-cell concatenation actually occurs.

---

## Knowledge References
<!-- token-budget: 80 words -->

- `src/Internal/SfGrid.razor` — root renderer; selects `GridHeader` and `GridContent` for the standard path
- `src/Internal/Renderer/GridHeader.razor` — header table, header rows, frozen-row tbody rendering; `OnParametersSet()` (line 188), `GetRows()` (line 287), `GetStackedColumns()` (line 396), `GenerateCells()` (line 424)
- `src/Internal/Renderer/GridContent.razor` — content table, body rows, frozen-row skip logic; `OnParametersSet()` (line 276)
- `src/SfGrid.razor.cs` — `RearrangeColumns()` (line 1292), stacked frozen/movable projection, freeze labels
- `src/Internal/Actions/Freeze.cs` — freeze state management, column count queries, frozen/movable column organization, `SetStackedFrozenAndMovableColumns()` (line 249), `SetFrozenMovableLabel()` (line 325)
- `src/SfGrid.Lifecycle.cs` — `FreezeModule` initialization (line 41)
- `src/Internal/Generators/RowModelGenerator.cs` — body row and base cell generation order; `GenerateRow()` (line 92), `GenerateCells()` (line 162)
- `src/Internal/Renderer/GridRow.razor` — per-row DOM emission
- `src/Internal/Renderer/GridRow.razor.cs` — `GetCells()` (line 298) and column-virtualization concat path
- `.github/skills/virtualization-dom-structure-skill/SKILL.md` — compare when virtualization replaces the standard render path

---

## Core Rendering Facts
<!-- token-budget: 160 words -->

- **Standard non-virtual path uses `GridHeader` + `GridContent`**. In `src/Internal/SfGrid.razor`, when `EnableVirtualization == false` and `EnableColumnVirtualization == false`, the grid renders a header `<table>` via `GridHeader` and a content `<table>` via `GridContent`.
- **Frozen rows are split across header/content sections**. `GridHeader` renders the pinned top rows inside its own `<tbody>`. `GridContent` removes those rows from the scrollable body using `Rows = Parent.Rows?.Skip((int)Parent.FrozenRows)?.ToList()`.
- **Frozen columns are not rendered as separate tables in the standard path**. The DOM stays as a single header table and a single content table; the column order is rearranged before row/header cell generation.
- **`RearrangeColumns()` is the architectural pivot**. It computes frozen-left, movable/fixed, and frozen-right order, and for stacked headers it also projects the parent-child column hierarchy into the same frozen-zone ordering.
- **Row-cell concat is not the primary standard-path mechanism**. In the normal non-column-virtual path, `RowModelGenerator.GenerateCells()` builds `row.Cells` in already-rearranged order. Explicit `FrozenLeftCells.Concat(Cells).Concat(FrozenRightCells)` occurs in `GridRow.GetCells()` for the column-virtualized retrieval path.

---

## Render Ownership Map
<!-- token-budget: 220 words -->

### `SfGrid.razor`
- Owns the top-level branch decision.
- Standard DOM path:
  - `.e-gridheader > .e-headercontent > GridHeader`
  - `.e-gridcontent > .e-content > GridContent`
- Adds `.e-frozenrow-border` when `FrozenRows > 0`.

### `GridHeader.razor`
- Calls `Columns = Parent.RearrangeColumns(HeaderParameters?.Columns!)` in `OnParametersSet()`.
- Builds header row models through `GetRows()`.
- For stacked headers, computes multiple header rows with `GetStackedColumns()` and `GenerateCells(levelColumns, false, maxDepth, level)`.
- Maintains `RowCollection` for frozen top rows.
- Renders frozen rows inside the header `<tbody>` using `GridRow`.

### `GridContent.razor`
- Generates or receives `Rows` for data content.
- When `Parent.FrozenRows > 0`, it triggers header refresh and then skips the frozen top rows from the body list.
- Uses `GridRow` to render the remaining scrollable rows.
- If frozen columns exist together with grouping and no stacked header, it may reassign `Parent.Columns = Parent.RearrangeColumns(Parent.Columns!)` to maintain matching content order.

### `RowModelGenerator<T>`
- Generates data rows with `GenerateRow()`.
- Uses `Parent.RearrangeColumns(GridUtils.GetColumns(Parent))` when frozen/fixed columns are present.
- Produces a single ordered `row.Cells` list for the standard path.

### `GridRow.razor.cs`
- Returns row cells to the renderer.
- In normal non-column-virtual rendering, returns `RowParameters.Row.Cells` directly.
- In column-virtual rendering, reconstructs visible cell output with:
  - frozen-left cells
  - movable/fixed visible window
  - frozen-right cells

---

## Architecture Flow — Standard Frozen Rows + Frozen Columns
<!-- token-budget: 500 words -->

```text
User config
  ├─ FrozenRows > 0 ?
  ├─ FrozenColumns > 0 ?
  ├─ Any column.IsFrozen Left/Right/Fixed ?
  └─ Stacked header columns present ?
          │
          ▼
SfGrid.razor
  ├─ renders .e-gridheader
  │    └─ GridHeader
  ├─ if FrozenRows > 0
  │    └─ renders .e-frozenrow-border
  └─ renders .e-gridcontent > .e-content
       └─ GridContent

GridHeader.OnParametersSet()
  ├─ Columns = Parent.RearrangeColumns(HeaderParameters.Columns)
  ├─ Rows = GetRows()
  │    ├─ non-stacked: single header row
  │    └─ stacked: multiple header rows by depth
  └─ RowCollection = frozen top data rows

GridContent.OnParametersSet()
  ├─ Rows = RowModelGenerator.GenerateRows(...) or Parent.Rows
  ├─ if FrozenRows > 0
  │    ├─ trigger HeaderStateChanged
  │    └─ Rows = Parent.Rows.Skip(FrozenRows)
  └─ render only scrollable body rows

RowModelGenerator.GenerateRow()
  └─ row.Cells = GenerateCells(row)
         ├─ columns = Parent.RearrangeColumns(all leaf columns)
         └─ emit cells in frozen-left → movable/fixed → frozen-right order

GridRow.razor
  └─ foreach cell in GetCells(...)
         └─ emit <td> in resolved order
```

**Meaning**
- Frozen rows are a **header/body split problem**.
- Frozen columns are an **ordered-cell generation problem**.
- Stacked headers with frozen columns are a **hierarchical projection + ordering problem**.

---

## ASCII Diagram — Frozen Rows Only
<!-- token-budget: 260 words -->

```text
SfGrid<TValue>
├── <div class="e-gridheader">
│   └── <div class="e-headercontent">
│       └── GridHeader
│           └── <table id="{ID}_header_table" class="e-table">
│               ├── <thead>
│               │   └── header <tr> / <th> structure
│               └── <tbody class="{HideClass}">
│                   └── GridRow × FrozenRows
│                       └── pinned top rows only
│
├── <div class="e-frozenrow-border"></div>
│
└── <div class="e-gridcontent">
    └── <div class="e-content">
        └── GridContent
            └── <table id="{ID}_content_table" class="e-table">
                └── <tbody>
                    └── GridRow × remaining rows
                        where source rows = Parent.Rows.Skip(FrozenRows)
```

**Rule**
- `GridHeader` owns the DOM for pinned top rows.
- `GridContent` must not duplicate them; it explicitly skips them.
- Any bug that shows duplicated top rows usually means the `Skip(FrozenRows)` path or `HeaderStateChanged` sync path regressed.

---

## ASCII Diagram — Frozen Columns Only
<!-- token-budget: 320 words -->

```text
Input column model
  [C0, C1, C2, C3, C4, C5]
     │   │       │       │
     │   │       │       └─ frozen-right
     │   │       └──────── movable/fixed
     │   └──────────────── frozen-left
     └──────────────────── frozen-left

RearrangeColumns()
  ├─ frozen-left  = [C0, C1]
  ├─ movable/fixed= [C2, C3, C4]
  └─ frozen-right = [C5]

Resolved order
  [C0, C1, C2, C3, C4, C5]
        ↓
Header generation (`GridHeader.GetRows()` / `GenerateCells()`)
        ↓
Body generation (`RowModelGenerator.GenerateCells()`)
        ↓
GridRow renders `<th>` / `<td>` in that resolved order
```

**DOM shape**

```text
GridHeader
└── single <table>
    └── <thead>
        └── one or more <tr>
            └── <th> in rearranged frozen-aware order

GridContent
└── single <table>
    └── <tbody>
        └── <tr>
            └── <td> in rearranged frozen-aware order
```

**Rule**
- Standard frozen columns do **not** split into three separate tables.
- The freeze behavior is achieved by **ordered cells + freeze CSS/class semantics**, not separate content tables in this renderer.

---

## ASCII Diagram — Frozen Rows + Frozen Columns Combination
<!-- token-budget: 360 words -->

```text
                         +----------------------+
                         |    SfGrid.razor      |
                         +----------+-----------+
                                    |
                +-------------------+-------------------+
                |                                       |
                v                                       v
      +--------------------+                 +----------------------+
      |     GridHeader     |                 |     GridContent      |
      +---------+----------+                 +----------+-----------+
                |                                       |
                | RearrangeColumns()                    | RowModelGenerator
                | for header columns                    |   └─ RearrangeColumns()
                v                                       v
      +----------------------+                +-----------------------+
      | Header cell models   |                | Body row cell models  |
      | frozen-aware order   |                | frozen-aware order    |
      +----------+-----------+                +-----------+-----------+
                 |                                            |
                 |                                            |
      +----------v-----------+                    +-----------v----------+
      | <thead> header rows  |                    | <tbody> body rows    |
      +----------------------+                    +----------------------+
      | <tbody> frozen rows  |                    | rows after Skip()    |
      | pinned top section   |                    | scrollable section   |
      +----------------------+                    +----------------------+
```

**Interpretation**
- The header pipeline and content pipeline both depend on the same column-ordering logic.
- The frozen-row split happens **after** row generation and before body rendering.
- The frozen-column ordering happens **before** cell generation for both header and body.

---

## Stacked Header + Frozen Columns Projection Flow
<!-- token-budget: 520 words -->

`RearrangeColumns()` does more than flatten order when `IsStackedHeader == true`.

### What it does
1. Inspects the top-level stacked columns.
2. Computes `FrozenMovableLabel` for leaf columns using:
   - `FrozenLeft`
   - `FrozenLeftLast`
   - `Movable`
   - `MovableFixed`
   - `FrozenRight`
   - `FrozenRightFirst`
3. Rebuilds stacked parent nodes by recursively filtering child branches with `SetStackedFrozenAndMovableColumns()`.
4. Produces a top-level ordered column tree:
   - frozen-left stacked branches
   - movable/fixed stacked branches
   - frozen-right stacked branches
5. `GridHeader.GetRows()` then converts that projected tree into row-depth-based stacked header rows.

### ASCII flow

```text
Original stacked tree

Parent A
├─ A1 (frozen-left)
├─ A2 (movable)
└─ A3 (frozen-right)

Parent B
├─ B1 (movable)
└─ B2 (movable)

RearrangeColumns() + SetStackedFrozenAndMovableColumns()

Frozen-left projection              Movable projection                Frozen-right projection
Parent A                            Parent A                          Parent A
└─ A1                               └─ A2                             └─ A3

Parent B                            Parent B                          (none)
(none)                              ├─ B1
                                    └─ B2

Final stacked order
  [ Parent A(left), Parent A(movable), Parent B(movable), Parent A(right) ]

GridHeader.GetRows()
  ├─ MeasureColumnDepth()
  ├─ GetStackedColumns(level 1..N)
  └─ GenerateCells(levelColumns, requireFlatten:false, maxDepth, level)
         ↓
      <thead>
      ├─ row 1: stacked parents in projected frozen-aware order
      ├─ row 2: child headers in matching order
      └─ row N: leaf headers in matching order
```

**Rules**
- Stacked header projection is not a later cosmetic pass. It is part of `RearrangeColumns()`.
- Parent stacked nodes may be duplicated into multiple projected branches when their children span left/movable/right zones.
- Header/body alignment depends on using the same rearranged/projection result everywhere.

---

## Row Cell Ordering vs Concat — Exact Behavior
<!-- token-budget: 340 words -->

### Standard non-column-virtual path

```text
RowModelGenerator.GenerateCells(row)
  ├─ columns = Parent.RearrangeColumns(GetColumns(...))
  ├─ foreach column in columns
  │    └─ cells.Add(GenerateCell(...))
  └─ row.Cells now already match frozen-aware order

GridRow.GetCells()
  └─ if !EnableColumnVirtualization
       └─ return RowParameters.Row.Cells
```

### Column-virtualized retrieval path

```text
GridRow.GetCells(start, end)
  ├─ FrozenLeftCells  = row.Cells.Where(left frozen)
  ├─ FrozenRightCells = row.Cells.Where(right frozen)
  ├─ Cells            = movable/fixed visible window
  └─ return FrozenLeftCells
            .Concat(Cells)
            .Concat(FrozenRightCells)
```

**Important guardrail**
- Do **not** document standard frozen-column rendering as depending on concat. That is incomplete.
- The primary standard-path invariant is: `row.Cells` are already ordered by `RearrangeColumns()`.
- Concat is the **window assembly step** used when column virtualization must splice the visible movable segment between frozen left/right segments.

---

## Combination Architecture Matrix
<!-- token-budget: 420 words -->

| Combination | Primary renderer path | Ordering authority | Special risk |
|---|---|---|---|
| Frozen rows only | `GridHeader` tbody + `GridContent` tbody | row split by `FrozenRows` | duplicated or missing top rows |
| Frozen columns only | `GridHeader`/`GridContent` standard tables | `RearrangeColumns()` | header/body misalignment |
| Frozen rows + frozen columns | header/body split + frozen-aware cell order | `RearrangeColumns()` + `Skip(FrozenRows)` | pinned rows and body rows diverge |
| Stacked header + frozen columns | `GridHeader.GetRows()` stacked mode | `RearrangeColumns()` + `SetStackedFrozenAndMovableColumns()` | wrong parent projection or colspan/rowspan mismatch |
| Grouping + frozen columns | standard content with possible column rearrange refresh | `RearrangeColumns()` re-applied in content | grouped indent cells can offset wrong columns |
| Column virtualization + frozen columns | `GridVirtualHeader`/`GridVirtualContent` or `GridRow.GetCells()` virtual windowing | concat of left + movable window + right | visible-window splice breaks frozen edges |
| Frozen rows + add-new-row top | `GridHeader` can host add-new-row with frozen/infinite/virtual conditions | header-body coordination | add row appears in wrong section |
| Infinite scroll + frozen rows | header frozen tbody + content append/skip logic | `HeaderStateChanged` and row cache sync | stale frozen row mirror |

---

## Debugging Checklist
<!-- token-budget: 260 words -->

When debugging frozen DOM issues, validate in this order:

1. **Root path** — confirm the grid is on the standard path (`GridHeader` + `GridContent`) and not virtual renderers.
2. **Column order source** — inspect the output of `RearrangeColumns()`.
3. **Stacked projection** — if stacked headers exist, inspect the projected parent-child tree, not just flattened leaves.
4. **Header rows** — verify `GridHeader.GetRows()` created the expected count and order of header cells.
5. **Frozen row collection** — verify `GridHeader.RowCollection` holds exactly the first `FrozenRows` data rows.
6. **Body row skip** — verify `GridContent.Rows = Parent.Rows.Skip(FrozenRows)`.
7. **Rendered cell source** — for standard mode, inspect `row.Cells`; for column virtualization, inspect `GridRow.GetCells()` concat output.
8. **Alignment bugs** — compare header cell order and body cell order after rearrangement, not before.

---

## JavaScript Interop
<!-- token-budget: 180 words -->

Frozen row/column features require JavaScript coordination for freeze-line drag-and-drop and height synchronization.

### C# → TypeScript Interop Calls

| C# Method | Location | TypeScript Function | Location | Purpose |
|-----------|----------|---------------------|----------|---------|
| `InvokeMethod("sfBlazor.Grid.preventFreezeLineMoving")` | `Freeze.cs` lines 1052, 1056 | `preventFreezeLineMoving()` | `scripts/sf-grid.ts` line 120 | Cancel or allow freeze line drag operation |
| `InvokeMethod("sfBlazor.Grid.freezeLineMovedActions")` | `Freeze.cs` line 1131 | `freezeLineMovedActions()` | `scripts/sf-grid.ts` line 127 | Update frozen column counts and trigger height recalculation after freeze line moves |
| `InvokeMethod("sfBlazor.Grid.frozenHeight")` | `Freeze.cs` line 1155 | `frozenHeight()` | `scripts/sf-grid.ts` line 142 | Synchronize frozen row heights and handle text wrap scenarios |

### TypeScript Functions

**`preventFreezeLineMoving(dataId: string, isCancel: boolean)`**  
Delegates to `frozenDragDropModule.preventFreezeLineMoving()` to control freeze-line dragging behavior based on event cancellation.

**`freezeLineMovedActions(dataId: string, freezeLineMovingClientOptions: FreezeLineMovingClientOptions)`**  
Updates grid instance options (actualFrozenColumns, frozenLeftCount, frozenRightCount, etc.), calls `freezeLineMovedAction()`, and triggers `setFrozenHeight()`.

**`frozenHeight(dataId: string, options: IGridOptions)`**  
Calls `freezeModule.setFrozenHeight()` and handles text wrap refresh with `refreshRowHeight()` and `refreshFreeze()`.

### Responsibility Split

| Layer | Responsibility |
|-------|----------------|
| **C# (Freeze.cs)** | Event handling, freeze state management, column rearrangement logic, event args construction |
| **TypeScript (sf-grid.ts)** | DOM manipulation, freeze-line drag UI, height synchronization across frozen/movable zones, scroll coordination |

### CRITICAL Rules
1. **Always invoke JS after C# freeze state updates** to ensure DOM matches model
2. **Never skip `setFrozenHeight()` after freeze changes** or row height mismatches will occur
3. **Freeze-line moving requires both event prevention and action completion calls** to maintain consistent state
4. **Text wrap + frozen rows requires `refreshFreeze()` with 'textwrap' case** for proper height recalculation

---

## Prompt Template for Agents
<!-- token-budget: 220 words -->

Use this workflow when working on frozen DOM bugs or features:

1. Identify whether the issue is in the **standard** path or the **virtualized** path.
2. Trace `SfGrid.razor` to confirm which renderer pair is active.
3. Inspect `RearrangeColumns()` first; for stacked headers also inspect `SetStackedFrozenAndMovableColumns()`.
4. Verify header generation in `GridHeader.GetRows()`.
5. Verify body generation in `RowModelGenerator.GenerateCells()` and `GridContent.OnParametersSet()`.
6. If column virtualization is active, inspect `GridRow.GetCells()` concat logic separately.
7. If freeze-line moving or height sync issues occur, verify JavaScript interop calls in `Freeze.cs` and corresponding TypeScript functions in `sf-grid.ts`.
8. Preserve these invariants:
   - frozen rows live in header tbody
   - content skips frozen top rows
   - standard row cells are already rearranged
   - stacked parents are projected per frozen zone
   - header/body order must come from the same rearranged model
   - JavaScript interop must be called after C# state updates

If any expected flow differs, stop and validate before editing.
