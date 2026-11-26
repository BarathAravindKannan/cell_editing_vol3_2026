using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// State of the DOM virtualization content renderer that <see cref="DomVirtualHandler{T}"/> needs to scroll to
    /// and render a row. Implemented by the renderer, which keeps ownership of its state and of <see cref="EnsureLock"/>.
    /// </summary>
    internal interface IDomVirtualContent
    {
        int ScrollRowCount { get; }

        int RenderedRowCount { get; }

        int WindowStartIndex { get; }

        int WindowEndIndex { get; }

        int RowHeight { get; }

        int ViewportHeight { get; }

        long TotalDataHeight { get; }

        int BrowserMaxScrollHeight { get; }

        int CurrentScrollTop { get; set; }

        /// <summary>
        /// True while a scroll-triggered fetch owns the window or a refresh is queued behind it.
        /// </summary>
        bool IsFetching { get; }

        SemaphoreSlim EnsureLock { get; }

        bool HasRenderedRow(int rowIndex);

        /// <summary>
        /// Drops the window row cache so the next render regenerates every window row (entry indexes shifted).
        /// </summary>
        void ClearRowCache();

        Task UpdateRenderedRowsAsync(bool force);
    }

    /// <summary>
    /// Handles DOM virtualization row access for the grid: total row count and rendering a row that is outside the rendered window.
    /// Focus related state and keyboard navigation stay in <see cref="FocusHandler{T}"/>.
    /// </summary>
    /// <typeparam name="T">TValue of grid component.</typeparam>
    internal partial class DomVirtualHandler<T>
    {
        #region Private Properties

        private SfGrid<T> _parent { get; set; }

        #endregion

        #region Constructor

        public DomVirtualHandler(SfGrid<T> parent)
        {
            _parent = parent;
        }

        #endregion

        #region Internal Properties

        /// <summary>
        /// The content renderer. Registered by the renderer while it is alive.
        /// </summary>
        internal IDomVirtualContent? Content { get; set; }

        /// <summary>
        /// Total number of rows the scroll track represents (whole data set, or the current page when paging).
        /// Lets keyboard navigation reason about rows that are not rendered.
        /// </summary>
        internal int TotalRowCount { get; set; }

        #endregion

        #region Edit State Retention

        /// <summary>
        /// Row models that carry edit state (edited, dirty, deleted, in an open editor) and are currently outside the rendered window,
        /// keyed by their global row index. Only models are kept: no editor component, no DOM reference. The index is a valid key
        /// because every change of data order or size regenerates the rows and calls <see cref="ResetEditState"/>.
        /// </summary>
        internal Dictionary<int, Row<object>> RetainedRows { get; } = new();

        /// <summary>
        /// Rows added through the grid (batch or normal add) that have not been saved yet. They are not part of the data source,
        /// so they are re-attached to the window instead of being indexed.
        /// </summary>
        internal List<Row<object>> AddedRows { get; } = new();

        private readonly HashSet<int> _selectedRowIndexes = new();
        private readonly HashSet<int> _deselectedRowIndexes = new();
        private readonly Dictionary<int, object> _selectedRowData = new();
        private readonly Dictionary<int, HashSet<int>> _selectedCellIndexes = new();
        private bool _allRowsSelected;

        internal static bool HasEditState(Row<object> row)
        {
            if (row.IsEdit || row.IsDirty || row.EditedData != null || row.Action != EditAction.None)
            {
                return true;
            }
            List<Cell<object>> cells = row.Cells;
            for (int i = 0; i < cells.Count; i++)
            {
                if (cells[i].IsEdit || cells[i].IsDirty)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Retains selected row and cell flags before a data row leaves the rendered window.
        /// </summary>
        internal void CaptureSelectionState(Row<object> row)
        {
            if (!_parent.EnableDomVirtualization || !row.IsDataRow || row.IsDetailRow || row.Index is not int rowIndex || rowIndex < 0)
            {
                return;
            }

            if (row.IsSelected)
            {
                _selectedRowIndexes.Add(rowIndex);
                _selectedRowData[rowIndex] = row.Data!;
                _deselectedRowIndexes.Remove(rowIndex);
            }
            else
            {
                _selectedRowIndexes.Remove(rowIndex);
                _selectedRowData.Remove(rowIndex);
                if (_allRowsSelected)
                {
                    _deselectedRowIndexes.Add(rowIndex);
                }
            }

            HashSet<int> selectedCells = row.Cells
                .Where(cell => cell.IsSelected && cell.Index.HasValue)
                .Select(cell => cell.Index!.Value)
                .ToHashSet();
            if (selectedCells.Count > 0)
            {
                _selectedCellIndexes[rowIndex] = selectedCells;
            }
            else
            {
                _selectedCellIndexes.Remove(rowIndex);
            }
        }

        /// <summary>
        /// Checks whether a row index is explicitly selected or covered by header select-all.
        /// </summary>
        internal bool IsRowSelected(int rowIndex) =>
            _selectedRowIndexes.Contains(rowIndex) || (_allRowsSelected && !_deselectedRowIndexes.Contains(rowIndex));

        /// <summary>
        /// Checks whether the specified cell is retained as selected.
        /// </summary>
        internal bool IsCellSelected(int rowIndex, int? cellIndex) =>
            cellIndex.HasValue && _selectedCellIndexes.TryGetValue(rowIndex, out HashSet<int> selectedCells)
            && selectedCells.Contains(cellIndex.Value);

        /// <summary>
        /// Gets retained selected rows and their data.
        /// </summary>
        internal IEnumerable<KeyValuePair<int, object>> GetRetainedSelectedRows() =>
            _selectedRowData;

        /// <summary>
        /// Gets retained selected cell indexes.
        /// </summary>
        internal IEnumerable<(int RowIndex, int CellIndex)> GetRetainedSelectedCells() =>
            _selectedCellIndexes.SelectMany(row => row.Value.Select(cellIndex => (row.Key, cellIndex)));

        /// <summary>
        /// Marks every row as selected until selection is cleared or the data view changes.
        /// </summary>
        internal void SetAllRowsSelected()
        {
            _allRowsSelected = true;
            _deselectedRowIndexes.Clear();
        }

        /// <summary>
        /// Clears retained row, cell, and select-all state.
        /// </summary>
        internal void ClearRetainedSelectionState()
        {
            _selectedRowIndexes.Clear();
            _deselectedRowIndexes.Clear();
            _selectedRowData.Clear();
            _selectedCellIndexes.Clear();
            _allRowsSelected = false;
        }

        /// <summary>
        /// Clears retained row selection, optionally preserving one row and its cells.
        /// </summary>
        internal void ClearRetainedRowSelectionState(int? exceptRowIndex, bool clearCells)
        {
            _allRowsSelected = false;
            _deselectedRowIndexes.Clear();
            foreach (int rowIndex in _selectedRowIndexes.Where(index => index != exceptRowIndex).ToArray())
            {
                _selectedRowIndexes.Remove(rowIndex);
                _selectedRowData.Remove(rowIndex);
            }
            if (clearCells)
            {
                foreach (int rowIndex in _selectedCellIndexes.Keys.Where(index => index != exceptRowIndex).ToArray())
                {
                    _selectedCellIndexes.Remove(rowIndex);
                }
            }
        }

        /// <summary>
        /// Removes one retained row selection and records its deselection under select-all.
        /// </summary>
        internal void ClearRetainedRowSelection(int rowIndex, bool clearCells)
        {
            _selectedRowIndexes.Remove(rowIndex);
            _selectedRowData.Remove(rowIndex);
            if (_allRowsSelected)
            {
                _deselectedRowIndexes.Add(rowIndex);
            }
            if (clearCells)
            {
                _selectedCellIndexes.Remove(rowIndex);
            }
        }

        /// <summary>
        /// Clears retained cell selection, optionally retaining a cell and existing row selection.
        /// </summary>
        internal void ClearRetainedCellSelection(int? exceptRowIndex, int? exceptCellIndex, bool preserveAllRows)
        {
            foreach (int rowIndex in _selectedCellIndexes.Keys.ToArray())
            {
                if (rowIndex == exceptRowIndex && exceptCellIndex.HasValue)
                {
                    HashSet<int> selectedCells = _selectedCellIndexes[rowIndex];
                    selectedCells.RemoveWhere(cellIndex => cellIndex != exceptCellIndex.Value);
                    if (selectedCells.Count == 0)
                    {
                        _selectedCellIndexes.Remove(rowIndex);
                    }
                }
                else
                {
                    _selectedCellIndexes.Remove(rowIndex);
                }
            }

            if (!preserveAllRows)
            {
                int? retainedRowIndex = exceptCellIndex.HasValue ? exceptRowIndex : null;
                _allRowsSelected = false;
                _deselectedRowIndexes.Clear();
                foreach (int rowIndex in _selectedRowIndexes.Where(index => index != retainedRowIndex).ToArray())
                {
                    _selectedRowIndexes.Remove(rowIndex);
                    _selectedRowData.Remove(rowIndex);
                }
            }
        }

        /// <summary>
        /// Window rows plus the retained rows. Used by edit code that must see every edited row, not only the rendered ones.
        /// </summary>
        internal IEnumerable<Row<object>> GetEditRows()
        {
            foreach (Row<object> row in _parent.Rows)
            {
                yield return row;
            }
            foreach (Row<object> row in RetainedRows.Values)
            {
                yield return row;
            }
            foreach (Row<object> row in AddedRows)
            {
                if (!_parent.Rows.Contains(row))
                {
                    yield return row;
                }
            }
        }

        internal Row<object>? FindEditRow(Func<Row<object>, bool> predicate)
        {
            foreach (Row<object> row in GetEditRows())
            {
                if (predicate(row))
                {
                    return row;
                }
            }
            return null;
        }

        /// <summary>
        /// Drops the retained state after save, cancel, or any data change. Retained rows that still show edit flags are reset.
        /// </summary>
        internal void ResetEditState()
        {
            RetainedRows.Clear();
            AddedRows.Clear();
            PendingInputValues = null;
        }

        /// <summary>
        /// Clears the edit flags of rows outside the window when the edit session ends, then drops them.
        /// </summary>
        internal void EndEditSession()
        {
            foreach (Row<object> row in RetainedRows.Values)
            {
                row.IsEdit = false;
                row.Cells.ForEach(cell => cell.IsEdit = false);
            }
            RetainedRows.Clear();
            AddedRows.RemoveAll(row => !row.IsDirty);
            PendingInputValues = null;
        }

        /// <summary>
        /// Text typed into the editor inputs that was not committed to the edit data yet (the editors commit on blur),
        /// keyed by input id. Captured while the editing row leaves the window, applied when its editor is rendered again.
        /// </summary>
        internal Dictionary<string, string>? PendingInputValues { get; set; }

        /// <summary>
        /// Reads the live editor inputs from the DOM and keeps the ones that differ from the edit data.
        /// Must run before the window render that removes the editor.
        /// </summary>
        internal async Task CaptureEditInputsAsync()
        {
            Dictionary<string, string>? live = await _parent.InvokeMethod<Dictionary<string, string>>("sfBlazor.Grid.getDomVirtualEditInputs", false, new object[] { _parent.DataId }).ConfigureAwait(true);
            object? editData = _parent.EditModule?.CloneData;
            if (live == null || live.Count == 0 || editData == null)
            {
                return;
            }
            Dictionary<string, string>? changed = null;
            foreach (GridColumn column in GridUtils.GetColumns(_parent))
            {
                if (string.IsNullOrEmpty(column.Field) || !live.TryGetValue(Edit<T>.GetComplexName(column.Field), out string? text))
                {
                    continue;
                }
                if (!IsSameText(_parent.PropHelper?.GetObject(column.Field, editData)?.ToString(), text))
                {
                    (changed ??= new Dictionary<string, string>())[Edit<T>.GetComplexName(column.Field)] = text;
                }
            }
            PendingInputValues = changed;
        }

        internal async Task ApplyPendingInputValuesAsync()
        {
            Dictionary<string, string>? pending = PendingInputValues;
            if (pending == null || pending.Count == 0)
            {
                return;
            }
            PendingInputValues = null;
            await _parent.InvokeMethod("sfBlazor.Grid.applyDomVirtualInputValues", new object[] { _parent.DataId, pending }).ConfigureAwait(true);
        }

        private static bool IsSameText(string? first, string? second)
        {
            if (string.Equals(first ?? string.Empty, second ?? string.Empty, StringComparison.Ordinal))
            {
                return true;
            }
            return double.TryParse(first, NumberStyles.Any, CultureInfo.CurrentCulture, out double x)
                && double.TryParse(second, NumberStyles.Any, CultureInfo.CurrentCulture, out double y) && x == y;
        }

        /// <summary>
        /// Number of unsaved rows added at the bottom. The scroll track is one row taller for each of them.
        /// </summary>
        internal int AddedBottomCount
        {
            get
            {
                int count = 0;
                for (int i = 0; i < AddedRows.Count; i++)
                {
                    if (AddedRows[i].IsAddedBottom)
                    {
                        count++;
                    }
                }
                return count;
            }
        }

        /// <summary>
        /// Registers the added row, brings it into the window (top or bottom of the data) and renders it. The window is only rebuilt by
        /// scrolling, so an add needs this explicit refresh. A bottom add also grows the scroll track by one row first, so scrolling to the
        /// end reaches the add form.
        /// </summary>
        internal async Task RevealAddedRowAsync(Row<object> row)
        {
            IDomVirtualContent? content = Content;
            if (content == null)
            {
                return;
            }
            if (!AddedRows.Contains(row))
            {
                if (row.IsAddedTop)
                {
                    AddedRows.Insert(0, row);
                }
                else
                {
                    AddedRows.Add(row);
                }
            }
            if (!row.IsAddedTop)
            {
                await content.EnsureLock.WaitAsync().ConfigureAwait(true);
                try
                {
                    int scrollTop = ScrollTopForEffectivePosition(content, Math.Max(0, content.TotalDataHeight - content.ViewportHeight));
                    content.CurrentScrollTop = scrollTop;
                    await content.UpdateRenderedRowsAsync(true).ConfigureAwait(true);
                    if (_parent.IsRendered)
                    {
                        await _parent.InvokeMethod("sfBlazor.Grid.setDomVirtualScrollTop", new object[] { _parent.DataId, scrollTop }).ConfigureAwait(true);
                    }
                }
                finally
                {
                    content.EnsureLock.Release();
                }
                return;
            }
            if (content.ScrollRowCount > 0)
            {
                await EnsureRowRenderedAsync(0, 1).ConfigureAwait(true);
            }
            await content.UpdateRenderedRowsAsync(true).ConfigureAwait(true);
        }

        /// <summary>
        /// Re-renders the current window after edit state changed without any scroll (cancel of an add, batch cancel).
        /// </summary>
        internal Task RefreshWindowAsync() => Content?.UpdateRenderedRowsAsync(true) ?? Task.CompletedTask;

        #endregion

        #region Detail Template State

        private sealed class DetailEntry
        {
            internal Row<object> Row { get; set; } = null!;

            internal int Height { get; set; }
        }

        private readonly Dictionary<int, DetailEntry> _details = new();
        private readonly Dictionary<string, int> _detailUids = new();
        private int[] _detailIndexes = Array.Empty<int>();
        private long[] _detailCumulative = Array.Empty<long>();
        private int _lastMeasuredDetailHeight;

        /// <summary>
        /// True while the renderer has expanded detail rows. The renderer uses it to take the cheap uniform-height path otherwise.
        /// </summary>
        internal bool HasDetails => _details.Count > 0;

        /// <summary>
        /// Sum of the heights of all expanded detail rows. Added to the scroll height of the uniform rows.
        /// </summary>
        internal long DetailTotalHeight => _detailCumulative.Length == 0 ? 0 : _detailCumulative[_detailCumulative.Length - 1];

        /// <summary>
        /// Registers an expanded row. The detail row model is kept here (not in the window) so the expansion survives the row leaving the window.
        /// The height is an estimate until the client reports the measured one.
        /// </summary>
        internal void ExpandDetail(int rowIndex, Row<object> detailRow)
        {
            int height = _lastMeasuredDetailHeight > 0 ? _lastMeasuredDetailHeight : (Content?.RowHeight ?? 30);
            CollapseDetail(rowIndex, false);
            _details[rowIndex] = new DetailEntry { Row = detailRow, Height = height };
            if (detailRow.Uid != null)
            {
                _detailUids[detailRow.Uid] = rowIndex;
            }
            RebuildDetailHeights();
        }

        internal void CollapseDetail(int rowIndex, bool rebuild = true)
        {
            if (_details.Remove(rowIndex, out DetailEntry? removed))
            {
                if (removed.Row.Uid != null)
                {
                    _detailUids.Remove(removed.Row.Uid);
                }
                if (rebuild)
                {
                    RebuildDetailHeights();
                }
            }
        }

        /// <summary>
        /// Looks up the detail row of an expanded data row.
        /// </summary>
        internal bool TryGetDetailRow(int rowIndex, out Row<object> detailRow)
        {
            if (_details.Count > 0 && _details.TryGetValue(rowIndex, out DetailEntry? entry))
            {
                detailRow = entry.Row;
                return true;
            }
            detailRow = null!;
            return false;
        }

        internal void ResetDetailState()
        {
            if (_details.Count == 0)
            {
                return;
            }
            _details.Clear();
            _detailUids.Clear();
            RebuildDetailHeights();
        }

        /// <summary>
        /// Applies heights measured by the client (keyed by the detail row uid) and re-renders the window when one changed.
        /// </summary>
        internal async Task SetDetailHeightsAsync(string[] uids, int[] heights)
        {
            bool changed = false;
            int count = Math.Min(uids.Length, heights.Length);
            for (int i = 0; i < count; i++)
            {
                if (heights[i] > 0 && _detailUids.TryGetValue(uids[i], out int rowIndex) && _details[rowIndex].Height != heights[i])
                {
                    _details[rowIndex].Height = heights[i];
                    _lastMeasuredDetailHeight = heights[i];
                    changed = true;
                }
            }
            if (changed)
            {
                RebuildDetailHeights();
                await RefreshWindowAsync().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Keeps a sorted index list with inclusive cumulative heights. It is rebuilt only when a detail is expanded, collapsed or
        /// re-measured, so scrolling costs O(log k) for k expanded rows and never walks the data set.
        /// </summary>
        private void RebuildDetailHeights()
        {
            // Details are keyed by the data row index; the height math works on scroll positions. They are the same for flat data;
            // with grouping the position is the row's visible entry and details of collapsed groups take no space.
            List<(int Position, int Height)> positions = new(_details.Count);
            foreach (KeyValuePair<int, DetailEntry> detail in _details)
            {
                int position = IsGroupMode ? DataEntryIndex(detail.Key) : detail.Key;
                if (position >= 0)
                {
                    positions.Add((position, detail.Value.Height));
                }
            }
            positions.Sort((x, y) => x.Position.CompareTo(y.Position));
            int[] indexes = new int[positions.Count];
            long[] cumulative = new long[positions.Count];
            long sum = 0;
            for (int i = 0; i < positions.Count; i++)
            {
                indexes[i] = positions[i].Position;
                sum += positions[i].Height;
                cumulative[i] = sum;
            }
            _detailIndexes = indexes;
            _detailCumulative = cumulative;
        }

        /// <summary>
        /// Number of expanded rows whose index is lower than <paramref name="rowIndex"/> (lower bound).
        /// </summary>
        private int ExpandedCountBefore(int rowIndex)
        {
            int low = 0;
            int high = _detailIndexes.Length;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                if (_detailIndexes[mid] < rowIndex)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }
            return low;
        }

        /// <summary>
        /// Top offset of a row: uniform rows plus the details expanded above it.
        /// </summary>
        internal long TopOf(int rowIndex, int rowHeight)
        {
            long top = (long)rowIndex * rowHeight;
            if (_detailIndexes.Length == 0)
            {
                return top;
            }
            int before = ExpandedCountBefore(rowIndex);
            return before == 0 ? top : top + _detailCumulative[before - 1];
        }

        /// <summary>
        /// Inverse of <see cref="TopOf"/>: the row at the effective offset. An offset inside an expanded detail maps to its parent row,
        /// so a tall detail that starts above the viewport still renders the parent.
        /// </summary>
        internal int IndexAt(long offset, int rowHeight)
        {
            if (_detailIndexes.Length == 0)
            {
                return (int)Math.Max(0, offset / rowHeight);
            }
            // Number of details that end at or before the offset (their end offset grows monotonically).
            int low = 0;
            int high = _detailIndexes.Length;
            while (low < high)
            {
                int mid = (low + high) >> 1;
                if (((long)(_detailIndexes[mid] + 1) * rowHeight) + _detailCumulative[mid] <= offset)
                {
                    low = mid + 1;
                }
                else
                {
                    high = mid;
                }
            }
            long adjusted = offset - (low > 0 ? _detailCumulative[low - 1] : 0);
            long index = Math.Max(0, adjusted / rowHeight);
            if (low < _detailIndexes.Length && index > _detailIndexes[low])
            {
                index = _detailIndexes[low];
            }
            return (int)Math.Min(int.MaxValue, index);
        }

        #endregion

        #region Row Access

        /// <summary>
        /// Inverse of the renderer's effective scroll state: the scrollTop whose effective (un-amplified) position is given.
        /// </summary>
        private int ScrollTopForEffectivePosition(IDomVirtualContent content, long effectivePosition)
        {
            long additionalPixels = Math.Max(0, content.TotalDataHeight - content.BrowserMaxScrollHeight);
            long maxScrollable = Math.Max(1, Math.Min(content.TotalDataHeight, content.BrowserMaxScrollHeight) - content.ViewportHeight);
            double factor = 1d + (additionalPixels / (double)maxScrollable);
            double scrollTop = Math.Max(0d, effectivePosition) / factor;
            return (int)Math.Min(maxScrollable, Math.Round(scrollTop));
        }

        /// <summary>
        /// Makes sure the row at <paramref name="targetIndex"/> exists in the DOM (scrolling, fetching and rendering it
        /// when it is outside the current window) and moves the browser scroll position to it. The rows reach the DOM after
        /// this returns, so callers must focus the cell after the window has rendered.
        /// The second argument is the index navigation started from.
        /// </summary>
        internal async Task<bool> EnsureRowRenderedAsync(int targetIndex, int fromIndex)
        {
            IDomVirtualContent? content = Content;
            if (content == null || content.ScrollRowCount == 0)
            {
                return false;
            }

            await content.EnsureLock.WaitAsync().ConfigureAwait(true);
            try
            {
                targetIndex = Math.Clamp(targetIndex, 0, content.ScrollRowCount - 1);
                bool inWindow = content.RenderedRowCount > 0 && targetIndex >= content.WindowStartIndex && targetIndex < content.WindowEndIndex;
                if (inWindow)
                {
                    return true;
                }

                long effectiveTop = targetIndex > fromIndex
                    ? TopOf(targetIndex + 1, content.RowHeight) - content.ViewportHeight
                    : TopOf(targetIndex, content.RowHeight);
                int scrollTop = ScrollTopForEffectivePosition(content, effectiveTop);
                content.CurrentScrollTop = scrollTop;
                await content.UpdateRenderedRowsAsync(true).ConfigureAwait(true);

                // A scroll-triggered fetch may own the window right now; it re-runs with the latest CurrentScrollTop when done.
                for (int attempt = 0; attempt < 200 && content.IsFetching; attempt++)
                {
                    await Task.Delay(10).ConfigureAwait(true);
                }

                if (_parent.IsRendered)
                {
                    await _parent.InvokeMethod("sfBlazor.Grid.setDomVirtualScrollTop", new object[] { _parent.DataId, scrollTop }).ConfigureAwait(true);
                }
                return content.HasRenderedRow(targetIndex);
            }
            finally
            {
                content.EnsureLock.Release();
            }
        }

        #endregion
    }
}
