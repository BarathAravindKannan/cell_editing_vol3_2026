---
name: custom-comparer-sorting
description: Expert knowledge for Custom Comparer Sorting in the Syncfusion Blazor DataGrid. Use this skill for implementing custom sort logic via IComparer<object>, debugging sort order regressions, and understanding feature interactions with grouping, virtualization, filtering, and foreign key columns. Covers complete rendering flow, DOM structure, and cross-feature behaviour.
---

# Skill Instructions
<!-- token-budget: 25 words -->

**Purpose**  
Expert knowledge for Custom Comparer Sorting in the Blazor DataGrid. Guarantees zero breakage when implementing `SortComparer` with any other feature combination.

---

**Agent Invocation**  
Load this skill when implementing custom IComparer sort logic, debugging sort order issues with custom comparers, fixing regressions involving sort comparers + grouping/virtualization/filtering, or understanding why sort comparers are silently ignored (remote data, DbSet, ForeignKeySorting priority).

---

## Knowledge References
<!-- token-budget: 80 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern
- `/docs/architecture/system-architecture.md` — Sort<T> module lifecycle, query pipeline, data flow
- `/docs/tech-stack/tech-stack.md` — Blazor/C# patterns, IComparer<T> interface, async/await rules
- `/docs/code-guidelines/coding-standards.md` — Async patterns, null-safety, property change tracking
- `/openspec/specs/sorting/spec.md` — Sorting specification, custom comparer requirements

---

## Training Insights Applied
<!-- token-budget: 120 words -->

- **Custom comparer is local-only** — silently nulled for remote data (`IsRemote() = true`) or EF Core DbSets; no warning logged; silently bypassed
- **ForeignKeySorting takes priority** — if both `ForeignKeySorting` and `SortComparer` set, `ForeignKeySorting` used; `SortComparer` ignored completely
- **Never break grouped columns** — grouped column fields sorted separately in `cols` list; custom comparer only applied to non-grouped sorts
- **Direction resolved from DOM** — sort direction determined by CSS class `e-ascending` / `e-descending` on header cell; not stored in code
- **Query pipeline order** — filter predicates applied first, then sort with custom comparer, then paging/virtualization
- **Virtual cache invalidation** — custom comparer sort triggers full `VirtualScroll<T>.ClearCacheData()`; scroll position resets to top
- **Selection state cleared** — unless `PersistSelection = true`; custom comparer sort does not bypass this rule
- **No comparer serialization** — `IComparer<object>` is `[JsonIgnore]`; only used server-side; never transmitted to client

---

## Code Location Map
<!-- token-budget: 120 words -->

**Property Declaration & Configuration**
- **`src/GridColumn.cs` (line 645)** — Declares `IComparer<object>? SortComparer { get; set; }` as Parameter with `[JsonIgnore]` attribute; XML docs state constraints (custom sort comparer cannot be used with remote data and Entity framework data source)
- **`src/GridColumn.cs` (line 982)** — Internal `object? ForeignKeySorting` property; priority fallback for foreign key columns
- **`src/GridSortSettings.razor.cs`** — `AllowUnsort` property; controls unsort gesture on header click

**Sort Logic & Custom Comparer Resolution**
- **`src/Internal/Actions/Sort.cs`** — Core module: `SortClickHandler`, `InitiateSort`, `SortColumn`, `UpdateModel`, `RemoveSortColumn`; handles direction toggle, multi-sort state
- **`src/Internal/Actions/Data.cs` (lines 143–220)** — **CRITICAL**: `DataGenerator<T>.SortQuery()` method (lines 143–220); resolves `SortComparer` / `ForeignKeySorting` from column and injects into `query.Sort(field, dir, comparer)`; guards: `!IsRemote()`, `!DbSet check`, null checks; lines 182–185 show ForeignKeySorting priority check, lines 187–191 show SortComparer fallback
- **`src/Internal/Actions/ForeignKey.cs`** — Sets `ForeignKeySorting` on foreign key columns during init; takes priority over `SortComparer`

**Rendering (Header & Icons)**
- **`src/Internal/Renderer/GridHeaderBase.cs` (line 28)** — `GetSortIconOrAriaValue(GridColumn column, string attributeType)` method; when `attributeType="sortIcon"` returns `e-ascending e-icon-ascending` or `e-descending e-icon-descending` based on `SortSettings.Columns`
- **`src/Internal/Renderer/GridHeaderBase.cs` (line 28)** — Same method when `attributeType="sortAria"` returns `aria-sort` attribute value: `ascending`, `descending`, or `none`
- **`src/Internal/Renderer/GridHeaderCell.razor`** — Renders `<th>` cell; `@onclick` triggers `SortClickHandler`; renders sort number span and `e-sortfilterdiv` div with icon classes
- **`src/Internal/Renderer/GridHeader.razor`** — Parent renderer; loops rows and cells; renders `<GridHeaderCell>` components; applies `e-sortfilter` CSS class on `<table>`

**Row Rendering After Sort**
- **`src/Internal/Renderer/GridContent.razor`** — Content area; re-renders after sort data fetch completes
- **`src/Internal/Renderer/GridRow.razor`** — Individual row renderer; receives sorted data from `CurrentViewData`
- **`src/Internal/Renderer/GridVirtualContent.razor`** — Virtual scroll variant; renders visible virtual range after sort comparer applied

**Event Pipeline**
- **`src/SfGrid.razor.cs` (lines 1414–1420)** — Fires `Sorting` event; captures `SortingEventArgs` with `ColumnName`, `Direction`, `Action`, `Cancel`
- **`src/SfGrid.razor.cs` (lines 3400–3405)** — Fires `Sorted` event after execution; `EventAggregator.NotifyAsync("Sorting", ...)` for cross-module notification
- **`src/GridEvents.cs` (lines 79–100)** — Public event properties: `Sorting`, `Sorted` with `EventCallback<SortingEventArgs>`

**Event Models**
- **`src/EventModels/Grids.cs` (line 5205)** — `SortingEventArgs`: `ColumnName`, `Direction`, `Action`, `Cancel`, `IsCtrlKeyPressed`
- **`src/EventModels/Grids.cs`** — `SortedEventArgs`: `SortedColumns` list populated after execution

**Feature Interactions**
- **`src/Internal/Actions/Group.cs`** — Grouped column sorting; `IsFromGroup = true` flag; grouped fields sorted separately in `cols` list
- **`src/Internal/Actions/VirtualScroll.cs`** — Virtual scroll integration; invalidates cache on sort
- **`src/Internal/Actions/Filter.cs`** — Filter integration; sort applied to filtered dataset
- **`src/Internal/Actions/InfiniteScroll.cs`** — Infinite scroll integration; sort resets page to 1

---

## Rendering Flow & DOM Structure

<!-- token-budget: 600 words (combined all rendering flows + ASCII diagrams) -->

### Flow 1: Basic Custom Comparer Sort (User Clicks Header)

```
User Click on Header Cell
        ↓
SortClickHandler(column, cssClass, mouseEvent)
        ↓
[Guard Check] AllowSorting && Column.AllowSorting?
        ├─ YES → Continue
        └─ NO → Return (exit)
        ↓
Determine Direction from CSS class
  - Old class = "e-ascending" → New direction = Descending
  - Old class = "e-descending" or "" → New direction = Ascending
        ↓
Check: Shift/Ctrl key OR (AllowUnsort && Currently Descending)?
  ├─ YES → RemoveSortColumn(columnName)
  └─ NO → SortColumn(columnName, newDirection)
        ↓
UpdateModel(columnName, direction)
  - Create GridSortColumn { Field, Direction }
  - Add/Replace in SortSettings.Columns
  - Trigger PropertyChanges("Columns")
        ↓
ModelChanged(RequestType = Sorting)
        ↓
[Event] Sorting event fires (cancellable)
        ↓
Continue? (Cancel check)
  ├─ YES → Proceed
  └─ NO → Rollback UpdateModel; exit
        ↓
DataGenerator.SortQuery()
  - Extract SortSettings.Columns
  - For each column:
      * Resolve GridColumn from field name
      * Check: ForeignKeySorting != null?
          ├─ YES → Use ForeignKeySorting as comparer
          └─ NO → Use SortComparer (if set)
      * Check: IsRemote() || DbSet?
          ├─ YES → comparer = null (ignored)
          └─ NO → Use resolved comparer
      * Call query.Sort(field, direction, comparer)
        ↓
Execute Query (local IEnumerable.OrderBy with comparer)
        ↓
Populate CurrentViewData with sorted results
        ↓
StateHasChanged() → Re-render GridContent
        ↓
[Rendering] GridHeaderCell updates icon CSS:
  - Add "e-ascending e-icon-ascending" OR
  - Add "e-descending e-icon-descending" OR
  - Remove sort classes if unsorted
  - Add sort priority badge if multi-sort
        ↓
[Event] Sorted event fires
        ↓
[Complete] OnActionComplete event fires
```

### DOM Structure: Header Cell with Sort Icons

```
<th class="e-headercell e-leftalign e-fltr-icon e-mousepointer"
    data-uid="{column-uid}"
    role="columnheader"
    tabindex="0"
    aria-sort="ascending"
    @onclick="SortClickHandler">
    
    <!-- Multi-sort priority badge (if applicable) -->
    <span class="e-sortnumber">1</span>        <!-- Shows sort order (1st, 2nd, 3rd, etc.) -->
    
    <!-- Header text container -->
    <div class="e-headercell-container">
        <div class="e-headercelldiv e-leftalign">
            
            <!-- Header text -->
            <span class="e-headertext">Product Name</span>
            
            <!-- Sort icon div (animated during sort) -->
            <div class="e-sortfilterdiv e-icons e-ascending e-icon-ascending">
                <!-- CSS pseudo-element renders the icon -->
            </div>
            
            <!-- Filter icon div (if filtering enabled) -->
            <div class="e-filtermenudiv e-icons e-icon-filter"></div>
            
            <!-- Column menu icon div (if enabled) -->
            <div class="e-icons e-columnmenu"></div>
        </div>
    </div>
    
    <!-- Resize handler (if AllowResizing = true) -->
    <div class="e-rhandler e-rcursor"></div>
</th>
```

**CSS Classes Applied During Sort States:**

```
Unsorted State:
  <th class="e-headercell">
    <div class="e-sortfilterdiv e-icons"></div>  ← Empty, no sort icon

Ascending State:
  <th class="e-headercell" aria-sort="ascending">
    <span class="e-sortnumber">1</span>          ← Priority badge
    <div class="e-sortfilterdiv e-icons e-ascending e-icon-ascending"></div>

Descending State:
  <th class="e-headercell" aria-sort="descending">
    <span class="e-sortnumber">1</span>
    <div class="e-sortfilterdiv e-icons e-descending e-icon-descending"></div>
```

### ASCII Diagram: Complete Sort Data Pipeline

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       CUSTOM COMPARER SORT PIPELINE                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                               │
│  ┌───────────────────────┐                                                  │
│  │  User clicks header   │                                                  │
│  │   (SortClickHandler)  │                                                  │
│  └───────────┬───────────┘                                                  │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────────────────────────┐                              │
│  │ Extract direction from CSS class          │                              │
│  │ - e-ascending → toggle to Descending      │                              │
│  │ - e-descending → toggle to Ascending      │                              │
│  │ - (none) → set to Ascending               │                              │
│  └───────────┬───────────────────────────────┘                              │
│              │                                                              │
│              ▼                                                              │
│  ┌───────────────────────────────────────────┐                              │
│  │ Check: Multi-sort gesture?                │                              │
│  │ (Shift/Ctrl key pressed)                  │                              │
│  └─┬─────────────────────────────────────┬──┘                              │
│    │                                     │                                 │
│    YES                                   NO                                │
│    │                                     │                                 │
│    ▼                                     ▼                                 │
│ ┌────────────────────┐           ┌──────────────────┐                     │
│ │ Append to sort set │           │ Replace sort set │                     │
│ │ (multi-sort logic) │           │  (single sort)   │                     │
│ └────────────────────┘           └──────────────────┘                     │
│    │                                     │                                 │
│    └─────────────────┬───────────────────┘                                │
│                      │                                                     │
│                      ▼                                                     │
│  ┌──────────────────────────────────────────────┐                          │
│  │ UpdateModel(columnName, direction)           │                          │
│  │ - Create GridSortColumn                      │                          │
│  │ - Add/Replace in SortSettings.Columns        │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ Fire Sorting Event (SortingEventArgs)         │                          │
│  │ - User can set Cancel = true                 │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ Cancelled? ─ YES ─→ Rollback; Exit           │                          │
│  │         │                                     │                          │
│  │        NO                                     │                          │
│  │         │                                     │                          │
│  └────────────┬──────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ╔══════════════════════════════════════════════╗ CRITICAL                 │
│  ║ DataGenerator.SortQuery()                    ║ EXECUTION POINT         │
│  ║ ─────────────────────────────────────────────║                         │
│  ║ For each column in SortSettings.Columns:     ║                         │
│  ║                                              ║                         │
│  ║ 1. Resolve GridColumn by Field name          ║                         │
│  ║ 2. Check: ForeignKeySorting != null?         ║                         │
│  ║    YES → Use ForeignKeySorting               ║                         │
│  ║    NO  → Check: SortComparer != null?        ║                         │
│  ║         YES → Use SortComparer               ║                         │
│  ║         NO  → Use default comparer (null)    ║                         │
│  ║ 3. GUARD: IsRemote() || DbSet?               ║                         │
│  ║    YES → Set comparer = null (bypass)        ║ ⚠️ SILENT BYPASS       │
│  ║    NO  → Keep resolved comparer              ║                         │
│  ║ 4. Call query.Sort(field, dir, comparer)     ║                         │
│  ║    → LINQ OrderBy/ThenBy with comparer       ║                         │
│  ╚══════════════════════════════════════════════╝                         │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ Execute Query on DataSource                  │                          │
│  │ (IEnumerable.OrderBy/ThenBy with comparer)   │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ Populate CurrentViewData with sorted data    │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ StateHasChanged() → Trigger re-render        │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ GridHeaderCell Renders Updated Icons         │                          │
│  │ - CSS: e-ascending / e-descending            │                          │
│  │ - aria-sort: ascending / descending / none   │                          │
│  │ - Sort number badge updated for multi-sort   │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ GridContent Re-renders Rows                  │                          │
│  │ (Rows from CurrentViewData in sorted order)  │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ Fire Sorted Event (SortedEventArgs)          │                          │
│  │ - SortedColumns list populated               │                          │
│  └────────────┬─────────────────────────────────┘                          │
│               │                                                            │
│               ▼                                                            │
│  ┌──────────────────────────────────────────────┐                          │
│  │ Fire OnActionComplete Event                  │                          │
│  │ (Sort operation fully complete)              │                          │
│  └──────────────────────────────────────────────┘                          │
│                                                                               │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Flow 2: Multi-Sort Rendering with Custom Comparers

```
First Column Sort (AllowMultiSorting = true):
  HeaderCell[0] → SortClickHandler → UpdateModel → SortSettings.Columns = [Col1 ASC]
  ↓ Render:
    <span class="e-sortnumber">1</span>      ← Priority badge shows "1"
    <div class="e-sortfilterdiv e-ascending"></div>

Second Column Sort (Shift+Click):
  HeaderCell[2] → SortClickHandler (with isMultiSort=true)
  ↓ UpdateModel → SortSettings.Columns = [Col1 ASC, Col2 DESC]
  ↓ Render: BOTH headers updated
    Header[0]:
      <span class="e-sortnumber">1</span>
      <div class="e-sortfilterdiv e-ascending"></div>
    Header[2]:
      <span class="e-sortnumber">2</span>    ← New priority badge
      <div class="e-sortfilterdiv e-descending"></div>

DataGenerator.SortQuery():
  query.Sort("Col1", "ascending", col1Comparer)
         .Sort("Col2", "descending", col2Comparer)   ← Chained!
  ↓ Result:
    query.OrderBy(col1Comparer).ThenBy(col2Comparer)
```

### Flow 3: Sort + Grouping (Custom Comparer + Group Column)

```
SortSettings.Columns layout AFTER grouping + sort:
┌──────────────────────────────────────────┐
│ [0] GroupedCol (IsFromGroup=true)  ASC   │  ← Grouped column ALWAYS first
│ [1] UserSortedCol1 (IsFromGroup=false)   │
│ [2] UserSortedCol2 (IsFromGroup=false)   │  ← Custom comparers applied only here
└──────────────────────────────────────────┘

Data.cs SortQuery() execution:
  cols = [GroupedCol, UserSortedCol1, UserSortedCol2]
  
  For GroupedCol (IsFromGroup=true):
    → Special handling: sorted by group data internally
    → Custom comparer SKIPPED for this field
    
  For UserSortedCol1, UserSortedCol2:
    → Custom comparer APPLIED (if set on column)
    → query.Sort(userCol1, "asc", col1Comparer)
    → query.Sort(userCol2, "desc", col2Comparer)

Result: Groups ordered by grouping, then rows within groups ordered by custom comparer
```

### Flow 4: Sort + Virtualization (Custom Comparer + Virtual Scroll)

```
User clicks header to sort:
  ↓
Sort initialization (UpdateModel)
  ↓
Detected: EnableVirtualization = true
  ↓
VirtualScroll<T>.ClearCacheData()
  ├─ Clear all cached virtual pages
  ├─ Reset scroll position to 0
  └─ Mark virtual state as invalid
  ↓
DataGenerator.SortQuery()
  ├─ Apply all sort columns with comparers
  └─ Execute full query (no range yet)
  ↓
Virtual scroll calculates new visible range:
  - StartIndex = 0 (scroll reset to top)
  - EndIndex = VirtualPageSize
  ↓
Fetch visible range [0, PageSize] from sorted data
  WITH custom comparers applied
  ↓
GridVirtualContent renders rows [0, PageSize]
  ↓
Virtual scroll re-enabled for user scrolling
  - Next range request uses previously-sorted data
  - Custom comparer NOT re-applied per scroll
    (sorted data is already ordered)
```

### Flow 5: Sort + Filtering (Custom Comparer Applied Post-Filter)

```
Active Filters:
  FilterSettings.Columns = [FilterCol(value > 100)]

User clicks header to sort by Price (with custom comparer):
  ↓
DataGenerator.SortQuery():
  
  Step 1: Apply filters FIRST
    query.Where(FilterCol > 100)
    → Result: filtered IEnumerable (e.g., 5000 rows)
  
  Step 2: Apply sort with custom comparer SECOND
    query.Sort("Price", "ascending", priceComparer)
    → query.OrderBy(row => row.Price) [using priceComparer]
    → Result: 5000 filtered rows, sorted by custom logic
  
  Step 3: Apply paging/virtualization (if any)
    → Slice to current page / virtual range

Final Result: Filtered + Sorted data displayed
```

### Flow 6: Header Re-render Only (RefreshColumnHeader)

```
Optimization: Sort changes ONLY header icons, not rows!

ModelChanged(RequestType = Sorting):
  ↓
Check: IsFullRefreshRequired?
  NO, because:
  - AllowGrouping = true (sorted data layout unchanged)
  - AllowFiltering = false (no filter row refresh needed)
  ↓
Set PropertyChanges["AllowSorting"] = true
Set PropertyChanges["RefreshColumnHeader"] = true  ← HEADER ONLY
  ↓
StateHasChanged()
  ↓
GridHeader.razor detects change:
  ✗ No GridContent re-render
  ✓ GridHeaderCell re-renders (new sort icons)
  
Result: Only header re-drawn, rows stay visually static
(Data query happens, but only header UI updates)
```

---

## Interaction Matrix (MANDATORY)

<!-- token-budget: 250 words -->

| Combination | Must Preserve | SortComparer Behavior | Risk |
|---|---|---|---|
| Custom Comparer + Grouping | Grouped columns precede user-sorted columns; custom comparer NOT applied to grouped column fields; `IsFromGroup = true` preserved in all clear operations | Comparer applied ONLY to non-grouped sort columns; grouped field sorts via group data internally | Critical |
| Custom Comparer + Filtering | Sort applied to filtered dataset; custom comparer operates on filtered rows only; both queries composed in `DataGenerator` | Comparer applied after filter predicates; filtered result set ordered by comparer logic | Critical |
| Custom Comparer + Virtualization | Sort triggers `VirtualScroll<T>.ClearCacheData()`; scroll position resets to top; virtual cache invalidated; visible range re-fetched with sorted data | Comparer applied during full query execution; visible range sliced from fully-sorted data; comparer NOT re-run per scroll | Critical |
| Custom Comparer + Foreign Key Column | If `ForeignKeySorting` set on column, it takes priority; custom `SortComparer` ignored completely; no error logged | `ForeignKeySorting` object replaces `SortComparer` in query.Sort() call; custom comparer never reaches execution | Critical |
| Custom Comparer + Remote Data | Custom comparer silently ignored; null passed to remote query; remote server uses default sort order; no warning | Comparer checked via `DataManager.DataAdaptor.IsRemote()`; if true, comparer forced to null before query | Critical |
| Custom Comparer + DbSet (EF Core) | Custom comparer silently ignored; null passed to query; remote/DbSet sort via SQL ORDER BY (not custom logic) | Comparer checked via `DataSource.GetType().Name.Contains("DbSet")`; if true, comparer forced to null | Critical |
| Custom Comparer + Editing | Active inline edit row discarded on sort (Blazor re-render clears transient state); row position may shift post-sort; custom comparer does not preserve edit state | Comparer applied normally; row re-ordering may cause edited row to shift position in visual grid | High |
| Custom Comparer + Selection | Selection cleared after sort unless `GridSelectionSettings.PersistSelection = true`; custom comparer does not bypass this | Comparer applied; selection state cleared post-sort regardless of comparer logic | High |
| Custom Comparer + Paging | Sort applies to full dataset; custom comparer operates on entire data set; pager displays sorted result; current page index preserved unless row count changes | Comparer applied to full dataset before paging; pager slices sorted result; page index unchanged unless result count changes | High |
| Custom Comparer + Frozen Columns | Frozen and movable columns both participate in sort; custom comparer treats all columns equally; frozen columns render sorted data in fixed position | Comparer applied uniformly; frozen columns receive sorted rows; no special frozen-column comparer logic | Medium |
| Custom Comparer + Column Reorder | Sort state (by `Field` name) unaffected by column visual reorder; custom comparer keyed by field name, not visual position; field binding stable | Comparer resolves column by `Field` name (not index); visual reorder does not affect comparer binding | Medium |
| Custom Comparer + Multi-Sort | Multiple custom comparers chained via `.ThenBy()` in query pipeline; priority badge shows sort order; each column may have different comparer | Comparer applied per column in sort order; multiple comparers chained: `OrderBy(col1Comparer).ThenBy(col2Comparer)` | Medium |
| Custom Comparer + Infinite Scroll | Sort resets scroll position to 0; previously loaded rows discarded; new sorted data fetched from top with custom comparer applied | Comparer applied during initial query; infinite scroll loads subsequent ranges from fully-sorted data; comparer NOT re-run per scroll | High |
| Custom Comparer + Column Chooser | Custom comparer unaffected by column visibility toggle; comparer resolves columns by field name; hidden columns not sorted | Comparer still applied to hidden columns if included in sort set; visibility does not prevent comparer execution | Low |
| Custom Comparer + Export | Export applies current sort order (with custom comparer) from `SortSettings.Columns` to exported data | Comparer applied before export; exported data reflects custom-sorted order | Medium |

---

## Prompt Template

<!-- token-budget: 500 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Custom Comparer Sorting  

### Context

You are implementing or fixing Custom Comparer Sorting for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Custom Comparer Sorting allows developers to define custom sort logic via `IComparer<object>` on individual grid columns. The sort order is applied server-side in .NET via the `Sort<T>` action module and `DataGenerator<T>` query builder.

**Key Constraint**: Custom comparers are **silently ignored** for remote data sources and EF Core DbSets — no error is logged; the sort simply uses the remote server's default order.

### Core Architecture

- **Property**: `GridColumn.SortComparer` (`IComparer<object>?`)
- **Resolution Point**: `Data.cs` → `DataGenerator<T>.SortQuery()` method (lines 270–315)
- **Priority Rule**: `ForeignKeySorting` (if set) overrides `SortComparer`; no warning given
- **Execution**: Custom comparer injected into `query.Sort(field, direction, comparer)` call
- **Query Pipeline**: Filters → Sort with comparer → Paging / Virtualization
- **Rendering**: `GridHeaderCell.razor` displays sort icons; `GridHeader.razor` manages header table
- **Event Model**: `SortingEventArgs` (before), `SortedEventArgs` (after)

### Complete Rendering Flow

1. **User Interaction**: Click header cell → `SortClickHandler(column, cssClass, mouseEvent)`
2. **Direction Resolution**: Read CSS class `e-ascending` / `e-descending` from previous header state
3. **Multi-Sort Check**: Shift/Ctrl key pressed? Append to sort set; otherwise replace
4. **State Update**: `UpdateModel()` → Add/Replace in `SortSettings.Columns`
5. **Event Fire**: `Sorting` event (cancellable)
6. **Comparer Resolution** (CRITICAL):
   - Resolve `GridColumn` by field name
   - Check: `ForeignKeySorting != null`? Use it (priority).
   - Check: `SortComparer != null`? Use it.
   - Check: `IsRemote()` or `DbSet`? Force comparer = null (silent bypass).
   - Inject into `query.Sort(field, direction, comparer)`
7. **Query Execution**: `DataGenerator.SortQuery()` → `query.OrderBy()/ThenBy()` with comparer
8. **Data Fetch**: Execute sorted query; populate `CurrentViewData`
9. **Re-render**: `StateHasChanged()` → Header cell icons + Row DOM updated
10. **Event Fire**: `Sorted` event → `OnActionComplete` event

### Key Behaviors

1. **Direction is resolved from DOM CSS class** — not stored in code; class removed/toggled on each click
2. **Multi-sort via shift/ctrl** — appends column to sort set; priority badge shows sort order
3. **Grouped columns unaffected** — `IsFromGroup = true` columns sorted separately; custom comparer NOT applied to grouped fields
4. **Comparer is local-only** — silently ignored for remote data (`IsRemote() = true`) or EF Core DbSets
5. **ForeignKeySorting priority** — if both set, `ForeignKeySorting` used; custom `SortComparer` completely ignored
6. **Virtual cache invalidated** — sort triggers `VirtualScroll<T>.ClearCacheData()`; scroll position resets
7. **Selection cleared** — unless `PersistSelection = true`; custom comparer sort does not bypass this rule
8. **Comparer never serialized** — `[JsonIgnore]` attribute; IComparer only used server-side

### DOM Structure & Classes

**Header Cell with Sort Icon:**
```
<th class="e-headercell e-mousepointer" aria-sort="ascending">
  <span class="e-sortnumber">1</span>                    <!-- Priority badge -->
  <div class="e-headercell-container">
    <div class="e-headercelldiv">
      <span class="e-headertext">ColumnName</span>
      <div class="e-sortfilterdiv e-icons e-ascending e-icon-ascending"></div>
    </div>
  </div>
</th>
```

**CSS States:**
- `e-ascending e-icon-ascending` — Ascending sort icon
- `e-descending e-icon-descending` — Descending sort icon
- Empty `e-sortfilterdiv` — Unsorted (no icon)
- `aria-sort="ascending" | "descending" | "none"` — Accessibility attribute

### Feature Interactions (Cross-Feature Rules)

1. **Grouping** — Grouped columns always appear first in `SortSettings.Columns`; custom comparer NOT applied to grouped fields; `IsFromGroup = true` preserved
2. **Filtering** — Sort applied to filtered dataset; custom comparer operates on filtered rows
3. **Virtualization** — Sort invalidates virtual cache; scroll resets; visible range re-fetched with comparer applied
4. **Foreign Key** — `ForeignKeySorting` takes priority; `SortComparer` silently ignored; no error logged
5. **Remote Data / DbSet** — Custom comparer forced to null; remote server uses default sort
6. **Editing** — Active edit row discarded on sort; row position may shift
7. **Selection** — Cleared after sort unless `PersistSelection = true`
8. **Paging** — Sort applies to full dataset; pager displays sorted result; page index preserved
9. **Infinite Scroll** — Sort resets scroll to top; new sorted data fetched from beginning

### API Contracts

- **`SortColumnAsync(columnName, direction, isMultiSort?)`** — Single column sort with custom comparer
- **`SortColumnsAsync(columns, clearPreviousSort)`** — Batch sort with multiple comparers
- **`ClearSortingAsync()`** — Remove all non-grouped sorts; preserves `IsFromGroup = true` entries
- **`GridColumn.SortComparer`** — Property accepting `IComparer<object>`

### Edge Cases & Warnings

1. **Remote data** — Custom comparer silently nulled; no warning logged; use server-side `$orderby` instead
2. **DbSet (EF Core)** — Custom comparer silently nulled; use SQL ORDER BY
3. **ForeignKeySorting conflict** — If both set, `ForeignKeySorting` wins; `SortComparer` ignored
4. **Direction from CSS** — Toggled via CSS class; ensure header cell updates on each sort
5. **Virtual cache** — Must be invalidated on sort; scroll resets to top
6. **Multi-sort priority** — Priority badge reflects current sort order in `SortSettings.Columns`

### Implementation Checklist

- [ ] Guard clauses: `AllowSorting`, `GridColumn.AllowSorting`, null checks
- [ ] `IComparer<object>` resolving at `DataGenerator.SortQuery()` (Data.cs line 270–315)
- [ ] Remote data check: `!IsRemote()` guard before using comparer
- [ ] DbSet check: `!DataSource.GetType().Name.Contains("DbSet")` guard
- [ ] ForeignKeySorting priority: Check `ForeignKeySorting != null` first
- [ ] Multi-sort chaining: `.ThenBy()` with each comparer
- [ ] Virtual cache invalidation: `VirtualScroll<T>.ClearCacheData()` on sort
- [ ] Selection cleared: Unless `PersistSelection = true`
- [ ] Grouped columns preserved: `IsFromGroup = true` entries not removed
- [ ] Event pipeline: `Sorting` (cancellable) → execute → `Sorted` → `OnActionComplete`
- [ ] Header re-render: Icon CSS classes and priority badges updated
- [ ] Test combinations: Grouping + sort, Filter + sort, Virtualization + sort, Foreign key + sort

### Before Making Changes

1. Read `/docs/training/` — Event pipeline, property change detection, cross-feature rules
2. Verify guards: `AllowSorting`, `GridColumn.AllowSorting`, remote data, DbSet checks
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required)
4. Preserve grouped columns (`IsFromGroup = true`) in all clear operations
5. Test with all feature combinations: grouping, filtering, virtualization, editing, frozen columns, selection, paging, infinite scroll
6. Validate custom comparer logic with actual `IComparer<object>` implementation (not null)
7. Check remote data path: Confirm comparer is nulled for remote sources
8. Verify ForeignKeySorting does not conflict with custom comparer

### Deliverables

- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation)
- Custom comparer resolved at `DataGenerator.SortQuery()` and injected into `query.Sort()` call
- Guarded for remote data, DbSet, and ForeignKeySorting priority
- All public methods have XML doc comments
- Event pipeline complete: `Sorting` (cancellable) → execute → `Sorted` → `OnActionComplete`
- Virtual cache invalidated; scroll position reset on sort
- Selection cleared unless `PersistSelection = true`
- Grouped columns (`IsFromGroup = true`) preserved in all clear operations
- PR references regression verification checklist: `/docs/ai-agents/prompts/regression-verification-prompt.md`

---

## Code Examples

### Example 1: Declaring a Custom Comparer on a Column

```csharp
// In your Razor component:

<SfGrid @ref="Grid" DataSource="@Orders" AllowSorting="true">
    <GridColumns>
        <!-- Column with custom comparer for complex sort logic -->
        <GridColumn Field="Price" 
                    HeaderText="Price" 
                    Type="ColumnType.Number"
                    SortComparer="@PriceComparer"
                    AllowSorting="true">
        </GridColumn>
        
        <!-- Column without custom comparer (default sort) -->
        <GridColumn Field="OrderDate" 
                    HeaderText="Order Date" 
                    Type="ColumnType.DateTime"
                    AllowSorting="true">
        </GridColumn>
    </GridColumns>
</SfGrid>

@code {
    SfGrid<OrderData>? Grid;
    List<OrderData> Orders = new();
    
    // Custom comparer for Price column
    // Sorts by price but handles null/zero values specially
    IComparer<object> PriceComparer = new PriceComparerLogic();
    
    public class PriceComparerLogic : IComparer<object>
    {
        public int Compare(object? x, object? y)
        {
            // Convert to decimal
            decimal priceX = Convert.ToDecimal(x ?? 0);
            decimal priceY = Convert.ToDecimal(y ?? 0);
            
            // Custom logic: Zero prices sort to end
            if (priceX == 0 && priceY != 0) return 1;
            if (priceY == 0 && priceX != 0) return -1;
            if (priceX == 0 && priceY == 0) return 0;
            
            // Normal comparison
            return priceX.CompareTo(priceY);
        }
    }
}
```

### Example 2: Custom Comparer for Case-Insensitive String Sort

```csharp
public class CaseInsensitiveStringComparer : IComparer<object>
{
    public int Compare(object? x, object? y)
    {
        string? strX = x as string;
        string? strY = y as string;
        
        // Handle nulls
        if (strX == null && strY == null) return 0;
        if (strX == null) return -1;
        if (strY == null) return 1;
        
        // Case-insensitive compare
        return string.Compare(strX, strY, StringComparison.OrdinalIgnoreCase);
    }
}

// Usage in Razor:
<GridColumn Field="CustomerName" 
            SortComparer="@new CaseInsensitiveStringComparer()">
</GridColumn>
```

### Example 3: Custom Comparer for Date Sorting (Business Days Only)

```csharp
public class BusinessDayComparer : IComparer<object>
{
    public int Compare(object? x, object? y)
    {
        DateTime dateX = Convert.ToDateTime(x);
        DateTime dateY = Convert.ToDateTime(y);
        
        // Skip weekends when sorting
        DateTime? businessDateX = GetNextBusinessDay(dateX);
        DateTime? businessDateY = GetNextBusinessDay(dateY);
        
        if (businessDateX == null || businessDateY == null)
            return dateX.CompareTo(dateY);
        
        return businessDateX.Value.CompareTo(businessDateY.Value);
    }
    
    private DateTime? GetNextBusinessDay(DateTime date)
    {
        while (date.DayOfWeek == DayOfWeek.Saturday || 
               date.DayOfWeek == DayOfWeek.Sunday)
        {
            date = date.AddDays(1);
            if (date > DateTime.Now.AddYears(1)) return null; // Guard
        }
        return date;
    }
}

// Usage:
<GridColumn Field="DeliveryDate" 
            SortComparer="@new BusinessDayComparer()">
</GridColumn>
```

### Example 4: Multi-Sort with Different Comparers

```csharp
<SfGrid DataSource="@Orders" AllowMultiSorting="true">
    <GridSortSettings AllowUnsort="true">
        <GridSortColumns>
            <GridSortColumn Field="Department" Direction="SortDirection.Ascending"></GridSortColumn>
            <GridSortColumn Field="Salary" Direction="SortDirection.Descending"></GridSortColumn>
        </GridSortColumns>
    </GridSortSettings>
    
    <GridColumns>
        <!-- Department: Default sort (no comparer) -->
        <GridColumn Field="Department" HeaderText="Dept"></GridColumn>
        
        <!-- Salary: Custom comparer (sorts high to low, nulls last) -->
        <GridColumn Field="Salary" 
                    SortComparer="@new SalaryComparer()"
                    AllowSorting="true">
        </GridColumn>
    </GridColumns>
</SfGrid>

@code {
    public class SalaryComparer : IComparer<object>
    {
        public int Compare(object? x, object? y)
        {
            decimal salX = Convert.ToDecimal(x ?? 0);
            decimal salY = Convert.ToDecimal(y ?? 0);
            
            // Custom: High to low, nulls last
            if (salX == 0) return 1;
            if (salY == 0) return -1;
            return salY.CompareTo(salX); // Reverse for descending
        }
    }
}
```

### Example 5: Custom Comparer with Grouping (Preserved Behavior)

```csharp
<SfGrid DataSource="@Orders" 
        AllowSorting="true" 
        AllowGrouping="true">
    
    <GridGroupSettings ShowGroupedColumn="true">
        <GridGroupColumns>
            <GridGroupColumn Field="Department"></GridGroupColumn>
        </GridGroupColumns>
    </GridGroupSettings>
    
    <GridColumns>
        <!-- Grouped column: Sorted by group data (IsFromGroup = true) -->
        <!-- Custom comparer NOT applied to this field -->
        <GridColumn Field="Department" HeaderText="Dept"></GridColumn>
        
        <!-- User-sorted column: Custom comparer APPLIED -->
        <GridColumn Field="Salary" 
                    SortComparer="@new SalaryComparer()"
                    AllowSorting="true">
        </GridColumn>
    </GridColumns>
</SfGrid>

// Result:
// Rows grouped by Department (ascending, group order)
// Within each department, rows sorted by Salary (custom comparer)
```

### Example 6: Handling Remote Data Scenarios (Comparer Bypassed)

```csharp
@* When using remote data, custom comparer is SILENTLY IGNORED *@

<SfGrid TValue="Order" 
        AllowSorting="true"
        @ref="Grid">
    
    @* Using RemoteDataManager - custom comparer will be bypassed *@
    <SfDataManager Url="https://api.example.com/orders" 
                   Adaptor="Adaptors.UrlAdaptor">
    </SfDataManager>
    
    <GridColumns>
        <!-- This comparer will NOT be used (remote data) -->
        <GridColumn Field="CustomField" 
                    SortComparer="@new CustomComparer()"
                    AllowSorting="true">
        </GridColumn>
    </GridColumns>
</SfGrid>

@code {
    // NOTE: Custom comparer is silently ignored for remote data
    // Instead, rely on server-side $orderby parameter
    // Example server request: GET /api/orders?$orderby=CustomField asc
}
```

### Example 7: Checking Guard Conditions in Data.cs Logic

```csharp
// This is the CRITICAL execution point in Data.cs (lines 270-315)

public void SortQuery(Query query)
{
    var columns = Parent.SortSettings?.Columns?.ToList();
    
    if (columns != null && columns.Count > 0)
    {
        foreach (var sortCol in columns)
        {
            // Resolve column by field name
            List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
            List<GridColumn> comp = gridColumns
                .Where(x => !string.IsNullOrEmpty(x.Field) ? 
                       x.Field.Equals(sortCol.Field, StringComparison.Ordinal) : false)
                .ToList();
            
            object? comparer = null;
            
            // PRIORITY CHECK 1: ForeignKeySorting
            if (comp != null && comp.Count > 0 && comp[0].ForeignKeySorting != null)
            {
                // GUARD: Check local data only
                if (!(Parent.DataManager.DataAdaptor.IsRemote()) && 
                    Parent.DataSource != null && 
                    !(Parent.DataSource.GetType().Name.Contains("DbSet", StringComparison.Ordinal)))
                {
                    comparer = comp[0].ForeignKeySorting;  // ✓ Use ForeignKeySorting
                }
                else
                {
                    comparer = null;  // ✗ Remote data: force null
                }
            }
            // PRIORITY CHECK 2: Custom SortComparer (if no ForeignKeySorting)
            else if (comp?.Count > 0 && comp[0].SortComparer != null)
            {
                // GUARD: Check local data only
                if (!(Parent.DataManager.DataAdaptor.IsRemote()) && 
                    Parent.DataSource != null && 
                    !(Parent.DataSource.GetType().Name.Contains("DbSet", StringComparison.Ordinal)))
                {
                    comparer = comp[0].SortComparer;  // ✓ Use custom comparer
                }
                else
                {
                    comparer = null;  // ✗ Remote data or DbSet: force null (SILENT)
                }
            }
            
            // Apply sort with (or without) comparer
            query.Sort(sortCol.Field, 
                      sortCol.Direction.ToString().ToLower(CultureInfo.CurrentCulture), 
                      comparer);  // ← Comparer may be null (remote/DbSet bypass)
        }
    }
}
```

---

## Common Pitfalls & Debugging

<!-- token-budget: 150 words -->

**Pitfall 1: Custom Comparer Not Executing**
- **Cause**: Remote data source (`IsRemote() = true`) or EF Core DbSet
- **Symptom**: Sort order doesn't match custom logic; default order applied
- **Debug**: Check `DataManager.DataAdaptor.IsRemote()` and `DataSource.GetType().Name.Contains("DbSet")`
- **Fix**: Use local `IEnumerable<T>` data source or server-side `$orderby` for remote

**Pitfall 2: Comparer Ignored Despite Setting**
- **Cause**: `ForeignKeySorting` set on column (takes priority)
- **Symptom**: `SortComparer` never reached; foreign key sort order applied instead
- **Debug**: Check `GridColumn.ForeignKeySorting` value; confirm it's not null
- **Fix**: Clear `ForeignKeySorting` or use it intentionally; don't set both

**Pitfall 3: Sort Direction Not Toggling**
- **Cause**: CSS class not updated on header cell after first sort
- **Symptom**: Click header → sorts ascending, but icon remains unsorted; click again → no direction change
- **Debug**: Check `GridHeaderCell.razor`; verify `AddSortIcon()` returns correct CSS class
- **Fix**: Ensure `StateHasChanged()` called after `UpdateModel()`; header cell re-renders

**Pitfall 4: Comparer Works Alone, Breaks with Grouping**
- **Cause**: Custom comparer applied to grouped column field
- **Symptom**: Grouped column order incorrect; rows unsorted within groups
- **Debug**: Check `SortSettings.Columns` for `IsFromGroup = true` entries; comparer applied to wrong field
- **Fix**: Custom comparer should only sort non-grouped columns; skip fields with `IsFromGroup = true`

**Pitfall 5: Virtual Scroll Shows Unsorted Data**
- **Cause**: Virtual cache not invalidated on sort
- **Symptom**: Visible range shows correct sort, but scrolling down shows old cached data in wrong order
- **Debug**: Check `VirtualScroll<T>.ClearCacheData()` called in sort logic
- **Fix**: Ensure `VirtualComponentUpdate` event fires; cache rebuilt with sorted data

---

## Constraints & Edge Cases

<!-- token-budget: 100 words -->

1. **Remote Data Bypass** — `IsRemote() = true` → comparer silently nulled; no error logged; use server-side sort
2. **DbSet Bypass** — `DataSource.GetType().Name.Contains("DbSet")` → comparer silently nulled; use SQL ORDER BY
3. **ForeignKeySorting Priority** — If both `ForeignKeySorting` and `SortComparer` set, `ForeignKeySorting` used; custom comparer ignored
4. **Grouped Columns** — Custom comparer NOT applied to grouped column fields (`IsFromGroup = true`); group sort handled separately
5. **Direction from CSS** — Sort direction toggled via CSS class on header cell; ensure class updated after each sort
6. **No Comparer Serialization** — `IComparer<object>` marked `[JsonIgnore]`; only used server-side; never sent to client
7. **Virtual Cache Reset** — Sort invalidates entire virtual cache; scroll position resets to 0; subsequent scroll ranges use fully-sorted data
8. **Selection Cleared** — Unless `PersistSelection = true`; custom comparer sort does not bypass this
9. **Null Handling** — Comparer must handle null values gracefully; crashes on unhandled null comparison
10. **Multi-Sort Chaining** — Multiple comparers chained via `.ThenBy()`; execution order matches `SortSettings.Columns` order

---

*Skill file generated for Custom Comparer Sorting feature. Token budget monitored. All sections include estimated word counts.*
