---
name: column-virtualization-skill
description: Expert knowledge for the Column Virtualization feature (EnableColumnVirtualization) in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to horizontal column windowing, column offset cache (cOffsets), translateX DOM transform, VirtualHeaderRenderer, Tab/Shift+Tab keyboard navigation in column-virtual mode, RTL support, frozen column width adjustment, and cross-feature interaction guarantees with frozen columns, row virtualization, sorting, filtering, grouping, editing, paging, and selection.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Provide authoritative, code-verified guidance for implementing and debugging the Column Virtualization feature in the Syncfusion Blazor DataGrid component.

---

**Agent Invocation**
Load this skill whenever a task involves:
- `EnableColumnVirtualization = true` on `SfGrid<TValue>`
- Horizontal column windowing, column offset cache, or `cOffsets` dictionary
- `VirtualHeaderRenderer`, `VirtualElementHandler`, or `VirtualHelper` classes
- `StartColumnIndex`, `EndColumnIndex`, `TranslateX`, or `VirtualizedColumns` state
- `setColVTableWidthAndTranslate()`, `columnVirtualizationKeyDownHandler()`, or `applyHorizontalNavigationFocus()`
- Tab/Shift+Tab keyboard navigation when column virtualization is active
- RTL + horizontal virtualization scroll direction inversion
- Column-virtual interactions with frozen columns, row virtualization, paging, editing, or grouping

---

## Knowledge References

The following files contain the definitive implementation. Read them before writing any code or diagnosis.

| Priority | File | Why Read It |
|----------|------|-------------|
| P0 | `docs/training/00-START-HERE.md` | Mandatory entry point — sets architecture and naming conventions |
| P0 | `docs/training/TRAINING-INDEX.md` | Full map of training docs |
| P0 | `docs/training/01-getting-started/architecture-overview.md` | 4-layer architecture; JS DOM-only rule; C# data/state ownership |
| P1 | `scripts/virtual-scroll.ts` | Complete JS column virtualization engine: `VirtualContentRenderer`, `VirtualHeaderRenderer`, `VirtualElementHandler`, `VirtualHelper` (2371 lines) |
| P1 | `src/SfGrid.razor.cs` | C# `VirtualScrollModule` state; `TranslateX` width adjustments; `Fixed` freeze filter; editing/paging guards |
| P1 | `scripts/sf-grid-fn.ts` | `virtualHeaderModule` instantiation (line 143); `enableColumnVirtualization` guards at 15+ sites; paging special path (lines 1021, 1042) |
| P2 | `src/SfGrid.Properties.cs` | `EnableColumnVirtualization` `[Parameter]` declaration and `[JsonPropertyName]` |
| P2 | `src/EventModels/Grids.cs` | `StartColumnIndex`, `EndColumnIndex`, `TranslateX` in JS→.NET event args |
| P2 | `scripts/interfaces.ts` | `GridOptions.enableColumnVirtualization` TypeScript interface |
| P3 | `scripts/width-controller.ts` | `setWidthToTable()` — freeze+column-virt guard skips table width update |
| P3 | `scripts/scroll.ts` | Horizontal scroll event routing to column virtual module |

---

## Training Insights Applied

The following are non-obvious, code-verified rules that agents MUST follow when working on column virtualization. Violating any of these causes runtime regressions.

### 1 — X-Axis Observer Only When Column Virtualization Is Active
`VirtualContentRenderer.renderTable()` configures the `InterSectionObserver` axes:
```typescript
axes: enableColumnVirtualization ? ['X', 'Y'] : ['Y']
```
When `enableColumnVirtualization = false`, only vertical scroll is observed. Enabling both axes without the feature flag causes spurious horizontal re-renders. Never hard-code `['X','Y']` unconditionally.

### 2 — `cOffsets` Is the Single Source of Truth for Column Positions
`VirtualHelper.cOffsets` is a dictionary `{ [columnIndex: number]: cumulativeWidth }` built by `refreshColOffsets()`. Every column-offset lookup (`getColumnOffset(idx)`) reads from this cache. It must be rebuilt after every column show/hide, resize, reorder, or group-indent change. **Never compute column offsets inline** — always call `vHelper.refreshColOffsets()` first.

### 3 — `hasColumnIndexChanged` Guards the C# Re-Render
`setColVTableWidthAndTranslate()` compares the previous and current column index window using:
```typescript
const hasColumnIndexChanged = JSON.stringify(prevIndexes) !== JSON.stringify(currentIndexes);
```
C# is only notified (via `SetPageSizeAndCIndex`) when the column window actually changes. On every horizontal scroll tick the JS layer runs silently unless the window shifts. Do not call `SetPageSizeAndCIndex` unconditionally — this causes excessive Blazor re-renders.

### 4 — `SetPageSizeAndCIndex` Carries Four Required Fields
The JS→.NET interop call that notifies C# of a column window change carries exactly four properties:
```typescript
{ pageSize, startColumnIndex, endColumnIndex, VTableWidth }
```
All four must be consumed by the C# receiver (`VirtualScrollModule`). Missing `VTableWidth` breaks virtual table width calculation. Missing `startColumnIndex`/`endColumnIndex` breaks frozen column filtering and TranslateX adjustment.

### 5 — `isScrollByFocus` Prevents Duplicate Scroll Events During Keyboard Navigation
`applyHorizontalNavigationFocus()` sets `isScrollByFocus = true` before programmatically scrolling to bring a focused cell into view. The scroll handler checks this flag and skips re-triggering column-index recomputation. After the scroll handler runs, the flag is cleared. Never remove this guard — it prevents an infinite focus↔scroll loop.

### 6 — RTL Requires Negated Translate Values
`VirtualHelper.getColumnIndexes()` negates `scrollLeft` for RTL:
```typescript
const scrollLeft = enableRtl ? -1 * container.scrollLeft : container.scrollLeft;
```
Similarly, `horizontalTranslateValue` is negated before being passed to `adjustTable()`. Never apply a positive translateX value directly in RTL mode. All column offset arithmetic must flow through `getColumnIndexes()` for RTL correctness.

### 7 — Column-Virt + Frozen: Width Adjustment Via `TranslateX`
When both `EnableColumnVirtualization` and frozen columns are active, the C# render path adjusts left/right widths using `VirtualScrollModule.TranslateX`:
```csharp
// Freeze.cs lines 629, 653, 725-726, 762
if (_parent.EnableColumnVirtualization && _parent.VirtualScrollModule != null && _parent.VirtualScrollModule.TranslateX != 0)
{
    LeftRightWidth = LeftRightWidth - _parent.VirtualScrollModule.TranslateX; // line 629 (left frozen)
    // OR
    LeftRightWidth = LeftRightWidth + _parent.VirtualScrollModule.TranslateX; // line 653 (right frozen)
    // OR
    LeftWidth = LeftWidth - _parent.VirtualScrollModule.TranslateX;  // lines 725, 762
    RightWidth = RightWidth + _parent.VirtualScrollModule.TranslateX; // line 726
}
```
Always guard frozen width calculations with a `TranslateX != 0` check. A zero `TranslateX` means no column offset is active and no adjustment should be made.

### 8 — `Fixed` Freeze Columns Are Filtered by Column Window
When `EnableColumnVirtualization = true`, `Fixed`-freeze columns are filtered to only those within the current viewport window:
```csharp
// Freeze.cs line 662 in GetFrozenColumnsStyle()
var frozenColumnsFixed = _parent.EnableColumnVirtualization
    ? _parent.Columns.Where(x => x.Freeze == FreezeDirection.Fixed && x.IsFrozen
        && x.Index >= _parent.VirtualScrollModule?.StartColumnIndex
        && x.Index <= _parent.VirtualScrollModule.EndColumnIndex).ToList()
    : _parent.Columns.Where(x => x.Freeze == FreezeDirection.Fixed && x.IsFrozen).ToList();
```
Never use the full `Columns` list for `Fixed` freeze rendering when column virtualization is active.

### 9 — Use `VirtualizedColumns`, Not `Columns`, for Boundary Checks
`VirtualScrollModule.VirtualizedColumns` holds the currently rendered column slice. For frozen right-column boundary detection in column-virtual mode, use:
```csharp
// Freeze.cs line 451 in GetFrozenColumnsStyle() - isFirstRightFrozenColumn check
bool isFirstRightFrozenColumn = (!_parent.EnableColumnVirtualization && FrozenRightColumns?.Count > 0 
    && (Column.Index == FrozenRightColumns[0].Index)) 
    || (_parent.EnableColumnVirtualization 
        && Column.Index == _parent.VirtualScrollModule?.VirtualizedColumns
            .Where(x => x.IsFrozen && x.Freeze.Equals(FreezeDirection.Right))
            .FirstOrDefault()?.Index);
```
Using the full `Columns` list instead returns the absolute first right-frozen column, which may not be in the current viewport window.

### 10 — `frozenMidScroll` Tracks Mid-Scroll Frozen Boundary Crossing for Tab Navigation
`columnVirtualizationKeyDownHandler()` sets `frozenMidScroll = true` when Tab/Shift+Tab navigation crosses a frozen boundary. This flag prevents the next scroll event from triggering a spurious column-index recalculation mid-navigation. Always clear `frozenMidScroll` once navigation settles.

### 11 — `ensurePageSize()` Must Run After `cOffsets` Are Fresh
`ensurePageSize()` reads `cOffsets` to compute `startColIndex`, `endColIndex`, and `virtualTableWidth`. If called before `refreshColOffsets()`, it uses stale offset data. Always call `vHelper.refreshColOffsets()` before `ensurePageSize()` on any operation that changes column widths or visibility.

### 12 — Column-Virt Only Mode: Y-Translate Is Zero
When `EnableColumnVirtualization = true` but `EnableVirtualization = false` (no row virtualization), `adjustTable()` must set `yValue = 0`. The Y-axis table transform is managed exclusively by row virtualization. Setting a non-zero Y translate in column-virt-only mode offsets the table vertically and corrupts row alignment.

---

## Code Location Map

| File / Class | Responsibility |
|---|---|
| `scripts/virtual-scroll.ts → VirtualContentRenderer` | Primary column virtualization engine. Owns `startColIndex`, `endColIndex`, `movableTranslateX`, `isScrollByFocus`, `frozenMidScroll`, `selectedCellNavigation`, `selectedRowNavigation`, `vHelper` (VirtualHelper reference), `header` (VirtualHeaderRenderer reference). Key methods: `setColVTableWidthAndTranslate()`, `columnVirtualizationKeyDownHandler()`, `ensurePageSize()`, `refreshOnDataChange()`, `applyHorizontalNavigationFocus()`, `getInfoFromView()` |
| `scripts/virtual-scroll.ts → VirtualHeaderRenderer` | Column header virtual rendering. `renderTable()` calls `vHelper.refreshColOffsets()` and `setColumnIndexesInView()`. Owns `virtualEle` (VirtualElementHandler for the header table). |
| `scripts/virtual-scroll.ts → VirtualElementHandler` | DOM transform wrapper. `adjustTable(x, y, direction)` applies `transform: translate(Xpx, Ypx)` to the virtual table. Recalculates frozen cell `left`/`right` CSS via `FrozenLineOffsetPosition` and `translateLeftRightValue` loop. `setWrapperWidth(tableWidth)` sets the wrapper element width. |
| `scripts/virtual-scroll.ts → VirtualHelper` | Column offset cache. `cOffsets: { [idx: number]: number }` — cumulative column widths. `mOffsets` — frozen movable column offsets. `refreshColOffsets()` builds both from visible column widths (frozen path for `frozenColumns > 0`). `getColumnIndexes()` — RTL-aware algorithm: negates `scrollLeft` for RTL, uses `cOffsets` + viewport width (`cWidth`) to compute `startColIndex`/`endColIndex` and assign them back to `virtualContentModule`. |
| `scripts/sf-grid-fn.ts` | `virtualHeaderModule = new VirtualHeaderRenderer(this)` (line 143); `virtualContentModule` instantiation. `enableColumnVirtualization` guards at lines 159, 162, 181, 206, 293, 330, 843, 859, 1021, 1042, 1780, 1790, 2009 (15+ sites). Paging special path (lines 1021/1042): `enableColumnVirtualization && allowPaging` triggers horizontal scroll reset. `updateColumnWidth()` builds `nColumnOffsets` used to refresh `cOffsets` after resize. |
| `scripts/width-controller.ts → setWidthToTable()` | Freeze+column-virt guard: skips table width update when `frozenColumns && enableColumnVirtualization` is true, unless autofit columns exist or the resize module triggered the call. |
| `scripts/interfaces.ts` | `GridOptions.enableColumnVirtualization: boolean` TypeScript interface declaration (line 32). |
| `src/SfGrid.Properties.cs` | `[Parameter] public bool EnableColumnVirtualization { get; set; }` with `[JsonPropertyName("enableColumnVirtualization")]` attribute. |
| `src/Internal/Actions/VirtualScroll.cs → VirtualScroll<T>` | C# state: `StartColumnIndex` (int, line 64), `EndColumnIndex` (int, line 66), `TranslateX` (int, line 68), `VirtualizedColumns` (List\<GridColumn\>, line 46), `GetVirtualizedColumns()` method (line 310 in GetVirtualizedColumns() method). Interop serialization (line 1809 in SfGrid.razor.cs): `virtualizedColumns = VirtualScrollModule?.GetVirtualizedColumns()`. |
| `src/Internal/Actions/Freeze.cs` | Frozen width adjustment: `LeftRightWidth` adjusted by `- TranslateX` (line 629), `LeftRightWidth` adjusted by `+ TranslateX` (line 653), `LeftWidth` adjusted by `- TranslateX` (lines 725, 762), `RightWidth` adjusted by `+ TranslateX` (line 726). Fixed freeze filter (line 662): filters by `StartColumnIndex ≤ index ≤ EndColumnIndex`. Right freeze boundary check (line 451 in isFirstRightFrozenColumn): uses `VirtualizedColumns.Where` not full `Columns`. |
| `src/SfGrid.DataProcessing.cs` | Editing guard (line 66): `EnableColumnVirtualization && FilterType.FilterBar && IsAdd` triggers special Save+Add path. |
| `src/SfGrid.razor.cs` | JS options (line 1781): `enableColumnVirtualization = EnableColumnVirtualization`. |
| `src/EventModels/Grids.cs` | `StartColumnIndex` (int, line 224), `EndColumnIndex` (int, line 187), `TranslateX` (double, line 242) — properties in JS→.NET event arg objects received from `SetPageSizeAndCIndex` interop call. |

---

## JavaScript Interop
<!-- token-budget: 150 words -->

Column virtualization requires bidirectional JavaScript-C# communication to synchronize column window state and trigger Blazor re-renders when the horizontal viewport changes.

### **C# → JavaScript Calls**

| C# Method | JS Function | Purpose | Location |
|-----------|-------------|---------|----------|
| `InvokeMethod("sfBlazor.Grid.updateVirtualColumns", ...)` | `updateVirtualColumns()` | Updates TypeScript column metadata after C# re-render | VirtualScroll.cs line 2001 |
| `InvokeMethod("sfBlazor.Grid.clientTransformUpdate", ...)` | `clientTransformUpdate()` | Applies translateX/Y transform to virtual table wrapper | VirtualScroll.cs lines 1943, 1972 |
| `InvokeMethod("sfBlazor.Grid.virtualHeight", ...)` | `virtualHeight()` | Recalculates virtual scroll container height | SfGrid.razor.cs lines 835, 869 |

### **JavaScript → C# Calls**

| JS Method | C# Handler | Purpose | Location |
|-----------|------------|---------|----------|
| `dotNetRef.invokeMethodAsync('SetPageSizeAndCIndex', pageConfiguration)` | `SetPageSizeAndCIndex(object args)` | Notifies C# of column window change (startColumnIndex, endColumnIndex, VTableWidth, pageSize) | virtual-scroll.ts line 203 → GridJSInteropAdaptor.cs line 107 |
| `dotNetRef.invokeMethodAsync('VirtualRefresh', ...)` | `VirtualRefresh(...)` | Triggers row-level virtual refresh (row virtualization) | virtual-scroll.ts lines 167, 336 |

### **TypeScript Column Virtualization Functions**

| Function | Responsibility | Location |
|----------|----------------|----------|
| `setColVTableWidthAndTranslate()` | Computes column window (startColIndex/endColIndex), virtualTableWidth, and translateX; notifies C# if window changed | virtual-scroll.ts (VirtualContentRenderer) |
| `ensurePageSize()` | Reads `cOffsets` to calculate optimal column indexes and virtual table width | virtual-scroll.ts (VirtualContentRenderer) |
| `getColumnIndexes()` | RTL-aware algorithm: computes startColIndex/endColIndex from scrollLeft + cOffsets + viewport width | virtual-scroll.ts (VirtualHelper) |
| `refreshColOffsets()` | Rebuilds `cOffsets` dictionary (cumulative column widths) from visible column metadata | virtual-scroll.ts (VirtualHelper) |

### **CRITICAL Rules**

1. **C# must not call JS until after `OnAfterRenderAsync`** — DOM must exist before JS can manipulate transforms or offsets.
2. **JS must call `SetPageSizeAndCIndex` only when column window actually changes** — `hasColumnIndexChanged` guard prevents excessive Blazor re-renders.
3. **`pageConfiguration` payload must include all 4 fields** — `pageSize`, `startColumnIndex`, `endColumnIndex`, `VTableWidth` (line 198-203 in virtual-scroll.ts).
4. **`cOffsets` must be refreshed before `ensurePageSize()` runs** — stale offsets corrupt column window calculations.

---

## Interaction Matrix (MANDATORY)

Every combination below has been verified against live source code. Each row identifies the exact invariant that must be preserved when implementing or fixing column virtualization alongside the paired feature.

| Feature Combination | Must Preserve | Risk Level |
|---|---|---|
| **ColVirt + Frozen Columns** | `LeftRightWidth`/`LeftWidth`/`RightWidth` adjusted by `± TranslateX` (Freeze.cs lines 629, 653, 725-726, 762). `Fixed`-freeze cols filtered by `StartColumnIndex ≤ index ≤ EndColumnIndex` (Freeze.cs line 662). Right-freeze boundary uses `VirtualizedColumns` not `Columns` (Freeze.cs line 451). `setFrozenHeight()` uses virtual col width sum. | **Critical** |
| **ColVirt + Row Virtualization** | Both scroll axes active (`axes: ['X','Y']`). `adjustTable(x, existingY)` preserves non-zero `yValue` from row virtualization. When `EnableColumnVirtualization = true` but `EnableVirtualization = false`, `adjustTable` must set `yValue = 0`. Never mix Y-translate management between features. | **Critical** |
| **ColVirt + Scroll Management** | Horizontal scroll triggers X-axis `InterSectionObserver` → `getColumnIndexes()` → `setColVTableWidthAndTranslate()`. `cOffsets` must be current before the scroll handler reads them. `isScrollByFocus` flag prevents focus-triggered scroll from cascading. | **Critical** |
| **ColVirt + RTL** | `getColumnIndexes()` negates `scrollLeft`: `-1 * container.scrollLeft`. `horizontalTranslateValue` is negated before `adjustTable()`. `columnTranslateX` is negated. Never apply translateX as a positive value in RTL. All column offset arithmetic must route through `getColumnIndexes()`. | **High** |
| **ColVirt + Column Resize** | `setWidthToTable()` guard: skips when `frozenColumns && enableColumnVirtualization` unless autofit or resize module triggered. After resize: `updateColumnWidth()` rebuilds `nColumnOffsets`; `refreshColOffsets()` must run before the next `ensurePageSize()` call. | **High** |
| **ColVirt + Column Reorder** | After column reorder, `refreshColOffsets()` must run to rebuild `cOffsets` with new column order. `startColIndex`/`endColIndex` must be recalculated against the new column sequence. Never cache pre-reorder column indexes. | **High** |
| **ColVirt + Sorting** | Column window (`startColIndex`/`endColIndex`) is preserved across sort because column positions do not change. `cOffsets` remains valid. `sf-grid-fn.ts` line 3139 forces a re-render on sorting when `EnableColumnVirtualization = true` (overrides the sort-only render optimization). | **High** |
| **ColVirt + Filtering** | Column window preserved across filter operations. `TotalItemCount` change does not affect column offsets. FilterBar Add-row guard: `EnableColumnVirtualization && FilterType.FilterBar && IsAdd` triggers special Save+Add path (SfGrid.DataProcessing.cs line 66). | **High** |
| **ColVirt + Grouping** | Group indent columns affect `cOffsets` offset calculation. `groupCount` is used in `refreshColOffsets()` to offset the starting index of non-group columns. Must call `refreshColOffsets()` after group/ungroup to account for indent column width changes. | **High** |
| **ColVirt + Paging** | Special scroll reset path in `sf-grid-fn.ts` lines 1021/1042: `enableColumnVirtualization && allowPaging` resets horizontal scroll position on page change. `startColIndex`/`endColIndex` reset to initial viewport window. Paging does not invalidate `cOffsets` (column positions unchanged). | **Medium** |
| **ColVirt + Selection** | `selectedCellNavigation` and `selectedRowNavigation` counters in `VirtualContentRenderer` track keyboard navigation state. Both must survive a column window change (C# re-render). Tab/Shift+Tab delegates to `columnVirtualizationKeyDownHandler()` which uses `frozenMidScroll` to guard mid-navigation scroll events. | **Medium** |
| **ColVirt + Editing (Add Row + FilterBar)** | When `EnableColumnVirtualization && FilterType.FilterBar && IsAdd && requestType == Save`, a special guard path is taken (SfGrid.DataProcessing.cs line 66). This prevents the column filter bar from overwriting the new-row editor input on save. Never remove this guard without verifying Add+FilterBar round-trip. | **High** |
| **ColVirt + Export** | Export must fetch all columns — not just `VirtualizedColumns`. Use `GetVirtualizedColumns()` only for rendering; for export, iterate the full `Columns` list regardless of `StartColumnIndex`/`EndColumnIndex`. | **High** |
| **ColVirt + Infinite Scroll** | Column virtualization can coexist with infinite scroll (but not with row virtualization simultaneously). On each new block load, `cOffsets` must be refreshed if column widths are recalculated. Horizontal scroll state is preserved across block loads. | **Medium** |
| **ColVirt + AutoFit** | `autofitFrozenColumns(autoFitVirtual: true)` calls `getOrderedFrozenColumns()` which is aware of column virtualization. After autofit, `refreshColOffsets()` must run to incorporate new column widths into `cOffsets`. `setWidthToTable()` guard is bypassed when autofit is the trigger. | **Medium** |
| **ColVirt + Stacked Headers** | Stacked headers span multiple columns. Virtual column window must ensure all spanned child columns are rendered or the span cell collapses. `VirtualizedColumns` slice must include all children of any partially-visible stacked column. | **Medium** |

---

## Prompt Template

Use this prompt template when invoking an agent for a column virtualization task:

```
Feature: Column Virtualization (EnableColumnVirtualization)
Task: {describe the bug or implementation goal}

Context:
- EnableColumnVirtualization: {true/false}
- EnableVirtualization (row): {true/false}
- FrozenColumns / FrozenRows: {count or 0}
- AllowPaging: {true/false}
- EnableRtl: {true/false}
- Affected operation: {scroll | keyboard-nav | resize | reorder | sort | filter | group | page-change | edit | export}
- Symptom: {what goes wrong — translateX offset wrong, column window not updating, keyboard nav stuck, frozen width misaligned, etc.}

Relevant code locations to examine first:
1. scripts/virtual-scroll.ts → VirtualContentRenderer.setColVTableWidthAndTranslate()
2. scripts/virtual-scroll.ts → VirtualHelper.refreshColOffsets() and getColumnIndexes()
3. src/Internal/Actions/Freeze.cs → Frozen width adjustment (lines 629, 653, 725-726, 762)
4. src/Internal/Actions/VirtualScroll.cs → VirtualScroll<T> state management (lines 64-68, 310)
5. scripts/sf-grid-fn.ts → enableColumnVirtualization guards near the affected operation

Constraints:
- JS layer must not own column data — only DOM transforms and offset cache
- C# must be notified via SetPageSizeAndCIndex only when column window actually changes
- RTL: always negate scrollLeft and translateX values through getColumnIndexes()
- cOffsets must be rebuilt (refreshColOffsets) before any ensurePageSize() call
- VirtualizedColumns (not Columns) for frozen boundary checks when EnableColumnVirtualization = true
- See JavaScript Interop section for complete JS↔C# call flow
```

---

## DOM Rendering Architecture

Column virtualization uses a parallel component tree that activates whenever `EnableColumnVirtualization` is `true`. The hierarchy below shows the exact component nesting, lifecycle hooks, data-flow decisions, and DOM output order — verified against `GridVirtualHeader.razor`, `GridVirtualContent.razor`, and `GridRow.razor`.

```
SfGrid<TValue>  [SfGrid.razor — Root]
│
├── WidthController.razor              [Column width CSS enforcement]
├── Preloader.razor                    [Loading spinner overlay]
├── GridToolbar.razor                  [Toolbar]
├── GroupDropArea.razor                [Drag-to-group drop zone]
│
├── ── HEADER ZONE ──────────────────────────────────────────────
│
│   [EnableColumnVirtualization = true]
│   ├── GridVirtualHeader<TValue>      [Virtual header wrapper]
│   │   │  OnInitialized()
│   │   │    └── EventAggregator.Add("VirtualHeaderComponentUpdate")
│   │   │  OnParametersSet()
│   │   │    ├── RearrangeColumns()
│   │   │    ├── SetFrozenAndMovableColumns()   [if IsFreezeLineMoved]
│   │   │    └── SetVirtualColumns()
│   │   │          ├── Skip(FrozenColumns) / filter Visible
│   │   │          ├── frozenLeftColumns  ──┐
│   │   │          ├── virtualColumn        ├─► VirtualizedColumns
│   │   │          └── frozenRightColumns ──┘     = frozen + window + frozen-right
│   │   │
│   │   └── <div class="e-virtualtable"
│   │           style="transform:translate({TranslateX}px,0px);
│   │                  width:{VirtualTableWidth}px">
│   │       │
│   │       └── GridHeader<TValue>             [Renders VirtualizedColumns only]
│   │               └── GridHeaderCell.razor   [Per-column <th>]
│   │
│   [EnableColumnVirtualization = false]
│   └── GridHeader<TValue>             [Standard full-column header]
│           └── GridHeaderCell.razor
│
├── ── CONTENT ZONE ─────────────────────────────────────────────
│
│   [EnableVirtualization OR EnableColumnVirtualization = true]
│   ├── GridVirtualContent<TValue>     [Virtual content wrapper]
│   │   │  OnInitialized()
│   │   │    ├── Parent.Content = this
│   │   │    └── EventAggregator.Add("VirtualComponentUpdate")
│   │   │
│   │   │  VirtualComponentUpdate(args)
│   │   │    ├── args has "StartIndex"  → IsXAxis = false  [Y-axis scroll]
│   │   │    │     RowStartIndex / RowEndIndex updated
│   │   │    └── args has "Axis"        → IsXAxis = true   [X-axis scroll]
│   │   │          (rows NOT regenerated — only StateHasChanged)
│   │   │
│   │   │  OnParametersSet()  — Row generation routing
│   │   │   │
│   │   │   ├── [Grouping + EnableVirtualization]
│   │   │   │     GroupModelGenerator.GroupGeneratorRows()
│   │   │   │     └── VirtualScrollModule.SetGroupIndexOnRows()
│   │   │   │
│   │   │   ├── [Grouping, no row virtualization]
│   │   │   │     GroupModelGenerator.GenerateRows()
│   │   │   │
│   │   │   ├── [EnableVirtualization, no grouping]
│   │   │   │     RowModelGenerator.GeneratorRows(QueriedData)
│   │   │   │     VirtualScrollModule.AddGeneratedRows()
│   │   │   │     └── VirtualScrollModule.SetRows(Start, End)
│   │   │   │
│   │   │   ├── [EnableInfiniteScrolling]
│   │   │   │     Rows = Parent.Rows  (no regeneration)
│   │   │   │
│   │   │   └── [EnableColumnVirtualization only]
│   │   │         RowModelGenerator.GeneratorRows(Data)  [full dataset]
│   │   │
│   │   └── <div class="e-virtualtable"
│   │           style="transform:translate({TranslateX}px,{TranslateY}px);
│   │                  width:{VTableWidth}px; position:absolute">
│   │       │
│   │       └── <table class="e-table">
│   │           │
│   │           ├── WidthController              [Virtual column widths]
│   │           │
│   │           └── <tbody>
│   │               │
│   │               ├── [Mask rows — top]        [e-masked-row placeholders]
│   │               │
│   │               ├── @foreach Row in Rows
│   │               │     │
│   │               │     ├── [Row.IsDetailRow]
│   │               │     │     └── GridDetailRow<TRow>
│   │               │     │
│   │               │     └── [Row.Visible]
│   │               │           └── GridRow<TRow>          ← see below
│   │               │
│   │               └── [Mask rows — bottom]     [e-masked-row placeholders]
│   │
│   [no virtualization]
│   └── GridContent<TValue>            [Standard content]
│           └── <tbody> → GridRow<TRow>
│
└── ── ROW CELL ASSEMBLY ────────────────────────────────────────

    GridRow<TRow>  [GridRow.razor]
    │
    └── <tr role="row">
        │
        └── @foreach cell in GetCells(StartColumnIndex, EndColumnIndex)
              │
              │  GetCells()  [GridRow.razor.cs]
              │  │
              │  ├── [EnableColumnVirtualization = false]
              │  │     AllowFreezeLineMoving?
              │  │       yes → FrozenLeft + Movable + FrozenRight
              │  │       no  → Cells (full list)
              │  │
              │  └── [EnableColumnVirtualization = true]
              │        ├── Edit mode?  → edit cells (no windowing)
              │        ├── IsFrozen + IsFrozenRight → FrozenRightCells
              │        ├── IsFrozen (left)          → FrozenLeftCells
              │        └── Movable panel (primary path)
              │              indentCells = Cells where
              │                CellType ∈ {RowDrag, Indent,
              │                            DetailIndent, Detail}
              │              movableCells = Cells.Except(indentCells)
              │              window = movableCells
              │                         .Skip(StartColumnIndex)
              │                         .Take(window size)
              │              ──────────────────────────────────────
              │              DOM ORDER (BLAZ-1017334 fix):
              │              [indentCells]           ← FIRST
              │              + [FrozenLeftCells]     ← SECOND
              │              + [window cells]
              │              + [FrozenRightCells]
              │              ──────────────────────────────────────
              │
              └── Per-cell render dispatch
                    │
                    ├── CellType.Expand          → RenderExpandCell
                    ├── CellType.GroupCaption    → RenderGroupCaptionCell
                    │     └── GroupCaptionRenderer.GetStyle()
                    │           Accumulate +30px per Indent/Expand cell
                    │           break at first GroupCaption cell ← fix
                    │           → style="left:{N×30}px"
                    ├── CellType.Indent /
                    │   CellType.DetailIndent    → RenderIndentCell
                    ├── CellType.RowDrag         → RenderRowDragCell
                    ├── CellType.Detail          → RenderDetailCell
                    ├── CellType.CaptionSummary  → RenderCaptionSummaryCell
                    ├── CellType.Summary         → RenderSummaryCell
                    ├── CellType.CommandColumn   → RenderCommandColumnCell
                    └── CellType.Data            → RenderCell  <td>
```
