````skill
---
name: infinite-scroll-dom-structure-skill
description: Expert knowledge for the rendered DOM structure and Blazor component rendering flow for Infinite Scrolling, including Normal Infinite Scroll, Grouped Infinite Scroll, Lazy Load Grouped Infinite Scroll, Cache mode, and post-DOM-refresh scroll restoration. Use this skill when implementing or debugging infinite scroll rendering flow, row generation, mask table, group expand/collapse, and lazy child load sequences.
---

# Infinite Scrolling DOM Structure Skill
<!-- token-budget: 20 words -->

**Purpose**
Authoritative reference for the Blazor component render tree and DOM layout produced by Infinite Scrolling. Covers `GridContent`, `InfiniteScroll<T>`, `RowModelGenerator`, `GroupModelGenerator`, mask table, and all combination flows (plain grid, grouping, lazy-load grouping). Guarantees correct DOM output without breaking any other feature.

**Companion Skill**
Also load `.github/skills/infinite-scroll-skill/SKILL.md` for feature rules, state reset behavior, query construction, cache dictionary semantics, and cross-feature action handling. DOM-structure analysis is incomplete if only one of the two skills is used.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `.github/skills/infinite-scroll-skill/SKILL.md` — Companion skill for feature rules, state-reset paths, `ResetInfiniteProperties`, `IntialInfinitePageQuery`, and cross-feature integration
- `src/Internal/Actions/InfiniteScroll.cs` — Core infinite scroll engine: line 422 `LoadLazyLoadChildData()`, page query, scroll direction detection, cache management, lazy load child data, grouped row slicing
- `src/Internal/Renderer/GridContent.razor` — Content renderer: line 519 `HandleGroupedInfiniteRows()`, mask table gate, row list assignment, `InvokeStateChange` listener
- `src/Internal/Generators/RowModelGenerator.cs` — Line 42 `GenerateInfiniteRows()` for flat infinite rows; index continuation from last row
- `src/Internal/Generators/GroupModelGenerator.cs` — Line 121 `GenerateInfiniteGroupedRows()` for grouped infinite rows; `GenerateCaptionRow()`, `GenerateDataRows()`
- `src/Internal/Actions/Group.cs` — Line 403 `LazyExpandCollapse()`, `LazyExpand()`, `LazyCollapse()`, `LazyRows` management
- `src/Internal/Base/GridJSInteropAdaptor.cs` — Line 619 `LoadInfiniteData()` JS-invokable entry point; line 631 `updateClonedMaskTranslates` JS call; `isLazyLoadChild` gate
- `src/SfGrid.razor.cs` — Line 1650 `InvokeSuccessAsync()` → line 1686 `sfBlazor.Grid.contentReady` JS call after DOM update
- `src/SfGrid.Methods.cs` — `CollapseAllGroupAsync()`, `ExpandAllGroupAsync()` with infinite-scroll reset paths
- `src/GridInfiniteScrollSettings.razor.cs` — `InitialBlocks`, `MaximumBlocks`, `EnableCache` parameter definitions
- `src/Internal/SfGrid.razor` — Root template; decides `GridContent` (not `GridVirtualContent`) for infinite scroll
- `scripts/sf-grid.ts` — Line 25 `contentReady()`, line 1021 `updateClonedMaskTranslates()`, line 1108 `resetExpandCollapseAllScroll()` TypeScript functions

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Infinite scroll always uses `GridContent`, never `GridVirtualContent`** — `SfGrid.razor` routes to `GridContent` for `EnableInfiniteScrolling`; the virtual track div is absent
- **`RenderMaskTable` is exclusive to the lazy load child path** — the `<table .e-masked-table>` is shown ONLY when `isLazyLoadChild=true` in `LoadInfiniteData`; normal infinite scroll (down/up page fetch) and normal grouped infinite scroll do NOT show the mask table; `ContentStateChanged(true)` is what sets `RenderMaskTable = true` and it is only called from the `isLazyLoadChild` branch of `GridJSInteropAdaptor`
- **Normal mode (`EnableCache=false`) keeps ALL rows in DOM** — each down scroll appends `PageSize` new `<tr>` elements; up scroll prepends them; the DOM `<tbody>` grows continuously; rows are never removed; initial load renders `InitialBlocks × PageSize` rows
- **Cache mode (`EnableCache=true`) caps the DOM at `MaximumBlocks × PageSize` rows** — on each scroll append/prepend, `RefreshInfiniteCacheRows` evicts the opposite end: down scroll calls `RemoveTopRows`, up scroll calls `RemoveBottomRows`; the DOM always contains at most `MaximumBlocks × PageSize` `<tr>` elements; initial load still renders `InitialBlocks × PageSize` rows (clamped to `MaximumBlocks` if `InitialBlocks > MaximumBlocks`)
- **Row index is *continued*, not reset** — `GenerateInfiniteRows` starts `rowIndex` from `lastRow.Index + 1` (down scroll) or `firstRow.Index − PageSize` (up scroll); indexes are absolute, not relative
- **`InfiniteGeneratedRows` dictionary caches rows by absolute index** — prevents duplicate row generation; `CacheGeneratedRows()` deduplicates on each page append
- **Grouped infinite scroll uses a two-phase model**: (1) `GenerateInfiniteGroupedRows` builds the *complete* flat row list into `GeneratedInfiniteGroupedRows`; (2) `SetCurrentGroupedRows` slices the viewport window from that list; no mask table shown during group scroll
- **Lazy load grouping bypasses phase-1 entirely** — child rows are inserted directly by `LoadLazyLoadChildData` via `InsertRange`; `ContentStateChanged(true)` fires first to show mask, then `ContentStateChanged(null)` triggers re-render after rows are inserted; this is the ONLY scroll code path that shows the mask table
- **Cache eviction is symmetric** — down scroll removes top rows (`RemoveTopRows`); up scroll removes bottom rows (`RemoveBottomRows`); eviction quantity is `min(GetMinimumValueCount(), Count − MaximumBlocks × PageSize)`
- **`InvokeSuccessAsync` → `sfBlazor.Grid.contentReady` fires after every DOM refresh** — JS restores scroll position, recalculates row heights, and re-attaches intersection observers for the next load trigger

---

## DOM Structure Map
<!-- token-budget: 700 words -->

> **Focus**: Infinite Scrolling feature only — Plain, Grouped, Lazy-Load Grouped, and Cache mode.

---

### 1. Plain Infinite Scroll (`EnableInfiniteScrolling = true`, no grouping)

> No mask table in this mode — `RenderMaskTable` is never set `true` here.

```
SfGrid<TValue>  (#ID  .e-grid)
├── .e-gridheader
│   └── .e-headercontent
│       └── GridHeader                          [always GridHeader — no virtual header]
│           └── <table .e-table #ID_header_table>
│               └── <thead>
│                   └── <tr .e-columnheader>
│                       └── GridHeaderCell × VisibleColumns.Count
│
└── .e-gridcontent
    └── .e-content
        └── GridContent<TValue>                 [always GridContent — no GridVirtualContent]
            └── <table .e-table #ID_content_table>
                ├── <WidthController>
                └── <tbody role="rowgroup">
                    └── GridRow × Rows.Count    [ALL accumulated rows — ever-growing in normal mode]
                        └── <tr role="row" class="e-row e-altrow?">
                            └── <td .e-rowcell> × VisibleColumns.Count
```

**Normal mode vs Cache mode — DOM row count over time**
```
Normal mode (EnableCache = false):
  Initial render:  InitialBlocks × PageSize  <tr> elements in DOM
  After scroll 1:  (InitialBlocks × PageSize) + PageSize  <tr> elements
  After scroll N:  (InitialBlocks × PageSize) + (N × PageSize)  <tr> elements
  → DOM grows indefinitely; no rows ever removed

Cache mode (EnableCache = true):
  Initial render:  InitialBlocks × PageSize  <tr> elements  (≤ MaximumBlocks × PageSize)
  After scroll 1:  still MaximumBlocks × PageSize  <tr> elements
                   ← PageSize new rows appended at bottom
                   ← PageSize old rows removed from top  (RemoveTopRows)
  After scroll N:  always capped at MaximumBlocks × PageSize  <tr> elements
  → DOM stays bounded; eviction is symmetric on both ends
```

**Key State Values — Plain Infinite Scroll**
| Property | Source | Purpose |
|---|---|---|
| `Rows` (SfGrid) | Accumulated via `AddRange` (down) / `InsertRange(0)` (up) in `GenerateInfiniteRows` | Full rendered row list |
| Initial DOM row count | `InitialBlocks × PageSize` | Number of `<tr>` elements on first load |
| Cache DOM cap | `MaximumBlocks × PageSize` | Max `<tr>` count when `EnableCache=true` |
| `rowIndex` start | `lastRow.Index + 1` (down) or `firstRow.Index − PageSize` (up) | Absolute index for new rows |
| `RenderMaskTable` | **Never `true` in this path** | Mask table not used for plain infinite scroll |
| `CacheGeneratedRows` | Called when `EnableCache=true` | Deduplicates rows by absolute index via `InfiniteGeneratedRows` dictionary |

---

### 2. Grouped Infinite Scroll (`EnableInfiniteScrolling = true` + `AllowGrouping = true`, `EnableLazyLoading = false`)

> No mask table in this mode — same rule as plain infinite scroll; `RenderMaskTable` is never set `true` here.

```
SfGrid<TValue>  (#ID  .e-grid)
└── .e-gridcontent
    └── .e-content
        └── GridContent<TValue>
            └── <table .e-table #ID_content_table>
                └── <tbody role="rowgroup">
                    ├── GridRow [CaptionRow]     class="e-groupcaptionrow"
                    │   └── <tr>
                    │       ├── <td .e-indentcell> × GroupDepth     [indent cells]
                    │       └── <td .e-groupcaption colspan=N>
                    │           └── ExpandCellRenderer + caption text
                    │
                    ├── GridRow [DataRow]        class="e-row"    (when caption expanded)
                    │   └── <tr>
                    │       ├── <td .e-indentcell> × GroupDepth
                    │       └── <td .e-rowcell> × VisibleColumns.Count
                    │
                    └── GridRow [SummaryRow]     class="e-summaryrow" (if aggregates)
                        └── <tr>
                            └── <td .e-summarycell> × VisibleColumns.Count
```

**Two-Phase Group Row Generation**
```
Phase 1 — Full list build (HandleGroupedInfiniteRows → GenerateInfiniteGroupedRows):
  GroupModelGenerator<T>.GenerateInfiniteGroupedRows(Data)
    ├── Iterates all Group<T> items in CurrentViewData
    ├── GetGroupedRecords() → recursive caption + data rows with Visible flags
    ├── Down scroll: infiniteGroupedRows.AddRange(Rows)
    ├── Up scroll:   infiniteGroupedRows.InsertRange(0, Rows)
    └── Result stored in InfiniteScrollModule.GeneratedInfiniteGroupedRows

Phase 2 — Viewport slice (SetCurrentGroupedRows):
  InfiniteScroll<T>.SetCurrentGroupedRows()
    ├── newRows = GeneratedInfiniteGroupedRows.Where(x => x.Visible).ToList()
    ├── Down scroll: skip to lastRow position, Take(PageSize), AddRange to Parent.Rows
    ├── Up scroll:   upStart = newRows.FindIndex(firstRow), InsertRange(0, slice)
    └── Cache eviction: RefreshInfiniteCacheRows applied if EnableCache
```

---

### 3. Lazy Load Grouped Infinite Scroll (`EnableInfiniteScrolling = true` + `AllowGrouping = true` + `GroupSettings.EnableLazyLoading = true`)

```
SfGrid<TValue>
└── .e-gridcontent
    └── .e-content
        └── GridContent<TValue>
            └── <table .e-table #ID_content_table>
                └── <tbody role="rowgroup">
                    │
                    ├── GridRow [CaptionRow — top level]    Indent=0
                    │   └── <tr .e-groupcaptionrow>
                    │
                    ├── GridRow [CaptionRow — nested]       Indent=1  (if multi-level group)
                    │   └── <tr .e-groupcaptionrow>
                    │
                    ├── GridRow [DataRow — lazily loaded]   Indent=GroupDepth
                    │   class may include:
                    │   ├── "e-firstchildrow"               ← first visible child row
                    │   ├── "e-lazyload-middle-down"        ← middle sentinel (triggers mid-load)
                    │   └── "e-lazyload-last-down"          ← last sentinel (triggers next page)
                    │
                    └── GridRow [SummaryRow]  (if aggregates configured)
```

**Lazy Load Child Sequence**
```
JS detects sentinel row in viewport
  └── LoadInfiniteData(isLazyLoadChild=true, middleRowUid, lastRowUid)  [GridJSInteropAdaptor.cs line 619]
        │
        ├── ContentStateChanged(true)                  ← shows mask table
        ├── sfBlazor.Grid.updateClonedMaskTranslates   [sf-grid.ts line 1021] ← JS aligns mask over real table
        ├── await Task.Yield()                         ← lets Blazor render mask
        │
        └── InfiniteScrollModule.LoadLazyLoadChildData()
              ├── Locate parentCaptionRow via childRow.ParentUid
              ├── Compute renderedChildRows vs childDataItems.Count()
              ├── GroupModelGenerator.GenerateLazyRowsobject(currentChild, index, parentUid)
              │     └── Produces new data rows with correct ParentUid linkage
              ├── Mark e-firstchildrow / e-lazyload-middle-down / e-lazyload-last-down CSS
              ├── Parent.Rows.InsertRange(insertIndex + 1, lazyRows)
              ├── GroupModule.LazyRows = Parent.Rows
              ├── SoftRefresh = true; RenderMaskTable = false
              ├── EventAggregator.Trigger("ContentStateChanged", null!)
              └── InvokeSuccessAsync()  →  sfBlazor.Grid.contentReady  [sf-grid.ts line 25] (scroll restore)
```

**Sentinel CSS Classes and Their Roles**
| CSS Class | Row Position | JS Intersection Observer Action |
|---|---|---|
| `e-lazyload-middle-down` | `LazyLoadBlockSize`-th child row | Fires `LoadInfiniteData(isLazyLoadChild=true, middleRowUid=uid)` |
| `e-lazyload-last-down` | `LazyLoadPageSize - 1`-th child row | Fires `LoadInfiniteData(isLazyLoadChild=true, lastRowUid=uid)` |
| `e-firstchildrow` | First visible child under a parent | Marks the start of rendered child block |

---

## Combination Architecture Flow (ASCII)
<!-- token-budget: 350 words -->

### A. Plain Infinite Scroll — Down Scroll Flow

> No mask table. `LoadInfiniteData` goes directly to `GenerateInfiniteScrollDatas`.

```
User scrolls to bottom
        │
        ▼
[JS: sfBlazor.Grid — intersection observer fires]
        │
        ▼
LoadInfiniteData(args, isBottom=true, isLazyLoadChild=false)
        │                                      [GridJSInteropAdaptor.cs]
        ▼
InfiniteScrollModule.GenerateInfiniteScrollDatas(isBottom=true)
        │
        ├── IsDownScroll = true
        ├── Compute currentPageUpdate from lastRow.Index
        │     currentPage = ceil(lastRow.Index / PageSize) + 1
        ├── PageSettings.UpdateProperties("CurrentPage", newPage)
        │
        ▼
DataProcess(action)                            [SfGrid — fetches new page from server]
        │
        ▼
RefreshInfiniteCurrentViewData(newData)
        ├── _infiniteCurrentViewData.AddRange(newData)   [normal mode]
        └── (EnableCache) if count > MaximumBlocks×PageSize:
              _infiniteCurrentViewData.RemoveRange(0, itemsToRemove)
        │
        ▼
ContentParameters.Data = CurrentViewData       [triggers OnParametersSet in GridContent]
        │
        ▼
RowModelGenerator.GenerateInfiniteRows(Data)
        ├── rowIndex = lastRow.Index + 1
        ├── GenerateRow() × PageSize  →  new Row<object> list
        ├── (normal mode)  Parent.Rows.AddRange(newRows)
        │     → DOM <tbody> grows by PageSize new <tr> elements
        └── (cache mode)   CacheGeneratedRows() deduplicates
                           Parent.Rows.AddRange(cachedRows)
                           RefreshInfiniteCacheRows → RemoveTopRows
                           → DOM <tbody> stays at MaximumBlocks×PageSize <tr> elements
        │
        ▼
GridContent.Rows = Parent.Rows                 [Blazor re-renders <tbody>]
        │
        ▼
InvokeSuccessAsync()                           [SfGrid.razor.cs line 1650]
        └── sfBlazor.Grid.contentReady         [sf-grid.ts line 25]
              ← JS: restores scrollTop, re-attaches
                intersection observer on last row
```

---

### B. Grouped Infinite Scroll — Down Scroll Flow

```
User scrolls to bottom
        │
        ▼
LoadInfiniteData(isBottom=true)
        │
        ▼
InfiniteScrollModule.GenerateInfiniteScrollDatas(isBottom=true)
        │
        ├── [GroupSettings.Columns.Length > 0, EnableLazyLoading = false]
        ├── Compute lastRowIndex from GeneratedInfiniteGroupedRows visible list
        ├── PageSettings.UpdateProperties("CurrentPage", newPage)
        │
        ├── SetCurrentGroupedRows()             ← No new data fetch; slices from full list
        │     ├── newRows = GeneratedInfiniteGroupedRows.Where(Visible)
        │     ├── skip = indexOf(lastRow) + 1
        │     ├── Parent.Rows.AddRange(newRows.Skip(skip).Take(PageSize))
        │     └── (EnableCache) RefreshInfiniteCacheRows → RemoveTopRows
        │
        ├── CallStateHasChangedAsync()           ← triggers GridContent re-render
        │
        └── InvokeSuccessAsync()                [SfGrid.razor.cs line 1650]
              └── sfBlazor.Grid.contentReady    [sf-grid.ts line 25]
```

> **Note on Phase 1 rebuild**: `HandleGroupedInfiniteRows()` in `GridContent.OnParametersSet` rebuilds
> `GeneratedInfiniteGroupedRows` via `GroupModelGenerator.GenerateInfiniteGroupedRows(Data)` whenever
> `Data != _data` AND `RequestType != "GroupExpandCollapse"`.
> Group expand/collapse skips phase 1 and calls `SetCurrentGroupedRows(captionRow)` directly.

---

### C. Lazy Load Grouped Infinite Scroll — Caption Expand Flow

```
User clicks expand on a CaptionRow
        │
        ▼
Group.LazyExpandCollapse(Row, Rows)            [Group.cs]
        │
        ├── Row.IsExpand = true
        ├── DataManager.ExecuteQuery (fetch child data for this group)
        ├── (Row.Data as Group<T>).Items = fetched data
        │
        ├── [EnableInfiniteScrolling]
        │     └── InfiniteScrollModule.SetLazyLoadPageSize(gridHeight)
        │           ├── LazyLoadPageSize = floor(height / RowHeight) * 3
        │           └── LazyLoadBlockSize = ceil(LazyLoadPageSize / 2)
        │
        ├── LazyExpand(data, index, Row, _expandedRowIndexes)
        │     ├── InfiniteScroll.LazyLoadExpandCollapse(data, Row)
        │     │     ├── dataValues = data.Skip(0).Take(LazyLoadPageSize)
        │     │     ├── For Group<T> items: GenerateCaptionRow × LazyLoadPageSize
        │     │     │     └── Mark e-lazyload-middle-down / e-lazyload-last-down
        │     │     └── For data items: GroupModelGenerator.GenerateDataRows
        │     │           └── Mark e-lazyload-middle-down / e-lazyload-last-down
        │     └── GroupModule.LazyRows.InsertRange(index+1, expandRow)
        │
        └── EventAggregator.Trigger("ContentStateChanged", null!)
              └── GridContent.InvokeStateChange(null)
                    ├── Rows = Parent.Rows   (LazyRows now reflected in Parent.Rows)
                    └── StateHasChanged()
```

---

### D. Post-DOM Refresh Scroll Restoration

```
After any Blazor re-render that changes row count:
        │
        ▼
InvokeSuccessAsync()                           [SfGrid.razor.cs line 1650]
        │
        ├── await Task.Yield()                 ← ensures DOM is committed
        │
        └── sfBlazor.Grid.contentReady(DataId, ClientOption, action, isResetData)
              │                                [sf-grid.ts line 25 — JS side]
              ├── Recalculate content div scrollHeight
              ├── Restore scrollTop to pre-render position      [prevents jump]
              ├── Re-register intersection observer on sentinel rows
              │     ├── e-lazyload-middle-down  →  mid-page load trigger
              │     └── e-lazyload-last-down    →  next-page load trigger
              └── (KeyInteractionScroll path)
                    OnAfterRenderAsync in GridContent:
                      ├── Read CurrentRowIndex from InfiniteScrollModule
                      └── FocusModule.ProcessKeyDown(LastKeyCombination, Row, Cell)
```

---

### E. Group Expand/Collapse All with Infinite Scroll

```
ExpandAllGroupAsync() / CollapseAllGroupAsync()
        │
        ├── ForceUpdate = true
        ├── InfiniteScrollModule.RequestType = "GroupExpandCollapseAll"
        ├── InfiniteScrollModule.ResetInfiniteProperties("GroupExpandCollapseAll")
        │     ├── InfiniteForceRefresh = true  (non-lazy grouping)
        │     ├── currentPage = 1
        │     └── InfiniteGeneratedRows.Clear()
        │
        ├── Iterate GeneratedInfiniteGroupedRows  → set Visible / IsExpand flags
        │
        ├── CallStateHasChangedAsync()
        │     └── GridContent re-renders with InfiniteForceRefresh gate:
        │           HandleGroupedInfiniteRows()
        │             └── (InfiniteForceRefresh) Rows = Parent.Rows  [use existing]
        │                 InfiniteForceRefresh = false
        │
        └── sfBlazor.Grid.resetExpandCollapseAllScroll(DataId, RequestType)
              [Group.cs lines 1204, 1295 → sf-grid.ts line 1108]
              └── JS resets scrollTop to 0 and re-registers observers
```

---

## JavaScript Interop
<!-- token-budget: 150 words -->

Infinite scrolling requires tight coordination between Blazor C# and JavaScript for scroll position management, mask table positioning, and intersection observer lifecycle.

### C# → JS Interop Calls

| C# Method | Line | JS Function | TS Line | Purpose |
|-----------|------|-------------|---------|---------|
| `InvokeSuccessAsync()` | 1686 | `sfBlazor.Grid.contentReady` | 25 | Restore scroll position after DOM update, re-register intersection observers |
| `LoadInfiniteData()` (lazy child) | 631 | `sfBlazor.Grid.updateClonedMaskTranslates` | 1021 | Position mask table over real content during lazy child load |
| `ExpandAllGroupAsync()` / `CollapseAllGroupAsync()` | 1204, 1295 | `sfBlazor.Grid.resetExpandCollapseAllScroll` | 1108 | Reset scrollTop to 0, re-register observers after expand/collapse all |

### TypeScript Function Signatures

```typescript
// Line 25 in sf-grid.ts
contentReady(dataId: string, options: IGridOptions, action: string, isResetData?: boolean): InitModulesResults

// Line 1021 in sf-grid.ts
updateClonedMaskTranslates(dataId: string): void

// Line 1108 in sf-grid.ts
resetExpandCollapseAllScroll(dataId: string, requestType: string): void
```

### Responsibility Split

| Responsibility | Blazor C# | JavaScript |
|---|---|---|
| Detect scroll events, trigger page fetch | ❌ | ✅ Intersection observer on sentinel rows |
| Fetch new page data from server | ✅ `GenerateInfiniteScrollDatas` | ❌ |
| Build new row models, update `Rows` list | ✅ `GenerateInfiniteRows` / `SetCurrentGroupedRows` | ❌ |
| Restore scroll position after render | ❌ | ✅ `contentReady` reads cached scrollTop |
| Show/hide mask table during lazy load | ✅ `RenderMaskTable` flag | ✅ `updateClonedMaskTranslates` positions it |
| Re-register intersection observers | ❌ | ✅ `contentReady` attaches to new sentinel rows |

### CRITICAL Interop Rules

1. **`InvokeSuccessAsync` MUST be the last call in every scroll path** — JS needs DOM to be committed before restoring scroll position; missing this causes scroll jump
2. **Mask table lifecycle: `RenderMaskTable = true` → JS positions → `RenderMaskTable = false`** — must complete before `ContentStateChanged(null)` or mask will render over real data
3. **`contentReady` must run after `StateHasChanged()`** — uses `await Task.Yield()` to ensure Blazor DOM commit is complete before JS scroll restore
4. **JS never modifies `Rows` list** — all row additions/removals happen in C#; JS only reads DOM and manages scroll/observers

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live codebase. Pairs with no interaction risk are omitted.

| Combination | Must Preserve | Risk |
|---|---|---|
| InfiniteScroll + Grouping (normal) | Phase-1 `GenerateInfiniteGroupedRows` runs on every `Data != _data` except `GroupExpandCollapse`; `SetCurrentGroupedRows` always called for viewport slice | Critical |
| InfiniteScroll + Lazy Load Grouping | `LoadLazyLoadChildData` path is gated by `isLazyLoadChild=true` in `LoadInfiniteData`; sentinel CSS classes must be applied correctly; `RenderMaskTable` lifecycle must be true→false | Critical |
| InfiniteScroll + Sorting/Filtering/Searching | `ResetInfiniteProperties` must be called with correct action string; `IsInfiniteInitialRender = true` resets to page 1; `InfiniteGeneratedRows` cleared | Critical |
| InfiniteScroll + Editing (Save/Delete) | `RequestType = "Save"/"Delete"` takes special path in `IntialInfinitePageQuery` and `SetCurrentGroupedRows`; row indexes must be recalculated from `Parent.Rows.First().Index` | High |
| InfiniteScroll + Cache (`EnableCache=true`) | DOM always capped at `MaximumBlocks × PageSize` `<tr>` elements; `CacheGeneratedRows` deduplicates by absolute index; `RemoveTopRows`/`RemoveBottomRows` must not reduce below `PageSize`; `MaximumBlocks` must be ≥ `InitialBlocks` (auto-corrected in `IntialInfinitePageQuery`) | High |
| InfiniteScroll + Column Virtualization | `WidthController` in mask table uses `VirtualScrollModule.GetVirtualColumns()` for frozen; must not break mask cell alignment | High |
| InfiniteScroll + Frozen Columns | Mask table adds `IsFrozen`/`IsFrozenRight` from `ContentParameters`; frozen `ContentParameters.Rows` path used (not `GenerateInfiniteRows`) | High |
| InfiniteScroll + Row Drag-and-Drop | `RequestType = "RowDragAndDrop"` triggers `ResetInfiniteProperties`; `IsInfiniteInitialRender = true`; page reset to 1 | Medium |
| InfiniteScroll + Aggregates | `FetchChildAggregateCount` used in `SetCurrentGroupedRows` to include summary rows in take-count; omitting breaks caption row child counts | Medium |
| InfiniteScroll + Key Navigation | `KeyInteractionScroll = true` set before `GenerateInfiniteScrollDatas`; `CurrentRowIndex` stored; `OnAfterRenderAsync` in `GridContent` re-fires `ProcessKeyDown` after render | Medium |

---

## Prompt Template
<!-- token-budget: 300 words -->

```
Mode: infinite-scroll-dom-structure | bug-fix | feature-implementation
Skill: infinite-scroll-dom-structure-skill

Context:
  EnableInfiniteScrolling = true
  AllowGrouping = [true|false]
  GroupSettings.EnableLazyLoading = [true|false]
  EnableCache = [true|false]

Task: [describe the bug or feature here]

MANDATORY pre-work:
1. Read src/Internal/Actions/InfiniteScroll.cs — GenerateInfiniteScrollDatas, SetCurrentGroupedRows, LoadLazyLoadChildData, RefreshInfiniteCacheRows
2. Read src/Internal/Renderer/GridContent.razor — OnParametersSet, HandleGroupedInfiniteRows, InvokeStateChange
3. Read src/Internal/Generators/RowModelGenerator.cs — GenerateInfiniteRows (index continuation logic)
4. Read src/Internal/Generators/GroupModelGenerator.cs — GenerateInfiniteGroupedRows, GetGroupedRecords
5. For lazy load grouping: read src/Internal/Actions/Group.cs — LazyExpandCollapse, LazyExpand, LoadLazyLoadChildData flow
6. Read src/Internal/Base/GridJSInteropAdaptor.cs — LoadInfiniteData (JS invokable entry point)
7. Consult Interaction Matrix before any change — identify which combinations are affected

DOM invariants to NEVER break:
- Mask table (.e-masked-table) is ONLY used in the lazy load child path (isLazyLoadChild=true); never add it to plain infinite scroll or normal grouped infinite scroll
- RenderMaskTable must be reset to false before ContentStateChanged(null) fires in LoadLazyLoadChildData
- Normal mode (EnableCache=false): rows only ever grow — AddRange (down) / InsertRange(0) (up); never remove rows
- Cache mode (EnableCache=true): DOM always capped at MaximumBlocks×PageSize <tr> elements; RemoveTopRows on down, RemoveBottomRows on up; initial load = InitialBlocks×PageSize rows
- Rows list must only use AddRange (down) / InsertRange(0) (up) — never replace entirely during scroll
- GeneratedInfiniteGroupedRows must be rebuilt on every Data change except GroupExpandCollapse
- Sentinel CSS classes (e-lazyload-middle-down, e-lazyload-last-down) must be set at LazyLoadBlockSize and LazyLoadPageSize-1 positions within lazily loaded child rows only
- InvokeSuccessAsync must be called last in every scroll path to trigger contentReady scroll restore
- MaximumBlocks >= InitialBlocks is auto-enforced in IntialInfinitePageQuery; never rely on user setting them correctly
```
````
