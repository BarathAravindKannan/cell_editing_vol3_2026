---
name: command-column-skills
description: Expert knowledge for the Command Column Editing feature in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to command column rendering behaviour, including inline edit row rendering (Normal mode), dialog edit rendering, batch edit mode handling, command button click routing (Edit/Delete/Save/Cancel/Custom), validation tooltip positioning, and cross-feature interaction guarantees with virtual scroll, frozen columns, grouping, infinite scroll, and ShowAddNewRow scenarios.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**
Expert knowledge for Command Column Editing & Rendering in the Syncfusion Blazor DataGrid. Guarantees no breakage with any other feature.

---

**Agent Invocation**
<!-- token-budget: 40 words -->
- Scope: `feature-implementation` | `bug-fix` for Command Column rendering, editing, button click handlers, and validation tooltip positioning
- Load this skill ONLY for work scoped to Command Column Editing. Do NOT load alongside other feature skills.
- One agent invocation = one feature skill maximum.

---

## Knowledge References
<!-- token-budget: 60 words -->
All content derived from reading these files — do NOT reproduce their content here:

- `docs/training/00-START-HERE.md`
- `docs/training/01-getting-started/architecture-overview.md`
- `docs/training/02-requirements-analysis/understanding-requirements.md`
- `docs/training/03-llm-best-practices/working-with-llms.md`
- `docs/training/04-code-processing/optimal-chunking-strategies.md`
- `docs/training/05-practical-examples/feature-implementation-walkthrough.md`
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

Key rules and edge cases from `/docs/training/` that directly govern Command Column Editing work:

- **RenderFragment pattern**: All rendering logic in `Internal/Renderer/*.cs` MUST use `RenderTreeBuilder` for incremental rendering. No direct HTML strings. Every attribute, event handler, and child component must increment the sequence counter.
- **CSS class application**: Command column cells use `e-unboundcell`, `e-rightalign`, buttons use `e-edit-delete e-{Type}button`. Never add alignment classes to the cell if `Column.Commands != null` — alignment is set on the button container `<div>`.
- **EventAggregator for state changes**: `RenderCommandColumnCell()` and `CommandClickHandler()` must NOT call `StateHasChanged()` directly. Instead, trigger events: `"RowStateChanged"`, `"ContentStateChanged"`, `"VirtualComponentUpdate"`, `"ToolbarStateChanged"`.
- **Validation tooltip positioning**: `edit.ts → createTooltip()` (line 17) positions validation tooltips after `EditContext.Validate()` fails. Tooltip position must account for frozen rows/columns, virtual scroll, show-add-new-row, adaptive UI, and RTL — all conditions must be tested. See JavaScript Interop section for complete C# → TypeScript flow.
- **PreventShowAddForm flag**: When `ShowAddNewRow = true` and `NewRowPosition = Bottom` and rendering the add form, the `PreventShowAddForm` flag must disable Save/Cancel/None buttons in `NormalEdit.razor` to prevent accidental saves before all required fields are filled. `IsDisableRow` flag forces use of a blank model row for input display. `IsShowNewAddForm` controls CSS class (`e-adding` vs `e-editing`) on the `EditForm`.
- **NormalEdit lifecycle**: `OnInitialized` sets `edittemplateData.Data = EditModule.CloneData`. `OnAfterRenderAsync(firstRender)` syncs `EditContext` back to `EditModule` and calls `EditModule.FocusEditableCell(frozenEdit)` on first render.
- **GetCommandUid static generator**: `GridCommandColumn.GetCommandUid(string prefix)` uses a static `int sequence` counter to produce unique UIDs like `"prefix_1"`, `"prefix_2"`. Never reset this counter; only call it during component initialization.
- **SoftRefresh and cell caching**: `GridRow.CellRenderer.cs` caches cell state in `_cellCache[cacheKey]` to avoid re-rendering identical cells. Command column cells must check `Parent.SoftRefresh` or `cell.Changes` flags before skipping re-render.
- **Tab index and focus**: Command column cell `<td>` must have correct `tabindex` based on row/cell position. When `IsEdit == false`, first/last visible cells get `tabindex=0`; otherwise `tabindex=-1`. Command buttons inside must NOT grab focus unless navigated to via Tab key.
- **FocusModule integration**: After `CommandClickHandler` triggers `StartEdit`, the `FocusModule.SetCurrent(row, firstEditableCell)` call moves focus to the first editable cell. If FocusModule is null, no exception — guard with null-conditional `?.`.
- **IsLastCellInternal() logic**: A command column cell is the "last cell" if its the rightmost visible cell AND either (1) focus is on a child template/command, or (2) it's a summary row, or (3) grouping is active and it's the last data cell. Keyboard navigation TAB/Shift-Tab depends on this check — modifications must preserve all three conditions.
- **Config.await(true) on every await**: All async calls in renderer components must use `.ConfigureAwait(true)` to maintain Blazor synchronization context.

---

## Code Location Map
<!-- token-budget: 85 words -->

All Command Column Editing-related code lives at these paths (no code reproduced — paths + one-line purpose only):

| Path | Purpose |
|------|---------|
| `src/GridCommandColumn.cs` | Public API component: `Type` (CommandButtonType), `Title`, `ID`, `ButtonOption` (CommandButtonOptions), `Uid`. Static `GetCommandUid(string prefix)` generates sequential UIDs. `OnInitializedAsync` sets `HasColumnChanges = true` on parent. |
| `src/GridCommandColumns.razor` + `.razor.cs` | Container: holds `List<GridCommandColumn> Commands`; `UpdateChildProperty(GridCommandColumn)` appends to list; `OnInitializedAsync` calls `Parent?.UpdateChildProperties(nameof(Commands), Commands)` to sync to parent `GridColumn` |
| `src/GridEditSettings.cs` | Edit mode config: `AllowEditing`, `AllowAdding`, `AllowDeleting`, `Mode` (Normal/Dialog/Batch), `ShowAddNewRow`, `NewRowPosition`, `ShowDeleteConfirmDialog`, `AllowEditOnDblClick` (default `true`), `AllowNextRowEdit`, `ShowConfirmDialog` (batch confirm, default `true`), `Dialog` (DialogSettings?), `Template` (RenderFragment), `HeaderTemplate` (RenderFragment), `FooterTemplate` (RenderFragment), `Validator` (RenderFragment — custom validation component) |
| `src/GridEvents.cs` | Events: `CommandClicked` → `CommandClickEventArgs<TValue>`; also `RowCreating`/`RowCreated`, `RowUpdating`/`RowUpdated`, `RowDeleting`/`RowDeleted`, `RowEditing`/`RowEdited`, `EditCanceling`/`EditCanceled`, `OnBeginEdit` (cancellable), `OnRowEditStart` (has `PreventDataClone`), `OnActionBegin`/`OnActionComplete` |
| `src/EventModels/Grids.cs` | Models: `CommandClickEventArgs<T>` (Cancel, RowData, EditedData, CommandColumn); `CommandModel` (ButtonOption, Title, Type, ID, Uid); `CommandButtonOptions` (Content, CssClass, Disabled, EnableRtl, IconCss, **IconPosition**, **IsPrimary**, **IsToggle**) |
| `src/Enumeration/GridsEnumerations.cs` | `CommandButtonType` (None/Edit/Delete/Save/Cancel); `EditMode` (Normal/Dialog/Batch); `NewRowPosition` (Top/Bottom); `FreezeDirection` (**Left/Right/Fixed** — no `None` value); `RowDirection` (Horizontal/Vertical); internal `EditAction` (None/Edited/Deleted/Added); internal `SaveActionType` (Edited/Added) |
| `src/Internal/Renderer/CommandColumnRenderer.cs` | **CORE RENDER** (line 26): `RenderCommandColumnCell()` builds TD; `RenderCommandButton()` renders SfButton per command; `GetCommandColumnTextAlign()` resolves alignment; `GetCommandColumnAttributes()` adds `data-cell` for adaptive UI |
| `src/Internal/Renderer/GridRow.CellRenderer.cs` | Cell dispatch: routes `CellType.CommandColumn` → `RenderCommandColumnCell()`. Contains `CommandClickHandler()` (line 1322) — fires `OnRecordClick`, then `CommandClicked` (check `Cancel`), then CRUD switch; fires `RecordClick` via EventAggregator for TreeGrid/FileManager |
| `src/Internal/Renderer/GridRow.razor` | Dispatch per cell type: `CellType.CommandColumn` calls `@RenderCommandColumnCell(cellParams)`. During edit-Normal, calls `<NormalEdit>` |
| `src/Internal/Renderer/NormalEdit.razor` | Edit row: renders Save/Cancel/None buttons inside command cell (Edit/Delete hidden during edit). Uses `<EditForm>`, `<DataAnnotationsValidator>`, `<ColumnsValidator>`, `<ValidationTooltip>`. Supports `EditSettings.Validator` and `EditSettings.Template` overrides. `OnAfterRenderAsync` calls `EditModule.FocusEditableCell()`. |
| `src/Internal/Renderer/AdaptiveDialogRenderer.razor` | Renders responsive filter/sort dialog for mobile (`EnableAdaptiveUI`). Not directly involved in command column CRUD but co-exists with command column in adaptive mode. |
| `src/Internal/Renderer/ValidationTooltip.razor` | Renders validation error tooltip below edited cell. Referenced from `NormalEdit.razor`; requires correct position for frozen/virtual/RTL/adaptive combos. |
| `src/Internal/Renderer/GridAddNewRow.razor` | Renders the `ShowAddNewRow` persistent add row. Uses `PreventShowAddForm`/`IsDisableRow`/`IsShowNewAddForm` flags passed to `NormalEdit`. |
| `src/Internal/Renderer/BatchEdit.razor` | Batch edit cell form; `ShowValidationMessage` callback; validation tooltip handling (~15 lines) |
| `src/Internal/Renderer/DialogEdit.razor` | Dialog edit mode; `HeaderTemplate`, `Template`, `FooterTemplate`, `Column.EditTemplate` rendering (~30 lines) |
| `src/Internal/Actions/Edit.cs` | **HEAVILY RESTRUCTURED (~4244 lines)** — CRUD: `StartEdit(Row)`, `EndEdit()`, `DeleteRecord()`, `CloseEdit()` called by `CommandClickHandler()`. Internal state: `IsAdd`, `IsLastRow`, `IsCancelAction`, `HasBatchChanges`, `CloneData`, `EditRowIndex`, `EditedRow`, `EditContext`. |
| `src/SfGrid.razor.cs` | `SetButtonOptions(GridCommandColumn)` → builds attribute dict (Content, IconPosition, Disabled, EnableRtl, IsPrimary, IsToggle, title — localised); `IsDataModified()` compares original vs cloned cell values for Save/Cancel types only |
| `scripts/edit.ts` | `Edit.createTooltip()` (line 17) — validation tooltip positioning for all feature combos |

---

## Rendering Pipeline Flow Diagrams
<!-- token-budget: 200 words -->

### 1. NORMAL VIEW (No Edit Active)
```
GridRow.razor
│
├─ Loop: foreach Cell in row.Cells
│  │
│  └─ if cell.CellType == CellType.CommandColumn
│     │
│     └─ RenderCommandColumnCell(cellParams)  [GridRow.CellRenderer.cs]
│        │
│        ├─ <td class="e-rowcell e-unboundcell e-rightalign" ...>
│        │  ├─ onclick → CellClickHandlerInternal(e, row, cell, false)
│        │  ├─ onfocus → focusClass = "e-focus e-focused"
│        │  ├─ onblur  → focusClass = "" + FocusModule.ClearFocus()
│        │  ├─ onkeydown → FocusModule.ProcessKeyDown(e, row, cell)
│        │  │
│        │  └─ <div class="e-unboundcelldiv" onclick:stopPropagation>
│        │     │
│        │     └─ foreach command where Type != Save && Type != Cancel
│        │        │
│        │        └─ <SfButton ...>
│        │           │
│        │           onclick → CommandClickHandler(command, row, cell)
│        │           │
│        │           ├─ Fire GridEvents.CommandClicked event (check Cancel)
│        │           │
│        │           ├─ switch (command.Type):
│        │           │  ├─ Edit    → EditModule.StartEdit(row)
│        │           │  ├─ Delete  → SelectRowAsync + DeleteRecordAsync()
│        │           │  ├─ Save    → EditModule.EndEdit()
│        │           │  ├─ Cancel  → EditModule.CloseEdit()
│        │           │  └─ None    → (custom action via event only)
│        │           │
│        │           └─ EventAggregator.Trigger("RowStateChanged", row)
│        │
│        └─ </td>
```

**DOM Output (Normal View)**:
```html
<td class="e-rowcell e-unboundcell e-rightalign" data-uid="cell-uid-123" 
    role="gridcell" tabindex="-1" 
    onclick="..." onfocus="..." onblur="..." onkeydown="...">
    <div class="e-unboundcelldiv" onclick:stopPropagation>
        <SfButton Type="button" CssClass="e-edit-delete e-EditButton" 
                  onclick="CommandClickHandler(Edit, ...)" />
        <SfButton Type="button" CssClass="e-edit-delete e-DeleteButton" 
                  onclick="CommandClickHandler(Delete, ...)" />
        <!-- Custom buttons -->
    </div>
</td>
```

---

### 2. EDIT STATE ACTIVE (Normal Mode - Inline)
```
GridRow.razor
│
├─ if row.IsEdit == true && Mode != Dialog
│  │
│  └─ <NormalEdit TCell Row Cells ...>  [NormalEdit.razor]
│     │
│     ├─ <td class="e-editcell e-normaledit">
│     │  │
│     │  ├─ colspan = (VisibleCols + IndentCount)
│     │  │
│     │  └─ <EditForm EditContext="CloneData">
│     │     │
│     │     ├─ DataAnnotationsValidator / ColumnsValidator
│     │     │
│     │     ├─ foreach Cell in Cells (Edit Row Cells)
│     │     │  │
│     │     │  ├─ if Cell.Column.Commands != null
│     │     │  │  │
│     │     │  │  └─ <td class="e-unboundcell e-rightalign">
│     │     │  │     │
│     │     │  │     └─ <div class="e-unboundcelldiv">
│     │     │  │        │
│     │     │  │        └─ foreach command where Type == Save || Cancel || None
│     │     │  │           │
│     │     │  │           └─ <SfButton Disabled="PreventShowAddForm" ...>
│     │     │  │              │
│     │     │  │              onclick → CommandClickHandler(command, row, cell)
│     │     │  │              │
│     │     │  │              ├─ Save   → EditModule.EndEdit()
│     │     │  │              │          (validates, fires RowUpdating, updates row)
│     │     │  │              │
│     │     │  │              ├─ Cancel → EditModule.CloseEdit()
│     │     │  │              │          (discards cloneData, fires EditCanceling)
│     │     │  │              │
│     │     │  │              └─ None   → (custom action)
│     │     │  │
│     │     │  ├─ else (normal data cell in edit)
│     │     │  │  │
│     │     │  │  └─ <TextBoxEditCell /> | <NumericEditCell /> | <DropDownEditCell /> ...
│     │     │  │     (renders input for field editing)
```

**DOM Output (Edit - Normal Mode)**:
```html
<td class="e-editcell e-normaledit e-edit-frozen" colspan="5">
    <EditForm EditContext="@CloneData" class="e-gridform e-lib e-formvalidator e-editing">
        <DataAnnotationsValidator />
        <ColumnsValidator ... />
        
        <!-- Command cell in edit -->
        <table class="e-table e-inline-edit">
            <tbody>
                <tr>
                    <td class="e-rowcell e-unboundcell e-rightalign" e-mappinguid="cmd-col-uid">
                        <div class="e-unboundcelldiv">
                            <SfButton Type="button" CssClass="e-edit-delete e-SaveButton" .../>
                            <SfButton Type="button" CssClass="e-edit-delete e-CancelButton" .../>
                        </div>
                    </td>
                    <!-- Other edit cells ... -->
                </tr>
            </tbody>
        </table>
    </EditForm>
</td>
```

---

### 3. EDIT STATE - DIALOG MODE
```
GridRow.razor (Normal Command Cell - Edit/Delete visible)
│
└─ RenderCommandColumnCell(cellParams)  [No special handling for dialog]
   │
   └─ Normal TD with Edit/Delete buttons  (Save/Cancel hidden)

PLUS (Parallel Rendering):

DialogEdit.razor
│
└─ <SfDialog @ref="EditDialogInstance" Title="Edit" AllowDragging="true" ...>
   │
   ├─ Dialog Header: "Edit Record" / "Add Record" (from DialogSettings)
   │
   ├─ Dialog Content:
   │  │
   │  ├─ <EditForm EditContext="CloneData">
   │  │  │
   │  │  ├─ <table class="e-inline-edit">
   │  │  │  │
   │  │  │  └─ foreach Column in Columns (except CommandColumn)
   │  │  │     │
   │  │  │     └─ <TextBoxEditCell /> | <NumericEditCell /> ...
   │  │  │        (no command cells in dialog body)
   │  │  │
   │  │  └─ Validation Message Display
   │  │
   │  └─ Dialog Footer:
   │     │
   │     ├─ <SfButton Type="button" CssClass="e-flat" Content="Save" onclick="EndEdit()" />
   │     ├─ <SfButton Type="button" CssClass="e-flat" Content="Cancel" onclick="CloseEdit()" />
   │     └─ Custom footer via FooterTemplate if provided
```

**Interaction**: 
- When Dialog mode is active, row-level command cell remains visible but Save/Cancel never show
- Dialog controls all edit actions (Save/Cancel buttons are in footer)
- `CommandClickHandler` still routes Edit/Delete commands correctly

---

### 4. BATCH EDIT MODE
```
GridRow.razor (Normal View)
│
├─ Loop: foreach Cell in row.Cells
│  │
│  └─ if cell.CellType == CellType.CommandColumn
│     │
│     └─ RenderCommandColumnCell(cellParams)  [SKIPS BatchEdit.razor]
│        │
│        └─ <td> with Edit/Delete buttons
│           (Save/Cancel NOT shown — batch uses toolbar)
│           
           Command cell CANNOT enter batch edit state.
           No inline cell-level edit for command column.

TOOLBAR (Separate):
│
└─ GridToolbar.razor
   │
   ├─ Save Changes button  (enabled if any row.IsDirty == true)
   ├─ Discard Changes button
   ├─ Edit button (routes to: EditCell on selected cell)
   └─ Delete button (routes to: BulkDelete on selected rows)
```

**Key Constraint**: 
- Command column does NOT support cell-level batch editing
- Edit/Delete buttons trigger `StartEdit` or `DeleteRecord` directly
- Save/Cancel are in toolbar only for batch mode

---

### 5. FEATURE: FROZEN COLUMNS
```
Physical Layout (Two Separate Tables):

┌─ Frozen Table (left, sticky) ────┬─ Movable Table (center scroll) ─┐
│                                  │                                │
│ Row 1 │ Command Column (frozen?) │ Col A │ Col B │ Col C │ ...  │
│       │ ┌─────┬─────┐            │ ...   │       │       │      │
│       │ │Edit │Delete│           │                              │
│       │ └─────┴─────┘            │                              │
│       │                          │                              │
└───────┴──────────────────────────┴──────────────────────────────┘

If Column.Freeze == FreezeDirection.Left:
  → Command cell rendered in LEFT table
  → Sticky positioning applied via CSS

If Column.Freeze == FreezeDirection.Right or not set (no freeze):
  → Command cell rendered in MOVABLE table
  → Scrolls with other columns

NOTE: `FreezeDirection` enum has values `Left`, `Right`, `Fixed` ONLY — there is NO `FreezeDirection.None` value in source.
Use `Parent.Columns!.Any(x => x.Freeze == FreezeDirection.Fixed)` to check for Fixed freeze, not `FreezeDirection.None`.
```

**Rendering Change**: 
```csharp
// In CommandColumnRenderer.cs

if (Parent.GetFrozenCount() > 0)
{
    frozenClasses = Parent.ApplyFrozenColumnsClass(cell.Column);
    classNames = $"{classNames} {frozenClasses}";
}

// Generates: "e-rowcell e-unboundcell e-rightalign e-frozenleft" 
//                                                      or e-frozenright
```

---

### 6. FEATURE: VIRTUAL SCROLL + INFINITE SCROLL
```
VirtualContent.razor (Only visible rows in DOM)
│
├─ Compute: RowStartIndex, RowEndIndex based on scroll position
│
├─ GetCells(startColumnIndex?, endColumnIndex?)
│  │
│  └─ Filter Rows: Parent.Rows.Where(r => r.Index >= RowStartIndex && <= RowEndIndex)
│
└─ Render: only visible row <tr> elements
   │
   ├─ Row 50 ──┐
   │           ├─ CommandColumnRenderer.RenderCommandColumnCell(row 50, cell)
   ├─ Row 51 ──┤
   │           └─ NO re-render (cached if cell.Changes == false)
   ├─ Row 52 ──┐
   │           └─ RenderCommandColumnCell(row 52, cell)
   │
   └─ Scroll event → update RowStartIndex/RowEndIndex → re-render visible rows only

When edit state is active in virtual mode:
  → VirtualScrollModule.ScrollToEditedRowAsync(validationErrors)
    → Calls JS: sfBlazor.Grid.scrollIntoView(gridId, rowIndex, -1)
    → Virtual scroll updates RowStartIndex so edited row becomes visible
    → EditCell re-renders in virtual viewport
```

**Command Column in Virtual Mode**: 
- Normal rendering via `RenderCommandColumnCell` (NOT special-cased for virtual)
- Cell click handlers MUST check `Parent.VirtualScrollModule != null` and update `CurrentRowIndex`
- Edit mode: `StartEdit` automatically calls `ScrollToEditedRowAsync` if needed

---

### 7. FEATURE: SHOWADDNEWROW
```
Grid renders with AddNewRow at position (Top or Bottom):

Top Position (NewRowPosition = Top):
┌─ AddNewRow (empty cells + Save/Cancel buttons) ───────────────────┐
│ Command cell: ┌─────┬────────┐                                     │
│              │Save │Cancel │  (PreventShowAddForm=false → enabled) │
│              └─────┴────────┘                                     │
├──────────────────────────────────────────────────────────────────┤
│ Row 0 (data)   │ Edit  Delete  │                                │
│ Row 1 (data)   │ Edit  Delete  │                                │
│ Row 2 (data)   │ Edit  Delete  │                                │
└──────────────────────────────────────────────────────────────────┘

Bottom Position (NewRowPosition = Bottom):
┌──────────────────────────────────────────────────────────────────┐
│ Row 0 (data)   │ Edit  Delete  │                                │
│ Row 1 (data)   │ Edit  Delete  │                                │
│ Row 2 (data)   │ Edit  Delete  │                                │
├──────────────────────────────────────────────────────────────────┤
│ AddNewRow (empty cells + Save/Cancel buttons)                    │
│ Command cell: ┌─────┬────────┐                                   │
│              │Save │Cancel │  (PreventShowAddForm=false → enabled) │
│              └─────┴────────┘                                     │
└──────────────────────────────────────────────────────────────────┘

PreventShowAddForm Flag Logic:
  if (ShowAddNewRow == true && IsAdd == true && Column.Commands != null)
  {
      PreventShowAddForm = false;   // Buttons ENABLED
  }
  else if (ShowAddNewRow == true && NewRowPosition == Bottom && IsEdit == true)
  {
      PreventShowAddForm = true;    // Buttons DISABLED for other rows
  }
  else
  {
      PreventShowAddForm = false;
  }

After Save/Cancel on AddNewRow:
  → EventAggregator.Trigger("ResetAddFormValues", null!)
  → AddNewRow cells cleared, ready for next add
```

---

### 8. FEATURE: GROUPING
```
Grouped Grid Structure:

┌─ Group 1 (Caption Row)                          ────────────────┐
│ ├─ Indent Cell │ Caption "Group A (3)" │ (NO command column)    │
├─────────────────────────────────────────────────────────────────┤
│ ├─ Row 0 (data)     │ Command │ Edit  Delete  │                │
│ ├─ Row 1 (data)     │ Command │ Edit  Delete  │                │
│ ├─ Row 2 (data)     │ Command │ Edit  Delete  │                │
├─────────────────────────────────────────────────────────────────┤
│ ├─ Group Footer (if aggregate)  │ SUM: 50 │ (NO command)      │
├─────────────────────────────────────────────────────────────────┤
│ ├─ Group 2 (Caption Row)        │ "Group B (2)" │             │
│ ├─ Row 3 (data)     │ Command │ Edit  Delete  │                │
│ ├─ Row 4 (data)     │ Command │ Edit  Delete  │                │
└─────────────────────────────────────────────────────────────────┘

Command column rendering:
  → Only data rows render command cells
  → Caption rows, group footer rows use IndentCell or GropuCaptionCell instead
  → CellType.CommandColumn is skipped for GroupCaption, GroupFooter, Indent rows
```

**Edit in Grouped Mode**:
```csharp
// In Edit.cs → GenerateRow for grouped data:

if (Parent.AllowGrouping && Parent.GroupSettings.Columns?.Length > 0)
{
    return new GroupModelGenerator<T>(Parent)
        .GenerateRow(ReflectionExtension.TryCreateInstance<T>(), 0, 
                     indent: Parent.GroupSettings.Columns.Length);
}
// GenerateRow creates cells with extra indent cells for grouping depth
// Command column is generated normally as the last data column
```

---

### 9. ADAPTIVE UI (Mobile)
```
Adaptive Vertical Layout:

Normal (Desktop):
┌─────────────────────────────────────────┐
│ Col A │ Col B │ Col C │ Command Column │
│ value │ value │ value │ Edit │ Delete  │
└─────────────────────────────────────────┘

Adaptive (Mobile - Vertical):
┌────────────────────────┐
│ Col A: value           │
├────────────────────────┤
│ Col B: value           │
├────────────────────────┤
│ Col C: value           │
├────────────────────────┤
│ Command Column:        │
│ [Edit] [Delete]        │  (stacked vertically)
└────────────────────────┘
```

**Rendering Change** (CommandColumnRenderer.cs):
```csharp
if (Parent.EnableAdaptiveUI && Parent.RowRenderingMode.Equals(RowDirection.Vertical))
{
    attributes["data-cell"] = cell.Column?.HeaderText ?? "Command";
    // Adds: data-cell="Command" for mobile CSS to display label
}
```

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 180 words -->

> Built from live codebase + `/docs/training/` risk assessment.
> Omitted pairs have no interaction risk.

| Combination | Must Preserve | Risk |
|---|---|---|
| Command + Editing (Normal mode) | `NormalEdit.razor` renders Save/Cancel/None buttons in command cell. `PreventShowAddForm` disables buttons when `ShowAddNewRow = true`. `CommandClickHandler` routes to `StartEdit/EndEdit/CloseEdit`. | Critical |
| Command + Editing (Dialog mode) | Dialog renders separate from row-level command cell. Command cell shows Edit/Delete only. Dialog footer has Save/Cancel. Both must fire correct events (`RowUpdating`, `EditCanceling`). | Critical |
| Command + Editing (Batch mode) | Command column does NOT enter batch cell edit. Edit/Delete buttons trigger row-level actions. Toolbar has Save/Discard. `CommandClickHandler` must NOT spawn inline batch cell edit. | Critical |
| Command + Validation Tooltip | `edit.ts → createTooltip()` (line 17) positions validation tooltip relative to edited cell after `EditContext.Validate()` fails. Must account for: frozen rows/columns, virtual scroll viewport, show-add-new-row position, RTL direction, adaptive UI. Missing position scenario causes tooltip off-screen. | Critical |
| Command + Virtual Scroll | Command cell rendered only when row is visible. `CurrentRowIndex` updated on click to track position. `ScrollToEditedRowAsync` called after `StartEdit` if row off-screen. Cell cache `_cellCache["{row.Uid}_{cell.Uid}"]` persists across scroll events if `Parent.SoftRefresh == false`. | High |
| Command + Frozen Columns | `ApplyFrozenColumnsClass(column)` applied to command cell. If column frozen-left, renders in left table. Keyboard focus must skip frozen column boundaries correctly when Tab key navigates from command cell. | High |
| Command + Grouping | Command cell only in data rows, NOT group captions/footers. `GenerateRow` for grouped data includes indent cells before command column. `CellIndex` calculation accounts for indent depth. | High |
| Command + Selection | `SelectRowAsync` called before `DeleteRecordAsync`. Command cell click does NOT trigger row selection unless configured. After delete via command, `ClearSelectionAsync` called if `PersistSelection == false`. | High |
| Command + ShowAddNewRow | `PreventShowAddForm` flag controls button enabled state. Add row is NOT marked for delete on Cancel. `ResetAddFormValues` triggered after Save/Cancel on add row. Must preserve "prevent accidental save" intent. | High |
| Command + Infinite Scroll | `DeleteRecordAsync` calls `InfiniteScrollModule.ResetInfiniteProperties` after delete. `StartEdit` does NOT trigger infinite scroll cache invalidation. Row indexes managed by module, command cell click updates `CurrentRowIndex`. | Medium |
| Command + Sort | `CommandClickHandler` does NOT directly trigger sort. Sort module listens to `DataBound` event. After sort, row indexes shift — command cells are re-rendered with new row indexes automatically. No persist required. | Medium |
| Command + Filter | `CommandClickHandler` not affected by filter state. `DeleteRecordAsync` removes row from filtered view. After filter change, command cells re-rendered for remaining visible rows. | Medium |
| Command + Export | Export captures `SelectedRecords` BEFORE edit is triggered. If command Edit clicked, export reads OLD data. No synchronization required. | Low |
| Command + Master-Detail | Command column is data-row–level only. Detail rows do NOT have command cells. Expanding detail row does NOT affect command cell in parent row. | Low |

---

## Command Button Types & State Transitions
<!-- token-budget: 80 words -->

```
┌─────────────────────────────────────┐
│  CommandButtonType Enum             │
├─────────────────────────────────────┤
│ None   → Custom action (event only) │
│ Edit   → Start edit on row          │
│ Delete → Delete row                 │
│ Save   → Commit edit (validate)     │
│ Cancel → Discard edit               │
└─────────────────────────────────────┘

Button Visibility Rules:

NORMAL VIEW (IsEdit == false):
  ✓ Edit     (always shown)
  ✓ Delete   (always shown)
  ✗ Save     (hidden)
  ✗ Cancel   (hidden)
  ✓ None     (shown if configured)

EDIT STATE - Normal Mode:
  ✗ Edit     (hidden)
  ✗ Delete   (hidden)
  ✓ Save     (shown, unless PreventShowAddForm)
  ✓ Cancel   (shown, unless PreventShowAddForm)
  ✓ None     (shown if configured)

EDIT STATE - Dialog Mode:
  ✓ Edit     (shown, triggers dialog — if clicked during edit)
  ✓ Delete   (shown, triggers delete — if clicked during edit)
  ✗ Save     (hidden — in dialog footer instead)
  ✗ Cancel   (hidden — in dialog footer instead)
  ✓ None     (shown if configured)

EDIT STATE - Batch Mode:
  ✓ Edit     (shown → triggers inline cell edit)
  ✓ Delete   (shown → marks row deleted)
  ✗ Save     (hidden — in toolbar)
  ✗ Cancel   (hidden — in toolbar)
  ✓ None     (shown if configured)
```

---

## JavaScript Interop
<!-- token-budget: 100 words -->

### C# → TypeScript: Validation Tooltip Positioning

**TypeScript Function:**
```typescript
// scripts/edit.ts line 17
public createTooltip(results: object[], isAdd: boolean): void
```

**Call Flow:**
1. C# `EditContext.Validate()` fails → generates `ErrorResult` list in `Edit<T>.cs`
2. `ValidationTooltip.razor` renders error divs with CSS positions
3. TypeScript `Edit.createTooltip()` calculates tooltip position based on:
   - Frozen rows/columns offsets
   - Virtual scroll viewport position
   - ShowAddNewRow position (Top/Bottom)
   - RTL direction
   - Adaptive UI mode
4. Returns position dictionary → stored in `EditModule.position[columnUid]`

**Responsibility Split:**

| Layer | Responsibility |
|-------|----------------|
| **C# (Edit.cs, ValidationTooltip.razor)** | Validation logic, error message collection, tooltip DOM structure |
| **TypeScript (edit.ts)** | Pixel-perfect positioning calculation for all feature combos, DOM measurement |

**CRITICAL Rules:**
1. **Never calculate tooltip position in C#** — feature combo offsets (frozen, virtual, RTL) require precise DOM measurements only available in TypeScript
2. **Validation errors drive rendering** — `EditModule.ErrorResult` is the single source of truth; tooltip shows only if field in error list
3. **Position cache keyed by column UID** — `EditModule.position[columnUid]` persists until validation passes or edit canceled
4. **Arrow position flag** — `EditModule.ArrowPosition` ("top"|"bottom") controls tooltip arrow direction based on `IsLastRow` or `NewRowPosition.Bottom`

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

```
Mode: {feature-implementation | bug-fix}
Skill: Command Column Editing & Rendering
Component: SfGrid<TValue> — Syncfusion.Blazor.Grids

=== AGENT IDENTITY ===
You are a Code AI for the Syncfusion Blazor DataGrid Command Column feature.
Scope: Internal/Renderer/CommandColumnRenderer.cs, GridRow.CellRenderer.cs,
       Internal/Renderer/NormalEdit.razor, Edit.cs, scripts/edit.ts, and related surfaces only.

=== WHAT YOU MUST KNOW (pre-loaded) ===

PUBLIC API:
  - GridCommandColumn: Type (CommandButtonType), Title, ID, ButtonOption (CommandButtonOptions), Uid
  - GridEditSettings: AllowEditing, AllowAdding, AllowDeleting, Mode (Normal/Dialog/Batch),
                      ShowAddNewRow, NewRowPosition, ShowDeleteConfirmDialog,
                      AllowEditOnDblClick (default true), AllowNextRowEdit, ShowConfirmDialog (batch, default true),
                      Dialog (DialogSettings?), Template, HeaderTemplate, FooterTemplate, Validator (RenderFragment)
  - CommandButtonType enum: None, Edit, Delete, Save, Cancel
  - CommandButtonOptions: Content, CssClass, Disabled, EnableRtl, IconCss, IconPosition, IsPrimary, IsToggle
  - FreezeDirection enum: Left, Right, Fixed (NO None value — use absence of freeze to mean unset)
  - GridEvents: CommandClicked → CommandClickEventArgs<T> (Cancel, RowData, EditedData, CommandColumn)
  - Additional events in Command Column flow: OnBeginEdit (Cancel), OnRowEditStart (PreventDataClone),
    RowCreating/RowCreated, RowUpdating/RowUpdated, RowDeleting/RowDeleted, RowEditing/RowEdited,
    EditCanceling/EditCanceled, OnActionBegin/OnActionComplete

CORE RENDERING ENTRY POINTS:
  - RenderCommandColumnCell(cellParameters) → renders <td> with command buttons [CommandColumnRenderer.cs]
  - RenderCommandButton(builder, ref seq, row, cell, command) → renders individual SfButton via SfButton component
  - GetCommandColumnTextAlign(cell) → resolves alignment class (defaults to e-rightalign; overridden by Column.TextAlign)
  - GetCommandColumnAttributes(cell, row) → adds data-cell attribute for adaptive vertical mode
  - CommandClickHandler(command, row, cell) → (1) fires OnRecordClick if subscribed / TreeGrid / FileManager;
      (2) computes IsDataModified for EditedData; (3) fires CommandClicked (check Cancel);
      (4) switch on command.Type → StartEdit / SelectRow+DeleteRecord / EndEdit / CloseEdit [NormalEdit.razor / GridRow.CellRenderer.cs]
  - SetButtonOptions(GridCommandColumn) → builds attribute dict: Content, IconPosition, Disabled, EnableRtl, IsPrimary, IsToggle, title (localized) [SfGrid.razor.cs]
  - IsDataModified(Row, CloneData, column) → compares field values; returns false for Edit/Delete/None types [SfGrid.razor.cs]
  - edit.ts → createTooltip(results[], isAdd) → positions validation error divs

CRITICAL BEHAVIORAL RULES:
  1. Normal View: Show Edit/Delete/None buttons; Hide Save/Cancel
  2. Edit State (Normal mode): Hide Edit/Delete; Show Save/Cancel/None (unless PreventShowAddForm=true)
  3. Edit State (Dialog mode): Show Edit/Delete/None in row; Save/Cancel in dialog footer only
  4. Edit State (Batch mode): Show Edit/Delete/None; Hide Save/Cancel (use toolbar)
  5. PreventShowAddForm: disables Save/Cancel buttons when ShowAddNewRow active
  6. CommandClickEventArgs: Check Cancel property; if Cancel=true, skip CRUD action
  7. EditedData: populated ONLY for Save/Cancel types (check via IsDataModified())
  8. Button disabled state: derives from column.ButtonOption.Disabled OR PreventShowAddForm logic
  9. Focus/Tab navigation: command cell must track tabindex based on row position; buttons don't steal focus
  10. Validation tooltip (TS): must account for frozen rows/columns, virtual scroll, showAddNewRow, RTL, adaptive UI
  11. CommandButtonOptions full property set: Content, CssClass, Disabled, EnableRtl, IconCss, IconPosition (default Left), IsPrimary, IsToggle — all passed via SetButtonOptions() attribute dict
  12. FreezeDirection has NO None value: valid values are Left, Right, Fixed only — never compare `== FreezeDirection.None`
  13. OnRecordClick fires before CommandClicked if GridEvents.OnRecordClick.HasDelegate == true or IsRenderedFromTreeGrid/FileManager
  14. GridEditSettings.Validator (RenderFragment): if set, replaces DataAnnotationsValidator in NormalEdit form
  15. GridEditSettings.Template (RenderFragment): if set, replaces the inline edit table in NormalEdit/Dialog — command buttons still render via command cell

EVENT FLOW:
  - Click Edit button   → OnRecordClick? → CommandClicked (check Cancel) → StartEdit()
                          → OnBeginEdit (Cancel?) → OnRowEditStart (PreventDataClone?) → RowEditing → RowEdited
  - Click Delete button → OnRecordClick? → CommandClicked (check Cancel) → SelectRow + DeleteRecord()
                          → RowDeleting → RowDeleted → OnActionBegin/OnActionComplete
  - Click Save button   → OnRecordClick? → CommandClicked (check Cancel) → EndEdit()
                          → Validate → RowUpdating/RowCreating → RowUpdated/RowCreated → OnActionBegin/OnActionComplete
  - Click Cancel button → OnRecordClick? → CommandClicked (check Cancel) → CloseEdit()
                          → EditCanceling → EditCanceled
  NOTE: If CommandClickEventArgs.Cancel = true, NO CRUD action is performed.

RENDER STATE DEPENDENCIES:
  - cell.CellType == CellType.CommandColumn (routed via GridRow.razor)
  - row.IsEdit (determines Save/Cancel vs Edit/Delete visibility)
  - Parent.EditSettings.Mode (Normal/Dialog/Batch → different render paths)
  - Column.Commands (list of GridCommandColumn objects)
  - Parent.SoftRefresh (re-render trigger via EventAggregator)
  - cell.Changes (re-render flag for cell cache)
  - Parent.EnableAdaptiveUI && Parent.RowRenderingMode == RowDirection.Vertical (adds data-cell attribute)
  - Parent.GetFrozenCount() > 0 || Columns.Any(x => x.Freeze == FreezeDirection.Fixed) (applies frozen CSS classes)
  - EditModule.IsAdd (distinguishes add vs edit for RowCreating vs RowUpdating event path)
  - EditModule.CloneData (the working copy of the edited row — field values for IsDataModified check)
  - PreventShowAddForm / IsDisableRow / IsShowNewAddForm (flags controlling NormalEdit button state and form CSS)

=== BEFORE YOU MAKE ANY CHANGE ===
1. Verify which render path applies (Normal/Edit Normal/Dialog/Batch).
2. Check PreventShowAddForm logic if ShowAddNewRow is active.
3. Confirm all event handlers fire via EventAggregator (NOT StateHasChanged).
4. Confirm validation tooltip position tested for ALL feature combos: frozen, virtual, grouping, adaptive, RTL.
5. Check cell focus state after command click (must maintain correct tabindex).

=== CONSTRAINTS (all mandatory) ===
- No behavior change outside stated scope
- No new public API without explicit task authorization
- Zero analyzer warnings; XML comments on all new/modified public and internal members
- All await calls use .ConfigureAwait(true)
- No direct StateHasChanged() — set SoftRefresh = true and trigger via EventAggregator
- RenderTreeBuilder sequence counters MUST increment correctly
- No direct JSRuntime.InvokeAsync — route through GridJSInteropAdaptor<T>
- Follow naming-conventions.md (PascalCase methods, _camelCase private, TValue public / T internal)
- Follow error-handling.md (guard clauses first, no silent catch blocks)

=== SCOPE ===
{Describe the exact method(s) to implement or fix — one method per task}

=== INPUT ===
{Paste extracted code chunk — see training/04-code-processing/optimal-chunking-strategies.md}

=== OUTPUT ===
1. Root cause / design rationale (3–5 sentences)
2. Modified method(s) only — no full-file reproduction
3. Interaction risk flags (reference Interaction Matrix above)
4. Required test cases (Given-When-Then format)

After implementation, run regression verification using:
/docs/ai-agents/prompts/regression-verification-prompt.md
```
