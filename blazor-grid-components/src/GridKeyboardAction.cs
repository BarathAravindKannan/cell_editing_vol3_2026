// <copyright file="GridKeyboardAction.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 1 (task 1.1)
// Public GridKeyboardAction enum. Append-only; do not renumber or remove values.
// Current in-enum catalog: 27 actions (Phase 1-9). The full 47-action target catalog
// (Phase 1-17) is documented in openspec/changes/custom-keyboard-shortcuts/RECONCILIATION_NOTES.md
// and will be appended in subsequent OpenSpec changes per the append-only rule.
// </copyright>

using System;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Enumerates all keyboard actions exposed by the grid. Values are append-only;
    /// removing a public member would be a breaking change. New actions are added
    /// at the end (values 28+) or as the next available number in the existing
    /// logical grouping.
    /// </summary>
    /// <remarks>
    /// The authoritative list, default chords, owning entry path and applicable zone
    /// for each value lives in <c>openspec/changes/custom-keyboard-shortcuts/RECONCILIATION_NOTES.md</c>
    /// ("Authoritative Action List"). Do not duplicate the canonical table here; the XML
    /// docs on each value name only the contract, not the runtime defaults (defaults
    /// are emitted by <c>GridKeyboardActionDefaults</c>).
    /// </remarks>
    public enum GridKeyboardAction
    {
        /// <summary>
        /// Represents no keyboard action. This is the default enum value and
        /// is not associated with any keyboard shortcut or grid operation.
        /// </summary>
        None = 0,
        // ---- 8 Navigation actions (1..8) ----
        /// <summary>Move focus one cell up. Default: <c>Shift+Enter</c>. Owner: blazor.</summary>
        NavigateUp = 1,
        /// <summary>Move focus one cell down. Default: <c>Enter</c>, <c>ArrowDown</c>, <c>Shift+Down</c>. Owner: blazor.</summary>
        NavigateDown = 2,
        /// <summary>Move focus one cell left. Default: <c>Shift+Tab</c>. Owner: blazor.</summary>
        NavigateLeft = 3,
        /// <summary>Move focus one cell right. Default: <c>Tab</c>, <c>ArrowRight</c>, <c>Shift+Right</c>. Owner: blazor.</summary>
        NavigateRight = 4,
        /// <summary>Move focus to the first cell of the current row. Default: <c>Home</c>. Owner: blazor.</summary>
        NavigateRowStart = 5,
        /// <summary>Move focus to the last cell of the current row. Default: <c>End</c>. Owner: blazor.</summary>
        NavigateRowEnd = 6,
        /// <summary>Move focus to the first cell of the grid. Default: <c>Ctrl+Home</c>. Owner: blazor.</summary>
        NavigateGridStart = 7,
        /// <summary>Move focus to the last cell of the grid. Default: <c>Ctrl+End</c>. Owner: blazor.</summary>
        NavigateGridEnd = 8,

        // ---- Cell actions (10..14) ----
        /// <summary>Start editing the focused cell. Default: <c>F2</c>. Owner: js.</summary>
        StartEdit = 10,
        /// <summary>Add a new row to the grid. Default: <c>Insert</c>. Owner: js.</summary>
        AddRecord = 11,
        /// <summary>Delete the focused/selected row(s). Default: <c>Delete</c>. Owner: js.</summary>
        DeleteRecord = 12,
        /// <summary>Select every row in the grid. Default: <c>Ctrl+A</c>. Owner: js.</summary>
        SelectAll = 13,
        /// <summary>Cancel the current edit / close the active popup. Default: <c>Escape</c>. Owner: js.</summary>
        Escape = 14,

        // ---- Common scope (15) ----
        /// <summary>
        /// Open the browser print dialog. Default: <c>Ctrl+P</c> / <c>Cmd+P</c>.
        /// Valid in Header, DataCell, GroupRow and DetailRow zones via the <c>Common</c> fallback scope.
        /// </summary>
        Print = 15,

        // ---- Header actions (20..24) ----
        /// <summary>Sort the focused header column. Default: <c>Enter</c>, <c>Ctrl+Enter</c>, <c>Shift+Enter</c>. Owner: blazor.</summary>
        SortColumn = 20,
        /// <summary>Group or ungroup the focused header column. Default: <c>Ctrl+Space</c>. Owner: blazor.</summary>
        GroupColumn = 21,
        /// <summary>Move the focused header column one position to the left. Default: <c>Ctrl+Left</c>. Owner: blazor.</summary>
        ReorderColumnLeft = 22,
        /// <summary>Move the focused header column one position to the right. Default: <c>Ctrl+Right</c>. Owner: blazor.</summary>
        ReorderColumnRight = 23,
        /// <summary>Toggle the header checkbox on a <see cref="ColumnType.CheckBox"/> column. Default: <c>Space</c>. Owner: blazor.</summary>
        ToggleHeaderCheckbox = 24,

        // ---- Paging actions (30..35, Phase 8) ----
        /// <summary>Go to the previous page. Default: <c>PageUp</c>. Owner: js.</summary>
        PageUp = 30,
        /// <summary>Go to the next page. Default: <c>PageDown</c>. Owner: js.</summary>
        PageDown = 31,
        /// <summary>Go to the first page. Default: <c>Ctrl+Alt+PageUp</c>. Owner: js.</summary>
        FirstPage = 32,
        /// <summary>Go to the last page. Default: <c>Ctrl+Alt+PageDown</c>. Owner: js.</summary>
        LastPage = 33,
        /// <summary>Jump to the next page group. Default: <c>Alt+PageDown</c>. Owner: js.</summary>
        NextPageGroup = 34,
        /// <summary>Jump to the previous page group. Default: <c>Alt+PageUp</c>. Owner: js.</summary>
        PreviousPageGroup = 35,

        // ---- Selection actions (40..41, Phase 9) ----
        /// <summary>Toggle selection of the focused row in a checkbox-only column. Default: <c>Space</c>. Owner: blazor.</summary>
        SelectToggle = 40,
        /// <summary>Toggle row selection. Default: <c>Ctrl+Space</c>. Owner: blazor.</summary>
        ToggleRowSelection = 41,

        // ---- Phases 10, 13, 14, 15 (values 50..60) ----
        // Phase 15 (LOW risk) — focus jumps
        /// <summary>
        /// Move keyboard focus to the first cell of the first content row.
        /// Default: <c>Alt+W</c> (document-level listener). Valid in DataCell and Header zones.
        /// Owner: blazor.
        /// </summary>
        MoveFocusToFirstRow = 50,
        /// <summary>
        /// Move keyboard focus into the filter bar.
        /// Default: <c>ArrowDown</c> from a header cell with filter enabled, <c>ArrowUp</c> from the
        /// first content cell when the column is filterable. Valid in DataCell and Header zones.
        /// Owner: blazor.
        /// </summary>
        FocusFilterBar = 51,
        /// <summary>
        /// Move keyboard focus into the "add new row" form when <c>ShowAddNewRow</c> is enabled.
        /// Default: <c>ArrowDown</c> from a header cell, <c>ArrowUp</c> from the first content cell.
        /// Valid in DataCell and Header zones. Owner: blazor.
        /// </summary>
        FocusAddForm = 52,

        // Phase 14 (MED risk) — header popups
        /// <summary>
        /// Open the column menu for the focused header column when <c>ShowColumnMenu</c> is enabled.
        /// Default: <c>Alt+ArrowDown</c>. Valid in Header zone only. Owner: blazor.
        /// </summary>
        OpenColumnMenu = 53,
        /// <summary>
        /// Open the filter menu for the focused header column when filter mode is Menu.
        /// Default: <c>Alt+ArrowDown</c>. Valid in Header zone only. Owner: blazor.
        /// </summary>
        OpenFilterMenu = 54,

        // Phase 10 (MED risk) — group caption expand/collapse
        /// <summary>
        /// Expand the focused group caption row. Default: <c>ArrowRight</c>.
        /// Valid in GroupRow zone. Owner: blazor.
        /// </summary>
        ExpandGroup = 55,
        /// <summary>
        /// Collapse the focused group caption row. Default: <c>ArrowLeft</c>.
        /// Valid in GroupRow zone. Owner: blazor.
        /// </summary>
        CollapseGroup = 56,

        // Phase 13 (MED risk) — detail row expand/collapse
        /// <summary>
        /// Expand the focused detail row. Default: <c>Enter</c> on the detail expand cell of a
        /// collapsed row, or <c>Alt+ArrowDown</c>. Valid in DetailRow zone. Owner: blazor.
        /// </summary>
        ExpandDetailRow = 57,
        /// <summary>
        /// Collapse the focused detail row. Default: <c>Enter</c> on the detail expand cell of an
        /// expanded row, or <c>Alt+ArrowUp</c>. Valid in DetailRow zone. Owner: blazor.
        /// </summary>
        CollapseDetailRow = 58,
        /// <summary>
        /// Expand every detail row. Default: <c>Ctrl+ArrowDown</c>. Valid in DetailRow zone.
        /// Owner: blazor.
        /// </summary>
        ExpandAllDetailRows = 59,
        /// <summary>
        /// Collapse every detail row. Default: <c>Ctrl+ArrowUp</c>. Valid in DetailRow zone.
        /// Owner: blazor.
        /// </summary>
        CollapseAllDetailRows = 60,

        // Phase 10 (MED risk) — group caption expand all / collapse all
        /// <summary>
        /// Expand all group caption rows. Default: <c>Ctrl+ArrowDown</c>.
        /// Valid in GroupRow zone. Owner: blazor.
        /// </summary>
        ExpandAllGroup = 70,
        /// <summary>
        /// Collapse all group caption rows. Default: <c>Ctrl+ArrowUp</c>.
        /// Valid in GroupRow zone. Owner: blazor.
        /// </summary>
        CollapseAllGroup = 71,

        // ---- Phases 11, 12, 16, 17 (values 61..69) ----
        // Phase 11 (MED risk) — clipboard (Common scope); Cut excluded per D7.
        /// <summary>
        /// Copy the selected cells to the clipboard. Default: <c>Ctrl+C</c> / <c>Cmd+C</c>.
        /// Valid in all zones via the <c>Common</c> fallback scope. Owner: js.
        /// </summary>
        Copy = 61,
        /// <summary>
        /// Copy the selected cells to the clipboard along with the header row.
        /// Default: <c>Ctrl+Shift+H</c> (keyCode 72). Valid in all zones via the
        /// <c>Common</c> fallback scope. Owner: js.
        /// </summary>
        CopyWithHeaders = 62,
        /// <summary>
        /// Paste clipboard content into the grid. Default: <c>Ctrl+V</c> / <c>Cmd+V</c>.
        /// Valid in all zones via the <c>Common</c> fallback scope. Owner: js.
        /// </summary>
        Paste = 63,

        // Phase 12 (MED risk) — undo/redo; four-gate alignment
        // (JS gate, Utils.GetKeyCombination, HandleUndo, GridRow.CellRenderer.cs).
        /// <summary>
        /// Undo the most recent edit. Default: <c>Ctrl+Z</c> / <c>Cmd+Z</c>.
        /// Valid in DataCell zone. Owner: blazor.
        /// </summary>
        Undo = 64,
        /// <summary>
        /// Redo the most recently undone edit. Default: <c>Ctrl+Y</c> (Win) /
        /// <c>Cmd+Shift+Z</c> (Mac). Valid in DataCell zone. Owner: blazor.
        /// </summary>
        Redo = 65,

        // Phase 16 (MED risk) — selection extensions; already-handled in code.
        /// <summary>
        /// Extend the current selection by one row/column in the pressed direction.
        /// Default: <c>Shift+Up</c>, <c>Shift+Down</c>, <c>Shift+Left</c>, <c>Shift+Right</c>.
        /// Valid in DataCell zone. Consumed by <c>Selection.ClearRedundantCellsInBoxMode</c>.
        /// Owner: blazor.
        /// </summary>
        SelectRowRange = 66,
        /// <summary>
        /// Extend the current selection via mouse <c>Shift+Click</c> / <c>Ctrl+Click</c>.
        /// No default keyboard binding; the legacy <c>MouseAndKeyArgs</c> path drives
        /// this from the JS mouse event, so the validator marks it as
        /// <c>[NonBindable]</c> and the compiler rejects any keyboard override.
        /// Valid in DataCell zone. Owner: blazor.
        /// </summary>
        ExtendSelection = 67,

        // Phase 17 (MED risk) — edit-end tab transitions; already-handled in code.
        /// <summary>
        /// Commit the active edit and move focus to the next editable cell.
        /// Default: <c>Tab</c> while editing. Valid in EditCell zone. Owner: blazor.
        /// </summary>
        EditEndAndTabNext = 68,
        /// <summary>
        /// Commit the active edit and move focus to the previous editable cell.
        /// Default: <c>Shift+Tab</c> while editing. Valid in EditCell zone. Owner: blazor.
        /// </summary>
        EditEndAndShiftTabPrev = 69
    }
}
