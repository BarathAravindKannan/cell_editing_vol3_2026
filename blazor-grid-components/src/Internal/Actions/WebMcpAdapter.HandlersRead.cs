using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Read-handlers partial — implements tools which query
    /// grid state without mutating persisted data. Each handler delegates to the
    /// matching <see cref="SfGrid{TValue}"/> public async API and wraps the
    /// result. Includes the transient Find &amp; Highlight orchestrator, which
    /// paints cells visually but does not modify grid data.
    ///
    /// All failures are normalized to <see cref="WebMcpException"/>
    /// (ErrorCode <c>ExecutionFailed</c>) so the dispatcher sees a uniform
    /// error contract across read and write tools.
    /// </summary>
    public partial class WebMcpAdapter<TValue>
    {
        // READ HANDLERS — annotate readOnlyHint=true in the schema (see Catalog partial)

        private async Task<object> HandleGetSelectedRecords(Dictionary<string, object> args)
        {
            try
            {
                // No required args - read-only operation
                var records = await _parent.GetSelectedRecordsAsync().ConfigureAwait(true);
                return new
                {
                    records = records,
                    count = records?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getSelectedRecords",
                    "Failed to get selected records", ex);
            }
        }

        private async Task<object> HandleGetSelectedRowIndexes(Dictionary<string, object> args)
        {
            try
            {
                var indexes = await _parent.GetSelectedRowIndexesAsync().ConfigureAwait(true);
                return new { indexes = indexes };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getSelectedRowIndexes",
                    "Failed to get selected row indexes", ex);
            }
        }

        private async Task<object> HandleGetFilteredRecords(Dictionary<string, object> args)
        {
            try
            {
                var isStrict = args.ContainsKey("isStrict") && args["isStrict"] is bool b && b;
                var records = await _parent.GetFilteredRecordsAsync(isStrict).ConfigureAwait(true);
                return new { records = records };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getFilteredRecords",
                    "Failed to get filtered records", ex);
            }
        }

        private async Task<object> HandleGetCurrentViewRecords(Dictionary<string, object> args)
        {
            try
            {
                var records = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
                return new { records = records };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getCurrentViewRecords",
                    "Failed to get current view records", ex);
            }
        }

        private async Task<object> HandleGetColumnByField(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getColumnByField", "fieldName");
                var fieldName = args.GetString("fieldName");
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "getColumnByField",
                        "Argument 'fieldName' must be a non-empty string.");
                }
                var column = await _parent.GetColumnByFieldAsync(fieldName).ConfigureAwait(true);
                return new { column = column };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getColumnByField",
                    "Failed to get column by field", ex);
            }
        }

        private async Task<object> HandleGetVisibleColumns(Dictionary<string, object> args)
        {
            try
            {
                var columns = await _parent.GetVisibleColumnsAsync().ConfigureAwait(true);
                return new { columns = columns };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getVisibleColumns",
                    "Failed to get visible columns", ex);
            }
        }

        private async Task<object> HandleGetPersistData(Dictionary<string, object> args)
        {
            try
            {
                var data = await _parent.GetPersistDataAsync().ConfigureAwait(true);
                return new { data = data };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getPersistData",
                    "Failed to get persisted grid state", ex);
            }
        }

        private async Task<object> HandleGetPrimaryKeyFieldNames(Dictionary<string, object> args)
        {
            try
            {
                var fieldNames = await _parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                return new
                {
                    fieldNames = fieldNames,
                    count = fieldNames?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getPrimaryKeyFieldNames",
                    "Failed to get primary-key field names", ex);
            }
        }

        private async Task<object> HandleGetRowIndexByPrimaryKey(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getRowIndexByPrimaryKey", "value");
                var value = args["value"];
                if (value is null)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "getRowIndexByPrimaryKey",
                        "Argument 'value' must not be null.");
                }

                var searchAcrossRecords = args.GetBool("searchAcrossRecords");
                var rowIndex = await _parent.GetRowIndexByPrimaryKeyAsync(value, searchAcrossRecords).ConfigureAwait(true);
                return new
                {
                    rowIndex = rowIndex,
                    found = rowIndex >= 0,
                    searchAcrossRecords = searchAcrossRecords
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getRowIndexByPrimaryKey",
                    "Failed to find row by primary key", ex);
            }
        }

        private async Task<object> HandleGetColumns(Dictionary<string, object> args)
        {
            try
            {
                var columns = await _parent.GetColumnsAsync().ConfigureAwait(true);
                return new
                {
                    columns = columns,
                    count = columns?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getColumns",
                    "Failed to get grid columns", ex);
            }
        }

        private async Task<object> HandleGetForeignKeyColumns(Dictionary<string, object> args)
        {
            try
            {
                var columns = await _parent.GetForeignKeyColumnsAsync().ConfigureAwait(true);
                return new
                {
                    columns = columns,
                    count = columns?.Count ?? 0
                };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getForeignKeyColumns",
                    "Failed to get foreign-key columns", ex);
            }
        }

        private async Task<object> HandleGetColumnIndexByUid(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getColumnIndexByUid", "uid");
                var uid = args.GetString("uid");
                if (string.IsNullOrWhiteSpace(uid))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "getColumnIndexByUid",
                        "Argument 'uid' must be a non-empty string.");
                }

                var index = await _parent.GetColumnIndexByUidAsync(uid).ConfigureAwait(true);
                return new { uid = uid, index = index };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getColumnIndexByUid",
                    "Failed to get column index by UID", ex);
            }
        }

        private async Task<object> HandleGetColumnByUid(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getColumnByUid", "uid");
                var uid = args.GetString("uid");
                var column = await _parent.GetColumnByUidAsync(uid).ConfigureAwait(true);
                return new { column = column };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getColumnByUid",
                    "Failed to get column by UID", ex);
            }
        }

        private async Task<object> HandleGetColumnFieldNames(Dictionary<string, object> args)
        {
            try
            {
                var fieldNames = await _parent.GetColumnFieldNamesAsync().ConfigureAwait(true);
                return new { fieldNames = fieldNames, count = fieldNames.Count };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getColumnFieldNames",
                    "Failed to get column field names", ex);
            }
        }

        private async Task<object> HandleGetCellValue(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getCellValue", "rowIndex", "columnIndex");
                var rowIndex = ParsePositiveIndex(args["rowIndex"], "getCellValue", "rowIndex") - 1;
                var columnIndex = ParsePositiveIndex(args["columnIndex"], "getCellValue", "columnIndex") - 1;
                var value = await _parent.GetCellValueByIndexAsync(rowIndex, columnIndex).ConfigureAwait(true);
                return new { rowIndex = rowIndex + 1, columnIndex = columnIndex + 1, value = value };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getCellValue",
                    "Failed to get cell value", ex);
            }
        }

        private async Task<object> HandleGetEditState(Dictionary<string, object> args)
        {
            try
            {
                if (_parent.EditSettings?.AllowEditing != true)
                {
                    return new { hasContext = false, isModified = false, validationMessages = Array.Empty<string>() };
                }

                var context = await _parent.GetEditContextAsync().ConfigureAwait(true);
                if (context == null)
                {
                    return new { hasContext = false, isModified = false, validationMessages = Array.Empty<string>() };
                }

                return new
                {
                    hasContext = true,
                    isModified = context.IsModified(),
                    validationMessages = context.GetValidationMessages().ToList()
                };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getEditState",
                    "Failed to inspect the current edit state", ex);
            }
        }

        private async Task<object> HandleIsDirty(Dictionary<string, object> args)
        {
            try
            {
                if (_parent.EditSettings?.Mode != EditMode.Batch)
                {
                    return new { isDirty = false };
                }

                return new { isDirty = await _parent.IsDirtyAsync().ConfigureAwait(true) };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "isDirty",
                    "Failed to inspect pending batch edits", ex);
            }
        }

        private static int ParsePositiveIndex(object value, string toolName, string argumentName)
        {
            if (!int.TryParse(value?.ToString(), System.Globalization.NumberStyles.Integer,
                    System.Globalization.CultureInfo.InvariantCulture, out var parsed) || parsed < 1)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                    $"Argument '{argumentName}' must be a positive 1-based integer.");
            }
            return parsed;
        }

        // PHASE 4 EXPANSION (18): find & highlight — paints matching cells and walks matches.
        // ReadOnlyHint=true in the catalog: highlights are a transient visual effect,
        // grid data and persisted state are not modified. Same dispatcher arm list
        // as the mutation-style handlers, so it shares their import block here.
        private async Task<object> HandleFindAndHighlight(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("findAndHighlight", "action");
                var action = args.GetString("action") ?? "highlight";

                switch (action.ToUpperInvariant())
                {
                    case "RESET":
                        await _parent.ResetFindAsync().ConfigureAwait(true);
                        return new { success = true, action = "reset" };

                    case "NEXT":
                        await _parent.FindNextHighlightAsync().ConfigureAwait(true);
                        return new { success = true, action = "next" };

                    case "PREVIOUS":
                        await _parent.FindPreviousHighlightAsync().ConfigureAwait(true);
                        return new { success = true, action = "previous" };

                    case "GOTO":
                        args.ValidateRequired("findAndHighlight", "matchNumber");
                        var gotoIndexStr = args.GetString("matchNumber");
                        if (!int.TryParse(gotoIndexStr, out var gotoIndex) || gotoIndex < 1)
                        {
                            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "findAndHighlight",
                                $"Argument 'matchNumber' must be a positive integer. Received: '{gotoIndexStr}'.");
                        }
                        await _parent.FindGoToAsync(gotoIndex).ConfigureAwait(true);
                        return new { success = true, action = "goto", matchNumber = gotoIndex };

                    case "HIGHLIGHT":
                        args.ValidateRequired("findAndHighlight", "findKey");
                        var findKey = args.GetString("findKey") ?? string.Empty;
                        await _parent.FindHighlightAsync(findKey).ConfigureAwait(true);
                        return new { success = true, action = "highlight", findKey = findKey };

                    default:
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "findAndHighlight",
                            $"Unknown action '{action}'. Use one of: highlight, reset, next, previous, goto.");
                }
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "findAndHighlight",
                    "Failed to apply find & highlight", ex);
            }
        }

        // PHASE 5 EXPANSION (27/41): read selected cell indexes
        private async Task<object> HandleGetSelectedRowCellIndexes(Dictionary<string, object> args)
        {
            try
            {
                var tuples = await _parent.GetSelectedRowCellIndexesAsync().ConfigureAwait(true);
                // Convert internal 0-based (rowIndex, cellIndex) tuples back to 1-based for the agent.
                var indexes = (tuples ?? new List<ValueTuple<int, int>>())
                    .Select(t => new { rowIndex = t.Item1 + 1, cellIndex = t.Item2 + 1 })
                    .ToList();
                return new
                {
                    count = indexes.Count,
                    indexes = indexes,
                    selectionMode = _parent.SelectionSettings?.Mode.ToString() ?? string.Empty
                };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getSelectedRowCellIndexes",
                    "Failed to get selected row cell indexes", ex);
            }
        }

        // PHASE 6 EXPANSION (43/46): GetHiddenColumnsAsync
        private async Task<object> HandleGetHiddenColumns(Dictionary<string, object> args)
        {
            try
            {
                var columns = await _parent.GetHiddenColumnsAsync().ConfigureAwait(true);
                return new { count = columns?.Count ?? 0, columns = columns ?? new List<GridColumn>() };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getHiddenColumns",
                    "Failed to get hidden columns", ex);
            }
        }

        // PHASE 6 EXPANSION (44/46): GetColumnIndexByFieldAsync
        private async Task<object> HandleGetColumnIndexByField(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getColumnIndexByField", "fieldName");
                var fieldName = args.GetString("fieldName");
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "getColumnIndexByField",
                        "Argument 'fieldName' must be a non-empty string.");
                }
                var idx = await _parent.GetColumnIndexByFieldAsync(fieldName!).ConfigureAwait(true);
                return new { index = idx, fieldName = fieldName };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getColumnIndexByField",
                    "Failed to get column index by field", ex);
            }
        }

        // PHASE 6 EXPANSION (45/46): GetUidByColumnFieldAsync
        private async Task<object> HandleGetUidByColumnField(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("getUidByColumnField", "fieldName");
                var fieldName = args.GetString("fieldName");
                if (string.IsNullOrWhiteSpace(fieldName))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "getUidByColumnField",
                        "Argument 'fieldName' must be a non-empty string.");
                }
                var uid = await _parent.GetUidByColumnFieldAsync(fieldName!).ConfigureAwait(true);
                return new { uid = uid ?? string.Empty };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "getUidByColumnField",
                    "Failed to get UID by column field", ex);
            }
        }
    }
}
