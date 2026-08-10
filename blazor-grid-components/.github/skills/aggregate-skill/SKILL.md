---
name: aggregate-skill
description: Expert knowledge for the Aggregate feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to aggregate behaviour, including Footer/GroupFooter/GroupCaption aggregates, aggregate templates, reactive recalculation on filtering/editing/grouping, and cross-feature interaction guarantees with virtualization, grouping, filtering, sorting, paging, editing, selection, and frozen columns.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Aggregates in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

## Agent Invocation
<!-- token-budget: 40 words -->
- Paired custom agent: `/docs/ai-agents/custom-agents/aggregate-agent.md` (create if needed)
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to Aggregates. Do NOT load alongside other feature skills.
- One agent invocation = one feature skill maximum.

---

## Knowledge References
<!-- token-budget: 80 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`

---

## Training Insights Applied
<!-- token-budget: 100 words -->

Key rules and edge cases from `/docs/training/` that directly govern Aggregate work:

- **Aggregate lifecycle is reactive**: Aggregates recalculate in response to state changes (filtering, batch editing, grouping) through the `ReactiveAggregate<T>` module — e.g. `ReactiveAggregate.RefreshFooterAggregate()` (`src/Internal/Actions/Aggregate.cs:28-37`) and `MergeAggregateDataWithBatchChangesAsync()` for batch-edit merges. Custom aggregates resolve via `DataUtil.PerformAggregation()`. Any modification to displayed aggregate values must go through the `ReactiveAggregate<T>` module (`Aggregate.cs`).
- **Three distinct render positions**: Footer (grid-wide aggregate via `FooterContent.razor` line 18), GroupFooter (per-group aggregate via `GroupModelGenerator.GenerateFooterRow()` line 621), GroupCaption (group header aggregate display via `GenerateCaptionRow()` line 323). Each has its own template property and rendering path.
- **`HasAggregateChanges` flag gates updates**: When aggregate configuration changes (`Columns` added/removed/modified), `HasAggregateChanges` must be set to `true` in the component initialization chain.
- **Frozen column handling is mandatory**: Aggregates must respect frozen column boundaries; footer table may split into frozen/unfrozen sections via `e-summary-frozen` wrapper (handled by `FooterContent.razor`).
- **Reactive recalculation must account for filtering, batch editing, and row drag**: `ReactiveAggregate.RefreshFooterAggregate()` (Aggregate.cs line 28) must re-query filtered data, merge batch changes via `MergeAggregateDataWithBatchChangesAsync()`, and re-apply `DataUtil.PerformAggregation()`.
- **Foreign key aggregates require lookup**: When grouping by foreign key column, `CaptionSummaryRenderer.BuildAggregateContext()` must resolve the display value from the foreign key data source.
- **Zero `StateHasChanged()` calls in generators**: `SummaryModelGenerator` (line 22) and `GroupModelGenerator` (line 323) must NOT call `StateHasChanged()` — rendering is controlled by `GridContent.razor` and `RefreshAggregate.razor` lifecycle.

---

## Code Location Map
<!-- token-budget: 100 words -->

All Aggregate-related code lives at these paths (no code reproduced — paths + one-line purpose only):

### Configuration Layer (Public API)
| Path | Purpose |
|------|---------|
| `src/GridAggregate.razor.cs` | Root aggregate container — cascades parent reference, registers with `GridAggregates` |
| `src/GridAggregates.razor.cs` | Collection container — updates parent `IGrid.Aggregates` collection, sets `HasAggregateChanges` |
| `src/GridAggregateColumn.razor.cs` | Per-column config — `Field`, `ColumnName`, `Type` (Sum/Average/Max/Min/Count/TrueCount/FalseCount/Custom), `Format`, three template RenderFragments |
| `src/GridAggregateColumns.razor.cs` | Child collection wrapper — updates parent `GridAggregate.Columns` list |

### Data / Model Layer
| Path | Purpose |
|------|---------|
| `src/EventModels/Grids.cs` | `AggregateTemplateContext` (12 properties: Sum, Average, Max, Min, Count, TrueCount, FalseCount, Custom, Field, ForeignKey, HeaderText, Key) — template context for all three template types |
| `src/EventModels/Grids.cs` | `CaptionTemplateContext` — group caption template context; carries `GroupAggregates: AggregateTemplateContext` |
| `src/Enumeration/GridsEnumerations.cs` | `AggregateType` enum — Sum, Average, Max, Min, Count, TrueCount, FalseCount, Custom |
| `src/Enumeration/GridsEnumerations.cs` | `AggregateTemplateType` enum — GroupCaption, Footer, GroupFooter (used in export) |
| `src/Internal/Base/Enums.cs` | `CellType` enum — defines Summary, CaptionSummary, GroupCaption cell types |
| `src/Internal/Models/Cell.cs` | `Cell<T>` model — carries `AggregateColumn`, `AggregateValue`, `CellType` for rendering dispatch |

### Generation Layer (Row/Cell Model Building)
| Path | Purpose |
|------|---------|
| `src/Internal/Generators/SummaryModelGenerator.cs` (lines 22, 47, 138) | Builds `Row<object>` list for Footer rows via `GeneratorRows()` (line 22); `GenerateCells()` (line 47) populates cells; `GenerateFooterSummary()` (line 138) creates individual aggregate cells with `Cell.AggregateValue` from `SfGrid.Aggregate` dictionary; handles frozen column cell type assignment |
| `src/Internal/Generators/GroupModelGenerator.cs` (lines 323, 369, 439, 621) | Builds GroupCaption and GroupFooter rows: `GenerateCaptionRow()` (line 323) creates caption rows; `GetCaptionRowCells()` (line 369) builds cell list; `GetCaptionRowCell()` (line 439) populates aggregate cells; `GenerateFooterRow()` (line 621) creates group footer rows; decides CellType (CaptionSummary vs Summary); applies aggregate format via `DataUtil.GetFormattedValue()` |

### Rendering Layer (DOM Output)
| Path | Purpose |
|------|---------|
| `src/Internal/Renderer/FooterContent.razor` (line 18) | Renders `<tfoot><tr class="e-summaryrow">` DOM structure (line 18); dispatches on `FooterTemplate` or raw aggregate value; applies frozen column classes (`e-leftfreeze`, `e-rightfreeze`) and inline styles |
| `src/Internal/Renderer/RefreshAggregate.razor` | Wraps `FooterContent` in `<div class="e-gridfooter">`; listens to `RenderAggregate` event from `ReactiveAggregate`; calls `ShouldRenderFooter()` visibility check; triggers `StateHasChanged()` on aggregate updates |
| `src/Internal/Renderer/CaptionSummaryRenderer.cs` | Renders `CellType.CaptionSummary` cells — three paths: `RenderGroupFooterTemplateCell`, `RenderGroupCaptionTemplateCell`, `RenderSimpleAggregateCell`; builds `AggregateTemplateContext` |
| `src/Internal/Renderer/GroupCaptionRenderer.cs` | Renders `CellType.GroupCaption` cell (colspan TD); `GetCaption()` populates `CaptionTemplateContext.GroupAggregates`; resolves foreign key display value |
| `src/Internal/Renderer/SummaryCellRenderer.cs` | Renders `CellType.Summary` empty spacer cells in footer rows (placeholder TDs with no aggregate value) |
| `src/Internal/Renderer/GridRow.razor` | Dispatch hub — routes each `Cell.CellType` to correct render fragment; calls `RenderCaptionSummaryCell`, `RenderGroupCaptionCell`, `RenderSummaryCell` |
| `src/Internal/Renderer/GridContent.razor` | Top-level content table — calls generators and passes `Rows` to `GridRow` for rendering |

### Reactive/Refresh Layer
| Path | Purpose |
|------|---------|
| `src/Internal/Actions/Aggregate.cs` (lines 15, 28, 196) | Contains `ReactiveAggregate<T>` partial class part 1: `UpdateEmptyList()` (line 15), `RefreshFooterAggregate()` (line 28) for footer aggregate refresh logic with data source resolution and filtering operations, `UpdateGroupCaptionFooterAggregates()` (line 196) for group-level aggregate recalculation |
| `src/Internal/Actions/ReactiveAggregate.cs` (line 12) | Partial class part 2 — `ReactiveAggregate<T>` class declaration (line 12); contains group caption/footer aggregate updates, batch edit merging via `UpdateCollection()`, aggregate building via `BuildAggregateList()`, and event triggering |
| `src/SfGrid.Lifecycle.cs` | Instantiates `ReactiveAggregateModule` in `OnInitializedAsync()` |
| `src/SfGrid.Properties.cs` | Exposes `Aggregate` (computed dictionary), `Aggregates` (configuration list), `HasAggregateChanges` flag |
| `src/SfGrid.Methods.cs` | Public methods that may trigger aggregate refresh (data refresh, filter apply, batch save) |

### JavaScript / Scroll Sync
| Path | Purpose |
|------|---------|
| `scripts/scroll.ts` (lines 123, 281, 284, 286) | **TypeScript source** — Footer div scroll sync: queries `.e-gridfooter` (line 123); syncs `footer.scrollLeft` with content scroll (lines 281, 284, 286) to keep footer horizontally aligned with content table during horizontal scrolling |
| `scripts/freeze.ts` | **TypeScript source** — Frozen column handling: footer table splits into left/right frozen sections when `FrozenColumns` or `FrozenRightColumns` set; freeze line position affects footer layout rendering and scroll behavior |

---

## DOM Structure & Rendering Approach

<!-- token-budget: 200 words — ASCII diagrams and rendering flow documentation -->

### ASCII Diagram 1: Grid Layout with Footer Aggregate (No Grouping)

```
┌────────────────────────────────────────────────────────────────┐
│ SfGrid<T> (e-grid)                                             │
├────────────────────────────────────────────────────────────────┤
│ ┌──────────────────────────────────────────────────────────┐   │
│ │ GridHeader (e-grid-header)                               │   │
│ │  table#Grid_header_table                                 │   │
│ │    thead: column names, sort indicators                  │   │
│ └──────────────────────────────────────────────────────────┘   │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │ GridContent (e-gridcontent)                              │   │
│ │  table#Grid_content_table                                │   │
│ │    tbody: data rows (Row objects with CellType.Data)     │   │
│ └──────────────────────────────────────────────────────────┘   │
│ ┌──────────────────────────────────────────────────────────┐   │
│ │ RefreshAggregate (e-gridfooter wrapper)                  │   │
│ │  ┌────────────────────────────────────────────────────┐  │   │
│ │  │ FooterContent                                      │  │   │
│ │  │  div.e-summarycontent [+ e-summary-frozen]        │  │   │
│ │  │    table#Grid_footer_table                         │  │   │
│ │  │      colgroup (WidthController)                    │  │   │
│ │  │      tbody (empty — HTML spec)                     │  │   │
│ │  │      tfoot (rowgroup)                              │  │   │
│ │  │        tr.e-summaryrow ← One per GridAggregate    │  │   │
│ │  │          td.e-indentcelltop (grouped indent)       │  │   │
│ │  │          td.e-summarycell (no value)               │  │   │
│ │  │          td.e-summarycell.e-templatecell           │  │   │
│ │  │            {FooterTemplate render}                 │  │   │
│ │  │          td.e-summarycell [e-leftfreeze]           │  │   │
│ │  │            {aggregate value or FooterTemplate}     │  │   │
│ │  │          ...                                        │  │   │
│ │  └────────────────────────────────────────────────────┘  │   │
│ └──────────────────────────────────────────────────────────┘   │
└────────────────────────────────────────────────────────────────┘
```

### ASCII Diagram 2: Grid Layout with Grouping (GroupCaption + GroupFooter)

```
┌─────────────────────────────────────────────────────────────┐
│ SfGrid<T> with Grouping Enabled (e-grid)                   │
├─────────────────────────────────────────────────────────────┤
│ GridContent (e-gridcontent)                                 │
│  table#Grid_content_table                                   │
│    tbody                                                    │
│      tr[RowType=GroupCaption] ← GroupCaptionRenderer       │
│      │  td.e-indentcell (group level indent)              │
│      │  td.e-recordplusexpand (expand/collapse icon)      │
│      │  td[colspan=N].e-groupcaption ← GroupCaption cell  │
│      │    "Department: Sales - 10 items"                  │
│      │    OR {GroupSettings.CaptionTemplate render}        │
│      │  td.e-summarycell ← CaptionSummary cell            │
│      │    {GroupCaptionTemplate OR aggregate value}       │
│      │                                                     │
│      tr[IsDataRow=true] ← Data row 1                       │
│      │  td: Indent cells                                   │
│      │  td: Column 1 data                                  │
│      │  td: Column 2 data                                  │
│      │  ...                                                │
│      │                                                     │
│      tr[IsDataRow=true] ← Data row 2                       │
│      │  td: Indent cells                                   │
│      │  td: Column 1 data                                  │
│      │  td: Column 2 data                                  │
│      │  ...                                                │
│      │                                                     │
│      tr[RowType=Summary] ← GroupFooter (GroupModelGen)    │
│      │  td.e-indentcelltop (group level indent)          │
│      │  td.e-summarycell ← CaptionSummary cell           │
│      │    {GroupFooterTemplate OR aggregate value}       │
│      │  ...                                                │
│      │                                                     │
│      tr[RowType=GroupCaption] ← Next group...             │
│      tr[IsDataRow=true]                                    │
│      tr[IsDataRow=true]                                    │
│      tr[RowType=Summary] ← GroupFooter                    │
│      ...                                                   │
│                                                            │
│ ┌────────────────────────────────────────────────────────┐ │
│ │ RefreshAggregate (e-gridfooter) ← Grid-wide Footer   │ │
│ │  FooterContent → table → tfoot → tr.e-summaryrow     │ │
│ │    Grid-wide totals/aggregates                        │ │
│ └────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

### ASCII Diagram 3: Cell Type Dispatch Flow in GridRow.razor

```
GridRow.razor receives Row<object> and Cells[]
  │
  ├─ For each Cell<object> in Cells:
  │
  ├─ Determine CellType:
  │   ├─ CellType.Data → RenderCell() (normal data cell)
  │   ├─ CellType.Expand → RenderExpandCell()
  │   ├─ CellType.GroupCaption → RenderGroupCaptionCell()
  │   │   └─ GroupCaptionRenderer.RenderGroupCaptionCell()
  │   │       ├─ GetCaption() → CaptionTemplateContext + GroupAggregates
  │   │       ├─ If GroupSettings.CaptionTemplate exists:
  │   │       │   └─ Render template with CaptionTemplateContext
  │   │       └─ Else:
  │   │           └─ Render: "{ColName}: {Key} - {N} items"
  │   │
  │   ├─ CellType.CaptionSummary → RenderCaptionSummaryCell()
  │   │   └─ CaptionSummaryRenderer.RenderCaptionSummaryCell()
  │   │       ├─ BuildAggregateContext() → AggregateTemplateContext
  │   │       ├─ Branch on Cell.AggregateColumn.Type:
  │   │       │   ├─ If GroupFooterTemplate exists:
  │   │       │   │   └─ RenderGroupFooterTemplateCell()
  │   │       │   │       └─ Render template with context
  │   │       │   ├─ Else if GroupCaptionTemplate exists:
  │   │       │   │   └─ RenderGroupCaptionTemplateCell()
  │   │       │   │       └─ Render template with context
  │   │       │   └─ Else:
  │   │       │       └─ RenderSimpleAggregateCell()
  │   │       │           └─ Render Cell.AggregateValue as text
  │   │
  │   ├─ CellType.Summary → RenderSummaryCell()
  │   │   └─ SummaryCellRenderer.RenderSummaryCell()
  │   │       └─ Render empty <td> (spacer cell)
  │   │
  │   ├─ CellType.Indent → RenderIndentCell()
  │   ├─ CellType.DetailIndent → RenderIndentCell()
  │   ├─ CellType.RowDrag → RenderRowDragCell()
  │   ├─ CellType.Detail → RenderDetailCell()
  │   └─ CellType.CommandColumn → RenderCommandColumnCell()
  │
  └─ Combine all rendered cells into <tr> element
```

---

## JavaScript Interop in Aggregate Footer Scroll Sync
<!-- token-budget: 80 words -->

**JavaScript interop is used for footer scroll synchronization only.** Aggregate rendering itself is pure Blazor/C#.

**TypeScript Functions (scripts/scroll.ts):**

| Location | Function | Purpose |
|----------|----------|---------|
| Line 123 | Query `.e-gridfooter` | Locates footer DOM element for scroll sync |
| Lines 281, 284, 286 | `footer.scrollLeft = left` | Synchronizes footer horizontal scroll with content table scroll position |

**Responsibility Split:**

| Component | Handles |
|-----------|---------|
| **TypeScript (scroll.ts)** | Horizontal scroll synchronization between `.e-gridfooter` and `.e-gridcontent` |
| **Blazor (FooterContent.razor)** | Aggregate value rendering, template rendering, frozen column DOM structure |
| **Blazor (ReactiveAggregate.cs)** | Aggregate computation, data filtering, batch edit merging, event triggering |

**CRITICAL Rules:**
- DO NOT modify footer DOM structure from TypeScript (scroll sync only)
- DO NOT recalculate aggregates in JavaScript (Blazor owns computation)
- DO await scroll sync completion before programmatic scrolling
- DO preserve `e-gridfooter` class for scroll sync to work

---

### ASCII Diagram 4: Aggregate Data Flow (Reactive Recalculation)

```
┌─────────────────────────────────────────────────────────────────┐
│ Data Modification Event (load / filter / batch edit / drag)   │
└──────────────────────────────────┬──────────────────────────────┘
                                   │
                    ┌──────────────▼──────────────┐
                    │ ReactiveAggregate.cs Module │
                    └──────────────┬──────────────┘
                                   │
                    ┌──────────────▼──────────────────────┐
                    │ RefreshFooterAggregate()            │
                    │ • Determine data source:            │
                    │   - Remote: use CurrentViewData     │
                    │   - Local: Execute DataManager.Query │
                    │ • Apply filtering if active         │
                    │ • Merge batch changes:              │
                    │   - UpdateCollection()              │
                    │   - Merge ChangedRecords            │
                    │   - Add AddedRecords                │
                    │   - Remove DeletedRecords           │
                    └──────────────┬──────────────────────┘
                                   │
                    ┌──────────────▼──────────────────────┐
                    │ DataUtil.PerformAggregation()       │
                    │ • For each GridAggregateColumn:     │
                    │   - Field: {Field}                  │
                    │   - Type: {Sum|Average|Max|Min|...} │
                    │ • Compute aggregate value           │
                    │ • Store in dict: key = "{Field} - {Type}" │
                    └──────────────┬──────────────────────┘
                                   │
                    ┌──────────────▼──────────────────────┐
                    │ SfGrid.Aggregate = result dict      │
                    │ EventAggregator.Trigger(             │
                    │   "RenderAggregate"                 │
                    │ )                                    │
                    └──────────────┬──────────────────────┘
                                   │
                    ┌──────────────▼──────────────────────┐
                    │ RefreshAggregate.razor listens      │
                    │ • StateHasChanged()                 │
                    │ • FooterContent re-renders          │
                    │ • SummaryModelGenerator.GeneratorRows():
                    │   - Creates Row<object> from        │
                    │     GridAggregates                  │
                    │   - Populates Cell.AggregateValue   │
                    │     from SfGrid.Aggregate dict      │
                    └──────────────┬──────────────────────┘
                                   │
                    ┌──────────────▼──────────────────────┐
                    │ FooterContent.razor renders         │
                    │ <table><tfoot>                      │
                    │ For each Row in Rows:               │
                    │   For each Cell in Row.Cells:       │
                    │     if FooterTemplate: render it    │
                    │     else: render AggregateValue     │
                    └──────────────┬──────────────────────┘
                                   │
                    ┌──────────────▼──────────────────────┐
                    │ DOM Update: Footer summary row      │
                    │ displays new aggregate values       │
                    └──────────────────────────────────────┘
```

### ASCII Diagram 5: Frozen Column Aggregate Rendering

```
Without Frozen Columns:
┌─────────────────────────────────────────────────────┐
│ RefreshAggregate                                    │
│  div.e-summarycontent                              │
│    table#Grid_footer_table                          │
│      tfoot                                          │
│        tr.e-summaryrow (all columns in one table)   │
│          td (Col1 aggregate)                        │
│          td (Col2 aggregate)                        │
│          td (Col3 aggregate)                        │
└─────────────────────────────────────────────────────┘

With Frozen Columns (FrozenColumns=2):
┌──────────────────────────────────────────────────────────┐
│ RefreshAggregate                                         │
│  div.e-summarycontent.e-summary-frozen                  │
│    ┌─────────────────────┐  ┌──────────────────────────┐ │
│    │ Frozen Footer       │  │ Unfrozen Footer         │ │
│    │ table#Grid_footer   │  │ table#Grid_footer       │ │
│    │  tfoot              │  │  tfoot                  │ │
│    │    tr               │  │    tr                   │ │
│    │      td.e-leftfreeze│  │      td (normal)        │ │
│    │      td.e-leftfreeze│  │      td (normal)        │ │
│    └─────────────────────┘  │      td (normal)        │ │
│                              └──────────────────────────┘ │
│  JS scroll sync: both tables scroll together horizontally│
└──────────────────────────────────────────────────────────┘
```

---

## Rendering Approaches by Aggregate Type

### Approach 1: Footer Aggregates (No Template)

**Trigger**: `RefreshAggregate.razor` → `FooterContent.razor`

**Flow**:
1. `SummaryModelGenerator.GeneratorRows()` creates `Row<object>` list from `SfGrid.Aggregates`
2. For each `GridAggregate`:
   - `GenerateCells()` iterates visible columns
   - For columns WITH aggregate mapping: `GenerateFooterSummary()` creates `Cell` with `AggregateValue`
   - For columns WITHOUT: creates empty `Cell` with `CellType.Summary`
3. `FooterContent.razor` loops through rows/cells:
   - If `Cell.AggregateColumn.FooterTemplate` is set: render the template
   - Else if `Cell.AggregateValue` is not null: render the value
   - Else: render empty space

**Key properties**:
- `GridAggregateColumn.FooterTemplate: RenderFragment<object>`
- `GridAggregateColumn.Format: string` (applied via `DataUtil.GetFormattedValue()`)
- Cell class: `e-summarycell` + optional `e-templatecell` (if template used)

---

### Approach 2: GroupFooter Aggregates (Per-Group Aggregate Row)

**Trigger**: `GroupModelGenerator.GenerateFooterRow()` inside `GetGroupedRecords()`

**Flow**:
1. After generating data rows for a group, `GenerateFooterRow()` is called:
   - `GetCaptionRowCell()` with `isCaption = false`
   - Populates aggregate cells via `GenerateFooterSummary()` / `GenerateCaptionSummary()`
2. `GroupModelGenerator.GetCaptionRowCell()` populates `Cell.AggregateValue` from `(data as Group<T>).Aggregates` dictionary
3. Cell rendering in `GridRow.razor`:
   - `CellType.CaptionSummary` → `RenderCaptionSummaryCell()`
   - Calls `RenderGroupFooterTemplateCell()` if `GroupFooterTemplate` exists
   - Else `RenderSimpleAggregateCell()` to render the value

**Key properties**:
- `GridAggregateColumn.GroupFooterTemplate: RenderFragment<object>`
- Row type: `"Summary"` (RowType property)
- Cell class: `e-summarycell` or `e-summarycell e-templatecell`
- Row visibility: affected by group expand/collapse state

---

### Approach 3: GroupCaption Aggregates (Group Header Display)

**Trigger**: `GroupCaptionRenderer.RenderGroupCaptionCell()` → `GetCaption()`

**Flow**:
1. Group caption row is generated by `GroupModelGenerator.GenerateCaptionRow()`
2. `RenderGroupCaptionCell()` in `GridRow.razor` calls `GetCaption()`:
   - Builds `CaptionTemplateContext` with `Field`, `Key`, `Count`, `HeaderText`
   - If `GroupSettings.CaptionTemplate` exists: calls `GetFirstAggregate()` to get first aggregate value
   - Populates `GroupAggregates` property with `AggregateTemplateContext`
3. If `GroupSettings.CaptionTemplate` is set:
   - Render the template with `CaptionTemplateContext` (which includes `GroupAggregates`)
4. Else:
   - Render default caption: `"{ColumnName}: {GroupValue} - {Count} {items|item}"`

**Key properties**:
- `GroupSettings.CaptionTemplate: RenderFragment<CaptionTemplateContext>`
- `CaptionTemplateContext.GroupAggregates: AggregateTemplateContext`
- Cell type: `CellType.GroupCaption`
- Cell class: `e-groupcaption` with colspan spanning all visible columns

---

### Approach 4: GroupCaption with GroupCaptionTemplate (Per-Column Aggregate in Caption)

**Trigger**: `CaptionSummaryRenderer.RenderCaptionSummaryCell()` → `RenderGroupCaptionTemplateCell()`

**Flow**:
1. `GroupModelGenerator.GetCaptionRowCell()` checks `isCaption = true` and `GroupCaptionTemplate != null`
2. Populates aggregate cells via `GenerateCaptionSummary()`
3. Cell rendering in `GridRow.razor`:
   - `CellType.CaptionSummary` → `RenderCaptionSummaryCell()`
   - Calls `RenderGroupCaptionTemplateCell()` if template exists
4. `RenderGroupCaptionTemplateCell()`:
   - Builds `AggregateTemplateContext` from `Cell.AggregateValue`
   - Renders template with context

**Key properties**:
- `GridAggregateColumn.GroupCaptionTemplate: RenderFragment<object>`
- Template receives: Sum, Average, Max, Min, Count, TrueCount, FalseCount, Custom (one per aggregate type)
- Cell class: `e-summarycell e-templatecell`

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 180 words -->

> Built from live code analysis + `/docs/training/` risk assessment.  
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|----------------|------|
| Aggregate + Grouping | Group footer rows generated with aggregates via `GroupModelGenerator.GenerateFooterRow()`; group caption may display first aggregate value; `ReactiveAggregate.UpdateGroupCaptionFooterAggregates()` recalcs group totals on batch edit. | **Critical** |
| Aggregate + Frozen Columns | Footer table splits via `e-summary-frozen` wrapper; `FooterContent` receives `IsFrozen`/`IsFrozenRight` params; `GetStyles()` and `GetFrozenClass()` apply frozen column styles. | **Critical** |
| Aggregate + Filtering | `ReactiveAggregate.RefreshFooterAggregate()` re-queries filtered data via `GetFilteredRecordsAsync()`; aggregate computed on filtered records only; remote adaptor uses `CurrentViewData`. | **Critical** |
| Aggregate + Batch Editing | `ReactiveAggregate.UpdateCollection()` merges batch changes (ChangedRecords, AddedRecords, DeletedRecords) before aggregation; aggregate recalc reflects unsaved edits. | **High** |
| Aggregate + Virtualization | Footer is NOT virtualized — always renders full grid aggregate; group footer rows must remain visible in virtual viewport; group caption aggregate display unaffected. | **High** |
| Aggregate + Paging | Footer aggregate reflects FULL dataset (not current page) for local data; remote data uses server-aggregated result in `CurrentViewData`. | **High** |
| Aggregate + Infinite Scroll | `ReactiveAggregate` uses `CurrentViewData` path; aggregate recalc triggered on each scroll load. | **High** |
| Aggregate + Column Visibility | `ShouldRenderFooter()` checks column visibility; `SummaryModelGenerator` filters aggregate cells by visible columns only; hidden columns omitted from footer. | **High** |
| Aggregate + Row Drag-and-Drop | `SummaryModelGenerator.GenerateCells()` adds `CellType.RowDrag` indent cell before aggregate cells if drag enabled; drag does not affect aggregate values. | **Medium** |
| Aggregate + Detail Row | `SummaryModelGenerator` adds `CellType.DetailIndent` cell before aggregate cells if detail template exists; detail row expand/collapse does not affect aggregates. | **Medium** |
| Aggregate + Foreign Key Column | `BuildAggregateContext()` resolves foreign key display value via column data source; aggregate computed on underlying field value (not display text). | **Medium** |
| Aggregate + Sorting | Sort does not affect aggregate computation — aggregates are data-level, not order-level; no special handling required. | **Low** |
| Aggregate + Selection | Selection does not affect aggregates; aggregate values remain static regardless of selected rows. | **Low** |
| Aggregate + Editing (Normal/Dialog Mode) | Open edit does not clear aggregates; aggregate recalc on save triggers refresh. | **Low** |

---

## Prompt Template
<!-- token-budget: 320 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Aggregate
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Aggregate feature.
Scope: Aggregate configuration, rendering, and reactive recalculation only.

=== WHAT YOU MUST KNOW (pre-loaded) ===

**Component Tree**:
- GridAggregates → GridAggregate → GridAggregateColumns → GridAggregateColumn
- GridAggregateColumn carries THREE templates: FooterTemplate, GroupCaptionTemplate, GroupFooterTemplate

**Rendering Positions** (Four types):
1. Footer (grid-wide aggregate) — rendered by FooterContent.razor
2. GroupFooter (per-group aggregate) — rendered by CaptionSummaryRenderer.RenderGroupFooterTemplateCell
3. GroupCaption (group header) — rendered by GroupCaptionRenderer.RenderGroupCaptionCell
4. GroupCaption aggregate cells — rendered by CaptionSummaryRenderer.RenderGroupCaptionTemplateCell

**Template Contexts**:
- AggregateTemplateContext (12 properties: Sum, Average, Max, Min, Count, TrueCount, FalseCount, Custom, Field, ForeignKey, HeaderText, Key)
- CaptionTemplateContext (carries GroupAggregates: AggregateTemplateContext)

**Cell Types Used**:
- CellType.Summary (empty spacer cell in footer)
- CellType.CaptionSummary (aggregate cell with value or template)
- CellType.GroupCaption (group header cell with colspan)

**Model Generators**:
- SummaryModelGenerator.GeneratorRows() — builds footer rows from SfGrid.Aggregates
- GroupModelGenerator.GenerateFooterRow() / GetCaptionRowCell() — builds group footer/caption aggregate cells

**Reactive Recalculation** (ReactiveAggregate<T> module):
- RefreshFooterAggregate() — re-queries filtered data, merges batch edits, calls DataUtil.PerformAggregation()
- UpdateGroupCaptionFooterAggregates() — recalcs group-level aggregates on batch edit
- Result stored in SfGrid.Aggregate dictionary

**Aggregate Types** (AggregateType enum):
- Sum, Average, Max, Min, Count, TrueCount, FalseCount, Custom

**Frozen Column Handling**:
- FooterContent receives IsFrozen/IsFrozenRight parameters
- Frozen footer splits into left/right sections with e-summary-frozen wrapper class
- Styles applied via GetFrozenClass() and GetStyles()

**Event Flow**:
- Data modified → ReactiveAggregate.RefreshFooterAggregate()
- DataUtil.PerformAggregation() computes values
- EventAggregator.Trigger("RenderAggregate")
- RefreshAggregate.razor listens and calls StateHasChanged()
- FooterContent re-renders with new Cell.AggregateValue

=== BEFORE YOU MAKE ANY CHANGE ===

1. Identify which rendering path is affected:
   - Footer? → Check FooterContent.razor, SummaryModelGenerator, RefreshAggregate.razor
   - GroupFooter? → Check GroupModelGenerator.GenerateFooterRow(), CaptionSummaryRenderer.RenderGroupFooterTemplateCell
   - GroupCaption? → Check GroupCaptionRenderer.RenderGroupCaptionCell, GetCaption()
   - GroupCaption aggregate cell? → Check CaptionSummaryRenderer.RenderGroupCaptionTemplateCell

2. Check affected feature interactions (use Interaction Matrix above):
   - Does change affect Grouping, Frozen, Filtering, Batch Editing, or Virtualization?
   - If yes, verify no breakage in those areas

3. Verify reactive recalculation:
   - Does data change? → ReactiveAggregate.RefreshFooterAggregate() must trigger
   - Does GroupCaption need recalc? → UpdateGroupCaptionFooterAggregates() must be called
   - Are aggregate values properly populated into Cell.AggregateValue?

4. Test frozen column rendering if applicable:
   - Check e-summary-frozen wrapper class is applied correctly
   - Verify scroll sync between left/right footer tables

5. Validate no direct StateHasChanged() calls in generators:
   - SummaryModelGenerator and GroupModelGenerator must NOT call StateHasChanged()
   - Only RefreshAggregate.razor and event-triggered re-renders should call it
```

---

## Additional Context

### Common Pitfalls

1. **Forgetting reactive recalculation**: Changing data (filter, batch edit) requires `ReactiveAggregate.RefreshFooterAggregate()` to re-compute aggregates. Without this, footer displays stale values.

2. **Confusing CellType.Summary with CellType.CaptionSummary**:
   - `CellType.Summary` = empty placeholder cell (no aggregate value)
   - `CellType.CaptionSummary` = cell with actual aggregate value or template

3. **Foreign key aggregate lookup failure**: When grouping by foreign key, `BuildAggregateContext()` must resolve the display text from the column data source, not just use the key value.

4. **Frozen column style loss**: If `GetFrozenClass()` or `GetStyles()` is not called, frozen footer cells lose their positioning styles.

5. **GroupCaption template context missing GroupAggregates**: `CaptionTemplateContext` must be populated with `GroupAggregates` property, or template cannot access aggregate values.

6. **Batch edit aggregate not reflecting unsaved changes**: `UpdateCollection()` must merge batch changes BEFORE calling `DataUtil.PerformAggregation()`, or aggregate reflects pre-edit state.

7. **Virtualization footer not visible**: Footer is rendered outside the virtual scroll container and should always be visible. Ensure `RefreshAggregate.razor` is NOT inside a virtual scroll wrapper.

### Testing Scenarios

- **Scenario 1**: Add footer aggregate to a grid. Verify footer row renders with correct total.
- **Scenario 2**: Enable grouping with group footer aggregates. Verify each group shows sub-total. Verify grid-wide footer shows grand total.
- **Scenario 3**: Apply filter. Verify footer recalc reflects filtered data only.
- **Scenario 4**: Edit a cell in batch mode. Verify footer aggregate reflects unsaved value (if reactive recalc is implemented).
- **Scenario 5**: Freeze columns. Verify footer table splits correctly and scroll sync works.
- **Scenario 6**: Enable foreign key column grouping. Verify group caption displays foreign key text (not key value) in aggregate display.

---
