---
name: column-chooser-skill
description: Expert knowledge for the Column Chooser feature in the Syncfusion Blazor DataGrid. Use this skill when implementing or debugging column chooser rendering, DOM structure, template composition, adaptive dialog flow, event sequencing, and cross-feature column visibility interactions.
---

# Column Chooser Skill
<!-- token-budget: 20 words -->

**Purpose**  
Authoritative reference for Column Chooser rendering, DOM structure, dialog flow, template composition, visibility state transitions, and all related public/internal events in `SfGrid<TValue>`.

---

**Agent Invocation**  
Load this skill when implementing Column Chooser UI, tracing how it renders in normal or adaptive mode, debugging checkbox/list/template behavior, or fixing regressions in column visibility workflows triggered from toolbar, column menu, adaptive UI, or `OpenColumnChooserAsync()`.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `.github/skills/virtualization-dom-structure-skill/SKILL.md` — reference pattern for DOM-structure-focused skill writing
- `.github/skills/filtering-skill/SKILL.md` — reference structure for interaction matrix and prompt style
- `src/Internal/SfGrid.razor` — root mounting point for `ColumnChooser`
- `src/Internal/Renderer/ColumnChooser.razor` — core standard/adaptive chooser renderer and state machine
- `src/GridColumnChooserItem.razor` — template helper for one chooser row / select-all row
- `src/GridColumnChooserItemGroup.razor` — template helper for grouped chooser sections
- `src/GridColumnChooserSettings.cs` — chooser settings, templates, operator, text-wrap
- `src/SfGrid.Methods.cs` — `OpenColumnChooserAsync()` entry point
- `src/SfGrid.Properties.cs` — `ShowColumnChooser`, `ColumnChooserSettings`
- `src/SfGrid.razor.cs` — `RenderColumnChooser` internal toggle state
- `src/GridColumn.cs` — `ShowInColumnChooser`
- `src/GridEvents.cs` — `BeforeOpenColumnChooser`, `ColumnVisibilityChanging`, `ColumnVisibilityChanged`
- `src/EventModels/Grids.cs` — chooser template/footer contexts and `ColumnChooserEventArgs`
- `src/Internal/Base/InternalClass.cs` — `IColumnChooser` contract and locale key
- `src/Internal/Renderer/ColumnMenu.razor` — Column Menu integration path and sub-item checkbox flow
- `src/Internal/Renderer/AdaptiveDialogRenderer.razor` — adaptive toolbar path that launches chooser
- `scripts/column-chooser.ts` — JS module for chooser positioning and media-column visibility sync
- `tests/Actions/ColumnChooser.razor` — standard and adaptive chooser tests
- `tests/Actions/ColumnChooserTemplate.razor` — template rendering test coverage

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- Column Chooser is not only a popup; it is a column-visibility transaction pipeline with preview state (`ColumnVisible`) before commit.
- Render path splits into **standard dialog** and **adaptive fullscreen dialog**, but both reuse the same checkbox/search/content model.
- `Template` replaces the default list body only; `FooterTemplate` replaces footer only for the non-adaptive layout.
- `BeforeOpenColumnChooser` is the only public pre-open chooser-specific hook; visibility commit itself flows through generic column-state events.
- Internal state uses `showColumns` and `hideColumns` Uid lists as the commit source of truth.
- Search filters chooser items by `HeaderText`, not by field name.
- `ShowInColumnChooser=false` excludes a column from chooser UI but does not prevent normal column visibility logic elsewhere.

---

## Code Location Map
<!-- token-budget: 80 words -->

- `src/Internal/SfGrid.razor` — mounts `ColumnChooser` when `ShowColumnChooser` or adaptive column-menu path is active
- `src/Internal/Renderer/ColumnChooser.razor` — full dialog markup (lines 1-750+), search box, checkbox list, template slots, footer buttons, event wiring, commit/cancel flow
- `src/GridColumnChooserItem.razor` — reusable chooser checkbox row for template authors
- `src/GridColumnChooserItemGroup.razor` — reusable grouping wrapper for template authors
- `src/GridColumnChooserSettings.cs` — public settings: `Operator`, `Template`, `FooterTemplate`, `AllowTextWrap`
- `src/SfGrid.Methods.cs` line 2074 — `OpenColumnChooserAsync()` method toggles `RenderColumnChooser`, sets `ShowChooser`, triggers `ColumnChooserComp`
- `src/SfGrid.razor.cs` line 137 — `RenderColumnChooser` internal mount/open toggle backing field
- `src/SfGrid.Properties.cs` line 1134 — `ShowColumnChooser` property; line 308 — `ColumnChooserSettings` property
- `src/GridColumn.cs` line 630 — per-column `ShowInColumnChooser` property
- `src/GridEvents.cs` line 2349 — `BeforeOpenColumnChooser` event; line ~783 — `ColumnVisibilityChanging`; line 806 — `ColumnVisibilityChanged`
- `src/EventModels/Grids.cs` line 1475 — `ColumnChooserTemplateContext`; line 1486 — `ColumnChooserFooterTemplateContext`; line 1511 — `ColumnChooserEventArgs`
- `src/Internal/Base/InternalClass.cs` line 460 — `IColumnChooser` interface contract
- `src/Internal/Renderer/ColumnMenu.razor` — column-menu chooser path and direct checkbox sub-menu handling
- `src/Internal/Renderer/AdaptiveDialogRenderer.razor` — adaptive toolbar launch path
- `scripts/column-chooser.ts` line 26 — `renderColumnChooser()` method; line 35 — `setMediaColumns()` method for media query visibility sync

---

## Render Entry Points
<!-- token-budget: 120 words -->

Column Chooser can be opened from four distinct launch paths:

1. **Grid toolbar "ColumnChooser" button**  
	 Grid toolbar action eventually toggles `RenderColumnChooser` and triggers chooser rendering.

2. **Public API: `OpenColumnChooserAsync(x?, y?)`**  
	 Sets `RenderColumnChooser = !RenderColumnChooser`, `ShowChooser = true`, and fires `EventAggregator.Trigger("ColumnChooserComp", chooserpositions)`.

3. **Column Menu -> ColumnChooser item**  
	 `ColumnMenu.razor` toggles `Parent.RenderColumnChooser`, then participates in direct per-item column visibility changes when submenu checkboxes are used.

4. **Adaptive dialog toolbar path**  
	 `AdaptiveDialogRenderer.razor` handles the `"ColumnChooser"` action, toggles chooser state, and triggers `ColumnChooserComp`.

The component itself is mounted from `SfGrid.razor` whenever `ShowColumnChooser || (ShowColumnMenu && EnableAdaptiveUI)` is true.

---

## Component Decision Flow (ASCII)
<!-- token-budget: 140 words -->

```text
SfGrid.razor chooser selection
══════════════════════════════════════════════════════════════════════

ShowColumnChooser || (ShowColumnMenu && EnableAdaptiveUI)
				│
				└── render <ColumnChooser TContent="TValue">
								│
								├── waits for RenderColumnChooser + ShowChooser/open trigger
								│
								├── if NOT adaptive layout target
								│       │
								│       └── Standard chooser dialog
								│             ├── SfDialog width=250
								│             ├── optional search box
								│             ├── default checkbox list OR custom Template
								│             └── default footer buttons OR custom FooterTemplate
								│
								└── else if adaptive chooser icon path active
												│
												└── Adaptive fullscreen chooser dialog
															├── SfDialog width=100% height=100%
															├── close icon + Apply button in header
															├── optional search box
															├── default checkbox list OR custom Template
															└── no footer-template injection path

══════════════════════════════════════════════════════════════════════
```

---

## Standard Dialog DOM Structure
<!-- token-budget: 260 words -->

```text
ColumnChooser<TContent>
└── <div> root event-shield
		└── <div id="{GridId}_columnChooserDialog">
				└── <SfDialog id="{GridId}_ccdlg" class="e-ccdlg" width="250">
						├── <Header>
						│   └── "Choose Columns"
						│
						├── <Content>
						│   └── <div class="e-main-div e-cc">
						│       │
						│       ├── [default content path only]
						│       │   └── <div class="e-cc-searchdiv e-cc e-input-group {e-input-focus?}">
						│       │       ├── <input id="{GridId}_CCSearchBox"
						│       │       │         class="e-ccsearch e-cc e-input">
						│       │       └── <span class="e-ccsearch-icon e-icons e-cc e-input-group-icon {e-cc-cancel?}">
						│       │
						│       └── <div class="e-cc-contentdiv">
						│           ├── if Columns.Count == 0
						│           │   └── <div class="e-innerdiv e-cc e-ccnmdiv">
						│           │       └── <span class="e-cc e-nmatch">No matches</span>
						│           │
						│           └── else
						│               └── <div class="e-innerdiv e-cc">
						│                   ├── default list path
						│                   │   └── <ul class="e-ccul-ele e-cc">
						│                   │       ├── <li class="e-cclist e-cc e-cc-selectall e-cclist-hidden">
						│                   │       │   └── <div class="e-ccheck" uid="grid-selectAll">
						│                   │       │       └── <CheckBoxRenderer class="e-choosercheck ... e-selectall">
						│                   │       │
						│                   │       └── <li class="e-cclist e-cc e-cclist-hidden" id="e-ccli_{Uid}"> × N
						│                   │           └── <div class="e-ccheck" uid="{Uid}">
						│                   │               └── <CheckBoxRenderer class="e-choosercheck ...">
						│                   │
						│                   └── template path
						│                       └── <CascadingValue Value="this">
						│                           └── ColumnChooserSettings.Template(ColumnChooserTemplateContext)
						│
						└── <DialogButtons> [default footer only]
								├── OK button class="e-cc e-cc_okbtn"
								└── Cancel button class="e-flat e-cc e-cc-cnbtn"
```

**Standard layout notes**
- Root wrapper stops mouse propagation so outside grid handlers do not immediately close the chooser.
- Search box exists only when `Template == null`.
- Footer buttons exist only when `FooterTemplate == null`.
- `FooterTemplate` is injected through `CCDialog.FooterTemplates = FooterFragment`, not through static markup.

---

## Adaptive Dialog DOM Structure
<!-- token-budget: 240 words -->

```text
ColumnChooser<TContent>
└── <div> root event-shield
		└── <div id="{GridId}_adaptive_columnChooserDialog">
				└── <SfDialog id="{GridId}_ccdlg"
											class="e-bigger e-responsive-dialog e-dlg-fullscreen e-ccdlg"
											width="100%" height="100%">
						├── <Header>
						│   ├── <SfButton class="e-dlg-closeicon-btn"> close </SfButton>
						│   └── <div class="e-dlg-header">
						│       └── <div class="e-res-custom-element">
						│           ├── <div class="e-dlg-custom-header" id="{GridId}_columnchooser_dialog_wrapper">
						│           │   └── "Choose Columns"
						│           └── <SfButton class="e-primary e-flat e-res-apply-btn"> OK </SfButton>
						│
						└── <Content>
								└── <div class="e-maincolumnchooserdiv e-mainfilterdiv e-default-filter"
													id="columchooser-main-filter">
										└── <div class="e-main-div e-cc">
												├── [default content path only]
												│   └── <div class="e-cc-searchBox">
												│       └── <div class="e-cc-searchdiv e-cc e-input-group {e-input-focus?}">
												│           ├── <input class="e-ccsearch e-cc e-input">
												│           └── <span class="e-ccsearch-icon e-icons e-cc e-input-group-icon {e-cc-cancel?}">
												│
												└── <div class="e-cc-contentdiv">
														├── empty state → `.e-ccnmdiv > .e-nmatch`
														└── non-empty state
																└── `.e-innerdiv.e-cc`
																		├── default `<ul class="e-ccul-ele e-cc">` list
																		└── or custom `Template(ColumnChooserTemplateContext)`
```

**Adaptive layout notes**
- Adaptive chooser renders only when `IsColumnChooserIconClicked` is true and adaptive mode conditions match device mode.
- Apply action lives in the header, not in a footer region.
- `FooterTemplate` is not used in adaptive fullscreen path.
- `DialogEvents` wire both `Closed="ColumnChooserClosed"` and `Created="Rendered"`.

---

## Template Composition Model
<!-- token-budget: 200 words -->

There are three authoring modes:

### 1. Default chooser list
- Search input
- Select-all row
- One checkbox row per eligible column
- Default OK / Cancel footer buttons

### 2. Custom content template
`GridColumnChooserSettings.Template` replaces the inner chooser body list.

```text
ColumnChooser.razor
└── CascadingValue(this as IColumnChooser)
		└── Template(new ColumnChooserTemplateContext { Columns = filteredColumns })
```

Template authors can use:
- `GridColumnChooserItem`
- `GridColumnChooserItemGroup`
- `ColumnChooserTemplateContext.Columns`

### 3. Custom footer template
`GridColumnChooserSettings.FooterTemplate` replaces only the footer in non-adaptive mode.

```text
FooterTemplateContext
├── Columns = UpdateColumnVisible(Columns)
└── CancelAsync = CancelBtnClick
```

**Important template rules**
- Search still filters `Columns` before passing them to the content template.
- `GridColumnChooserItem` delegates checkbox behavior back to the parent chooser through `IColumnChooser`.
- `GridColumnChooserItemGroup` is only a structural wrapper; it does not own visibility state.
- `AllowTextWrap` affects default `CheckBoxRenderer` label class via `GetLabelClassName()`, not custom template markup.

---

## Template Helper DOM Items
<!-- token-budget: 180 words -->

### `GridColumnChooserItem`

```text
IsSelectAll == true
└── <div class="e-ccheck e-cchooser-chk" uid="grid-selectAll">
		└── <CheckBoxRenderer class="e-choosercheck ... e-selectall">

IsSelectAll == false
└── <div class="e-ccheck e-cchooser-chk" uid="{Column.Uid}">
		└── <CheckBoxRenderer class="e-choosercheck ...">
```

### `GridColumnChooserItemGroup`

```text
<div>
├── optional <div class="e-headercelldiv e-headercell-border">
│   └── <span class="e-label">{Title}</span>
└── <CascadingValue Value="this">
		└── ChildContent
```

These components are intended for **custom chooser templates**, not for the built-in default chooser list.

---

## JavaScript Interop
<!-- token-budget: 180 words -->

The Column Chooser feature uses 4 JavaScript interop calls from C# to TypeScript for positioning, search behavior, frozen layout synchronization, and adaptive dialog styling.

### C# → JavaScript Calls

| C# Call Location | Method Signature | Purpose |
|---|---|---|
| `ColumnChooser.razor` line 320 | `sfBlazor.Grid.renderColumnChooser(false, dataId)` | Calculates and returns dialog position for sticky header scenarios |
| `ColumnChooser.razor` line 469, 719 | `sfBlazor.Grid.searchClear(dataId, searchBoxId)` | Clears search input value and removes cancel icon class |
| `ColumnChooser.razor` line 631 | `sfBlazor.Grid.frozenHeight(dataId, options, null!)` | Synchronizes frozen row/column heights after visibility changes |
| `ColumnChooser.razor` line 634 | `sfBlazor.Grid.customFilterDialog(dataId, dialogId)` | Applies adaptive dialog custom styling and behavior |

> **Interop arity notes (C# call vs TS definition):**
> - `renderColumnChooser` — C# passes 2 args (`false`, `dataId`), but the TS definition (`scripts/sf-grid.ts:250`) takes only `dataId`; the leading arg is ignored.
> - `frozenHeight` — C# passes 3 args with a trailing `null!`, but TS (`scripts/sf-grid.ts:142`) accepts 2; the 3rd is safely ignored.
> - `customFilterDialog` — TS (`scripts/sf-grid.ts:54`) declares 3 params (incl. `isExcel`), C# passes 2; the missing arg defaults to `undefined`.

### TypeScript Implementation

| TypeScript Location | Function Signature | Responsibility |
|---|---|---|
| `scripts/column-chooser.ts` line 26 | `renderColumnChooser(): OffsetPosition` | Returns calculated position; adds sticky class to dialog element |
| `scripts/column-chooser.ts` line 35 | `setMediaColumns(isResetPersistData?: boolean): void` | Manages media query visibility synchronization for HideAtMedia columns |

**Responsibility Split**
- **C# (ColumnChooser.razor):** Dialog lifecycle, checkbox state, preview visibility (`ColumnVisible`), search filtering, commit logic, event firing
- **TypeScript (column-chooser.ts):** Dialog positioning for sticky headers, media query column visibility tracking, persistent storage updates

### CRITICAL Rules
1. `renderColumnChooser` is only called when `EnableStickyHeader && !EnableAdaptiveUI`
2. `searchClear` is called on cancel icon click and dialog closed events
3. `frozenHeight` must run after column visibility commit when frozen columns/rows exist
4. `customFilterDialog` is only called in adaptive mode during `DialogEvents.Created`

---

## Render / State Flow
<!-- token-budget: 260 words -->

```text
Open trigger
	↓
RenderColumnChooser toggled + ShowChooser = true
	↓
EventAggregator.Trigger("ColumnChooserComp", optionalPosition)
	↓
ColumnChooserComp()
	├── IsVisible = true
	├── capture XPosition/YPosition if supplied
	├── inject FooterTemplate for non-adaptive path
	├── else set IsColumnChooserIconClicked = true
	├── OnParametersSet()
	└── StateHasChanged()
	↓
OnAfterRenderAsync()
	├── if sticky header and non-adaptive -> JS `sfBlazor.Grid.renderColumnChooser`
	└── OpenColumnChooser(XPosition, YPosition)
	↓
OpenColumnChooser()
	├── initialize search operator from settings
	├── invoke BeforeOpenColumnChooser
	├── if canceled -> hide dialog and stop
	├── set initial button/select-all state
	├── show dialog
	└── snapshot visible/hidden Uids into showColumns/hideColumns
```

### Preview state model

Before clicking OK, chooser edits **preview visibility** through `GridColumn.ColumnVisible`, while actual committed visibility still lives in `GridColumn.Visible`.

```text
Visible                = committed grid state
ColumnVisible          = chooser preview checkbox state
showColumns/hideColumns = Uid-based commit lists used by OK action
```

### Search flow

```text
input -> InputArgs()
			├── empty input -> restore eligible Columns
			└── non-empty -> DataOperations.PerformFiltering(CCColumns, WhereFilters, searchOpt)
											using Field = "HeaderText"
```

### Commit flow

```text
OK click -> OnBtnClick()
				 ├── build visible/hidden column lists from showColumns/hideColumns
				 ├── fire OnActionBegin(ColumnState)
				 ├── fire ColumnVisibilityChanging
				 ├── cancel if either args.Cancel
				 ├── ShowColumnsOperation(...HeaderText...)
				 ├── HideColumnsOperation(...HeaderText...)
				 ├── fire OnActionComplete(ColumnState)
				 ├── fire ColumnVisibilityChanged
				 ├── hide dialog and reset chooser state
				 └── refresh view / frozen height when needed
```

---

## Public and Internal Event Paths
<!-- token-budget: 260 words -->

### Public API / public events

| Event / Method | Where fired | Purpose |
|---|---|---|
| `OpenColumnChooserAsync(x?, y?)` | `SfGrid.Methods.cs` | Programmatically opens chooser and optionally sets popup position |
| `BeforeOpenColumnChooser` | `ColumnChooser.OpenColumnChooser()` | Last public hook before dialog shows; can cancel and change search operator |
| `OnActionBegin` with `RequestType = ColumnState` | `ColumnChooser.OnBtnClick()` | Generic grid action-begin event for chooser commit |
| `ColumnVisibilityChanging` | `ColumnChooser.OnBtnClick()` | Specific pre-commit visibility-change event; cancelable |
| `OnActionComplete` with `RequestType = ColumnState` | `ColumnChooser.OnBtnClick()` | Generic post-commit event |
| `ColumnVisibilityChanged` | `ColumnChooser.OnBtnClick()` | Specific post-commit visibility change event |

### Internal chooser-specific callbacks

| Internal event | Producer / consumer | Purpose |
|---|---|---|
| `ColumnChooserComp` | producer: grid open paths; consumer: `ColumnChooser.OnInitialized()` | Brings chooser to visible state and applies optional position |
| `HideColumnChooser` | consumer: `OutSideClickHandler()` | Closes chooser on outside interaction |
| `CloseColumnMenuAdaptiveDialog` | producer: chooser commit | Closes adaptive column-menu host after chooser action |
| `FilterComponentUpdate` | producer: adaptive close/closed path | Refreshes adaptive overlay ecosystem after chooser close |

### Dialog lifecycle callbacks

| Callback | Path | Notes |
|---|---|---|
| `ColumnChooserClosed()` | `DialogEvents.Closed` | clears cancel icon, adaptive state, and reset path |
| `Rendered()` | adaptive `DialogEvents.Created` | calls JS `customFilterDialog` for adaptive dialog behavior |
| `CloseHandler()` | adaptive header close button | hides dialog and resets adaptive chooser state |
| `CancelBtnClick()` | standard footer cancel | delegates to `HideDialog()` |

---

## Event Sequence Diagrams (ASCII)
<!-- token-budget: 220 words -->

### A. Programmatic open

```text
Caller
	└── grid.OpenColumnChooserAsync(x, y)
				├── RenderColumnChooser = !RenderColumnChooser
				├── ShowChooser = true
				└── EventAggregator.Trigger("ColumnChooserComp", positions)
							└── ColumnChooserComp()
										└── OnAfterRenderAsync()
													└── OpenColumnChooser()
																├── BeforeOpenColumnChooser(args)
																└── CCDialog.ShowAsync()
```

### B. Commit visibility change

```text
User clicks OK / Apply
	└── OnBtnClick()
				├── collect sColumn / hColumn
				├── OnActionBegin(ColumnState)
				├── ColumnVisibilityChanging
				├── [cancel? stop]
				├── ShowColumnsOperation()
				├── HideColumnsOperation()
				├── OnActionComplete(ColumnState)
				├── ColumnVisibilityChanged
				├── CCDialog.HideAsync()
				└── InvokeViewRefresh()
```

### C. Cancel / outside close

```text
Cancel button / close icon / outside click
	└── HideDialog() or OutSideClickHandler()
				├── restore committed `Visible` state when needed
				├── hide dialog
				└── clearValues()
```

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->

| Combination | Must Preserve | Risk |
|---|---|---|
| Column Chooser + Column Menu | Both launch paths must remain valid; column-menu chooser item and submenu checkbox path must not desynchronize `Visible`/`ColumnVisible` state | Critical |
| Column Chooser + Adaptive UI | Adaptive fullscreen chooser must use header Apply/Close actions, correct dialog classes, and `IsColumnChooserIconClicked` gating | Critical |
| Column Chooser + Frozen Columns / Frozen Rows | After commit, `InvokeViewRefresh()` and `sfBlazor.Grid.frozenHeight` must run so header/content heights and frozen layout stay aligned | Critical |
| Column Chooser + Templates | `Template` must receive filtered `Columns`; `FooterTemplate` must receive updated preview visibility and `CancelAsync`; default DOM must disappear cleanly | High |
| Column Chooser + Toolbar API | Toolbar launch and `OpenColumnChooserAsync()` must both converge on the same chooser component and event pipeline | High |
| Column Chooser + Editing | Chooser should not commit while edit state blocks reopening path; `OnBtnClick()` guards against edit/open race with `Parent.IsEdit` and `RenderColumnChooser` conditions | High |
| Column Chooser + Search | Search must filter by `HeaderText` only and preserve correct select-all / intermediate states for filtered subset | High |
| Column Chooser + Column Virtualization / Reorder | Showing/hiding columns must refresh visible column collection and DOM without leaving stale header/body slices | High |
| Column Chooser + Selection / Paging / Sorting / Filtering / Grouping | Column visibility commits must refresh grid view without breaking existing row-state features that depend on stable column metadata | Medium |
| Column Chooser + HideAtMedia / Persistence | JS media visibility synchronization must not corrupt chooser-driven visible state or persisted column settings | Medium |

---

## Quick DOM Landmark Reference
<!-- token-budget: 130 words -->

| Element | Class / Id | Meaning |
|---|---|---|
| Standard chooser host | `{GridId}_columnChooserDialog` | Non-adaptive popup target container |
| Adaptive chooser host | `{GridId}_adaptive_columnChooserDialog` | Fullscreen chooser host |
| Dialog id | `{GridId}_ccdlg` | Actual Syncfusion dialog element |
| Search box | `.e-cc-searchdiv` / `.e-ccsearch` | Chooser search input area |
| Search clear icon | `.e-ccsearch-icon.e-cc-cancel` | Clear current search term |
| Content wrapper | `.e-cc-contentdiv` | Scrollable chooser list body |
| List wrapper | `.e-ccul-ele.e-cc` | Default chooser checkbox list |
| List item | `.e-cclist.e-cc` | One chooser row |
| Checkbox wrapper | `.e-ccheck` / `.e-cchooser-chk` | Click target around rendered checkbox |
| Empty state | `.e-ccnmdiv .e-nmatch` | No search matches |
| Adaptive root content | `.e-maincolumnchooserdiv` | Fullscreen adaptive body container |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Column Chooser

**Context**  
You are working on the Syncfusion Blazor DataGrid Column Chooser. This skill covers how the chooser is mounted, which dialog layout is rendered, how the default checkbox list differs from `Template` / `FooterTemplate`, which state fields represent preview vs committed visibility, and which public/internal events run before and after visibility changes.

**Core Rules**
- Component mount point is `src/Internal/SfGrid.razor`.
- Main renderer is `src/Internal/Renderer/ColumnChooser.razor`.
- Standard and adaptive chooser share the same search/list model but render different dialog shells.
- `Template` replaces the default list body. `FooterTemplate` replaces footer only for the non-adaptive path.
- Search filters `Columns` by `HeaderText` using `DataOperations.PerformFiltering`.
- Preview checkbox state uses `GridColumn.ColumnVisible`; committed grid state uses `GridColumn.Visible`.
- OK / Apply builds visible and hidden column lists from `showColumns` and `hideColumns`.

**Public Event Sequence**
1. `BeforeOpenColumnChooser` before dialog show; can cancel.
2. `OnActionBegin(RequestType = ColumnState)` before commit.
3. `ColumnVisibilityChanging` before commit; can cancel.
4. `ShowColumnsOperation` / `HideColumnsOperation` commit visibility.
5. `OnActionComplete(RequestType = ColumnState)` after commit.
6. `ColumnVisibilityChanged` after commit.

**Internal Signals**
- `ColumnChooserComp` opens and positions chooser.
- `HideColumnChooser` closes chooser from outside interaction.
- `CloseColumnMenuAdaptiveDialog` closes adaptive wrapper after commit.

**Cross-feature checks before editing**
- Verify standard vs adaptive render path.
- Verify template vs default-content path.
- Verify footer template only on non-adaptive path.
- Re-test Column Menu launch, toolbar/API launch, frozen columns/rows, search, and column visibility events.
- If change affects DOM shape, compare classes and wrapper hierarchy in both dialog modes.

---

## Quick Reference: Mode Comparison

| Aspect | Standard Chooser | Adaptive Chooser | Template Mode |
|---|---|---|---|
| Host container | `{GridId}_columnChooserDialog` | `{GridId}_adaptive_columnChooserDialog` | same as active shell |
| Dialog class | `e-ccdlg` | `e-bigger e-responsive-dialog e-dlg-fullscreen e-ccdlg` | same as active shell |
| Width / Height | `250` / auto | `100%` / `100%` | inherited from active shell |
| Search box | yes if `Template == null` | yes if `Template == null` | filtered `Columns` passed to template |
| List body | built-in `<ul>/<li>` | built-in `<ul>/<li>` | custom content via `Template(context)` |
| Select-all DOM | built-in first list item | built-in first list item | template author may use `GridColumnChooserItem IsSelectAll=true` |
| Footer buttons | default buttons unless `FooterTemplate` exists | Apply in header; no footer-template injection | custom footer only in non-adaptive mode |
| Open close hook | `DialogEvents.Closed` | `DialogEvents.Closed` + `Created` | same as active shell |
| Primary entry path | toolbar / API / column menu | adaptive toolbar / adaptive menu | same launch path, custom body |

