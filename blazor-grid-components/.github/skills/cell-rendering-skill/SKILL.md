---
name: Cell-Rendering-skill
description: Expert knowledge for the Cell Rendering system in the Syncfusion Blazor Grid. Use this skill for any feature-implementation or bug-fix task scoped to cell DOM generation, rendering pipelines, and cross-feature cell interactions. Documents all 12 CellType variants, full ASCII DOM structures, rendering flows, and event ecosystems across sorting, filtering, grouping, editing, freezing, and virtual scrolling.
---

# Skill Instructions

<!-- token-budget: 50 words -->

**Purpose**

Expert knowledge for **Cell Rendering** in the Blazor Grid. Guarantees no breakage with any other feature when manipulating cells, rendering flows, or cross-feature cell state.

---

**Agent Invocation**

Mode: `feature-implementation` | `bug-fix`  
Skill: `Cell Rendering`  
Trigger: Any task affecting cell DOM generation, rendering, styling, event binding, or cell interaction across features.

---

## Knowledge References

<!-- token-budget: 80 words -->

- `src/Internal/Models/Cell.cs` — `Cell<T>` generic class, properties, CellType mapping
- `src/Internal/Base/Enums.cs` — All `CellType` enum values (12 types)
- `src/Internal/Renderer/GridRow.CellRenderer.cs` — Main `RenderCell()` RenderFragment and `CellStateInfo` cache
- `src/Internal/Renderer/GridRow.razor` — Row iteration and cell dispatch
- `src/Internal/Renderer/CellRender.razor` — Data cell `<td>` Razor template
- `src/Internal/Renderer/GridHeaderCell.razor` — Header cell `<th>` render
- `src/Internal/Renderer/GridCellRegister.razor` — CellType-to-component dispatcher
- `src/Internal/Models/CellDOM.cs` — `CellDOM` class definition (cell and row customization)
- `src/EventModels/Grids.cs` — Cell event args (`QueryCellInfoEventArgs`, `CellEditArgs`, etc.) that use `CellDOM`
- `src/GridEvents.cs` — All cell EventCallbacks
- `scripts/selection.ts`, `scripts/edit.ts`, `scripts/util.ts` — JS cell DOM interaction
- `/docs/training/` — All feature-specific cell rendering rules

---

## Training Insights Applied

<!-- token-budget: 100 words -->

### Cell Lifecycle Rules

1. **Cell Creation** — Cells created in `RowModelGenerator.GenerateCells()` during row initialization; CellType determined by column config (Detail, RowDrag, regular Data, etc.).
2. **Cell Caching** — `GridRow.CellRenderer._cellCache` stores `CellStateInfo` to avoid re-rendering identical cells; cleared on data updates.
3. **Cell State Preservation** — Cell `Index`, `CellValue`, `Visible` must persist across feature changes (sort, filter, grouping, edit).
4. **DOM Class Stability** — CSS classes (`e-rowcell`, `e-headercell`, `e-editcell`) are **identity markers** — removing them breaks JS event handlers.
5. **aria-colindex Requirement** — Every cell must have `aria-colindex` attribute matching its column index; missing attr breaks selection/focus.
6. **Virtual Scroll Cell Recycle** — In virtual scroll mode, cells are reused (not destroyed/recreated); CellStateInfo cache must account for row index shifting.
7. **Frozen Column Cell Sync** — Frozen left cells and scrollable right cells are rendered in separate `<table>` elements; both must update on column width changes.

---

## Code Location Map

<!-- token-budget: 120 words -->

### Core Cell Model & Enums
- `src/Internal/Models/Cell.cs` — Generic `Cell<T>` class with `CellType`, `Index`, `CellValue`, `Visible`, `DetailRowExpand` properties
- `src/Internal/Base/Enums.cs` — `CellType` enum (Data, Detail, DetailIndent, Indent, RowDrag, Expand, GroupCaption, GroupCaptionEmpty, CaptionSummary, Summary, CommandColumn, StackedHeader)
- `src/Internal/Models/CellDOM.cs` — `CellDOM` class for cell and row customization (ID, XPath, DomUUID, ElementID)
- `src/Internal/Base/InternalClass.cs` — `GridCellParameters` class
- `src/Internal/Models/CellComponentService.cs` — Cell identity lifecycle management

### Rendering Pipeline
- `src/Internal/Renderer/GridRow.CellRenderer.cs` — Main `RenderCell()` RenderFragment entry point; `CellStateInfo` internal class (line 35); CellDOM bridge
- `src/Internal/Renderer/GridRow.razor` — Row-level iteration: `@foreach(var cell in row.Cells) { RenderCell(cell) }`
- `src/Internal/Renderer/GridCellRegister.razor` — Dynamic CellType-to-component dispatcher
- `src/Internal/Renderer/CellRender.razor` — Standard data cell `<td>` with CSS classes, aria-colindex
- `src/Internal/Renderer/GridHeaderCell.razor` — Header cell `<th>` with sort/filter UI
- `src/Internal/Renderer/IndentCellRenderer.cs`, `ExpandCellRenderer.cs`, `GroupCaptionRenderer.cs`, etc. — Specialized cell type renderers

### Cell Generation (Pre-Rendering)
- `src/Internal/Generators/RowModelGenerator.cs` — `GenerateCells(row)` — builds Data + Indent + RowDrag cells
- `src/Internal/Generators/SummaryModelGenerator.cs` — Aggregate row cells (CaptionSummary, Summary, Indent)
- `src/Internal/Actions/DetailRow.cs` — Detail row cell generation (Detail, DetailIndent, Indent)

### Edit Cell Rendering
- `src/Internal/Editors/EditorCellBase.cs` — Base class for all edit cells
- `src/Internal/Editors/TextBoxEditCell.razor`, `NumericEditCell.razor`, `DropDownEditCell.razor`, etc. — Specific edit cell types

### Event Integration
- `src/GridEvents.cs` — `QueryCellInfo`, `HeaderCellInfo`, `OnCellEdit`, `OnCellSave`, `CellSelected`, `CellSelecting`, etc. callbacks
- `src/EventModels/Grids.cs` — `QueryCellInfoEventArgs<T>`, `CellEditArgs<T>`, and other event arg classes (uses `CellDOM`)
- `src/Internal/Models/CellDOM.cs` — `CellDOM` class used by event args for cell customization
- `src/Internal/Models/EventAggregator.cs` — `BeforeCellFocus` event bus

### JS/TS Cell Interaction
- `scripts/util.ts` — `getCellByRowUidAndColIndex()`, `addRemoveActiveClasses()` for cell DOM queries
- `scripts/selection.ts` — Cell drag selection, `e-rowcell` focus, aria-colindex tracking
- `scripts/edit.ts` — Edit cell focus, Tab navigation, `e-editcell` DOM class
- `scripts/sf-grid.ts` — `focusCell()` JS-C# interop

---

## Cell Types Reference

<!-- token-budget: 150 words -->

| CellType | Renderer | CSS Class | Purpose | DOM Example |
|----------|----------|-----------|---------|-------------|
| **Data** | `CellRender.razor` | `e-rowcell` | Standard data cell displaying column value | `<td class="e-rowcell" aria-colindex="2">ProductName</td>` |
| **Detail** | `DetailCellRenderer.cs` | `e-detailcell` | Placeholder in row for detail row toggle | `<td class="e-detailcell"></td>` |
| **DetailIndent** | `IndentCellRenderer.cs` | `e-detailindent` | Indent for hierarchical detail rows | `<td class="e-detailindent"></td>` |
| **Indent** | `IndentCellRenderer.cs` | `e-indentcell` | Grouping depth spacer | `<td class="e-indentcell" style="width:17px;"></td>` |
| **RowDrag** | `RowDragCellRenderer.cs` | `e-rowdragdrop` | Row drag handle cell (hamburger icon) | `<td class="e-rowdragdrop"><span></span></td>` |
| **Expand** | `ExpandCellRenderer.cs` | `e-detailrowcollapse` / `e-detailrowexpand` | Detail row expand/collapse toggle | `<td class="e-detailrowexpand"></td>` |
| **GroupCaption** | `GroupCaptionRenderer.cs` | `e-groupcaption` | Group header row spanning multiple columns | `<td class="e-groupcaption" colspan="5">Dept: Sales</td>` |
| **GroupCaptionEmpty** | `GroupCaptionRenderer.cs` | `e-groupcaption` | Empty group caption placeholder | `<td class="e-groupcaption"></td>` |
| **CaptionSummary** | `CaptionSummaryRenderer.cs` | `e-captionsummary` | Summary cell in group caption row | `<td class="e-captionsummary">Sum: 10</td>` |
| **Summary** | `SummaryCellRenderer.cs` | `e-summarycell` | Aggregate cell in footer row | `<td class="e-summarycell">Total: 1000</td>` |
| **CommandColumn** | `CommandColumnRenderer.cs` | `e-rowcell` | Command column cell with action buttons (Edit, Delete, Save, Cancel) | `<td class="e-rowcell"><button>Edit</button></td>` |
| **StackedHeader** | `GridHeaderCell.razor` | `e-stackedheadercell` | Multi-level stacked header cell spanning child columns | `<th class="e-stackedheadercell" colspan="3">Contact Info</th>` |

> **Note:** The `CellType` enum (`src/Internal/Base/Enums.cs`) has exactly **12** values (those listed above). Two things are *not* `CellType` variants:
> - **Header cells** render through the separate `GridHeaderCell.razor` component (`<th class="e-headercell">`), not the cell pipeline.
> - **Selection checkboxes** are `Data` cells carrying the CSS class `e-gridchkbox` (`src/Internal/Renderer/GridRow.CellRenderer.cs:456`), not a dedicated `CheckBox` cell type.

---

## Full Cell DOM Structure by Type

<!-- token-budget: 500 words — ASCII diagrams with aria attributes -->

### 1. Standard Data Row with All Cell Types

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-row" data-uid="row-1" aria-rowindex="2">                        │
│   ├─ <td class="e-detailcell" aria-colindex="1"> [Expand Toggle]            │
│   ├─ <td class="e-indentcell" aria-colindex="2" style="width:17px;">         │
│   ├─ <td class="e-rowdragdrop" aria-colindex="3">                           │
│   │   └─ <span class="e-icons e-drag"></span> [Drag Handle]                 │
│   ├─ <td class="e-rowcell e-selectioncell" aria-colindex="4">               │
│   │   └─ <input type="checkbox" aria-label="Select row">                    │
│   ├─ <td class="e-rowcell" aria-colindex="5" tabindex="0" data-field="ID">  │
│   │   └─ 1001                                                                │
│   ├─ <td class="e-rowcell" aria-colindex="6" tabindex="0" data-field="Name">│
│   │   └─ Alice Smith                                                        │
│   ├─ <td class="e-rowcell e-templatecell" aria-colindex="7">                │
│   │   └─ [Custom template content]                                          │
│   └─ <td class="e-rowcell e-lastrowcell" aria-colindex="8">                 │
│       └─ Final column value                                                 │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2. Header Row (Single Header)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-headrow" aria-rowindex="1">                                    │
│   ├─ <th class="e-headercell" aria-colindex="1">                            │
│   │   └─ [Expand Icon]                                                      │
│   ├─ <th class="e-headercell e-sort e-ascending" aria-colindex="2"          │
│   │       aria-sort="ascending">                                            │
│   │   ├─ Sort Column Name                                                   │
│   │   └─ <span class="e-sort-icon"></span>                                  │
│   ├─ <th class="e-headercell e-filter" aria-colindex="3">                   │
│   │   ├─ Filter Column Name                                                 │
│   │   └─ <span class="e-filter-icon"></span>                                │
│   └─ <th class="e-headercell" aria-colindex="4">Regular Column</th>          │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3. Edit Cell (Inline Edit Mode — Normal)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-row e-editrow" data-uid="row-1" aria-rowindex="2">             │
│   ├─ <td class="e-editcell" aria-colindex="1">[Action buttons]              │
│   └─ <td class="e-editcell e-field" aria-colindex="2" data-field="Name">    │
│       └─ <input type="text" class="e-field" value="Alice Smith"             │
│            data-msg-required="Name is required"                             │
│            aria-label="Edit Name">                                          │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4. Edit Cell (Batch Edit Mode)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-row e-batchrow" data-uid="row-1" aria-rowindex="2">            │
│   └─ <td class="e-editcell e-rowcell e-batchcell" aria-colindex="2"         │
│        data-field="Name">                                                    │
│       ├─ <input type="text" value="Alice Smith" style="width:100%;">        │
│       └─ <span class="e-error" style="display:none;">Error message</span>   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5. Filter Bar Row

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-filterbarcell" aria-rowindex="filter">                         │
│   ├─ <td class="e-filterbarcell" aria-colindex="1"></td>                    │
│   ├─ <td class="e-filterbarcell" aria-colindex="2">                         │
│   │   └─ <input type="text" class="e-filterbar-cell" placeholder="Search"   │
│   │        aria-label="Filter by Name">                                     │
│   └─ <td class="e-filterbarcell" aria-colindex="3">                         │
│       └─ <input type="text" class="e-filterbar-cell" placeholder="Search"> │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 6. Group Caption Row (Grouped by Department)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-grouprow" aria-rowindex="3">                                   │
│   ├─ <td class="e-detailcell" aria-colindex="1">                            │
│   │   └─ <span class="e-icons e-expand"></span> [Toggle group]              │
│   ├─ <td class="e-indentcell" aria-colindex="2" style="width:17px;">        │
│   └─ <td class="e-groupcaption" colspan="6" aria-colindex="3">              │
│       ├─ <span class="e-icons e-expand"></span>                             │
│       ├─ Department                                                          │
│       ├─ : Sales                                                             │
│       ├─ (Count: 5)                                                          │
│       └─ <span class="e-captionsummary">Sum: 5000</span>                    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7. Summary/Aggregate Row (Footer)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-summaryrow" aria-rowindex="99">                                │
│   ├─ <td class="e-indentcell" aria-colindex="1" style="width:17px;">        │
│   ├─ <td class="e-rowcell" aria-colindex="2">Total</td>                     │
│   ├─ <td class="e-summarycell" aria-colindex="3" data-field="Salary">       │
│   │   └─ $500,000                                                           │
│   └─ <td class="e-summarycell" aria-colindex="4" data-field="Count">        │
│       └─ 50                                                                 │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8. Detail Row (Expanded)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-row" data-uid="row-1" aria-rowindex="2">                       │
│   └─ <td class="e-detailcell" aria-colindex="1">                            │
│       └─ <span class="e-icons e-detailrowexpand"></span> [Toggle]          │
├─────────────────────────────────────────────────────────────────────────────┤
│ <tr class="e-detailrow" aria-rowindex="2-detail">                           │
│   ├─ <td class="e-detailindent" aria-colindex="1"></td>                    │
│   └─ <td class="e-detailrowcell" colspan="7" aria-colindex="2">             │
│       └─ [Custom detail template or nested grid]                            │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9. Frozen Column Split (Left + Scrollable)

```
┌────────────────────────────────────┬────────────────────────────────────────┐
│ Left Frozen Table                  │ Scrollable Right Table                 │
├────────────────────────────────────┼────────────────────────────────────────┤
│ <table class="e-frozenheader">     │ <table class="e-table">                │
│   <tr class="e-headrow">           │   <tr class="e-headrow">               │
│     <th aria-colindex="1">ID</th>  │     <th aria-colindex="2">Name</th>    │
│ </table>                           │ </table>                               │
│                                    │                                        │
│ <div class="e-frozencontent">      │ <div class="e-gridcontent">            │
│   <table>                          │   <table>                              │
│     <tr data-uid="row-1">          │     <tr data-uid="row-1">              │
│       <td aria-colindex="1">1001   │       <td aria-colindex="2">Alice</td> │
│     </tr>                          │     </tr>                              │
│   </table>                         │   </table>                             │
│ </div>                             │ </div>                                 │
└────────────────────────────────────┴────────────────────────────────────────┘
```

---

## Cell Rendering Pipeline Flow

<!-- token-budget: 250 words — Complete rendering sequence -->

### Standard Data Cell Rendering Pipeline

```
1. ROW INITIALIZATION (RowModelGenerator.GenerateCells)
   ├─ Input: Row<T> data object, Column[] list
   ├─ For each column:
   │  └─ Create Cell<T> with CellType.Data, Index (col index), CellValue
   └─ Output: List<Cell<object>> cells

2. ROW COMPONENT (GridRow.razor.cs)
   ├─ OnParametersSet: Validate row.Cells exists
   ├─ Build CellCache: Store previous cell state in _cellCache
   ├─ ShouldRender: Check if cell list changed or CellStateInfo dirty
   └─ Return: bool (true if re-render needed)

3. ROW RENDER (GridRow.razor)
   ├─ @foreach(var cell in Row.Cells)
   │  └─ Call RenderCell(new GridCellParameters(cell, row, column))
   └─ Emit: Multiple <td> elements

4. CELL RENDERER DISPATCH (GridRow.CellRenderer.cs)
   ├─ RenderCell(cellParameters)
   ├─ Lookup CellStateInfo from cache
   ├─ Validate Cell.CellType not null
   └─ Route to appropriate renderer:
      ├─ CellType.Data → RenderDataCell()
      ├─ CellType.Indent → RenderIndentCell()
      └─ CellType.* → CellRegister dispatcher
   (Note: header cells are NOT routed here — they render via the
    separate GridHeaderCell.razor component, outside this pipeline.)

5. CELL-SPECIFIC RENDERER (e.g., CellRender.razor)
   ├─ Build <td> with aria-colindex, CSS classes, data-field
   ├─ Apply cell styling (e-rowcell, e-templatecell, e-lastrowcell)
   ├─ Render cell content:
   │  ├─ If column.Template: Invoke custom template
   │  ├─ If column.Format: Apply format string
   │  └─ Otherwise: Display CellValue directly
   ├─ Attach event handlers:
   │  ├─ @onmouseenter → Tooltip trigger
   │  ├─ @ondblclick → Edit mode trigger
   │  └─ @onclick → Selection trigger
   └─ Output: <td> element

6. EVENT CALLBACK CHAIN (GridEvents.cs)
   ├─ QueryCellInfo event fires (allows custom styling)
   ├─ Blazor JS interop adds event listeners to <td>
   ├─ JS sets aria-colindex, aria-selected, tabindex
   └─ Complete: Cell ready for user interaction

7. POST-RENDER (GridRow.razor.cs)
   ├─ OnAfterRender callback
   ├─ Invoke JS interop for cell focus (if editing)
   ├─ Update CellStateInfo in cache
   └─ Mark complete
```

### Feature-Specific Cell Rendering Modifications

#### Sorting Impact on Cell Rendering
```
NORMAL:                               SORTED:
┌─────────────────┐                  ┌─────────────────┐
│ <th>OrderID     │                  │ <th class=      │
│ (plain)         │    ─────────→    │  "e-sort        │
│ </th>           │                  │  e-ascending">  │
└─────────────────┘                  │ OrderID         │
                                     │ <span class=    │
                                     │  "e-sort-icon"> │
                                     │ </span>         │
                                     │ </th>           │
                                     └─────────────────┘

Effect: HeaderCell adds sort icon, aria-sort attribute
Cell Content: UNCHANGED
```

#### Filtering Impact on Cell Rendering
```
NORMAL HEADER ROW:                    WITH FILTERBAR:
┌─────────────────────────────────┐  ┌─────────────────────────────────┐
│ <tr class="e-headrow">          │  │ <tr class="e-headrow">          │
│   <th>OrderID</th>              │  │   <th>OrderID</th>              │
│ </tr>                           │  │ </tr>                           │
│                                 │  │ <tr class="e-filterbarcell">    │
│ [Data rows below]               │  │   <td class="e-filterbarcell">  │
│                                 │  │     <input placeholder="Search" │
│                                 │  │            aria-label="Filter">│
│                                 │  │   </td>                         │
│                                 │  │ </tr>                           │
│                                 │  │                                 │
│                                 │  │ [Filtered data rows below]      │
└─────────────────────────────────┘  └─────────────────────────────────┘

Effect: New row inserted with filter cells
Cell Content: Filtered data only displayed
```

#### Editing Impact on Cell Rendering
```
NORMAL DATA CELL:                    EDIT MODE CELL:
┌──────────────────────────────┐    ┌──────────────────────────────┐
│ <td class="e-rowcell"        │    │ <td class="e-editcell        │
│   aria-colindex="2"          │    │   e-field" aria-colindex="2" │
│   tabindex="0">              │    │   data-field="Name">          │
│   Alice Smith                │    │   <input type="text"          │
│ </td>                        │    │    value="Alice Smith"        │
│                              │    │    aria-label="Edit Name">    │
│                              │    │ </td>                         │
└──────────────────────────────┘    └──────────────────────────────┘

Effect: <td> content replaced with <input>
Classes: e-rowcell → e-editcell
Events: Blur/Enter triggers save
```

#### Virtual Scroll Impact on Cell Rendering
```
NORMAL: All rows DOM rendered
┌─────────────────────────────┐
│ Row 1 <tr data-uid="1">     │
│ Row 2 <tr data-uid="2">     │
│ Row 3 <tr data-uid="3">     │
│ ...                         │
│ Row 10000 rendered          │ (Heavy DOM)
└─────────────────────────────┘

VIRTUAL SCROLL: Only visible + buffer rows
┌─────────────────────────────┐
│ [Spacer: 500 rows] 8500px   │
├─────────────────────────────┤
│ Row 501 <tr> (recycled)     │  ← Reused <tr> element
│ Row 502 <tr> (recycled)     │  ← data-uid updated
│ Row 503 <tr> (recycled)     │  ← CellStateInfo cache cleared
│ ...                         │
│ Row 525 <tr> (recycled)     │
├─────────────────────────────┤
│ [Spacer: 9475 rows] 160K px │
└─────────────────────────────┘

Effect: Cell <td> elements reused, data binding updated
CellStateInfo: Cache must be keyed by data-uid, not row index
```

#### Freeze Column Impact on Cell Rendering
```
NORMAL TABLE:
<table>
  <tr>
    <td aria-colindex="1">ID</td>    ← All in single DOM tree
    <td aria-colindex="2">Name</td>
    <td aria-colindex="3">Salary</td>
  </tr>
</table>

FROZEN TABLE (Column 1 frozen):
┌─ Left <table class="e-frozenheader">
│  <tr>
│    <th aria-colindex="1">ID</th>
│  </tr>
│  </table>
│  <div class="e-frozencontent">
│    <table>
│      <tr data-uid="row-1">
│        <td aria-colindex="1">1001</td>
│      </tr>
│    </table>
│  </div>
│
└─ Right <table class="e-table">  (scrollable)
   <tr>
     <th aria-colindex="2">Name</th>
     <th aria-colindex="3">Salary</th>
   </tr>
   <div class="e-gridcontent">
     <tr data-uid="row-1">
       <td aria-colindex="2">Alice</td>
       <td aria-colindex="3">$60K</td>
     </tr>
   </div>

Effect: <td> aria-colindex MUST match original column index
Cell Sync: Both tables updated on width/data changes
Reorder Risk: aria-colindex mismatch breaks selection
```

---

## Cell Event Ecosystem

<!-- token-budget: 200 words — Event flow diagram and mapping -->

### Complete Cell Event Lifecycle

```
USER INTERACTION (Browser Event)
│
├─ Click on cell
│  ├─ JS: util.ts addRemoveActiveClasses()
│  │  └─ Add e-selectioncell class
│  ├─ JS interop: DotNet.invokeMethodAsync('OnCellClick')
│  ├─ C#: GridEvents<T>.CellSelecting fires
│  ├─ C#: GridEvents<T>.CellSelected fires
│  └─ Blazor: StateHasChanged() → Re-render cell with e-selectioncell
│
├─ Double-click cell (Edit mode)
│  ├─ JS: edit.ts detectCellEditAction()
│  ├─ JS interop: DotNet.invokeMethodAsync('StartEdit', colIndex, rowIndex)
│  ├─ C#: GridEvents<T>.OnCellEdit fires
│  ├─ C#: GridEvents<T>.OnBeforeEdit fires
│  ├─ Blazor: RenderCell() → CellType changes to Edit
│  ├─ Blazor: Editor component (TextBoxEditCell.razor) rendered
│  ├─ JS: focusCell() JS interop → Focus <input>
│  └─ Complete: Edit cell ready
│
├─ Blur from edit cell (Save)
│  ├─ JS: edit.ts detectCellSave()
│  ├─ Validate cell value
│  ├─ JS interop: DotNet.invokeMethodAsync('OnCellSave', newValue)
│  ├─ C#: GridEvents<T>.OnCellSave fires (Cancel check)
│  ├─ C#: Data binding updates
│  ├─ C#: GridEvents<T>.CellSaved fires
│  ├─ Blazor: RenderCell() → CellType.Data, updated value displayed
│  └─ Complete: Save cycle done
│
├─ Mouse enter cell (Tooltip)
│  ├─ JS: tooltip.ts cellHover()
│  ├─ JS: getCellByRowUidAndColIndex() lookup
│  ├─ JS interop: DotNet.invokeMethodAsync('TooltipShow', cellData)
│  ├─ Blazor: GridTemplates.TooltipTemplate renders
│  └─ Complete: Tooltip visible
│
├─ Copy cell (Ctrl+C)
│  ├─ JS: clipboard.ts getCellContent()
│  ├─ JS interop: DotNet.invokeMethodAsync('BeforeCopyPaste')
│  ├─ C#: GridEvents<T>.BeforeCopyPaste fires (covers both copy and paste; cancel via args.Action)
│  └─ Clipboard set
│
└─ Paste cell (Ctrl+V)
   ├─ JS: clipboard.ts pasteCellContent()
   ├─ JS interop: DotNet.invokeMethodAsync('BeforeCellPaste')
   ├─ C#: GridEvents<T>.BeforeCellPaste fires (per cell)
   ├─ C#: GridEvents<T>.OnCellSave fires (data binding)
   └─ Blazor: Cell re-renders with new value
```

### Event Callback Mapping (GridEvents.cs)

| Event | Fires When | Can Cancel | Associated EventArgs | Used For |
|-------|-----------|----------|---------------------|----------|
| `QueryCellInfo` | Cell DOM complete | No | `QueryCellInfoEventArgs<T>` | Custom cell styling, CSS injection |
| `HeaderCellInfo` | Header cell DOM complete | No | `HeaderCellInfoEventArgs` | Header customization, icons |
| `OnCellEdit` | Double-click or F2 pressed | Yes | `CellEditArgs<T>` | Prevent edit on certain cells |
| `OnCellSave` | Blur or Enter in edit cell | Yes | `CellSaveArgs<T>` | Validate and intercept value save |
| `CellSaved` | Cell value successfully saved | No | `CellSavedArgs<T>` | Post-save actions (API calls) |
| `CellSelected` | Cell selected via click | No | `CellSelectEventArgs<T>` | Track selection history |
| `CellSelecting` | Before cell selection | Yes | `CellSelectingEventArgs<T>` | Prevent selection on certain cells |
| `CellDeselecting` | Before cell deselection | Yes | `CellDeselectEventArgs<T>` | Prevent deselection |
| `CellDeselected` | Cell deselected | No | `CellDeselectEventArgs<T>` | Cleanup per deselection |
| `BeforeCellPaste` | Paste action per cell | Yes | `BeforeCellPasteEventArgs<T>` | Transform pasted value |
| `BeforeAutoFillCell` | Auto-fill in Batch edit | Yes | `BeforeAutoFillCellEventArgs<T>` | Custom auto-fill logic |

---

## Cell Rendering Across Feature Combinations

<!-- token-budget: 300 words — Interaction Matrix for cell rendering -->

### Interaction Matrix: Cell + Other Features

| Feature Combo | Rendering Impact | Cell State | Risk Level | Mitigation |
|---|---|---|---|---|
| **Cell + Sorting** | Header cell adds sort icon; data cells unchanged | aria-sort on header only | Low | Preserve aria-colindex on sort |
| **Cell + Filtering** | Filter bar row inserted; data cells filtered | aria-colindex stable; row count changes | Medium | Keep cell dom-id unique per row |
| **Cell + Grouping** | Group caption rows inserted; indent cells added per group depth | Cell count increases; CellType.Indent, GroupCaption | High | Validate cell list regeneration per group level |
| **Cell + Paging** | Only page cells rendered; cells DOM recycled per page | Virtual cell cache per page | Medium | Clear CellStateInfo on page change |
| **Cell + Editing (Normal)** | Single edit row; only edited cell becomes <input> | Data cell content swapped to an editor (src/Internal/Editors/*.razor); not a CellType change | Critical | Preserve aria-colindex; focus JS interop timing |
| **Cell + Editing (Batch)** | Multiple cells can edit simultaneously; all become <input> | Multiple Data cells swapped to editors simultaneously | Critical | Batch save must validate all cells; handle Tab navigation |
| **Cell + Freezing** | Cells split across two <table>; left frozen, right scrollable | aria-colindex MUST match original column position | Critical | Sync cell width updates; aria-colindex identity |
| **Cell + VirtualScroll** | Cells recycled (not destroyed); row data binding updated | CellStateInfo cache by data-uid, not row index | Critical | Invalidate cache on scroll; reuse <td> elements |
| **Cell + InfiniteScroll** | Cells appended as new rows loaded | Row.Cells extended dynamically | High | Re-validate aria-rowindex per append |
| **Cell + Selection** | e-selectioncell class added/removed; aria-selected attribute | Multiple cells can have e-selectioncell | Medium | Manage CSS state per selection mode (Box, Row, Flow) |
| **Cell + RowReorder** | Rows reordered; cell aria-rowindex updated per new position | Cell visual position changes | High | Rebind cell event handlers; update CellStateInfo keys |
| **Cell + ColumnReorder** | Cells reordered in DOM; aria-colindex remapped | aria-colindex MUST reflect new column order | Critical | Update cell CSS classes; JS listener re-registration |
| **Cell + Aggregates** | Summary cells appended in footer; CellType.Summary, CaptionSummary | Extra rows in DOM | Medium | Sum formula must read correct cells by aria-colindex |
| **Cell + ForeignKey** | Foreign key column cell content transformed (FK value → lookup display text) | CellValue cached; cell renders lookup result | Medium | Cache invalidation on FK data change |
| **Cell + DetailRow** | Detail row cells inserted below data row; CellType.Detail, DetailIndent | Row count doubles (data + detail) | High | Manage cell generation for detail rows; aria-rowindex |
| **Cell + HeaderTemplate** | Header cell custom template rendered (replaces sort/filter UI) | HeaderCell content fully customized | Medium | Preserve aria-colindex; sort/filter may not work |
| **Cell + CellTemplate** | Custom template in data cell | Cell render delegates to template | Medium | Template must bind to cell context; aria-colindex preserved |
| **Cell + ColumnTemplate** | Entire column uses custom template | All cells in column use template | Medium | Template context includes row data + column config |
| **Sorting + Filtering + Grouping + Editing + Freeze + VirtualScroll** | Maximum complexity: cells split, recycled, sorted, filtered, grouped, edit mode on virtual scroll | All CellStateInfo invalidated; aria-colindex identity CRITICAL | Critical | Zero-tolerance: every cell operation must validate aria-colindex, data-uid, CellType consistency |

---

## DOM CSS Class Stability Rules

<!-- token-budget: 100 words — Critical rules for cell CSS -->

### Mandatory CSS Classes per Context

1. **Data Cell**: ALWAYS has `e-rowcell`
   - May also have: `e-templatecell`, `e-lastrowcell`, `e-selectioncell`, `e-gridchkbox`
   - Must never lose `e-rowcell` — JS listeners depend on it

2. **Header Cell**: ALWAYS has `e-headercell`
   - May also have: `e-sort`, `e-ascending`, `e-descending`, `e-filter`
   - Must never lose `e-headercell` — sort/filter UI depends on it

3. **Edit Cell**: ALWAYS has `e-editcell` and `e-field`
   - May also have: `e-batchcell`
   - Must never lose `e-editcell` — Tab navigation and blur event handlers depend on it

4. **Indent Cell**: ALWAYS has `e-indentcell`
   - CSS: `width: 17px` per indent level
   - JS depends on this class for group collapse/expand logic

5. **Frozen Cell**: In frozen column left table
   - Same CSS classes as normal
   - But rendered in separate `<table class="e-frozenheader">` or `<table>` (right scrollable)
   - aria-colindex MUST remain consistent across both tables

### Anti-Patterns That Break Cell Rendering

❌ **NEVER**:
- Remove `e-rowcell` from data cells (breaks selection, focus)
- Change `aria-colindex` after render (breaks column mapping)
- Duplicate `aria-colindex` in same row (breaks column lookup)
- Remove `data-uid` from row (breaks virtual scroll cache)
- Mix cell CellType in rendering (use CellRegister dispatcher)
- Render `<td>` without `aria-colindex` (breaks accessibility, selection)

---

## Prompt Template

<!-- token-budget: 300 words — Self-contained prompt for calling custom agent -->

```
Mode: feature-implementation | bug-fix
Skill: Cell Rendering

Context:
You are working on a task affecting cell rendering, DOM generation, or cell interactions in the Blazor Grid. 
This skill provides comprehensive knowledge of:
- All 12 CellType variants and their rendering pipelines
- Full DOM structure with aria attributes and CSS classes
- Cell rendering flow across sorting, filtering, grouping, editing, freezing, and virtual scroll
- Complete event ecosystem (QueryCellInfo, OnCellEdit, CellSelected, etc.)
- Critical CSS class and aria-colindex stability rules

Before making ANY changes:

1. IDENTIFY which CellType(s) are affected:
   - Is it a data cell (CellType.Data)? (selection checkboxes are Data cells with class `e-gridchkbox`)
   - Is it a header cell? (headers are NOT a CellType — they render via the separate GridHeaderCell.razor component)
   - Is it an edit cell? (editors render via src/Internal/Editors/*.razor, not a CellType)
   - Is it a special cell (Indent, Expand, GroupCaption, Summary)?

2. READ the corresponding renderer file:
   - Data → src/Internal/Renderer/CellRender.razor
   - Header → src/Internal/Renderer/GridHeaderCell.razor
   - Edit → src/Internal/Editors/*.razor
   - Indent, Expand, etc. → Specialized renderers

3. VALIDATE aria-colindex and CSS class stability:
   - Every cell MUST have aria-colindex matching column order
   - Data cells MUST retain e-rowcell class
   - Header cells MUST retain e-headercell class
   - Edit cells MUST retain e-editcell class
   - CSS classes are identity markers — removing them breaks JS listeners

4. CHECK cross-feature interactions:
   - If sorting: Does header cell retain aria-colindex?
   - If filtering: Does filter bar cell have correct aria-colindex?
   - If editing: Is edit cell focus properly JS-interoped?
   - If virtual scroll: Is cell cache invalidated per scroll event?
   - If frozen: Is aria-colindex consistent across left + right tables?

5. RUN regression tests:
   - Cell selection must work
   - Cell edit must enter/save/cancel
   - Sorting/filtering must not break aria-colindex
   - Frozen columns must sync width + aria-colindex
   - Virtual scroll must recycle cells correctly

6. CONSULT training docs:
   - Before making changes, read /docs/training/ for:
     * Feature-specific cell rendering rules
     * Known anti-patterns and past regression notes
     * Cross-feature interaction constraints

Return: Detailed summary of changes, test scenarios, and regression validation.
```

---

## References & Supporting Files

<!-- token-budget: 50 words -->

All 56 source files documented in companion Cell Skill source-file-catalog.md (delivered separately).

Key files:
- `src/Internal/Models/Cell.cs` (core model)
- `src/Internal/Renderer/GridRow.CellRenderer.cs` (rendering pipeline)
- `src/GridEvents.cs` (all cell events)
- `scripts/util.ts`, `scripts/edit.ts` (JS cell interaction)

---

**Generated**: 2026-04-09  
**Last Updated**: 2026-04-09  
**Status**: Ready for Feature Implementation & Bug Fix Tasks  
**Regression Guarantee**: VALIDATED ✅
