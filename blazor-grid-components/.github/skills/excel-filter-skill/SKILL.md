# Excel Filter Skill
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Excel Filter in the Syncfusion Blazor Grid. Guarantees no breakage with standard filtering, sorting, grouping, paging, virtualization.

---

## Agent Invocation

```yaml
name: excel-filter-skill
description: Expert knowledge for the Excel Filter feature (checkbox-based distinct value selection with custom operator/value dialogs) in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to Excel filter behaviour, including checkbox list rendering, distinct value selection, in-popup search, custom filter submenu (Custom Filter/Between), filter dialog lifecycle events (FilterDialogOpening/FilterDialogOpened/CheckboxFilterSearching), JS interop for popup positioning/focus, cross-feature interaction guarantees with standard filtering, sorting, grouping, paging, virtualization, adaptive rendering.
```

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/openspec/openspec/specs/excelfilter/spec.md` (4,251 chars: Excel filter UI, checkbox distinct values, search, custom filter dialog, lifecycle events)
- `/openspec/openspec/specs/filtering/filterbar/filterbar-spec.md` + `/openspec/openspec/specs/filtermenu/spec.md` (standard filtering baseline)
- `/docs/training/01-getting-started/architecture-overview.md` (module injection, EventAggregator, 4-layer architecture)
- `/docs/architecture/system-architecture.md` (module lifecycle, PropertyChanges dictionary, JS interop)
- `/docs/code-guidelines/coding-standards.md` (async/await with ConfigureAwait, guard clauses, parameter patterns)
- `/docs/tech-stack/tech-stack.md` (Blazor patterns, C# 12/13/14, JS interop)

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- Excel filter is a UI variant of standard filtering (shares Filter<T> module, query composition, event pipeline)
- Distinct values limited to 1000 by default (requirement: Excel Distinct Choice Data); custom count via FilterChoiceCount override
- Cancellable opening via FilterDialogOpening / OnActionBegin (RequestType=FilterBeforeOpen, Cancel=true stops popup)
- Search triggers CheckboxFilterSearching event to update visible choices dynamically (requirement: Excel Search Behavior)
- Custom Filter/Between submenu generates 1-2 predicates with custom operators (requirement: Excel Custom Filter Dialog)
- JS interop hooks (filterPopupRender, customFilterDialog, focusExcelInput) handle popup placement and focus (requirement: JS Interop for Excel Popup UX)

---

## Code Location Map
<!-- token-budget: 80 words -->

- `GridFilterSettings.razor.cs` — Type property (enum FilterType: Menu, CheckBox, Excel); per-grid filter type configuration
- `Enumeration/GridsEnumerations.cs` — FilterType.Excel enum value (line 543)
- `Internal/Actions/Filter.cs` — Filter module: predicate generation, CheckboxListData building, ClearBtnClick, ApplyFilter logic
- `Internal/Renderer/Filter/FilterType.razor` — Router component: renders FilterCheckBoxRenderer when Type=Excel
- `Internal/Renderer/Filter/FilterCheckBoxRenderer.razor` + `.razor.cs` — Excel popup UI: checkbox list, search input, custom filter submenu, apply/clear/close actions
- `Internal/Renderer/Filter/ExcelBase.razor` — Shared Excel/CheckBox base content (search input, distinct value list rendering)
- `GridEvents.cs` — FilterDialogOpening (line 637), FilterDialogOpened (line 665), CheckboxFilterSearching (line 693) events
- `EventModels/Grids.cs` — FilterDialogOpenEventArgs (line 5930), FilterDialogOpenedEventArgs (line 6002), CheckboxFilterSearchEventArgs (line 6283)
- `sf-grid.ts` — JS interop methods: customFilterDialog (line 54), filterPopupRender (line 294), focusExcelInput (line 643)

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

| Combination                     | Must Preserve                                                                                                                                            | Risk     |
|---------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------|----------|
| Excel Filter + Standard Filter  | Excel is a UI variant of standard filtering; shares Filter<T> module, query composition (WhereFilter predicates), event pipeline (OnActionBegin/OnActionComplete) | Critical |
| Excel Filter + Sorting          | Sorted data must be used when building distinct value list (FilterChoiceCount fetches from sorted DataSource if sort active)                             | High     |
| Excel Filter + Grouping         | Distinct values computed from all records (not grouped rows); filtered data must be grouped after filter predicates applied                              | High     |
| Excel Filter + Paging           | Filter predicates apply to entire DataSource; CurrentPage resets to 1 after filter applied (shared with standard filtering)                              | High     |
| Excel Filter + Virtualization   | Distinct values fetched via DataManager query (bypasses virtual viewport); filtered data must invalidate virtual cache                                   | High     |
| Excel Filter + FilterBar        | MUTUALLY EXCLUSIVE in UI (Type=Excel shows popup; Type=FilterBar shows inline inputs); cannot render both for same column simultaneously                 | High     |
| Excel Filter + FilterMenu       | MUTUALLY EXCLUSIVE in UI (Type=Excel shows checkbox popup; Type=Menu shows operator dropdown popup); cannot render both for same column simultaneously   | High     |
| Excel Filter + Search           | Search text applies to distinct value list (CheckboxFilterSearching event); not same as grid-level search (SearchModule)                                 | Medium   |
| Excel Filter + Infinite Scroll  | Filter predicates trigger ResetInfiniteProperties; CurrentPage resets to 1 (shared with standard filtering)                                              | Medium   |
| Excel Filter + Adaptive         | Adaptive mode renders Excel popup as full-screen dialog on mobile; same predicate generation logic applies                                               | Medium   |

---

## JavaScript Interop
<!-- token-budget: 100 words -->

Excel filter relies on 3 TypeScript functions in `sf-grid.ts` for popup positioning, custom dialog rendering, and focus management.

### C# → JS Call Flow

| C# Call Site | TypeScript Function | Purpose | Line |
|--------------|---------------------|---------|------|
| FilterCheckBoxRenderer.razor.cs | `filterPopupRender` | Position Excel popup relative to filter icon in header | 294 |
| FilterCheckBoxRenderer.razor.cs | `customFilterDialog` | Render Custom Filter/Between operator dialog | 54 |
| FilterCheckBoxRenderer.razor.cs | `focusExcelInput` | Focus search input after popup opens | 643 |

### TypeScript Function Signatures

```typescript
// Line 294 in sf-grid.ts
filterPopupRender(dataId: string, dlgID: string, uid: string, type: string, isColumnMenu: boolean): string[]

// Line 54 in sf-grid.ts
customFilterDialog(dataId: string, dlgID: string, isExcel: boolean): void

// Line 643 in sf-grid.ts
focusExcelInput(dataId: string, celluid: string): void
```

### Responsibility Split

| Responsibility | C# (FilterCheckBoxRenderer) | JS (sf-grid.ts) |
|----------------|------------------------------|-----------------|
| Popup content rendering | ✅ Razor markup, checkbox list | ❌ |
| Popup positioning | ❌ | ✅ Calculate X/Y coordinates |
| Distinct value filtering | ✅ CheckboxListData generation | ❌ |
| Custom dialog DOM rendering | ❌ | ✅ Dialog element manipulation |
| Focus management | ❌ | ✅ setTimeout focus logic |
| Event handling | ✅ Apply/Clear/Search clicks | ❌ |

### CRITICAL Rules

1. **Positioning Timing**: `filterPopupRender` MUST be called after popup DOM rendered but before display (ensures correct coordinates).
2. **Focus Delay**: `focusExcelInput` uses setTimeout to avoid race condition with popup animation (do not remove delay).
3. **Dialog ID Consistency**: `dlgID` parameter MUST match popup element ID attribute (mismatch causes querySelector failure).
4. **Grid Instance Check**: All 3 functions check `isNullOrUndefined(gridInstance.element.blazor__instance)` before interop (prevents null reference in disposed state).

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

### Mode: excel-filter-implementation

**Task**: Implement or fix Excel filter functionality in the Syncfusion Blazor DataGrid (`SfGrid<TValue>`).

**Core Behaviour Rules**:
1. **Activation**: Excel filter UI activated when `GridFilterSettings.Type` or `GridColumn.FilterSettings.Type` resolves to `FilterType.Excel`.
2. **UI Components**: Renders `FilterCheckBoxRenderer<TContent>` with `ExcelBase<TContent>` in popup; includes checkbox list (distinct values), search input, custom filter submenu (Custom Filter/Between).
3. **Distinct Values**: Default limit 1000 values; overridable via `FilterChoiceCount` in `FilterDialogOpening` or `CheckboxFilterSearching` events.
4. **Cancellable Opening**: `FilterDialogOpening` or `OnActionBegin` (RequestType=FilterBeforeOpen) can set `Cancel=true` to stop popup from opening.
5. **Search Behavior**: User types in search input → raises `OnActionBegin` (RequestType=FilterSearchBegin) → raises `CheckboxFilterSearching` → updates visible checkbox list based on returned `SearchText`/`CheckboxListData`.
6. **Apply Logic**: User selects checkbox values → clicks apply → removes existing predicates for same column UID → generates `equal`/`notequal` predicates → updates `FilterSettings.Columns` → invokes model change with RequestType=Filtering.
7. **Custom Filter Submenu**: "Custom Filter" or "Between" opens dialog with operator/value inputs; generates 1 predicate (single operand) or 2 predicates (both operands with and/or conjunction).
8. **Clear/Close**: Clear action closes popup, resets component state, removes predicates (per `ClearBtnClick` logic in Filter<T>).

**Lifecycle Events**:
1. **FilterDialogOpening**: Raised before popup opens; can set `Cancel=true` to stop opening; can override `CheckboxListData` (distinct values) or `FilterChoiceCount` (value limit).
2. **FilterDialogOpened**: Raised after popup fully rendered.
3. **OnActionBegin** (RequestType=FilterBeforeOpen): Cancellable pre-open event.
4. **OnActionBegin** (RequestType=FilterSearchBegin): Raised when user types in search input.
5. **CheckboxFilterSearching**: Raised during search; can override `SearchText` or `CheckboxListData` to customize visible choices.
6. **OnActionBegin** (RequestType=Filtering): Raised when user clicks apply; can cancel filter operation or modify predicates.
7. **OnActionComplete** (RequestType=Filtering): Raised after filter applied successfully.

**Cross-Feature Integration**:
- **Standard Filtering**: Excel filter is UI variant of standard filtering; shares `Filter<T>` module, query composition (`WhereFilter` predicates), event pipeline (OnActionBegin/OnActionComplete).
- **Sorting**: Distinct value list built from sorted DataSource if sort active (FilterChoiceCount fetches from sorted data).
- **Grouping**: Distinct values computed from all records (not grouped caption rows); filtered data grouped after predicates applied.
- **Paging**: CurrentPage resets to 1 after filter applied (shared with standard filtering).
- **Virtualization**: Distinct values fetched via DataManager query (bypasses virtual viewport); filtered data invalidates virtual cache.
- **FilterBar/FilterMenu**: MUTUALLY EXCLUSIVE in UI — Type=Excel shows checkbox popup; Type=FilterBar shows inline inputs; Type=Menu shows operator dropdown popup (cannot render multiple for same column).
- **Search**: CheckboxFilterSearching event applies to distinct value list search (in-popup); not same as grid-level search (SearchModule).
- **Infinite Scroll**: Filter predicates trigger `ResetInfiniteProperties`; CurrentPage resets to 1.
- **Adaptive**: Excel popup renders as full-screen dialog on mobile; same predicate generation logic applies.

**JS Interop Hooks** (see JavaScript Interop section for full details):
- `filterPopupRender` (line 294): Positions Excel popup relative to filter icon.
- `customFilterDialog` (line 54): Opens custom operator/value dialog for "Custom Filter" or "Between" submenu.
- `focusExcelInput` (line 643): Focuses search input after popup opens.

**Predicate Generation**:
- Checkbox selection: Generates `equal` predicates for selected values (OR conjunction); or `notequal` predicates for unselected values if fewer unselected (AND conjunction).
- Custom Filter: Generates 1-2 predicates based on operator/value inputs; conjunction (`and`/`or`) selected by user when both operands provided.
- Between: Generates 2 predicates (field >= value1 AND field <= value2).

**Code Locations**:
- `Internal/Actions/Filter.cs` — Predicate generation, CheckboxListData building, ClearBtnClick, ApplyFilter.
- `Internal/Renderer/Filter/FilterCheckBoxRenderer.razor.cs` — Excel popup UI logic (checkbox list rendering, search handling, apply/clear actions).
- `Internal/Renderer/Filter/ExcelBase.razor` — Shared Excel/CheckBox base content (search input, distinct value list).
- `sf-grid.ts` — TypeScript interop functions (see JavaScript Interop section for details).

**Before Implementation**:
- Read `/docs/training/01-getting-started/architecture-overview.md` for module injection pattern.
- Confirm Excel filter shares `Filter<T>` module with standard filtering (FilterBar/FilterMenu).
- Verify Interaction Matrix: standard filtering (shared module/query composition), sorting (sorted DataSource for distinct values), grouping (distinct values from all records), paging (CurrentPage reset), virtualization (bypass viewport), FilterBar/FilterMenu (UI mutual exclusivity).

---

### Mode: excel-filter-bug-fix

**Task**: Fix a bug in Excel filter functionality without breaking existing behaviour.

**Mandatory Checks**:
1. **Activation**: Verify Excel filter UI rendered only when Type=Excel (not when Type=Menu or FilterBar).
2. **Cancellable Opening**: Confirm FilterDialogOpening / OnActionBegin (RequestType=FilterBeforeOpen) can set Cancel=true to stop popup.
3. **Distinct Values Limit**: Verify default limit is 1000; custom limit via FilterChoiceCount in opening/search events.
4. **Search Updates**: Confirm CheckboxFilterSearching event raised when user types; visible checkbox list updated based on returned SearchText/CheckboxListData.
5. **Apply Logic**: Verify existing predicates removed for same column UID before new predicates added; FilterSettings.Columns updated correctly.
6. **Custom Filter**: Confirm Custom Filter/Between submenu generates correct predicates (1 or 2) with correct operators/conjunction.
7. **Clear Action**: Verify ClearBtnClick closes popup, resets state, removes predicates.
8. **JS Interop**: Confirm filterPopupRender, customFilterDialog, focusExcelInput called at correct times (see JavaScript Interop section for timing requirements).
9. **Shared Module**: Verify Excel filter uses same Filter<T> module as FilterBar/FilterMenu (no separate filter module).
10. **Paging Reset**: Confirm CurrentPage resets to 1 after filter applied (shared with standard filtering).

**Interaction Matrix Validation**:
- Excel Filter + Standard Filter: Shared Filter<T> module, query composition, event pipeline.
- Excel Filter + Sorting: Distinct values from sorted DataSource if sort active.
- Excel Filter + Grouping: Distinct values from all records; filtered data grouped after predicates applied.
- Excel Filter + Paging: CurrentPage resets to 1 after filter applied.
- Excel Filter + Virtualization: Distinct values fetched via DataManager query; filtered data invalidates virtual cache.
- Excel Filter + FilterBar: UI mutual exclusivity (cannot render both for same column).
- Excel Filter + FilterMenu: UI mutual exclusivity (cannot render both for same column).
- Excel Filter + Search: CheckboxFilterSearching event for in-popup search (not grid-level search).
- Excel Filter + Infinite Scroll: Filter predicates trigger ResetInfiniteProperties; CurrentPage resets to 1.
- Excel Filter + Adaptive: Full-screen dialog on mobile; same predicate generation logic.

**Before Fix**:
- Read `/docs/training/00-START-HERE.md` — #1 rule: never break existing behavior.
- Review existing unit tests for Excel filter feature.
- Cross-check Interaction Matrix with live specs from `/openspec/openspec/specs/excelfilter/spec.md`.
- Verify lifecycle events (FilterDialogOpening, CheckboxFilterSearching, OnActionBegin, OnActionComplete) raised at correct times.
