---
name: sorting-skill
description: Expert knowledge for the Sorting feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to sorting behaviour, including single/multi-column sort, programmatic sort APIs, unsort gestures, cross-feature interaction guarantees with grouping, filtering, virtualization, editing, and selection.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Sorting in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing sort features, debugging sort order issues, adding programmatic sort APIs, or fixing regressions involving sorting with grouping, filtering, virtualization, editing, frozen columns, or selection.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern, module communication
- `/openspec/specs/sorting/spec.md` — Complete sorting specification
- `/docs/architecture/system-architecture.md` — Sort<T> module lifecycle, event flow architecture, PropertyChanges tracking
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# version constraints, async/await rules
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, parameter declaration pattern, event pipeline

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — sort must preserve grouped columns (`IsFromGroup = true`), clear non-persistent selection, invalidate virtual cache
- **Module communication via EventAggregator** — `ModelChanged(RequestType = Sorting)` triggers cross-module notifications
- **PropertyChanges dictionary drives incremental updates** — `AllowSorting` / `SortSettings` change triggers `RefreshColumnHeader = true` (header re-render only)
- **Action event pipeline mandatory** — `OnActionBegin` → user cancellation check → execute → `OnActionComplete`; `Sorting` event fires before mutation, `Sorted` after
- **Guard clauses first** — validate `AllowSorting`, `GridColumn.AllowSorting` before any state mutation
- **Single source of truth** — `SortSettings.Columns` is the canonical sort state; `Sort<T>` module reads/mutates this list directly

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`Internal/Actions/Sort.cs`** — Core sorting engine: `SortClickHandler` (line 41), `SortColumn` (line 93), `RemoveSortColumn` (line 146), `UpdateModel` (line 189)
- **`SfGrid.Properties.cs`** — Public API properties: `AllowSorting` (line 241), `AllowMultiSorting` (line 119), `SortSettings` (line 1167)
- **`SfGrid.Methods.cs`** — Public methods: `SortColumnAsync` (line 3191), `SortColumnsAsync` (line 3228), `ClearSortingAsync` (line 466)
- **`GridSortSettings.razor.cs`** — Child component holding `AllowUnsort` (line 41), `Columns` list (line 51)
- **`GridSortColumns.razor.cs`** — Container for declarative `GridSortColumn` children
- **`GridSortColumn.cs`** — Per-column sort descriptor: `Field` (line 44), `Direction` (line 38), `IsFromGroup` (line 50)
- **`Internal/Actions/Data.cs` (DataGenerator<T>)** — Consumes `SortSettings.Columns` to produce `SortBy()` query clauses
- **`Internal/Renderer/GridHeaderCell.razor`** — Renders sort priority badge `e-sortnumber` (line 98), sort icon container `e-sortfilterdiv` (line 187)

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Sorting + Grouping | Grouped columns always precede user-sorted columns in `SortSettings.Columns`; `IsFromGroup = true` flag; `ClearSortingAsync()` skips grouped entries | Critical |
| Sorting + Filtering | Sort applied to filtered dataset; sort does not clear filter predicates; both queries composed in `DataGenerator` | Critical |
| Sorting + Virtualization | Sort triggers `VirtualScroll<T>.ClearCacheData()`; scroll position resets to top; `VirtualComponentUpdate` event after data reload | Critical |
| Sorting + Editing | Sort during active edit discards uncommitted changes (standard Blazor re-render behaviour); inline edit row may shift position post-sort | High |
| Sorting + Selection | Selection cleared after sort unless `GridSelectionSettings.PersistSelection = true` | High |
| Sorting + Paging | Sort applies to full dataset; pager displays sorted result; current page index preserved unless row count changes | High |
| Sorting + Frozen Columns | Frozen and movable columns both participate in sort; frozen columns render sorted data in their fixed position | Medium |
| Sorting + Column Reorder | Sort state (by `Field` name) unaffected by column visual reorder; `Field` binding remains stable | Medium |
| Sorting + Export | Export applies current sort order from `SortSettings.Columns` to exported data | Medium |
| Sorting + Infinite Scroll | Sort resets scroll position; new sorted data fetched from top; previously loaded rows discarded | High |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Sorting  

**Context**  
You are implementing or fixing Sorting for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Sorting reorders grid rows based on one or more column values. All sort logic executes server-side in .NET via the `Sort<T>` action module.

**Core Architecture**  
- **Module**: `Sort<T>` in `Internal/Actions/Sort.cs`
- **State**: `SortSettings.Columns` (List<GridSortColumn>) — single source of truth
- **Entry points**: User header click → `SortClickHandler`; API calls → `SortColumnAsync`, `SortColumnsAsync`, `ClearSortingAsync`
- **Query builder**: `DataGenerator<T>` consumes `SortSettings.Columns` to produce `SortBy()` clauses
- **UI**: `GridHeaderCell.razor` renders `e-ascending` / `e-descending` icons and priority badges
- **Public API**: `AllowSorting`, `AllowMultiSorting`, `GridSortSettings.AllowUnsort`, `GridColumn.AllowSorting`

**Key Behaviors**  
1. **Direction cycle**: Unsorted → Ascending → Descending → (Unsorted if `AllowUnsort = true`, else stays Descending).
2. **Multi-sort**: Shift/Ctrl+Click appends column to sort set when `AllowMultiSorting = true`; priority badge shows sort order.
3. **Grouped columns preserved**: `IsFromGroup = true` entries always precede user sorts; `ClearSortingAsync()` skips them.
4. **Selection cleared**: Unless `PersistSelection = true`, selection is wiped after sort.
5. **Event pipeline**: `OnActionBegin` → `Sorting` (cancellable) → execute → `Sorted` → `OnActionComplete`.
6. **Header-only re-render**: `RefreshColumnHeader = true` triggers `GridHeaderCell` re-render (not full grid) for icon updates.

**Cross-Feature Rules** (from Training + Interaction Matrix)  
- **Grouping**: Grouped columns always appear first in `SortSettings.Columns`; user cannot remove them via `ClearSortingAsync`.
- **Filtering**: Sort operates on filtered dataset; `DataGenerator` composes both `FilterQuery` and `SortQuery`.
- **Virtualization**: Sort invalidates virtual cache via `ClearCacheData()`; scroll position resets to top; `VirtualComponentUpdate` fires.
- **Editing**: Active edit form discards uncommitted changes on sort (Blazor re-render clears transient state).
- **Selection**: Cleared unless `PersistSelection = true` (documented behaviour).
- **Paging**: Sort applies to full dataset; current page index stays unless filtered row count changes.

**API Contracts**  
- **`SortColumnAsync(columnName, direction, isMultiSort?)`**: Single column sort; `isMultiSort = true` appends, `false` replaces.
- **`SortColumnsAsync(columns, clearPreviousSort)`**: Batch sort; `clearPreviousSort = true` removes existing non-grouped sorts first.
- **`ClearSortingAsync()`**: Removes all non-grouped sort columns; triggers single data refresh.
- **`ClearSortingAsync(fieldNames)`**: Removes only specified fields from sort set.

**Event Payloads**  
- **`SortingEventArgs`**: `ColumnName`, `Direction`, `Action` (Add/Replace/Remove/Reset), `Cancel` (settable), `IsCtrlKeyPressed`.
- **`SortedEventArgs`**: `ColumnName`, `Direction`, `Action`, `SortedColumns` (populated by `SortColumnsAsync`).

**Edge Cases**  
- Empty dataset → sort completes normally; no error.
- Remote data with multi-sort → all `$orderby` / `sortBy` parameters sent in single request.
- `EnablePersistence = true` + stale field → persisted field not in current columns → silently ignored.
- `AllowUnsort = false` + Descending → cycles back to Ascending (never removes sort).
- Cancellation via `Sorting.Cancel = true` → rollback `UpdateModel` mutation; prevent data fetch.
- Template/command columns → `AllowSorting` auto-set to `false` (no `Field` binding).

**Before Making Changes**  
1. Read `/docs/training/` — understand event pipeline, parameter change detection, and cross-feature rules.
2. Verify guard clauses: `AllowSorting`, `GridColumn.AllowSorting`, null checks before mutation.
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required).
4. Preserve grouped columns (`IsFromGroup = true`) in all clear operations.
5. Test with grouping, filtering, virtualization, editing, frozen columns enabled.

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public methods/properties have XML doc comments with `<summary>`, `<param>`, `<returns>`, `<remarks>`.
- Event pipeline complete: `OnActionBegin` → `Sorting` → execute → `Sorted` → `OnActionComplete`.
- Selection cleared unless `PersistSelection = true`.
- Virtual cache invalidated via `ClearCacheData()` when virtualization enabled.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.
