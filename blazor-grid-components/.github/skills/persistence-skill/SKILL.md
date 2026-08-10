---
name: persistence-skill
description: Expert knowledge for State Persistence in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to state persistence behaviour, including EnablePersistence API, localStorage serialization, cross-feature state restoration, rendering flow after persistence restore, DOM structure synchronization, and interaction guarantees with virtualization, frozen columns, filtering, sorting, grouping, paging, selection, and infinite scrolling.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for State Persistence in the Blazor DataGrid. Guarantees zero breakage with any other feature. Covers rendering flow, DOM structure, and feature-specific restore sequences.

---

**Agent Invocation**  
Load this skill when:
- Implementing or debugging `EnablePersistence` API
- Fixing state restore issues (columns, sort, filter, group, page, scroll, selection)
- Debugging localStorage serialization/deserialization
- Resolving rendering/DOM layout problems after persistence restore
- Testing feature interactions with persistence enabled
- Investigating scroll position, virtualization cache, or frozen column width resets

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, cross-feature interaction rules
- `/docs/training/02-requirements-analysis/` — State model, persistence properties table, feature interaction rules
- `/docs/architecture/system-architecture.md` — Module lifecycle, EventAggregator, property change propagation
- `/src/SfGrid.Properties.cs` — `EnablePersistence` property at line 732, backing field `_enablePersistence`
- `/src/SfGrid.Lifecycle.cs` — Persistence initialization in `OnParametersSetAsync()` (line 438), `OnAfterRenderAsync()` (lines 344-356)
- `/src/SfGrid.razor.cs` — `SerializeModel()` at line 1177, `SetLocalStorage()` at lines 1709 & 2836, persistence restoration methods
- `/src/SfGrid.Methods.cs` — `GetPersistDataAsync()` public API at line 1339
- `/scripts/virtual-scroll.ts` — Scroll position capture at line 274, restore at lines 1822–1833
- `/scripts/column-chooser.ts` — Media column visibility persistence at lines 35–54 in `setMediaColumns()` method
- `/scripts/sf-grid-fn.ts` — Frozen column width sync on persistence restore at lines 835–836

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Storage key invariant**: State stored as `grid{ID}` in localStorage — if `ID` not set, persistence is inconsistent or broken.
- **Serialization excludes templates**: `JsonIgnoreCondition.WhenWritingNull` + template properties always null = templates never written to storage (see line 1177 in SfGrid.razor.cs).
- **Baseline capture on init**: `_originalProp` captured once at first `OnAfterRenderAsync()` (line 344 in SfGrid.Lifecycle.cs) via `SerializeModel()` → used as factory reset for `ResetPersistDataAsync()`.
- **Feature-specific restore order**: PageSettings → FilterSettings → SortSettings → GroupSettings → SearchSettings → Columns → Selection state.
- **Virtualization + persistence**: Scroll position stored client-side in JS grid object (`gridObject.scrollPosition`), not in C# model → requires explicit JS restore in `virtual-scroll.ts` at lines 1822–1833.
- **Frozen column width sync**: `sf-grid-fn.ts` lines 835–836 guard `if (!isGridFirstRender && frozenColumns && (enablePersistence || isResetData))` ensures frozen widths recalculated after restore.
- **Media column visibility bypass**: `column-chooser.ts` (lines 35–54) directly mutates localStorage entry, bypassing C# serialization path — must be treated as separate mutation channel.
- **Selection state persistence**: `PersistSelection` in `GridSelectionSettings` (line 110) is separate flag; when true, checkbox state restored via `Selection.SetPersistData()` (line 1508) across filter/page/sort operations.

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`src/SfGrid.Properties.cs` (line 732)** — Public `EnablePersistence` property in EnablePersistence getter/setter, `_enablePersistence` backing field
- **`src/SfGrid.Lifecycle.cs` (lines 264, 344–356, 438, 533)** — Init baseline capture in OnAfterRenderAsync(), persistence guard conditions, property update in OnParametersSetAsync()
- **`src/SfGrid.razor.cs` (line 1177)** — Core engine: `SerializeModel()` method; `SetLocalStorage()` at lines 1709, 2836; persistence restoration methods throughout file. `IsSetPersistDataCalled` flag at line 178 (skips localStorage write when custom storage is set)
- **`src/SfGrid.Methods.cs` (line 1339)** — `GetPersistDataAsync()` public API method for custom storage backends; `SetPersistDataAsync()` at line 3921 (apply custom storage JSON)
- **`src/SfGrid.DataProcessing.cs` (lines 651–657)** — `SkipLocalStorageSet` flag prevents recursive localStorage writes; storage key composed as `"grid" + ID`
- **`src/GridPageSettings.razor.cs`** — Persisted properties: CurrentPage, PageSize, PageCount, EnableQueryString, EnableExternalMessage
- **`src/GridSortSettings.razor.cs`** — All properties persisted
- **`src/GridFilterSettings.razor.cs`** — All properties persisted
- **`src/GridGroupSettings.razor.cs`** — Persisted settings; note `PersistGroupState` is separate from `EnablePersistence`
- **`src/GridSearchSettings.cs`** — All properties persisted
- **`src/GridSelectionSettings.cs` (line 110)** — `PersistSelection` property flag (selection-specific; separate from grid `EnablePersistence`)
- **`src/Internal/Actions/Selection.cs` (line 1508)** — `SetPersistData()` method restores checkbox state
- **`scripts/virtual-scroll.ts` (line 274, lines 1822–1833)** — Capture scroll position in scrollListener(), restore on init in onContentReady()
- **`scripts/column-chooser.ts` (lines 35–54)** — `setMediaColumns()` method: Media column visibility persistence via localStorage direct mutation
- **`scripts/sf-grid-fn.ts` (lines 835–836)** — Frozen column width recalculation guard in updateFrozenColumnStyles() conditional

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Calls**
| C# Method | JS Function | Line | Purpose |
|-----------|-------------|------|---------|
| `SetLocalStorage()` | `window.localStorage.setItem` | SfGrid.razor.cs:1709, 2836 | Write serialized grid state to browser localStorage |
| `OnAfterRenderAsync()` | `window.localStorage.getItem` | SfGrid.Lifecycle.cs:344 | Read persisted state on grid initialization |
| `ResetPersistDataAsync()` | `sfBlazor.Grid.removePersistItem` | SfGrid.razor.cs (via JS) | Clear localStorage entry on factory reset |
| `OnAfterRenderAsync()` | `sfBlazor.Grid.setMediaColumns` | SfGrid.Lifecycle.cs:351 | Restore media column visibility from localStorage |

**JavaScript-Side Persistence Operations**
| TypeScript File | Function/Logic | Lines | Responsibility |
|-----------------|----------------|-------|----------------|
| `virtual-scroll.ts` | Scroll position capture | 274 | Store scroll offset in `gridObject.scrollPosition` on scroll event |
| `virtual-scroll.ts` | Scroll position restore | 1822-1829 | Read `gridObject.scrollPosition` and apply `scrollTop`/`scrollLeft` on init |
| `column-chooser.ts` | Media column visibility persistence | 35-54 | **Direct localStorage mutation** bypassing C# serialization path |
| `sf-grid-fn.ts` | Frozen column width sync | 835-836 | Recalculate frozen widths when `enablePersistence || isResetData` |

**Responsibility Split**
| Concern | C# Responsibility | JavaScript Responsibility |
|---------|-------------------|---------------------------|
| **State Serialization** | SerializeModel(), JSON serialization, feature-specific restore order | None — purely C# domain |
| **localStorage Write/Read** | Invoke JS interop (`setItem`, `getItem`, `removeItem`) | Execute actual localStorage API calls |
| **Scroll Position** | None — not in C# model | Capture scroll offset, store in `gridObject.scrollPosition`, restore on init |
| **Media Column Visibility** | Invoke `setMediaColumns()` JS method | **Directly mutate localStorage** JSON without C# involvement |
| **Frozen Column Widths** | None — styling is JS domain | Detect persistence restore flag, recalculate widths via ColumnWidthService |

**CRITICAL Rules**
1. **C# never reads scroll position** — `scrollTop`/`scrollLeft` stored client-side in `gridObject.scrollPosition`, not in serialized model.
2. **Media column visibility dual-channel** — `column-chooser.ts` mutates localStorage **independently** of C# SerializeModel() path; must treat as separate state mutation.
3. **localStorage key contract** — Always `"grid" + ID`; if `ID` property not set, persistence breaks (key becomes `gridundefined` or `gridnull`).
4. **C# serialization excludes templates** — `JsonIgnoreCondition.WhenWritingNull` + templates always null = templates never written to storage; JS never sees template content.

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 200 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Persistence + Paging | `CurrentPage`, `PageSize`, `PageCount`, `EnableQueryString`, `EnableExternalMessage` all persisted; page navigated to last viewed page on restore | Critical |
| Persistence + Sorting | `SortSettings` + columns' `SortOrder`/`Field` persisted; sort applied post-restore before rendering | Critical |
| Persistence + Filtering | `FilterSettings` + filter columns persisted; filters applied post-restore; `TotalItemCount` recalculated from filtered set | Critical |
| Persistence + Grouping | `GroupSettings` + group columns persisted; note: `PersistGroupState` is separate flag — expand/collapse state **not** persisted by `EnablePersistence` | High |
| Persistence + Virtualization | Scroll position (`scrollPosition.top`/`.left`) stored client-side in JS; restored in `virtual-scroll.ts` lines 1822–1833; cache invalidated then re-fetched for visible window | Critical |
| Persistence + Frozen Columns | Frozen column widths recalculated via `sf-grid-fn.ts` lines 835–836 guard post-restore; frozen left/right sections re-rendered with restored column order | High |
| Persistence + Selection | If `GridSelectionSettings.PersistSelection = true`, checkbox header state restored via `Selection.SetPersistData()`; persists across filter/page/sort | Medium |
| Persistence + Infinite Scroll | Scroll position **not** restored in infinite scroll mode (mutually exclusive with virtualization); page fetching resets on persist restore | Medium |
| Persistence + Column Reorder | Column `Index`/`OriginalIndex` + `Visible` + `Width` persisted; column order restored from deserialized column list | High |
| Persistence + Row Drag-Drop | Row reorder not persisted (data source mutation); column-level reorder persisted via `Reorder.cs` | Medium |

---

## Rendering Flow — State Persistence Initialization
<!-- token-budget: 150 words — ASCII flow diagram below -->

```
┌─────────────────────────────────────────────────────────────────────────┐
│  Grid Lifecycle: EnablePersistence = true                               │
└─────────────────────────────────────────────────────────────────────────┘

┌─ OnInitializedAsync()
│  └─ Initialize all modules (VirtualScroll, Sort, Filter, Group, etc.)
│
┌─ OnParametersSetAsync()
│  └─ Update PropertyChanges dictionary with EnablePersistence flag
│
┌─ OnAfterRenderAsync() — First Render (isFirstRender = true)
│  ├─ Capture baseline: _originalProp = SerializeModel(this)
│  │  └─ Captures: columns[], filterSettings, sortSettings, groupSettings, 
│  │            pageSettings, searchSettings, autoSpan
│  │
│  ├─ Check localStorage for 'grid{ID}' entry
│  │  ├─ Found → Load persisted state
│  │  │   ├─ PersistProperties(properties)
│  │  │   │  ├─ Deserialize JSON → feature-specific models
│  │  │   │  ├─ Call DataProcess() to trigger feature-specific restores
│  │  │   │  └─ Update UI (SetLocalStorage not called — prevent double-write)
│  │  │   │
│  │  │   ├─ Feature Restore Order:
│  │  │   │  1. Columns: Index, OriginalIndex, Visible, Width, Freeze state
│  │  │   │  2. PageSettings: CurrentPage, PageSize
│  │  │   │  3. SortSettings: Apply sort via SortModule
│  │  │   │  4. FilterSettings: Apply filters via FilterModule
│  │  │   │  5. GroupSettings: Apply groups via GroupModule
│  │  │   │  6. SearchSettings: Apply search query
│  │  │   │  7. VirtualScroll: Reset cache, restore scroll position
│  │  │   │  8. Selection: If PersistSelection=true, restore checkbox state
│  │  │   │
│  │  │   └─ JS-side restores (via sf-grid.ts):
│  │  │       ├─ setMediaColumns() — restore HideAtMedia column visibility
│  │  │       ├─ virtual-scroll.ts — restore scrollPosition (top, left)
│  │  │       └─ sf-grid-fn.ts — recalc frozen column widths
│  │  │
│  │  └─ Not found → Use default declarative state (PageSettings, etc.)
│  │
│  └─ Post-restore:
│     ├─ DOM re-render with restored state
│     ├─ Call SetLocalStorage() to update localStorage (if not IsSetPersistDataCalled)
│     └─ Mark grid as "render complete"
│
└─ Subsequent Renders (data change, user action)
   ├─ On any feature change (sort, filter, group, page, etc.)
   │  └─ Call SetLocalStorage() to persist new state
   │
   ├─ Manual Reset
   │  └─ ResetPersistDataAsync()
   │     ├─ Call PersistProperties(_originalProp!, isResetPersistData=true)
   │     ├─ Remove 'grid{ID}' from localStorage
   │     ├─ Reset VirtualScroll cache
   │     └─ Re-render with factory defaults
   │
   └─ Manual SetPersistData (custom storage backend)
      └─ SetPersistDataAsync(customStorageJson)
         ├─ Call PersistProperties(customStorageJson)
         ├─ Skip localStorage write (IsSetPersistDataCalled flag)
         └─ Re-render with restored state
```

---

## DOM Structure & Rendering — Before/After Persistence Restore

### Scenario 1: Normal Grid (No Virtualization)

#### Before Persistence (Initial Render)
```
┌─────────────────────────────────────────────────────────────────┐
│ .e-grid (Host)                                                  │
├─────────────────────────────────────────────────────────────────┤
│ .e-gridheader                                                   │
│   ├─ .e-grid-header-inner                                       │
│   │  └─ table.e-table                                           │
│   │     └─ thead                                                │
│   │        └─ tr.e-headercell                                   │
│   │           └─ [Default 3 cols: Name, Order ID, Customer]    │
│   │              ├─ Visible: true, Width: 150px, Index: 0      │
│   │              ├─ Visible: true, Width: 150px, Index: 1      │
│   │              └─ Visible: true, Width: 200px, Index: 2      │
│   │                                                             │
│   └─ .e-gridheader-scrollbar (h-scroll)                         │
│                                                                 │
├─ .e-gridcontent                                                │
│  ├─ GridContent.razor (NOT virtualized)                        │
│  └─ table.e-table                                              │
│     └─ tbody                                                   │
│        ├─ tr.e-row (Row 1 — data row)                          │
│        │  ├─ td: "Nancy" (Col 0)                               │
│        │  ├─ td: "10248" (Col 1)                               │
│        │  └─ td: "VINET" (Col 2)                               │
│        │                                                       │
│        ├─ tr.e-row (Row 2)                                     │
│        │  ├─ td: "Andrew"                                      │
│        │  ├─ td: "10249"                                       │
│        │  └─ td: "TOMSP"                                       │
│        │                                                       │
│        └─ tr.e-row (Row N)                                     │
│           ...                                                  │
│                                                                │
└─ .e-gridpager                                                  │
   └─ Page: 1, PageSize: 10                                      │
```

#### After Persistence Restore
```
┌──────────────────────────────────────────────────────────────┐
│ .e-grid (Host) [class updated with persist flag]            │
├──────────────────────────────────────────────────────────────┤
│ .e-gridheader                                                │
│   ├─ .e-grid-header-inner                                    │
│   │  └─ table.e-table                                        │
│   │     └─ thead                                             │
│   │        └─ tr.e-headercell                                │
│   │           └─ [RESTORED COLUMNS from localStorage]        │
│   │              ├─ Order ID (REORDERED to Index:0)         │
│   │              │  Visible: ✓, Width: 150px, Freeze: Left  │
│   │              │                                           │
│   │              ├─ Customer (REORDERED to Index:1)         │
│   │              │  Visible: ✓, Width: 180px                │
│   │              │                                           │
│   │              └─ Name (REORDERED to Index:2, HIDDEN)     │
│   │                 Visible: ✗, Width: 150px                │
│   │                                                          │
│   └─ .e-gridheader-scrollbar (h-scroll, scrollLeft: 0)       │
│                                                              │
├─ .e-gridcontent                                             │
│  ├─ GridContent.razor (still NOT virtualized)               │
│  └─ table.e-table                                           │
│     └─ tbody                                                │
│        ├─ tr.e-row (SORTED rows — restored SortSettings)   │
│        │  ├─ td: "10248" (Col 0 = Order ID)                 │
│        │  ├─ td: "VINET" (Col 1 = Customer)                │
│        │  └─ [Col 2 = Name — HIDDEN, not rendered]         │
│        │                                                    │
│        ├─ tr.e-row (FILTERED rows — only matching filter)  │
│        │  ├─ td: "10249"                                    │
│        │  ├─ td: "TOMSP"                                    │
│        │  └─ [hidden]                                       │
│        │                                                    │
│        └─ tr.e-row (Row N)                                  │
│           [PAGINATED: rows 21–30, showing Page 3 of 10]     │
│                                                             │
└─ .e-gridpager                                               │
   └─ Page: 3, PageSize: 10 [RESTORED from persistence]       │
```

#### Render Changes After Restore:
- ✅ Column visibility: Name hidden
- ✅ Column order: Index reordered (Order ID → Customer → Name)
- ✅ Column width: Order ID 150px → 150px (unchanged), Customer 150px → 180px
- ✅ Column freeze: Order ID now frozen left
- ✅ Sorting: Rows sorted by Order ID descending
- ✅ Filtering: Only rows matching filter criteria rendered
- ✅ Paging: Page 3 rendered (rows 21–30)
- ✅ Selection: Checkbox header tri-state if `PersistSelection=true`

---

### Scenario 2: Grid with Row Virtualization

#### Before Persistence (Initial Render)
```
┌─────────────────────────────────────────────────────────────────┐
│ .e-grid (Host) [EnableVirtualization=true]                      │
├─────────────────────────────────────────────────────────────────┤
│ .e-gridheader                                                   │
│   └─ table.e-table                                              │
│      └─ thead                                                   │
│         └─ tr.e-headercell (3 cols: Name, Order, Customer)     │
│                                                                 │
├─ .e-gridcontent [height: 400px, overflow-y: scroll]           │
│  ├─ GridVirtualContent.razor (virtualized rows)                │
│  │  └─ .e-virtualtable (virtual container)                     │
│  │     ├─ .e-virtual-track (total height: 100000px)            │
│  │     │  └─ [Translates rows into viewport via translateY]    │
│  │     │                                                        │
│  │     └─ table.e-table [position: absolute, translateY: 0]    │
│  │        └─ tbody [RenderStart:0, RenderEnd:20]               │
│  │           ├─ tr.e-row (Virtual Row 0)                       │
│  │           │  ├─ td: "Nancy"                                 │
│  │           │  ├─ td: "10248"                                 │
│  │           │  └─ td: "VINET"                                 │
│  │           ├─ tr.e-row (Virtual Row 1)                       │
│  │           │  ...                                            │
│  │           └─ tr.e-row (Virtual Row 19)                      │
│  │                                                              │
│  ├─ JS-side:                                                    │
│  │  scrollTop: 0                                                │
│  │  RowStartIndex: 0, RowEndIndex: 20                           │
│  │  TranslateY: 0px                                             │
│  │  VirtualCache[0..19]: populated                              │
│  │                                                              │
│  └─ [User scrolls down → scroll event → ...scroll management]  │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

#### After Persistence Restore (Row Virtualization)
```
┌────────────────────────────────────────────────────────────────────┐
│ .e-grid (Host) [EnableVirtualization=true, persistence restored]   │
├────────────────────────────────────────────────────────────────────┤
│ .e-gridheader                                                      │
│   └─ table.e-table                                                 │
│      └─ thead                                                      │
│         └─ tr.e-headercell [RESTORED column order + visibility]    │
│            ├─ Order ID (frozen left)                               │
│            ├─ Customer                                             │
│            └─ [Name hidden]                                        │
│                                                                    │
├─ .e-gridcontent [height: 400px, overflow-y: scroll]               │
│  ├─ GridVirtualContent.razor (virtualized rows)                    │
│  │  └─ .e-virtualtable                                             │
│  │     ├─ .e-virtual-track [total height recalculated for filtered │
│  │     │  set: 5000px (50 rows × 100px after filter applied)]      │
│  │     │                                                            │
│  │     └─ table.e-table [position: absolute]                      │
│  │        [translateY: 2000px — RESTORED scroll position]          │
│  │        └─ tbody [RenderStart:20, RenderEnd:40]                 │
│  │           ├─ tr.e-row (Virtual Row 20)                         │
│  │           │  ├─ td: "10268" (Order ID)                          │
│  │           │  ├─ td: "GROCE" (Customer)                          │
│  │           │  └─ [Name hidden]                                   │
│  │           ├─ tr.e-row (Virtual Row 21)                         │
│  │           │  ...                                                │
│  │           └─ tr.e-row (Virtual Row 39)                         │
│  │                                                                 │
│  ├─ JS-side Restoration:                                           │
│  │  scrollTop: 2000px [RESTORED from gridObject.scrollPosition]   │
│  │  RowStartIndex: 20, RowEndIndex: 40                             │
│  │  TranslateY: 2000px [Applied via CSS transform]                │
│  │  VirtualCache cleared then re-fetched for visible window       │
│  │                                                                 │
│  ├─ Filtering applied:                                             │
│  │  Total rows: 10000 → 50 (after filter)                          │
│  │  TotalItemCount updated; virtual track height recalc            │
│  │                                                                 │
│  └─ Sorting applied:                                              │
│     Rows sorted by Order ID; cache order reflects sort             │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

#### Render Changes After Restore (Virtualization):
- ✅ Column visibility: Name hidden
- ✅ Column order: Frozen Order ID, then Customer
- ✅ Row virtualization: Viewport scrolled to row 20 (pixel 2000)
- ✅ Virtual track height: Recalculated from filtered rows (50 rows = 5000px)
- ✅ TranslateY applied: Rows shifted 2000px down via CSS transform
- ✅ Cache invalidated: Old cache cleared; new rows fetched for [20, 40] window
- ✅ Sorting: Applied before virtual cache fetch
- ✅ Filtering: Applied; TotalItemCount updated to 50

---

### Scenario 3: Grid with Row + Column Virtualization + Frozen Columns

#### Before Persistence
```
┌────────────────────────────────────────────────────────────────────┐
│ .e-grid [EnableVirtualization=true, EnableColumnVirtualization=   │
│          true, FrozenColumns=1]                                    │
├────────────────────────────────────────────────────────────────────┤
│ .e-gridheader                                                      │
│  ├─ .e-grid-frozen-left-container [width: 200px]                  │
│  │  └─ table.e-table (frozen header)                              │
│  │     └─ thead                                                    │
│  │        └─ tr.e-headercell                                       │
│  │           └─ th (Frozen Col: ID, width: 200px)                 │
│  │                                                                 │
│  ├─ .e-grid-movable-header [flex: 1, overflow-x: scroll]          │
│  │  └─ table.e-table (virtual header)                             │
│  │     └─ thead                                                    │
│  │        └─ tr.e-headercell                                       │
│  │           ├─ th (Virtual Col 0: Name, width: 150px)            │
│  │           ├─ th (Virtual Col 1: Order, width: 150px)           │
│  │           └─ th (Virtual Col 2..N: [...])                      │
│  │              [scrollLeft: 0, StartColumnIndex: 0]               │
│  │                                                                 │
│  └─ .e-gridheader-scrollbar (h-scroll)                             │
│                                                                    │
├─ .e-gridcontent [height: 400px, overflow: hidden]                 │
│  ├─ .e-grid-frozen-left-container [width: 200px]                  │
│  │  ├─ .e-virtualtable (frozen rows)                              │
│  │  │  ├─ .e-virtual-track [height: 100000px]                     │
│  │  │  └─ table.e-table [translateY: 0]                           │
│  │  │     └─ tbody [RenderStart:0, RenderEnd:20]                  │
│  │  │        ├─ tr (Row 0: ID=1)                                  │
│  │  │        ├─ tr (Row 1: ID=2)                                  │
│  │  │        └─ tr (Row 19: ID=20)                                │
│  │  │                                                              │
│  │  └─ .e-gridheader-scrollbar-v (v-scroll)                       │
│  │     [scrollTop: 0, synchronized with movable rows]             │
│  │                                                                 │
│  └─ .e-grid-movable-content [flex: 1, overflow: auto]             │
│     ├─ GridVirtualContent.razor (virtual rows + cols)             │
│     │  └─ .e-virtualtable                                          │
│     │     ├─ .e-virtual-track [height: 100000px]                  │
│     │     ├─ table.e-table [translateY: 0, translateX: 0]         │
│     │     │  └─ tbody [RowStart:0, RowEnd:20]                     │
│     │     │     ├─ tr (virtual row)                               │
│     │     │     │  ├─ [Virtual Col 0..4 rendered]                 │
│     │     │     │  │  [scrollLeft: 0, StartColumnIndex: 0]        │
│     │     │     │  └─ [Cols 5+ not rendered]                      │
│     │     │     └─ ...                                            │
│     │     │                                                        │
│     │     └─ .e-virtual-track [h-scroll track]                    │
│     │                                                              │
│     └─ [scrollTop: 0, scrollLeft: 0]                              │
│        [JS: row scroll synced with frozen left]                    │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

#### After Persistence Restore (Row + Column Virt + Frozen)
```
┌────────────────────────────────────────────────────────────────────┐
│ .e-grid [All virtualizations + frozen, persistence applied]        │
├────────────────────────────────────────────────────────────────────┤
│ .e-gridheader                                                      │
│  ├─ .e-grid-frozen-left-container [width: RECALCULATED 250px]    │
│  │  └─ table.e-table (frozen header)                              │
│  │     └─ thead                                                    │
│  │        └─ tr.e-headercell                                       │
│  │           └─ th (Frozen Col: ID, width: 250px [RESTORED])      │
│  │              [Freeze state: Left [PERSISTED]]                   │
│  │                                                                 │
│  ├─ .e-grid-movable-header [overflow-x: scroll]                   │
│  │  └─ table.e-table (virtual header)                             │
│  │     └─ thead                                                    │
│  │        └─ tr.e-headercell [Column order RESTORED]              │
│  │           ├─ th (Virtual Col 0: Order, width: 180px)           │
│  │           ├─ th (Virtual Col 1: Customer, width: 200px)        │
│  │           └─ th (Virtual Col 2..N: [...])                      │
│  │              [scrollLeft: 500px (RESTORED scroll)]              │
│  │              [StartColumnIndex: 2 (CALCULATED from scroll)]     │
│  │                                                                 │
│  └─ .e-gridheader-scrollbar [scrollLeft: 500px]                   │
│                                                                    │
├─ .e-gridcontent                                                   │
│  ├─ .e-grid-frozen-left-container [width: 250px]                  │
│  │  ├─ .e-virtualtable (frozen rows [RenderStart:15, End:35])    │
│  │  │  ├─ .e-virtual-track [height: 5000px (filtered)]            │
│  │  │  │  [recalc from filtered rows after filter applied]        │
│  │  │  │                                                           │
│  │  │  └─ table.e-table [translateY: 1500px (RESTORED)]           │
│  │  │     └─ tbody                                                 │
│  │  │        ├─ tr (Row 15: ID=115)                               │
│  │  │        ├─ tr (Row 16: ID=116)                               │
│  │  │        └─ tr (Row 34: ID=134)                               │
│  │  │                                                              │
│  │  └─ .e-gridheader-scrollbar-v [scrollTop: 1500px]             │
│  │     [Synchronized with movable rows; width recalc]            │
│  │                                                                 │
│  └─ .e-grid-movable-content                                       │
│     ├─ GridVirtualContent.razor (rows + cols virtual)             │
│     │  └─ .e-virtualtable                                          │
│     │     ├─ .e-virtual-track [height: 5000px, width: 8000px]    │
│     │     │                                                        │
│     │     └─ table.e-table                                         │
│     │        [translateY: 1500px, translateX: 500px (RESTORED)]   │
│     │        └─ tbody [RowStart:15, RowEnd:35]                   │
│     │           ├─ tr (row 15)                                    │
│     │           │  ├─ td (Virtual Col 2: Order = 10240)           │
│     │           │  ├─ td (Virtual Col 3: Customer = VINET)        │
│     │           │  ├─ td (Virtual Col 4..6)                       │
│     │           │  └─ [Cols 0–1 NOT rendered (frozen)]            │
│     │           │                                                  │
│     │           ├─ tr (row 16)                                    │
│     │           │  ...                                            │
│     │           │                                                  │
│     │           └─ tr (row 34)                                    │
│     │              [SORTED by Order ID DESC (SortSettings)]       │
│     │              [FILTERED by Order > 10250 (FilterSettings)]   │
│     │                                                              │
│     └─ [scrollTop: 1500px, scrollLeft: 500px]                     │
│        [Both scroll restores applied from gridObject.scrollPosition]
│        [JS: frozen row scroll synced; frozen column frozen]        │
│                                                                    │
└────────────────────────────────────────────────────────────────────┘
```

#### Render Restoration Flow (Row + Col Virt + Frozen):
1. **Frozen column width recalc**: `sf-grid-fn.ts` lines 835–836 guard fires → `freezeModule.updateFrozenColumnStyles()` recalcs width from persisted column metadata
2. **Column order restore**: Column list deserialized → reordered by `Index` → frozen/movable split applied
3. **Filter + Sort apply**: FilterSettings, SortSettings applied → `TotalItemCount` updated → virtual track height recalculated
4. **Virtual cache invalidate**: Old cache cleared; fetch window [15, 35] from filtered+sorted dataset
5. **Scroll position restore**: `virtual-scroll.ts` sets `scrollTop = 1500px`, `scrollLeft = 500px` → `TranslateY` and `TranslateX` applied
6. **DOM render**: Frozen left section + movable content rendered at new scroll position with restored column/row visibility

---

## Feature-Specific Restore Sequences

### Paging Restore
```c#
// Inside PersistProperties():
var PersistPage = JsonSerializer.Deserialize<GridPageSettings>(
    PersistProp?["pageSettings"]?.ToString()!
);

// Apply restored page settings:
PageSettings.CurrentPage = PersistPage.CurrentPage;      // e.g., 3
PageSettings.PageSize = PersistPage.PageSize;            // e.g., 10
PageSettings.EnableQueryString = PersistPage.EnableQueryString;

// Grid re-renders page 3 on next DataProcess()
```

### Sorting Restore
```c#
// Inside PersistProperties():
var PersistSort = JsonSerializer.Deserialize<GridSortSettings>(
    PersistProp?["sortSettings"]?.ToString()!
);

// Columns with SortOrder from persisted list:
// Col 0 (Order ID): SortOrder = "Descending"
// Col 1 (Name): SortOrder = null

Columns = Columns.Select(c => {
    var persistCol = PersistSort.Columns
        ?.FirstOrDefault(x => x.Field == c.Field);
    return persistCol != null 
        ? new GridColumn { ..., SortOrder = persistCol.SortOrder }
        : c;
}).ToList();

// SortModule applies sort on DataProcess()
```

### Filtering Restore
```c#
// Inside PersistProperties():
var PersistFilter = JsonSerializer.Deserialize<GridFilterSettings>(
    PersistProp?["filterSettings"]?.ToString()!
);

FilterSettings.FilterColumns = PersistFilter.FilterColumns;
// e.g., [{ Field: "Order", Operator: "greaterthan", Value: 10250 }]

// FilterModule applies filter on DataProcess()
// TotalItemCount recalculated from filtered dataset
```

### Selection Restore
```c#
// Inside DataProcess() after feature restores:
if (SelectionSettings?.PersistSelection == true && 
    IsSetPersistDataCalled)
{
    // Restore checkbox header state (tri-state):
    SelectionModule.SetPersistData(state: CheckBoxState);
    // CheckBoxState = "Indeterminate" | "Checked" | "Unchecked"
}
```

### Virtualization Scroll Restore (JS-side)
```typescript
// In virtual-scroll.ts, onContentReady() at lines 1822–1833:
if (enablePersistence && gridObject.scrollPosition) {
    this.content.scrollTop = gridObject.scrollPosition.top;      // e.g., 2000px
    if (this.options.enableColumnVirtualization) {
        this.content.scrollLeft = gridObject.scrollPosition.left;  // e.g., 500px
    }
    // TranslateY and TranslateX applied automatically via CSS
}
```

---

## Serialization Model — What Gets Persisted

```csharp
// SerializeModel() — Line 1177 in SfGrid.razor.cs
private static string SerializeModel(SfGrid<TValue> comp)
{
    IDictionary<string, object> model = new Dictionary<string, object>()
    {
        { "columns", comp.Columns! },                   // List<GridColumn>
        { "filterSettings", comp.FilterSettings! },     // GridFilterSettings
        { "searchSettings", comp.SearchSettings! },     // GridSearchSettings
        { "sortSettings", comp.SortSettings! },         // GridSortSettings
        { "groupSettings", comp.GroupSettings! },       // GridGroupSettings
        { "pageSettings", comp.PageSettings! },         // GridPageSettings
        { "autoSpanning", comp.AutoSpan }               // bool
    };
    return JsonSerializer.Serialize(model, 
        new JsonSerializerOptions() { 
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull 
        }
    );
}

// Result stored in localStorage as:
// localStorage['grid{ID}'] = '{ "columns": [...], "filterSettings": {...}, ... }'
```

### Persisted Properties per Feature
| Feature | Persisted | NOT Persisted | Notes |
|---------|-----------|---------------|-------|
| **Columns** | `Index`, `OriginalIndex`, `Visible`, `Width`, `Field`, `Type`, `Freeze`, `DisplayAsCheckBox`, `AllowReordering` | `Template`, `HeaderTemplate`, `FilterTemplate`, `EditTemplate`, `SortComparer`, `AutoSpan`, `FilterItemTemplate`, `EditorSettings` | Templates are always null; excluded by `WhenWritingNull` |
| **PageSettings** | `CurrentPage`, `PageSize`, `PageCount`, `EnableQueryString`, `EnableExternalMessage` | `Template` | Pager UI state excluded |
| **SortSettings** | All properties | None | Complete sort state persisted |
| **FilterSettings** | All properties | None | Complete filter criteria persisted |
| **GroupSettings** | All properties | `CaptionTemplate`, `ExpandAllGroups`, `PersistGroupState` | Group expand/collapse not persisted by `EnablePersistence` |
| **SearchSettings** | All properties | None | Complete search state persisted |
| **Selection** | Via `PersistSelection` flag only | (Depends on checkbox state via `SetPersistData()`) | Separate from grid `EnablePersistence` |

---

## localStorage Storage & Retrieval

### Storage Key Format
```
Key: "grid" + Grid.ID
Value: JSON string from SerializeModel()

Example:
localStorage['gridOrderDetails'] = 
'{"columns":[{"index":0,"field":"Order ID","visible":true,"width":"150px"},...],"pageSettings":{"currentPage":3,...},...}'
```

### Storage Retrieval
```typescript
// In column-chooser.ts, setMediaColumns() at lines 35–54:
const persistData = 
    JSON.parse(window.localStorage.getItem('grid' + this.parent.element.id));

// Apply persisted media column visibility:
persistData.columns.forEach(col => {
    if (col.hideAtMedia) {
        col.visible = false;  // Restore hidden state
    }
});

window.localStorage.setItem(
    'grid' + this.parent.element.id, 
    JSON.stringify(persistData)
);
```

### Cleanup
```typescript
// In sf-grid.ts, removePersistItem():
localStorage.removeItem(gridId);  // e.g., "gridOrderDetails"
```

---

## Critical Guard Conditions

### Prevent Double-Write
```csharp
internal bool SkipLocalStorageSet;  // Prevents recursive localStorage writes

// Usage:
if (!SkipLocalStorageSet)
{
    await InvokeMethod("window.localStorage.setItem", 
        new object[] { $"grid{ID}", SerializeModel(this) });
}
SkipLocalStorageSet = false;  // Reset after write
```

### Virtualization + Persistence Guard
```csharp
// Line 1563 in SfGrid.razor.cs:
if (!EnableVirtualization || CurrentViewData == null || 
    (EnableVirtualization && GroupSettings != null && 
     GroupSettings.Columns?.Length > 0) || 
    (EnableVirtualization && _updateVirtualPageSize) || 
    (EnableVirtualization && Reset) || 
    (EnableVirtualization && 
     ((AllowFiltering && FilterSettings?.Columns?.Count > 0 && 
       requestType != "Save" && requestType != "Delete") || 
      (SearchSettings!.Key?.Length > 0) || 
      (EnablePersistence && requestType != "Save" && 
       requestType != "Delete"))) && 
    (actionArgs?.RequestType != Action.Sorting || requestType != "Sorting"))
{
    // Cache invalidation triggered on persistence restore
}
```

### Frozen Column Width Sync
```typescript
// Lines 835-836 in sf-grid-fn.ts:
if (!this.isGridFirstRender && this.options.frozenColumns && 
    (this.options.enablePersistence || isResetData))
{
    this.freezeModule.updateFrozenColumnStyles();
    // Recalculates frozen column widths from persisted metadata
}
```

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: State Persistence  

**Context**  
You are implementing or fixing State Persistence for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Persistence saves grid configuration (columns, filters, sort, groups, pages, scroll) to browser localStorage and restores it on page reload. The API is `EnablePersistence = true` and storage key is `grid{ID}` — the grid's `ID` property **must be explicitly set**.

**Core Architecture**  
- **Storage**: Browser localStorage via `window.localStorage.setItem/getItem`
- **Serialization**: `SerializeModel()` captures grid state as JSON; uses `JsonIgnoreCondition.WhenWritingNull` (templates excluded)
- **Baseline**: `_originalProp` captured at first `OnAfterRenderAsync()` → used for `ResetPersistDataAsync()`
- **Restoration Flow**: Deserialize JSON → apply feature-specific restores (Paging → Sorting → Filtering → Grouping → Search → Columns → Selection) → trigger `DataProcess()`
- **Public API**: `GetPersistDataAsync()` (read), `SetPersistDataAsync(string)` (custom storage), `ResetPersistDataAsync()` (factory reset)
- **Flags**: `SkipLocalStorageSet`, `IsSetPersistDataCalled`, `EnablePersistence` property

**Key Behaviors**  
1. **Storage key = `grid{ID}`** — If `ID` not set, persistence breaks; always require explicit `ID`.
2. **Serialization excludes templates**: `JsonIgnoreCondition.WhenWritingNull` + template properties always null → templates never stored.
3. **Baseline capture once**: `_originalProp = SerializeModel(this)` called on first render; reused for factory reset.
4. **Feature restore order**: PageSettings → SortSettings → FilterSettings → GroupSettings → SearchSettings → Columns → Selection.
5. **VirtualScroll scroll restore**: Client-side via `virtual-scroll.ts` lines 1822–1833 (not in C# model).
6. **Frozen column width sync**: `sf-grid-fn.ts` line 834 guard recalcs on restore.
7. **Media column visibility**: `column-chooser.ts` direct localStorage mutation (separate from C# serialization).
8. **Selection persistence**: `PersistSelection` flag separate from `EnablePersistence`; checkbox state restored via `SetPersistData()`.

**Cross-Feature Rules**  
- **Sorting**: `SortSettings` fully persisted; applied before virtual cache fetch.
- **Filtering**: `FilterSettings` fully persisted; `TotalItemCount` recalculated; virtual track height adjusted.
- **Paging**: `CurrentPage`, `PageSize`, `PageCount` persisted; user returns to last viewed page.
- **Grouping**: `GroupSettings` persisted; note `PersistGroupState` (expand/collapse) is **separate** flag.
- **Virtualization**: Scroll position stored in JS grid object; row/column cache invalidated and re-fetched; TranslateY/TranslateX re-applied.
- **Frozen Columns**: Frozen widths recalculated post-restore; column order preserved.
- **Infinite Scroll**: Mutually exclusive with virtualization; scroll position not restored.

**Public API Methods**  
- `GetPersistDataAsync()` → returns serialized state as string (for custom storage backends)
- `SetPersistDataAsync(properties: string)` → restore from custom storage (skips localStorage write)
- `ResetPersistDataAsync()` → reset to factory defaults (clears localStorage, reverts to `_originalProp`)

**localStorage Contract**  
- **Write**: `await SetLocalStorage()` calls `window.localStorage.setItem('grid' + ID, SerializeModel(this))`
- **Read**: On init, JS retrieves `localStorage['grid' + ID']` and passes to `SetPersistDataAsync()`
- **Clear**: `ResetPersistDataAsync()` calls `sfBlazor.Grid.removePersistItem(DataId, 'grid' + ID)`

**JS-Interop**  
- `window.localStorage.setItem(key, value)` — Write serialized model
- `window.localStorage.getItem(key)` — Read on init
- `sfBlazor.Grid.removePersistItem(dataId, key)` — Remove entry on reset
- `sfBlazor.Grid.setMediaColumns(isResetPersistData)` — Restore column visibility
- `virtual-scroll.ts`: Restore scroll position from `gridObject.scrollPosition`

**Edge Cases**  
- **ID not set**: Persistence uses `null` or `undefined` as key → breaks; always require explicit ID.
- **Data source replaced at runtime**: Filtered/sorted rows change; virtual track height recalculates; cache invalidated.
- **User navigates away → returns**: localStorage persists; page reloads with restored state automatically (if `EnablePersistence=true`).
- **Programmatic feature change (sort, filter, group)**: `SetLocalStorage()` called → new state written to localStorage.
- **Mixed persist modes**: `EnablePersistence=true` + `GetPersistDataAsync()` + custom DB storage → custom storage takes precedence via `SetPersistDataAsync()`.

**When to Use Related Skills**  
- **For DOM rendering issues, component tree, or CSS transform problems** → Load `.github/skills/virtualization-dom-structure-skill/SKILL.md` or `.github/skills/scroll-management-skill/SKILL.md`
- **For feature-specific interactions** → Load skills for Sorting, Filtering, Grouping, Paging, Selection, etc.

**Before Making Changes**  
1. Read `/docs/training/` — Understand cross-feature rules and edge cases documented for this grid.
2. Verify `ID` property is explicitly set on `<SfGrid>` component.
3. Test persistence with all feature combinations (sort + filter + page + virtualization).
4. Ensure `ConfigureAwait(true)` on all async persistence calls (Blazor context required).
5. Validate serialization excludes templates (check `WhenWritingNull` logic).
6. Test `ResetPersistDataAsync()` clears localStorage and reverts to factory defaults.
7. For scroll/virtualization issues, consult scroll management or virtualization-dom-structure skills.

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public properties have XML doc comments with `<summary>`, `<value>`, `<remarks>`.
- Persistence state fully traceable via browser dev tools (`localStorage['grid{ID}']`).
- Playwright E2E test covering: enable persistence → modify state (sort, filter, page, column reorder) → reload page → verify state restored.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.

---

## Regression Verification Checklist

**Before PR submission**, verify:

- ✅ Storage key correctly uses `grid{ID}` (ID property explicitly set)
- ✅ `SerializeModel()` excludes all template properties (verify in localStorage JSON)
- ✅ Serialization includes: columns[], filterSettings, sortSettings, groupSettings, pageSettings, searchSettings, autoSpan
- ✅ `_originalProp` captured once on first render; used for factory reset
- ✅ Feature restore order: Page → Sort → Filter → Group → Search → Columns → Selection
- ✅ `VirtualScroll` scroll position restored from JS (`gridObject.scrollPosition`)
- ✅ Frozen column widths recalculated via `sf-grid-fn.ts` guard (line 834)
- ✅ Column media visibility restored via `column-chooser.ts` direct localStorage mutation
- ✅ `PersistSelection` checkbox state restored via `Selection.SetPersistData()`
- ✅ `SkipLocalStorageSet` flag prevents double-writes
- ✅ `EnablePersistence` toggle guards cache invalidation
- ✅ `SetPersistDataAsync()` skips localStorage write (custom storage mode)
- ✅ `ResetPersistDataAsync()` clears localStorage and reverts to `_originalProp`
- ✅ Feature combinations tested: Virtualization + Persistence, Frozen + Persistence, Selection + Persistence, Grouping + Persistence
- ✅ Page reload test: Enable persistence → modify state → reload → verify state restored
- ✅ Factory reset test: Modify state → call `ResetPersistDataAsync()` → verify revert to factory defaults

---

## References
- **Skill**: [virtualization-skill](./virtualization-skill/SKILL.md) — Scroll position capture/restore mechanism
- **Skill**: [virtualization-dom-structure-skill](./virtualization-dom-structure-skill/SKILL.md) — DOM structure and CSS transforms
- **Skill**: [frozen-column-skill](./frozen-column-skill/SKILL.md) — Frozen column width calculations
- **Skill**: [scroll-management-skill](./scroll-management-skill/SKILL.md) — Scroll synchronization across virtual sections
- **Skill**: [selection-skill](./selection-skill/SKILL.md) — Checkbox state persistence (`PersistSelection`)
- **Files**: Training docs, architecture docs, tech-stack docs (reference in Knowledge References section above)
