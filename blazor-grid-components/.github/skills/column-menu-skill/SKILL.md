---
name: column-menu-skill
description: Expert knowledge for the Column Menu feature in the Syncfusion Blazor DataGrid. Use this skill when implementing or debugging column menu rendering, DOM structure, adaptive/fullscreen menu flow, submenu composition, event sequencing, and cross-feature interactions with filtering, sorting, grouping, column chooser, and keyboard navigation.
---

# Column Menu Skill
<!-- token-budget: 24 words -->

**Purpose**  
Authoritative reference for Column Menu rendering, DOM structure, launch paths, submenu behavior, adaptive UI flow, event sequencing, filter-popup integration, and all related public/internal events in `SfGrid<TValue>`.

---

**Agent Invocation**  
Load this skill when implementing or debugging Grid Column Menu UI, tracing how it renders in standard or adaptive mode, documenting the menu DOM, investigating submenu and checkbox behavior, or fixing regressions involving sorting, grouping, filtering, column chooser, focus navigation, or per-column menu enablement.

---

## Knowledge References
<!-- token-budget: 90 words -->

- `.github/skills/column-chooser-skill/SKILL.md` — reference structure for render/DOM-focused Grid feature skills
- `src/Internal/SfGrid.razor` — root mounting point for `ColumnMenu`, `ColumnChooser`, and adaptive dialog helpers
- `src/Internal/Renderer/ColumnMenu.razor` — primary standard/adaptive column menu renderer and event pipeline
- `src/Internal/Renderer/GridHeaderCell.razor` — header icon render path and click/open orchestration
- `src/Internal/Renderer/AdaptiveDialogRenderer.razor` — adaptive toolbar menu path that shares `ColumnMenuClass`
- `src/SfGrid.Properties.cs` — public API: `ShowColumnMenu`, `ColumnMenuItems`
- `src/SfGrid.razor.cs` — internal menu state fields: `ColumnMenuColumn`, `ColumnMenuInstance`, `ColumnMenuClass`, `IsColumnMenuFilter`
- `src/SfGrid.Lifecycle.cs` — initial hidden menu CSS state
- `src/GridColumn.cs` — per-column `ShowColumnMenu`, chooser-related `ShowInColumnChooser`
- `src/GridEvents.cs` — public column menu events
- `src/EventModels/Grids.cs` — `ColumnMenuClickEventArgs`, `ColumnMenuOpenEventArgs`, `ColumnMenuItemModel`
- `src/Internal/Base/GridJSInteropAdaptor.cs` — JS-to-.NET callbacks for filter hover and close prevention
- `src/Internal/Actions/Filter.cs` — filter-side close/reset logic for menu-hosted popup filtering
- `src/Internal/Renderer/Filter/FilterMenuRenderer.razor` — menu filter popup positioning/cleanup
- `src/Internal/Renderer/Filter/FilterCheckBoxRenderer.razor.cs` — Excel/checkbox filter path from column menu host
- `src/Internal/Renderer/Filter/ExcelBase.razor` — adaptive close hooks after Excel filter actions
- `src/Internal/Actions/FocusHandler.cs` — keyboard-driven open path
- `src/SfGrid.Methods.cs` — `OpenColumnChooserAsync()` path reused by column menu chooser integration
- `src/Internal/Renderer/ColumnChooser.razor` — chooser component opened from column menu
- `scripts/column-menu.ts` — popup positioning, header anchor calculation, hover bridge for filter submenu
- `scripts/filter.ts` — filter popup alignment and close-prevention bridge when hosted from column menu
- `scripts/sf-grid.ts` — JS exports used by the Blazor side for render/position methods
- `scripts/sf-grid-fn.ts` — client-side module wiring for `columnMenuModule`
- `tests/bunit/Actions/ColumnMenu.razor` — direct test coverage for events and item invocation
- `tests/bunit/Actions/AdaptiveDialog.razor` — adaptive dialog rendering coverage
- `tests/bunit/Actions/ShowHide.razor` — coverage for `ShowColumnMenu=false` and column visibility combinations

---

## Training Insights Applied
<!-- token-budget: 110 words -->

- Column Menu is not a single popup; it is a coordinated feature involving header-cell rendering, menu-host state, JS positioning, filter-popup hosting, adaptive dialog rendering, and optional column chooser launch.
- Standard mode renders a real `SfContextMenu`; adaptive header-icon mode renders a fullscreen `SfDialog` with explicit menu-row markup.
- Adaptive toolbar menu is a separate path in `AdaptiveDialogRenderer`, but it reuses the same hidden/visible CSS conventions through `ColumnMenuClass`.
- `OnColumnMenuOpen` is raised for the first-level popup open and for submenu opens; `ColumnMenuIndex` indicates hierarchy depth.
- The Column Chooser submenu inside standard menu is template-driven and uses `SfCheckBox` rows, not plain text-only menu items.
- Direct checkbox changes in the Column Chooser submenu call show/hide column APIs immediately and raise post-change events, but they do not run the full chooser dialog commit pipeline.
- Filter hosted from Column Menu keeps the menu alive using `IsColumnMenuFilter` and JS/.NET hover callbacks.
- `ShowColumnMenu` exists at both grid and per-column levels; a column icon renders only when both levels allow it.

---

## Code Location Map
<!-- token-budget: 100 words -->

- `src/Internal/SfGrid.razor` — mounts `ColumnMenu` and related adaptive helpers
- `src/Internal/Renderer/GridHeaderCell.razor` — renders `.e-columnmenu` icon and dispatches open requests
- `src/Internal/Renderer/ColumnMenu.razor` (line 585 in renderColumnMenu()) — main menu renderer, item factory, submenu logic, open/close/click handlers
- `src/Internal/Renderer/AdaptiveDialogRenderer.razor` — adaptive toolbar export/chooser menu
- `src/SfGrid.Properties.cs` (lines 371, 1151) — public API surface: `ColumnMenuItems`, `ShowColumnMenu`
- `src/SfGrid.razor.cs` — menu state backing fields used across renderers
- `src/SfGrid.Lifecycle.cs` (line 425) — initial hidden class setup and property tracking
- `src/GridColumn.cs` (line 619) — per-column `ShowColumnMenu` visibility control
- `src/GridEvents.cs` (lines 1355, 1384) — public callbacks: `ColumnMenuItemClicked`, `OnColumnMenuOpen`
- `src/EventModels/Grids.cs` (lines 1547, 1628) — event and custom-item payload types
- `src/Internal/Base/GridJSInteropAdaptor.cs` (lines 330, 335) — JS invokables: `FilterMouseOverHandler`, `PreventColumnMenuClose`
- `src/Internal/Actions/Filter.cs` (line 1067) — shared hide/reset helper for menu-hosted filtering
- `src/Internal/Renderer/Filter/FilterMenuRenderer.razor` — menu filter popup placement/close path
- `src/Internal/Renderer/Filter/FilterCheckBoxRenderer.razor.cs` — checkbox/excel filter popup placement/close path
- `src/Internal/Actions/FocusHandler.cs` — keyboard open path
- `scripts/column-menu.ts` (line 26) — `renderColumnMenu()` popup position and filter hover bridge
- `scripts/filter.ts` — filter popup position when hosted by menu
- `tests/bunit/Actions/ColumnMenu.razor` — behavioral reference for standard menu

---

## Public API Surface
<!-- token-budget: 90 words -->

### Grid-level properties
- `SfGrid<TValue>.ShowColumnMenu` — enables the feature globally
- `SfGrid<TValue>.ColumnMenuItems` — overrides default built-in item set with built-in/custom items

### Column-level properties
- `GridColumn.ShowColumnMenu` — disables icon/menu for a specific column
- `GridColumn.ShowInColumnChooser` — controls whether the column appears under the chooser submenu path

### Public events
- `GridEvents<TValue>.OnColumnMenuOpen`
- `GridEvents<TValue>.ColumnMenuItemClicked`

### Related follow-up events from submenu checkbox path
- `GridEvents<TValue>.OnActionComplete`
- `GridEvents<TValue>.ColumnVisibilityChanged`

---

## Render Entry Points
<!-- token-budget: 150 words -->

Column Menu can be entered through four practical launch paths:

1. **Header icon click in standard desktop mode**  
   `GridHeaderCell.razor` renders `.e-columnmenu`; click sets `Parent.ColumnMenuColumn`, raises `ColumnMenuUpdate`, and opens `ColumnMenuInstance`.

2. **Header icon click in adaptive mode**  
   `GridHeaderCell.razor` still dispatches `ColumnMenuUpdate`, but `ColumnMenu.razor` switches into fullscreen `SfDialog` rendering via `IsColumnMenuIconClicked`.

3. **Keyboard/focus open path**  
   `FocusHandler.cs` detects a header cell with `ShowColumnMenu`, sets `ColumnMenuColumn`, triggers `ColumnMenuUpdate`, and calls `OpenAsync(1, 1)` on the menu instance.

4. **Adaptive toolbar menu path**  
   `AdaptiveDialogRenderer.razor` opens a separate `SfContextMenu` for export/chooser items in adaptive layouts. It is not the header column menu itself, but it participates in the same broader menu ecosystem and chooser launch path.

The root component mount occurs from `SfGrid.razor` whenever `ShowColumnMenu` is enabled.

---

## Component Decision Flow (ASCII)
<!-- token-budget: 170 words -->

```text
SfGrid.razor menu selection
══════════════════════════════════════════════════════════════════════

ShowColumnMenu
    │
    ├── render <ColumnMenu TContent="TValue">
    │       │
    │       ├── if !Parent.EnableAdaptiveUI
    │       │       │
    │       │       └── Standard header popup path
    │       │               ├── SfContextMenu host
    │       │               ├── built-in/custom items
    │       │               ├── optional ColumnChooser submenu
    │       │               └── optional Filter submenu hosting
    │       │
    │       └── else if adaptive header icon clicked
    │               │
    │               └── Fullscreen adaptive column menu dialog
    │                       ├── manual responsive rows
    │                       ├── action-specific click handlers
    │                       └── chooser/filter launch shortcuts
    │
    └── if EnableAdaptiveUI and toolbar/adaptive helper needed
            │
            └── render <AdaptiveDialogRenderer TContent="TValue">
                    └── adaptive toolbar menu context menu
                            ├── export items
                            ├── print
                            └── column chooser launch

══════════════════════════════════════════════════════════════════════
```

---

## Header Icon Render Path
<!-- token-budget: 170 words -->

The header cell is where the visible Column Menu affordance originates.

### Icon render conditions
A header cell shows the menu icon only when all are true:
- grid `ShowColumnMenu == true`
- current column `ShowColumnMenu == true`
- column is not checkbox-only header path
- column has a non-empty `Field`

### Header DOM fragment

```text
GridHeaderCell
└── header cell action container
    └── <div class="e-icons e-columnmenu {filtered-state?}"
            role="button"
            title="{ColumnMenuIconARIA}"
            @onclick="ColumnMenuIconClick(column)">
```

### Header click flow

```text
User clicks .e-columnmenu
    └── ColumnMenuIconClick(column)
            ├── if another column menu filter popup is active for different column
            │       ├── close old menu instance
            │       ├── reset hidden css class
            │       └── continue with new target
            ├── Parent.ColumnMenuColumn = column
            ├── EventAggregator.Trigger("ColumnMenuUpdate")
            └── if menu instance currently hidden
                    └── ColumnMenuInstance.OpenAsync(1, 1)
```

Keyboard navigation follows the same state update path through `FocusHandler.cs`.

---

## Standard Column Menu DOM Structure
<!-- token-budget: 320 words -->

```text
ColumnMenu<TContent> standard path
└── <SfContextMenu class="{ColumnMenuClass}" items="MenuItems" enableScrolling="true">
    ├── <MenuTemplates>
    │   └── <Template>
    │       ├── if item maps to chooser column
    │       │   └── <SfCheckBox label="{HeaderText|Field}" checked="column.Visible">
    │       │
    │       ├── if item has icon
    │       │   └── <span class="e-menu-icon {IconCss}"></span>
    │       │
    │       ├── if item is plain/custom text item
    │       │   └── item.Text
    │       │
    │       └── if item is Filter or has submenu items
    │           └── <span class="e-icons e-caret"></span>
    │
    └── <MenuEvents>
        ├── OnOpen="BeforeOpenHandler"
        ├── Opened="ColumnMenuOpenedHandler"
        ├── Closed="ColumnMenuClosedHandler"
        ├── OnClose="ColumnMenuclose"
        └── ItemSelected="ColumnMenuItemClick"
```

### Effective top-level item shapes

```text
Top-level menu item
├── built-in command item
│   ├── AutoFitAll
│   ├── AutoFit
│   ├── Group
│   ├── Ungroup
│   ├── SortAscending
│   ├── SortDescending
│   ├── ColumnChooser
│   └── Filter
│
├── built-in ColumnChooser item
│   └── submenu items generated from eligible grid columns
│       └── each submenu row renders an SfCheckBox template
│
└── custom item from ColumnMenuItemModel
    ├── custom text
    ├── custom icon
    ├── optional custom submenu items
    └── optional hide/separator/url metadata
```

### Important standard-mode DOM notes
- The actual popup host visibility is controlled by CSS classes on `ColumnMenuClass`, not by conditional component mount/unmount.
- The `ColumnChooser` submenu is still part of the context menu model, but its rows render as checkboxes via the menu template.
- Filter menu item receives a caret because it opens a hosted filter UI path.
- Any submenu item also receives a caret because `Item.Items != null`.

---

## Adaptive Header Column Menu DOM Structure
<!-- token-budget: 290 words -->

When adaptive UI is enabled and the header icon path is used, `ColumnMenu.razor` does not render the standard `SfContextMenu` popup. Instead it renders a fullscreen dialog with explicitly authored rows.

```text
ColumnMenu<TContent> adaptive header path
└── <div id="ResDialog" class="-dlg-grid-element e-hide">
    └── <SfDialog id="{GridId}columnmenu"
                  class="e-bigger e-customcolumnmenu e-customfilterdiv e-responsive-dialog e-dlg-fullscreen"
                  width="100%" height="100%">
        ├── <Header>
        │   ├── <SfButton class="e-dlg-closeicon-btn"> close </SfButton>
        │   └── <div class="e-dlg-header">
        │       └── <div class="e-res-custom-element">
        │           └── <div class="e-dlg-custom-header"
        │                   id="{GridId}responsive_columnmenu_dialog_wrapper">
        │               └── "Column Menu"
        │
        └── <Content>
            └── <div class="columndiv columnmenudiv">
                ├── <div class="e-responsivecoldiv e-responsiveautofitalldiv {e-disabled?}">
                ├── <div class="e-responsivecoldiv e-responsiveautofitdiv {e-disabled?}">
                ├── <div class="e-responsivecoldiv e-responsivegroupdiv {e-disabled?}">
                ├── <div class="e-responsivecoldiv e-responsiveungroupdiv {e-disabled?}">
                ├── <div class="e-responsivecoldiv e-responsiveascendingdiv {e-disabled?}">
                ├── <div class="e-responsivecoldiv e-responsivedescendingdiv {e-disabled?}">
                ├── <div class="e-responsivecoldiv e-responsivecolumndiv">
                └── <div class="e-responsivecoldiv e-responsivefilterdiv {e-disabled?}">
```

### Adaptive-mode notes
- Item presence is decided by checking whether corresponding localized text exists in `MenuItems`.
- Each row has a dedicated click handler rather than a generic context-menu `ItemSelected` pipeline.
- `ColumnChooser` and `Filter` are launchers into separate adaptive workflows.
- There is no submenu tree rendering in this fullscreen path.

---

## Adaptive Toolbar Menu DOM Structure
<!-- token-budget: 210 words -->

Adaptive toolbar menu is not the same component as the header column menu, but it matters because it shares menu CSS conventions and can launch the column chooser.

```text
AdaptiveDialogRenderer toolbar menu path
└── <SfContextMenu class="{Parent.ColumnMenuClass}" items="BuildDefaultItems()">
    └── <MenuEvents>
        ├── OnOpen="BeforeOpenHandler"
        ├── Created="OnMenuCreated"
        ├── Opened="ColumnMenuOpenedHandler"
        ├── ItemSelected="ItemSelected"
        ├── OnClose="OnCloseMenu"
        └── Closed="ColumnMenuClosedHandler"
```

### Toolbar menu item set
- `PdfExport`
- `ExcelExport`
- `CsvExport`
- `Print`
- `ColumnChooser`

### Why this belongs in the skill
- It reuses `ColumnMenuClass` hide/show behavior.
- It is rendered only in adaptive layouts.
- It participates in the user-visible "menu" ecosystem around the grid.
- It launches `ColumnChooserComp`, so it affects cross-feature interaction documentation.

---

## Menu Item Generation Rules
<!-- token-budget: 250 words -->

### Default top-level item order
`ColumnMenu.razor` generates the default top-level items in this sequence:

```text
AutoFitAll
AutoFit
[Group, Ungroup]          only if AllowGrouping
[SortAscending, SortDescending] only if AllowSorting
ColumnChooser
[Filter]                  only if AllowFiltering && FilterType != FilterBar
```

### Custom item source
If `Parent.ColumnMenuItems` is supplied:
- string values are mapped to built-in item factories
- `ColumnMenuItemModel` values are converted to `MenuItem`
- `Hide=true` is applied later during open processing

### ColumnChooser submenu generation
The `ColumnChooser` item receives submenu items from `createChooserItems()`.
A column is included only when:
- `ShowInColumnChooser == true`
- `Field` is non-empty

The standard submenu checkbox template further narrows practical interaction to columns where chooser checkbox behavior is meaningful.

### Disabled-state computation
During `ColumnMenuOpenedHandler`, built-in items are disabled according to current feature state:
- `Group` disabled when grouping not allowed, already grouped, or column disallows grouping
- `Ungroup` disabled when target column is not grouped
- `SortAscending` / `SortDescending` disabled when sorting is not allowed or already active in same direction
- `Filter` disabled when filtering not allowed, column disallows filtering, or `FilterBar` mode is used
- `AutoFit` / `AutoFitAll` remain enabled in current implementation

### Hidden-item computation
Custom `ColumnMenuItemModel.Hide == true` causes later `ShowItems(..., false)` processing during open.

---

## Standard Open Flow
<!-- token-budget: 260 words -->

```text
Header click or keyboard open
    └── ColumnMenuUpdate event
            └── ColumnMenuUpdate(object args)
                    ├── targetColumn = Parent.ColumnMenuColumn
                    ├── if adaptive -> IsColumnMenuIconClicked = true
                    └── StateHasChanged()

Then if standard popup instance opens:
    └── ColumnMenuOpenedHandler(args)
            ├── detect initial open by checking hidden css class
            ├── if initial open
            │   ├── call JS sfBlazor.Grid.renderColumnMenu(dataId, uid, isFilter, key)
            │   ├── compute Left / Top
            │   └── build ColumnMenuOpenEventArgs
            │
            ├── if GridEvents.OnColumnMenuOpen delegate exists
            │   ├── invoke event for top-level open or submenu open
            │   └── stop if args.Cancel
            │
            ├── if initial open
            │   ├── copy possibly customized Items back to MenuItems
            │   ├── compute disabled items
            │   ├── compute hidden items
            │   ├── apply EnableItems / ShowItems mutations
            │   ├── open popup at computed Left / Top
            │   ├── switch ColumnMenuClass to visible css
            │   └── StateHasChanged()
            │
            └── else if opening ColumnChooser submenu
                    └── Parent.IsColumnMenuFilter = false
```

### Important open-flow rule
`OnColumnMenuOpen` is the only public pre-open column-menu-specific hook. It can alter `Items`, `Left`, `Top`, or cancel rendering.

---

## Close and Cancel Flow
<!-- token-budget: 210 words -->

```text
Menu close request
    └── ColumnMenuclose(BeforeOpenCloseMenuEventArgs)
            ├── if Parent.IsColumnMenuFilter == true
            │   └── args.Cancel = true   // keep menu alive while filter popup is active
            ├── if IsCheckbox == true
            │   ├── args.Cancel = true   // keep submenu alive during chooser checkbox toggles
            │   └── reset IsCheckbox when parent item is ColumnChooser
            └── persist scrollHeight

After actual close
    └── ColumnMenuClosedHandler(OpenCloseMenuEventArgs)
            ├── only for top-level close (ParentItem == null)
            ├── reset ColumnMenuClass to hidden css
            ├── re-enable temporarily disabled items
            ├── re-show temporarily hidden items
            ├── clear DisableItems and HiddenItems caches
            ├── reset filter flags
            └── Parent.ColumnMenuColumn = null
```

### Filter-driven close helpers
When filter renderer decides menu host should close:
- `Filter.HideColumnMenuPopup()` sets hidden CSS, clears `IsColumnMenuFilter`, and closes the popup instance.
- filter renderers also reset menu CSS after their own apply/cancel flows.

---

## Column Chooser Submenu Path
<!-- token-budget: 260 words -->

The standard Column Menu contains a special `ColumnChooser` top-level item. Its submenu rows render checkboxes inside the menu template.

```text
Top-level ColumnChooser item
└── submenu row per eligible column
    └── <SfCheckBox Checked="column.Visible"
                    ValueChange="CheckboxChange(args, column)">
```

### Chooser submenu behavior
- The submenu is generated by `createChooserItems()`.
- Checkbox rows are recognized using `isChooserItem()`.
- `IsCheckbox` prevents the menu from closing while interacting with chooser checkboxes.

### Checkbox change flow

```text
CheckboxChange(args, column)
    ├── guard against hiding the last visible chooser column in WASM edge path
    ├── Parent.IsColumnHeaderChange = true
    ├── Parent.ShowHideEvent = false
    ├── Parent.RenderColumnChooser = true
    ├── if checked -> ShowColumnAsync(column.Field, "field")
    ├── else      -> HideColumnAsync(column.Field, "field")
    ├── if render flag was reset during operation
    │       └── restore column visibility state and stop
    ├── if OnActionComplete / ColumnVisibilityChanged / TreeGrid path present
    │       ├── invoke OnActionComplete(RequestType = ColumnState)
    │       └── invoke ColumnVisibilityChanged with current visible/hidden columns
    ├── Parent.RenderColumnChooser = false
    ├── Parent.ShowHideEvent = true
    └── Parent.InvokeViewRefresh(refCols)
```

### Critical event note
This direct submenu checkbox path raises **post-change** events (`OnActionComplete`, `ColumnVisibilityChanged`) but does **not** run the full chooser-dialog pre-commit sequence.

---

## Filter Integration Flow
<!-- token-budget: 320 words -->

Column Menu filter handling is a cross-component workflow.

### Opening intent
Selecting the `Filter` menu item in standard mode does not immediately close the menu:

```text
ColumnMenuItemClick(item = Filter)
    └── Parent.IsColumnMenuFilter = true
```

### Hover/keyboard bridge
JS attaches listeners to the menu popup for filter submenu behavior:

```text
scripts/column-menu.ts renderColumnMenu(..., isFilter, key)
    └── if isFilter
            ├── EventHandler.add(element, 'mouseover', appendFilter)
            └── EventHandler.add(element, 'keydown', appendFilter)
```

`appendFilter()` decides whether filter UI should remain shown and invokes:
- `FilterMouseOverHandler(uid, showDialog)` on `.NET`

### .NET side reaction
`GridJSInteropAdaptor.FilterMouseOverHandler()`:
- resolves the column by `uid`
- sets `FilterModule.FilterIconIsClicked`
- sets `FilterModule.IsColumnMenuFilter`
- sets `FilterModule.FilterIconColumn`
- triggers `FilterComponentUpdate`

### Popup positioning
Filter renderers then use `sfBlazor.Grid.filterPopupRender(...)` with `isColumnMenu=true` to position the filter popup relative to the active menu host.

### Prevent close while filter popup is active
Two layers keep the menu alive:

1. `ColumnMenuclose()` cancels close when `Parent.IsColumnMenuFilter` is true.  
2. `scripts/filter.ts` calls `PreventColumnMenuClose(true)` through JS interop while menu-hosted filtering is active.

### Cleanup paths
When filter popup completes or cancels:
- `Filter.HideColumnMenuPopup()` resets menu host CSS and closes popup
- `FilterMenuRenderer.razor`, `FilterCheckBoxRenderer.razor.cs`, and `ExcelBase.razor` also trigger close/reset and adaptive cleanup events where required

### Practical implication
Column Menu and Filter should be treated as a shared popup stack, not as isolated components.

---

## Item Click Action Map
<!-- token-budget: 180 words -->

### Standard mode `ColumnMenuItemClick`

| Item | Action |
|---|---|
| `AutoFit` | `AutoFitColumnsAsync(new[] { targetColumn.Field })` |
| `AutoFitAll` | `AutoFitColumnsAsync(new string[] { })` |
| `Group` | `GroupColumnAsync(targetColumn.Field)` |
| `Ungroup` | `UngroupColumnAsync(targetColumn.Field)` |
| `SortAscending` | `SortColumnAsync(targetColumn.Field, Ascending)` |
| `SortDescending` | `SortColumnAsync(targetColumn.Field, Descending)` |
| `Filter` | sets `Parent.IsColumnMenuFilter = true` |
| chooser checkbox item | handled through `CheckboxChange()` |
| custom item | no built-in action; forwarded through click event |

After built-in processing, `ColumnMenuItemClicked` is raised with a `ColumnMenuClickEventArgs` payload.

### Adaptive header mode
Adaptive header rows call dedicated methods:
- `AutoFitItemClicked()`
- `AutoFitAllItemClicked()`
- `GroupItemClicked()`
- `UnGroupItemClicked()`
- `SortAscendingClicked()`
- `SortDescendingClicked()`
- `FilterItemClicked()`
- `ColumnsItemClicked()`

---

## Public and Internal Events
<!-- token-budget: 340 words -->

### Public events

| Event | Where fired | Purpose |
|---|---|---|
| `OnColumnMenuOpen` | `ColumnMenuOpenedHandler()` | customize menu items/position and optionally cancel top-level or submenu open |
| `ColumnMenuItemClicked` | `ColumnMenuItemClick()` | inspect clicked item, column, DOM element, and event args |
| `OnActionComplete(RequestType = ColumnState)` | chooser checkbox path | post-change notification after direct column show/hide from submenu |
| `ColumnVisibilityChanged` | chooser checkbox path | reports resulting visible/hidden columns after submenu checkbox change |

### Public event payloads
- `ColumnMenuOpenEventArgs`
  - `Cancel`
  - `Column`
  - `Items`
  - `Left`
  - `Top`
  - `ParentItem`
  - `ColumnMenuIndex`
- `ColumnMenuClickEventArgs`
  - `Column`
  - `Element`
  - `Event`
  - `Item`
- `ColumnMenuItemModel`
  - `Hide`
  - `IconCss`
  - `Id`
  - `Items`
  - `Separator`
  - `Text`
  - `Url`

### Internal signals / state transitions

| Internal event / state | Producer / consumer | Purpose |
|---|---|---|
| `ColumnMenuUpdate` | producer: header click/focus handler; consumer: `ColumnMenu.razor` | updates target column and toggles adaptive icon mode |
| `CloseColumnMenuAdaptiveDialog` | filter renderers -> `ColumnMenu.razor` | closes adaptive header menu dialog after filter actions |
| `FilterComponentUpdate` | JS adaptor / filter renderers | refreshes filter-related adaptive/menu state |
| `Parent.ColumnMenuColumn` | header/focus -> menu renderer | identifies current target column |
| `Parent.ColumnMenuInstance` | menu renderer -> rest of grid | shared reference for open/close/reset |
| `Parent.ColumnMenuClass` | lifecycle/open/close/filter paths | CSS-based hidden/visible state |
| `Parent.IsColumnMenuFilter` | click/filter interop/close logic | prevents premature menu close while filter popup is active |
| `IsCheckbox` | menu renderer local state | prevents close while chooser checkbox submenu is being used |
| `IsColumnMenuIconClicked` | adaptive header path local state | gates fullscreen adaptive dialog render |

---

## Event Sequence Diagrams (ASCII)
<!-- token-budget: 260 words -->

### A. Standard header open

```text
User clicks header menu icon
    └── GridHeaderCell.ColumnMenuIconClick(column)
            ├── Parent.ColumnMenuColumn = column
            ├── Trigger("ColumnMenuUpdate")
            └── ColumnMenuInstance.OpenAsync(1,1)
                    └── ColumnMenuOpenedHandler()
                            ├── JS renderColumnMenu(...) => Left/Top
                            ├── OnColumnMenuOpen(args)
                            ├── apply hidden/disabled/customized items
                            └── open popup at computed position
```

### B. Standard item click

```text
User selects menu item
    └── ColumnMenuItemClick(args)
            ├── run built-in action if known
            ├── set IsColumnMenuFilter when Filter chosen
            └── ColumnMenuItemClicked(public args)
```

### C. Column chooser submenu checkbox

```text
User toggles chooser checkbox
    └── CheckboxChange(args, column)
            ├── ShowColumnAsync / HideColumnAsync
            ├── OnActionComplete(ColumnState)
            ├── ColumnVisibilityChanged
            └── InvokeViewRefresh()
```

### D. Filter hosted from menu

```text
User opens Filter from column menu
    ├── ColumnMenuItemClick -> Parent.IsColumnMenuFilter = true
    ├── JS hover bridge -> FilterMouseOverHandler(uid, showDialog)
    ├── Filter popup positioned via filterPopupRender(..., isColumnMenu=true)
    └── menu close canceled while filter popup is active
```

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 190 words -->

| Combination | Must Preserve | Risk |
|---|---|---|
| Column Menu + Filtering | Filter popup must anchor correctly, keep menu alive during interaction, and fully reset menu CSS/state on close | Critical |
| Column Menu + FilterType.FilterBar | `Filter` item must be disabled or omitted according to current path because popup filtering is not supported through FilterBar mode | Critical |
| Column Menu + Column Chooser submenu | Checkbox submenu must not prematurely close; show/hide operations must refresh columns and raise post-change events | Critical |
| Column Menu + Adaptive header UI | Fullscreen dialog path must render responsive rows, dedicated actions, and chooser/filter launchers correctly | Critical |
| Column Menu + Adaptive toolbar menu | Toolbar adaptive menu must keep independent item model while reusing menu CSS conventions and chooser launch behavior | High |
| Column Menu + Sorting | sort items must disable correctly when the same direction is already active or sorting is disallowed | High |
| Column Menu + Grouping | group/ungroup items must reflect grouped state and per-column grouping permissions | High |
| Column Menu + Per-column `ShowColumnMenu=false` | icon must not render for excluded columns even when grid-level menu is enabled | High |
| Column Menu + Keyboard navigation | focus-driven open path must mirror click behavior and target the same header column | High |
| Column Menu + Column visibility / chooser | `ShowInColumnChooser=false` must remove only chooser submenu entry, not the header menu icon itself | Medium |
| Column Menu + RTL | popup positioning logic must anchor from left rather than right in standard mode | Medium |
| Column Menu + TreeGrid integration | click events and post-change visibility events may be rerouted through event aggregator notifications | Medium |

---

## JavaScript Interop
<!-- token-budget: 120 words -->

Column Menu uses bidirectional JavaScript interop for popup positioning and filter submenu behavior.

### C# → JS Calls

| C# Method Location | JS Function | Purpose |
|---|---|---|
| `ColumnMenu.razor:585` | `sfBlazor.Grid.renderColumnMenu(dataId, uid, isFilter, key)` | Calculates popup position relative to header icon, attaches hover listeners for filter submenu |
| `ColumnMenu.razor:475` | `sfBlazor.Grid.customFilterDialog(dataId, dialogId)` | Sets up adaptive dialog for filter workflows |

### JS → C# Callbacks

| JS Source | C# Method | Purpose |
|---|---|---|
| `column-menu.ts` hover bridge | `GridJSInteropAdaptor.FilterMouseOverHandler(uid, showDialog)` | Signals filter popup should open when hovering over Filter menu item |
| `filter.ts` close prevention | `GridJSInteropAdaptor.PreventColumnMenuClose(IsPrevent)` | Prevents menu from closing while filter popup is active |

### TypeScript Module Structure
- `scripts/column-menu.ts:26` — `ColumnMenu.renderColumnMenu()` — position calculation and event attachment
- `scripts/sf-grid-fn.ts:56` — `columnMenuModule` property declaration
- `scripts/sf-grid-fn.ts:141` — `new ColumnMenu(this)` instantiation
- `scripts/sf-grid.ts:259` — public export wrapper for Blazor invocation

**CRITICAL**: Always reference TypeScript source files (`.ts`), never compiled JavaScript bundles (`.js`).

---

## Quick DOM Landmark Reference
<!-- token-budget: 150 words -->

| Element | Class / Id | Meaning |
|---|---|---|
| Header menu icon | `.e-columnmenu` | clickable header affordance for column menu |
| Standard menu host | `.e-{GridId}-column-menu.e-grid-column-menu.e-grid-menu` | actual context menu popup root |
| Hidden standard menu host | `.e-hide-menu.e-{GridId}-column-menu.e-grid-column-menu.e-grid-menu` | off/hidden menu CSS state |
| Menu icon inside item | `.e-menu-icon` | built-in/custom item icon span |
| Submenu caret | `.e-icons.e-caret` | indicates filter or submenu expansion |
| Adaptive header dialog host | `{GridId}columnmenu` | fullscreen dialog id for adaptive header column menu |
| Adaptive menu content root | `.columndiv.columnmenudiv` | adaptive header menu body |
| Adaptive menu row | `.e-responsivecoldiv` | one responsive action row |
| Adaptive chooser row | `.e-responsivecolumndiv` | opens chooser from adaptive header menu |
| Adaptive filter row | `.e-responsivefilterdiv` | opens adaptive filter path |

---

## Prompt Template
<!-- token-budget: 320 words -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Column Menu

**Context**  
You are working on the Syncfusion Blazor DataGrid Column Menu feature. This skill covers the complete rendering model for standard desktop popup mode, adaptive fullscreen header-menu mode, and the adaptive toolbar menu path that coexists with the feature. It also covers the DOM structure, public/internal event flow, filter popup hosting, column chooser submenu behavior, and per-column enablement rules.

**Core Rules**
- Main renderer is `src/Internal/Renderer/ColumnMenu.razor`.
- Header icon render and open trigger live in `src/Internal/Renderer/GridHeaderCell.razor`.
- Root mounting lives in `src/Internal/SfGrid.razor`.
- Standard mode renders `SfContextMenu`; adaptive header mode renders `SfDialog` with authored responsive rows.
- `OnColumnMenuOpen` is raised before the popup or submenu is shown and can cancel the operation.
- `ColumnMenuItemClicked` fires after an item is selected.
- Menu-hosted filter behavior depends on `IsColumnMenuFilter`, JS hover callbacks, and filter popup repositioning.
- `ShowColumnMenu` is enforced at both grid and column levels.
- `ColumnChooser` inside the standard menu is a submenu with checkbox template rows, not a standalone dialog.
- Direct chooser submenu checkbox changes call show/hide column APIs immediately and raise post-change visibility events.
- JS interop: C# calls `sfBlazor.Grid.renderColumnMenu()` for positioning; JS calls `FilterMouseOverHandler()` and `PreventColumnMenuClose()` for filter integration.
- Always reference TypeScript sources (`.ts`), never compiled JavaScript (`.js`).

**Public Event Sequence**
1. Header click/focus selects `ColumnMenuColumn`.
2. `ColumnMenuOpenedHandler()` computes position and builds `ColumnMenuOpenEventArgs`.
3. `OnColumnMenuOpen(args)` may alter items/position or cancel.
4. Menu item selection runs built-in action if applicable.
5. `ColumnMenuItemClicked(args)` fires.
6. If chooser checkbox path is used, `OnActionComplete(ColumnState)` and `ColumnVisibilityChanged` may follow.

**Cross-feature checks before editing**
- Verify standard vs adaptive header render path.
- Verify toolbar adaptive menu path if adaptive UI is involved.
- Verify filter popup behavior from menu path.
- Verify chooser submenu checkboxes stay open and refresh columns correctly.
- Verify per-column `ShowColumnMenu=false` behavior.
- Verify disabled-state logic for sort/group/filter items.
- If DOM changes, compare wrapper classes and ids in both standard and adaptive paths.

---

## Quick Reference: Mode Comparison

| Aspect | Standard Header Menu | Adaptive Header Menu | Adaptive Toolbar Menu |
|---|---|---|---|
| Host component | `SfContextMenu` | `SfDialog` | `SfContextMenu` |
| Primary source file | `ColumnMenu.razor` | `ColumnMenu.razor` | `AdaptiveDialogRenderer.razor` |
| Entry trigger | header icon / keyboard | header icon in adaptive UI | toolbar item |
| Positioning | JS `renderColumnMenu()` | fullscreen dialog | open at `(1,1)` then JS adjust |
| Item rendering | menu items + template | explicit responsive `<div>` rows | menu items |
| Submenus | yes | no tree submenu | no tree submenu |
| Filter handling | hosted popup | launches adaptive filter workflow | none |
| Chooser integration | submenu checkboxes | row launches chooser dialog | item launches chooser dialog |
| Close guard | `IsColumnMenuFilter`, `IsCheckbox` | dialog close button / filter close hooks | menu close events |
| Main DOM landmark | `.e-{GridId}-column-menu` | `{GridId}columnmenu` | `.e-{GridId}-column-menu` |
