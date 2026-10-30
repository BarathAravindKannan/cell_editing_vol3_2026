using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Dispatcher partial — owns the public <c>ExecuteHandler</c> pipeline,
    /// the <c>GetTools</c> accessor, and the lightweight tracing helper.
    ///
    /// Pipeline: strip prefix → verify tool → fire
    /// <c>OnWebMcpExecute</c> (host may cancel, log/audit, set
    /// a result for custom tools, or throw a <see cref="WebMcpException"/>
    /// for a structured error) → if the host set a result, return it;
    /// otherwise route to the verified built-in handler → wrap in
    /// success / error response. Any exception is converted to a
    /// structured <see cref="WebMcpToolResponse"/> — never propagated to
    /// the JS layer.
    /// </summary>
    public partial class WebMcpAdapter<TValue>
    {
        // ===== Phase 2: Cancellation sentinel =====
        // The literal prefix returned to the AI agent when a tool execution is cancelled.
        // Agents are expected to treat this as a terminal signal and not retry.
        internal const string CancelledSentinel = "[USER_CANCELLED]";

        // ===== Phase 2: Lightweight logger =====
        // Set to true to enable [WebMCP] prefix logs to System.Diagnostics.Trace during development.
        // Production deployments should leave this false to avoid log spam from high-frequency agent calls.
        internal static bool EnableLogging;

        // Thread-safe counter for telemetry
        private static int _executionCounter;

        /// <summary>
        /// Get all available WebMCP tools, optionally filtered by name.
        /// Framework-internal — only <see cref="SfGrid{TValue}.GetWebMcpToolsAsync"/>
        /// calls it from the same assembly.
        /// </summary>
        internal static List<IWebMcpTool> GetTools(IEnumerable<string>? toolNames = null)
        {
            if (toolNames == null || !toolNames.Any())
                return webMcpTools
                    .Select(t => (IWebMcpTool)new WebMcpToolInfo
                    {
                        Name = t.Name,
                        Description = t.Description,
                        InputSchema = t.InputSchema
                    })
                    .ToList();

            return webMcpTools
                .Where(t => toolNames.Contains(t.Name))
                .Select(t => (IWebMcpTool)new WebMcpToolInfo
                {
                    Name = t.Name,
                    Description = t.Description,
                    InputSchema = t.InputSchema
                })
                .ToList();
        }

        /// <summary>
        /// Execute a tool command and return response.
        /// This is the main dispatcher called from JavaScript.
        /// </summary>
        /// <param name="command">Full tool name with optional prefix (e.g., "Grid_sortByColumn").</param>
        /// <param name="args">Dictionary of arguments provided by the agent.</param>
        /// <returns>A structured response with content and error indicator.</returns>
        /// <remarks>
        /// Execution flow:
        /// <list type="number">
        /// <item><description>Strip prefix from command name.</description></item>
        /// <item><description>Resolve the effective tool against the merged catalog
        ///   (built-in catalog + <see cref="GridWebMcpSettings"/> overrides).</description></item>
        /// <item><description>Fire <c>OnWebMcpExecute</c> — the host may
        ///   cancel, log/audit, set a result for fully custom tools, or
        ///   throw a <see cref="WebMcpException"/> for a structured error.</description></item>
        /// <item><description>If the host set a result, return it; if the host
        ///   threw a <see cref="WebMcpException"/>, return a structured error
        ///   response.</description></item>
        /// <item><description>For built-in tools with no host-set result, route to
        ///   the verified built-in handler.</description></item>
        /// <item><description>Return success or error response. Any exception is
        ///   caught and returned as a structured error response.</description></item>
        /// </list>
        /// </remarks>
        // Marked internal (not public) because the return type
        // WebMcpToolResponse is internal. The public JS-invokable
        // surface is GridJSInteropAdaptor.InvokeWebMcpToolExecute,
        // which serialises the response to a string and never exposes
        // the WebMcpToolResponse CLR type to external code.
        internal async Task<WebMcpToolResponse> ExecuteHandler(
            string command,
            Dictionary<string, object>? args)
        {
            // CA1062: public method on a publicly-exposed dispatcher — validate
            // the non-nullable command parameter. The dispatcher's outer
            // catch(Exception) converts this into a structured error response
            // so the JS layer never sees an unhandled ArgumentException.
            if (string.IsNullOrEmpty(command))
            {
                throw new ArgumentException("Command must be a non-empty tool name.", nameof(command));
            }

            var stopwatch = EnableLogging ? Stopwatch.StartNew() : null;
            var executionId = EnableLogging ? Interlocked.Increment(ref _executionCounter) : 0;

            try
            {
                // CS8602: ensure the parent reference is non-null before
                // dereferencing _parent.WebMcpSettings / _parent.GridEvents
                // later in the pipeline. A null _parent means the adapter
                // was created outside its expected lifecycle.
                if (_parent == null)
                {
                    throw new WebMcpException(
                        WebMcpErrorCode.ExecutionFailed,
                        command,
                        "WebMcpAdapter has no parent grid.");
                }

                // Ensure args is never null for handlers
                args ??= new Dictionary<string, object>();

                // Strip prefix if present (e.g., "Grid_sortByColumn" → "sortByColumn")
                var sep = command.IndexOf('_', StringComparison.Ordinal);
                var baseCommand = sep >= 0
                    ? command.Substring(sep + 1)
                    : command;

                // Resolve the merged (effective) catalog. When the user did
                // not supply settings, this returns the full built-in list
                // unchanged.
                var effectiveTools = ResolveEffectiveTools(_parent.WebMcpSettings);
                var toolDef = effectiveTools.FirstOrDefault(t => t.Name == baseCommand);

                if (toolDef == null)
                {
                    throw new WebMcpException(
                        WebMcpErrorCode.UnknownTool,
                        baseCommand,
                        $"Unknown tool '{baseCommand}'. Verify the tool name in the WebMCP tool catalog.");
                }

                var isCustomTool = _parent.WebMcpSettings?.ChildTools?.Any(
                    tool => tool != null &&
                        tool.Command == GridWebMcpToolCommand.Custom &&
                        string.Equals(tool.Name, baseCommand, StringComparison.Ordinal)) == true;
                var hasExecuteHandler =
                    _parent.GridEvents?.OnWebMcpExecute.HasDelegate == true;
                if (isCustomTool && !hasExecuteHandler)
                {
                    throw new WebMcpException(
                        WebMcpErrorCode.ExecutionFailed,
                        baseCommand,
                        $"Custom WebMCP tool '{baseCommand}' requires a GridEvents.OnWebMcpExecute handler that sets Result.");
                }

                // Fire OnWebMcpExecute for every tool — built-in or
                // custom. The host may cancel, log / audit, set a result
                // (for fully custom tools), or throw a WebMcpException to
                // return a structured error. This is the SINGLE execution
                // point for custom tools (no per-tool Handler callback,
                // no per-tool confirmation dialog — see the class-level
                // doc on WebMcpToolExecuteEventArgs).
                var eventArgs = new WebMcpToolExecuteEventArgs
                {
                    Command = GetEventToolCommand(baseCommand, isCustomTool),
                    ToolName = GetEventToolName(baseCommand, isCustomTool),
                    Arguments = args
                };
                if (_parent!.GridEvents != null && _parent.GridEvents.OnWebMcpExecute.HasDelegate == true)
                {
                    try
                    {
                        await _parent.GridEvents.OnWebMcpExecute.InvokeAsync(eventArgs).ConfigureAwait(true);
                    }
                    catch (WebMcpException ex)
                    {
                        // Host threw a structured error — honour it as if it
                        // came from a built-in handler. Intentional denial
                        // still uses args.Cancel = true, not a throw.
                        LogExecution(executionId, baseCommand, $"Host-Error:{ex.ErrorCode}", stopwatch);
                        return CreateStructuredErrorResponse(ex.ErrorCode, baseCommand, ex.Message);
                    }
                    catch (Exception)
                    {
                        LogExecution(executionId, baseCommand, "Host-Error:ExecutionFailed", stopwatch);
                        return CreateStructuredErrorResponse(
                            WebMcpErrorCode.ExecutionFailed,
                            baseCommand,
                            $"The GridEvents.OnWebMcpExecute handler failed while executing tool '{baseCommand}'.");
                    }
                }

                // 1. Host cancellation wins over everything else.
                if (eventArgs.Cancel)
                {
                    LogExecution(executionId, baseCommand, "Cancelled", stopwatch);
                    return CreateCancelledResponse(
                        $"{CancelledSentinel} Tool \"{baseCommand}\" was cancelled by host. This is final. Do NOT retry.");
                }

                // 2. Host pre-set a result (typical for custom tools).
                if (eventArgs.Result != null)
                {
                    LogExecution(executionId, baseCommand, "Success", stopwatch);
                    return CreateSuccessResponse(eventArgs.Result);
                }

                if (isCustomTool)
                {
                    throw new WebMcpException(
                        WebMcpErrorCode.ExecutionFailed,
                        baseCommand,
                        $"The GridEvents.OnWebMcpExecute handler did not set Result for custom tool '{baseCommand}'.");
                }

                // 3. Route to the verified built-in handler.
                object result = baseCommand switch
                {
                    "getSelectedRecords" => await HandleGetSelectedRecords(args!).ConfigureAwait(true),
                    "getSelectedRowIndexes" => await HandleGetSelectedRowIndexes(args!).ConfigureAwait(true),
                    "getFilteredRecords" => await HandleGetFilteredRecords(args!).ConfigureAwait(true),
                    "getCurrentViewRecords" => await HandleGetCurrentViewRecords(args!).ConfigureAwait(true),
                    "getColumnByField" => await HandleGetColumnByField(args!).ConfigureAwait(true),
                    "getVisibleColumns" => await HandleGetVisibleColumns(args!).ConfigureAwait(true),
                    "sortByColumn" => await HandleSortByColumn(args!).ConfigureAwait(true),
                    "filterByColumn" => await HandleFilterByColumn(args!).ConfigureAwait(true),
                    "navigateToPage" => await HandleNavigateToPage(args!).ConfigureAwait(true),
                    "selectRows" => await HandleSelectRows(args!).ConfigureAwait(true),
                    "clearSelection" => await HandleClearSelection(args!).ConfigureAwait(true),
                    "clearCellSelection" => await HandleClearCellSelection(args!).ConfigureAwait(true),
                    "setCellValue" => await HandleSetCellValue(args!).ConfigureAwait(true),
                    "setRowData" => await HandleSetRowData(args!).ConfigureAwait(true),
                    "crudLifecycle" => await HandleCrudLifecycle(args!).ConfigureAwait(true),
                    "deleteRow" => await HandleDeleteRow(args!).ConfigureAwait(true),
                    "excelExport" => await HandleExcelExport(args!).ConfigureAwait(true),
                    "pdfExport" => await HandlePdfExport(args!).ConfigureAwait(true),
                    "preventRender" => await HandlePreventRender(args!).ConfigureAwait(true),
                    "groupColumn" => await HandleGroupColumn(args!).ConfigureAwait(true),
                    "searchText" => await HandleSearchText(args!).ConfigureAwait(true),
                    "findAndHighlight" => await HandleFindAndHighlight(args!).ConfigureAwait(true),
                    "sortColumns" => await HandleSortColumns(args!).ConfigureAwait(true),
                    "sortColumnsWithClear" => await HandleSortColumnsWithClear(args!).ConfigureAwait(true),
                    "clearSorting" => await HandleClearSorting(args!).ConfigureAwait(true),
                    "clearSortingByFields" => await HandleClearSortingByFields(args!).ConfigureAwait(true),
                    "clearFiltering" => await HandleClearFiltering(args!).ConfigureAwait(true),
                    "selectCell" => await HandleSelectCell(args!).ConfigureAwait(true),
                    "selectCells" => await HandleSelectCells(args!).ConfigureAwait(true),
                    "selectCellsByRange" => await HandleSelectCellsByRange(args!).ConfigureAwait(true),
                    "getSelectedRowCellIndexes" => await HandleGetSelectedRowCellIndexes(args!).ConfigureAwait(true),
                    "selectRowsByRange" => await HandleSelectRowsByRange(args!).ConfigureAwait(true),
                    "saveCell" => await HandleSaveCell(args!).ConfigureAwait(true),
                    "editCell" => await HandleEditCell(args!).ConfigureAwait(true),
                    "ungroupColumn" => await HandleUngroupColumn(args!).ConfigureAwait(true),
                    "expandAllGroups" => await HandleExpandAllGroups(args!).ConfigureAwait(true),
                    "collapseAllGroups" => await HandleCollapseAllGroups(args!).ConfigureAwait(true),
                    "clearGrouping" => await HandleClearGrouping(args!).ConfigureAwait(true),
                    "showColumns" => await HandleShowColumns(args!).ConfigureAwait(true),
                    "hideColumns" => await HandleHideColumns(args!).ConfigureAwait(true),
                    "reorderColumn" => await HandleReorderColumn(args!).ConfigureAwait(true),
                    "reorderColumns" => await HandleReorderColumns(args!).ConfigureAwait(true),
                    "reorderColumnByIndex" => await HandleReorderColumnByIndex(args!).ConfigureAwait(true),
                    "reorderColumnByTargetIndex" => await HandleReorderColumnByTargetIndex(args!).ConfigureAwait(true),
                    "autoFitColumns" => await HandleAutoFitColumns(args!).ConfigureAwait(true),
                    "openColumnChooser" => await HandleOpenColumnChooser(args!).ConfigureAwait(true),
                    "getHiddenColumns" => await HandleGetHiddenColumns(args!).ConfigureAwait(true),
                    "getColumnIndexByField" => await HandleGetColumnIndexByField(args!).ConfigureAwait(true),
                    "getUidByColumnField" => await HandleGetUidByColumnField(args!).ConfigureAwait(true),
                    "printGrid" => await HandlePrintGrid(args!).ConfigureAwait(true),
                    "copyToClipboard" => await HandleCopyToClipboard(args!).ConfigureAwait(true),
                    "pasteFromClipboard" => await HandlePasteFromClipboard(args!).ConfigureAwait(true),
                    "showSpinner" => await HandleShowSpinner(args!).ConfigureAwait(true),
                    "hideSpinner" => await HandleHideSpinner(args!).ConfigureAwait(true),
                    "refreshGrid" => await HandleRefreshGrid(args!).ConfigureAwait(true),
                    "refreshColumns" => await HandleRefreshColumns(args!).ConfigureAwait(true),
                    "refreshHeader" => await HandleRefreshHeader(args!).ConfigureAwait(true),
                    "focusGrid" => await HandleFocusGrid(args!).ConfigureAwait(true),
                    "scrollIntoView" => await HandleScrollIntoView(args!).ConfigureAwait(true),
                    "enableToolbarItems" => await HandleEnableToolbarItems(args!).ConfigureAwait(true),
                    "expandCollapseDetailRow" => await HandleExpandCollapseDetailRow(args!).ConfigureAwait(true),
                    "expandAllDetailRows" => await HandleExpandAllDetailRows(args!).ConfigureAwait(true),
                    "collapseAllDetailRows" => await HandleCollapseAllDetailRows(args!).ConfigureAwait(true),
                    "mergeCells" => await HandleMergeCells(args!).ConfigureAwait(true),
                    "mergeCellsBulk" => await HandleMergeCellsBulk(args!).ConfigureAwait(true),
                    "unmergeCells" => await HandleUnmergeCells(args!).ConfigureAwait(true),
                    "unmergeCellsBulk" => await HandleUnmergeCellsBulk(args!).ConfigureAwait(true),
                    "unmergeAll" => await HandleUnmergeAll(args!).ConfigureAwait(true),
                    "getPersistData" => await HandleGetPersistData(args!).ConfigureAwait(true),
                    "getPrimaryKeyFieldNames" => await HandleGetPrimaryKeyFieldNames(args!).ConfigureAwait(true),
                    "getRowIndexByPrimaryKey" => await HandleGetRowIndexByPrimaryKey(args!).ConfigureAwait(true),
                    "getColumns" => await HandleGetColumns(args!).ConfigureAwait(true),
                    "getForeignKeyColumns" => await HandleGetForeignKeyColumns(args!).ConfigureAwait(true),
                    "getColumnIndexByUid" => await HandleGetColumnIndexByUid(args!).ConfigureAwait(true),
                    "clearRowSelection" => await HandleClearRowSelection(args!).ConfigureAwait(true),
                    "undo" => await HandleUndo(args!).ConfigureAwait(true),
                    "redo" => await HandleRedo(args!).ConfigureAwait(true),
                    "getColumnByUid" => await HandleGetColumnByUid(args!).ConfigureAwait(true),
                    "getColumnFieldNames" => await HandleGetColumnFieldNames(args!).ConfigureAwait(true),
                    "getCellValue" => await HandleGetCellValue(args!).ConfigureAwait(true),
                    "getEditState" => await HandleGetEditState(args!).ConfigureAwait(true),
                    "isDirty" => await HandleIsDirty(args!).ConfigureAwait(true),
                    "batchEditWorkflow" => await HandleBatchEditWorkflow(args!).ConfigureAwait(true),
                    "updateGridData" => await HandleUpdateGridData(args!).ConfigureAwait(true),
                    "reorderRow" => await HandleReorderRow(args!).ConfigureAwait(true),
                    "persistGridState" => await HandlePersistGridState(args!).ConfigureAwait(true),
                    "updatePageSize" => await HandleUpdatePageSize(args!).ConfigureAwait(true),
                    "updateExternalMessage" => await HandleUpdateExternalMessage(args!).ConfigureAwait(true),
                    "undoRedoHistory" => await HandleUndoRedoHistory(args!).ConfigureAwait(true),
                    _ => throw new WebMcpException(
                        WebMcpErrorCode.UnknownTool,
                        baseCommand,
                        $"Unknown tool: {baseCommand}")
                };

                LogExecution(executionId, baseCommand, "Success", stopwatch);
                return CreateSuccessResponse(result);
            }
            catch (WebMcpException wmEx)
            {
                LogExecution(executionId, command, $"WebMCP-Error:{wmEx.ErrorCode}", stopwatch);
                return CreateStructuredErrorResponse(wmEx.ErrorCode, wmEx.ToolName, wmEx.Message);
            }
        }

        // ===== Phase 2: Lightweight logging =====
        private static void LogExecution(int executionId, string toolName, string outcome, Stopwatch? stopwatch)
        {
            if (!EnableLogging) return;
            var ms = stopwatch?.ElapsedMilliseconds ?? 0;
            System.Diagnostics.Trace.WriteLine(
                $"[WebMCP] #{executionId} {toolName} → {outcome} ({ms} ms)");
        }

        private string GetEventToolName(string baseCommand, bool isCustomTool)
        {
            var configuredTools = _parent.WebMcpSettings?.ChildTools;
            if (isCustomTool)
            {
                return configuredTools?
                    .LastOrDefault(tool =>
                        tool != null &&
                        tool.Command == GridWebMcpToolCommand.Custom &&
                        string.Equals(tool.Name, baseCommand, StringComparison.Ordinal))
                    ?.Name ?? baseCommand;
            }

            var configuredCommand = configuredTools?
                .LastOrDefault(tool =>
                    tool != null &&
                    tool.Command != GridWebMcpToolCommand.Custom &&
                    tool.Command != GridWebMcpToolCommand.None &&
                    string.Equals(
                        string.IsNullOrWhiteSpace(tool.Name)
                            ? ResolveToolNameForCommand(tool.Command)
                            : tool.Name,
                        baseCommand,
                        StringComparison.Ordinal))
                ?.Command;

            var builtInCommand = configuredCommand ?? ResolveCommandForBaseName(baseCommand);
            return builtInCommand?.ToString() ?? baseCommand;
        }

        private GridWebMcpToolCommand GetEventToolCommand(string baseCommand, bool isCustomTool)
        {
            if (isCustomTool)
            {
                return GridWebMcpToolCommand.Custom;
            }

            var configuredCommand = _parent.WebMcpSettings?.ChildTools?
                .LastOrDefault(tool =>
                    tool != null &&
                    tool.Command != GridWebMcpToolCommand.Custom &&
                    tool.Command != GridWebMcpToolCommand.None &&
                    string.Equals(
                        string.IsNullOrWhiteSpace(tool.Name)
                            ? ResolveToolNameForCommand(tool.Command)
                            : tool.Name,
                        baseCommand,
                        StringComparison.Ordinal))
                ?.Command;

            return configuredCommand ?? ResolveCommandForBaseName(baseCommand) ?? GridWebMcpToolCommand.None;
        }
    }
}
