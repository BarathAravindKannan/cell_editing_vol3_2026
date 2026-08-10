---
name: selection-skill
description: Expert knowledge for the Selection feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to selection behaviour, including Row/Cell/Both selection modes, Single/Multiple selection types, checkbox selection with header tri-state, persist selection across pages/filters/sort, drag selection, programmatic selection APIs, and cross-feature interaction guarantees with virtualization, grouping, filtering, sorting, paging, editing, and frozen columns.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for all Selection modes and types in the Syncfusion Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
<!-- token-budget: 40 words -->
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to Selection (Row, Cell, Both, Checkbox, Persist, Drag). Do NOT load alongside other feature skills.
- One agent invocation = one feature skill maximum.

---

## Knowledge References
<!-- token-budget: 60 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

Key rules and edge cases from `/docs/training/` that directly govern Selection work:

**Selection Mode & Type Guards**
- `SelectRow()` / `SelectRows()` return early if `Mode == SelectionMode.Cell` unless edit mode is Batch. Any mode-switching code must preserve this guard.
- `Ctrl+A` (CtrlA key combination) immediately returns when `Type == SelectionType.Single` — never allow select-all in single-selection mode.
- `CheckboxOnly = true` makes `RowSelectionClickHandler` return immediately when `IsCheckBox == false` — plain row clicks are fully blocked.
- `CheckboxMode = ResetOnRowClick`: a plain row click (no Ctrl/Shift) deselects all others, routing through single-select logic via `HasCheckBoxColumn() == false`.
- Mode switches (`SelectionMode` change) inject `"CellSelectionModeChanged"`, `"RowSelectionModeChanged"`, or `"BothSelectionModeChanged"` into `PropertyChanges` so `ClearCellSelection` can distinguish a mode-change clear from a regular deselect.

**Persist Selection Rules**
- `PersistSelection = true` requires at least one column with `IsPrimaryKey = true`. `Selection<T>.PrimaryKey` is computed lazily from `GridUtils.GetColumns(Parent)`. If the primary key column is absent, persist dictionaries silently stay empty — most common persist bug.
- `_persistedData` (selected keys → data) and `DeSelectedPersistData` (deselected keys → data) are complementary. For remote data (`DataSource == null`), `DeSelectedPersistData` is the canonical source of truth for header checkbox state.
- `IsHeaderCheckboxChecked = true` keeps the persist collection in "filter-active" mode. Clearing filters or search does NOT automatically reset this flag; call `ResetPersistSelection()` or explicitly update `IsSelectFilteredField` / `IsSelectSearchKey`.
- `GetCurrentFilterData(requestType)` keeps `_filteredOrSearchedData` in sync with `_persistedData` — must be called on every filter/search clear with the correct `requestType` string ("ClearFiltering", "ClearSearch").

**Cell Selection Specifics**
- `CellSelectionMode.Flow`: range selects all cells from start to end index including cells in between rows.
- `CellSelectionMode.Box`: range selects only cells within the start/end column range across the rows within the range.
- `SelectCells()` indexes by array position (not row index) against `_dataRows`; must filter out DetailRow types.
- Box-mode range spanning a frozen column boundary may produce incorrect `startCellIndex/endCellIndex` — frozen column indices must remain contiguous when computing the range.

**Both Mode (Row + Cell)**
- `RowCellSelectionClickHandler` must update both row-level `IsSelected` and cell-level `IsSelected` state on every click.
- `ClearCellSelection` with `IsClearRowSelectionNeeded: true` must be called before re-selecting to avoid stale cell states.

**Checkbox Header Tri-State**
- `SetHeaderCheckState()` computes a `CheckState` enum value (`Check`, `UnCheck`, `Intermediate`) from 6 boolean conditions. Any modification must re-run all 6 conditions.
- `UpdateCBoxSelection(CheckState.UnCheck)` removes all rows from `_persistedData` using `PrimaryKey`; must be called when header checkbox is unchecked.

**Drag Selection**
- `AllowDragSelection = true` requires `Type == SelectionType.Multiple`.
- When `PersistSelection = true` and drag-deselecting, `ClearRowSelection` must remove the row's primary key from `_persistedData`. Drag module must NOT call `ClearRowSelection` on drop target refresh — use the selection API instead.

**Virtual Scroll Integration**
- `SelectRowsAsync` must write to `VirtualScrollModule.SelectRowsMethodIndexes` so off-screen selections are restored when rows scroll into view.
- `GetRowsObject()` switches between `Parent.Rows` and `VirtualScrollModule.GeneratedRows` based on `EnableVirtualization`. Always use `GetRowsObject()` — never access `Parent.Rows` directly in Selection<T>.
- `ShiftSelectionRowIndexes` / `ShiftSelectionCellIndexes` must be reset to `(-1, -1)` on clear when virtualization is active.

**Adaptive UI Toolbar Sync**
- After any selection change when `EnableAdaptiveUI == true` and delete/edit is enabled, `EventAggregator.Trigger("ToolbarStateChanged", null!)` must be fired. Missing this leaves Edit/Delete buttons in wrong state on mobile.

**Render & Concurrency**
- Use `Parent.SoftRefresh = true` + `EventAggregator.Trigger("RowStateChanged", row)` — never call `StateHasChanged()` directly.
- All `await` calls must use `.ConfigureAwait(true)` for Blazor context continuity.
- No direct module-to-module calls — all cross-module events via `EventAggregator`.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `src/Internal/Actions/Selection.cs` | Primary module `Selection<T>` — all row/cell/checkbox selection logic, persist, header checkbox state, keyboard handler, click handlers, range selection, cell-by-row, drag selection |
| `src/GridSelectionSettings.cs` | Public `[Parameter]` properties: `Mode`, `Type`, `PersistSelection`, `CheckboxOnly`, `CheckboxMode`, `EnableSimpleMultiRowSelection`, `EnableToggle`, `CellSelectionMode`, `AllowDragSelection` |
| `src/SfGrid.Properties.cs` | Root grid params: `AllowSelection` (bool, default true), `SelectedRowIndex` (int), `SelectionSettings` (GridSelectionSettings), `CheckBoxState` (CheckState) |
| `src/SfGrid.Methods.cs` | Public async API: `SelectRowAsync`, `SelectRowsAsync`, `SelectRowsByRangeAsync`, `SelectCellAsync`, `SelectCellsAsync`, `SelectCellsByRangeAsync`, `ClearSelectionAsync`, `GetSelectedRecordsAsync`, `GetSelectedRowIndexesAsync`, `GetSelectedRowCellIndexesAsync` |
| `src/EventModels/Grids.cs` | Event args: `RowSelectingEventArgs<T>`, `RowSelectEventArgs<T>`, `RowDeselectEventArgs<T>`, `CellSelectingEventArgs<T>`, `CellSelectEventArgs<T>`, `CellDeselectEventArgs<T>` |
| `src/GridEvents.cs` | Event callbacks on `GridEvents<TValue>`: `RowSelecting`, `RowSelected`, `RowDeselecting`, `RowDeselected`, `CellSelecting`, `CellSelected`, `CellDeselecting`, `CellDeselected` |
| `src/Enumeration/GridsEnumerations.cs` | Enums: `SelectionMode` (Row/Cell/Both), `SelectionType` (Single/Multiple), `CheckboxSelectionType` (Default/ResetOnRowClick), `CellSelectionMode` (Flow/Box), `CheckState` (Check/UnCheck/Intermediate) |
| `src/Internal/Actions/VirtualScroll.cs` | `SelectRowsMethodIndexes`, `ShiftSelectionRowIndexes`, `ShiftSelectionCellIndexes`, `IsSelAllChangedByRowClick`, `IsSelectAllWithFilter` — virtual scroll selection state holders |
| `src/Internal/Actions/FocusHandler.cs` | `SelectedRowIndex`, `SelectedCellIndex` — focus sync after programmatic selection; null-guarded if focus module disabled |
| `src/Internal/Base/GridJSInteropAdaptor.cs` | Routes JS click events to `ClickHandler`; no direct selection JS calls |
| `scripts/selection.ts` | TypeScript selection module — fires click/keyboard events back to .NET; autofill positioning functions (`updateAutofillPosition`, `createBorder`); no selection state held in JS |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live feature cross-reference + `/docs/training/` risk tables.  
> Omitted pairs have no meaningful interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| Selection + Paging | `RefreshSelectionOnPaging()` must be called on every page change: resets `_lastSelectedRow/_lastSelectedCell` and sets `CheckBoxState = UnCheck` when `PersistSelection = false`. Changing paging logic must call `RefreshSelectionOnPaging()` before re-rendering rows. | Critical |
| Selection + Persist + Filter/Search | `GetCurrentFilterData(requestType)` keeps `_filteredOrSearchedData` in sync with `_persistedData`. Any filter/search clear that does not pass the correct `requestType` string leaves the persist dictionary stale, causing phantom selections on subsequent pages. | Critical |
| Selection + Virtualization | `GetRowsObject()` must always be used (switches to `VirtualScrollModule.GeneratedRows` when `EnableVirtualization`). Programmatic `SelectRowsAsync` must write to `VirtualScrollModule.SelectRowsMethodIndexes`. `ShiftSelectionRowIndexes` / `ShiftSelectionCellIndexes` must be reset to `(-1,-1)` on clear. Lazy-loaded group rows require `VirtualScrollModule.VisibleGroupRows`. | Critical |
| Selection + Editing | `Edit<T>` sets `EditModule.ClearSelection = true` to signal editing. `SelectRow()` checks this flag and calls `ClearRowSelection` before selecting the new row. Persist selection must NOT clear `_persistedData` while `PersistSelection && Parent.IsEdit` guard is active. | High |
| Selection + Grouping | `SelectByRow` and `ClearSelectionByRow` must sync both `Parent.Rows[i].IsSelected` AND the matching `Parent.CurrentGroupedData[i].IsSelected` by Uid. Forgetting the `CurrentGroupedData` sync causes incorrect header-checkbox tri-state rendering. | High |
| Selection + Sorting | Selection listens to `DataBound` (fired after sort) and re-applies persist selection by primary key, NOT by the stale pre-sort row index. Row indexes shift after sort — always use `_persistedData` (key-based) for reselection. | High |
| Selection + Checkbox Column | `HasCheckBoxColumn()` checks `GridUtils.GetColumns(Parent)` for `ColumnType.CheckBox`. Header checkbox tri-state (`SetHeaderCheckState`) depends on 6 boolean conditions — all must be re-evaluated on every row state change. `UpdateCBoxSelection(UnCheck)` removes all keys from `_persistedData`. | High |
| Selection + Infinite Scroll | On each new page append, `RefreshSelectionOnPaging` is NOT called (infinite scroll preserves existing selection). Persist selection must re-mark newly loaded rows as selected using `_persistedData` key lookup during `DataBound`. | High |
| Selection + Cell (Box mode) + Frozen Columns | Box-mode range spanning a frozen column boundary produces incorrect `startCellIndex/endCellIndex` unless frozen column logical indices remain contiguous. Verify `SelectRangeOfCells` uses absolute column index (not visual position) when frozen columns are present. | High |
| Selection + Detail Row | `SelectRows` and `ClearRowSelection` both explicitly filter out `RowType == "DetailRow"`. Any new iteration over `Parent.Rows` in Selection must replicate this guard. | Medium |
| Selection + Row Drag/Drop | When `AllowDragSelection = true` and `PersistSelection = true`, `ClearRowSelection` removes the row's primary key from `_persistedData` on drag-deselect. The drag module must NOT call `ClearRowSelection` on drop-target refresh — use `SelectRowAsync` instead. | Medium |
| Selection + Export | `GetSelectedRecordsAsync` returns rows from `Parent.Rows` where `IsSelected == true`. With persist mode and remote data, only the current page's rows are in `Parent.Rows`; `_persistedData` may contain more keys. Export must decide whether to use current-page `SelectedRecords` or the full `_persistedData` dictionary. | Medium |
| Selection + Aggregates | `ReactiveAggregate<T>` listens to `DataBound`. Selection triggers `RowStateChanged` (not `DataBound`) — aggregates are NOT recalculated on selection. Do not tie aggregate refresh to `RowStateChanged`. | Medium |
| Selection + Both Mode (Row+Cell) | `RowCellSelectionClickHandler` must update both row-level `IsSelected` AND specific cell `IsSelected`. `ClearCellSelection(IsClearRowSelectionNeeded: true)` must precede any new selection to avoid stale combined state. `SelectByRow` in Both mode does NOT mark all cells selected (`if (!IsBothMode())` guard). | Medium |
| Selection + FocusHandler | After `SelectByRow` with `isSelectionMethodInvoked = true`, `FocusModule.Focus()` is invoked. If `FocusModule` is null (disabled), the call is null-conditional guarded — no crash but focus won't move. After programmatic selection with `isScrollIntoView = true`, `FocusModule.SelectedRowIndex` and `SelectedCellIndex` must be set before calling `Focus()`. | Low |
| Selection + AdaptiveUI | After any selection change when `EnableAdaptiveUI == true` and `AllowEditing/AllowDeleting`, `EventAggregator.Trigger("ToolbarStateChanged", null!)` must fire. Missing this leaves toolbar buttons in wrong state on mobile. | Low |

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Calls**

Selection uses JavaScript Interop for autofill cell selection visual feedback positioning:

| C# Method | Line | JS Function | TS Line | Purpose |
|-----------|------|-------------|---------|---------|
| `Selection<T>.SelectByCell()` | 1850 | `sfBlazor.Grid.updateAutofillPosition` | 425 | Calculates autofill handle position based on selected cell bounds |
| `Selection<T>.SelectByCell()` | 1880 | `sfBlazor.Grid.createBorder` | 465 | Creates visual border around cell selection range for autofill drag |

**Responsibility Split**

| Layer | Owns | Does NOT Own |
|-------|------|--------------|
| **C# (Selection.cs)** | Selection state (_persistedData, IsSelected flags), row/cell click routing, persist logic, range calculation, event firing (RowSelected, CellSelected) | DOM measurements, CSS positioning, autofill handle rendering |
| **TypeScript (selection.ts)** | DOM measurements (getBoundingClientRect), autofill handle positioning (left/right/top), border element creation, mouse drag event handling | Selection state, persist dictionaries, row/cell IsSelected flags, event arg creation |

**CRITICAL Rules**
1. **No State in JS**: `selection.ts` NEVER stores selected row/cell state — all state lives in C# `Selection<T>._persistedData` and row/cell `IsSelected` flags.
2. **Position-Only Interop**: JS calls return only CSS position objects (`{ Left, Right, Top }`). C# decides WHEN to call based on selection mode and cell range logic.
3. **Frozen Column Awareness**: `createBorder()` must account for frozen columns when computing visual bounds — use absolute column index, not visual position offset.
4. **Virtualization Guard**: Autofill positioning calls occur AFTER virtual scroll row generation completes (`VirtualScrollModule.GeneratedRows` populated). Missing this guard causes null cell references.

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Selection (All Types & Combinations)
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Selection feature.
Scope: src/Internal/Actions/Selection.cs and the surfaces listed in Code Location Map only.

=== SELECTION TYPES & MODES (pre-loaded) ===

SelectionMode:
  Row   — default; RowSelectionClickHandler routes all clicks
  Cell  — CellSelectionClickHandler; SelectRow/SelectRows block unless Batch edit
  Both  — RowCellSelectionClickHandler; both row + cell state must be updated together

SelectionType:
  Single   — default; Ctrl+A, Shift, multi-click blocked
  Multiple — Ctrl+click (add), Shift+click/key (range), EnableSimpleMultiRowSelection (click to add without Ctrl)

CheckboxMode:
  Default           — checkbox click toggles; row click adds without resetting others
  ResetOnRowClick   — row click resets others (single-select behaviour); routes through single-select logic

CellSelectionMode (requires Mode = Cell or Both):
  Flow  — default; range includes all cells between start and end across rows
  Box   — range limited to start/end column window across rows in range

Key flags:
  CheckboxOnly = true   → RowSelectionClickHandler returns if IsCheckBox == false
  EnableToggle = true   → selected row/cell can be deselected by clicking again (CanToggle())
  AllowDragSelection    → requires Type = Multiple; drag to range-select rows/cells
  PersistSelection      → requires IsPrimaryKey column; uses _persistedData + DeSelectedPersistData

=== PUBLIC API (SfGrid.Methods.cs) ===
  SelectRowAsync(index, isToggle?, selectAcrossPages)
  SelectRowsAsync(int[] rowIndexes)
  SelectRowsByRangeAsync(startIndex, endIndex?)
  SelectCellAsync((rowIndex, cellIndex), isToggle?)
  SelectCellsAsync((rowIndex, cellIndex)[])
  SelectCellsByRangeAsync((startRow, startCell), (endRow, endCell))
  ClearSelectionAsync()
  GetSelectedRecordsAsync()      → List<TValue>
  GetSelectedRowIndexesAsync()   → List<int>
  GetSelectedRowCellIndexesAsync() → List<ValueTuple<int,int>>

=== INTERNAL STATE ===
  _persistedData             → Dictionary<key, data>  — selected rows by primary key
  DeSelectedPersistData      → Dictionary<key, data>  — explicitly deselected rows (remote data)
  _filteredOrSearchedData    → in-sync with active filter/search for persist
  IsHeaderCheckboxChecked    → tri-state gate for filter-active persist mode
  _lastSelectedRow / _lastSelectedCell → anchor for shift-range operations
  RangeStartIndex / RangeEndIndex     → range selection bounds

=== BEFORE MAKING ANY CHANGE ===
1. Identify which SelectionMode/SelectionType combination is affected.
2. Check persist guard: if mutating _persistedData, confirm PersistSelection == true.
3. Check virtual guard: if EnableVirtualization, use GetRowsObject() not Parent.Rows.
4. Check grouped data sync: if AllowGrouping active, sync CurrentGroupedData[i].IsSelected by Uid.
5. Check header checkbox: after any row state change, call SetHeaderCheckState().
6. Check Both-mode: if Mode == Both, update both row and cell IsSelected state.
7. Consult /docs/training/02-requirements-analysis/understanding-requirements.md for edge cases.
8. Consult /docs/training/06-reference/quick-reference-guides.md for regression risk combos.

=== MANDATORY CONSTRAINTS ===
- No direct StateHasChanged() — set SoftRefresh = true + Trigger("RowStateChanged", row)
- No direct JSRuntime.InvokeAsync — route through GridJSInteropAdaptor<T>
- No module-to-module direct calls — use EventAggregator
- All await calls use .ConfigureAwait(true)
- Zero analyzer warnings; XML comments on all new/modified public and internal members
- Follow naming-conventions.md (PascalCase methods, _camelCase private fields, TValue public / T internal)
- Follow error-handling.md (guard clauses first, no silent catch blocks)

=== SCOPE ===
{Describe the exact method(s) or selection combination to implement or fix — one method per task}

=== INPUT ===
{Paste the extracted code chunk — see training/04-code-processing/optimal-chunking-strategies.md}

=== OUTPUT ===
1. Root cause / design rationale (3–5 sentences), identifying which SelectionMode+Type+flag combination is involved
2. Modified method(s) only — no full-file reproduction
3. Interaction risk flags (reference Interaction Matrix above for affected combinations)
4. Required test cases (Given-When-Then format, covering the specific mode/type combination)

After implementation, run regression verification using:
/docs/ai-agents/prompts/regression-verification-prompt.md
```
