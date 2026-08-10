---
name: cell-merging-skill
description: Expert knowledge for Cell Merging (Row & Column Spanning) in Syncfusion Blazor DataGrid. Use this skill for feature-implementation or bug-fix tasks scoped to manual merging (MergeCells/UnmergeCells APIs), automatic spanning (AutoSpan modes), and cross-feature interaction guarantees with frozen columns, grouping, virtualization, and rendering pipelines.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Cell Merging in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

## Agent Invocation
<!-- token-budget: 40 words -->
- Skill scope: Cell Merging (`MergeCells`, `UnmergeCells`, `AutoSpan`, manual/automatic spanning)
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to cell merging/spanning. Do NOT load alongside other feature skills.
- One agent invocation = one feature skill maximum.

---

## Knowledge References
<!-- token-budget: 60 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `src/Internal/Actions/MergeHandler.cs` — core merging engine
- `src/Internal/Models/Cell.cs` — cell state (ColSpan, RowSpan, IsSpanned, IsRowSpanned)
- `src/Internal/Models/Row.cs` — row state and boundaries
- `src/Internal/Renderer/GridRow.CellRenderer.cs` — DOM rendering output layer
- `src/Internal/Generators/RowModelGenerator.cs`, `GroupModelGenerator.cs` — pipeline entry points
- `src/SfGrid.Methods.cs`, `SfGrid.Properties.cs`, `SfGrid.Lifecycle.cs` — public API & initialization
- `src/GridColumn.cs` — column-level AutoSpan config
- `src/Enumeration/GridsEnumerations.cs` — AutoSpanMode enum
- `src/EventModels/Grids.cs` — MergeCellInfo, UnmergeCellInfo, QueryCellInfoEventArgs
- `src/GridEvents.cs` — QueryCellInfo event hook

---

## Training Insights Applied
<!-- token-budget: 90 words -->

Key rules and edge cases from the codebase that directly govern Cell Merging work:

- **Two independent paths**: Manual merging (`ManuallyMergedCells` list) and automatic spanning (`AutoSpanMode`) operate separately — manual merges are applied first, then auto passes skip excluded cells.
- **Frozen region partitioning is mandatory**: Auto column-spanning processes left-frozen, movable, and right-frozen columns in separate passes; right-frozen processes in **reverse** to avoid overlap.
- **Frozen rows require range-split processing**: Auto row-spanning separates at `FrozenRows` boundary — frozen and unfrozen rows are spanned independently.
- **Grouping requires boundary respect**: Manual merges check `ParentUid` and `RowType` boundaries — spans cannot cross group caption/data/summary boundaries.
- **Exclusion HashSet replaces O(n) .Any() calls**: Pre-computed exclusion set enables O(1) lookup for skipping excluded cells during auto-span passes.
- **Spanned-away cells are never rendered**: `GridRow.CellRenderer.RenderCell()` returns early if `IsSpanned || IsRowSpanned` — the HTML TD is not emitted at all for covered cells.
- **QueryCellInfo fires per data cell**: Event allows runtime override of `ColSpan` and `RowSpan` values before rendering.
- **Last row border aware of rowspan depth**: `ApplyLastRowBorder()` adds `e-lastrowcell` class to cells whose rowspan extends to the last visible row.
- **No direct `StateHasChanged()`**: Merging changes trigger `Refresh()` which goes through grid's internal render scheduling.
- **`SuppressAutoSpanning` flag for atomic operations**: `UnmergeAllAsync()` sets flag to skip auto passes during the reset, then re-enables them.

---

## Code Location Map (SRP Architecture)
<!-- token-budget: 100 words -->

Cell Merging logic remains in dedicated modules (no SRP partial splits):

| Path | Lines | Purpose |
|------|-------|---------|
| **`src/Internal/Actions/MergeHandler.cs`** | — | **Core engine** — `Process()`, `GroupingProcess()`, manual merge application (`ProcessManualMerges`), auto column/row spanning logic (`ProcessAutomaticColumnSpanning`, `ProcessAutomaticRowSpanning`), frozen partitioning, exclusion HashSet handling |
| `src/Internal/Models/Cell.cs` | — | Cell state: `ColSpan`, `RowSpan`, `IsSpanned`, `IsRowSpanned` properties |
| `src/Internal/Models/Row.cs` | — | Row boundaries: `IsDataRow`, `Cells`, `Index`, `ParentUid`, `RowType`, `Visible` |
| `src/Internal/Generators/RowModelGenerator.cs` | — | Calls `MergeModule.Process(rows)` after row generation |
| `src/Internal/Generators/GroupModelGenerator.cs` | — | Calls `MergeModule.GroupingProcess(rows)` for grouped data pipelines |
| **`src/Internal/Renderer/GridRow.CellRenderer.cs`** | 57, 530, 571, 678 | **DOM output layer** — `RenderCell()` method (line 57): early exit if `IsSpanned || IsRowSpanned`; `AddSpanAttributesInternal()` method (line 678): writes `colspan`/`rowspan` HTML attributes; `ApplyLastRowBorder()` method (line 571): rowspan-aware borders; line 530 invokes span attributes |
| `src/SfGrid.Methods.cs` | — | Public APIs: `MergeCellsAsync()`, `UnmergeCellsAsync()`, `UnmergeAllAsync()` |
| `src/SfGrid.Properties.cs` | — | `AutoSpan` property, `SuppressAutoSpanning` flag |
| `src/SfGrid.Lifecycle.cs` | — | `MergeModule` initialization |
| `src/GridColumn.cs` | 764-922 | Column-level `AutoSpan` property (line 918), `GetEffectiveAutoSpanning()` method (line 920) for grid-column mode intersection |
| `src/Enumeration/GridsEnumerations.cs` | 2036-2080 | `AutoSpanMode` enum: `None`, `Row`, `Column`, `HorizontalAndVertical` (line 2036) |
| `src/EventModels/Grids.cs` | 4258, 7349, 7423 | `QueryCellInfoEventArgs<T>` class (line 4258), `MergeCellInfo` class (line 7349), `UnmergeCellInfo` class (line 7423) |
| `src/GridEvents.cs` | — | `QueryCellInfo` event — runtime span injection hook |

**Note**: Cell merging logic remains a standalone action module. `MergeHandler.cs` is invoked by generators after row creation.

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 200 words -->

> Built from live code analysis and codebase cross-reference.  
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| **Cell Merging + Frozen Columns (Left)** | `ProcessAutomaticColumnSpanning` partitions frozen-left columns separately; left-frozen processes forward (index 0→N). Manual merges respect frozen state but operate across all regions. Border class `e-freezeleftborder` applied to rightmost spanning cell in left-frozen band. | Critical |
| **Cell Merging + Frozen Columns (Right)** | Right-frozen columns processed in **reverse** (index N→0) to avoid overlap. Manual merges validate span boundaries. Border class `e-freezerightborder` applied to leftmost spanning cell in right-frozen band. | Critical |
| **Cell Merging + Frozen Rows** | `ProcessAutomaticRowSpanningWithFrozenSupport` splits row-spanning at `FrozenRows` boundary — frozen and unfrozen sections span independently. Spans cannot cross frozen/unfrozen boundary. | Critical |
| **Cell Merging + Grouping** | Manual merges check `ParentUid` and `RowType` — spans cannot cross caption/data/summary boundaries. `GroupingProcess()` path used instead of `Process()`. Spanned caption rows do not collapse sub-group visibility. | Critical |
| **Cell Merging + Virtual Scroll** | Row indices in `MergeCellInfo` are page-relative (index within current view, not absolute in data set). Virtual scroll recalculates visible rows on every navigation; merge state re-applied per render. Spanned rows remain spanned across scroll regions. | High |
| **Cell Merging + Infinite Scroll** | `RowModelGenerator` has `ProcessMerges(infiniteRows)` path. Row indices append-relative; merges persist across load-more cycles. `InfiniteScrollModule` does not re-index merged cells. | High |
| **Cell Merging + Paging** | Manual merge `RowIndex` values reset **per page** (0-based per-page index, not absolute). Page change triggers re-render; merge state re-applied to new page rows. Auto-spanning re-runs per page. | High |
| **Cell Merging + Sorting** | Sort does NOT preserve manual merge state — rows reorder, but `RowIndex` in `MergeCellInfo` references original row indices. After sort, user must re-apply merges with new row indices. Auto-spanning detects and re-applies to sorted rows. | High |
| **Cell Merging + Filtering** | Filter reduces row set; manual merge `RowIndex` values are relative to **filtered view**, not original data. Hidden rows are not counted. Auto-spanning runs only on visible (filtered) rows. | High |
| **Cell Merging + Selection** | `AttributeSelectionInternal` applies selection classes **after** span attributes. Spanned cells are not rendered (no TD), so no stale selection DOM. Spanned-away cells cannot be selected (not in DOM). | High |
| **Cell Merging + Editing** | Edit mode opens cell-level or row-level editor. Spanning does not block editor rendering. Spanned cells skip edit state check (not rendered). Manual edit in a cell that is part of a span edits the anchor cell value only. | High |
| **Cell Merging + Aggregates** | Aggregates are computed **before** merging (data-level, not DOM-level). Aggregate rows appear after spanned data rows; spans do not extend into aggregate rows. | High |
| **Cell Merging + Export (Excel/PDF)** | Export respects `ColSpan` / `RowSpan` — `ExcelQueryCellInfoEventArgs` and `PdfQueryCellInfoEventArgs` carry span metadata. Exported file contains merged cells with same span values as grid DOM. | Medium |
| **Cell Merging + Column Reorder** | Reorder changes column sequence in `GridUtils.GetColumns()`. Merge `ColumnIndex` values must be updated to reflect new positions, OR merges re-identified by `Field` name (if supported). Manual merges can become misaligned if reorder happens after merge. | Medium |
| **Cell Merging + ForeignKey** | ForeignKey cells span normally; display text is merged (not underlying value). Merge logic is agnostic to foreign key display. | Medium |
| **Cell Merging + Persistence** | `EnablePersistence = true` does NOT persist manual merge state — persisted data is column widths, sort, group, filter only. Manual merges must be re-applied on page reload. Auto-spanning re-runs automatically on load. | Medium |
| **Cell Merging + QueryCellInfo** | `QueryCellInfo` event fires **before** merging. User code can read/modify span values in event args. Changes persist to rendered cell attributes. | Medium |
| **Cell Merging + Column Hiding** | Hidden columns are skipped during span calculations — `Visible` check prevents hidden column cells from participating. Manual merge `ColumnIndex` must account for hidden columns (index shifts). | Medium |

---

## Rendering Approach & DOM Structure

### 🔄 Complete Data Flow (ASCII Diagram)

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        GRID DATA REFRESH CYCLE                          │
│                   (Paging | Sorting | Grouping | Filter)                │
└─────────────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
            ┌─────────────────────────────────────┐
            │  RowModelGenerator.GenerateRows()   │  OR
            │  GroupModelGenerator.GenerateRows() │
            │  (Build Row<T> with Cell<T> list)   │
            └─────────────────────────────────────┘
                                 │
                    ┌────────────┴────────────┐
                    ▼                         ▼
            [Regular Rows]          [Grouped Rows]
                    │                         │
                    └────────────┬────────────┘
                                 ▼
        ┌──────────────────────────────────────────────────────┐
        │         MergeModule.Process(rows)                    │
        │     OR  MergeModule.GroupingProcess(rows)            │
        │                                                      │
        │  ┌──────────────────────────────────────────────┐   │
        │  │ 1. ProcessManualMerges(rows)                 │   │
        │  │    - Read ManuallyMergedCells list           │   │
        │  │    - For each MergeCellInfo:                 │   │
        │  │      • Validate RowIndex, ColumnIndex bounds │   │
        │  │      • Check ParentUid/RowType boundaries    │   │
        │  │      • Set Cell.ColSpan & Cell.RowSpan       │   │
        │  │      • Mark spanned-away cells as IsSpanned  │   │
        │  │      • Mark row-spanned cells as IsRowSpanned│   │
        │  └──────────────────────────────────────────────┘   │
        │                        │                            │
        │  ┌──────────────────────────────────────────────┐   │
        │  │ 2. IF AutoSpan enabled (not suppressed)      │   │
        │  │    ProcessAutomaticColumnSpanning(rows)      │   │
        │  │    - Partition columns: Left | Movable | Right   │
        │  │    - For each row:                           │   │
        │  │      FOR each column partition (in order):   │   │
        │  │      • Build HashSet from ExcludedCells      │   │
        │  │      • Scan adjacent cells in row:           │   │
        │  │        IF same value & same rowspan:         │   │
        │  │          → Set cell.ColSpan = spanCount      │   │
        │  │          → Mark next cells as IsSpanned       │   │
        │  │        [Right-frozen: scan in reverse]       │   │
        │  └──────────────────────────────────────────────┘   │
        │                        │                            │
        │  ┌──────────────────────────────────────────────┐   │
        │  │ 3. IF AutoSpan enabled (not suppressed)      │   │
        │  │    ProcessAutomaticRowSpanning(rows)         │   │
        │  │    - IF FrozenRows > 0:                      │   │
        │  │      ProcessRange(rows, 0, FrozenRows)       │   │
        │  │      ProcessRange(rows, FrozenRows, end)     │   │
        │  │    ELSE: ProcessRange(rows, 0, end)          │   │
        │  │                                              │   │
        │  │    For each column (left | movable | right): │   │
        │  │      Scan adjacent rows in range:            │   │
        │  │      IF same value (after column-spans):     │   │
        │  │        → Set cell.RowSpan = spanCount        │   │
        │  │        → Mark next rows as IsRowSpanned      │   │
        │  └──────────────────────────────────────────────┘   │
        │                                                      │
        │  RESULT: All cells have correct span metadata        │
        └──────────────────────────────────────────────────────┘
                                 │
                                 ▼
        ┌──────────────────────────────────────────────────────┐
        │          GridContent.razor (Row Iteration)           │
        │   @foreach (var row in ParentGrid.Rows)              │
        │     <GridRow @key="row.Uid" />                       │
        └──────────────────────────────────────────────────────┘
                                 │
                                 ▼
        ┌──────────────────────────────────────────────────────┐
        │    GridRow.razor (Cell Iteration)                    │
        │   @foreach (var cell in row.Cells)                   │
        │     RenderCell(new GridCellParameters                │
        │     { Row = row, Cell = cell })                      │
        └──────────────────────────────────────────────────────┘
                                 │
                                 ▼
        ┌──────────────────────────────────────────────────────┐
        │   GridRow.CellRenderer.RenderCell(CellParameters)    │
        │                                                      │
        │  IF cell.IsSpanned || cell.IsRowSpanned:            │
        │    → return (EARLY EXIT — no <td> emitted)          │
        │                                                      │
        │  ELSE: GetOrUpdateCellState(cell, row)              │
        │    • Invoke QueryCellInfoEventIfNeeded()            │
        │    • Allow user code to modify span values          │
        │    • Build cell class/style list                    │
        │    • AddSpanAttributesInternal():                   │
        │      - IF cell.ColSpan > 1:                         │
        │        SET colspan="N" + aria-colspan="N"           │
        │      - IF cell.RowSpan > 1:                         │
        │        SET rowspan="N" + aria-rowspan="N"           │
        │    • ApplyFrozenColumnStyles():                     │
        │      - IF cell at frozen-left boundary & ColSpan>1: │
        │        ADD "e-freezeleftborder" class               │
        │      - IF cell at frozen-right boundary & ColSpan>1:│
        │        ADD "e-freezerightborder" class              │
        │    • ApplyLastRowBorder():                          │
        │      - IF cell.RowSpan extends to last visible row: │
        │        ADD "e-lastrowcell" class                    │
        │    • Emit <td> with all attributes                  │
        └──────────────────────────────────────────────────────┘
                                 │
                                 ▼
        ┌──────────────────────────────────────────────────────┐
        │              HTML <table> Output                     │
        │    <table role="grid">                               │
        │      <tbody>                                         │
        │        <tr role="row">                               │
        │          <td colspan="2" rowspan="1" ...>           │
        │            [Cell content]                            │
        │          </td>                                       │
        │          <!-- Skip spanned cells — no <td> here --> │
        │          <td colspan="1" rowspan="3" ...>           │
        │            [Merged cell content]                     │
        │          </td>                                       │
        │        </tr>                                         │
        │      </tbody>                                        │
        │    </table>                                          │
        └──────────────────────────────────────────────────────┘
```

---

### 📊 Cell State Machine (Before/After Merge)

```
┌─────────────────────────────────────────────────────────────┐
│  CELL STATE BEFORE MERGING (Initial)                        │
├─────────────────────────────────────────────────────────────┤
│  Cell[0,0]  Cell[0,1]  Cell[0,2]  Cell[0,3]                │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐               │
│  │        │ │        │ │        │ │        │               │
│  │ Value  │ │ Value  │ │ Value  │ │ Value  │               │
│  │        │ │        │ │        │ │        │               │
│  │ColSpan:│ │ColSpan:│ │ColSpan:│ │ColSpan:│               │
│  │   1    │ │   1    │ │   1    │ │   1    │               │
│  │RowSpan:│ │RowSpan:│ │RowSpan:│ │RowSpan:│               │
│  │   1    │ │   1    │ │   1    │ │   1    │               │
│  │Spanned:│ │Spanned:│ │Spanned:│ │Spanned:│               │
│  │ FALSE  │ │ FALSE  │ │ FALSE  │ │ FALSE  │               │
│  └────────┘ └────────┘ └────────┘ └────────┘               │
│                                                              │
│  Cell[1,0]  Cell[1,1]  Cell[1,2]  Cell[1,3]                │
│  ┌────────┐ ┌────────┐ ┌────────┐ ┌────────┐               │
│  │        │ │        │ │        │ │        │               │
│  │ Value  │ │ Value  │ │ Value  │ │ Value  │               │
│  │ ...    │ │ ...    │ │ ...    │ │ ...    │               │
│  └────────┘ └────────┘ └────────┘ └────────┘               │
└─────────────────────────────────────────────────────────────┘

                    ↓ Apply Merge ↓
         MergeCellInfo {
           RowIndex: 0, ColumnIndex: 1,
           RowSpan: 2, ColumnSpan: 2
         }

┌─────────────────────────────────────────────────────────────┐
│  CELL STATE AFTER MERGING                                   │
├─────────────────────────────────────────────────────────────┤
│  Cell[0,0]  [ANCHOR]      [COVERED]   Cell[0,3]            │
│  ┌────────┐ ┌─────────────────────┐  ┌────────┐            │
│  │        │ │                     │  │        │            │
│  │ Value  │ │ ColSpan: 2          │  │ Value  │            │
│  │        │ │ RowSpan: 2          │  │        │            │
│  │ColSpan:│ │ IsSpanned: FALSE    │  │ColSpan:│            │
│  │   1    │ │ (Renders as <td>)   │  │   1    │            │
│  │RowSpan:│ │                     │  │RowSpan:│            │
│  │   1    │ │ [MERGED CONTENT]    │  │   1    │            │
│  │Spanned:│ │                     │  │Spanned:│            │
│  │ FALSE  │ │                     │  │ FALSE  │            │
│  └────────┘ └─────────────────────┘  └────────┘            │
│             (colspan="2" rowspan="2")                       │
│                                                              │
│  Cell[0,2]  [COVERED BY ABOVE]         Cell[1,3]           │
│  ┌────────┐ ┌─────────────────────┐  ┌────────┐            │
│  │ IsSpan:│ │ IsSpanned: TRUE      │  │        │            │
│  │  TRUE  │ │ (NOT RENDERED)       │  │ Value  │            │
│  │(Skip)  │ │ ← No <td> in HTML    │  │        │            │
│  └────────┘ └─────────────────────┘  └────────┘            │
│                                                              │
│  Cell[1,0]  Cell[1,1]  [COVERED]      Cell[1,3]            │
│  ┌────────┐ │Spanned:  └─────────────┐┌────────┐           │
│  │        │ │  TRUE    (NOT RENDERED)││        │           │
│  │ Value  │ │  (Skip)                ││ Value  │           │
│  │        │ └─────────────────────────┘│        │           │
│  │ ...    │                            │ ...    │           │
│  └────────┘                            └────────┘           │
└─────────────────────────────────────────────────────────────┘

HTML Output (colspan/rowspan attributes):

<table>
  <tr>
    <td>Value</td>
    <td colspan="2" rowspan="2">[MERGED CONTENT]</td>
    <!-- Cell[0,2] NOT emitted — IsSpanned = true -->
    <td>Value</td>
  </tr>
  <tr>
    <td>Value</td>
    <!-- Cell[1,1] NOT emitted — IsSpanned = true -->
    <!-- Cell[1,2] NOT emitted — IsSpanned = true -->
    <td>Value</td>
  </tr>
</table>
```

---

### 🎯 Rendering Pipeline with Frozen Columns

```
┌──────────────────────────────────────────────────────────────────┐
│  COLUMN SPANNING WITH FROZEN COLUMNS (Left & Right)              │
│                                                                  │
│  Grid has:                                                       │
│  - Frozen Left:   Col[0], Col[1]   (FreezeDirection.Left)        │
│  - Movable:       Col[2], Col[3], Col[4]                         │
│  - Frozen Right:  Col[5], Col[6]   (FreezeDirection.Right)       │
└──────────────────────────────────────────────────────────────────┘

COLUMN SPANNING PROCESSING:

┌─────────────────────────────────────────────────────────────────┐
│ ProcessAutomaticColumnSpanning(rows)                            │
│                                                                 │
│ FOR each row:                                                   │
│   ┌─────────────────────────────────────────────────────────┐  │
│   │ Pass 1: LEFT-FROZEN (forward scan: index 0→1)          │  │
│   │ ┌─────────────────────────────────────────────────────┐ │  │
│   │ │ FOR Col[0], Col[1]:                                 │ │  │
│   │ │   IF Col[0]Value == Col[1]Value:                    │ │  │
│   │ │     Col[0].ColSpan = 2                              │ │  │
│   │ │     Col[1].IsSpanned = TRUE                          │ │  │
│   │ │   → Process INDEPENDENTLY within frozen-left        │ │  │
│   │ │   → Apply e-freezeleftborder to rightmost cell      │ │  │
│   │ └─────────────────────────────────────────────────────┘ │  │
│   │                                                          │  │
│   │ ┌─────────────────────────────────────────────────────┐ │  │
│   │ │ Pass 2: MOVABLE (forward scan: index 2→3→4)         │ │  │
│   │ │ ┌─────────────────────────────────────────────────┐ │ │  │
│   │ │ │ FOR Col[2], Col[3], Col[4]:                     │ │ │  │
│   │ │ │   IF Col[2]Value == Col[3]Value == Col[4]Value: │ │ │  │
│   │ │ │     Col[2].ColSpan = 3                          │ │ │  │
│   │ │ │     Col[3].IsSpanned = TRUE                      │ │ │  │
│   │ │ │     Col[4].IsSpanned = TRUE                      │ │ │  │
│   │ │ │   → Spans within movable band, no frozen border  │ │ │  │
│   │ │ └─────────────────────────────────────────────────┘ │ │  │
│   │ │                                                      │ │  │
│   │ │ ┌─────────────────────────────────────────────────┐ │ │  │
│   │ │ │ Pass 3: RIGHT-FROZEN (REVERSE scan: index 6→5) │ │ │  │
│   │ │ │ ┌─────────────────────────────────────────────┐ │ │ │  │
│   │ │ │ │ Start at Col[6], scan backward to Col[5]:   │ │ │ │  │
│   │ │ │ │   IF Col[6]Value == Col[5]Value:            │ │ │ │  │
│   │ │ │ │     Col[6].ColSpan = 2 (relative to right)  │ │ │ │  │
│   │ │ │ │     Col[5].IsSpanned = TRUE                 │ │ │ │  │
│   │ │ │ │   → Reverse scan to avoid overlap with movable│ │ │ │  │
│   │ │ │ │   → Apply e-freezerightborder to leftmost    │ │ │ │  │
│   │ │ │ └─────────────────────────────────────────────┘ │ │ │  │
│   │ │ └─────────────────────────────────────────────────┘ │ │  │
│   │ └─────────────────────────────────────────────────────┘ │  │
│   └─────────────────────────────────────────────────────────┘  │
│                                                                 │
│ RESULT: Row now has correct ColSpan values across all bands     │
└─────────────────────────────────────────────────────────────────┘

VISUAL RESULT (HTML):

Left-Frozen Band:     │  Movable Band:       │  Right-Frozen Band:
┌───────────────────┐ │ ┌────────────────┐  │  ┌────────────────┐
│ Col[0] Col[1]     │ │ │ Col[2] Col[3] │  │  │ Col[5] Col[6]  │
├─ ─ ─ ─ ─ ─ ─ ─ ─┤ │ ├─ ─ ─ ─ ─ ─ ─┤  │  ├─ ─ ─ ─ ─ ─ ─┤
│        SPAN       │ │ │    SPAN      │  │  │     SPAN       │
│      (colspan=2)  │ │ │  (colspan=3) │  │  │  (colspan=2)   │
│ ◄──border class   │ │ │              │  │  │border class──► │
└───────────────────┘ │ │              │  │  │                │
                      └────────────────────┘  │                │
                                             └────────────────┘
```

---

### 📍 Row Spanning with Frozen Rows

```
┌──────────────────────────────────────────────────────────────────┐
│  ROW SPANNING WITH FROZEN ROWS                                   │
│                                                                  │
│  Grid has FrozenRows = 2                                         │
│  - Rows[0], Rows[1] = Frozen section                             │
│  - Rows[2], Rows[3], ... = Scrollable section                    │
└──────────────────────────────────────────────────────────────────┘

ROW SPANNING PROCESSING:

Step 1: Identify Frozen Boundary
┌──────────────────────────────────┐
│ IF FrozenRows > 0:               │
│   BoundaryIndex = FrozenRows     │
│   → Split at index 2             │
│                                  │
│ Frozen Region:  Rows[0..1]       │
│ Scrollable:     Rows[2..end]     │
└──────────────────────────────────┘

Step 2: Process Frozen Region Independently
┌──────────────────────────────────────────────────────────┐
│ ProcessRowSpanningInRange(rows, columns, 0, 2)           │
│                                                          │
│ FOR each column:                                         │
│   FOR each row in [0, 1]:                                │
│     IF row[i].Value == row[i+1].Value:                   │
│       row[i].RowSpan = 2                                 │
│       row[i+1].IsRowSpanned = TRUE                       │
│                                                          │
│ Result: Only Rows[0] and Rows[1] participate             │
│         Spans do NOT cross into scrollable region        │
└──────────────────────────────────────────────────────────┘

Step 3: Process Scrollable Region Independently
┌──────────────────────────────────────────────────────────┐
│ ProcessRowSpanningInRange(rows, columns, 2, end)         │
│                                                          │
│ FOR each column:                                         │
│   FOR each row in [2, 3, 4, ...]:                        │
│     IF row[i].Value == row[i+1].Value:                   │
│       row[i].RowSpan = 2                                 │
│       row[i+1].IsRowSpanned = TRUE                       │
│                                                          │
│ Result: Only scrollable rows participate                 │
│         Spans do NOT cross frozen boundary               │
└──────────────────────────────────────────────────────────┘

VISUAL RESULT:

┌────────────────┐  Frozen Header
│ Col1 │ Col2 │  │  (FrozenRows=2)
├─────┼──────┤
│ Val1 │ Val1 │  Row[0] ──┐ RowSpan=2
├─────┼──────┤           │
│     │ Val1 │  Row[1] ──┘
├─────┼──────┤  ←──── Frozen/Scrollable Boundary (never crosses)
│ Val2 │ Val2 │  Row[2] ──┐ RowSpan=2
├─────┼──────┤           │ (Independent span, can equal 2)
│     │ Val2 │  Row[3] ──┘
├─────┼──────┤
│ Val3 │ Val3 │  Row[4] ──┐ RowSpan=3
├─────┼──────┤           │
│     │ Val3 │  Row[5] ──┤
├─────┼──────┤           │
│     │ Val3 │  Row[6] ──┘
└─────┴──────┘

KEY RULE: Spans within frozen section never extend into
          scrollable section, and vice versa.
```

---

### 🎨 Rendering Cell State → HTML Output

```
┌─────────────────────────────────────────────────────────────┐
│  GRIDROW.CELLRENDERER EXECUTION (Per Cell)                  │
└─────────────────────────────────────────────────────────────┘

RenderCell(CellParameters { Cell, Row })
  │
  ├─ IF cell.IsSpanned || cell.IsRowSpanned:
  │    └─→ RETURN (SKIP, no <td> emitted)
  │        /* Spanned cell is covered by another cell's span */
  │
  └─ ELSE: Process cell for rendering
       │
       ├─ GetOrUpdateCellState(cell, row)
       │    ├─ Retrieve cached CellStateInfo OR create new
       │    ├─ IF cell changed OR SoftRefresh flag set:
       │    │    └─ InvokeQueryCellInfoIfNeeded()
       │    │        /* User event: can override ColSpan/RowSpan */
       │    │        args.Cell.ColSpan = customValue  (?)
       │    │        args.Cell.RowSpan = customValue  (?)
       │    └─ Rebuild cell class/style/attribute lists
       │
       ├─ AddSpanAttributesInternal(cell, cellState)
       │    ├─ IF cell.ColSpan.HasValue && cell.ColSpan > 1:
       │    │    cellState.Attributes["colspan"] = cell.ColSpan
       │    │    cellState.Attributes["aria-colspan"] = cell.ColSpan
       │    ├─ IF cell.RowSpan.HasValue && cell.RowSpan > 1:
       │    │    cellState.Attributes["rowspan"] = cell.RowSpan
       │    │    cellState.Attributes["aria-rowspan"] = cell.RowSpan
       │    └─ Store in cellState
       │
       ├─ ApplyFrozenColumnStyles(cell)
       │    ├─ IF cell.Index + cell.ColSpan == LastFrozenLeftIndex
       │    │    && cell.ColSpan > 1:
       │    │    cellState.ClassList.Add("e-freezeleftborder")
       │    └─ IF cell at right-frozen boundary && cell.ColSpan > 1:
       │         cellState.ClassList.Add("e-freezerightborder")
       │
       ├─ ApplyLastRowBorder(cell, row)
       │    ├─ anchorIndex = row.Index
       │    ├─ spanDepth = cell.RowSpan ?? 1
       │    ├─ lastVisibleIndex = GetLastVisibleDataRowIndex()
       │    ├─ IF (anchorIndex + spanDepth - 1) >= lastVisibleIndex:
       │    │    cellState.ClassList.Add("e-lastrowcell")
       │    │    /* Span extends to last visible row */
       │    └─ Else: Remove "e-lastrowcell" if present
       │
       ├─ Build final HTML <td> element:
       │    <td
       │      class="{cellState.ClassList}"
       │      colspan="{cellState.Attributes['colspan']}"
       │      rowspan="{cellState.Attributes['rowspan']}"
       │      aria-colspan="{cellState.Attributes['aria-colspan']}"
       │      aria-rowspan="{cellState.Attributes['aria-rowspan']}"
       │      ... other attributes
       │    >
       │      {cellState.CellValue}
       │    </td>
       │
       └─ Return RenderFragment with <td>

EXAMPLE HTML OUTPUT:

Input State:
  Row[0]: Cell[0](Value="A", ColSpan=2, RowSpan=1, IsSpanned=F)
          Cell[2](IsSpanned=T)
          Cell[3](Value="C", ColSpan=1, RowSpan=1, IsSpanned=F)

Output HTML:
  <tr role="row">
    <td colspan="2" aria-colspan="2" role="gridcell" class="e-rowcell">
      A
    </td>
    <!-- Cell[1] (index 1) not emitted — IsSpanned = true -->
    <!-- Cell[2] (index 2) not emitted — IsSpanned = true -->
    <td colspan="1" aria-colspan="1" role="gridcell" class="e-rowcell">
      C
    </td>
  </tr>
```

---

### 🔌 QueryCellInfo Event Hook (Runtime Modification)

```
┌──────────────────────────────────────────────────────────────┐
│  QUERYCELLINFO EVENT — INJECT CUSTOM SPAN VALUES AT RUNTIME  │
└──────────────────────────────────────────────────────────────┘

Execution Flow:

GetOrUpdateCellState()
  │
  ├─ IF cell changed OR SoftRefresh:
  │    └─ InvokeQueryCellInfoIfNeeded()
  │         │
  │         ├─ Create QueryCellInfoEventArgs<TRow>:
  │         │    - args.Data = row.Data          (row object)
  │         │    - args.Column = cell.Column     (column def)
  │         │    - args.Cell = new CellDOM(      (DOM object)
  │         │        ClassList,
  │         │        Styles,
  │         │        Attributes
  │         │      )
  │         │
  │         ├─ Invoke user event handler:
  │         │    GridEvents.QueryCellInfo?.Invoke(args)
  │         │
  │         ├─ User code can NOW modify:
  │         │    args.Cell.ClassList.Add("custom-class")
  │         │    args.Cell.Styles.Add("color: red;")
  │         │    args.Cell.AttributeList["data-custom"] = "value"
  │         │    args.Cell.ColSpan = 3          ← UPDATE SPAN!
  │         │    args.Cell.RowSpan = 2          ← UPDATE SPAN!
  │         │
  │         ├─ Copy modified values back to cellState:
  │         │    cellState.ClassList = args.Cell.ClassList
  │         │    cellState.StyleList = args.Cell.Styles
  │         │    cellState.Attributes = args.Cell.AttributeList
  │         │    /* Span values already in cell object */
  │         │
  │         └─ Continue rendering with modified state

USER CODE EXAMPLE:

@code {
    void QueryCellInfoHandler(QueryCellInfoEventArgs<OrderData> args)
    {
        // Apply custom span based on data value
        if (args.Data.Quantity > 100)
        {
            args.Cell.ColSpan = 2;  // Merge across 2 columns
            args.Cell.ClassList.Add("high-quantity");
        }
        
        // Apply formatting
        args.Cell.Styles.Add("font-weight: bold;");
        args.Cell.AttributeList["data-value"] = args.Data.Quantity.ToString();
    }
}

RENDERED OUTPUT (for Quantity > 100):

<td colspan="2" aria-colspan="2" 
    class="e-rowcell high-quantity" 
    style="font-weight: bold;" 
    data-value="500"
    role="gridcell">
  500
</td>
```

---

### 🚀 Public API → Internal Execution Path

```
┌──────────────────────────────────────────────────────────────┐
│  USER API CALLS → INTERNAL MERGE ENGINE                      │
└──────────────────────────────────────────────────────────────┘

┌─ MergeCellsAsync(MergeCellInfo info)
│  │
│  ├─ Create or get MergeModule (MergeHandler<T>)
│  ├─ ManuallyMergedCells.Add(info)
│  ├─ Call Refresh()   (triggers full re-render)
│  │
│  ├─→ Refresh() → Render Pipeline
│  │    └─ MergeModule.Process(rows)
│  │        ├─ ProcessManualMerges(rows)
│  │        │  └─ Find and apply this new MergeCellInfo
│  │        ├─ ProcessAutomaticColumnSpanning(rows)
│  │        └─ ProcessAutomaticRowSpanning(rows)
│  │
│  └─ Grid re-renders with merged cells visible

┌─ UnmergeCellsAsync(UnmergeCellInfo info)
│  │
│  ├─ IF cell is in ManuallyMergedCells:
│  │    └─ Remove from list
│  ├─ ELSE: Add to ExcludedFromAutomaticSpanning
│  │
│  ├─ Call Refresh()   (triggers full re-render)
│  │
│  ├─→ Refresh() → Render Pipeline
│  │    └─ MergeModule.Process(rows)
│  │        ├─ ProcessManualMerges(rows)
│  │        │  └─ Skips the unmerged cell
│  │        ├─ ProcessAutomaticColumnSpanning(rows)
│  │        │  └─ Checks ExcludedFromAutomaticSpanning
│  │        │     (this cell won't auto-span)
│  │        └─ ProcessAutomaticRowSpanning(rows)
│  │
│  └─ Grid re-renders with cell unmerged

┌─ UnmergeAllAsync()
│  │
│  ├─ Clear ManuallyMergedCells = null
│  ├─ Clear ExcludedFromAutomaticSpanning = null
│  ├─ Set SuppressAutoSpanning = true
│  ├─ Call Refresh()
│  │
│  ├─→ Refresh() → Render Pipeline
│  │    └─ MergeModule.Process(rows)
│  │        ├─ ProcessManualMerges(rows) → NO-OP (list is null)
│  │        ├─ (AutoSpan skipped due to SuppressAutoSpanning flag)
│  │        └─ Cells rendered with default span=1
│  │
│  ├─ Set SuppressAutoSpanning = false  (re-enable)
│  └─ Grid fully unmerged

┌─ SfGrid.AutoSpan = AutoSpanMode.Row (property setter)
│  │
│  ├─ Detect change via PropertyChanges
│  ├─ Call Refresh()
│  │
│  ├─→ Refresh() → Render Pipeline
│  │    └─ MergeModule.Process(rows)
│  │        ├─ ProcessManualMerges(rows)
│  │        ├─ ProcessAutomaticColumnSpanning(rows)
│  │        │  └─ Uses new AutoSpanMode to enable/disable
│  │        └─ ProcessAutomaticRowSpanning(rows)
│  │
│  └─ Grid re-renders with new AutoSpan mode
```

---

## Prompt Template
<!-- token-budget: 350 words — self-contained for LLM execution -->

```
Mode: {feature-implementation | bug-fix}
Skill: Cell Merging (Row Spanning / Column Spanning)
Scope: Manual merge APIs (MergeCells/UnmergeCells), automatic spanning (AutoSpan modes)

RENDERING APPROACH SUMMARY:

1. **Data Processing** (MergeHandler.Process):
   - Manual merges applied first from ManuallyMergedCells list
   - Automatic column spanning runs (if AutoSpan permits)
   - Automatic row spanning runs (if AutoSpan permits), respecting frozen boundaries
   - Result: All cells have ColSpan/RowSpan/IsSpanned/IsRowSpanned flags set

2. **DOM Rendering** (GridRow.CellRenderer.RenderCell):
   - Skipped cells: IF cell.IsSpanned || cell.IsRowSpanned → no <td> element
   - Anchor cells: Emit <td> with colspan/rowspan/aria-* attributes
   - Frozen borders: e-freezeleftborder / e-freezerightborder classes for spanning cells at boundaries
   - Last row border: e-lastrowcell class if rowspan extends to last visible row

3. **Frozen Column Strategy**:
   - Partition into left-frozen, movable, right-frozen bands
   - Each band processes independently
   - Right-frozen spans in REVERSE order to avoid overlap
   - Frozen border classes applied to edge cells in each band

4. **Frozen Row Strategy**:
   - Split at FrozenRows boundary
   - Frozen rows (0..FrozenRows-1) span independently
   - Scrollable rows (FrozenRows..end) span independently
   - Spans never cross frozen/scrollable boundary

5. **Event Hooks**:
   - QueryCellInfo fires per data cell before rendering
   - User code can override ColSpan/RowSpan in event handler
   - Modified values used in final HTML output

CRITICAL RULES:

✓ Manual merges checked for boundary validity (RowIndex, ColumnIndex, span dimensions)
✓ Grouping: Merges respect ParentUid and RowType boundaries (no cross-group spans)
✓ Exclusion HashSet used for O(1) lookup during auto-span passes
✓ Spanned cells NOT rendered (no stale DOM nodes)
✓ Selection, editing, export all read ColSpan/RowSpan metadata
✓ Paging: RowIndex relative to current page (resets per page)
✓ Sorting: Auto-spans re-run after sort; manual merges do NOT follow reordered rows
✓ Filtering: Auto-spans run on visible (filtered) rows only
✓ Persistence: Manual merges NOT persisted (must be re-applied); auto-spanning re-runs on load

FOR BUG FIXES:
- Check MergeHandler.Process for boundary validation and frozen partition logic
- Verify GridRow.CellRenderer.AddSpanAttributesInternal for colspan/rowspan HTML output
- Check ApplyLastRowBorder for rowspan-aware last-row class application
- Trace Refresh() path to ensure merge module is called in correct order
- Validate ExcludedFromAutomaticSpanning HashSet construction in ProcessRowSpanningInRange

FOR NEW FEATURES:
- Always call MergeModule methods within Refresh() context
- Update ManuallyMergedCells or ExcludedFromAutomaticSpanning lists
- Test with frozen columns: left, right, both, and no frozen
- Test with frozen rows + frozen columns combination
- Test with grouping (caption/data/summary row boundaries)
- Test with virtualization and infinite scroll (row index relativity)
- Test QueryCellInfo event for custom span injection
```

---

**End of Cell Merging Skill**
