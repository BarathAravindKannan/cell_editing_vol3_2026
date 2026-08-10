---
name: scroll-management-skill
description: Expert knowledge for scroll management in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to scroll behaviour, including row scrolling, column scrolling, virtual scroll coordination, infinite scroll on-demand loading, focus-driven scrolling, frozen column/row synchronization, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for scroll management in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**  
When working on scroll-related bugs or implementing scroll features, load this skill first before consulting architecture files. Use it to understand viewport rendering, scroll offset calculations, frozen column coordination, and scroll event chains.

---

## Knowledge References
<!-- token-budget: 60 words -->

**Training Files Consulted**:
- `/docs/training/00-START-HERE.md`
- `/docs/training/01-getting-started/architecture-overview.md`
- `/docs/training/TRAINING-INDEX.md`

**Architecture Files**:
- `/docs/architecture/system-architecture.md`
- `/docs/code-guidelines/coding-standards.md`

**Live Source Locations**:
- `scripts/scroll.ts` — DOM scrolling, padding, height calculations
- `scripts/virtual-scroll.ts` — viewport renderer, cache management, offset tracking
- `scripts/infinite-scroll.ts` — on-demand loading, scroll position tracking
- `src/GridInfiniteScrollSettings.razor.cs` — infinite scroll configuration
- `src/Internal/Actions/VirtualScroll.cs` — row/column virtualization orchestration
- `src/Internal/Actions/InfiniteScroll.cs` — on-demand data fetching
- `src/SfGrid.Lifecycle.cs` — `VirtualScrollModule` and `InfiniteScrollModule` initialization (lines 33-34)
- `src/Internal/Base/GridJSInteropAdaptor.cs` — scroll event bridge to JS

---

## Training Insights Applied
<!-- token-budget: 80 words -->

**Key Rules from Training**:
1. Scroll management is a **hybrid concern**: C# handles viewport calculations and data fetch decisions; JavaScript handles DOM measurement and scroll position tracking.
2. **Three simultaneous environments**: .NET state, Blazor render tree, JS scroll coordinates must remain in sync.
3. **EventAggregator is mandatory**: Cross-module communication (e.g., VirtualScroll + Grouping) must use pub-sub, never direct calls.
4. **PropertyChanges-driven updates**: Only modules whose parameters changed are notified; scroll state must be re-initialized only when EnableVirtualization or EnableInfiniteScrolling changes.
5. **No StateHasChanged() calls**: Use internal render scheduling via grid's methods.

---

## Code Location Map
<!-- token-budget: 80 words -->

| File | Responsibility |
|------|-----------------|
| `scripts/scroll.ts` | DOM scroll measurement, content padding, frozen column/row height sync |
| `scripts/scroll.ts` — `updateFrozenShadow()` (lines 310–327) | Applies left/right frozen shadow styling based on scroll direction |
| `scripts/scroll.ts` — `onCustomScrollbar()` (lines 328–349) | Synchronizes `scrollLeft`/`scrollTop` across content, header, and footer |
| `scripts/virtual-scroll.ts` | Viewport rendering, cache, offset tracking, virtual row/col animation |
| `scripts/infinite-scroll.ts` | On-demand data trigger, scroll direction detection, block calculation |
| `src/Internal/Actions/VirtualScroll.cs` | Row/column cache invalidation, viewport size updates, freeze sync |
| `src/Internal/Actions/InfiniteScroll.cs` | Block offset calculation, direction detection, data request trigger |
| `src/SfGrid.Lifecycle.cs` | Module initialization, IsDataSourceChanged tracking |
| `src/SfGrid.DataProcessing.cs` | Virtual scroll state coordination during data operations |
| `src/Internal/Base/GridJSInteropAdaptor.cs` | JS-to-.NET scroll event bridge, initialization |
| `src/GridInfiniteScrollSettings.razor.cs` | InitialBlocks, MaximumBlocks, EnableCache parameter binding |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

Built from live codebase + training docs cross-reference. Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| Scroll + Virtualization | Offset tracking must survive sort/filter/group; cache invalidation on data change | Critical |
| Scroll + InfiniteScroll | Only one scroll mode active at a time; mutual exclusivity enforced | Critical |
| Scroll + Grouping | Group row heights affect virtual offset calculations; group expand/collapse resets scroll cache | High |
| Scroll + Sorting | Sort preserves scroll position unless data count changed; cache offsets invalidated | High |
| Scroll + Filtering | Filter reduces data count; virtual scroll must recalculate viewport start/end; offset cache cleared | High |
| Scroll + Paging | Paging resets scroll to top; infinite scroll incompatible with paging | High |
| Scroll + Editing | Edit mode may trigger focus-driven scroll; cell focus must be visible in viewport | Medium |
| Scroll + Selection | Row selection may trigger scroll-into-view; keyboard nav requires scroll sync | Medium |
| Scroll + FrozenColumns | Horizontal scroll must sync frozen and movable content; offset calculations must account for frozen width | High |
| Scroll + DetailRows | Expanding detail row may shift viewport; scroll cache must be invalidated | Medium |

---

## JavaScript Interop
<!-- token-budget: 150 words -->

Scroll management relies heavily on C# ↔ JavaScript coordination for DOM measurements and viewport calculations.

**C# → JavaScript Interop Calls**:

| C# Method | JavaScript Function | Line | Purpose |
|-----------|---------------------|------|---------|
| `VirtualScroll.HandleDataSourceChange()` | `sfBlazor.Grid.refreshOnDataChange` | 233 | Refresh virtual scroll on data change |
| `VirtualScroll.RefreshColumnIndex()` | `sfBlazor.Grid.refreshColumnIndex` | 220 | Update column indices after reorder |
| `VirtualScroll.GetClientHeight()` | `sfBlazor.Grid.clientHeight` | 1247 | Get viewport height for row calculations |
| `VirtualScroll.AutoFitColumns()` | `sfBlazor.Grid.autoFit` | 1837 | Trigger column auto-fit in virtual mode |
| `VirtualScroll.UpdateTransform()` | `sfBlazor.Grid.clientTransformUpdate` | 1861, 1943, 1971 | Apply CSS transform for virtual scrolling |
| `VirtualScroll.UpdateVirtualColumns()` | `sfBlazor.Grid.updateVirtualColumns` | 2001 | Update virtualized column range |
| `VirtualScroll.ScrollIntoView()` | `sfBlazor.Grid.scrollIntoView` | 2024 | Scroll to specific row/cell programmatically |

**TypeScript Functions (scripts/virtual-scroll.ts)**:

| Function | Line | Responsibility |
|----------|------|----------------|
| `scrollListener()` | 262 | Main scroll event handler, calculates viewport info |
| `getInfoFromView()` | 981 | Calculates visible row/column indices from scroll position |
| `handleDownwardScroll()` | 1072 | Manages cache for downward scroll direction |
| `handleUpwardScroll()` | 1175 | Manages cache for upward scroll direction |
| `setVirtualHeight()` | 1418 | Sets virtual content height via padding elements |
| `bindScrollEvent()` | 1804 | Attaches scroll event listeners to grid content |
| `resetScrollPosition()` | 1889 | Resets scroll to top after data operations |

**TypeScript Functions (scripts/infinite-scroll.ts)**:

| Function | Line | Responsibility |
|----------|------|----------------|
| `infiniteScrollHandler()` | 170 | Detects scroll-to-bottom, triggers data block load |
| `lazyLoadInfiniteScrollHandler()` | 238 | Handles lazy loading for hierarchical data |

**Responsibility Split**:

| Concern | C# Responsibility | JavaScript Responsibility |
|---------|-------------------|---------------------------|
| Viewport Calculations | Determine which rows/columns to render | Measure DOM dimensions (clientHeight, scrollTop) |
| Data Fetch Decisions | Trigger data queries, cache invalidation | Detect scroll direction, notify C# via InvokeAsync |
| Scroll Positioning | Calculate target scroll offset, validate indices | Apply CSS transforms, update scrollTop |
| Cache Management | Store row/column data, decide when to clear | Track offset cache for smooth scrolling |
| Event Coordination | Handle PropertyChanges, broadcast via EventAggregator | Listen to DOM scroll events, debounce, trigger C# |

**CRITICAL Interop Rules**:
1. **All scroll events must flow through GridJSInteropAdaptor** — never invoke JS directly from feature modules
2. **DOM measurements must happen in JavaScript** — C# should never cache clientHeight, scrollTop, or offsetWidth
3. **Scroll position must be validated** — ensure scroll offsets are within bounds before applying transforms
4. **Cache invalidation must be atomic** — clear both C# data cache and JS offset cache together on data change
5. **Frozen column scroll sync is JS-only** — C# triggers sync request, but JS handles actual scroll coordinate matching
6. **Focus-driven scrolling must not interfere** — keyboard navigation scroll requests must be queued, not interrupt user scroll

---

## Prompt Template
<!-- token-budget: 300 words -->

**Mode**: {feature-implementation | bug-fix}  
**Skill**: Scroll Management  

**Context Setup**:
Before implementing any scroll-related change, consult ALL of the following:
1. Read `/docs/training/01-getting-started/architecture-overview.md` (Scroll is a hybrid concern spanning Layers 1, 2, and 3)
2. Review `/docs/training/05-practical-examples/feature-implementation-walkthrough.md` for cross-feature testing patterns
3. Read the Interaction Matrix in this skill (above) — verify your change does not affect listed feature pairs

**Implementation Checklist**:
- [ ] Scroll logic touches **both JavaScript** (scroll.ts, virtual-scroll.ts, infinite-scroll.ts) **and C#** (VirtualScroll.cs, InfiniteScroll.cs)
- [ ] All JS-to-.NET scroll events flow **only** through `GridJSInteropAdaptor.cs`
- [ ] Changes to cache/offset calculations must **preserve scroll position** across sort/filter/group
- [ ] Frozen column horizontal scroll **must sync** with unfrozen content — test both at same time
- [ ] Infinite scroll and row virtualization are **mutually exclusive** — validate only one is enabled
- [ ] Focus-driven scrolling (keyboard navigation) must not interfere with user-initiated scroll
- [ ] Viewport size changes (grid resize, header height changes) must trigger **viewport recalculation**
- [ ] Group expand/collapse, filter apply, sort apply must **invalidate scroll cache** but **preserve position** if possible

**Regression Testing Required**:
- Scroll + Sorting: 10K rows, sort by multiple columns, verify scroll position preserved
- Scroll + Grouping: Group + expand/collapse, verify offset cache invalidated correctly
- Scroll + Filtering: Apply filter to reduce data, verify scroll does not jump beyond new viewport end
- Frozen Columns: Scroll horizontally, verify frozen column stays fixed and movable content scrolls
- Infinite Scroll: Scroll to bottom, verify data block loads without flicker or duplicate rows
- Keyboard Navigation: Tab/Arrow keys in virtualized grid, verify focus stays in viewport and scroll follows

**Code Path References**:
- Scroll initialization: `SfGrid.Lifecycle.cs` line 315 → `sfBlazor.Grid.initialize()` → TypeScript Grid constructor
- Virtual scroll trigger: DOM scroll event → `VirtualContentRenderer.scrollListener()` (line 262) → `getInfoFromView()` (line 981) → calculates viewport info
- Infinite scroll trigger: `InfiniteScroll.infiniteScrollHandler()` (line 170) → detects scroll-to-bottom → `dotNetRef.invokeMethodAsync('InfiniteScrolling')`
- Cache invalidation: `VirtualScroll.OnAfterRender()` → `InvokeMethod("sfBlazor.Grid.refreshOnDataChange")` → TypeScript `setVirtualHeight()` (line 1418)

