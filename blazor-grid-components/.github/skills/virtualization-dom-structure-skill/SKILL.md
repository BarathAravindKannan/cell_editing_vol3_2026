---
name: virtualization-dom-structure-skill
description: Expert knowledge for the rendered DOM structure and Blazor component rendering flow for Row Virtualization, Column Virtualization, Mask Row rendering, and their combined modes in the Syncfusion Blazor DataGrid. Use this skill when implementing or debugging rendering flow, component tree, DOM layout, translate transforms, and virtual track/table structure.
---

# Virtualization DOM Structure Skill
<!-- token-budget: 20 words -->

**Purpose**  
Authoritative reference for the Blazor component render tree and DOM layout produced by Row & Column Virtualization. Covers `GridVirtualHeader`, `GridVirtualContent`, mask rows, virtual track divs, and all combinations. Guarantees correct DOM output without breaking any other feature.

---

## Knowledge References
<!-- token-budget: 60 words -->

- **`.github/skills/virtualization-skill/SKILL.md`** — **Core virtualization engine** (use for cache management, scroll handlers, index calculation, `GeneratedData`, `ClearCacheData()`, `_coffSets`, `src/Internal/Actions/VirtualScroll.cs` logic)
- `src/Internal/Actions/VirtualScroll.cs` — Core virtualization engine module
- `src/SfGrid.Lifecycle.cs` — `VirtualScrollModule` initialization (line 33)
- `src/Internal/Renderer/GridVirtualContent.razor` — Virtual content component (rows)
- `src/Internal/Renderer/GridVirtualHeader.razor` — Virtual header component (columns)
- `src/Internal/Renderer/GridContent.razor` — Standard content component (also renders mask rows)
- `src/Internal/Renderer/GridHeader.razor` — Standard header component (renders frozen row tbody)
- `src/Internal/Renderer/GridRow.razor` — Individual row component used inside all content renderers
- `src/Internal/SfGrid.razor` — Root template; decides which header/content variant is rendered
- `scripts/sf-grid.ts` — Main grid JavaScript module with virtual scroll client-side functions (lines 158, 302, 309, 1137)
- `scripts/virtual-scroll.ts` — Virtual scroll client-side event handlers (lines 473, 2021)
- `scripts/scroll.ts` — Scroll event management and DOM manipulation

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **`EnableColumnVirtualization` governs header path** — when `true`, `SfGrid.razor` renders `GridVirtualHeader` instead of `GridHeader` inside `.e-gridheader > .e-headercontent`
- **`EnableVirtualization || EnableColumnVirtualization` governs content path** — when either is `true`, `SfGrid.razor` renders `GridVirtualContent` instead of `GridContent`
- **Both flags can be active simultaneously** — the same `GridVirtualContent` + `GridVirtualHeader` pair handles the combined mode; column windowing operates independently of row windowing
- **Mask rows are skeleton placeholders** — rendered when `EnableVirtualMaskRow = true` and the scroll position has outpaced the data cache; they appear above and below the real rows inside the same `<tbody>`
- **Virtual track div drives scrollbar height/width** — `.e-virtualtrack` height = `TotalItemCount × RHeight`; width = sum of all column widths; never contains real content
- **`transform: translate(X, Y)` on `.e-virtualtable`** — positions the rendered window inside the scrollable container without moving the scrollbar
- **Frozen rows rendered in `GridHeader`'s `<tbody>`** — not in `GridVirtualContent`; they stay pinned at top via the header section's tbody

---

## DOM Structure Map
<!-- token-budget: 500 words -->

> **Focus**: Virtualization features only — Row Virtualization, Column Virtualization, Mask Rows, and OverScan.

---

### 1. Row Virtualization Only (`EnableVirtualization = true`)

```
SfGrid<TValue>  (#ID  .e-grid)
├── .e-gridheader
│   └── .e-headercontent
│       └── GridHeader          [renders ALL column headers]
│           ├── <thead>
│           │   └── <tr .e-columnheader>
│           │       └── GridHeaderCell × VisibleColumns.Count   [ALL columns rendered]
│           │
│           └── <tbody>         (frozen rows if FrozenRows > 0)
│               └── GridRow × FrozenRows  [pinned, not scrolled]
│                   └── <tr>
│                       └── <td> × VisibleColumns.Count  [ALL cells for frozen rows]
│
└── .e-gridcontent
    └── .e-content
        └── GridVirtualContent  [replaces GridContent — row windowing active]
            ├── <div .e-virtualtable
            │       style="transform: translate(0px, {TranslateY}px);
            │              height: {viewport-window-height}px;">
            │   └── <table .e-table #ID_content_table>
            │       ├── <WidthController>  [ALL columns for width calculation]
            │       └── <tbody role="rowgroup">
            │           │
            │           ├─ TOP MASK ROWS (when EnableVirtualMaskRow=true AND cache miss)
            │           │  └── <tr .e-masked-row .e-row> × PageSize
            │           │      └── <td .e-masked-cell .e-rowcell> × VisibleColumns.Count
            │           │          └── <span .e-virtualcell></span>
            │           │
            │           ├─ REAL DATA ROWS (actual rendered viewport window)
            │           │  └── GridRow × (RowEndIndex − RowStartIndex)  ← computed window
            │           │      [Component: GridRow.razor]
            │           │      └── <tr role="row" class="e-row e-altrow?">
            │           │          └── GetCells(startColIdx=null, endColIdx=null)
            │           │              ↓ returns ALL cells (no column windowing)
            │           │              └── <td .e-rowcell> × VisibleColumns.Count
            │           │                  ├── <span .e-virtualcell>...data...</span>
            │           │                  └── (or CellRender component for templates)
            │           │
            │           └─ BOTTOM MASK ROWS (when RowEndIndex < TotalItemCount)
            │              └── <tr .e-masked-row .e-row> × min(PageSize, remaining)
            │                  └── <td .e-masked-cell .e-rowcell> × VisibleColumns.Count
            │                      └── <span .e-virtualcell></span>
            │
            └── <div .e-virtualtrack
                        style="height: {TotalItemCount × RHeight}px; width: 100%;">
                (empty spacer — drives vertical scrollbar range)
```

**Key Computed Values**
| Property | Source | Purpose |
|---|---|---|
| `RowStartIndex` | `⌊ScrollTop ÷ RHeight⌋ − OverscanCount` | First row index in viewport window (clamped ≥ 0) |
| `RowEndIndex` | `RowStartIndex + PageSize + (2 × OverscanCount)` | Last row index in viewport window (clamped ≤ TotalItemCount) |
| `TranslateY` | `RowStartIndex × RHeight` | CSS translateY offset to position window |
| `.e-virtualtrack` height | `TotalItemCount × RHeight` | Full logical scroll height |
| Top mask row count | `PageSize` (if cache miss at scroll start) | Skeleton rows shown before real data arrives |
| Bottom mask row count | `min(PageSize, TotalItemCount − RowEndIndex)` | Skeleton rows shown below viewport |
| **OverscanCount** | User-configured (default: 0) | Extra rows rendered above/below viewport to reduce flash during fast scroll |

---

### 2. Column Virtualization Only (`EnableColumnVirtualization = true`)

```
SfGrid<TValue>  (#ID  .e-grid)
├── .e-gridheader
│   └── .e-headercontent
│       └── GridVirtualHeader   [replaces GridHeader — column windowing active]
│           ├── <div .e-virtualtable
│           │       style="transform: translate({TranslateX}px, 0px);
│           │              width: {VirtualTableWidth}px;">
│           │   └── GridHeader  [inner component — renders VirtualizedColumns slice]
│           │       └── <table .e-table #ID_header_table>
│           │           ├── <WidthController columns=VirtualizedColumns>
│           │           │   ↓ VirtualizedColumns = FrozenLeftCols + WindowCols + FrozenRightCols
│           │           │
│           │           └── <thead>
│           │               └── <tr .e-columnheader>
│           │                   ├── GridHeaderCell × FrozenLeftCols.Count  [always rendered]
│           │                   ├── GridHeaderCell × (EndColIdx − StartColIdx + 1)  [window slice]
│           │                   └── GridHeaderCell × FrozenRightCols.Count  [always rendered]
│           │
│           └── <div .e-virtualtrack
│                       style="height: 1px;
│                              width: {sum of ALL column widths}px;">
│               (horizontal spacer — drives horizontal scrollbar range)
│
└── .e-gridcontent
    └── .e-content
        └── GridVirtualContent  [also active — column slicing per row]
            ├── <div .e-virtualtable
            │       style="transform: translate({TranslateX}px, 0px);
            │              width: {VirtualTableWidth}px;">
            │   └── <table .e-table>
            │       ├── <WidthController columns=VirtualizedColumns>
            │       └── <tbody>
            │           └── GridRow × TotalItemCount  [ALL rows rendered — no row windowing]
            │               [Component: GridRow.razor]
            │               └── <tr role="row">
            │                   └── GetCells(StartColIdx, EndColIdx)  ← column window passed
            │                       ↓ returns cells in range [StartColIdx..EndColIdx]
            │                       │  + FrozenLeft cells (always included)
            │                       │  + FrozenRight cells (always included)
            │                       │
            │                       ├── <td .e-rowcell> × FrozenLeftCols.Count  [pinned left]
            │                       ├── <td .e-rowcell> × (EndColIdx − StartColIdx + 1)  [window]
            │                       └── <td .e-rowcell> × FrozenRightCols.Count  [pinned right]
            │
            └── <div .e-virtualtrack
                        style="height: {TotalItemCount × RHeight}px;
                               width: {sum of ALL column widths}px;">
```

**Key Computed Values**
| Property | Source | Purpose |
|---|---|---|
| `StartColumnIndex` | Computed from `scrollLeft` + `_coffSets` cumulative width map | First movable column index in viewport |
| `EndColumnIndex` | `StartColumnIndex` + columns fitting in viewport width | Last movable column index in viewport |
| `TranslateX` | `_coffSets[StartColumnIndex]` (cumulative width offset) | CSS translateX offset to position column window |
| `VirtualTableWidth` | `GetColumnsWidth(VirtualizedColumns)` | Width of currently rendered column slice |
| `.e-virtualtrack` width | `GetColumnsWidth(ALL columns)` | Full logical scroll width |
| `VirtualizedColumns` | `FrozenLeft + Columns[Start..End] + FrozenRight` | Actual column list passed to WidthController and GridRow |

---

### 3. Combined Row + Column Virtualization (`EnableVirtualization = true` AND `EnableColumnVirtualization = true`)

```
SfGrid<TValue>  (#ID  .e-grid)
├── .e-gridheader
│   └── .e-headercontent
│       └── GridVirtualHeader   [column windowing active in header]
│           ├── <div .e-virtualtable
│           │       style="transform: translate({TranslateX}px, 0px);
│           │              width: {VirtualTableWidth}px;">
│           │   └── GridHeader (inner)
│           │       └── <table .e-table>
│           │           ├── <WidthController columns=VirtualizedColumns>
│           │           └── <thead> → <tr>
│           │               ├── GridHeaderCell × FrozenLeftCols.Count
│           │               ├── GridHeaderCell × (EndColIdx − StartColIdx + 1)
│           │               └── GridHeaderCell × FrozenRightCols.Count
│           │
│           └── <div .e-virtualtrack style="height:1px; width:{all-cols-width}px;">
│
└── .e-gridcontent
    └── .e-content
        └── GridVirtualContent  [BOTH row AND column windowing active]
            ├── <div .e-virtualtable
            │       style="transform: translate({TranslateX}px, {TranslateY}px);  ← 2D transform
            │              width: {VirtualTableWidth}px;
            │              height: {window-height}px;">
            │   └── <table .e-table>
            │       ├── <WidthController columns=VirtualizedColumns>
            │       └── <tbody>
            │           │
            │           ├─ TOP MASK ROWS (when EnableVirtualMaskRow=true AND cache miss)
            │           │  └── <tr .e-masked-row .e-row> × PageSize
            │           │      ├── <td .e-masked-cell> × FrozenLeftCols.Count
            │           │      ├── <td .e-masked-cell> × (EndColIdx − StartColIdx + 1)
            │           │      └── <td .e-masked-cell> × FrozenRightCols.Count
            │           │
            │           ├─ REAL DATA ROWS (row AND column window intersection)
            │           │  └── GridRow × (RowEndIndex − RowStartIndex + 2×OverscanCount)
            │           │      [Component: GridRow.razor]
            │           │      └── <tr role="row">
            │           │          └── GetCells(StartColIdx, EndColIdx)  ← both windows active
            │           │              ↓ returns cell window intersection
            │           │              │
            │           │              ├── <td .e-rowcell> × FrozenLeftCols  [always rendered]
            │           │              │   └── <span .e-virtualcell>...data...</span>
            │           │              │
            │           │              ├── <td .e-rowcell> × (EndColIdx − StartColIdx + 1)  [window]
            │           │              │   └── <span .e-virtualcell>...data...</span>
            │           │              │
            │           │              └── <td .e-rowcell> × FrozenRightCols  [always rendered]
            │           │                  └── <span .e-virtualcell>...data...</span>
            │           │
            │           └─ BOTTOM MASK ROWS (when RowEndIndex < TotalItemCount)
            │              └── <tr .e-masked-row .e-row> × min(PageSize, remaining)
            │                  ├── <td .e-masked-cell> × FrozenLeftCols.Count
            │                  ├── <td .e-masked-cell> × (EndColIdx − StartColIdx + 1)
            │                  └── <td .e-masked-cell> × FrozenRightCols.Count
            │
            └── <div .e-virtualtrack
                        style="height: {TotalItemCount × RHeight}px;
                               width: {all-cols-width}px;">
```

> **2D Windowing**: Both axes are independently windowed.  
> - Row `TranslateY` positions the vertical row window  
> - Column `TranslateX` positions the horizontal column window  
> - Both transforms applied together as `translate(X, Y)` on the same `.e-virtualtable`  
> - **OverscanCount** adds buffer rows above and below viewport to reduce blank flash during scroll

**Key Computed Values**
| Property | Source | Purpose |
|---|---|---|
| `RowStartIndex` | `⌊ScrollTop ÷ RHeight⌋ − OverscanCount` | First row in viewport (clamped ≥ 0) |
| `RowEndIndex` | `RowStartIndex + PageSize + (2 × OverscanCount)` | Last row in viewport (clamped ≤ TotalItemCount) |
| `StartColumnIndex` | Computed from `scrollLeft` + `_coffSets` | First movable column in viewport |
| `EndColumnIndex` | `StartColumnIndex` + columns fitting viewport width | Last movable column in viewport |
| `TranslateY` | `RowStartIndex × RHeight` | Vertical CSS offset |
| `TranslateX` | `_coffSets[StartColumnIndex]` | Horizontal CSS offset |
| DOM cell count | `(RowEnd−RowStart) × (EndCol−StartCol + FrozenCols)` | Actual rendered `<td>` elements (minimal) |

---

### 4. Frozen Rows with Row Virtualization (`FrozenRows > 0` + `EnableVirtualization = true`)

```
SfGrid<TValue>
├── .e-gridheader
│   └── .e-headercontent
│       └── GridHeader
│           ├── <thead> → column headers (all columns)
│           │
│           └── <tbody>          ← FROZEN ROW TBODY (pinned at top, never scrolls)
│               │
│               ├─ FROZEN DATA ROWS (real data from Parent.Rows[0..FrozenRows-1])
│               │  └── GridRow × FrozenRows
│               │      └── <tr>
│               │          └── <td .e-rowcell> × VisibleColumns.Count  [all cells]
│               │
│               └─ FROZEN MASK ROWS (only if EnableVirtualMaskRow=true)
│                  └── <tr .e-masked-row> × FrozenRows
│                      └── <td .e-masked-cell> × VisibleColumns.Count
│
├── <div .e-frozenrow-border>    ← visible horizontal divider line
│
└── .e-gridcontent
    └── GridVirtualContent
        └── <div .e-virtualtable style="transform: translate(0, {TranslateY}px);">
            └── <table .e-table>
                └── <tbody>
                    │
                    ├─ TOP MASK ROWS (when cache miss)
                    │  └── <tr .e-masked-row> × PageSize
                    │
                    ├─ SCROLLABLE DATA ROWS (virtualized window EXCLUDING frozen rows)
                    │  └── GridRow × (RowEndIndex − RowStartIndex)
                    │      [Source: Parent.Rows.Skip(FrozenRows + addedTopCount)]
                    │      └── <tr>
                    │          └── <td .e-rowcell> × VisibleColumns.Count
                    │
                    └─ BOTTOM MASK ROWS
                       └── <tr .e-masked-row> × min(PageSize, remaining)
```

**Key Rules**
| Aspect | Behavior |
|---|---|
| Frozen row rendering | Always in `GridHeader <tbody>`, never in `GridVirtualContent` |
| Frozen row data source | `Parent.Rows[0..FrozenRows-1]` |
| Virtual row data source | `Parent.Rows.Skip(FrozenRows + addedTopCount)` |
| Frozen row scroll behavior | Pinned at top; do not move with `TranslateY` |
| Virtual track height adjustment | `(TotalItemCount − FrozenRows) × RHeight` |
| Row index mapping | Virtual `RowStartIndex` = 0 corresponds to data row at index `FrozenRows` |

---

### 5. OverScan Behavior (`OverscanCount > 0`)

> **OverScan** renders extra rows above and below the visible viewport to reduce blank-flash during fast scroll.

```
Viewport boundary calculation with OverscanCount = 3:

Visual viewport height = 500px
RowHeight = 40px
→ Visible rows = ⌊500 ÷ 40⌋ = 12 rows

Current ScrollTop = 800px
→ Logical RowStartIndex = ⌊800 ÷ 40⌋ = 20

WITHOUT OverscanCount (OverscanCount = 0):
  RowStartIndex = 20
  RowEndIndex = 20 + PageSize = 20 + 24 = 44
  Rendered rows in DOM: 24 rows (index 20..43)
  
WITH OverscanCount = 3:
  RowStartIndex = 20 − 3 = 17  (clamped ≥ 0)
  RowEndIndex = 20 + PageSize + 3 = 20 + 24 + 3 = 47  (clamped ≤ TotalItemCount)
  Rendered rows in DOM: 30 rows (index 17..46)
  
  ┌──────────────────────────────────────────────────────┐
  │  Index 17, 18, 19            ← OVERSCAN TOP (3 rows) │
  ├──────────────────────────────────────────────────────┤
  │  Index 20..31                ← VISIBLE VIEWPORT      │
  │                              (12 rows @ 500px height)│
  ├──────────────────────────────────────────────────────┤
  │  Index 32..46                ← PAGE BUFFER (12 rows) │
  │  (includes 3 overscan bottom)                        │
  └──────────────────────────────────────────────────────┘

Fast scroll DOWN by 5 rows → new ScrollTop = 1000px
  New logical start = ⌊1000 ÷ 40⌋ = 25
  → Rows 22..24 (from previous overscan) already in DOM
  → Only rows 47..52 need rendering
  → NO blank flash; smooth transition
```

**OverscanCount Configuration**
| Value | Trade-off |
|---|---|
| 0 (default) | Minimal DOM; may show blank rows during fast scroll |
| 2–5 (recommended) | Smooth fast scroll; slight increase in DOM node count |
| 10+ (high) | Excellent scroll fluidity; higher memory/render cost |

---

### 6. Grouping with Virtualization (`AllowGrouping = true` + Row/Column Virtualization)

> **Grouping** changes row generation from `RowModelGenerator` to `GroupModelGenerator`. Each group creates a **caption row** + **data rows**, with indent cells for hierarchy.

#### Row Generation Flow with Grouping

```
Data Source → GroupedData → GroupModelGenerator → Row<object> collection
                                                    │
                                                    ├─ Caption Row (CellType.GroupCaption)
                                                    ├─ Data Row 1 (with Indent cells)
                                                    ├─ Data Row 2 (with Indent cells)
                                                    ├─ ...
                                                    │
                                                    ├─ Caption Row (next group)
                                                    └─ ...

GroupModelGenerator logic:
  1. Read GroupSettings.Columns (e.g., ["Country", "City"])
  2. For each group level:
     - Create caption row (CellType.GroupCaption + CellType.Expand)
     - Add indent cells (CellType.Indent × group depth)
     - Insert data rows (each with indent cells)
  3. Calculate VisibleGroupRows.Count (excludes collapsed groups)
  4. Set Row.IsExpand = true/false (controls visibility)

Virtual Track Height Calculation:
  - WITHOUT Grouping: TotalItemCount × RHeight
  - WITH Grouping: VisibleGroupRows.Count × RHeight
                   (includes caption rows + visible data rows)

Cache Management:
  - Row Virtualization: GroupGeneratedData[index] replaces GeneratedData[index]
  - Lazy Loading: Parent.GroupSettings.EnableLazyLoading = true
                 → Groups load on-demand via GetUiData() + GenerateLazyRowsobject()
```

#### DOM Structure: Row Virtualization + Grouping

```
SfGrid<TValue> (EnableVirtualization=true, AllowGrouping=true, GroupSettings.Columns=["Country"])
└── .e-gridcontent
    └── GridVirtualContent
        ├── <div .e-virtualtable style="transform: translate(0, {TranslateY}px);">
        │   └── <table .e-table>
        │       └── <tbody>
        │           │
        │           ├─ TOP MASK ROWS (if EnableVirtualMaskRow)
        │           │  └── <tr .e-masked-row> × PageSize
        │           │      ├── <td .e-masked-cell .e-indentcell></td>  ← indent for group depth
        │           │      └── <td .e-masked-cell .e-rowcell> × VisibleColumns.Count
        │           │
        │           ├─ GROUP CAPTION ROW (CellType.GroupCaption)
        │           │  └── GridRow (Row.CellType includes GroupCaption + Expand)
        │           │      └── <tr class="e-groupcaptionrow">
        │           │          ├── <td class="e-recordplusexpand" role="gridcell">
        │           │          │   └── <span class="e-icons e-gdiagonaldown"></span>  ← expand icon
        │           │          │
        │           │          └── <td class="e-groupcaption" colspan="{remaining cols}">
        │           │              └── "Country: USA - 15 items"  ← caption text
        │           │
        │           ├─ DATA ROW 1 (with indent cells)
        │           │  └── GridRow
        │           │      └── <tr class="e-row">
        │           │          ├── <td class="e-indentcell"></td>  ← indent (group depth × 30px)
        │           │          ├── <td class="e-rowcell">...CustomerID...</td>
        │           │          ├── <td class="e-rowcell">...OrderID...</td>
        │           │          └── <td class="e-rowcell">...Freight...</td>
        │           │
        │           ├─ DATA ROW 2 (with indent cells)
        │           │  └── <tr class="e-row">
        │           │      ├── <td class="e-indentcell"></td>
        │           │      └── <td .e-rowcell> × N
        │           │
        │           ├─ GROUP CAPTION ROW (next group)
        │           │  └── <tr class="e-groupcaptionrow">
        │           │      ├── <td class="e-recordplusexpand"></td>
        │           │      └── <td class="e-groupcaption">Country: Germany - 12 items</td>
        │           │
        │           ├─ DATA ROW 3 (Germany group)
        │           │  └── <tr class="e-row">
        │           │      ├── <td class="e-indentcell"></td>
        │           │      └── <td .e-rowcell> × N
        │           │
        │           └─ BOTTOM MASK ROWS
        │              └── <tr .e-masked-row> × min(PageSize, remaining)
        │
        └── <div .e-virtualtrack
                style="height: {VisibleGroupRows.Count × RHeight}px;">
                ↑ includes caption rows + visible data rows (excludes collapsed groups)
```

#### Multi-Level Grouping (e.g., Country → City)

```
GroupSettings.Columns = ["Country", "City"]

<tbody>
  ├─ CAPTION: Country = USA (Level 0)
  │  └── <tr class="e-groupcaptionrow">
  │      ├── <td class="e-recordplusexpand"></td>  ← expand/collapse
  │      └── <td class="e-groupcaption">Country: USA - 25 items</td>
  │
  ├─── CAPTION: City = New York (Level 1, nested under USA)
  │    └── <tr class="e-groupcaptionrow">
  │        ├── <td class="e-indentcell"></td>  ← indent for level 0
  │        ├── <td class="e-recordplusexpand"></td>  ← expand/collapse
  │        └── <td class="e-groupcaption">City: New York - 10 items</td>
  │
  ├──── DATA ROW (Level 2, nested under New York)
  │     └── <tr class="e-row">
  │         ├── <td class="e-indentcell"></td>  ← indent for level 0
  │         ├── <td class="e-indentcell"></td>  ← indent for level 1
  │         ├── <td class="e-rowcell">CustomerID</td>
  │         └── <td class="e-rowcell"> × N
  │
  ├──── DATA ROW
  │     └── <tr>
  │         ├── <td class="e-indentcell"></td>
  │         ├── <td class="e-indentcell"></td>
  │         └── <td .e-rowcell> × N
  │
  ├─── CAPTION: City = Los Angeles (Level 1)
  │    └── <tr class="e-groupcaptionrow">
  │        ├── <td class="e-indentcell"></td>
  │        ├── <td class="e-recordplusexpand"></td>
  │        └── <td class="e-groupcaption">City: Los Angeles - 15 items</td>
  │
  └──── ... (more data rows)
```

**Indent Cell Calculation**
| Group Level | Indent Cells | Total Indent Width |
|---|---|---|
| 0 (top-level caption) | 0 | 0px |
| 1 (nested caption) | 1 | 30px |
| 2 (data row under level 1) | 2 | 60px |
| 3 (data row under level 2) | 3 | 90px |

---

#### Column Virtualization + Grouping

```
SfGrid<TValue> (EnableColumnVirtualization=true, AllowGrouping=true)
└── .e-gridcontent
    └── GridVirtualContent
        ├── <div .e-virtualtable style="transform: translate({TranslateX}px, 0);">
        │   └── <table .e-table>
        │       └── <tbody>
        │           ├─ GROUP CAPTION ROW
        │           │  └── <tr class="e-groupcaptionrow">
        │           │      ├── <td class="e-recordplusexpand"></td>  ← ALWAYS rendered (not windowed)
        │           │      └── <td class="e-groupcaption" colspan="{VirtualizedColumns.Count}">
        │           │          ↑ colspan adjusts to visible column count
        │           │
        │           ├─ DATA ROW (column windowing active)
        │           │  └── GridRow
        │           │      └── <tr>
        │           │          ├── <td class="e-indentcell"></td>  ← ALWAYS rendered
        │           │          │
        │           │          └── GetCells(StartColIdx, EndColIdx)  ← column window
        │           │              ├── <td .e-rowcell> × FrozenLeftCols
        │           │              ├── <td .e-rowcell> × (EndCol−StartCol+1)
        │           │              └── <td .e-rowcell> × FrozenRightCols
        │           │
        │           └─ ... (more groups + rows)
        │
        └── <div .e-virtualtrack style="width: {sum of ALL column widths}px;">
```

**Key Rules for Grouping + Column Virtualization**
| Element | Column Windowing Behavior |
|---|---|
| Indent cells | Always rendered (bypass column window) |
| Expand/collapse cell | Always rendered (bypass column window) |
| Caption cell | Colspan = VirtualizedColumns.Count (adjusts dynamically) |
| Data cells | Subject to column windowing (GetCells filters) |

---

#### Combined Row + Column Virtualization + Grouping

```
SfGrid<TValue> (EnableVirtualization=true, EnableColumnVirtualization=true, AllowGrouping=true)
└── GridVirtualContent
    ├── <div .e-virtualtable style="transform: translate({TranslateX}px, {TranslateY}px);">
    │   └── <table>
    │       └── <tbody>
    │           ├─ TOP MASK ROWS (if EnableVirtualMaskRow)
    │           │  └── <tr .e-masked-row> × PageSize
    │           │      ├── <td .e-masked-cell .e-indentcell></td>  ← indent placeholder
    │           │      └── <td .e-masked-cell> × VirtualizedColumns.Count
    │           │
    │           ├─ GROUP CAPTION ROW (in row window)
    │           │  └── <tr class="e-groupcaptionrow">
    │           │      ├── <td class="e-recordplusexpand"></td>
    │           │      └── <td class="e-groupcaption" colspan="{VirtualizedColumns.Count}">
    │           │
    │           ├─ DATA ROWS (row window × column window intersection)
    │           │  └── GridRow × (RowEndIndex − RowStartIndex)
    │           │      └── <tr>
    │           │          ├── <td class="e-indentcell"> × groupDepth  [always rendered]
    │           │          │
    │           │          └── GetCells(StartColIdx, EndColIdx)
    │           │              ├── <td> × FrozenLeftCols
    │           │              ├── <td> × (EndCol−StartCol+1)
    │           │              └── <td> × FrozenRightCols
    │           │
    │           └─ BOTTOM MASK ROWS
    │
    └── <div .e-virtualtrack
                style="height: {VisibleGroupRows.Count × RHeight}px;
                       width: {sum ALL column widths}px;">
```

**Row Generation Formula (Grouping + Virtualization)**
```
RowStartIndex → maps to GroupGeneratedData[index]
  ↓
GroupModelGenerator builds:
  - Caption rows (with CellType.GroupCaption + Expand)
  - Data rows (with CellType.Indent × group depth)
  ↓
Parent.Rows = flattened list of caption + data rows
  ↓
Virtual window = Parent.Rows[RowStartIndex..RowEndIndex]
  ↓
VisibleGroupRows = rows where Row.IsExpand = true OR Row is caption
  ↓
Virtual track height = VisibleGroupRows.Count × RHeight
```

---

#### Lazy Loading with Grouping + Virtualization

```
GroupSettings.EnableLazyLoading = true
  ↓
Groups load on-demand when expanded:

BEFORE Expand (collapsed group):
  ├─ CAPTION: Country = USA (collapsed)
  │  └── <tr class="e-groupcaptionrow">
  │      ├── <td class="e-recordpluscollapse">
  │      │   └── <span class="e-icons e-gdiagonalright"></span>  ← right arrow
  │      └── <td class="e-groupcaption">Country: USA</td>
  │
  └─ (no data rows — not in DOM yet)

AFTER Expand (user clicks expand icon):
  ├─ CAPTION: Country = USA (expanded)
  │  └── <tr class="e-groupcaptionrow">
  │      ├── <td class="e-recordplusexpand">
  │      │   └── <span class="e-icons e-gdiagonaldown"></span>  ← down arrow
  │      └── <td class="e-groupcaption">Country: USA - 15 items</td>
  │
  ├─ DATA ROW 1 (fetched via GetUiData())
  │  └── <tr class="e-row e-lazyload-row">
  │
  ├─ DATA ROW 2
  │  └── <tr class="e-row e-lazyload-row">
  │
  └─ ... (15 rows total)

Virtualization Impact:
  - Only rows in [RowStartIndex..RowEndIndex] rendered
  - Collapsed groups = 1 row (caption only)
  - Expanded groups = 1 caption + N data rows (within window)
  - Virtual track height recalculates on each expand/collapse
```

**Lazy Loading API Flow**
```
User clicks expand icon
  ↓
GroupModule.HandleGroupExpand(captionRow)
  ↓
VirtualScroll.GetUiData(groupKey, level)
  ↓
DataManager.ExecuteQuery(groupQuery)
  ↓
GroupModelGenerator.GenerateLazyRowsobject(groupData)
  ↓
Insert rows into Parent.Rows at caption index + 1
  ↓
Update VisibleGroupRows.Count
  ↓
EventAggregator.Trigger("VirtualComponentUpdate")
  ↓
GridVirtualContent.StateHasChanged() → re-render with new rows
```

---

## Component Decision Flow (ASCII)
<!-- token-budget: 120 words -->

```
SfGrid.razor virtualization path selection:
═══════════════════════════════════════════════════════════════════════════

   HEADER SECTION (.e-gridheader > .e-headercontent)
   ┌───────────────────────────────────────────────────────────────────┐
   │  if (EnableColumnVirtualization)                                  │
   │     ↓                                                              │
   │     GridVirtualHeader                                             │
   │       └─► wraps inner GridHeader                                  │
   │            ├─ renders VirtualizedColumns slice                    │
   │            │  (FrozenLeft + Window[Start..End] + FrozenRight)     │
   │            ├─ applies TranslateX on .e-virtualtable               │
   │            └─ renders .e-virtualtrack (horizontal spacer)         │
   │  else                                                              │
   │     ↓                                                              │
   │     GridHeader (renders ALL columns — no windowing)               │
   └───────────────────────────────────────────────────────────────────┘

   CONTENT SECTION (.e-gridcontent > .e-content)
   ┌───────────────────────────────────────────────────────────────────┐
   │  if (EnableVirtualization || EnableColumnVirtualization)          │
   │     ↓                                                              │
   │     GridVirtualContent                                            │
   │       ├─► Row windowing (if EnableVirtualization)                 │
   │       │    └─ renders Rows[RowStart..RowEnd] ± OverscanCount      │
   │       │    └─ applies TranslateY on .e-virtualtable               │
   │       │    └─ renders top/bottom mask rows (if EnableVirtualMaskRow) │
   │       │                                                            │
   │       ├─► Column windowing (if EnableColumnVirtualization)        │
   │       │    └─ GetCells(StartColIdx, EndColIdx) per row            │
   │       │    └─ applies TranslateX on .e-virtualtable               │
   │       │                                                            │
   │       └─► .e-virtualtrack (drives scrollbar range)                │
   │            ├─ height = (TotalItemCount × RHeight)px               │
   │            └─ width = sum(ALL column widths)px                    │
   │                                                                    │
   │  else                                                              │
   │     ↓                                                              │
   │     GridContent (renders ALL rows, ALL columns — no virtualization)│
   └───────────────────────────────────────────────────────────────────┘

═══════════════════════════════════════════════════════════════════════════
```

---

## Cell Rendering Details (GridRow.razor)
<!-- token-budget: 120 words -->

```
GridRow component receives parameters:
  RowParameters.Row = Row<object>  (contains Cells collection)
  
How GetCells() works:
  ┌─────────────────────────────────────────────────────────────────┐
  │  GetCells(startColIdx?, endColIdx?)                             │
  │                                                                  │
  │  if (EnableColumnVirtualization && startColIdx != null)         │
  │    ↓ COLUMN WINDOWING ACTIVE                                    │
  │    └─► Filter Row.Cells based on column index range             │
  │        ├─ Include ALL frozen left cells (Freeze == Left)        │
  │        ├─ Include cells[startColIdx..endColIdx] (window)        │
  │        └─ Include ALL frozen right cells (Freeze == Right)      │
  │                                                                  │
  │  else                                                            │
  │    ↓ NO COLUMN WINDOWING                                        │
  │    └─► Return ALL Row.Cells (entire row rendered)               │
  └─────────────────────────────────────────────────────────────────┘

Each Cell<object> → rendered as <td>:

  Cell.CellType determines rendering path:
  
  ├─ CellType.Data (standard data cell)
  │   └─► RenderCell() or custom cell renderer
  │       └─► <td class="e-rowcell" role="gridcell">
  │           └─► if (Column.Template != null)
  │               └─► CellRender component
  │           └─► else
  │               └─► <span .e-virtualcell>{cell value}</span>
  │
  ├─ CellType.Indent (grouping indent cell)
  │   └─► <td class="e-indentcell"></td>
  │
  ├─ CellType.DetailIndent (detail template icon)
  │   └─► <td class="e-detailindentcell">
  │       └─► <span .e-dtdiagonaldown / .e-dtdiagonalright>
  │
  ├─ CellType.RowDrag (row reorder handle)
  │   └─► <td class="e-rowdragdrop">
  │       └─► <span .e-icons .e-rowdragicon>
  │
  ├─ CellType.Expand (group expand/collapse)
  │   └─► <td class="e-recordplusexpand / e-recordpluscollapse">
  │
  ├─ CellType.GroupCaption
  │   └─► <td colspan="{remaining cols}" class="e-groupcaption">
  │       └─► {Field}: {Key} - {Count} items
  │
  ├─ CellType.CommandColumn
  │   └─► <td class="e-rowcell">
  │       └─► <button> × N (Edit, Delete, Save, Cancel...)
  │
  └─ CellType.Summary / CaptionSummary
      └─► <td class="e-summarycell">
          └─► Aggregate value (Sum, Avg, Min, Max, Count...)

Mask cell rendering (when EnableVirtualMaskRow):
  <td class="e-masked-cell e-rowcell">
    └─► <span class="e-virtualcell"></span>  ← empty placeholder
```

**Key Points**
- `GetCells()` is called per-row during `GridRow` render
- Column windowing filters cells; frozen columns bypass the filter
- Cell templates invoke `CellRender` component (heavier render path)
- Mask cells are static HTML; no component instantiation
- Each `<td>` has `role="gridcell"` for accessibility (ARIA)

---

## Update Event Paths
<!-- token-budget: 80 words -->

```
Vertical scroll detected (JS)
  └─► VirtualScroll.HandleVerticalScrollAsync() [VirtualScroll.cs line 1871]
        └─► Updates RowStartIndex / RowEndIndex
        └─► EventAggregator.Trigger("VirtualComponentUpdate", {StartIndex, EndIndex}) [line 1921, 1958]
              └─► GridVirtualContent.VirtualComponentUpdate()
                    └─► StateHasChanged() → re-renders row window

Horizontal scroll detected (JS)
  └─► VirtualScroll.HandleHorizontalScrollAsync() [VirtualScroll.cs line 1984]
        └─► Updates StartColumnIndex / EndColumnIndex / TranslateX
        └─► EventAggregator.Trigger("VirtualHeaderComponentUpdate", args) [line 1999]
              └─► GridVirtualHeader.VirtualHeaderComponentUpdate()
                    └─► SetVirtualColumns() → updates VirtualizedColumns slice
                    └─► StateHasChanged() → re-renders column header
        └─► EventAggregator.Trigger("VirtualComponentUpdate", {Axis:"X"}) [line 2000]
              └─► GridVirtualContent.VirtualComponentUpdate()
                    └─► IsXAxis = true → re-renders cell column window
```

---

## JavaScript Interop
<!-- token-budget: 100 words -->

> **Virtualization relies on C# ↔ JavaScript interop for scroll handling, DOM transform updates, and client-side measurements.**

### C# → JavaScript Interop Calls

| C# Method Call | TypeScript Function | Source | Purpose |
|---|---|---|---|
| `InvokeMethod("sfBlazor.Grid.refreshColumnIndex", ...)` | `refreshColumnIndexes()` | `VirtualScroll.cs:220`<br>`virtual-scroll.ts:2021` | Recalculates column offset map (`_coffSets`) after column show/hide/reorder |
| `InvokeMethod("sfBlazor.Grid.refreshOnDataChange", ...)` | `refreshOnDataChange()` | `VirtualScroll.cs:233`<br>`virtual-scroll.ts:473` | Resets virtual scroll state after data source change |
| `InvokeMethod<int>("sfBlazor.Grid.clientHeight", ...)` | `clientHeight(dataId)` | `VirtualScroll.cs:1247`<br>`sf-grid.ts:302` | Returns viewport height (`max(content.clientHeight, window.innerHeight)`) |
| `InvokeMethod("sfBlazor.Grid.clientTransformUpdate", ...)` | `clientTransformUpdate(...)` | `VirtualScroll.cs:1861,1943,1971`<br>`sf-grid.ts:309` | Applies CSS `translate(X, Y)` to `.e-virtualtable` via `virtualContentModule.updateTransform()` |
| `InvokeMethod("sfBlazor.Grid.updateVirtualColumns", ...)` | `updateVirtualColumns(...)` | `VirtualScroll.cs:2001`<br>`sf-grid.ts:158` | Updates `options.virtualizedColumns` in JS instance for column windowing |
| `InvokeMethod("sfBlazor.Grid.forceUpdateTranslate", ...)` | `forceUpdateTranslate(...)` | `GridVirtualContent.razor:414`<br>`sf-grid.ts:1137` | Forces immediate `translate(0, Y)` update via `virtualEle.adjustTable()` |
| `InvokeMethod("sfBlazor.Grid.scrollIntoView", ...)` | `scrollIntoView(...)` | `VirtualScroll.cs:2024` | Scrolls viewport to specific row/column index |

### JavaScript → C# Event Callbacks

| JavaScript Event | C# Handler | Trigger | Flow |
|---|---|---|---|
| Vertical scroll detected | `HandleVerticalScrollAsync()` | User scrolls content div | JS detects `scrollTop` change → invokes C# → C# updates `RowStartIndex`/`RowEndIndex` → triggers `VirtualComponentUpdate` → Blazor re-renders row window |
| Horizontal scroll detected | `HandleHorizontalScrollAsync()` | User scrolls content div | JS detects `scrollLeft` change → invokes C# → C# updates `StartColumnIndex`/`EndColumnIndex` → triggers `VirtualHeaderComponentUpdate` + `VirtualComponentUpdate` → Blazor re-renders column slice |

### Responsibility Split

| Responsibility | Handled By | Location |
|---|---|---|
| **Scroll event detection** | JavaScript | `scripts/scroll.ts`, `scripts/virtual-scroll.ts` |
| **Row/column index calculation** | C# | `VirtualScroll.cs` (methods: `GetRowVirtualBlockIndexes()`, `GetColumnVirtualIndexes()`) |
| **CSS transform application** | JavaScript | `sf-grid.ts:309` (`clientTransformUpdate` → `virtualContentModule.updateTransform()`) |
| **Row window generation** | C# + Blazor | `GridVirtualContent.razor` (renders `GridRow` components for `[RowStart..RowEnd]`) |
| **Column slice filtering** | C# | `GridRow.razor` (`GetCells(startColIdx, endColIdx)` filters cells) |
| **Viewport height measurement** | JavaScript | `sf-grid.ts:302` (`clientHeight` returns `content.clientHeight`) |
| **Virtual track height/width** | C# | `GridVirtualContent.razor` / `GridVirtualHeader.razor` (`GetVirtualTrackStyles()`) |

### CRITICAL Rules

1. **ALWAYS call `refreshColumnIndex` after column visibility/reorder changes** — `_coffSets` must stay in sync with `VisibleColumns` to prevent incorrect `TranslateX` calculation (line 220 in `VirtualScroll.cs` → line 2021 in `virtual-scroll.ts`).

2. **NEVER bypass `clientTransformUpdate` for translate updates** — Direct DOM manipulation will desync C# state from JS virtual scroll module; always use `InvokeMethod("clientTransformUpdate", ...)` (lines 1861, 1943, 1971 in `VirtualScroll.cs` → line 309 in `sf-grid.ts`).

3. **ALWAYS await `clientHeight` before PageSize calculation** — Row virtualization depends on accurate viewport height; compute `PageSize = ⌊clientHeight ÷ RHeight⌋ × 2` only after `await InvokeMethod<int>("clientHeight", ...)` returns (line 1247 in `VirtualScroll.cs` → line 302 in `sf-grid.ts`).

4. **NEVER call `forceUpdateTranslate` during scroll event handling** — This bypasses normal scroll flow; use only for programmatic navigation (e.g., ScrollToRow, ScrollToColumn); see `GridVirtualContent.razor:414` → `sf-grid.ts:1137`.

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

| Combination | Must Preserve | Risk |
|---|---|---|
| Row Virt + Column Virt | Both `TranslateY` and `TranslateX` applied on same `.e-virtualtable`; column window recalc on horizontal scroll; row window recalc on vertical scroll; DOM cell count = `(RowWindow × ColWindow)` | Critical |
| Row Virt + Frozen Rows | Frozen rows always in `GridHeader <tbody>`; `GridVirtualContent` renders `Parent.Rows.Skip(FrozenRows)`; virtual track height excludes frozen row height | Critical |
| Column Virt + Frozen Columns | Frozen columns always fully rendered in every row; `StartColumnIndex`/`EndColumnIndex` apply only to movable columns; `VirtualizedColumns = frozenLeft + window + frozenRight` | Critical |
| Row Virt + Grouping | `GroupGeneratedData` replaces `GeneratedData`; `GroupModelGenerator` creates caption rows + data rows with indent cells; virtual track height = `VisibleGroupRows.Count × RHeight` (includes captions); `RowStartIndex`/`RowEndIndex` map to flattened group structure; expand/collapse recalculates visible count | Critical |
| Row Virt + Grouping + Lazy Load | `EnableLazyLoading = true` → groups load on-demand via `GetUiData()`; collapsed groups = 1 caption row in DOM; expanded groups fetch data rows dynamically; virtual track height updates on expand/collapse; cache invalidated per group fetch | Critical |
| Column Virt + Grouping | Indent cells and expand/collapse cells ALWAYS rendered (bypass column window); caption cell colspan = `VirtualizedColumns.Count` (adjusts dynamically); data cells subject to column windowing via `GetCells(StartColIdx, EndColIdx)` | High |
| Row Virt + Mask Rows (`EnableVirtualMaskRow`) | Top mask = PageSize rows rendered BEFORE real data when cache miss; bottom mask = `min(PageSize, TotalItemCount − RowEndIndex)` rows; mask rows contain `.e-virtualcell` spans | High |
| Row Virt + OverScan | `RowStartIndex -= OverscanCount` (clamped ≥ 0); `RowEndIndex += OverscanCount` (clamped ≤ TotalItemCount); extra rows pre-rendered to reduce scroll flash | High |
| Column Virt + Column Reorder | `RefreshColOffsets()` rebuilds `_coffSets` cumulative width map; `VirtualHeaderComponentUpdate` fires; new `StartColumnIndex` recalculated from updated offset map | High |
| Row Virt + Editing (Normal mode) | Add-Top only when `RowStartIndex == 0`; `ScrollToEditedRowAsync()` scrolls viewport to row before save; edit row injected into `Parent.Rows` at correct index; window recalculates | High |
| Row Virt + Paging | `PageSize` auto-computed = `(gridHeight ÷ RHeight) × 2`; controls both row window size and mask row count; overscan adds to effective page size | High |
| Column Virt + Show/Hide Column | `VirtualizedColumns` recalculated; `TranslateX` adjusted if hidden column before `StartColumnIndex`; `VirtualTableWidth` recomputed; `.e-virtualtrack` width updated | High |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Virtualization DOM Structure

**Context**  
You are working on the rendered DOM tree and Blazor component structure for Row and/or Column Virtualization in `SfGrid<TValue>`. This skill answers: which Razor component renders which HTML element, what CSS class/style is applied, and how updates are propagated.

**Component Selection Rules (from `SfGrid.razor`)**  
- `EnableColumnVirtualization == true` → Header section uses `GridVirtualHeader` (wraps inner `GridHeader` with `.e-virtualtable` + `.e-virtualtrack`)  
- `EnableVirtualization OR EnableColumnVirtualization` → Content section uses `GridVirtualContent` (`.e-virtualtable` + `.e-virtualtrack`)  
- Otherwise → `GridHeader` + `GridContent` (standard path)

**DOM Landmarks**  
| Element | Class | Purpose |
|---|---|---|
| Virtual table div | `.e-virtualtable` | Positioned window; receives `transform: translate(X, Y)` |
| Virtual track div | `.e-virtualtrack` | Full-size empty spacer; drives scrollbar range |
| Mask row | `.e-masked-row` + `.e-masked-cell` | Skeleton placeholder; contains `.e-virtualcell` spans |
| Real data `<table>` | `.e-table` | Contains `<WidthController>` + `<tbody>` with `GridRow` instances |

**Style Computation**  
- `TranslateY = RowStartIndex × RHeight` (set in `GridVirtualContent.OnParametersSet`)  
- `TranslateX` from `VirtualScroll._coffSets[StartColumnIndex]` (set by JS scroll handler)  
- `.e-virtualtrack` height = `TotalItemCount × RHeight` (row axis)  
- `.e-virtualtrack` width = `VirtualScroll.GetColumnsWidth(ALL columns)` (column axis)  
- `VirtualTableWidth` = `GetColumnsWidth(VirtualizedColumns)` (slice width)

**Update Triggers**  
- Vertical scroll → `VirtualComponentUpdate` event → `GridVirtualContent.StateHasChanged()`  
- Horizontal scroll → `VirtualHeaderComponentUpdate` + `VirtualComponentUpdate(Axis:"X")` → both header and content re-render  
- Feature state change (sort/filter/group/edit) → `ClearCacheData()` → `DataProcess()` → full re-render

**When to Use Related Skills**  
- **For cache management, scroll event handling, or index calculation issues** → Load `.github/skills/virtualization-skill/SKILL.md`  
  (Use when: cache not invalidating, `RowStartIndex`/`RowEndIndex` calculation wrong, `GeneratedData` cache issues, scroll handlers not firing, `_coffSets` column offset calculation, `ClearCacheData()` not triggering)
- **For DOM structure, component rendering, or CSS transform problems** → Use this skill (virtualization-dom-structure-skill)  
  (Use when: wrong component selected, missing DOM elements, `TranslateY`/`TranslateX` not applied, mask rows not rendering, cell rendering flow wrong, grouping caption/indent structure incorrect)
- **For combined issues** → Load both skills  
  (Example: "Cache invalidates correctly but DOM not updating" = engine issue + rendering issue)

**Before Making Changes**  
1. Confirm which render path is active (`EnableVirtualization`, `EnableColumnVirtualization`, or both).  
2. Check `GetStyles()` and `GetVirtualTrackStyles()` in `GridVirtualContent.razor` for affected style computation.  
3. Check `GetVirtualHeaderStyles()` and `GetVirtualTrackStyles()` in `GridVirtualHeader.razor` for header transform.  
4. Verify `VirtualizedColumns` slice in `GridVirtualHeader.SetVirtualColumns()` includes frozen columns.  
5. Validate mask row condition: `EnableVirtualMaskRow && EnableVirtualization && RowEndIndex != TotalItemCount`.  
6. For cache/engine bugs, consult `.github/skills/virtualization-skill/SKILL.md` for index calculation and cache logic.
7. Consult `/docs/training/` for cross-feature rules before any change.

---

## Quick Reference: Virtualization Mode Comparison

| Feature | Row Virt Only | Column Virt Only | Row + Column Virt | Frozen Rows + Row Virt | Row Virt + Grouping |
|---|---|---|---|---|---|
| **Header Component** | `GridHeader` | `GridVirtualHeader` | `GridVirtualHeader` | `GridHeader` | `GridHeader` |
| **Content Component** | `GridVirtualContent` | `GridVirtualContent` | `GridVirtualContent` | `GridVirtualContent` | `GridVirtualContent` |
| **Row Generator** | `RowModelGenerator` | `RowModelGenerator` | `RowModelGenerator` | `RowModelGenerator` | `GroupModelGenerator` |
| **Rows in DOM** | `RowEndIndex − RowStartIndex + 2×OverscanCount` | `TotalItemCount` (all) | `RowEndIndex − RowStartIndex + 2×OverscanCount` | `(RowEnd − RowStart)` + `FrozenRows` (pinned) | `RowWindow` (caption + data rows) |
| **Columns per Row** | `VisibleColumns.Count` (all) | `FrozenLeft + (EndCol−StartCol+1) + FrozenRight` | `FrozenLeft + (EndCol−StartCol+1) + FrozenRight` | `VisibleColumns.Count` (all) | `IndentCells + VisibleColumns.Count` |
| **TranslateY Applied?** | ✅ Yes | ❌ No | ✅ Yes | ✅ Yes (content only) | ✅ Yes |
| **TranslateX Applied?** | ❌ No | ✅ Yes | ✅ Yes | ❌ No | ❌ No (or ✅ if + ColVirt) |
| **Virtual Track Height** | `TotalItemCount × RHeight` | `TotalItemCount × RHeight` | `TotalItemCount × RHeight` | `(TotalItemCount − FrozenRows) × RHeight` | `VisibleGroupRows.Count × RHeight` |
| **Virtual Track Width** | `100%` | `Sum(ALL column widths)` | `Sum(ALL column widths)` | `100%` | `100%` |
| **Mask Rows Rendered?** | ✅ If `EnableVirtualMaskRow=true` | ❌ No | ✅ If `EnableVirtualMaskRow=true` | ✅ If `EnableVirtualMaskRow=true` | ✅ If `EnableVirtualMaskRow=true` |
| **GetCells() params** | `(null, null)` — all cells | `(StartColIdx, EndColIdx)` | `(StartColIdx, EndColIdx)` | `(null, null)` — all cells | `(null, null)` (or with indices if + ColVirt) |
| **Frozen Column Handling** | Always rendered (all) | Always rendered (separate from window) | Always rendered (separate from window) | Always rendered (all) | Always rendered (all) |
| **OverscanCount Effect** | Adds rows above/below viewport | ❌ N/A | Adds rows above/below viewport | Adds rows above/below scrollable area | Adds caption+data rows above/below |
| **Special Cells** | Standard data cells only | Standard data cells only | Standard data cells only | Standard data cells only | `Indent` + `Expand` + `GroupCaption` cells |
| **DOM Cell Count** | `(RowWindow) × (TotalCols)` | `(TotalRows) × (ColWindow)` | `(RowWindow) × (ColWindow)` | `(FrozenRows × TotalCols) + (RowWindow × TotalCols)` | `(RowWindow) × (IndentCells + TotalCols)` |

**Legend**:  
- `RowWindow` = `RowEndIndex − RowStartIndex + 2×OverscanCount`  
- `ColWindow` = `EndColumnIndex − StartColumnIndex + 1` (movable columns only)  
- `FrozenLeft` / `FrozenRight` = count of frozen columns (always included in `VirtualizedColumns`)  
- `TotalCols` = `VisibleColumns.Count`
