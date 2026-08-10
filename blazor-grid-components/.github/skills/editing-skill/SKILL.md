---
name: editing-skill
description: Expert knowledge for the Editing feature (CRUD operations) in the Syncfusion Blazor DataGrid. Use this skill for any feature-implementation or bug-fix task scoped to editing behaviour, including Normal/Dialog/Batch modes, add/update/delete operations, validation, cell editors, cross-feature interaction guarantees with virtualization, grouping, filtering, sorting, paging, and selection.
---

# Skill Instructions
<!-- token-budget: 20 words -->

**Purpose**  
Expert knowledge for Editing (CRUD) in the Blazor DataGrid. Guarantees zero breakage with any other feature.

---

**Agent Invocation**  
Load this skill when implementing CRUD operations, debugging edit mode issues, adding custom cell editors, or fixing regressions involving editing with virtualization, grouping, filtering, sorting, paging, validation, or selection.

---

## Knowledge References
<!-- token-budget: 60 words -->

- `/docs/training/00-START-HERE.md` — Component overview, #1 rule (never break existing behavior)
- `/docs/training/01-getting-started/architecture-overview.md` — 4-layer architecture, module injection, EventAggregator pattern
- `/openspec/specs/editing/spec.md` — Complete editing specification
- `/docs/architecture/system-architecture.md` — Edit<T> module lifecycle, event flow, PropertyChanges tracking
- `/docs/tech-stack/tech-stack.md` — Blazor patterns, C# version constraints, async/await rules, EditContext/DataAnnotationsValidator
- `/docs/code-guidelines/coding-standards.md` — Async patterns, ConfigureAwait rules, guard clauses, event pipeline

---

## Training Insights Applied
<!-- token-budget: 80 words -->

- **Never break existing behavior** — edit must clear selection unless `PersistSelection = true`, scroll to edited row when virtualized, respect column editability rules, handle validation errors
- **Module communication via EventAggregator** — `ModelChanged(RequestType = Save/Delete/BeginEdit/Cancel)` triggers cross-module notifications
- **PropertyChanges dictionary drives incremental updates** — `AllowEditing` / `EditSettings` change triggers toolbar re-render
- **Action event pipeline mandatory** — `OnActionBegin` → user cancellation check → execute → `OnActionComplete`; multiple cancellable events (OnRowEditStart, RowEditing, RowCreating, RowUpdating, EditCanceling, RowDeleting)
- **Guard clauses first** — validate `AllowEditing`, `AllowAdding`, `AllowDeleting`, column editability before any operation
- **EditContext integration** — Blazor's `EditContext` + `DataAnnotationsValidator` for form validation; complex fields get separate contexts in `ComplexEditContext`

---

## Code Location Map
<!-- token-budget: 80 words -->

- **`Internal/Actions/Edit.cs`** — Core editing engine (`Edit<T>`): `StartEdit` (line 2128), `AddRecord` (line 114), `EndEdit` (line 2270), `CloseEdit` (line 2572), `DeleteRecord` (line 592), `EditCell`, `SaveCell`, `BatchSave`, `BatchClose`
- **`GridEditSettings.cs`** — Settings component (line 12): `AllowEditing` (line 73), `AllowAdding` (line 33), `AllowDeleting` (line 41), `Mode`, `NewRowPosition`, `AllowEditOnDblClick`, `ShowAddNewRow`, `ShowConfirmDialog`
- **`Internal/Editors/EditorCellBase.cs`** — Base class for all cell editors (line 18)
- **`Internal/Editors/TextBoxEditCell.razor`** — String editor
- **`Internal/Editors/NumericEditCell.razor`** — Number editor
- **`Internal/Editors/DatePickerEditCell.razor`** — Date/DateOnly editor
- **`Internal/Editors/TimePickerEditCell.razor`** — TimeOnly editor
- **`Internal/Editors/BooleanEditCell.razor`** — Boolean editor
- **`Internal/Editors/DropDownEditCell.razor`** — Enum/list editor
- **`Internal/Editors/ForeignKeyEditCell.razor`** — Foreign key editor
- **`Internal/Editors/ColumnsValidator.razor`** — Validation integration
- **`SfGrid.Methods.cs`** — Public API: `StartEditAsync` (line 3309), `AddRecordAsync` (line 47), `EndEditAsync` (line 850), `DeleteRecordAsync` (line 724), `EditCellAsync`, `GetBatchChangesAsync`, `ApplyBatchChangesAsync`, `CloseEditAsync`, `GetEditContextAsync` (line 3907)

---

## Interaction Matrix (MANDATORY)
<!-- token-budget: 150 words -->
> Built from live feature folders + /docs/training/ cross-reference  
> Omit pairs with no interaction risk.

| Combination | Must Preserve | Risk |
|-------------|---------------|------|
| Editing + Virtualization | Edit row scrolled into view if outside viewport via `ScrollToEditedRowAsync`; virtual cache updated after save/delete | Critical |
| Editing + Infinite Scrolling | Edit row scrolled into view via `sfBlazor.Grid.scrollIntoView` JS interop; infinite buffer updated after save/delete | High |
| Editing + Grouping | Caption rows non-editable; data rows editable; group state preserved if `PersistGroupState = true` | High |
| Editing + Filtering | Edit operates on filtered data; save/delete triggers data reload with filter predicates intact | High |
| Editing + Sorting | Edit operates on sorted data; save/delete preserves sort order; Tab/Shift+Tab navigation respects sorted row order | High |
| Editing + Paging | Edit on current page; save/delete updates page data; Add respects `NewRowPosition` within page | High |
| Editing + Selection | Selection cleared after save/delete unless `GridSelectionSettings.PersistSelection = true` | High |
| Editing + Validation | `EditContext.Validate()` blocks save if validation fails; validation messages rendered; `ErrorResult` tracks failures | Critical |
| Editing + Frozen Columns | Edit form spans frozen and movable sections; frozen column editors rendered in correct section | Medium |
| Editing + Aggregates | Save/delete triggers aggregate recalculation via `ReactiveAggregateModule` | Medium |

---

## JavaScript Interop
<!-- token-budget: 100 words -->

**C# → JavaScript Call Flow**

| C# Location | Line | JS Function | TS Location | Purpose |
|-------------|------|-------------|-------------|---------|
| `Edit.cs` → `StartEdit` | 232 | `scrollIntoView` | `sf-grid.ts:1050` | Scroll edited row into viewport |
| `Edit.cs` → `AddRecord` | 2361 | `scrollIntoView` | `sf-grid.ts:1050` | Scroll new row into viewport |
| `Edit.cs` → `EndEdit` | 343 | `frozenHeight` | `sf-grid.ts:142` | Recalculate frozen row heights |
| `Edit.cs` → `UpdateEditCell` | 1647 | `focusCell` | `sf-grid.ts:764` | Focus specific cell in edit form |
| `Edit.cs` → `KeyDownHandler` | 2538 | `focusFilterBar` | `sf-grid.ts:489` | Focus filter bar after Tab key |

**TypeScript Functions (scripts/sf-grid.ts)**

```typescript
scrollIntoView(dataId: string, columnIndex: number, rowIndex: number, rowHeight: number, isAddBottom: boolean, isFromAddForm: boolean)  // Line 1050
frozenHeight(dataId: string, options: IGridOptions)  // Line 142
focusCell(dataId: string, field: string, isAdd: boolean, frozenEdit: boolean)  // Line 764
focusFilterBar(dataId: string, keyCombination: string, isFilterTemplate: boolean, index: number)  // Line 489
updateOptions(dataId: string, options: IGridOptions)  // Line 169
```

**Responsibility Split**

| Concern | C# (Edit.cs) | JavaScript (sf-grid.ts) |
|---------|--------------|-------------------------|
| Edit state management | ✅ Row.IsEdit, Cell.IsEdit flags | ❌ |
| Validation | ✅ EditContext.Validate() | ❌ |
| Scroll calculation | ❌ | ✅ Calculate viewport offset |
| Focus management | ❌ | ✅ DOM focus() calls |
| Frozen height sync | ❌ | ✅ Measure and apply heights |

**CRITICAL Rules**
1. **Always ConfigureAwait(true)** — JS interop requires Blazor synchronization context
2. **Scroll before edit** — Call `scrollIntoView` before entering edit mode when virtualized/infinite scroll enabled
3. **Focus after render** — Call `focusCell` after Blazor completes render cycle (use `Task.Yield()`)
4. **Frozen height sync** — Call `frozenHeight` after any row height change (add/edit/delete)

---

## Prompt Template
<!-- token-budget: 300 words — self-contained, no external doc reads required -->

**Mode**: `{feature-implementation | bug-fix}`  
**Skill**: Editing  

**Context**  
You are implementing or fixing Editing (CRUD operations) for the Syncfusion Blazor DataGrid (`SfGrid<TValue>`). Editing supports three modes: Normal (inline row), Dialog, and Batch (cell-level). All editing logic executes server-side in .NET via the `Edit<T>` action module.

**Core Architecture**  
- **Module**: `Edit<T>` in `Internal/Actions/Edit.cs`
- **Settings**: `GridEditSettings` — `AllowEditing`, `AllowAdding`, `AllowDeleting`, `Mode`, `NewRowPosition`, `AllowEditOnDblClick`, `ShowAddNewRow`, `ShowConfirmDialog`
- **Cell Editors**: `TextBoxEditCell`, `NumericEditCell`, `DatePickerEditCell`, `TimePickerEditCell`, `BooleanEditCell`, `DropDownEditCell`, `ForeignKeyEditCell`
- **Validation**: Blazor `EditContext` + `DataAnnotationsValidator`; `ErrorResult` tracks validation failures
- **Public API**: `StartEditAsync`, `AddRecordAsync`, `EndEditAsync`, `DeleteRecordAsync`, `CloseEditAsync`, `EditCellAsync`, `GetBatchChangesAsync`, `ApplyBatchChangesAsync`

**Key Behaviors**  
1. **Master switches**: `AllowEditing = true` enables edit; `AllowAdding = true` enables add; `AllowDeleting = true` enables delete.
2. **Edit modes**: `EditMode.Normal` (inline row), `EditMode.Dialog` (dialog popup), `EditMode.Batch` (cell-level).
3. **StartEdit lifecycle**: `OnActionBegin(BeforeBeginEdit)` → `OnRowEditStart` (cancellable) → `OnBeginEdit` → `OnActionBegin(BeginEdit)` → `RowEditing` (cancellable) → row state change → `OnActionComplete` → `RowEdited`.
4. **AddRecord lifecycle**: `OnActionBegin(Add)` → `RowCreating` (cancellable) → insert row → clear selection (unless `PersistSelection = true`) → `OnActionComplete` → `RowCreated`.
5. **EndEdit (Save) lifecycle**: Validation → `OnActionBegin(Save)` → `RowUpdating` (cancellable) → `ModelChanged` → restore selection → `OnActionComplete` → `RowUpdated`.
6. **CloseEdit (Cancel) lifecycle**: `OnActionBegin(Cancel)` → `EditCanceling` (cancellable) → row state reset → `OnActionComplete` → `EditCanceled`.
7. **DeleteRecord lifecycle**: `OnActionBegin(Delete)` → `RowDeleting` (cancellable) → `ModelChanged` → `OnActionComplete` → `RowDeleted`.
8. **Column editability**: Not editable if `AllowEditing = false` (unless adding + `AllowAdding = true`), `IsPrimaryKey = true` (unless adding), `IsIdentity = true`, `AllowAdding = false` (when adding), or not `Visible`.
9. **Batch mode**: Cell edit via `EditCell` → `SaveCell`; changes tracked as dirty; `BatchSave` commits all; `BatchClose` discards all; `ShowConfirmDialog` prompts before commit/discard.

**Cross-Feature Rules** (from Training + Interaction Matrix)  
- **Virtualization**: `ScrollToEditedRowAsync` scrolls edited row into view if outside viewport; virtual cache updated after save/delete.
- **Infinite Scrolling**: `sfBlazor.Grid.scrollIntoView` JS interop scrolls edited row into view; infinite buffer updated after save/delete.
- **Grouping**: Caption rows non-editable; data rows editable; `PersistGroupState = true` retains expand/collapse state across edit.
- **Filtering**: Edit operates on filtered data; save/delete triggers reload with filter predicates intact.
- **Sorting**: Edit operates on sorted data; save/delete preserves sort order; Tab/Shift+Tab navigation respects sorted row order.
- **Paging**: Edit on current page; save/delete updates page data; Add respects `NewRowPosition` within page.
- **Selection**: Selection cleared after save/delete unless `PersistSelection = true`.
- **Validation**: `EditContext.Validate()` blocks save; validation messages rendered; `ErrorResult` tracks failures.
- **Aggregates**: Save/delete triggers `ReactiveAggregateModule` recalculation.

**API Contracts**  
- **`StartEditAsync(int? rowIndex)`**: Enter edit mode for row; no-op if `AllowEditing = false` or row already editing.
- **`AddRecordAsync(TValue data, int? index)`**: Add new record; no-op if `AllowAdding = false` or already editing.
- **`EndEditAsync()`**: Save edited record; validation blocks if `EditContext.Validate()` fails.
- **`DeleteRecordAsync(string fieldName, TValue data)`**: Delete record; no-op if `AllowDeleting = false`.
- **`CloseEditAsync()`**: Cancel edit; discard changes; reset row state.
- **`EditCellAsync(int rowIndex, string fieldName)`**: Batch mode only; enter cell edit state.
- **`GetBatchChangesAsync()`**: Batch mode only; return `BatchChanges<TValue>` with added/changed/deleted records.
- **`ApplyBatchChangesAsync(BatchChanges<TValue> changes)`**: Batch mode only; apply batch state visually (not persisted until `EndEditAsync`).

**EditMode-Specific**  
- **Normal**: Inline edit form rendered as table row; `Row.IsEdit = true`; Tab/Shift+Tab navigate rows after save.
- **Dialog**: Edit form in `SfDialog`; `ShowDialog` event via EventAggregator; modal interaction.
- **Batch**: Cell-level editing; `Cell.IsEdit = true`; dirty tracking per cell/row; `HasBatchChanges` flag; `ShowConfirmDialog` prompts before save/cancel.

**Event Payloads**  
- **OnRowEditStart**: `RowIndex`, `Data`, `IsFrozen`, cancellable via `args.Cancel`.
- **RowEditing**: `Data`, `RequestType`, cancellable.
- **RowCreating**: `Data`, `RequestType`, cancellable.
- **RowUpdating**: `Data`, `PreviousData`, `Action` (Edit/Add), cancellable.
- **EditCanceling**: `Data`, cancellable.
- **RowDeleting**: `Data`, `Cancel`, cancellable.
- **OnCellEdit** (Batch): `RowIndex`, `Field`, `Value`, cancellable.
- **OnCellSave** (Batch): `RowIndex`, `Field`, `Value`, `PreviousValue`, cancellable.
- **OnBatchSave** (Batch): `BatchChanges`, cancellable.
- **OnBatchCancel** (Batch): cancellable.

**Edge Cases**  
- `AllowEditing = false` → all edit APIs no-op; no exception.
- `AllowEditOnDblClick = false` → double-click does not trigger edit.
- `ShowAddNewRow = true` → empty add form row always visible (independent of normal add flow).
- Primary key column → not editable on edit; editable on add.
- Identity column → never editable.
- `AllowEditing = false` + `AllowAdding = true` → column editable during add only.
- Validation failure → `EditContext.Validate()` returns false; validation messages rendered; save blocked.
- Complex fields (dot-notation) → separate `EditContext` per parent object in `ComplexEditContext`.
- Batch mode + `ShowConfirmDialog = true` → confirmation dialog before save/cancel.
- Batch mode + escape key → immediate cancel without confirmation.
- Virtualization + edit outside viewport → `ScrollToEditedRowAsync` called before/after validation.
- Tab from last cell in row → save → select next row.
- Shift+Tab from first cell in row → save → select previous row.
- Add with default values → `GridColumn.DefaultValue` applied to new record via `SetDefaultValue`.
- Delete by data → primary key lookup to find matching row; marked as deleted.

**Before Making Changes**  
1. Read `/docs/training/` — understand event pipeline, EditContext lifecycle, validation integration, cross-feature interaction.
2. Verify guard clauses: `AllowEditing`, `AllowAdding`, `AllowDeleting`, column editability rules before mutation.
3. Ensure `ConfigureAwait(true)` on all async calls (Blazor context required).
4. Test with virtualization, infinite scrolling, grouping, filtering, sorting, paging, validation enabled.
5. Verify selection cleared after save/delete unless `PersistSelection = true`.
6. Validate scroll-to-edited-row when virtualization/infinite scrolling enabled.

**Deliverables**  
- Implementation follows 4-layer architecture (Infrastructure → Data → Business → Presentation).
- All public methods/properties have XML doc comments with `<summary>`, `<param>`, `<returns>`, `<remarks>`.
- Event pipeline complete: multiple cancellable events in correct order.
- `EditContext` created/disposed correctly; complex fields get separate contexts.
- Validation integration functional; `ErrorResult` tracked; messages rendered.
- Selection cleared after save/delete unless `PersistSelection = true`.
- Scroll-to-edited-row when virtualization/infinite scrolling enabled.
- Batch mode dirty tracking, `BatchSave`, `BatchClose` functional.
- PR references regression verification checklist in `/docs/ai-agents/prompts/regression-verification-prompt.md`.
