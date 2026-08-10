---
name: adaptive-layout-skill
description: Expert knowledge for the Adaptive Layout feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to adaptive UI behaviour, including vertical row rendering, responsive dialogs, device mode detection, and cross-feature interaction guarantees.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Adaptive Layout in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature during vertical rendering, dialog transitions, and device mode switches.

---

## Agent Invocation
<!-- token-budget: 40 words -->
- Supported modes: `feature-implementation` | `bug-fix`
- Load this skill ONLY for work scoped to Adaptive Layout (EnableAdaptiveUI, RowRenderingMode, adaptive dialogs, device detection)
- One agent invocation = one feature skill maximum

---

## Knowledge References
<!-- token-budget: 60 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/06-reference/quick-reference-guides.md`
- `docs/architecture/system-architecture.md`
- `docs/architecture/component-architecture.md`
- `docs/tech-stack/tech-stack.md`
- `docs/code-guidelines/coding-standards.md`
- `docs/code-guidelines/naming-conventions.md`
- `docs/code-guidelines/error-handling.md`

---

## Training Insights Applied
<!-- token-budget: 100 words -->

Key rules and edge cases from `/docs/training/` that directly govern Adaptive Layout work:

- **Device detection is Syncfusion-level, not grid-level**: `SyncfusionService.IsDeviceMode` is set once at app startup. Adaptive UI responds to this global flag + grid-level `EnableAdaptiveUI` + `AdaptiveUIMode` (Both/Mobile/Desktop).
- **Vertical rendering triggers CSS class cascade**: When `EnableAdaptiveUI && RowRenderingMode == RowDirection.Vertical`, the grid adds `e-bigger e-row-responsive` to root. This class drives all responsive CSS behavior downstream. Never add these classes conditionally in row/cell renderers.
- **Data attributes carry header text inline**: In vertical mode, `data-cell="HeaderText"` attribute is injected into each `<td>`. This allows CSS `::before { content: attr(data-cell); }` to render column headers inline.
- **Adaptive dialogs suppress FilterBar**: When `EnableAdaptiveUI && AllowFiltering && !FilterSettings.Type.Equals(FilterType.FilterBar)`, the `AdaptiveDialogRenderer` opens a responsive dialog on filter icon click. FilterBar is not rendered at all in this config.
- **State flags isolate adaptive from normal UI**: `IsAdaptiveFilter`, `IsAdaptiveSort`, `IsAdaptiveToolbarMenu` flags in `Filter<T>` prevent normal and adaptive paths from colliding during the same render cycle.
- **Toolbar menu routing changes**: In adaptive mode, toolbar icons (Export, ColumnChooser, etc.) open a context menu dialog instead of inline toolbar. Toolbar must conditionally render.
- **Text wrap combines with vertical**: When both `AllowTextWrap && RowRenderingMode == RowDirection.Vertical`, add `e-verticalwrap` class to rows. This prevents cell overflow in vertical mode.

---

## Code Location Map
<!-- token-budget: 120 words -->

All Adaptive Layout code lives at these paths (no code reproduced — paths + one-line purpose only):

| Path | Purpose |
|------|---------|
| `src/SfGrid.Properties.cs` | `EnableAdaptiveUI` (bool), `AdaptiveUIMode` (enum), `RowRenderingMode` (enum) property declarations |
| `src/SfGrid.razor.cs` (GetClass method) | Adds `e-bigger e-row-responsive` CSS class when adaptive + vertical mode + device OR desktop mode condition met |
| `src/Enumeration/GridsEnumerations.cs` | `AdaptiveMode` enum: `Both`, `Mobile`, `Desktop`; `RowDirection` enum: `Horizontal`, `Vertical` |
| `src/Internal/SfGrid.razor` | Root DOM — conditionally injects `AdaptiveDialogRenderer`, `FilterType`, `ColumnChooser` when `EnableAdaptiveUI` is true |
| `src/Internal/Renderer/AdaptiveDialogRenderer.razor` | Full responsive dialog UI: filter dialog, sort dialog, toolbar context menu with all CSS classes (`e-responsive-dialog`, `e-dlg-fullscreen`, `e-res-custom-element`, etc.) |
| `src/Internal/Actions/Filter.cs` | `IsAdaptiveFilter`, `IsAdaptiveSort`, `IsAdaptiveToolbarMenu` state flags (lines 43–45) |
| `src/Internal/Renderer/GridRow.razor.cs` | Adds `e-verticalwrap` class (line 193 in `SetClass()` method) when `EnableAdaptiveUI && RowRenderingMode == RowDirection.Vertical && AllowTextWrap` |
| `src/Internal/Renderer/GridRow.CellRenderer.cs` | **CRITICAL**: Injects `data-cell="HeaderText"` attribute on `<td>` when `EnableAdaptiveUI && RowRenderingMode == RowDirection.Vertical` |
| `src/Internal/Renderer/CommandColumnRenderer.cs` | Adds `data-cell` attribute for command columns in adaptive vertical mode (consistency with regular cells) |
| `src/Internal/Renderer/GridHeader.razor` | Normal horizontal header table — NOT rendered when adaptive vertical mode is active |
| `src/Internal/Renderer/GridContent.razor` | Row iteration logic — all feature combinations flow through here; adapts to virtual scroll, grouping, paging |
| `src/Internal/Renderer/GridToolbar.razor` | Toolbar renders conditionally; toolbar menu icon triggers `IsAdaptiveToolbarMenu` state flag |
| `src/Internal/Renderer/Filter/FilterBarRenderer.razor` | FilterBar UI — NOT rendered when `FilterSettings.Type != FilterBar` and adaptive mode active |
| `scripts/sf-grid.ts` (lines 54, 269) | **TypeScript source**: `customFilterDialog()` sets dialog styles (line 54); `renderAdaptiveMenuItems()` calculates menu position with viewport overflow prevention (line 269) |

---

## DOM Rendering Structure with ASCII Diagrams
<!-- token-budget: 200 words -->

### 1. Root DOM Tree Structure

```
Grid Root <div id="grid" class="sf-grid e-grid e-responsive [e-bigger e-row-responsive]">
│
├─ Toolbar (conditional: Toolbar != null OR EnableAdaptiveUI + sort/filter)
│  └─ Toolbar items (Add, Edit, Delete, Search, etc.)
│
├─ Header Section <div class="e-gridheader">
│  └─ <table> (NOT RENDERED if AdaptiveMode=Vertical AND EnableAdaptiveUI)
│     ├─ GridHeader.razor (normal horizontal headers)
│     │  ├─ <tr> for each column header level (stacked headers)
│     │  └─ FilterBarRenderer (if FilterSettings.Type == FilterBar)
│     └─ GridVirtualHeader.razor (if EnableColumnVirtualization)
│
├─ Content Section <div class="e-gridcontent">
│  └─ <table class="e-table">
│     ├─ WidthController (handles column width calculations)
│     └─ <tbody>
│        ├─ GridContent.razor OR GridVirtualContent.razor
│        │  └─ For each row:
│        │     ├─ GridRow.razor <tr class="e-row [e-verticalwrap]">
│        │     │  ├─ For each cell (normal horizontal):
│        │     │  │  └─ <td class="e-rowcell">
│        │     │  │     └─ Cell content (NO data-cell attribute)
│        │     │  │
│        │     │  └─ For each cell (ADAPTIVE vertical):
│        │     │     └─ <td class="e-rowcell" data-cell="HeaderText">
│        │     │        ├─ ::before { content: attr(data-cell); } [CSS]
│        │     │        └─ Cell content rendered AFTER header text
│        │     │
│        │     ├─ GridDetailRow.razor (if detail row template)
│        │     └─ GridAddNewRow.razor (if ShowAddNewRow)
│        │
│        └─ Empty row placeholder (if no data)
│
├─ Pager (if AllowPaging)
│
├─ Column Chooser (if ShowColumnChooser OR (ShowColumnMenu && EnableAdaptiveUI))
│
├─ Context Menu / Column Menu
│
├─ AdaptiveDialogRenderer (if EnableAdaptiveUI + (AllowSorting OR AllowFiltering))
│  ├─ Filter Dialog <SfDialog> (Opened when: IsAdaptiveFilter == true)
│  │  ├─ Header: "Filter" + Clear button
│  │  ├─ Content: Column list (<div class="columndiv">)
│  │  │  └─ For each filterable column:
│  │  │     └─ <div class="e-responsivecoldiv"> (interactive row)
│  │  │        ├─ Column header text <span class="e-res-header-text">
│  │  │        └─ Filter icon <div class="e-icons e-res-icon e-filterset">
│  │  └─ Footer: n/a
│  │
│  ├─ Sort Dialog <SfDialog> (Opened when: IsAdaptiveSort == true)
│  │  ├─ Header: "Sort" + Apply + Clear buttons
│  │  ├─ Content: Column list with sort direction toggles
│  │  │  └─ For each sortable column:
│  │  │     └─ <div class="e-responsivecoldiv">
│  │  │        └─ <SfButton> (cycles: None → Ascending → Descending → None)
│  │  └─ Footer: "OK" (apply), "Cancel"
│  │
│  └─ Toolbar Menu <SfContextMenu> (Opened when: IsAdaptiveToolbarMenu == true)
│     └─ Menu items: PdfExport, ExcelExport, CsvExport, Print, ColumnChooser
│
├─ FilterType (container for adaptive filter dialog markup — if AllowFiltering)
│
└─ GridTooltip, ValidationDialog, etc. (feature-specific)
```

---

### 2. Rendering Decision Flow (Adaptive vs. Normal)

```
┌─ SfGrid.GetClass() Evaluation ─────────────────────────────────┐
│                                                                 │
│  EnableAdaptiveUI?                                              │
│  ├─ NO  → Return "sf-grid e-grid e-control e-responsive ..."   │
│  │       (No adaptive CSS, normal horizontal layout)            │
│  │                                                              │
│  └─ YES →                                                       │
│     ├─ RowRenderingMode == RowDirection.Vertical?              │
│     │  ├─ NO  → Return "sf-grid e-grid ..." (no e-row-responsive)
│     │  │                                                        │
│     │  └─ YES →                                                │
│     │     ├─ AdaptiveUIMode == AdaptiveMode.Both?              │
│     │     │  ├─ YES → ALWAYS add e-row-responsive              │
│     │     │  │        (Render adaptive on all devices)         │
│     │     │  │                                                 │
│     │     │  └─ NO →                                           │
│     │     │     ├─ AdaptiveUIMode == AdaptiveMode.Mobile?      │
│     │     │     │  ├─ YES → Is SyncfusionService.IsDeviceMode? │
│     │     │     │  │        ├─ YES → Add e-row-responsive      │
│     │     │     │  │        └─ NO  → Don't add (desktop)       │
│     │     │     │  │                                           │
│     │     │     │  └─ NO (AdaptiveMode.Desktop) →              │
│     │     │     │     ├─ Is SyncfusionService.IsDeviceMode?    │
│     │     │     │     │  ├─ YES → Don't add (mobile)           │
│     │     │     │     │  └─ NO  → Add e-row-responsive         │
│     │     │     │     │           (Render adaptive on desktop) │
│
│  Result: Class string contains "e-bigger e-row-responsive"     │
│          CSS cascade applies all responsive styles             │
└─────────────────────────────────────────────────────────────────┘
```

---

### 3. Cell Rendering: Horizontal vs. Vertical

```
NORMAL HORIZONTAL RENDERING (EnableAdaptiveUI=false OR RowDirection.Horizontal)
─────────────────────────────────────────────────────────────────────────────

GridHeader.razor renders:
  <tr>
    <th>Order ID</th>
    <th>Customer</th>
    <th>Amount</th>
  </tr>

GridRow.CellRenderer.cs renders:
  <tr class="e-row">
    <td class="e-rowcell">10001</td>
    <td class="e-rowcell">John</td>
    <td class="e-rowcell">$500</td>
  </tr>

Displayed as standard table grid.


ADAPTIVE VERTICAL RENDERING (EnableAdaptiveUI=true AND RowDirection.Vertical)
─────────────────────────────────────────────────────────────────────────────

GridHeader.razor: NOT RENDERED (DOM is empty, CSS hides it)

GridRow.CellRenderer.cs injects data-cell attribute:
  <tr class="e-row e-verticalwrap">      ← e-verticalwrap if AllowTextWrap
    <td class="e-rowcell" data-cell="Order ID">
      ::before { content: "Order ID"; }  ← CSS renders header inline
      10001
    </td>
    <td class="e-rowcell" data-cell="Customer">
      ::before { content: "Customer"; }
      John
    </td>
    <td class="e-rowcell" data-cell="Amount">
      ::before { content: "Amount"; }
      $500
    </td>
  </tr>

Displayed as vertical card-like layout:
  ┌─ Order ID: 10001
  ├─ Customer: John
  └─ Amount: $500
```

---

### 4. Adaptive Dialog State Machine

```
┌────────────────────────────────────────────────────────────┐
│ AdaptiveDialogRenderer State Machine                       │
└────────────────────────────────────────────────────────────┘

Initial State: IsFilterIconClicked=false, IsSortIconClicked=false, IsToolbarMenuIconClicked=false

    ┌─────────────────────────────────────────┐
    │ Filter Icon Clicked (Toolbar)           │
    │ → IsAdaptiveFilter = true               │
    │ → IsFilterIconClicked = true            │
    └─────────────────────────────────────────┘
              ↓
         Opens SfDialog(ID="ResDialog")
         ├─ Header: "Filter" + Clear button
         ├─ Content: Column list (<div class="columndiv">)
         │  └─ For each column: <div class="e-responsivecoldiv">
         │     ├─ Column name
         │     └─ Filter icon (if already filtered)
         └─ Listens to: CloseAdaptiveDialog event
              ↓
         User clicks column → FilterItemClicked()
         → Triggers adaptive filter UI (submenu)
              ↓
         User clicks "OK" or Close
         → IsFilterIconClicked = false
         → Parent.EventAggregator.Trigger("FilterComponentUpdate")
         → Dialog closes


    ┌─────────────────────────────────────────┐
    │ Sort Icon Clicked (Toolbar)             │
    │ → IsAdaptiveSort = true                 │
    │ → IsSortIconClicked = true              │
    └─────────────────────────────────────────┘
              ↓
         Opens SfDialog(ID="ResDialog")
         ├─ Header: "Sort" + Apply + Clear buttons
         ├─ Content: Column list
         │  └─ For each column: <SfButton> (None → Asc → Desc → None)
         └─ Maintains Directions[col.Uid] state
              ↓
         User clicks sort button → SortClick()
         → Cycles direction in state
              ↓
         User clicks "OK" → Submit()
         → Applies sort changes via Parent.SortModule
         → Fires OnActionBegin/OnActionComplete
         → Dialog closes


    ┌─────────────────────────────────────────┐
    │ Toolbar Menu Icon (≡) Clicked           │
    │ → IsAdaptiveToolbarMenu = true          │
    └─────────────────────────────────────────┘
              ↓
         Opens SfContextMenu
         ├─ Menu items: PdfExport, ExcelExport, CsvExport, Print, ColumnChooser
         └─ Routes ItemSelected → ItemSelected()
              ↓
         User selects item
         → Calls Parent.ExportToPdfAsync(), ColumnChooser toggle, etc.
         → Menu closes
         → IsAdaptiveToolbarMenu = false
```

---

### 5. JavaScript Interop in Adaptive Dialogs
<!-- token-budget: 100 words -->

**JavaScript interop is required** for adaptive dialog DOM manipulation and positioning. Never modify dialog DOM in Blazor after JS interop executes.

**C# → JavaScript Interop Calls:**

| Location | Method Call | Lifecycle Hook | Purpose |
|----------|-------------|----------------|---------|
| AdaptiveDialogRenderer.razor (line 191) | `Parent.InvokeMethod("sfBlazor.Grid.customFilterDialog", new object[] { Parent.DataId, $"{Parent?.ID}customfilter" })` | `Rendered()` event after dialog creation | Sets dialog element styles (max-height, border, top position) |
| AdaptiveDialogRenderer.razor (line 364) | `Parent.InvokeMethod("sfBlazor.Grid.renderAdaptiveMenuItems", new object[] { Parent.DataId })` | `ColumnMenuOpenedHandler()` when toolbar menu opens | Calculates and sets context menu position with viewport overflow prevention |

**TypeScript Functions (scripts/sf-grid.ts):**

```typescript
// Line 54 - Dialog Styling
customFilterDialog(dataId: string, dlgID: string, isExcel: boolean) {
    const gridInstance = this.sfBlazor.getCompInstance(dataId);
    let dialogElement: HTMLElement = document.querySelector('#' + dlgID);
    dialogElement.style.maxHeight = '100%';
    dialogElement.style.border = '1px';
    dialogElement.style.top = '0px';
    // Handles Excel filter menu caret padding
    // Supports TreeGrid integration
}

// Line 269 - Menu Positioning
renderAdaptiveMenuItems: function (dataId: string) {
    const columnMenuElement = document.getElementsByClassName(`e-${gridInstance.element.id}-column-menu`)[0];
    let left: number = btnOffset.left + scrollX;
    let top: number = btnOffset.bottom + scrollY;
    // Calculates viewport overflow and adjusts position
    // Handles scrollbar width compensation
    columnMenuElement.style.left = Math.ceil(left + 1) + 'px';
}
```

**Responsibility Split:**

| Component | Handles |
|-----------|---------|
| **JavaScript** | Dialog element styling, menu position calculation, viewport boundary detection, scroll offset handling, DOM measurements |
| **SfDialog Component** | Focus management, keyboard trap (ESC/Tab), ARIA attributes, modal overlay |
| **Blazor (AdaptiveDialogRenderer)** | Dialog open/close state, event routing (OnOpen/OnClose/Created), data binding, column list rendering |

**CRITICAL Rules:**
- DO NOT modify dialog `maxHeight`, `border`, or `top` CSS in Blazor after `Rendered()` fires
- DO NOT calculate menu position in Blazor — JS handles viewport math
- DO await JS interop calls — dialog may render incorrectly if skipped
- DO preserve TreeGrid integration checks in JavaScript functions

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 200 words -->

> Built from live feature cross-reference + adaptive-specific interaction testing.  
> Omitted pairs have no interaction risk.

| Combination | Rendering Path | Must Preserve | Risk |
|-------------|---|---|---|
| Adaptive + Sorting | IsAdaptiveSort flag opens responsive dialog; sort applied via Parent.SortModule after submit | Sort direction state (Directions dict) persists across dialog open/close; OnActionBegin/Complete pipeline fires; header indicators update | Critical |
| Adaptive + Filtering | IsAdaptiveFilter flag opens responsive dialog; filter applied via Parent.FilterModule after selection | Filter column state persists; OnActionBegin/Complete pipeline fires; no FilterBar rendered | Critical |
| Adaptive + Grouping | GroupSettings columns are still rendered; grouping happens in data layer, not adaptive layer | Grouped column indent cells still render with data-cell attributes; GroupedHeader styles apply; grouping stacks with vertical rendering | High |
| Adaptive + Editing (Normal/Inline) | Edit mode renders inline in-row; row remains vertical layout during edit | Editing cell styles applied after data-cell attribute; tab navigation works within vertical row; close edit → re-render cell with data-cell | High |
| Adaptive + Editing (Dialog) | DialogEdit renders separate dialog; grid row stays read-only in background | Dialog mode is NOT affected by adaptive layout (separate DOM tree); return from dialog closes it; grid reverts to normal row rendering | Medium |
| Adaptive + Editing (Batch) | Batch cells render with e-editedbatchcell class; data-cell attribute still injected | Edit cells styled after data-cell; validation highlights remain visible; save/cancel batch applies to vertical layout | High |
| Adaptive + Paging | Pager rendered below grid; data refresh works normally; page size determines rows per page | Paging controls adapt to responsive CSS; page change triggers DataBound, which updates row data; vertical layout applies to new page data | Medium |
| Adaptive + Virtual Scroll | Virtual scroll row range logic unaffected by adaptive UI; only rendering is affected | Row start/end indices same; virtual mask rows render with data-cell attributes; scroll performance unchanged | Medium |
| Adaptive + Infinite Scroll | Infinite scroll loads next batch; new rows render with data-cell attributes | Batch indicators and loading spinner adapt to responsive CSS; scroll position maintained during vertical rendering | Medium |
| Adaptive + Selection | Selection works row or cell level; selected cells/rows get e-selectionbackground + e-active classes | data-cell attribute does NOT interfere with selection state; aria-selected remains; selection checkboxes visible | High |
| Adaptive + Detail Rows | Detail row template renders below each master row; detail rows also get data-cell attributes if they have data columns | Master row toggles expand icon; detail row layout adapts to vertical if within adaptive grid | Medium |
| Adaptive + Frozen Columns | Frozen left/right columns are still frozen; adaptive affects rendering, not layout | Frozen columns keep their position; data-cell attributes applied to frozen cells; e-freezeleftborder, e-freezerightborder classes preserved | High |
| Adaptive + Text Wrap | e-verticalwrap class added to rows when both AllowTextWrap + RowDirection.Vertical | Row height grows for wrapped text; data-cell header still renders inline; no overflow | High |
| Adaptive + Toolbar | Toolbar conditionally renders or toolbar menu icon opens IsAdaptiveToolbarMenu dialog | Toolbar items (Add, Edit, Delete, etc.) hidden in fully adaptive; only menu icon visible; menu dialog lists all toolbar actions | Critical |
| Adaptive + Column Chooser | ColumnChooser opens as dialog OR checkbox column list in adaptive context | Column visibility toggles work; grid re-renders after column change; data-cell attributes update if column is hidden | High |
| Adaptive + Export (PDF/Excel) | Export triggered via toolbar menu in adaptive; exports current sorted/filtered/grouped data | Export includes vertical layout info or exports flat table format (check export implementation); sort/filter state respected | Medium |
| Adaptive + Aggregates | Aggregates computed over data; reactive aggregate listens to DataBound | Aggregate row renders with data-cell attributes; aggregate values correct; row class e-aggregatecell preserved | Medium |
| Adaptive + Search | SearchSettings applies to data filter; search UI in toolbar or toolbar menu | Search input in toolbar (or menu); results update; vertical layout applies to filtered data; data-cell attributes refresh | Medium |
| Adaptive + Persistence | EnablePersistence persists column visibility, sort, filter, grouping state | AdaptiveUIMode + RowRenderingMode NOT persisted (set in code); other settings persist and re-apply on load | Medium |
| Adaptive + RTL | EnableRtl applies RTL CSS; grid root gets e-rtl class | data-cell attributes work in RTL; column order reversed; responsive dialog text direction respected | Low |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Adaptive Layout
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Adaptive Layout feature.
Scope: Adaptive UI rendering, vertical row layout, responsive dialogs, device detection logic.

=== WHAT YOU MUST KNOW (pre-loaded) ===
- Adaptive activation: EnableAdaptiveUI (bool) + RowRenderingMode (Horizontal|Vertical) + AdaptiveUIMode (Both|Mobile|Desktop)
- CSS class injection: SfGrid.GetClass() adds "e-bigger e-row-responsive" when condition is met
- Device detection: SyncfusionService.IsDeviceMode (read-only, set at app startup)
- Critical attribute: data-cell="HeaderText" injected in GridRow.CellRenderer.cs line ~XXX for vertical cells
- CSS class wrap: e-verticalwrap class added in GridRow.razor.cs line 193 (SetClass method) when AllowTextWrap enabled
- Dialog flags: IsAdaptiveFilter, IsAdaptiveSort, IsAdaptiveToolbarMenu in Filter<T> class (Filter.cs lines 43-45)
- Dialog component: AdaptiveDialogRenderer.razor — contains SfDialog + SfContextMenu rendering
- Header hiding: GridHeader.razor NOT rendered when adaptive vertical mode is active (CSS hides it)
- Toolbar integration: GridToolbar.razor conditionally renders toolbar items based on AdaptiveUIMode
- Event pipeline: Adaptive filter/sort must fire OnActionBegin/OnActionComplete via Parent.FilterModule/SortModule
- CSS cascade: All responsive styling driven by e-row-responsive + e-bigger classes on grid root

=== RENDERING PATHS (memorize these) ===
Normal (Horizontal):
  Grid root (no e-row-responsive) → GridHeader (visible) → GridRow with <td> (NO data-cell)

Adaptive (Vertical):
  Grid root (e-row-responsive + e-bigger) → GridHeader (hidden by CSS) → GridRow with <td data-cell="...">

Adaptive Dialogs:
  Toolbar icon click → EventAggregator trigger → AdaptiveDialogRenderer opens SfDialog/SfContextMenu
  User action in dialog → IsAdaptiveFilter/Sort/ToolbarMenu flag → Submit() → Module applies change → Dialog closes

=== BEFORE YOU MAKE ANY CHANGE ===
1. Consult /docs/training/00-START-HERE.md for adaptive behavior expectations.
2. Consult /docs/training/06-reference/quick-reference-guides.md for known edge cases.
3. Confirm EnableAdaptiveUI does NOT override other rendering modes (virtualization, grouping, paging all work alongside adaptive).
4. Confirm data-cell attribute is injected consistently on ALL cells in vertical mode, including frozen, detail, and command columns.
5. Confirm adaptive dialogs route through EventAggregator + Parent modules, never directly call module methods.

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope (adaptive UI only)
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all modified public members
- All await calls use .ConfigureAwait(true)
- data-cell attribute must be injected in GridRow.CellRenderer.cs ONLY (not elsewhere)
- Adaptive CSS classes added in SfGrid.GetClass() ONLY (not in individual renderers)
- Dialog state flags managed in Filter<T> ONLY (IsAdaptiveFilter, IsAdaptiveSort, IsAdaptiveToolbarMenu)
- EventAggregator used for all cross-module communication (no direct module-to-module calls)
- Follow naming-conventions.md (PascalCase methods, _camelCase private fields)
- Follow error-handling.md (guard clauses first, no silent catch blocks)

=== SCOPE ===
{Describe the exact rendering path or dialog behavior to implement — one aspect per task}

=== INPUT ===
{Paste the extracted code chunk — see training/04-code-processing/optimal-chunking-strategies.md}

=== OUTPUT ===
1. Root cause / design rationale (3–5 sentences)
2. Modified method(s) with rendering flow explanation
3. Interaction risk flags (reference Interaction Matrix above)
4. Required test cases (Given-When-Then format):
   - Test vertical rendering with data-cell attributes
   - Test adaptive dialog open/close state
   - Test device mode switching
   - Test feature combinations (e.g., adaptive + grouping + sorting)

After implementation, run regression verification using:
/docs/ai-agents/prompts/regression-verification-prompt.md
```

---

## Feature Combination Rendering Diagram
<!-- token-budget: 150 words -->

```
┌──────────────────────────────────────────────────────────────────┐
│ ADAPTIVE LAYOUT FEATURE COMBINATION MATRIX                       │
│ (Shows rendering path for each combination)                      │
└──────────────────────────────────────────────────────────────────┘

                                   ┌─ Adaptive OFF ─────────────────┐
EnableAdaptiveUI                   │ → Normal horizontal grid         │
     ├─ true                       │ → GridHeader + GridRows         │
     │  └─ RowRenderingMode        │ → No data-cell attrs            │
     │     ├─ Vertical             └─────────────────────────────────┘
     │     │  └─ AdaptiveUIMode
     │     │     ├─ Both ──────┐                ┌─ Adaptive ON (Always) ──────┐
     │     │     │             │────────────────┤ → e-row-responsive class     │
     │     │     ├─ Mobile     │                │ → GridHeader hidden by CSS   │
     │     │     │  └─ Device? │                │ → GridRow cells have data-cell
     │     │     │     ├─ Yes ─┤                │ → Vertical card layout       │
     │     │     │     └─ No ──┤    (No Adapt)  └─────────────────────────────┘
     │     │     │             │                ┌─ Adaptive ON (Mobile only) ─┐
     │     │     └─ Desktop    │────────────────┤ → Check IsDeviceMode flag    │
     │     │        └─ Device? │                │ → Render adaptive if mobile  │
     │     │            ├─ Yes │    (No Adapt)  │ → Normal layout if desktop   │
     │     │            └─ No ─┤                └─────────────────────────────┘
     │     │                   │                ┌─ Adaptive ON (Desktop only)─┐
     │     │                   └────────────────┤ → Render adaptive if desktop │
     │     │                   (Adapt)          │ → Normal layout if mobile    │
     │     │                                    └─────────────────────────────┘
     │     └─ Horizontal ────────────────────────────────────────────┐
     │                                                                │
     │                                    No Adaptive, normal grid ───┘
     │
     └─ false ──────────────────────────────────────────────────────┐
                                           No Adaptive, normal grid ─┘


FEATURE + ADAPTIVE COMBINATION RENDERING:

AllowGrouping + Adaptive:
  ├─ Data layer: GroupSettings still applied, group captions rendered
  └─ DOM: Grouped rows use vertical layout, data-cell attrs on group cells

AllowSorting + Adaptive:
  ├─ UI: Sort icon in toolbar opens IsAdaptiveSort dialog
  └─ DOM: Sort applied, vertical grid refreshes with data-cell attrs

AllowFiltering + Adaptive:
  ├─ UI: Filter icon in toolbar opens IsAdaptiveFilter dialog (no FilterBar)
  └─ DOM: Filter applied, vertical grid refreshes

AllowPaging + Adaptive:
  ├─ UI: Pager below grid, renders normally
  └─ DOM: Each page of data renders in vertical layout

AllowEditing + Adaptive:
  ├─ Normal/Batch: Edit cells rendered inline, data-cell attr still injected
  └─ Dialog: DialogEdit renders separate, grid row stays read-only background

AllowSelection + Adaptive:
  ├─ UI: Row or cell selection checkboxes visible
  └─ DOM: Selected rows/cells get e-selectionbackground class, data-cell unchanged

FrozenColumns + Adaptive:
  ├─ DOM: Frozen left/right columns stay frozen
  └─ Rendering: Frozen cells also get data-cell attributes for vertical layout

EnableVirtualization + Adaptive:
  ├─ Data: Virtual scroll logic unchanged
  └─ Rendering: Virtual rows render with data-cell attributes

DetailTemplate + Adaptive:
  ├─ Master row: Renders with data-cell (vertical)
  └─ Detail row: Expands below, also adapts to vertical if it has data columns
```

---

## Edge Cases & Known Constraints
<!-- token-budget: 80 words -->

1. **No partial vertical rendering**: If EnableAdaptiveUI && RowRenderingMode == Vertical, ALL rows render vertically. Cannot mix horizontal and vertical in one grid.

2. **GridHeader.razor is NOT rendered**: When adaptive vertical mode is active, the header table is completely absent from the DOM (CSS hides it with `display:none` or similar). This is NOT a bug — it's by design.

3. **data-cell attribute positioning**: The attribute is injected in `GridRow.CellRenderer.cs`, not in individual cell render fragments. Changes to cell rendering must preserve this injection point.

4. **AdaptiveUIMode does NOT persist**: Settings like `EnableAdaptiveUI` and `RowRenderingMode` are NOT persisted to localStorage. Persistence applies to sort/filter/grouping only. Adaptive UI mode is set in code and stays static per page reload.

5. **Dialog state flags are transient**: `IsAdaptiveFilter`, `IsAdaptiveSort`, `IsAdaptiveToolbarMenu` are temporary flags that exist during dialog open only. They reset to false when dialog closes. Do NOT rely on these for permanent state.

6. **Text wrap interaction**: When `AllowTextWrap && RowRenderingMode == Vertical`, the row gets `e-verticalwrap` class. Cell height expands automatically. The data-cell header ::before still renders inline regardless of wrap.

7. **Virtual scroll performance**: Virtual scrolling works with adaptive layout, but row height is harder to calculate due to dynamic wrap. Ensure `RowHeight` is set explicitly if virtualization is enabled with text wrap.

---

*This skill file is the canonical reference for Adaptive Layout implementation. For regressions, consult the Interaction Matrix and test all combinations listed.*
