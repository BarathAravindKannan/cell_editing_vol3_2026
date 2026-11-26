using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Handles focus and keyboard navigation in grid.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal partial class FocusHandler<T>
    {
        private static readonly string[] MoveUpCellAction = { "MoveUpCell" };

        private static readonly string[] MoveDownCellAction = { "MoveDownCell" };

        private static readonly string[] MoveLeftCellAction = { "MoveLeftCell" };

        private static readonly string[] MoveRightCellAction = { "MoveRightCell" };

        #region Private Properties

        private SfGrid<T> _parent;

        private bool _isHeader { get; set; }

        private bool _isJump { get; set; }

        // Set by ProcessKeyDown and ProcessKeyCombination when the resolved
        // GridKeyboardAction came from a user-bound custom chord (resolver Layer 1/2)
        // rather than a built-in default (Layer 4) or a suppression (Layer 3). The
        // Layer 1/2 vs Layer 4 distinction uses ResolveResult.PreventDefault: only
        // Layer 1/2 set it to true. Read by the action-switch branches and the Batch
        // pre-gate to decide whether to bypass the legacy "no modifiers" guard that
        // exists for the default F2/Insert/Delete chords.
        private bool _isCustomKeyboardBinding { get; set; }

        private Cell<Object>? _previouslyFocusedCell { get; set; }

        private ValueTuple<string, string> _current = (string.Empty, string.Empty);

        private List<Row<object>> _currentRows
        {
            get { return _isHeader ? _parent.HeaderRows : _parent.Rows; }
        }

        internal bool isPopUpOpened { get; set; }

        private List<Row<object>> _frozenHeaderRows
        {
            get { return _parent.FrozenHeaderRows; }
        }

        private readonly Dictionary<string, bool> _focusedCells = new();

        #endregion

        #region Internal Properties

        internal bool IsByKey { get; set; }

        /// <summary>
        /// DOM virtualization: data index and column index of the focused content cell. Unlike the Row/Cell objects
        /// (which are dropped when a row leaves the rendered window) these survive re-rendering.
        /// </summary>
        internal int? DomFocusRowIndex { get; private set; }

        internal int? DomFocusCellIndex { get; private set; }

        internal bool DomFocusOutline { get; private set; }

        // DOM virtualization: a keyboard jump (Ctrl+Home/End, Ctrl+Up/Down) to a row that is not rendered yet.
        // The row/cell cannot be focused until the new window is in the DOM, so the target is kept here and applied
        // from ApplyPendingDomFocusAsync after the window has rendered.
        private (int RowIndex, int? CellIndex, KeyboardEventArgs Args, string KeyCombination)? _domPendingFocus;

        internal bool HasPendingDomFocus => _domPendingFocus != null;

        internal bool IsChildFocused { get; set; }

        internal int? SelectedCellIndex { get; set; }

        internal int? SelectedRowIndex { get; set; }

        internal Row<object>? KeyInvokedRow { get; set; }

        internal Cell<object>? KeyInvokedCell { get; set; }

        internal bool ChangeLastCellTabIndex { get; set; }

        internal object? AriaLabel { get; set; }

        internal bool IsKeyPressedUpOrDown { get; set; }

        internal KeyboardEventArgs? LastKeyCombination;

        internal int LastNavigatedCellIdx;

        internal bool IsSelectAllClicked { get; set; }

        internal string ClickedCheckBoxId { get; set; } = string.Empty;

        internal List<Row<object>> HeaderRows
        {
            get { return _parent.HeaderRows; }
        }

        #endregion

        #region Constructor & Initialization

        public FocusHandler(SfGrid<T> parent)
        {
            _parent = parent;
            _parent.EventAggregator.Add("HeaderMouseDown", _ => _isHeader = true);
        }

        #endregion

        #region Helper Methods

        private bool IsGroupingFilterTemplate(Cell<object> cell) => 
            _parent.GroupSettings?.Columns?.Length > 0 && !cell.IsDataCell && _parent.GetVisibleColumnsAsync().Result.FirstOrDefault()?.FilterTemplate != null;

        private GridColumn? GetFirstVisibleColumn()
        {
            return _parent.GetVisibleColumnsAsync().Result.FirstOrDefault();
        }

        private GridColumn? GetLastVisibleColumn()
        {
            return _parent.GetVisibleColumnsAsync().Result.LastOrDefault();
        }

        private bool IsHeaderRow(Row<object>? row)
        {
            return row != null && _parent.HeaderRows.Concat(_parent.FrozenHeaderRows).Concat(_parent.FrozenRightHeaderRows).Any(headerRow => headerRow?.Uid == row.Uid);
        }

        internal Cell<object> GetLastVisibleCell()
        {
            return _currentRows.Last().Cells.Where(_ => _.Visible).Last();
        }

        /// <summary>
        /// Finds the next or previous editable cell and its row for Tab/Shift+Tab navigation in Cell edit mode.
        /// Searches within the current row first, then wraps to the next/previous row if needed.
        /// Skips non-editable columns (read-only, hidden, non-data cells).
        /// For Tab: wraps forward within the current page; for Shift+Tab: wraps backward within the current page.
        /// </summary>
        /// <param name="currentRow">The current row being edited.</param>
        /// <param name="currentCell">The current cell being edited.</param>
        /// <param name="isTab">True for Tab (forward), false for Shift+Tab (backward).</param>
        /// <returns>Tuple of (nextCell, nextRow), or (null, null) if at grid boundaries.</returns>
        private (Cell<object>?, Row<object>?) FindNextEditableCell(Row<object> currentRow, Cell<object> currentCell, bool isTab)
        {
            var visibleRows = _parent.Rows?.Where(r => r.Visible && r.IsDataRow).ToList() ?? new List<Row<object>>();
            if (visibleRows.Count == 0)
            {
                return (null, null);
            }

            int currentRowIndex = visibleRows.FindIndex(r => r.Uid == currentRow.Uid);
            if (currentRowIndex < 0)
            {
                return (null, null);
            }

            // Search for next editable cell in the current row
            int currentCellIndex = currentRow.Cells.FindIndex(c => c.Uid == currentCell.Uid);
            if (currentCellIndex < 0)
            {
                return (null, null);
            }

            // Try to find an editable cell in the current row
            if (isTab)
            {
                // Tab: search forward in current row
                for (int i = currentCellIndex + 1; i < currentRow.Cells.Count; i++)
                {
                    var cell = currentRow.Cells[i];
                    if (IsEditableCell(cell))
                    {
                        return (cell, currentRow);
                    }
                }

                // If no editable cell found in current row, wrap to next row
                // Check if we're not at the last row
                if (currentRowIndex < visibleRows.Count - 1)
                {
                    for (int rowIdx = currentRowIndex + 1; rowIdx < visibleRows.Count; rowIdx++)
                    {
                        var firstEditableCell = visibleRows[rowIdx].Cells.FirstOrDefault(IsEditableCell);
                        if (firstEditableCell != null)
                        {
                            return (firstEditableCell, visibleRows[rowIdx]);
                        }
                    }
                }

                // At last row with no editable cells ahead - exit grid naturally (return null)
                return (null, null);
            }
            else
            {
                // Shift+Tab: search backward in current row
                for (int i = currentCellIndex - 1; i >= 0; i--)
                {
                    var cell = currentRow.Cells[i];
                    if (IsEditableCell(cell))
                    {
                        return (cell, currentRow);
                    }
                }

                // If no editable cell found in current row, wrap to previous row
                // Check if we're not at the first row
                if (currentRowIndex > 0)
                {
                    for (int rowIdx = currentRowIndex - 1; rowIdx >= 0; rowIdx--)
                    {
                        var lastEditableCell = visibleRows[rowIdx].Cells.LastOrDefault(IsEditableCell);
                        if (lastEditableCell != null)
                        {
                            return (lastEditableCell, visibleRows[rowIdx]);
                        }
                    }
                }

                // At first row with no editable cells before - exit grid naturally (return null)
                return (null, null);
            }
        }

        /// <summary>
        /// Checks if a cell is editable based on column and cell properties.
        /// </summary>
        private bool IsEditableCell(Cell<object> cell)
        {
            return cell != null
                && cell.Visible
                && cell.IsDataCell
                && cell.Column != null
                && (_parent!.EditModule?.isEditable(cell.Column) == true)
                && cell.CellType.Equals(CellType.Data);
        }

        private async Task StartEditFromKeyboardAsync(Row<object>? row, Cell<object>? cell)
        {
            row ??= KeyInvokedRow ?? _currentRows?.FirstOrDefault(currentRow => currentRow.Uid == _current.Item1);
            cell ??= KeyInvokedCell ?? row?.Cells.FirstOrDefault(currentCell => currentCell.Uid == _current.Item2);

            if (_parent?.EditSettings?.AllowEditing != true
                || _parent.EditModule == null
                || row?.IsDataRow != true
                || cell == null
                || cell.IsEdit
                || cell.EditDisabled
                || !IsEditableCell(cell))
            {
                return;
            }

            if (_parent.EditModule.IsCellOrBatchEditMode())
            {
                if (_parent.EditModule.IsCellEditMode() && _parent.IsEdit)
                {
                    return;
                }

                await ClearFocus(row, cell).ConfigureAwait(true);
                await _parent.EditModule.EditCell(row, cell).ConfigureAwait(true);
                return;
            }

            if (!_parent.IsEdit)
            {
                await _parent.EditModule.StartEdit(row).ConfigureAwait(true);
            }
        }

        private (Row<object>, Cell<object>) GetCurrentRowAndCell(Row<object>? row, Cell<object>? staleCell)
        {
            Row<object>? currentRow = _parent?.Rows.Find(r => r.Uid == _current.Item1);
            Cell<object>? currentCell = GridUtils.ResolveCurrentCell(currentRow, staleCell) ?? staleCell;
            if (currentRow == null)
            {
                currentRow = _parent?.Rows.FirstOrDefault();
            }
            if (currentCell == null)
            {
                currentCell = currentRow?.Cells.FirstOrDefault();
            }
            return (currentRow!, currentCell!);
        }

        /// <summary>
        /// Determines if the provided key combination is "Enter" key.
        /// </summary>
        private static bool IsEnterKeyPressed(string? keyCombination)
            => keyCombination?.Equals("Enter", StringComparison.Ordinal) == true;

        /// <summary>
        /// Determines if the provided key combination is "ShiftEnter" key.
        /// </summary>
        private static bool IsShiftEnterKeyPressed(string? keyCombination)
            => keyCombination?.Contains("ShiftEnter", StringComparison.Ordinal) == true;

        /// <summary>
        /// Determines if the provided key combination is "F2" key.
        /// </summary>
        private static bool IsF2KeyPressed(string? keyCombination)
            => keyCombination?.Contains("F2", StringComparison.Ordinal) == true;

        /// <summary>
        /// Determines if the provided key combination is "Tab" key.
        /// </summary>
        private static bool IsTabKeyPressed(string? keyCombination)
            => keyCombination?.Equals("Tab", StringComparison.Ordinal) == true;

        /// <summary>
        /// Determines if the provided key combination is "Shift+Tab" key.
        /// </summary>
        private static bool IsShiftTabKeyPressed(string? keyCombination)
            => keyCombination?.Equals("ShiftTab", StringComparison.Ordinal) == true;

        /// <summary>
        /// Determines if Enter key should be handled in Cell edit mode.
        /// </summary>
        private bool ShouldHandleEnterAndF2InCellEditMode(Cell<object>? cell, string? keyCombination, bool isTabKey, bool isShiftTabKey)
        {
            return _parent.EditSettings?.Mode == EditMode.Cell
                && IsEditableCell(cell!)
                && (IsEnterKeyPressed(keyCombination)
                || IsShiftEnterKeyPressed(keyCombination) || IsF2KeyPressed(keyCombination))
                && !isTabKey && !isShiftTabKey;
        }

        /// <summary>
        /// Handles Enter key in Cell edit mode: enters edit mode if not editing, or saves current cell if editing.
        /// Honors <see cref="GridEditSettings.EnterKeyDirection"/>: after the save, may move focus
        /// to the next editable cell in the same column (Row) or the next editable cell on the
        /// same row (Column), or stay on the current cell (None).
        /// </summary>
        private async Task HandleEnterKeyInCellEditMode(Row<object>? row, Cell<object>? cell, string? keyCombination)
        {
            if (cell?.IsEdit != true && !_parent.IsEdit && (IsEnterKeyPressed(keyCombination) || IsF2KeyPressed(keyCombination)))
            {
                await _parent.EditModule!.EditCell(row!, cell!).ConfigureAwait(true);
            }
            else if (cell?.IsEdit == true)
            {
                await _parent.EditModule!.EndEdit(cell).ConfigureAwait(true);
            }
        }

        private static string[] MapEnterKeyDirectionToActions(EnterKeyDirection direction, string? keyCombination)
        {
            if (direction == EnterKeyDirection.None)
            {
                return Array.Empty<string>();
            }

            bool isShiftEnter = keyCombination?.Equals("ShiftEnter", StringComparison.Ordinal) == true;

            if (direction == EnterKeyDirection.NextRow)
            {
                return new[] { isShiftEnter ? "MoveUpCell" : "MoveDownCell" };
            }
            return new[] { isShiftEnter ? "MoveLeftCell" : "MoveRightCell" };
        }

        private bool ShouldHandleKeyPressInCellEditMode(Cell<object>? cell, KeyboardEventArgs e)
        {
            return _parent.EditSettings?.Mode == EditMode.Cell
                && _parent.EditSettings?.AllowEditOnKeyPress == true
                && cell?.IsEdit != true
                && !_parent.IsEdit
                && IsPrintableKey(e?.Key) && cell?.IsDataCell == true;
        }

        private static bool IsPrintableKey(string? key)
        {
            if (string.IsNullOrEmpty(key)) return false;
            // Length must be 1: "Enter", "Tab", "ArrowUp", "ShiftTab", "F2", etc. are longer.
            if (key.Length != 1) return false;
            char c = key[0];
            return !char.IsControl(c);
        }

        private async Task HandleKeyPressInCellEditMode(Row<object>? row, Cell<object>? cell, string? keyChar)
        {
            if (row == null || cell == null || _parent.EditModule == null || string.IsNullOrEmpty(keyChar))
                return;
            await _parent.EditModule.EditCell(row, cell, focusFirstCellOnShiftTab: false, prefillChar: keyChar).ConfigureAwait(true);
        }

        /// <summary>
        /// Determines if Tab/Shift+Tab navigation should be handled in Cell edit mode.
        /// </summary>
        private bool ShouldHandleTabNavigationInCellEditMode(Row<object> row, Cell<object>? cell, string? keyCombination)
        {
            var UpdatedCell = GetCurrentRowAndCell(row, cell).Item2 ?? cell;
            return UpdatedCell?.IsEdit == true 
                && _parent.IsEdit
                && _parent.EditSettings?.Mode == EditMode.Cell
                && (IsTabKeyPressed(keyCombination) || IsShiftTabKeyPressed(keyCombination));
        }

        /// <summary>
        /// Handles Tab/Shift+Tab navigation in Cell edit mode: saves current cell and moves to next/previous editable cell.
        /// </summary>
        private async Task HandleTabNavigationInCellMode(Row<object>? row, Cell<object>? cell, string? keyCombination)
        {
            if (row == null || cell == null || _parent.EditModule == null)
                return;
            await _parent.EditModule!.EndEdit(cell).ConfigureAwait(true);

            // If validation failed, the editor will still be open (IsEdit remains true), so don't navigate
            if (cell.IsEdit)
            {
                return;
            }

            // After EndEdit, the row is re-rendered with new Cell instances, so the original `cell`
            // reference is stale (its Uid no longer matches any cell in _currentRows). Resolve the
            // fresh row from _parent.Rows by Uid (row Uid is stable across re-render) and locate the
            // current cell within it by column identity. This ensures FindNextEditableCell receives
            // a Cell object that actually exists in the current row's Cells collection.
            ValueTuple<Row<object>, Cell<object>> currentRowAndCell = GetCurrentRowAndCell(row, cell);
            row = currentRowAndCell.Item1;
            cell = currentRowAndCell.Item2;
            bool isTab = IsTabKeyPressed(keyCombination);
            var (nextEditCell, nextEditRow) = FindNextEditableCell(row, cell, isTab);
            SelectedCellIndex = nextEditCell?.Index;
            if (nextEditCell != null && nextEditRow != null)
            {
                _current = (nextEditRow.Uid!, nextEditCell.Uid!);
                await TransitionToNextCell(cell, nextEditCell, nextEditRow).ConfigureAwait(true);
            }
        }

        private async Task HandleEditEndNavigation(Row<object>? row, Cell<object> cell, bool moveNext)
        {
            if (_parent.EditModule == null)
            {
                return;
            }

            if (_parent.EditSettings?.Mode == EditMode.Cell && row != null)
            {
                await HandleTabNavigationInCellMode(row, cell, moveNext ? "Tab" : "ShiftTab").ConfigureAwait(true);
                return;
            }

            if (_parent.EditSettings?.Mode == EditMode.Batch && row != null)
            {
                await _parent.EditModule.SaveCell().ConfigureAwait(true);
                if (cell.IsEdit)
                {
                    return;
                }

                row = _parent.Rows?.FirstOrDefault(currentRow => currentRow.Uid == row.Uid) ?? row;
                cell = GridUtils.ResolveCurrentCell(row, cell) ?? cell;
                var (nextEditCell, nextEditRow) = FindNextEditableCell(row, cell, moveNext);
                SelectedRowIndex = nextEditRow?.Index;
                SelectedCellIndex = nextEditCell?.Index;
                if (nextEditCell != null && nextEditRow != null)
                {
                    _current = (nextEditRow.Uid!, nextEditCell.Uid!);
                    await TransitionToNextCell(cell, nextEditCell, nextEditRow).ConfigureAwait(true);
                }

                return;
            }

            await _parent.EditModule.EndEdit(cell).ConfigureAwait(true);
        }

        private (Row<object>? Row, Cell<object>? Cell) GetActiveEditRowAndCell()
        {
            var editModule = _parent.EditModule;
            Row<object>? row = editModule?.OriginalRow ?? editModule?.EditedRow ?? KeyInvokedRow;
            Cell<object>? cell = editModule?.OriginalCell ?? KeyInvokedCell;

            if (row == null)
            {
                row = _parent.Rows?.FirstOrDefault(currentRow => currentRow.Uid == _current.Item1);
            }

            if (row != null)
            {
                row = _parent.Rows?.FirstOrDefault(currentRow => currentRow.Uid == row.Uid) ?? row;
                cell = row.Cells.FirstOrDefault(currentCell => currentCell.IsEdit)
                    ?? (cell == null ? null : GridUtils.ResolveCurrentCell(row, cell));
                cell ??= row.Cells.FirstOrDefault(currentCell => currentCell.IsFocused)
                    ?? row.Cells.FirstOrDefault(currentCell => currentCell.Uid == _current.Item2);
            }

            return (row, cell);
        }

        /// <summary>
        /// Transitions focus from current cell to next cell and auto-enters edit mode.
        /// </summary>
        private async Task TransitionToNextCell(Cell<object> currentCell, Cell<object> nextCell, Row<object> nextRow)
        {
            // Move focus to the next editable cell
            nextCell.IsFocused = true;
            currentCell.IsFocused = false;

            // Auto-enter edit mode on the new cell
            if (_parent.EditModule != null)
            {
                _parent.EditModule.OriginalRow = nextRow;
                _parent.EditModule.OriginalCell = nextCell;
                await _parent.EditModule.EditCell(nextRow, nextCell).ConfigureAwait(true);
            }
        }

        #endregion

        #region Focused Cell Management

        internal string GetFocusClass(Cell<object> cell)
            => _focusedCells.TryGetValue(cell.Uid, out var focused) && focused
                ? "e-focus e-focused"
                : string.Empty;

        internal void SetFocusedCell(string cellUid, bool isFocused)
            => _focusedCells[cellUid] = isFocused;

        #endregion

        #region Cell Click and Focus Handling

        internal async Task CellClickHandler(ValueTuple<Row<object>, Cell<object>> target, MouseEventArgs e, bool isHeader = false)
        {
            var eventArgs = e;
            _isHeader = isHeader;
            IsChildFocused = false;
            Row<object> row = target.Item1;
            Cell<object> cell = target.Item2;
            ClearCurrent();
            SetCurrent(row, cell);
            _parent.SoftRefresh = true;
            if (!_parent.AllowSelection)
            {
                _parent.EventAggregator.Trigger("RowStateChanged", row);
                await Focus(row?.Uid!, cell?.Uid!, cellColIndex: cell?.Index + 1 ?? -1).ConfigureAwait(true);
            }
            if (_parent.ShowTooltip && _parent.TooltipInstance != null)
            {
                await (_parent.TooltipInstance?.CloseAsync())!.ConfigureAwait(true);
            }
        }

        internal async Task Refresh(Row<object> Row, Cell<object> Cell, bool isHeader = false, bool isCtrlOrShiftKeyPressed = false)
        {
            _isHeader = !isHeader ? isHeader : _isHeader;
            if (!_currentRows.Any(_ => _?.Uid == Row?.Uid))
            {
                return;
            }

            _parent.SoftRefresh = true;
            _parent.EventAggregator.Trigger("RowStateChanged", Row);
            if (isCtrlOrShiftKeyPressed)
            {
                await Focus(Row?.Uid ?? _current.Item1, Cell?.Uid ?? _current.Item2, cellColIndex: Cell?.Index + 1 ?? -1).ConfigureAwait(true);
            }
        }

        #endregion

        #region Current Cell and Focus State Management

        internal void ClearCurrent()
        {
            if (!string.IsNullOrEmpty(_current.Item1))
            {
                List<Row<object>> _allRows = _parent.HeaderRows.Concat(_parent.Rows).ToList();
                Row<object>? _row = _allRows.Find(_ => _?.Uid?.Equals(_current.Item1, StringComparison.Ordinal) == true);
                Cell<object>? _cell = _row?.Cells.Find(_ => _?.Uid == _current.Item2);
                //Below code is to focus the current cell in cell edit mode when press tab then esc key must focus on the current cell therfore we use SelectedCellIndex
                //_cell = _parent.EditSettings?.Mode == EditMode.Cell ? _row?.Cells.Where(_ => _.CellType == CellType.Data).ToList()[SelectedCellIndex ?? 0] : _cell;
                if (_cell != null && _cell.IsFocused)
                {
                    _cell.TabIndex = -1;
                    _cell.IsEdit = false;
                    _cell.IsFocused = false;
                    _cell.ShowFocusLine = false;
                    _parent.SoftRefresh = true;
                    _parent.EventAggregator.Trigger("RowStateChanged", _row!);
                }
            }
        }

        internal void SetCurrent(Row<object> row, Cell<object> cell, bool outline = false)
        {
            if (!cell.IsFocused)
            {
                cell.TabIndex = 0;
                cell.IsFocused = true;
                cell.ShowFocusLine = outline;
                SelectedCellIndex = cell.Index;
                if (_parent.EnableDomVirtualization)
                {
                    DomFocusRowIndex = row.Index;
                    DomFocusCellIndex = row.Index != null ? cell.Index : null;
                    DomFocusOutline = outline;
                    _domPendingFocus = null;
                }
                if (IsByKey && row.IsSelected)
                {
                    SelectedRowIndex = row.Index;
                }
                _current = (row?.Uid!, cell?.Uid!);
            }
        }

        internal async Task ClearFocus(Row<object>? _row = null, Cell<object>? _cell = null)
        {
            List<Row<object>>? _visibleFrozenheader = _frozenHeaderRows?.Where(_ => _.Visible != false).ToList();
            if (_parent.IsEdit && _parent.EditSettings?.Mode == EditMode.Normal)
            {
                return;
            }
            if (_cell != null && _cell.IsFocused)
            {
                _cell.TabIndex = -1;
                _cell.IsFocused = false;
                _cell.ShowFocusLine = false;
                if (_visibleFrozenheader?.Count == 0)
                {
                    ClearCurrent();
                }
                _current = (string.Empty, string.Empty);
                _parent.SoftRefresh = true;
                _parent.EventAggregator.Trigger("RowStateChanged", _row!);
                await Task.CompletedTask.ConfigureAwait(true);
            }
        }

        #endregion

        #region Keyboard Event Processing

        /// <summary>
        /// Detects the zone for the current (row, cell). Returns <see cref="GridKeyboardZone.None"/>
        /// for filter bar, toolbar, pager, search box, dialogs, column chooser, footer and any other chrome.
        /// </summary>
        private GridKeyboardZone DetectZone(Row<object>? row, Cell<object>? cell)
        {
            if (row == null || cell == null)
            {
                return GridKeyboardZone.None;
            }
            if (row.IsDetailRow || cell.CellType == CellType.Detail)
            {
                return GridKeyboardZone.DetailRow;
            }
            if (row.IsCaptionRow)
            {
                return GridKeyboardZone.GroupRow;
            }
            if (row.Index == null)
            {
                return GridKeyboardZone.Header;
            }
            if (cell.IsEdit || _parent.IsEdit)
            {
                return GridKeyboardZone.EditCell;
            }
            return GridKeyboardZone.DataCell;
        }

        internal static GridKeyboardZone ResolveKeyboardZone(bool isEditing, bool isHeader, GridKeyboardZone detectedZone)
        {
            if (isEditing)
            {
                return GridKeyboardZone.EditCell;
            }
            return isHeader ? GridKeyboardZone.Header : detectedZone;
        }

        internal async Task ProcessKeyDown(KeyboardEventArgs e, Row<object> row, Cell<object> cell, bool isHeader = false)
        {
            IsByKey = true;
            _isHeader = isHeader;
            GridKeySettings? _settings = _parent?.KeySettings;
            LastKeyCombination = e;
            LastNavigatedCellIdx = (int)(row?.Cells?.Find(c => c == cell)?.Index ?? -1);
            if (_parent != null && _parent.EnableColumnVirtualization)
            {
                List<GridColumn> orderedColumns = _parent.RearrangeColumns(_parent.Columns!);
                LastNavigatedCellIdx = cell != null && !string.IsNullOrEmpty(cell.Column?.Field) ? orderedColumns.FindIndex(x => x.Field == cell?.Column.Field)
                    : orderedColumns.Count - 1;
            }
            KeyInvokedRow = row!;
            KeyInvokedCell = cell!;
            var keyCombination = e.GetKeyCombination(isMacDevice: _parent!.IsMacDevice ?? false, isEnableUndoRedo: _parent!.EditSettings!.EnableUndoRedo);

            // Resolve the keystroke to a GridKeyboardAction using the existing
            // resolver. The resolver handles BOTH user-bound custom bindings AND
            // default chords via GridKeyboardActionDefaults, so the action id
            // drives the same switch the legacy keyCombination switch used to
            // drive. The resolver call is fully inlined here so the existing
            // switch remains the single source of truth for what each action does.
            //
            // This is the Blazor direct path (ProcessKeyDown). Pager focus and
            // toolbar focus are JS-side concepts; this entry point never has
            // them, so they default to false. See ProcessKeyCombination for
            // the JS-routed path which propagates the real pager/toolbar state
            // from ProcessGridKeyDown.
            //
            // Normalized-first: the resolver compares the physical `e.code`
            // via `GetNormalizedKey`, so we no longer require a non-null
            // `keyCombination` (the legacy token from `GetKeyCombination`).
            // This is what makes F3/F4/F5… and any chord outside the legacy
            // whitelist user-bindable. When the legacy token is null the
            // resolver simply skips the default/suppression layers and only
            // consults the user-bound ScopeBindings.
            GridKeyboardAction? action = null;
            GridKeyboardAction? suppressedAction = null;
            if (_parent == null)
            {
                return;
            }

            var zone = DetectZone(row, cell);
            if (zone == GridKeyboardZone.DataCell && GridKeyDispatchTable.IsHeaderOnlyLegacyToken(keyCombination))
            {
                zone = GridKeyboardZone.Header;
            }
            var resolved = GridKeyResolver.ResolveForKeystroke(
                _parent.CompiledKeyboardShortcuts,
                e.GetNormalizedKey(_parent.IsMacDevice ?? false),
                keyCombination,
                zone);
            action = resolved.Action;
            suppressedAction = resolved.SuppressedOriginalAction;
            // True ONLY when the keystroke matched a user-bound custom chord
            // (resolver Layer 1/2). Layer 3 (suppression), Layer 4 (built-in
            // default like plain F2) and Layer 5 (no match) all return
            // PreventDefault=false, so this guard excludes them. This is what
            // makes the if/else wrappers below safe for default F2 users: a
            // plain F2 keystroke keeps the legacy "no modifiers" guard intact.
            _isCustomKeyboardBinding = resolved.Consumed && resolved.PreventDefault && action != null;

            bool isFilterBar = _parent.AllowFiltering && _parent.FilterSettings!.Type == FilterType.FilterBar;
            bool isAddForm = _parent.EditSettings!.ShowAddNewRow;
            bool isArrowUp = keyCombination != null && keyCombination.Equals("ArrowUp", StringComparison.Ordinal);
            bool isArrowDown = keyCombination != null && keyCombination.Equals("ArrowDown", StringComparison.Ordinal);
            bool isArrowLeft = keyCombination != null && keyCombination.Equals("ArrowLeft", StringComparison.Ordinal);
            bool isArrowRight = keyCombination != null && keyCombination.Equals("ArrowRight", StringComparison.Ordinal);
            bool isArrowKeys = isArrowUp || isArrowDown || isArrowLeft || isArrowRight;
            bool isTabKey = IsTabKeyPressed(keyCombination);
            bool isShiftTabKey = IsShiftTabKeyPressed(keyCombination);
            bool isAltDownPressed = keyCombination?.Equals("AltDown", StringComparison.Ordinal) ?? false;
            bool isEnterKeyPressed = keyCombination?.Equals("Enter", StringComparison.Ordinal) ?? false;
            // Handle keyboard interactions for popup components in batch editing mode
            if (cell?.IsEdit == true && _parent.IsEdit && (_parent.EditSettings.Mode == EditMode.Batch || _parent.EditSettings.Mode == EditMode.Cell))
            {
                if ((isPopUpOpened && isEnterKeyPressed) || isArrowKeys || isAltDownPressed)
                {
                    isPopUpOpened = false;
                    return;
                }
            }

            if (suppressedAction.HasValue)
            {
                return;
            }

            var activeEditTarget = GetActiveEditRowAndCell();

            // Edit-end actions are also dispatched by ProcessGridKeyDown. Let that
            // path own Batch transitions so a single key cannot save and advance twice.
            if (_isCustomKeyboardBinding
                && _parent.EditSettings?.Mode == EditMode.Batch
                && activeEditTarget.Cell?.IsEdit == true
                && (action == GridKeyboardAction.EditEndAndTabNext
                    || action == GridKeyboardAction.EditEndAndShiftTabPrev))
            {
                return;
            }

            if (ShouldHandleKeyPressInCellEditMode(cell, e))
            {
                await HandleKeyPressInCellEditMode(row, cell, e?.Key).ConfigureAwait(true);
                return;
            }

            if (ShouldHandleEnterAndF2InCellEditMode(cell, keyCombination, isTabKey, isShiftTabKey))
            {
                await HandleEnterKeyInCellEditMode(row, cell, keyCombination).ConfigureAwait(true);
                if(cell?.IsEdit == false)
                {
                    var enterDirection = _parent.EditSettings?.EnterKeyDirection ?? EnterKeyDirection.None;
                    string[] directionAction = MapEnterKeyDirectionToActions(enterDirection, keyCombination);
                    ValueTuple<Row<object>, Cell<object>> currentRowAndCell = GetCurrentRowAndCell(row, cell);
                    if (row?.Uid != currentRowAndCell.Item1.Uid)
                    {
                        row = currentRowAndCell.Item1;
                        cell = currentRowAndCell.Item2;
                        if (directionAction.Length > 0)
                        {
                            await MoveFocusCell(directionAction, row!, cell!, e, keyCombination!).ConfigureAwait(true);
                        }
                        else
                        {
                            await FocusCell(row: row!, cell: cell!, navigator: (0, 0), e: e, actions: directionAction, keyCombination: keyCombination!).ConfigureAwait(true);
                        }
                        return;
                    }
                }
                if(IsEnterKeyPressed(keyCombination) || cell?.IsEdit == true)
                    return;
            }

            // EditMode.Cell: Tab/Shift+Tab commits the current cell and moves focus to the next/previous
            // editable cell, then auto-enters edit mode. If validation fails, the movement is blocked.
            // This enables continuous editing experience like Excel.
            if (ShouldHandleTabNavigationInCellEditMode(row!, cell, keyCombination))
            {
                await HandleTabNavigationInCellMode(row, cell, keyCombination).ConfigureAwait(true);
                return;
            }

            if (row?.Index == null && _parent.HeaderRows?.Count > 0 && _parent.HeaderRows.Where(e => e == row).Any() && (row!.Cells?.Where(e => e.Visible).FirstOrDefault() == cell || row.Cells?.Where(e => e.Visible).LastOrDefault() == cell))
            {
                cell!.IsFocused = true;
            }
            if (action == GridKeyboardAction.FocusFilterBar && cell?.Column?.AllowFiltering == true && isFilterBar
                && ((_parent.Rows?.Count > 0 && _parent.Rows.FirstOrDefault()!.Equals(row) && isArrowUp && !isAddForm) || (_isHeader && isArrowDown)))
            {
                int columnIndex = -1;
                bool filterTemplate = false;
                ClearCurrent();
                if (isArrowUp && _parent.SelectedRecords?.Count > 0 && !_parent.SelectionSettings!.PersistSelection && _parent.SelectionModule != null)
                {
                    await _parent.SelectionModule.ClearSelection().ConfigureAwait(true);
                }
                if (_parent.Columns?.Where(col => !col.Visible).Any() == true)
                {
                    var visibleColumns = await _parent.GetVisibleColumnsAsync().ConfigureAwait(true);
                    columnIndex = visibleColumns.FindIndex(col => col.Index == cell.Column.Index);
                    columnIndex = _parent.AllowGrouping && _parent.GroupSettings?.Columns != null && _parent.GroupSettings.Columns.Length > 0 && cell.Column.FilterTemplate != null ? _parent.GroupSettings.Columns.Length + columnIndex : columnIndex;
                }
                filterTemplate = ((cell?.IsDataCell == true || _isHeader) && cell?.Column?.FilterTemplate != null) || IsGroupingFilterTemplate(cell!);
                await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, filterTemplate, columnIndex }).ConfigureAwait(true);
                return;
            }

            if (row?.Index == null && !isFilterBar && GetLastVisibleColumn() != null && cell?.Index == GetLastVisibleColumn()?.Index && _parent.Rows?.Count == 0 && _isHeader && keyCombination != null && keyCombination.Equals("Tab", StringComparison.Ordinal))
            {
                ClearCurrent();
                await _parent.InvokeMethod("sfBlazor.Grid.blurActiveElement", new object[] { _parent.DataId }).ConfigureAwait(true);
                return;
            }
            if (!string.IsNullOrEmpty(keyCombination) && keyCombination == "Insert" && _parent.SelectedRecords?.Count == 0)
            {
                return;
            }
            if (cell!.IsFocused && _parent.EditSettings!.AllowEditing && _parent.EditSettings.Mode == EditMode.Batch && !string.IsNullOrEmpty(keyCombination) && keyCombination == "Enter")
            {
                cell.IsFocused = false;
            }

            bool isColumnFilter = _parent.Columns?.Where(e => e.AllowFiltering).Any() == true;
            bool isFilterTemplate = false;

            // to focus the filter bar if tab is pressed from the last header cell.
            if (isTabKey && isHeader && isColumnFilter && row?.Index == null && cell?.Index == row?.Cells?.Where(e => e.Visible).LastOrDefault()?.Index && isFilterBar)
            {
                ClearCurrent();
                isFilterTemplate = _parent.Columns?.Where(e => e.Visible).Any() == true && _parent.Columns?.Where(e => e.Visible).FirstOrDefault()?.FilterTemplate != null;
                await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, isFilterTemplate, -1 }).ConfigureAwait(true);
                return;
            }

            // to focus the last filter bar cell when shiftTab is pressed from the first content cell.
            if (isShiftTabKey && _parent.Rows?.Count > 0 && isColumnFilter && _parent.Rows.First().Equals(row) && _parent.Rows.First().Cells?.Where(_ => _.Visible).FirstOrDefault()!.Equals(cell) == true && isFilterBar && !isAddForm)
            {
                ClearCurrent();
                isFilterTemplate = _parent.Columns?.Where(e => e.Visible).Any() == true && _parent.Columns.Where(e => e.Visible).LastOrDefault()?.FilterTemplate != null;
                if (cell?.IsEdit == true)
                {
                    await _parent.EditModule!.SaveCell().ConfigureAwait(true);
                }
                await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, isFilterTemplate, -1 }).ConfigureAwait(true);
                return;
            }

            if(isAddForm)
            {
                // to focus on the first cell of the 'Add form' when tab is pressed from the last header cell.
                if (isTabKey && isHeader && row?.Index == null && cell?.Index == row?.Cells?.Where(e => e.Visible).LastOrDefault()?.Index && _parent.EditSettings!.NewRowPosition == NewRowPosition.Top)
                {
                    FocusAddForm(keyCombination!);
                    return;
                }

                // to focus on the last cell of the 'Add form' when shiftTab is pressed from the first content cell.
                if (isShiftTabKey && _parent.Rows?.Count > 0 && _parent.Rows.First().Equals(row) && _parent.Rows.First().Cells?.Where(_ => _.Visible).FirstOrDefault()?.Equals(cell) == true && _parent.EditSettings!.NewRowPosition == NewRowPosition.Top)
                {
                    FocusAddForm(keyCombination!);
                    return;
                }

                if ((_parent.Rows?.Count > 0 && _parent.Rows.FirstOrDefault()!.Equals(row) && isArrowUp) || (_isHeader && isArrowDown))
                {
                    FocusAddForm(keyCombination!);
                    return;
                }
            }

            if (row?.IsSelected == true && (isTabKey || isShiftTabKey))
            {
                SelectedRowIndex = row?.Index;
            }
            SelectedCellIndex = cell?.Index;

            if (string.IsNullOrEmpty(keyCombination) ||
                (keyCombination.Equals("Space", StringComparison.Ordinal) &&
                 (_parent.IsEdit || (!isHeader && cell?.Column != null &&
                                     cell.Column.Type.Equals(ColumnType.CheckBox) && !_parent.SelectionSettings!.CheckboxOnly))))
            {
                return;
            }

            string[] actions = _settings?.GetAction(keyCombination) ?? Array.Empty<string>();
            GridKeyboardAction? legacyNavigationAction = actions.FirstOrDefault() switch
            {
                "MoveUpCell" => GridKeyboardAction.NavigateUp,
                "MoveDownCell" => GridKeyboardAction.NavigateDown,
                "MoveLeftCell" => GridKeyboardAction.NavigateLeft,
                "MoveRightCell" => GridKeyboardAction.NavigateRight,
                _ => null
            };
            if (legacyNavigationAction.HasValue
                && action != legacyNavigationAction
                && (_isCustomKeyboardBinding
                    || _parent.CompiledKeyboardShortcuts.OverrideActions.Contains(legacyNavigationAction.Value)))
            {
                actions = Array.Empty<string>();
            }

            // OPSX-20261016-ctrl-arrow-virtualization-nav: Handle Ctrl+Arrow Up/Down for first/last row navigation
            // in virtualized grids. Silent no-op when virtualization is off, infinite scroll is on, or grouping has columns grouped.
            // Restriction: Only execute when focus is on content cells (not headers, filter rows, or grouping headers).
            // Feature works when: AllowGrouping is false OR (AllowGrouping is true AND Columns is null - meaning grouping enabled but no columns grouped)
            if ((keyCombination.Equals("CtrlUp", StringComparison.Ordinal) || keyCombination.Equals("CtrlDown", StringComparison.Ordinal))
                && !_isCustomKeyboardBinding
                && !_isHeader
                && !_parent.IsRenderedFromTreeGrid
                && (_parent.EnableVirtualization || IsDomVirtualNavigationSupported())
                && !_parent.EnableInfiniteScrolling
                && (!_parent.AllowGrouping || _parent.GroupSettings?.Columns == null))
            {
                await HandleCtrlArrowFirstLastRowNavigation(keyCombination, e).ConfigureAwait(true);
                return;
            }
            bool isMoveUpOrDown = actions.Length > 0 && (actions[0].Equals("MoveUpCell", StringComparison.Ordinal) || actions[0].Equals("MoveDownCell", StringComparison.Ordinal));

            // Escape from template cell to parent cell
            bool isReturn = EscapeFromTempalteCell(keyCombination) || (_parent.AllowSorting == false && keyCombination == "Enter" && row?.Index == null && _parent.HeaderRows?.Count > 0 && _parent.HeaderRows.Where(x => x == row).Any()) || (keyCombination == "ShiftEnter" && row?.Index == 0);
            if (isReturn)
            {
                return;
            }

            if (await FocusChild(row!, cell!, e).ConfigureAwait(true))
            {
                return;
            }

            if (isHeader && _parent.HeaderRows!.FirstOrDefault()!.Cells?.Where(x => x.Index != null).FirstOrDefault() == cell && keyCombination == "Tab")
            {
                foreach (var item in _parent.HeaderRows![0].Cells)
                {
                    item.IsFocused = false;
                }
            }

            if (!_isHeader && _parent.Rows?.Count == 0 || !cell?.IsEdit == true && !cell?.EditDisabled == true &&
                ShouldExitGrid(actions!, row!, cell!, keyCombination))
            {
                ClearCurrent();
                return;
            }

            if (actions.Length > 0)
            {
                await MoveFocusCell(actions, row!, cell!, e, keyCombination, action).ConfigureAwait(true);
            }
            else
            {
                List<Row<object>>? _rows = _currentRows?.Where(x => x.Visible != false).ToList();
                List<Row<object>>? _dataRows = _rows?.Where(_ => _.IsDataRow).ToList();
                Row<object>? nextRow = row;
                Cell<object> nextCell = cell!;
                bool isCheckBoxCell = cell?.Column != null && cell.Column.Type.Equals(ColumnType.CheckBox);

                switch (action)
                {
                    case GridKeyboardAction.NavigateGridStart:
                        if (IsDomVirtualNavigationSupported() && await TryFocusDomRowAsync(0, null, e, keyCombination).ConfigureAwait(true))
                        {
                            return;
                        }
                        nextRow = _rows?[0]!;
                        nextCell = _rows?[0]?.Cells?.Where(x => x.Visible && !x.CellType.Equals(CellType.RowDrag)).First()!;
                        break;
                    case GridKeyboardAction.NavigateGridEnd:
                        if (IsDomVirtualNavigationSupported() && await TryFocusDomRowAsync(_parent.DomVirtualModule!.TotalRowCount - 1, int.MaxValue, e, keyCombination).ConfigureAwait(true))
                        {
                            return;
                        }
                        nextRow = _rows?.Last()!;
                        nextCell = nextRow?.Cells?.Where(x => x.Visible).Last()!;
                        break;
                    case GridKeyboardAction.NavigateRowStart:
                        nextCell = nextRow?.Cells?.Where(x => x.Visible && !x.CellType.Equals(CellType.Indent) && !x.CellType.Equals(CellType.RowDrag)).First()!;
                        break;
                    case GridKeyboardAction.NavigateRowEnd:
                        nextCell = nextRow?.Cells?.Where(x => x.Visible).Last()!;
                        break;
                    case GridKeyboardAction.SelectToggle:
                    case GridKeyboardAction.ToggleHeaderCheckbox:
                        // The resolver maps Space to SelectToggle (data cell) or
                        // ToggleHeaderCheckbox (header zone). For checkbox cells
                        // the legacy Space handler also routes the selection
                        // toggle — keep that body here.
                        if (isCheckBoxCell && !isHeader && !nextRow?.IsSelected == true)
                        {
                            await _parent.SelectionModule!.SelectByRow(nextRow!).ConfigureAwait(true);
                        }
                        else if (isCheckBoxCell && nextRow?.IsSelected == true && (_parent != null && _parent.SelectionModule != null && _parent.SelectionModule.CanToggle()) && !isHeader)
                        {
                            await _parent.SelectionModule.ClearSelectionByRow(nextRow).ConfigureAwait(true);
                        }
                        // Header actions are dispatched by GridHeaderBase through
                        // BeforeCellFocus. Do not toggle here as well; that would
                        // run the header-checkbox workflow twice for one keystroke.
                        break;
                    case GridKeyboardAction.NavigateUp:
                        await MoveFocusCell(MoveUpCellAction, row!, cell!, e, keyCombination, action).ConfigureAwait(true);
                        return;
                    case GridKeyboardAction.NavigateDown:
                        await MoveFocusCell(MoveDownCellAction, row!, cell!, e, keyCombination, action).ConfigureAwait(true);
                        return;
                    case GridKeyboardAction.NavigateLeft:
                        await MoveFocusCell(MoveLeftCellAction, row!, cell!, e, keyCombination, action).ConfigureAwait(true);
                        return;
                    case GridKeyboardAction.NavigateRight:
                        await MoveFocusCell(MoveRightCellAction, row!, cell!, e, keyCombination, action).ConfigureAwait(true);
                        return;
                    case GridKeyboardAction.ToggleRowSelection:
                        if (_parent.AllowSelection && _parent.SelectionModule != null && nextRow != null)
                        {
                            if (nextRow.IsSelected)
                            {
                                await _parent.SelectionModule.ClearSelectionByRow(nextRow).ConfigureAwait(true);
                            }
                            else
                            {
                                await _parent.SelectionModule.SelectByRow(nextRow).ConfigureAwait(true);
                            }
                        }
                        break;
                    case GridKeyboardAction.SelectAll:
                        await _parent.SelectAllRecordsAsync().ConfigureAwait(true);
                        break;
                    case GridKeyboardAction.StartEdit:
                        if (_isCustomKeyboardBinding || (e != null && !e.AltKey && !e.CtrlKey && !e.ShiftKey))
                        {
                            await StartEditFromKeyboardAsync(row, cell).ConfigureAwait(true);
                        }
                        return;

                    // ---- Phase 15: focus jumps (LOW risk) ----
                    case GridKeyboardAction.MoveFocusToFirstRow:
                        // Default AltW — move focus to first visible cell of first content row.
                        if (IsDomVirtualNavigationSupported() && await TryFocusDomRowAsync(0, null, e, keyCombination).ConfigureAwait(true))
                        {
                            return;
                        }
                        if (_parent.Rows != null && _parent.Rows.Count > 0 && string.IsNullOrEmpty(_parent.EditModule?.AlertMessage))
                        {
                            var firstRow = _parent.Rows.Where(_ => _.Visible).FirstOrDefault();
                            var firstCell = firstRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).FirstOrDefault();
                            if (firstRow != null && firstCell != null)
                            {
                                ClearCurrent();
                                SetCurrent(firstRow, firstCell, true);
                                _parent.SoftRefresh = true;
                                _parent.EventAggregator.Trigger("RowStateChanged", firstRow);
                                await Focus(firstRow.Uid!, firstCell.Uid!, "MoveRightCell", keyCombination, cellColIndex: (firstCell.Index ?? -1) + 1).ConfigureAwait(true);
                            }
                        }
                        return;
                    case GridKeyboardAction.FocusFilterBar:
                        // Default ArrowUp from first content cell OR ArrowDown from header.
                        // Reuse the existing JS interop; route via resolver when a custom
                        // binding is also present.
                        if (_isCustomKeyboardBinding
                            && _parent.AllowFiltering
                            && _parent.FilterSettings?.Type == FilterType.FilterBar
                            && cell?.Column?.AllowFiltering == true)
                        {
                            int columnIndex = -1;
                            bool filterTemplate = ((cell?.IsDataCell == true || _isHeader) && cell?.Column?.FilterTemplate != null) || IsGroupingFilterTemplate(cell!);
                            ClearCurrent();
                            await _parent.InvokeMethod("sfBlazor.Grid.focusFilterBar", new object[] { _parent.DataId, keyCombination!, filterTemplate, columnIndex }).ConfigureAwait(true);
                            return;
                        }
                        break;
                    case GridKeyboardAction.FocusAddForm:
                        // Default ArrowDown from header OR ArrowUp from first content cell
                        // when ShowAddNewRow is enabled. Reuse existing private method.
                        if (_parent.EditSettings?.ShowAddNewRow == true)
                        {
                            FocusAddForm(keyCombination!);
                            return;
                        }
                        break;

                    // ---- Phase 14: header popups (MED risk) ----
                    case GridKeyboardAction.OpenColumnMenu:
                        if (_parent.ShowColumnMenu && cell?.Column?.ShowColumnMenu == true && isHeader)
                        {
                            _parent.ColumnMenuColumn = cell?.Column;
                            _parent.EventAggregator.Trigger("ColumnMenuUpdate", null!);
                            if (_parent.ColumnMenuInstance != null && _parent.ColumnMenuInstance.CssClass.Contains("e-hide-menu", StringComparison.Ordinal))
                            {
                                await _parent.ColumnMenuInstance.OpenAsync(1, 1).ConfigureAwait(true);
                            }
                            return;
                        }
                        break;
                    case GridKeyboardAction.OpenFilterMenu:
                        if (_parent.AllowFiltering && cell?.Column?.AllowFiltering == true && _parent.FilterSettings!.Type != FilterType.FilterBar && isHeader && _parent.FilterModule != null)
                        {
                            _parent.FilterModule.FilterIconIsClicked = true;
                            _parent.FilterModule.FilterIconColumn = cell?.Column!;
                            _parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                            return;
                        }
                        break;

                    // ---- Phase 11: clipboard (Common scope) ----
                    // The JS gate (scripts/clipboard.ts) normally consumes Ctrl+C/Ctrl+V
                    // before the keydown reaches C#. These cases are the C# backstop
                    // and the route for any custom-bound chord.
                    // (ProcessKeyDown has no BeforeCellFocus handle; suppression
                    //  is applied in the ProcessKeyCombination sister switch.)
                    case GridKeyboardAction.Copy:
                        if (!_parent.IsEdit)
                        {
                            await _parent.CopyAsync(null).ConfigureAwait(true);
                            return;
                        }
                        break;
                    case GridKeyboardAction.CopyWithHeaders:
                        if (!_parent.IsEdit)
                        {
                            await _parent.CopyAsync(true).ConfigureAwait(true);
                            return;
                        }
                        break;
                    case GridKeyboardAction.Paste:
                        if (!_parent.IsEdit)
                        {
                            await _parent.PasteAsync().ConfigureAwait(true);
                            return;
                        }
                        break;

                    // ---- Phase 12: undo/redo (four-gate alignment) ----
                    // The legacy inline branch (CtrlZ / CtrlY / CtrlShiftZ) at the top of
                    // ProcessKeyCombination returns before the resolver is consulted, so
                    // these cases are the route for any custom-bound chord (e.g. Alt+Z).
                    case GridKeyboardAction.Undo:
                        if (_parent.EditSettings?.EnableUndoRedo == true
                            && _parent.EditSettings?.Mode == EditMode.Batch
                            && _parent.UndoRedoManager != null)
                        {
                            if (_parent.IsEdit && _parent.EditModule != null)
                            {
                                await _parent.EditModule.CloseEdit(true).ConfigureAwait(true);
                            }
                            await _parent.UndoRedoManager.PerformUndoAsync().ConfigureAwait(true);
                            await _parent.CallStateHasChangedAsync().ConfigureAwait(true);
                            _parent.EventAggregator?.Trigger("UndoRedoStackChanged", null!);
                            return;
                        }
                        break;
                    case GridKeyboardAction.Redo:
                        if (_parent.EditSettings?.EnableUndoRedo == true
                            && _parent.EditSettings?.Mode == EditMode.Batch
                            && _parent.UndoRedoManager != null)
                        {
                            if (_parent.IsEdit && _parent.EditModule != null)
                            {
                                await _parent.EditModule.CloseEdit(true).ConfigureAwait(true);
                            }
                            await _parent.UndoRedoManager.PerformRedoAsync().ConfigureAwait(true);
                            await _parent.CallStateHasChangedAsync().ConfigureAwait(true);
                            _parent.EventAggregator?.Trigger("UndoRedoStackChanged", null!);
                            return;
                        }
                        break;

                    // ---- Phase 16: selection extensions ----
                    // Shift+Up/Down/Left/Right is already consumed by Selection.cs
                    // (ClearRedundantCellsInBoxMode → SelectRangeOfCells) and reaches
                    // this switch only as the resolution result for a custom-bound
                    // chord (e.g. Alt+Shift+Down). Return to mark consumed.
                    case GridKeyboardAction.SelectRowRange:
                        return;

                    // ExtendSelection is mouse-driven (Shift+Click / Ctrl+Click) and
                    // marked [NonBindable] by the validator (SFGK011). This case is
                    // a no-op that exists so the resolver can return a typed action
                    // and the legacy MouseAndKeyArgs payload can be introspected
                    // programmatically via grid.State.
                    case GridKeyboardAction.ExtendSelection:
                        return;

                    // ---- Phase 17: edit-end tab transitions ----
                    // Cell-mode edit already ends on Tab/Shift+Tab via
                    // HandleTabNavigationInCellMode (called from FocusCell). This case
                    // is the C# backstop for the EditCell-zone resolved action; the
                    // existing tab-navigation behavior is preserved.
                    case GridKeyboardAction.EditEndAndTabNext:
                        if (activeEditTarget.Cell?.IsEdit == true && _parent.EditModule != null)
                        {
                            await HandleEditEndNavigation(activeEditTarget.Row, activeEditTarget.Cell, moveNext: true).ConfigureAwait(true);
                        }
                        return;
                    case GridKeyboardAction.EditEndAndShiftTabPrev:
                        if (activeEditTarget.Cell?.IsEdit == true && _parent.EditModule != null)
                        {
                            await HandleEditEndNavigation(activeEditTarget.Row, activeEditTarget.Cell, moveNext: false).ConfigureAwait(true);
                        }
                        return;

                    case null:
                        // AltDown has no default action id and falls through here.
                        // For users with no custom bindings, route it to the same
                        // OpenColumnMenu / OpenFilterMenu handlers used by
                        // custom-bound chords (cases above). If the user has bound
                        // a custom key for that action, the default AltDown must
                        // NOT also fire — the custom key alone is the trigger, by
                        // the same principle every other action follows (resolver
                        // Layer 3 / OverrideActions suppression). This eliminates
                        // the duplicated column-menu / filter-icon logic that used
                        // to live inline in this branch — the named-action cases
                        // are now the single source of truth.
                        if (keyCombination == "AltDown" && isHeader && cell?.Column != null)
                        {
                            var overrides = _parent.CompiledKeyboardShortcuts.OverrideActions;
                            if (_parent.ShowColumnMenu && cell.Column.ShowColumnMenu
                                && !overrides.Contains(GridKeyboardAction.OpenColumnMenu))
                            {
                                action = GridKeyboardAction.OpenColumnMenu;
                                goto case GridKeyboardAction.OpenColumnMenu;
                            }
                            if (_parent.AllowFiltering && cell.Column.AllowFiltering
                                && _parent.FilterSettings!.Type != FilterType.FilterBar && _parent.FilterModule != null
                                && !overrides.Contains(GridKeyboardAction.OpenFilterMenu))
                            {
                                action = GridKeyboardAction.OpenFilterMenu;
                                goto case GridKeyboardAction.OpenFilterMenu;
                            }
                        }
                        break;
                    default:
                        break;
                }

                // Other than navigation other keys will be handled here.
                await FocusCell(row: nextRow!, cell: nextCell, navigator: (0, 0), e: e, actions: actions, keyCombination: keyCombination, resolvedAction: action).ConfigureAwait(true);
            }
            // Only allow scrollToFocusedCell for Shift+Tab and Left Arrow when frozen columns exist
            bool isShiftTab = keyCombination != null && keyCombination.Equals("ShiftTab", StringComparison.Ordinal);
            bool isLeftArrow = keyCombination != null && keyCombination.Equals("ArrowLeft", StringComparison.Ordinal);
            bool shouldScrollForLeftFrozen = (_parent!.FrozenColumns > 0 || _parent.FreezeModule!.GetFreezeLeftCount() > 0) && (isShiftTab || isLeftArrow);
            if (_parent != null && _parent.FreezeModule!.GetFrozenCount() > 0 && actions?.Length != 0 && !(isMoveUpOrDown && _parent.IsEdit && _parent.EditSettings!.Mode == EditMode.Batch) 
                && (_parent.FreezeModule!.GetFreezeRightCount() > 0 || shouldScrollForLeftFrozen))
            {
                await _parent.InvokeMethod("sfBlazor.Grid.scrollToFocusedCell", _parent.DataId).ConfigureAwait(true);
            }
            _previouslyFocusedCell = cell!;
            KeyInvokedRow = null!;
            KeyInvokedCell = null!;
        }

        #endregion

        #region Focus Movement and Navigation

        private async void FocusAddForm(string keyCombination)
        {
            ClearCurrent();
            await _parent.InvokeMethod("sfBlazor.Grid.focusAddForm", _parent.DataId, keyCombination).ConfigureAwait(true);
        }
        private bool EscapeFromTempalteCell(string keyCombination)
        {
            bool isReturn = false;
            if (IsChildFocused)
            {
                if (keyCombination == "Escape")
                {
                    IsChildFocused = false;
                }
                else
                {
                    isReturn = true;
                }
            }
            return isReturn;
        }

        #endregion

        #region Focus Cell Movement

        private async Task MoveFocusCell(string[] actions, Row<object> row, Cell<object> cell, KeyboardEventArgs e, string keyCombination, GridKeyboardAction? resolvedAction = null)
        {
            switch (actions?[0])
            {
                case "MoveLeftCell":
                    if (row?.Index == null && cell?.Index == GetFirstVisibleColumn()?.Index && (keyCombination.Equals("ArrowLeft", StringComparison.Ordinal)))
                    {
                        return;
                    }
                    await FocusCell(row: row!, cell: cell!, navigator: (0, -1), e: e, actions: actions, keyCombination: keyCombination, resolvedAction: resolvedAction).ConfigureAwait(true);
                    break;
                case "MoveRightCell":
                    if (row?.Index == null && cell?.Index == GetLastVisibleColumn()?.Index && (keyCombination.Equals("ArrowRight", StringComparison.Ordinal) || (_parent.AllowFiltering && _parent.FilterSettings?.Type == FilterType.FilterBar && _parent.Columns?.Where(e => e.AllowFiltering).Any() == true)))
                    {
                        return;
                    }
                    await FocusCell(row: row!, cell: cell!, navigator: (0, 1), e: e, actions: actions, keyCombination: keyCombination, resolvedAction: resolvedAction).ConfigureAwait(true);
                    break;
                case "MoveUpCell":
                    await FocusCell(row: row, cell: cell, navigator: (-1, 0), e: e, actions: actions, keyCombination: keyCombination, resolvedAction: resolvedAction).ConfigureAwait(true);
                    break;
                case "MoveDownCell":
                    await FocusCell(row: row, cell: cell, navigator: (1, 0), e: e, actions: actions, keyCombination: keyCombination, resolvedAction: resolvedAction).ConfigureAwait(true);
                    break;
            }
        }

        #endregion

        #region Grid Exit Validation

        // This ensures the key control can be provided to other elements in page.
        private bool ShouldExitGrid(string[] actions, Row<object> row, Cell<object> cell, string combination)
        {
            var _stackedHeader = HeaderRows?.Where(_ => _.Visible != false).ToList();
            if (actions?.Length != 0)
            {
                switch (combination)
                {
                    case "ShiftTab":
                        if (ShouldExit(row, _stackedHeader!))
                        {
                            _parent.InvokeMethod("sfBlazor.Grid.gridFocus", new object[] { _parent.DataId, true, false, combination }).ConfigureAwait(true);
                            return true;
                        }
                        break;
                    case "Tab":
                        if (_parent.FreezeModule!.GetFrozenCount() > 0 && _parent.FreezeModule!.GetFreezeRightCount() > 0)
                        {
                            var rightFrozenColumn = _parent.FreezeModule!.GetFrozenRightFreezeColumns();
                            var lastRightFrozenColumn = rightFrozenColumn?.Where(x => x.Visible).LastOrDefault();
                            if (cell?.Index == lastRightFrozenColumn?.Index 
                                || (!string.IsNullOrEmpty(cell?.Column?.Field) && cell?.Column?.Field == lastRightFrozenColumn?.Field && _parent.EnableColumnVirtualization))
                            {
                                return false;
                            }
                        }
                        if (row?.Index == null && (cell?.Index == GetLastVisibleColumn()?.Index || _stackedHeader?.Count > 1))
                        {
                            return false;
                        }
                        else
                        {
                            Row<object>? _veryLastRow = _currentRows?.Where(e => e.Visible).LastOrDefault();
                            Cell<object>? _veryLastCell = _veryLastRow?.Cells?.Where(_ => _.Visible).LastOrDefault();
                            return _veryLastCell?.Equals(cell) == true && !row?.IsDetailRow == true;
                        }
                }
            }

            return false;
        }

        private bool ShouldExit(Row<object> row, List<Row<object>> stackedHeader)
        {
            var _visibleFrozenheader = _frozenHeaderRows?.Where(_ => _.Visible != false).ToList();
            var focusedCellIndex = row?.Cells?.Where(_ => _.IsFocused).Any() == true ? row.Cells.Where(_ => _.IsFocused).FirstOrDefault()?.Index : null;
            var focusedCell = row?.Cells?.Where(_ => _.IsFocused).FirstOrDefault();
            var firstColumnIndex = _parent.FreezeModule!.GetFrozenCount() > 0 ? focusedCell?.Column?.Index : _parent.Columns?.Where(e => e.Visible).FirstOrDefault()?.Index;

            if (row?.Index == null && focusedCellIndex != null && firstColumnIndex != null && focusedCellIndex == firstColumnIndex && ((_visibleFrozenheader?.Count == 0 && stackedHeader?[0]?.Cells?.Where(e => e.Visible).FirstOrDefault()?.IsFocused == true) || (_visibleFrozenheader?.Count != 0 && (_visibleFrozenheader != null && _visibleFrozenheader[0].Cells?[0]?.IsFocused == true))))
            {
                return true;
            }
            if (row?.IsCaptionRow == true && !_parent.GroupSettings!.ShowGroupedColumn && _parent.Columns?.Count == _parent.Columns?.Where(e => e.Visible == false).Count() && _currentRows?.FirstOrDefault()?.Cells?.FirstOrDefault()?.IsFocused == true)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region Grid Key Down Processing

        internal async Task ProcessGridKeyDown(KeyboardEventArgs e, BeforeCellFocus? bf = null, bool isPagerFocused = true, bool isToolbarFocused = false, int? cellIndex = null, int? rowIndex = null, int? templateCellIndex = null, bool focusColumnTemplate = false, string? focusCellUid = null)
        {
            var keyCombination = e.GetKeyCombination(isMacDevice: _parent.IsMacDevice ?? false, isEnableUndoRedo: _parent!.EditSettings!.EnableUndoRedo);
            var isTabKey = keyCombination != null && keyCombination.Equals("Tab", StringComparison.Ordinal);
            var isShiftTabKey = keyCombination != null && keyCombination.Equals("ShiftTab", StringComparison.Ordinal);
            var editedRow = _parent.EditModule!.EditedRow;
            var editedRowFirstCell = editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag).FirstOrDefault();
            var editedRowLastCell = editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag).LastOrDefault();
            var cellUId = cellIndex != null ? editedRow?.Cells[(int)cellIndex] : null;
            var isExcelOrCheckBoxFilter = _parent.AllowFiltering && ((_parent.FilterSettings != null && _parent.FilterSettings.Type != FilterType.FilterBar && _parent.FilterSettings.Type != FilterType.Menu) || (_parent .FilterModule != null &&  _parent.FilterModule.FilterIconColumn?.FilterSettings != null && !_parent.FilterModule.FilterIconColumn.FilterSettings.Type.Equals(FilterType.FilterBar) && !_parent.FilterModule.FilterIconColumn.FilterSettings.Type.Equals(FilterType.Menu)));
            ClickedCheckBoxId = isExcelOrCheckBoxFilter ? string.Empty : ClickedCheckBoxId;
            IsSelectAllClicked = isExcelOrCheckBoxFilter ? false : IsSelectAllClicked;

            // Handling the focus from the template column.		
            if (rowIndex != null && (templateCellIndex != null || focusCellUid != null) && focusColumnTemplate && keyCombination != null)
            {
                var templateRow = _parent.Rows?.Where(e => e.Index == (int)rowIndex).FirstOrDefault();
                var templateCell = focusCellUid != null
                    ? templateRow?.Cells?.FirstOrDefault(cell => string.Equals(cell.Uid, focusCellUid, StringComparison.Ordinal))
                    : templateRow?.Cells[(int)templateCellIndex!];
                if (templateRow == _parent.Rows?.Where(e => e.IsLastRow).FirstOrDefault() &&
                    templateCell == templateRow?.Cells?.Where(e => e.Visible).LastOrDefault() &&
                    templateCell?.CellType != CellType.CommandColumn &&
                    keyCombination.Equals("Tab", StringComparison.Ordinal))
                {
                    return;
                }
                IsChildFocused = false;
                if (_parent.IsRenderedFromTreeGrid && e != null && _parent.IsEdit && keyCombination == "Escape"
                    && templateRow != null && templateCell != null)
                {
                    await ProcessKeyDown(e, templateRow, templateCell).ConfigureAwait(true);
                    await ProcessKeyCombination(keyCombination, 0).ConfigureAwait(true);
                }
                ClearCurrent();
                SetCurrent(templateRow!, templateCell!);
                await Task.Yield();
                var focusColumnIndex = focusCellUid != null ? -1 : templateCell?.Index + 1 ?? -1;
                await Focus(templateRow?.Uid!, templateCell?.Uid!, keyCombination: keyCombination, cellColIndex: focusColumnIndex).ConfigureAwait(true);
                return;
            }

            // Handling the mouse click focusing when clicked while the row is in editable state.
            if (cellIndex != null && editedRow != null)
            {
                bool isShiftTabAndDrag = isShiftTabKey && _parent.AllowRowDragAndDrop;
                bool isTabAndDrag = isTabKey && _parent.AllowRowDragAndDrop;

                if (_parent.IsEdit || (isTabKey && editedRowLastCell?.Index == (int)cellIndex) || (isShiftTabKey && editedRowFirstCell?.Index == (int)cellIndex) || (isTabAndDrag && editedRowLastCell?.Uid == cellUId?.Uid) || (isShiftTabAndDrag && editedRowFirstCell?.Uid == cellUId?.Uid) && (isTabKey || isShiftTabKey))
                {
                    ClearCurrent();
                    SetCurrent(editedRow, editedRow.Cells[(int)cellIndex]);
                }
            }

            if (ChangeLastCellTabIndex)
            {
                ChangeLastCellTabIndex = false;
                _parent.EventAggregator.Trigger("RowStateChanged", _parent.Rows?.Last()!);
            }

            // Handling the focus when a record is in editable state in inline editing.
            if (!isToolbarFocused && _parent.IsEdit && _parent.EditSettings?.Mode == EditMode.Normal && _parent.EditSettings.AllowEditing && editedRow != null)
            {
                var currentCellObject = editedRow.Cells?.Where(e => e.IsFocused && e.CellType != CellType.RowDrag).FirstOrDefault();
                var currentCellIndex = !_parent.AllowRowDragAndDrop ? currentCellObject?.Index : editedRow?.Cells?.IndexOf(currentCellObject!);
                var lastCellIndex = !_parent.AllowRowDragAndDrop ? editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag && (e.Column != null && e.Column.AllowEditing)).LastOrDefault()?.Index : editedRow?.Cells?.IndexOf(editedRowLastCell!);
                var firstCellIndex = !_parent.AllowRowDragAndDrop ? editedRow?.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag && (e.Column != null && e.Column.AllowEditing)).FirstOrDefault()?.Index : editedRow?.Cells?.IndexOf(editedRowFirstCell!);

                if (currentCellIndex != null && lastCellIndex != null && firstCellIndex != null && ((isTabKey && currentCellIndex != lastCellIndex) || (isShiftTabKey && currentCellIndex != firstCellIndex)))
                {
                    var focusCellIndex = GetNextCellIndex(isTabKey, (int)currentCellIndex, editedRow!);
                    // to change the cell index if the next column is an primary key column while editing.
                    if (editedRow?.Cells?[(int)focusCellIndex]?.Column?.IsPrimaryKey == true)
                    {
                        focusCellIndex = isTabKey ? focusCellIndex + 1 : focusCellIndex == 0 ? focusCellIndex : focusCellIndex - 1;
                    }
                    // handling the columns visiblity set to false.
                    while (editedRow?.Cells?[(int)focusCellIndex]?.Visible != true)
                    {
                        focusCellIndex = isTabKey ? focusCellIndex + 1 : focusCellIndex - 1;
                    }
                    ClearCurrent();
                    SetCurrent(editedRow, editedRow?.Cells[(int)focusCellIndex]!);
                }
            }
            bool hasCurrentRows = _currentRows != null && _currentRows.Count > 0 && _currentRows.FirstOrDefault()?.Index != null;
            if (string.IsNullOrEmpty(keyCombination) && _parent.SelectedRowIndexes?.Count > 0)
            {
                var rowIndexForTree = (int)_parent.SelectedRowIndexes[0];
                if (hasCurrentRows && _currentRows?[0].Index != 0 && rowIndexForTree >= _currentRows?[0].Index)
                {
                    rowIndexForTree -= (int)_currentRows[0].Index!;
                }
                var isTHeader = _isHeader;
                BeforeCellFocus tbcf = new BeforeCellFocus()
                {
                    KeyCombination = keyCombination!, 
                    Action = null!,
                    Cancel = false,
                    Cell = hasCurrentRows ? _currentRows?[rowIndexForTree]?.Cells[0]! : null!,
                    Row = hasCurrentRows ? _currentRows?[rowIndexForTree]! : null!,
                    KeyArgs = e,
                    IsKeyEvent = e != null,
                    IsHeader = isTHeader
                };
                _parent.EventAggregator.Trigger("HandleNullKey", tbcf);
            }
            if (string.IsNullOrEmpty(keyCombination) || IsChildFocused)
            {
                return;
            }

            DataReadyArgs<T> eventArgs = new DataReadyArgs<T>() { Count = _parent.TotalItemCount };
            _parent.EventAggregator.Trigger("GetPageCount", eventArgs);
            _parent.TotalItemCount = eventArgs.Count;
            int tPage = 0;
            if (_parent.PageModule != null)
            {
                tPage = _parent.PageModule.CalculateTotalPages();
            }

            //IsRendered from TreeGrid to prevent insert key from add new record on cell edit.
            if (_parent.IsRenderedFromTreeGrid && _parent.EditSettings?.Mode == EditMode.Batch && _parent.IsEdit && keyCombination == "Insert")
            {
                _parent.EventAggregator.Trigger("OnKeyDown", e!);
            }

            await ProcessKeyCombination(keyCombination, tPage, bf, isPagerFocused, e).ConfigureAwait(true);

            // Legacy AltW inline handler removed: MoveFocusToFirstRow is now resolved
            // through the standard action switch in ProcessKeyCombination. The default
            // chord AltW is registered in GridKeyboardActionDefaults._dataCellDefaults.
            if (((_parent.GridEvents?.RowDataBound.HasDelegate == true) || _parent.IsRenderedFromTreeGrid) && !_parent.IsEdit && _parent.EditSettings!.Mode.Equals(EditMode.Batch) && _parent.Rows!.Any(_ => _.Cells.Any(_ => _.IsDirty)) && (_parent.FocusModule != null && _parent.FocusModule.SelectedRowIndex.HasValue))
            {
                var selectedRow = _currentRows?.Find(_ => _.Index == _parent.FocusModule.SelectedRowIndex);
                var selectedCell = _parent.FocusModule.SelectedCellIndex.HasValue ? selectedRow?.Cells[(int)_parent.FocusModule.SelectedCellIndex] : selectedRow?.Cells[0];

                await ProcessKeyDown(e!, selectedRow!, selectedCell!).ConfigureAwait(true);
            }
        }

        // The below method returns the next editable cell index based on Tab or ShiftTab key.
        /// <summary>
        /// Gets the next editable cell index based on Tab or ShiftTab key.
        /// </summary>
        /// <param name="isTabKey">Specifies whether tab key is invoked or not.</param>
        /// <param name="currentCellIndex">Specifies the current editable cell index, where the key is invoked.</param>
        /// <param name="editedRow">Specifies the edited row object.</param>
        /// <returns><see cref="System.Threading.Tasks.Task{Integer}"/>.</returns>
        /// <exclude/>
        private static int GetNextCellIndex(bool isTabKey, int currentCellIndex, Row<object> editedRow)
        {
            if (isTabKey)
            {
                for (int i = currentCellIndex + 1; i <= editedRow.Cells.Count; i++)
                {
                    var cell = editedRow.Cells[i];
                    if (cell.Column != null && cell.Column.AllowEditing && cell.Visible)
                    {
                        return i;
                    }
                }
            }
            else
            {
                for(int i = currentCellIndex - 1; i >= 0; i--)
                {
                    var cell = editedRow.Cells[i];
                    if (cell.Column != null && cell.Column.AllowEditing && cell.Visible && cell.CellType != CellType.RowDrag)
                    {
                        return i;
                    }
                }
            }
            return currentCellIndex;
        }

        #endregion

        #region Key Combination Processing

        private async Task ProcessKeyCombination(string keyCombination, int tPage, BeforeCellFocus? bf = null, bool isPagerFocused = true, KeyboardEventArgs? e = null)
        {

            // Resolve the keystroke to a GridKeyboardAction using the existing
            // resolver. The resolver handles BOTH user-bound custom bindings AND
            // default chords via GridKeyboardActionDefaults, so the action id
            // drives the same switch the legacy keyCombination switch used to
            // drive. The resolver call is fully inlined here so the existing
            // switch remains the single source of truth for what each action does.
            //
            // Zone detection for the JS path:
            //   - When _isHeader is true we always take the Header zone, regardless of whether
            //     KeyInvokedRow/KeyInvokedCell are populated. This matches the legacy JS
            //     handler: keys like Tab/Enter on a header cell are header-zone keys regardless
            //     of whether the Blazor-side cell focus has been re-evaluated.
            //   - When _isHeader is false but we have a populated KeyInvokedRow/KeyInvokedCell,
            //     use DetectZone as before (covers DataCell/EditCell/GroupRow/DetailRow).
            //   - When _isHeader is false and there is no KeyInvoked signal at all, default to
            //     DataCell for legacy parity (the previous code did the same).
            //   This fixes the prior bug where the dispatcher silently routed header-zone
            //     custom bindings (e.g. SortColumn, ReorderColumn) to DataCell when the key
            //     arrived from the JS path before any cell-focus had been triggered.
            GridKeyboardAction? action = null;
            if (_parent != null)
            {
                var detectedZone = KeyInvokedRow != null && KeyInvokedCell != null
                    ? DetectZone(KeyInvokedRow, KeyInvokedCell)
                    : GridKeyboardZone.DataCell;
                var zone = ResolveKeyboardZone(_parent.IsEdit, _isHeader, detectedZone);
                // When no reliable zone signal is available (e.g. JS-routed key arrived
                // before the Blazor-side row/cell state was populated) and the legacy
                // token is one of the header-default tokens, fall back to the Header
                // zone so custom bindings for SortColumn/GroupColumn/ReorderColumn/
                // ToggleHeaderCheckbox still resolve from JS-driven keys.
                if (zone == GridKeyboardZone.DataCell && GridKeyDispatchTable.IsHeaderOnlyLegacyToken(keyCombination))
                {
                    zone = GridKeyboardZone.Header;
                }
                var resolve = GridKeyResolver.ResolveForKeystroke(
                    _parent.CompiledKeyboardShortcuts,
                    e != null ? e.GetNormalizedKey(_parent.IsMacDevice ?? false) : default,
                    keyCombination,
                    zone);
                action = resolve.Action;
                // True ONLY when the keystroke matched a user-bound custom chord
                // (resolver Layer 1/2). Layer 3 (suppression), Layer 4 (built-in
                // default like plain F2) and Layer 5 (no match) all return
                // PreventDefault=false, so this guard excludes them. This is what
                // makes the if/else wrappers below safe for default-key users: a
                // plain F2 keystroke keeps the legacy "no modifiers" guard intact.
                _isCustomKeyboardBinding = resolve.Consumed && resolve.PreventDefault && action != null;
            }

            if (keyCombination == "Escape")
            {
                if (_parent != null && _parent.ShowColumnChooser && _parent.ChooserDialogInstance != null)
                {
                    await _parent.ChooserDialogInstance.HideAsync().ConfigureAwait(true);
                }
            }

            switch (action)
            {
                case GridKeyboardAction.AddRecord:
                    if ((_parent != null && _parent.EditModule!.AlertMessage is "EditAlert" or "DeleteAlert") || (_parent != null && _parent.IsEdit && _parent.EditSettings?.Mode == EditMode.Batch))
                    {
                        return;
                    }
                    await _parent!.AddRecordAsync().ConfigureAwait(true);
                    break;
                case GridKeyboardAction.StartEdit:
                    if (_isCustomKeyboardBinding || (e != null && !e.AltKey && !e.CtrlKey && !e.ShiftKey))
                    {
                        await StartEditFromKeyboardAsync(KeyInvokedRow, KeyInvokedCell).ConfigureAwait(true);
                    }
                    break;
                case GridKeyboardAction.DeleteRecord:
                    await _parent!.DeleteRecordAsync().ConfigureAwait(true);
                    break;
                case GridKeyboardAction.Escape:
                    // Close all pop-ups here.
                    if (_parent != null && _parent.IsEdit || (_parent != null && _parent.EditSettings != null && _parent.EditSettings.ShowAddNewRow && !_parent.IsEdit && _parent.IsAdd))
                    {
                        ClearCurrent();
                        IsByKey = true;
                        await _parent.EditModule!.CloseEdit(true).ConfigureAwait(true);
                        IsByKey = false;
                    }

                    break;
                case GridKeyboardAction.PageUp:
                case GridKeyboardAction.PageDown:
                case GridKeyboardAction.FirstPage:
                case GridKeyboardAction.LastPage:
                case GridKeyboardAction.NextPageGroup:
                case GridKeyboardAction.PreviousPageGroup:
                    // Custom-binding branch: when the user has bound a non-default chord
                    // to a paging action (e.g. Shift+Ctrl+Alt+PageUp → FirstPage) and the
                    // resolver consumed it, honor the binding by bypassing the legacy
                    // !e.ShiftKey guard. The isPagerFocused and !IsEdit guards remain
                    // because they are state guards, not modifier guards.
                    if (!isPagerFocused && _parent != null && !_parent.IsEdit && (_isCustomKeyboardBinding || (e != null && !e.ShiftKey)))
                    {
                        // Map the action id back to the legacy token the existing
                        // ProcessPageKeyCombinations switch consumes. Done inline
                        // here so we don't introduce a wrapper method.
                        var pagingToken = action switch
                        {
                            GridKeyboardAction.PageUp            => "PageUp",
                            GridKeyboardAction.PageDown          => "PageDown",
                            GridKeyboardAction.FirstPage         => "CtrlAltPageUp",
                            GridKeyboardAction.LastPage          => "CtrlAltPageDown",
                            GridKeyboardAction.NextPageGroup     => "AltPageDown",
                            GridKeyboardAction.PreviousPageGroup => "AltPageUp",
                            _                                    => keyCombination,
                        };
                        await ProcessPageKeyCombinations(pagingToken, tPage, bf).ConfigureAwait(true);
                    }

                    if (_parent != null && _parent.ShowTooltip && _parent.TooltipInstance != null)
                    {
                        await (_parent.TooltipInstance?.CloseAsync())!.ConfigureAwait(true);
                    }
                    break;
                case GridKeyboardAction.Print:
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    await _parent!.PrintAsync().ConfigureAwait(true);
                    break;

                // ---- Phase 15: document-level focus jump ----
                case GridKeyboardAction.MoveFocusToFirstRow:
                    // AltW (default) arrives here from documentKeyHandler. The
                    // resolver routed it via _dataCellDefaults (the most general
                    // scope). Move focus to the first cell of the first content row.
                    if (_parent != null && _parent.Rows != null && _parent.Rows.Count > 0 && string.IsNullOrEmpty(_parent.EditModule?.AlertMessage))
                    {
                        var firstRow = _parent.Rows.Where(_ => _.Visible).FirstOrDefault();
                        var firstCell = firstRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).FirstOrDefault();
                        if (firstRow != null && firstCell != null)
                        {
                            await ProcessKeyDown(e!, firstRow!, firstCell!).ConfigureAwait(true);
                        }
                    }
                    break;

                // ---- Phase 11: clipboard (Common scope) ----
                // Document-level backstop. Ctrl+C / Ctrl+V normally consumed by the JS
                // gate; this branch handles custom-bound chords (e.g. F8 for Paste).
                case GridKeyboardAction.Copy:
                    if (_parent != null && (!_parent.IsEdit))
                    {
                        await _parent.CopyAsync(null).ConfigureAwait(true);
                        if (bf != null) { bf.Cancel = true; }
                        return;
                    }
                    break;
                case GridKeyboardAction.CopyWithHeaders:
                    if (_parent != null && (!_parent.IsEdit))
                    {
                        await _parent.CopyAsync(true).ConfigureAwait(true);
                        if (bf != null) { bf.Cancel = true; }
                        return;
                    }
                    break;
                case GridKeyboardAction.Paste:
                    if (_parent != null && !(_parent.IsEdit))
                    {
                        await _parent!.PasteAsync().ConfigureAwait(true);
                        if (bf != null) { bf.Cancel = true; }
                        return;
                    }
                    break;

                // ---- Phase 12: undo/redo (four-gate alignment) ----
                // The resolver fires these cases for BOTH the default chord (Ctrl+Z /
                // Ctrl+Y / Ctrl+Shift+Z) and any user-bound custom chord. The legacy
                // inline `if (keyCombination?.Equals("CtrlZ", ...))` block at the top
                // of this method was removed — action dispatch is the only path now.
                case GridKeyboardAction.Undo:
                    if (_parent?.EditSettings?.EnableUndoRedo == true
                        && _parent.EditSettings?.Mode == EditMode.Batch
                        && _parent.UndoRedoManager != null)
                    {
                        if (_parent.IsEdit && _parent.EditModule != null)
                        {
                            await _parent.EditModule.CloseEdit(true).ConfigureAwait(true);
                        }
                        await _parent.UndoRedoManager.PerformUndoAsync().ConfigureAwait(true);
                        await _parent.CallStateHasChangedAsync().ConfigureAwait(true);
                        _parent.EventAggregator?.Trigger("UndoRedoStackChanged", null!);
                        if (bf != null) { bf.Cancel = true; }
                        return;
                    }
                    break;
                case GridKeyboardAction.Redo:
                    if (_parent?.EditSettings?.EnableUndoRedo == true
                        && _parent.EditSettings?.Mode == EditMode.Batch
                        && _parent.UndoRedoManager != null)
                    {
                        if (_parent.IsEdit && _parent.EditModule != null)
                        {
                            await _parent.EditModule.CloseEdit(true).ConfigureAwait(true);
                        }
                        await _parent.UndoRedoManager.PerformRedoAsync().ConfigureAwait(true);
                        await _parent.CallStateHasChangedAsync().ConfigureAwait(true);
                        _parent.EventAggregator?.Trigger("UndoRedoStackChanged", null!);
                        if (bf != null) { bf.Cancel = true; }
                        return;
                    }
                    break;

                // ---- Phase 16: selection extensions ----
                // Shift+Up/Down/Left/Right is already routed through Selection.cs
                // (ClearRedundantCellsInBoxMode → SelectRangeOfCells). Cases below
                // mark the gesture as consumed when reached via the resolver.
                case GridKeyboardAction.SelectRowRange:
                    if (bf != null) { bf.Cancel = true; }
                    return;
                case GridKeyboardAction.ExtendSelection:
                    if (bf != null) { bf.Cancel = true; }
                    return;

                // ---- Phase 17: edit-end tab transitions ----
                // In Cell edit mode Tab / Shift+Tab already ends edit and moves
                // focus via HandleTabNavigationInCellMode (called from FocusCell).
                // These cases are the C# backstop for any custom-bound chord.
                case GridKeyboardAction.EditEndAndTabNext:
                case GridKeyboardAction.EditEndAndShiftTabPrev:
                    var editTarget = GetActiveEditRowAndCell();
                    if (editTarget.Cell?.IsEdit == true && _parent != null && _parent.EditModule != null)
                    {
                        await HandleEditEndNavigation(
                            editTarget.Row,
                            editTarget.Cell,
                            moveNext: action == GridKeyboardAction.EditEndAndTabNext).ConfigureAwait(true);
                        if (bf != null) { bf.Cancel = true; }
                        return;
                    }
                    break;
                case GridKeyboardAction.FocusFilterBar:
                    if (_isCustomKeyboardBinding && _parent != null
                        && _parent.AllowFiltering
                        && _parent.FilterSettings?.Type == FilterType.FilterBar
                        && KeyInvokedCell?.Column?.AllowFiltering != false)
                    {
                        if (bf != null) { bf.Cancel = true; }
                        bool isFilterTemplate = KeyInvokedCell?.Column?.FilterTemplate != null;
                        await _parent.InvokeMethod(
                            "sfBlazor.Grid.focusFilterBar",
                            new object[] { _parent.DataId, keyCombination, isFilterTemplate, -1 }).ConfigureAwait(true);
                        return;
                    }
                    break;

                case null:
                default:
                    // No matching action (either no CompiledKeyboardShortcuts loaded
                    // or the keystroke did not match any default). Silent no-op,
                    // matching the pre-Custom-Key behavior for unrecognized keys.
                    break;
            }
        }

        #endregion

        #region Page Key Combination Processing

        private async Task ProcessPageKeyCombinations(string keyCombination, int tPage, BeforeCellFocus? bf = null)
        {
            switch (keyCombination)
            {
                case "PageUp":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        int pNo = _parent.PageSettings!.CurrentPage - 1;
                        if (pNo > 0)
                        {
                            await _parent.GoToPageAsync(pNo).ConfigureAwait(true);
                        }
                    }

                    break;
                case "PageDown":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        int pNo = _parent.PageSettings!.CurrentPage + 1;
                        if (pNo <= tPage)
                        {
                            await _parent.GoToPageAsync(pNo).ConfigureAwait(true);
                        }
                    }
                    break;
                case "CtrlAltPageDown":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        await _parent.GoToPageAsync(tPage).ConfigureAwait(true);
                    }

                    break;
                case "CtrlAltPageUp":
                    if (bf != null)
                    {
                        bf.Cancel = true;
                    }

                    if (_parent.Rows?.Count > 0)
                    {
                        await _parent.GoToPageAsync(1).ConfigureAwait(true);
                    }
                    break;
                case "AltPageUp":
                    if (_parent.Rows?.Count > 0)
                    {
                        if (_parent.PageModule != null)
                        {
                            await _parent.PageModule.EllipsisButtonClickHandler("PreviousPage").ConfigureAwait(true);
                        }
                    }

                    break;
                case "AltPageDown":
                    if (_parent.Rows?.Count > 0 && _parent.PagerRef != null)
                    {
                        if (_parent.PageModule != null)
                        {
                            await _parent.PageModule.EllipsisButtonClickHandler("NextPage").ConfigureAwait(true);
                        }
                    }
                    break;
            }
        }

        #endregion

        #region Grid Focus Processing

        //Need to check use case of this method
        internal void ProcessGridFocus(FocusEventArgs e)
        {
            List<Row<object>>? _visibleParentRows = _parent.Rows?.Where(_ => _.Visible).ToList();
            List<Cell<object>>? _frozenCells = null;
            for (int i = 0; i < _visibleParentRows?.Count; i++)
            {
                var totalCells = _visibleParentRows[i].Cells.Count;
                for (int j = 0; j < totalCells; j++)
                {
                    if (_visibleParentRows[i]?.Cells[j]?.IsFrozen == true)
                    {
                        _frozenCells = _visibleParentRows[i]?.Cells?.Where(_ => _.IsFrozen).ToList();
                    }
                }
            }
            if (string.IsNullOrEmpty(_current.Item1) && (_parent.HeaderRows?.Count > 0 || _parent.Rows?.Count > 0))
            {
                if (_frozenCells != null && _frozenCells.Count != 0)
                {
                    _frozenCells[0].IsFocused = true;
                }
                IsChildFocused = false;
                _parent.SoftRefresh = true;
            }
        }

        #endregion

        #region Primary Focus Management

        internal async Task Focus(string rowUid, string cellUid, string? action = null, string? keyCombination = null, string? headerUid = null, int cellColIndex = -1, bool isSelectionMethodInvoked = false,
            bool isLastBatchEditCell = false)
        {
            if (_parent.IsRendered)
            {
                if (!string.IsNullOrEmpty(action) && action.Equals("SaveCell", StringComparison.Ordinal))
                {
                    await _parent.InvokeMethod("sfBlazor.Grid.gridFocus", new object[] { _parent.DataId, false, isLastBatchEditCell }).ConfigureAwait(true);
                }
                await _parent.InvokeMethod("sfBlazor.Grid.focus", new object[] { _parent.DataId, rowUid, cellUid, action!, keyCombination!, headerUid!, cellColIndex, isSelectionMethodInvoked, isLastBatchEditCell }).ConfigureAwait(true);
            }
        }

        private async Task<bool> FocusFirstChild(string rowUid, string cellUid, int cellColIndex = -1)
        {
            if (_parent.IsRendered)
            {
                bool val = await _parent.InvokeMethod<bool>("sfBlazor.Grid.focusChild", false, new object[] { _parent.DataId, rowUid, cellUid, cellColIndex }).ConfigureAwait(true);
                return val;
            }
            else
            {
                return false;
            }
        }

        /// Checks if the pressed key is the F2 key without any modifier keys (Shift, Ctrl, Alt).
        private static bool IsF2KeyPressed(KeyboardEventArgs e)
        {
            return e?.Key == "F2" && !(e.ShiftKey || e.CtrlKey || e.AltKey);
        }

        #endregion

        #region Cell Focus and Navigation

        private async Task FocusCell(Row<object> row, Cell<object> cell,
    string keyCombination, string[] actions, KeyboardEventArgs e,
    ValueTuple<int, int> navigator, GridKeyboardAction? resolvedAction = null)
        {
            var isheader = _isHeader;
            ValueTuple<Row<object>, Cell<object>> tuple = (null!, null!);
            string? action = null;
            if (actions?.Length != 0)
            {
                action = actions?[0];
                if (_parent.DetailRowModule != null && await _parent.DetailRowModule.HandleDetailTemplateFocusNavigationAsync(row, cell, e, _previouslyFocusedCell).ConfigureAwait(true))
                {
                    ClearCurrent();
                    return;
                }
                else
                {
                    bool isEnterKeyFromDetailCell = keyCombination.Equals("Enter", StringComparison.Ordinal) && cell?.CellType == CellType.Detail;
                    tuple = isEnterKeyFromDetailCell ? (row!, cell!) : GetNextRowCell(row!, cell!, e, navigator, action!);
                    if (_isCustomKeyboardBinding
                        && row?.IsDataRow == true
                        && cell != null
                        && (resolvedAction == GridKeyboardAction.NavigateRight || resolvedAction == GridKeyboardAction.NavigateLeft))
                    {
                        var navigableCells = row.Cells
                            .Where(currentCell => currentCell.Visible
                                && currentCell.CellType != CellType.RowDrag
                                && currentCell.CellType != CellType.Indent
                                && currentCell.CellType != CellType.DetailIndent)
                            .ToList();
                        bool isHorizontalBoundary = navigableCells.Count > 0
                            && (resolvedAction == GridKeyboardAction.NavigateRight
                                ? navigableCells[^1].Uid == cell.Uid
                                : navigableCells[0].Uid == cell.Uid);

                        if (isHorizontalBoundary)
                        {
                            var adjacentRowCell = GetAdjacentDataRowCell(
                                row,
                                moveNext: resolvedAction == GridKeyboardAction.NavigateRight);
                            if (adjacentRowCell.Row != null && adjacentRowCell.Cell != null)
                            {
                                tuple = (adjacentRowCell.Row, adjacentRowCell.Cell);
                            }
                        }
                    }
                }

                if (_parent.AllowPaging && tuple.Item1 == null && tuple.Item2 == null)
                {
                    if (_parent.EditSettings != null && _parent.EditSettings.Mode == EditMode.Batch && _parent.EditModule!.IsLastRow && keyCombination == "Enter")
                    {
                        await _parent.EditModule.SaveCell().ConfigureAwait(true);
                    }
                    ClearCurrent();
                    return;
                }
            }
            else
            {
                tuple = (row, cell);
            }
            _isHeader = IsHeaderRow(tuple.Item1);
            object cellValue = _parent.PropHelper?.GetObject(tuple.Item2?.Column?.Field!, tuple.Item1?.IsDirty == true ? tuple.Item1?.EditedData! : tuple.Item1?.Data!)!;
            if (e.Key == "Tab" || e.Key == "ShiftTab" || e.Key == "ArrowRight" || e.Key == "ArrowLeft")
            {
                AriaLabel = cellValue + " " + _parent.Localizer!.GetText(GridLocaleKeys.ColumnHeaderARIA) + " " + tuple.Item2?.Column?.Field;
            }
            if (e.Key == "ArrowDown" || e.Key == "ArrowUp")
            {
                IsKeyPressedUpOrDown = true;
                AriaLabel = cellValue;
            }

            BeforeCellFocus bcf = new BeforeCellFocus()
            {
                KeyCombination = keyCombination,
                Action = resolvedAction?.ToString() ?? action!,
                Cancel = false,
                Cell = cell!,
                Row = row!,
                KeyArgs = e,
                IsKeyEvent = e != null,
                IsHeader = isheader,
                Tuple = tuple,
                IsCustomBinding = _isCustomKeyboardBinding
            };

            _parent.EventAggregator.Trigger("BeforeCellFocus", bcf);
            bool isBatchEditArrowNavigation = _parent.EditSettings?.Mode == EditMode.Batch
                && cell?.IsEdit == true
                && (keyCombination is "ArrowUp" or "ArrowDown" or "ArrowLeft" or "ArrowRight"
                    or "ShiftUp" or "ShiftDown" or "ShiftLeft" or "ShiftRight");
            if (isBatchEditArrowNavigation
                || (_isCustomKeyboardBinding && resolvedAction == GridKeyboardAction.StartEdit
                    && _parent.EditSettings != null && _parent.EditSettings.Mode.Equals(EditMode.Batch) && !cell?.EditDisabled == true))
            {
                return;
            }
            if (bcf.Cancel == true || (resolvedAction == GridKeyboardAction.StartEdit
                && _parent.EditSettings != null && _parent.EditSettings.Mode.Equals(EditMode.Batch) && IsF2KeyPressed(e!) && !cell?.EditDisabled == true))
            {
                return;
            }
            if (_parent.IsRenderedFromTreeGrid)
            {
                tuple = bcf.Tuple;
            }
            if (tuple.Item2?.CellType != CellType.RowDrag)
            {
                bool isSameRow;
                if (keyCombination.Equals("AltW", StringComparison.Ordinal))
                {
                    ClearCurrent();
                    var firstRow = _parent.Rows?.Where(_ => _.Visible).First();
                    var firstCell = firstRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).First();
                    tuple = (firstRow, firstCell)!;
                    isSameRow = _current.Item1 == null ? false : _current.Item1.Equals(tuple.Item1?.Uid, StringComparison.Ordinal);
                    SetCurrent(tuple.Item1!, tuple.Item2!, true);
                }
                else
                {
                    isSameRow = _current.Item1 == null ? false : _current.Item1.Equals(tuple.Item1?.Uid, StringComparison.Ordinal);
                    ClearCurrent();
                    SetCurrent(tuple.Item1!, tuple.Item2!, true);
                }

                CellFocused cf = new CellFocused()
                {
                    KeyCombination = keyCombination,
                    Action = action!,
                    Cancel = false,
                    Cell = tuple.Item2!,
                    Row = tuple.Item1!,
                    KeyArgs = e!,
                    IsKeyEvent = e != null,
                    IsRowChanged = !isSameRow,
                    IsHeader = _isHeader,
                    IsJump = _isJump
                };
                if (!keyCombination.Equals("F2", StringComparison.Ordinal))
                {
                    _parent.EventAggregator.Trigger("CellFocused", cf);
                }
                _isJump = false;
                if (cf.Cancel == true)
                {
                    return;
                }

                if ((action != null && (action == "MoveUpCell") || (action == "MoveDownCell") || (action == "MoveRightCell") || (action == "MoveLeftCell")) || keyCombination == "CtrlHome" || keyCombination == "CtrlEnd" || keyCombination == "AltW")
                {
                    _parent.VirtualScrollModule!.CurrentRowIndex = _parent.InfiniteScrollModule!.CurrentRowIndex = tuple.Item1?.Index != null ? (int)tuple.Item1.Index : 0;
                }

                _parent.SoftRefresh = true;
                _parent.EventAggregator.Trigger("RowStateChanged", cf.Row);
                if (!cf.PreventDOMFocus || (cf.PreventDOMFocus && !(cf.Cell?.Column?.AllowEditing == true && cf.Cell?.Column?.AllowAdding == true)))
                {
                    bool isCtrlUpOrDown = keyCombination.Equals("CtrlUp", StringComparison.Ordinal) || keyCombination.Equals("CtrlDown", StringComparison.Ordinal);

                    if (_parent.AllowGrouping && tuple.Item1?.GroupKey != null && tuple.Item1?.Index == null && isCtrlUpOrDown)
                    {
                        bool isFocused = await _parent.InvokeMethod<bool>("sfBlazor.Grid.focusNextFrame", false, new object[] { _parent.DataId }).ConfigureAwait(true);
                    }
                    await Focus(tuple.Item1?.Uid!, tuple.Item2?.Uid!, action, keyCombination, cellColIndex: tuple.Item2?.Index + 1 ?? -1).ConfigureAwait(true);
                }
            }
        }

        internal async Task RestoreFocus(int EditRowIndex = -1)
        {
            var rowIndex = SelectedRowIndex;

            if (SelectedRowIndex != null && SelectedRowIndex >= EditRowIndex && _parent!.EditModule!.IsDeleteAction)
            {
                rowIndex = EditRowIndex;
                _parent!.EditModule!.IsDeleteAction = false;
            }
            var cellIndex = SelectedCellIndex;

            if (SelectedRowIndex == null || SelectedCellIndex == null || _parent?.EnableVirtualization == true || _parent?.EnableInfiniteScrolling == true)
            {
                return;
            }

            var row = _parent?.Rows?.Find(r => r.Index == rowIndex);

            if (row != null && row.Cells.Find(c => c.CellType == CellType.Detail) != null)
            {
                cellIndex = cellIndex + 1;
            }

            if (row == null || cellIndex == null || cellIndex < 0 || cellIndex >= row.Cells.Count)
            {
                return;
            }

            var nonIndentCells = row.Cells.Where(x => x.CellType != CellType.Indent && x.CellType != CellType.RowDrag && x.CellType != CellType.DetailIndent).ToList();

            var cell = nonIndentCells[(int)cellIndex];
            try
            {
                await Focus(row.Uid!, cell.Uid, cellColIndex: cell.Index + 1 ?? -1).ConfigureAwait(true);
            }
            catch (TaskCanceledException)
            {

            }
        }

        #endregion

        #region Row and Cell Navigation

        private (Row<object>? Row, Cell<object>? Cell) GetAdjacentDataRowCell(Row<object> row, bool moveNext)
        {
            var dataRows = _parent.Rows?
                .Where(currentRow => currentRow.Visible && currentRow.IsDataRow && !currentRow.IsDetailRow)
                .ToList();
            if (dataRows == null)
            {
                return (null, null);
            }

            int currentRowIndex = dataRows.FindIndex(currentRow => currentRow.Uid == row.Uid);
            int nextRowIndex = currentRowIndex + (moveNext ? 1 : -1);
            if (currentRowIndex < 0 || nextRowIndex < 0 || nextRowIndex >= dataRows.Count)
            {
                return (null, null);
            }

            Row<object> nextRow = dataRows[nextRowIndex];
            var cells = nextRow.Cells
                .Where(currentCell => currentCell.Visible
                    && currentCell.CellType != CellType.RowDrag
                    && currentCell.CellType != CellType.Indent
                    && currentCell.CellType != CellType.DetailIndent)
                .ToList();

            return (nextRow, moveNext ? cells.FirstOrDefault() : cells.LastOrDefault());
        }

        private ValueTuple<Row<object>, Cell<object>> GetNextRowCell(Row<object> row, Cell<object> cell,
    KeyboardEventArgs e, ValueTuple<int, int> navigator, string? action = null)
        {
            var keyCombination = e.GetKeyCombination(isMacDevice: _parent!.IsMacDevice ?? false, isEnableUndoRedo: _parent!.EditSettings!.EnableUndoRedo);
            bool isArrowKeys = keyCombination != null && (keyCombination.Equals("ArrowUp", StringComparison.Ordinal) || keyCombination.Equals("ArrowDown", StringComparison.Ordinal) || keyCombination.Equals("ArrowLeft", StringComparison.Ordinal) || keyCombination.Equals("ArrowRight", StringComparison.Ordinal));
            SetCurrentArea(row, cell, e, navigator, action);
            List<Row<object>>? _visiblerows = _currentRows?.Where(_ => _.Visible != false && !(_.CssClass != null && _.CssClass.Contains("e-hiddenrow", StringComparison.Ordinal))).ToList();
            // On Shift+Tab from a detail row, navigate to the last cell of its parent row.
            if (row?.IsDetailRow == true && action == "MoveLeftCell" && e.IsShiftTab() && _visiblerows?.Count > 0)
            {
                int detailRowIndex = _visiblerows?.IndexOf(row) ?? -1;
                if (detailRowIndex > 0)
                {
                    var parentRow = _visiblerows?[detailRowIndex - 1];
                    if (parentRow != null && !parentRow.IsDetailRow)
                    {
                        var lastDataCell = parentRow.Cells?.Where(x => x.Visible && (x.CellType == CellType.Data || x.CellType == CellType.CommandColumn))?.LastOrDefault();
                        if (lastDataCell != null)
                        {
                            return (parentRow!, lastDataCell!);
                        }
                    }
                }
            }
            List<Row<object>>? _visibleheaderrows = HeaderRows?.Where(_ => _.Visible != false).ToList();
            List<Row<object>>? _visibleFrozenheader = _frozenHeaderRows?.Where(_ => _.Visible != false).ToList();
            var _frozenHeaderCount = _visibleFrozenheader?.Count - 1;
            var currentfrozenIndex = 0;
            var _headerRowsCount = (_visibleheaderrows?.Count - 1) ?? 0;
            var currentHeaderIndex = 0;
            int newCIndex = 0;
            int newRIndex = 0;
            List<GridColumn> orderedColumns = _parent.FreezeModule!.GetFrozenCount() > 0 ? _parent.RearrangeColumns(_parent.Columns!) : _parent.Columns!;
            for (int i = 0; i <= _frozenHeaderCount; i++)
            {
                if (_visibleFrozenheader != null && _visibleFrozenheader[i]?.Cells?.Count == row?.Cells?.Count)
                {
                    currentfrozenIndex = i;
                }
            }

            for (int i = 0; i <= _headerRowsCount; i++)
            {
                if (_visibleheaderrows?[i]?.Cells?.Count == row?.Cells?.Count)
                {
                    currentHeaderIndex = i;
                }
            }

            //To focus the first group caption cell when up arrow key is pressed from the first caption cell while grouping all columns.
            bool allColumnsAreInvisible = _parent.Columns?.Count == _parent.Columns?.Where(e => e.Visible == false).Count();
            if (allColumnsAreInvisible && action == "MoveUpCell" && _parent.Rows?.Count > 0 && _parent.Rows.First().Equals(row))
            {
                return (row, cell);
            }

            // To focus the first content cell when tab key is pressed from the last header cell.
            bool columnVirtualLastCellFocus = false;
            if (_parent.EnableColumnVirtualization && orderedColumns?.Count > 0)
            {
                int selectedCellNavigation = _parent.VirtualScrollModule!.SelectedCellNavigation;
                if (row?.Cells?.Where(e => e.Visible).Last()?.IsFocused == true && !string.IsNullOrEmpty(cell?.Column!.Field))
                {
                    columnVirtualLastCellFocus = cell?.Column!.Field == orderedColumns.LastOrDefault()?.Field;
                }
                if (selectedCellNavigation > -1 && selectedCellNavigation == _parent.Columns?.Count - 1)
                {
                    columnVirtualLastCellFocus = orderedColumns[selectedCellNavigation] == orderedColumns.LastOrDefault();
                }
            }
            bool normalGridLastCellFocus = !_parent.EnableColumnVirtualization && row?.Cells?.Where(e => e.Visible).Last()?.IsFocused == true;
            if (row?.Index == null && _parent.Rows?.Count > 0 && !row?.IsCaptionRow == true && (normalGridLastCellFocus || columnVirtualLastCellFocus) && action?.Equals("MoveRightCell", StringComparison.Ordinal) == true && _visibleheaderrows?[_headerRowsCount]?.Cells?.Where(e => e.Visible).Last().IsFocused == true)
            {
                row = _parent.Rows?.FirstOrDefault(currentRow => currentRow.Visible && !currentRow.Action.Equals(EditAction.Deleted))!;
                cell = row?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag).First()!;
                return (row!, cell);
            }

            // To focus the last header cell when shift tab is pressed from the first content cell.
            Row<object>? firstFocusableRow = _visiblerows?.FirstOrDefault(currentRow => currentRow.Visible && !currentRow.Action.Equals(EditAction.Deleted));
            if (firstFocusableRow != null && !(_parent.AllowFiltering && _parent.FilterSettings != null && _parent.FilterSettings.Type.Equals(FilterType.FilterBar)) && firstFocusableRow.Equals(row) && row!.Cells?.Where(e => e.Visible && e.CellType != CellType.RowDrag).First().Equals(cell) == true && action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true && !e.Key.Equals("ArrowLeft", StringComparison.Ordinal) && e.GetKeyCombination() == "ShiftTab")
            {
                row = HeaderRows?.Last()?.Cells != null ? HeaderRows.Last() : row;
                cell = HeaderRows?.Last()?.Cells?.Where(e => e.Visible).Last()!;
                return (row, cell);
            }

            // Handling the row and cell index when shift tab is pressed, from the first cell of the caption row.
            if (_visiblerows != null && row?.Index == null && row?.IsCaptionRow == true && e.IsShiftTab() && row?.Cells?.Where(e => e.Visible).FirstOrDefault()?.Equals(cell) == true && !allColumnsAreInvisible)
            {
                var currentRowIndex = _visiblerows.IndexOf(row);
                if (!_visiblerows.FirstOrDefault()?.Equals(_visiblerows?[currentRowIndex]) == true)
                {
                    var newRow = _visiblerows?[currentRowIndex - 1];
                    var newCell = newRow?.Cells?.Where(e => e.Visible).LastOrDefault();
                    return (newRow!, newCell!);
                }
            }

            if (_visibleFrozenheader?.Count != 0 && (_visibleheaderrows != null && _visibleheaderrows[0]?.Cells?.Where(e => e.Visible).First().IsFocused == true) && action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true)
            {
                row = _visibleFrozenheader?.Last()?.Cells != null ? _visibleFrozenheader!.Last() : row!;
                cell = _visibleFrozenheader?.Last()?.Cells?.Where(e => e.Visible).Last()!;
                return (row, cell);
            }

            if (currentHeaderIndex > 0 && _visibleheaderrows?[currentHeaderIndex]?.Equals(row) == true && row?.Cells?.Where(e => e.Visible).First().IsFocused == true && action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true)
            {
                row = _visibleheaderrows?[currentHeaderIndex - 1]!;
                cell = _visibleheaderrows?[currentHeaderIndex - 1]?.Cells?.Where(e => e.Visible).Last()!;
                return (row, cell);
            }

            if (_visibleFrozenheader?.Count != 0 && _visibleFrozenheader?[currentfrozenIndex]?.Equals(row) == true && !row?.Cells?.Where(e => e.Visible).Last().IsFocused == true && action?.Equals("MoveRightCell", StringComparison.Ordinal) == true && _visibleFrozenheader[0]?.Cells?.Count != 1)
            {
                newCIndex = (row?.Cells?.IndexOf(cell!) ?? 0) + navigator.Item2;
                newRIndex = _visibleFrozenheader.IndexOf(row!) + navigator.Item1;
            }
            else if (_visibleFrozenheader?.Count != 0 && _visibleFrozenheader?[currentfrozenIndex]?.Equals(row) == true && (row?.Cells?.Where(e => e.Visible).Last().IsFocused == true || _visibleFrozenheader[0]?.Cells?.Count == 1) && action?.Equals("MoveRightCell", StringComparison.Ordinal) == true)
            {
                row = _visibleheaderrows?.FirstOrDefault()!;
                cell = _visibleheaderrows?[0]?.Cells?.Where(_ => _.Visible).First()!;
                return (row, cell);
            }
            else
            {
                if (_parent.EnableColumnVirtualization && row?.Index == null && _parent.Rows?.Count > 0 && _parent.Columns?.Count > 0 && cell != null && !isArrowKeys)
                {
                    List<GridColumn>? visibleColumns = orderedColumns?.Where(x => x.Visible).ToList();
                    newCIndex = visibleColumns!.IndexOf(cell.Column!) + navigator.Item2;
                }
                else
                {
                    newCIndex = (row?.Cells?.IndexOf(cell!) ?? 0) + navigator.Item2;
                }

                newRIndex = (_visiblerows?.IndexOf(row!) ?? 0) + navigator.Item1;
            }

            if (_visiblerows?.Count <= newRIndex)
            {
                if (_parent.AllowPaging)
                {
                    if(_parent.AllowGrouping && row?.IsCaptionRow == true && !row?.IsExpand == true || (_parent.IsRenderedFromTreeGrid && row != null && row.IsLastRow))
                    {
                        newRIndex = _visiblerows.Count - 1;
                    }
                    else
                    {
                        return (null!, null!);
                    }
                }
                else
                {
                    newRIndex = _visiblerows.Count - 1;
                }
            }

            if (newRIndex < 0)
            {
                if (action == "MoveUpCell" && _visiblerows?.Count - 1 > 0)
                {
                    newRIndex = _visiblerows?.Count - 1 ?? 0;
                }
                else
                {
                    newRIndex = 0;
                }
            }

            var nRow = _visiblerows?[newRIndex];
            var isRowDataCellVisible = nRow!.Cells.Where(x => x.IsDataCell).Select(e => e.Visible).LastOrDefault();
            if ((action == "MoveDownCell" || action == "MoveLeftCell" || action == "MoveUpCell") && _parent.GroupSettings != null && _parent.GroupSettings.Columns?.Length > 0 && !isRowDataCellVisible && !nRow.IsCaptionRow && !nRow.IsExpand && nRow.RowType == "Data" && allColumnsAreInvisible)
            {
                nRow = (action == "MoveUpCell" || _visiblerows?.Count - 1 <= newRIndex) ? _visiblerows?[newRIndex - 1] : _visiblerows?[newRIndex + 1];
            }
            if (_visibleFrozenheader?.Count != 0 && (_visibleFrozenheader != null && _visibleFrozenheader[currentfrozenIndex]?.Equals(row) == true) && row?.Cells?.Where(e => e.Visible).Last().IsFrozen == true && _visibleFrozenheader[0]?.Cells?[0]?.IsFocused != true && _visibleFrozenheader[0]?.Cells?.Count != 1)
            {
                nRow = _visibleFrozenheader[newRIndex];
            }
            int cellCount = (int)(_parent.EnableColumnVirtualization && nRow?.Index == null ? _parent.Columns?.Count : nRow?.Cells?.Count)!;

            if (newCIndex < 0)
            {
                newCIndex = 0;
            }

            if (cellCount <= newCIndex)
            {
                newCIndex = cellCount - 1 < 0 ? 0 : cellCount - 1;
            }

            if (_parent.EnableColumnVirtualization &&  nRow?.Index == null)
            {
                List<GridColumn>? visibleColumns = orderedColumns?.Where(x => x.Visible).ToList();
                newCIndex = !isArrowKeys ? nRow!.Cells.FindIndex(x => x.Column != null && !string.IsNullOrEmpty(x.Column.Field) && x.Column.Field == visibleColumns?[newCIndex].Field) : newCIndex;
                if(newCIndex < 0 && (action?.Equals("MoveLeftCell", StringComparison.Ordinal) == true 
                    || action?.Equals("MoveRightCell", StringComparison.Ordinal) == true))
                {
                    return (nRow!, nRow?.Cells?.LastOrDefault()!);
                }
            }
            var nCell = nRow?.Cells?[newCIndex];
            if (nCell?.Index == null && nCell?.CellType == CellType.RowDrag)
            {
                nCell = nRow?.Cells?.Where(_ => _.Visible && _.CellType != CellType.RowDrag && _.CellType != CellType.Indent).FirstOrDefault();
            }
            var oRow = nRow;
            var oCell = nCell;

            // If cell is invisible or indent cell, then find next visible cell.
            (nRow, nCell) = CheckInVisibleOrIndentCell(nCell!, oRow!, oCell!, nRow!, e, navigator, action);

            //If grid has single column Shift + Tab from last cell not working scenario
            var lastVisibleDataCell = _visiblerows != null && _visiblerows.Count > 0 ? _visiblerows[_visiblerows.Count - 1]?.Cells?.Where(x => x.Visible && x.IsDataCell).ToList() : null;
            bool singleColumnGrid = lastVisibleDataCell != null && lastVisibleDataCell.Count > 0 && nRow?.IsLastRow == true && _visiblerows?[_visiblerows.Count - 1]?.Cells?.Where(x => x.Visible)?.LastOrDefault()?.Equals(cell) == true && lastVisibleDataCell.FirstOrDefault()!.Equals(cell);

            // same cell is focused then go to next row
            // also swap the action as we need to search cell in opposite direction.
            if (!(action == "MoveDownCell" && _currentRows?.LastOrDefault()?.Equals(nRow) == true) && nRow?.Equals(row) == true && nCell?.Equals(cell) == true && (e.IsTab() || e.IsShiftTab())
                && !(_visiblerows?.Count > 0 && _visiblerows.Last().Equals(row) && _visiblerows[_visiblerows.Count - 1]?.Cells?.Where(x => x.Visible).Last().Equals(cell) == true) || (singleColumnGrid && e.IsShiftTab() && !(nRow?.Index == 0 && nCell?.Index == 0)))
            {
                (nRow, nCell) = GetNextRowCell(nRow!, nCell!, e,
                    navigator: (e.IsShiftTab() ? -1 : 1, e.IsShiftTab() ? cellCount : -cellCount),
                    action: e.IsShiftTab() ? "MoveRightCell" : "MoveLeftCell");
            }

            return (nRow!, nCell!);
        }

        #endregion

        #region Visible Cell and Spanned Cell Handling

        private ValueTuple<Row<object>, Cell<object>> CheckInVisibleOrIndentCell(Cell<object> nCell, Row<object> oRow, Cell<object> oCell,
     Row<object> nRow, KeyboardEventArgs e, ValueTuple<int, int> navigator, string? action = null)
        {
            if (nCell.Visible == false || nCell.IsSpanned == true || nCell.IsRowSpanned == true || (nCell.CellType.Equals(CellType.Indent) || nCell.CellType.Equals(CellType.DetailIndent)))
            {
                if (action == "MoveLeftCell" || action == "MoveRightCell")
                {
                    if (action == "MoveLeftCell" && (nCell.CellType.Equals(CellType.Indent) || nCell.CellType.Equals(CellType.DetailIndent)))
                    {
                        (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, 1), action);
                    }
                    else
                    {
                        if (action == "MoveLeftCell" && nCell.Index.HasValue && nCell.Index.Value == 0)
                        {
                            (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, 1), action);
                        }
                        else if (action == "MoveRightCell" && nCell.Index.HasValue && nRow.Cells.Last().Equals(nCell))
                        {
                            (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, -1), action);
                        }
                        else
                        {
                            (nRow, nCell) = GetNextRowCell(oRow, oCell, e, navigator, action);
                        }
                    }
                }

                if (action == "MoveUpCell" || action == "MoveDownCell")
                {
                    (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, 1), action);
                    if (nCell.Visible == false)
                    {
                        (nRow, nCell) = GetNextRowCell(oRow, oCell, e, (0, -1), action);
                    }
                }
            }
            return (nRow, nCell);
        }

        #endregion

        #region Focus Area Management

        private void SetCurrentArea(Row<object> row, Cell<object> cell,
   KeyboardEventArgs e, ValueTuple<int, int> navigator, string? action = null)
        {
            var eventArgs = e;
            var Navigator = navigator;
            var cellObject = cell;
            if (_isHeader)
            {
                if (action == "MoveDownCell")
                {
                    bool isLast = _currentRows?.LastOrDefault()?.Equals(row) == true;
                    if (isLast && _parent.Rows?.Count != 0)
                    {
                        _isHeader = false;
                        _isJump = true;
                    }
                }
            }
            else
            {
                if (action == "MoveUpCell")
                {
                    bool isFirst = _currentRows?.FirstOrDefault()?.Equals(row) == true;
                    if (isFirst)
                    {
                        _isHeader = true;
                        _isJump = true;
                    }
                }
            }
        }

        #endregion

        #region Child Control Focus Handling

        private async Task<bool> FocusChild(Row<object> row, Cell<object> cell, KeyboardEventArgs e)
        {
            string keyComb = e.GetKeyCombination();
            bool hasChild = false;
            if (!IsChildFocused && (keyComb == "Enter")
                && (cell.IsTemplate || cell.CellType.Equals(CellType.CommandColumn)) && !cell.IsEdit)
            {
                hasChild = await FocusFirstChild(row.Uid!, cell.Uid, cell.Index + 1 ?? -1).ConfigureAwait(true);
            }

            if (IsChildFocused && keyComb == "Escape")
            {
                hasChild = false;
            }

            return IsChildFocused = hasChild;
        }

        #endregion

        #region Row Selection Focus Management

        /// <summary>
        /// Handles focus management when a row is selected.
        /// Determines the cell to focus based on grouping settings and updates focus state.
        /// </summary>
        internal async Task HandleRowSelectionFocus(Row<object> rowToSelect, bool isScrollIntoView, bool isSelectionMethodInvoked, bool isCellClicked, bool isAdd, bool isCancelAction)
        {
            if (rowToSelect == null || SelectedCellIndex == null || rowToSelect.Cells == null)
                return;

            Cell<object>? cell = rowToSelect.Cells.FirstOrDefault(c => c.Index == (int)SelectedCellIndex);
            bool isGrouped = _parent.AllowGrouping && _parent.GroupSettings?.Columns != null && _parent.GroupSettings.Columns?.Length > 0 && !_parent.GroupSettings.ShowGroupedColumn && !cell!.Visible;
            var cellToSelect = isGrouped ? rowToSelect.Cells.FirstOrDefault(c => c.Visible && c.Index > (int)SelectedCellIndex) : null;
            string? cellUid = isGrouped ? cellToSelect?.Uid! : cell?.Uid;
            
            if (!(isCellClicked && isAdd))
            {
                ClearCurrent();
                var cellToFocus = rowToSelect.Cells?.FirstOrDefault(_ => _.Index == (int)SelectedCellIndex)!;
                SetCurrent(rowToSelect, cellToFocus, outline: true);
            }
            
            int colIndex = isGrouped ? (cellToSelect?.Index + 1) ?? -1 : (cell?.Index + 1) ?? -1;
            string? focusMode = isAdd ? "UpdateRecord" : isScrollIntoView ? "ScrollSelect" : null;
            
            if (!isCellClicked && !isCancelAction)
            {
                await Focus(rowToSelect.Uid!, cellUid!, focusMode, cellColIndex: colIndex, isSelectionMethodInvoked: isSelectionMethodInvoked).ConfigureAwait(true);
            }
        }

        #endregion


        #region Header Focus Management

        internal async Task ToIndexHasValue(int? tIndex, int frIndx, BeforeCellFocus evt)
        {
            if (tIndex.HasValue)
            {
                evt.Cancel = true;
                var fromColumns = new List<GridColumn> { _parent!.Columns?[frIndx]! } ;
                var toColumn = _parent.Columns?[(int)tIndex];
                var ar = new ActionEventArgs<T>()
                {
                    RequestType = Grids.Action.Reorder,
                    Cancel = false,
                    Parent = _parent,
                    FromColumns = fromColumns,
                    ToColumn = toColumn!
                };
                var reorderEventArgs = new ColumnReorderingEventArgs() {ReorderingColumns = fromColumns , ToColumn = toColumn!, Cancel = false, Parent = _parent };
                await _parent.ModelChanged(ar, new ActionArgs() { FromIndex = frIndx, ToIndex = (int)tIndex }, eventArgs: reorderEventArgs, requestType:"Reorder").ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<ActionEventArgs<T>>(_parent.GridEvents?.OnActionComplete!, ar).ConfigureAwait(true);
                await _parent.EventAggregator.NotifyAsync("ActionComplete", ar).ConfigureAwait(true);
                await SfBaseUtils.InvokeEvent<ColumnReorderedEventArgs>(_parent.GridEvents?.ColumnReordered!, reorderEventArgs).ConfigureAwait(true);
                await _parent.EventAggregator.NotifyAsync("ColumnReordered", reorderEventArgs).ConfigureAwait(true);
                await _parent.InvokeSuccessAsync(ar, requestType: "Reorder").ConfigureAwait(true);
            }
        }

        internal async Task ShiftEnterHandler(GridColumn _col, BeforeCellFocus evt)
        {
            if (_parent!.AllowSorting)
            {
                evt.Cancel = true;
                string sortIcon = _parent.SortModule?.GetSortIconClass(_col.Field!) ?? string.Empty;
                await (_parent?.SortModule?.SortClickHandler(
                    _col,
                    sortIcon,
                    new MouseEventArgs() { CtrlKey = evt.KeyArgs!.CtrlKey, ShiftKey = evt.KeyArgs.ShiftKey, MetaKey = evt.KeyArgs.MetaKey }
                ))!.ConfigureAwait(true)!;
            }
        }

        internal async Task CtrlSpaceHandler(BeforeCellFocus evt, GridColumn _col)
        {
            if (_parent != null && _parent.AllowGrouping)
            {
                evt.Cancel = true;
                string[] _groups = _parent.GroupSettings?.Columns ?? Array.Empty<string>();
                if (!string.IsNullOrEmpty(_col?.Field) && !_col.FixedColumn)
                {
                    if (_groups.Contains(_col.Field))
                    {
                        await _parent.UngroupColumnAsync(_col.Field).ConfigureAwait(true);
                    }
                    else
                    {
                        await _parent.GroupColumnAsync(_col.Field).ConfigureAwait(true);
                    }
                }
            }
        }

        /// <summary>
        /// Handles Ctrl+Arrow Up/Down keyboard navigation to first/last row in virtualized grids.
        /// Preserves the current column index and dispatches through ScrollIntoViewAsync.
        /// </summary>
        private async Task HandleCtrlArrowFirstLastRowNavigation(string keyCombination, KeyboardEventArgs e)
        {
            if (_parent?.VirtualScrollModule == null || _parent.TotalItemCount <= 0)
            {
                return;
            }

            int targetRowIndex = keyCombination.Equals("CtrlDown", StringComparison.Ordinal)
                ? (IsDomVirtualNavigationSupported() ? _parent.DomVirtualModule!.TotalRowCount : _parent.TotalItemCount) - 1
                : 0;

            if (IsDomVirtualNavigationSupported())
            {
                await TryFocusDomRowAsync(targetRowIndex, null, e, keyCombination).ConfigureAwait(true);
                return;
            }

            int columnIndex = _parent.FocusModule?.SelectedCellIndex ?? -1;

            // Preserve PreNavigatedIndex for the post-render focus pipeline
            _parent.VirtualScrollModule.PreNavigatedIndex = _parent.VirtualScrollModule.CurrentRowIndex;
            _parent.VirtualScrollModule.CurrentRowIndex = targetRowIndex;

            // Dispatch through the existing ScrollIntoViewAsync API, which integrates
            // with the VirtualRefresh → VirtualComponentUpdate → HandleVirtualScrollKeyDown
            // event-driven pipeline. This ensures the DOM is updated and focus is applied.
            await _parent.ScrollIntoViewAsync(columnIndex, targetRowIndex, -1).ConfigureAwait(true);
        }

        #endregion

        #region DOM Virtualization Navigation

        /// <summary>
        /// Row-index based navigation only applies to a flat list of data rows; grouped, detail-template and
        /// column-virtualized grids keep the regular behaviour for now.
        /// </summary>
        internal bool IsDomVirtualNavigationSupported()
        {
            return _parent.EnableDomVirtualization
                && _parent.DomVirtualModule?.Content != null
                && !_isHeader
                && !_parent.EnableColumnVirtualization
                && !(_parent.AllowGrouping && _parent.GroupSettings?.Columns?.Length > 0)
                && ((IGrid)_parent).GridTemplates?.DetailTemplate == null;
        }

        private Row<object>? FindWindowRow(int rowIndex)
        {
            return _parent.Rows?.FirstOrDefault(r => r.Index == rowIndex && r.IsDataRow && r.Visible);
        }

        private static Cell<object>? FindNavigableCell(Row<object> row, int? cellIndex)
        {
            List<Cell<object>> cells = row.Cells.Where(c => c.Visible && c.CellType != CellType.RowDrag
                && c.CellType != CellType.Indent && c.CellType != CellType.DetailIndent).ToList();
            if (cellIndex == null)
            {
                return cells.FirstOrDefault();
            }
            if (cellIndex == int.MaxValue)
            {
                return cells.LastOrDefault();
            }
            return cells.FirstOrDefault(c => c.Index == cellIndex) ?? cells.FirstOrDefault();
        }

        /// <summary>
        /// Moves focus straight to a row/column (Ctrl+Home/End, Ctrl+Up/Down, Alt+W). When the row is not rendered yet,
        /// the target is remembered and the window is scrolled/rendered; the focus is applied once the rows are in the DOM.
        /// <paramref name="cellIndex"/> null picks the first cell, int.MaxValue the last, otherwise the cell with that index.
        /// </summary>
        private async Task<bool> TryFocusDomRowAsync(int rowIndex, int? cellIndex, KeyboardEventArgs e, string keyCombination)
        {
            if (cellIndex == null && DomFocusCellIndex != null && !keyCombination.StartsWith("CtrlHome", StringComparison.Ordinal)
                && !keyCombination.Equals("AltW", StringComparison.Ordinal))
            {
                cellIndex = DomFocusCellIndex;
            }
            if (rowIndex < 0 || rowIndex >= _parent.DomVirtualModule!.TotalRowCount)
            {
                return false;
            }
            Row<object>? target = FindWindowRow(rowIndex);
            if (target == null)
            {
                _domPendingFocus = (rowIndex, cellIndex, e, keyCombination);
                if (!await _parent.DomVirtualModule!.EnsureRowRenderedAsync(rowIndex, DomFocusRowIndex ?? 0).ConfigureAwait(true))
                {
                    _domPendingFocus = null;
                    return false;
                }
                return true;
            }
            Cell<object>? targetCell = FindNavigableCell(target, cellIndex);
            if (targetCell == null)
            {
                return false;
            }
            await FocusCell(row: target, cell: targetCell, navigator: (0, 0), e: e, actions: Array.Empty<string>(), keyCombination: keyCombination).ConfigureAwait(true);
            return true;
        }

        /// <summary>
        /// Called after the DOM virtualization window rendered: focuses the row/cell a keyboard jump was waiting for.
        /// Returns true when the focus was applied.
        /// </summary>
        internal async Task<bool> ApplyPendingDomFocusAsync()
        {
            if (_domPendingFocus == null)
            {
                return false;
            }
            var pending = _domPendingFocus.Value;
            Row<object>? row = FindWindowRow(pending.RowIndex);
            Cell<object>? cell = row != null ? FindNavigableCell(row, pending.CellIndex) : null;
            if (row == null || cell == null)
            {
                return false;
            }
            _domPendingFocus = null;
            await FocusCell(row: row, cell: cell, navigator: (0, 0), e: pending.Args, actions: Array.Empty<string>(), keyCombination: pending.KeyCombination).ConfigureAwait(true);
            return true;
        }

        #endregion
    }

    #region Event Argument Classes

    /// <summary>
    /// BeforeCell focus event argument class.
    /// </summary>
    /// <exclude/>
    internal class BeforeCellFocus
    {
        public bool Cancel { get; set; }

        public KeyboardEventArgs? KeyArgs { get; set; }

        public string? KeyCombination { get; set; }

        public string? Action { get; set; }

        public Row<object>? Row { get; set; }

        public Cell<object>? Cell { get; set; }

        public bool IsKeyEvent { get; set; }

        public bool IsHeader { get; set; }

        public ValueTuple<Row<object>, Cell<object>> Tuple { get; set; }

        /// <summary>
        /// True when the keystroke that triggered this focus event was resolved from a
        /// user-bound custom chord (KeyboardShortcuts Layer 1/2/3) rather than a
        /// built-in default (Layer 4). Subscribers (e.g. Edit.CurrentFocus) use this
        /// to bypass legacy "no modifiers" guards for default chords like F2/Insert.
        /// </summary>
        public bool IsCustomBinding { get; set; }
    }

    /// <summary>
    /// Cell focus event argument class.
    /// </summary>
    /// <exclude/>
    public class CellFocused
    {

        /// <summary>
        /// Gets or sets a value indicating whether the focus action should be canceled.
        /// </summary>
        public bool Cancel { get; set; }

        /// <summary>
        /// Gets or sets the keyboard event arguments associated with the focus action.
        /// </summary>
        public KeyboardEventArgs? KeyArgs { get; set; }

        /// <summary>
        /// Gets or sets the key combination used during the focus action.
        /// </summary>
        public string? KeyCombination { get; set; }

        /// <summary>
        /// Gets or sets the action performed during the focus event.
        /// </summary>
        public string? Action { get; set; }

        /// <summary>
        /// Gets or sets the row that contains the focused cell.
        /// </summary>
        public Row<object>? Row { get; set; }

        /// <summary>
        /// Gets or sets the cell that is currently focused.
        /// </summary>
        public Cell<object>? Cell { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the row has changed during the focus event.
        /// </summary>
        public bool IsRowChanged { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focus event was triggered by a keyboard action.
        /// </summary>
        public bool IsKeyEvent { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focused cell belongs to the header row.
        /// </summary>
        public bool IsHeader { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the focus involves jumping to another cell.
        /// </summary>
        public bool IsJump { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether DOM focus should be prevented for the cell.
        /// </summary>
        public bool PreventDOMFocus { get; set; }
    }

    #endregion
}
