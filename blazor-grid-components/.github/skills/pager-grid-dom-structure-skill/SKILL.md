---
name: pager-grid-dom-structure-skill
description: Expert knowledge for the Pager Grid DOM structure in the Syncfusion Blazor DataGrid. Use this skill for understanding, implementing, and debugging pager component rendering, CSS class hierarchies, state-driven DOM mutations, feature interactions, and keyboard navigation across all feature combinations including filtering, sorting, grouping, virtualization, and persistence.
---

# Skill Instructions
<!-- token-budget: 30 words -->

**Purpose**  
Expert knowledge for Pager DOM Structure in the Blazor DataGrid. Guarantees correct understanding of rendered HTML structure, CSS classes, state flow, and DOM mutations.

---

**Agent Invocation**  
Load this skill when implementing pager DOM features, debugging page navigation issues, adding keyboard accessibility, designing custom templates, integrating pager with other features, or testing pager DOM correctness across feature combinations.

---

## Knowledge References
<!-- token-budget: 80 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, Blazor component lifecycle, bindings
- `/openspec/specs/paging/spec.md` — Complete paging feature specification
- `/docs/architecture/system-architecture.md` — Grid rendering pipeline, event aggregator, state management
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, CSS frameworks, accessibility standards
- `/docs/code-guidelines/coding-standards.md` — Component binding patterns, event handlers, lifecycle rules

---

## Training Insights Applied
<!-- token-budget: 100 words -->

**Key Rules & Edge Cases from Training & Live Codebase**

- **Pager is external component**: `SfPager` from `Syncfusion.Blazor.Navigations` package. Grid only manages bindings (`CurrentPage`, `PageSize`, `TotalItemsCount`, etc.) and event callbacks (`PageChanged`, `PageSizeChanged`). Never directly manipulate pager DOM; always update backing state.
- **State drives DOM**: 100% of pager DOM mutations are Blazor binding-driven (e.g., `<SfPager CurrentPage="@PageSettings.CurrentPage" />`). When state changes, re-render automatically updates DOM via SfPager's internal template engine.
- **Page resets are implicit**: Whenever data set changes (filter, sort, group, search), page ALWAYS resets to 1. This is enforced in `DataGenerator.PageQuery()` line 53 logic — never manually call `PageValueChanged`.
- **Query order is mandatory**: Filter → Sort → Page (lines 315 in Data.cs). Changing order breaks paging with filters/sorts.
- **Mutual exclusion**: Paging, Virtualization, Infinite Scroll are mutually exclusive. Priority: Virtualization > Infinite Scroll > Paging.
- **Event pipeline mandatory**: All page changes fire `OnActionBegin` → user cancellation check → execute → `OnActionComplete`. Skipping this breaks developer guards.
- **Guard clauses first**: Always validate `AllowPaging` before any navigation operation.
- **Single source of truth**: `GridPageSettings` (CurrentPage, PageSize, PageCount, PageSizes) is canonical paging state. `DataGenerator` reads this for query composition.
- **EventAggregator communication**: `ModelChanged(RequestType = "paging")` triggers "DataBound" event broadcast to all modules (Selection, FocusHandler, Aggregates).
- **CSS selector strategy**: All pager DOM queries scope to `.e-gridpager` root. Always query from grid container: `.e-grid .e-gridpager`.
- **Keyboard nav context**: `.e-focused` class tracks keyboard focus (independent of `.e-active`). Arrow keys move focus; Enter key navigates page.
- **External message context**: `.e-pagerexternalmsg` shows filter/search status; content set by FilterSettings.ExternalFilterMessage or SearchSettings. NOT custom-configurable per pager event.

---

## Code Location Map
<!-- token-budget: 100 words -->

**All Pager DOM & Rendering Code**

| Path | Lines/Purpose |
|------|--------------|
| `src/GridPageSettings.razor` | — | Pager settings template (CascadingValue wrapper) |
| `src/GridPageSettings.razor.cs` | — | Pager state properties (CurrentPage, PageSize, PageCount, PageSizes, Template, ExternalMessage) |
| `src/SfGrid.Properties.cs` | 137, 972 | AllowPaging property, PageSettings property declarations |
| `src/SfGrid.razor.cs` | 241, 696, 1858, 1866-1867 | PagerRef property, EnsurePagerDropdown() method, PageValueChanged callback, DropDownChanged callback, TotalItemCount binding |
| `src/SfGrid.Methods.cs` | 1648 | GoToPageAsync(int pageNo) public API method |
| `src/SfGrid.Lifecycle.cs` | — | PageSettings registration & sync |
| `src/Internal/SfGrid.razor` | 131–147 | **<SfPager> instantiation block** (THE rendering entry point) |
| `src/GridEvents.cs` | 46, 73 | PageChanging event (line 46), PageChanged event (line 73) |
| `src/EventModels/Grids.cs` | 710–740 | GridPageChangingEventArgs class (line 710), GridPageChangedEventArgs class (line 732) |
| `src/Internal/Actions/Data.cs` | 53–61, 210 | Paging gate condition (lines 53-61), PageQuery() method (line 210) |
| `scripts/sf-grid.ts` | 750–810 | Keyboard navigation, pager focus handling (keydown handlers) |
| `scripts/sf-grid-fn.ts` | 1285, 1332, 1566, 1840 | CurrentPageFocus (1285), pagerFocus (1332), pager event routing, focus detection |
| `scripts/interfaces.ts` | — | GridOptions interface (allowPaging, pageSize, totalItemCount) |

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 200 words -->

**Feature Combinations & DOM Behavior**

| Combination | Must Preserve | Risk | DOM Changes |
|-------------|--------------|------|------------|
| Paging + Filtering | Filter predicates applied first; page resets to 1; TotalItemCount recalculated; .e-pagerexternalmsg updates with filter status | Critical | Numeric button count may decrease; .e-active resets to page 1 |
| Paging + Sorting | Sort applied after filter; page resets to 1; same numeric buttons | Critical | .e-active moves to page 1; sort indicator on column header |
| Paging + Grouping | Grouped data paged normally; page resets to 1 on group/ungroup | Critical | Numeric buttons recalculated for grouped record count; .e-active→page 1 |
| Paging + Virtualization | Mutually exclusive; virtualization takes precedence | Critical | .e-gridpager NOT rendered; virtual scroll container displayed instead |
| Paging + Infinite Scroll | Mutually exclusive; infinite scroll takes precedence | Critical | .e-gridpager NOT rendered; infinite scroll container displayed instead |
| Paging + Selection | Selection persisted across pages (if PersistSelection=true) | High | No pager DOM changes; checkboxes reapplied on page change |
| Paging + Editing | Page navigation discards active edit (Blazor re-render) | High | No pager DOM changes; edit form closed on page change |
| Paging + Aggregates | Aggregates recalculated per page (if Type="Page") | Medium | No pager DOM changes; footer summary updates per page |
| Paging + RTL | RTL CSS applied to pager; logic unchanged | Medium | .e-rtl class on .e-gridpager; buttons visually reversed |
| Paging + Persistence | CurrentPage, PageSize saved/restored from localStorage | High | No pager DOM changes; restored state re-applied on reload |
| Paging + Export | Export respects current page if ExportCurrentPageOnly=true | Medium | No pager DOM changes; export operates in background |
| Paging + Search | Search results paged; page resets to 1; external message updates | High | Same as filtering; .e-pagerexternalmsg shows search summary |
| Paging + Custom Template | Template replaces default pager UI | Medium | Template content re-renders on page change; default elements may be hidden |
| Paging + QueryString | CurrentPage appended to URL query string | Medium | No pager DOM changes; URL updates on page navigation |
| Paging + Column Reorder | No interaction (paging data-independent) | Low | No pager DOM changes |

---

## JavaScript Interop
<!-- token-budget: 150 words -->

**Overview**  
Pager keyboard focus management and grid initialization require JavaScript interop for DOM manipulation that cannot be performed from Blazor (e.g., focus() calls, querySelector operations). Grid uses `GridJSInteropAdaptor` to communicate with TypeScript functions in `sf-grid-fn.ts`.

**C# → JavaScript Calls**

| C# Method | TypeScript Function | Purpose | Line References |
|-----------|---------------------|---------|-----------------|
| `InvokeMethod("sfBlazor.Grid.initialize", ...)` | Grid initialization | Registers grid instance, sets up event handlers, initializes modules | SfGrid.razor.cs:1732 |
| `InvokeMethod("sfBlazor.Grid.contentReady", ...)` | Content ready callback | Updates DOM after data render, applies focus, synchronizes pager state | SfGrid.razor.cs:1686 |
| `InvokeMethod("sfBlazor.Grid.destroy", ...)` | Grid cleanup | Removes event handlers, clears pager references, cleanup | SfGrid.razor.cs:1726, 2839 |

**TypeScript Functions (sf-grid-fn.ts)**

| Function | Lines | Purpose | Called From |
|----------|-------|---------|-------------|
| `CurrentPageFocus` | 1285 | Focuses first or last numeric page button after page change | Pager keyboard navigation (indirect via keydown handler) |
| `pagerFocus` | 1332 | Focuses pager container element | Grid focus management (indirect) |
| Keydown handler with `isPagerFocused` | 1319, 1331, 1366 | Detects when keyboard focus is inside pager element tree | Grid-wide keyboard event routing |

**Responsibility Split**

| Responsibility | Blazor (C#) | JavaScript (TypeScript) |
|----------------|-------------|-------------------------|
| Pager state management | ✅ PageSettings, CurrentPage, PageSize, TotalItemCount binding | ❌ |
| Page navigation logic | ✅ PageValueChanged, DropDownChanged callbacks, data fetching | ❌ |
| Pager DOM rendering | ✅ SfPager component instantiation, binding updates | ❌ |
| Focus management | ❌ | ✅ .focus() calls, querySelector, isPagerFocused detection |
| Keyboard event detection | ❌ | ✅ keydown handler, arrow key routing, Tab navigation |
| Pager instance tracking | ❌ | ✅ sfBlazor.getCompInstance(dataId) for SfPager reference |

**CRITICAL Rules**

1. **Never call JavaScript interop for pager state changes**: State updates trigger Blazor re-render → SfPager updates DOM automatically
2. **Focus operations MUST use JavaScript**: Blazor cannot call .focus() on DOM elements; use InvokeMethod → TypeScript function
3. **Keyboard detection in TypeScript**: `isPagerFocused` flag set in TypeScript keydown handler, passed to Blazor via event args
4. **Pager instance retrieval in JS only**: SfPager component instance accessed via `sfBlazor.getCompInstance(dataId)` in TypeScript

---

## Pager DOM Architecture
<!-- token-budget: 300 words — self-contained reference -->

### Overview & Rendering Gate

The **SfPager** component instantiates in `src/Internal/SfGrid.razor` (lines 131–147) when ALL conditions are true:

```
_isLoaded = true
  AND AllowPaging = true
  AND EnableVirtualization = false
  AND EnableInfiniteScroll = false
```

**Key Fact**: SfPager is from external `Syncfusion.Blazor.Navigations` package. Grid's role is data binding + event callbacks only.

---

### Complete Pager DOM Tree (Annotated)

```
.e-gridpager ◄─────────────────── Root container (CssClass="e-gridpager")
│
└─ .e-pagercontainer ◄──────────── Main pager layout
   │
   ├─ .e-pagerexternalmsg ◄────── External message line (optional)
   │  └─ Content: "Filter: CustomerID = anton" (if EnableExternalMessage)
   │     Appears only when filter/search active
   │
   ├─ .e-firstpage.e-pager-default ◄── First page button
   │  ├─ Icon: [<<]
   │  ├─ State: .e-disabled when CurrentPage = 1
   │  └─ Callback: PageValueChanged(1)
   │
   ├─ .e-prevpage.e-pager-default ◄─── Previous page button
   │  ├─ Icon: [<]
   │  ├─ State: .e-disabled when CurrentPage = 1
   │  └─ Callback: PageValueChanged(CurrentPage - 1)
   │
   ├─ .e-numericcontainer ◄────── Page number buttons container
   │  ├─ .e-link.e-numericitem ··· [1]
   │  ├─ .e-link.e-numericitem.e-active ··· [2] ◄── Current page, highlighted
   │  ├─ .e-link.e-numericitem ··· [3]
   │  └─ ... (up to PageSettings.PageCount buttons, default 8)
   │     Total: min(PageCount, calculated page count)
   │
   ├─ .e-nextpage.e-pager-default ◄─── Next page button
   │  ├─ Icon: [>]
   │  ├─ State: .e-disabled when CurrentPage = TotalPages
   │  └─ Callback: PageValueChanged(CurrentPage + 1)
   │
   ├─ .e-lastpage.e-pager-default ◄─── Last page button
   │  ├─ Icon: [>>]
   │  ├─ State: .e-disabled when CurrentPage = TotalPages
   │  └─ Callback: PageValueChanged(TotalPages)
   │
   └─ .e-pagerdropdown ◄───────── Page size dropdown
      │
      ├─ .e-ddl-wrapper
      │  ├─ <input class="e-input" value="12" />  ◄── Shows current page size
      │  └─ <span class="e-ddl-icon"></span>  ◄─── Dropdown toggle arrow
      │
      └─ Popup (rendered outside e-gridpager, positioned absolute)
         └─ <ul class="e-list-parent">
            ├─ <li class="e-list-item">10</li>
            ├─ <li class="e-list-item e-active">12</li>  ◄── Current selection
            ├─ <li class="e-list-item">20</li>
            ├─ <li class="e-list-item">30</li>
            └─ <li class="e-list-item">All</li>  ◄── Shows all records (if PageSizes=true)
               Options derived from: PageSettings.PageSizes property
```

---

### CSS Class Reference (Complete)

**Navigation Buttons**

| CSS Class | Element | Role | States |
|-----------|---------|------|--------|
| `.e-firstpage` | `<div>` | First page button | `.e-pager-default` (clickable), `.e-disabled` (on page 1) |
| `.e-prevpage` | `<div>` | Previous page button | `.e-pager-default`, `.e-disabled` |
| `.e-nextpage` | `<div>` | Next page button | `.e-pager-default`, `.e-disabled` (on last page) |
| `.e-lastpage` | `<div>` | Last page button | `.e-pager-default`, `.e-disabled` |
| `.e-np` | `<div>` | Shorthand for next/prev | Used in keyboard nav |

**Numeric Page Buttons**

| CSS Class | Applied To | Purpose | States |
|-----------|-----------|---------|--------|
| `.e-numericcontainer` | `<div>` | Container for page numbers | — |
| `.e-link` | `<a>` | Clickable element | Base class for all clickable pager items |
| `.e-numericitem` | `<a>` (in numeric container) | Page number button | `.e-active` (current), `.e-focused` (keyboard nav) |
| `.e-active` | `.e-numericitem` or `.e-list-item` | Current/selected item | Only ONE per container at a time |

**State Classes**

| CSS Class | Meaning | Applied When |
|-----------|---------|--------------|
| `.e-active` | Current page or selected item | CurrentPage = page number; one `.e-numericitem` always has this |
| `.e-focused` | Keyboard navigation focus | User presses arrow keys in pager |
| `.e-disabled` | Button out of action | On first/last page; navigation buttons disable appropriately |

**Container Classes**

| CSS Class | Applied To | Purpose |
|-----------|-----------|---------|
| `.e-gridpager` | Root `<div>` | Grid-specific pager wrapper (CSS hook for grid styling) |
| `.e-pagercontainer` | `<div>` | Main pager layout container (flex or grid) |
| `.e-pagerexternalmsg` | `<div>` | External message line (filter/search status) |
| `.e-pagerdropdown` | `<div>` | Page size dropdown wrapper |
| `.e-pager-default` | `<div>` (nav buttons) | Default styling for navigation buttons |

---

### Rendering Entry Points & Flow

**Entry Point 1: Initial Grid Render**

```
SfGrid.OnInitializedAsync()
  ├─ Set _isLoaded = false
  └─ Register PageSettings
     ↓
SfGrid.OnAfterRenderAsync()
  ├─ Set _isLoaded = true
  └─ Invoke StateHasChanged()
     ↓
Internal/SfGrid.razor render phase
  ├─ Check: @if (_isLoaded && AllowPaging)
  ├─ YES: Render <SfPager> with bindings → Pager appears
  └─ NO: Skip pager block → Pager NOT rendered
```

**Entry Point 2: AllowPaging Changes**

```
SfGrid.OnParametersSetAsync()
  ├─ Detect: "AllowPaging" in PropertyChanges
  ├─ Changed FALSE→TRUE: Invoke StateHasChanged() → Pager rendered
  └─ Changed TRUE→FALSE: Invoke StateHasChanged() → Pager hidden
```

**Entry Point 3: PageSettings Changes (CurrentPage, PageSize, PageCount)**

```
GridPageSettings.OnParametersSetAsync()
  ├─ UpdateProperty("CurrentPage", "PageSize", "PageCount", "PageSizes")
  ├─ Detect changes in PropertyChanges dict
  ├─ Call BaseParent.PropertyChanged()
  └─ SfGrid re-renders → <SfPager> binding updates
     ├─ CurrentPage changed: <SfPager CurrentPage="..." /> re-renders
     │  └─ SfPager updates .e-active class on new page
     ├─ PageSize changed: EnsurePagerDropdown() rebuilds _pagerDropdownData
     │  └─ <SfPager PageSizes="..." /> re-renders
     └─ PageSizes changed: Rebuild dropdown options
        └─ Dropdown UI updates with new options
```

**Entry Point 4: Page Navigation (User Click)**

```
User clicks page number (e.g., "3")
  ↓ [JavaScript event]
GridJSInteropAdaptor detects .e-numericitem click
  ↓ [JS-to-.NET interop]
SfGrid.PageValueChanged(3)
  ├─ Validate: 1 <= 3 <= TotalPages
  ├─ PageSettings.CurrentPage = 3
  └─ ModelChanged(RequestType="paging")
     ├─ Trigger: OnActionBegin event (user can Cancel)
     ├─ Fetch data for page 3 (DataGenerator.PageQuery applied)
     ├─ Trigger: OnActionComplete event
     └─ Invoke StateHasChanged()
        ↓ [Render cycle]
        <SfPager CurrentPage="3" /> binding updates
        ↓ [DOM mutation]
        Remove .e-active from old page button
        Add .e-active to new page button
        ↓ [Visual result]
        User sees page 3 data with active button highlighted
```

**Entry Point 5: PageSize Dropdown Change**

```
User selects dropdown item (e.g., "20")
  ↓ [JavaScript event]
SfPager.PageSizeChanged callback fires
  ↓ [JS-to-.NET interop]
SfGrid.DropDownChanged(20)
  ├─ PageSettings.PageSize = 20
  ├─ Implicit: CurrentPage → 1 (reset)
  └─ ModelChanged(RequestType="paging_size_change")
     ├─ Fetch data with new page size (PageSettings.PageSize = 20)
     └─ Invoke StateHasChanged()
        ↓ [Re-render]
        <SfPager PageSize="20" CurrentPage="1" /> updates
        ↓ [DOM mutations]
        Dropdown input: "12" → "20"
        .e-active: old page → page 1
        Table: 12 rows → 20 rows displayed
```

---

### State-Driven DOM Mutations

**Mutation 1: CurrentPage Changes**

```
TRIGGER: PageSettings.CurrentPage updated (2 → 3)
  ↓
<SfPager CurrentPage="@PageSettings.CurrentPage" /> binding updates
  ↓
DOM MUTATION:
  ├─ Find all .e-numericitem
  ├─ Remove .e-active from page 2 button
  ├─ Add .e-active to page 3 button
  └─ Result: Page highlight moves
```

**Mutation 2: PageSize Changes**

```
TRIGGER: PageSettings.PageSize updated (12 → 20)
  ↓
EnsurePagerDropdown() rebuilds _pagerDropdownData
  ↓
<SfPager PageSize="20" PageSizes="@_pagerDropdownData" /> re-renders
  ↓
DOM MUTATIONS:
  ├─ Dropdown input value: "12" → "20"
  ├─ Dropdown options list refreshed
  ├─ If "All" option added/removed, popup DOM reflects
  └─ Result: Dropdown reflects new page size
```

**Mutation 3: TotalItemCount Changes (Data Refresh)**

```
TRIGGER: SfGrid.TotalItemCount updated (100 → 50 after filter)
  ↓
<SfPager TotalItemsCount="50" /> binding updates
  ↓
SfPager recalculates: pageCount = ⌈50 / PageSize⌉
  ↓
DOM MUTATIONS:
  ├─ Numeric button count may decrease (fewer pages needed)
  ├─ If CurrentPage > new pageCount, auto-corrects
  ├─ .e-nextpage/.e-lastpage buttons may toggle .e-disabled
  └─ Result: Pager reflects new data set size
```

**Mutation 4: AllowPaging Toggles**

```
TRIGGER: AllowPaging false → true
  ↓
Render condition: @if (_isLoaded && AllowPaging)
  ├─ Condition becomes TRUE
  └─ <SfPager> block executes
     ↓
DOM MUTATION:
  ├─ <div class="e-gridpager"> inserted into DOM
  └─ Pager appears at grid bottom

TRIGGER: AllowPaging true → false
  ↓
Render condition becomes FALSE
  ├─ <SfPager> block skipped
  └─ DOM MUTATION:
     ├─ <div class="e-gridpager"> removed from DOM
     └─ Pager disappears
```

---

### Event Routing & DOM Interactions

**Event Flow 1: Numeric Page Button Click**

```
User clicks ".e-numericitem[pageNo=3]"
  ├─ Browser: click event bubbles to .e-pagercontainer
  ├─ GridJSInteropAdaptor: Detects .e-numericitem in event.target
  ├─ InvokeAsync(".NET", "PageValueChanged", 3)
  ├─ SfGrid.PageValueChanged(3):
  │  ├─ Validate: 1 <= 3 <= TotalPages
  │  └─ PageSettings.CurrentPage = 3
  ├─ ModelChanged(RequestType="paging"):
  │  ├─ OnActionBegin (cancellable)
  │  ├─ Data fetch via DataGenerator.PageQuery(1, 3, 12)
  │  └─ OnActionComplete
  ├─ EventAggregator.Trigger("DataBound"):
  │  └─ All modules react (Selection, FocusHandler, etc.)
  ├─ StateHasChanged():
  │  └─ <SfPager CurrentPage="3" /> re-renders
  └─ DOM: .e-active moves from page 2 to page 3 button
```

**Event Flow 2: Navigation Arrow Click (e.g., Next)**

```
User clicks ".e-nextpage"
  ├─ SfPager calculates: pageNo = CurrentPage + 1
  ├─ If pageNo > TotalPages: pageNo = CurrentPage (no-op)
  └─ Otherwise: Same as numeric button click flow (above)
     └─ .e-active moves forward (if not on last page)
```

**Event Flow 3: KeyDown in Pager (Arrow Keys)**

```
User focuses pager element (e.g., page "2" button) and presses ArrowRight
  ├─ sf-grid.ts keydown handler detects arrow key
  ├─ Finds next element in focus order: page "3" button
  ├─ Adds .e-focused to page "3"
  ├─ Removes .e-focused from page "2"
  ├─ DOM: Focus ring moves to page "3" (visual only, no page change)
  │
  ├─ User presses Enter (while page "3" focused)
  ├─ Same as clicking page "3" button
  └─ .e-active moves to page "3", data loads
```

---

### Feature Combination Flows

**Paging + Filtering Integration**

```
BEFORE FILTER: 100 records, page 2, 10 buttons (1-10)
  .e-numericcontainer: 1 2* 3 4 5 6 7 8 [>] [>>]
  Table: rows 13-24

USER APPLIES FILTER: CustomerID = "anton" (3 matches)
  ├─ CurrentPage implicitly resets to 1
  ├─ TotalItemCount: 100 → 3
  ├─ PageCount: 10 → 1
  └─ ExternalMessage: "" → "CustomerID: anton"

AFTER FILTER: 3 records, page 1, 1 button
  .e-numericcontainer: 1*
  .e-pagerexternalmsg: "CustomerID: anton"
  Table: 3 rows (all records, only 1 page)

DOM MUTATIONS:
  ├─ Numeric buttons: 8 → 1 (extra hidden)
  ├─ .e-active: page 2 → page 1
  ├─ .e-pagerexternalmsg: innerHTML updated
  ├─ .e-nextpage/.e-lastpage: toggle .e-disabled
```

**Paging + Sorting Integration**

```
BEFORE SORT: sorted by OrderID, page 3, rows 25-36 displayed
  .e-numericitem[2]: .e-active (page 3)

USER CLICKS "Freight" COLUMN HEADER:
  ├─ Sort: OrderID → Freight ascending
  ├─ CurrentPage implicitly resets to 1
  └─ Data re-fetched for page 1 (sorted by Freight)

AFTER SORT: page 1, rows show lowest Freight values
  .e-numericitem[0]: .e-active (page 1)
  Column header "Freight": Sort indicator added (↑ or ↓)

DOM MUTATIONS:
  ├─ .e-active: page 3 → page 1
  ├─ Numeric buttons: 8 remain (same count)
  ├─ Column header: Sort icon added
```

**Paging vs Virtualization (Mutual Exclusion)**

```
CONDITION: AllowPaging=true AND EnableVirtualization=true

RENDER LOGIC (Internal/SfGrid.razor):
  @if (EnableVirtualization || EnableColumnVirtualization)
    Render <GridVirtualContent>  ◄─ Virtual scroll takes precedence
    
  @if (_isLoaded && AllowPaging && !EnableVirtualization)
    Render <SfPager>  ◄─ Skipped when virtualization active

RESULT: Only ONE scroll mode renders
  .e-gridpager: NOT in DOM ✗
  Virtual container: Present ✓
```

**Keyboard Navigation in Pager**

```
USER TABS INTO PAGER & PRESSES ARROWKEYS:

Focus sequence (logical order):
  [<<] [<] 1 2* 3 4 5 6 7 8 [>] [>>] [▼]
   ↑                         ↑         ↑
   First                 Current  Dropdown

ArrowRight (from page 2):
  ├─ Focus moves to page 3
  ├─ .e-focused moves from page 2 to page 3
  └─ DOM: Remove .e-focused from page 2, add to page 3

Enter (while page 3 focused):
  ├─ Page changes to 3
  ├─ .e-active moves to page 3
  ├─ .e-focused remains on page 3
  └─ Data loads for page 3

ArrowUp (from page 3):
  ├─ Jump to first button: .e-firstpage
  ├─ Focus shifts to [<<]
  └─ DOM: .e-focused on .e-firstpage

CSS Classes Involved:
  .e-focused: Current keyboard focus
  .e-active:  Current page (independent of focus)
```

---

### RTL Mode (Right-to-Left)

```
CONDITION: EnableRtl="true"

DOM CLASS: .e-rtl applied to .e-gridpager

VISUAL LAYOUT CHANGES:
  ├─ Buttons displayed right-to-left
  ├─ [<<] [<] appear on right side (logically first/prev)
  ├─ [>] [>>] appear on left side (logically next/last)
  ├─ Numeric container: right alignment
  └─ Dropdown: positioned on right

LOGIC: UNCHANGED
  ├─ Page 3 is still page 3 (logic unaffected)
  ├─ .e-active on page 3 means page 3 (visually on right)
  ├─ Arrow keys: ArrowLeft still moves to prev (visual left)
  └─ All behavior identical to LTR mode

CSS FRAMEWORK: Handles .e-rtl class styling
  ├─ Text direction: rtl
  ├─ Button order: flex-direction reversed
  ├─ Icon mirroring: handled by Syncfusion CSS
```

---

### Custom Pager Template

```
DEVELOPER DEFINES:
<GridPageSettings>
  <Template>
    <span class="e-pagenomsg">${currentPage} of ${totalPages} pages</span>
  </Template>
</GridPageSettings>

RESULT: Custom template replaces default pager UI

DOM TREE (WITH TEMPLATE):
  .e-gridpager
    .e-pagercontainer
      <!-- Default elements still exist (hidden) -->
      .e-firstpage, .e-prevpage, .e-numericcontainer, .e-nextpage, .e-lastpage, .e-pagerdropdown
      
      <!-- Custom template content overlaid/replaces -->
      .e-pagenomsg: "2 of 10 pages"

PAGE CHANGE: Template re-renders
  ├─ SfPager template engine updates ${} placeholders
  ├─ .e-pagenomsg: "2 of 10" → "3 of 10"
  └─ DOM updates with new values
```

---

### External Message (.e-pagerexternalmsg)

```
RENDERING: Only when EnableExternalMessage=true AND message exists

CONTENT SOURCE:
  ├─ Filter applied: GridFilterSettings.ExternalFilterMessage
  │  └─ "CustomerID: anton"
  ├─ Search applied: GridSearchSettings.ExternalMessage
  │  └─ "Search Key: anton"
  └─ Multiple filters: Comma-separated summary

DOM ELEMENT:
  .e-pagerexternalmsg
    Content: "CustomerID: anton"
    Location: First element in .e-pagercontainer
    Height: ~24px

UPDATES:
  ├─ When filter applied: Message appears/updates
  ├─ When search applied: Message updates
  ├─ When filter/search cleared: Message hidden
  └─ Persists across page navigation
```

---

### Dropdown & PageSizes Property

```
PageSizes PROPERTY CAN BE:
  1. null or false: Dropdown hidden
  2. true: Show default options [10, 12, 20, 30, All]
  3. int[]: Show custom options [5, 10, 15, 20]

DROPDOWN RENDERING:
  .e-pagerdropdown
    .e-ddl-wrapper
      <input value="12" />  ◄─ Current page size
      <span>▼</span>        ◄─ Dropdown arrow

POPUP OPTIONS (Rendered outside pager):
  <ul class="e-list-parent">
    <li class="e-list-item">10</li>
    <li class="e-list-item e-active">12</li>  ◄─ Selected
    <li class="e-list-item">20</li>
    <li class="e-list-item">30</li>
    <li class="e-list-item">All</li>  ◄─ If PageSizes=true

"All" OPTION:
  ├─ Shows all records on single page
  ├─ PageSize = TotalItemCount
  ├─ Single pager button (page 1)
  └─ No pagination needed (one page)
```

---

## Prompt Template
<!-- token-budget: 400 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix | dom-structure-analysis}`  
**Skill**: Pager Grid DOM Structure  

**Context**  
You are working with Pager Grid DOM structure, rendering flows, and feature interactions in the Syncfusion Blazor DataGrid. Pager renders at grid bottom when `AllowPaging="true"` and renders a page navigation component. Pager is external component (`SfPager` from `Syncfusion.Blazor.Navigations`). Grid's responsibility: bindings + event callbacks only.

**Core Architecture**  
- **Root Container**: `.e-gridpager` CSS class wraps entire pager
- **Main Layout**: `.e-pagercontainer` holds all pager elements
- **Navigation Buttons**: `.e-firstpage`, `.e-prevpage`, `.e-nextpage`, `.e-lastpage`
- **Numeric Buttons**: `.e-numericcontainer` with `.e-numericitem` children
- **Active State**: `.e-active` class on current page button (ONE at a time)
- **Dropdown**: `.e-pagerdropdown` with DropDownList component
- **External Message**: `.e-pagerexternalmsg` shows filter/search status
- **State-Driven**: 100% of DOM updates driven by Blazor bindings
- **Query Order**: Filter → Sort → Page (MANDATORY)
- **Mutual Exclusion**: Paging XOR Virtualization XOR Infinite Scroll
- **Rendering Gate**: `_isLoaded && AllowPaging && !Virtualization && !InfiniteScroll`

**Key Behaviors**  
1. **Page Navigation**: `.e-numericitem` click → JS event → PageValueChanged → data fetch → DOM: .e-active moves
2. **Page Reset**: Filter/Sort/Group/Search applied → CurrentPage = 1 (implicit)
3. **PageSize Change**: Dropdown selection → DropDownChanged → PageSize updated → CurrentPage = 1 → data fetch
4. **Numeric Buttons**: Count = min(PageCount=8, ceil(TotalItemCount/PageSize))
5. **Disabled States**: `.e-disabled` on .e-firstpage/.e-prevpage when page=1; on .e-nextpage/.e-lastpage when page=TotalPages
6. **Keyboard Navigation**: Arrow keys move `.e-focused` between buttons; Enter navigates page (TypeScript focus management via CurrentPageFocus function)
7. **RTL Mode**: `.e-rtl` class flips visual layout; logic unchanged
8. **Custom Template**: RenderFragment replaces default UI; ${currentPage}, ${totalPages} placeholders
9. **External Message**: `.e-pagerexternalmsg` shows "CustomerID: anton" during active filter
10. **Persistence**: CurrentPage & PageSize saved/restored from localStorage if EnablePersistence=true
11. **JavaScript Interop**: Focus management handled in TypeScript (sf-grid-fn.ts); state management in C#

**API Contracts**  
- **`GoToPageAsync(int pageNo)`**: Public API; navigates to page; no-op if AllowPaging=false
- **`PageSettings.CurrentPage`**: Gets/sets current page (1-based)
- **`PageSettings.PageSize`**: Gets/sets rows per page; resets CurrentPage to 1
- **`PageSettings.PageCount`**: Gets/sets max numeric buttons (default 8)
- **`PageSettings.PageSizes`**: Gets/sets dropdown options (int[], bool, or null)
- **`PageSettings.Template`**: Gets/sets custom pager template (RenderFragment)
- **`SfGrid.AllowPaging`**: Gets/sets master switch; when false, pager hidden
- **`SfGrid.TotalItemCount`**: Gets total row count (triggers pager page count recalc)

**CSS Selectors (for DOM queries)**  
- `.e-gridpager` — Pager root
- `.e-pagercontainer` — Main pager layout
- `.e-numericitem` — Page number button
- `.e-numericitem.e-active` — Current page
- `.e-numericitem.e-focused` — Keyboard focus
- `.e-pagerexternalmsg` — External message line
- `.e-pagerdropdown .e-input` — Dropdown input (page size)
- `.e-firstpage`, `.e-prevpage`, `.e-nextpage`, `.e-lastpage` — Navigation buttons

**Feature Interactions** (Critical Rules)  
- **Filtering**: Page resets to 1; TotalItemCount recalculated; external message updates
- **Sorting**: Page resets to 1; sort indicator added to column header
- **Grouping**: Page resets to 1; numeric buttons recalculated for grouped data
- **Virtualization**: Pager NOT rendered; virtual scroll takes precedence
- **Infinite Scroll**: Pager NOT rendered; infinite scroll takes precedence
- **Selection**: Selection persisted across pages (if PersistSelection=true)
- **RTL**: Visual layout reversed; logic unchanged
- **Persistence**: State saved/restored from localStorage
- **Query Order**: MUST be Filter → Sort → Page (breaking this order breaks paging)

**Before Making Changes**  
1. Verify render gate condition: `_isLoaded && AllowPaging && !Virtualization && !InfiniteScroll`
2. Check CSS class selectors against actual DOM output
3. Ensure page resets to 1 when data changes (filter/sort/group/search)
4. Test with all feature combinations (especially mutual exclusions)
5. Validate keyboard navigation (.e-focused tracking, CurrentPageFocus interop)
6. Check RTL mode CSS class application
7. Verify query composition order: Filter → Sort → Page
8. Test external message updates on filter changes
9. Identify if JavaScript interop is needed (focus operations, DOM queries) vs. Blazor state updates
10. Never call JavaScript interop for pager state changes; use Blazor bindings only

**Testing & Validation**  
- `.e-gridpager` present when `AllowPaging=true`, absent when `false`
- `.e-active` on exactly ONE `.e-numericitem` at all times
- `.e-active` moves on page change
- `.e-numericitem` count = min(PageCount, calculated pages)
- `.e-disabled` states correct on first/last pages
- Dropdown options match PageSettings.PageSizes
- Custom template renders if Template provided
- .e-focused moves on arrow key presses (verify CurrentPageFocus JS function called)
- RTL mode: .e-rtl class applied; visual flip works
- Pager hidden when Virtualization/InfiniteScroll enabled
- Page resets to 1 when filter/sort/group applied
- JavaScript interop calls succeed for focus management (check browser console for errors)
- isPagerFocused flag correctly detected in TypeScript keydown handler

**Deliverables**  
- DOM structure understanding verified against actual rendered output
- CSS class selectors validated (test with browser DevTools)
- Event routing flow traced (JS→.NET interop, callbacks)
- State-driven mutations explained (binding → re-render → DOM update)
- Feature interactions tested (especially mutual exclusions)
- Keyboard navigation validated
- PR includes test cases for pager DOM correctness

```

---

## Validation Checklist (LLM Agent Reference)

Use this checklist to verify pager DOM implementation:

- [ ] `.e-gridpager` container present when `AllowPaging="true"`
- [ ] `.e-gridpager` absent when `AllowPaging="false"`
- [ ] `.e-active` class on exactly ONE `.e-numericitem` at all times
- [ ] `.e-numericitem` count matches `PageSettings.PageCount` or calculated pages (whichever smaller)
- [ ] `.e-pagerexternalmsg` updates when filter/search applied
- [ ] `.e-pagerexternalmsg` hidden when no filter/search active
- [ ] `.e-disabled` class on `.e-firstpage`/`.e-prevpage` when on page 1
- [ ] `.e-disabled` class on `.e-nextpage`/`.e-lastpage` when on last page
- [ ] Page size dropdown options match `PageSettings.PageSizes`
- [ ] Custom template renders if `PageSettings.Template` set
- [ ] `.e-focused` class applies during keyboard navigation (independent of `.e-active`)
- [ ] `.e-rtl` class applied to `.e-gridpager` when `EnableRtl="true"`
- [ ] Pager hidden when `EnableVirtualization="true"` or `EnableInfiniteScroll="true"`
- [ ] Page resets to 1 when filter/sort/group/search changes
- [ ] Selection persists across pages if `PersistSelection="true"`
- [ ] External message reflects current filter/search status
- [ ] Numeric buttons responsive: Add/remove based on page count changes
- [ ] Navigation arrows disabled appropriately on page boundaries
- [ ] Keyboard navigation works: Tab, Arrow keys, Enter
- [ ] Query composition order verified: Filter → Sort → Page
- [ ] JavaScript interop: `CurrentPageFocus` function called for keyboard focus (check in browser DevTools)
- [ ] JavaScript interop: `isPagerFocused` flag correctly detected in keydown handler
- [ ] No JavaScript interop used for state changes (state updates via Blazor bindings only)

---

## Key Training Insights

**Critical Rules (Non-Negotiable)**

1. **Pager is external**: Grid does NOT own SfPager rendering. Grid provides bindings only.
2. **State drives DOM**: Never directly manipulate pager DOM. Update state; bindings drive DOM updates.
3. **Page resets are implicit**: When data changes, page ALWAYS resets to 1 (enforced in DataGenerator).
4. **Query order matters**: Filter BEFORE Sort BEFORE Page. Breaking this breaks paging.
5. **Mutual exclusion**: Only ONE scroll mode active. Virtualization > Infinite > Paging.
6. **CSS selectors**: Always query from `.e-gridpager` root. Use `.e-grid .e-gridpager` from grid context.
7. **Keyboard nav**: `.e-focused` ≠ `.e-active`. Focus for keyboard, active for current page.
8. **JavaScript interop for focus only**: Use TypeScript functions (CurrentPageFocus, pagerFocus) for focus management; never for state changes.
8. **RTL is CSS-only**: EnableRtl applies CSS flip; logic unchanged.
9. **EventAggregator**: Cross-module communication via "DataBound" event.
10. **Guard clauses**: Always check `AllowPaging` before navigation operations.

**Edge Cases to Handle**

- `AllowPaging=false` + `GoToPageAsync()` → no-op; no exception
- `PageSize=0` → auto-corrected to 12
- `CurrentPage` > max pages → auto-corrected to last valid page
- `PageSizes=true` + "All" selected → single page, all records
- Filter/sort removes rows → page adjusted if out of range
- Persistence restored + invalid page → corrected on restore
- Multiple feature conflicts (e.g., virtualization + paging) → highest priority wins
- Template + default elements → custom template may hide defaults via CSS

---

*Skill Version: 1.0 | Updated: 2026-04-02 | Scope: Pager Grid DOM Structure, Rendering Flow, Feature Integration*


