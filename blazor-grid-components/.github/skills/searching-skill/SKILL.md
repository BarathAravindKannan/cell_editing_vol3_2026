---
name: searching-skill
description: Expert knowledge for the Searching feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to toolbar search behaviour, query generation, DOM rendering, event flow, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Searching in the Syncfusion Blazor DataGrid. Guarantees no breakage with paging, virtualization, grouping, foreign key, adaptive toolbar, persistence, and selection.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to toolbar search UI, `GridSearchSettings`, `SearchAsync(string)`, `AllowSearching`, search predicate generation, search-related rendering, or search event flow.
- Do NOT use this skill for checkbox/excel filter popup search unless the bug explicitly overlaps toolbar search and filter search plumbing.
- One agent invocation = one feature skill maximum.
- Always finish with regression verification using `docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## Knowledge References
<!-- token-budget: 60 words -->

All content derived from reading these files — do NOT reproduce their content here:

- `docs/ai-agents/skills/_skill-template-standard.md`
- `docs/training/00-START-HERE.md`
- `docs/training/README.md`
- `docs/training/TRAINING-INDEX.md`
- `docs/training/DELIVERY-SUMMARY.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/01-getting-started/project-setup-guide.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/training/04-code-processing/optimal-chunking-strategies.md`
- `docs/training/05-practical-examples/feature-implementation-walkthrough.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/architecture/dependency-map.md`
- `docs/architecture/data-flow.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`
- `docs/code-guidelines/logging-guidelines.md`
- `docs/overview/product-overview.md`
- `docs/overview/features-extracted.md`
- `docs/overview/glossary.md`
- `src/GridSearchSettings.cs`
- `src/SfGrid.Properties.cs`
- `src/SfGrid.Methods.cs`
- `src/SfGrid.razor.cs`
- `src/GridColumn.cs`
- `src/GridEvents.cs`
- `src/EventModels/Grids.cs`
- `src/Interfaces/IGrid.cs`
- `src/Enumeration/GridsEnumerations.cs`
- `src/Internal/Actions/Searching.cs`
- `src/Internal/Actions/Data.cs`
- `src/Internal/Actions/Filter.cs`
- `src/Internal/Actions/Group.cs`
- `src/Internal/Renderer/GridToolbar.razor`
- `src/Internal/Base/InternalClass.cs`
- `scripts/sf-grid.ts`
- `scripts/sf-grid-fn.ts`
- `tests/Actions/Searching.razor`
- `tests/Actions/Toolbar.razor`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Searching has a lightweight standalone action module**: `src/Internal/Actions/Searching.cs` (`Searching<T>`, lines 18-199, incl. `CancelIconClickSearch()` at 186-194) handles the toolbar search-box interactions. The feature is otherwise state-light and query-driven — runtime behavior is defined by `GridSearchSettings`, `SfGrid.SearchAsync(string)`, `GridToolbar.razor`, and `DataGenerator<T>.SearchQuery()`.
- **Toolbar search is the only primary UI surface**: do not model search like FilterBar or Excel filter search. Toolbar rendering, adaptive search expansion, and icon state are the actual presentation contract.
- **`SearchSettings.Key` is the source of truth**: rendering, persistence, query generation, clear-icon visibility, and post-action state all derive from the current key.
- **Column inclusion rules are strict**: searchable fields come from `SearchSettings.Fields` or from visible grid columns where `AllowSearching = true` and `Field` is not empty; grouped hidden columns remain eligible.
- **Paging reset is mandatory**: after search execution, pager state must be normalized through `FilterModule.UpdatePageSizes()` so search results always start from the first page.
- **Virtualization guard is intentional**: when `SearchSettings.Key` has content, virtual render optimizations are bypassed so stale cache/viewport slices are not reused.
- **Selection and filtered-record behavior are linked**: `PersistSelection` flows can depend on search being treated like a filtered dataset; do not skip filtered-record requirements during search actions.
- **JS key handling is regression-sensitive**: `sf-grid-fn.ts` marks Enter inside `.e-searchinput` with `isSearchInput` so focus/navigation logic does not hijack toolbar search input.
- **Clear behavior is split between C# and JS**: C# clears logical search state through `SearchAsync("")`; JS `sfBlazor.Grid.searchClear(...)` clears and refocuses the DOM input.
- **Adaptive toolbar search is a separate render branch**: `IsSearchClicked` changes the toolbar DOM shape in mobile/vertical mode; desktop and adaptive search rendering must be reasoned about independently.
- **ODataV4 search path is type-aware**: typed `WhereFilter` generation is not interchangeable with the default `query.Search(...)` branch.
- **Column chooser search is not toolbar search**: `CCSearchBox` and toolbar search box have different IDs, handlers, and semantics; do not merge or generalize their DOM/query behavior.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `src/GridSearchSettings.cs` | Public search configuration component — `Key`, `Fields`, `Operator`, `IgnoreCase`, `IgnoreAccent`, and internal key update helper |
| `src/SfGrid.Properties.cs` | Declares `SearchSettings` as grid-level state and persistence surface |
| `src/SfGrid.Methods.cs` | Public `SearchAsync(string)` API at line 3593 that mutates search state and starts the action pipeline |
| `src/SfGrid.razor.cs` | Search clear icon state, persistence restore, event aggregation, virtualization guard, and filtered-record interaction |
| `src/GridColumn.cs` | Column-level `AllowSearching` gate for field eligibility |
| `src/GridEvents.cs` | Search lifecycle events: `Searching` and `Searched` |
| `src/EventModels/Grids.cs` | `SearchingEventArgs`, `SearchedEventArgs`, and action payload contracts |
| `src/Interfaces/IGrid.cs` | Interface contract exposing `SearchSettings` |
| `src/Enumeration/GridsEnumerations.cs` | `Action.Searching` and related action enum values used by pipeline dispatch |
| `src/Internal/Actions/Searching.cs` | `Searching<T>` module — manages search query operations, foreign key search handling, and predicate generation (lines 18-199, class definition); key method `CancelIconClickSearch()` at lines 186-194 |
| `src/Internal/Actions/Data.cs` | `DataGenerator<T>.SearchQuery()` at line 72 — the core predicate generation path for local, remote, OData, and foreign key search |
| `src/Internal/Actions/Filter.cs` | Pager normalization after search through `PageModule.UpdatePageSizes()` at lines 133, 333 |
| `src/Internal/Actions/Group.cs` | Group lazy-load path that composes search predicates with grouping logic |
| `src/Internal/Renderer/GridToolbar.razor` | All toolbar search UI rendering, input handlers, icon handlers, adaptive mobile branch, placeholder/title wiring |
| `src/Internal/Base/InternalClass.cs` | Search-related locale keys and toolbar support constants |
| `scripts/sf-grid.ts` | `searchClear(...)` DOM helper to clear and refocus the toolbar search input |
| `scripts/sf-grid-fn.ts` | Keyboard/focus coordination for `.e-searchinput`, including `isSearchInput` guard flags |
| `tests/Actions/Searching.razor` | Primary baseline tests for search UI, search action, and search + paging behavior |
| `tests/Actions/Toolbar.razor` | Toolbar integration coverage including the search item and toolbar rendering interactions |

### Rendering Surface Map

| Surface | Owner | Notes |
|--------|-------|-------|
| Public configuration | `GridSearchSettings` | Declarative state or initial key/field/operator selection |
| Public API entry | `SfGrid.SearchAsync(string)` | Programmatic search or clear operation |
| Primary UI | `GridToolbar.razor` | Search box, search icon, clear icon, adaptive toggle path |
| Query composition | `DataGenerator<T>.SearchQuery()` | Injects search predicates into the master query pipeline |
| Event pipeline | `GridEvents.cs`, `EventModels/Grids.cs` | Cancellable pre-event and post-completion event |
| DOM cleanup / focus | `sf-grid.ts`, `sf-grid-fn.ts` | Clear input and prevent keyboard navigation conflicts |

### Rendering Approach

Searching is rendered as a **toolbar-owned interaction model**, not as a body-level grid row/cell feature. The grid body does not directly render search UI; it reacts to the changed `SearchSettings.Key` by rebuilding the query and re-rendering result rows.

Rendering splits into two main branches:

1. **Desktop / standard toolbar rendering**
   - Search input is always part of toolbar item collection.
   - Input value mirrors `SearchSettings.Key`.
   - Clear icon visibility is controlled by grid state (`SearchClearIcon`).
   - Search executes on Enter, icon click, or programmatic API invocation.

2. **Adaptive / vertical toolbar rendering**
   - Toolbar initially shows a search wrapper/icon entry.
   - User interaction flips `IsSearchClicked = true`.
   - Toolbar re-renders into an expanded search-input layout with back navigation.
   - Clearing or backing out can collapse the adaptive search UI without changing unrelated toolbar items.

### DOM Rendering Structure

#### Standard Toolbar Search DOM

```text
SfGrid<TValue>
└─ Toolbar container (.e-toolbar)
   ├─ Left toolbar items ...
   ├─ Spacer/template slot ...
   └─ Search item (#<GridId>_search)
      └─ div.e-toolbar-search
         └─ span.e-input-group.e-control-wrapper[.e-input-focus]
            ├─ input.e-searchinput.e-input
            │  ├─ type="search"
            │  ├─ id="<GridId>_ToolbarSearchBox"
            │  ├─ value = SearchSettings.Key
            │  └─ placeholder = localized Search text
            ├─ span.e-clear-icon[conditional]
            │  └─ visible when search text is non-empty
            └─ span.e-searchclear.e-search-icon.e-input-group-icon
               └─ click triggers search execution
```

#### Adaptive Toolbar Search DOM

```text
SfGrid<TValue>
└─ Toolbar container (.e-toolbar)
   ├─ Default toolbar item set
   │  └─ span/div.e-search-wrapper
   │     └─ search icon button
   └─ When IsSearchClicked = true
      └─ Adaptive search layout
         ├─ back icon / navigation affordance
         ├─ input.e-searchinput.e-input
         ├─ clear icon
         └─ search icon / submit trigger
```

#### Query-Driven Re-render Flow

```text
Toolbar search input change
        │
        ├─ local toolbar handler updates transient input state
        │
        ├─ Enter / icon click / API call
        ▼
SfGrid.SearchAsync(searchText)
        ▼
SearchSettings.Key updated
        ▼
ModelChanged(RequestType = Action.Searching)
        ▼
Searching event (cancel possible)
        ▼
DataGenerator.GenerateQuery()
        ├─ FilterQuery()
        ├─ SearchQuery()
        ├─ AggregateQuery()
        ├─ SortQuery()
        ├─ PageQuery()
        └─ GroupQuery()
        ▼
Grid data refreshed
        ▼
Rows / content area re-rendered
        ▼
Searched event fired
        ▼
Pager normalized + clear icon state updated
```

### Search Rendering Flow by Trigger

#### 1. Keyboard Enter flow

```text
User types in .e-searchinput
        ▼
GridToolbar.InputArgs() stores current input text
        ▼
GridToolbar.KeyDownHandler(Enter)
        ▼
SfGrid.SearchAsync(currentText)
        ▼
Action.Searching pipeline executes
        ▼
Grid content refreshes with searched dataset
```

#### 2. Search icon click flow

```text
User clicks .e-search-icon
        ▼
GridToolbar.SearchText()
        ▼
SfGrid.SearchAsync(currentText)
        ▼
Search query generated and grid re-rendered
```

#### 3. Clear icon flow

```text
User clicks .e-clear-icon
        ▼
GridToolbar.CancelIconClick()
        ▼
SfGrid.SearchAsync("")
        ▼
Query rebuilt without search predicates
        ▼
sfBlazor.Grid.searchClear(dataId, inputId)
        ▼
DOM input cleared + focus restored
```

#### 4. Programmatic API flow

```text
Developer calls grid.SearchAsync("abc")
        ▼
SearchSettings.Key becomes "abc"
        ▼
Same event + query + render pipeline as UI search
```

### DOM Ownership Rules

- Toolbar markup owns the search input DOM.
- Grid state owns the current search key and clear-icon visibility.
- Query generation owns the searchable field list and predicate construction.
- JS owns only the final DOM clear/focus assistance and keyboard navigation guard coordination.
- Grid body rows are downstream render results, not the owner of search state.

---

## JavaScript Interop
<!-- token-budget: 100 words -->

Searching uses minimal JavaScript interop for DOM manipulation and keyboard navigation coordination.

### C# → JavaScript Interop Calls

| C# Location | JS Function | Purpose | Parameters |
|-------------|-------------|---------|------------|
| `Searching.cs` line 190 | `sfBlazor.Grid.searchClear` | Clear input value and restore focus after search clear | `dataId`, `inputId` (ToolbarSearchBox) |

### TypeScript Functions

| Function | File | Line | Responsibility |
|----------|------|------|----------------|
| `searchClear(dataId, inputId, inputValue)` | `sf-grid.ts` | 84 | Set input element value to empty string and restore focus to search input |
| `isSearchInput` flag logic | `sf-grid-fn.ts` | 1459-1460 | Prevent Enter key inside `.e-searchinput` from triggering grid cell navigation |

### Responsibility Split

| Layer | Owns | Does NOT Own |
|-------|------|--------------|
| **C# (Searching.cs, SfGrid.Methods.cs)** | Search state (`SearchSettings.Key`), query predicate generation, event firing (Searching/Searched), pager normalization | DOM input manipulation, focus management |
| **C# (GridToolbar.razor)** | Search input rendering, input change handlers, clear/search icon click routing | Final DOM cleanup after clear |
| **JS (sf-grid.ts)** | Input element value reset, focus restoration after clear | Search state, query logic, event flow |
| **JS (sf-grid-fn.ts)** | Keyboard navigation guards (`isSearchInput` flag to prevent cell nav) | Search execution, toolbar rendering |

### CRITICAL Rules

1. **NEVER** call `searchClear()` from C# without first updating `SearchSettings.Key` through `SearchAsync("")`.
2. **NEVER** merge toolbar search input keyboard handling with filter popup search or column chooser search — they have different DOM IDs and event flows.
3. **ALWAYS** preserve the `isSearchInput` flag check in `sf-grid-fn.ts` to prevent Enter key from becoming cell navigation when user is in search input.
4. **NEVER** manipulate search input DOM directly from C# — use JS interop for focus/value changes after state updates.

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live docs, code, and tests.  
> Omitted pairs have no meaningful interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Searching + Paging | Search must rebuild the full dataset query first and then normalize pager state through `FilterModule.UpdatePageSizes()` so result navigation starts from page 1. | Critical |
| Searching + Virtualization | A non-empty `SearchSettings.Key` must invalidate or bypass virtual diff/cache assumptions so the viewport does not reuse stale rows. | Critical |
| Searching + Grouping | Group lazy-load and grouped-column search eligibility must remain intact; grouped hidden columns still participate in search field resolution. | High |
| Searching + ForeignKey | Search predicate generation must preserve separate foreign-key handling and respect per-column `AllowSearching`. | High |
| Searching + PersistSelection | Search must continue to behave like filtered data so persisted row selection can request filtered records correctly. | High |
| Searching + EnablePersistence | `searchSettings` serialization/deserialization must restore `Key` and `Fields` without skipping the normal search-state contract. | Medium |
| Searching + Adaptive Toolbar | `IsSearchClicked` must continue to switch toolbar DOM shape correctly between icon mode and expanded input mode. | Medium |
| Searching + Editing | Search must not break edit-state guards or search-input keyboard handling while inline/add-new-row UI is active. | Medium |
| Searching + Column Show/Hide / Reorder | Auto-resolved searchable fields must continue to honor visibility and `AllowSearching`, except for grouped hidden columns. | Medium |
| Searching + RTL | Search markup and icon rendering must remain inside the same toolbar structure and respect RTL classes without separate logic forks. | Low |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```text
Mode: {feature-implementation | bug-fix}
Skill: Searching
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

Load this only for toolbar search UI, GridSearchSettings behavior, search event flow, search DOM rendering, SearchAsync(string), or DataGenerator<T>.SearchQuery() work.

Before changing code, consult:
- docs/training/02-requirements-analysis/understanding-requirements.md
- docs/training/06-reference/quick-reference-guides.md

Known rules:
- Core files: src/GridSearchSettings.cs, src/SfGrid.Methods.cs, src/SfGrid.razor.cs, src/Internal/Actions/Searching.cs, src/Internal/Renderer/GridToolbar.razor, src/Internal/Actions/Data.cs
- Public API entry is SearchAsync(string)
- Search state source of truth is SearchSettings.Key
- Column search eligibility comes from SearchSettings.Fields or visible columns where AllowSearching = true and Field is non-empty
- Grouped hidden columns remain searchable
- Clear search path is SearchAsync("") plus JS helper sfBlazor.Grid.searchClear(...)
- Query order is Filter → Search → Aggregate → Sort → Page → Group inside DataGenerator.GenerateQuery()
- GridEvents exposes Searching (cancellable) and Searched (post-action)
- Adaptive UI uses IsSearchClicked to switch toolbar DOM shape
- JS key handling uses isSearchInput to prevent Enter in toolbar search from becoming grid cell navigation
- ODataV4 path uses typed WhereFilter generation and must stay separate from default query.Search(...) logic
- CCSearchBox is for column chooser search, not toolbar search

Rendering model:
- Search UI is toolbar-owned, not row-owned
- Toolbar input updates trigger search pipeline; body rows are downstream re-render results
- Clear icon visibility is state-driven
- Search icon, Enter key, and API call all converge on the same SearchAsync path

ASCII DOM map:
SfGrid
└─ Toolbar
   └─ Search item
      └─ .e-toolbar-search
         └─ .e-input-group
            ├─ input.e-searchinput
            ├─ span.e-clear-icon
            └─ span.e-search-icon

ASCII action flow:
Input/Click/API
  → SearchAsync(text)
  → ModelChanged(Action.Searching)
  → Searching event
  → DataGenerator.SearchQuery()
  → grid data refresh
  → Searched event
  → pager normalize / DOM clear-state update

Constraints:
- No API changes unless explicitly requested
- No direct StateHasChanged() forcing for search unless required by existing patterns
- Preserve guard clauses, ConfigureAwait(true), and XML docs on public touches
- Do not break paging, virtualization, grouping, foreign key, persistence, selection, or adaptive toolbar behavior listed above
- Do not merge toolbar search with column chooser search or filter-popup search

Output:
1. Root cause / design note
2. Modified method(s) only
3. Rendering branch affected (desktop, adaptive, JS, query, or event pipeline)
4. Interaction risks touched
5. Required regression tests

After implementation, run:
docs/ai-agents/prompts/regression-verification-prompt.md
```
