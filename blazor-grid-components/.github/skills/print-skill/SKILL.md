---
name: print-skill
description: Expert knowledge for the Print feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to print behaviour, including print rendering, off-screen DOM cloning, print mode selection (AllPages/CurrentPage), hierarchy print modes (Expanded/All/None), cross-feature print rendering with grouping, sorting, filtering, paging, aggregates, templates, virtualization, and print-specific DOM transformations.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Print in the Blazor DataGrid. Guarantees zero breakage with any other feature during print rendering and DOM cloning.

---

**Agent Invocation**  
Load this skill when implementing print features, debugging print rendering issues, handling print mode selection, adding print-specific DOM transformations, or fixing regressions involving print with grouping, sorting, filtering, paging, virtualization, aggregates, templates, hierarchy grids, and keyboard shortcuts.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, component lifecycle, rendering pipeline
- `/docs/architecture/system-architecture.md` — Grid rendering, virtual scroll, feature module integration
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# async/await rules, CSS class naming conventions
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, guard clauses, event pipeline

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — print must clone full grid state, forward all feature settings (sorting, filtering, grouping, aggregates), disable virtual scroll in print clone, fix grouped colspan via `removeColGroup()` in JS, and preserve selection/edit settings
- **Off-screen DOM cloning strategy** — `.e-print-offscreen` div hosts cloned grid; print grid ID includes `_sfprint` suffix to avoid conflicts with main grid
- **Print mode semantics** — `AllPages` renders full dataset in cloned grid; `CurrentPage` forces `AllowPaging=true` in clone with persisted `CurrentPage` value
- **Keyboard trigger** — `Ctrl+P` / `Cmd+P` calls `PrintAsync()` via FocusHandler; sets `IsPrinting=true` → StateHasChanged() → PrintLayout injected
- **Hierarchy print modes** — `Expanded` pre-expands detail rows before print; `All` expands ALL regardless of UI state; `None` prints master grid only
- **Virtual scroll disabled in print** — PrintLayout never passes `EnableVirtualization` flag; print always renders flat full set
- **Cross-module DOM sync** — Print clone forwards ALL settings (GroupSettings.Columns, SortSettings.Columns, FilterSettings.Columns, Aggregates, Templates); these determine printed structure

---

## Code Location Map
<!-- token-budget: 80 words -->

| Path | Purpose |
|------|---------|
| `src/Internal/Renderer/PrintLayout.razor` | Print DOM host: cloned grid off-screen; forwards all settings; disabled virtualization; triggers `InvokePrint()` on DataBound |
| `src/Internal/SfGrid.razor` (line 181) | `@if (IsPrinting)` injects `<PrintLayout>` outside main grid |
| `src/SfGrid.razor.cs` | `IsPrinting` flag declaration (internal bool) |
| `src/SfGrid.Methods.cs` (line 2185) | `PrintAsync()` public API: sets `IsPrinting=true`, calls `CallStateHasChangedAsync()` |
| `src/SfGrid.Properties.cs` (lines 932-993) | `PrintMode` (AllPages/CurrentPage) at line 991, `HierarchyPrintMode` (Expanded/All/None) at line 955, `HierarchyGridPrintMode` enum at line 201 |
| `src/SfGrid.Lifecycle.cs` (lines 449-451, 543, 545) | Caches `_printMode`, `_hierarchyPrintMode` during `OnParametersSet` |
| `src/Enumeration/GridsEnumerations.cs` (lines 201, 236) | `HierarchyGridPrintMode` enum at line 201; `PrintMode` enum at line 236 |
| `src/Internal/Actions/FocusHandler.cs` (lines 820-821) | `CtrlP` / `MetaP` keyboard handler calls `PrintAsync()` |
| `scripts/sf-grid-fn.ts` (lines 2076-2084) | `print()` method at line 2076: opens browser window; `removeColGroup()` at line 2084: fixes grouped colspan; invokes EJ2 base `print(element, window)` |
| `scripts/sf-grid.ts` (line 906) | `printGrid(dataId)` JS interop bridge: retrieves cloned grid instance, calls `.print()` |
| `src/GridGroupSettings.razor.cs` | `ShowDropArea`, `Columns` — forwarded to print clone |
| `src/GridSortSettings.razor.cs` | `Columns` (SortDescriptor[]) — forwarded to print clone |
| `src/GridFilterSettings.razor.cs` | `Columns` (FilterDescriptor[]) — forwarded to print clone |
| `src/GridPageSettings.razor.cs` | `CurrentPage`, `PageSize` — forwarded when `PrintMode==CurrentPage` |
| `src/GridAggregates.razor` + `GridAggregate.razor` | Aggregates — footer rows in print clone |
| `src/GridTemplates.cs` | `RowTemplate`, `DetailTemplate` — forwarded to print clone |
| `src/Internal/Renderer/GridDetailRow.razor` | Detail rows (hierarchy) — cloned based on `HierarchyPrintMode` |
| `src/Internal/Renderer/GroupCaptionRenderer.cs` | Group captions in print — `removeColGroup()` fixes colspan |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Print + Grouping | Group columns + caption rows forwarded to print clone; `removeColGroup()` fixes colspan depth; `ShowDropArea=false` forced in clone | Critical |
| Print + Sorting | Sort columns forwarded to print clone; sorted rows rendered in print order | Critical |
| Print + Filtering | Filter columns forwarded to print clone; filtered rows rendered in print | Critical |
| Print + Paging | `AllowPaging=true` forced in clone when `PrintMode==CurrentPage`; `CurrentPage` preserved; `AllPages` disables paging in clone | Critical |
| Print + Aggregates | Aggregate settings forwarded; footer rows rendered in print clone | High |
| Print + Templates | `RowTemplate`, `DetailTemplate` forwarded; print renders template-based rows | High |
| Print + Virtualization | Virtualization DISABLED in print clone; `EnableVirtualization` not forwarded; print always renders flat full set | Critical |
| Print + Hierarchy (Detail Rows) | `HierarchyPrintMode` controls detail row inclusion: `Expanded` pre-expands visible details; `All` expands ALL; `None` prints master only | Critical |
| Print + Selection | Selection clearing NOT applied in print clone; `AllowSelection=false` forced in clone | Medium |
| Print + Keyboard | `Ctrl+P` / `Cmd+P` triggers `PrintAsync()` → `IsPrinting=true` → PrintLayout injection | Medium |
| Print + Editing | Edit mode NOT forwarded to print clone; `AllowEditing=false` forced in clone | Medium |
| Print + State Persistence | Print settings (PrintMode, HierarchyPrintMode) may be persisted via `EnablePersistence`; restored on next render | Low |

---

## ASCII Diagrams & DOM Rendering Structure
<!-- token-budget: 200 words -->

### Print Rendering Flow (High-Level)

```
+---------------------------------------------------------------+
| User Interaction / API Call                                   |
+---------------------------------------------------------------+
|  [A] Ctrl+P / Cmd+P  --> FocusHandler.ProcessKeyDown()        |
|  [B] await grid.PrintAsync() --> SfGrid.Methods.cs            |
|  [C] Toolbar Print Button  --> Click Handler                  |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| SfGrid.PrintAsync()  [SfGrid.Methods.cs]                      |
+---------------------------------------------------------------+
|  Step 1 : IsPrinting = true                                   |
|  Step 2 : await CallStateHasChangedAsync()                    |
|  Step 3 : Triggers Blazor component re-render                 |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| SfGrid.razor : @if (IsPrinting) check  [line 181]            |
+---------------------------------------------------------------+
|  Condition = TRUE                                             |
|  --> Injects: <PrintLayout Parent="this" />                   |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| PrintLayout.razor  Component Lifecycle                        |
+---------------------------------------------------------------+
|  OnInitialized():                                             |
|    * Reads Parent grid settings                               |
|    * Builds DataManager attributes dict (if DataManager used) |
|                                                               |
|  Renders:                                                     |
|    <div class="e-print-offscreen">                            |
|      <SfGrid @ref="_printGrid"                                |
|              ID="Parent.ID + '_sfprint'">                     |
|        DataSource    : Parent.DataSource                      |
|        Query         : Parent.Query.Clone()                   |
|        AllowSorting  : Parent.AllowSorting                    |
|        AllowFiltering: Parent.AllowFiltering                  |
|        AllowGrouping : Parent.AllowGrouping                   |
|        Columns       : Parent.Columns                         |
|        Aggregates    : Parent.Aggregates                      |
|        Templates     : Parent.GridTemplates (Row+Detail)      |
|        PageSettings  : forwarded if PrintMode==CurrentPage    |
|        GroupSettings : ShowDropArea=false (forced)            |
|        AllowSelection: false (forced)                         |
|        Height/Width  : "auto" (forced)                        |
|        Toolbar       : null (forced)                          |
|        Virtualization: NOT forwarded (always false)           |
|      </SfGrid>                                                |
|    </div>                                                     |
|                                                               |
|  EventRegister "InternalDataBound" --> InvokePrint()          |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| Print Grid Rendering  (Off-Screen DOM)                        |
+---------------------------------------------------------------+
|  * SfGrid<T> renders fully inside .e-print-offscreen div      |
|  * Not visible to user (CSS off-viewport)                     |
|  * Grouping, Sorting, Filtering all render normally           |
|  * Virtual scroll DISABLED (defaults to GridContent)          |
|  * Full dataset loaded and rendered into DOM                  |
|  * DataBound event fires when render is complete              |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| PrintLayout.InvokePrint()  Handler                            |
+---------------------------------------------------------------+
|  * EventRegister("InternalDataBound") fires                   |
|  * Calls JS interop:                                          |
|    _printGrid.InvokeMethod(                                   |
|      "sfBlazor.Grid.printGrid",                               |
|      new object[] { _printGrid.DataId })                      |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| sf-grid.ts : printGrid(dataId)  [line 906]                   |
+---------------------------------------------------------------+
|  1. Get instance: sfBlazor.getCompInstance(dataId)            |
|  2. Call: element.blazor__instance.print()                    |
|     --> routes to sf-grid-fn.ts print() method                |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| sf-grid-fn.ts : print()  [line 2076]                         |
+---------------------------------------------------------------+
|  Step A : removeColGroup()                                    |
|           * Normalises colspan on all group caption rows      |
|           * Hides indent <col> elements at grouping depth     |
|                                                               |
|  Step B : window.open('', 'print',                            |
|             'height='+outerHeight+',width='+outerWidth)       |
|           * Opens new browser window full-screen              |
|                                                               |
|  Step C : print(this.element, printWind)   [EJ2 base]         |
|           * Copies print grid DOM into new window             |
|           * Injects print CSS                                 |
|           * Triggers browser native print dialog              |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| Browser Print Dialog                                          |
+---------------------------------------------------------------+
|  * User selects printer / Save as PDF / Cancel                |
|  * Print preview shown (browser-dependent)                    |
|  * User confirms -> pages sent to printer                     |
+---------------------------+-----------------------------------+
                            |
                            v
+---------------------------------------------------------------+
| PrintLayout : Cleanup                                         |
+---------------------------------------------------------------+
|  * IsPrinting = false                                         |
|  * Parent.CallStateHasChanged()                               |
|  * PrintLayout component unmounts from DOM                    |
|  * Main grid resumes normal rendering                         |
+---------------------------------------------------------------+
```

---

### DOM Structure: Main Grid vs Print Clone

```
MAIN GRID RENDERING  (SfGrid.razor)
====================================

<div id="grid-main" role="grid">
  |
  +-- <div class="e-gridheader">
  |     +-- <div class="e-headercontent">
  |           +-- <GridHeader>  (or <GridVirtualHeader> if virtual cols)
  |
  +-- <div class="e-content">
  |     +-- <div class="e-gridcontent">
  |           +-- <table>
  |                 +-- <colgroup>
  |                 +-- <tbody>
  |                 |     +-- <GridRow>        [data rows]
  |                 |     +-- <GridRow>        [GroupCaption row - if grouping]
  |                 |     +-- <GridDetailRow>  [detail/child rows - if hierarchy]
  |                 |     +-- ...
  |                 +-- <tfoot>               [aggregate footer - if aggregates]
  |
  +-- <SfPager>          (rendered only if AllowPaging=true)
  +-- <ColumnChooser>    (rendered only if ShowColumnChooser=true)
  +-- <ColumnMenu>       (rendered only if ShowColumnMenu=true)
  +-- <ContextMenu>      (rendered only if ContextMenuItems set)
  +-- <DialogEdit>       (rendered only if EditMode=Dialog)
  +-- <GridTooltip>      (rendered only if ShowTooltip=true)


PRINT CLONE RENDERING  (PrintLayout.razor injects SfGrid<T> clone)
====================================================================

<div class="e-print-offscreen">           <-- off-screen host div
  |
  +-- <div id="[gridId]_sfprint"          <-- "_sfprint" suffix avoids ID clash
          role="grid"
          class="e-print-grid-layout">
        |
        +-- <div class="e-gridheader">
        |     +-- <div class="e-headercontent">
        |           +-- <GridHeader>      (same column structure as main grid)
        |
        +-- <div class="e-content">
              +-- <div class="e-gridcontent">
                    +-- <table>
                          |
                          +-- <colgroup>
                          |     NOTE: after removeColGroup() runs in JS,
                          |     indent <col> elements are hidden (display:none)
                          |     so grouped colspan values render correctly.
                          |
                          +-- <tbody>
                          |     |
                          |     +-- <GridRow> x N rows
                          |     |     PrintMode=AllPages  --> ALL rows rendered
                          |     |     PrintMode=CurrentPage --> current page only
                          |     |
                          |     +-- <GridRow RowType="GroupCaption">
                          |     |     +-- <td class="e-indentcell"> (per level)
                          |     |     +-- <td class="e-groupcaption"
                          |     |              colspan=N>  (fixed by JS)
                          |     |           [+] GroupKey (item count)
                          |     |         child data rows follow when Visible=true
                          |     |
                          |     +-- <GridDetailRow>
                          |     |     rendered per HierarchyPrintMode:
                          |     |       Expanded --> only IsExpand=true rows
                          |     |       All      --> all rows force-expanded
                          |     |       None     --> skipped entirely
                          |     |     <td colspan="totalCols">
                          |     |       [DetailTemplate content]
                          |     |     </td>
                          |     |
                          |     +-- <tr class="e-summaryrow"> (if aggregates)
                          |
                          +-- <tfoot>
                                +-- <tr class="e-summary-row">
                                      [aggregate footer rows]

        COMPONENTS EXCLUDED FROM PRINT CLONE:
        +-- SfPager        HIDDEN  (AllowPaging forced false unless CurrentPage mode)
        +-- SfSpinner      SKIPPED (ID contains "_sfprint" check in SfGrid.razor)
        +-- GridToolbar    SKIPPED (Toolbar=null forced)
        +-- ColumnChooser  SKIPPED
        +-- ContextMenu    SKIPPED
        +-- DialogEdit     SKIPPED
        +-- GridTooltip    SKIPPED
        +-- AllowSelection FORCED false (no selection highlight)
        +-- Virtualization FORCED false (GridContent used, not GridVirtualContent)

```

---

### Print Rendering Flow: Feature-by-Feature DOM Structure

```
SCENARIO 1: Grouping + Sorting + Filtering + Aggregates
═══════════════════════════════════════════════════════════════

Source State:
  ├─ AllowGrouping = true
  │  └─ GroupSettings.Columns = ["Country", "City"]
  │
  ├─ AllowSorting = true
  │  └─ SortSettings.Columns = [{Field: "Country", Direction: Ascending, IsFromGroup: true},
  │                              {Field: "OrderDate", Direction: Descending}]
  │
  ├─ AllowFiltering = true
  │  └─ FilterSettings.Columns = [{Field: "Amount", Predicate: "equal", Value: 1000}]
  │
  └─ Aggregates: [{Field: "Amount", Type: "Sum", FooterTemplate: ...}, ...]


Print Clone DOM After DataBound:

  +---------------------------------------------------------------+
  | <table>  [Print grid DOM — after DataBound fires]             |
  +---------------------------------------------------------------+
  | <thead>                                                       |
  |   <tr class="e-columnheader">                                 |
  |     <th class="e-headercell"> [Indent] </th>  <- group indent |
  |     <th class="e-headercell"> Country  </th>  <- grouped col  |
  |     <th class="e-headercell"> City     </th>  <- grouped col  |
  |     <th class="e-headercell"> OrderDate</th>  <- sorted desc  |
  |     <th class="e-headercell"> Amount   </th>  <- aggregated   |
  |   </tr>                                                       |
  | </thead>                                                      |
  +---------------------------------------------------------------+
  | <tbody>                                                       |
  |                                                               |
  |  -- Group Level 1 : Country = "USA" --                        |
  |   <tr class="e-groupcaption e-caption-row">                   |
  |     <td class="e-indentcell"></td>                            |
  |     <td class="e-groupcaption" colspan="4">                   |
  |       ^--- colspan FIXED by removeColGroup()                  |
  |       [+] Country : USA  (5 items)                            |
  |     </td>                                                     |
  |   </tr>                                                       |
  |                                                               |
  |  -- Group Level 2 : City = "New York" --                      |
  |   <tr class="e-groupcaption">                                 |
  |     <td class="e-indentcell"></td>                            |
  |     <td class="e-indentcell"></td>                            |
  |     <td class="e-groupcaption" colspan="3">                   |
  |       ^--- colspan FIXED by removeColGroup()                  |
  |       [+] City : New York  (3 items)                          |
  |     </td>                                                     |
  |   </tr>                                                       |
  |                                                               |
  |  -- Data rows under City: New York --                         |
  |   <tr class="e-row">                                         |
  |     <td class="e-indentcell"></td>                            |
  |     <td> USA        </td>                                     |
  |     <td> New York   </td>                                     |
  |     <td> 2026-01-15 </td>  <- sorted desc                     |
  |     <td> 1000       </td>  <- filtered: Amount = 1000         |
  |   </tr>                                                       |
  |   ... (more data rows)                                        |
  |                                                               |
  |  -- Group footer (aggregate per city) --                      |
  |   <tr class="e-groupfooter">                                  |
  |     <td class="e-indentcell"></td>                            |
  |     <td></td>                                                 |
  |     <td> City Total </td>                                     |
  |     <td></td>                                                 |
  |     <td> 5000 </td>  <- Sum(Amount) for this city             |
  |   </tr>                                                       |
  |                                                               |
  |  ... repeat for other cities / countries ...                  |
  |                                                               |
  | </tbody>                                                      |
  +---------------------------------------------------------------+
  | <tfoot>                                                       |
  |   <tr class="e-summary-row">                                  |
  |     <td colspan="4"> Grand Total </td>                        |
  |     <td> 25000 </td>  <- Sum(Amount) across all rows          |
  |   </tr>                                                       |
  | </tfoot>                                                      |
  +---------------------------------------------------------------+
```

---

### Hierarchy Print Modes: DetailRow Rendering

```
SCENARIO 2: Hierarchy Grid + Detail Rows + HierarchyPrintMode
═══════════════════════════════════════════════════════════════

HierarchyPrintMode = "Expanded"
────────────────────────────────
  Print ONLY expanded detail rows from UI state
  
  PrintLayout Behavior:
    • Reads Parent.HierarchyPrintMode = "Expanded"
    • Pass to cloned grid: HierarchyPrintMode="Expanded"
    • GridDetailRow.razor renders detail rows with IsExpand check
    • Only visible detail rows render (Visible=IsExpand from main grid)
  
  Print DOM:
    <tr class="e-detailrow"> ← Only if IsExpand was true in main grid
      <td colspan="10"> [DetailTemplate content] </td>
    </tr>


HierarchyPrintMode = "All"
──────────────────────────
  Print ALL detail rows (expand all, regardless of UI state)
  
  PrintLayout Behavior:
    • Reads Parent.HierarchyPrintMode = "All"
    • Pass to cloned grid: HierarchyPrintMode="All"
    • GridDetailRow logic: Force IsExpand=true for all rows
    • All detail rows render (Visible=true for all)
  
  Print DOM:
    <tr class="e-detailrow"> ← ALWAYS rendered for EVERY master row
      <td colspan="10"> [DetailTemplate content] </td>
    </tr>


HierarchyPrintMode = "None"
──────────────────────────
  Print ONLY master rows (NO detail rows)
  
  PrintLayout Behavior:
    • Reads Parent.HierarchyPrintMode = "None"
    • Pass to cloned grid: HierarchyPrintMode="None"
    • GridDetailRow.razor: Skip rendering (if check fails or removed)
  
  Print DOM:
    <!-- NO <tr class="e-detailrow"> rows -->
    <!-- Master rows only -->


PRINT FLOW FOR HIERARCHY:
═════════════════════════

1. User calls PrintAsync() (or Ctrl+P)
2. IsPrinting = true
3. PrintLayout injects SfGrid clone with:
   - Parent.HierarchyGridPrintMode forwarded
   - Parent.GridTemplates.DetailTemplate forwarded
4. Print grid renders:
   - Header rows (normal)
   - Master data rows
   - Detail rows (based on HierarchyPrintMode + Visible state)
   - Aggregate rows (if applicable)
5. removeColGroup() runs (fixes colspan)
6. Print dialog opens with full master + detail structure
```

---

### Virtual Scroll Interaction: Why Virtual Scroll is DISABLED in Print

```
NORMAL RENDERING (Virtual Scroll Enabled)
═════════════════════════════════════════

<GridVirtualContent>
  ├─ RenderRows(RowStartIndex, RowEndIndex)
  │  └─ Renders ONLY 20-50 rows in viewport
  │
  ├─ Additional rows loaded on scroll
  └─ Scroll offset tracked: VirtualScrollModule.NextRowToNavigate

  Result: Smooth scrolling, low memory, fast initial render


PRINT CLONE RENDERING (Virtual Scroll DISABLED)
═════════════════════════════════════════════════

PrintLayout.razor:
  • Never passes EnableVirtualization property to cloned grid
  • EnableVirtualization defaults to false
  • GridVirtualContent NOT injected
  • GridContent injected instead (renders ALL rows)

<GridContent>
  ├─ RenderRows(RowStartIndex = 0, RowEndIndex = TotalRowCount)
  │  └─ Renders ALL rows in dataset
  │
  └─ Result: Full page layout, complete DOM tree


IMPLICATION FOR LARGE DATASETS:
════════════════════════════════

  +--------------+----------------------+----------------------+------------+
  | Dataset Size | Virtual Scroll (UI)  | Print Clone DOM      | Print Time |
  +--------------+----------------------+----------------------+------------+
  | 100 rows     | ~20 rows in DOM      | 100 rows in DOM      | ~1 sec     |
  | 10,000 rows  | ~50 rows in DOM      | 10,000 rows in DOM   | ~5 sec     |
  | 100,000 rows | ~50 rows in DOM      | 100,000 rows in DOM  | ~30 sec    |
  +--------------+----------------------+----------------------+------------+

⚠️  KNOWN LIMITATION:
    Print clone may be slow / memory-intensive for very large datasets
    (> 10,000 rows). This is expected behavior, not a bug.
    Recommendation: Document in user guide or provide pagination during print.
```

---

### Colspan Fix: removeColGroup() Algorithm

```
PROBLEM: Grouped Columns Create Indent Columns
═══════════════════════════════════════════════

Initial Group Structure (GroupSettings.Columns = ["Country", "City"]):

<colgroup id="grid_colGroup">
  <col width="30"> ← Indent for Group Level 1
  <col width="30"> ← Indent for Group Level 2
  <col width="100"> ← Country
  <col width="100"> ← City
  <col width="120"> ← OrderDate
  <col width="80"> ← Amount
</colgroup>

Caption Row Colspan Issue:
  <tr class="e-groupcaption">
    <td colspan="6"> ← Spans ALL 6 columns (2 indents + 4 data)
      Country: USA
    </td>
  </tr>


PROBLEM: Print shows WRONG colspan because it includes indent columns
         that should be hidden in print.


SOLUTION: removeColGroup() [sf-grid-fn.ts]
═══════════════════════════════════════════

Algorithm:
  1. depth = GroupSettings.Columns.length = 2 (for Country + City)
  
  2. Find all caption rows:
     groupCaption = element.querySelectorAll('.e-groupcaption') = [3 rows]
  
  3. Get colspan from last caption row:
     colSpan = groupCaption[depth-1].getAttribute('colspan') = "6"
     → This is the ORIGINAL colspan (all columns)
  
  4. Apply colSpan to ALL caption rows:
     for i=0; i<groupCaption.length; i++:
       groupCaption[i].setAttribute('colspan', "6")
     → All captions now span the same width (normalizes them)
  
  5. HIDE indent colgroups:
     colGroups = element.querySelectorAll('colgroup#grid_colGroup')
     for each colgroup:
       for j=0; j<depth; j++:
         colgroup.children[j].style.display = 'none'
     → First 2 col elements hidden (indent columns)
  
  6. Result: Print output shows:
     <tr class="e-groupcaption">
       <td colspan="4"> ← Now correct: excludes 2 hidden indent cols
         Country: USA
       </td>
     </tr>


BEFORE & AFTER VISUAL:
══════════════════════

BEFORE removeColGroup()  [WRONG — indent cols included in colspan]:

  +---------+---------+-----------+---------+---------+
  | [Indent]| [Indent]| Country   | City    | Amount  |
  +---------+---------+-----------+---------+---------+
  | [Indent col 1 + col 2 + Country: USA  colspan=6 ] |
  +---------------------------------------------------+
  | [Indent]| [Indent col 2 + City: New York colspan4]|
  +---------+------------------------------------------+

AFTER removeColGroup()   [CORRECT — indent cols hidden]:

  +-----------+---------+-----------+---------+
  | Country   | City    | OrderDate | Amount  |
  +-----------+---------+-----------+---------+
  | Country: USA                    colspan=4 |
  +------------------+------------------------+
  | [indent] | City: New York       colspan=3 |
  +----------+--------------------------------+

Code Path:
  PrintLayout.razor → InvokePrint() 
    → JS Interop: sfBlazor.Grid.printGrid(dataId)
    → sf-grid.ts: printGrid(dataId)
    → sf-grid-fn.ts: this.print()
    → removeColGroup() executes
    → window.open() + EJ2 print(element, window)
```

---

## Prompt Template
<!-- token-budget: 400 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Print
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Print feature.
Scope: PrintLayout.razor, Print-related properties, keyboard handlers, and JS interop bridges ONLY.

=== WHAT YOU MUST KNOW (pre-loaded) ===

PRINT ARCHITECTURE:
- Print is triggered by: Ctrl+P keyboard shortcut (FocusHandler) OR await grid.PrintAsync() (SfGrid.Methods)
- Print flag: SfGrid.IsPrinting (internal bool)
- Print component: PrintLayout.razor (off-screen cloned grid)
- Print grid ID: Parent.ID + "_sfprint" (distinguishes clone from main)
- Print output: Browser print dialog → new window → native print

CORE PRINT FLOW:
1. User triggers print (Ctrl+P or PrintAsync())
2. IsPrinting = true
3. SfGrid.razor injects: <PrintLayout Parent="this"></PrintLayout>
4. PrintLayout clones SfGrid<T> off-screen with settings forwarded:
   - DataSource, Query, Columns, Sorting, Filtering, Grouping, Aggregates, Templates
   - PageSettings (CurrentPage preserved if PrintMode==CurrentPage)
   - AllowSelection=false, AllowPaging=(PrintMode==CurrentPage), Toolbar=null
   - EnableVirtualization=false (ALWAYS)
5. PrintLayout registers DataBound event → InvokePrint()
6. InvokePrint() calls JS interop: sfBlazor.Grid.printGrid(dataId)
7. sf-grid.ts retrieves cloned grid instance → calls print()
8. sf-grid-fn.ts: removeColGroup() (fixes grouped colspan) → window.open() → EJ2 print(element, window)
9. Browser print dialog opens
10. IsPrinting = false → PrintLayout unmounts

PRINT MODES:
- PrintMode.AllPages: Renders FULL dataset in cloned grid; AllowPaging=false
- PrintMode.CurrentPage: Renders CURRENT PAGE ONLY in cloned grid; AllowPaging=true; CurrentPage value forwarded

HIERARCHY PRINT MODES:
- HierarchyGridPrintMode.Expanded: Prints master + expanded detail rows (respects UI expand state)
- HierarchyGridPrintMode.All: Prints master + ALL detail rows (force-expands all)
- HierarchyGridPrintMode.None: Prints master ONLY (no detail rows)

KEY PROPERTIES (SfGrid.Properties.cs):
- PrintMode: enum { AllPages, CurrentPage } — default AllPages
- HierarchyPrintMode: enum { Expanded, All, None } — default Expanded
- IsPrinting: internal bool flag (set by PrintAsync())

KEY METHODS (SfGrid.Methods.cs):
- PrintAsync(): Public API entry point
  • Sets IsPrinting = true
  • Calls CallStateHasChangedAsync()
  • Returns Task

KEYBOARD INTEGRATION (FocusHandler.cs, lines 820-821):
- "CtrlP" or "MetaP" key combination triggers:
  • if (bf != null) bf.Cancel = true (prevents default browser print)
  • await _parent.PrintAsync()

JS INTEROP BRIDGE:
- Blazor call: _printGrid?.InvokeMethod("sfBlazor.Grid.printGrid", new object[] { _printGrid.DataId })
- sf-grid.ts: printGrid(dataId) retrieves instance, calls gridInstance.element.blazor__instance.print()
- sf-grid-fn.ts: print() method:
  • removeColGroup() fixes grouped colspan depth
  • window.open('', 'print', 'height=' + window.outerHeight + ',width=' + window.outerWidth + ',tabbar=no')
  • print(this.element, printWind) — EJ2 base library function

CRITICAL CONSTRAINTS (MANDATORY):
- NEVER pass EnableVirtualization to PrintLayout cloned grid
- ALWAYS force AllowSelection=false in cloned grid
- ALWAYS force Toolbar=null in cloned grid
- ALWAYS forward GroupSettings, SortSettings, FilterSettings, Aggregates to clone
- ALWAYS forward RowTemplate and DetailTemplate to clone
- ALWAYS fix grouped colspan via removeColGroup() JS function
- ALWAYS set ID suffix "_sfprint" for cloned grid to distinguish from main
- ConfigureAwait(true) on all async calls
- Guard clause: if (Parent == null) return
- No direct StateHasChanged() — use CallStateHasChangedAsync()

CROSS-FEATURE RULES (MANDATORY):
- Grouping + Print: Forward GroupSettings.Columns; removeColGroup() fixes colspan; ShowDropArea=false forced
- Sorting + Print: Forward SortSettings.Columns; sorted rows printed in sort order
- Filtering + Print: Forward FilterSettings.Columns; filtered rows printed
- Paging + Print: If PrintMode==CurrentPage, forward CurrentPage value; AllowPaging=true forced; if PrintMode==AllPages, AllowPaging=false
- Aggregates + Print: Forward Aggregates; footer rows rendered in print
- Templates + Print: Forward RowTemplate and DetailTemplate; template-based rows rendered
- Virtualization + Print: NEVER forward EnableVirtualization; print always renders flat
- Hierarchy + Print: Forward HierarchyPrintMode; detail rows rendered per mode (Expanded/All/None)
- Editing + Print: Force AllowEditing=false in clone; no edit UI in print
- Selection + Print: Force AllowSelection=false in clone; no selection UI in print

=== BEFORE YOU MAKE ANY CHANGE ===
1. Consult /docs/training/ — understand print rendering flow, DOM cloning, feature forwarding strategy.
2. Verify that PrintLayout forwards ALL relevant settings (sort, filter, group, aggregate, template).
3. Confirm removeColGroup() is called in sf-grid-fn.ts before print dialog opens.
4. Confirm IsPrinting flag is SET before PrintLayout injection and CLEARED after print completes.
5. Confirm keyboard shortcut (Ctrl+P) routes through FocusHandler → PrintAsync().
6. Test print with sorting, filtering, grouping, paging, virtualization, templates, hierarchy, aggregates ENABLED.

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
