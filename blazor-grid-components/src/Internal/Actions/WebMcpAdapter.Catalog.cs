using Syncfusion.Blazor.Grids.Internal;
using System;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Tool catalog partial — owns the static registry of all built-in WebMCP
    /// tools and their JSON schemas (input/output/annotations).
    ///
    /// Split out from <see cref="WebMcpAdapter{TValue}"/> for single-responsibility:
    /// schema definitions change rarely and are reviewed independently from
    /// the runtime dispatcher and handlers.
    ///
    /// Every Name registered here MUST match a case label in the dispatcher
    /// switch in <see cref="WebMcpAdapter{TValue}.ExecuteHandler"/>. Keeping the
    /// catalog and dispatcher in lock-step ensures user-supplied overrides
    /// via <c>GridWebMcpSettings</c> always resolve to a built-in handler.
    /// </summary>
    public partial class WebMcpAdapter<TValue>
    {
        // ─────────────────────── SHARED SCHEMA ARRAY CONSTANTS ───────────────────────
        // Every distinct `required` list and `enum` vocabulary used by the
        // schemas below is defined once here as a `private static readonly`
        // field. This silences CA1861 (prefer static readonly over
        // constant array arguments) and CA1825 (avoid unnecessary
        // zero-length array allocations — use Array.Empty<string>()).
        // Without these fields, each tool initialization re-allocated
        // identical arrays.

        // Shared zero-length `required` array (CA1825).
        private static readonly string[] _emptyRequired = System.Array.Empty<string>();
        private static readonly string[] _reqPrimaryKeyValueColumnField = new[] { "primaryKeyValue", "columnField" };
        private static readonly string[] _reqFieldName = new[] { "fieldName" };
        private static readonly string[] _reqColumnName = new[] { "columnName" };
        private static readonly string[] _reqColumnNames = new[] { "columnNames" };
        private static readonly string[] _reqFieldNames = new[] { "fieldNames" };
        private static readonly string[] _reqSearchString = new[] { "searchString" };
        private static readonly string[] _reqColumns = new[] { "columns" };
        private static readonly string[] _reqAction = new[] { "action" };
        private static readonly string[] _reqKeyRowData = new[] { "key", "rowData" };
        private static readonly string[] _reqStartIndex = new[] { "startIndex" };
        private static readonly string[] _reqRowCellIndexes = new[] { "rowCellIndexes" };
        private static readonly string[] _reqRowIndexFieldName = new[] { "index", "fieldName" };
        private static readonly string[] _reqFindKeyAction = new[] { "findKey", "action" };
        private static readonly string[] _reqInfos = new[] { "infos" };
        private static readonly string[] _reqFieldNameValue = new[] { "fieldName", "value" };
        private static readonly string[] _reqRowIndex = new[] { "rowIndex" };
        private static readonly string[] _reqFieldWhereEquals = new[] { "field", "equals" };
        private static readonly string[] _reqFromFieldNameToFieldName = new[] { "fromFieldName", "toFieldName" };
        private static readonly string[] _reqFromFieldNamesToFieldName = new[] { "fromFieldNames", "toFieldName" };
        private static readonly string[] _reqFromIndexToIndex = new[] { "fromIndex", "toIndex" };
        private static readonly string[] _reqFieldNameToIndex = new[] { "fieldName", "toIndex" };
        private static readonly string[] _reqRowIndexCellIndex = new[] { "rowIndex", "cellIndex" };
        private static readonly string[] _reqStartRowIndexStartCellIndex = new[] { "startRowIndex", "startCellIndex" };
        private static readonly string[] _reqRowIndexColumnIndex = new[] { "rowIndex", "columnIndex" };
        private static readonly string[] _reqFieldDirection = new[] { "field", "direction" };
        private static readonly string[] _reqUid = new[] { "uid" };
        private static readonly string[] _reqValue = new[] { "value" };
        private static readonly string[] _reqMessage = new[] { "message" };
        private static readonly string[] _reqIsDirty = new[] { "isDirty" };

        // Enum vocabularies (string[]) — shared across tools.
        private static readonly string[] _enumCrudAction = new[]
        {
            "startEdit", "endEdit", "closeEdit", "addRecord", "deleteRecord"
        };
        private static readonly string[] _enumSortDirection = new[] { "Ascending", "Descending" };
        private static readonly string[] _enumExportFormat = new[] { "xlsx", "csv" };
        private static readonly string[] _enumColumnLookup = new[] { "HeaderText", "Field" };
        private static readonly string[] _enumPredicate = new[] { "and", "or" };
        private static readonly string[] _enumExportType = new[] { "AllPages", "CurrentPage" };
        private static readonly string[] _enumBatchEditAction = new[] { "inspect", "isDirty", "updateRow", "apply" };
        private static readonly string[] _enumGridDataAction = new[] { "cell", "row" };
        private static readonly string[] _enumPageSizeAction = new[] { "fixed", "viewport" };
        private static readonly string[] _enumUndoRedoAction = new[] { "undoAll", "redoAll", "clear" };
        private static readonly string[] _enumPersistAction = new[] { "get", "set", "reset" };

        /// <summary>
        /// Initialize the static WebMCP tool registry. Each tool is a
        /// workflow-oriented operation (sort, filter, page, export, etc.) —
        /// not a thin wrapper over a single public method. The schema mirrors
        /// the handler contract exactly so the agent's arguments are
        /// validated at parse time.
        /// </summary>
        private static void InitializeTools()
        {
            if (webMcpTools.Count > 0) return; // Already initialized.

            // ─────────────────────── READ TOOLS ───────────────────────
            webMcpTools.Add(new WebMcpTool
            {
                Name = "getSelectedRecords",
                Description = "Get selected row records from the grid",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        records = new { type = "array", description = "Selected row records" },
                        count = new { type = "integer", description = "Number of selected records" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getSelectedRowIndexes",
                Description = "Get indexes of selected rows",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        indexes = new { type = "array", items = new { type = "integer" }, description = "Selected row indexes" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getFilteredRecords",
                Description = "Get records matching current filters",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        isStrict = new { type = "boolean", description = "Use strict filtering (optional)" }
                    },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        records = new { type = "array", description = "Filtered records" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getCurrentViewRecords",
                Description = "Get visible records after sort, filter, and grouping",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        records = new { type = "array", description = "Current view records" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getColumnByField",
                Description = "Get column definition by field name",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldName = new { type = "string", description = "Field name to search" }
                    },
                    required = _reqFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        column = new { type = "object", description = "Column definition" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getVisibleColumns",
                Description = "Get list of visible columns",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columns = new { type = "array", description = "Visible column definitions" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            // ─────────────────────── WRITE TOOLS ───────────────────────

            webMcpTools.Add(new WebMcpTool
            {
                Name = "filterByColumn",
                Description = "Filter the grid by column value. filterOperator is optional; omitted means case-insensitive 'contains'. Canonical Syncfusion operators and friendly aliases are accepted. Optional predicate, matchCase, ignoreAccent, actualFilterValue, actualOperator, and columnUid are forwarded to FilterByColumnAsync.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldName = new { type = "string", description = "Field/property name on the grid's data item to filter (case-sensitive)." },
                        filterOperator = new
                        {
                            type = "string",
                            description = "Operator token (canonical lowercase keyword). OPTIONAL — omit for default case-insensitive 'contains'. Also accepts friendly aliases: eq / == / isequal, neq / != / ne / isnotequal, startswith / beginswith / ^ / prefix, endswith / suffix, contains / has / includes, greaterthan / gt / >, lessthan / lt / <, greaterthanorequal / gte / >=, lessthanorequal / lte / <=, isnull / null, isnotnull / notnull, isempty / empty / blank, isnotempty / notempty, like, wildcard / glob. Whitespace, dashes and underscores in the alias are stripped before lookup, so 'Starts_With', 'starts with' and 'starts-with' all resolve to 'startswith'. Locale-safe — Turkish dotted/dotless concerns bypassed by OrdinalIgnoreCase."
                        },
                        value = new { description = "Filter value. For isnull/isnotnull/isempty/isnotempty pass null." },
                        predicate = new { type = "string", @enum = _enumPredicate, description = "Predicate joining this filter with other column filters." },
                        matchCase = new { type = "boolean", description = "Whether string matching is case-sensitive. Defaults to false." },
                        ignoreAccent = new { type = "boolean", description = "Whether accent differences are ignored." },
                        actualFilterValue = new { description = "Optional typed filter value supplied to the grid filter module." },
                        actualOperator = new { description = "Optional typed filter operator supplied to the grid filter module." },
                        columnUid = new { type = "string", description = "Optional column UID to disambiguate foreign-key columns sharing a field name." }
                    },
                    required = _reqFieldNameValue
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        requestedFilterOperator = new { type = "string", description = "Normalized requested operator, defaulting to 'contains'." },
                        appliedFilterOperator = new { type = "string", description = "Operator applied after string equality promotion." },
                        promoted = new { type = "boolean", description = "Whether equality was promoted to substring matching for a string column." },
                        caseInsensitive = new { type = "boolean", description = "Whether matching is case-insensitive." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "navigateToPage",
                Description = "Switch the grid's current page. Use action='goto' (default) with pageNo for an explicit destination, or action='next' / 'previous' / 'first' / 'last' for relative moves. Out-of-range pages are clamped to the current row of buttons.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new
                        {
                            type = "string",
                            description = "Navigation verb. One of: 'goto' (default, requires pageNo), 'next', 'previous', 'first', 'last'. Unknown actions return InvalidArgument."
                        },
                        pageNo = new
                        {
                            type = "integer",
                            description = "1-based destination page. Required only when action='goto'."
                        }
                    },
                    required = _emptyRequired // Either action='goto'+pageNo, or one of the relative actions, must be supplied. Enforced by the handler.
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        action = new { type = "string", description = "The action that was actually applied (may be clamped if 'next' was called on the last page, etc.)." },
                        currentPage = new { type = "integer", description = "The current page after the navigation (1-based)." },
                        totalPages = new { type = "integer", description = "Total available pages — synthesized from TotalItemCount / PageSettings.PageSize when paging is on, otherwise 1." },
                        requestedPage = new { type = "integer", description = "The page that was actually requested before clamping (default 0 when action='next'/'previous')." }
                    }
                },
                Annotations = new { ReadOnlyHint = true } // ReadOnly because paging navigation does not mutate grid data — only viewport position.
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "selectRows",
                Description = "Select one or more rows in the grid. Pass rowIndexes as an array for multi-row positional selection (e.g. 'select rows 2 through 4' → rowIndexes=[2,3,4]), or use where for field-value match (raise where.limit > 1 to select multiple matching rows). Combine both inputs to select the union. Works for both SelectionType.Single and SelectionType.Multiple grids — when the grid is configured for Single only the first resolved row is selected (multi-row intents succeed silently with one row).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndexes = new
                        {
                            type = "array",
                            items = new { type = "integer" },
                            description = "1-based row indexes against the currently visible rows (rowIndex=1 selects the first row). Provide this for positional intents like 'select rows 2, 3, and 4' or 'select the 5th record'."
                        },
                        where = new
                        {
                            type = "object",
                            description = "Field-value lookup against the grid's current-view records. Provide this for intents like 'the row whose CustomerID equals TOMSP'. 'limit' defaults to 1 (pick the first match).",
                            properties = new
                            {
                                field = new { type = "string", description = "Public property name on the grid's item type (case-sensitive, e.g. 'CustomerID')." },
                                equals = new { type = "string", description = "String value to match. Other types (numbers, dates) should be supplied as their string form; comparison is Doneby `String.Equals(value.ToString(), equals, OrdinalIgnoreCase)` unless caseSensitive=true." },
                                limit = new { type = "integer", description = "Maximum matches to select (default 1). Use higher limits for non-unique fields." },
                                caseSensitive = new { type = "boolean", description = "When true, comparison is case-sensitive. Default false (OrdinalIgnoreCase)." }
                            },
                            required = _reqFieldWhereEquals
                        },
                        toggle = new { type = "boolean", description = "Toggle selection instead of replacing it (optional)." }
                    },
                    required = _emptyRequired // At least one of rowIndexes/where is enforced by the handler.
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        count = new { type = "integer", description = "Total rows actually applied (1 for SelectionType.Single grids; equals appliedIndexes.length otherwise)." },
                        selectionMode = new { type = "string", description = "The grid's SelectionSettings.Type at the time the call ran — 'Single' or 'Multiple'. When Single, only the first index from rowIndexes/where is honored." },
                        fromRowIndexes = new { type = "array", items = new { type = "integer" }, description = "0-based row indexes resolved from the positional argument." },
                        fromWhere = new { type = "array", items = new { type = "integer" }, description = "0-based row indexes resolved from the field-value argument." },
                        appliedIndexes = new { type = "array", items = new { type = "integer" }, description = "Final unique 0-based view indexes forwarded to the grid's selection API. For SelectionType.Single this is a single-element array." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearSelection",
                Description = "Clear all row and cell selections",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearCellSelection",
                Description = "Clear cell selection only. Row selection is left unchanged; no cell selection is cleared when SelectionSettings.Mode is Row.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "setCellValue",
            Description = "Set one cell by primary key and field, or apply a list of cell updates by primary key. The grid must define a primary-key column.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        key = new { description = "Row key/ID" },
                        fieldName = new { type = "string", description = "Column field name" },
                    value = new { description = "New cell value; null clears a nullable cell." },
                    cellUpdates = new
                    {
                        type = "array",
                        description = "Alternative bulk form. Each update has primaryKeyValue, columnField, and newValue.",
                        items = new
                        {
                            type = "object",
                            properties = new
                            {
                                primaryKeyValue = new { description = "Primary key value identifying the row." },
                                columnField = new { type = "string" },
                                newValue = new { description = "New cell value; null is allowed." }
                            },
                            required = _reqPrimaryKeyValueColumnField
                        }
                    }
                },
                required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        updatedCount = new { type = "integer", description = "Number of cell values submitted for update." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "setRowData",
                Description = "Update entire row data by key and rowData object. Set preventDataUpdate=true to refresh the UI without updating the backing data source.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        key = new { description = "Row key/ID" },
                        rowData = new { type = "object", description = "Updated row data" },
                        preventDataUpdate = new { type = "boolean", description = "When true, refreshes the grid UI without updating the data source." }
                    },
                    required = _reqKeyRowData
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        preventDataUpdate = new { type = "boolean", description = "Whether the data source was intentionally left unchanged." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "crudLifecycle",
                Description = "Execute a CRUD lifecycle action on the grid. Supported actions are startEdit, endEdit, closeEdit, addRecord, and deleteRecord.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new
                        {
                            type = "string",
                            @enum = _enumCrudAction,
                            description = "CRUD lifecycle action to execute. addRecord opens the blank add form when data is omitted, or inserts the supplied record. deleteRecord deletes the selected row when fieldName/data are omitted, or deletes the supplied record."
                        },
                        fieldName = new { type = "string", description = "Primary key column field name; supply together with data to delete a specific record." },
                        data = new { type = "object", description = "Optional record data for addRecord; when omitted, opens the blank add form. For deleteRecord, supply with fieldName to delete that record." },
                        index = new { type = "integer", description = "Optional zero-based insertion index for addRecord when data is supplied." }
                    },
                    required = _reqAction
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        action = new { type = "string" },
                        message = new { type = "string" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "deleteRow",
                Description = "Delete a row from the current data view using its 1-based data-row index. Requires AllowDeleting=true and exactly one primary-key column.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { rowIndex = new { type = "integer", minimum = 1 } },
                    required = _reqRowIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        rowIndex = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false, requiresConfirmation = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "excelExport",
                Description = "Export grid data as xlsx or csv. By default the browser downloads the file; set asMemoryStream=true to return the exported bytes as base64 in the tool result instead.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fileName = new { type = "string", description = "Export base name (extension is auto-attached). Directory separators, control chars and Windows-reserved filename chars (<>:\"\\|/?*) are stripped/replaced. Max 120 chars after sanitisation." },
                        asMemoryStream = new { type = "boolean", description = "When true, return the exported file as base64 instead of triggering a browser download. Defaults to false." },
                        format = new
                        {
                            type = "string",
                            @enum = _enumExportFormat,
                            description = "Output format. 'xlsx' (default; Office Open XML is internally ZIP-structured XML but the file extension pins the browser to recognise it as a spreadsheet) or 'csv'. Aliases: 'xls'/'excel'/'xlsb'→xlsx, 'text'/'tsv'→csv."
                        }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        requestedFileName = new { type = "string", description = "The fileName argument before sanitisation/ext." },
                        resolvedFileName = new { type = "string", description = "The exact filename passed to the browser download — always ends with .xlsx or .csv." },
                        format = new { type = "string", description = "The format that was actually used (xlsx or csv)." },
                        contentType = new { type = "string", description = "MIME type the browser uses for the download: application/vnd.openxmlformats-officedocument.spreadsheetml.sheet for xlsx, text/csv for csv." },
                        delivery = new { type = "string", description = "'browserDownload' (default) or 'base64' when asMemoryStream=true." },
                        data = new { type = "string", description = "Base64-encoded file bytes; present only when asMemoryStream=true." },
                        note = new { type = "string", description = "Human-readable explanation of how the file content is structured (defuses the 'why is this a zip?' surprise)." }
                    }
                },
                Annotations = new { ReadOnlyHint = true } // Export is read-only; it never mutates grid state.
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "pdfExport",
                Description = "Export the grid to PDF. Requires AllowPdfExport=true. Optional settings control page selection, headers, command columns, and column overflow.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        exportType = new
                        {
                            type = "string",
                            @enum = _enumExportType,
                            description = "Export all pages or only the current page. Defaults to the PDF exporter setting."
                        },
                        includeHeaderRow = new { type = "boolean", description = "Include the grid column header row." },
                        includeCommandColumn = new { type = "boolean", description = "Include command columns in the PDF." },
                        allowHorizontalOverflow = new { type = "boolean", description = "Allow columns to overflow horizontally across PDF pages." },
                        disableAutoFitWidth = new { type = "boolean", description = "Use grid column widths instead of automatically fitting them to the PDF page." },
                        isRepeatHeader = new { type = "boolean", description = "Repeat the PDF header on each page." },
                        isThemeEnabled = new { type = "boolean", description = "Enable the grid theme in the exported PDF." }
                    },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        exportType = new { type = "string" },
                        delivery = new { type = "string", description = "The PDF is downloaded by the browser." }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "preventRender",
                Description = "Enable or disable grid rendering. Omitting preventRender matches PreventRender's default and suppresses rendering; set false to re-enable it.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        preventRender = new { type = "boolean", description = "True suppresses rendering; false re-enables it. Defaults to true." }
                    },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        preventRender = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "groupColumn",
                Description = "Group grid by a column",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columnName = new { type = "string", description = "Column field name to group by" }
                    },
                    required = _reqColumnName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 4 EXPANSION (17/27): toolbar-style search — filters rows
            webMcpTools.Add(new WebMcpTool
            {
                Name = "searchText",
                Description = "Toolbar-style search that filters rows to match the supplied search key (uses GridSearchSettings).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        searchString = new { type = "string", description = "Search text to filter rows by. Pass an empty string to clear the active search." }
                    },
                    required = _reqSearchString
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        searchString = new { type = "string" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 4 EXPANSION (18/27): find & highlight — paints matching cells and walks matches
            webMcpTools.Add(new WebMcpTool
            {
                Name = "findAndHighlight",
                Description = "Find and Highlight: highlights every cell containing 'findKey' (no row filtering), or navigates the active match. action='highlight' (default) applies the key, 'reset' clears highlights, 'next'/'previous' jumps to the next/previous match, 'goto' activates a specific match by 1-based index.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        findKey = new { type = "string", description = "Text to highlight. Required for action='highlight'; ignored for the other actions. Pass empty string to clear the active highlight." },
                        action = new { type = "string", description = "Navigation action. One of: 'highlight' (default), 'reset', 'next', 'previous', 'goto'." },
                        matchNumber = new { type = "integer", description = "1-based match index. Required when action='goto'; ignored otherwise." }
                    },
                    required = _reqFindKeyAction
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        action = new { type = "string" },
                        findKey = new { type = "string" },
                        matchNumber = new { type = "integer", description = "1-based index of the match that was activated (best-effort; the highlight feature does not currently echo the resolved index back)." }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            // PHASE 5 EXPANSION (19/41) — SortColumnAsync(field, direction): single-column sort
            webMcpTools.Add(new WebMcpTool
            {
                Name = "sortByColumn",
                Description = "Sort the grid by a single column. direction must be 'Ascending' or 'Descending' (case-insensitive; defaults to 'Ascending' when omitted or unrecognised). multiSort=true appends to existing sorts when the grid has AllowMultiSorting=true; otherwise it replaces.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldName = new { type = "string", description = "Field name of the column to sort by (matches GridColumn.Field)." },
                        direction = new
                        {
                            type = "string",
                            @enum = _enumSortDirection,
                            description = "Sort direction. Case-insensitive; defaults to 'Ascending' when omitted or unrecognised."
                        },
                        multiSort = new { type = "boolean", description = "When true, appends to existing sorts (requires AllowMultiSorting=true). Default false (replaces the current sort)." }
                    },
                    required = _reqFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        message = new { type = "string", description = "Human-readable description of the applied sort." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (20/41) — SortColumnsAsync(List<SortColumn>): multi-column sort
            webMcpTools.Add(new WebMcpTool
            {
                Name = "sortColumns",
                Description = "Sort the grid by multiple columns at once (additive — keeps any existing sorts). Each entry in the columns array must list its field and direction. direction uses the same vocabulary as sortByColumn ('Ascending' or 'Descending').",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columns = new
                        {
                            type = "array",
                            description = "List of columns to sort by. Each item must have 'field' (string) and 'direction' (Ascending|Descending).",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    field = new { type = "string", description = "Field name of the column to sort by." },
                                    direction = new { type = "string", @enum = _enumSortDirection, description = "Sort direction." }
                                },
                                required = _reqFieldDirection
                            }
                        }
                    },
                    required = _reqColumns
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        appliedCount = new { type = "integer", description = "Number of columns forwarded to the sort module." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (20/41) — SortColumnsAsync(List<SortColumn>, bool clearPreviousSort)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "sortColumnsWithClear",
                Description = "Sort by multiple columns, optionally clearing any prior sorts first. Use clearPreviousSort=true to replace the entire sort stack; false to append.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columns = new
                        {
                            type = "array",
                            description = "Columns to sort by (same shape as sortColumns).",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    field = new { type = "string" },
                                    direction = new { type = "string", @enum = _enumSortDirection }
                                },
                                required = _reqFieldDirection
                            }
                        },
                        clearPreviousSort = new { type = "boolean", description = "When true, clears existing sorts before applying this list." }
                    },
                    required = _reqColumns
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        cleared = new { type = "boolean", description = "Whether the prior sort stack was cleared before applying." },
                        appliedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (21/41) — ClearSortingAsync(): clear all sorts
            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearSorting",
                Description = "Clear all currently active sort columns and restore the grid to its natural order. Does not affect grouped columns.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (22/41) — ClearSortingAsync(List<string>): clear sort on specific columns
            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearSortingByFields",
                Description = "Clear sort only on the specified column field names. Columns not in the list keep their sort direction (if any). Useful for multi-sort scenarios where you want to drop a single sort level.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldNames = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Field names of sorted columns to clear."
                        }
                    },
                    required = _reqFieldNames
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        clearedCount = new { type = "integer", description = "Number of field names forwarded to the sort module." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (23/41) — ClearFilteringAsync: clear all filters, or specific fields
            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearFiltering",
                Description = "Clear column filters. Pass 'fieldNames' (array) to clear specific columns only; omit it to clear every active filter at once.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldNames = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Optional list of column field names whose filters should be cleared. Omit (or pass null/empty) to clear all filters."
                        }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        scope = new { type = "string", description = "'all' when fieldNames was omitted/empty; 'specific' when a list was provided." },
                        fieldCount = new { type = "integer", description = "Number of fields forwarded for clearing (0 when scope='all')." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (24/41) — SelectCellAsync: select one cell by (row, col)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "selectCell",
                Description = "Select a single cell in the grid. Requires the grid's SelectionSettings.Mode to be SelectionMode.Cell (or Both). Coordinates are 1-based (row 1 = first row, col 1 = first visible column).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndex = new { type = "integer", description = "1-based row index against the visible data rows." },
                        cellIndex = new { type = "integer", description = "1-based column index against the visible data columns." },
                        toggle = new { type = "boolean", description = "Optional. When true, deselects the cell if it was already selected instead of forcing it on. Default false." }
                    },
                    required = _reqRowIndexCellIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        applied = new
                        {
                            type = "object",
                            description = "The 0-based (rowIndex, cellIndex) tuple forwarded to the grid.",
                            properties = new
                            {
                                row = new { type = "integer" },
                                cell = new { type = "integer" }
                            }
                        },
                        selectionMode = new { type = "string", description = "'Cell', 'Row', or 'Both' — the grid's SelectionSettings.Mode at execution time." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (25/41) — SelectCellsAsync: select many cells in one call
            webMcpTools.Add(new WebMcpTool
            {
                Name = "selectCells",
                Description = "Select multiple cells in the grid by their (row, column) 1-based indexes. Each entry is { rowIndex, cellIndex }. Empty or invalid indexes are rejected with InvalidArgument. Requires SelectionMode.Cell or Both.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowCellIndexes = new
                        {
                            type = "array",
                            description = "Cells to select. Each item must include rowIndex and cellIndex (both 1-based).",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    rowIndex = new { type = "integer" },
                                    cellIndex = new { type = "integer" }
                                },
                                required = _reqRowIndexCellIndex
                            }
                        }
                    },
                    required = _reqRowCellIndexes
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        appliedCount = new { type = "integer", description = "Number of cells forwarded to the grid's selection module." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 5 EXPANSION (26/41) — SelectCellsByRangeAsync: select a rectangular range
            webMcpTools.Add(new WebMcpTool
            {
                Name = "selectCellsByRange",
                Description = "Select a rectangular range of cells from startIndex to endIndex (1-based, inclusive). Omit endIndex to select only the startIndex cell. Works in SelectionMode.Cell or Both.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        startRowIndex = new { type = "integer", description = "1-based row index of the top-left anchor." },
                        startCellIndex = new { type = "integer", description = "1-based column index of the top-left anchor." },
                        endRowIndex = new { type = "integer", description = "Optional 1-based row index of the bottom-right corner. Omit (or null) to select a single cell." },
                        endCellIndex = new { type = "integer", description = "Optional 1-based column index of the bottom-right corner. Omit (or null) to select a single cell." }
                    },
                    required = _reqStartRowIndexStartCellIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        startIndex = new
                        {
                            type = "object",
                            description = "0-based (rowIndex, cellIndex) tuple used as the selection anchor.",
                            properties = new
                            {
                                row = new { type = "integer" },
                                cell = new { type = "integer" }
                            }
                        },
                        endIndex = new
                        {
                            type = "object",
                            description = "0-based (rowIndex, cellIndex) tuple of the bottom-right corner. Null when endRowIndex/endCellIndex were omitted.",
                            properties = new
                            {
                                row = new { type = "integer" },
                                cell = new { type = "integer" }
                            }
                        }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            //PHASE 5 EXPANSION(27 / 41) — GetSelectedRowCellIndexesAsync: read selected cell indexes

           webMcpTools.Add(new WebMcpTool
           {
               Name = "getSelectedRowCellIndexes",
               Description = "Read the indexes of currently selected cells (returns empty list when grid is in SelectionMode.Row, no rows, or selection is disabled). Each item is { rowIndex, cellIndex } (1-based).",
               InputSchema = new
               {
                   type = "object",
                   properties = new { },
                   required = _emptyRequired
           },
               OutputSchema = new
               {
                   type = "object",
                   properties = new
                   {
                       count = new { type = "integer", description = "Number of selected cell indexes returned." },
                       indexes = new
                       {
                           type = "array",
                           description = "Selected cells as 1-based { rowIndex, cellIndex } pairs.",
                           items = new
                           {
                               type = "object",
                               properties = new
                               {
                                   rowIndex = new { type = "integer" },
                                   cellIndex = new { type = "integer" }
                               }
                           }
                       },
                       selectionMode = new { type = "string", description = "The grid's SelectionSettings.Mode at execution time. Empty list when 'Row'." }
                   }
               },
               Annotations = new { ReadOnlyHint = true }
           });

            // PHASE 6 EXPANSION (28/41) — SelectRowsByRangeAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "selectRowsByRange",
                Description = "Select a contiguous range of rows from startIndex to endIndex (1-based, inclusive). Omit endIndex to select a single row at startIndex. Use the same selection module that the host app's keyboard/mouse drag uses.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        startIndex = new { type = "integer", description = "1-based row index of the range start (inclusive)." },
                        endIndex = new { type = "integer", description = "Optional 1-based row index of the range end (inclusive). Omit to select just the startIndex row." }
                    },
                    required = _reqStartIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        appliedRange = new
                        {
                            type = "object",
                            description = "The 0-based inclusive range forwarded to the grid's selection module.",
                            properties = new
                            {
                                start = new { type = "integer" },
                                end = new { type = "integer" }
                            }
                        }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (29/41) — SaveCellAsync (Batch mode cell commit)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "saveCell",
                Description = "Commit the value of the cell currently being edited. Only effective when GridEditSettings.Mode is Batch and AllowEditing is true. Does not save to the data source; only flushes the cell into grid state.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (30/41) — EditCellAsync(idx, field)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "editCell",
                Description = "Put a specific cell into edit mode by its row index and field name. Only works in EditMode.Batch or EditMode.Cell with AllowEditing=true. The CellEditing event fires before the editor opens.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        index = new { type = "integer", description = "0-based row index in the current view." },
                        fieldName = new { type = "string", description = "Field name of the column to edit." }
                    },
                    required = _reqRowIndexFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (31/41) — UngroupColumnAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "ungroupColumn",
                Description = "Remove a column from the group set by its field name. No-op if the column isn't currently grouped.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columnName = new { type = "string", description = "Field name of the column to ungroup." }
                    },
                    required = _reqColumnName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (32/41) — ExpandAllGroupAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "expandAllGroups",
                Description = "Expand every grouped row in the grid. Triggers DataProcess when EnableLazyLoading or virtualization is enabled; otherwise flips each group's IsExpand flag.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (33/41) — CollapseAllGroupAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "collapseAllGroups",
                Description = "Collapse every expanded grouped row in the grid. Resets to the first page when paging is enabled. Honors EnableLazyLoading by re-fetching data if it is on.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (34/41) — ClearGroupingAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearGrouping",
                Description = "Remove every column from the group set, restoring the grid to its un-grouped state.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        removedCount = new { type = "integer", description = "Number of grouped columns that were removed." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (35/41) — ShowColumnsAsync / ShowColumnAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "showColumns",
                Description = "Make hidden columns visible. By default column names are matched against GridColumn.HeaderText; pass showBy='field' to match against GridColumn.Field. Pass a single-element array to show a single column.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columnNames = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "One or more column names to show."
                        },
                        showBy = new
                        {
                            type = "string",
                            @enum = _enumColumnLookup,
                            description = "How to interpret columnNames. Default 'HeaderText' (matches the GridColumn.HeaderText property)."
                        }
                    },
                    required = _reqColumnNames
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        showBy = new { type = "string" },
                        requestedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (36/41) — HideColumnsAsync / HideColumnAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "hideColumns",
                Description = "Hide visible columns. By default column names are matched against GridColumn.HeaderText; pass hideBy='field' to match against GridColumn.Field.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columnNames = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "One or more column names to hide."
                        },
                        hideBy = new
                        {
                            type = "string",
                            @enum = _enumColumnLookup,
                            description = "How to interpret columnNames. Default 'HeaderText'."
                        }
                    },
                    required = _reqColumnNames
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        hideBy = new { type = "string" },
                        requestedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (37/41) — ReorderColumnAsync (single column)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "reorderColumn",
                Description = "Move a column to the location of another column. Both arguments are column field names (GridColumn.Field). Requires AllowReordering=true.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fromFieldName = new { type = "string", description = "Field name of the column to move." },
                        toFieldName = new { type = "string", description = "Field name of the column whose position the moved column should take." }
                    },
                    required = _reqFromFieldNameToFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (38/41) — ReorderColumnsAsync (multi)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "reorderColumns",
                Description = "Move one or more columns to the location of a destination column. Pass fromFieldNames as an array of field names. Requires AllowReordering=true.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fromFieldNames = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Field names of the columns to move."
                        },
                        toFieldName = new { type = "string", description = "Field name of the destination column." }
                    },
                    required = _reqFromFieldNamesToFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        movedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (39/41) — ReorderColumnByIndexAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "reorderColumnByIndex",
                Description = "Move the column at fromIndex to toIndex by position. Requires AllowReordering=true. Calling this multiple times is not idempotent — indexes shift after each reorder.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fromIndex = new { type = "integer", description = "0-based source index." },
                        toIndex = new { type = "integer", description = "0-based destination index." }
                    },
                    required = _reqFromIndexToIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (40/41) — ReorderColumnByTargetIndexAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "reorderColumnByTargetIndex",
                Description = "Move the column identified by fieldName to a specific index. Requires AllowReordering=true. Idempotent — calling it twice with the same args is a no-op.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldName = new { type = "string", description = "Field name of the column to move." },
                        toIndex = new { type = "integer", description = "0-based destination index." }
                    },
                    required = _reqFieldNameToIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (41/41) — AutoFitColumnsAsync + AutoFitColumnAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "autoFitColumns",
                Description = "Auto-fit columns to their content width. Pass fieldNames as a non-empty array to fit specific columns; omit it (or pass an empty array) to fit every column. Hidden columns are ignored. If EnablePersistence is true, the autofit state is persisted.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldNames = new
                        {
                            type = "array",
                            items = new { type = "string" },
                            description = "Optional list of column field names to autofit. Omit (or pass an empty array) to autofit every visible column."
                        }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        scope = new { type = "string", description = "'all' when fieldNames was omitted/empty; 'specific' when a list was provided." },
                        fittedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (42/41) — OpenColumnChooserAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "openColumnChooser",
                Description = "Open the column chooser dialog at the supplied screen coordinates. Requires ShowColumnPicker=true. Pass both x and y to anchor it; omit both to let the chooser open at its default position.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        x = new { type = "number", description = "Optional X position in pixels. Must be paired with y." },
                        y = new { type = "number", description = "Optional Y position in pixels. Must be paired with x." }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        position = new
                        {
                            type = "object",
                            description = "Null when both x and y were omitted; otherwise the anchor passed to the chooser.",
                            properties = new
                            {
                                x = new { type = "number" },
                                y = new { type = "number" }
                            }
                        }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 6 EXPANSION (43/41) — GetHiddenColumnsAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "getHiddenColumns",
                Description = "List the columns currently hidden in the grid (GridColumn.Visible == false).",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        count = new { type = "integer" },
                        columns = new { type = "array", description = "Hidden column definitions." }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            // PHASE 6 EXPANSION (44/41) — GetColumnIndexByFieldAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "getColumnIndexByField",
                Description = "Get the 0-based index of the column identified by its field name. Returns -1 if no match.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldName = new { type = "string", description = "Field name to look up." }
                    },
                    required = _reqFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        index = new { type = "integer" },
                        fieldName = new { type = "string" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            // PHASE 6 EXPANSION (45/41) — GetUidByColumnFieldAsync
            webMcpTools.Add(new WebMcpTool
            {
                Name = "getUidByColumnField",
                Description = "Resolve a column's unique identifier (UID) by its field name. Useful for tools that accept uid instead of field.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldName = new { type = "string", description = "Field name to look up." }
                    },
                    required = _reqFieldName
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        uid = new { type = "string", description = "The column's UID, or empty string when no match was found." }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            // PHASE 6 EXPANSION (46/41) — Print / Copy / Paste / Spinner / Refresh / Focus / Scroll
            // (compact entry for the rest of the UX/state surface)
            webMcpTools.Add(new WebMcpTool
            {
                Name = "printGrid",
                Description = "Print the grid. Hides the pager during printing.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "copyToClipboard",
                Description = "Copy the selected rows or cells into the clipboard. Pass withHeader=true to include column header names; null/omitted uses the grid's configured default.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        withHeader = new { type = "boolean", description = "Optional. true copies with header names; false copies raw cells; null uses the grid's default." }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        withHeader = new { type = "boolean", description = "Echoed withHeader argument (resolved to false when null)." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "pasteFromClipboard",
                Description = "Paste clipboard content into the grid's currently selected cells. Follows the grid's existing editing/validation/clipboard behaviour.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "showSpinner",
                Description = "Show the grid's spinner overlay. No-op if the grid was configured without a spinner.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "hideSpinner",
                Description = "Hide the grid's spinner overlay. No-op if the grid was configured without a spinner.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "refreshGrid",
                Description = "Refresh the grid. Pass isModelRefresh=true to reinitialize when column/aggregate definitions have changed dynamically; false (default) refreshes the UI only.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        isModelRefresh = new { type = "boolean", description = "When true, reinitializes the grid (use when columns/aggregates have been added or removed at runtime)." }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        isModelRefresh = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "refreshColumns",
                Description = "Refresh only the column definitions. Useful after column properties are mutated externally and the UI hasn't picked them up.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "refreshHeader",
                Description = "Refresh only the header cells.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "focusGrid",
                Description = "Move keyboard focus to the grid element. No-op if the grid hasn't finished rendering.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "scrollIntoView",
                Description = "Scroll the grid so that the supplied row/column cell is visible. Both indexes are 1-based; pass -1 to skip a direction. rowHeight is only consulted by row virtualization.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columnIndex = new { type = "integer", description = "1-based column index to scroll into view, or -1 to skip." },
                        rowIndex = new { type = "integer", description = "1-based row index to scroll into view, or -1 to skip." },
                        rowHeight = new { type = "integer", description = "Optional row offsetHeight used by row virtualization to compute scroll offsets." }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        columnIndex = new { type = "integer" },
                        rowIndex = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (57/64) — Detail rows: expand/collapse by data
            webMcpTools.Add(new WebMcpTool
            {
                Name = "expandCollapseDetailRow",
                Description = "Toggle (expand if collapsed, collapse if expanded) the detail row for a specific record. Pass 'data' when the supplied TValue reference still exists in the grid's current view; pass 'fieldName' + 'value' when matching against a field (e.g. EmployeeID equals 1).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        data = new
                        {
                            type = "object",
                            description = "Record whose detail row should be toggled. Use when the TValue reference is still in the current view."
                        },
                        fieldName = new
                        {
                            type = "string",
                            description = "Field name to look up against the current view. Required when 'data' is omitted."
                        },
                        value = new
                        {
                            type = "object",
                            description = "Value the field must equal. Required when 'data' is omitted."
                        }
                    },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        matchMode = new { type = "string", description = "'data' when the data arg was supplied, 'field' when fieldName+value were used, 'none' when neither matched." }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (58/64) — Detail rows: expand all
            webMcpTools.Add(new WebMcpTool
            {
                Name = "expandAllDetailRows",
                Description = "Expand every detail row currently in the grid (including those already expanded).",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (59/64) — Detail rows: collapse all
            webMcpTools.Add(new WebMcpTool
            {
                Name = "collapseAllDetailRows",
                Description = "Collapse every detail row currently in the grid (including those already collapsed).",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
            },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (60/64) — Cell merging: single
            webMcpTools.Add(new WebMcpTool
            {
                Name = "mergeCells",
                Description = "Merge a rectangular region of cells starting at the supplied top-left cell. rowIndex/columnIndex are 0-based against visible data rows/leaf columns. rowSpan/columnSpan default to 1 and must be >= 1. Merges are computed per current view; results change after paging, sort, filter, group expand/collapse, column show/hide/reorder, frozen changes, or virtualization updates.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndex = new { type = "integer", description = "0-based row index of the anchor cell." },
                        columnIndex = new { type = "integer", description = "0-based column index of the anchor cell among visible leaf columns." },
                        rowSpan = new { type = "integer", description = "Number of rows to span (>=1). Default 1." },
                        columnSpan = new { type = "integer", description = "Number of columns to span (>=1). Default 1." }
                    },
                    required = _reqRowIndexColumnIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        rowIndex = new { type = "integer" },
                        columnIndex = new { type = "integer" },
                        rowSpan = new { type = "integer" },
                        columnSpan = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (61/64) — Cell merging: bulk
            webMcpTools.Add(new WebMcpTool
            {
                Name = "mergeCellsBulk",
                Description = "Merge multiple rectangular regions in one combined operation. Each entry must include rowIndex + columnIndex (both 0-based); rowSpan/columnSpan default to 1. The grid validates every entry first and aborts the whole batch on any rule violation (no partial application).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        infos = new
                        {
                            type = "array",
                            description = "List of merge requests. Each request must include rowIndex, columnIndex, and (optionally) rowSpan, columnSpan.",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    rowIndex = new { type = "integer" },
                                    columnIndex = new { type = "integer" },
                                    rowSpan = new { type = "integer" },
                                    columnSpan = new { type = "integer" }
                                },
                                required = _reqRowIndexColumnIndex
                            }
                        }
                    },
                    required = _reqInfos
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        appliedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (62/64) — Cell merging: unmerge single
            webMcpTools.Add(new WebMcpTool
            {
                Name = "unmergeCells",
                Description = "Remove the merged region anchored at the supplied top-left cell (0-based). Non-anchor coordinates are a no-op. After unmerge, all covered cells become individually navigable again.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndex = new { type = "integer", description = "0-based row index of the merged region's anchor cell." },
                        columnIndex = new { type = "integer", description = "0-based column index of the merged region's anchor cell." }
                    },
                    required = _reqRowIndexColumnIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (63/64) — Cell merging: unmerge bulk
            webMcpTools.Add(new WebMcpTool
            {
                Name = "unmergeCellsBulk",
                Description = "Remove multiple merged regions in one combined operation. Each entry must include rowIndex + columnIndex (0-based) of an existing merged region's anchor cell. Non-anchor entries are skipped silently (the rest still apply).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        infos = new
                        {
                            type = "array",
                            description = "List of unmerge requests keyed by anchor cell coordinates.",
                            items = new
                            {
                                type = "object",
                                properties = new
                                {
                                    rowIndex = new { type = "integer" },
                                    columnIndex = new { type = "integer" }
                                },
                                required = _reqRowIndexColumnIndex
                            }
                        }
                    },
                    required = _reqInfos
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        requestedCount = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            // PHASE 7 EXPANSION (64/64) — Cell merging: unmerge all
            webMcpTools.Add(new WebMcpTool
            {
                Name = "unmergeAll",
                Description = "Remove every merged region in the current view. Use this to reset merges before re-applying based on a new view (after paging, sort, or filter changes).",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getPersistData",
                Description = "Get the grid's persisted state as a JSON string that can be stored and later restored by the host application.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Serialized grid state." }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getPrimaryKeyFieldNames",
                Description = "List the field names of columns configured as primary keys.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldNames = new { type = "array", items = new { type = "string" } },
                        count = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getRowIndexByPrimaryKey",
                Description = "Find a row's index by its primary-key value. By default, searches the current view; set searchAcrossRecords=true to search the full data set when supported by the data source.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        value = new { description = "Primary-key value to match." },
                        searchAcrossRecords = new { type = "boolean", description = "Search beyond the current view. Defaults to false." }
                    },
                    required = _reqValue
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndex = new { type = "integer", description = "Zero-based row index, or -1 when no matching row is found." },
                        found = new { type = "boolean" },
                        searchAcrossRecords = new { type = "boolean" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getColumns",
                Description = "Get the grid's column definitions, including visible and hidden columns.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columns = new { type = "array", description = "Column definitions." },
                        count = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getForeignKeyColumns",
                Description = "List columns configured with foreign-key data.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        columns = new { type = "array", description = "Foreign-key column definitions." },
                        count = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getColumnIndexByUid",
                Description = "Get the zero-based index of a column by its unique identifier (UID).",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        uid = new { type = "string", description = "Column UID to look up." }
                    },
                    required = _reqUid
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        uid = new { type = "string" },
                        index = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "clearRowSelection",
                Description = "Clear selected rows without changing cell selection state.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new { success = new { type = "boolean" } }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "undo",
                Description = "Undo the most recent recorded batch edit operation. This is a no-op when undo/redo is disabled or the undo stack is empty.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new { success = new { type = "boolean" } }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "redo",
                Description = "Redo the most recently undone batch edit operation. This is a no-op when undo/redo is disabled or the redo stack is empty.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { },
                    required = _emptyRequired
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new { success = new { type = "boolean" } }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getColumnByUid",
                Description = "Get a column definition by its unique UID. Returns null when no matching column exists.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { uid = new { type = "string", description = "Column UID." } },
                    required = _reqUid
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new { column = new { type = "object" } }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getColumnFieldNames",
                Description = "Get the field names for all columns configured in the grid.",
                InputSchema = new { type = "object", properties = new { }, required = _emptyRequired },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fieldNames = new { type = "array", items = new { type = "string" } },
                        count = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getCellValue",
                Description = "Read the formatted value of a cell in the current view. rowIndex and columnIndex are 1-based positions among data rows and visible data columns.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndex = new { type = "integer", description = "1-based data-row position in the current view." },
                        columnIndex = new { type = "integer", description = "1-based visible data-column position." }
                    },
                    required = _reqRowIndexColumnIndex
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        rowIndex = new { type = "integer" },
                        columnIndex = new { type = "integer" },
                        value = new { description = "Formatted cell value, or null when the position is out of range." }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "batchEditWorkflow",
                Description = "Inspect staged batch changes and dirty state, update one staged row, or apply a supplied BatchChanges object. Requires batch edit mode for all actions. Actions: inspect, isDirty, updateRow, apply.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", @enum = _enumBatchEditAction },
                        rowData = new { type = "object", description = "Record to stage; required for updateRow." },
                        batchChanges = new
                        {
                            type = "object",
                            description = "Changes to stage; required for apply.",
                            properties = new
                            {
                                addedRecords = new { type = "array", items = new { type = "object" } },
                                changedRecords = new { type = "array", items = new { type = "object" } },
                                deletedRecords = new { type = "array", items = new { type = "object" } }
                            }
                        }
                    },
                    required = _reqAction
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        action = new { type = "string" },
                        isDirty = new { type = "boolean" },
                        changes = new { type = "object" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "updateGridData",
                Description = "Update a cell or replace a row without entering edit mode. Actions: cell requires rowIndex, fieldName, value; row requires rowIndex and rowData. rowIndex is 1-based in the current view.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", @enum = _enumGridDataAction },
                        rowIndex = new { type = "integer", description = "1-based position in current view." },
                        fieldName = new { type = "string", description = "Field to update for action=cell." },
                        value = new { description = "New cell value for action=cell; null is allowed." },
                        rowData = new { type = "object", description = "Replacement record for action=row." }
                    },
                    required = _reqAction
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new { success = new { type = "boolean" }, action = new { type = "string" } }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "reorderRow",
                Description = "Move a row to another position. fromIndex and toIndex are 1-based positions in the current view.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        fromIndex = new { type = "integer" },
                        toIndex = new { type = "integer" }
                    },
                    required = _reqFromIndexToIndex
                },
                OutputSchema = new { type = "object", properties = new { success = new { type = "boolean" } } },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "persistGridState",
                Description = "Read persisted grid state, load a supplied serialized state, or reset to declarative defaults. Actions: get, set, reset. Reset discards persisted state.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", @enum = _enumPersistAction },
                        properties = new { type = "string", description = "Serialized grid state; required for action=set." }
                    },
                    required = _reqAction
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        action = new { type = "string" },
                        data = new { type = "string", description = "Serialized grid state when action=get." }
                    }
                },
                Annotations = new { ReadOnlyHint = false, requiresConfirmation = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "updatePageSize",
                Description = "Change the page size, or recalculate virtual page size from container height and row height. Use action=fixed with pageSize, or action=viewport with height and rowHeight.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", @enum = _enumPageSizeAction },
                        pageSize = new { type = "integer", description = "Positive page size; required for action=fixed." },
                        height = new { type = "integer", description = "Container height; required for action=viewport." },
                        rowHeight = new { type = "integer", description = "Positive row height; required for action=viewport." }
                    },
                    required = _reqAction
                },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        success = new { type = "boolean" },
                        action = new { type = "string" },
                        pageSize = new { type = "integer" }
                    }
                },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "updateExternalMessage",
                Description = "Set the external message displayed by the grid pager.",
                InputSchema = new
                {
                    type = "object",
                    properties = new { message = new { type = "string" } },
                    required = _reqMessage
                },
                OutputSchema = new { type = "object", properties = new { success = new { type = "boolean" } } },
                Annotations = new { ReadOnlyHint = false }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "undoRedoHistory",
                Description = "Perform a bulk undo/redo or clear both history stacks. Actions: undoAll, redoAll, clear. Clearing permanently discards undo/redo history.",
                InputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        action = new { type = "string", @enum = _enumUndoRedoAction }
                    },
                    required = _reqAction
                },
                OutputSchema = new { type = "object", properties = new { success = new { type = "boolean" }, action = new { type = "string" } } },
                Annotations = new { ReadOnlyHint = false, requiresConfirmation = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "getEditState",
                Description = "Inspect the current editor's EditContext without exposing the context object itself. Returns whether a context exists, whether fields are modified, and current validation messages.",
                InputSchema = new { type = "object", properties = new { }, required = _emptyRequired },
                OutputSchema = new
                {
                    type = "object",
                    properties = new
                    {
                        hasContext = new { type = "boolean" },
                        isModified = new { type = "boolean" },
                        validationMessages = new { type = "array", items = new { type = "string" } }
                    }
                },
                Annotations = new { ReadOnlyHint = true }
            });

            webMcpTools.Add(new WebMcpTool
            {
                Name = "isDirty",
                Description = "Check whether the grid has pending batch edits. Returns false when batch edit mode is not enabled.",
                InputSchema = new { type = "object", properties = new { }, required = _emptyRequired },
                OutputSchema = new
                {
                    type = "object",
                    properties = new { isDirty = new { type = "boolean" } },
                    required = _reqIsDirty
                },
                Annotations = new { ReadOnlyHint = true }
            });
        }
    }
}
