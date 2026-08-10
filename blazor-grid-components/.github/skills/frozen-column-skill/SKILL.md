---
name: frozen-column-skill
description: Expert knowledge for the Frozen Column and Frozen Row feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to frozen/pinned column and row behaviour, including left/right/fixed freeze modes, freeze line moving, row-height synchronization, stacked header freeze, column virtualization with freeze, and cross-feature interaction guarantees with sorting, filtering, grouping, editing, virtualization, infinite scroll, selection, paging, scroll management, and row drag-and-drop.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for the Frozen Column and Frozen Row feature in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**  
When working on frozen column or frozen row bugs, or implementing freeze-related features, load this skill first before consulting architecture files. Use it to understand freeze mode resolution, freeze line moving via drag, row-height synchronization between frozen and movable panes, and the JS/C# split for all freeze DOM operations.

---

## Knowledge References
<!-- token-budget: 60 words -->

**Training Files Consulted**:
- `/docs/training/00-START-HERE.md`
- `/docs/training/TRAINING-INDEX.md`
- `/docs/training/01-getting-started/architecture-overview.md`

**Architecture Files**:
- `/docs/architecture/system-architecture.md`
- `/docs/code-guidelines/coding-standards.md`

**Live Source Locations**:
- `scripts/freeze.ts` — frozen/movable pane row-height sync, wrap-height calculation, stacked header height, `setFrozenHeight`, CSS left-position updates for `e-leftfreeze` cells
- `scripts/frozen-drag-drop.ts` — freeze line drag-to-move (`FrozenDD`), pointer event wiring, border index resolution, left/right/fixed column re-ordering via drag
- `scripts/sf-grid-fn.ts` — `SfGrid` class: `freezeModule`, `frozenDragDropModule`, `frozenColumnModel`, `updateColumnLevelFrozen()`, `getOrderedFrozenColumns()`, `autofitFrozenColumns()`, `freezeLineMovedAction()`, `updateFixedcolumns()`, `checkFixedColumns()`, `destroy()` cleanup
- `scripts/sf-grid.ts` — `freezeModule` lifecycle orchestration, `frozenColumns` option sync after freeze-line move, `setFrozenHeight` call paths, `isFreezeLineMoved` flag handling
- `scripts/interfaces.ts` — `GridOptions.frozenColumns`, `frozenRows`, `frozenName` (`freezeTable`), `allowFreezeLineMoving`, `isFreezeLineMoved`, `actualFrozenColumns`, `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount`, `isFixedColumnPresent`; `Column.freeze`, `Column.isFrozen`; `FreezeLineMovingClientOptions` interface
- `scripts/width-controller.ts` — `ColumnWidthService.setWidthToTable()`: freeze + column-virtualization special guard for autofit; frozen column autofit ordering
- `scripts/reorder.ts` — `updateFrozenScrollPosition()`, `updateColumnLevelFrozen()` call on drop, `frozenColumnModel` usage for column reorder across freeze boundary
- `src/SfGrid.Properties.cs` — `FrozenColumns` (int), `FrozenRows` (int), `AllowFreezeLineMoving` (bool) — all `[JsonPropertyName]`-decorated `[Parameter]` properties
- `src/GridColumn.cs` — `IsFrozen` (bool), `Freeze` (`FreezeDirection`, default `Left`), `FixedColumn` (bool — incompatible with frozen), `EnableFixedLeftFreezeLineCursor` / `EnableFixedRightFreezeLineCursor` (internal cursor flags); `PropertyChanges` propagation: `IsFrozen`/`Freeze` → `"FrozenColumns"` on parent; `FixedColumn` has its own key
- `src/Enumeration/GridsEnumerations.cs` — `FreezeDirection` enum: `Left`, `Right`, `Fixed`
- `src/EventModels/Grids.cs` — `FrozenColumnsUidCollection`, `IsFrozen` in event args

---

## Training Insights Applied
<!-- token-budget: 80 words -->

1. **Hybrid DOM concern**: Frozen column layout is split — C# resolves which columns are frozen and serializes `frozenColumns`/`frozenName`/`actualFrozenColumns` to JS options; JS (`freeze.ts`, `frozen-drag-drop.ts`, `sf-grid-fn.ts`) manages all DOM measurement, row-height equalization, and pointer-event-driven freeze-line moving.
2. **Three freeze names**: `frozenName` can be `"Left"`, `"Right"`, or `"LeftRight"`. When `"LeftRight"`, three separate table panes exist (left frozen, movable, right frozen); all row-height sync loops must handle the third pane (`frRows`).
3. **`IsFrozen` vs `FrozenColumns`**: Column-level `IsFrozen`/`Freeze` properties are the modern API; grid-level `FrozenColumns` (count-based) is the legacy API. Both ultimately produce the same JS `frozenColumns` option but must be tracked via separate `PropertyChanges` keys.
4. **`PropertyChanges` bubbling**: Changes to `IsFrozen` or `Freeze` on a `GridColumn` automatically add `"FrozenColumns"` to the parent grid's `PropertyChanges` dictionary — do not add it a second time. `FixedColumn` changes have their own separate `PropertyChanges` key.
5. **`FixedColumn` is incompatible with `IsFrozen`**: `GridColumn.FixedColumn = true` pins a column at the start of the column model order and blocks reordering/grouping — it is explicitly documented as incompatible with the frozen column feature. Never combine both on the same column.
6. **No `StateHasChanged()` directly**: Use internal render scheduling; frozen-column refresh is triggered via `EventAggregator` or `PropertyChanges` detection in `OnAfterRenderAsync`.
7. **`AllowFreezeLineMoving` is a C# `[Parameter]`**: `SfGrid.AllowFreezeLineMoving` maps to `options.allowFreezeLineMoving` in JS. `FrozenDD` only wires pointer events when this flag is `true`. Always verify this guard before adding freeze-line drag logic.
8. **`isFreezeLineMoved` flag + `freezeLineMovedAction()`**: After a freeze-line drag completes, JS sets `options.isFreezeLineMoved = true`. On the next render, `sf-grid-fn.ts` detects this flag, calls `freezeLineMovedAction()` which resets the flag, recalculates column-level frozen state via `updateColumnLevelFrozen()`, resets `movableContent.scrollLeft = 0`, and calls `ColumnWidthService.setWidthToTable()`.
9. **Four granular frozen count options**: `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount` are separate from `frozenColumns`. `FreezeLineMovingClientOptions` carries all four back to C# after a drag. Any code computing "how many columns are frozen" must use the correct count field for the direction.
10. **`frozenColumnModel` is the ordered column array**: `updateColumnLevelFrozen()` in `sf-grid-fn.ts` rebuilds `frozenColumnModel` as `[leftFrozen…, movable…, rightFrozen…]`. Column reorder (`reorder.ts`) uses this model for drop-index calculation — never read `columnModel` directly when freeze is active.
11. **Row-height equalization is JS-only**: `setWrapHeight()` in `freeze.ts` reads `getBoundingClientRect().height` for each row in both frozen and movable panes and sets the taller value on both — this must never be replicated in C#.
12. **Column virtualization + freeze**: When both `EnableColumnVirtualization` and `FrozenColumns > 0`, the movable scrollbar width uses a special path in `setFrozenHeight()` (total visible column widths + scrollbar gutter). `ColumnWidthService.setWidthToTable()` also has a freeze+virtualization guard — only runs for autofit or resize, not on every render.

---

## Code Location Map
<!-- token-budget: 80 words -->

| File | Responsibility |
|------|----------------|
| `scripts/freeze.ts` | `Freeze` class (line 8): row-height sync (`setWrapHeight` line 74), frozen content height (`setFrozenHeight` line 133), stacked header equalization (`refreshStackedHdrHgt` line 210), wrap-height clear, CSS left-position update for `e-leftfreeze` cells (`updateLeftFreezePosition` line 311, `updateFrozenColumnStyles` line 306) |
| `scripts/frozen-drag-drop.ts` | `FrozenDD` class (line 30): freeze line drag-to-move (`dragStart` line 70), pointer event wiring, border index resolution, visible column ordering for left/right/fixed modes, `preventFreezeLineMoving` line 176 |
| `scripts/sf-grid-fn.ts` | `SfGrid` class: `freezeModule` + `frozenDragDropModule` instantiation; `frozenColumnModel` line 48 (ordered left+movable+right); `updateColumnLevelFrozen()` line 292 rebuilds model; `getOrderedFrozenColumns()` for autofit ordering; `autofitFrozenColumns()` line 231 returns field list by freeze direction; `isFreezeLineMoved` detection → `freezeLineMovedAction()` line 246 reset + scroll reset; `updateFixedcolumns()` + `checkFixedColumns()` for `FixedColumn` ordering; `destroy()` cleanup |
| `scripts/sf-grid.ts` | `freezeModule` lifecycle orchestration; syncs `frozenColumns`/`actualFrozenColumns` after freeze-line drag; triggers `setFrozenHeight` on resize/textwrap; detects `isFreezeLineMoved` flag; JS interop entry points: `preventFreezeLineMoving` line 120, `freezeLineMovedActions` line 127, `frozenHeight` line 142 |
| `scripts/interfaces.ts` | `GridOptions`: `frozenColumns`, `frozenRows`, `frozenName`, `allowFreezeLineMoving`, `isFreezeLineMoved`, `actualFrozenColumns`, `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount`, `isFixedColumnPresent`; `Column.freeze`, `Column.isFrozen`; `FreezeLineMovingClientOptions` (post-drag payload) |
| `scripts/width-controller.ts` | `ColumnWidthService.setWidthToTable()` line 341: freeze + virtualization guard — only runs autofit column width calculation when `!frozenColumns` OR (frozen + not virtualization + autoFit columns exist) OR (frozen + virtualization + resize module); freeze-aware column autofit ordering |
| `scripts/reorder.ts` | `updateFrozenScrollPosition()` line 487 during column header drag; calls `updateColumnLevelFrozen()` on drop; uses `frozenColumnModel` for drop-index calculation; `frozenName !== 'None'` guard on all reorder paths |
| `src/SfGrid.Properties.cs` | `FrozenColumns` line 818 (int), `FrozenRows` line 852 (int), `AllowFreezeLineMoving` line 835 (bool) — all `[JsonPropertyName]`-decorated `[Parameter]` properties |
| `src/GridColumn.cs` | `IsFrozen` line 547 (bool), `Freeze` line 678 (`FreezeDirection`, default `Left`), `FixedColumn` line 588 (bool — incompatible with frozen; pins column order, blocks reorder/grouping), `EnableFixedLeftFreezeLineCursor` / `EnableFixedRightFreezeLineCursor` (internal drag cursor flags for `Fixed` direction); `PropertyChanges` bubble: `IsFrozen`/`Freeze` → parent `"FrozenColumns"`; `FixedColumn` has own key |
| `src/Enumeration/GridsEnumerations.cs` | `FreezeDirection` enum line 333: `Left`, `Right`, `Fixed` |
| `src/EventModels/Grids.cs` | `FrozenColumnsUidCollection`, `IsFrozen` in column-change event args |
| `src/Internal/Actions/Freeze.cs` | C# freeze action handlers: `InvokeFreezeLineMoving` line 1042, `InvokeFreezeLineMoved` line 1059, `InvokeClientFrozenHeight` line 1151; orchestrates JS interop calls for freeze-line drag lifecycle |
| `Internal/Base/GridJSInteropAdaptor.cs` | JS-to-.NET callback bridge for freeze-line move results (`FreezeLineMovingClientOptions` payload) |

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Interop Calls**:

| C# Method Call | TypeScript Entry Point | Responsibility |
|----------------|------------------------|----------------|
| `InvokeMethod("sfBlazor.Grid.preventFreezeLineMoving", ...)` (Freeze.cs line 1052, 1056) | `preventFreezeLineMoving(dataId, isCancel)` (sf-grid.ts line 120) → `FrozenDD.preventFreezeLineMoving(isCancel)` (frozen-drag-drop.ts line 176) | Cancel or allow freeze-line drag based on `FreezeLineMoving` event result; controls pointer event continuation |
| `InvokeMethod("sfBlazor.Grid.freezeLineMovedActions", ...)` (Freeze.cs line 1131) | `freezeLineMovedActions(dataId, freezeLineMovingClientOptions)` (sf-grid.ts line 127) | Sync freeze counts after drag completes; updates `frozenColumns`, `actualFrozenColumns`, `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount`; calls `setFrozenHeight()` |
| `InvokeMethod("sfBlazor.Grid.frozenHeight", ...)` (Freeze.cs line 1155) | `frozenHeight(dataId, options)` (sf-grid.ts line 142) → `Freeze.setFrozenHeight()` (freeze.ts line 133) | Recalculate frozen/movable pane heights and scrollbar widths after data/option changes; handles column virtualization special case |

**Responsibility Split**:

| Concern | C# (Freeze.cs) | TypeScript (freeze.ts, frozen-drag-drop.ts) |
|---------|----------------|---------------------------------------------|
| **Freeze mode resolution** | ✅ Resolves `FrozenColumns` count vs `IsFrozen`/`Freeze` per-column; serializes `frozenColumns`, `frozenName`, `actualFrozenColumns` to JS options | ❌ Consumes pre-resolved options; does not compute freeze mode |
| **Freeze-line drag detection** | ❌ Does not track pointer events | ✅ `FrozenDD` wires pointer events; detects drag start/move/stop; resolves drop border index |
| **Row-height equalization** | ❌ Never reads `getBoundingClientRect()` | ✅ `setWrapHeight()` measures all row heights in frozen/movable/right-frozen panes; sets taller value on both |
| **Scrollbar width calculation** | ❌ Does not manipulate scrollbar DOM | ✅ `setFrozenHeight()` computes movable scrollbar width from total column widths + gutter; handles column virtualization case |
| **Event lifecycle orchestration** | ✅ Fires `FreezeLineMoving`/`FreezeLineMoved` events; calls `StateHasChanged`; updates `FrozenColumns` parameter; manages batch edit save | ❌ No event firing; only DOM measurement and manipulation |

**CRITICAL RULES**:
1. **Never replicate row-height sync in C#** — `setWrapHeight()` must remain JS-only; C# triggers refresh via `InvokeMethod("frozenHeight")` but does not measure DOM
2. **All pointer events in JS** — `FrozenDD` exclusively owns freeze-line drag; C# only provides cancellation signal via `preventFreezeLineMoving`
3. **Six count fields must sync** — After `freezeLineMovedActions` completes, `frozenColumns`, `actualFrozenColumns`, `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount` in JS must match C# state
4. **`isFreezeLineMoved` flag contract** — Set by JS after drag; detected by `sf-grid-fn.ts`; reset inside `freezeLineMovedAction()`; C# sets `IsFreezeLineMoved = true` to trigger `StateHasChanged`

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live codebase + training docs cross-reference. Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| Frozen + Column Virtualization | `setFrozenHeight()` uses special scrollbar width path when both are active; movable child scrollbar width must account for all virtual column widths plus scrollbar gutter | Critical |
| Frozen + Scroll Management | Horizontal scroll must move only the movable pane; frozen pane must remain stationary; `setPadding()` in `scroll.ts` must be called after `setFrozenHeight()` | Critical |
| Frozen + Sorting | Frozen pane and movable pane sort indicator headers must update together; sort does not alter frozen column layout but must not reset row heights | High |
| Frozen + Filtering | Filter bar cells exist in both frozen and movable panes; filter apply must not disrupt row-height equalization | High |
| Frozen + Text Wrap | `refreshFreeze({ case: 'textwrap' })` must be triggered when `AllowTextWrap` or `WrapMode` changes; row-height equalization loop covers all three panes when `frozenName === 'LeftRight'` | High |
| Frozen + Grouping | Group caption rows span both panes; row-height sync must iterate caption rows in frozen content as well | High |
| Frozen + Editing | In-line and dialog edit cells in the frozen pane must be reachable via Tab key; edit row height must sync across panes | High |
| Frozen + Row Drag-and-Drop | Dragging rows across frozen/movable boundary must use `FrozenDD` collision detection; drop index calculation must account for frozen column offset | High |
| Frozen + Stacked Headers | `refreshStackedHdrHgt()` must align stacked header row heights for frozen, movable, and right-frozen panes; rowspan calculation loop covers all three panes | High |
| Frozen + Resize | After column resize, `updateResizeHandler()` syncs resize handle heights across frozen and movable panes; `setFrozenHeight()` recalculates movable scrollbar width | Medium |
| Frozen + Selection | Row selection highlight must span all three panes simultaneously; checkbox selection column is typically frozen-left and must not be de-frozen by `IsFrozen` changes | Medium |
| Frozen + Paging | Page change resets scroll to top; frozen row-height sync must re-run after new page data renders | Medium |
| Frozen + Infinite Scroll | Infinite scroll is incompatible with `FrozenRows > 0`; frozen column with infinite scroll requires `setFrozenHeight()` re-run after each block load | Medium |
| Frozen + Detail Rows | Expanding detail row adds height; frozen pane must sync that row's height via `setWrapHeight` | Medium |
| Frozen + Column Reorder | `reorder.ts` uses `frozenColumnModel` for drop-index; calls `updateColumnLevelFrozen()` after drop; drag over frozen boundary uses `updateFrozenScrollPosition()`; `FixedColumn` blocks reorder entirely | High |
| Frozen + AutoFit | `autofitFrozenColumns()` returns field list ordered Left→Fixed→Right; `ColumnWidthService.setWidthToTable()` guard must be passed — skipped when frozen+virtualization unless triggered by resize module | Medium |
| Frozen + FixedColumn | `FixedColumn = true` is explicitly incompatible with `IsFrozen`; `updateFixedcolumns()` reorders `columnModel` putting fixed columns first; never set both on same column | High |

---

## Prompt Template
<!-- token-budget: 300 words -->

**Mode**: {feature-implementation | bug-fix}  
**Skill**: Frozen Column / Frozen Row  

**Context Setup**:  
Before implementing any frozen column change, consult ALL of the following:
1. Read `/docs/training/01-getting-started/architecture-overview.md` — Frozen column is a Layer 1 (JS DOM) + Layer 3 (C# action) hybrid concern.
2. Confirm the freeze mode in use: grid-level `FrozenColumns` (count-based) vs column-level `IsFrozen`/`Freeze` (per-column API) — both ultimately drive the JS `frozenColumns` option but follow different `PropertyChanges` propagation paths.
3. Determine `frozenName` value (`"Left"`, `"Right"`, `"LeftRight"`) — `"LeftRight"` requires all loops to handle three panes (`fRows`, `mRows`, `frRows`).

**Implementation Checklist**:
- [ ] JS DOM operations (row-height sync, scrollbar width, freeze-line drag) are in `freeze.ts` and `frozen-drag-drop.ts` — **never replicate in C#**
- [ ] All JS-to-.NET callbacks flow through `GridJSInteropAdaptor.cs`
- [ ] `IsFrozen` or `Freeze` changes on `GridColumn` must bubble to parent grid's `PropertyChanges["FrozenColumns"]` — do not add a duplicate key; `FixedColumn` changes use their own key
- [ ] Never combine `FixedColumn = true` and `IsFrozen = true` on the same column — they are explicitly incompatible
- [ ] Column virtualization + frozen: verify `setFrozenHeight()` special path is entered (`frozenColumns > 0 && height === '100%' && enableColumnVirtualization`)
- [ ] `"LeftRight"` freeze mode: all row-height loops must include `frRows` (right frozen pane)
- [ ] After freeze-line drag, `FreezeLineMovingClientOptions` payload updates `frozenColumns`, `actualFrozenColumns`, `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount` — all six must sync
- [ ] `isFreezeLineMoved` flag is reset inside `freezeLineMovedAction()` — do not reset it elsewhere; `movableContent.scrollLeft` is also reset to 0 in that method
- [ ] `frozenColumnModel` must be rebuilt via `updateColumnLevelFrozen()` after any freeze change before column reorder drop-index calculation
- [ ] Text wrap change must trigger `refreshFreeze({ case: 'textwrap' })` — do not call `setWrapHeight` directly
- [ ] `AllowFreezeLineMoving` (C# param) → `allowFreezeLineMoving` (JS option) must be `true` before `FrozenDD` wires pointer events — verify this guard before adding drag logic
- [ ] AutoFit with freeze: use `autofitFrozenColumns()` for field ordering (Left→Fixed→Right); ensure `ColumnWidthService.setWidthToTable()` freeze+virtualization guard passes

**Regression Testing Required**:
- Freeze left + horizontal scroll: frozen pane stays fixed, movable pane scrolls
- `frozenName = 'LeftRight'`: row heights equal across all three panes after data load
- Column virtualization + freeze: scrollbar child width recalculates correctly at 100% height
- Freeze line drag: all six count fields (`frozenColumns`, `actualFrozenColumns`, `frozenLeftCount`, `frozenRightCount`, `frozenLeftColumnsCount`, `frozenRightColumnsCount`) sync in JS + C# after drag; `movableContent.scrollLeft` resets to 0
- `isFreezeLineMoved` flag: resets to `false` after `freezeLineMovedAction()` runs; `frozenColumnModel` is rebuilt
- Text wrap + freeze: `refreshFreeze` re-equalizes row heights in all panes
- Stacked header + freeze: stacked header row heights sync correctly via `refreshStackedHdrHgt`
- Resize + freeze: resize handle heights update after `setFrozenHeight` call
- Column reorder + freeze: drop-index calculation uses `frozenColumnModel`; frozen boundary drag triggers `updateFrozenScrollPosition`
- AutoFit + freeze: `autofitFrozenColumns()` returns correct Left→Fixed→Right field order; `setWidthToTable` guard passes for non-virtual freeze
- `FixedColumn = true`: column reorders to front of `columnModel`, cannot be dragged or grouped; combining with `IsFrozen` is blocked

**Code Path References**:
- Freeze module init: `sf-grid-fn.ts → SfGrid constructor` → `this.freezeModule = new Freeze(this)` + `this.frozenDragDropModule = new FrozenDD(this)`
- Row-height sync: `freeze.ts → Freeze.refreshFreeze({ case })` → `setWrapHeight()` → `getBoundingClientRect().height` per row
- Frozen height/scrollbar: `freeze.ts → Freeze.setFrozenHeight()` → `movableChildScrollBarX.style.width`
- Freeze line drag: `frozen-drag-drop.ts → FrozenDD.dragStart()` → `dragMove()` → `dragStop()` → JS-interop callback → `FreezeLineMovingClientOptions` payload
- Post-drag state reset: `sf-grid-fn.ts → options.isFreezeLineMoved` detected → `freezeLineMovedAction()` → `updateColumnLevelFrozen()` + scroll reset + `ColumnWidthService.setWidthToTable()`
- Ordered column model rebuild: `sf-grid-fn.ts → updateColumnLevelFrozen()` → `frozenColumnModel = [left…, movable…, right…]`
- Column `IsFrozen`/`Freeze` change: `GridColumn.OnParametersSetAsync()` → `PropertyChanges["FrozenColumns"]` bubbled to parent grid
- `FixedColumn` change: `GridColumn.OnParametersSetAsync()` → own `PropertyChanges["FixedColumn"]` key → `updateFixedcolumns()` reorders `columnModel`
- Left-freeze CSS positions: `freeze.ts → Freeze.updateFrozenColumnStyles()` → `updateLeftFreezePosition()` → `cells[j].style.left`
- AutoFit with freeze: `sf-grid-fn.ts → autofitFrozenColumns()` returns `[left fields…, movable fields…, right fields…]` → passed to `ColumnWidthService`
- Column reorder over freeze boundary: `reorder.ts → dragStop()` → `updateColumnLevelFrozen()` + `frozenColumnModel` for drop-index → `updateFrozenScrollPosition(x, cliRect)`
```
