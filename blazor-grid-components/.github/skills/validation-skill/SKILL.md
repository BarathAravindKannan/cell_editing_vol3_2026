---
name: validation-skill
description: Expert knowledge for the Validation feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to validation behaviour, including DataAnnotations, column-level ValidationRules, custom Validator templates, FluentValidation integration, validation tooltip DOM rendering (Normal/Dialog/Batch modes), and cross-feature interaction guarantees with editing, virtualization, frozen columns, grouping, paging, and infinite scroll.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Validation in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**
Load this skill when implementing or debugging validation rules, tooltip positioning, custom validator templates, DataAnnotations integration, or any feature that touches `ErrorResult`, `ValidationTooltip`, `ColumnsValidator`, or `ShowValidationPopup`.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, EventAggregator pattern
- `/docs/architecture/system-architecture.md` — Edit<T> module lifecycle, validation pipeline
- `/docs/tech-stack/tech-stack.md` — Blazor `EditContext`, `DataAnnotationsValidator`, `ValidationMessageStore`, async/await rules
- `/docs/code-guidelines/coding-standards.md` — Async patterns, `ConfigureAwait(true)`, guard clauses

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — tooltip positioning is pixel-precise; changes to `div.style.top/left/bottom` break all edit modes.
- **`ErrorResult` is the single source of truth** — `ValidationTooltip.razor` renders only entries present in `Edit<T>.ErrorResult`; clear it carefully.
- **EventAggregator drives tooltip re-render** — trigger `"ShowValidationMessage"` to refresh `ValidationTooltip`; do NOT call `StateHasChanged` directly.
- **Dual validation pipeline** — DataAnnotations (`_customStore`) AND column-level `ValidationRules` (`_store`) both run; both must clear on valid.
- **Complex fields (dot-notation)** — use `ComplexEditContext` per parent object; `FieldIdentifier` must reference the correct sub-model; `GetComplexName(field)` replaces `.` with `___` for DOM ids.
- **Validator template override** — when `GridEditSettings.Validator != null`, `ColumnsValidator` is NOT rendered; the custom template owns `ShowValidationMessage` callback.
- **Virtualization** — `VirtualValidation` list preserves errors across virtual scroll; re-invoked after scroll completes.
- **`ForceValidate` flag** — set to `true` before `EditContext.Validate()` in `EndEdit`, `SaveCell`, and `CurrentFocus` (keyboard nav); ensures `ApplyFormValidation` processes results even when `ValueChanged == false`; always reset to `false` after.
- **`ClearRules()` method** — resets both `ErrorResult` and `position` to new empty collections; called on `CloseEdit`, `BatchClose`, `StartEdit` (with `ShowAddNewRow`), and `DeleteRecord` (Batch mode).
- **`IsEmptyRowRendered` flag** — set `true` when `AllowPaging && TotalItemCount == 0 && IsAdd` with active errors, or Batch mode with ≤2 rows; triggers `RowStateChanged` to add a dummy row so the tooltip has vertical space to render; cleared when `position.Count == 0`.
- **`ValidateNextCell()`** — called in `EndEdit`/`BulkAddRow` (Batch add); runs `EditContext.Validate()` on the add row and re-opens the cell with the first error if validation fails.
- **`ValidationRules()` method** — builds `IDictionary<string, object>` keyed by column `Uid` for all visible columns with non-null `ValidationRules`; passed to `ColumnsValidator` as the `ValidationRules` parameter.
- **`ShowValidationPopup` position keys** — JS sends column `Uid` strings (not `Uid_Error`); Blazor merges them into `Edit<T>.position` dictionary which `ValidationTooltip` reads by `Column.Uid`.

---

## Code Location Map
<!-- token-budget: 80 words -->

### C# / Blazor Source
- **`src/Internal/Actions/Edit.cs`** — **HEAVILY RESTRUCTURED (4244 lines changed)** — `Edit<T>` module: `ErrorResult` list, `ArrowPosition`, `IsLastRow`, `ApplyFormValidation()`, `InvokeValidation()`, `position` dictionary (uid → CSS style string), `ForceValidate` flag, `IsEmptyRowRendered` flag, `ShouldPreventEditFormRender` flag, `ClearRules()`, `ValidateNextCell()`, `ValidationRules()`, `GetComplexName()` static helper, `EnsureDataAndEditContext()`
- **`src/Internal/Editors/ColumnsValidator.razor`** — Blazor component wiring `EditContext.OnFieldChanged` + `OnValidationRequested`; runs DataAnnotations via `DataAnnotationHandler()` and column-level rules via `Validator.TryValidate()`; owns `ValidationMessageStore` instances `_store` (column rules) and `_customStore` (DataAnnotations); `ValidateRequested` skips dot-notation columns where `ValidationRules == null`; `DataAnnotationHandler` traverses the object graph by splitting dot-notation field before calling `Validator.TryValidateProperty`
- **`src/Internal/Renderer/ValidationTooltip.razor`** — Renders one `div.e-tooltip-wrap.e-griderror` per visible column; reads `Edit<T>.ErrorResult` and `Edit<T>.position`; subscribes to `"ShowValidationMessage"` event; emits `e-tip-top` / `e-tip-bottom` arrow elements; `GetClassName` adds `e-unfreeze` when `(column.IsFrozen == false && FrozenColumns == 0)` OR `(FrozenColumns > 0 && column.Index > FrozenColumns - 1)` (~24 lines modified)
- **`src/Internal/Renderer/ValidationDialog.razor`** — Renders `SfDialog` for alert messages (EditAlert, DeleteAlert, BatchSaveConfirm, CancelEdit, BatchSaveLostChanges); subscribes to `"ShowValidationDialog"` event (~146 lines modified)
- **`src/Internal/Renderer/NormalEdit.razor`** — Hosts `<EditForm>`, conditionally renders `Validator` template OR `<DataAnnotationsValidator>` + `<ColumnsValidator>`; always renders `<ValidationTooltip>` unless `PreventShowAddForm=true` (~32 lines modified)
- **`src/Internal/Renderer/DialogEdit.razor`** — Same pattern as NormalEdit but inside SfDialog; `ShowValidationMessage` callback wired to `ApplyFormValidation`; renders two dialog variants: standard (`{ID}_dialogEdit_wrapper`) and adaptive (`{ID}_adaptive_dialogEdit_wrapper`); adaptive variant uses `e-adaptive-align-left/right` RTL classes (~30 lines)
- **`src/Internal/Renderer/BatchEdit.razor`** — Batch cell editor form; `ShowValidationMessage` callback wired to `ApplyFormValidation`; on `firstRender` in `OnAfterRenderAsync`, if `ErrorResult.Count > 0` and `!KeyPressed`, calls `InvokeValidation(new List { ErrorResult[0] })` to re-show tooltip when batch edit cell re-renders; `ShouldPreventEditFormRender` suppresses form render when Tab exits last cell (~15 lines)
- **`src/Internal/Base/GridJSInteropAdaptor.cs:414-429`** — `[JSInvokable] ShowValidationPopup(IDictionary<string,string> position, string arrowPosition)` (line 414): position keys are column `Uid` values (not `_Error`-suffixed); merges into `Edit<T>.position` via `AddOrUpdateItem`; sets `ArrowPosition`; triggers `"ShowValidationMessage"`. `[JSInvokable] RemoveValidationPopup()` (line 427): unconditionally calls `Edit<T>.ClearValidationErrors()` (`Edit.cs:1253`); the guard lives inside `ClearValidationErrors()`, which only clears (replaces `ErrorResult` with a new empty list and `position` with a new empty dict, then triggers `"ShowVa...
- **`src/Internal/Actions/VirtualScroll.cs`** — `VirtualValidation` list: caches active errors during virtual scroll; in post-scroll `OnAfterRenderAsync`: saves `errorResult`, clears `ErrorResult`, triggers `"ShowValidationMessage"` (hides stale tooltip), restores `ErrorResult`, calls `InvokeValidation(VirtualValidation)`, resets `VirtualValidation` to empty list. `ScrollToEditedRowAsync(validateFields)`: stores into `VirtualValidation` and scrolls to edited row.
- **`src/GridColumn.cs`** — `ValidationRules` property (type `ValidationRules`); serialized as `validationRules` JSON to JS; supports `_validationRules` backing field with `UpdateProperty` pattern
- **`src/GridEditSettings.cs`** — `Validator` property (`RenderFragment<object>?`): custom validator template; when set, disables `ColumnsValidator` and `DataAnnotationsValidator`
- **`src/EventModels/Grids.cs`** — `ValidationRules` class (extends `ValidationRuleBase`): `Required`, `MinLength`, `MaxLength`, `Min`, `Max`, `Range`, `RangeLength`, `Email`, `Number`, `RegexPattern`, `Messages`. `ValidatorTemplateContext` class: `Data`, `EditContext`, `ShowValidationMessage` callback (`Action<string, bool, string>`). `ValidationResult` internal class: `FieldName`, `Uid`, `IsValid`, `Message`.
- **`scripts/edit.ts`** — `Edit.createTooltip(results, isAdd)`: computes pixel-precise `top/left/bottom` for each error tooltip div using `getBoundingClientRect()`; resolves element by `e-mappinguid` attribute; handles frozen rows/columns, virtual scroll, infinite scroll, Dialog mode, RTL, bottom-row position; calls `.invokeMethodAsync('ShowValidationPopup', toolTipPos, arrowPosition)`.
- **`scripts/interfaces.ts`** — `validationRules?: Object` on Column interface (line 512)

---

## DOM Structure — Validation Tooltip
<!-- token-budget: 120 words -->

Rendered by `ValidationTooltip.razor` inside the edit form (`<EditForm>`). One tooltip element per editable column.

```
<!-- Normal/Batch mode: inside .e-editcell > EditForm -->
<div class="e-tooltip-wrap e-lib e-control e-popup e-griderror [e-unfreeze?]"
     id="{complexField}_Error"
     e-mappinguid="{columnUid}_Error"
     data-sf-style="top:Npx; left:Npx; display:[none|'']">

    <div class="e-tip-content">
        <label class="e-error" for="{complexField}" id="{complexField}-info">
            {validation message}
        </label>
    </div>

    <!-- Arrow shown when tooltip above the cell -->
    <div class="e-arrow-tip e-tip-top" data-sf-style="display:[block|none]">
        <div class="e-arrow-tip-outer e-tip-top"></div>
        <div class="e-arrow-tip-inner e-tip-top"></div>
    </div>

    <!-- Arrow shown when tooltip below the cell (last row / bottom add) -->
    <div class="e-arrow-tip e-tip-bottom" data-sf-style="display:[block|none]">
        <div class="e-arrow-tip-outer e-tip-bottom"></div>
        <div class="e-arrow-tip-inner e-tip-bottom"></div>
    </div>
</div>
```

**Key CSS classes:**
| Class | Meaning |
|---|---|
| `e-tooltip-wrap e-lib e-control e-popup` | Base Syncfusion popup wrapper |
| `e-griderror` | Grid validation error tooltip marker |
| `e-unfreeze` | Added when `(column.IsFrozen == false && FrozenColumns == 0)` OR `(FrozenColumns > 0 && column.Index > FrozenColumns - 1)` — i.e., column is in the movable section |
| `e-tip-top` | Arrow pointing upward (tooltip appears below cell) |
| `e-tip-bottom` | Arrow pointing downward (tooltip appears above cell — last row or bottom add) |
| `e-error` | Label inside tooltip that receives the message text |

**Position logic:**
- `display: none` → no error for this column
- `display: ''` (empty) → error present; position CSS applied inline via `data-sf-style`
- Tooltip `top/left/bottom` set by JS (`edit.ts → createTooltip`) and relayed to Blazor via `ShowValidationPopup` JSInvokable
- Frozen + Normal/Batch mode: `div.style.position = 'absolute'`; closest `.e-editcell` or `.e-table` gets `position: 'relative'`
- RTL: `left` adjusted so tooltip right-edge aligns with input right-edge

---

## Validation Pipeline — Full Flow
<!-- token-budget: 160 words -->

### Trigger: Field value change (inline editing)
```
EditContext.OnFieldChanged
  └── ColumnsValidator.ValidateField()
        ├── DataAnnotationHandler()        // System.ComponentModel.DataAnnotations
        │     └── _customStore updated
        └── ValidateFieldIdentifier()      // GridColumn.ValidationRules
              └── Validator.TryValidate()
                    └── _store updated
                    └── OnValidated?.Invoke(ValidationResult)
                          └── Edit<T>.ApplyFormValidation(result)
                                ├── ErrorResult updated (add/remove)
                                └── InvokeValidation(results)
                                      └── sfBlazor.Grid.validation (JS)
                                            └── Edit.createTooltip()
                                                  └── DotNetRef.invokeMethodAsync
                                                        ('ShowValidationPopup', pos, arrow)
                                                            └── GridJSInteropAdaptor
                                                                  .ShowValidationPopup()
                                                                    ├── position dict updated
                                                                    ├── ArrowPosition set
                                                                    └── EventAggregator
                                                                          .Trigger("ShowValidationMessage")
                                                                              └── ValidationTooltip
                                                                                    .InvokeStateChange()
                                                                                      └── StateHasChanged()
```

### Trigger: EndEdit / Save attempt
```
EditContext.Validate()  ←── called in Edit<T>.EndEdit()
  └── OnValidationRequested fired
        └── ColumnsValidator.ValidateRequested()
              └── foreach column → ValidateFieldIdentifier()
                    └── same pipeline as above
  └── returns bool (valid = ErrorResult.Count == 0)
  └── if invalid → save blocked; tooltip displayed
```

### Trigger: Virtual scroll with active error
```
VirtualScroll.OnAfterRenderAsync() [post-scroll]
  └── if VirtualValidation.Count > 0 && EditModule != null
        ├── errorResult = EditModule.ErrorResult  // save
        ├── EditModule.ErrorResult = new List<>() // clear
        ├── EventAggregator.Trigger("ShowValidationMessage") // hide stale tooltip
        ├── EditModule.ErrorResult = errorResult  // restore
        ├── await EditModule.InvokeValidation(VirtualValidation)
        │     └── JS tooltip repositioning → ShowValidationPopup → re-render
        └── VirtualValidation = new List<>()      // reset cache

VirtualScroll.ScrollToEditedRowAsync(validateFields)
  └── VirtualValidation = validateFields          // store for post-render replay
  └── ScrollIntoViewAsync(edited row position)
```

### Trigger: JS-side RemoveValidationPopup (e.g., escape or cancel)
```
GridJSInteropAdaptor.RemoveValidationPopup()
  └── calls Edit<T>.ClearValidationErrors() unconditionally
       └── guard inside ClearValidationErrors(): only clears if ErrorResult.Count > 0
  └── EditModule.ErrorResult = new List<>()
  └── EditModule.position = new Dictionary<>()
  └── EventAggregator.Trigger("ShowValidationMessage")
        └── ValidationTooltip hides all tooltips
```

### Trigger: Batch keyboard navigation (Tab/Enter on edited cell)
```
Edit<T>.CurrentFocus() [BeforeCellFocus handler]
  └── ForceValidate = true
  └── EditContext.Validate()
        └── if invalid → focus.Cancel = true; InvokeValidation(result)
        └── if valid   → proceed to SaveCell / EditCell
  └── ForceValidate = false
```

### Trigger: Batch add-row validation on EndEdit / BulkAddRow
```
Edit<T>.ValidateNextCell()
  └── ForceValidate = true
  └── EditContext.Validate()
        └── if invalid → re-open cell with ErrorResult[0]
        └── if valid   → IsAdd = false
  └── ForceValidate = false
```

---

## Validation Types Reference
<!-- token-budget: 80 words -->

| Rule | Property on `ValidationRules` | Maps to DataAnnotation |
|---|---|---|
| Required field | `Required = true` | `[Required]` |
| Min numeric value | `Min = N` | `[Range(N, ...)]` |
| Max numeric value | `Max = N` | `[Range(..., N)]` |
| Min string length | `MinLength = N` | `[MinLength(N)]` |
| Max string length | `MaxLength = N` | `[MaxLength(N)]` |
| Numeric only | `Number = true` | `[RegularExpression(@"\d+")]` |
| Regex pattern | `RegexPattern = "pattern"` | `[RegularExpression("pattern")]` |
| Email format | `Email = true` | `[EmailAddress]` |
| Value range | `Range = new[] {min, max}` | `[Range(min, max)]` |
| Range length | `RangeLength = new[] {min, max}` | `[MinLength(min)][MaxLength(max)]` |
| Custom messages | `Messages = new Dictionary<string,string>` | Overrides default rule messages |

**Custom Validator template** (`GridEditSettings.Validator`):
- `RenderFragment<object>` where `object` is `ValidatorTemplateContext`
- Context provides: `Data` (current edit record), `EditContext`, `ShowValidationMessage(field, isValid, message)` callback
- When set: `ColumnsValidator` and `DataAnnotationsValidator` are NOT rendered
- FluentValidation can be integrated here via custom `IValidator<T>` invoked inside the template

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + codebase cross-reference
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| Validation + Normal Edit | Tooltip `display:''` on error, `display:none` on valid; position recalculated via JS; arrow direction flips for last row (`e-lastrowcell`); `ForceValidate=true` before `EditContext.Validate()` in `EndEdit` | Critical |
| Validation + Dialog Edit | Tooltip rendered inside dialog wrapper (`#ID_dialogEdit_wrapper .e-dlg-content`); `element` resolved via `[e-mappinguid={uid}_Dialog]` selector | Critical |
| Validation + Batch Edit | `ValidationTooltip` only renders tooltip for `OriginalCell.Column.Field` column; keyboard nav (`CurrentFocus`) sets `ForceValidate=true` + cancels focus if error; `ValidateNextCell()` re-opens add-row cell on error; `BatchEdit.OnAfterRenderAsync` replays `InvokeValidation` on first render if `ErrorResult.Count > 0` | Critical |
| Validation + Virtualization | `VirtualValidation` caches errors; post-scroll: save→clear→trigger→restore→InvokeValidation→reset; tooltip repositioned after virtual rows repaint; `ScrollToEditedRowAsync` stores errors then scrolls | Critical |
| Validation + Infinite Scroll | Same `VirtualValidation` cache mechanism applies; `showAddNewRow` with infinite scroll resolves element from header section when `isHeader = true`; `InfiniteScrollModule.ResetInfiniteProperties` called before `InvokeValidation` on add-row validation fail | High |
| Validation + Frozen Columns | `e-unfreeze` added when `IsFrozen==false && FrozenColumns==0` OR `FrozenColumns>0 && column.Index > FrozenColumns-1`; frozen+Normal/Batch → `closest('.e-editcell').style.position = 'relative'`; `div.style.left` uses `input.offsetLeft` instead of `getBoundingClientRect` | Critical |
| Validation + Frozen Rows | `isHeader = true` path in `createTooltip`; element resolved from `getHeaderContent()`; `validationForBottomRowPos = true` when `rowIndex + 1 === frozenRows` | High |
| Validation + Grouping + Virtualization | `rows` query changes to `querySelectorAll('tr')` with `.pop()` (removes add row); `viewPortRowCount` calculation uses content rows | High |
| Validation + Paging | `AllowPaging` + `TotalItemCount==0` + `IsAdd` + errors → `IsEmptyRowRendered=true` + `RowStateChanged`; also triggered for Batch mode with ≤2 rows or last row edit; cleared when `position.Count==0` | Medium |
| Validation + ShowAddNewRow | Separate `EditForm` instances (add form vs edit form); `PreventShowAddForm=true` suppresses `<ValidationTooltip>` on disabled add form; `ClearRules()` called in `StartEdit` before enabling add form | High |
| Validation + RTL | `enableRtl` in `createTooltip` adjusts `div.style.left` so tooltip right-edge aligns with input right-edge | Medium |
| Validation + TreeGrid | Element resolved via `#DataItem___${name}` prefix for TreeGrid rendered columns | Medium |
| Validation + AdaptiveUI | `dialogWrapperText` changes to `_adaptive_dialogEdit_wrapper`; Dialog mode element lookup uses adaptive wrapper; adaptive dialog uses `e-adaptive-align-left/right` instead of `e-textAlign-left/right` | Medium |
| Validation + Complex Fields (dot-notation) | `FieldName.replace(/[.]/g, '___')` for DOM id; `ComplexEditContext` dict built in `EnsureDataAndEditContext`; `complexFieldIdentifier` used for `_store` operations; `ColumnsValidator.ValidateRequested` skips if `ValidationRules==null` and field has `.` | High |
| Validation + CloseEdit / BatchClose | `ClearRules()` called explicitly; resets both `ErrorResult` and `position`; no `ShowValidationMessage` trigger needed since form is torn down | Medium |

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`
**Skill**: Validation

**Context**
You are implementing or fixing Validation for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Validation integrates with Blazor's `EditContext` and supports: DataAnnotations (`[Required]`, `[Range]`, etc.), column-level `GridColumn.ValidationRules`, and custom `GridEditSettings.Validator` templates. Validation errors are rendered as positioned tooltip elements in the DOM.

**Core Architecture**
- **Validator component**: `Internal/Editors/ColumnsValidator.razor` — wires `OnFieldChanged` + `OnValidationRequested`; runs both DataAnnotations (`_customStore`) and column rules (`_store`)
- **Error state**: `Edit<T>.ErrorResult` (`List<ValidationResult>`) — single source of truth for active errors
- **Position state**: `Edit<T>.position` (`Dictionary<string, object>`) — uid → CSS style string
- **Arrow direction**: `Edit<T>.ArrowPosition` — `"top"` (normal) or `"bottom"` (last row / bottom add)
- **Tooltip renderer**: `Internal/Renderer/ValidationTooltip.razor` — one `div.e-griderror` per column; subscribes to `"ShowValidationMessage"` EventAggregator event
- **JS bridge**: `scripts/edit.ts → Edit.createTooltip()` computes pixel positions; calls `dotNetRef.invokeMethodAsync('ShowValidationPopup', toolTipPos, arrowPosition)`
- **JSInvokable entry points**: `GridJSInteropAdaptor.ShowValidationPopup()` and `RemoveValidationPopup()`
- **Custom validator**: `GridEditSettings.Validator` (`RenderFragment<ValidatorTemplateContext>`) — replaces ColumnsValidator; context provides `ShowValidationMessage(field, isValid, message)` callback

**Key Behaviors**
1. `EditContext.Validate()` in `EndEdit()` blocks save if any column fails validation; `ForceValidate=true` is set before calling `Validate()` and reset after.
2. Field change triggers `ColumnsValidator.ValidateField()` → `ApplyFormValidation()` → `InvokeValidation()` → JS `createTooltip()` → `ShowValidationPopup()` → EventAggregator → tooltip re-render.
3. Tooltip hidden (`display:none`) when `ErrorResult` has no entry for the column; shown with position when present.
4. Batch mode: tooltip only shown for `OriginalCell.Column.Field` column; other columns hidden.
5. Virtualization: `VirtualValidation` caches errors; post-scroll replay: save `ErrorResult` → clear → trigger → restore → `InvokeValidation(VirtualValidation)` → reset `VirtualValidation`.
6. Frozen columns: tooltip gets `e-unfreeze` class for movable-section columns; exact condition: `(IsFrozen==false && FrozenColumns==0) || (FrozenColumns>0 && Index > FrozenColumns-1)`.
7. Complex fields: field name uses `___` instead of `.` for DOM ids via `GetComplexName()`; `ComplexEditContext` maps sub-model built in `EnsureDataAndEditContext`.
8. `ClearRules()` resets both `ErrorResult` and `position`; called on `CloseEdit`, `BatchClose`, `StartEdit` (ShowAddNewRow), `DeleteRecord` (Batch).
9. `IsEmptyRowRendered` ensures a dummy row exists in paged grid when adding with errors and `TotalItemCount==0`; also for Batch mode with ≤2 rows.
10. `ValidationRules()` on `Edit<T>` builds the uid-keyed rules dict passed to `ColumnsValidator`; only includes visible columns.

**Cross-Feature Rules**
- **Normal Edit**: tooltip in `.e-editcell`; arrow `top` unless last row (`e-lastrowcell`) or bottom add.
- **Dialog Edit**: element resolved by `[e-mappinguid={uid}_Dialog]`; tooltip inside dialog content; adaptive uses `{ID}_adaptive_dialogEdit_wrapper`.
- **Batch Edit**: keyboard nav in `CurrentFocus` sets `ForceValidate=true`; `ValidateNextCell()` handles add-row validation; `BatchEdit.OnAfterRenderAsync` replays tooltip on first render.
- **Virtualization**: save `ErrorResult` → clear → trigger `"ShowValidationMessage"` → restore → `InvokeValidation(VirtualValidation)`.
- **Frozen**: `closest('.e-editcell').style.position = 'relative'`; tooltip uses `input.offsetLeft` for `left` calc.
- **RTL**: adjust `div.style.left` using `inputRight - elemRight` offset.

**DOM IDs / Selectors**
- Tooltip div id: `{complexField}_Error` (dots replaced with `___` via `GetComplexName()`)
- Tooltip `e-mappinguid`: `{columnUid}_Error`
- `ShowValidationPopup` position dict keys: column `Uid` (no suffix)
- Dialog element selector: `[e-mappinguid={uid}_Dialog]`
- Grid form class: `e-gridform e-lib e-formvalidator` + `e-adding` (add) or `e-editing` (edit)
- Error label: `.e-error` inside `.e-tip-content`

**Before Making Changes**
1. Never mutate `ErrorResult` without triggering `"ShowValidationMessage"` afterwards.
2. Never call `StateHasChanged()` on `ValidationTooltip` directly — use EventAggregator.
3. Use `ClearRules()` (not manual assignment) when clearing both `ErrorResult` and `position` together.
4. Always set `ForceValidate=true` before `EditContext.Validate()` and reset it in both success and failure branches.
5. When adding new validation logic, test with: Normal + Dialog + Batch, frozen columns, virtualization + grouping, complex fields, RTL, ShowAddNewRow.
6. Ensure `ConfigureAwait(true)` on all async calls.
7. Preserve the `ErrorResult.Count > 0` guard inside `ClearValidationErrors()` (not in `RemoveValidationPopup`, which delegates to it unconditionally) — it must not clear an already-empty state.
