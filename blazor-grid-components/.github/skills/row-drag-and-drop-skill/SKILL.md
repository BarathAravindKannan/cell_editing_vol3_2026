---
name: row-drag-and-drop-skill
description: Expert knowledge for the Row Drag and Drop feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to row drag-and-drop behaviour, including within-grid reorder, cross-grid transfer, grouping-aware drop, virtualization-aware index resolution, edit-mode interplay, and cross-feature interaction guarantees with sorting, filtering, grouping, paging, selection, virtualization, infinite scroll, frozen rows/columns, and detail rows.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Row Drag and Drop in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**

Load this skill when a task involves:
- `AllowRowDragAndDrop`, `GridRowDropSettings`, `TargetID`, or `AllowEmptyAreaDrop`
- Events: `RowDragStarting`, `RowDropping`, `RowDropped`
- Cross-grid row transfer (`RowDropSettings.TargetID`)
- The `RowReorder<T>` action module or `RowDD` TypeScript class
- Bug reports about row index mismatches, ghost drag elements, drag-cursor state, or drop position borders

---

## Knowledge References
<!-- token-budget: 60 words -->

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/architecture/data-flow.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`
- `docs/tech-stack/tech-stack.md`

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **#1 Rule**: Never break existing behavior — row drag-and-drop shares data mutation paths with editing, grouping, and selection. Every code change must be regression-checked against all interaction matrix entries below.
- All `await` calls must use `.ConfigureAwait(true)` — drag completion invokes `ReorderRows` which calls `DataManager.SaveChanges` asynchronously on the Blazor sync context.
- Modules must never call each other directly; use the `Parent` reference (`_parent.GroupModule`, `_parent.SelectionModule`).
- Guard clauses always before state mutation (e.g., confirm `_parent.IsEdit` and close/save before allowing the drop to proceed).
- `RowDropping.Cancel = true` must suppress the `RowDropped` event and abort any data mutation — verified in `RowReorder.cs` lines 427–433 (sets `_isDropCancelled = true` and returns early).
- Never use `dynamic` in event arg models; `RowDragStartingEventArgs<T>`, `RowDroppingEventArgs<T>`, and `RowDroppedEventArgs<T>` are all strongly typed generic classes.
- JS-side drag state (`RowDD` class in `row-reorder.ts`) calls `.invokeMethodAsync('ReorderRows', ...)` and `.invokeMethodAsync('RowDragStartEvent', ...)` — changes to the C# method signatures require a matching update in `row-reorder.ts`.
- `AllowEmptyAreaDrop` defaults to `true`; its JS counterpart is `gObj.options.allowEmptyAreaDrop` — both sides must remain in sync when serializing `IGridOptions`.

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Description |
|------|-------------|
| `src/Internal/Actions/RowReorder.cs` | C# action module — `RowReorder<T>`. Key methods: `ReorderRows` (line 364), `ReorderRowsAction` (line 174), `RowDragStartEvent` (line 480). Event-pipeline logic for `RowDragStarting`, `RowDropping`, `RowDropped`. |
| `src/GridRowDropSettings.cs` | `GridRowDropSettings` child component — `TargetID`, `AllowEmptyAreaDrop` parameters; registers itself via `Parent.UpdateChildProperties`. |
| `src/SfGrid.Properties.cs` | `AllowRowDragAndDrop` property (`[Parameter]`, line 207) and `RowDropSettings` property (`[Parameter]`, line 1024). |
| `src/GridEvents.cs` | Event callbacks: `RowDragStarting` (line 2033), `RowDropping` (line 2062), `RowDropped` (line 2090). |
| `src/EventModels/Grids.cs` | Event arg classes: `RowDragStartingEventArgs<T>` (line 4643), `RowDroppingEventArgs<T>` (line 4666), `RowDroppedEventArgs<T>` (line 4759). |
| `scripts/row-reorder.ts` | Client-side `RowDD` class — `helper`, `dragStart`, `drag`, `dragStop`, `columnDrop`, `processDragStop`. Invokes `ReorderRows` and `RowDragStartEvent` via `dotNetRef.invokeMethodAsync`. |
| `scripts/sf-grid-fn.ts` | Instantiates `RowDD` as `rowDragAndDropModule`; exposes `initializeDrag` and `destroy` lifecycle. |
| `scripts/content-drag-drop.ts` | Handles content-area drag events; coordinates with `row-reorder.ts` for drop border rendering. |
| `src/Internal/Actions/Data.cs` | `DataGenerator<T>` — called inside `ReorderRowsAction` via `_parent.DataManager.SaveChanges` for local reorder. |

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**Call Flow: JS → C#**

| TypeScript (row-reorder.ts) | C# Target (RowReorder.cs) | Purpose |
|------------------------------|----------------------------|---------|
| `dotNetRef.invokeMethodAsync('RowDragStartEvent', fromIdx, dragUid)` (line 136) | `RowDragStartEvent(int FromIndex, string Uid)` (line 480) | Fires `RowDragStarting` event when drag begins; resolves `DragRow` from `Uid` |
| `dotNetRef.invokeMethodAsync('ReorderRows', fromIdx, toIdx, action, isDragWithinGrid, targetClass, targetID, positions, destInstance, outsideGrid, differentTValue, fromUid, toUid, withinBothGrid, clientX, clientY)` (lines 473, 514, 558, 997, 998, 1003) | `ReorderRows(int fromIndex, int toIndex, ...14 parameters)` (line 364) | Executes row reorder logic: raises `RowDropping` event, mutates datasource, raises `RowDropped` event |

**Responsibility Split**

| Layer | Owns |
|-------|------|
| **TypeScript (RowDD)** | DOM drag events, helper clone rendering, drop border styling (`e-dragtop`/`e-dragbottom`), drag cursor state, cross-grid element detection, viewport position (`clientX`/`clientY`) |
| **C# (RowReorder<T>)** | Event pipeline (`RowDragStarting` → `RowDropping` → `RowDropped`), datasource mutation (`DataManager.SaveChanges`), validation (edit mode, grouping, paging offsets), index resolution (virtualization, paging, grouping), cross-grid record transfer |

**CRITICAL Rules**
1. **Method Signature Sync**: Any change to `ReorderRows` or `RowDragStartEvent` C# signatures requires matching update in `row-reorder.ts` `invokeMethodAsync` calls.
2. **Parameter Order**: `ReorderRows` accepts 15 parameters; TypeScript calls must pass them in exact order (positional, not named).
3. **Cancel Propagation**: If `args.Cancel = true` in `RowDropping` event, C# sets `_isDropCancelled = true` (line 430); TypeScript must not render drop changes.
4. **Grid Instance Validity**: `destInstance` parameter passes target grid's `DotNetObjectReference`; must be non-null for cross-grid transfers.

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

> Built from live codebase analysis + architecture/training cross-reference.
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| RowDragAndDrop + Grouping | Drop target must resolve to correct group caption boundary; dashed-border styling (`e-dragtop`/`e-dragbottom`) must wrap the correct group rows; cross-group drops must re-associate the row's `ParentUid`; all-columns-grouped state must block drag | Critical |
| RowDragAndDrop + Virtualization | `toIndex` computation uses `VirtualScrollModule.GeneratedData` or `CurrentViewDataLookup` for remote/local virtual sources; `fromIndex`/`toIndex` must be absolute datasource indices, not viewport-relative row indices | Critical |
| RowDragAndDrop + Editing (Normal/Batch) | Active `Normal` edit must be closed (`CloseEdit`) before drag proceeds; active `Batch` cell must be saved (`SaveCell`) and validation errors must abort the drag; `HasAddedRecord` flag adjusts `selectedDataIndex` when a newly-added row precedes the drop target | Critical |
| RowDragAndDrop + Selection | Multi-row drag derives dragged records from `GetSelectedRecordsAsync`; `SelectedIndexes` must include `fromIndex` for a valid drag; single-row drag when row is not selected falls back to `CurrentViewRecords[fromIndex]` | High |
| RowDragAndDrop + Paging | `targetIndex` in `columnDrop` is offset by `(currentPage * pageSize) - pageSize` to convert viewport index to datasource index; must re-test when `AllowPaging` toggles | High |
| RowDragAndDrop + Infinite Scroll (cached) | `EnableInfiniteScrolling && EnableCache` path skips the `CurrentRecords[fromIndex]` fallback; record resolution must use `_rowsToReorder` staging list | High |
| RowDragAndDrop + Frozen Rows/Columns | `isFrozenRowsAndColumn` flag in `drag` handler forces `moveDragRows` even when `startedRow === dragTarget`; frozen row/column layout must not shift during drag | High |
| RowDragAndDrop + Detail Rows | `helper` removes `e-detailrowcollapse`/`e-detailrowexpand` cells from the clone; drag from within a detail-row grid targets its parent's `e-detailrow` container | Medium |
| RowDragAndDrop + Aggregates | Aggregates recalculate after the datasource mutation triggered by `DataManager.SaveChanges`; `ReactiveAggregateModule` listens on `DataBound` — no special handling needed unless remote adaptor is used | Medium |
| RowDragAndDrop + Column Reorder | Column reorder drag (`Reorder<T>`) and row drag (`RowDD`) both attach to grid content; `processStackedColumns` is called inside the `drag` handler to guard the all-columns-grouped blocking condition | Medium |
| RowDragAndDrop + RTL | `queryPositionInfo` in `helper` adjusts `left` position when `enableRtl = true` and `rowDropTarget` is absent; must not regress on RTL layouts | Medium |
| RowDragAndDrop + Cross-Grid Transfer | `RowDropSettings.TargetID` must reference a valid mounted grid element; `tValue` equality check gates whether records are moved (`add`+`delete`) or deleted only (different `TValue`); `e-allowRowDrop` CSS class must be added/removed on the target grid's content | Critical |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Row Drag and Drop
Component: SfGrid<TValue> — Syncfusion Blazor DataGrid

## Context
Row Drag and Drop allows users to reorder rows within a grid or transfer rows to another grid.
Enabled via AllowRowDragAndDrop = true on SfGrid.
Cross-grid transfer configured via <GridRowDropSettings TargetID="targetGridId" />.
AllowEmptyAreaDrop (default true) controls whether rows can be dropped in empty content areas.

## Key Files (read before making any changes)
- src/Internal/Actions/RowReorder.cs          ← C# action module
- src/GridRowDropSettings.cs                  ← settings child component
- src/SfGrid.Properties.cs                    ← AllowRowDragAndDrop, RowDropSettings parameters
- src/GridEvents.cs                           ← RowDragStarting, RowDropping, RowDropped events
- src/EventModels/Grids.cs                    ← event arg models
- scripts/row-reorder.ts                      ← JS drag/drop logic (RowDD class)
- scripts/sf-grid-fn.ts                       ← module instantiation (rowDragAndDropModule)

## Mandatory Rules
1. Always consult docs/training/ before writing code — especially architecture-overview.md.
2. JS method calls (invokeMethodAsync) must match C# [JSInvokable] method signatures exactly.
3. AllowEmptyAreaDrop changes must be reflected in both GridRowDropSettings.cs (C#) and options serialized to JS (IGridOptions interface).
4. Action event pipeline: raise RowDragStarting → allow cancel → raise RowDropping → allow cancel → mutate data → raise RowDropped.
5. Edit-mode interplay: close Normal edit / save Batch cell before permitting drop; abort if validation fails.
6. Virtualization: resolve fromIndex/toIndex against GeneratedData (virtual) or CurrentViewData (standard); never use DOM rowIndex directly.
7. Paging: offset targetIndex by (currentPage * pageSize) - pageSize in columnDrop.
8. Grouping: check DragRow.ParentUid === DropRow.ParentUid for same-group block; handle group caption row drops.
9. Cross-grid: verify RowDropSettings.TargetID matches a live grid element; check tValue equality to gate move vs. delete-only path.
10. All awaited calls must use .ConfigureAwait(true). No fire-and-forget.

## Regression Checklist (run after every change)
- Within-grid single-row reorder (local datasource)
- Within-grid multi-row reorder (multi-select)
- Cross-grid transfer (same TValue)
- Cross-grid transfer (different TValue — delete only)
- Drop blocked during active Normal edit
- Drop blocked during active Batch edit with validation error
- Reorder with AllowGrouping + grouped column
- Reorder with EnableVirtualization (local + remote)
- Reorder with AllowPaging
- Reorder with FrozenRows
- AllowEmptyAreaDrop = false: drop into empty area is blocked
- RowDropping.Cancel = true: RowDropped must NOT fire, data must NOT change
- RTL layout: drag clone position correct
```
