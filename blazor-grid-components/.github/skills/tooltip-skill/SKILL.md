---
name: tooltip-skill
description: Expert knowledge for tooltip behavior in the Syncfusion Blazor DataGrid. Use this skill for implementation or bug-fix work involving EllipsisWithTooltip, ShowTooltip, validation tooltips, DOM rendering, JS-to-.NET flow, and tooltip interaction risks with grouping, virtualization, frozen sections, editing, and text wrapping.
---

# Skill Instructions
<!-- token-budget: 30 words -->

**Purpose**  
Provide a single LLM-ready reference for every tooltip rendering path in the Blazor DataGrid, including source-file ownership, render flow, DOM structure, state transitions, and feature-combination risks.

---

## Agent Invocation
<!-- token-budget: 80 words -->
- Load this skill for any task involving grid tooltip rendering, tooltip bugs, DOM inspection, hover behavior, validation popups, or tooltip-template work.
- This skill covers three separate systems that must never be merged mentally:
  1. `EllipsisWithTooltip` — JS-created tooltip DOM for overflowed header/content cells.
  2. `ShowTooltip` — Blazor `SfTooltip` wrapper for general header/content hover.
  3. Validation tooltip — Razor-rendered error popup used during edit validation.
- When debugging, first identify which of the three systems is active.

---

## Knowledge References
<!-- token-budget: 140 words -->
All content below is derived from these files and should be cross-checked there before changing behavior:

- `scripts/tooltip.ts`
- `scripts/edit.ts`
- `scripts/sf-grid-fn.ts`
- `scripts/interfaces.ts`
- `src/Internal/SfGrid.razor`
- `src/Internal/Renderer/GridTooltip.razor`
- `src/Internal/Renderer/ValidationTooltip.razor`
- `src/Internal/Renderer/GridHeaderCell.razor`
- `src/Internal/Renderer/GridRow.CellRenderer.cs`
- `src/Internal/Base/GridColumnBase.cs`
- `src/Internal/Annotation/GridAnnotation.cs`
- `src/Internal/Base/GridJSInteropAdaptor.cs`
- `src/Internal/Actions/FocusHandler.cs`
- `src/Internal/Actions/Edit.cs`
- `src/SfGrid.Properties.cs`
- `src/EventModels/Grids.cs`
- `src/GridTemplates.cs`
- `src/Internal/Renderer/NormalEdit.razor`
- `src/Internal/Renderer/BatchEdit.razor`
- `src/Internal/Renderer/DialogEdit.razor`

---

## Tooltip Taxonomy
<!-- token-budget: 130 words -->
The DataGrid does **not** have one tooltip feature. It has **three independent tooltip pipelines**:

| Tooltip Type | Trigger | Renderer | DOM Owner | Primary File |
|---|---|---|---|---|
| `EllipsisWithTooltip` | `ClipMode.EllipsisWithTooltip` at grid or column level | TypeScript manual DOM creation | `document.body` | `scripts/tooltip.ts` |
| `ShowTooltip` | `ShowTooltip = true` | Blazor `SfTooltip` component | Popup component managed by Syncfusion Tooltip | `src/Internal/Renderer/GridTooltip.razor` |
| Validation tooltip | Edit validation failure | Razor markup + JS-provided positioning | Inline in edit row/dialog render tree | `src/Internal/Renderer/ValidationTooltip.razor` + `scripts/edit.ts` |

Core debugging rule: before fixing anything, classify the issue into one of these three lanes. Most tooltip regressions happen because a developer assumes all three use the same DOM and lifecycle.

---

## End-to-End Architecture Overview
<!-- token-budget: 220 words -->

### Master View

```text
+--------------------- Blazor Grid Tooltip Landscape ---------------------+
|                                                                         |
|  [C# render path]                     [JS runtime path]                 |
|                                                                         |
|  GridHeaderCell.razor  ----class----> e-ellipsistooltip                |
|  GridRow.CellRenderer  ----class----> e-ellipsistooltip                |
|                                         |                               |
|                                         v                               |
|                                 CustomToolTip (tooltip.ts)              |
|                                         |                               |
|                                         v                               |
|                             Manual tooltip DOM in document.body         |
|                                                                         |
|  SfGrid.razor --ShowTooltip=true--> GridTooltip.razor -> SfTooltip      |
|                                         |                               |
|                                         v                               |
|                   JS getContentCell(top,left) -> resolve row/header     |
|                                         |                               |
|                                         v                               |
|                        TooltipTemplate or plain text content             |
|                                                                         |
|  Edit validation error -> scripts/edit.ts -> ShowValidationPopup()      |
|                                         |                               |
|                                         v                               |
|                         ValidationTooltip.razor rerender                 |
|                                         |                               |
|                                         v                               |
|                     Inline tooltip DOM inside row/dialog content         |
+-------------------------------------------------------------------------+
```

### Ownership Rule
- If tooltip DOM is appended to `document.body`, it is the `EllipsisWithTooltip` path.
- If tooltip content is coming from `TooltipTemplateContext`, it is the `ShowTooltip` path.
- If tooltip contains `.e-griderror` and a `<label class="e-error">`, it is the validation path.

---

## Code Location Map
<!-- token-budget: 220 words -->

| Path | Purpose |
|---|---|
| `scripts/tooltip.ts` | `CustomToolTip` class. Wires mouse/scroll/key events, decides whether overflow exists, builds tooltip DOM manually, positions it, and destroys it. |
| `scripts/sf-grid-fn.ts` | Creates `toolTipModule`, closes tooltip during data-ready and double-click flows, updates tooltip event wiring when `clipMode` changes, exposes `getContentCell()`. |
| `scripts/edit.ts` | Computes validation tooltip location for inline, batch, virtualized, frozen, and dialog editing paths. |
| `scripts/interfaces.ts` | JS-side option metadata for `clipMode`, column description, and tooltip-related properties. |
| `src/Internal/Renderer/GridRow.CellRenderer.cs` | Adds `e-ellipsistooltip` to content cell classes when cell/grid `ClipMode` requires it. |
| `src/Internal/Renderer/GridHeaderCell.razor` | Adds `e-ellipsistooltip` to header cell classes and preserves header `e-mappinguid` lookup. |
| `src/Internal/Base/GridColumnBase.cs` | Owns column `Description`, used as header tooltip content when annotation metadata is available. |
| `src/Internal/Annotation/GridAnnotation.cs` | Converts model metadata description into `ClipMode.EllipsisWithTooltip` + `Description`. |
| `src/Internal/SfGrid.razor` | Mounts `<GridTooltip>` only when `ShowTooltip` is enabled. |
| `src/Internal/Renderer/GridTooltip.razor` | Defines `SfTooltip`, receives hover coordinates, resolves cell context, and renders plain text or `TooltipTemplate`. |
| `src/EventModels/Grids.cs` | Defines `TooltipTemplateContext`. |
| `src/GridTemplates.cs` | Defines `TooltipTemplate` surface. |
| `src/Internal/Renderer/ValidationTooltip.razor` | Renders edit validation tooltip markup with arrow direction and dynamic inline style. |
| `src/Internal/Base/GridJSInteropAdaptor.cs` | Receives JS callback `ShowValidationPopup()` (line 414) and `RemoveValidationPopup()` (line 426), publishes render refresh via EventAggregator. |
| `src/Internal/Actions/FocusHandler.cs` | Closes `ShowTooltip` tooltip instance during focus navigation (lines 142-144, 815-817). Total file: 1427 lines. |
| `src/Internal/Renderer/NormalEdit.razor` | Hosts `<ValidationTooltip>` (line 174) in normal edit UI. Total file: 318 lines. |
| `src/Internal/Renderer/BatchEdit.razor` | Hosts `<ValidationTooltip>` (line 74) in batch edit UI. Total file: 121 lines. |
| `src/Internal/Renderer/DialogEdit.razor` | Hosts `<ValidationTooltip>` (lines 138, 312) in dialog/adaptive dialog edit UI. Total file: 390 lines. |

---

## Tooltip Type 1 — EllipsisWithTooltip
<!-- token-budget: 420 words -->

### Intent
Used only when a cell/header is clipped and should reveal its full text on hover. This system is **not** backed by `SfTooltip`; it is a hand-built popup.

### Activation Inputs
- Grid-level `ClipMode = EllipsisWithTooltip`
- Column-level `GridColumn.ClipMode = EllipsisWithTooltip`
- Implicit activation from annotation metadata when `Description` exists

### Render Preparation
1. `GridRow.CellRenderer.cs` calls `ApplyClipMode()` for data cells.
2. `GridHeaderCell.razor` computes classes for header cells.
3. Both paths add `e-ellipsistooltip` to eligible DOM.
4. `SfGrid.initModules()` creates `new CustomToolTip(this)`.
5. `CustomToolTip` calls `isEllipsisTooltip()`.
6. If no column or grid uses this mode, no hover listeners are wired.

### Runtime Flow

```text
T0  Grid initialized
    |
    +--> sf-grid-fn.ts:initModules()
            |
            +--> toolTipModule = new CustomToolTip(this)
                    |
                    +--> isEllipsisTooltip() ?
                           | yes
                           v
                       wireEvents()
                       - content scroll
                       - grid mousemove
                       - grid mouseout
                       - grid keydown

T1  mousemove on grid element
    |
    +--> tooltip.ts:mouseMoveHandler(e)
            |
            +--> if allowTextWrap + wrapMode blocks tooltip => return
            |
            +--> parentsUntil(target, 'e-ellipsistooltip')
            |        |
            |        +--> null => close previous tooltip if needed
            |
            +--> if new element differs from prevElement => close old tooltip
            |
            +--> skip device hover for A/BUTTON/INPUT in mobile cases
            |
            +--> getTooltipStatus(element)
                    |
                    +--> clone/measure header or content width
                    +--> compare offsetWidth vs scrollWidth / clone width
                    +--> true only when visual overflow really exists
            |
            +--> set content
            |       - header: column.Description OR .e-headertext innerText
            |       - content: element.innerText
            |
            +--> open(element)
                    |
                    +--> close()
                    +--> create tooltip root if needed
                    +--> add aria-describedby + data-tooltip-id to target
                    +--> renderToolTip()
                    +--> setPosition(target)

T2  mouseout / scroll / Tab / Shift+Tab / dblclick / data refresh
    |
    +--> close()
```

### State Model

```text
[Disabled]
   | isEllipsisTooltip=false
   v
[No listeners]

[Enabled, Idle]
   | mousemove enters e-ellipsistooltip overflow target
   v
[Candidate Found]
   | getTooltipStatus=true
   v
[Opening]
   | renderToolTip + setPosition
   v
[Visible]
   | mouseout / scroll / Tab / ShiftTab / dblclick / contentReady / new target
   v
[Closing]
   | remove attributes, remove DOM, null tooltip ref
   v
[Enabled, Idle]
```

### DOM Structure

```text
document.body
└── div.e-tooltip-wrap.e-popup.e-lib.e-control.e-popup-open
    ├── role="tooltip"
    ├── id="{generatedCtrlId}_content"
    ├── div.e-tip-content
    │   └── innerHTML = full header/cell text
    └── div.e-arrow-tip.e-tip-bottom
        ├── div.e-arrow-tip-outer.e-tip-bottom
        └── div.e-arrow-tip-inner.e-tip-bottom
```

### Host Element Markers

```text
th.e-headercell.e-ellipsistooltip
└── div.e-headercelldiv[e-mappinguid="..."]
    └── span.e-headertext

td.e-rowcell.e-ellipsistooltip
└── rendered cell content
```

### Positioning Rules
- Uses `calculatePosition(target, 'Center', 'Top')`.
- Offsets upward by tooltip height plus arrow height.
- If grid is inside `.e-dialog` or `.e-multicolumn-list`, raises `z-index` above container.
- Tooltip root is appended to `document.body`, not to grid DOM.

### Invariants
- Tooltip must only show when content is actually truncated.
- Existing visible tooltip must close before another opens.
- `aria-describedby` and `data-tooltip-id` must be cleaned up on close.
- `allowTextWrap` may intentionally suppress this tooltip.

---

## Tooltip Type 2 — ShowTooltip
<!-- token-budget: 420 words -->

### Intent
Used when the grid wants general hover tooltips for header and content cells regardless of ellipsis overflow. This system is backed by `SfTooltip` and may render a custom `TooltipTemplate`.

### Activation Inputs
- `SfGrid.ShowTooltip = true`
- Optional `GridTemplates.TooltipTemplate`

### Mount Flow
1. `src/Internal/SfGrid.razor` checks `@if (ShowTooltip)`.
2. It renders `<GridTooltip TValue="TValue"></GridTooltip>`.
3. `GridTooltip.razor` renders `SfTooltip` targeting `#{Parent.ID} .e-headercell, #{Parent.ID} .e-rowcell`.
4. Tooltip opens on hover using Syncfusion popup behavior, not manual DOM creation in grid TS.

### Runtime Flow

```text
T0  Grid render
    |
    +--> SfGrid.razor
            |
            +--> ShowTooltip == true ? render GridTooltip : nothing
                    |
                    +--> GridTooltip.razor OnParametersSet()
                            |
                            +--> RenderTooltip = true
                            |
                            +--> render SfTooltip
                                  - Target = #gridId .e-headercell, .e-rowcell
                                  - OpensOn = Hover
                                  - MouseTrail = true
                                  - Position = BottomCenter
                                  - OnRender = SetTooltip

T1  Hover begins
    |
    +--> SfTooltip raises OnRender(TooltipEventArgs)
            |
            +--> SetTooltip(args)
                    |
                    +--> Parent.InvokeMethod("sfBlazor.Grid.getContentCell", ...top,left)
                            |
                            +--> scripts/sf-grid.ts route
                            +--> SfGrid.getContentCell(element, top, left)
                                    |
                                    +--> document.elementFromPoint(left, top)
                                    +--> resolve headercell or rowcell
                                    +--> read aria-colindex
                                    +--> if header => rowIndex = -1, mappingUid resolved
                                    +--> if content => rowIndex from aria-rowindex - 1
                                    +--> return { ColumnIndex, RowIndex, MappingUid }
                    |
                    +--> if RowIndex == -1
                    |       - resolve header column by uid
                    |       - use HeaderText or stacked header text
                    |
                    +--> else
                    |       - GetCellInformationAsync(rowIndex, colIndex)
                    |       - fetch row/cell value from Parent.Rows
                    |
                    +--> if empty string or null => cancel tooltip
                    |
                    +--> else store tooltipContent/value
                    +--> Parent.TooltipInstance = tooltipReference

T2  Content template render
    |
    +--> TooltipTemplate exists?
            | yes -> UpdateTooltip() -> build TooltipTemplateContext
            | no  -> render tooltipContent string

T3  Keyboard focus navigation / programmatic focus changes
    |
    +--> FocusHandler closes TooltipInstance.CloseAsync()
```

### State Model

```text
[Not Mounted]
   | ShowTooltip=true
   v
[Mounted / Waiting]
   | hover event
   v
[Hit-Test Resolve]
   | getContentCell(top,left)
   v
[Context Resolved]
   | empty? -> cancel
   | non-empty -> continue
   v
[Template Decision]
   | TooltipTemplate exists ?
   |--> [Template Render]
   |--> [Plain Text Render]
            
   v
[Visible]
   | focus move / hover out / component close
   v
[Closed]
   | next hover
   v
[Mounted / Waiting]
```

### Data Resolution Rules
- Header hover returns `RowIndex = -1` and resolves by column `Uid`.
- Stacked header hover uses stacked-column lookup.
- Content hover reads from grid rows.
- Grouped grids filter to `RowType == "Data"` rows before indexing.
- Virtualization uses row `Index` matching, not simple sequence position.

### DOM Model
The final popup is owned by `SfTooltip`, but the **grid target side** looks like this:

```text
Grid Root
├── th.e-headercell
│   └── div.e-headercelldiv[e-mappinguid="columnUid"]
└── td.e-rowcell
    └── cell content
```

The tooltip **content source** is either:

```text
Plain text mode
└── tooltipContent string

Template mode
└── TooltipTemplateContext
    ├── RowIndex
    ├── ColumnIndex
    ├── Data
    ├── Column
    └── Value
```

### Invariants
- This tooltip is not restricted to overflowed cells.
- Empty values cancel rendering.
- Focus navigation must close visible tooltip.
- `TooltipTemplate` must not assume row data exists when `RowIndex = -1`.

---

## Tooltip Type 3 — Validation Tooltip
<!-- token-budget: 460 words -->

### Intent
Used only during edit validation to show error messages near invalid editors. This system is hybrid: JS computes geometry; Razor owns visible error tooltip markup.

### Activation Inputs
- Edit mode active (`Normal`, `Batch`, or `Dialog`)
- Validation error exists for one or more fields
- `scripts/edit.ts:createTooltip()` is called

### Host Components
- `NormalEdit.razor`
- `BatchEdit.razor`
- `DialogEdit.razor`
All host `<ValidationTooltip>`.

### Runtime Flow

```text
T0  User edits cell / form field
    |
    +--> validation fails
            |
            +--> edit.ts:createTooltip(results, isAdd)
                    |
                    +--> for each invalid field
                            |
                            +--> resolve editor/td element
                            |      - template edit
                            |      - dialog edit
                            |      - virtualized add row
                            |      - frozen rows/header add row
                            |      - tree grid paths
                            |
                            +--> determine row/cell/container geometry
                            +--> determine inline vs dialog coordinates
                            +--> determine if bottom-position arrow required
                            +--> locate existing _Error tooltip node for uid
                            +--> mutate style.top/style.left/style.bottom/style.width
                            +--> set error text into .e-error label
                            +--> compute style string map by column uid
                    |
                    +--> dotNetRef.invokeMethodAsync('ShowValidationPopup', toolTipPos, arrowPosition)

T1  .NET receives callback
    |
    +--> GridJSInteropAdaptor.ShowValidationPopup(position, arrowPosition)
            |
            +--> Parent.EditModule.position[key] = style string
            +--> Parent.EditModule.ArrowPosition = top|bottom
            +--> Parent.EventAggregator.Trigger("ShowValidationMessage")

T2  Razor rerender
    |
    +--> ValidationTooltip.razor subscribed to ShowValidationMessage
            |
            +--> StateHasChanged()
            +--> loop all columns
                    |
                    +--> if column not visible or batch non-active column => skip
                    +--> find matching validation result
                    +--> read position dictionary by column uid
                    +--> display tooltip div with e-griderror
                    +--> choose e-tip-top or e-tip-bottom visibility
```

### State Model

```text
[Hidden]
   | validation error raised
   v
[JS Geometry Calculation]
   | invoke ShowValidationPopup()
   v
[Position State Stored in EditModule]
   | EventAggregator: ShowValidationMessage
   v
[Razor Rerender]
   | matching column + validation result found
   v
[Visible Error Tooltip]
   | value corrected / popup removed / RemoveValidationPopup()
   v
[Hidden]
```

### DOM Structure

```text
Inline Edit / Batch / Dialog Host
└── div.e-tooltip-wrap.e-lib.e-control.e-popup.e-griderror[.e-unfreeze]
    ├── id="{fieldOrComplexField}_Error"
    ├── e-mappinguid="{columnUid}_Error"
    ├── data-sf-style="top:...; left:...; display:block; ..."
    ├── div.e-tip-content
    │   └── label.e-error[for="fieldName"]
    │       └── validation message text
    ├── div.e-arrow-tip.e-tip-bottom  (visible when arrowPosition=bottom)
    │   ├── div.e-arrow-tip-outer.e-tip-bottom
    │   └── div.e-arrow-tip-inner.e-tip-bottom
    └── div.e-arrow-tip.e-tip-top     (visible when arrowPosition=top)
        ├── div.e-arrow-tip-outer.e-tip-top
        └── div.e-arrow-tip-inner.e-tip-top
```

### Positioning Decision Branches
- **Dialog edit:** positions relative to dialog content rectangle.
- **Normal/Batch inline edit:** positions relative to content/header table.
- **Frozen rows:** may resolve active editor from header section instead of content section.
- **Virtualization + add new row:** special lookup path finds active cell among duplicated mapping UIDs.
- **Bottom row / add row at bottom / last visible row:** flips arrow and sets `bottom` rather than `top`.
- **RTL:** applies horizontal correction if tooltip exceeds input right boundary.

### Invariants
- JS is source of geometry; Razor is source of actual visible markup.
- `ValidationTooltip.razor` does not independently calculate pixel coordinates.
- Column UID is the synchronization key between JS and Razor.
- Removing validation must clear both `ErrorResult` and `position` dictionary.

---

## DOM Rendering Cheat Sheet
<!-- token-budget: 260 words -->

### Quick DOM Identification

```text
If you inspect DOM and see...

1) document.body > .e-tooltip-wrap.e-popup.e-popup-open
   => EllipsisWithTooltip

2) tooltip content coming from TooltipTemplateContext / SfTooltip
   => ShowTooltip

3) .e-tooltip-wrap.e-griderror with label.e-error
   => Validation tooltip
```

### Target-to-Popup Mapping

```text
[Header Tooltip Target]
th.e-headercell
└── div.e-headercelldiv[e-mappinguid]
    └── span.e-headertext

[Content Tooltip Target]
td.e-rowcell
└── text / template / formatted display

[Validation Tooltip Host]
edit row / batch row / dialog content
└── div#{field}_Error[e-mappinguid="{uid}_Error"]
```

### Attribute Contracts
- `aria-colindex` on header/content cells is required for `getContentCell()`.
- `aria-rowindex` is required for content row resolution.
- `e-mappinguid` on header inner div is required for header tooltip lookup.
- `aria-describedby` + `data-tooltip-id` are applied only by `EllipsisWithTooltip` runtime popup.
- `_Error` suffix is the contract for validation tooltip node lookup.

---

## Feature Interaction Matrix
<!-- token-budget: 260 words -->

| Combination | Tooltip Type Affected | Must Preserve | Risk |
|---|---|---|---|
| Tooltip + TextWrap | EllipsisWithTooltip | `allowTextWrap` plus `wrapMode` can intentionally suppress ellipsis tooltip in header/content/both zones. | Critical |
| Tooltip + Frozen Columns | EllipsisWithTooltip, Validation | Frozen split DOM and hover synchronization must remain correct; validation popup may require `e-unfreeze`. | Critical |
| Tooltip + Frozen Rows | Validation | Add-row and validation lookup may resolve active cell from header region. | Critical |
| Tooltip + Virtualization | ShowTooltip, Validation | `ShowTooltip` must resolve rows by row index semantics; validation popup must find active mapped cell among virtualized DOM. | Critical |
| Tooltip + Grouping | ShowTooltip | Tooltip value lookup must use only data rows when group caption rows exist. | High |
| Tooltip + ClipMode runtime update | EllipsisWithTooltip | `setOptions()` must call `toolTipModule.updateEvents()` when grid or column clip mode changes. | High |
| Tooltip + Dialog Edit | Validation | Positioning must use dialog wrapper content rectangle, not content table geometry. | High |
| Tooltip + Keyboard Navigation | ShowTooltip, EllipsisWithTooltip | Focus movement or Tab/Shift+Tab must close visible tooltip. | High |
| Tooltip + Description annotation | EllipsisWithTooltip | Header content must prefer column `Description` over visible header text. | Medium |
| Tooltip + TooltipTemplate | ShowTooltip | Header tooltips have `RowIndex = -1`; template must handle null `Data`. | Medium |
| Tooltip + RTL | Validation | Horizontal correction must prevent overflow beyond input boundary. | Medium |
| Tooltip + Adaptive UI vertical mode | EllipsisWithTooltip | Text-wrap height adjustments and adaptive layout must not reopen incorrect tooltip on stale coordinates. | Medium |

---

## LLM Working Rules
<!-- token-budget: 220 words -->
- Do not treat `ShowTooltip` and `EllipsisWithTooltip` as the same feature.
- Do not replace the JS-created ellipsis popup with `SfTooltip` unless the task explicitly redesigns behavior.
- When investigating tooltip content bugs, first decide whether content comes from:
  - `element.innerText`
  - `column.Description`
  - `TooltipTemplateContext.Value`
  - validation `ErrorResult.Message`
- When investigating positioning bugs, first decide whether position is computed by:
  - `tooltip.ts:setPosition()`
  - `SfTooltip` internal popup placement
  - `edit.ts:createTooltip()`
- When investigating missing tooltip bugs, check the activation gate first:
  - `ClipMode.EllipsisWithTooltip`
  - `ShowTooltip = true`
  - validation error exists and `ShowValidationPopup()` fired
- For header issues, preserve `e-mappinguid` and `aria-colindex` contracts.
- For content issues, preserve `aria-rowindex` contracts.
- For validation issues, preserve `columnUid -> position dictionary -> ValidationTooltip.razor` contract.

---

## JavaScript Interop
<!-- token-budget: 250 words -->

The tooltip system uses bidirectional C# ↔ JavaScript interop:

### Flow 1: ShowTooltip (C# → JS)
```
C# (GridTooltip.razor:87)
  └─ Parent.InvokeMethod<cellInfoDictionary>("sfBlazor.Grid.getContentCell", dataId, top, left)

JS Route (sf-grid.ts:1123-1126)
  └─ gridInstance.getContentCell(gridInstance.element, top, left)

JS Handler (sf-grid-fn.ts:770)
  └─ return { ColumnIndex, RowIndex, MappingUid }
```

### Flow 2: Validation Tooltip (JS → C#)
```
JS (edit.ts:createTooltip)
  └─ dotNetRef.invokeMethodAsync('ShowValidationPopup', toolTipPos, arrowPosition)

C# Handler (GridJSInteropAdaptor.cs:414-420)
  └─ ShowValidationPopup(position, arrowPosition)
     ├─ EditModule.position.AddOrUpdateItem(...)
     ├─ EditModule.ArrowPosition = arrowPosition
     └─ EventAggregator.Trigger("ShowValidationMessage")

JS (edit.ts)
  └─ dotNetRef.invokeMethodAsync('RemoveValidationPopup')

C# Handler (GridJSInteropAdaptor.cs:426-428)
  └─ RemoveValidationPopup() → EditModule.ClearValidationErrors()
```

### TypeScript Function Signatures
```typescript
// sf-grid-fn.ts:770
public getContentCell(element: HTMLElement, top: number, left: number): any {
  // Returns: { ColumnIndex: number, RowIndex: number, MappingUid: string }
}

// edit.ts (tooltip positioning)
public createTooltip(results: any, isAdd: boolean): void {
  // Computes validation popup geometry in JS
}
```

### CRITICAL Rules
- **Always** preserve `aria-colindex` and `aria-rowindex` for `getContentCell` hit-testing
- **Never** call `ShowValidationPopup` directly; only `edit.ts:createTooltip` triggers it
- **Always** call `RemoveValidationPopup` to clean up validation state
- **Do not** cache `dotNetRef` across grid lifecycle; re-acquire on each call

---

## See Also
<!-- token-budget: 80 words -->

Related skill files for cross-feature tooltip interactions:

- [editing-skill](editing-skill/SKILL.md) — Validation tooltip deep-dive and edit state management
- [virtualization-skill](virtualization-skill/SKILL.md) — `ShowTooltip` + virtual scroll row index semantics
- [frozen-column-skill](frozen-column-skill/SKILL.md) — EllipsisWithTooltip + frozen split DOM synchronization
- [grouping-skill](grouping-skill/SKILL.md) — Tooltip value lookup filtering only data rows

---

## Prompt Template
<!-- token-budget: 320 words -->

```text
Mode: {feature-implementation | bug-fix}
Skill: Tooltip
Component: Syncfusion.Blazor.Grids SfGrid<TValue>

You are working on DataGrid tooltip behavior. This skill preloads three separate tooltip systems:

1. EllipsisWithTooltip
- Activation: grid/column ClipMode = EllipsisWithTooltip
- Render source: scripts/tooltip.ts
- DOM owner: document.body
- Trigger target: .e-ellipsistooltip on th/td
- Content source: column.Description, header text, or cell innerText
- Positioning source: calculatePosition(target, 'Center', 'Top')

2. ShowTooltip
- Activation: ShowTooltip = true
- Render source: src/Internal/Renderer/GridTooltip.razor
- Popup owner: SfTooltip
- Context source: scripts/sf-grid-fn.ts:getContentCell(top,left)
- Template source: GridTemplates.TooltipTemplate using TooltipTemplateContext
- Header context rule: RowIndex = -1, Data = null

3. Validation tooltip
- Activation: edit validation failure
- Geometry source: scripts/edit.ts:createTooltip()
- Popup DOM source: src/Internal/Renderer/ValidationTooltip.razor
- Sync key: columnUid / {columnUid}_Error
- Arrow state: Parent.EditModule.ArrowPosition = top|bottom

Mandatory checks before changing code:
- Identify exactly which tooltip system is failing.
- Preserve `aria-colindex`, `aria-rowindex`, and `e-mappinguid` contracts.
- Preserve frozen, grouped, virtualized, and dialog-edit branches.
- Preserve close behavior on scroll, mouseout, focus move, Tab, Shift+Tab, and data refresh where applicable.
- Do not assume popup DOM lives inside grid root.

Required output from the agent:
1. Root cause in 3-5 sentences
2. Exact method/component to modify
3. Interaction risks from the matrix above
4. Minimal patch only
5. Regression cases for header, content, virtualization, frozen, grouping, edit validation, and template tooltip
```
