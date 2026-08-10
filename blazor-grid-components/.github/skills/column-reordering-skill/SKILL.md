---
name: column-reordering-skill
description: Expert knowledge for Column Reordering feature in Syncfusion Blazor DataGrid. Use this skill for feature implementation or bug fixes covering drag-drop reordering, programmatic APIs, column menu/chooser interactions, stacked headers, frozen columns, virtual scrolling, persistence, and cross-feature integrations with sorting, filtering, grouping, and selection.
---

# Skill Instructions

<!-- token-budget: 25 words -->

**Purpose**

Comprehensive knowledge base for implementing and debugging the Column Reordering feature in SfGrid<T>, guaranteeing zero regression across all 9 reordering scenarios and feature combinations.

---

**Agent Invocation**

Load this skill when:
- Implementing drag-drop column reordering
- Building column menu/chooser reordering UX
- Implementing public APIs (`ReorderColumnAsync`, `ReorderColumnsAsync`, `ReorderColumnByIndex`)
- Handling stacked or frozen column edge cases
- Debugging cross-feature interactions (reorder + sort/filter/group)
- Implementing column persistence with reorder state
- Optimizing reorder performance in virtual scroll scenarios

---

## Knowledge References

<!-- token-budget: 60 words -->

**Live Codebase Files Analyzed**

### TypeScript (Client-side)
- `scripts/reorder.ts` — Reorder class, drag/drop handlers, arrow indicators, moveColumns logic
- `scripts/header-drag-drop.ts` — Draggable/Droppable init, visual clone creation, event coordination
- `scripts/sf-grid-fn.ts` — SfGrid instance, reorderModule registration
- `scripts/interfaces.ts` — IGridOptions (allowReordering, frozenColumns, enableColumnVirtualization flags)
- `scripts/util.ts` — Helpers (getElementIndex, getPosition, inArray, parentsUntil)
- `scripts/freeze.ts` — Frozen scroll container layout
- `scripts/group.ts` — Group drag-drop coordination with reorder

### C# / Razor (Server-side)
- `src/Internal/Actions/Reorder.cs` — Server reorder module, PerformReorder (line 35), ColumnReordered handler (line 227), column swap logic (Swap at line 303)
- `src/SfGrid.Methods.cs` — Public APIs (ReorderColumnAsync line 2411, ReorderColumnsAsync line 2345)
- `src/Internal/Base/GridJSInteropAdaptor.cs` — JS interop entry point, [JSInvokable] ColumnReordered (line 91)
- `src/SfGrid.Lifecycle.cs` — ReorderModule instantiation & wiring
- `src/GridEvents.cs` — ColumnReordering (line 722) & ColumnReordered (line 750) event callbacks
- `src/EventModels/Grids.cs` — ColumnReorderedEventArgs payload

### Rendering (DOM)
- `src/Internal/Renderer/GridHeader.razor` — Header table structure, row/cell iteration
- `src/Internal/Renderer/GridHeaderCell.razor` — Individual <th> cell rendering, e-headercell/e-mappinguid
- `src/Internal/Renderer/GridHeaderBase.cs` — GenerateCells(), GetRows(), depth calculation for stacked headers

**Training Materials Consulted**
- `/docs/training/` — Feature edge cases, frozen column reorder constraints, stacked header depth handling

---

## Training Insights Applied

<!-- token-budget: 80 words -->

**Key Rules & Constraints**

1. **Frozen column reorder lock**: Columns with `IsFrozen=true` CANNOT reorder outside their frozen section (left, right, or fixed).
2. **Stacked header depth**: Parent column reorder must recursively update child column indices; depth measured in `MeasureColumnDepth()`.
3. **Fixed column vs IsFrozen**: `FixedColumn` property on GridColumn prevents reorder entirely; `IsFrozen` allows reorder within section only.
4. **Column virtualization**: When `EnableColumnVirtualization=true`, reorder must update `Parent.VirtualScrollModule` column ranges.
5. **Drag abort on `.e-rhandler`**: Resize handle (class `e-rhandler`) must NOT trigger reorder; set as `abort` in Draggable config.
6. **Persistence workflow**: After reorder, `Parent.SetColumnIndex()` must be called to persist column order (if `EnablePersistence=true`).
7. **Grouping co-op**: Grouped columns CAN be reordered if `GridGroupSettings.AllowReordering=true`; but reorder event fires alongside group events.
8. **Event cancellation**: `ColumnReordering` fires BEFORE swap via `ColumnReorderingEventArgs.Cancel`; set `Cancel = true` to abort. `ColumnReordered` fires ONLY if NOT cancelled. Multiple-column reorder passes `FromColumnUid[]` array + `IsMultipleReorder=true` in `ActionArgs`.
9. **Drag priority rules**: `AllowFreezeLineMoving` active → `helper()` returns `false`, blocking reorder init. Drop on `.e-groupdroparea` → grouping takes priority over reorder in `dragStop()`. `.e-rhandler` (resize handle) is set as Draggable `abort` target — never triggers reorder.
10. **Drag clone lifecycle**: `HeaderDragDrop.helper()` creates `div.e-cloneproperties.e-dragclone.e-headerclone` (appended to `document.body`, `position:absolute`, `zIndex:10000`). Destroyed in `ondragend()`. Original cell gets `e-clonedheader` class during drag.
11. **Frozen rows + reorder**: When `FrozenRows > 0`, frozen row cells regenerate with new column order post-reorder. Frozen row count/height is unaffected; only cell sequence changes.

---

## Code Location Map

<!-- token-budget: 80 words -->

| Component | File Path | Key Methods/Classes | Purpose |
|-----------|-----------|-------------------|---------|
| **Reorder Engine** | `scripts/reorder.ts` | `Reorder` class, `moveColumns()` (line 166), `headerDrop()` (line 84), `drag()` (line 435) | Core drag-drop state machine & column swap |
| **Drag Init** | `scripts/header-drag-drop.ts` | `HeaderDragDrop` class (line 12), `initializeHeaderDrag()` (line 38), `helper()` | Draggable/Droppable setup, visual clone |
| **Server Swap** | `src/Internal/Actions/Reorder.cs` | `Reorder<T>` (line 13), `PerformReorder()` (line 35), `Swap()` (line 303) | Column list swap, frozen constraints |
| **Public API** | `src/SfGrid.Methods.cs` | `ReorderColumnAsync()` (line 2411), `ReorderColumnsAsync()` (line 2345) | User-facing entry points |
| **JS Interop Entry** | `src/Internal/Base/GridJSInteropAdaptor.cs` | `ColumnReordered()` (line 91, `[JSInvokable]`) | Bridge from TypeScript to server |
| **JS Interop Handler** | `src/Internal/Actions/Reorder.cs` | `ColumnReordered()` (line 227), `PerformReorder()` (line 35) | Deserialize payload, execute reorder logic |
| **Events** | `src/GridEvents.cs` | `ColumnReordering` (line 722), `ColumnReordered` (line 750) callbacks | Pre/post-reorder event slots |
| **Rendering** | `src/Internal/Renderer/GridHeader.razor` | `GetRows()`, `GenerateCells()` | Rebuilds header DOM with new column order |

---

## Interaction Matrix (MANDATORY)

<!-- token-budget: 150 words -->

Built from live feature analysis + training constraints.

| Feature Combination | Must Preserve | Risk Level | Notes |
|---|---|---|---|
| **Reorder + Sorting** | Sort icons stay on correct columns; sort state preserved post-reorder | Medium | `GridHeaderCell` re-renders sort glyph per new column position |
| **Reorder + Filtering** | Filter state tied to column field, not position; filtered column re-renders | Medium | Filter.Field must track across reorder; FilterSettings.Columns indexed by UID |
| **Reorder + Grouping** | Grouped columns must move within grouping structure; group visual indicators update | High | `GroupSettings.Columns[]` must be reordered alongside parent.Columns |
| **Reorder + Selection** | Row selection state preserved; header selection checkbox stays sync'd | Low | Selection module keyed by row UID, not column order |
| **Reorder + Paging** | Page data row order unchanged; column order only affects display | Low | Reorder is column layout only; no data fetch impact |
| **Reorder + Virtual Scroll** | VirtualScrollModule.StartColumnIndex/EndColumnIndex recalculated post-reorder | Critical | Virtualized column ranges MUST match new column sequence |
| **Reorder + Frozen Columns** | Frozen left/right sections must be respected; cross-section reorder blocked | Critical | `ReorderFrozenColumns()` enforces frozen+unfrozen separation |
| **Reorder + Stacked Headers** | Parent+child indices swapped together; colspan/rowspan recalculated | High | `processStackedColumns()` flattens for validation; child column indices must stay aligned |
| **Reorder + Persistence** | Column order persisted & restored on reload; UID used as key | Medium | `Parent.SetColumnIndex()` called post-reorder; `Parent.ColumnIndex` updated |

---

## Key Scenario Flows

<!-- token-budget: 400 words -->

### S1 — Basic Drag-Drop Reordering

```
dragStart() → drag() [show arrows] → dragStop() → headerDrop()
→ moveColumns(newIdx, column) [columns.splice()]
→ invokeMethodAsync('ColumnReordered')
→ Reorder<T>.PerformReorder() → Swap() → SetColumnIndex()
→ StateHasChanged() → GridHeader re-renders all <th> cells
```
**DOM key classes:** `e-headercell`, `e-mousepointer`, `e-mappinguid` (column UID attr on inner div), `e-reorderuparrow`/`e-reorderdownarrow` (shown on valid drop only).

---

### S2 — Stacked Header Reordering

```
drag() detects isStackedCell=true → processStackedColumns() flattens hierarchy
→ chkDropPosition() validates same <tr> level
→ moveColumns() swaps parent column (children travel with parent)
→ GridHeaderBase.GetRows() recalculates MeasureColumnDepth()
→ Both level-1 and level-2 rows re-rendered with updated colspan/rowspan
```
**Rule:** Children are NEVER moved independently from their stacked parent.

---

### S2.5 — Multiple Column Reordering (`ReorderColumnsAsync`)

ActionArgs payload: `{ isMultipleReorder: true, fromColumnUid: ["uid1","uid2"], toColumnUid: "uid3" }`

```
reorderMultipleColumns(fromFNames[], toFName)
→ For each: moveColumns(destIndex, column)
→ toIndex++ if next column in batch appears after current drop point
→ C#: Reorder<T>.ReorderColumns() loops Swap() per column, adjusting index
→ Single ColumnReordering + ColumnReordered fire (once each, with array payload)
```
**Rule:** All columns must be within the same scoped parent — no cross-stacked batch reorder.

---

### S3 — Frozen Column Reordering

```
chkDropPosition(): srcElem.parentElement == destElem.parentElement?
  NO  (cross-section: movable → frozen) → returns -1 → e-notallowedcur
  YES (within frozen or within movable)  → continues
→ ReorderFrozenColumns(scopedColumns) enforces frozen/movable boundary
→ Frozen section unchanged; only same-section columns swap
```
**Rule:** Cross-section drag always blocked. `IsFrozen=true` columns stay inside their freeze partition.

---

### S3.5 — Frozen Rows + Column Reordering

`FrozenRows > 0` → `GridHeader.razor` takes `RowCollection = Rows.Take(FrozenRows)`.  
After reorder: frozen row cells regenerate with new column order. Frozen row count/height unchanged.  
**Rule:** Reorder affects cell sequence in frozen rows, never the row count or scroll offset.

---

### S3.6 — AllowFreezeLineMoving Blocks Reorder

`header-drag-drop.ts helper()`: if `parentsUntil(target, '.e-freezeline-moving')` → `return false`.  
**Priority:** Freeze-line drag > column reorder. Drag simply never initiates — no event, no cursor, no clone.

---

### S4 — Column Menu Reordering

```
ColumnMenu click → SfGrid.ReorderColumnAsync(fromField, toField)
→ ReorderModule.ReorderColumns(from, to) [TS]
→ invokeMethodAsync('ColumnReordered') → Reorder<T>.PerformReorder()
→ StateHasChanged()
```

---

### S4.5 — Grouping Drop Area Blocks Reorder

`dragStop()`: `parentsUntil(dropTarget, '.e-groupdroparea')` → grouping takes priority → reorder aborted.  
**Priority order:** group-drop-area > freeze-line > reorder. Single drop = one operation only.

---

### S5 — Column Chooser Reordering

```
ColumnChooser Apply → loops ReorderColumnByIndex(oldIdx, newIdx) for each changed column
→ Server Reorder<T>.ReorderColumn(fromIdx, toIdx) per iteration
→ Final StateHasChanged() after all swaps
```

---

### S5.5 — Drag Clone (Visual Feedback)

`helper()` creates `div.e-cloneproperties.e-dragclone.e-headerclone` — deep clone of dragged `<th>`, appended to `document.body`, `position:absolute`, `zIndex:10000`.  
`ondrag()` repositions it to follow mouse. `ondragend()` removes it.  
Original `<th>` gets `e-clonedheader` class (faded) during drag; removed on drop.

---

### S6 — Virtual Scroll Column Reordering

`headerDrop()` detects `enableColumnVirtualization=true` → resolves column by UID from `draggedHeader`, not live DOM.  
After swap: `VirtualScrollModule.RecalculateColumnRanges()` updates `StartColumnIndex`/`EndColumnIndex`.  
**Rule:** Always recalculate virtual ranges post-reorder — stale range = wrong columns rendered.

---

### S7 — Persistence (Save & Restore)

Save: post-reorder `Reorder<T>.PerformReorder()` calls `Parent.SetColumnIndex()` → serialises UID sequence to localStorage.  
Restore: `OnAfterRenderAsync(firstRender)` reads saved UIDs → calls `RestoreColumnOrder(List<string> uids)` → `ReorderColumn()` per uid → `SetColumnIndex()`.

---

### S8 — Reorder + Sorting

Sort state is keyed by `Column.Field`, not position. After reorder, `GridHeaderCell` re-renders sort glyph on whichever `<th>` now holds that field. Data sort order unchanged.

---

### S9 — Reorder + Grouping

`GroupSettings.Columns[]` tracks field names, not positions. Reordering a grouped column keeps it grouped; toggle buttons follow the column.  
`GroupSettings.Columns[]` is NOT modified by reorder — only visual column order changes.

---

## JavaScript Interop

<!-- token-budget: 120 words -->

**Purpose:** Documents the TypeScript ↔ C# communication bridge for column reordering operations.

### TypeScript → C# Call Flow

| TypeScript Method | Line | C# Handler | Line | Purpose |
|---|---|---|---|---|
| `invokeMethodAsync('ColumnReordered')` | `scripts/reorder.ts:219` | `GridJSInteropAdaptor.ColumnReordered()` | `src/Internal/Base/GridJSInteropAdaptor.cs:91` | Drag-drop reorder notification |
| `invokeMethodAsync('ColumnReordered')` | `scripts/reorder.ts:304` | `Reorder<T>.ColumnReordered()` | `src/Internal/Actions/Reorder.cs:227` | Multiple column reorder notification |

**Call Chain:**
```
TypeScript: reorder.ts moveColumns() 
    → invokeMethodAsync('ColumnReordered', payload)
        → C#: GridJSInteropAdaptor.ColumnReordered(args) [line 91]
            → Reorder<T>.ColumnReordered(args) [line 227]
                → Deserialize ActionArgs
                → PerformReorder(action) [line 35]
                    → Swap columns
                    → Fire events
                    → StateHasChanged()
```

### C# → TypeScript Call Flow

| C# Method | Line | TypeScript Function | Purpose |
|---|---|---|---|
| `ReorderColumnAsync()` | `src/SfGrid.Methods.cs:2411` | `sfBlazor.Grid.reorderColumns()` | API-triggered single column reorder |
| `ReorderColumnsAsync()` | `src/SfGrid.Methods.cs:2345` | `sfBlazor.Grid.reorderMultipleColumns()` | API-triggered multiple column reorder |

### ActionArgs Payload Structure

**Single Column Reorder:**
```json
{
  "requestType": "reorder",
  "fromIndex": 0,
  "toIndex": 2,
  "toColumnUid": "uid-ShipCity"
}
```

**Multiple Column Reorder:**
```json
{
  "requestType": "reorder",
  "isMultipleReorder": true,
  "fromColumnUid": ["uid-OrderID", "uid-CustomerID"],
  "toColumnUid": "uid-ShipCity"
}
```

### Responsibility Split

| Layer | Responsibility |
|---|---|
| **TypeScript (reorder.ts)** | Drag-drop UX, visual indicators, DOM manipulation, validation, payload construction |
| **C# (GridJSInteropAdaptor)** | Entry point, deserialization, routing to Reorder module |
| **C# (Reorder<T>)** | Business logic, frozen column validation, column swap, event firing, persistence |

### CRITICAL Interop Rules

1. **TypeScript MUST NOT modify Parent.Columns directly** — Only C# layer updates column collection
2. **Payload must include toColumnUid for scope resolution** — Required for stacked header reordering
3. **Interop is one-way per operation** — TypeScript calls C#; C# never calls back during same reorder
4. **Events fire server-side only** — ColumnReordering/ColumnReordered are C# EventCallback, not JS events

---

## DOM Rendering Structure

<!-- token-budget: 60 words -->

### Non-Virtualized Header DOM

```
<div class="e-grid" id="grid1">
  ├─ <div class="e-headercontent">
  │  ├─ <div class="e-reorderuparrow" style="display:none"></div>
  │  ├─ <div class="e-reorderdownarrow" style="display:none"></div>
  │  └─ <table class="e-table e-sortfilter">
  │     ├─ <thead role="rowgroup">
  │     │  └─ <tr role="row" class="e-columnheader">
  │     │     ├─ <th class="e-headercell e-mousepointer" aria-colindex="1"
  │     │     │      data-uid="uid-col1" tabindex="0">
  │     │     │   <div class="e-headercell-container">
  │     │     │    <div e-mappinguid="uid-col1"
  │     │     │        class="e-headercelldiv e-leftalign">
  │     │     │     <span class="e-headertext">OrderID</span>
  │     │     │    </div>
  │     │     │    <div class="e-sortfilterdiv e-icons"></div>
  │     │     │    <div class="e-rhandler e-rcursor"></div>
  │     │     │   </div>
  │     │     │  </th>
  │     │     ├─ <th> ShipCity </th>
  │     │     └─ ...more columns...
  │     │  </tr>
  │     └─ ...more rows (if stacked headers)...
  │
  └─ <div class="e-gridcontent">
     ├─ <div class="e-movableheader"> (if frozen)
     ├─ <div class="e-frozen-left"> (if frozen left)
     └─ <div class="e-tablecontent"> (data rows)
```

---

### Scenario 2.5: Multiple Column Reordering

**Scenario: User reorders multiple columns at once (via API or Column Chooser)**

**Grid Configuration:**

```csharp
<SfGrid AllowReordering="true">
  <GridColumns>
    <GridColumn Field="OrderID"></GridColumn>
    <GridColumn Field="CustomerID"></GridColumn>
    <GridColumn Field="ShipName"></GridColumn>
    <GridColumn Field="ShipCity"></GridColumn>
    <GridColumn Field="Freight"></GridColumn>
  </GridColumns>
</SfGrid>
```

**User Code:**

```csharp
// Reorder multiple columns at once
await gridRef.ReorderColumnsAsync(
  new string[] { "OrderID", "CustomerID" },  // fromFieldNames
  "ShipCity"  // toFieldName - move both BEFORE ShipCity
);
```

**Event Payload (ActionArgs):**

```json
{
  "requestType": "reorder",
  "isMultipleReorder": true,
  "fromColumnUid": ["uid-OrderID", "uid-CustomerID"],
  "toColumnUid": "uid-ShipCity",
  "type": "actionBegin"
}
```

**TypeScript Flow (scripts/reorder.ts):**

```
reorderMultipleColumns(fromFNames[], toFName):
            ↓
toIndex = parent.getColumnIndexByField(toFName)  → 3 (ShipCity)
            ↓
For i = 0 to fromFNames.length:
  column = parent.getColumnByField(fromFNames[i])
  parent = getColParent(column, parent.options.columns)
  columns = parent ? parent.columns : parent.options.columns
  destIndex = inArray(column, columns)
            ↓
  moveColumns(destIndex, column, true, false)
    (preventRefresh=false for bulk, true for last only)
            ↓
  if (parent.getColumnIndexByField(fromFNames[i+1]) >= destIndex):
    toIndex++  // Adjust for right-to-left moves
            ↓
After all moves:
  cols = parent.getColumns()
  dotNetRef.invokeMethodAsync('ColumnReordered', {
    fromColumnUid: [uid1, uid2],
    toColumnUid: toColumn.uid,
    isMultipleReorder: true,
    requestType: 'reorder',
    type: 'actionBegin'
  })
```

**Server-side C# (Reorder<T>):**

```csharp
public async Task PerformReorder(ActionArgs action)
{
    if (action.IsMultipleReorder)
    {
        // fromColumnUid = ["uid-OrderID", "uid-CustomerID"]
        // toColumnUid = "uid-ShipCity"
        ReorderColumns(action.FromColumnUid!, action.ToColumnUid!);
    }
    else
    {
        ReorderColumn((int)action.ToIndex, (int)action.FromIndex, action?.ToColumnUid!);
    }
    // ... persistence logic ...
}

public void ReorderColumns(string[] fromNames, string toName)
{
    // Find columns by field
    List<GridColumn> ToScopedColumns = 
        Reorder<T>.GetScopeRecursively(toName, (List<GridColumn>)Parent.Columns!);
    
    int fromIndex, toIndex = ToScopedColumns.FindIndex(col => col.Uid == toName);
    
    for (int i = 0; i < fromNames.Length; i++)
    {
        var name = fromNames[i];
        List<GridColumn> FromScopedColumns = 
            Reorder<T>.GetScopeRecursively(name, (List<GridColumn>)Parent.Columns!);
        
        if (FromScopedColumns == ToScopedColumns)
        {
            fromIndex = FromScopedColumns.FindIndex(col => col?.Uid == name);
            Swap(ToScopedColumns, fromIndex, toIndex);
            
            // Adjust toIndex if next column is beyond current drop point
            var tmp = i + 1 > fromNames.Length - 1 ? -1 : i + 1;
            if (tmp != -1 && ToScopedColumns.FindIndex(col => col?.Uid == fromNames[tmp]) >= toIndex)
            {
                toIndex++;
            }
        }
    }
}
```

**DOM Result:**

```
Initial:  [OrderID, CustomerID, ShipName, ShipCity, Freight]
                ↓
Drag both OrderID + CustomerID → Before ShipCity
                ↓
Final:    [ShipName, OrderID, CustomerID, ShipCity, Freight]
          (moved to positions 1 and 2)
```

**Critical Points:**

```
1. Multiple reorder swaps columns IN ORDER (left to right or right to left)
2. toIndex increments if next column in batch appears after current drop point
3. All columns must be in SAME scoped parent (no cross-stacked reorder)
4. Events:
   - ColumnReordering fires ONCE with all columns
   - ColumnReordered fires ONCE with fromColumnUid[] (array!)
5. Persistence stores all reordered UIDs in correct sequence
```

---

### Scenario 2: Stacked Header Reordering

**Scenario Configuration:**

```csharp
// Blazor Component
<GridColumns>
  <GridColumn Field="ShipName" HeaderText="Ship"></GridColumn>
  <GridColumn HeaderText="Address">
    <GridColumns>
      <GridColumn Field="ShipCity" HeaderText="City"></GridColumn>
      <GridColumn Field="ShipPostalCode" HeaderText="Postal Code"></GridColumn>
    </GridColumns>
  </GridColumn>
</GridColumns>
```

**Rendered DOM Structure:**

```
<tr class="e-columnheader">  ← Level 1 (Parent headers)
  <th class="e-headercell e-stackedheadercell">  
    <div e-mappinguid="uid-address" 
         class="e-stackedheadercelldiv"
         colspan="2">
      Address
    </div>
  </th>
  <th class="e-headercell">
    <div e-mappinguid="uid-shipname"
         class="e-headercelldiv">
      Ship
    </div>
  </th>
</tr>
<tr class="e-columnheader">  ← Level 2 (Child headers)
  <th class="e-headercell">
    <div class="e-headercelldiv">
      City
    </div>
  </th>
  <th class="e-headercell">
    <div class="e-headercelldiv">
      Postal Code
    </div>
  </th>
</tr>
```

**Reorder Action & Flow:**

```
User drags "Address" parent column over "Ship"
                ↓
HeaderDragDrop.helper() extracts e-mappinguid="uid-address"
                ↓
reorderModule.dragStart() stores element + x position
                ↓
reorderModule.drag() validates:
  - closest() finds .e-headercell with matching e-mappinguid
  - isStackedCell = true (column.columns != null)
  - Calls reorderModule.processStackedColumns()
                ↓
processStackedColumns() flattens stacked hierarchy:
  Input: [Address[City, PostalCode], Ship]
  Output: [Address[City, PostalCode], Ship] (already at correct level)
                ↓
chkDropPosition() validates drop target
  - Checks if srcElem.parentElement == destElem.parentElement ✓
  - Both in <thead> level 1 ✓
                ↓
moveColumns(newIndex, addressColumn):
  columns.splice(destIdx, 0, columns.splice(srcIdx, 1)[0])
  Result: columns = [Ship, Address[City, PostalCode]]
                ↓
Child column indices NOT changed in swap (children stay with parent)
                ↓
dotNetRef.invokeMethodAsync('ColumnReordered', {...})
                ↓
GridHeader.GetRows() recalculates MeasureColumnDepth()
                ↓
GridHeaderBase.GetStackedColumns() re-builds level 1 + level 2 rows
                ↓
Both parent row + child row re-rendered:
  Result:
  <tr> Ship | Address[colspan=2] </tr>
  <tr> | City | PostalCode </tr>
```

---

### Scenario 3.5: Frozen Rows + Column Reordering

**Scenario: User reorders columns when both FrozenRows and FrozenColumns exist**

**Grid Configuration:**

```csharp
<SfGrid FrozenRows="2" FrozenColumns="2" AllowReordering="true">
  <GridColumns>
    <GridColumn Field="OrderID" IsFrozen="true" Freeze="FreezeDirection.Left"></GridColumn>
    <GridColumn Field="CustomerID" IsFrozen="true" Freeze="FreezeDirection.Left"></GridColumn>
    <GridColumn Field="ShipName"></GridColumn>
    <GridColumn Field="ShipCity"></GridColumn>
  </GridColumns>
</SfGrid>
```

**Rendered DOM Structure (with Frozen Rows):**

```
<div class="e-grid">
  ├─ <div class="e-headercontent">
  │  ├─ <div class="e-reorderuparrow"></div>
  │  ├─ <div class="e-reorderdownarrow"></div>
  │  └─ Frozen Header + Movable Header tables
  │
  ├─ <div class="e-gridcontent">
  │  ├─ <div class="e-frozen-left">
  │  │  ├─ <table> Frozen left header (OrderID, CustomerID) </table>
  │  │  ├─ <table> Frozen rows data (first 2 rows, frozen columns only) </table>
  │  │  └─ <table> Scrollable frozen-left data (rest of rows, frozen columns only) </table>
  │  │
  │  ├─ <div class="e-movableheader">
  │  │  ├─ <table> Movable header (ShipName, ShipCity) </table>
  │  │  └─ <table> Movable frozen rows (first 2 rows, scrollable columns) </table>
  │  │
  │  ├─ <div class="e-frozencontent">
  │  │  └─ <table> Frozen rows content (2 rows, frozen columns only) </table>
  │  │
  │  ├─ <div class="e-movablecontent">
  │  │  └─ <table> Scrollable content (remaining rows, movable columns) </table>
  │  │
  │  └─ Horizontal/Vertical scrollbars
  │
  └─ <div class="e-summarycontent"> (if aggregates)
```

**Reorder Action: Drag ShipName → Before ShipCity (within movable section)**

```
User drags ShipName header
            ↓
HeaderDragDrop detects drag on movable header only
(frozen header is separate table, not draggable into movable)
            ↓
reorderModule.drag() shows arrows around ShipCity
            ↓
User drops
            ↓
moveColumns(newIndex, ShipNameColumn)
            ↓
Parent.Columns swapped: [ShipCity, ShipName] in movable section
Frozen section UNCHANGED: [OrderID, CustomerID]
            ↓
StateHasChanged()
            ↓
GridHeader re-renders:
  - Frozen header: OrderID | CustomerID (unchanged)
  - Movable header: ShipCity | ShipName (new order)
            ↓
GridContent re-renders:
  - Frozen rows (first 2 rows) re-used (no column change)
  - Movable rows: columns in new order [ShipCity, ShipName]
            ↓
HTML updated:
  Frozen columns still render first 2 rows in fixed position
  Movable columns scroll normally with new column order
  ✓ FrozenRows scroll behavior UNCHANGED
  ✓ Column reorder ONLY affects movable columns
```

**Critical Code Path (GridHeader.razor):**

```csharp
// In GridHeader.razor OnParametersSet()
if (Parent.FrozenRows > 0 && arg == null)
{
    var Addedrec = Parent.Rows?.FindAll(_ => _.IsAddedTop)?.Count;
    if (Addedrec > 0)
    {
        RowCollection = Parent.Rows
            ?.Take(Convert.ToInt32(Parent.FrozenRows) + Addedrec ?? 0)
            ?.ToList()!;
    }
    else
    {
        RowCollection = Parent.Rows
            ?.Take(Convert.ToInt32(Parent.FrozenRows))
            ?.ToList()!;
    }
}

// RowCollection is re-rendered with UPDATED Columns order
// Frozen rows cell rendering respects new column sequence
```

**Key Differences from Pure Frozen Columns:**

```
1. Frozen Rows are ALWAYS visible at top (no scrolling)
   - Re-render when columns change
   - Column reorder applies to frozen row cells too

2. After reorder:
   - Frozen rows DOM cells regenerated with new column order
   - Frozen row visual still at top (not affected by scroll)
   - Movable rows below frozen rows follow new column order

3. No height/scroll impact:
   - Frozen rows occupy fixed height (RowHeight × FrozenRows)
   - Reorder does NOT change frozen row count or height
```

---

### Scenario 3: Frozen Column Reordering

**Grid Configuration:**

```csharp
<SfGrid FrozenColumns="2" AllowReordering="true">
  <GridColumns>
    <GridColumn Field="OrderID" IsFrozen="true" Freeze="FreezeDirection.Left"></GridColumn>
    <GridColumn Field="CustomerID" IsFrozen="true" Freeze="FreezeDirection.Left"></GridColumn>
    <GridColumn Field="ShipName"></GridColumn>
    <GridColumn Field="ShipCity"></GridColumn>
  </GridColumns>
</SfGrid>
```

**Initial Rendering:**

```
Frozen Left Section (FrozenColumns=2):  |  Movable Section:
  [OrderID, CustomerID]                 |  [ShipName, ShipCity]
                ↓
DOM Structure:
<div class="e-frozen-left">
  <table> OrderID | CustomerID </table>
</div>
<div class="e-movableheader">
  <table> ShipName | ShipCity </table>
</div>
<div class="e-movablecontent">
  (scrollable data rows)
</div>
```

**Reorder Attempt 1: INVALID (cross-section)**

```
User tries: Drag ShipName (movable) → Before OrderID (frozen)
                ↓
HeaderDragDrop.initializeHeaderDrag():
  Draggable targets .e-headercell in BOTH frozen + movable headers
                ↓
reorderModule.dragStart() stores:
  this.element = ShipName <th>
  this.x = position in movable header
                ↓
reorderModule.drag() calls updateScrollPosition()
  Detects scroll elem = .e-movablecontent (movable section)
                ↓
chkDropPosition() validates:
  dropElement closest = OrderID <th> in frozen table
  srcElem.parentElement (movable <tr>)
    .isEqualNode(destElem.parentElement (frozen <tr>)) ?
  → FALSE ✗
                ↓
targetParentContainerIndex() attempts to map:
  Compares frozen headers list vs movable headers list
  → Indices don't align (different parents)
  → Returns -1 (INVALID)
                ↓
classList(cloneElement, ['e-notallowedcur'], ['e-defaultcur'])
reorderModule.setDisplay('none')  (hide arrows)
                ↓
User sees RED cursor (e-notallowedcur) = DROP NOT ALLOWED
```

**Reorder Attempt 2: VALID (within-section)**

```
User drags: ShipName → ShipCity (both movable)
                ↓
chkDropPosition() validates:
  srcElem.parentElement == destElem.parentElement ✓
  Both <th> in same <tr> in .e-movableheader ✓
                ↓
targetParentContainerIndex() calculates:
  Returns destIndex in movable columns = 1
                ↓
reorderModule.updateArrowPosition(target, isLeft):
  Positions up/down arrows around ShipCity cell
  up-arrow: cliRect.top + cliRect.height - 7
  down-arrow: cliRect.top - 2
                ↓
classList(target, ['e-allowDrop', 'e-reorderindicate'])
User sees GREEN cursor = DROP ALLOWED
                ↓
User releases mouse
                ↓
reorderModule.headerDrop():
  moveColumns(1, ShipNameColumn)
                ↓
Server Reorder<T>.PerformReorder():
  ReorderColumn(fromIndex: 0, toIndex: 1, uid: ShipName)
  columns.splice(0, 1, columns[0])  → [ShipCity, ShipName]
                ↓
  ReorderFrozenColumns(scopedColumns = movable columns):
    FrozenLeftColumns = [] (no frozen in movable)
    FrozenRightColumns = [] (no frozen in movable)
    → Frozen left section unchanged = [OrderID, CustomerID] ✓
                ↓
Result:
  Frozen Left: [OrderID, CustomerID]
  Movable:     [ShipCity, ShipName]
  ✓ Zero regression: Frozen boundary respected
```

---

### Scenario 3.6: AllowFreezeLineMoving Blocks Reorder (Edge Case)

**Scenario: User attempts to drag column header while actively moving the freeze line**

**Grid Configuration:**

```csharp
<SfGrid AllowReordering="true" AllowFreezeLineMoving="true" FrozenColumns="2">
  <GridColumns>
    <GridColumn Field="OrderID" IsFrozen="true"></GridColumn>
    <GridColumn Field="CustomerID" IsFrozen="true"></GridColumn>
    <GridColumn Field="ShipName"></GridColumn>
    <GridColumn Field="ShipCity"></GridColumn>
  </GridColumns>
</SfGrid>
```

**Drag Operation When Freeze Line Is Active:**

```
Step 1: User hovers over freeze line boundary
          ↓
        Cursor changes to ↔ (resize cursor)
        Freeze line becomes draggable
        Class ".e-freezeline-moving" applied to freeze line element
          ↓
Step 2: User clicks and drags freeze line to expand frozen area
          ↓
        During drag:
          - ".e-freezeline-moving" class remains on freeze line DOM
          - Grid knows freeze resize is in progress
          ↓
Step 3: User tries to drag a column header (while freeze line drag is active)
          ↓
        header-drag-drop.ts helper() method called:
        
        helper(args) {
            if (parentsUntil(args.target, '.e-freezeline-moving')) {
                return false;  // ❌ ABORT DRAG
            }
            // ... continue normal drag logic
        }
          ↓
        Result: ❌ Column drag NOT initiated
        No visual feedback (arrows, clone)
        No ColumnReordering event fired
        ✓ Freeze line drag takes priority
```

**TypeScript Code Location (header-drag-drop.ts):**

```typescript
private helper = (args: DragEventArgs): Element | string | false => {
    let target = args.target as HTMLElement;
    
    // CHECK: Is freeze line currently being moved?
    if (this.parent.fsModule && 
        this.parent.fsModule.isRightFreezeLine(target as HTMLElement)) {
        if (this.parent.fsModule.isFreezeLineMoving) {
            return false;  // Block reorder during freeze move
        }
    }
    
    // Also check for ".e-freezeline-moving" CSS class
    if (parentsUntil(target, '.e-freezeline-moving')) {
        return false;  // Safety check: block if class present
    }
    
    // ... rest of normal drag initialization
    return this.gridHeader.element;
};
```

**Why This Design:**

| Situation | Behavior | Reason |
|---|---|---|
| **Freeze line moving** | Reorder drag blocked | Two overlapping drag operations would cause visual glitches and state corruption |
| **Freeze line idle** | Reorder drag allowed | Only one drag operation active at a time |
| **Priority** | Freeze line > Reorder | Freeze resize is more critical (affects layout partition) |

**User Experience Impact:**

```
✓ Expected: User must complete freeze line drag before reordering columns
✓ Feedback: Cursor changes when hovering freeze line (visual cue)
❌ Not Allowed: Simultaneous freeze resize + column reorder
```

**Critical Points:**

1. **Freeze Line Has Priority** — If `AllowFreezeLineMoving=true`, freeze drag blocks reorder initialization
2. **CSS Class Check** — Helper checks both `isFreezeLineMoving` flag AND `.e-freezeline-moving` class
3. **No Silent Failure** — Drag simply doesn't start; no error event or user notification needed
4. **Session Isolation** — Once freeze move completes (drop), reorder works normally again

---

### Scenario 4: Column Menu Reordering

**UI Flow:**

```
User right-clicks OrderID header
            ↓
ColumnMenu component renders with options:
  - Move Before → [Dropdown: CustomerID, ShipName, ...]
  - Move After  → [Dropdown: CustomerID, ShipName, ...]
  - Hide Column
  - etc.
            ↓
User clicks: "Move Before" → selects "ShipCity"
            ↓
ColumnMenu.OnMenuItemClick(moveBeforeAction, ShipCity)
            ↓
ColumnMenu calls:
  SfGrid.MoveColumnAsync("OrderID", "ShipCity")
            ↓
SfGrid.ReorderColumnAsync("OrderID", "ShipCity"):
  - Finds column by field: "OrderID"
  - Finds target by field: "ShipCity"
  - Calls: reorderModule.ReorderColumns(fromField, toField)
            ↓
    reorderModule.ReorderColumns(from: "OrderID", to: "ShipCity"):
      fColumn = this.parent.getColumnByField("OrderID")
      toColumn = this.parent.getColumnByField("ShipCity")
      → Validation:
        - fColumn.allowReordering ✓
        - !fColumn.fixedColumn ✓
        - toColumn.allowReordering ✓
      → Finds parent scope:
        const column = this.parent.getColumnByField("ShipCity")
        const parent = this.getColParent(column, this.parent.options.columns)
        const columns = parent ? parent.columns : this.parent.options.columns
      → Calculates dest index:
        const destIndex = inArray(toColumn, columns) = 2
      → Calls moveColumns(2, fColumn, true):
        - Swaps in parent.columns list
        - Calls dotNetRef.invokeMethodAsync('ColumnReordered')
            ↓
C# Handler: SfGrid.razor.cs ColumnReordered(json):
  actionArgs = JsonSerializer.Deserialize<ActionArgs>(json)
  reorderModule.PerformReorder(actionArgs)
            ↓
Reorder<T>.PerformReorder():
  ReorderColumn(fromIndex, toIndex, uid)
  → columns.RemoveAt(fromIndex)
  → columns.Insert(toIndex, fColumn)
  → Parent.SetColumnIndex(Parent.Columns, false)
            ↓
StateHasChanged() → Re-render GridHeader
            ↓
GridHeader renders: [CustomerID, ShipCity, OrderID, ...]
            ↓
User sees reordered grid
```

**Code Snippet (C# Public API):**

```csharp
// In SfGrid.Methods.cs
public async Task ReorderColumnAsync(string fromFName, string toFName)
{
    if (!AllowReordering || string.IsNullOrEmpty(fromFName) || string.IsNullOrEmpty(toFName))
        return;

    ReorderModule?.ReorderColumns(fromFName, toFName);
    await Task.CompletedTask;
}

// In SfGrid.razor.cs (JS Interop)
[JSInvokable]
public async Task ColumnReordered(string actionJson)
{
    var action = JsonSerializer.Deserialize<ActionArgs>(actionJson);
    if (action != null)
    {
        await ReorderModule.PerformReorder(action);
        StateHasChanged();
    }
}

// In Reorder<T> (Internal action)
public async Task PerformReorder(ActionArgs action)
{
    if (action.IsMultipleReorder)
    {
        ReorderColumns(action.FromColumnUid!, action.ToColumnUid!);
    }
    else
    {
        ReorderColumn((int)action.ToIndex, (int)action.FromIndex, action?.ToColumnUid!);
    }

    if (!Parent.EnablePersistence)
    {
        Parent.ColumnIndex = -1;
        Parent.SetColumnIndex(Parent.Columns!, false);
    }

    IsColumnReordered = true;
    EventAggregator?.Trigger("ColumnReordered", null);
}
```

---

### Scenario 4.5: Grouping Drop Area Blocks Reorder (Feature Priority)

**Scenario: User drags column header over the Group Drop Area during reorder**

**Grid Configuration:**

```csharp
<SfGrid AllowReordering="true" AllowGrouping="true" 
        GroupSettings="new GridGroupSettings { ShowGroupedColumn = false }">
  <GridColumns>
    <GridColumn Field="OrderID"></GridColumn>
    <GridColumn Field="CustomerID"></GridColumn>
    <GridColumn Field="ShipName"></GridColumn>
    <GridColumn Field="ShipCity"></GridColumn>
  </GridColumns>
</SfGrid>
```

**Rendered Group Drop Area:**

```
┌─ e-grid ────────────────────────────────────────┐
│ ┌─ Group Drop Area ────────────────────────────┐ │
│ │  "Drag column header here to group by..."    │ │
│ │  <div class="e-groupdroparea"></div>         │ │
│ └──────────────────────────────────────────────┘ │
│                                                   │
│ Column Headers (draggable): OrderID | Customer...│
└───────────────────────────────────────────────────┘
```

**Drag Over Group Drop Area:**

```
User drags ShipName header
         ↓
reorderModule.drag() shows reorder arrows around target
         ↓
User moves drag cursor OVER the Group Drop Area div
         ↓
reorderModule.dragStop() called when user releases
         ↓
IN dragStop(): Validate drop target
    
    const dropTarget = e.target as HTMLElement;
    
    // CHECK: Is drop target inside group drop area?
    if (parentsUntil(dropTarget, '.e-groupdroparea')) {
        // ❌ STOP: Group operation takes priority
        return;  // Do NOT perform reorder
    }
    
    // Otherwise continue reorder logic...
         ↓
Result: ❌ Column NOT reordered
        ✓ ShipName added to Group instead
        Grouping event fires: GroupingEventArgs with ShipName column
```

**Decision Tree (dragStop Logic):**

```
DROP DETECTED
    │
    ├─ Is target inside ".e-groupdroparea"?
    │  ├─ YES → Fire grouping action → EXIT (no reorder)
    │  └─ NO  → Continue...
    │
    ├─ Is drop position valid for reorder?
    │  ├─ NO (invalid index) → EXIT
    │  └─ YES → Continue...
    │
    ├─ Check frozen column boundaries
    │  ├─ Violates freeze → EXIT
    │  └─ OK → Continue...
    │
    └─ Perform reorder → Fire ColumnReordering event
       → Swap columns → StateHasChanged() → Re-render
```

**TypeScript Code Location (reorder.ts):**

```typescript
private dragStop(e: DragEventArgs): void {
    if (this.parent.isDestroyed) return;
    
    // Get the element under the drop cursor
    const dropElem = e.target as HTMLElement;
    
    // PRIORITY CHECK #1: Is this the group drop area?
    if (this.parent.groupSettings && 
        this.parent.groupModule && 
        parentsUntil(dropElem, '.e-groupdroparea')) {
        
        // Grouping takes priority — trigger group operation
        this.parent.groupModule.processGrouping(this.draggedColumnName);
        return;  // ❌ ABORT REORDER
    }
    
    // PRIORITY CHECK #2: Is this a freeze line?
    if (parentsUntil(dropElem, '.e-freezeline')) {
        // Cannot reorder onto freeze boundary
        return;  // ❌ ABORT REORDER
    }
    
    // PASSED ALL CHECKS → Continue with reorder
    const newIndex = this.calculateDropIndex(dropElem);
    if (newIndex >= 0) {
        this.moveColumns(newIndex, this.draggedColumnIndex);
        this.triggerColumnReordering();  // Fire event to server
    }
}
```

**User Experience:**

```
Scenario 1 (Drop on header area):
  Drag ShipName header
  Release over OrderID header
  Result: ✓ Columns reordered [ShipName, OrderID, CustomerID, ShipCity]

Scenario 2 (Drop on group area):
  Drag ShipName header
  Release over "Drag here to group" area
  Result: ✓ ShipName added to Group, columns remain in original order
          ✓ Grid grouped by ShipName
          ✓ No reorder occurs

Scenario 3 (Drag away from grid):
  Drag ShipName header
  Release outside grid boundaries
  Result: ❌ Drag cancelled, columns unchanged
```

**Critical Decision Points:**

1. **Feature Priority** — Grouping (`e-groupdroparea`) takes priority over reordering
2. **Target Detection** — Drop target checked via `parentsUntil()` to find closest `.e-groupdroparea` ancestor
3. **Mutual Exclusion** — Single drag operation can result in EITHER reorder OR grouping, never both
4. **No Error State** — If dropped on group area, grouping quietly takes over (no conflict error)

**Why This Design:**

- **Group area is explicit UI** → User intent to group is clear when dropping on it
- **Reorder is implicit position-based** → Drop over headers triggers reorder
- **Mutual exclusion prevents ambiguity** → One drop = one operation
- **Consistent UX** → User always gets the result they "aimed for" with mouse position

---

### Scenario 5: Column Chooser Reordering

**UI Flow:**

```
User clicks "Column Chooser" toolbar button
                ↓
ColumnChooser modal opens showing available columns:
  ┌─────────────────────────┐
  │ Column Chooser          │
  ├─────────────────────────┤
  │ ☑ OrderID               │ ← Drag handle
  │ ☑ CustomerID            │
  │ ☑ ShipName              │
  │ ☐ ShipAddress           │
  │ ☑ ShipCity              │
  └─────────────────────────┘
                ↓
User drags "ShipName" → Position between "OrderID" and "CustomerID"
                ↓
ColumnChooser internal drag-drop reorders local list:
  chosenColumns = [OrderID, CustomerID, ShipName, ShipCity]
                ↓
Reordered: [OrderID, ShipName, CustomerID, ShipCity]
                ↓
User clicks "Apply" / "OK" button
                ↓
ColumnChooser.OnApplyClick():
  newColumnOrder = chosenColumns.ToList()
  For i = 0 to newColumnOrder.Count:
    oldIndex = Parent.Columns.IndexOf(newColumnOrder[i])
    newIndex = i
    If oldIndex != newIndex:
      SfGrid.ReorderColumnByIndex(oldIndex, newIndex)
                ↓
ReorderColumnByIndex loops through all columns:
  reorderModule.reorderColumnByIndex(0, 0)  // OrderID stays
  reorderModule.reorderColumnByIndex(2, 1)  // ShipName moves to 1
  reorderModule.reorderColumnByIndex(1, 2)  // CustomerID shifts to 2
  reorderModule.reorderColumnByIndex(3, 3)  // ShipCity stays
                ↓
Server-side Reorder<T>.ReorderColumn(fromIdx, toIdx):
  Swap columns[fromIdx] ↔ columns[toIdx]
  After each swap, Parent.Columns updated
                ↓
Final result: [OrderID, ShipName, CustomerID, ShipCity]
                ↓
StateHasChanged() → GridHeader re-renders
                ↓
ColumnChooser modal closes
```

**Code Snippet (TypeScript):**

```typescript
// In scripts/reorder.ts
public reorderColumnByIndex(fromIndex: number, toIndex: number): void
{
    const column: Column = this.parent.getColumnByIndex(fromIndex);
    this.moveTargetColumn(column, toIndex);
}

private moveTargetColumn(column: Column, toIndex: number) : void
{
    if (toIndex > -1) {
        this.moveColumns(toIndex, column, true);
    }
}

// In scripts/column-chooser.ts (hypothetical)
public async OnApplyClick(): Promise<void>
{
    const chosenColumns = this.ChosenColumns;
    for (let i = 0; i < chosenColumns.length; i++) {
        const column = chosenColumns[i];
        const oldIndex = this.Parent.Columns.IndexOf(column);
        const newIndex = i;
        if (oldIndex !== newIndex) {
            await this.Parent.ReorderColumnByIndex(oldIndex, newIndex);
        }
    }
    this.Close();
}
```

---

### Scenario 5.5: Drag Clone Visual DOM Structure (During Drag Operation)

**Scenario: User is actively dragging a column header — understanding the visual clone element**

**Full DOM During Drag Operation:**

```
┌─────────────────────────────────────────────────────────────────┐
│ <div class="e-grid">                                             │
│                                                                   │
│  ┌─ <div class="e-headercontainer">                             │
│  │  ┌─ <div class="e-headercontent">                            │
│  │  │   <table class="e-table">                                 │
│  │  │     <thead>                                               │
│  │  │       <tr class="e-columnheader">                         │
│  │  │         <th> OrderID    </th>                             │
│  │  │         <th> ← Dragging CustomerID                       │
│  │  │         <th> ShipName   </th>                             │
│  │  │       </tr>                                               │
│  │  │     </thead>                                              │
│  │  │   </table>                                                │
│  │  │                                                            │
│  │  │   ↑ ↑ ↑  (e-reorderuparrow - visible when drop valid)    │
│  │  │                                                            │
│  │  └─ <div class="e-cloneproperties e-dragclone e-headerclone">│
│  │     ┌─────────────────────────────────────┐                 │
│  │     │ CustomerID                          │  ← CLONE ELEMENT
│  │     │ (floating, follows mouse cursor)    │     (position: absolute)
│  │     └─────────────────────────────────────┘                 │
│  │                                                               │
│  │     ↓ ↓ ↓  (e-reorderdownarrow - visible when drop valid)   │
│  │                                                               │
│  └─ </div>                                                       │
│  └─ </div>                                                       │
│                                                                   │
│  ┌─ <div class="e-gridcontent">                                │
│  │  ... (grid body rows)                                        │
│  └─ </div>                                                       │
│                                                                   │
└─────────────────────────────────────────────────────────────────┘
```

**Clone Element (e-cloneproperties) Properties:**

```javascript
// Created by: header-drag-drop.ts helper() method
// Lifecycle: Created on drag START → Destroyed on drag STOP

Clone Structure: {
  element:        <div> containing column header cell content
  className:      "e-cloneproperties e-dragclone e-headerclone"
  styling:        {
    position:       "absolute"
    top:            calculated pixel offset from mouse
    left:           calculated pixel offset from mouse
    zIndex:         "10000" (always above grid content)
    backgroundColor: matches header background
    border:         "1px solid #ccc" (visual boundary)
    padding:        "4px 8px"
    opacity:        "0.9" (slightly transparent)
  }
  content:        <th> inner HTML cloned from source header cell
  parent:         document.body (appended temporarily)
  lifetime:       Only exists during drag (destroyed on drop)
}
```

**Drag Lifecycle with Clone:**

```
DRAG INITIALIZATION (dragStart):
  1. User mousedown on header cell (e.g., CustomerID)
  2. HeaderDragDrop.ondragstart() triggers
  3. reorderModule.dragStart() called
  4. helper() method invoked → Creates clone
  
  helper() {
    let cloneElement = this.gridHeader.element
      .querySelector('th[e-mappinguid="CustomerID"]')
      .cloneNode(true);  // Deep copy of header cell
    
    let clone = document.createElement('div');
    clone.className = 'e-cloneproperties e-dragclone e-headerclone';
    clone.appendChild(cloneElement);
    clone.style.position = 'absolute';
    clone.style.zIndex = '10000';
    
    document.body.appendChild(clone);  // Append to body for floating effect
    return clone;
  }

DRAG IN PROGRESS (drag event fires many times):
  4. ondrag() repositions clone to follow mouse cursor
     clone.style.left = event.pageX + offset + 'px';
     clone.style.top  = event.pageY + offset + 'px';
  
  5. For each drag position:
     - Calculate valid drop target (based on mouse X position)
     - If valid → Show arrows (e-reorderuparrow, e-reorderdownarrow)
     - If invalid → Hide arrows, show "no-drop" cursor
  
  6. Original header cell styles change:
     - e-clonedheader class added (slightly faded appearance)
     - Visual indication that it's being dragged

DRAG END (dragStop):
  7. User releases mouse
  8. ondragend() or dragStop() called
  9. Clone element removed from document:
     clone.remove();
  
  10. Reorder logic executes if valid drop position
  11. Grid re-renders with new column order
  12. Original header cell restored to normal appearance
      - e-clonedheader class removed
```

**Visual Appearance During Drag:**

```
BEFORE DRAG:
  ┌─────────┬──────────┬──────────┐
  │ OrderID │ Customer │ ShipName │
  └─────────┴──────────┴──────────┘

DURING DRAG (mouse over ShipName):
  ┌─────────┬──────────┬──────────┐
  │ OrderID │ Customer │ ShipName │ (original header, slightly faded)
  └─────────┴──────────┴──────────┘
                ↑ ↑ ↑  (insert arrows visible)
  
       ┌──────────┐
       │ Customer │ (clone: floating element, follows mouse)
       └──────────┘
       cursor: ⊕ (move indicator)

AFTER DRAG (on drop):
  Clone removed, column order swapped:
  ┌──────────┬─────────┬──────────┐
  │ Customer │ OrderID │ ShipName │ (NEW ORDER)
  └──────────┴─────────┴──────────┘
```

**Code Location (header-drag-drop.ts):**

```typescript
private helper = (args: DragEventArgs): Element | string | false => {
    const gridHeader = this.gridHeader;
    const draggedTH = args.target as HTMLElement;
    
    // Create clone of the header cell being dragged
    const cloneNode = draggedTH.cloneNode(true) as HTMLElement;
    
    const cloneWrapper = document.createElement('div');
    cloneWrapper.className = 'e-cloneproperties e-dragclone e-headerclone';
    cloneWrapper.appendChild(cloneNode);
    
    // Position: floating above grid
    cloneWrapper.style.position = 'absolute';
    cloneWrapper.style.zIndex = '10000';
    cloneWrapper.style.opacity = '0.9';
    cloneWrapper.style.border = '1px solid #cccccc';
    
    document.body.appendChild(cloneWrapper);
    this.cloneElement = cloneWrapper;  // Store reference for cleanup
    
    return cloneWrapper;
};

private ondrag = (args: DragEventArgs): void => {
    if (this.cloneElement) {
        // Update position continuously during drag
        this.cloneElement.style.left = (args.pageX + 8) + 'px';
        this.cloneElement.style.top = (args.pageY - 16) + 'px';
    }
};

private ondragend = (args: DragEventArgs): void => {
    if (this.cloneElement) {
        this.cloneElement.remove();  // ← Clone destroyed here
        this.cloneElement = null;
    }
};
```

**Critical Points:**

1. **Clone is Temporary** — Only exists during drag operation, destroyed on drop
2. **Floating Position** — `position: absolute` + appended to `document.body` makes it float above grid
3. **Visual Feedback** — User sees real-time where column is being dragged to
4. **Performance** — Clone is simple DIV with cloned innerHTML, minimal overhead
5. **Accessibility** — Clone doesn't affect grid data or accessibility tree (temporary DOM element)

---

### Scenario 6: Virtual Scroll Column Reordering

**Grid Configuration:**

```csharp
<SfGrid EnableColumnVirtualization="true" AllowReordering="true">
  <GridColumns>
    <!-- 50+ columns, but only ~5 visible at a time -->
  </GridColumns>
</SfGrid>
```

**Initial State:**

```
Total columns: 50
Visible range: StartColumnIndex=0, EndColumnIndex=4
Rendered: [col0, col1, col2, col3, col4]
Hidden (virtualized): [col5, col6, ..., col49]
                ↓
DOM:
<table>
  <tr> col0 | col1 | col2 | col3 | col4 </tr>
</table>
(Only 5 columns rendered to DOM)
```

**Reorder Action:**

```
User scrolls horizontally to show col20-col24
                ↓
VirtualScrollModule.HandleScroll():
  StartColumnIndex = 20
  EndColumnIndex = 24
  Regenerates header: [col20, col21, col22, col23, col24]
                ↓
User drags col22 → Before col20
                ↓
reorderModule.dragStart():
  this.element = <th e-mappinguid="uid-col22">
  this.draggedHeader = this.element.cloneNode(true)
                ↓
reorderModule.headerDrop() detects:
  if (this.parent.options.enableColumnVirtualization) {
    dropElement = this.draggedHeader.querySelector(...headercelldiv)
    // Use draggedHeader because DOM col22 might not be in viewport
                ↓
    sourceUid = dropElement.getAttribute('e-mappinguid')
    → "uid-col22"
                ↓
    col = this.parent.getColumns(false)
          .filter(c => c.uid === sourceUid)
    → [GridColumn for col22]
                ↓
    colMatchIndex = null
    Parent.getColumns(false).forEach(col, index) {
      if (col.uid === destUid) {
        colMatchIndex = index
        // Find index in FULL column list (not just visible)
      }
    }
    → colMatchIndex = 20 (index of col20 in full list)
                ↓
    moveColumns(20, col22Column)
  }
                ↓
Server-side swap:
  columns.splice(22, 1)  // Remove col22
  columns.splice(20, 0, col22)  // Insert at pos 20
  Result: col22 now at index 20, col20 at index 21
                ↓
Parent.VirtualScrollModule.GetVirtualColumns():
  Recalculates visible range based on UPDATED column order
  New sequence at indices 20-24: [col22, col20, col21, col23, col24]
                ↓
GridHeader.OnParametersSet():
  Retrieves virtual columns: [col22, col20, col21, col23, col24]
  Renders new header: col22 | col20 | col21 | col23 | col24
                ↓
User sees reordered virtual columns
```

**Code Snippet (Virtual Scroll Handling):**

```csharp
// In Reorder<T> (server-side)
public void ReorderColumn(int fromIndex, int toIndex, string? uid = null)
{
    List<GridColumn> scopedColumns = (List<GridColumn>)Parent.Columns!;
    
    if (!string.IsNullOrEmpty(uid) && scopedColumns != null)
    {
        // Find scope recursively (for stacked headers)
        scopedColumns = Reorder<T>.GetScopeRecursively(uid, (List<GridColumn>)Parent.Columns!);
        ReorderFrozenColumns(scopedColumns);
    }

    Swap(scopedColumns!, fromIndex, toIndex);
    
    // CRITICAL: Recalculate virtual scroll ranges
    if (Parent.EnableColumnVirtualization && Parent.VirtualScrollModule != null)
    {
        Parent.VirtualScrollModule.RecalculateColumnRanges();
    }
}

public static List<GridColumn> GetScopeRecursively(string uid, List<GridColumn> parent)
{
    if (parent.Any(c => c.Uid == uid))
    {
        return parent;
    }

    List<GridColumn>? result = null;
    parent.ForEach((col) =>
    {
        if (result == null && col?.Columns != null)
        {
            result = Reorder<T>.GetScopeRecursively(uid, (List<GridColumn>)col.Columns);
        }
    });

    return result!;
}
```

---

### Scenario 7: Persistence (Save & Restore)

**Enable Persistence:**

```csharp
<SfGrid EnablePersistence="true" AllowReordering="true">
  <!-- Columns, etc. -->
</SfGrid>
```

**After User Reorders (Save Workflow):**

```
User drags columns to new order:
  Initial: [OrderID, CustomerID, ShipName, ShipCity]
  Final:   [ShipCity, OrderID, CustomerID, ShipName]
                ↓
dotNetRef.invokeMethodAsync('ColumnReordered', {...})
                ↓
SfGrid.razor.cs → ColumnReordered(json)
                ↓
Reorder<T>.PerformReorder():
  columns.splice(...)  → Final column order applied
                ↓
  Parent.ColumnIndex = -1  // Reset index
                ↓
  Parent.SetColumnIndex(Parent.Columns!, false):
    For i = 0 to Columns.Count:
      Columns[i].Index = i
    (Columns[0].Index = 0, Columns[1].Index = 1, etc.)
                ↓
  PersistenceManager.SaveGridState():
    Serializes state to JSON:
    {
      "columns": [
        { "uid": "uid-ShipCity", "field": "ShipCity", "index": 0 },
        { "uid": "uid-OrderID", "field": "OrderID", "index": 1 },
        { "uid": "uid-CustomerID", "field": "CustomerID", "index": 2 },
        { "uid": "uid-ShipName", "field": "ShipName", "index": 3 }
      ],
      "columnOrder": ["uid-ShipCity", "uid-OrderID", "uid-CustomerID", "uid-ShipName"]
    }
                ↓
  Stores in: localStorage / sessionStorage / Database
                ↓
User navigates away
```

**On Next Page Load (Restore Workflow):**

```
Page initializes SfGrid
                ↓
SfGrid.OnAfterRenderAsync():
  PersistenceManager.LoadGridState():
    Reads from localStorage:
    {
      "columnOrder": ["uid-ShipCity", "uid-OrderID", "uid-CustomerID", "uid-ShipName"]
    }
                ↓
  For each uid in columnOrder:
    finds column by uid
    moves to position matching array index
                ↓
  Calls Reorder<T>.ReorderColumn(oldIdx, newIdx) for each
                ↓
  Final Parent.Columns order: [ShipCity, OrderID, CustomerID, ShipName]
                ↓
  Parent.SetColumnIndex(Parent.Columns, false)
    Recalculates .Index for each column
                ↓
  GridHeader renders with restored column order
                ↓
User sees grid with previously saved column order
```

**Code Snippet (C#):**

```csharp
// In SfGrid.Lifecycle.cs
protected override async Task OnAfterRenderAsync(bool firstRender)
{
    if (firstRender && EnablePersistence)
    {
        // Load saved column order from persistence provider
        var savedState = await PersistenceProvider.GetState();
        if (savedState?.ColumnOrder != null && savedState.ColumnOrder.Any())
        {
            // Reorder columns to match saved order
            ReorderModule?.RestoreColumnOrder(savedState.ColumnOrder);
            await Task.Delay(100); // Allow state settle
            StateHasChanged();
        }
    }
    await base.OnAfterRenderAsync(firstRender);
}

// In Reorder.cs
internal async Task RestoreColumnOrder(List<string> columnUids)
{
    var targetColumns = (List<GridColumn>)Parent.Columns!;
    for (int i = 0; i < columnUids.Count; i++)
    {
        var uid = columnUids[i];
        var column = targetColumns.FirstOrDefault(c => c.Uid == uid);
        if (column != null)
        {
            int currentIndex = targetColumns.IndexOf(column);
            if (currentIndex != i)
            {
                Swap(targetColumns, currentIndex, i);
            }
        }
    }
    Parent.SetColumnIndex(targetColumns, false);
}
```

---

### Scenario 8: Cross-Feature Interaction: Reorder + Sort

**Initial State:**

```csharp
<SfGrid AllowReordering="true" AllowSorting="true">
  <GridColumns>
    <GridColumn Field="OrderID"></GridColumn>
    <GridColumn Field="CustomerID"></GridColumn>
    <GridColumn Field="ShipCity"></GridColumn>
  </GridColumns>
</SfGrid>

User sorts by OrderID ascending (↑ icon appears)
```

**DOM Before Reorder:**

```
<tr class="e-columnheader">
  <th aria-colindex="1">
    <div class="e-headercelldiv">
      OrderID
      <div class="e-sortfilterdiv e-icons e-sort-ascending"></div>
    </div>
  </th>
  <th aria-colindex="2">
    <div class="e-headercelldiv">
      CustomerID
    </div>
  </th>
  <th aria-colindex="3">
    <div class="e-headercelldiv">
      ShipCity
    </div>
  </th>
</tr>
```

**Reorder Action: Drag CustomerID → Position 0**

```
User drags CustomerID <th> before OrderID <th>
                ↓
moveColumns(0, customerIdColumn)
  columns.splice(0, 0, columns.splice(1, 1)[0])
  Result: [CustomerID, OrderID, ShipCity]
                ↓
dotNetRef.invokeMethodAsync('ColumnReordered')
                ↓
GridHeader re-renders
                ↓
GridHeaderCell for each column:
  For column = CustomerID (now at index 0):
    No sort applied to CustomerID
    Render: <div class="e-headercelldiv">CustomerID</div>
    (no sort glyph)
                ↓
  For column = OrderID (now at index 1):
    Check: SortSettings.Columns.Any(col => col.Field == "OrderID")
    → TRUE (still sorted ascending)
    Render: <div class="e-headercelldiv">
              OrderID
              <div class="e-sort-ascending"></div>
            </div>
    (sort glyph STILL appears, now on right side)
                ↓
  For column = ShipCity (now at index 2):
    No sort applied
    Render: <div class="e-headercelldiv">ShipCity</div>
```

**DOM After Reorder:**

```
<tr class="e-columnheader">
  <th aria-colindex="1">
    <div class="e-headercelldiv">
      CustomerID
    </div>
  </th>
  <th aria-colindex="2">
    <div class="e-headercelldiv">
      OrderID
      <div class="e-sortfilterdiv e-icons e-sort-ascending"></div>
    </div>
  </th>
  <th aria-colindex="3">
    <div class="e-headercelldiv">
      ShipCity
    </div>
  </th>
</tr>
```

**Critical Point:**

```
Sort state is tied to Column.Field ("OrderID"), NOT to position.
When column moves, sort follows the column.
User sees: OrderID column now at visual position 2, sort still applied ✓
Grid data sorted by OrderID ascending (unchanged) ✓
Zero regression!
```

---

### Scenario 9: Cross-Feature Interaction: Reorder + Grouping

**Initial Configuration:**

```csharp
<SfGrid AllowReordering="true" AllowGrouping="true">
  <GridGroupSettings Columns="@new List<string> { "ShipCity", "OrderID" }"></GridGroupSettings>
  <GridColumns>
    <GridColumn Field="ShipCity"></GridColumn>
    <GridColumn Field="OrderID"></GridColumn>
    <GridColumn Field="CustomerID"></GridColumn>
    <GridColumn Field="Freight"></GridColumn>
  </GridColumns>
</SfGrid>
```

**DOM Before Reorder (with grouping):**

```
Columns rendered:
  [GroupIndent, ShipCity(grouped), OrderID(grouped), CustomerID, Freight]

<tr class="e-columnheader">
  <th class="e-grouptopleftcell">  ← Indent for grouping
    <div class="e-emptycell"></div>
  </th>
  <th>
    <div class="e-headercelldiv">
      ShipCity
      <span class="e-grptogglebtn">⊟ Ungroup</span>
    </div>
  </th>
  <th>
    <div class="e-headercelldiv">
      OrderID
      <span class="e-grptogglebtn">⊟ Ungroup</span>
    </div>
  </th>
  <th>
    <div class="e-headercelldiv">
      CustomerID
    </div>
  </th>
  <th>
    <div class="e-headercelldiv">
      Freight
    </div>
  </th>
</tr>

Data rows grouped:
  ShipCity: NY
    OrderID: 1001  | CustomerID: ALFKI | Freight: 10
    OrderID: 1002  | CustomerID: ANATR | Freight: 20
  ShipCity: LA
    OrderID: 1003  | CustomerID: ANTON | Freight: 15
```

**Reorder Action: Drag CustomerID → Position 1 (after ShipCity)**

```
User drags CustomerID <th> to position 1
                ↓
moveColumns(1, customerIdColumn):
  columns.splice(1, 0, columns.splice(2, 1)[0])
  Result: Parent.Columns = [ShipCity, CustomerID, OrderID, Freight]
                ↓
GridGroupSettings.Columns UNCHANGED:
  Still = ["ShipCity", "OrderID"]
                ↓
GridHeader re-renders columns in NEW order:
  [GroupIndent, ShipCity, CustomerID, OrderID, Freight]
```

**DOM After Reorder:**

```
<tr class="e-columnheader">
  <th class="e-grouptopleftcell">
    <div class="e-emptycell"></div>
  </th>
  <th>
    <div class="e-headercelldiv">
      ShipCity
      <span class="e-grptogglebtn">⊟ Ungroup</span>
    </div>
  </th>
  <th>
    <div class="e-headercelldiv">
      CustomerID
    </div>
  </th>
  <th>
    <div class="e-headercelldiv">
      OrderID
      <span class="e-grptogglebtn">⊟ Ungroup</span>
    </div>
  </th>
  <th>
    <div class="e-headercelldiv">
      Freight
    </div>
  </th>
</tr>
```

**Data Rows After Reorder:**

```
Still grouped by ShipCity, then OrderID:
  ShipCity: NY
    OrderID: 1001 | CustomerID: ALFKI | Freight: 10
    OrderID: 1002 | CustomerID: ANATR | Freight: 20
  ShipCity: LA
    OrderID: 1003 | CustomerID: ANTON | Freight: 15

Grouping logic UNCHANGED:
  - Groups still collapse/expand by ShipCity ✓
  - Subgroups still by OrderID ✓
  - Aggregates still calculated per group ✓
```

**Critical Points:**

```
1. GroupSettings.Columns tracks by field name, NOT by position
   → "ShipCity" still grouped even though it moved to position 1

2. Reordering a grouped column keeps it grouped
   Reordering an ungrouped column keeps it ungrouped
   (Column position ≠ Group state)

3. Data bucketing unaffected:
   Grid still sorts/groups data by ShipCity, then OrderID
   Column render order only affects visual layout

4. Group toggle buttons stay on grouped columns:
   ShipCity: ⊟ Ungroup (still present)
   OrderID: ⊟ Ungroup (still present)

✓ Zero regression: Grouping integrity preserved!
```

---

## Scenario Decision Tree (For LLM Code Generation)

<!-- token-budget: 150 words -->

**When implementing a reorder feature, ask:**

```
1. Is reorder INITIATED BY USER?
   YES → Is it drag-drop?
         YES → Use scripts/reorder.ts + HeaderDragDrop
         NO → Is it column menu?
              YES → Use SfGrid.ReorderColumnAsync(field1, field2)
              NO → Is it column chooser?
                   YES → Use ReorderColumnByIndex(idx1, idx2) in bulk
2. Is reorder INITIATED BY CODE (API)?
   YES → Which API?
         - ReorderColumnAsync(fromField, toField) → Single field-based
         - ReorderColumnsAsync(fromFields[], toField) → Multiple fields
         - ReorderColumnByIndex(fromIdx, toIdx) → Index-based
         - ReorderColumnByTargetIndex(field/fields[], toIdx) → Target index
3. Do FROZEN COLUMNS exist?
   YES → Call ReorderFrozenColumns() in server action
4. Are STACKED HEADERS present?
   YES → Call processStackedColumns() to flatten for validation
5. Is VIRTUALIZATION enabled?
   YES → Update VirtualScrollModule column ranges post-reorder
6. Is PERSISTENCE enabled?
   YES → Call Parent.SetColumnIndex() to update indices for save
7. Fire events in order:
   FIRST → ColumnReordering (pre-action, cancellable)
   THEN → Perform reorder
   LAST → ColumnReordered (post-action, informational)
```

---

## Prompt Template (For Custom Agent Invocation)

<!-- token-budget: 300 words -->

```
Skill: Column Reordering
Mode: [feature-implementation | bug-fix]
Context: Blazor DataGrid Column Reordering

TASK BRIEFING:
You are implementing or debugging a {MODE} in the Syncfusion Blazor DataGrid 
column reordering feature. This feature allows users and code to reorder columns 
via drag-drop, column menu, column chooser, or public APIs.

SCOPE:
- TypeScript layer: /scripts/reorder.ts, /scripts/header-drag-drop.ts
- C# server layer: /src/Internal/Actions/Reorder.cs, /src/SfGrid.Methods.cs
- Rendering: /src/Internal/Renderer/GridHeader.razor, GridHeaderCell.razor
- Event callbacks: /src/GridEvents.cs, /src/EventModels/Grids.cs

MANDATORY CONSTRAINTS:
1. Frozen columns (IsFrozen=true) cannot reorder outside their frozen section.
2. FixedColumn=true columns cannot reorder at all.
3. Stacked headers must reorder parents + children together; indices must align.
4. Column virtualization: Update VirtualScrollModule ranges post-reorder.
5. After reorder, call Parent.SetColumnIndex() to update indices for persistence.
6. Drag-drop abort on .e-rhandler (resize handle); use Draggable abort: '.e-rhandler'.
7. Sort/Filter/Group state must be tied to column field, NOT position.
8. Fire ColumnReordering event BEFORE reorder; ColumnReordered AFTER.
9. Use column UID for identity tracking, field name for data binding.
10. In persistence scenarios, restore column order from saved UID sequence.

REFERENCE MATERIALS:
- Interaction Matrix (above) lists all cross-feature combinations.
- DOM Rendering Structure (above) shows all 9 scenario flows.
- Training Insights (above) documents edge cases & rules.

BEFORE YOU WRITE CODE:
1. Identify which of the 9 scenarios your task touches.
2. Consult the Decision Tree (above) to pick the right code path.
3. Cross-reference the Interaction Matrix to ensure zero regression.
4. Check if frozen columns or stacked headers are involved; if yes, add validation.

After completing your code, verify:
- All 9 scenarios still work (if your change is global)
- No frozen/stacked header edge case violations
- Events fire in correct order
- VirtualScrollModule updated (if applicable)
- Persistence indices refreshed (if applicable)
```

---

## Implementation Checklist

<!-- token-budget: 100 words -->

**Use this when implementing a reorder feature:**

- [ ] **Drag-Drop Flow** — Verify `dragStart()` → `drag()` → `dragStop()` → `headerDrop()` chain
- [ ] **Frozen Validation** — Check `chkDropPosition()` + `targetParentContainerIndex()` return valid indices
- [ ] **Stacked Header Handling** — If stacked columns exist, verify `processStackedColumns()` flattens correctly
- [ ] **Column Swap** — Confirm `columns.splice()` preserves column field/uid identity
- [ ] **Event Sequence** — Verify `ColumnReordering` fires before swap, `ColumnReordered` after
- [ ] **Virtualization** — If `EnableColumnVirtualization=true`, update VirtualScrollModule ranges
- [ ] **Persistence** — Call `SetColumnIndex()` post-reorder if `EnablePersistence=true`
- [ ] **Cross-Feature** — Test sort/filter/group state survived the reorder
- [ ] **Visual Indicators** — Arrow up/down appear on valid drop targets only
- [ ] **Cursor Feedback** — Show `e-notallowedcur` on invalid targets, `e-defaultcur` otherwise

---

## Related Features & Cross-References

<!-- token-budget: 60 words -->

- **Sorting** (`sorting-skill.md`) — Sort state indexed by column field; reorder does NOT affect
- **Filtering** (`filtering-skill.md`) — Filter state indexed by column field + UID; cross-verified on reorder
- **Grouping** (`grouping-skill.md`) — Grouped column positions must be reordered in sync
- **Column Virtualization** (`column-virtualization-skill.md`) — Virtual column ranges must be recalculated
- **Frozen Columns** (`frozen-column-skill.md`) — Frozen sections separate from movable; reorder respects boundary
- **Selection** (`selection-skill.md`) — Row selection by UID; unaffected by column reorder
- **Persistence** — Column order persisted; restored on reload via UID sequence

---

## Appendix: Glossary

| Term | Definition |
|------|-----------|
| **e-headercell** | CSS class on `<th>` header cell container |
| **e-mappinguid** | Attribute on inner `<div>` storing column UID for identification |
| **e-reorderuparrow / e-reorderdownarrow** | Visual indicators showing drop target position (up/down) |
| **e-cloneproperties** | CSS class on drag ghost element created by HeaderDragDrop.helper() |
| **allowReordering** | IGridOptions flag; if false, Reorder class not instantiated |
| **targetParentContainerIndex()** | Validates drop target is in same frozen section; returns -1 if invalid |
| **processStackedColumns()** | Flattens stacked column hierarchy to 1D array for index calculation |
| **ColumnReordered** | .NET event callback fired AFTER reorder completes |
| **ColumnReordering** | .NET event callback fired BEFORE reorder; cancellable |
| **FrozenColumns** | Count of columns frozen on left (frozen section stays left) |
| **EnableColumnVirtualization** | If true, only visible columns rendered; ranges recalculated on reorder |

---

**End of Skill Document**
