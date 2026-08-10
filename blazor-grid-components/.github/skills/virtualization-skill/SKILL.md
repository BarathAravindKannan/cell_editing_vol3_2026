---
name: virtualization-skill
description: Expert knowledge for the Row & Column Virtualization feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to virtualization behaviour, including viewport rendering, cache management, scroll offset computation, and cross-feature interaction guarantees with sorting, filtering, grouping, editing, and frozen columns.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Row & Column Virtualization in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing virtualization features, debugging scroll issues, optimizing cache strategies, or fixing regressions involving virtual rendering with sorting, filtering, grouping, editing, or frozen columns.

---

## Knowledge References
<!-- token-budget: 60 words -->

- **`.github/skills/virtualization-dom-structure-skill/SKILL.md`** — **DOM structure & component rendering flow** (use for rendering bugs, DOM layout issues, transform errors, mask row problems, cell rendering flow, grouping combinations)
- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern
- `/openspec/openspec/specs/virtualization/spec.md` — Complete virtualization specification
- `/docs/architecture/system-architecture.md` — VirtualScroll module lifecycle, initialization sequence, internal flags
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# version constraints, JS interop boundaries
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, performance rules

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — virtualization cache invalidation must trigger on every feature state change (sort, filter, group, edit, column reorder)
- **JS-interop scoped to DOM operations only** — scroll measurement and CSS transforms live in `scripts/sf-grid.ts`; all data logic stays in C#
- **Module communication via EventAggregator** — `VirtualComponentUpdate` and `VirtualHeaderComponentUpdate` trigger re-renders without tight coupling
- **PropertyChanges dictionary drives incremental updates** — `EnableVirtualization` toggle requires cleanup via `sfBlazor.Grid.virtualDisconnect`
- **Cache precedence prevents redundant fetches** — `GeneratedData[index]` hit = synchronous re-render; miss = async `DataProcess()`
- **Row height uniformity constraint** — `RHeight` measured once at init; variable row heights unsupported in virtual mode

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`Internal/Actions/VirtualScroll.cs`** — Core virtualization engine (cache, index calculation, scroll handlers)
- **`Internal/Renderer/GridVirtualContent.razor`** — Virtual row container replacing `GridContent.razor` when `EnableVirtualization = true`
- **`Internal/Renderer/GridVirtualHeader.razor`** — Virtual column header renderer for `EnableColumnVirtualization = true`
- **`Internal/Base/GridJSInteropAdaptor.cs`** — JS interop bridge; invokes `sfBlazor.Grid.initialize`, `clientTransformUpdate`, `updateVirtualColumns`
- **`scripts/sf-grid.ts` (`window.sfBlazor.Grid`)** — TypeScript module handling scroll listeners, row height measurement, CSS transform application
- **`SfGrid.Properties.cs` (lines 640–800)** — Public API properties: `EnableVirtualization`, `EnableColumnVirtualization`, `EnableVirtualMaskRow`, `OverscanCount`

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Virtualization + Sorting | Cache invalidated on sort; re-render maintains sort order; `ClearCacheData()` called | Critical |
| Virtualization + Filtering | Cache invalidated on filter; virtual window recalculates from filtered dataset; `TotalItemCount` updates | Critical |
| Virtualization + Grouping | `GroupGeneratedData` replaces `GeneratedData`; lazy group loading uses `GetUiData()` + `GenerateLazyRowsobject()` | Critical |
| Virtualization + Editing | CRUD Add-Top only when `RowStartIndex == 0`; Add-Bottom when `IsBottomAddForm()` true; `ScrollToEditedRowAsync()` before save | High |
| Virtualization + Paging | `PageSize` auto-overridden by `EnsurePageSize()` from grid height ÷ `RHeight` × 2; fetch window = `PageSize` rows | High |
| Virtualization + Column Reorder | `RefreshColOffsets()` rebuilds `_coffSets` cumulative width dictionary; cache invalidated | High |
| Virtualization + Frozen Columns | Frozen columns always rendered; only movable columns in `StartColumnIndex`/`EndColumnIndex` window | High |
| Virtualization + Selection | Selected rows outside viewport preserved in `Selection<T>` internal state; DOM re-render applies selection CSS | Medium |
| Virtualization + Infinite Scroll | Mutually exclusive; virtualization = fixed viewport windowing; infinite scroll = append-on-demand | Critical |
| Virtualization + Export | Export must fetch full dataset; virtual cache insufficient; `DataSource` query without skip/take limits | High |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Virtualization  

**Context**  
You are implementing or fixing Row & Column Virtualization for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Virtualization renders only visible rows/columns in the DOM, keeping memory and rendering cost proportional to viewport size, not dataset size.

**Core Architecture**  
- **Module**: `VirtualScroll<T>` in `Internal/Actions/VirtualScroll.cs`
- **Renderers**: `GridVirtualContent.razor` (rows), `GridVirtualHeader.razor` (columns)
- **JS Bridge**: `GridJSInteropAdaptor<T>` calling `sfBlazor.Grid.initialize`, `clientTransformUpdate`, `updateVirtualColumns`
- **Cache**: `GeneratedData[RowStartIndex]` (rows), `_coffSets` (column offsets)
- **Public API**: `EnableVirtualization`, `EnableColumnVirtualization`, `EnableVirtualMaskRow`, `OverscanCount`

**Key Behaviors**  
1. **Cache-first**: Check `GeneratedData[index]` before fetch. Hit = sync re-render; miss = async `DataProcess()`.
2. **Cache invalidation**: `ClearCacheData()` on sort, filter, group, column reorder, row reorder, data source change.
3. **Index calculation**: `RowStartIndex`/`RowEndIndex` from `ScrollTop ÷ RHeight`; `StartColumnIndex`/`EndColumnIndex` from `_coffSets` and `scrollLeft`.
4. **CSS positioning**: `transform: translateY(${TranslateY}px)` for rows; `translateX(${TranslateX}px)` for columns.
5. **CRUD guards**: Add-Top only if `RowStartIndex == 0`; Add-Bottom via `IsBottomAddForm()`; `ScrollToEditedRowAsync()` before save.
6. **Frozen columns**: Always rendered; virtualization applies only to movable columns.
7. **Grouping**: Uses `GroupGeneratedData`; lazy loading switches to `GetUiData()` path.

**Cross-Feature Rules** (from Training + Interaction Matrix)  
- **Sorting**: `ClearCacheData()` on sort; maintain sort order in re-rendered window.
- **Filtering**: `TotalItemCount` updates; cache wiped; virtual window recalculated from filtered set.
- **Editing**: `HasAddOrCancelAction` flag prevents scroll-edit conflicts; `ScrollToEditedRowAsync()` scrolls viewport to edited row.
- **Paging**: `EnsurePageSize()` auto-calculates from grid height ÷ `RHeight` × 2; overrides default `PageSize = 12`.
- **Column Reorder**: `RefreshColOffsets()` rebuilds `_coffSets`; triggers `VirtualHeaderComponentUpdate`.
- **Infinite Scroll**: Mutually exclusive; document constraint if both enabled.

**JS-Interop Contract**  
- **Init**: `sfBlazor.Grid.initialize()` returns `{ RowHeight, IndentWidth }` → stored as `RHeight`.
- **Scroll events**: JS fires vertical/horizontal scroll → `HandleVerticalScrollAsync`/`HandleHorizontalScrollAsync`.
- **Transform apply**: `sfBlazor.Grid.clientTransformUpdate({ translateY, translateX, maskFlag, bottomFlag })`.
- **Column update**: `sfBlazor.Grid.updateVirtualColumns(columnSlice)` pushes new column metadata to JS.
- **Cleanup**: `sfBlazor.Grid.virtualDisconnect()` when `EnableVirtualization` toggled off.

**Performance Constraints**  
- DOM node count = O(viewport), not O(dataset).
- Target ≥ 60 fps scroll on 100k rows / 200 columns.
- `OverscanCount` minimum = 2–5 to reduce blank-flash on fast scroll.

**Edge Cases**  
- Fast scroll past cached window → mask rows if `EnableVirtualMaskRow = true`.
- Column with no explicit `Width` → defaults to `200px`; `_coffSets` still computes.
- `DataSource` replaced at runtime → `IsDataSourceChanged = true` → `refreshOnDataChange` wipes cache, resets scroll.
- Row height varies → unsupported; `RHeight` uniform per initialization.
- Grid height not set → `EnsurePageSize()` cannot auto-calculate; stays at default/configured value.

**When to Use Related Skills**  
- **For DOM rendering issues, component selection logic, or CSS transform problems** → Load `.github/skills/virtualization-dom-structure-skill/SKILL.md`  
  (Use when: wrong DOM structure, missing `.e-virtualtable`, `TranslateY`/`TranslateX` not applied, mask rows not rendering, `GridVirtualContent` vs `GridContent` selection, cell rendering flow, grouping caption/indent cells)
- **For cache management, scroll handlers, or index calculation logic** → Use this skill (virtualization-skill)  
  (Use when: cache not invalidating, `RowStartIndex`/`RowEndIndex` wrong, `GeneratedData` issues, scroll events not firing, `_coffSets` offset calculation, `ClearCacheData()` not called)
- **For combined issues spanning both areas** → Load both skills  
  (Example: "Rows not rendering after scroll AND wrong TranslateY" = cache issue + rendering issue)

**Before Making Changes**  
1. Read `/docs/training/` — understand cross-feature interaction rules and anti-patterns.
2. Verify cache invalidation triggers for every feature state change.
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required).
4. Test with sorting, filtering, grouping, editing, frozen columns enabled.
5. Validate `OverscanCount` math: `CurrentIndexes(start, end)` → `(start - overscan, end + overscan)` clamped.
6. For DOM/rendering bugs, consult `.github/skills/virtualization-dom-structure-skill/SKILL.md` for component tree and element structure.

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public properties have XML doc comments with `<summary>`, `<value>`, `<remarks>`.
- Cache hit/miss logic traceable via internal diagnostic logging (DEBUG builds).
- Playwright E2E test covering fast scroll, cache hit, CRUD-in-virtual-mode.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## JavaScript Interop
<!-- token-budget: 120 words -->

**C# → JavaScript Call Flow**

| C# Method (VirtualScroll.cs) | TypeScript Function (scripts/sf-grid.ts) | Purpose | Line References |
|------------------------------|------------------------------------------|---------|-----------------|
| `RefreshColumnIndex()` | `refreshColumnIndex()` | Rebuild column index cache after reorder | C#: line 220, TS: line 339 |
| `RefreshOnDataChange()` | `refreshOnDataChange()` | Reset virtual scroll state on data change | C#: line 233, TS: line 656 |
| `ClientTransformUpdate()` | `clientTransformUpdate()` | Apply CSS transforms (translateY/X) for positioning | C#: lines 1861, 1943, 1971, TS: line 309 |
| `UpdateVirtualColumns()` | `updateVirtualColumns()` | Push virtualized column metadata to JS | C#: line 2001, TS: line 158 |
| `VirtualDisconnect()` | `virtualDisconnect()` | Cleanup JS listeners when virtualization disabled | TS: line 221 |
| `GetClientHeight()` | `clientHeight()` | Measure grid viewport height | C#: line 1247 |
| `AutoFit()` | `autoFit()` | Trigger column auto-fit calculation | C#: line 1837 |
| `ScrollIntoView()` | `scrollIntoView()` | Programmatically scroll to row/column | C#: line 2024 |

**Responsibility Split**

| Layer | Responsibilities | Files |
|-------|------------------|-------|
| **C# (Business Logic)** | Cache management, index calculation (`RowStartIndex`/`RowEndIndex`), data fetching (`DataProcess`), state synchronization (`ClearCacheData`), feature coordination (sort/filter/group) | `VirtualScroll.cs`, `GridVirtualContent.razor`, `GridVirtualHeader.razor` |
| **TypeScript (DOM Operations)** | Scroll event listeners, row height measurement (`RHeight`), CSS transform application (`translateY`/`translateX`), viewport dimension queries, DOM manipulation for mask rows | `scripts/sf-grid.ts`, `scripts/sf-grid-fn.ts` |

**CRITICAL Rules**
1. **All data logic stays in C#** — JavaScript never fetches or processes grid data
2. **Transform application is JS-owned** — C# calculates offsets, JS applies CSS transforms
3. **Cache invalidation is C#-driven** — JS notifies scroll events, C# decides cache strategy
4. **Cleanup contract required** — Call `virtualDisconnect()` when `EnableVirtualization` toggled off to prevent memory leaks
