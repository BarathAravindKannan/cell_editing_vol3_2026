# Infinite Scroll Skill
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Infinite Scroll in the Syncfusion Blazor Grid. Guarantees no breakage with paging, grouping, editing, filtering, sorting, cache mode.

---

## Agent Invocation

```yaml
name: infinite-scroll-skill
description: Expert knowledge for the Infinite Scroll feature (load-on-demand scrolling) in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to infinite-scroll behaviour, including on-demand data loading via scroll position, cache mode DOM management, InitialBlocks/MaximumBlocks configuration, state reset on grid actions, grouping integration, lazy-load grouping child data, editing integration, cross-feature interaction guarantees with paging (mutual exclusivity), virtualization, grouping, editing, filtering, sorting.
```

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/openspec/openspec/specs/infinite-scroll/spec.md` (19,054 chars: on-demand loading, cache mode, DOM management, grouping/lazy-load integration)
- `/docs/training/01-getting-started/architecture-overview.md` (module injection, EventAggregator, 4-layer architecture)
- `/docs/architecture/system-architecture.md` (module lifecycle, PropertyChanges dictionary, JS interop)
- `/docs/code-guidelines/coding-standards.md` (async/await with ConfigureAwait, guard clauses, parameter patterns)
- `/docs/tech-stack/tech-stack.md` (Blazor patterns, C# 12/13/14, JSInvokable attribute)

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- Infinite scrolling and paging are mutually exclusive (constraint: EnableInfiniteScrolling + AllowPaging = undefined behaviour)
- Cache mode (EnableCache=true) caps DOM rows to MaximumBlocks × PageSize by trimming opposite-end rows (requirement: Cache Mode DOM Management)
- State reset required on filter/sort/search/group/edit actions to avoid stale scroll position (requirement: State Reset on Grid Actions)
- InitialBlocks clamped to MaximumBlocks when EnableCache=true to prevent DOM overflow (requirement: InitialBlocks Controls First-Load Quantity)
- JS interop entry point LoadInfiniteData invoked by sf-grid.js when scroll hits bottom/top (technical note: JS Interop Entry Point)
- Row cache dictionary InfiniteGeneratedRows prevents duplicate row generation (requirement: Row Cache Dictionary)

---

## Code Location Map
<!-- token-budget: 80 words -->

- `Internal/Actions/InfiniteScroll.cs` — Core engine: GenerateInfiniteScrollDatas, IntialInfinitePageQuery, ResetInfiniteProperties, CacheGeneratedRows, RemoveTopRows/RemoveBottomRows, LoadLazyLoadChildData
- `GridInfiniteScrollSettings.razor.cs` — Settings component: InitialBlocks (default=3), MaximumBlocks (default=3), EnableCache (default=false)
- `SfGrid.Properties.cs` (lines 1270–1302) — EnableInfiniteScrolling parameter (default=false), InfiniteScrollSettings parameter
- `SfGrid.Lifecycle.cs` (line 34) — Module instantiation: InfiniteScrollModule = new InfiniteScroll<T>(this)
- `Internal/Base/GridJSInteropAdaptor.cs` (lines 619–620) — `[JSInvokable]` decorator at 619; `LoadInfiniteData` method signature at 620: `LoadInfiniteData(object args, bool isBottom = false, bool isTop = false, bool isLazyLoadChild = false, string middleRowUid = null!, string lastRowUid = null!, int middleRowIndex = 0, bool keyInteractionScroll = false)`
- `Internal/Actions/Data.cs` (lines 55–57) — Calls IntialInfinitePageQuery to configure skip/take query
- `Internal/Actions/Edit.cs` (line 240, line 2348) — Resets infinite state after CRUD operations via ResetInfiniteProperties(RequestType); CurrentRowIndex reset to 0 on cancel/save
- `Internal/Actions/Group.cs` (lines 287–288, 343, 1136, 1181, 1224, 1272) — Integrates grouped-row generation: SetCurrentGroupedRows (line 288), GeneratedInfiniteGroupedRows (lines 343, 1181, 1272), ResetInfiniteProperties (lines 287, 1136, 1224)

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

| Combination                      | Must Preserve                                                                                                                                              | Risk     |
|----------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------|----------|
| Infinite Scroll + Paging         | MUTUALLY EXCLUSIVE: EnableInfiniteScrolling and AllowPaging cannot coexist; setting both is undefined behaviour (one must be false)                        | Critical |
| Infinite Scroll + Grouping       | Caption rows must accumulate via SetCurrentGroupedRows; GeneratedInfiniteGroupedRows stores grouped rows; cache mode trims rows via RefreshInfiniteCacheRows | Critical |
| Infinite Scroll + Lazy Grouping  | LoadLazyLoadChildData fetches LazyLoadPageSize children per scroll event; CSS markers (e-lazyload-middle-down, e-lazyload-last-down) applied to child rows  | Critical |
| Infinite Scroll + Editing        | Add in cache mode resets CurrentPage to 1; Save/Delete triggers ResetInfiniteProperties; CurrentRowIndex reset to 0 on cancel/save                         | High     |
| Infinite Scroll + Filtering      | ResetInfiniteProperties("Filtering") clears InfiniteGeneratedRows, sets IsInfiniteInitialRender=true, resets CurrentPage to 1                              | High     |
| Infinite Scroll + Sorting        | ResetInfiniteProperties("Sorting") clears cached rows and resets CurrentPage to 1                                                                          | High     |
| Infinite Scroll + Virtualization | Horizontal virtual scrolling handled by VirtualScrollModule (independent); vertical virtualization disabled when infinite scrolling enabled                | High     |
| Infinite Scroll + Export         | Export calls ResetInfiniteProperties before fetching data when EnableCache=false; all records fetched for ExportType=AllPages                              | High     |
| Infinite Scroll + Search         | ResetInfiniteProperties("Searching") clears cached rows and resets scroll position                                                                         | Medium   |
| Infinite Scroll + Column Reorder | ResetInfiniteProperties("Reorder") called to rebuild DOM rows with new column order                                                                        | Medium   |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

### Mode: infinite-scroll-implementation

**Task**: Implement or fix infinite scroll functionality in the Syncfusion Blazor DataGrid (`SfGrid<TValue>`).

**Core Behaviour Rules**:
1. **EnableInfiniteScrolling**: Flag (default=false) enables load-on-demand scrolling; when true, InfiniteScrollModule is instantiated in OnInitializedAsync.
2. **Mutual Exclusivity**: EnableInfiniteScrolling and AllowPaging CANNOT both be true (undefined behaviour).
3. **InitialBlocks**: Controls first-load quantity (InitialBlocks × PageSize rows fetched on initial render).
4. **MaximumBlocks**: When EnableCache=true, caps DOM rows to MaximumBlocks × PageSize.
5. **EnableCache**: When true, excess rows trimmed from opposite end of scroll direction (RemoveTopRows/RemoveBottomRows); when false, rows accumulate indefinitely.
6. **On-Demand Loading**: JS interop `LoadInfiniteData` invoked by sf-grid.js when scroll hits bottom (isBottom=true) or top (isTop=true in cache mode).
7. **Query Construction**: CurrentPage = ceil((lastRowIndex + 1) / PageSize) + 1 for downward scroll; query uses Page(CurrentPage, PageSize) to fetch next block.
8. **Row Cache**: InfiniteGeneratedRows dictionary stores pre-generated rows keyed by index; CacheGeneratedRows returns cached row if index exists, else stores new row.

**Settings Properties (GridInfiniteScrollSettings)**:
- `InitialBlocks` (default=3): Number of page blocks loaded on initial render.
- `MaximumBlocks` (default=3): Maximum page blocks in DOM when EnableCache=true.
- `EnableCache` (default=false): When true, caps DOM rows and trims excess; when false, rows accumulate.

**State Reset Rules (ResetInfiniteProperties)**:
- **Filtering/Sorting/Searching/Refresh/Reorder/Grouping/UnGrouping**: Set IsInfiniteInitialRender=true, clear InfiniteGeneratedRows, clear _infiniteCurrentViewData, reset CurrentPage=1.
- **Add (cache mode)**: Reset CurrentPage=1, clear InfiniteGeneratedRows (new row appears at top).
- **Delete**: Clear InfiniteGeneratedRows and _infiniteCurrentViewData; preserve CurrentPage (scroll position).
- **GroupExpandCollapseAll**: Set InfiniteForceRefresh=true (non-lazy grouping), reset CurrentPage=1, clear InfiniteGeneratedRows.

**Cross-Feature Integration**:
- **Paging**: MUTUALLY EXCLUSIVE — cannot enable both infinite scroll and paging simultaneously.
- **Grouping (Standard)**: Rows stored in GeneratedInfiniteGroupedRows; SetCurrentGroupedRows appends (IsDownScroll) or prepends (IsUpScroll) grouped rows; RefreshInfiniteCacheRows trims in cache mode.
- **Lazy-Load Grouping**: LoadLazyLoadChildData fetches LazyLoadPageSize children per scroll event; LazyLoadPageSize = floor(Height / RowHeight) × 3; CSS markers (e-lazyload-middle-down at LazyLoadBlockSize index, e-lazyload-last-down at LazyLoadPageSize - 1 index) applied to child rows.
- **Editing**: Add in cache mode resets CurrentPage to 1; Save/Delete triggers ResetInfiniteProperties with RequestType="Save"/"Delete"; CurrentRowIndex reset to 0 on cancel/save.
- **Filtering/Sorting/Search**: Each action calls ResetInfiniteProperties to clear cached rows and reset scroll position.
- **Virtualization**: Horizontal virtual scrolling handled by VirtualScrollModule (independent); vertical virtualization disabled when infinite scrolling enabled.
- **Export**: ResetInfiniteProperties called before fetching data when EnableCache=false; ExportType=AllPages fetches all records (not just cached rows).

**DOM Management (Cache Mode)**:
- **Downward Scroll**: New block appended; RemoveTopRows trims excess rows from top if total > MaximumBlocks × PageSize.
- **Upward Scroll**: New block prepended; RemoveBottomRows trims excess rows from bottom if total > MaximumBlocks × PageSize.
- **ViewData Trimming**: RefreshInfiniteCurrentViewData removes leading (downward) or trailing (upward) items to keep _infiniteCurrentViewData within MaximumBlocks × PageSize.

**JS Interop Entry Point**:
- `LoadInfiniteData` ([JSInvokable] in GridJSInteropAdaptor.cs) invoked by sf-grid.js with parameters: `isBottom`, `isTop`, `keyInteractionScroll`, `isLazyLoadChild`, `lastRowUid`.
- Calls InfiniteScrollModule.GenerateInfiniteScrollDatas(action, isBottom, isTop).

**Code Locations**:
- `Internal/Actions/InfiniteScroll.cs` — Core engine (GenerateInfiniteScrollDatas, IntialInfinitePageQuery, ResetInfiniteProperties, CacheGeneratedRows, RemoveTopRows/RemoveBottomRows, LoadLazyLoadChildData).
- `GridInfiniteScrollSettings.razor.cs` — Settings component (InitialBlocks, MaximumBlocks, EnableCache).
- `SfGrid.Lifecycle.cs` (line 34) — Module instantiation.
- `Internal/Base/GridJSInteropAdaptor.cs` (lines 581–598) — LoadInfiniteData entry point.

**Before Implementation**:
- Read `/docs/training/01-getting-started/architecture-overview.md` for module injection pattern.
- Confirm EnableInfiniteScrolling and AllowPaging are mutually exclusive.
- Verify Interaction Matrix: paging (mutual exclusivity), grouping (SetCurrentGroupedRows, GeneratedInfiniteGroupedRows), editing (CurrentPage reset on Add in cache mode), filtering/sorting (ResetInfiniteProperties), virtualization (horizontal independent, vertical disabled).

---

### Mode: infinite-scroll-bug-fix

**Task**: Fix a bug in infinite scroll functionality without breaking existing behaviour.

**Mandatory Checks**:
1. **Mutual Exclusivity**: Verify EnableInfiniteScrolling and AllowPaging are not both true.
2. **InitialBlocks Clamping**: Confirm InitialBlocks is clamped to MaximumBlocks when EnableCache=true.
3. **State Reset**: Verify ResetInfiniteProperties called on filter/sort/search/group/edit actions with correct parameters.
4. **Row Cache**: Confirm InfiniteGeneratedRows dictionary used to avoid duplicate row generation.
5. **DOM Trimming (Cache Mode)**: Verify RemoveTopRows (downward scroll) and RemoveBottomRows (upward scroll) called when total rows exceed MaximumBlocks × PageSize.
6. **Query Construction**: Confirm CurrentPage calculation is correct: ceil((lastRowIndex + 1) / PageSize) + 1 for downward scroll.
7. **Grouping Integration**: Verify SetCurrentGroupedRows appends (IsDownScroll) or prepends (IsUpScroll) grouped rows; RefreshInfiniteCacheRows trims in cache mode.
8. **Lazy-Load Grouping**: Confirm LoadLazyLoadChildData fetches LazyLoadPageSize children; CSS markers applied to child rows.
9. **Editing Integration**: Verify Add in cache mode resets CurrentPage to 1; CurrentRowIndex reset to 0 on cancel/save.
10. **Export Integration**: Confirm ResetInfiniteProperties called before export when EnableCache=false.

**Interaction Matrix Validation**:
- Infinite Scroll + Paging: Mutually exclusive (cannot coexist).
- Infinite Scroll + Grouping: SetCurrentGroupedRows, GeneratedInfiniteGroupedRows, RefreshInfiniteCacheRows.
- Infinite Scroll + Lazy Grouping: LoadLazyLoadChildData, CSS markers (e-lazyload-middle-down, e-lazyload-last-down).
- Infinite Scroll + Editing: Add in cache mode resets CurrentPage to 1; Save/Delete triggers ResetInfiniteProperties.
- Infinite Scroll + Filtering: ResetInfiniteProperties("Filtering") clears cached rows, resets CurrentPage to 1.
- Infinite Scroll + Sorting: ResetInfiniteProperties("Sorting") clears cached rows, resets CurrentPage to 1.
- Infinite Scroll + Virtualization: Horizontal independent, vertical disabled.
- Infinite Scroll + Export: ResetInfiniteProperties called before export when EnableCache=false.
- Infinite Scroll + Search: ResetInfiniteProperties("Searching") clears cached rows.
- Infinite Scroll + Column Reorder: ResetInfiniteProperties("Reorder") rebuilds DOM rows.

**Before Fix**:
- Read `/docs/training/00-START-HERE.md` — #1 rule: never break existing behavior.
- Review existing unit tests for infinite scroll feature.
- Cross-check Interaction Matrix with live specs from `/openspec/openspec/specs/infinite-scroll/spec.md`.
- Verify JS interop entry point LoadInfiniteData signature matches sf-grid.js invocation.
