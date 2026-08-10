---
name: filtering-skill
description: Expert knowledge for the Filtering feature (FilterBar and FilterMenu) in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to filtering behaviour, including inline filter bar, column-level filter dialogs, operator-driven predicates, type-specific value inputs, cross-feature interaction guarantees with sorting, grouping, virtualization, editing, paging, and selection.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Filtering (FilterBar & FilterMenu) in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing filter features (FilterBar, FilterMenu, Excel, CheckBox), debugging filter predicate issues, adding operator logic, or fixing regressions involving filtering with sorting, grouping, virtualization, editing, frozen columns, paging, or selection.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern
- `/openspec/specs/filtering/filterbar/filterbar-spec.md` — Complete FilterBar specification
- `/openspec/specs/filtermenu/spec.md` — Complete FilterMenu specification
- `/docs/architecture/system-architecture.md` — Filter<T> module lifecycle, event flow, PropertyChanges tracking
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# version constraints, async/await rules, timer patterns
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, guard clauses, event pipeline

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — filter must clear non-persistent selection, compose with sort/group queries, invalidate virtual cache
- **Module communication via EventAggregator** — `ModelChanged(RequestType = Filtering/ClearFiltering)` triggers cross-module notifications
- **PropertyChanges dictionary drives incremental updates** — `AllowFiltering` / `FilterSettings` change triggers header re-render
- **Action event pipeline mandatory** — `OnActionBegin` → user cancellation check → execute → `OnActionComplete`; cancellable via `args.Cancel`
- **Guard clauses first** — validate `AllowFiltering`, `GridColumn.AllowFiltering` before any state mutation
- **Single source of truth** — `FilterSettings.Columns` (List<GridFilterColumn>) is canonical filter state; `Filter<T>` reads/mutates directly
- **Timer cleanup required** — FilterBar `Immediate` mode uses `System.Timers.Timer`; must call `StopTimer()` before disposal

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`Internal/Actions/Filter.cs`** — Core filtering engine: `FilterByColumn()` (line 76), `RemoveFilterColumnByField()` (line 287), `UpdateFilterMessage()` calls (lines 128, 314), predicate building
- **`Internal/Renderer/Filter/FilterBarRenderer.razor`** — Inline filter row beneath headers (FilterType.FilterBar)
- **`Internal/Renderer/Filter/FilterInput.razor`** — Per-column input cell; debounce timer, operator parsing (line 640), clear-icon handling, JS interop calls (lines 576, 640, 771, 795, 820, 1085)
- **`Internal/Renderer/Filter/FilterMenuRenderer.razor`** — Per-column dialog popup (FilterType.Menu)
- **`SfGrid.Properties.cs`** — Public API: `AllowFiltering` property (line 82), `FilterSettings`
- **`GridFilterSettings.razor.cs`** — Child component: `Type`, `Mode`, `Columns`, `ImmediateModeDelay`, `ShowFilterBarStatus`
- **`GridFilterColumn.cs`** — Predicate model: `Field`, `Operator`, `Value`, `Predicate` (and/or), `MatchCase`, `IgnoreAccent`, `Uid`
- **`Internal/Actions/Data.cs` (DataGenerator<T>)** — Consumes `FilterSettings.Columns` to produce `Where()` query clauses (line 16)

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Filtering + Sorting | Filter applies to full dataset; sort operates on filtered result; both `Where()` and `SortBy()` composed in `DataGenerator` | Critical |
| Filtering + Grouping | Filter applies before grouping; grouped columns excluded from clear; `GroupedData` recalculated from filtered set | Critical |
| Filtering + Virtualization | Filter triggers `VirtualScroll<T>.ClearCacheData()`; scroll position resets to top; `VirtualComponentUpdate` after reload | Critical |
| Filtering + Paging | Filter applies to full dataset; pager displays filtered row count; current page preserved unless row count < page size | High |
| Filtering + Editing | Filter during active edit discards uncommitted changes (Blazor re-render clears transient state) | High |
| Filtering + Selection | Selection cleared after filter unless `GridSelectionSettings.PersistSelection = true` | High |
| Filtering + Frozen Columns | FilterBar splits cells into frozen-left, movable, frozen-right sections; FilterMenu dialog works on frozen columns | Medium |
| Filtering + Column Virtualization | FilterBar renders only cells in `[StartColumnIndex-1, EndColumnIndex+1]` window; FilterMenu unaffected | Medium |
| Filtering + Export | Export applies current filter predicates from `FilterSettings.Columns` to exported data | Medium |
| Filtering + Infinite Scroll | Filter resets scroll position; new filtered data fetched from top; previously loaded rows discarded | High |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Filtering  

**Context**  
You are implementing or fixing Filtering for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Filtering narrows displayed rows based on column predicates. Two UI modes: **FilterBar** (inline inputs) and **FilterMenu** (per-column dialog). All filter logic executes server-side in .NET via the `Filter<T>` action module.

**Core Architecture**  
- **Module**: `Filter<T>` in `Internal/Actions/Filter.cs`
- **State**: `FilterSettings.Columns` (List<GridFilterColumn>) — single source of truth
- **UI Modes**:
  - **FilterBar**: `FilterBarRenderer.razor` + `FilterInput.razor` — inline row beneath headers; `FilterType.FilterBar`
  - **FilterMenu**: `FilterMenuRenderer` — per-column dialog popup; `FilterType.Menu`
- **Query builder**: `DataGenerator<T>` consumes `FilterSettings.Columns` to produce `Where()` clauses
- **Public API**: `AllowFiltering`, `GridFilterSettings.Type`, `Mode`, `ImmediateModeDelay`, `ShowFilterBarStatus`, `GridColumn.AllowFiltering`

**Key Behaviors**  
1. **FilterBar — OnEnter mode**: User types → press Enter → filter applied.
2. **FilterBar — Immediate mode**: User types → debounce timer (`ImmediateModeDelay` ms) → filter applied.
3. **FilterBar — Clear**: Escape key or clear-icon click → predicate removed → data reloaded.
4. **FilterMenu — Dialog**: Filter icon click → operator dropdown + value input → "Filter" button → predicate applied; "Clear" button → predicate removed.
5. **Operator inference**: FilterBar auto-detects prefix operators (`*`, `%`, `>`, `<`, `=`, `!=`) from user input.
6. **One predicate per column**: New filter replaces existing predicate for same column (unless multi-value with `or` predicate).
7. **Selection cleared**: Unless `PersistSelection = true`, selection wiped after filter.
8. **Event pipeline**: `OnActionBegin(Filtering)` → execute → `OnActionComplete(Filtering)`; cancellable via `args.Cancel`.

**Cross-Feature Rules** (from Training + Interaction Matrix)  
- **Sorting**: Filter → Sort composed in `DataGenerator`; both queries applied to single data fetch.
- **Grouping**: Filter applies before grouping; grouped columns excluded from `ClearFiltering`; `GroupedData` recalculated.
- **Virtualization**: Filter invalidates virtual cache via `ClearCacheData()`; scroll resets to top; `VirtualComponentUpdate` fires.
- **Editing**: Active edit form discards uncommitted changes on filter (Blazor re-render clears transient state).
- **Selection**: Cleared unless `PersistSelection = true`.
- **Paging**: Filter applies to full dataset; pager shows filtered count; current page preserved unless filtered count < page size.
- **Frozen Columns**: FilterBar splits cells by frozen sections; FilterMenu dialog works on frozen columns.

**API Contracts**  
- **`FilterByColumn(field, operator, value, predicate, matchCase, ignoreAccent, uid, actualValue)`**: Apply filter predicate for a column.
- **`RemoveFilterColumnByField(field, uid, foreignKeyField)`**: Remove column's filter predicate.
- **`ClearFiltering(fields?)`**: Clear all filters or specified fields; triggers single data refresh.

**FilterBar Specific**  
- **Modes**: `OnEnter` (Enter key required) vs `Immediate` (auto-trigger after delay).
- **Timer management**: `System.Timers.Timer` for debounce; must call `StopTimer()` before disposal.
- **Operator parsing**: Prefix symbols (`*`, `%`, `>`, `<`, `=`, `!=`) auto-detected; default operator `startswith` (string) or `equal` (non-string).
- **Pager message**: `ShowFilterBarStatus = true` → `UpdateFilterMessage` sets pager `ExternalMessage` showing active predicates.
- **Clear icon**: Visible when input has value; click triggers `RemoveFilterColumnByField`.

**FilterMenu Specific**  
- **Dialog lifecycle**: Filter icon → `FilterDialogOpening` (cancellable) → `OnActionBegin(FilterBeforeOpen)` → `MenuDialog.ShowAsync()` → `OnActionComplete(FilterAfterOpen)` → `FilterDialogOpened`.
- **Operator list**: Type-specific defaults; overridable via `FilterSettings.Operators` or `FilterDialogOpeningEventArgs.FilterOperators`.
- **Value input**: Type-specific components (`SfAutoComplete`, `SfNumericTextBox`, `SfDatePicker`, `SfDateTimePicker`, `SfTimePicker`, `SfDropDownList<bool?>`).
- **Null operators**: `isnull` / `isnotnull` disable value input; no value required.
- **Foreign key columns**: Predicate `Field` = `ForeignKeyValue`; `SfAutoComplete` uses column's own `DataManager`.
- **Adaptive mode**: Mobile fullscreen dialog via `IsDeviceMode` + `AdaptiveUIMode`.

**Event Payloads**  
- **FilterBar**: `OnActionBegin` / `OnActionComplete` with `RequestType = Filtering` or `ClearFiltering`.
- **FilterMenu**: `FilterDialogOpening` (cancellable; `FilterOperators` mutable), `FilterDialogOpened`, `Filtering` (cancellable), `Filtered`.

**Edge Cases**  
- Empty `FilteredValue` → `RemoveFilterColumnByField` called (no empty-string predicate).
- `isnull` / `isnotnull` operator → value input disabled; `FilterByColumn` called with `null` value.
- No value entered (non-null operator) → "Filter" button no-op.
- `FilterTemplate` → replaces operator dropdown + value input; receives `PredicateModel<TValue>`.
- Foreign key column → predicate `Field` = `ForeignKeyValue`; `SfAutoComplete` uses column's `DataManager`.
- `EnablePersistence = true` → `FilterSettings.Columns` deserialized from `localStorage["grid{ID}"]` on init.
- Re-opening dialog with active filter → existing `GridFilterColumn` found by `Uid`; operator + value pre-populated.
- `FilterDialogOpening.Cancel = true` → dialog not shown; state returns Idle.
- Cancellation via `Filtering.Cancel = true` → predicate not applied; rollback model mutation.
- Column virtualization + FilterBar → only cells in `[StartColumnIndex-1, EndColumnIndex+1]` rendered.
- Frozen columns + FilterBar → splits into three `<tr>` sections (frozen-left, movable, frozen-right).
- Timer leak (FilterBar Immediate) → `StopTimer()` called before every `StartTimer()` and on disposal.

**Before Making Changes**  
1. Read `/docs/training/` — understand event pipeline, timer management, and cross-feature rules.
2. Verify guard clauses: `AllowFiltering`, `GridColumn.AllowFiltering`, null checks before mutation.
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required).
4. Test with sorting, grouping, virtualization, editing, frozen columns, paging enabled.
5. Verify timer cleanup in FilterBar `Immediate` mode (no memory leaks).

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public methods/properties have XML doc comments with `<summary>`, `<param>`, `<returns>`, `<remarks>`.
- Event pipeline complete: `OnActionBegin` → execute → `OnActionComplete`.
- FilterBar timer cleanup in `Dispose` / `DisposeAsync`.
- Selection cleared unless `PersistSelection = true`.
- Virtual cache invalidated via `ClearCacheData()` when virtualization enabled.
- Pager `ExternalMessage` updated when `ShowFilterBarStatus = true`.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Calls** (from `FilterInput.razor`, `ExcelBase.razor`)

| C# Method Call | TypeScript Function | Purpose |
|----------------|---------------------|---------|
| `InvokeMethod("sfBlazor.Grid.customFilterDialog")` | `customFilterDialog()` (line 54) | Style filter dialog for Excel/custom filter menus |
| `InvokeMethod("sfBlazor.Grid.setCustomFilterDialogPadding")` | `setCustomFilterDialogPadding()` (line 76) | Apply padding to custom filter dialog content |
| `InvokeMethod("sfBlazor.Grid.searchClear")` | `searchClear()` (line 84) | Clear filter input value via DOM manipulation |
| `InvokeMethod("sfBlazor.Grid.updateFilterBarCell")` | `updateFilterBarCell()` (line 478) | Update FilterBar cell values after filter applied |
| `InvokeMethod("sfBlazor.Grid.focusFilterBar")` | `focusFilterBar()` (line 489) | Focus FilterBar cell on Tab/Shift+Tab navigation |
| `InvokeMethod("sfBlazor.Grid.IsInputfocus")` | `IsInputfocus()` (line 1162) | Focus filter input after operator dropdown selection |
| `InvokeMethod("sfBlazor.Grid.getIsDeviceMode")` | `getIsDeviceMode()` (line 1184) | Detect mobile/adaptive mode for dialog rendering |

**TypeScript Source:** `scripts/sf-grid.ts`

**Responsibility Split**

| Layer | Responsibility |
|-------|----------------|
| **C# (Filter.cs, FilterInput.razor)** | Filter logic, predicate building, data operations, event pipeline, state management |
| **TypeScript (sf-grid.ts)** | DOM manipulation (focus, clear, style), dialog positioning, device detection, cell value updates |

**CRITICAL Rules**
1. **Never replicate filter logic in JavaScript** — all predicate building, operator parsing, value validation happens in C#.
2. **JavaScript is DOM-only** — TypeScript functions only manipulate existing DOM elements; never trigger data refresh or state changes.
3. **Async calls require ConfigureAwait(true)** — all `InvokeMethod` calls in Blazor context must preserve synchronization context.
4. **Device detection via JS** — `getIsDeviceMode()` reads `sf.base.Browser.isDevice`; C# cannot access browser APIs directly.
