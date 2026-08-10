---
name: paging-skill
description: Expert knowledge for the Paging feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to paging behaviour, including page navigation, page size dropdown, pager templates, query string synchronization, cross-feature interaction guarantees with sorting, filtering, grouping, virtualization, infinite scrolling, and state persistence.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Paging in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing paging features, debugging page navigation issues, adding pager templates, or fixing regressions involving paging with sorting, filtering, grouping, virtualization, infinite scrolling, editing, or state persistence.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern
- `/openspec/openspec/specs/paging/spec.md` — Complete paging specification
- `/docs/architecture/system-architecture.md` — DataGenerator<T> query building, lifecycle, PropertyChanges tracking
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# version constraints, async/await rules
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, guard clauses, event pipeline

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — paging must reset to page 1 on filter/search/group changes, compose with sort/filter/group queries, respect lazy-load grouping
- **Module communication via EventAggregator** — `ModelChanged(RequestType = Paging)` triggers cross-module notifications
- **PropertyChanges dictionary drives incremental updates** — `AllowPaging` / `PageSettings` change triggers pager component re-render
- **Action event pipeline mandatory** — `OnActionBegin` → user cancellation check → execute → `OnActionComplete`; cancellable via `args.Cancel`
- **Guard clauses first** — validate `AllowPaging` before any navigation operation
- **Single source of truth** — `GridPageSettings` (CurrentPage, PageSize) is canonical paging state; `DataGenerator` reads this to build `Page()` query clause

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`src/Internal/Actions/Paging.cs`** — Dedicated `Paging<T>` module handling: `UpdatePageSizes()` (syncs pager dropdown with filtered data), `UpdateFilterMessage()` (filter bar status in pager), `UpdateColumnFormat()` helper, page index calculations, pager UI synchronization (213 lines)
- **`GridPageSettings.razor.cs`** — Paging settings component: `CurrentPage`, `PageSize`, `PageCount`, `PageSizes`, `EnableQueryString`, `Template`, `ExternalMessage`
- **`SfGrid.Properties.cs`** — Public API: `AllowPaging` (bool, default false), `PageSettings` (GridPageSettings?)
- **`SfGrid.Methods.cs`** — Public method: `GoToPageAsync(int pageNo)` (line 1648)
- **`SfGrid.razor.cs`** — `TotalItemCount` (line 381), `PagerRef` (line 241), `PageValueChanged()` (line 1858, handles page navigation events), out-of-range page correction, pager event handling; instantiates `PagingModule`
- **`Internal/Actions/Data.cs` (DataGenerator<T>)** — `PageQuery()` appends `query.Page(CurrentPage, PageSize)` to data query
- **`SfGrid.Lifecycle.cs`** — Registers `PageSettings` in `OnInitializedAsync`, synchronizes in `OnParametersSetAsync`; instantiates `Paging<T>` module
- **`Syncfusion.Blazor.Navigations.SfPager`** — External pager component rendered at grid footer (referenced via `PagerRef`)

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Paging + Filtering | Filter applies to full dataset; pager displays filtered row count; `CurrentPage` resets to 1 on filter change | Critical |
| Paging + Sorting | Sort applies to full dataset; pager displays sorted result; `CurrentPage` preserved unless row count changes | High |
| Paging + Grouping | `GroupSettings.EnableLazyLoading = true` suppresses `PageQuery`; pager shows grouped data page count; `CurrentPage` resets to 1 on group change | Critical |
| Paging + Virtualization | Mutually exclusive; virtualization replaces `PageQuery` with `Range` query; pager hidden when `EnableVirtualization = true` | Critical |
| Paging + Infinite Scrolling | Mutually exclusive; infinite scrolling replaces `PageQuery` with `IntialInfinitePageQuery`; pager hidden when `EnableInfiniteScrolling = true` | Critical |
| Paging + Editing | Page navigation during active edit discards uncommitted changes (Blazor re-render clears transient state) | Medium |
| Paging + Selection | Selection cleared after page navigation unless `GridSelectionSettings.PersistSelection = true` | High |
| Paging + State Persistence | `EnablePersistence = true` restores `CurrentPage` and `PageSize` from `localStorage["grid{ID}"]`; out-of-range page auto-corrected | High |
| Paging + Export | Export applies current page only if `ExportType = CurrentPage`; otherwise exports full dataset | Medium |
| Paging + Searching | Search applies to full dataset; pager displays search result count; `CurrentPage` resets to 1 on search | High |

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**Purpose**: Focus management for pager navigation elements after page changes.

**TypeScript Functions** (`scripts/sf-grid.ts`):
- **`CurrentPageFocus(dataId: string, key: string, currentPage: string)`** (line 798) — Sets focus on the current page numeric button after navigation; determines whether to focus last numeric item or trigger re-render based on key and currentPage position
- **`pagerFocus(dataId: string, key: string)`** (line 808) — Manages focus on pager container elements (first/prev/next/last page icons) based on keyboard navigation key

**C# → JS Call Flow**:
- Grid keyboard navigation → invokes `CurrentPageFocus` or `pagerFocus` via `InvokeMethod` → TypeScript updates DOM focus

**Responsibility Split**:
- **C# (Blazor)**: Page navigation logic, data refresh, state updates
- **JS (TypeScript)**: DOM focus manipulation, keyboard navigation enhancement

**CRITICAL Rules**:
1. TypeScript functions never modify grid state—focus only
2. All navigation state changes happen in C# (`PageValueChanged`, `GoToPageAsync`)
3. Focus functions called only after successful page navigation
4. Functions handle null checks for pager elements before focus

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Paging  

**Context**  
You are implementing or fixing Paging for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Paging divides data into discrete pages, renders a `SfPager` component at the grid footer for navigation, and integrates with sorting, filtering, grouping, and persistence. All paging logic executes server-side in .NET via `DataGenerator<T>`.

**Core Architecture**  
- **Settings Component**: `GridPageSettings` in `GridPageSettings.razor.cs`
- **State**: `GridPageSettings.CurrentPage`, `PageSize`, `PageCount`, `PageSizes`, `Template`, `EnableQueryString`
- **Query builder**: `DataGenerator<T>.PageQuery()` appends `query.Page(CurrentPage, PageSize)` to data query
- **Pager UI**: `Syncfusion.Blazor.Navigations.SfPager` component rendered at grid footer (reference via `PagerRef`)
- **Public API**: `AllowPaging` (master switch), `PageSettings` (settings object), `GoToPageAsync(pageNo)` (programmatic navigation)

**Key Behaviors**  
1. **Master switch**: `AllowPaging = true` renders pager; `false` hides pager and disables page query.
2. **Default settings**: `CurrentPage = 1`, `PageSize = 12`, `PageCount = 8` (numeric buttons), `PageSizes = null`.
3. **Auto-correction**: `PageSize = 0` resets to `12`; `CurrentPage` exceeding max page resets to last valid page.
4. **Page navigation**: User clicks page number → `PageValueChanged()` → `CurrentPage` updated → `ModelChanged(RequestType = Paging)` → data reload.
5. **Programmatic navigation**: `GoToPageAsync(pageNo)` updates `CurrentPage` and triggers data refresh; no-op if `AllowPaging = false`.
6. **Page size dropdown**: `PageSizes = new int[]{ 10, 20, 30 }` renders dropdown; changing size resets `CurrentPage` to 1.
7. **"All" option**: `PageSizes = true` enables "All" option; selecting it renders full dataset on single page.
8. **Event pipeline**: `OnActionBegin(Paging)` → execute → `OnActionComplete(Paging)`; cancellable via `args.Cancel`.
9. **Query string sync**: `EnableQueryString = true` appends `CurrentPage` to browser URL as query parameter.

**Cross-Feature Rules** (from Training + Interaction Matrix)  
- **Filtering**: Filter → page count recalculated; `CurrentPage` resets to 1; `FilterQuery` composed before `PageQuery` in `DataGenerator`.
- **Sorting**: Sort → pager displays sorted result; `CurrentPage` preserved unless filtered count changes; `SortQuery` composed before `PageQuery`.
- **Grouping**: `EnableLazyLoading = true` → `PageQuery` suppressed; pager shows grouped page count; `CurrentPage` resets to 1 on group/ungroup.
- **Virtualization**: Mutually exclusive; `EnableVirtualization = true` → pager hidden; `Range` query replaces `PageQuery`.
- **Infinite Scrolling**: Mutually exclusive; `EnableInfiniteScrolling = true` → pager hidden; `IntialInfinitePageQuery` replaces `PageQuery`.
- **Editing**: Page navigation discards active edit uncommitted changes (Blazor re-render clears transient state).
- **Selection**: Selection cleared after page navigation unless `PersistSelection = true`.
- **State Persistence**: `EnablePersistence = true` → `CurrentPage` and `PageSize` restored from `localStorage["grid{ID}"]`; out-of-range page auto-corrected to last valid page.
- **Export**: Export current page only if `ExportType = CurrentPage`; otherwise exports full dataset.
- **Searching**: Search → `CurrentPage` resets to 1; pager shows search result count.

**API Contracts**  
- **`GoToPageAsync(int pageNo)`**: Navigate to specified page; no-op if `AllowPaging = false`; no exception thrown.
- **`GridPageSettings.CurrentPage`**: Gets/sets current page (1-based index).
- **`GridPageSettings.PageSize`**: Gets/sets rows per page; triggers data reload; resets `CurrentPage` to 1.
- **`GridPageSettings.PageCount`**: Gets/sets number of numeric page buttons in pager.
- **`GridPageSettings.PageSizes`**: Gets/sets dropdown options (`int[]`, `bool` for "All", or `null` to hide).
- **`GridPageSettings.Template`**: Gets/sets custom pager template (`RenderFragment<PagerTemplateContext>`); replaces default pager UI.
- **`GridPageSettings.EnableQueryString`**: Gets/sets URL query string synchronization flag.
- **`SfGrid.TotalItemCount`**: Gets total row count (after filter/search/group applied).

**Event Payloads**  
- **`OnActionBegin`**: `RequestType = Action.Paging`, `CurrentPage`, `PreviousPage`, cancellable via `args.Cancel`.
- **`OnActionComplete`**: `RequestType = Action.Paging`, `CurrentPage`, `PreviousPage`.

**Edge Cases**  
- `AllowPaging = false` + `GoToPageAsync` call → no-op; no exception.
- `PageSize = 0` → auto-corrected to `12`.
- `CurrentPage` exceeds max page → auto-corrected to last valid page.
- `EnablePersistence = true` + persisted `CurrentPage` out of range → corrected on restore.
- `PageSizes = true` + "All" selected → full dataset rendered; `PageSize` set to `TotalItemCount`.
- Page navigation cancelled via `OnActionBegin.Cancel = true` → `CurrentPage` unchanged; no data reload.
- Filter/Search/Group applied on page > 1 → `CurrentPage` resets to 1 before data fetch.
- `GroupSettings.EnableLazyLoading = true` → `PageQuery` suppressed; grouped data paging handled separately.
- Pager template provided → default pager UI replaced; template receives `PagerTemplateContext` with `CurrentPage`, `TotalPages`, `TotalItemsCount`.
- `ExternalMessage` (filter summary) displayed in pager when `ShowFilterBarStatus = true` and filter active.

**Before Making Changes**  
1. Read `/docs/training/` — understand query composition order, page reset rules, and cross-feature interaction.
2. Verify guard clauses: `AllowPaging`, null checks before navigation.
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required).
4. Test with filtering, sorting, grouping, virtualization, infinite scrolling, editing enabled.
5. Verify `CurrentPage` resets to 1 on filter/search/group changes.
6. Validate mutual exclusivity with virtualization and infinite scrolling.

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public methods/properties have XML doc comments with `<summary>`, `<param>`, `<returns>`, `<remarks>`.
- Event pipeline complete: `OnActionBegin` → execute → `OnActionComplete`.
- `CurrentPage` reset logic on filter/search/group changes.
- Pager hidden when `EnableVirtualization = true` or `EnableInfiniteScrolling = true`.
- Selection cleared after page navigation unless `PersistSelection = true`.
- Out-of-range page auto-correction on persistence restore.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.
