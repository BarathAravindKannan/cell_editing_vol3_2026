---
name: keyboard-navigation-skill
description: Expert knowledge for the Keyboard Navigation feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to keyboard navigation behaviour, including cell/row focus movement, configurable key bindings (GridKeySettings), all key combinations across features, FocusHandler lifecycle, JS-side keydown pipeline, and cross-feature interaction guarantees with editing, selection, filtering, grouping, paging, virtualization, and frozen columns.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Keyboard Navigation in the Syncfusion Blazor DataGrid. Guarantees no regression in any keyboard-driven workflow across all features.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to Keyboard Navigation / FocusHandler.
- Do NOT load alongside other feature skills.
- One agent invocation = one feature skill maximum.

---

## Knowledge References
<!-- token-budget: 60 words -->

All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/training/04-code-processing/optimal-chunking-strategies.md`
- `docs/training/05-practical-examples/feature-implementation-walkthrough.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`
- `src/GridKeySettings.cs`
- `src/Internal/Actions/FocusHandler.cs`
- `scripts/sf-grid-fn.ts` (keydown handlers)
- `scripts/sf-grid.ts` (focus interop function definitions)
- `scripts/infinite-scroll.ts`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

Key rules and edge cases from `/docs/training/` and codebase analysis that directly govern Keyboard Navigation work:

- **Three parallel keydown pipelines**: `gridKeyDownHandler` (DOM/clipboard), `keyDownHandler` (filter bar, template cell Tab), and `keyActionHandler` (edit form Enter/Save) all fire on the same `keydown` event. A change to one must not suppress calls to the others.
- **`GetKeyCombination()` is the canonical string**: All C# key routing uses `e.GetKeyCombination(isMacDevice)`. Never compare raw `e.Key` or `e.Code` in action modules — use the combination string.
- **`GridKeySettings` is the customisation contract**: `MoveUpCell`, `MoveDownCell`, `MoveLeftCell`, `MoveRightCell` are comma-delimited key combination strings configurable by users. `GetAction(keyCombination)` maps an incoming key to an action name. Never hard-code `ArrowUp`/`ArrowDown` in navigation logic — always go through `GetAction()`.
- **`ConfigureAwait(true)` is mandatory**: Every `await` in `FocusHandler<T>` must use `.ConfigureAwait(true)` for Blazor Server context continuity.
- **No direct `StateHasChanged()`**: Focus state changes must route through `SoftRefresh = true` + `EventAggregator.Trigger("RowStateChanged", row)`.
- **JS interop for DOM focus only**: Actual DOM `.focus()` calls must go through `sfBlazor.Grid.focus` / `sfBlazor.Grid.focusFilterBar` / `sfBlazor.Grid.focusAddForm` — never call `JSRuntime.InvokeAsync` directly from `FocusHandler<T>`.
- **`IsChildFocused` gate**: When focus is inside a template cell child element (`IsChildFocused = true`), all navigation keys are suppressed except `Escape`. Changes to template-cell focus must honour this gate.
- **`ShouldExitGrid` controls Tab boundary**: `Tab` from the very last cell and `ShiftTab` from the very first header cell must release focus to the browser's natural tab order — never trap the user inside the grid.
- **Mac device modifier awareness**: `GetKeyCombination(isMacDevice: true)` maps `Cmd` (Meta) as the Ctrl equivalent. All new key combos must be tested on Mac.
- **`documentKeyHandler` fires on `document.body`**: Alt+J (grid focus) and Alt+W (jump to first cell) are document-level listeners that bypass the element-scoped pipeline. Do not remove or re-order these listeners.
- **Zero-warning build is mandatory**: All modified `public` members on `FocusHandler<T>` and `GridKeySettings` must carry XML `/// <summary>` comments.

---

## Complete Key Combination Reference
<!-- token-budget: 200 words -->

### Cell Navigation (configurable via `GridKeySettings`)

| Key Combination | Default Action | `GridKeySettings` Property |
|-----------------|---------------|---------------------------|
| `ArrowUp` | Move focus to cell above | `MoveUpCell` |
| `ShiftEnter` | Move focus to cell above | `MoveUpCell` |
| `ShiftUp` | Move focus to cell above (with Shift) | `MoveUpCell` |
| `ArrowDown` | Move focus to cell below | `MoveDownCell` |
| `Enter` | Move focus to cell below | `MoveDownCell` |
| `ShiftDown` | Move focus to cell below (with Shift) | `MoveDownCell` |
| `ArrowLeft` | Move focus to left cell | `MoveLeftCell` |
| `ShiftTab` | Move focus to left cell / exit left | `MoveLeftCell` |
| `ShiftLeft` | Move focus to left cell (with Shift) | `MoveLeftCell` |
| `ArrowRight` | Move focus to right cell | `MoveRightCell` |
| `Tab` | Move focus to right cell / exit right | `MoveRightCell` |
| `ShiftRight` | Move focus to right cell (with Shift) | `MoveRightCell` |

> **Default-value note:** The public `MoveRightCell` parameter default is literally `"Tab,ArrowRight,,ShiftRight"` (`src/GridKeySettings.cs:64`) — note the **double comma** (an empty token). The internal normalized field `_moveRightCell` is `"Tab,ArrowRight,ShiftRight"`.

### Row/Grid Navigation (non-configurable)

| Key Combination | Action | Scope |
|-----------------|--------|-------|
| `Home` | Move to first visible cell in current row | Content + Header |
| `End` | Move to last visible cell in current row | Content + Header |
| `CtrlHome` | Move to first cell of first visible row | Content |
| `CtrlEnd` | Move to last cell of last visible row | Content |
| `AltW` | Jump to first cell of first content row (document-level) | Global |
| `AltJ` | Focus the grid element (Alt+J, document-level) | Global |

### Paging Navigation

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `PageUp` | Go to previous page | `AllowPaging = true`, pager not focused |
| `PageDown` | Go to next page | `AllowPaging = true`, pager not focused |
| `CtrlAltPageUp` | Go to first page | `AllowPaging = true` |
| `CtrlAltPageDown` | Go to last page | `AllowPaging = true` |
| `AltPageUp` | Go to previous ellipsis page group | `AllowPaging = true` |
| `AltPageDown` | Go to next ellipsis page group | `AllowPaging = true` |

### Editing Keys

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `F2` | Start editing focused cell/row | `AllowEditing = true`, no modifier keys |
| `Enter` (edit form) | Save and close edit (Normal/Dialog mode) | `isGridEditForm = true`, non-TEXTAREA |
| `Ctrl+Enter` (edit form) | Save multiline textarea | `isGridEditForm = true`, TEXTAREA |
| `Escape` | Cancel edit / close column chooser / close dialogs | `IsEdit = true` or dialog open |
| `Insert` | Add new record | `AllowAdding = true`, no active edit, no alert |
| `Delete` | Delete selected record(s) | `AllowDeleting = true` |
| `Tab` (Normal edit) | Move to next editable cell within edited row | `EditMode = Normal`, not last editable cell |
| `ShiftTab` (Normal edit) | Move to previous editable cell within edited row | `EditMode = Normal`, not first editable cell |
| `Tab` (Batch edit, last cell) | Save cell, move to next row | `EditMode = Batch`, last cell of row |

### Selection Keys

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `Space` | Select/deselect focused checkbox row | `ColumnType = CheckBox`, `!CheckboxOnly` or `AllowSelection = true` |
| `CtrlPlusA` | Select all rows | `AllowSelection = true`, `SelectionType = Multiple` |

### Filtering Keys

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `ArrowUp` (first content cell) | Move focus up to filter bar | `AllowFiltering = true`, `FilterType = FilterBar` |
| `ArrowDown` (header cell) | Move focus down to filter bar | `AllowFiltering = true`, `FilterType = FilterBar` |
| `Tab` (last header cell) | Move focus to first filter bar input | `AllowFiltering = true`, `FilterType = FilterBar` |
| `ShiftTab` (first content cell) | Move focus to last filter bar input | `AllowFiltering = true`, `FilterType = FilterBar` |
| `AltDown` (header cell) | Open filter menu / column menu | `FilterType != FilterBar` or `ShowColumnMenu = true` |

### Column Menu & Grouping Keys

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `AltDown` (header cell) | Open column menu | `ShowColumnMenu = true`, `ShowColumnMenu` on column |
| `AltDown` (header cell) | Open filter menu dialog | `AllowFiltering = true`, non-FilterBar, header focused |
| `CtrlUp` / `CtrlDown` | Focus next group caption row frame | `AllowGrouping = true`, caption row focused |

### Template & Detail Row Keys

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `Enter` (data cell with template) | Focus first focusable child inside template | `IsTemplate = true`, not in edit mode |
| `Escape` (inside template child) | Return focus to parent cell | `IsChildFocused = true` |
| `Tab` / `ShiftTab` (detail template) | Navigate focusable elements inside detail template | `DetailTemplate != null`, `IsDetailRow = true` |

### Print & Clipboard Keys

| Key Combination | Action | Condition |
|-----------------|--------|-----------|
| `Ctrl+P` / `Cmd+P` (Mac) | Print grid | Any |
| `Ctrl+C` / `Cmd+C` (Mac) | Copy selected rows to clipboard | `EnableClipboard` |
| `Ctrl+Shift+H` / `Cmd+Shift+H` (Mac) | Copy with header to clipboard | `EnableClipboard` |

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose | Key Lines |
|------|---------|-----------|
| `src/Internal/Actions/FocusHandler.cs` | Core keyboard navigation logic: `ProcessKeyDown` (line 230), `ProcessGridKeyDown` (line 589), `MoveFocusCell` (line 494), `ShouldExitGrid` (line 526), `FocusCell` (line 448) | 230, 494, 526, 589 |
| `src/GridKeySettings.cs` | `GridKeySettings` component — configurable key bindings for MoveUpCell / MoveDownCell / MoveLeftCell / MoveRightCell; `GetAction(keyCombination)` resolver (line 165) | 42, 64, 86, 108, 165 |
| `scripts/sf-grid-fn.ts` | JS keydown **handlers**: `gridKeyDownHandler` (line 1630), `keyDownHandler` (line 1363), `keyActionHandler` (line 1932), `documentKeyHandler` (line 1301), `gridKeyConfigs` map (line 2152) | 1301, 1363, 1630, 1932, 2152 |
| `scripts/sf-grid.ts` | DOM-focus **interop function definitions** invoked from `FocusHandler.cs`: `focus` (347), `scrollToFocusedCell` (382), `focusFilterBar` (489), `focusAddForm` (586), `focusChild` (705), `gridFocus` (949), `focusNextFrame` (1129) | 347, 382, 489, 586, 705, 949, 1129 |
| `scripts/infinite-scroll.ts` | Separate keydown listener for infinite scroll trigger on keyboard navigation past viewport boundary | N/A |
| `src/SfGrid.Properties.cs` | `KeySettings` parameter (type `GridKeySettings`) — public API entry point for binding `<GridKeySettings>` child component | N/A |
| `src/SfGrid.Methods.cs` | `FocusAsync()` — public API to programmatically set focus on the grid element | N/A |
| `src/Internal/Base/GridJSInteropAdaptor.cs` | `GridFocus`, `GridKeyDown`, `EndEdit` .NET-callable methods invoked by JS key handlers | N/A |
| `src/Enumeration/GridsEnumerations.cs` | `EditMode`, `FilterType`, `CellType`, `ColumnType` enums used by focus routing guards | N/A |
| `src/EventModels/Grids.cs` | `BeforeCellFocusArgs` (public event), `CellFocused` (internal event), `BeforeCellFocus` (internal DTO) | N/A |

---

## JavaScript Interop Architecture
<!-- token-budget: 150 words -->

Keyboard navigation requires tight C# ↔ JavaScript coordination for DOM focus manipulation. **All DOM `.focus()` calls must originate from TypeScript** — never directly from C# via `IJSRuntime`.

### C# → JS Interop Calls (FocusHandler.cs)

> **File note:** These interop function **definitions** live in `scripts/sf-grid.ts` (the `JS Line` column refers to that file). Only the keydown **handlers** (`gridKeyDownHandler`, `keyDownHandler`, `keyActionHandler`, `documentKeyHandler`) live in `scripts/sf-grid-fn.ts`.

| C# Call | TypeScript Function (`scripts/sf-grid.ts`) | Purpose | C# Line | JS Line |
|---------|---------------------|---------|---------|---------|
| `InvokeMethod("sfBlazor.Grid.focus", ...)` | `focus(dataId, rowUid, cellUid, ...)` | Set DOM focus on a specific cell with scroll adjustment | 960 | 347 |
| `InvokeMethod("sfBlazor.Grid.focusFilterBar", ...)` | `focusFilterBar(dataId, keyCombination, ...)` | Focus filter bar input for specified column | 280, 307, 320 | 489 |
| `InvokeMethod("sfBlazor.Grid.focusAddForm", ...)` | `focusAddForm(dataId, keyCombination)` | Focus first editable cell in ShowAddNewRow form | 471 | 586 |
| `InvokeMethod("sfBlazor.Grid.focusChild", ...)` | `focusChild(dataId, rowUid, cellUid, ...)` | Enter template cell and focus first focusable child | 968 | 705 |
| `InvokeMethod("sfBlazor.Grid.gridFocus", ...)` | `gridFocus(dataId, isFocusGrid, ...)` | Set focus on grid container element | 536, 958 | 949 |
| `InvokeMethod("sfBlazor.Grid.focusNextFrame", ...)` | `focusNextFrame(dataId)` | Navigate to next group caption frame (Ctrl+Down) | 1110 | 1129 |
| `InvokeMethod("sfBlazor.Grid.scrollToFocusedCell", ...)` | `scrollToFocusedCell(dataId)` | Scroll frozen/movable content to bring cell into view | 457 | 382 |

### JS → C# Interop Calls (sf-grid-fn.ts)

| JS Call | C# Method | Purpose | JS Line | C# Location |
|---------|-----------|---------|---------|-------------|
| `dotNetRef.invokeMethodAsync('GridFocus', e)` | `GridFocus(FocusEventArgs)` | Notify C# that grid element received focus | 1316, 1929 | GridJSInteropAdaptor.cs |
| `dotNetRef.invokeMethodAsync('GridKeyDown', ...)` | `GridKeyDown(KeyboardEventArgs, ...)` | Route JS keydown to C# FocusHandler pipeline | (embedded in KeyboardEvents) | GridJSInteropAdaptor.cs |

### Responsibility Split

| Responsibility | Handled By | Rationale |
|----------------|------------|-----------|
| Key combination parsing (`e.GetKeyCombination()`) | C# (FocusHandler) | Blazor KeyboardEventArgs provides normalized key strings |
| Action resolution (`GetAction(keyCombination)`) | C# (GridKeySettings) | User-configurable key bindings stored in Blazor component |
| Navigation logic (next cell calculation) | C# (FocusHandler) | Requires access to virtualization state, frozen columns, grouping rows |
| DOM `.focus()` execution | JavaScript (sf-grid.ts) | Browser APIs only accessible from JS; scroll position management |
| Focus state tracking (`cell.IsFocused`, `TabIndex`) | C# (Cell model) | Blazor re-renders cells based on state; JS only manipulates DOM |

### CRITICAL Interop Rules

1. **Always use `.ConfigureAwait(true)`** on all `InvokeMethod` calls in FocusHandler — Blazor Server requires synchronization context continuity.
2. **Never call `JSRuntime.InvokeAsync` directly** — route all interop through `_parent.InvokeMethod(...)` for unified error handling and module initialization checks.
3. **JS functions must return `void` or `boolean`** — complex state updates must happen C#-side via `dotNetRef.invokeMethodAsync` callbacks, not by returning large objects from JS.
4. **Interop calls are async but visually synchronous** — users perceive keyboard navigation as instant; avoid introducing artificial delays or multiple render cycles per keystroke.

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live codebase cross-reference + `/docs/training/06-reference/quick-reference-guides.md` risk tables.
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|--------------|------|
| Keyboard Nav + Editing (Normal) | `Tab`/`ShiftTab` inside an edited row must navigate only between editable cells (skipping primary key, invisible, disabled cells). `Enter` must save and commit. `Escape` must cancel and restore focus. `F2` must start edit on focused cell without modifier keys. | Critical |
| Keyboard Nav + Editing (Batch) | Arrow keys must NOT trigger `MoveFocusCell` when `cell.IsEdit = true` in Batch mode. `Enter` in Batch must move to next cell (save current cell). `Tab` on last cell must save and advance to next row. | Critical |
| Keyboard Nav + Filtering (FilterBar) | `ArrowUp` from first content cell / `ArrowDown` from header cell must hand focus to the correct filter bar input. `Tab` from last header cell must enter filter bar at column 0. `ShiftTab` from first content cell must land on last filter bar input. Filter template columns require separate `columnIndex` calculation. | Critical |
| Keyboard Nav + Frozen Columns | `ShiftTab` and `ArrowLeft` from movable content must trigger `sfBlazor.Grid.scrollToFocusedCell` when frozen columns exist. Focus must traverse frozen-left → movable → frozen-right in Tab order. | Critical |
| Keyboard Nav + Paging | `PageUp`/`PageDown`/`CtrlAltPageUp`/`CtrlAltPageDown`/`AltPageUp`/`AltPageDown` must only fire when pager is NOT focused and grid is NOT in edit mode. After page change, focus must reset to first content cell via `DataBound` → `FocusHandler` re-init. | High |
| Keyboard Nav + Virtualization (Row) | `CtrlHome`/`CtrlEnd`/Arrow navigation must update `VirtualScroll<T>.CurrentRowIndex` and `InfiniteScroll<T>.CurrentRowIndex` to trigger correct row range fetch. | High |
| Keyboard Nav + Virtualization (Column) | `Tab`/`ShiftTab` in a column-virtual header must resolve next cell by `orderedColumns` index, not by DOM cell index. `LastNavigatedCellIdx` must use `orderedColumns.FindIndex(x => x.Field == cell.Column.Field)`. | High |
| Keyboard Nav + Grouping | `Tab` navigation through group caption rows must be handled: caption rows are visible, have `IsCaptionRow = true`, and use `ShiftTab` boundary logic to step to the previous row's last cell. `CtrlUp`/`CtrlDown` must trigger `sfBlazor.Grid.focusNextFrame` for caption rows. | High |
| Keyboard Nav + Selection | `Space` on a checkbox cell must toggle selection. `CtrlPlusA` selects all rows. When `AllowSelection = false`, `CellClickHandler` must still invoke `Focus()` for visual feedback but skip `SelectionModule`. | High |
| Keyboard Nav + ShowAddNewRow | `Tab` from last header cell must focus AddForm first cell when `NewRowPosition = Top`. `ShiftTab` from first content cell must focus AddForm last cell. Arrow Up from first content cell / Arrow Down from header must also route to AddForm. Prevent duplicate `Tab` insertions via `previousTarget` guard in JS. | High |
| Keyboard Nav + Detail Template | `Enter` on a data cell must call `FocusFirstChild` → `sfBlazor.Grid.focusChild` to enter template children. `Tab`/`ShiftTab` inside detail template must invoke `sfBlazor.Grid.focusDetailTemplateElements`. `Escape` must clear `IsChildFocused` and return focus to parent cell. | High |
| Keyboard Nav + Editing (Dialog) | Dialog editing does not use `FocusHandler` cell navigation. `Escape` must close the dialog and return focus to the previously focused grid cell. | Medium |
| Keyboard Nav + Column Menu | `AltDown` from a header cell with `ShowColumnMenu = true` must open the column menu via `EventAggregator.Trigger("ColumnMenuUpdate")`. | Medium |
| Keyboard Nav + Export | Export does not interact with keyboard navigation at runtime. No interaction risk. | Low |
| Keyboard Nav + Infinite Scroll | `infinite-scroll.ts` adds its own `keydown` listener on the parent element. Key events that navigate past the last rendered row must allow `InfiniteScroll<T>` to load the next batch. Do not `stopPropagation` on ArrowDown in the last row. | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Keyboard Navigation
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Keyboard Navigation feature.
Scope: FocusHandler.cs, GridKeySettings.cs, and sf-grid-fn.ts (key handlers only).

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Core C# module: FocusHandler<T> in src/Internal/Actions/FocusHandler.cs
- Key binding config: GridKeySettings in src/GridKeySettings.cs
- Key resolution: e.GetKeyCombination(isMacDevice) → string → GetAction(keyCombination) → action name
- Configurable navigation keys: MoveUpCell, MoveDownCell, MoveLeftCell, MoveRightCell (comma-delimited key strings)
- Non-configurable keys handled in switch(keyCombination): CtrlHome, CtrlEnd, Home, End, Space, AltDown, Insert, Delete, F2, Escape, PageUp/Down, Ctrl+P, AltW
- JS keydown pipeline (sf-grid-fn.ts):
    * gridKeyDownHandler → DOM/clipboard (Ctrl+C, Ctrl+V), edit form Tab/Escape, filter popup guard
    * keyDownHandler → filter bar Shift+Tab fix, template cell Tab routing
    * keyActionHandler → edit form Enter/Save (triggers EndEdit on .NET side)
    * documentKeyHandler → Alt+J (grid focus), Alt+W (first cell jump)
    * gridKeyConfigs → named key config passed to KeyboardEvents module
- Focus state: cell.IsFocused, cell.TabIndex, _current (rowUid, cellUid) tuple
- DOM focus: always via sfBlazor.Grid.focus / focusFilterBar / focusAddForm (never raw JS focus())
- IsChildFocused gate: suppresses all keys except Escape when focus is inside template child element
- ShouldExitGrid: Tab from last cell and ShiftTab from first header cell release focus to browser tab order
- EventAggregator events triggered by FocusHandler: "RowStateChanged", "BeforeCellFocus", "CellFocused", "HandleNullKey"
- EventAggregator events listened by FocusHandler: "HeaderMouseDown", "EditBegin", "EditComplete", "DataBound"

=== BEFORE YOU MAKE ANY CHANGE ===
1. Read /docs/training/02-requirements-analysis/understanding-requirements.md for edge cases.
2. Read /docs/training/06-reference/quick-reference-guides.md §5 (Regression Risk Reference).
3. Confirm that the change does NOT call stopPropagation() on keys shared between gridKeyDownHandler and keyDownHandler.
4. Confirm IsChildFocused guard is preserved for all new key handlers.
5. Confirm ShouldExitGrid boundary behaviour is preserved for Tab/ShiftTab.

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all modified public members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() — use SoftRefresh = true + EventAggregator.Trigger("RowStateChanged", row)
- No direct JSRuntime.InvokeAsync — route through _parent.InvokeMethod(...)
- No direct module-to-module calls — use EventAggregator
- Key routing must always use GetKeyCombination() + GetAction(), never raw e.Key comparisons
- All new keys must be tested with isMacDevice = true (Meta replaces Ctrl)

=== SCOPE ===
{Describe the exact method(s) to implement or fix — one method per task}

=== INPUT ===
{Paste the extracted code chunk — see training/04-code-processing/optimal-chunking-strategies.md}

=== OUTPUT ===
1. Root cause / design rationale (3–5 sentences)
2. Modified method(s) only — no full-file reproduction
3. Interaction risk flags (reference Interaction Matrix above)
4. Required test cases (Given-When-Then format)

After implementation, run regression verification using:
/docs/ai-agents/prompts/regression-verification-prompt.md
```
