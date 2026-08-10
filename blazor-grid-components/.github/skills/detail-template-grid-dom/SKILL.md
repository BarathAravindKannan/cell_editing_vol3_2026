---
name: detail-template-grid-dom
description: Expert knowledge for Syncfusion Blazor DataGrid Detail Template DOM structure, rendering flow, expand and collapse lifecycle, row and cell composition, and feature-combination behavior. Use this skill when implementing, documenting, or debugging detail template rendering, detail rows, indent cells, expand icons, grouping interactions, virtualization interactions, and keyboard or focus behavior.
---

# Detail Template Grid DOM Structure Skill
<!-- token-budget: 24 words -->

**Purpose**  
Authoritative reference for the Blazor render tree and DOM output produced by Grid `DetailTemplate` in the Syncfusion Blazor DataGrid. Covers how the parent data row renders its detail toggle cell, how the detail row is inserted into `Parent.Rows`, how `GridDetailRow` renders the detail DOM, and how grouping, selection, virtualization, row drag, paging, RTL, frozen columns, and keyboard navigation alter the final structure.

---

## Knowledge References
<!-- token-budget: 120 words -->

### Primary source files
- `src/GridTemplates.cs` — Defines `GridTemplates.DetailTemplate` as `RenderFragment<object>`
- `src/Internal/SfGrid.razor` — Root component; chooses `GridContent` or `GridVirtualContent`
- `src/SfGrid.razor.cs` — Declares `DetailRowModule`; exposes `hasDetailTemplate` to client options; contains `GridTemplates.DetailTemplate != null` gates
- `src/SfGrid.Lifecycle.cs` — Initializes `DetailRowModule = new DetailRow<TValue>(this)`
- `src/Internal/Actions/DetailRow.cs` — Core expand or collapse logic; creates detail rows and cells (line 100: row model creation; line 204: GenerateDetailCellsForRow; line 341: JS interop for focus)
- `src/Internal/Renderer/GridContent.razor` — Main `<tbody>` loop; decides when `GridDetailRow` is rendered (line 68-70: detail row conditional render)
- `src/Internal/Renderer/GridRow.razor` — Renders normal data rows and dispatches `CellType.Detail` / `CellType.DetailIndent`
- `src/Internal/Renderer/GridRow.razor.cs` — Row lifecycle, `GetCells()`, row attributes, class and style assembly
- `src/Internal/Renderer/GridDetailRow.razor` — Renders `<tr class="e-detailrow">` and the `<th class="e-detailcell">` (line 23: row element; line 51: detail host cell)
- `src/Internal/Renderer/DetailCellRenderer.cs` — Renders the parent row detail toggle `<td>` (line 20: RenderDetailCell method)
- `src/Internal/Renderer/IndentCellRenderer.cs` — Renders indent cells and detail indent header cells
- `src/Internal/Renderer/ExpandCellRenderer.cs` — Group caption expand / collapse cell, relevant for grouping + detail template combinations
- `src/Internal/Base/InternalClass.cs` — `GridContentParameters`, `GridRowParameters`, `GridCellParameters`
- `src/GridEvents.cs` — Detail lifecycle events (line 1566: DetailDataBound; line 1593: DetailsExpanding)
- `src/EventModels/Grids.cs` — Detail event argument models (line 1978: DetailDataBoundEventArgs; line 1994: DetailsExpandingEventArgs; line 2032: DetailsCollapsingEventArgs)

### Secondary cross-feature references
- `src/Internal/Actions/Group.cs` — Grouping interactions
- `src/Internal/Actions/Selection.cs` — Selection interactions
- `src/Internal/Actions/VirtualScroll.cs` — Virtual refresh path
- `src/Internal/Actions/FocusHandler.cs` — Keyboard and focus routing
- `src/Internal/Renderer/GridVirtualContent.razor` — Virtual rendering mode
- `src/Internal/Renderer/RowDragCellRenderer.cs` — Row drag structural offset
- `scripts/sf-grid.ts` — Client-side detail-template awareness through `hasDetailTemplate` (line 454: focusDetailTemplateElements)
- `scripts/sf-grid-fn.ts` — Client-side DOM interaction helpers

---

## Core Training Insights
<!-- token-budget: 180 words -->

- **Detail template rendering is two-row based** — the visible data record first renders as a normal row, then an additional detail row is inserted immediately after it when expanded.
- **The detail toggle belongs to the parent row, not the detail row** — the toggle UI is rendered by `RenderDetailCell()` in `DetailCellRenderer.cs`.
- **The actual detail content belongs to a separate row** — rendered by `GridDetailRow.razor` as `<tr class="e-detailrow">`.
- **The detail row is model-driven** — it is not direct DOM insertion. `DetailRow<T>.DetailClick()` mutates `Parent.Rows`, inserting or removing a `Row<object>` with `RowType = "DetailRow"` and `IsDetailRow = true`.
- **Detail DOM appears only when both conditions hold**: the parent row is expanded and the generated detail row is visible in the `Rows` collection.
- **Indent structure is additive** — grouping indent cells, detail indent cell, row drag offsets, and visible-column colspan all combine to form the final detail row shape.
- **The detail content cell is rendered as `<th>`** — not `<td>`. `GridDetailRow.razor` emits `<th class="e-detailcell" colspan="..." scope="col">` for the actual template host.
- **Feature combinations do not replace detail rendering logic** — they adjust row generation, visible cells, colspan, visibility, or event routing around the same core flow.
- **Virtualization does not change the detail row contract** — it changes when content re-renders, not how the detail row model is structured.
- **Keyboard behavior is centralized** — `BeforeCellFocus` and `FocusModule.ProcessKeyDown()` route expand and collapse behavior.

---

## Rendering Entry Points
<!-- token-budget: 220 words -->

### Entry Point 1: Template declaration
`GridTemplates.DetailTemplate` is declared in `src/GridTemplates.cs`.

```razor
<SfGrid DataSource="@Data">
    <GridTemplates>
        <DetailTemplate>
            <!-- custom nested grid / custom HTML / forms / layout -->
        </DetailTemplate>
    </GridTemplates>
</SfGrid>
```

This means the grid has an available render fragment:

- `((IGrid)Parent).GridTemplates.DetailTemplate != null`
- client option `hasDetailTemplate = true`
- detail cell and detail row generation paths become active

### Entry Point 2: module initialization
`src/SfGrid.Lifecycle.cs`

- `DetailRowModule = new DetailRow<TValue>(this);`

This creates the action module that owns:
- row insertion and removal
- keyboard expand and collapse handling
- expand all / collapse all logic
- detail event sequencing

### Entry Point 3: parent content rendering
`src/Internal/SfGrid.razor`

- normal mode → `GridContent`
- virtualization mode → `GridVirtualContent`

Both eventually iterate `Parent.Rows` and render either:
- `GridRow` for normal data rows
- `GridDetailRow` for inserted detail rows

### Entry Point 4: user interaction
User clicks or keyboard-focuses the detail cell in the parent row.

`DetailCellRenderer.cs`:
- clears focus
- focuses target cell
- invokes `DetailCellClickHandlerInternal()`
- calls `Parent.DetailRowModule.DetailClick(row.Index, row.Uid)`

### Entry Point 5: detail row model generation
`DetailRow<T>.DetailClick()`:
- finds clicked row
- raises pre-expand or pre-collapse event
- creates or removes detail row model
- updates `Row.IsExpand`
- triggers content refresh / virtual update
- raises post event

---

## End-to-End Rendering Flow
<!-- token-budget: 260 words -->

```
DetailTemplate declared
    ↓
Grid initializes DetailRowModule
    ↓
RowModelGenerator / GroupModelGenerator creates normal parent rows
    ↓
Each parent row contains a CellType.Detail cell when DetailTemplate exists
    ↓
GridRow.razor loops through Row.Cells
    ↓
CellType.Detail → RenderDetailCell(cellParams)
    ↓
<td class="e-detailrowcollapse | e-detailrowexpand ...">
    <a class="e-icons ..."></a>
</td>
    ↓
User clicks detail cell or keyboard action triggers expand
    ↓
DetailRow<T>.DetailClick(parentRow.Index, parentRow.Uid)
    ↓
Create new Row<object>:
    RowType      = "DetailRow"
    IsDetailRow  = true
    IsExpand     = true
    Data         = parent row data
    Indent       = group column count
    Cells        = GenerateDetailCellsForRow(detailRow, visibleColumnsCount)
    ↓
Insert detail row into Parent.Rows immediately after clicked row
    ↓
Trigger ContentStateChanged or VirtualComponentUpdate
    ↓
GridContent / GridVirtualContent iterates Rows
    ↓
When Row.IsDetailRow == true and Row.IsExpand == true
    ↓
Render <GridDetailRow>
    ↓
GridDetailRow.razor renders:
<tr class="e-detailrow" data-uid="...">
    indent cells / detail indent cells / row drag cells if applicable
    <th class="e-detailcell" colspan="...">
        <div>
            @DetailTemplate(Row.Data)
        </div>
    </th>
</tr>
```

---

## Base DOM Structure
<!-- token-budget: 500 words -->

### 1. Standard grid shell with detail template capability

```
<div id="Grid" class="e-grid ..." role="grid | treegrid">
├── .e-gridheader
│   └── .e-headercontent
│       └── header table
│
├── .e-gridcontent
│   └── .e-content
│       └── content renderer
│           └── <table class="e-table" id="Grid_content_table" role="presentation">
│               ├── <WidthController>
│               └── <tbody role="rowgroup">
│                   ├── parent data row 1      ← GridRow
│                   ├── detail row for row 1   ← GridDetailRow (only when expanded)
│                   ├── parent data row 2
│                   ├── detail row for row 2
│                   └── ...
│
└── optional pager / chooser / menu / dialogs / tooltip
```

### 2. Parent row with detail toggle cell

```
<tr class="e-row ..." role="row" data-uid="parent-row-uid">
├── optional grouping indent <td class="e-indentcell"></td>
├── optional row drag        <td class="e-rowdragdrop"></td>
├── detail toggle            <td class="e-detailrowcollapse | e-detailrowexpand ..."
│                               aria-expanded="false | true"
│                               tabindex="0"
│                               data-uid="detail-cell-uid">
│                               <a class="e-icons e-dtdiagonalright e-icon-grightarrow"></a>
│                            </td>
├── data cell 1              <td class="e-rowcell">...</td>
├── data cell 2              <td class="e-rowcell">...</td>
└── ...
</tr>
```

### 3. Expanded detail row

```
<tr class="e-detailrow" data-uid="detail-row-uid">
├── optional grouping indent <td class="e-indentcell"></td>
├── optional detail indent   <th class="e-detailindentcell" scope="col"></th>
├── optional row drag offset <td class="e-rowdragdrop"></td>   [indirectly affects colspan logic]
└── detail template host     <th class="e-detailcell"
                                 colspan="visible columns ± feature offsets"
                                 tabindex="0"
                                 data-uid="detail-data-cell-uid"
                                 scope="col">
                                 <div>
                                     DetailTemplate(Row.Data)
                                 </div>
                              </th>
</tr>
```

### 4. Collapsed state
When collapsed:
- the parent row still exists
- the detail toggle cell class becomes `e-detailrowcollapse`
- the detail row is removed from `Parent.Rows`
- no `GridDetailRow` DOM exists for that record

---

## Row Model Structure
<!-- token-budget: 220 words -->

### Parent row model
A normal data row already exists in `Parent.Rows`.

Relevant state:
- `Row.IsDataRow = true`
- `Row.IsExpand = false | true`
- `Row.Cells` includes a `CellType.Detail` cell when detail template is available

### Inserted detail row model
Generated in `DetailRow<T>.DetailClick()`:

```csharp
Row<object> row = new Row<object>()
{
    RowType = "DetailRow",
    IsDataRow = true,
    IsExpand = true,
    Uid = uid,
    Index = null!,
    Data = Data!,
    IsDetailRow = true,
    Indent = Parent.AllowGrouping && Parent.GroupSettings?.Columns != null
        ? Parent.GroupSettings.Columns.Length
        : 0
};
```

### Generated cells for detail row
`GenerateDetailCellsForRow(row, colsSpan)` returns:
1. `CellType.Indent` × group indent count
2. `CellType.DetailIndent` if detail template exists
3. additional colspan adjustment if row drag is enabled
4. one data host cell with `ColSpan = colsSpan`

Meaning:
- the visual detail row is mostly a structural row
- only the final data host cell contains the actual template fragment
- earlier cells are alignment cells

---

## Cell Generation Logic for Detail Rows
<!-- token-budget: 260 words -->

`src/Internal/Actions/DetailRow.cs` → `GenerateDetailCellsForRow()`

```
GenerateDetailCellsForRow(detailRow, visibleColumnsCount)
    ↓
Create empty List<Cell<object>>
    ↓
for each group level
    add CellType.Indent
    ↓
if DetailTemplate exists
    add CellType.DetailIndent
    ↓
if AllowRowDragAndDrop
    colsSpan = colsSpan + 1
    ↓
add final host cell:
    new Cell<object>()
    {
        IsDataCell = true,
        Visible = true,
        ColSpan = colsSpan
    }
```

### Structural meaning
| Generated cell | Purpose | Rendered HTML |
|---|---|---|
| `CellType.Indent` | Align detail row under grouped rows | `<td class="e-indentcell">` |
| `CellType.DetailIndent` | Placeholder cell before template content | `<th class="e-detailindentcell">` |
| data host cell | Holds template fragment | `<th class="e-detailcell" colspan="...">` |

### Why the final host cell uses colspan
The detail template is designed to span the full visible record width. Instead of duplicating every visible column, the detail row creates one wide host cell that stretches across the current visible column count plus structural offsets.

---

## DOM Rendering by File
<!-- token-budget: 420 words -->

### `src/Internal/Renderer/GridRow.razor`
Responsible for rendering normal rows.

Important branch:
- loops over `GetCells()`
- checks `cell.CellType`
- for `CellType.Detail` calls `RenderDetailCell(cellParams)`
- for `CellType.Indent` or `CellType.DetailIndent` calls `RenderIndentCell(cellParams)`

This means the parent row toggle cell is part of the normal row rendering pipeline.

### `src/Internal/Renderer/DetailCellRenderer.cs`
Outputs:

```
<td class="e-detailrowexpand | e-detailrowcollapse ..."
    aria-expanded="true | false"
    tabindex="0"
    data-uid="..."
    onclick="..."
    onkeydown="...">
    <a class="e-icons e-dtdiagonaldown e-icon-gdownarrow"></a>
</td>
```

Behavior:
- applies focus handling
- raises record click routing if needed
- invokes `Parent.DetailRowModule.DetailClick(...)`
- refreshes focus when selection is disabled

### `src/Internal/Renderer/IndentCellRenderer.cs`
Outputs two possible shapes:

#### Regular indent
```
<td class="e-indentcell ..." data-uid="..." tabindex="..."></td>
```

#### Detail indent
```
<th class="e-detailindentcell ..." data-uid="..." tabindex="..." scope="col"></th>
```

### `src/Internal/Renderer/GridDetailRow.razor`
Renders only when:
- `RowParameters.Row.IsDetailRow == true`
- row is visible

Key output:

```
<tr class="e-detailrow" data-uid="detail-row-uid">
    ...indent cells...
    <th class="e-detailcell" colspan="..." tabindex="0" scope="col">
        <div>
            @DetailTemplate(RowParameters.Row.Data)
        </div>
    </th>
</tr>
```

Notable details:
- detail row uses `<tr class="e-detailrow">`
- template host is `<th>`, not `<td>`
- keyboard events are attached to the detail host cell
- nested content is wrapped in an inner `<div>`

### `src/Internal/Renderer/GridContent.razor`
The `<tbody>` loop decides final shape:

```
foreach (Row<object> Row in Rows)
{
    if (Row.IsExpand == true && Row.IsDetailRow == true && Row.Visible == true)
        <GridDetailRow ... />
    else if (Row.Visible)
        <GridRow ... />
}
```

This is the final switch that turns row-model state into actual DOM.

---

## CSS Classes and Semantics
<!-- token-budget: 260 words -->

| CSS class | Element | Meaning |
|---|---|---|
| `e-detailrow` | `<tr>` | Detail template row |
| `e-detailcell` | `<th>` | Host cell containing `DetailTemplate` fragment |
| `e-detailindentcell` | `<th>` | Structural placeholder before detail content |
| `e-detailrowexpand` | parent row toggle `<td>` | Parent row currently expanded |
| `e-detailrowcollapse` | parent row toggle `<td>` | Parent row currently collapsed |
| `e-dtdiagonaldown` | `<a>` icon | Expanded arrow icon |
| `e-dtdiagonalright` | `<a>` icon | Collapsed arrow icon |
| `e-icon-gdownarrow` | `<a>` icon | Expanded glyph style |
| `e-icon-grightarrow` | `<a>` icon | Collapsed glyph style |
| `e-indentcell` | `<td>` | Group alignment cell |
| `e-leftfreeze` | toggle/indent cell | Left frozen rendering alignment |
| `e-hiddenrow` | detail row or dirty row | Hidden from view, usually batch/delete or tree visibility case |
| `e-updatedtd` | dirty row | Marks updated or hidden modified row |
| `e-rowdragdrop` | row drag cell | Row drag handle cell; affects colspan indirectly |
| `e-groupcaptionrow` | grouping caption row | Relevant in grouped combinations |
| `e-recordplusexpand` | group caption expand cell | Group expanded |
| `e-recordpluscollapse` | group caption collapse cell | Group collapsed |

---

## JavaScript Interop
<!-- token-budget: 180 words -->

Detail template keyboard navigation uses JavaScript interop for focus management within nested detail content.

### C# → JavaScript Interop Calls

| C# Method | File | Line | JS Function | Purpose |
|-----------|------|------|-------------|---------|
| `InvokeMethod("sfBlazor.Grid.focusDetailTemplateElements", ...)` | DetailRow.cs | 341 | `focusDetailTemplateElements` | Manages Tab/ShiftTab focus within detail templates |

### TypeScript Functions

**File**: `scripts/sf-grid.ts`

#### `focusDetailTemplateElements(dataId: string, keyCombination: string, isDetailTemplateCell: boolean)`
**Line**: 454  
**Purpose**: Handles Tab/ShiftTab navigation between detail template focusable elements and parent grid cells  
**Logic**:
- **Tab**: Blurs current detail cell to allow natural focus flow to next focusable element
- **ShiftTab**: Iterates detail template elements, focuses last focusable element or moves to parent row cells

**Implementation details**:
- Validates grid instance and active element ancestry
- Checks for nested child grids within detail templates
- Manages reverse-tab focus from detail content back to parent row cells
- Uses `parentsUntil` helper to traverse DOM hierarchy safely

### Responsibility Split

| Layer | Responsibility |
|-------|----------------|
| **C# (DetailRow.cs)** | Detect Tab/ShiftTab on detail cell; check `IsDetailRow`, `IsFocused` state; invoke JS only when conditions met |
| **TypeScript (sf-grid.ts)** | Query DOM for detail template elements; iterate focusable children; manage focus transitions; handle nested grid scenarios |

### CRITICAL Rules
- Do not bypass C# detection logic; always check `isdetailTemplateFocused` before calling JS
- Do not call `focusDetailTemplateElements` for non-detail-template scenarios (check `GridTemplates.DetailTemplate != null`)
- JavaScript must safely handle missing detail templates with null/undefined checks
- Focus management must preserve ARIA semantics and keyboard navigation standards
- TreeGrid scenarios (`IsRenderedFromTreeGrid`) must be excluded from detail template focus logic

---

## 14 ASCII Diagrams
<!-- token-budget: 900 words -->

### Diagram 1 — Root render path

```
SfGrid.razor
│
├─ if (!EnableVirtualization && !EnableColumnVirtualization)
│    └─ GridContent
│         └─ standard tbody loop
│
└─ else
     └─ GridVirtualContent
          └─ virtual tbody loop
                ↓
           both loops inspect Parent.Rows
                ↓
           normal row  → GridRow
           detail row  → GridDetailRow
```

### Diagram 2 — Parent row with detail toggle

```
<tr class="e-row">
├── [optional] <td class="e-indentcell"></td>
├── [optional] <td class="e-rowdragdrop"></td>
├── <td class="e-detailrowcollapse" aria-expanded="false">
│     └── <a class="e-icons e-dtdiagonalright e-icon-grightarrow"></a>
├── <td class="e-rowcell">Column A</td>
├── <td class="e-rowcell">Column B</td>
└── <td class="e-rowcell">Column C</td>
```

### Diagram 3 — Expanded parent row + detail row pair

```
<tbody>
├── <tr class="e-row" data-uid="row-1">
│    ├── <td class="e-detailrowexpand" aria-expanded="true">
│    │    └── <a class="e-icons e-dtdiagonaldown e-icon-gdownarrow"></a>
│    └── data cells...
│
└── <tr class="e-detailrow" data-uid="detail-row-1">
     ├── <th class="e-detailindentcell" scope="col"></th>
     └── <th class="e-detailcell" colspan="3" scope="col">
          └── <div>
               └── DetailTemplate(context)
```

### Diagram 4 — Expand click event flow

```
User click
  ↓
RenderDetailCell onclick
  ↓
ClearFocus()
  ↓
Focus(row.Uid, cell.Uid)
  ↓
DetailCellClickHandlerInternal()
  ↓
DetailRowModule.DetailClick(row.Index, row.Uid)
  ↓
Create detail row + insert into Parent.Rows
  ↓
Trigger ContentStateChanged / VirtualComponentUpdate
  ↓
GridContent re-renders
  ↓
GridDetailRow appears in DOM
```

### Diagram 5 — Collapse click event flow

```
User click expanded toggle
  ↓
DetailClick(row.Index, row.Uid)
  ↓
Find inserted detail row at clickedRowIndex + 1
  ↓
Remove Parent.Rows[detailRowIndex]
  ↓
Set parent Row.IsExpand = false
  ↓
Trigger content refresh
  ↓
GridDetailRow no longer rendered
```

### Diagram 6 — Row generation for detail row

```
DetailClick()
  ↓
new Row<object>
  ├── RowType = "DetailRow"
  ├── IsDetailRow = true
  ├── IsExpand = true
  ├── Data = parent data
  └── Indent = group column count
        ↓
GenerateDetailCellsForRow()
  ├── Indent cells
  ├── DetailIndent cell
  └── host cell with ColSpan
```

### Diagram 7 — Grouping + detail template shape

```
<tr class="e-row">
├── <td class="e-indentcell"></td>         ← group level 1
├── <td class="e-detailrowexpand"></td>    ← detail toggle
└── data cells...

<tr class="e-detailrow">
├── <td class="e-indentcell"></td>         ← same group indent
├── <th class="e-detailindentcell"></th>
└── <th class="e-detailcell" colspan="N"></th>
```

### Diagram 8 — Multi-group + detail template

```
<tr class="e-row">
├── <td class="e-indentcell"></td>         ← group depth 1
├── <td class="e-indentcell"></td>         ← group depth 2
├── <td class="e-detailrowcollapse"></td>
└── data cells...

<tr class="e-detailrow">
├── <td class="e-indentcell"></td>
├── <td class="e-indentcell"></td>
├── <th class="e-detailindentcell"></th>
└── <th class="e-detailcell" colspan="N"></th>
```

### Diagram 9 — Row drag + detail template

```
Parent row:
<tr>
├── <td class="e-rowdragdrop"></td>
├── <td class="e-detailrowcollapse"></td>
└── data cells...

Detail row generated colspan:
visibleColumnsCount + 1

<tr class="e-detailrow">
├── <th class="e-detailindentcell"></th>
└── <th class="e-detailcell" colspan="visibleColumns + dragOffset"></th>
```

### Diagram 10 — Virtualization refresh path

```
Expanded detail row in virtualized grid
  ↓
DetailClick()
  ↓
if (EnableVirtualization)
    EventAggregator.Trigger("VirtualComponentUpdate")
else
    EventAggregator.Trigger("ContentStateChanged")
  ↓
GridVirtualContent / GridContent re-renders row list
  ↓
Detail row appears within current row model window
```

### Diagram 11 — Keyboard flows

```
BeforeCellFocus
  ↓
DetailRow.KeyHandler(args)
  ├── Enter    on CellType.Detail → DetailClick()
  ├── AltDown  on collapsed row   → DetailClick()
  ├── AltUp    on expanded row    → DetailClick()
  ├── CtrlDown → ExpandOrCollapseAll(true)
  └── CtrlUp   → ExpandOrCollapseAll(false)
```

### Diagram 12 — Event lifecycle

```
Expand request
  ↓
DetailsExpanding   (cancelable)
  ↓
Insert detail row
  ↓
DetailDataBound
  ↓
refresh DOM
  ↓
DetailsExpanded

Collapse request
  ↓
DetailsCollapsing  (cancelable)
  ↓
Remove detail row
  ↓
refresh DOM
  ↓
DetailsCollapsed
```

### Diagram 13 — TreeGrid visibility inheritance path

```
GridDetailRow.razor
  ↓
if Parent.IsRenderedFromTreeGrid
  ↓
check IsExpanded(record, true)
  ↓
if ancestor collapsed
  └── do not render detail row visible DOM
else
  └── render detail row
```

### Diagram 14 — Nested grid inside detail template

```
<tr class="e-detailrow">
└── <th class="e-detailcell" colspan="N">
     └── <div>
          └── <SfGrid>        ← nested hierarchy grid allowed
               ├── header
               ├── content
               └── pager
```

---

## All Major Feature Combinations
<!-- token-budget: 900 words -->

### 1. Base detail template only
**Files**: `GridTemplates.cs`, `DetailRow.cs`, `DetailCellRenderer.cs`, `GridDetailRow.razor`, `GridContent.razor`

Behavior:
- parent row renders one detail toggle cell
- expand inserts one detail row immediately after parent row
- detail row contains one detail indent header cell plus one spanning host cell

DOM rule:
- exactly one extra row per expanded parent row

### 2. Detail template + grouping
**Files**: `DetailRow.cs`, `Group.cs`, `IndentCellRenderer.cs`, `ExpandCellRenderer.cs`

Behavior:
- detail row `Indent` equals `GroupSettings.Columns.Length`
- each group level adds one `CellType.Indent`
- detail row visually aligns under grouped data row

DOM rule:
- group indent cells precede detail indent cell

### 3. Detail template + multi-level grouping
Behavior:
- multiple indent cells before both toggle cell and detail host cell
- expanded detail row does not replace group caption rows
- it is inserted only after the expanded parent data row

DOM rule:
- detail row depth mirrors parent row depth

### 4. Detail template + row drag and drop
**File**: `DetailRow.cs`

Behavior:
- `GenerateDetailCellsForRow()` increments `colsSpan` when `AllowRowDragAndDrop = true`
- drag handle column is accounted for in detail host width

DOM rule:
- detail host colspan grows by one

### 5. Detail template + selection
**File**: `Selection.cs`, `DetailCellRenderer.cs`

Behavior:
- clicking detail cell still routes through record-click and focus logic
- detail expansion does not inherently mean row selection toggle
- focus refresh occurs when selection is disabled

DOM rule:
- parent row selection classes may exist independent of detail row visibility

### 6. Detail template + keyboard navigation
**File**: `DetailRow.cs`, `FocusHandler.cs`

Behavior:
- `Enter` expands on detail cell
- `AltDown` expands collapsed row
- `AltUp` collapses expanded row
- `CtrlDown` expands all
- `CtrlUp` collapses all

DOM rule:
- keyboard path produces identical row insertion and removal as click path

### 7. Detail template + virtualization
**Files**: `GridVirtualContent.razor`, `VirtualScroll.cs`

Behavior:
- expand action triggers `VirtualComponentUpdate`
- row model still owns detail row insertion
- render contract is unchanged, only update path differs

DOM rule:
- detail row may appear only while its parent row exists in the current virtual row list

### 8. Detail template + column virtualization
Behavior:
- host detail row concept remains the same
- detail row structure still contains indent cells and one spanning host cell
- actual visible width alignment depends on virtual column path

DOM risk:
- incorrect colspan if visible-column calculation is misunderstood

### 9. Detail template + frozen columns
**Files**: `DetailCellRenderer.cs`, `IndentCellRenderer.cs`, `GridRow.razor.cs`

Behavior:
- detail toggle / indent cells may add `e-leftfreeze`
- `GetDetailCellStyle()` calculates left offset for frozen positioning

DOM rule:
- toggle cell and indent cells may carry freeze-alignment classes and `data-sf-style`

### 10. Detail template + paging
Behavior:
- page changes remove current DOM and recreate row list for active page
- expanded state depends on recreated `Parent.Rows`

DOM rule:
- detail row belongs only to rows present on current page render

### 11. Detail template + RTL
Behavior:
- core logic unchanged
- CSS layout direction changes externally

DOM rule:
- same structural classes; semantic DOM contract remains identical

### 12. Detail template + editing state
Behavior:
- detail rows can coexist near edited rows, added rows, or dirty rows
- dirty delete path in `GridDetailRow.razor` may add `e-hiddenrow e-updatedtd`

DOM rule:
- detail row may be present but hidden or marked updated under edit/delete scenarios

### 13. Detail template + TreeGrid hosting
**File**: `GridDetailRow.razor`, `GridRow.razor.cs`

Behavior:
- detail row visibility can depend on ancestor expanded state
- `IsExpanded(record, true)` guards render visibility

DOM rule:
- inserted detail row model may exist while final DOM rendering is suppressed by ancestor-collapse logic

### 14. Detail template + nested hierarchy grids
Behavior:
- `DetailTemplate` can render nested `<SfGrid>` instances
- each nested grid has independent header, content, pager, events, and detail templates

DOM rule:
- nested grid DOM is fully contained inside the parent row’s `.e-detailcell > div`

---

## Expand and Collapse Lifecycle Details
<!-- token-budget: 300 words -->

### Expand path
`DetailRow<T>.DetailClick()` when parent row is collapsed:

1. locate clicked parent row by `Uid`
2. compute `detailRowIndex = parentIndexInRows + 1`
3. raise `DetailsExpanding`
4. if canceled → stop
5. create detail row model
6. call `GenerateDetailCellsForRow()`
7. update parent row expand state via `DetailRowChanges(true, index, Parent.Rows)`
8. insert detail row into `Parent.Rows`
9. raise `DetailDataBound`
10. trigger refresh
11. raise `DetailsExpanded`

### Collapse path
When parent row is expanded:

1. locate clicked parent row
2. compute adjacent detail row position
3. raise `DetailsCollapsing`
4. if canceled → stop
5. remove detail row from `Parent.Rows`
6. update parent row expand state to false
7. trigger refresh
8. raise `DetailsCollapsed`

### Expand all / collapse all
`ExpandOrCollapseAll(bool ExpandAll)`:
- iterates all rows
- only expands normal data rows
- grouped caption rows mark their own `IsExpand` state when needed
- soft refreshes entire grid after batch operation

---

## Event Routing Reference
<!-- token-budget: 260 words -->

### Expand-related events
| Event | Timing | Cancelable | Purpose |
|---|---|---|---|
| `DetailsExpanding` | before insert | Yes | block expansion |
| `DetailDataBound` | after row model creation | No | initialize nested UI |
| `DetailsExpanded` | after DOM refresh path begins | No | post-expand notifications |

### Collapse-related events
| Event | Timing | Cancelable | Purpose |
|---|---|---|---|
| `DetailsCollapsing` | before removal | Yes | block collapse |
| `DetailsCollapsed` | after removal refresh path | No | post-collapse notifications |

### Related non-detail events
- `OnRecordClick` may also be triggered from detail cell click path
- `BeforeCellFocus` feeds keyboard expand and collapse handling
- `ContentStateChanged` and `VirtualComponentUpdate` drive final render updates

---

## Accessibility and Focus Contract
<!-- token-budget: 240 words -->

### Parent toggle cell
Rendered attributes:
- `aria-expanded="true | false"`
- `tabindex="0"`
- `data-uid="cell-uid"`
- `onkeydown` delegated to focus module

### Detail host cell
Rendered attributes:
- `tabindex="0"`
- `scope="col"`
- `onkeydown` delegates to `FocusModule.ProcessKeyDown(e, row, cell)`

### Focus flow
```
click detail toggle
  ↓
ClearFocus()
  ↓
Focus(row.Uid, cell.Uid)
  ↓
current cell becomes active focus target
  ↓
expand or collapse logic runs
```

### Keyboard action mapping
| Key action | Meaning |
|---|---|
| `Enter` | toggle current detail cell |
| `AltDown` | expand current row |
| `AltUp` | collapse current row |
| `CtrlDown` | expand all detail rows |
| `CtrlUp` | collapse all detail rows |

---

## Rendering Rules That Must Never Be Broken
<!-- token-budget: 220 words -->

- Do not render detail template content inside the parent data row.
- Do not replace the detail toggle cell with the detail content cell.
- Do not emit multiple detail rows for the same parent row from one expand action.
- Do not bypass `Parent.Rows`; detail rows must be model-driven.
- Do not use plain `<td>` for the detail template host when matching current implementation; it is rendered as `<th class="e-detailcell">`.
- Do not forget colspan adjustments for row drag and visible columns.
- Do not remove group indent alignment when grouping is active.
- Do not skip `DetailsExpanding` or `DetailsCollapsing` cancel checks.
- Do not mutate DOM directly in JS for detail rows; final DOM must come from Razor render.
- Do not break freeze style offsets for detail toggle cells.
- Do not assume virtualization changes structure; it changes update timing and visible row set.

---

## 6 Core LLM Training Prompts
<!-- token-budget: 700 words -->

### Prompt 1 — Explain base detail template DOM
**Task**: Explain how Syncfusion Blazor Grid renders detail template DOM from the parent row and the inserted detail row.

**Expected answer should include**:
- parent row contains `CellType.Detail`
- `RenderDetailCell()` outputs the expand/collapse toggle cell
- `DetailClick()` inserts a `DetailRow` model into `Parent.Rows`
- `GridDetailRow.razor` renders `<tr class="e-detailrow">`
- template host is `<th class="e-detailcell">`

### Prompt 2 — Diagnose missing detail row DOM
**Task**: A user clicks the detail toggle but no detail DOM appears. Explain the full checkpoints.

**Expected answer should inspect**:
- whether `GridTemplates.DetailTemplate` exists
- whether `DetailRowModule` is initialized
- whether `DetailsExpanding` canceled the action
- whether `Parent.Rows` received an inserted `IsDetailRow = true` row
- whether `GridContent` / `GridVirtualContent` is iterating that row and `Row.Visible == true`
- whether TreeGrid visibility logic suppressed output

### Prompt 3 — Explain grouping + detail template structure
**Task**: Describe the DOM when grouping and detail template are enabled together.

**Expected answer should include**:
- parent row indent cells
- detail row indent cells count = group depth
- `CellType.DetailIndent`
- alignment between grouped data row and inserted detail row
- group caption rows remain separate from detail rows

### Prompt 4 — Explain detail expand keyboard navigation
**Task**: Explain how keyboard shortcuts expand and collapse detail rows.

**Expected answer should include**:
- `BeforeCellFocus`
- `DetailRow.KeyHandler()`
- `Enter`, `AltDown`, `AltUp`, `CtrlDown`, `CtrlUp`
- no direct DOM mutation; same model-driven rendering path as click

### Prompt 5 — Explain virtualization + detail template
**Task**: Explain what changes and what stays the same when virtualization is enabled with detail template.

**Expected answer should include**:
- detail row model insertion stays the same
- refresh path changes to `VirtualComponentUpdate`
- DOM shape remains parent row + inserted detail row
- visibility depends on virtual content renderer and row window

### Prompt 6 — Generate a bug-fix checklist for wrong colspan / wrong alignment
**Task**: Create a checklist for debugging broken detail row width or alignment.

**Expected answer should include**:
- inspect `GenerateDetailCellsForRow()` visible column count
- inspect row drag offset logic
- inspect grouping indent count
- inspect `CellType.DetailIndent` rendering
- inspect frozen styles and left offsets
- inspect whether visible column calculation matches actual renderer mode

---

## Validation Checklist
<!-- token-budget: 240 words -->

Use this checklist before closing any detail-template-related task:

- [ ] `GridTemplates.DetailTemplate` exists and is non-null when expecting detail support
- [ ] parent row renders exactly one detail toggle cell
- [ ] collapsed row toggle has `e-detailrowcollapse`
- [ ] expanded row toggle has `e-detailrowexpand`
- [ ] toggle cell has `aria-expanded` matching row state
- [ ] one inserted detail row exists immediately after expanded parent row
- [ ] inserted row has class `e-detailrow`
- [ ] detail content host is `<th class="e-detailcell">`
- [ ] detail content host wraps `DetailTemplate(context)` inside `<div>`
- [ ] grouping adds matching count of `e-indentcell` cells before detail content
- [ ] row drag increases detail host colspan by one
- [ ] pre-expand and pre-collapse events can cancel action
- [ ] `DetailDataBound` fires after detail row creation
- [ ] virtualization path triggers `VirtualComponentUpdate`
- [ ] standard path triggers `ContentStateChanged`
- [ ] keyboard `Enter`, `AltDown`, `AltUp`, `CtrlDown`, `CtrlUp` work consistently
- [ ] TreeGrid-hosted visibility respects ancestor expanded state
- [ ] no direct client-only DOM insertion is relied upon

---

## Quick Source-to-Responsibility Map
<!-- token-budget: 220 words -->

| File | Responsibility |
|---|---|
| `src/GridTemplates.cs` | Declares `DetailTemplate` API |
| `src/SfGrid.Lifecycle.cs` | Initializes `DetailRowModule` |
| `src/SfGrid.razor.cs` | Stores detail module; publishes `hasDetailTemplate` |
| `src/Internal/SfGrid.razor` | Chooses content renderer |
| `src/Internal/Actions/DetailRow.cs` | Expand / collapse logic and detail row model creation |
| `src/Internal/Renderer/GridRow.razor` | Parent row cell dispatch |
| `src/Internal/Renderer/DetailCellRenderer.cs` | Toggle cell DOM and click / key interaction |
| `src/Internal/Renderer/IndentCellRenderer.cs` | Indent and detail-indent DOM |
| `src/Internal/Renderer/GridDetailRow.razor` | Final detail row DOM output |
| `src/Internal/Renderer/GridContent.razor` | Decides whether to render `GridRow` or `GridDetailRow` |
| `src/GridEvents.cs` | Detail event API |
| `src/EventModels/Grids.cs` | Event argument models |

---

## Short Implementation Guidance for Agents
<!-- token-budget: 200 words -->

When implementing or fixing detail-template-related behavior:

1. identify whether the issue is in **row model generation** or **DOM rendering**
2. if row not inserted, inspect `DetailRow.cs`
3. if row inserted but not shown, inspect `GridContent.razor` / `GridVirtualContent.razor`
4. if toggle UI wrong, inspect `DetailCellRenderer.cs`
5. if alignment wrong, inspect `GenerateDetailCellsForRow()` and `IndentCellRenderer.cs`
6. if grouped scenario fails, inspect indent count and caption-row relationships
7. if keyboard fails, inspect `DetailRow.KeyHandler()` and focus routing
8. if nested grid bugs occur, inspect only the content inside `.e-detailcell > div`; outer detail row contract should remain unchanged

**Golden rule**: the DOM is correct only when the row model and render tree agree. Never patch one while ignoring the other.
