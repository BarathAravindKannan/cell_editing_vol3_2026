---
name: clipboard-skills
description: Expert knowledge for the Clipboard feature in the Blazor Grid. Covers copy/paste operations, DOM textarea integration, JavaScript-C# interop, and cross-feature compatibility with Selection, Editing, Frozen Columns, Virtualization, and Filtering.
---

# Clipboard Skill

<!-- token-budget: 25 words -->

## Purpose

Expert knowledge for **Clipboard (Copy/Paste)** operations in the Blazor Grid. Guarantees correct DOM rendering, event sequencing, and no breakage across Selection, Editing, Frozen Columns, Virtualization, and Filtering features.

---

## Agent Invocation

**Mode:** `feature-implementation` or `bug-fix`  
**Skill:** Clipboard  
**When to Load:** Any task involving copy/paste functionality, clipboard text handling, event interception, or copy/paste interaction with other grid features.  
**Key Responsibilities:**
- Verify DOM textarea lifecycle and hidden state
- Validate copy data collection logic against selection mode (Row/Cell)
- Ensure paste routing through Batch Edit module
- Check event cascade (`BeforeCopyPaste` → `BeforeCellPaste`)
- Validate frozen column + virtual row offset handling in paste loops
- Test multiline cell paste (quoted newlines)

---

## Knowledge References

<!-- token-budget: 70 words -->

**Primary Sources:**
- `scripts/clipboard.ts` — Clipboard class, all public methods, DOM manipulation
- `scripts/sf-grid-fn.ts` — Module instantiation, keyboard handlers, integration
- `scripts/sf-grid.ts` — JS interop bridge methods
- `src/Internal/Base/GridJSInteropAdaptor.cs` — `InvokeCopyPasteAction`, `InvokePasteAction` interop
- `src/EventModels/Grids.cs` — `BeforeCopyPasteEventArgs`, `BeforeCellPasteEventArgs<T>`
- `src/GridEvents.cs` — Event callbacks definition
- `src/SfGrid.Methods.cs` — `CopyAsync()` public API

---

## Training Insights Applied

<!-- token-budget: 100 words -->

**Key Rules Discovered:**
1. **DOM Lifecycle:** Hidden textarea (`e-clipboard`) MUST be appended during `Clipboard` constructor, persists for entire grid lifetime (never destroyed/recreated).
2. **Copy is unconditional:** `copy()` / `setCopyData()` (`scripts/clipboard.ts:262`, `:138`) have **no** `allowEditing` / `editMode` gating — copy works in any selection mode whenever `navigator.clipboard` is available. (Only *paste* is gated.) When the user has an active DOM text selection inside `.e-grid`, copy writes `window.getSelection().toString()` to the clipboard instead of the grid selection (text-selection fallback path, `scripts/clipboard.ts:195-210`).
3. **Paste Gating:** Requires `allowEditing=true` AND `editMode='Batch'` AND `selectionMode='Cell'` AND `cellSelectionMode!='Flow'`.
4. **Multiline Handling:** If paste data wrapped in quotes, treat as single cell with encoded newlines (`\n` → newline).
5. **Frozen Row Offset:** When frozen columns present, must query both `getDataRows()` + `getMovableDataRows()` to find cell.
6. **Virtual Scroll:** `getSelectedRowIndexes(enableVirtualization)` returns offset-corrected indices; paste uses these directly.
7. **Checkbox Detection:** If cell contains `.e-check` or `.e-uncheck`, copy outputs `true`/`false` (not cell text).
8. **Event Cancellation:** If `BeforeCopyPaste.Cancel=true`, clipboard content is NOT written; subsequent Paste won't fire.

---

## Code Location Map

<!-- token-budget: 85 words -->

| Location | Purpose | Key Members |
|----------|---------|-------------|
| `scripts/clipboard.ts` | Core module | `constructor()`, `copy(withHeader?)`, `pasteHandler()`, `paste()`, `pasteData()`, `setCopyData()`, `getCopyData()`, `clipBoardData()`, `getSelectedRowCellIndexes()`, `checkBoxSelection()` |
| `scripts/sf-grid-fn.ts:140` | Instantiation | `this.clipboardModule = new Clipboard(this)` in constructor |
| `scripts/sf-grid-fn.ts:1705-1715` | Keyboard hooks | Ctrl+C (line 1705), Ctrl+Shift+H (line 1707), Ctrl+V (line 1710) in `gridKeyDownHandler()` |
| `scripts/sf-grid.ts:918-937` | JS bridge | `copyToClipBoard()`, `preventCopyToClipBoard()`, `preventPasteAction()` |
| `src/Internal/Base/GridJSInteropAdaptor.cs:640-675` | .NET interop (696 lines total) | `InvokeCopyPasteAction()`, `InvokePasteAction()` |
| `src/SfGrid.Methods.cs:572` | Public API | `CopyAsync(withHeader?)` |

---

## Interaction Matrix (MANDATORY)

<!-- token-budget: 180 words -->

> Built from live codebase + training insights.  
> Omit pairs with no interaction risk.

| Feature Combination | Must Preserve | Risk Level | Notes |
|---|---|---|---|
| **Clipboard + Row Selection** | Copy of selected rows (`.e-rowcell` iteration per `getSelectedRowIndexes()`); paste applies to each row sequentially | **Critical** | If selection changes during copy, content already collected; paste uses independent row indices |
| **Clipboard + Cell Selection** | Only `.e-cellselectionbackground` cells copied; `checkBoxSelection()` validates column alignment across rows; paste routes to `UpdateCell` per cell | **Critical** | Paste fails silently if target cell not found or is readonly/hidden |
| **Clipboard + Batch Edit Mode** | Paste ONLY works when `editMode='Batch'`; triggers `BeforeCellPaste` event per cell; `UpdateCell` invoked async | **Critical** | Non-batch edit modes (Inline, Dialog) reject paste at gate in `pasteHandler()` |
| **Clipboard + Frozen Columns** | Copy/paste must query BOTH `getDataRows()` (frozen) and `getMovableDataRows()` (movable); cell lookup via `aria-colindex` | **High** | If frozen structure changes mid-operation, paste offset can desync; index-based lookup prevents this |
| **Clipboard + Virtualization** | `getSelectedRowIndexes(enableVirtualization)` returns corrected indices (accounts for virtual offset); paste applies offset to find row | **High** | If virtual scroll occurs between copy and paste, row indices become invalid; paste silently skips missing rows |
| **Clipboard + Column Virtualization** | Copy includes only visible columns (`.e-hide` filtered); paste skips hidden columns (`!col.template`, `col.allowEditing` gated) | **Medium** | If column visibility toggled after copy, paste column indices may shift; no re-validation |
| **Clipboard + Filtering** | Copy only copies visible (filtered) rows from DOM; hidden rows not included; paste applies to visible row indices only | **Medium** | If filter applied after copy, paste indices referencing old filtered set become invalid |
| **Clipboard + Grouping** | Copy includes group caption rows if in selected set; paste applies to data rows within group; group structure preserved | **Medium** | Group row indices differ from flat data indices; paste uses DOM row indices (safe) |
| **Clipboard + Infinite Scroll** | Copy collects from rendered block only; paste applies within rendered block; out-of-view rows cannot be pasted to | **Medium** | If infinite scroll loads new block before paste completes, indices may shift |
| **Clipboard + Column Reordering** | Copy/paste use `aria-colindex` (logical order), not DOM position; column order changes don't break copy/paste | **Low** | Safe interaction; indices are column-agnostic |

---

## DOM Rendering Structure

<!-- token-budget: 150 words -->

### **Clipboard Hidden Textarea — Initial Injection**

```
Grid Constructor (SfGrid.razor.cs)
    ↓
SfGrid<T>.InitializeAsync()
    ↓
new Clipboard(this)  [scripts/clipboard.ts:constructor]
    ↓
createElement('textarea', {
  className: 'e-clipboard',
  styles: 'opacity: 0',
  attrs: { tabindex: '-1', 'aria-label': 'clipboard' }
})
    ↓
this.parent.element.appendChild(clipBoardTextArea)
    ↓
<div class="e-grid">
  <textarea class="e-clipboard" 
    style="opacity:0" 
    tabindex="-1" 
    aria-label="clipboard" />
  <!-- Hidden: used only for Ctrl+C/V bridge -->
</div>
```

### **Full DOM Tree — Copy/Paste Operation Context**

```
<div class="e-grid" id="grid_DataId">
│
├── <textarea class="e-clipboard" 
│   │   style="opacity:0" 
│   │   tabindex="-1" 
│   │   aria-label="clipboard" />
│   │   └─ [HIDDEN] Used as temporary clipboard bridge for Ctrl+C/V
│
├── <div class="e-gridheader">
│   └── <table>
│       └── <thead>
│           └── <tr class="e-headercells">
│               ├── <th class="e-headercell" aria-colindex="1">
│               │   └─ Header Text (copied if withHeader=true)
│               ├── <th class="e-headercell" aria-colindex="2">
│               └── ...
│
├── <div class="e-gridcontent">
│   └── <div class="e-gridscrollercontainer">
│       └── <table>
│           └── <tbody>
│               ├── <tr class="e-row" aria-rowindex="1">
│               │   ├── <td class="e-rowcell e-cellselectionbackground" 
│               │   │       aria-colindex="1">
│               │   │   └─ Selected cell content
│               │   ├── <td class="e-rowcell" aria-colindex="2">
│               │   │   └─ Non-selected cell
│               │   └── <td class="e-rowcell e-hide" aria-colindex="3">
│               │       └─ [HIDDEN] Not copied (display:none)
│               │
│               ├── <tr class="e-row e-editedbatchcell" aria-rowindex="2">
│               │   ├── <td class="e-rowcell e-cellselectionbackground">
│               │   │   └─ In edit mode (during Batch paste)
│               │   └── ...
│               │
│               └── <tr class="e-row" aria-rowindex="3">
│                   ├── <td class="e-rowcell e-gridchkbox">
│                   │   ├── <input type="checkbox" />
│                   │   └── <span class="e-check" />  ← if selected
│                   └── ...
│
├── <div class="e-frozen" style="...">  <!-- if frozenColumns > 0 -->
│   └── <table>
│       └── <tbody>
│           ├── <tr class="e-row" aria-rowindex="1">
│           │   ├── <td class="e-rowcell" aria-colindex="1">Frozen Data</td>
│           │   └── ...
│           └── ...
│
├── <div class="e-movabletable">  <!-- if frozenColumns > 0 -->
│   └── <table>
│       └── <tbody>
│           ├── <tr class="e-row" aria-rowindex="1">
│           │   ├── <td class="e-rowcell" aria-colindex="N">Movable Data</td>
│           │   └── ...
│           └── ...
│
└── <div class="e-pagercontainer">...</div>
```

---

## Process Flows with DOM Interaction

<!-- token-budget: 250 words -->

### **Flow 1: Copy Operation (Ctrl+C)**

```
┌─────────────────────────────────────────────────────────────────────────┐
│ USER PRESSES: Ctrl+C                                                    │
└──────────────────────────┬────────────────────────────────────────────────┘
                           ↓
         sf-grid-fn.ts:gridKeyDownHandler()
         if (e.keyCode === 67 && (e.ctrlKey))
                           ↓
      clipboardModule.copy(withHeader=false)
         [scripts/clipboard.ts:copy]
                           ↓
         ┌─ window.getSelection() === ""? ─────────────────┐
         ├─ YES (no text selected in DOM)                  │
         │   └─ PROCEED TO COPY                            │
         └─ NO (text selected by user)                     │
             └─ USE TEXT SELECTION (skip grid copy)
                           ↓
    ┌─────────── Grid Mode Check ──────────────┐
    │                                           │
    ├─ selectionMode==='Row' ──→ Copy row(s)   │
    │   ├─ Get getSelectedRowIndexes()         │
    │   ├─ Iterate each row: getRows()[idx]    │
    │   └─ Extract .e-rowcell innerText        │
    │       ├─ if cell has .e-check/.e-uncheck
    │       │   └─ append 'true'/'false'
    │       └─ else append cell innerText
    │                                           │
    ├─ selectionMode==='Cell' ──→ Copy cell(s) │
    │   ├─ Call checkBoxSelection()            │
    │   ├─ Get row/col indices matrix          │
    │   ├─ Validate aligned columns            │
    │   └─ Extract .e-cellselectionbackground  │
    │       per row, per column                │
    │                                           │
    └─ Else ──→ EXIT (unsupported mode)       │
                           ↓
   ┌─ withHeader=true? ─────────────────┐
   ├─ YES: Prepend column header names  │
   │   └─ getVisibleColumns()→headers   │
   └─ NO: Skip header row               │
                           ↓
    Build this.copyContent (string)
    {header row + data rows joined by \n, cells by \t}
                           ↓
   ┌─ isClipboardEventBinded? ──────────────────────┐
   ├─ YES: Invoke C# event BeforeCopyPaste          │
   │   └─ InvokeCopyPasteAction() → GridJSInterop   │
   │       ├─ GridEvents.BeforeCopyPaste.InvokeAsync
   │       ├─ if Cancel=true: RETURN (exit)
   │       └─ else: preventCopyToClipBoard()
   │           └─ clipBoardData(false, content)
   └─ NO: Skip event, go direct to clipBoardData
                           ↓
   clipBoardData(cancel=false, data)
   ├─ this.clipBoardTextArea.value = this.copyContent
   ├─ this.clipBoardTextArea.select()
   └─ navigator.clipboard.writeText(this.copyContent)
       [Browser Clipboard API]
                           ↓
   ✓ CLIPBOARD POPULATED (accessible to user paste)
```

**DOM State After Copy:**
```
<textarea class="e-clipboard" value="Header1\tHeader2\n{row1cell1}\t{row1cell2}\n...">
```

---

### **Flow 2: Paste Operation (Ctrl+V)**

```
┌──────────────────────────────────────────────────────────┐
│ USER PRESSES: Ctrl+V (while in cell selection mode)     │
└─────────────────────────────┬────────────────────────────┘
                              ↓
         sf-grid-fn.ts:gridKeyDownHandler()
         if (e.keyCode === 86 && e.ctrlKey)
                              ↓
        ┌─ Gate Check: allowEditing? ───────────────────┐
        ├─ NO → EXIT (paste disabled)                  │
        └─ YES → CONTINUE                              │
                              ↓
        ┌─ Gate Check: editMode==='Batch'? ───────────┐
        ├─ NO → EXIT (only Batch mode)                 │
        └─ YES → CONTINUE                              │
                              ↓
        ┌─ Gate Check: selectionMode==='Cell'? ───────┐
        ├─ NO → EXIT (cell selection required)        │
        └─ YES → CONTINUE                              │
                              ↓
        ┌─ Gate Check: cellSelectionMode!=='Flow'? ───┐
        ├─ NO (==='Flow') → EXIT (Box mode required)  │
        └─ YES → CONTINUE                              │
                              ↓
      clipboardModule.pasteHandler()
      [scripts/clipboard.ts:pasteHandler]
                              ↓
      ├─ Save activeElement (current focus)
      ├─ Set clipBoardTextArea.value = ''
      ├─ Move focus to clipBoardTextArea
      └─ Wait 10ms for paste event to fire...
                              ↓
      [Browser Ctrl+V Event Fires]
      └─ Clipboard API populates textarea.value
          with clipboard content
                              ↓
      Invoke C# method:
      InvokeCopyPasteAction({
          clipboardText: textarea.value
      }, action='Paste')
                              ↓
      GridJSInteropAdaptor.InvokeCopyPasteAction()
      ├─ Deserialize clipboardText
      ├─ Trigger BeforeCopyPaste event (action='Paste')
      ├─ if Cancel=true: RETURN (exit, no paste)
      └─ else: preventCopyToClipBoard(action='Paste')
                              ↓
      sf-grid.ts:preventCopyToClipBoard()
      └─ clipboardModule.pasteAction(
         value, rowIndex, colIndex, cancel)
                              ↓
    clipboardModule.paste(data, rowIndex, colIndex)
    [scripts/clipboard.ts:paste]
                              ↓
    ┌─ Parse clipboard data ─────────────────────────┐
    │                                                │
    ├─ Check if single cell multiline:              │
    │  if (data.trim() starts with " && ends with ")│
    │  └─ Strip quotes, decode \n to newlines      │
    │      └─ Paste as SINGLE cell                 │
    │                                                │
    └─ Else: Split by \n (rows), \t (columns)      │
         └─ Paste as MATRIX                         │
                              ↓
    Loop through rows: rows.split('\n')
    ├─ FOR each row:
    │  ├─ colIndex = startColIndex
    │  ├─ FOR each column in row.split('\t'):
    │  │  ├─ Find cell at (rIdx, cIdx)
    │  │  │  └─ Query DOM: grid.getCellFromIndex()
    │  │  │      ├─ if frozenColumns > 0:
    │  │  │      │   └─ Query both frozen + movable
    │  │  │      └─ else: single table
    │  │  │
    │  │  ├─ Get column metadata: grid.getColumnByIndex()
    │  │  │  └─ Check: allowEditing, isPrimaryKey, !template
    │  │  │
    │  │  ├─ if column LOCKED (read-only or primary key):
    │  │  │   └─ SKIP cell
    │  │  │
    │  │  └─ Invoke C# method:
    │  │      InvokePasteAction({cellValue}, rowIdx, colIdx)
    │  │      └─ [GridJSInteropAdaptor.InvokePasteAction]
    │  │          ├─ Trigger BeforeCellPaste event
    │  │          ├─ if Cancel=true: skip cell
    │  │          └─ else: preventPasteAction()
    │  │              └─ clipboardModule.pasteData()
    │  │                  ├─ editModule.UpdateCell()
    │  │                  │  [Blazor: Cell → EditState]
    │  │                  ├─ Grid cell element gets
    │  │                  │  classes: e-focus, e-focused
    │  │                  └─ Cell renders edit UI
    │  │
    │  ├─ colIndex += 1 (next column)
    │  └─ If no more cells in row: BREAK
    │
    └─ rIdx += 1 (next row)
                              ↓
    ✓ PASTE COMPLETE
    └─ All cells updated in EditState (batch edit)
       Each cell shows edit control (TextBox, Dropdown, etc.)
       User must save changes via Save button
```

**DOM State During/After Paste:**
```
BEFORE:
<tr class="e-row" aria-rowindex="2">
  <td class="e-rowcell" aria-colindex="1">Old Value</td>
  <td class="e-rowcell" aria-colindex="2">Old Value</td>
</tr>

AFTER (Batch Edit Mode):
<tr class="e-row e-editedbatchcell" aria-rowindex="2">
  <td class="e-rowcell e-batchcell" aria-colindex="1">
    <input type="text" class="e-field" value="Pasted Value" />
  </td>
  <td class="e-rowcell e-batchcell" aria-colindex="2">
    <input type="text" class="e-field" value="Pasted Value" />
  </td>
</tr>
```

---

## Event Cascade Architecture

<!-- token-budget: 120 words -->

### **BeforeCopyPaste Event (Intercept Entire Operation)**

```
C# Method Called
  └─ InvokeCopyPasteAction(args, action='Copy' | 'Paste')
      ├─ Deserialize ClipboardText
      ├─ Build BeforeCopyPasteEventArgs
      │  ├─ Action = 'Copy' | 'Paste'
      │  ├─ Cancel = false (default)
      │  └─ ClipboardText = content
      ├─ Trigger GridEvents.BeforeCopyPaste.InvokeAsync(args)
      │  └─ User event handler receives args
      │      ├─ Can read ClipboardText
      │      ├─ Can set Cancel=true to block operation
      │      └─ Can modify ClipboardText
      ├─ Check: if Cancel=true
      │  └─ Call preventCopyToClipBoard(cancel=true)
      │     └─ DO NOT write to clipboard/Do NOT paste
      └─ else: Call preventCopyToClipBoard(cancel=false)
          └─ PROCEED with clipboard write/paste
```

### **BeforeCellPaste Event (Intercept Per-Cell Paste)**

```
InvokePasteAction(args, rowIndex, colIndex, columnField)
  ├─ Build BeforeCellPasteEventArgs<T>
  │  ├─ Cancel = false
  │  ├─ CellValue = paste value
  │  ├─ RowIndex, ColumnIndex, ColumnName
  │  └─ Data = full row object
  ├─ Trigger GridEvents.BeforeCellPaste.InvokeAsync(args)
  │  └─ User can:
  │     ├─ Validate CellValue
  │     ├─ Transform CellValue
  │     ├─ Set Cancel=true to skip this cell
  │     └─ Access full row Data for context
  ├─ Check: if Cancel=true
  │  └─ DO NOT update this cell
  └─ else: Call UpdateCell(rowIndex, columnField, value)
      └─ Cell enters Batch Edit state
```

---

## Feature Combination Guidelines

<!-- token-budget: 200 words -->

### **Clipboard + Selection (Critical Interaction)**

**Rules:**
- Row Selection: Copy includes all visible cells in selected rows
- Cell Selection: Copy includes only cells with `.e-cellselectionbackground` class
- Mixed modes: If `selectionMode='Both'`, both row and cell modes active; copy behavior depends on current selection state

**LLM Guidance:**
When implementing copy for mixed selection, check `getSelectedRowIndexes()` first; if empty, fallback to cell selection via `checkBoxSelection()`.

---

### **Clipboard + Batch Editing (Critical Interaction)**

**Rules:**
- Paste ONLY works if grid has `allowEditing=true`, `editMode='Batch'`
- Each pasted cell triggers separate `BeforeCellPaste` event
- After paste completes, all cells are in edit state (show edit controls)
- User must click Save/Update button to commit changes to backend
- If user cancels edit, pasted values are discarded

**LLM Guidance:**
When pasting, never assume change is committed. Verify EditModule state and that grid is not in Inline/Dialog edit mode (those reject paste).

---

### **Clipboard + Frozen Columns (High Interaction)**

**Rules:**
- When `frozenColumns > 0`, grid renders TWO tables: frozen table + movable table
- Copy/paste must query BOTH tables
- Cell lookup by `aria-colindex` works across both tables
- Paste loop checks `getCellFromIndex()` which internally handles frozen/movable dispatch

**LLM Guidance:**
If paste fails for frozen columns, verify `getDataRows()` and `getMovableDataRows()` are both populated. Check that `aria-colindex` attributes are unique across both tables.

---

### **Clipboard + Virtualization (High Interaction)**

**Rules:**
- Virtual scroll renders only visible rows in DOM
- `getSelectedRowIndexes(enableVirtualization=true)` corrects row indices for virtual offset
- If row is not in rendered block, paste silently skips it (DOM doesn't have it)
- After virtual scroll occurs, row indices stored in copyContent become stale

**LLM Guidance:**
If paste silently fails for some rows, check if virtual scroll occurred after copy. Indices are NOT dynamically adjusted; paste uses indices captured at copy time. Recommend: copy → paste immediately, before scroll.

---

### **Clipboard + Filtered Data (Medium Interaction)**

**Rules:**
- Copy collects rows from current DOM (filtered set only)
- Hidden rows (via filter) are not in DOM, so not copied
- Paste applies to visible rows by their current DOM indices
- If filter applied after copy, row indices no longer match

**LLM Guidance:**
After copying, if user applies filter, inform them: "Row indices may have changed. Recommend copying again after filtering."

---

### **Clipboard + Column Virtualization (Medium Interaction)**

**Rules:**
- Column virtualization hides columns with `.e-hide` class
- Copy skips hidden columns (filters by `!classList.contains('e-hide')`)
- Paste skips columns with `!allowEditing`, `template`, or `isPrimaryKey`
- Column reordering doesn't break copy/paste (uses `aria-colindex`, not DOM position)

**LLM Guidance:**
Ensure all visible, editable columns are included in copy content. If a column should be excluded, set its `allowEditing=false` or `template=<custom>`.

---

## CSS Classes Reference

<!-- token-budget: 80 words -->

| Class | Purpose | Copy/Paste Impact |
|-------|---------|-------------------|
| `.e-clipboard` | Hidden textarea for clipboard bridge | Core to operation; must exist and be accessible |
| `.e-rowcell` | Data cell in grid | All cells found via this class; `innerText` extracted for copy |
| `.e-cellselectionbackground` | Selected cell indicator | Identifies cells to copy in Cell Selection mode |
| `.e-hide` | Hidden column | Excluded from copy (filter via `!classList.contains('e-hide')`) |
| `.e-gridchkbox` | Checkbox cell | Detected during copy; outputs `true`/`false` based on `.e-check`/`.e-uncheck` state |
| `.e-check` / `.e-uncheck` | Checkbox states | Used to determine if checkbox cell outputs true/false |
| `.e-editedbatchcell` | Batch edit row | Indicates row is in edit state during paste |
| `.e-batchcell` | Individual batch edit cell | Cell being edited; contains `<input>` |
| `.e-focus` / `.e-focused` | Focus state | Applied to cell after paste to indicate focus |

---

## Troubleshooting Guide for LLM

<!-- token-budget: 150 words -->

### **Issue: Copy Not Working**

1. **Check Gate:**
   - Verify `allowEditing` is NOT preventing copy (copy works even if editing disabled)
   - Verify selection mode is Row or Cell (not None)
   - Verify `e-clipboard` textarea exists in DOM

2. **Check Content:**
   - Inspect `copyContent` string in devtools
   - Verify cells have `.e-rowcell` class
   - Verify selected cells have `.e-cellselectionbackground` (Cell mode)

3. **Check Event:**
   - If `BeforeCopyPaste` event exists and fires, check if `Cancel=true`

### **Issue: Paste Not Working**

1. **Check Gates (all must be true):**
   ```
   allowEditing === true
   editMode === 'Batch'
   selectionMode === 'Cell'
   cellSelectionMode !== 'Flow'  (must be 'Box')
   user in cell (not row/group header)
   ```

2. **Check Clipboard:**
   - Verify clipboard has content (use `navigator.clipboard.readText()`)
   - Verify paste data doesn't have encoding issues

3. **Check Target Cells:**
   - Verify cells are found via `getCellFromIndex()`
   - Verify column `allowEditing=true` (not readonly/primary key)
   - Verify cell is not in a template

4. **Check Frozen Columns:**
   - If frozen, verify `getMovableDataRows()` is populated
   - Check `aria-colindex` on both frozen + movable cells

### **Issue: Paste Silently Fails for Some Rows**

1. Check virtualization offset: `getSelectedRowIndexes(enableVirtualization)` should correct indices
2. Check if virtual scroll occurred after copy (indices become stale)
3. Check if row is hidden by filter
4. Check if row was deleted before paste completed

---

## References to Training Documentation

<!-- token-budget: 50 words -->

Refer to:
- `/docs/training/` for grid-wide patterns, state management
- `/docs/architecture/system-architecture.md` for Blazor JS interop lifecycle
- `/docs/code-guidelines/` for naming, error handling standards
- `/docs/tech-stack/tech-stack.md` for Blazor/Razor/C# conventions

---

## Summary for LLM Implementation

<!-- token-budget: 80 words -->

**Clipboard is a TWO-PHASE feature:**

1. **Copy Phase:** Collect selected cell/row data into a JavaScript string, write to hidden textarea, sync to system clipboard via `navigator.clipboard.writeText()`
2. **Paste Phase:** Read system clipboard into textarea via keyboard handler, parse tab/newline delimiters, validate gates (Batch edit + Cell selection), route each cell through C# event cascade, update EditState

**Always verify:** DOM textarea lifecycle (persistent), event cancellation logic, frozen column cell dispatch, virtual row offset handling.

---

*Last Updated: 2026-04-07*  
*Skill Version: 1.0*
