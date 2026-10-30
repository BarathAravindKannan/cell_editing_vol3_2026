using Syncfusion.Blazor.Data;
using Syncfusion.ExcelExport;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Provides text matching and highlighting functionality for the Find and Highlight feature.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class FindHighlight<T>
    {
        #region Private Properties

        private SfGrid<T> _parent;
        #endregion

        #region Internal Properties
        /// <summary>
        /// Global counter for tracking current match position across all cells during rendering.
        /// Used by server-side active match highlighting to apply the active CSS class.
        /// </summary>
        internal int GlobalMatchCounter { get; set; }
        internal bool RenderFindInput { get; set; } = true;
        internal bool IsFindClickedInAdaptive { get; set; }

        internal bool IsInputValueUpdated { get; set; }

        /// <summary>
        /// Internal property to access the current Find and Highlight key
        /// </summary>
        internal string SearchHighlightKey { get; set; } = string.Empty;

        /// <summary>
        /// Internal property to access the current match index in the Find feature
        /// </summary>
        internal int CurrentMatchIndex { get; set; }

        /// <summary>
        /// Internal property to access the total matches found in the Find feature
        /// </summary>
        internal int TotalMatches { get; set; }


        /// <summary>
        /// Total matches found in rows BEFORE current virtual viewport.
        /// Used to calculate global match index across entire dataset.
        /// </summary>
        internal int TotalMatchesBeforeViewport { get; set; }

        /// <summary>
        /// Column index (in _parent.Columns) of the column that contains the active (orange) match.
        /// -1 means no active match is currently selected.
        /// Used to drive horizontal scroll updates when EnableColumnVirtualization is true.
        /// </summary>
        internal int ActiveColumnIndex { get; set; } = -1;

        internal bool IsFromMethodHighLight { get; set; }

        /// <summary>
        /// Tracks if user has navigated to an active match (clicked next/previous)
        /// Counter shows 0/0 until navigation starts
        /// </summary>
        internal bool HasNavigated { get; set; }

        /// <summary>
        /// Tracks the active match location to preserve highlighting during row selection SoftRefresh.
        /// Structure: {RowUid → {CellUid → ActiveMatchHtmlText}}
        /// Populated ONLY when user navigates to a match (Next/Previous/GoTo).
        /// Used during row selection to re-apply orange highlight without recalculation.
        /// Cleared on Find clear or new search.
        /// </summary>
        internal Dictionary<string, Dictionary<string, string>> ActiveMatchLocationTracker { get; set; } 
            = new Dictionary<string, Dictionary<string, string>>();


        /// <summary>
        /// Flag set to true when this row's state has changed (selection/deselection)
        /// Used to detect SoftRefresh triggered by RowStateChanged event
        /// Set true in InvokeStateChange(), cleared in OnAfterRender()
        /// </summary>

        internal bool IsStateChangeRefresh { get; set; }

        #endregion

        #region Constructor & Initialization

        public FindHighlight(SfGrid<T> parent)
        {
            _parent = parent;
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Validates the find key input.
        /// </summary>
        /// <param name="key">The find key to validate</param>
        /// <param name="errorMessage">Output parameter containing the error message if validation fails</param>
        /// <returns>True if the key is valid, false otherwise</returns>
        /// <remarks>
        /// Current validation rules:
        /// - Empty string is valid (clears highlighting)
        /// - Maximum length is 500 characters
        /// </remarks>
        private static bool ValidateFindKey(string key, out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrEmpty(key))
            {
                return true;
            }
            if (key.Length > 500)
            {
                errorMessage = "Find text cannot exceed 500 characters";
                return false;
            }

            return true;
        }

        /// <summary>
        /// Records the active match location (row/cell/html) for later retrieval during row selection SoftRefresh.
        /// Called when user navigates to a specific match via Next/Previous/GoTo.
        /// Clears previous tracking and stores the new active match location.
        /// </summary>
        internal void StoreActiveMatchLocation(string rowUid, string cellUid, string activeMatchHtmlText)
        {
            ActiveMatchLocationTracker.Clear();
            if (!ActiveMatchLocationTracker.TryGetValue(rowUid, out Dictionary<string, string>? cellTracker))
            {
                cellTracker = new Dictionary<string, string>();
                ActiveMatchLocationTracker[rowUid] = cellTracker;
            }
            ActiveMatchLocationTracker[rowUid][cellUid] = activeMatchHtmlText;
        }

        /// <summary>
        /// Checks if given row/cell contains the tracked active match.
        /// Validates both row UID and cell text to ensure data integrity.
        /// Returns the tracked orange highlight HTML if found and text matches.
        /// </summary>
        private bool TryGetActiveMatchHtml(string rowUid, string cellUid, 
            string currentCellText, out string activeMatchHtml)
        {
            activeMatchHtml = string.Empty;

            if (!ActiveMatchLocationTracker.TryGetValue(rowUid, out Dictionary<string, string>? cellTracker))
            {
                return false;
            }
            
            if (!ActiveMatchLocationTracker[rowUid].TryGetValue(cellUid, out string? storedHtml))
                return false;

            if (!ValidateActiveMatchStillExists(currentCellText, storedHtml))
                return false;
                
            activeMatchHtml = storedHtml;
            return true;
        }

        /// <summary>
        /// Validates that the cell still contains the highlighted text.
        /// Extracts the matched text from inside the &lt;mark&gt; tags and checks if it exists in current cell value.
        /// This handles cases where cell value is transformed (e.g., email contains "janet" as substring).
        /// </summary>
        private static bool ValidateActiveMatchStillExists(string currentCellText, string storedHighlightedHtml)
        {
            if (string.IsNullOrEmpty(currentCellText) || string.IsNullOrEmpty(storedHighlightedHtml))
                return false;
            
                var match = System.Text.RegularExpressions.Regex.Match(
                    storedHighlightedHtml,
                    @">([^<]+)</mark>",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                
                if (!match.Success)
                    return false;
                
                string extractedMatchedText = match.Groups[1].Value;
                
                if (string.IsNullOrEmpty(extractedMatchedText))
                    return false;

                return currentCellText.Contains(
                    extractedMatchedText,
                    StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Extracts only the active (orange) match HTML from the full highlighted cell HTML.
        /// This optimizes storage by keeping only the active mark element.
        /// Example: from "No<mark>rmal</mark> <mark class="e-find-highlight-active">text</mark>"
        /// extracts: &lt;mark class="e-find-highlight-active"&gt;text&lt;/mark&gt;
        /// </summary>
        /// <param name="fullHighlightedHtml">The complete cell HTML with all highlights</param>
        /// <returns>Only the active match HTML, or empty string if not found</returns>
        internal static string ExtractActiveMatchHtml(string fullHighlightedHtml)
        {
            if (string.IsNullOrEmpty(fullHighlightedHtml))
                return string.Empty;

            var match = System.Text.RegularExpressions.Regex.Match(
                fullHighlightedHtml,
                @"<mark class=""e-find-highlight-active"">.*?</mark>",
                System.Text.RegularExpressions.RegexOptions.Singleline);
            
            return fullHighlightedHtml;
        }

        /// <summary>
        /// Counts the number of matches of the find key in the given text.
        /// Supports both case-sensitive and case-insensitive matching.
        /// </summary>
        /// <param name="cellValue">The text to search in</param>
        /// <param name="findKey">The text to find</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive</param>
        /// <returns>The number of non-overlapping matches found</returns>
        private static int CountMatches(object? cellValue, string findKey, bool caseSensitive = false)
        {
            if (cellValue == null || string.IsNullOrEmpty(findKey))
            {
                return 0;
            }

            string text = cellValue.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return 0;
            }

            int count = 0;
            int index = 0;
            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            while ((index = text.IndexOf(findKey, index, comparison)) != -1)
            {
                count++;
                index += findKey.Length;
            }

            return count;
        }


        /// <summary>
        /// Gets the cell value from a row's data using the specified field.
        /// </summary>
        /// <param name="rowData">The row data object</param>
        /// <param name="field">The field path (supports nested fields like "Address.City")</param>
        /// <returns>The cell value or null if not found</returns>
        private static object? GetCellValue(object? rowData, string field)
        {
            if (rowData == null || string.IsNullOrEmpty(field))
            {
                return null;
            }
            // Handle nested fields like "Address.City"
            var fields = field.Split('.');
            object? current = rowData;

            foreach (var fieldPart in fields)
            {
                if (current == null)
                {
                    return null;
                }

                // 1) ExpandoObject - the dictionary itself exposes field values.
                if (current is IDictionary<string, object?> expandoDict)
                {
                    if (expandoDict.TryGetValue(fieldPart, out var expandoVal))
                    {
                        current = expandoVal;
                        continue;
                    }
                    return null;
                }

                // 2) Generic IDictionary (non-generic Expando) - case-insensitive lookup.
                if (current is System.Collections.IDictionary nonGenericDict)
                {
                    if (nonGenericDict.Contains(fieldPart))
                    {
                        current = nonGenericDict[fieldPart];
                        continue;
                    }
                    return null;
                }

                // 3) DynamicObject - use DataUtil.GetObject which knows how to
                //    invoke the dynamic members. DataUtil.GetObject returns null
                //    when the property is missing, so we surface that as a
                //    definitive "not found".
                if (current is IDynamicMetaObjectProvider)
                {
                    current = DataUtil.GetObject(fieldPart, current);
                    if (current == null)
                    {
                        return null;
                    }
                    continue;
                }

                // 4) Standard CLR type - reflection-based property access.
                var property = current.GetType().GetProperty(fieldPart);
                if (property == null)
                {
                    return null;
                }
                current = property.GetValue(current);
            }

            return current;
        }

        /// <summary>
        /// Escapes HTML special characters to prevent injection and ensure safe display.
        /// </summary>
        /// <param name="text">The text to escape</param>
        /// <returns>The HTML-escaped text</returns>
        private static string EscapeHtml(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return text
                .Replace("&", "&amp;", StringComparison.Ordinal)
                .Replace("<", "&lt;", StringComparison.Ordinal)
                .Replace(">", "&gt;", StringComparison.Ordinal)
                .Replace("\"", "&quot;", StringComparison.Ordinal)
                .Replace("'", "&#39;", StringComparison.Ordinal);
        }

        /// <summary>
        /// Gets the cell value for searching and highlighting, applying the column's display format when available.
        /// </summary>
        /// <param name="rowData">The row data object</param>
        /// <param name="column">The column definition</param>
        /// <returns>The formatted display value for the cell</returns>
        internal static object? GetFormattedCellValue(object? rowData, GridColumn? column)
        {
            if (rowData == null || column == null)
            {
                return null;
            }

            object? value = null;
            if (column.IsForeignColumn())
            {
                var keyField = column.ForeignKeyField ?? column.Field;
                var keyValue = GetCellValue(rowData, keyField);

                if (keyValue != null && column.ColumnData is System.Collections.IEnumerable fkData)
                {
                    // Build a WhereFilter equivalent to the cell-renderer
                    // path: match the foreign row whose mapping field
                    // equals the local key value.
                    var filtered = column.GetForeignkeyFilteredData(
                        ((System.Collections.IEnumerable)fkData).Cast<object>(),
                        new List<WhereFilter>
                        {
                            new WhereFilter
                            {
                                Field = keyField,
                                value = keyValue,
                                IgnoreCase = false,
                                Operator = "equal"
                            }
                        });

                    if (filtered != null && filtered.Count > 0 && column.ForeignKeyValue != null)
                    {
                        // Take the first matching row and pull the
                        // display value (ForeignKeyValue) from it.
                        value = DataUtil.GetVal(filtered, 0, column.ForeignKeyValue);
                    }
                    else
                    {
                        // No match in the foreign table - fall back to the
                        // local key value so the user can still find it.
                        value = keyValue;
                    }
                }
                else
                {
                    // No foreign data wired up - use the local value.
                    value = keyValue;
                } 
            }
            else
            {
                // 2) Standard column - GetCellValue now handles ExpandoObject,
                //    DynamicObject, and CLR types uniformly.
                value = GetCellValue(rowData, column.Field);
            }

            if (value is Enum enumValue)
            {
                value = MetadataExtension.GetDisplayName(enumValue);
            }

            if (column.ConvertEmptyStringToNull == true && value?.ToString()?.Length == 0)
            {
                value = null;
            }

            if (value == null && column.NullDisplayText != null)
            {
                value = column.NullDisplayText;
            }
            else if (value == null && column.Type == ColumnType.Boolean && column.DisplayAsCheckBox == true)
            {
                return false;
            }
            else if (value == null)
            {
                return string.Empty;
            }

            if (!string.IsNullOrEmpty(column.Format))
            {
                return DataUtil.GetFormattedValue(value, column.Format);
            }

            return value;
        }

        /// <summary>
        /// Fetches the grid data using the current query, respecting any filters, sorting, or paging applied.
        /// </summary>

        private async Task<object> GetGridDataWithQuery()
        {
            object result = await _parent.DataManager!.ExecuteQuery<T>(_parent.DataModule?.GenerateQuery(true)!).ConfigureAwait(true);
            return result is DataResult ? (IEnumerable<object>)((DataResult)result).Result! : result;
        }

        /// <summary>
        /// Fetches the current view records asynchronously.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        private async Task<List<T>> GetCurrentViewRecordsAsync()
        {
            var rows = _parent.Rows;
            if (_parent.EditModule != null && _parent.EditModule.EditedRow != null && _parent.IsEdit)
            {
                rows = _parent.Rows.Where(x => x.Index != _parent.EditModule.EditedRow.Index).ToList();
            }
            return await Task.FromResult<List<T>>(rows?.Where(_ => _.IsDataRow && !_.IsDetailRow)?.Select(x => (T)x.Data!)?.ToList()!).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the full dataset for match counting, respecting data source type and filters.
        /// </summary>
        /// <returns>IEnumerable of all records for counting matches</returns>
        private async Task<IEnumerable<object>> GetFullDatasetForCountingAsync()
        {
            if (_parent.EnableVirtualization && _parent.DataManager != null)
            {
                var data = await GetGridDataWithQuery().ConfigureAwait(true);
                return (IEnumerable<object>)data;
            }

            var currentView = await GetCurrentViewRecordsAsync().ConfigureAwait(true);
            if (currentView is IEnumerable<object> enumerable2)
            {
                return enumerable2.ToList();
            }
            return new List<object>();
        }

        /// <summary>
        /// Counts total matches across all columns in a single record.
        /// </summary>
        private int CountMatchesInRecord(object record, string findKey, bool caseSensitive)
        {
            int totalCount = 0;

            if (record == null || string.IsNullOrEmpty(findKey))
            {
                return 0;
            }

            var searchableColumns = _parent.Columns;
            if (searchableColumns == null || searchableColumns.Count == 0)
            {
                return 0;
            }
            
            foreach (var column in searchableColumns)
            {
                if (column?.Field == null) continue;
                if (column.Visible == false) continue;
                if (column.AllowSearchHighlight == false) continue;

                var cellValue = GetFormattedCellValue(record, column);
                int columnMatches = CountMatches(cellValue, findKey, caseSensitive);
                totalCount += columnMatches;
            }

            return totalCount;
        }

        /// <summary>
        /// Handles scrolling to the active match in virtualized or non-virtualized grids.
        /// </summary>
        /// <param name="currentMatchIndex">1-based match index (1 = first match)</param>
        internal async Task ScrollToActiveMatchAsync(int currentMatchIndex)
        {
            int targetRowIndex = await GetRowIndexForGlobalMatchIndexAsync(currentMatchIndex).ConfigureAwait(true);
            int targetColumnIndex = -1;
            if (_parent.EnableColumnVirtualization)
            {
                targetColumnIndex = await GetColumnIndexForGlobalMatchIndexAsync(currentMatchIndex).ConfigureAwait(true);
                ActiveColumnIndex = targetColumnIndex;
            }

            if (targetRowIndex >= 0 || targetColumnIndex >= 0)
            {
                await AutoScrollActiveMatch(targetRowIndex < 0 ? 0 : targetRowIndex,
                    isVirtual: _parent.EnableVirtualization,
                    columnIndex: targetColumnIndex).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Clears the find properties, resetting the search state.
        /// </summary>
        /// <returns></returns>

        internal async Task ClearFindProperties()
        {
            SearchHighlightKey = string.Empty;
            CurrentMatchIndex = 0;
            TotalMatches = 0;
            // Reset viewport-anchored state so that subsequent navigation calls
            // (GoToNextAsync / GoToPreviousAsync / GoToAsync) do not operate on
            // stale counters left over from the previous search.
            GlobalMatchCounter = 0;
            TotalMatchesBeforeViewport = 0;
            ActiveColumnIndex = -1;
            IsInputValueUpdated = false;
            ActiveMatchLocationTracker.Clear();
        }

        /// <summary>
        /// Refreshes the grid content based on the current virtualization settings.
        /// </summary>
        /// <returns></returns>
        internal async Task RefreshGrid()
        {
            if (_parent.EnableVirtualization || _parent.EnableColumnVirtualization)
            {
                _parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
            }
            else
            {
                _parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }
        }

        #region Virtual Helper methods

        /// <summary>
        /// Calculates total matches in all rows before the given row index.
        /// Called when virtual viewport changes to maintain correct global counter.
        /// </summary>
        /// <param name="rowStartIndex">Virtual row index where viewport starts</param>
        internal async Task<int> CalculateTotalMatchesBeforeRowAsync(int rowStartIndex)
        {
            if (rowStartIndex <= 0) return 0;

            int count = 0;
            var allRecords = await GetFullDatasetForCountingAsync().ConfigureAwait(true);
            int currentIndex = 0;

            foreach (var record in allRecords)
            {
                if (currentIndex >= rowStartIndex) break;

                if (record != null)
                {
                    count += CountMatchesInRecord(record, SearchHighlightKey, 
                        _parent.FindSettings?.CaseSensitive ?? false);
                }

                currentIndex++;
            }

            return count;
        }

        /// <summary>
        /// Maps a global match index (e.g., "find match #50 across entire dataset")
        /// to the actual row index in the data source.
        /// </summary>
        /// <param name="globalMatchIndex">1-based match index (1 = first match)</param>
        internal async Task<int> GetRowIndexForGlobalMatchIndexAsync(int globalMatchIndex)
        {
            if (globalMatchIndex <= 0) return -1;

            var allRecords = await GetFullDatasetForCountingAsync().ConfigureAwait(true);
            int matchCounter = 0;
            int rowIndex = 0;

            foreach (var record in allRecords)
            {
                if (record == null)
                {
                    rowIndex++;
                    continue;
                }

                int recordMatches = CountMatchesInRecord(record, SearchHighlightKey,
                    _parent.FindSettings?.CaseSensitive ?? false);

                if (matchCounter + recordMatches >= globalMatchIndex)
                {
                    // Found the row containing target match
                    return rowIndex;
                }

                matchCounter += recordMatches;
                rowIndex++;
            }

            return -1; // Not found
        }

        /// <summary>
        /// Calculates total matches in all cells that come BEFORE the active cell at (activeRowIndex, columnIndex) 
        /// in row-major order. Used to offset the per-viewport GlobalMatchCounter to identify the correct active match.
        /// </summary>
        /// <param name="columnIndex">Column index of the active cell (columns before this are counted in the active row)</param>
        /// <param name="activeRowIndex">Row index of the active cell (rows before this are fully counted)</param>
        /// <returns>Total matches in cells before (activeRowIndex, columnIndex) in row-major order</returns>
        internal async Task<int> CalculateTotalMatchesBeforeColumnAsync(int columnIndex, int activeRowIndex)
        {
            var allRecords = await GetFullDatasetForCountingAsync().ConfigureAwait(true);
            int count = 0;
            bool isCaseSensitive = _parent.FindSettings?.CaseSensitive ?? false;
            var columns = _parent.Columns;

            if (columns == null || columns.Count == 0) return 0;

            int currentRow = 0;
            foreach (var record in allRecords)
            {
                if (record == null)
                {
                    currentRow++;
                    continue;
                }

                // Determine how many columns to count for this row
                int upperCol;
                if (currentRow < activeRowIndex)
                {
                    // Rows before activeRowIndex: count all columns
                    upperCol = columns.Count;
                }
                else if (currentRow == activeRowIndex)
                {
                    // Active row: count only columns before columnIndex
                    upperCol = Math.Min(columnIndex, columns.Count);
                }
                else
                {
                    // Rows after activeRowIndex: count nothing (they come after the active cell in row-major order)
                    upperCol = 0;
                }

                // Count matches in columns [0, upperCol)
                for (int i = 0; i < upperCol; i++)
                {
                    var column = columns[i];
                    if (column == null || string.IsNullOrEmpty(column.Field)) continue;
                    if (column.AllowSearching == false) continue;
                    if (column.Visible == false) continue;
                    if (column.AllowSearchHighlight == false) continue;

                    var cellValue = GetFormattedCellValue(record, column);
                    count += CountMatches(cellValue, SearchHighlightKey, isCaseSensitive);
                }

                currentRow++;
            }

            return count;
        }

        /// <summary>
        /// Maps a global match index to the column index that contains the Nth match.
        /// Walks cells in row-major order (matching the order HighlightMatchesServerSideActive
        /// uses to increment GlobalMatchCounter): for each record in data-source order,
        /// iterate columns in order and increment the running counter. When the running
        /// counter reaches the target global match index, return the column index of that
        /// cell.
        /// </summary>
        /// <param name="globalMatchIndex">1-based match index (1 = first match)</param>
        /// <returns>Column index where this match exists, -1 if not found</returns>
        internal async Task<int> GetColumnIndexForGlobalMatchIndexAsync(int globalMatchIndex)
        {
            if (globalMatchIndex <= 0) return -1;

            var allRecords = await GetFullDatasetForCountingAsync().ConfigureAwait(true);
            bool isCaseSensitive = _parent.FindSettings?.CaseSensitive ?? false;
            var columns = _parent.Columns;

            if (columns == null || columns.Count == 0) return -1;

            int running = 0;
            foreach (var record in allRecords)
            {
                if (record == null) continue;
                for (int i = 0; i < columns.Count; i++)
                {
                    var column = columns[i];
                    if (column == null || string.IsNullOrEmpty(column.Field)) continue;
                    if (column.AllowSearching == false) continue;
                    if (column.Visible == false) continue;
                    if (column.AllowSearchHighlight == false) continue;
                    var cellValue = GetFormattedCellValue(record, column);
                    int cellMatches = CountMatches(cellValue, SearchHighlightKey, isCaseSensitive);

                    // A cell with N matches counts N times in the running total,
                    // just like HighlightMatchesServerSideActive increments the
                    // global counter once per match.
                    for (int m = 0; m < cellMatches; m++)
                    {
                        running++;
                        if (running == globalMatchIndex) return i;
                    }
                }
            }
            return -1;
        }

        internal async Task PostRowNotifyForVirtualGrid(List<Row<object>> rows, int virtualRowStartIndex)
        {
            // Notify GridRow component which row has the active highlight
            if (_parent.IsFindToolbarEnabled() && CurrentMatchIndex > 0 && rows?.Count > 0 && (virtualRowStartIndex > 0 || _parent.EnableColumnVirtualization))
            {

                // Get the actual row index from full dataset based on CurrentMatchIndex (1-based global position)
                // This method iterates through full data and counts matches to find the row containing the active match
                int activeRowIndex = await GetRowIndexForGlobalMatchIndexAsync(CurrentMatchIndex).ConfigureAwait(true);
                // Find the Row object in current viewport that matches this index
                var activeRow = rows.FirstOrDefault(r => r.Index == activeRowIndex);
                GlobalMatchCounter = 0;
                _ = UpdateTotalMatchesBeforeViewportAsync(activeRowIndex);

                // Column virtualization: re-anchor the column-axis TotalMatchesBeforeViewport
                // when the active column is outside the current column window. Mirrors the
                // row-virtualization logic above for the column axis. This complements
                // FindHighlight.OnColumnWindowChangedAsync (which fires from SetPageSizeAndCIndex
                // on every horizontal scroll tick) by handling the case where the user navigates
                // Next/Prev to a match whose column is far outside the current window — the
                // per-cell GlobalMatchCounter must restart from 0 with the correct prefix sum
                // so the orange active match lands on the right cell.
                if (_parent.EnableColumnVirtualization && _parent.VirtualScrollModule != null)
                {
                    int activeColumnIndex = await GetColumnIndexForGlobalMatchIndexAsync(CurrentMatchIndex).ConfigureAwait(true);
                    ActiveColumnIndex = activeColumnIndex;

                    int startCol = _parent.VirtualScrollModule.StartColumnIndex;
                    int endCol = _parent.VirtualScrollModule.EndColumnIndex;

                    // CRITICAL: TotalMatchesBeforeViewport must equal the count of matches
                    // in cells that come BEFORE the first rendered cell, in row-major order.
                    // The first rendered cell is (RowStartIndex, StartColumnIndex). We must
                    // pass RowStartIndex (the start of the rendered row window) — NOT the
                    // active match's row index. The cell renderer will walk from
                    // (RowStartIndex, StartColumnIndex) onwards, so the prefix sum must
                    // include all matches in rows [0, RowStartIndex) (all columns) plus
                    // columns [0, StartColumnIndex) in row RowStartIndex.
                    int rowStart = _parent.EnableVirtualization
                        ? _parent.VirtualScrollModule.RowStartIndex
                        : 0;
                    int rowEnd = _parent.VirtualScrollModule.RowEndIndex;

                    // Only re-anchor when the active column is genuinely outside the
                    // current column window. The second condition was previously
                    // "activeColumnIndex < endCol" which is almost always true — should
                    // be "activeColumnIndex > endCol".
                    if (activeColumnIndex >= 0 && (activeColumnIndex >= startCol && activeColumnIndex < endCol))
                    {
                        _ = UpdateTotalMatchesBeforeColumnAsync(startCol, activeRowIndex);
                    }
                }

                if (activeRow != null)
                {
                    _parent.EventAggregator?.Trigger("RowStateChanged", activeRow);
                }
            }
        }

        /// <summary>
        /// Async helper to calculate and update total matches before viewport.
        /// Called as fire-and-forget from synchronous OnParametersSet() to avoid blocking render.
        /// </summary>
        private async Task UpdateTotalMatchesBeforeViewportAsync(int virtualRowStartIndex)
        {
            GlobalMatchCounter = 0;
            int totalBefore = await CalculateTotalMatchesBeforeRowAsync(virtualRowStartIndex).ConfigureAwait(true);
            TotalMatchesBeforeViewport = totalBefore;
        }

        /// <summary>
        /// Async helper to calculate and update total matches before the column window.
        /// Mirrors UpdateTotalMatchesBeforeViewportAsync for the column axis when
        /// EnableColumnVirtualization is true. Called as fire-and-forget from
        /// OnAfterRenderAsync when the user navigates to a match whose column is
        /// outside the current [StartColumnIndex, EndColumnIndex] window.
        /// </summary>
        private async Task UpdateTotalMatchesBeforeColumnAsync(int startColumnIndex, int activeRowIndex)
        {
            if (!_parent.EnableColumnVirtualization) return;
            if (string.IsNullOrEmpty(SearchHighlightKey)) return;

            GlobalMatchCounter = 0;
            int totalBefore = await CalculateTotalMatchesBeforeColumnAsync(startColumnIndex, activeRowIndex).ConfigureAwait(true);
            TotalMatchesBeforeViewport = totalBefore;
        }


        #endregion

        internal async Task AutoScrollActiveMatch(int rowIndex = 0, bool isVirtual = false, int columnIndex = -1)
        {
            try
            {
                await _parent.InvokeMethod("sfBlazor.Grid.scrollActiveMatch", new object[] { _parent.DataId, rowIndex, isVirtual, columnIndex }).ConfigureAwait(true);
            }
            catch (TaskCanceledException)
            {

            }
        }


        /// <summary>
        /// Finds and highlights matches in the grid based on the provided search key.
        /// </summary>
        /// <param name="findKey"></param>
        /// <param name="isFromMethod"></param>
        /// <returns></returns>
        internal async Task FindHighlightAsync(string findKey, bool isFromMethod = false)
        {
            if (!_parent.IsFindToolbarEnabled() && !isFromMethod)
            {
                return;
            }
            IsInputValueUpdated = true;
            
            // Clear previous active match tracking when new search starts
            ActiveMatchLocationTracker.Clear();
            
            if (ValidateFindKey(findKey, out string errorMessage))
            {
                SearchHighlightKey = findKey;

                if (!string.IsNullOrEmpty(findKey))
                {
                    bool isCaseSensitive = _parent.FindSettings?.CaseSensitive ?? false;

                    // Use full dataset for counting, not just visible rows
                    // This ensures counter shows "0/10000" instead of "0/50" for large datasets
                    var allRecords = await GetFullDatasetForCountingAsync().ConfigureAwait(true);

                    // Count total matches across all records
                    TotalMatches = 0;
                    foreach (var record in allRecords)
                    {
                        if (record == null) continue;

                        int recordMatches = CountMatchesInRecord(record, findKey, isCaseSensitive);
                        TotalMatches += recordMatches;
                    }
                    CurrentMatchIndex = 0;
                }
                else
                {
                    TotalMatches = 0;
                    CurrentMatchIndex = 0;
                }

                GlobalMatchCounter = 0;
                ActiveColumnIndex = -1;
                await RefreshGrid().ConfigureAwait(true);
            }
            else
            {
                await ClearFindProperties().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Clears any existing search highlights when a form is opened.
        /// This prevents search highlighting from a previously opened Edit form
        /// from being retained when opening an Add form or another form instance.
        /// </summary>

        internal async Task ClearSearchHighlightOnFormOpenAsync()
        {
            if (_parent.IsFindToolbarEnabled() && !string.IsNullOrEmpty(SearchHighlightKey))
            {
                await _parent.ResetFindAsync().ConfigureAwait(true);
                _parent.EventAggregator.Trigger("RefreshSearchHighlight", null!);
            }
        }

        /// <summary>
        /// Advances the active (orange) highlight to the next match in row-major order.
        /// Wraps from the last match back to the first. If no find has been executed yet,
        /// or the current search produced zero matches, the call is a silent no-op.
        /// Mirrors the semantics of the Find toolbar's "Next" button and the Enter key.
        /// </summary>
        /// <remarks>
        /// Programmatic callers are gated by <see cref="IsFromMethodHighLight"/>: the public
        /// <c>FindNextHighlightAsync()</c> on <c>SfGrid</c> sets this flag before delegating
        /// so the call also works when the user has not added <c>"Find"</c> to the toolbar.
        /// </remarks>
        internal async Task GoToNextAsync()
        {
            if (!_parent.IsFindToolbarEnabled() && !IsFromMethodHighLight)
            {
                return;
            }
            if (TotalMatches <= 0 || string.IsNullOrEmpty(SearchHighlightKey))
            {
                return;
            }
            HasNavigated = true;
            if (CurrentMatchIndex <= 0)
            {
                // Fresh search with no prior navigation: jump to the first match.
                CurrentMatchIndex = 1;
            }
            else if (CurrentMatchIndex >= TotalMatches)
            {
                // Wrap-around at the upper boundary.
                CurrentMatchIndex = 1;
            }
            else
            {
                CurrentMatchIndex++;
            }

            IsInputValueUpdated = false;
            GlobalMatchCounter = 0;
            await ScrollToActiveMatchAsync(CurrentMatchIndex).ConfigureAwait(true);
            await RefreshGrid().ConfigureAwait(true);
            // Notify the GridFindInput toolbar component to re-render so the
            // match counter (e.g. "0/108" → "3/108") updates. This is the same
            // event the form-open clear path uses, so the existing handler
            // (StateHasChanged on the input) is reused — no new wiring needed.
            _parent.EventAggregator.Trigger("RefreshSearchHighlight", null!);
        }

        /// <summary>
        /// Moves the active (orange) highlight to the previous match in row-major order.
        /// Wraps from the first match to the last. If no find has been executed yet,
        /// or the current search produced zero matches, the call is a silent no-op.
        /// Mirrors the semantics of the Find toolbar's "Previous" button and Shift+Enter.
        /// </summary>
        /// <remarks>
        /// Programmatic callers are gated by <see cref="IsFromMethodHighLight"/>: the public
        /// <c>FindPreviousHighlightAsync()</c> on <c>SfGrid</c> sets this flag before delegating
        /// so the call also works when the user has not added <c>"Find"</c> to the toolbar.
        /// </remarks>
        internal async Task GoToPreviousAsync()
        {
            if (!_parent.IsFindToolbarEnabled() && !IsFromMethodHighLight)
            {
                return;
            }
            if (TotalMatches <= 0 || string.IsNullOrEmpty(SearchHighlightKey))
            {
                return;
            }
            HasNavigated = true;
            if (CurrentMatchIndex <= 0)
            {
                // Fresh search with no prior navigation: wrap to the last match.
                CurrentMatchIndex = TotalMatches;
            }
            else if (CurrentMatchIndex <= 1)
            {
                // Wrap-around at the lower boundary.
                CurrentMatchIndex = TotalMatches;
            }
            else
            {
                CurrentMatchIndex--;
            }

            IsInputValueUpdated = false;
            GlobalMatchCounter = 0;
            await ScrollToActiveMatchAsync(CurrentMatchIndex).ConfigureAwait(true);
            await RefreshGrid().ConfigureAwait(true);
            // See GoToNextAsync: the toolbar's GridFindInput listens for this
            // event to re-render its counter when navigation happens from code.
            _parent.EventAggregator.Trigger("RefreshSearchHighlight", null!);
        }

        /// <summary>
        /// Jumps the active (orange) highlight directly to the 1-based match number.
        /// Values outside <c>[1, TotalMatches]</c> are clamped to the nearest valid value.
        /// If no find has been executed yet, or the current search produced zero matches,
        /// the call is a silent no-op.
        /// </summary>
        /// <param name="matchNumber">
        /// 1-based index of the match to activate. Values less than <c>1</c> are clamped
        /// to <c>1</c>; values greater than <c>TotalMatches</c> are clamped to <c>TotalMatches</c>.
        /// </param>
        /// <remarks>
        /// Programmatic callers are gated by <see cref="IsFromMethodHighLight"/>: the public
        /// <c>FindGoToAsync(int)</c> on <c>SfGrid</c> sets this flag before delegating
        /// so the call also works when the user has not added <c>"Find"</c> to the toolbar.
        /// </remarks>
        internal async Task GoToAsync(int matchNumber)
        {
            if (!_parent.IsFindToolbarEnabled() && !IsFromMethodHighLight)
            {
                return;
            }
            if (TotalMatches <= 0 || string.IsNullOrEmpty(SearchHighlightKey))
            {
                return;
            }
            HasNavigated = true;
            // Clamp to [1, TotalMatches]; matches the AG-Grid findGoTo(matchNumber) contract.
            if (matchNumber < 1)
            {
                matchNumber = 1;
            }
            else if (matchNumber > TotalMatches)
            {
                matchNumber = TotalMatches;
            }

            CurrentMatchIndex = matchNumber;
            IsInputValueUpdated = false;
            GlobalMatchCounter = 0;
            await ScrollToActiveMatchAsync(CurrentMatchIndex).ConfigureAwait(true);
            await RefreshGrid().ConfigureAwait(true);
            // See GoToNextAsync: the toolbar's GridFindInput listens for this
            // event to re-render its counter when navigation happens from code.
            _parent.EventAggregator.Trigger("RefreshSearchHighlight", null!);
        }

        #endregion

        #region Cell Rendering Highlighting Helper methods
        /// <summary>
        /// Highlights matches in the given text by wrapping them with a mark element.
        /// Supports both case-sensitive and case-insensitive matching.
        /// </summary>
        /// <param name="cellValue">The cell value to highlight</param>
        /// <param name="findKey">The text to find and highlight</param>
        /// <param name="cssClass">The CSS class to apply to the mark element</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive</param>
        /// <returns>A tuple containing the match count and the highlighted HTML string</returns>
        /// <remarks>
        /// Returns the original text as plain HTML if no matches are found.
        /// The matched text is wrapped in: &lt;mark class="cssClass"&gt;matched text&lt;/mark&gt;
        /// </remarks>
        internal static (int matchCount, string highlightedHtml) HighlightMatches(object? cellValue, string findKey, string cssClass, bool caseSensitive = false)
        {
            if (cellValue == null || string.IsNullOrEmpty(findKey))
            {
                return (0, cellValue?.ToString() ?? string.Empty);
            }

            string text = cellValue.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return (0, text);
            }

            // Escape HTML special characters in the original text
            string escapedText = EscapeHtml(text);
            string escapedFindKey = EscapeHtml(findKey);

            int matchCount = 0;
            var result = new StringBuilder();
            int lastIndex = 0;
            int index;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Find all matches and build the highlighted HTML
            while ((index = escapedText.IndexOf(escapedFindKey, lastIndex, comparison)) != -1)
            {
                matchCount++;

                // Append text before the match
                result.Append(escapedText.AsSpan(lastIndex, index - lastIndex));

                // Append the match wrapped in a mark tag (preserve original case)
                result.Append(CultureInfo.InvariantCulture, $"<mark class=\"{cssClass}\">{escapedText.AsSpan(index, escapedFindKey.Length)}</mark>");

                // Move past this match
                lastIndex = index + escapedFindKey.Length;
            }

            // Append the remaining text
            result.Append(escapedText.AsSpan(lastIndex));

            return (matchCount, result.ToString());
        }

        /// <summary>
        /// Highlights matches with server-side active match tracking.
        /// Applies the active CSS class to the match at the specified active index.
        /// Returns the updated global match counter for tracking position across cells.
        /// </summary>
        /// <param name="cellValue">The cell value to highlight</param>
        /// <param name="findKey">The text to find and highlight</param>
        /// <param name="regularCssClass">The CSS class to apply to regular matches</param>
        /// <param name="activeCssClass">The CSS class to apply to the active match</param>
        /// <param name="activeMatchIndex">The index of the active match (1-based)</param>
        /// <param name="globalMatchCounter">Current global counter value (incremented by method and returned)</param>
        /// <param name="foundActiveMatch">Output: true if this cell contains the active match</param>
        /// <param name="caseSensitive">Whether the search should be case-sensitive</param>
        /// <returns>A tuple containing (matchCount, highlightedHtml, updatedGlobalMatchCounter, foundActiveMatch)</returns>
        internal (int matchCount, string highlightedHtml, int updatedCounter, bool foundActive) HighlightMatchesServerSideActive(
            object? cellValue,
            string findKey,
            string regularCssClass,
            string activeCssClass,
            int activeMatchIndex,
            int globalMatchCounter,
            out bool foundActiveMatch,
            bool caseSensitive = false)
        {
            foundActiveMatch = false;
            
            if (cellValue == null || string.IsNullOrEmpty(findKey))
            {
                return (0, cellValue?.ToString() ?? string.Empty, globalMatchCounter, false);
            }

            string text = cellValue.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(text))
            {
                return (0, text, globalMatchCounter, false);
            }

            // Escape HTML special characters in the original text
            string escapedText = EscapeHtml(text);
            string escapedFindKey = EscapeHtml(findKey);

            int matchCount = 0;
            var result = new StringBuilder();
            int lastIndex = 0;
            int index;

            var comparison = caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

            // Find all matches and build the highlighted HTML
            while ((index = escapedText.IndexOf(escapedFindKey, lastIndex, comparison)) != -1)
            {
                //if (!_parent.EnableVirtualization)
                //{
                    matchCount++;
                    globalMatchCounter++;
                //}
                 

                // Append text before the match
                result.Append(escapedText.AsSpan(lastIndex, index - lastIndex));

                // Compare global position (before+current) to activeMatchIndex
                // globalMatchCounter = per-viewport counter (resets each viewport)
                // TotalMatchesBeforeViewport = cumulative matches before this viewport
                // activeMatchIndex = user's global match number (e.g., 2 in "2/200")
                int globalMatchPosition = TotalMatchesBeforeViewport + globalMatchCounter;
                bool isActive = (globalMatchPosition == activeMatchIndex);
                string effectiveCssClass = isActive ? activeCssClass : regularCssClass;
                
                // Track if we found the active match in this cell
                if (isActive)
                {
                    foundActiveMatch = true;
                }

                // Append the match wrapped in a mark tag (preserve original case)
                result.Append(CultureInfo.InvariantCulture, $"<mark class=\"{effectiveCssClass}\">{escapedText.AsSpan(index, escapedFindKey.Length)}</mark>");

                // Move past this match
                lastIndex = index + escapedFindKey.Length;
            }

            // Append the remaining text
            result.Append(escapedText.AsSpan(lastIndex));

            return (matchCount, result.ToString(), globalMatchCounter, foundActiveMatch);
        }

        internal bool TryGetTrackedActiveMatchHtml( Row<object>? row, Cell<object>? cell, object? cellValue, bool isRowStateChangeRefresh,
            out string activeMatchHtml)
        {
            activeMatchHtml = string.Empty;

            return row?.Uid != null
                && cell?.Uid != null
                && cellValue != null
                && isRowStateChangeRefresh
                && TryGetActiveMatchHtml(
                    row.Uid,
                    cell.Uid,
                    cellValue.ToString()!,
                    out activeMatchHtml);
        }

        #endregion
    }
}
