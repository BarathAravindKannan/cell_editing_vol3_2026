---
name: grouping-dom-skill
description: Expert knowledge for the exact DOM structure, rendering pipeline, and feature-combination output of Grouping in the Syncfusion Blazor DataGrid. Use this skill when generating, validating, or debugging grouped-grid DOM, including caption rows, indent cells, expand or collapse cells, group footer rows, grouped chips, and all cross-feature DOM changes with lazy loading, virtualization, frozen columns, paging, infinite scrolling, detail rows, and row drag-and-drop.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for the **full grouping DOM model** in the Blazor DataGrid. Includes full embedded references for DOM structure, rendering flow, and feature combinations.

---

**Agent Invocation**  
Load this skill when implementing, reviewing, documenting, or debugging:
- Grouping DOM structure
- Caption row rendering
- Indent and expand cell alignment
- Caption/footer `colspan` calculation
- Group drop area chip DOM
- Grouping + virtualization/lazy/frozen/detail/drag DOM combinations
- Tests that assert grouping HTML, CSS classes, `aria-*`, or `data-*` attributes

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/openspec/specs/grouping/spec.md` — Complete grouping specification and behavior contracts
- `/docs/training/00-START-HERE.md` — Component overview, non-breaking rule
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, EventAggregator pattern
- `/docs/architecture/system-architecture.md` — Grid architecture, renderer flow, module interaction
- `/docs/tech-stack/tech-stack.md` — Blazor implementation constraints and async guidelines
- `/docs/code-guidelines/coding-standards.md` — Rendering/code organization conventions

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **`Row<object>` is the DOM blueprint** — every grouping `<tr>` comes from row model metadata like `RowType`, `Indent`, `Visible`, `IsExpand`, and `ParentUid`
- **`CellType` selects renderer** — `GridRow.razor` routes each cell to `RenderExpandCell()`, `RenderIndentCell()`, `RenderGroupCaptionCell()`, `RenderCaptionSummaryCell()`, or default cell rendering
- **Indent offsets are deterministic** — grouping cell left offsets follow `30px × cell position`
- **Caption `colspan` is computed** — it changes with visible columns, grouped depth, detail template presence, and hide/show operations
- **Feature combinations alter DOM shape** — virtualization adds mask rows, frozen columns can duplicate caption cells, lazy loading defers child row materialization
- **Collapsed grouped children are usually absent from DOM** — visibility is controlled by row generation and render conditions, not only by CSS hiding

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`Internal/Actions/Group.cs`** — expand/collapse flow, lazy-loading expand flow, group state mutation
  - `ExpandCollapse(Row<object>)` — line 325 (main expand/collapse logic)
  - `LazyExpandCollapse(Row, Rows)` — line 403 (lazy-load expand with server fetch)
  - `GroupColumn(field)` / `UnGroupColumn(field)` — group/ungroup operations
- **`Internal/Generators/GroupModelGenerator.cs`** — grouped row and cell model generation
  - `GenerateRows(data)` — line 227 (main row generation entry point)
  - `GenerateCaptionRow(data, indent, ...)` — line 323 (caption row factory)
  - `GenerateDataRows(data, indent, ...)` — line 531 (data row factory)
- **`Internal/Renderer/GridContent.razor`** — grouped tbody rendering for normal/infinite scenarios
- **`Internal/Renderer/GridVirtualContent.razor`** — grouped tbody rendering for virtualization with mask rows
- **`Internal/Renderer/GridRow.razor`** — per-row dispatch into cell renderers
- **`Internal/Renderer/GroupDropArea.razor`** — group drop area wrapper DOM
- **`Internal/Renderer/GroupedHeader.razor`** — grouped chip DOM and actions
- **`Internal/Renderer/ExpandCellRenderer.cs`** — expand/collapse cell DOM
- **`Internal/Renderer/IndentCellRenderer.cs`** — indent/detail-indent DOM
- **`Internal/Renderer/GroupCaptionRenderer.cs`** — caption cell DOM, text, frozen layout handling
- **`Internal/Renderer/CaptionSummaryRenderer.cs`** — group footer/caption summary DOM
- **`scripts/sf-grid.ts`** — JavaScript interop functions for grouping
  - `updateOptions(dataId, options)` — line 169
  - `lazyGroupExpand(dataId, options)` — line 188
  - `focusFirstGroupHeader(dataId)` — line 634
  - `lazyLoadGridHeight(dataId)` — line 1099
  - `resetExpandCollapseAllScroll(dataId, requestType)` — line 1108

---

## Embedded Reference Content
<!-- token-budget: 300 words -->

This skill embeds the complete content previously stored across three separate grouping DOM documents:

1. **DOM Structure** — exact HTML/component tree, CSS classes, data attributes, renderer mapping
2. **Rendering Flow** — data → grouped row model → content render → row render → cell render pipeline
3. **Feature Combinations** — how grouping DOM changes when combined with aggregates, lazy loading, virtualization, frozen columns, paging, infinite scroll, detail rows, and row drag-and-drop

Use the sections below as the single source of truth.

---

## Part 1 — Full DOM Structure Reference

### 1. Component Tree Overview (Blazor Side)

```
SfGrid<T>                                  (root grid component)
 ├── GroupDropArea.razor                   (e-groupdroparea — always present when ShowDropArea=true)
 │    └── GroupedHeader.razor  ×N          (one chip per grouped column)
 ├── GridHeader.razor                      (header row — contains indent <th> per grouped level)
 │    └── GridHeaderCell.razor  ×cols
 └── GridContent.razor  / GridVirtualContent.razor
			└── <table class="e-table">
					 └── <tbody role="rowgroup">
								├── <tr>  ← Caption Row      (RowType = "GroupCaption")
								│    ├── <td> ← ExpandCell   (CellType = Expand)
								│    ├── <td> ← IndentCell   (CellType = Indent, per extra nesting level)
								│    └── <td colspan=N> ← GroupCaption cell  (CellType = GroupCaption)
								├── <tr>  ← Data Row         (RowType = "Data", IsDataRow = true)
								│    ├── <td> ← IndentCell   ×(number of grouped columns)
								│    └── <td> ← DataCell     ×(visible columns)
								└── <tr>  ← Summary/Footer Row  (RowType = "Summary")  [optional]
										 ├── <td> ← IndentCell   ×indent
										 └── <td> ← CaptionSummaryCell  (CellType = CaptionSummary | Summary)
```

---

### 2. Group Drop Area — HTML Structure

Rendered by `GroupDropArea.razor`.  
**CSS class on outer `<div>`:**

| State | CSS classes |
|---|---|
| No columns grouped | `e-groupdroparea e-lib e-droppable e-draggable` |
| Columns grouped, no sticky | `e-groupdroparea e-lib e-droppable e-draggable e-grouped` |
| Columns grouped + sticky header | `e-groupdroparea e-lib e-droppable e-draggable e-grouped e-sticky` |

```
ASCII — Drop area (no grouped columns)
┌──────────────────────────────────────────────────────────┐
│ div.e-groupdroparea e-lib e-droppable e-draggable        │
│   "Drag a column header here to group its column"        │
└──────────────────────────────────────────────────────────┘

ASCII — Drop area (1 column grouped: "CustomerID")
┌──────────────────────────────────────────────────────────┐
│ div.e-groupdroparea.e-grouped                            │
│  ┌─────────────────────────────────────────────────┐    │
│  │ div.e-grid-icon.e-groupheadercell                │    │
│  │  ┌───────────────────────────────────────────┐  │    │
│  │  │ div (click → sort)                        │  │    │
│  │  │  span.e-grouptext         "Customer ID"   │  │    │
│  │  │  span.e-togglegroupbutton [toggle icon]   │  │    │
│  │  │  span.e-groupsort.e-ascending [sort icon] │  │    │
│  │  │  span.e-ungroupbutton.e-icon-hide [×]     │  │    │
│  │  └───────────────────────────────────────────┘  │    │
│  └─────────────────────────────────────────────────┘    │
└──────────────────────────────────────────────────────────┘
```

#### GroupedHeader chip element breakdown

Rendered by `GroupedHeader.razor`:

```html
<div class="e-grid-icon e-groupheadercell [e-focus e-focused?]" tabindex="0|−1">
	<div onclick="→ SortModule.SortColumn(column, ApplySort, true)">
		<span class="e-grouptext">Customer ID</span>

		<!-- ShowToggleButton=true only -->
		<span class="e-togglegroupbutton e-icons e-icon-ungroup e-toggleungroup"
					title="Ungroup by this column" role="button" tabindex="0"
					onclick:stopPropagation → GroupModule.UnGroupColumn(column)>
		</span>

		<!-- Always present -->
		<span class="e-groupsort e-icons e-ascending|e-descending e-icon-ascending|e-icon-descending"
					role="button" title="Sort ascending/descending [HeaderText]" tabindex="0">
			&nbsp;&nbsp;
		</span>

		<!-- ShowUngroupButton=true (default) -->
		<span class="e-ungroupbutton e-icons e-icon-hide [e-ungroupbutton-hidden?]"
					role="button" title="Ungroup" tabindex="0"
					onclick → GroupModule.UnGroupColumn(column)>
		</span>
	</div>
</div>
```

**Key CSS class rules for sort icon:**
- `Direction=Ascending` → `e-ascending e-icon-ascending`
- `Direction=Descending` → `e-descending e-icon-descending`

**Key CSS class rule for ungroup button:**
- `ShowUngroupButton=false` → adds `e-ungroupbutton-hidden`

---

### 3. Header Row — Indent Columns for Grouped Levels

Each grouped column adds one `<th>` indent cell to the header. Generated in `GridHeader.razor` + `GridHeaderBase.cs`.

```
ASCII — Header row (2 columns grouped)
┌──────────────────────────────────────────────────────────────────┐
│ <thead>                                                          │
│  <tr role="row" class="e-columnheader">                          │
│   <th class="e-grouptopleftcell"></th>   ← indent #1             │
│   <th class="e-grouptopleftcell"></th>   ← indent #2             │
│   <th class="e-headercell">OrderID</th>                          │
│   <th class="e-headercell">CustomerID</th>   (if ShowGroupedCol) │
│   <th class="e-headercell">ShipCity</th>     (if ShowGroupedCol) │
│   ...                                                            │
│  </tr>                                                           │
│ </thead>                                                         │
└──────────────────────────────────────────────────────────────────┘
```

---

### 4. Content tbody — Full Row-by-Row DOM

Rendered inside `GridContent.razor` → `<GridRow>` → `GridRow.razor` + `GridRowBase<TRow>`.

#### 4.1 Caption Row (`RowType = "GroupCaption"`)

```
ASCII — Caption row for "CustomerID: ALFKI − 2 Items" at indent level 0
┌─────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-row e-groupcaptionrow" role="row"                              │
│     data-uid="[row.Uid]" data-sf-rowindex="[rowIndex]">                     │
│                                                                             │
│  <td class="e-recordplusexpand|collapse e-indentcell e-leftfreeze?"         │
│      data-sf-style="Left:0px" tabindex="0" aria-expanded="true|false">     │
│    <a class="e-icons e-gdiagonaldown e-icon-gdownarrow"                     │
│       title="expanded">                         ← ExpandCell (expanded)     │
│    </a>                                                                     │
│  </td>                                                                      │
│                                                                             │
│  <td class="e-groupcaption e-leftfreeze?" colspan="[N]"                    │
│      tabindex="-1|0" title="Customer ID: ALFKI - 2 Items"                  │
│      aria-label="Customer ID: ALFKI - 2 Items [GroupCaption]"               │
│      data-sf-style="Left:30px" data-uid="[cell.Uid]">                      │
│    Customer ID: ALFKI - 2 Items                                             │
│  </td>                                                                      │
│                                                                             │
│  <!-- Frozen variant only: extra <td> for right frozen pane -->             │
│  <td class="e-groupcaption" colspan="[Colspan]" ...></td>                   │
│                                                                             │
│ </tr>                                                                       │
└─────────────────────────────────────────────────────────────────────────────┘
```

**Expand icon CSS rules:**
| State | `<td>` class prefix | `<a>` class |
|---|---|---|
| Expanded | `e-recordplusexpand` | `e-icons e-gdiagonaldown e-icon-gdownarrow` |
| Collapsed | `e-recordpluscollapse` | `e-icons e-gnextforward e-icon-grightarrow` |

**colspan value** = `visibleColumns + groupedColumnsCount + (detailTemplate ? 1 : 0) - indent`

---

#### 4.2 Indent Cell (`CellType = Indent`)

Each data row and nested caption row gets one `<td>` per grouping level above its indent.

```html
<td class="e-indentcell [e-leftfreeze?]"
		data-sf-style="Left: [30 * cellIndex]px"
		tabindex="-1"
		data-uid="[cell.Uid]">
</td>
```

`Left` offset formula: `30px × cellPositionIndex`  
- At depth 0 (top caption): `Left: 0px`  
- At depth 1 (nested caption or data): `Left: 30px`  
- At depth 2: `Left: 60px`  
- … and so on.

---

#### 4.3 Data Row (`IsDataRow = true`)

```
ASCII — Data row nested inside "CustomerID: ALFKI" (1 grouped level)
┌──────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-row [e-altrow?] [e-selectedrow?]" role="row"                   │
│     data-sf-rowindex="0" data-uid="[row.Uid]">                              │
│                                                                              │
│  <td class="e-indentcell e-leftfreeze?"                                     │
│      data-sf-style="Left:0px">                  ← indent #1 (depth 0)       │
│  </td>                                                                       │
│                                                                              │
│  <td class="e-rowcell e-leftfreeze?" data-colindex="0"                      │
│      tabindex="-1" data-uid="[cell.Uid]">                                   │
│    10248                                         ← OrderID value             │
│  </td>                                                                       │
│  <td class="e-rowcell" data-colindex="1">                                   │
│    07/04/1996                                                                │
│  </td>                                                                       │
│  ...                                                                         │
│ </tr>                                                                        │
└──────────────────────────────────────────────────────────────────────────────┘
```

---

#### 4.4 Group Footer / Caption Summary Row (`RowType = "Summary"`)

Rendered only when `GridAggregates` are configured with `GroupFooterTemplate` or `GroupCaptionTemplate`.

```
ASCII — Summary row after last child row of "CustomerID: ALFKI"
┌──────────────────────────────────────────────────────────────────────────────┐
│ <tr class="e-summaryrow e-hide?" role="row">                                │
│                                                                              │
│  <td class="e-indentcell">                      ← indent cells × depth       │
│  <td class="e-indentcell">                                                   │
│                                                                              │
│  <td class="e-summarycell [e-groupcaptionsummarycell?]"                     │
│      tabindex="-1" aria-label="[value] template column Column Header [hdr]" │
│      data-uid="[cell.Uid]">                                                 │
│    <!-- GroupFooterTemplate output OR plain aggregate value -->              │
│  </td>                                                                       │
│  ...                                                                         │
│ </tr>                                                                        │
└──────────────────────────────────────────────────────────────────────────────┘
```

**Cell type mapping for aggregate rows:**

| `cell.AggregateColumn` template | CellType used | CSS classes on `<td>` |
|---|---|---|
| `GroupFooterTemplate != null` | `CaptionSummary` (RowType=Summary) | `e-summarycell` |
| `GroupCaptionTemplate != null` | `CaptionSummary` (RowType=GroupCaption) | `e-summarycell e-groupcaptionsummarycell` |
| No template (plain value) | `CaptionSummary` | `e-summarycell` |

---

### 5. Full Multi-Level Grouping ASCII DOM

Two columns grouped: `CustomerID` → `ShipCity`.

```
<div class="e-groupdroparea e-grouped">
	[CustomerID chip] [ShipCity chip]
</div>

<table class="e-table"> ← header
	<thead>
		<tr class="e-columnheader">
			<th class="e-grouptopleftcell"></th>   ← indent for CustomerID level
			<th class="e-grouptopleftcell"></th>   ← indent for ShipCity level
			<th class="e-headercell">OrderID</th>
			...
		</tr>
	</thead>
</table>

<table class="e-table"> ← content
	<tbody role="rowgroup">

		── Level 0: CustomerID = "ALFKI" ─────────────────────────────
		<tr class="e-row e-groupcaptionrow">                ← Caption Row L0
			<td class="e-recordplusexpand e-indentcell"       ← ExpandCell  (L0, Left:0px)
					aria-expanded="true">
				<a class="e-icons e-gdiagonaldown e-icon-gdownarrow"></a>
			</td>
			<td class="e-groupcaption" colspan="N"            ← GroupCaption (Left:30px)
					data-sf-style="Left:30px">
				Customer ID: ALFKI - 4 Items
			</td>
		</tr>

			── Level 1: ShipCity = "Berlin" (child of ALFKI) ──────────
			<tr class="e-row e-groupcaptionrow">              ← Caption Row L1
				<td class="e-indentcell" data-sf-style="Left:0px"></td>  ← Indent for L0
				<td class="e-recordplusexpand e-indentcell"     ← ExpandCell  (L1, Left:30px)
						aria-expanded="true">
					<a class="e-icons e-gdiagonaldown e-icon-gdownarrow"></a>
				</td>
				<td class="e-groupcaption" colspan="N-1"        ← GroupCaption (Left:60px)
						data-sf-style="Left:60px">
					Ship City: Berlin - 2 Items
				</td>
			</tr>

				── Data rows (children of Berlin/ALFKI) ────────────────
				<tr class="e-row">                              ← Data Row
					<td class="e-indentcell" style="Left:0px"></td>   ← indent L0
					<td class="e-indentcell" style="Left:30px"></td>  ← indent L1
					<td class="e-rowcell">10248</td>
					<td class="e-rowcell">07/04/1996</td>
					...
				</tr>

				<tr class="e-row">                              ← Data Row
					<td class="e-indentcell" style="Left:0px"></td>
					<td class="e-indentcell" style="Left:30px"></td>
					<td class="e-rowcell">10249</td>
					...
				</tr>

				── (GroupFooter row, if aggregates configured) ─────────
				<tr class="e-summaryrow">
					<td class="e-indentcell"></td>
					<td class="e-indentcell"></td>
					<td class="e-summarycell">Sum: 20497</td>
					...
				</tr>

			── Level 1: ShipCity = "Münster" ──────────────────────────
			<tr class="e-row e-groupcaptionrow"> ... </tr>
			... (data rows) ...

			── L1 GroupFooter (if aggregates) ─────────────────────────
			<tr class="e-summaryrow"> ... </tr>

		── Level 0: CustomerID = "ANATR" ─────────────────────────────
		<tr class="e-row e-groupcaptionrow"> ... </tr>
		...

	</tbody>
</table>
```

---

### 6. Cell Type → Renderer Mapping

| `CellType` enum value | Renderer method | Source file | Rendered HTML element |
|---|---|---|---|
| `Expand` | `RenderExpandCell()` | `ExpandCellRenderer.cs` | `<td class="e-recordplusexpand|e-recordpluscollapse ...">` |
| `Indent` | `RenderIndentCell()` | `IndentCellRenderer.cs` | `<td class="e-indentcell ...">` |
| `DetailIndent` | `RenderIndentCell()` | `IndentCellRenderer.cs` | `<th class="e-detailindentcell ...">` |
| `GroupCaption` | `RenderGroupCaptionCell()` | `GroupCaptionRenderer.cs` | `<td class="e-groupcaption ...">` |
| `GroupCaptionEmpty` | `RenderGroupCaptionCell()` | `GroupCaptionRenderer.cs` | empty `<td>` |
| `CaptionSummary` | `RenderCaptionSummaryCell()` | `CaptionSummaryRenderer.cs` | `<td class="e-summarycell ...">` |
| `Summary` | `RenderSummaryCell()` | `SummaryCellRenderer.cs` | `<td class="e-summarycell ...">` |
| `Data` | `RenderCell()` | `CellRender.razor` | `<td class="e-rowcell ...">` |

---

### 7. CSS Class Reference for Grouping DOM

| CSS class | Applied to | Meaning |
|---|---|---|
| `e-groupdroparea` | drop area `<div>` | Container for grouped column chips |
| `e-grouped` | drop area `<div>` | At least one column is grouped |
| `e-sticky` | drop area `<div>` | Sticky header is active |
| `e-groupheadercell` | chip `<div>` | Grouped column chip wrapper |
| `e-grouptext` | chip `<span>` | Displays the column header text |
| `e-groupsort` | chip `<span>` | Sort direction indicator on chip |
| `e-ascending` / `e-descending` | chip sort `<span>` | Sort direction state |
| `e-icon-ascending` / `e-icon-descending` | chip sort `<span>` | Sort icon variant |
| `e-togglegroupbutton` | chip `<span>` | Toggle group/ungroup button (ShowToggleButton) |
| `e-ungroupbutton` | chip `<span>` | Remove-group button (ShowUngroupButton) |
| `e-ungroupbutton-hidden` | chip `<span>` | Hides ungroup button when `ShowUngroupButton=false` |
| `e-grouptopleftcell` | header `<th>` | Blank indent cell in header per grouped level |
| `e-groupcaptionrow` | caption `<tr>` | Marks a group caption row |
| `e-indentcell` | expand/indent `<td>` | Grouping indent spacer cell |
| `e-recordplusexpand` | expand `<td>` | Caption row is expanded |
| `e-recordpluscollapse` | expand `<td>` | Caption row is collapsed |
| `e-icon-gdownarrow` | expand `<a>` | Expanded icon (down arrow) |
| `e-icon-grightarrow` | expand `<a>` | Collapsed icon (right arrow) |
| `e-groupcaption` | caption text `<td>` | The main caption cell with colspan |
| `e-leftfreeze` | `<td>` / `<th>` | Cell is in the frozen-left pane |
| `e-summaryrow` | footer `<tr>` | Group footer/summary row |
| `e-summarycell` | footer `<td>` | Aggregate value cell in summary row |
| `e-groupcaptionsummarycell` | footer `<td>` | Caption-level aggregate (GroupCaptionTemplate) |
| `e-masked-row` | mask `<tr>` | Virtual-scroll mask row (loading placeholder) |
| `e-masked-cell` | mask `<td>` | Individual mask cell |
| `e-virtualcell` | mask `<span>` | Inner span of mask cell |
| `e-focus e-focused` | caption `<td>` | Keyboard focus highlight on caption cell |
| `e-hide` | summary `<tr>` | Summary row hidden (parent caption collapsed) |

---

### 8. Data Attributes on Grouping DOM Elements

| Element | Attribute | Value |
|---|---|---|
| Caption `<tr>` | `data-uid` | `row.Uid` (unique row identity) |
| Caption `<tr>` | `data-sf-rowindex` | zero-based row position |
| Caption `<tr>` | `role` | `"row"` |
| Expand `<td>` | `aria-expanded` | `"true"` or `"false"` |
| Expand `<td>` | `data-sf-style` | `"Left: {n}px"` |
| Expand `<td>` | `data-uid` | `cell.Uid` |
| GroupCaption `<td>` | `colspan` | computed span value |
| GroupCaption `<td>` | `title` | full caption text |
| GroupCaption `<td>` | `aria-label` | `"[caption] [GroupCaption locale]"` |
| GroupCaption `<td>` | `data-sf-style` | `"Left: {n}px"` |
| GroupCaption `<td>` | `data-uid` | `cell.Uid` |
| Indent `<td>` | `data-sf-style` | `"Left: {n}px"` |
| Summary `<td>` | `aria-label` | `"[value] template column Column Header [headerText]"` |

---

### 9. Key Source File → DOM Responsibility Map

| Source file | DOM element produced |
|---|---|
| `GroupDropArea.razor` | `<div class="e-groupdroparea">` + chips |
| `GroupedHeader.razor` | `<div class="e-groupheadercell">` chip internals |
| `GridHeader.razor` + `GridHeaderBase.cs` | `<th class="e-grouptopleftcell">` indent headers |
| `GroupModelGenerator.cs` | `Row<object>` + `Cell<object>` objects (data model → DOM) |
| `GridContent.razor` | `<table><tbody>` entry point; dispatches `GenerateRows` |
| `GridVirtualContent.razor` | Same as above for virtualized grouping |
| `GridRow.razor` | `<tr>` element per row; routes each cell to its renderer |
| `ExpandCellRenderer.cs` | `<td class="e-recordplusexpand|collapse">` |
| `IndentCellRenderer.cs` | `<td class="e-indentcell">` |
| `GroupCaptionRenderer.cs` | `<td class="e-groupcaption">` (main caption cell) |
| `CaptionSummaryRenderer.cs` | `<td class="e-summarycell">` (footer/caption aggregate) |
| `Group.cs` (Actions) | Expand/collapse logic — mutates `row.Visible` / `row.IsExpand` |

---

## Part 2 — Full Rendering Flow Reference

### 1. High-Level Rendering Pipeline

```
User Action / Init
				│
				▼
┌───────────────────────┐
│  SfGrid Lifecycle     │  OnInitializedAsync → Grouping<T>.InitialGroupSort()
│  (SfGrid.Lifecycle.cs)│  Injects sort entries for initial grouped columns
└──────────┬────────────┘
					 │
					 ▼
┌───────────────────────┐
│  DataManager Query    │  Applies Group() + Sort() + Filter() predicates
│  (Data.cs)            │  Returns IEnumerable<Group<T>> (nested groups)
└──────────┬────────────┘
					 │
					 ▼
┌───────────────────────────────┐
│  GroupModelGenerator<T>       │  Converts Group<T> tree → List<Row<object>>
│  (GroupModelGenerator.cs)     │  Produces: CaptionRows, DataRows, FooterRows
└──────────┬────────────────────┘
					 │
					 ▼
┌───────────────────────┐
│  GridContent.razor    │  OnParametersSet → assigns Rows list
│  (OnParametersSet)    │  Triggers GroupModelGenerator.GenerateRows()
└──────────┬────────────┘
					 │
					 ▼
┌───────────────────────┐
│  GridRow.razor        │  foreach Row → <tr> → foreach Cell → renderer
│  (per Row<object>)    │
└──────────┬────────────┘
					 │
		 ┌─────┴──────┐
		 │            │
CellType?       CellType?
		 │            │
Expand       GroupCaption
Indent       CaptionSummary
		 │            │
		 ▼            ▼
RendererCS   RendererCS
```

---

### 2. Detailed Step-by-Step: Initial Render with Grouping

#### Step 1 — `Grouping<T>.InitialGroupSort()`
**File:** `Internal/Actions/Group.cs`

- Called from `SfGrid` initialization.
- For each column in `GroupSettings.Columns`:
	- Sets `GridColumn.Visible = GroupSettings.ShowGroupedColumn`
	- Sets `IsHiddenByGrouping = true` (if was visible and `ShowGroupedColumn=false`)
	- Adds `GridSortColumn { Field, Direction=Ascending, IsFromGroup=true }` to `SortSettings.Columns`

#### Step 2 — DataManager executes query
**File:** `Internal/Actions/Data.cs`

- `GroupQuery()` adds `.Group(groupedColumns)` to the query.
- `SortQuery()` adds `.Sort(field, dir)` per grouped column.
- Result is `IEnumerable<Group<T>>` — a tree where each `Group<T>` has:
	- `Field` — the grouped column name
	- `Key` — the grouped value
	- `Items` — child `Group<T>` list (nested groups) or `IEnumerable<T>` (leaf data)
	- `CountItems` — count of leaf records in this group
	- `Aggregates` — aggregate values dictionary

#### Step 3 — `GroupModelGenerator<T>.GenerateRows(data)`
**File:** `Internal/Generators/GroupModelGenerator.cs`

Recursively walks the `Group<T>` tree and produces `List<Row<object>>`:

```
foreach Group<T> in data:
	① GenerateCaptionRow(group, indent, ...)  → Row { RowType="GroupCaption", IsCaptionRow=true }
		 └─ GenerateCaptionCells(indent)
				├─ Cell { CellType=Expand }
				├─ Cell { CellType=Indent } × indent (for nesting depth)
				└─ Cell { CellType=GroupCaption, ColSpan=computed }

	② Recurse into group.Items:
		 ├─ If Group<T>  → repeat from ①
		 └─ If T (leaf)  → GenerateDataRow(item, indent)
					 └─ Row { RowType="Data", IsDataRow=true }
							└─ Cells: [Indent × depth] + [DataCell × visible columns]

	③ GenerateFooterRow(group, ...) [only when aggregates exist]
		 → Row { RowType="Summary" }
				└─ Cells: [Indent × depth] + [CaptionSummaryCell × aggregate columns]
```

**Key properties set per row:**
| Property | Caption Row | Data Row | Footer Row |
|---|---|---|---|
| `RowType` | `"GroupCaption"` | `"Data"` | `"Summary"` |
| `IsCaptionRow` | `true` | `false` | `false` |
| `IsDataRow` | `false` | `true` | `false` |
| `IsExpand` | `true` (default) | N/A | N/A |
| `Visible` | `true` | `true` (if parent expanded) | `true` (if parent expanded) |
| `Indent` | depth level | depth level | depth level |
| `ParentUid` | parent caption Uid | parent caption Uid | parent caption Uid |
| `Uid` | generated unique ID | generated unique ID | generated unique ID |

#### Step 4 — `GridContent.razor` → `OnParametersSet()`
**File:** `Internal/Renderer/GridContent.razor`

```csharp
// Trigger condition
if (AllowGrouping && groupCount > 0)
{
		Rows = new GroupModelGenerator<TContent>(Parent).GenerateRows(Data);
		Parent.Rows = Rows;
}
```

Also handles:
- **InfiniteScroll:** `GenerateInfiniteGroupedRows(Data)`
- **LazyLoading:** uses `Parent.Rows` directly (rows already set by `Group.cs`)
- **Column hide/show with lazy loading:** recalculates `ColSpan` on caption cells

#### Step 5 — `GridContent.razor` renders `<table>`

```razor
<table class="e-table" id="{ID}_content_table">
	<WidthController ... />
	<tbody role="rowgroup">
		@foreach (Row<object> Row in Rows)
		{
			if (Row.Visible)
				<GridRow TRow="TContent" RowParameters="rowParameters" />
		}
	</tbody>
</table>
```

Only rows with `Visible=true` are rendered into the DOM.

#### Step 6 — `GridRow.razor` renders `<tr>` + dispatches cells

```razor
<tr class="@ClassNames" role="row" @attributes="Attributes">
	@foreach (Cell<object> cell in GetCells(...))
	{
			if (cell.CellType == CellType.Expand)       @RenderExpandCell(cellParams)
			if (cell.CellType == CellType.GroupCaption)  @RenderGroupCaptionCell(cellParams)
			if (cell.CellType == CellType.Indent)        @RenderIndentCell(cellParams)
			if (cell.CellType == CellType.CaptionSummary) @RenderCaptionSummaryCell(cellParams)
			// CellType.Data → RenderCell (normal data cell)
	}
</tr>
```

#### Step 7 — Individual cell renderers produce `<td>` HTML

Each renderer is a `RenderFragment` in a partial class of `GridRowBase<TRow>`:

| Renderer | File | Output |
|---|---|---|
| `RenderExpandCell` | `ExpandCellRenderer.cs` | `<td class="e-recordplusexpand|collapse"><a .../></td>` |
| `RenderIndentCell` | `IndentCellRenderer.cs` | `<td class="e-indentcell">` (or `<th>` for DetailIndent) |
| `RenderGroupCaptionCell` | `GroupCaptionRenderer.cs` | `<td class="e-groupcaption" colspan=N>text</td>` |
| `RenderCaptionSummaryCell` | `CaptionSummaryRenderer.cs` | `<td class="e-summarycell">value</td>` |

---

### 3. Expand / Collapse Flow

```
User clicks ExpandCell <td>
				│
				▼
ExpandCellRenderer.cs — onclick handler
	→ Parent.FocusModule.CellClickHandler(row, cell, e)
	→ Parent.GroupModule.ExpandCollapse(row)          ← core logic
	→ Parent.FocusModule.ClearFocus() + Focus(uid)

				│
				▼
Group<T>.ExpandCollapse(Row<object>)        [Group.cs]
	├─ if EnableLazyLoading → LazyExpandCollapse(Row, Rows)
	└─ else (normal):
			 for each child row in Rows after this row:
				 childRow.Visible = !Row.IsExpand    (recursively respects nested IsExpand)
			 Row.IsExpand = !Row.IsExpand

				│
				▼
EventAggregator.Trigger("ContentStateChanged")
	→ GridContent.InvokeStateChange()
	→ OnParametersSet() (if lazy) OR direct StateHasChanged()
	→ Blazor diff-renders only changed <tr> visibility
```

#### Lazy Load Expand Flow

```
LazyExpandCollapse(Row, Rows)               [Group.cs]
	├─ Row.IsExpand = true
	├─ Build query:
	│    FilterQuery(query, ...)              ← Where predicates for parent group keys
	│    GroupQuery(query, Row)              ← Group sub-levels below current indent
	│    SortQuery(query, Row)
	│    query.Queries.LazyLoad = true
	├─ DataManager.ExecuteQuery<T>(query)    ← fetch from server/local
	├─ LazyExpand(data, index, Row)
	│    ├─ GenerateCaptionRow() per sub-group
	│    └─ GenerateDataRows() for leaf data
	│    → InsertRange into LazyRows
	└─ RefreshContentAndPager()
			 → Parent.Rows = LazyRows
			 → EventAggregator "ContentStateChanged"
```

---

### 4. GroupDropArea Rendering Flow

```
SfGrid renders → GroupDropArea.razor
	│
	├─ Reads GroupSettings.Columns
	├─ If columns.Count == 0:
	│    renders: <div class="e-groupdroparea">
	│               [localized placeholder text]
	│             </div>
	└─ If columns.Count > 0:
			 renders: <div class="e-groupdroparea e-grouped [e-sticky?]">
									@foreach column in Columns:
										<GroupedHeader TContent="T" Column="@column" />
								</div>

Each GroupedHeader.razor:
	├─ Reads sort direction from SortSettings.Columns
	├─ Renders chip div with:
	│    span.e-grouptext            → HeaderText
	│    span.e-togglegroupbutton    → (ShowToggleButton only)
	│    span.e-groupsort            → ascending/descending icon
	│    span.e-ungroupbutton        → (ShowUngroupButton=true, default)
	└─ onclick → SortModule.SortColumn(column, newDirection, true)
		 UnGroupColumn onclick → GroupModule.UnGroupColumn(column)
```

---

### 5. Group Column / Ungroup Flow

#### GroupColumnAsync("CustomerID")

```
SfGrid.GroupColumnAsync("CustomerID")
	→ Grouping<T>.GroupColumn("CustomerID")
			 ├─ Guard checks (AllowGrouping, AllowGrouping on column, not already grouped, not FixedColumn)
			 ├─ Column.SetVisibility(ShowGroupedColumn)
			 ├─ Column.IsHiddenByGrouping = true  (if hidden)
			 ├─ Parent.GroupStates.Clear()
			 └─ UpdateModel("CustomerID")
						 ├─ GroupAddSortingQuery("CustomerID")   → adds to SortSettings
						 ├─ GroupSettings.UpdateProperties("Columns", newCols)
						 ├─ Parent.RefreshColumnHeader = true
						 └─ Parent.ModelChanged(RequestType=Grouping)
									 ├─ Fires "Grouping" event (before)
									 ├─ Re-queries DataManager
									 ├─ GroupModelGenerator generates new Rows
									 ├─ Fires "Grouped" event (after)
									 └─ Triggers full grid re-render
```

#### UngroupColumnAsync("CustomerID")

```
Grouping<T>.UnGroupColumn("CustomerID")
	├─ Column.IsHiddenByGrouping = false
	├─ Column.SetVisibility(true)
	├─ GCols.Remove("CustomerID")
	├─ Remove sort entry for "CustomerID" (if injected by grouping)
	├─ Parent.SortModule.SortedColumns.Remove("CustomerID")
	├─ Parent.RefreshColumnHeader = true
	└─ Parent.ModelChanged(RequestType=UnGrouping)
				├─ Fires "Grouping" event (before, Action=Remove)
				├─ Re-queries DataManager (without group)
				├─ Fires "Grouped" event (after, Action=Remove)
				└─ Grid re-renders without grouping DOM
```

---

### 6. Caption Text Rendering Flow

**File:** `GroupCaptionRenderer.cs` → `GetCaption(Row)`

```
GetCaption(row)
	├─ groupedData = row.Data as Group<TRow>
	├─ column = GetColumnByField(groupedData.Field, Parent.Columns)
	│
	├─ if column.IsForeignColumn():
	│    query foreign data for matching key
	│    templateData.ForeignKey = resolved display value
	│    colValue = templateData.ForeignKey
	│
	├─ Build CaptionTemplateContext:
	│    { Field, Count, Key, Level, HeaderText, ForeignKey, ForeignKeyValue, GroupGuid, GroupAggregates }
	│
	├─ if Parent.GroupSettings.CaptionTemplate != null:
	│    → render CaptionTemplate(templateData)          ← custom template
	│
	└─ else default text:
			 colName = column.HeaderText ?? column.Field
			 count = groupedData.CountItems
			 strItems = count > 1 ? "Items" : "Item"         ← localized
			 return $"{colName}: {colValue} - {count} {strItems}"
```

---

### 7. Virtualization + Grouping Flow

```
VirtualScrollModule.SetGeneratedData(startIndex, endIndex, currentViewData)
	│
	├─ Calls GroupModelGenerator.GroupGeneratorRows(groupedDataParams)
	│    ← uses GroupedDataParams records (Data, Uid, ParentUid, Indent, IsExpand, IsVisible)
	│
	├─ After ExpandCollapse:
	│    Grouping<T>.ExpandCollapse(Row)
	│      → updates CurrentGroupedData (expand/collapse state)
	│      → VirtualScrollModule.SetGeneratedData(startIdx, endIdx, queriedViewData)
	│      → EventAggregator.Trigger("VirtualComponentUpdate")
	│      → GridVirtualContent re-renders visible slice
	│
	└─ Mask rows (loading placeholders):
			 <tr class="e-masked-row e-row">
				 <td class="e-masked-cell"> ×groupedCols  ← indent mask cells
				 <td class="e-masked-cell"><span class="e-virtualcell"></span></td> ×visibleCols
			 </tr>
```

---

### 8. Infinite Scroll + Grouping Flow

```
GridContent.HandleGroupedInfiniteRows()
	├─ GeneratedInfiniteGroupedRows = GroupModelGenerator.GenerateInfiniteGroupedRows(Data)
	│    └─ Walks Group<T> tree; appends to existing rows on downscroll,
	│       prepends on upscroll (IsDownScroll / IsUpScroll flags)
	│
	├─ On GroupExpandCollapse (RequestType = "GroupExpandCollapse"):
	│    InfiniteScrollModule.SetCurrentGroupedRows(Row)
	│    → InvokeMethod "sfBlazor.Grid.updateOptions" (JS interop)
	│
	└─ Rows = InfiniteScrollModule.SetCurrentGroupedRows()
			 → returns visible slice from GeneratedInfiniteGroupedRows
```

---

### 9. PersistGroupState Flow

```
ExpandCollapse(Row)
	│
	├─ if PersistGroupState=true AND !EnableLazyLoading:
	│    key = Row.GroupKey.ToString() + GetUniqueGroupKey(Rows, Row.ParentUid)
	│    Parent.GroupStates[key] = !currentState   ← toggle persisted state
	│
After re-render (sort/filter/page):
	GroupModelGenerator.GenerateCaptionRow(group, ...)
	│
	└─ if _isStatePersistence (PersistGroupState=true AND GroupStates.Count > 0):
			 key = group.Key + unique ancestor path
			 captionRow.IsExpand = GroupStates[key]    ← restore saved state
			 child rows Visible = match IsExpand
```

---

### 10. Frozen Columns + Grouping DOM Notes

When frozen columns are active (`GetFreezeLeftCount() > 0` or `GetFrozenCount() > 0`):

```
Caption Row:
	┌─────────────────────────────────────────────┐
	│ <td class="e-groupcaption e-leftfreeze"     │  ← LEFT PANE (frozen)
	│     colspan=[ActualColSpan]                 │
	│     data-sf-style="Left: Npx">              │
	│   Customer ID: ALFKI - 2 Items              │
	│ </td>                                        │
	│                                             │
	│ <td class="e-groupcaption"                  │  ← RIGHT PANE (scrollable)
	│     colspan=[Columns.Count - ActualColSpan] │
	│     data-sf-style="Left: Npx">              │
	│   (same text)                               │
	│ </td>                                        │
	└─────────────────────────────────────────────┘
```

**ActualColSpan** is computed in `GetStyle()` by summing visible column widths until they fill `(GridWidth - indent)`. The right pane gets `colspan = Columns.Count - ActualColSpan`.

**`DisableExtraFrozenTd`** flag on `GroupModule`: when `true`, the second `<td>` is suppressed (e.g. when `CaptionTemplate` is in use).

---

### 11. State Change Event Chain

```
GroupModule.ExpandCollapse()
	└─ EventAggregator.Trigger("ContentStateChanged")
				└─ GridContent.InvokeStateChange(args)
							├─ OnParametersSet()           ← recalculates Rows
							└─ InvokeAsync(StateHasChanged) ← Blazor re-renders tbody
```

```
GroupColumn / UnGroupColumn
	└─ Parent.ModelChanged(requestType:"Grouping"|"UnGrouping")
				├─ Fires ActionEvents (before/after)
				├─ DataManager re-query
				└─ Full grid StateHasChanged
```

```
Virtual expand/collapse
	└─ EventAggregator.Trigger("VirtualComponentUpdate")
				└─ GridVirtualContent triggers StateHasChanged
```

---

## Part 3 — Full Feature Combination DOM Reference

### 1. Grouping + Aggregates (Group Footer)

#### When active
- `GridAggregates` configured with `GridAggregateColumn` having `GroupFooterTemplate`.

#### Extra DOM produced

```
ASCII — caption row + 2 data rows + group footer row
┌──────────────────────────────────────────────────────────┐
│ <tr class="e-row e-groupcaptionrow">                     │
│   <td class="e-recordplusexpand ...">▼</td>              │
│   <td class="e-groupcaption" colspan=N>                  │
│     Customer ID: ALFKI - 2 Items                         │
│   </td>                                                  │
│ </tr>                                                     │
│                                                          │
│ <tr class="e-row">   ← data row 1                        │
│   <td class="e-indentcell">...</td>                      │
│   <td class="e-rowcell">10248</td>  ...                  │
│ </tr>                                                     │
│                                                          │
│ <tr class="e-row">   ← data row 2                        │
│   ...                                                    │
│ </tr>                                                     │
│                                                          │
│ <tr class="e-summaryrow [e-hide?]">  ← GROUP FOOTER      │
│   <td class="e-indentcell"></td>                         │
│   <td class="e-summarycell">                             │
│     <!-- GroupFooterTemplate output -->                  │
│   </td>                                                  │
│   <td class="e-summarycell">Sum: 1234.56</td>            │
│   ...                                                    │
│ </tr>                                                     │
└──────────────────────────────────────────────────────────┘
```

**Rules:**
- Footer row `Visible` mirrors the caption row's `IsExpand`.
- On collapse: `e-hide` class is added OR `Visible=false` prevents `<tr>` rendering.
- `RowType = "Summary"` distinguishes footer rows from caption rows.
- Rendered via `RenderCaptionSummaryCell()` in `CaptionSummaryRenderer.cs`.
- Indent count on footer row = **same as its sibling data rows** (parent caption indent + 1).

---

### 2. Grouping + Caption Aggregates (GroupCaptionTemplate)

#### When active
- `GridAggregateColumn` has `GroupCaptionTemplate` set (not `GroupFooterTemplate`).

#### DOM change
The extra aggregate cell is embedded **inside the caption row itself** as additional `<td>` cells after the main `GroupCaption` cell:

```
<tr class="e-row e-groupcaptionrow">
	<td class="e-recordplusexpand ...">▼</td>
	<td class="e-groupcaption" colspan=N>Customer ID: ALFKI - 2 Items</td>

	<!-- Extra CaptionSummary cells (GroupCaptionTemplate) -->
	<td class="e-summarycell e-groupcaptionsummarycell" tabindex="-1"
			aria-label="[value] template column Column Header [hdr]">
		<!-- GroupCaptionTemplate rendered output -->
	</td>
</tr>
```

**Key difference from GroupFooter:**
| | GroupFooterTemplate | GroupCaptionTemplate |
|---|---|---|
| Extra row? | Yes — `RowType="Summary"` row after data | No — extra `<td>` in caption row |
| Cell CSS | `e-summarycell` | `e-summarycell e-groupcaptionsummarycell` |
| Cell type | `CaptionSummary` (in Summary row) | `CaptionSummary` (in Caption row) |
| `DisableExtraFrozenTd` | not affected | set to `true` → suppresses frozen pane duplicate `<td>` |

---

### 3. Grouping + Lazy Loading (`EnableLazyLoading=true`)

#### Initial render DOM (all collapsed)

```
<tbody role="rowgroup">
	<tr class="e-row e-groupcaptionrow">  ← only caption rows present
		<td class="e-recordpluscollapse ...">►</td>
		<td class="e-groupcaption" colspan=N>Customer ID: ALFKI - 0 Items</td>
	</tr>
	<tr class="e-row e-groupcaptionrow">
		<td class="e-recordpluscollapse ...">►</td>
		<td class="e-groupcaption" colspan=N>Customer ID: ANATR - 0 Items</td>
	</tr>
	...
	<!-- NO data rows rendered yet -->
</tbody>
```

#### After user expands "ALFKI"

```
<tbody role="rowgroup">
	<tr class="e-row e-groupcaptionrow">
		<td class="e-recordplusexpand ...">▼</td>          ← expanded
		<td class="e-groupcaption">Customer ID: ALFKI - 2 Items</td>
	</tr>

	<!-- Data rows fetched from server and inserted -->
	<tr class="e-row e-firstchildrow">                   ← first child has e-firstchildrow
		<td class="e-indentcell"></td>
		<td class="e-rowcell">10248</td>  ...
	</tr>
	<tr class="e-row">
		<td class="e-indentcell"></td>
		<td class="e-rowcell">10249</td>  ...
	</tr>

	<!-- GroupFooter (if aggregates, lazy) -->
	<tr class="e-summaryrow">
		<td class="e-indentcell"></td>
		<td class="e-summarycell">Sum: 20497</td>
	</tr>

	<!-- Last child gets special class for infinite+lazy -->
	<!-- <tr class="e-row e-lazyload-last-down"> (infinite scroll only) -->

	<tr class="e-row e-groupcaptionrow">                 ← next top-level caption
		...
	</tr>
</tbody>
```

**Lazy-specific CSS classes:**
| Class | Applied to | When |
|---|---|---|
| `e-firstchildrow` | first data row after expand | Lazy expand of a group |
| `e-lazyload-last-down` | last child row | Infinite scroll + lazy loading |

---

### 4. Grouping + Virtualization (`EnableVirtualization=true`)

#### Virtual content wrapper

Rendered in `GridVirtualContent.razor` instead of `GridContent.razor`:

```
<div class="e-virtualtable" style="height:[total]px; transform:translateY([offset]px)">
	<table class="e-table">
		<WidthController />
		<tbody role="rowgroup">

			<!-- TOP mask rows (before visible slice) -->
			<tr class="e-masked-row e-row" style="height:[rowHeight]px">
				<td class="e-masked-cell e-rowcell e-centeralign">  ← per grouped column
					<span class="e-virtualcell"></span>
				</td>
				<td class="e-masked-cell e-rowcell">                ← per visible column
					<span class="e-virtualcell"></span>
				</td>
			</tr>  ×PageSize

			<!-- Visible rows (current virtual window) -->
			<tr class="e-row e-groupcaptionrow"> ... </tr>
			<tr class="e-row"> ... </tr>
			...

			<!-- BOTTOM mask rows (after visible slice) -->
			<tr class="e-masked-row e-row"> ... </tr>  ×MaskRowCount

		</tbody>
	</table>
</div>
```

**Mask row indent cells:** for each grouped column, a `<td class="e-masked-cell e-rowcell e-centeralign">` is added before the data column mask cells, matching the indent width structure.

**ExpandAllGroups=true behavior:**
- On init, `GroupSettings.ExpandAllGroups=true` causes `VirtualScrollModule` to pre-expand all captions.
- All rows added to `CurrentGroupedData` with `IsExpand=true`.
- Ignored when `EnableLazyLoading=true`.

---

### 5. Grouping + Frozen Columns

#### Two-table layout (FrozenColumns > 0)

Grid renders two separate `<table>` elements — one frozen, one scrollable:

```
<div class="e-frozen-left-header">
	<table class="e-table"> ← frozen pane header
		<thead>
			<tr>
				<th class="e-grouptopleftcell"></th>  ← indent per group level
				<th class="e-headercell e-leftfreeze">OrderID</th>  ← frozen col
			</tr>
		</thead>
	</table>
</div>

<div class="e-movable-content">
	<table class="e-table"> ← scrollable pane content
		...
	</table>
</div>
```

#### Caption row in frozen layout

The `RenderGroupCaptionCell()` produces TWO `<td>` elements per caption row when frozen pane is active:

```html
<!-- Frozen pane table row -->
<tr class="e-row e-groupcaptionrow">
	<td class="e-recordplusexpand e-indentcell e-leftfreeze" ...>▼</td>
	<td class="e-groupcaption e-leftfreeze"
			colspan="[ActualColSpan]"
			data-sf-style="Left: Npx">
		Customer ID: ALFKI - 2 Items
	</td>
	<!-- Extra frozen TD (right pane spacer) -->
	<td class="e-groupcaption"
			colspan="[TotalColumns - ActualColSpan]"
			data-sf-style="Left: Npx">
		Customer ID: ALFKI - 2 Items          ← same text repeated
	</td>
</tr>
```

**`DisableExtraFrozenTd=true` suppresses the second `<td>`** — this is set when `CaptionTemplate` with `GroupCaptionTemplate` aggregate is in use.

**`ActualColSpan` computation:**
```
GridWidth = parsed width (or GroupModule.GridOffsetWidth for 100%/auto)
cumWidth = 0; count = 0
foreach column in Columns:
		cumWidth += column.Width
		if cumWidth < (GridWidth - indent): count++
ActualColSpan = count
```

---

### 6. Grouping + Paging (`AllowPaging=true`)

#### DOM impact
- No visual DOM change to grouping rows — standard caption/data/footer structure applies.
- **`TotalItemCount`** drives the pager, updated on expand/collapse.
- **Lazy load + paging:** only rows fitting within `PageSize` are rendered.
	- After expand overflow: `CurrentPageData = expandedData.Take(pageSize - rowIndex)`.
	- Pager refreshes via `Parent.PagerRef.TotalItemsCount = Parent.TotalItemCount`.

#### CalculatePageIndex usage
When programmatically navigating to a row inside a group:
```
GroupModule.CalculatePageIndex(currentViewData, targetIndex, pageSize)
	→ counts caption rows (don't occupy page slots)
	→ returns correct page number for the target data row index
```

---

### 7. Grouping + Infinite Scroll (`EnableInfiniteScrolling=true`)

#### Row generation
```
GenerateInfiniteGroupedRows(data)
	├─ if IsDownScroll:  Index = lastRow.Index + 1  → append to existing rows
	└─ if IsUpScroll:    Index = firstRow.Index - PageSize → prepend to existing rows
```

#### Special CSS class on last-loaded child
```html
<tr class="e-row e-lazyload-last-down">   ← marks boundary for next fetch
	...
</tr>
```

#### GroupExpandCollapse with Infinite Scroll
```
Group.ExpandCollapse()
	→ InfiniteScrollModule.RequestType = "GroupExpandCollapse"
	→ InfiniteScrollModule.InfiniteForceRefresh = true
	→ InfiniteScrollModule.SetCurrentGroupedRows(Row)
	→ JS Interop: sfBlazor.Grid.updateOptions(DataId, GetClientOption())
```

---

### 8. Grouping + Detail Row (`DetailTemplate`)

#### Extra indent cell
When `DetailTemplate` is configured, an additional `<td class="e-detailindentcell">` header placeholder appears, and data rows get an extra expand cell for detail rows:

```
Header:
<tr class="e-columnheader">
	<th class="e-grouptopleftcell"></th>   ← group indent #1
	<th class="e-detailheadercell"></th>   ← detail expand placeholder
	<th class="e-headercell">OrderID</th>
	...
</tr>

Caption Row:
	colspan calculation includes +1 for detail template column

Data Row (with detail):
<tr class="e-row">
	<td class="e-indentcell"></td>          ← group indent
	<td class="e-detailrowcollapse ...">►</td>  ← detail expand
	<td class="e-rowcell">10248</td>
	...
</tr>

Detail Row (when expanded):
<tr class="e-detailrow">
	<td class="e-indentcell"></td>
	<td></td>
	<td class="e-detailcell" colspan=N>
		<!-- DetailTemplate content -->
	</td>
</tr>
```

**ColSpan formula with Detail Template:**
```
ColSpan = visibleColumns + groupedColumnsCount + 1 (detail col) - indent
```
The `+1` is added in `GridContent.razor`:
```csharp
if (((IGrid)Parent)?.GridTemplates?.DetailTemplate != null)
		spanCount++;
```

---

### 9. Grouping + Row Drag & Drop (`AllowRowDragAndDrop`)

Extra `<td class="e-rowdragdrop">` is added to each row. In caption rows, this is a `CellType.RowDrag` cell rendered via `RenderRowDragCell()` in `RowDragCellRenderer.cs`.

```
spanCount++ also applies in GridContent empty-row colspan:
if (Parent.AllowRowDragAndDrop) spanCount++;
```

---

### 10. Grouping + ShowGroupedColumn

#### ShowGroupedColumn=false (default behavior)

```
GroupColumn("CustomerID") called:
	→ Column["CustomerID"].Visible = false
	→ Column.IsHiddenByGrouping = true
	→ Header row: "CustomerID" <th> hidden
	→ Drop area: chip for "CustomerID" visible
	→ Caption cell displays "Customer ID: ALFKI - 2 Items"
	→ Indent count still matches depth
```

#### ShowGroupedColumn=true

```
GroupColumn("CustomerID") called:
	→ Column["CustomerID"].Visible = true (unchanged)
	→ Header row: "CustomerID" <th> still visible
	→ Data rows: CustomerID column cell still rendered
```

**CSS class `e-hide` is NOT used** — `Visible=false` on `GridColumn` prevents the `<th>` and `<td>` from rendering entirely.

---

### 11. Combined Feature DOM: Virtualization + Lazy Load + Grouping

```
GridVirtualContent.razor
 └── <div class="e-virtualtable">
			└── <table class="e-table">
					 └── <tbody role="rowgroup">

							 TOP MASK (PageSize rows)
							 ┌──────────────────────────────────────────┐
							 │ <tr class="e-masked-row e-row">           │
							 │   <td class="e-masked-cell">×groupCols    │  ← indent mask
							 │   <td class="e-masked-cell"><span class=  │
							 │       "e-virtualcell"/></td> ×visibleCols │
							 │ </tr>                                     │
							 └──────────────────────────────────────────┘

							 VISIBLE CAPTION ROWS ONLY (lazy: collapsed)
							 ┌──────────────────────────────────────────┐
							 │ <tr class="e-row e-groupcaptionrow">      │
							 │   <td class="e-recordpluscollapse">►</td> │
							 │   <td class="e-groupcaption">...</td>     │
							 │ </tr>                                     │
							 │  ... (N caption rows in virtual window)   │
							 └──────────────────────────────────────────┘

							 BOTTOM MASK (MaskRowCount rows)
							 ┌──────────────────────────────────────────┐
							 │ <tr class="e-masked-row e-row"> ... </tr> │
							 └──────────────────────────────────────────┘
```

After lazy expand (virtual + lazy):
- `VirtualScrollModule.RefreshVirtualContent(index, Rows)` recalculates start/end indexes.
- JS interop: `sfBlazor.Grid.lazyGroupExpand(DataId, GetClientOption())` notifies client.
- Only the virtual window slice is re-rendered; mask rows adjust counts.

---

### 12. Quick Reference: DOM Differences by Feature Flag

| Feature Flag | Caption Row change | Extra rows | Extra cells | CSS additions |
|---|---|---|---|---|
| Default (basic grouping) | Standard caption `<td>` | — | Indent `<td>` × depth | `e-groupcaptionrow`, `e-indentcell` |
| `GroupFooterTemplate` | None | `<tr class="e-summaryrow">` after children | — | `e-summaryrow`, `e-summarycell` |
| `GroupCaptionTemplate` | Extra `<td>` in caption row | None | `e-groupcaptionsummarycell` `<td>` | `e-groupcaptionsummarycell` |
| `EnableLazyLoading` | Same structure, initially collapsed | Inserted on expand | `e-firstchildrow` on first child | `e-firstchildrow`, `e-lazyload-last-down` |
| `EnableVirtualization` | Standard; mask rows around window | Mask `<tr>` ×PageSize top/bottom | Mask `<td>` ×groupCols per mask row | `e-masked-row`, `e-masked-cell`, `e-virtualcell` |
| `FrozenColumns` | Second `<td>` in caption (right pane) | None | Extra frozen `<td>` | `e-leftfreeze` on left pane cells |
| `AllowPaging` | None | None | None | — |
| `EnableInfiniteScrolling` | None | `e-lazyload-last-down` tr | None | `e-lazyload-last-down` |
| `DetailTemplate` | +1 to colspan | Detail `<tr>` per expanded data row | `e-detailindentcell <th>` in header | `e-detailrow`, `e-detailcell` |
| `AllowRowDragAndDrop` | +1 to colspan | None | `e-rowdragdrop <td>` | `e-rowdragdrop` |
| `ShowGroupedColumn=true` | No change | None | Column `<th>`/`<td>` remains | — |
| `ShowGroupedColumn=false` | No change | None | Column `<th>`/`<td>` removed | `IsHiddenByGrouping=true` |
| `EnableStickyHeader` | No change | None | None | `e-sticky` on `e-groupdroparea` |
| `ShowToggleButton=true` | No change in caption | None | `e-togglegroupbutton` in chip | `e-togglegroupbutton` |
| `ShowUngroupButton=false` | No change in caption | None | Chip ungroup btn hidden | `e-ungroupbutton-hidden` |

---

## JavaScript Interop Reference
<!-- token-budget: 100 words -->

### C# → JavaScript Calls

The grouping feature uses JavaScript interop for client-side DOM operations, scroll management, and grid state synchronization.

| C# Method Location | JavaScript Function | Purpose |
|---|---|---|
| `Group.cs` line 289 | `updateOptions(dataId, options)` | Update grid options after group/ungroup operation |
| `Group.cs` line 430 | `lazyLoadGridHeight(dataId)` | Get grid content height for lazy load calculation |
| `Group.cs` lines 453, 459 | `lazyGroupExpand(dataId, options)` | Notify client of lazy group expansion for virtual scroll sync |
| `Group.cs` lines 1204, 1295 | `resetExpandCollapseAllScroll(dataId, requestType)` | Reset infinite scroll positions after expand/collapse all |
| `GroupedHeader.razor` line 65 | `focusFirstGroupHeader(dataId)` | Focus first grouped column chip for keyboard navigation |

### TypeScript Function Signatures

**File:** `scripts/sf-grid.ts`

```typescript
// Update grid client-side options
updateOptions(dataId: string, options: IGridOptions): void  // line 169

// Sync lazy group expansion with virtual scroll
lazyGroupExpand(dataId: string, options: IGridOptions): void  // line 188

// Set focus to first group header chip
focusFirstGroupHeader(dataId: string): void  // line 634

// Get content height for lazy load row calculation
lazyLoadGridHeight(dataId: string): number  // line 1099

// Reset infinite scroll state after expand/collapse all
resetExpandCollapseAllScroll(dataId: string, requestType: string): void  // line 1108
```

### Responsibility Split

| Responsibility | C# (Blazor) | JavaScript (TypeScript) |
|---|---|---|
| Group state mutation (`IsExpand`, `Visible`) | ✅ | ❌ |
| Row model generation (`GroupModelGenerator`) | ✅ | ❌ |
| Expand/collapse logic and lazy loading | ✅ | ❌ |
| Virtual scroll position synchronization | ❌ | ✅ |
| Grid options client-side update | ❌ | ✅ |
| Lazy load height calculation (DOM measurement) | ❌ | ✅ |
| Infinite scroll state reset | ❌ | ✅ |
| Keyboard focus management for chips | ❌ | ✅ |

### CRITICAL Rules

1. **Never mutate group state from JavaScript** — All expand/collapse operations, row visibility changes, and group membership must go through `Group.cs` on the C# side. JavaScript only reads state and updates client options.

2. **Always await JS interop calls** — All `InvokeMethod` calls use `.ConfigureAwait(true)` to ensure Blazor synchronization context is preserved. Never fire-and-forget JS interop for grouping operations.

3. **Verify grid instance exists** — All TypeScript functions check `!isNullOrUndefined(gridInstance)` and `!isNullOrUndefined(gridInstance.element.blazor__instance)` before performing operations to prevent null reference errors.

4. **Use DataId for instance lookup** — Never cache JavaScript grid instances across operations. Always use `sfBlazor.getCompInstance(dataId)` to retrieve the current instance, as the instance may be recreated during lifecycle events.

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{dom-generation | dom-review | bug-fix}`  
**Skill**: Grouping DOM Structure  

**Context**  
You are implementing, documenting, or debugging the DOM output of the Syncfusion Blazor DataGrid when grouping is active. This skill contains the complete embedded reference for grouping DOM structure, rendering flow, and feature-combination DOM behavior.

**Core Operating Rules**
1. Keep the standard grouped row hierarchy: caption row → nested caption/data rows → optional summary row.
2. Derive DOM from row/cell metadata, not from guessed markup.
3. Use the correct renderer for each `CellType`.
4. Maintain `Left: Npx` offsets using the grouping indent formulas in this skill.
5. Recompute caption `colspan` whenever visible columns, grouping depth, or detail template participation changes.
6. Preserve feature-combination DOM differences exactly as documented in Part 3.
7. For lazy loading and virtualization, validate both the visible grouped rows and the placeholder/mask row structure.
8. For frozen layouts, validate whether a second caption `<td>` is required or explicitly suppressed.

**What to verify before changing code**
- The `RowType`, `Indent`, `Visible`, and `IsExpand` values that drive DOM shape
- The `CellType` sequence in each row
- Caption text source and template handling
- CSS classes on drop area, grouped chips, caption rows, summary rows, and virtual placeholders
- `aria-*`, `data-uid`, `data-sf-style`, and `colspan` correctness

**Deliverables**
- DOM changes remain consistent with the embedded Part 1, Part 2, and Part 3 references
- No cross-feature breakage for lazy loading, virtualization, frozen columns, paging, infinite scroll, detail rows, or row drag-and-drop
- Any fix preserves both render flow correctness and final HTML structure
```
