---
name: autofit-skill
description: Expert knowledge for the Column AutoFit feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to auto-sizing column widths to fit content, including programmatic APIs, column menu triggers, double-click resize handle activation, initial render auto-fit, persistence, and cross-feature interaction guarantees with frozen columns, virtualization, grouping, stacked headers, editing, infinite scroll, and paging.
---

# Skill Instructions

**Purpose**  
Expert knowledge for the Column AutoFit feature in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature when implementing auto-fit.

**Agent Invocation**  
Load this skill when working on AutoFit bugs, implementing auto-fit triggers, or handling AutoFit interactions with other features. Use it to understand the complete AutoFit pipeline: C# API → JS bridge → DOM measurement → column width calculation → table layout synchronization (across frozen/movable/right panes) → persistence.

---

## Knowledge References

**Live Source Locations** (Column AutoFit Feature):

- `src/SfGrid.Methods.cs` (lines 121-226) — `AutoFitColumnsAsync()` (all columns at line 121), `AutoFitColumnsAsync(string[])` (specific at line 188), `AutoFitColumnAsync(string)` (single at line 225), `IsAutoFitEnabled` flag management
- `src/SfGrid.Properties.cs` (lines 1328-1334) — `AutoFit` property (bool, `[JsonPropertyName("autoFit")]` at line 1333)
- `src/GridColumn.cs` (lines 138-140, 518) — `AutoFit` property (bool at line 138), `IsPersistAutoFit` property (bool at line 518), `PropertyChanges` tracking
- `src/SfGrid.Lifecycle.cs` (lines 355-357) — `OnAfterRenderAsync()`: persistence check, `_isPersistAutoFit` flag, re-invoke `autoFitColumns` after data load
- `src/SfGrid.razor.cs` (lines 79, 347, 643, 1628-1637) — `_isPersistAutoFit` field (line 79), `IsAutoFitEnabled` flag (line 347), `GetTableStyles()` method (line 643) width validation, `InvokeViewRefresh()` method (line 1628) conditions with `IsAutoFitEnabled`
- `src/Internal/Renderer/ColumnMenu.razor` (lines 531-540) — "AutoFit" (single, line 531) and "AutoFitAll" (all, line 537) menu item click handlers
- `src/Internal/Base/GridJSInteropAdaptor.cs` (lines 144-216) — **[JSInvokable] ColumnWidthChanged()** method (line 144): centralized JS callback receiver for all column width changes including AutoFit; deserializes args, updates column models via `ColumnWidthChanger()` (line 160), stores persistence (TableWidth/LeftFrozenTableWidth/RightFrozenTableWidth at lines 183-188), triggers ResizeStopped event
- `scripts/resize.ts` — `autoFitColumns()`, `resizeColumn()`, `createTable()` (off-screen measurement), `callAutoFit()` (double-click handler)
- `scripts/sf-grid.ts` — `autoFitColumns()` JS bridge entry point
- `scripts/sf-grid-fn.ts` — `getColumns()`, `getOrderedFrozenColumns()`, `autofitFrozenColumns()`
- `scripts/width-controller.ts` — `setColumnWidth()`, `setWidthToTable()`, `setPersistedWidth()`, pane-specific methods
- `scripts/util.ts` — `formatUnit()`, `getScrollBarWidth()`

---

## Architecture Overview

### SRP Architecture (Post-Refactor)

AutoFit functionality is distributed across the SRP-compliant architecture:

**C# Layer** (No separate partial; logic in main SfGrid files):
- `SfGrid.Methods.cs` (lines 121-226): All public AutoFit APIs
- `SfGrid.razor.cs` (lines 79, 347): Internal state flags (`_isPersistAutoFit`, `IsAutoFitEnabled`)
- `GridJSInteropAdaptor.cs` (lines 144-216): **Centralized JS callback hub** for `ColumnWidthChanged()`

**JS Layer** (scripts/):
- `resize.ts`: Core measurement engine
- `sf-grid.ts`: Bridge between C# and resize module
- `width-controller.ts`: DOM width application logic

---

## JavaScript Interop Architecture

AutoFit relies on **bidirectional C# ↔ JavaScript communication** for DOM measurement and state synchronization.

### C# → JavaScript Calls

| C# Location | Method | JavaScript Target | Parameters | Purpose |
|-------------|--------|-------------------|------------|---------|
| `SfGrid.Methods.cs:124` | `InvokeMethod()` | `sfBlazor.Grid.autoFitColumns` | `[DataId, Columns, [], true]` | Trigger AutoFit for all columns |
| `SfGrid.Methods.cs:196` | `InvokeMethod()` | `sfBlazor.Grid.autoFitColumns` | `[DataId, Columns, fieldNames, true]` | Trigger AutoFit for specific columns |
| `SfGrid.Lifecycle.cs:357` | `InvokeMethod()` | `sfBlazor.Grid.autoFitColumns` | `[DataId, Columns, _targetColumns, _isPersistAutoFit, _isColumnResized]` | Restore persisted AutoFit state |

### JavaScript → C# Callbacks

| JavaScript Location | Method | C# Target | Parameters | Purpose |
|---------------------|--------|-----------|------------|---------|
| `resize.ts:~180` | `dotNetRef.invokeMethodAsync()` | `GridJSInteropAdaptor.ColumnWidthChanged` | `[{columnUid, width, allowStopEvent}, false]` | Report measured column width back to C# |

### TypeScript Functions (scripts/)

| File | Function | Line(s) | Responsibility |
|------|----------|---------|----------------|
| `sf-grid.ts` | `autoFitColumns()` | — | **JS Bridge entry point**: receives C# call, delegates to `resizeModule.autoFitColumns()`, applies persisted widths via `setPersistedWidth()` |
| `resize.ts` | `autoFitColumns()` | 61 | **Main orchestrator**: routes to `findColumn()` for measurement, handles frozen column ordering via `autofitFrozenColumns()` |
| `resize.ts` | `findColumn()` | — | **Column iterator**: loops through field names, calls `resizeColumn()` for each visible column |
| `resize.ts` | `resizeColumn()` | 94 | **Measurement engine**: clones cells, creates off-screen table, measures width via `createTable()`, applies constraints, invokes C# callback |
| `resize.ts` | `createTable()` | 245 | **DOM measurement**: creates off-screen `<table>` with `table-layout:auto`, clones header/content cells, measures `getBoundingClientRect().width`, cleans up |
| `resize.ts` | `callAutoFit()` | 348 | **Double-click handler**: wired to `.e-rhandler` dblclick event, triggers `resizeColumn()` for single column |
| `sf-grid-fn.ts` | `getColumns()` | — | Returns column list; if `autoFitVirtual=true`, returns frozen + virtually rendered columns only |
| `sf-grid-fn.ts` | `autofitFrozenColumns()` | — | Filters and orders columns by freeze direction: Left → Movable → Right |
| `width-controller.ts` | `setColumnWidth()` | — | Applies width to `<colgroup><col>` in header/content/footer tables |
| `width-controller.ts` | `setWidthToTable()` | — | Synchronizes all three pane tables (frozen-left, movable, frozen-right) with new widths |
| `width-controller.ts` | `setPersistedWidth()` | — | Restores persisted `TableWidth`, `LeftFrozenTableWidth`, `RightFrozenTableWidth` from column models to DOM |

### Responsibility Split

| Layer | Responsibilities | Does NOT Handle |
|-------|------------------|-----------------|
| **C# (SfGrid.Methods.cs)** | API exposure, state flags (`IsAutoFitEnabled`), persistence flags (`_isPersistAutoFit`), JS bridge invocation | ❌ DOM measurement, width calculation |
| **C# (GridJSInteropAdaptor.cs)** | Receive measured widths, update column models, store persistence, trigger events (`ResizeStopped`), set render flags (`ForceUpdate`) | ❌ DOM manipulation, cell cloning |
| **JS (resize.ts)** | DOM measurement, cell cloning, off-screen table creation, width calculation, constraint application (min/max width) | ❌ Persistence storage, event triggering, column model updates |
| **JS (width-controller.ts)** | Apply widths to `<colgroup><col>`, synchronize three pane tables, restore persisted widths | ❌ Width calculation, measurement logic |

### CRITICAL Interop Rules

1. **🚨 NEVER measure widths in C#** — All DOM measurement MUST happen in JavaScript via `getBoundingClientRect()`. C# only stores and persists results.

2. **🚨 NEVER update column models in JS** — JavaScript measures and reports widths; C# updates `GridColumn.Width` via `SetWidth()`. JS reads column models but never writes.

3. **🚨 ALWAYS invoke C# callback after measurement** — Every `resizeColumn()` call MUST invoke `dotNetRef.invokeMethodAsync('ColumnWidthChanged', ...)` to sync state.

4. **🚨 ALWAYS synchronize all three pane tables** — When `FrozenColumns > 0`, width changes MUST update left-frozen, movable, and right-frozen tables via `setWidthToTable()`. Missing any pane causes visual misalignment.

---

### Core Concepts (No Duplication)

1. **Two-level API**: AutoFit is exposed via C# async methods (`AutoFitColumnsAsync()`) in `SfGrid.Methods.cs` which invoke JS bridge (`sfBlazor.Grid.autoFitColumns()`) that delegates to `resizeModule.autoFitColumns()`. JS layer does all DOM measurement and width calculation.

2. **Centralized JS Interop**: **All JS callbacks route through `GridJSInteropAdaptor.cs`**. The `[JSInvokable] ColumnWidthChanged()` method (line 144) is the single entry point for width changes from JS, including AutoFit results. This replaces the old scattered callback pattern.

3. **Off-screen measurement pattern**: `createTable()` in `resize.ts` clones target column cells to an off-screen hidden table with `table-layout:auto`, measures intrinsic width via `getBoundingClientRect().width`, then removes clone. Safe across browsers, no layout blocking.

4. **Three table panes (when frozen)**: Grid DOM has separate `<table>` for left-frozen, movable, right-frozen. AutoFit must synchronize all three via `setWidthToFrozenLeftTable()`, `setWidthToMovableTable()`, `setWidthToFrozenRightTable()`. Key insight: Each pane's width is calculated independently by filtering columns via `calcMovableOrFreezeColWidth()`.

5. **autoFitVirtual parameter**: When both `EnableColumnVirtualization` and AutoFit active, `getColumns(true)` returns only virtually rendered columns plus frozen columns ordered via `getOrderedFrozenColumns()`. Measurement includes frozen + visible virtual subset only.

6. **Column filtering**: `autofitFrozenColumns(autofitAllColumns?)` filters columns by visibility and freeze state. Hidden columns excluded. Non-matching freeze directions skipped.

7. **Persistence contract**: When `EnablePersistence && _isPersistAutoFit`, after autofit completes, `GridJSInteropAdaptor.ColumnWidthChanged()` stores `TableWidth`, `LeftFrozenTableWidth`, `RightFrozenTableWidth` on every column model (lines 183-188 in `ColumnWidthChanger()` method). On next init, `SetLocalStorage()` serializes these and `setPersistedWidth()` restores all three pane widths.

8. **IsAutoFitEnabled flag**: Set true in `AutoFitColumnsAsync()` C# methods (line 123 in `SfGrid.Methods.cs`) AND menu click handlers. Used in `InvokeViewRefresh()` condition (line 1635 in `SfGrid.razor.cs`) when `Width == "100%"` or `"auto"` to trigger JS viewRefresh.

9. **Stacked header support**: `getStackedWidth()` recursively sums child column widths. Stacked columns measured via `createTable()` header clones with children.

10. **Double-click trigger**: `callAutoFit()` event handler on `dblclick` on `.e-rhandler` (resize handle). Calls `resizeColumn()` with single column only (not bulk).

11. **Initial render auto-fit**: In `OnAfterRenderAsync()`, if `AutoFit` property is true, `autoFitColumns()` JS invoked with all non-hidden columns.

---

## Complete DOM Structure & AutoFit Interaction

### Standard Grid + Frozen Columns (Single Section)

```
┌──────────────────────────────────────────────────────────────────┐
│ .e-grid (root, data-id="grid_xyz")                               │
├──────────────────────────────────────────────────────────────────┤
│ HEADER SECTION                                                   │
│                                                                  │
│ ┌─ No Frozen ───────────┬─ With FrozenColumns=2 ────────────────┐
│ │ .e-gridheader         │ .e-gridheader (3 panes)               │
│ │                       │                                       │
│ │ .e-table              │ ┌──────────┬────────┬─────────────┐   │
│ │ <colgroup>            │ │.e-frozen-│.e-mov- │.e-frozen-  │   │
│ │  <col w:80px>  ┐      │ │left-head │ able   │right-header│   │
│ │  <col w:120px> ├ sync │ │          │header  │            │   │
│ │  <col auto>    ┘ w/   │ │.e-table  │.e-tbl │.e-table    │   │
│ │ </colgroup>    content│ │<colg>    │<colg> │<colg>      │   │
│ │                       │ │<thead>   │<thead>│<thead>     │   │
│ │ <thead>               │ │  <th>    │  <th>│  <th>      │   │
│ │  <tr>                 │ │[col-uid] │      │[col-uid]   │   │
│ │   <th [e-mappinguid]> │ │ OrderID  │Cust. │ Discount   │   │
│ │    <div.e-headercell> │ │   </th>  │Name  │  </th>     │   │
│ │     <span>OrderID</span> │   <th>│  </th>│            │   │
│ │    <div.e-rhandler>   │ │  CustID  │      │            │   │
│ │    </div>             │ │  </th>   │      │            │   │
│ │   </th>               │ │ </thead> │</thd>│ </thead>   │   │
│ │   ...more headers...  │ └──────────┴────────┴─────────────┘   │
│ │  </tr>                │                                       │
│ │ </thead>              │ Key: All three tables synced via      │
│ │                       │ setWidthToFrozenLeftTable(),         │
│ │                       │ setWidthToMovableTable(),            │
│ │                       │ setWidthToFrozenRightTable()         │
│ └───────────────────────┴─────────────────────────────────────┘
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│ CONTENT SECTION (Scrollable)                                     │
│                                                                  │
│ ┌─ No Frozen ────────────┬─ With FrozenColumns=2 ──────────────┐
│ │ .e-gridcontent         │ .e-gridcontent (3 panes)             │
│ │ (overflow-x/y: auto)   │                                      │
│ │                        │ ┌──────────┬────────┬─────────────┐  │
│ │ .e-table               │ │.e-frozen-│.e-mov- │.e-frozen-  │  │
│ │                        │ │left-cntnt│ ablecnt│right-cntnt  │  │
│ │ <colgroup>             │ │(fixed)   │(scroll)│(fixed)      │  │
│ │  <col w:80px> ┐        │ │.e-table  │.e-tbl │.e-table     │  │
│ │  <col w:120px>├ sync   │ │<colg>    │<colg> │<colg>       │  │
│ │  <col auto>   ┘ w/     │ │<tbody>   │<tbody>│<tbody>      │  │
│ │ </colgroup>    header  │ │ <tr>     │ <tr> │ <tr>        │  │
│ │                        │ │  <td>... │ <td> │ <td>...     │  │
│ │ <tbody>                │ │ </tr>    │</tr> │ </tr>       │  │
│ │  <tr class="e-row">    │ │</tbody>   │</td> │</tbody>     │  │
│ │   <td class="e-rowcll">│ │          │      │             │  │
│ │    10248                │ └──────────┴────────┴─────────────┘  │
│ │   </td>                 │                                      │
│ │   ...more cells...      │ Frozen panes: position:fixed         │
│ │  </tr>                  │ Movable pane: scrollable only        │
│ │  ...more rows...        │ All colgroups synced (see Section 5) │
│ │ </tbody>                │                                      │
│ │                         │                                      │
│ └─────────────────────────┴──────────────────────────────────────┘
│                                                                  │
├──────────────────────────────────────────────────────────────────┤
│ OFF-SCREEN MEASUREMENT TABLE (During AutoFit)                    │
│                                                                  │
│ <div style="display:inline-block; visibility:hidden;            │
│            position:absolute; left:-99999px; top:-99999px;">    │
│                                                                  │
│  <table class="e-resizetable"                                    │
│         style="table-layout:auto; width:auto;                   │
│                border-collapse:separate;">                      │
│   <colgroup>                                                     │
│    <col>  <!-- NO width: browser auto-sizes -->                 │
│   </colgroup>                                                    │
│   <thead>                                                        │
│    <tr>                                                          │
│     <th>[cloned header cell with e-mappinguid]</th>             │
│    </tr>                                                         │
│   </thead>                                                       │
│   <tbody>                                                        │
│    <tr><td>[cloned content cell sample]</td></tr>               │
│    <tr><td>[all rows, or visible virtual range]</td></tr>       │
│    ...                                                           │
│   </tbody>                                                       │
│  </table>                                                        │
│                                                                  │
│  Then: width = Math.ceil(table.getBoundingClientRect().width)  │
│  Store: column.width = formatUnit(width)                        │
│  Apply: colgroup <col>.style.width = formatUnit(width)          │
│  Cleanup: document.body.removeChild(tempTable)                  │
│                                                                  │
│ </div>                                                           │
│                                                                  │
└──────────────────────────────────────────────────────────────────┘
```

### Three-Pane Width Synchronization Example

```
Initial:  [Left: 170px] | [Movable: 300px] | [Right: 100px]
           OrderID:80px  CustomerName:?    Discount:100px
           CustID:90px   Quantity:?
                         UnitPrice:?

AutoFit Measurement:
  Left columns:   OrderID(104px) + CustID(115px) = 219px
  Movable cols:   CustName(150px) + Qty(80px) + Price(95px) = 325px
  Right columns:  Discount(105px) = 105px

After:    [Left: 219px] | [Movable: 325px] | [Right: 105px]
           OrderID:104px CustomerName:150px Discount:105px
           CustID:115px  Quantity:80px
                        UnitPrice:95px

DOM Updates (via width-controller.ts):
  header.querySelector('.e-frozen-left-header .e-table colgroup').children[0].style.width = "104px"
  header.querySelector('.e-frozen-left-header .e-table colgroup').children[1].style.width = "115px"
  header.querySelector('.e-movableheader .e-table colgroup').children[0].style.width = "150px"
  header.querySelector('.e-movableheader .e-table colgroup').children[1].style.width = "80px"
  header.querySelector('.e-movableheader .e-table colgroup').children[2].style.width = "95px"
  header.querySelector('.e-frozen-right-header .e-table colgroup').children[0].style.width = "105px"
  (Same for content & footer tables if aggregates enabled)
```

---

## AutoFit Execution Pipeline

### Unified Flow with 5 Trigger Points

```
TRIGGER POINTS:
═══════════════════════════════════════════════════════════════════

1. PROGRAMMATIC API
   await grid.AutoFitColumnsAsync()              [All columns]
   await grid.AutoFitColumnsAsync([field1, field2])  [Specific]
   await grid.AutoFitColumnAsync(field)          [Single]

2. COLUMN MENU / CONTEXT MENU
   User right-clicks header → "AutoFit" or "AutoFitAll"

3. DOUBLE-CLICK RESIZE HANDLE
   User double-clicks column header border (.e-rhandler)

4. GRID/COLUMN PROPERTY INITIALIZATION
   <GridColumn Field="OrderID" AutoFit="true" />
   or
   <SfGrid AutoFit="true" ...>

5. PERSISTENCE RESTORE
   Page reloads with EnablePersistence=true


UNIFIED EXECUTION PATH:
═══════════════════════════════════════════════════════════════════

C# Layer (SfGrid.Methods.cs):
  AutoFitColumnsAsync(fieldNames?)
  ├─ Set IsAutoFitEnabled = true
  ├─ If EnablePersistence:
  │   └─ Set _isPersistAutoFit = true, IsPersistAutoFit on all columns
  └─ await InvokeMethod("sfBlazor.Grid.autoFitColumns",
       [DataId, Columns, fieldNames, isAutoFit=true])

JS Bridge (scripts/sf-grid.ts):
  autoFitColumns(dataId, columns, fieldNames, isAutoFit)
  ├─ gridInstance.options.columns = columns
  ├─ instance.resizeModule.autoFitColumns(fieldNames)
  └─ If isColumnResized or _isPersistAutoFit:
      └─ widthService.setPersistedWidth(columns[0])

Resize Module (scripts/resize.ts):
  autoFitColumns(fName?)
  ├─ If !fName (all columns):
  │   ├─ If frozenColumns > 0:
  │   │   └─ fName = parent.autofitFrozenColumns(true)
  │   │       [Returns Left→Movable→Right ordered field list]
  │   └─ Else:
  │       └─ fName = getColumns(autoFitVirtual).map(x => x.field)
  ├─ findColumn(fName)
  └─ If frozenColumns > 0:
      └─ widthService.setWidthToTable()

Find & Measure (scripts/resize.ts):
  findColumn(fName[])
  └─ For each fieldName:
      ├─ Get columnIndex via getColumnIndexByField()
      ├─ If index > -1 && column.visible:
      │   └─ resizeColumn(fieldName, columnIndex)
      └─ [Invokes C# ColumnWidthChanged() callback after each]

Width Measurement (scripts/resize.ts):
  resizeColumn(fName, index)
  ├─ Clone header cell: closest(headerTable.querySelector('[e-mappinguid]'), 'th')
  ├─ Clone content cells:
  │   ├─ If frozen: querySelectorAll('[aria-colindex]')
  │   └─ Else: querySelectorAll('td:nth-child(index)')
  ├─ createTable(headerClone, 'e-gridheader') → measure headerWidth
  ├─ createTable(contentClones[], 'e-gridcontent') → measure contentWidth
  ├─ width = Math.max(headerWidth, contentWidth, footerWidth)
  ├─ Apply minWidth/maxWidth constraints
  ├─ column.width = formatUnit(width)
  ├─ widthService.setColumnWidth(column) → Apply to colgroup
  └─ parent.dotNetRef.invokeMethodAsync('ColumnWidthChanged', {...})

C# Callback (GridJSInteropAdaptor.cs line 144):
  [JSInvokable] ColumnWidthChanged(args, isResizing=false)
  ├─ Deserialize args → ActionArgs + ColumnResizeStop
  ├─ Call ColumnWidthChanger() (private async void)
  │   ├─ Find column by uid: columns.FirstOrDefault(x => x.Uid == action.ColumnUid)
  │   ├─ column.SetWidth(action.Width.ToString())
  │   ├─ If AllowStopEvent: Trigger ResizeStopped event (await Parent.GridEvents.ResizeStopped.InvokeAsync)
  │   ├─ If AllowStopEvent && AllowResizing && EnablePersistence:
  │   │   ├─ Store on ALL columns: TableWidth, LeftFrozenTableWidth, RightFrozenTableWidth
  │   │   └─ await Parent.SetLocalStorage()
  │   ├─ Parent.ForceUpdate = true
  │   ├─ Parent.EventAggregator.Trigger("ColumnWidthStateChange", args)
  │   └─ If frozenColumns > 0 && AllowStopEvent && DetailTemplate: Parent.SoftRefresh = true

Final State:
  Grid re-renders with updated column widths
  Widths persist across page reload (if EnablePersistence)
```

### Width Calculation Algorithm (Single Source of Truth)

```typescript
// In scripts/resize.ts: resizeColumn()

// Step 1: Clone header cell
const headerClone = closest(
  headerTable.querySelector('[e-mappinguid="' + columnUid + '"]'), 
  'th'
).cloneNode(true);

// Step 2: Clone content cells (all rows or visible virtual range)
const contentClones = frozenColumns 
  ? contentTable.querySelectorAll(`[aria-colindex="${columnIndex}"]`)
  : contentTable.querySelectorAll(`td:nth-child(${columnIndex + 1})`);

// Step 3: Create off-screen measurement table
const tempTable = createElement('table');
tempTable.style.cssText = 'table-layout:auto;width:auto;border-collapse:separate';
tempTable.appendChild(createElement('colgroup')).appendChild(createElement('col'));
tempTable.appendChild(createElement('tbody'))
  .appendChild(createElement('tr')).appendChild(headerClone);
contentClones.forEach(cell => {
  tempTable.querySelector('tbody').appendChild(cell.cloneNode(true));
});
document.body.appendChild(tempTable);

// Step 4: Measure intrinsic width
const width = Math.ceil(tempTable.getBoundingClientRect().width);
document.body.removeChild(tempTable);

// Step 5: Apply constraints
let finalWidth = width;
if (minWidth && finalWidth < minWidth) finalWidth = minWidth;
if (maxWidth && finalWidth > maxWidth) finalWidth = maxWidth;

// Step 6: Store and apply
column.width = formatUnit(finalWidth);
widthService.setColumnWidth(column);
  // Inside setColumnWidth:
  // colgroup.children[columnIndex].style.width = formatUnit(finalWidth);
  // (Header, content, and footer if applicable)

// Step 7: Callback to C#
parent.dotNetRef.invokeMethodAsync('ColumnWidthChanged', {
  columnUid: columnUid,
  width: finalWidth.toString(),
  allowStopEvent: isDblClk
});
```

---

## Code Location Map (SRP Architecture)

| File | Lines | Responsibility |
|------|-------|---|
| `src/SfGrid.Methods.cs` | 121-226 | All `AutoFitColumnsAsync()` overloads; set `IsAutoFitEnabled = true`; invoke JS bridge; persistence flag management |
| `src/SfGrid.Lifecycle.cs` | 355-357 | `OnAfterRenderAsync()`: persistence check, `_isPersistAutoFit` flag, re-invoke `autoFitColumns` after data load |
| `src/SfGrid.razor.cs` | 79, 347, 643, 1628-1637 | `_isPersistAutoFit` field (line 79), `IsAutoFitEnabled` flag (line 347); `GetTableStyles()` method (line 643): validate all columns have defined width; `InvokeViewRefresh()` method (line 1628): condition includes `IsAutoFitEnabled` |
| `src/GridColumn.cs` | 138-140, 518 | `AutoFit` property (bool, line 138), `IsPersistAutoFit` property (bool, line 518), `PropertyChanges` tracking, `_autoFit` private field |
| `src/Internal/Renderer/ColumnMenu.razor` | 531-540 | Click handlers: `AutoFitItemClicked()` (single, line 531), `AutoFitAllItemClicked()` (all, line 537); set `Parent.IsAutoFitEnabled = true` |
| **`src/Internal/Base/GridJSInteropAdaptor.cs`** | **144-216** | **[JSInvokable] ColumnWidthChanged()** method (line 144): **Centralized JS callback hub**; deserializes args, updates column models via `ColumnWidthChanger()` (line 160) calling `SetWidth()`, stores persistence state (TableWidth/LeftFrozenTableWidth/RightFrozenTableWidth on all columns at lines 183-188), triggers `ResizeStopped` event, sets `ForceUpdate` and `SoftRefresh` flags |
| `scripts/resize.ts` | — | `autoFitColumns(fName?)`: entry, route to `findColumn()`; `findColumn()`: iterate, call `resizeColumn()`; `resizeColumn()`: measure + apply; `createTable()`: off-screen measurement; `callAutoFit()`: double-click handler |
| `scripts/sf-grid.ts` | — | JS bridge: `autoFitColumns(...)`: main entry, calls `resizeModule.autoFitColumns()`, applies persisted widths |
| `scripts/sf-grid-fn.ts` | — | `getColumns(autoFitVirtual)`: with flag, returns frozen+virtual; `getOrderedFrozenColumns()`: Left→Movable→Right; `autofitFrozenColumns()`: filter & order by freeze |
| `scripts/width-controller.ts` | — | `setColumnWidth()`: apply to colgroup; `setWidthToTable()`: sync all panes; `setPersistedWidth()`: restore state; pane-specific methods |
| `scripts/util.ts` | — | `formatUnit()`: number to 'Xpx'; `getScrollBarWidth()`: gutter calc |

---

## Interaction Matrix (MANDATORY CROSS-FEATURE CHECKS)

| Combination | Must Preserve | Risk |
|---|---|---|
| AutoFit + Frozen Columns | `autofitFrozenColumns()` returns Left→Fixed→Right; all three tables (left/movable/right) must sync colgroup widths via `setWidthToFrozenLeftTable()`, `setWidthToMovableTable()`, `setWidthToFrozenRightTable()` | **Critical** |
| AutoFit + Column Virtualization | `getColumns(autoFitVirtual=true)` returns frozen+virtual merged; measurement only clones visible virtual subset; `setWidthToTable()` uses `virtualizedColumns` array | **High** |
| AutoFit + Row Virtualization | Measure at least sample row from visible virtual range; footer cells included if aggregates enabled; virtualization does not affect column logic | **Medium** |
| AutoFit + Grouping | Group indent column measured via `getIndentCount() * 30`; grouping does not change autofit logic | **Medium** |
| AutoFit + Stacked Headers | `getStackedWidth()` sums child widths; stacked parent cloned with children; parent width = sum of leaves | **High** |
| AutoFit + Editing | Edit table also gets pane width sync; dialog edit does not require width sync | **Medium** |
| AutoFit + Persistence | Store `TableWidth`, `LeftFrozenTableWidth`, `RightFrozenTableWidth` on each column; restore via `setPersistedWidth()` on init | **Critical** |
| AutoFit + Infinite Scroll | AutoFit measures visible rows only; widths persist on new data load; user must re-call `AutoFitColumnsAsync()` if content changes | **Low** |
| AutoFit + Text Wrap | Wrapped content included in measurement via `getBoundingClientRect()`; `updateResizeEleHeight()` ensures wrapped rows accounted for | **Medium** |
| AutoFit + Selection | AutoFit does not affect selection; selection column treated as regular column | **Low** |
| AutoFit + Sorting | AutoFit does not affect sort indicator; sorted columns autofit like any other | **Low** |
| AutoFit + Filtering | Filter bar cells included in measurement; autofit does not affect filter state | **Low** |

---

## Implementation Guidelines

### Scenario 1: Programmatic AutoFit All Columns

```csharp
await grid.AutoFitColumnsAsync();
```

**Execution Path**: SfGrid.Methods.cs → sf-grid.ts → resize.ts → createTable() (per column) → ColumnWidthChanged() → SetLocalStorage()

**Checklist**:
- ✓ `IsAutoFitEnabled` set to true
- ✓ fieldNames array is empty (triggers all-columns logic)
- ✓ JS bridge invoked with empty fieldNames
- ✓ If frozen: `autofitFrozenColumns(true)` returns ordered field list
- ✓ All three pane tables width synced if frozen
- ✓ Callback invoked once per column, not once total
- ✓ Persistence stored if enabled

### Scenario 2: Column Menu "AutoFit"

```csharp
case "AutoFit":
    Parent.IsAutoFitEnabled = true;
    await Parent.AutoFitColumnsAsync(new string[] { targetColumn.Field });
    break;
```

**Execution Path**: Same as Scenario 1, but with specific fieldNames

**Checklist**:
- ✓ Single column field passed as array
- ✓ Only that column is measured, others unchanged
- ✓ If frozen: included in frozen ordering if applicable
- ✓ Callback fires once for that column
- ✓ Persistence stores only that column's width (but on all columns)

### Scenario 3: Double-Click Resize Handle

```typescript
private callAutoFit(event: PointerEvent): void {
    if ((event.target as HTMLElement).classList.contains('e-rhandler')) {
        const col = this.getTargetColumn(event);
        this.resizeColumn(col.field, columnIndex, col.uid, true);
    }
}
```

**Key Difference**: Single column only, no JS bridge invocation needed

**Checklist**:
- ✓ dblclick event wired on .e-rhandler
- ✓ Target column extracted via [e-mappinguid]
- ✓ resizeColumn() called directly (not via findColumn)
- ✓ All rows cloned and measured
- ✓ Width applied to colgroup immediately
- ✓ Callback invoked to store width

### Scenario 4: AutoFit Persistence Restore

```csharp
// OnAfterRenderAsync()
if (_isPersistAutoFit || _isColumnResized)
{
    await InvokeMethod("sfBlazor.Grid.autoFitColumns",
        new object[] { DataId, Columns, _targetColumns, _isPersistAutoFit, true });
}
```

**Execution Path**: sf-grid.ts → setPersistedWidth() → apply all three pane widths

**Checklist**:
- ✓ _isPersistAutoFit flag is set from persisted state
- ✓ Columns contain restored `TableWidth`, `LeftFrozenTableWidth`, `RightFrozenTableWidth`
- ✓ `isColumnResized` parameter is true to trigger setPersistedWidth
- ✓ All three pane tables get width applied (if frozen)
- ✓ No re-measurement happens, just DOM updates

---

## Persistence Storage & Retrieval

### Storage Phase

```csharp
// GridJSInteropAdaptor.cs: ColumnWidthChanged()
foreach(var col in Parent.Columns!)
{
    col.TableWidth = columnResizeStop.TableWidth;           // "649px"
    col.LeftFrozenTableWidth = columnResizeStop.LeftFrozenTableWidth;   // "219px"
    col.RightFrozenTableWidth = columnResizeStop.RightFrozenTableWidth; // "105px"
}
await Parent.SetLocalStorage(); // Serializes to localStorage
```

**Result**: localStorage["gridMyGridID"] = { columns: [...], properties: {...} }

### Retrieval Phase

```csharp
// SfGrid.Lifecycle.cs: OnAfterRenderAsync()
if (EnablePersistence && !IsDataLoaded)
{
    var data = await InvokeMethod<string>("window.localStorage.getItem", false, new[] { $"grid{ID}" });
    await PersistProperties(data); // Deserialize → set _isPersistAutoFit = true
}
```

**Result**: Column models contain restored widths, _isPersistAutoFit flag is true

### Restoration Phase

```typescript
// scripts/width-controller.ts: setPersistedWidth()
public setPersistedWidth(column: Column): void {
    if (this.parent.options.frozenColumns) {
        this.setWidthToFrozenLeftTable(column.leftFrozenTableWidth);    // "219px"
        this.setWidthToMovableTable(column.tableWidth);                 // "649px"
        this.setWidthToFrozenRightTable(column.rightFrozenTableWidth);  // "105px"
    } else {
        (this.parent.getHeaderTable() as HTMLTableElement).style.width = column.tableWidth;
    }
}
```

**Result**: All three pane tables have colgroup widths applied, grid displays with restored layout

---

## Testing & Known Limitations

### Testing Checklist

1. **Unit**: Measure algorithm in isolation — mock cells, verify `getBoundingClientRect().width` is reasonable
2. **Integration**: Full flow with grid — call `AutoFitColumnsAsync()`, verify colgroup widths set
3. **Cross-Feature**: AutoFit+Frozen (three panes sync), AutoFit+Virtual (only visible columns), AutoFit+Persistence (widths survive reload)
4. **E2E**: User interactions — menu autofit, double-click, property initialization, persistence

### Known Limitations

1. **Hidden columns excluded**: Only visible columns measured. Hidden columns retain width or get 0.
2. **First render cost**: Off-screen measurement synchronous; large data can be expensive.
3. **Text wrap mid-autofit**: If wrap enabled after autofit, row heights change; no auto-re-measure. User must re-call `AutoFitColumnsAsync()`.
4. **Stacked parent not auto-updated**: Parent width = sum of leaves at measurement time only.
5. **Infinite scroll no auto-adjust**: AutoFit measures visible rows only. New data requires manual re-call.
6. **Percentage widths not autofit**: Only pixel widths applied.

---

## Quick Reference: Trigger Point Routes

| Trigger | Entry | Fieldnames | Result |
|---|---|---|---|
| `AutoFitColumnsAsync()` | SfGrid.Methods.cs | Empty array | All columns, ordered by freeze if applicable |
| `AutoFitColumnsAsync([...])` | SfGrid.Methods.cs | Field names | Specific columns only |
| Column Menu "AutoFit" | ColumnMenu.razor | Single field | One column |
| Column Menu "AutoFitAll" | ColumnMenu.razor | Empty array | All columns |
| Double-click handle | resize.ts direct | N/A | One column only |
| Property init | OnAfterRenderAsync | Auto-detected | Columns with AutoFit=true |
| Persistence restore | OnAfterRenderAsync | Restored fields | Re-apply stored widths |

---

## References

- Frozen Column Skill: `frozen-column-skill/SKILL.md`
- Virtualization Skill: `virtualization-skill/SKILL.md`
- Blazor Lifecycle Skill: `blazor-lifecycle/SKILL.md`
- Testing Skill: `testing/SKILL.md`
