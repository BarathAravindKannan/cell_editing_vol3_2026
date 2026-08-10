---
name: column-headers-skill
description: Expert knowledge for Column Headers DOM rendering, component architecture, event flow, and feature integration in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to column header rendering, styling, interactions, overlays (filtering, sorting, grouping, reordering, resizing), virtualization, freezing, and event handling.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Column Headers in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing column header features, debugging header rendering issues, adding keyboard handlers, fixing event flow bugs, or resolving regressions involving headers with sorting, filtering, grouping, reordering, resizing, freezing, virtualization, or selection.

---

## Knowledge References
<!-- token-budget: 60 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/training/04-code-processing/optimal-chunking-strategies.md`
- `docs/training/05-practical-examples/feature-implementation-walkthrough.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`

---

## Training Insights Applied
<!-- token-budget: 100 words -->

Key rules and edge cases from `/docs/training/` that directly govern Column Headers work:

- **Header rendering is data-driven**: Column headers are rendered from `GridColumn` list, not hardcoded. Any header modification must update the `GridColumn` model, not direct DOM manipulation.
- **Frozen/movable split**: Headers are split into frozen and movable sections via `GridHeaderCell.IsFrozen` property. Both sections must stay synchronized (width, sort state, filter state).
- **Feature overlay stacking**: Headers support FilterBar rows, ColumnMenu overlays, and GroupDropArea above. Feature toggles must not corrupt the underlying header cell markup or CSS classes.
- **Virtual column tracking**: Column virtualization (`EnableColumnVirtualization = true`) translates visible columns via `VirtualGridHeaderParameters`. Header cell widths must remain consistent in collapsed state.
- **Header key handlers are critical**: Sort, Group, Reorder, and Freeze operations are triggered from header keyboard handlers (`Enter` → sort, `Ctrl+→/←` → reorder, `Ctrl+Space` → group). Never add header interactivity without re-checking `GridHeaderBase.OnKeyDown()`.
- **`RefreshColumnHeader` flag drives re-render**: Any grid property that affects headers (`AllowSorting`, `ShowColumnMenu`, `SortSettings`, `Columns`) must set `RefreshColumnHeader = true` in `PropertyChanges`.
- **No direct DOM queries in C#**: Header state management must not call `JS.InvokeAsync("querySelectorAll")` from C# — all DOM work goes through `GridJSInteropAdaptor<T>`.
- **Accessibility is mandatory**: All header cells must have correct `role`, `aria-sort`, `aria-label`, `aria-selected` attributes. Never remove or simplify ARIA markup.

---

## Core Header Rendering Logic (Critical Implementation Details)
<!-- token-budget: 200 words -->

### GridHeader.GetRows() — Header Row Generation (4 Branches)

**Branch 1: Stacked Header Mode** (`GridColumn.Columns` nested)
- Creates multiple header rows with rowspan/colspan calculated from nesting depth
- Top-level columns with children get `e-stackedheadercell` class
- Leaf columns span to bottom row

**Branch 2: Frozen Column Mode** (`IsFrozen = true`)
- Splits columns into frozen-left, movable, frozen-right sections
- Frozen columns rendered in separate `<table>` with identical `<thead>` structure
- Both tables must have synchronized `<colgroup>` widths

**Branch 3: Frozen Right Mode** (`Freeze = Right`)
- Right-frozen columns rendered after movable columns
- Scroll sync handled via JS interop

**Branch 4: Default Mode**
- Single header row, all columns visible, no frozen split

### GridHeader.GetCells() — Cell Generation with Frozen/Virtual Windowing

**Frozen Virtualization Logic:**
- When `EnableColumnVirtualization + FrozenColumns > 0`:
  - Frozen columns always included (indices 0 to `FrozenColumns-1`)
  - Movable columns windowed (indices `StartColumnIndex` to `EndColumnIndex`)
  - Frozen-right columns always included (last N columns)

**Special Cell Types Generated:**
- `CellType.Indent` — group indent cells (count = `GroupSettings.Columns.Count`)
- `CellType.DetailIndent` — detail template indent cell (if `DetailTemplate` exists)
- `CellType.RowDrag` — row drag handle cell (if `AllowRowDragAndDrop = true`)
- `CellType.StackedHeader` — nested header cell (if stacked headers active)
- `CellType.Data` — regular header cell

### GridHeader.GenerateCells() — Indent/Detail/RowDrag Cell Insertion

**Insertion Order (left to right):**
1. Detail indent cell (if `DetailTemplate != null`)
2. Row drag cell (if `AllowRowDragAndDrop = true`)
3. Group indent cells (count = `GroupSettings.Columns.Count`)
4. Data cells (from `GridColumn.Columns`)

### GridHeaderCell CSS Class Resolution

**Cell-Specific Classes:**
- `e-headercell` — all header cells
- `e-stackedheadercell` — stacked/nested header cells
- `e-grouptopleftcell` — group indent header cell
- `e-detailheadercell` — detail template indent header cell
- `e-rowdragheader` — row drag handle header cell
- `e-ascending` / `e-descending` — sort direction indicators
- `e-rsuppress` — resize disabled (when `AllowResizing = false`)

**Table Class Logic:**
```csharp
// GridHeader.razor adds "e-sortfilter" to <table> when:
if (AllowFiltering || AllowSorting || AllowGrouping)
{
    tableClass += " e-sortfilter";
}
```

### Header Keyboard Handler — GridHeaderBase.OnKeyDown()

**Supported Keys:**
- `Enter` → Sort column (calls `Sort<T>.SortColumn()`)
- `Shift+Enter` → Multi-sort (appends to sort list)
- `Ctrl+Enter` → Opens filter menu
- `Ctrl+Space` → Group/ungroup column
- `Ctrl+Right` → Reorder right (calls `Reorder<T>.PerformReorder()`)
- `Ctrl+Left` → Reorder left
- `Space` → Toggle checkbox select-all (if `Type = Checkbox`)
- `AltDown` → (reserved for future extension)
- `Tab` / `Shift+Tab` → Focus navigation (handled by `FocusHandler<T>`)

**Call Chain for Reorder:**
```csharp
OnKeyDown(Ctrl+Right) → Reorder<T>.PerformReorder() 
→ Parent.Columns.Move(sourceIndex, targetIndex) 
→ ModelChanged(RequestType = "reorder") 
→ GridEvents.ColumnReordered.InvokeAsync() 
→ RefreshColumnHeader = true 
→ StateHasChanged()
```

### Header Re-Render Optimization Flags

**`RefreshColumnHeader` Flag:**
- Set to `true` in `SfGrid.OnParametersSetAsync()` when:
  - `SortSettings.Columns` changed
  - `FilterSettings.Columns` changed
  - `GroupSettings.Columns` changed
  - `GridColumn.Columns` list modified
  - `AllowSorting` / `AllowFiltering` / `ShowColumnMenu` toggled
- **MUST be reset to `false` at end of `OnParametersSetAsync()`** to prevent infinite loop

**`IsColumnHeaderChange` Flag:**
- Used when `EnablePersistence = true` + `FrozenColumns > 0`
- Drives `SequenceEqual` comparison: `FrozenColumns.SequenceEqual(previousFrozenColumns)`
- If order changed (`!SequenceEqual`), triggers column index reset

### ShowAddNewRow in Header (`GridAddNewRow` Component)

**Rendering Condition:**
```csharp
if (ShowAddNewRow && NewRowPosition == NewRowPosition.Top)
{
    // GridAddNewRow.razor rendered INSIDE <thead> after header row
    // Generates <tr class="e-addnewrow"> with editable cells
}
```

### Frozen Row Virtual Mask (`EnableVirtualMaskRow` + `FrozenRows`)

**Rendering Path:**
```csharp
// GridHeader.razor renders <tbody> inside header table when:
if (EnableVirtualMaskRow && FrozenRows > 0)
{
    // <tbody> with class "e-frozenrows-tbody"
    //   └─ <tr class="e-masked-row"> × FrozenRows
    //      └─ <td class="e-masked-cell"> × VisibleColumns.Count
    //         └─ <span class="e-virtualcell"></span>
}
```

### HeaderCellInfo Event — Customization Hook Implementation

**Guard Pattern:**
```csharp
// GridHeader.razor before rendering each <th>:
if (Parent.GridEvents?.HeaderCellInfo.HasDelegate == true)
{
    var cellDOM = new CellDOM() { HasChanges = false };
    var args = new HeaderCellInfoEventArgs { Cell = cellDOM, Column = column };
    await Parent.GridEvents.HeaderCellInfo.InvokeAsync(args).ConfigureAwait(true);
    
    // Apply changes ONLY if cellDOM.HasChanges == true (optimization)
    if (cellDOM.HasChanges)
    {
        // Merge cellDOM.ClassList, cellDOM.Styles into <th>
    }
}
```

---

## Code Location Map
<!-- token-budget: 100 words -->

All Column Headers-related code lives at these paths (no code reproduced — paths + one-line purpose only):

### Razor Renderers (Presentation Layer)
| Path | Purpose |
|------|---------|
| `Internal/Renderer/GridHeader.razor` | Root header container — renders `<table><thead>`, iterates rows, mounts FilterBar, virtual frozen rows |
| `Internal/Renderer/GridHeader.razor.cs` | (does not exist; see GridHeaderBase) |
| `Internal/Renderer/GridHeaderBase.cs` | Base logic for header rendering — `AddSortIcon()`, `AddSortAria()`, key handlers |
| `Internal/Renderer/GridHeaderCell.razor` | Per-column `<th>` cell — renders header text, sort icon, filter/menu icons, resize handle, stacked header |
| `Internal/Renderer/GridHeaderCell.razor.cs` | (does not exist; logic in GridHeaderCell.razor) |
| `Internal/Renderer/GroupedHeader.razor` | Grouped header chip UI in GroupDropArea — displays group field + sort icon + ungroup button |
| `Internal/Renderer/WidthController.razor` | `<colgroup><col>` entries — enforces column widths; interacts with header resize handles |
| `Internal/Renderer/GridVirtualHeader.razor` | Virtual scroll header wrapper — translates header via CSS transform; manages `VirtualizedColumns` list |
| `Internal/Renderer/GroupDropArea.razor` | Drag-to-group zone above headers; hosts `GroupedHeader` chips |

### Feature Overlays on Headers
| Path | Purpose |
|------|---------|
| `Internal/Renderer/Filter/FilterBarRenderer.razor` | Renders `<tr class="e-filterbar">` row(s) below header cells — handles frozen/movable split |
| `Internal/Renderer/Filter/FilterInput.razor` | Individual filter input cell inside FilterBar row |
| `Internal/Renderer/Filter/FilterMenuRenderer.razor` | Menu-type filter dialog anchored to header cell's filter icon |
| `Internal/Renderer/ColumnMenu.razor` | Context menu dropdown triggered by column menu icon in header cell |
| `Internal/Renderer/ColumnChooser.razor` | Popup for show/hide columns — triggered from ColumnMenu |

### Business/Action Modules (Layer 3)
| Path | Purpose |
|------|---------|
| `Internal/Actions/Sort.cs` | Sort click handler called from `GridHeaderCell` `@onclick`; updates `SortSettings`; fires `DataBound` event |
| `Internal/Actions/Reorder.cs` | `PerformReorder()` reorders `Parent.Columns` list; triggers `RefreshColumnHeader` |
| `Internal/Actions/Filter.cs` | `FilterIconClicked` property set by header cell; triggers filter menu/dialog |
| `Internal/Actions/Group.cs` | Group/Ungroup from header key handler; manages `GroupSettings.Columns`; adds/removes indent cells |
| `Internal/Actions/FocusHandler.cs` | Keyboard focus coordination in header row (`Tab`, `Arrow` keys) |

### Data Models (Layer 2)
| Path | Purpose |
|------|---------|
| `Internal/Base/InternalClass.cs` | `GridHeaderParameters`, `GridHeaderCellParameters`, `FilterBarParameters`, `GridVirtualHeaderParameters` — parameter bags passed between components |
| `Internal/Models/Cell.cs` | `Cell<object>` — `CellType` (Data, Indent, DetailIndent, RowDrag, StackedHeader), `ColSpan`, `RowSpan`, `IsFrozen`, `IsStacked` |
| `Internal/Models/Row.cs` | `Row<object>` — carries `Cells` list, `Uid`, used in `GridHeader.GetRows()` |
| `GridColumn.cs` | `HeaderText`, `Uid`, `AllowSorting`, `AllowGrouping`, `AllowFiltering`, `AllowReordering`, `AllowResizing`, `IsFrozen`, `Freeze`, `CustomAttributes`, `HeaderTemplate`, `ShowColumnMenu`, `ShowInColumnChooser` |

### JavaScript Interop (Layer 1)
| Path | Purpose |
|------|---------|
| `scripts/resize.ts` | Resize class — targets `th.e-headercell`; clones for auto-fit; manages `e-rhandler` / `e-rcursor` divs |
| `scripts/header-drag-drop.ts` | `HeaderDragDrop` class — attaches `Draggable` to `.e-columnheader` rows; handles reorder and group drag |
| `scripts/reorder.ts` | Column reorder drop logic — calculates target column index from DOM |
| `scripts/freeze.ts` / `scripts/frozen-drag-drop.ts` | Frozen line cursor movement; freeze line drag interop |
| `scripts/sf-grid.ts` | `renderColumnMenu()` (line 259) — computes menu position relative to header cell; scroll sync |
| `scripts/scroll.ts` | Syncs header scroll position with content scroll |
| `scripts/sf-grid.ts` | `onHeaderClick`, `onHeaderKeyDown` — JS event handlers that call .NET callbacks |

### Grid Root & Properties (Layer 1 & 3)
| Path | Purpose |
|------|---------|
| `SfGrid.Properties.cs` | `AllowSorting`, `AllowFiltering`, `AllowGrouping`, `AllowReordering`, `AllowResizing`, `ShowColumnMenu`, `FrozenColumns`, `EnableColumnVirtualization`, `TextWrapSettings`, `ClipMode` — all control header rendering branches |
| `SfGrid.Lifecycle.cs` | `OnParametersSetAsync` — sets `RefreshColumnHeader`, `HasColumnChanges`, `ForceUpdate` flags |
| `SfGrid.Methods.cs` | Public async methods that modify headers: `GroupColumnAsync()`, `UngroupColumnAsync()`, `ReorderColumnsAsync()` |
| `Internal/SfGrid.razor` | Root markup — passes `HeaderParameters` into `GridHeader` via `CascadingValue` |

---

## DOM Rendering Architecture with ASCII Diagrams
<!-- token-budget: 200 words -->

### Diagram 1: Header DOM Structure (Complete Hierarchy)

```
<div class="e-gridcontent">
  ├─ <table class="e-headertable">
  │  └─ <thead class="e-gridheader">
  │     ├─ <tr class="e-columnheader">          ← Row 0: First header row (or stacked header)
  │     │  ├─ <th data-mappinguid="col-1" class="e-headercell e-ascending" tabindex="0">
  │     │  │  ├─ <span class="e-sortnumber">1</span>                   [sort order indicator]
  │     │  │  ├─ <div class="e-headertext">Column Header</div>         [header text/template]
  │     │  │  ├─ <div class="e-sort">▲</div>                           [sort icon]
  │     │  │  ├─ <div class="e-filtermenucell">≣</div>                 [filter icon]
  │     │  │  ├─ <div class="e-columnmenu">⋮</div>                     [column menu icon]
  │     │  │  ├─ <div class="e-rhandler"></div>                        [resize handle]
  │     │  │  ├─ <div class="e-fcursor"></div>                         [frozen line cursor (if frozen)]
  │     │  │  └─ <div class="e-stackedheader"></div>                   [stacked header (if nested)]
  │     │  ├─ <th class="e-headercell"> ... [Column 2] ... </th>
  │     │  └─ <th class="e-headercell"> ... [Column N] ... </th>
  │     │
  │     └─ <tr class="e-filterbar">                 ← OPTIONAL: Filter row (if AllowFiltering=true)
  │        ├─ <td class="e-filterbarcell">
  │        │  └─ <input type="text" class="e-filterinput" />
  │        ├─ <td class="e-filterbarcell"> ... </td>
  │        └─ <td class="e-filterbarcell"> ... </td>
  │
  ├─ <colgroup>                                      ← Column width enforcement
  │  ├─ <col style="width: 150px;" />
  │  ├─ <col style="width: 200px;" />
  │  └─ <col style="width: 100px;" />
  │
  └─ (Frozen columns: separate table with identical structure)

<div class="e-groupdroparea" style="display: flex;">  ← Group drop zone above header
  ├─ <div class="e-text">Drag here to group</div>
  └─ <div class="e-groupedheader">
     ├─ <div class="e-headercell">
     │  ├─ <span class="e-groupheadertext">Country</span>
     │  ├─ <div class="e-sort">▲</div>
     │  └─ <button class="e-ungroupbutton">✕</button>
     └─ <div class="e-groupedheader"> ... [nested grouped headers] ... </div>
```

**Key CSS Classes**:
- `e-columnheader` — row container
- `e-headercell` — individual cell (`<th>`)
- `e-headertext` — text content div
- `e-sort`, `e-ascending`, `e-descending` — sort indicators
- `e-rhandler` — resize handle div
- `e-fcursor` — frozen line cursor (only if `IsFrozen=true`)
- `e-stackedheader` — nested/grouped header
- `e-filterbar` — filter input row
- `e-filterbarcell` — filter cell
- `e-groupdroparea` — group drop zone
- `e-groupedheader` — grouped column chip

---

### Diagram 2: Header Rendering Flow (Component & Action Pipeline)

```
Code snippet in Markdown format:

User Action (e.g., click sort icon in header)
        │
        ▼
GridHeaderCell.razor @onclick handler fired
        │
        ▼
Sort<T>.SortClickHandler called via Parent.SortModule.SortColumn()
        │
        ├─ Check AllowSorting, AllowMultiSorting flags
        ├─ Update SortSettings.Columns list
        ├─ Fire GridEvents.OnActionBegin (user can Cancel)
        │
        ▼ (if not cancelled)
DataGenerator<T>.GenerateQuery()
        │
        ├─ Apply SortQuery via .SortBy() method
        ├─ (Filter is applied BEFORE sort in query pipeline)
        │
        ▼
SfDataManager.ExecuteQuery() — executes against adaptor
        │
        ▼
CurrentViewData updated with sorted results
        │
        ├─ EventAggregator.Trigger("DataBound", this)
        │
        ▼
Subscribers notified:
        ├─ ReactiveAggregate<T>: recalculate footer values
        ├─ Selection<T>: re-apply persistent selection to new row order
        ├─ VirtualScroll<T>: recalculate visible row range
        ├─ FocusHandler<T>: move focus to new row position
        │
        ▼
StateHasChanged() — Blazor renders affected components
        │
        ├─ GridContent.razor re-renders (new row order)
        ├─ GridHeader.razor re-renders (sort icon, aria-sort attribute)
        ├─ GridRow.razor re-renders for each visible row
        ├─ CellRender.razor re-renders each cell
        │
        ▼
GridEvents.OnActionComplete raised — user handler invoked
        │
        ▼
JS update: if virtual scroll active, recalculate scroll offset
        │
        ▼
Render complete — user sees sorted data


Rendering Layer Detail:

GridHeader.razor:
    │
    ├─ if (EnableColumnVirtualization) render GridVirtualHeader.razor
    │
    ├─ Iterate GridColumn.Columns via @foreach
    │
    ├─ For each column:
    │  └─ render GridHeaderCell.razor with:
    │     ├─ Column metadata (AllowSorting, AllowFiltering, AllowReordering)
    │     ├─ Sort state from SortSettings.Columns
    │     ├─ Filter state from FilterSettings.Columns
    │     ├─ IsFrozen property
    │
    └─ if (AllowFiltering) render FilterBarRenderer.razor after header row
```

---

### Diagram 3: Feature Overlays on Headers (FilterBar, ColumnMenu, GroupDropArea)

```
ASCII representation of feature layering:

┌────────────────────────────────────────────────────────────────────┐
│                      GROUP DROP AREA                               │
│  (if AllowGrouping=true, shown above header)                       │
│  ├─ [Grouped Header Chip 1] [Grouped Header Chip 2]               │
│  └─ "Drag here to group" placeholder                              │
└────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│                    COLUMN HEADER ROW                               │
│  (e-columnheader <tr> — rendered by GridHeader.razor)              │
│                                                                    │
│  ┌──────────┬──────────┬──────────┬──────────┐                    │
│  │ [Sort▲]  │ [Filter▲]│[Menu▮]   │Resize┃  │  ← Header cell    │
│  │ Country  │ City     │Product   │Price │  │     (each <th>)  │
│  └──────────┴──────────┴──────────┴──────────┘                    │
│       │           │          │          │                        │
│       │           │          │          └─ @onclick → Resize.ts  │
│       │           │          └───────── @onclick → ColumnMenu    │
│       │           └──────────────────── @onclick → FilterMenu    │
│       └──────────────────────────────── @onclick → Sort module   │
└────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌────────────────────────────────────────────────────────────────────┐
│               FILTER BAR ROW (Optional)                            │
│  (e-filterbar <tr> — rendered by FilterBarRenderer.razor)          │
│  (only if AllowFiltering=true)                                    │
│                                                                    │
│  ┌──────────────┬──────────────┬──────────────┐                  │
│  │ [search...]  │ [search...]  │ [search...]  │ ← Filter input   │
│  │ Country      │ City         │ Product      │    cells         │
│  └──────────────┴──────────────┴──────────────┘                  │
│       │              │               │                           │
│       └─ @oninput ──→ Filter module ──→ FilterSettings.Columns   │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

**Feature Overlay Interaction Rules:**
- **GroupDropArea above header**: Allows drag-drop of header cells to group. If active, header row visibility and z-index must not be affected.
- **ColumnMenu (overlay)**: Triggered by menu icon in header cell. Dropdown positioned via `renderColumnMenu()` JS function. Must not interfere with header scroll.
- **FilterMenu (overlay or embedded)**: Can be a dialog anchored to filter icon or an embedded filter bar row. FilterBar row row must stay synchronized with header scroll.
- **Virtual scroll**: FilterBar row must update visible columns in sync with header row when `EnableColumnVirtualization=true`.

---

### Diagram 4: Virtual Column Scroll (Column Virtualization)

```
User scrolls header horizontally (left-right)
        │
        ▼
JS scroll event fired on header scroll container
        │
        ▼
GridJSInteropAdaptor.OnHeaderScroll() invoked
        │
        ▼
GridVirtualHeader.razor receives scroll offset (translateX)
        │
        ├─ Calculate visible column range
        ├─ Update VirtualizedColumns list (only visible columns)
        ├─ Apply CSS transform: transform: translateX(-{offset}px)
        │
        ▼
GridHeaderCell.razor re-renders only for visible columns
        │
        ├─ Hidden columns (left/right offscreen) not in DOM
        ├─ Visible columns render with correct header text, sort icons, resize handles
        │
        ▼
WidthController.razor:
        │
        ├─ <colgroup> adjusts <col> entries for visible columns only
        ├─ <col> widths must match the virtualized header cells
        │
        ▼
Content scroll (vertical) must stay in sync with header scroll (horizontal)
        │
        └─ Both use the same left offset value (managed by VirtualScroll<T>)
```

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 180 words -->

> Built from live feature cross-reference + `/docs/training/` risk analysis.  
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| Headers + Sorting | Header click handler calls `Sort<T>.SortColumn()`. Sort icon updates must render via `GridHeaderCell.razor`. Sort order must be visible in header. | Critical |
| Headers + Filtering | FilterBar row renders below header when `AllowFiltering=true`. Filter icon click must open filter menu/dialog. Filter state must persist in header cell ARIA attributes. | Critical |
| Headers + Grouping | GroupDropArea above header allows drag-drop of columns. Grouped header chips render above header row. Header cells must remain draggable even when grouped. | Critical |
| Headers + Reordering | Header cells must be draggable (via `header-drag-drop.ts`). Reorder target column index calculated from header DOM. Column order in header must match `GridColumn.Columns` list after reorder. | Critical |
| Headers + Resizing | Resize handle (e-rhandler div) in each header cell. Drag resize must update `WidthController.razor` <col> widths. Header and content columns must stay aligned. | Critical |
| Headers + Frozen Columns | Frozen columns rendered in separate header table with identical structure. Frozen header row must sync scroll state and CSS classes with movable header row. Frozen line cursor must remain visible during scroll. | Critical |
| Headers + Virtualization (Column) | When `EnableColumnVirtualization=true`, header row rendered by `GridVirtualHeader.razor`. Only visible columns in DOM. Hidden columns (left/right) must not break header layout. | Critical |
| Headers + Virtualization (Row) | Row virtualization does NOT affect header rendering — header is always fully rendered regardless of row virtualization status. No interaction risk. | Low |
| Headers + Infinite Scroll | Infinite scroll loads new rows at bottom; does not affect header. Header must remain stable and scrollable independently. | Low |
| Headers + Selection | Header checkbox (if present) must clear/select all visible rows. Header row must support keyboard focus and Tab navigation. | High |
| Headers + Editing | Edit mode (Normal/Dialog/Batch) does not render inside header row. Header must remain visible and interactive during edit. Tab from header must navigate to first editable row. | High |
| Headers + Aggregates | Footer aggregate rows render below content. Header ARIA attributes (aria-colindex) must match aggregate column indices. | Medium |
| Headers + Paging | Page size change does not affect header rendering. Header must remain stable across page transitions. | Low |
| Headers + Column Menu | ColumnMenu overlay triggered by menu icon in header cell. Menu position computed via `renderColumnMenu()` JS. Menu must not hide header content or break scrolling. | Medium |
| Headers + Column Chooser | ColumnChooser dialog triggered from ColumnMenu. Hiding columns via ColumnChooser must immediately hide corresponding header cells. | Medium |
| Headers + Text Wrap | TextWrapSettings applied to header cells. Header height must expand if text wraps. Content column widths must not shift due to header wrap. | Medium |
| Headers + Custom Attributes | CustomAttributes on GridColumn must apply to header `<th>` element. Header-specific attributes (data-mappinguid, role) must not be overwritten. | Medium |
| Headers + Export | Export must include header row in output. Sort indicators and filter state must be reflected in export. | Low |
| Headers + Persistence | `EnablePersistence=true`: column order, sort state, filter state persisted and restored. Header must reflect restored state on reload. | Medium |

---

## Event Flow & Event Models (MANDATORY)
<!-- token-budget: 250 words -->

### Header-Related Events (Firing Order During User Actions)

**Event Flow Chain:**

```
User Interaction (click/key/drag in header)
        │
        ▼
GridHeaderCell.razor @onclick / @onkeydown fired
        │
        ├─ OnActionBegin (User can Cancel=true to prevent)
        │  [ActionEventArgs<TValue>]
        │  ├─ RequestType: "sort" | "reorder" | "filtering" | "grouping" | "freeze" | "resize"
        │  ├─ Data: row data (null for header operations)
        │  └─ Cancel: set by event handler to abort
        │
        ▼ (if not cancelled)
Action module invoked (Sort<T> | Reorder<T> | Filter<T> | Group<T>)
        │
        └─→ Query updated (Sort, Filter, or Group applied)
        │
        ▼
EventAggregator.NotifyAsync("DataBound")
        │
        ├─ Sorting event (if sort action)
        │  [SortingEventArgs]
        │  ├─ ColumnName: field name being sorted
        │  ├─ Direction: Ascending | Descending
        │  └─ Cancel: can cancel sort
        │
        ├─ Sorted event (after sort completes)
        │  [SortedEventArgs]
        │
        ├─ ColumnReordering event (if reorder action)
        │  [ColumnReorderingEventArgs]
        │  ├─ FromColumn: source column
        │  └─ Cancel: can cancel reorder
        │
        ├─ ColumnReordered event (after reorder completes)
        │  [ColumnReorderedEventArgs]
        │  ├─ ToColumn: destination column
        │
        ├─ ColumnVisibilityChanging event (if show/hide from menu)
        │  [ColumnVisibilityChangingEventArgs]
        │
        ├─ HeaderCellInfo event (before header cell renders)
        │  [HeaderCellInfoEventArgs]
        │  └─ Cell: CellDOM object with AddClass(), AddStyle(), SetAttribute()
        │
        ▼
DataBound event fires
        │
        ▼
OnActionComplete fires
        │
        ▼
GridHeader.razor re-renders → GridHeaderCell.razor updates sort icon, ARIA, CSS classes
```

### Key Event Models for Headers

| Event | EventArgs Class | Fired When | Cancellable |
|-------|-----------------|-----------|------------|
| `Sorting` | `SortingEventArgs` | Before sort action | ✅ Yes (`args.Cancel = true`) |
| `Sorted` | `SortedEventArgs` | After sort completes | ❌ No |
| `Grouping` | `GroupingEventArgs` | Before group action | ✅ Yes |
| `Grouped` | `GroupedEventArgs` | After group completes | ❌ No |
| `Filtering` | `FilteringEventArgs` | Before filter action | ✅ Yes |
| `Filtered` | `FilteredEventArgs` | After filter completes | ❌ No |
| `ColumnReordering` | `ColumnReorderingEventArgs` | Before reorder | ✅ Yes |
| `ColumnReordered` | `ColumnReorderedEventArgs` | After reorder | ❌ No |
| `ColumnVisibilityChanging` | `ColumnVisibilityChangingEventArgs` | Before hide/show | ✅ Yes |
| `ColumnVisibilityChanged` | `ColumnVisibilityChangedEventArgs` | After hide/show | ❌ No |
| `HeaderCellInfo` | `HeaderCellInfoEventArgs` | Before header cell renders | ❌ No |
| `FilterDialogOpening` | `FilterDialogOpeningEventArgs` | Before filter menu opens | ✅ Yes |
| `FilterDialogOpened` | `FilterDialogOpenedEventArgs` | After filter menu opens | ❌ No |
| `OnColumnMenuOpen` | `ColumnMenuOpenEventArgs` | Before column menu opens | ✅ Yes |
| `ColumnMenuItemClicked` | `ColumnMenuClickEventArgs` | After menu item clicked | ❌ No |
| `OnActionBegin` | `ActionEventArgs<TValue>` | Before any grid action | ✅ Yes |
| `OnActionComplete` | `ActionEventArgs<TValue>` | After any grid action | ❌ No |
| `OnResizeStart` | `ResizeArgs` | Before column resize | ✅ Yes |
| `ResizeStopped` | `ResizeArgs` | After column resize | ❌ No |

### Header Event Guard Checks (Critical for Interop)

```csharp
// Pattern used in Sort.cs, Reorder.cs, Filter.cs:

// Check if event has delegate before invoking (prevents null-ref):
if (Parent.GridEvents?.Sorting.HasDelegate == true)
{
    var args = new SortingEventArgs { /* ... */ };
    await Parent.GridEvents.Sorting.InvokeAsync(args).ConfigureAwait(true);
    if (args.Cancel) return; // User cancelled — abort action
}

// After action, fire OnActionComplete:
if (Parent.GridEvents?.OnActionComplete.HasDelegate == true)
{
    await Parent.GridEvents.OnActionComplete.InvokeAsync(
        new ActionEventArgs<TValue>() 
        { 
            RequestType = "sort", 
            Parent = Parent 
        }
    ).ConfigureAwait(true);
}

// Also trigger EventAggregator subscribers:
await Parent.EventAggregator.NotifyAsync("DataBound", args).ConfigureAwait(true);
```

### HeaderCellInfo Event (Customization Hook)

```csharp
// GridHeader.razor calls HeaderCellInfo before rendering each <th>:

if (Parent.GridEvents?.HeaderCellInfo.HasDelegate == true)
{
    var cellDOM = new CellDOM() 
    { 
        HasChanges = false, 
        ClassList = new List<string>(), 
        Styles = new Dictionary<string, string>() 
    };
    
    var args = new HeaderCellInfoEventArgs 
    { 
        Cell = cellDOM, 
        Column = column 
    };
    
    await Parent.GridEvents.HeaderCellInfo.InvokeAsync(args).ConfigureAwait(true);
    
    // Apply custom classes, styles from cellDOM:
    if (cellDOM.HasChanges)
    {
        // Merge cellDOM into header cell rendering
    }
}
```

---

## Prompt Template
<!-- token-budget: 350 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Column Headers
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Column Headers feature.
Scope: Internal/Renderer/GridHeader.razor*, Internal/Renderer/GridHeaderCell.razor*, 
        Internal/Renderer/WidthController.razor, Internal/Renderer/GridVirtualHeader.razor,
        Internal/Actions/Sort.cs (header handler), Internal/Actions/Reorder.cs (header handler),
        Internal/Actions/Filter.cs (header handler), scripts/resize.ts, scripts/header-drag-drop.ts

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Root header component: GridHeader.razor — renders <table><thead> with <tr class="e-columnheader"> rows
- Per-column header cell: GridHeaderCell.razor — renders <th> with sort icon, filter icon, menu icon, resize handle
- Feature overlay containers: FilterBarRenderer.razor (filter row), ColumnMenu.razor, GroupDropArea.razor
- Header interactivity routes: 
  * Click sort icon → Sort<T>.SortColumn() → fires Sorting / Sorted events
  * Click filter icon → Filter<T>.HandleFilterMenu() → fires FilterDialogOpening / FilterDialogOpened
  * Click menu icon → ColumnMenu.razor → fires OnColumnMenuOpen / ColumnMenuItemClicked
  * Drag header → Reorder<T> → fires ColumnReordering / ColumnReordered
  * Drag resize handle → Resize.ts → fires OnResizeStart / ResizeStopped
  * ALL actions → OnActionBegin (cancellable) → OnActionComplete (after)
- Virtual column headers: GridVirtualHeader.razor wraps header, applies CSS translateX, manages visible column list
- Column width enforcement: WidthController.razor renders <colgroup><col> entries that must sync with header cell widths
- Frozen headers: Separate <table> for frozen columns with identical header structure
- CSS classes: e-columnheader (row), e-headercell (<th>), e-sort (sort icon), e-ascending/e-descending (sort state), e-rhandler (resize), e-fcursor (frozen line)
- ARIA attributes: aria-sort (ascending|descending|none), aria-label (for accessibility), aria-colindex (for logical order)
- Event pipeline: OnActionBegin (user can Cancel) → Action → EventAggregator.Trigger("DataBound") → subscribers → StateHasChanged()
- Event guard pattern: Always check HasDelegate before invoking: `if (Parent.GridEvents?.EventName.HasDelegate == true)`

=== KEY BEHAVIORS ===
1. **Data-driven rendering** — Headers rendered from GridColumn.Columns list, never hardcoded DOM.
2. **Frozen/movable split** — Frozen columns in separate <table>; both must stay synchronized (widths, sort state, CSS).
3. **Keyboard handlers** — Enter → sort, Ctrl+→/← → reorder, Ctrl+Space → group, Space → select-all checkbox.
4. **Event pipeline** — OnActionBegin (cancellable) → Action (Sort/Filter/Group/Reorder) → DataBound → OnActionComplete.
5. **Virtual column windowing** — EnableColumnVirtualization = true: only visible columns in DOM; translateX positioning.
6. **Feature overlay stacking** — FilterBar row below header; ColumnMenu overlay on click; GroupDropArea above header.
7. **RefreshColumnHeader optimization** — Flag triggers header-only re-render when sort/filter/column state changes.
8. **ARIA mandatory** — All <th> must have role="columnheader", aria-sort, aria-label, aria-colindex.
9. **Stacked headers** — Multi-level headers use rowspan/colspan; leaf columns span to bottom row; GetRows() has 4 branches.
10. **Special cell types** — Indent (group), DetailIndent, RowDrag, StackedHeader cells auto-inserted left of data cells.
11. **Table class logic** — <table> gets "e-sortfilter" class when AllowFiltering || AllowSorting || AllowGrouping.
12. **RefreshColumnHeader reset** — MUST be set to false at end of OnParametersSetAsync() to prevent infinite loop.

=== BEFORE YOU MAKE ANY CHANGE ===
1. Consult /docs/training/02-requirements-analysis/understanding-requirements.md for edge cases.
2. Consult the Interaction Matrix above (sections on Sorting, Filtering, Grouping, Reordering, Virtualization, Frozen Columns).
3. Confirm all header cells use `e-headercell` class, `e-columnheader` row class, and correct ARIA attributes.
4. Confirm frozen header table structure matches movable header table structure exactly.
5. Confirm virtual column scroll offset is applied uniformly to all header cells in visible range.
6. Confirm FilterBar row stays synchronized with header scroll position and column visibility.
7. Confirm resize handle width does not corrupt header text alignment.
8. Confirm event firing order: OnActionBegin → feature action (Sort/Filter/Group/Reorder) → DataBound → OnActionComplete.

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all modified public members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() in header rendering — use grid internal scheduling
- No direct JSRuntime.InvokeAsync outside GridJSInteropAdaptor<T>
- No DOM queries from C# (no querySelectorAll, getElementById) — only JS-interop bridge
- Follow naming-conventions.md: PascalCase methods, _camelCase private fields, no abbreviations
- Follow error-handling.md: guard clauses first, no silent catch blocks, always log JS-interop failures
- All header cells must have correct role (columnheader), aria-sort, aria-label, aria-colindex attributes
- Frozen/movable header tables must have identical DOM structure — differences only in CSS classes and data attributes
- Column virtualization must not break unfrozen header rendering when disabled
- ALL event invocations must use HasDelegate guard: `if (Parent.GridEvents?.EventName.HasDelegate == true)`

=== SCOPE ===
{Describe the exact method(s) to implement or fix — one rendering area or action handler per task}

=== INPUT ===
{Paste the extracted code chunk — see training/04-code-processing/optimal-chunking-strategies.md}

=== OUTPUT ===
1. Root cause / design rationale (3–5 sentences)
2. Modified method(s) and Razor fragments only — no full-file reproduction
3. Interaction risk flags (reference Interaction Matrix section)
4. Event flow verification: which events fire in what order, cancellation points
5. Required test cases (Given-When-Then format) including:
   - Basic scenario (header renders correctly)
   - Feature interaction (header + sorting, header + filtering, header + virtualization, etc.)
   - Event cancellation (OnActionBegin Cancel=true prevents action)
   - Regression check (header behavior with each feature disabled)
6. Required DOM structure verification (ASCII diagram of expected HTML output)

After implementation, run regression verification using:
/docs/ai-agents/prompts/regression-verification-prompt.md
```

---

## TypeScript Implementation Reference (DOM-Level Operations)
<!-- token-budget: 300 words -->

> All TypeScript classes operate on live DOM elements in the header. Never replicate these operations in C#.

---

### `HeaderDragDrop` (`scripts/header-drag-drop.ts`)

**Initialization Guard:**
- Only initialized when `options.allowGrouping || options.allowReordering`
- `initializeHeaderDrag()` skipped when `!allowReordering && !(allowGrouping && showDropArea)`

**Draggable Setup on `.e-columnheader` rows:**
```
new Draggable(headerRow, {
    dragTarget: '.e-headercell',       ← only headercell starts a drag
    distance: 5,                       ← 5px threshold before drag activates
    abort: '.e-rhandler',              ← drag ABORTED if mousedown on resize handle
    helper: this.helper,               ← creates .e-cloneproperties.e-dragclone.e-headerclone
})
```

**`helper()` — Drag Clone Element DOM:**
```
<div class="e-cloneproperties e-dragclone e-headerclone"
     e-mappinguid="{columnUid}"
     style="width:{col.offsetWidth}px; height:{col.offsetHeight}px; line-height:{h-6}px">
  {column.headerText || column.field}
</div>
```
- Appended directly to `gObj.element` (grid root), NOT inside header
- Returns `false` (cancels drag) when: `column.fixedColumn`, `e-checkselectall` checkbox column, `e-freezeline-moving` active, stacked header with all children locked

**`drag()` — Live Drag Cursor Logic:**
- Outside grid bounds → adds `e-notallowedcur`, removes `e-defaultcur`, hides arrows
- Valid drop target → adds `e-allowDrop e-reorderindicate` to target `<th>`

**`drop()` — calls:**
- `reorderModule.headerDrop({ target })` — triggers column reorder
- `groupModule.columnDrop({ target, droppedElement })` — triggers group drop

---

### `Reorder` (`scripts/reorder.ts`)

**Reorder Arrow DOM (created in `.e-headercontent`):**
```
<div class="e-icons e-icon-reorderuparrow e-reorderuparrow" style="display:none"></div>
<div class="e-icons e-icon-reorderdownarrow e-reorderdownarrow" style="display:none"></div>
```
- For column virtualization: classes replaced with `e-reorderuparrow-virtual` / `e-reorderdownarrow-virtual`
- Arrow `top`/`left` positioned absolutely relative to grid element during drag

**`moveColumns()` → .NET Call Chain:**
```
cols.splice(destIndex, 0, cols.splice(srcIdx, 1)[0])   ← mutates options.columns in-place
gObj.getColumns(true)                                    ← refreshes column model
setTimeout(() => {
    dotNetRef.invokeMethodAsync('ColumnReordered', {
        requestType: 'reorder',
        fromIndex: destIndex,
        toIndex: srcIdx,
        toColumnUid: column.uid
    })
}, 10)                                                   ← 10ms delay for render to settle
```

**`processStackedColumns()` — Fixed Column Ordering:**
- Splits columns into `fixedColumns[]` + `unfixedColumns[]`
- `unfixedColumns` sorted by `column.index` before merging
- Result: fixed columns always appear first in header order

**`headerDrop()` — `e-mappinguid` Resolution:**
- Reads `e-mappinguid` from `.e-headercelldiv` or `.e-stackedheadercelldiv`
- Uses `getColumnByUid(uId)` to get Column model
- Guards: `!column.allowReordering`, `column.fixedColumn`, `isStackedColumnLocked` all prevent drop

---

### `Resize` (`scripts/resize.ts`)

**Resize CSS Constants (`resizeClassList`):**
```typescript
{
    root:     'e-rhandler',    ← resize handle div inside <th>
    suppress: 'e-rsuppress',   ← handle when AllowResizing=false (height set but no drag)
    icon:     'e-ricon',       ← touch device icon state
    helper:   'e-rhelper',     ← vertical drag line helper div
    header:   'th.e-headercell', ← CSS selector for parent <th>
    cursor:   'e-rcursor'      ← cursor class added to all <th> during resize drag
}
```

**`resizeStart()` → .NET Call:**
```
touchstart on .e-rhandler
    → appendHelper() creates <div class="e-rhelper"> positioned at <th> right edge
    → getTargetColumn() reads .e-headercelldiv[e-mappinguid] to find Column
    → dotNetRef.invokeMethodAsync('ResizeStarted', { columnUid, columnList })
    → preventResizeAction(isCancel) called from .NET
        → if !cancel: adds touchmove listener, adds e-rcursor to all <th>
```

**`resizeEnd()` → .NET Call:**
```
touchend
    → detach(helper)
    → closest(<th>).classList.add('e-resized')
    → dotNetRef.invokeMethodAsync('ColumnWidthChanged', {
          width, columnUid, allowStopEvent,
          tableWidth, leftFrozenTableWidth, rightFrozenTableWidth
      })
    → updateColumnWidth(columnModel) — sync <col> widths
    → addTableBorderClass()
```

**`autoFitColumns()` — Clone Technique:**
```
For each column to auto-fit:
    1. Clone <th> → headerTextClone (deep clone)
    2. Clone all <td> in that column → contentTextClone[]
    3. createTable(): build hidden off-screen <table class="e-resizetable">
                      style="table-layout:auto; width:auto; visibility:hidden; position:absolute"
    4. Measure table.getBoundingClientRect().width
    5. Set column.width = max(headerWidth, contentWidth, footerWidth)
    6. widthService.setColumnWidth() updates <col> element
```

**Frozen Column Resize — `frozenHeaderRefresh()`:**
- After resize of frozen-left column: recalculates `sticky left` CSS for all `.e-leftfreeze` and `.e-fixedfreeze` `<th>` cells
- After resize of frozen-right column: recalculates `sticky right` CSS for `.e-rightfreeze` and `.e-fixedfreeze` `<th>` cells
- Uses `applyStickyLeftRightPosition(node, offsetPx, enableRtl, direction)` utility

**`refreshResizeFrozenColumns()` — translateX Adjustment:**
- When `enableColumnVirtualization + frozenColumns`: reads current `translateX` from `.e-virtualtable` CSS transform
- Subtracts `translateX` from sticky `left` values to compensate for virtual scroll offset

---

### `Freeze` (`scripts/freeze.ts`)

**`refreshFreeze()` — Header Row Height Sync:**
- Called when `textwrap` or `refreshHeight` case
- Reads frozen header `<tr>` heights + movable header `<tr>` heights
- Sets both rows to `maxHeight = Math.max(fRowHgt, mRowHgt, frRowHgt)`
- For `LeftRight` freeze: includes third set of rows from right frozen header

**`refreshStackedHdrHgt()` — Stacked Header Height Sync:**
- Finds all `.e-columnheader` rows in frozen + movable + right-frozen headers
- `updateStackedFrozenHeight()`: for rows with `rowSpan > 1`, sums heights of spanned rows
- Sets merged cell's `style.height` to match combined row height
- Ensures stacked headers align visually across frozen/movable split

**`updateFrozenColumnStyles()` — Sticky Left Position Init:**
- Iterates all header `<tr>` rows, finds `.e-leftfreeze` `<th>` cells
- Sets `style.left = calculatedLeftValue(j-1) + 'px'` — sum of all preceding frozen column widths

**`updateResizeHandler()` — Resize Handle Height Update:**
- After frozen column resize, updates `style.height` on all `.e-rhandler` elements in header
- Height = parent `<th>` row height

---

### `Scroll` (`scripts/scroll.ts`)

**Header ↔ Content Scroll Sync:**
```
content scroll (left) change
    → scroll.ts syncronizes header scroll position
    → header.scrollLeft = content.scrollLeft
    → prevents header/content misalignment during horizontal scroll
```

**Frozen Header Padding (`setPadding()`):**
- When vertical scrollbar visible: adds right padding to frozen header to prevent overlap
- When scrollbar hidden: removes padding

**Frozen Rows Height (`setHeight()`):**
- When `frozenRows > 0 && height !== 'auto'`: reads `tbody` in `.e-headercontent`
- Subtracts `tbody.offsetHeight` from grid `height` to set `.e-content` height correctly

---

## Additional DOM Diagrams (Frozen & Stacked Headers)
<!-- token-budget: 150 words -->

### Diagram 5: Frozen Header DOM (Left + Movable + Right Split)

```
<div class="e-gridheader">
  │
  ├─ <div class="e-frozenheader e-frozen-left-header">      ← Frozen-Left columns
  │     <table class="e-table">
  │       <colgroup> <col width="150px"> </colgroup>
  │       <thead>
  │         <tr class="e-columnheader">
  │           <th class="e-headercell e-leftfreeze"
  │               style="left: 0px; position: sticky">      ← sticky left: 0
  │             <div class="e-headercelldiv e-headertext">CustomerID</div>
  │             <div class="e-rhandler"></div>
  │           </th>
  │         </tr>
  │         <tr class="e-filterbar"> ... </tr>
  │       </thead>
  │     </table>
  │  </div>
  │
  ├─ <div class="e-movableheader">                           ← Movable columns (scrollable)
  │     <table class="e-table">
  │       <colgroup> <col width="200px"> <col width="180px"> </colgroup>
  │       <thead>
  │         <tr class="e-columnheader">
  │           <th class="e-headercell"> ... City ... </th>
  │           <th class="e-headercell"> ... Country ... </th>
  │         </tr>
  │         <tr class="e-filterbar"> ... </tr>
  │       </thead>
  │     </table>
  │  </div>
  │
  └─ <div class="e-frozenheader e-frozen-right-header">      ← Frozen-Right columns
        <table class="e-table">
          <colgroup> <col width="120px"> </colgroup>
          <thead>
            <tr class="e-columnheader">
              <th class="e-headercell e-rightfreeze"
                  style="right: 0px; position: sticky">     ← sticky right: 0
                <div class="e-headercelldiv e-headertext">Actions</div>
                <div class="e-rhandler"></div>
              </th>
            </tr>
            <tr class="e-filterbar"> ... </tr>
          </thead>
        </table>
     </div>

Key Classes for Frozen Headers:
  e-frozenheader           ← container div for frozen section
  e-frozen-left-header     ← left frozen header container
  e-frozen-right-header    ← right frozen header container
  e-movableheader          ← movable (scrollable) header container
  e-leftfreeze             ← <th> class for left-frozen cell
  e-rightfreeze            ← <th> class for right-frozen cell
  e-fixedfreeze            ← <th> class for Fixed freeze direction
  e-freezeline-moving      ← class added to content during freeze line drag
```

---

### Diagram 6: Stacked Header DOM (Multi-Level Headers with rowspan/colspan)

```
SfGrid with stacked columns:
  <GridColumn HeaderText="Order Info">
    <GridColumn Field="OrderID" />
    <GridColumn Field="OrderDate" />
  </GridColumn>
  <GridColumn Field="CustomerID" />

Rendered DOM:
<thead>
  <tr class="e-columnheader">                         ← Row 0 (top level)
    │
    ├─ <th class="e-headercell e-stackedheadercell"
    │       colspan="2" rowspan="1"                   ← spans 2 child columns
    │       data-mappinguid="{stackedUid}">
    │    <div class="e-stackedheadercelldiv">Order Info</div>
    │  </th>
    │
    └─ <th class="e-headercell"
            rowspan="2"                               ← spans both header rows
            data-mappinguid="{customerUid}">
         <div class="e-headercelldiv e-headertext">CustomerID</div>
         <div class="e-rhandler"></div>
       </th>

  <tr class="e-columnheader">                         ← Row 1 (leaf columns)
    │
    ├─ <th class="e-headercell"
    │       data-mappinguid="{orderIdUid}">
    │    <div class="e-headercelldiv e-headertext">OrderID</div>
    │    <div class="e-sort"></div>
    │    <div class="e-rhandler"></div>
    │  </th>
    │
    └─ <th class="e-headercell"
            data-mappinguid="{orderDateUid}">
         <div class="e-headercelldiv e-headertext">OrderDate</div>
         <div class="e-sort"></div>
         <div class="e-rhandler"></div>
       </th>

Rules:
  - Stacked parent: colspan = count of all leaf children
  - Stacked parent: rowspan = 1 (only spans its own row)
  - Leaf column: rowspan = total header rows - depth of leaf column
  - CustomerID has rowspan=2 because it has no children and total rows = 2
  - GetRows() calculates depth via recursive column traversal
  - Freeze.refreshStackedHdrHgt() syncs row heights across frozen/movable split
```

---

### Diagram 7: Reorder Arrow DOM + Drag Clone DOM

```
During column drag (after HeaderDragDrop.initializeHeaderDrag()):

Grid element (e-grid):
  │
  ├─ <div class="e-headercontent">
  │    ├─ <div class="e-icons e-icon-reorderuparrow e-reorderuparrow"
  │    │        style="display:none; position:absolute; top:{Ypx}; left:{Xpx}">
  │    │  </div>
  │    └─ <div class="e-icons e-icon-reorderdownarrow e-reorderdownarrow"
  │             style="display:none; position:absolute; top:{Ypx}; left:{Xpx}">
  │       </div>
  │
  └─ <div class="e-cloneproperties e-dragclone e-headerclone"
             e-mappinguid="{columnUid}"
             style="width:{px}; height:{px}; line-height:{h-6}px;
                    position:absolute; visibility:hidden">
       {column.headerText}
     </div>

State machine during drag:
  mousedown on .e-headercell (not .e-rhandler)
      │  distance >= 5px
      ▼
  dragStart() → clone element created, appended to grid root
      │
      ▼
  drag() → arrows shown at drop target position
         → e-notallowedcur if outside grid or column.fixedColumn
         → e-reorderindicate on valid drop target <th>
      │
      ▼
  dragStop() / drop()
      → remove(e.helper) — clone detached
      → reorderModule.headerDrop() — moves column in model
      → dotNetRef.invokeMethodAsync('ColumnReordered', {...})
      → arrows hidden (display:none)
```

---

## Additional Reference: Complete Feature Interaction Quick Map
<!-- token-budget: 50 words -->

**To quickly identify interaction risks, cross-reference the feature you are implementing with this map:**

- **Rendering depends on**: GridColumn list, SortSettings, FilterSettings, GroupSettings, SelectedRows, FrozenColumns, EnableColumnVirtualization
- **Affects**: Sort click handler, Filter menu position, Reorder drag target, Freeze line cursor, Virtual scroll offset
- **Must sync with**: WidthController (column widths), FocusHandler (keyboard navigation), Selection (checkbox header)
- **Events to fire**: OnActionBegin → {Sorting|Filtering|Grouping|ColumnReordering} → {Sorted|Filtered|Grouped|ColumnReordered} → OnActionComplete

---
