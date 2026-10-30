using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Write-handlers partial — implements state-changing and data-operation workflows.
    ///
    /// All handlers:
    ///   • Validate required string arguments via <see cref="DictionaryExtensions.ValidateRequired"/>
    ///   • Re-throw <see cref="WebMcpException"/> unchanged so the
    ///     dispatcher surfaces <c>InvalidArgument</c> errors verbatim.
    ///   • Wrap unexpected failures as <see cref="WebMcpException"/>
    ///     (<c>ExecutionFailed</c>) with the original cause as
    ///     <see cref="Exception.InnerException"/>.
    /// </summary>
    public partial class WebMcpAdapter<TValue>
    {
        private static readonly System.Text.Json.JsonSerializerOptions _caseInsensitiveJsonSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        // WRITE HANDLERS — annotate readOnlyHint=false in the schema (see Catalog partial)

        private async Task<object> HandleSortByColumn(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("sortByColumn", "columnName");
                var columnName = args.GetString("columnName");
                var directionStr = args.GetString("direction");
                var multiSort = args.GetBool("multiSort");

                // Parse string direction to SortDirection enum
                SortDirection direction = directionStr?.Equals("Descending", StringComparison.OrdinalIgnoreCase) ?? false
                    ? SortDirection.Descending
                    : SortDirection.Ascending;

                await _parent.SortColumnAsync(columnName!, direction, multiSort).ConfigureAwait(true);

                return new
                {
                    success = true,
                    message = $"Sorted by {columnName} ({direction})"
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "sortByColumn",
                    "Failed to sort by column", ex);
            }
        }

        private async Task<object> HandleFilterByColumn(Dictionary<string, object> args)
        {
            try
            {
                // 'filterOperator' is OPTIONAL — omitted falls back to a case-insensitive
                // 'contains' so natural-language agents ("filter where City contains Lon")
                // still succeed without needing to specify the operator keyword.
                args.ValidateRequired("filterByColumn", "fieldName");
                var fieldName = args.GetString("fieldName")!;
                if (!args.TryGetValue("value", out var value))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "filterByColumn",
                        "Argument 'value' is required; pass null for null/empty operators.");
                }
                var rawOp = args.GetString("filterOperator");

                var requested = NormalizeFilterOperator(rawOp) ?? "contains";

                // Promote literal equality/nonequality on string-typed columns to substring
                // matching when the supplied value looks like a text fragment. Equal-on-string
                // against a substring is almost universally a wrong intent from natural-language
                // prompts ("filter where CustomerID equals VICTE" plainly meant "contains VICTE").
                // Numeric / date columns keep equality semantics — equality there is unambiguous.
                var applied = PromoteEqualityOnStringColumn(fieldName, requested, value);

                var predicate = args.GetString("predicate");
                if (!string.IsNullOrWhiteSpace(predicate)
                    && !predicate.Equals("and", StringComparison.OrdinalIgnoreCase)
                    && !predicate.Equals("or", StringComparison.OrdinalIgnoreCase))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "filterByColumn",
                        "Argument 'predicate' must be 'and' or 'or'.");
                }

                var matchCase = GetOptionalBoolean(args, "matchCase", "filterByColumn") ?? false;
                var ignoreAccent = GetOptionalBoolean(args, "ignoreAccent", "filterByColumn");
                var actualFilterValue = args.TryGetValue("actualFilterValue", out var actualValue)
                    ? actualValue
                    : null;
                var actualOperator = args.TryGetValue("actualOperator", out var actualOperatorValue)
                    ? actualOperatorValue
                    : null;
                var columnUid = args.GetString("columnUid");

                if (string.IsNullOrWhiteSpace(columnUid))
                {
                    var column = _parent.Columns?
                        .FirstOrDefault(c =>
                            string.Equals(c.Field, fieldName, StringComparison.OrdinalIgnoreCase));

                    if (column == null)
                    {
                        throw new WebMcpException(
                            WebMcpErrorCode.InvalidArgument,
                            "filterByColumn",
                            $"No column found for field '{fieldName}'.");
                    }

                    columnUid = column.Uid;
                }

                await _parent.FilterByColumnAsync(
                    fieldName,
                    applied,
                    value!,
                    predicate!,
                    matchCase,
                    ignoreAccent,
                    actualFilterValue!,
                    actualOperator!,
                    columnUid!).ConfigureAwait(true);

                return new
                {
                    success = true,
                    requestedFilterOperator = requested,
                    appliedFilterOperator = applied,
                    promoted = !string.Equals(applied, requested, StringComparison.Ordinal),
                    caseInsensitive = !matchCase
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "filterByColumn",
                    "Failed to filter by column", ex);
            }
        }

        private static bool? GetOptionalBoolean(
            Dictionary<string, object> args,
            string argumentName,
            string toolName)
        {
            if (!args.TryGetValue(argumentName, out var rawValue))
            {
                return null;
            }

            if (rawValue is bool value)
            {
                return value;
            }

            if (rawValue is System.Text.Json.JsonElement element
                && (element.ValueKind == System.Text.Json.JsonValueKind.True
                    || element.ValueKind == System.Text.Json.JsonValueKind.False))
            {
                return element.GetBoolean();
            }

            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                $"Argument '{argumentName}' must be a boolean.");
        }

        /// <summary>
        /// Resolves <paramref name="operatorText"/> against the canonical Syncfusion operator
        /// list, returning the lowercase token the grid's <c>Filter.GetOperator</c> dispatcher
        /// expects. Returns <c>null</c> when no alias matched so the caller can apply its
        /// default. <paramref name="value"/> is consulted only to skip auto-promotion when
        /// the value is numeric-looking (e.g. "12" against an integer column is still equality).
        /// </summary>
        private string PromoteEqualityOnStringColumn(string fieldName, string operatorText, object? value)
        {
            if (!string.Equals(operatorText, "equal", StringComparison.Ordinal) &&
                !string.Equals(operatorText, "notequal", StringComparison.Ordinal))
            {
                return operatorText;
            }

            // Only promote when the supplied value is a string-shaped value — numeric fragments
            // like "12" against an Integer column are legitimately equality even if the agent
            // forgot to specify the column type.
            var valueIsText = value is string s
                ? System.Text.RegularExpressions.Regex.IsMatch(s, @"[^\d\.\-]")
                : false;
            if (!valueIsText)
            {
                return operatorText;
            }

            var column = _parent.Columns?.FirstOrDefault(c =>
                string.Equals(c.Field, fieldName, StringComparison.Ordinal) ||
                string.Equals(c.Uid, fieldName, StringComparison.Ordinal));
            if (column == null)
            {
                // Unknown field — assume string-typed for safety (most matches live there).
                return operatorText == "equal" ? "contains" : "doesnotcontain";
            }
            if (column.Type == Syncfusion.Blazor.Grids.ColumnType.String)
            {
                return operatorText == "equal" ? "contains" : "doesnotcontain";
            }

            return operatorText;
        }

        /// <summary>
        /// Resolves any reasonable dialect an agent might send for the filter operator
        /// into the canonical lowercase keyword the grid's <c>Filter.GetOperator</c>
        /// dispatcher expects (<c>Filter.cs:427</c>). The normalisation is:
        ///   1. Trim + uppercase with <see cref="StringComparison.Ordinal"/> (avoids the
        ///      Turkish-locale trap where <c>"Contains"</c> → <c>"ıontains"</c>).
        ///   2. Strip every non-alphanumeric char (<c>'_'</c>, <c>'-'</c>, <c>' '</c>) so
        ///      <c>"starts_with"</c>, <c>"Starts-With"</c> and <c>"starts with"</c> all
        ///      collapse to the same lookup key.
        ///   3. Match through a plain alias table (case-sensitive once step 1+2 have run).
        /// Returns <c>null</c> if no alias matched — the caller decides the default.
        /// The returned canonical token is lowercased on the way out so the
        /// public contract ("canonical lowercase keyword") is preserved while
        /// the in-method case fold uses <c>ToUpperInvariant</c> per CA1308.
        /// </summary>
        private static string? NormalizeFilterOperator(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                return null;
            }

            switch (raw.Trim())
            {
                case "==":
                case "===":
                case "=":
                    return "equal";
                case "!=":
                case "<>":
                case "=/=":
                    return "notequal";
                case "^":
                    return "startswith";
                case "$":
                    return "endswith";
                case "*":
                    return "contains";
                case ">":
                    return "greaterthan";
                case ">=":
                    return "greaterthanorequal";
                case "<":
                    return "lessthan";
                case "<=":
                    return "lessthanorequal";
            }

            var upper = raw.Trim().ToUpperInvariant();
            var scrubbed = new char[upper.Length];
            var len = 0;
            foreach (var c in upper)
            {
                if ((c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9'))
                {
                    scrubbed[len++] = c;
                }
            }
            var canonical = new string(scrubbed, 0, len);
            if (canonical.Length == 0)
            {
                return null;
            }

            // Alias table — ordered to allow both expanded forms and programming-language
            // shortcuts (==, !=, <, >, ^, $, *) that humans routinely type.
            switch (canonical)
            {
                // Equality
                case "EQUAL":
                case "EQ":
                case "ISEQUAL":
                case "ISEQUALTO":
                case "==":
                case "===":
                case "=":
                    return "equal";

                case "NOTEQUAL":
                case "NOTEQUALS":
                case "NEQ":
                case "ISNOTEQUAL":
                case "ISNOTEQUALTO":
                case "!=":
                case "<>":
                case "=/=":
                    return "notequal";

                // String — start
                case "STARTSWITH":
                case "BEGINSWITH":
                case "STARTS":
                case "BEGIN":
                case "PREFIX":
                case "^":
                    return "startswith";

                case "DOESNOTSTARTWITH":
                case "DOESNOTBEGINWITH":
                case "NOTSTARTWITH":
                case "NOTBEGINSWITH":
                    return "doesnotstartwith";

                // String — end
                case "ENDSWITH":
                case "SUFFIX":
                case "ENDING":
                case "$":
                    return "endswith";

                case "DOESNOTENDWITH":
                case "DOESNOTENDSWITH":
                case "NOTENDSWITH":
                    return "doesnotendwith";

                // String — substring
                case "CONTAINS":
                case "CONTAIN":
                case "HAS":
                case "INCLUDES":
                case "SUBSTRING":
                case "*":
                    return "contains";

                case "DOESNOTCONTAIN":
                case "DOESNTCONTAIN":
                case "CONTAINNOT":
                case "NOTCONTAIN":
                case "NOTCONTAINS":
                case "EXCLUDES":
                    return "doesnotcontain";

                // Wildcard / like
                case "LIKE":
                    return "like";
                case "WILDCARD":
                case "GLOB":
                case "MATCH":
                    return "wildcard";

                // Numeric — greater
                case "GREATERTHAN":
                case "GT":
                case "ISGREATERTHAN":
                case ">":
                    return "greaterthan";
                case "GREATERTHANOREQUAL":
                case "GTE":
                case "ISGREATERTHANOREQUAL":
                case "ISGREATERTHANOREQUALTO":
                case "GREATEROREQUAL":
                case ">=":
                    return "greaterthanorequal";

                // Numeric — lesser
                case "LESSTHAN":
                case "LT":
                case "ISLESSTHAN":
                case "<":
                    return "lessthan";
                case "LESSTHANOREQUAL":
                case "LTE":
                case "ISLESSTHANOREQUAL":
                case "ISLESSTHANOREQUALTO":
                case "LESSOREQUAL":
                case "<=":
                    return "lessthanorequal";

                // Null / empty
                case "ISNULL":
                case "NULL":
                case "ISNIL":
                    return "isnull";
                case "ISNOTNULL":
                case "ISNOTNIL":
                case "NOTNULL":
                case "NOTNIL":
                    return "isnotnull";
                case "ISEMPTY":
                case "EMPTY":
                case "ISBLANK":
                case "BLANK":
                    return "isempty";
                case "ISNOTEMPTY":
                case "ISNOTBLANK":
                case "NOTEMPTY":
                case "NOTBLANK":
                    return "isnotempty";

                default:
                    // Already canonical (case-insensitive)? Lowercase and return.
                    var lower = raw.Trim();
                    if (string.Equals(canonical, lower, StringComparison.OrdinalIgnoreCase))
                    {
                        return lower;
                    }
                    return null;
            }
        }

        private async Task<object> HandleNavigateToPage(Dictionary<string, object> args)
        {
            try
            {
                // Default action is 'goto' for backward compatibility — agents that
                // passed only 'pageNo' keep working unchanged.
                var action = (args.TryGetValue("action", out var actionRaw)
                    ? actionRaw?.ToString() ?? "goto"
                    : "goto").Trim().ToUpperInvariant();

                int currentPage = _parent.PageSettings?.CurrentPage ?? 1;
                int pageSize    = _parent.PageSettings?.PageSize    ?? 0;
                int totalItems   = _parent.TotalItemCount;
                int totalPages   = _parent.AllowPaging && pageSize > 0
                    ? (int)Math.Ceiling((double)totalItems / pageSize)
                    : 1;
                if (totalPages < 1) totalPages = 1;

                int requested = 0;
                int target;

                switch (action)
                {
                    case "FIRST":
                        requested = currentPage;
                        target = 1;
                        break;

                    case "LAST":
                        requested = currentPage;
                        target = totalPages;
                        break;

                    case "NEXT":
                        requested = currentPage;
                        target = Math.Min(currentPage + 1, totalPages);
                        break;

                    case "PREVIOUS":
                    case "PREV":
                        requested = currentPage;
                        target = Math.Max(currentPage - 1, 1);
                        break;

                    case "GOTO":
                        var pageNoStr = args.GetString("pageNo");
                        if (string.IsNullOrWhiteSpace(pageNoStr))
                        {
                            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "navigateToPage",
                                "Argument 'pageNo' is required when action='goto'.");
                        }
                        if (!int.TryParse(pageNoStr, out var parsed) || parsed < 1)
                        {
                            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "navigateToPage",
                                $"Argument 'pageNo' must be a positive integer. Received: '{pageNoStr}'.");
                        }
                        requested = parsed;
                        target = parsed;
                        break;

                    default:
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "navigateToPage",
                            $"Unknown action '{action}'. Use one of: goto, next, previous, first, last.");
                }

                // Clamp to [1, totalPages] so relative actions don't overshoot.
                var clamped = Math.Clamp(target, 1, totalPages);
                if (clamped != currentPage)
                {
                    await _parent.GoToPageAsync(clamped).ConfigureAwait(true);
                }

                // Refresh the post-await view of CurrentPage — GoToPageAsync may have re-routed
                // through PageModule which updates PageSettings asynchronously.
                var resolvedPage = _parent.PageSettings?.CurrentPage ?? clamped;

                return new
                {
                    success = true,
                    action = action.Trim(),
                    currentPage = resolvedPage,
                    totalPages = totalPages,
                    requestedPage = requested,
                    clamped = clamped != target,
                    target = target
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "navigateToPage",
                    "Failed to navigate to page", ex);
            }
        }

        private async Task<object> HandleSelectRows(Dictionary<string, object> args)
        {
            try
            {
                // The tool accepts the same argument shapes regardless of selection type;
                // behavior is decided at the end. For Single, only the first resolved row
                // is honored; for Multiple, the full union is applied.
                var isSingleSelection = _parent.SelectionSettings?.Type == SelectionType.Single;

                var hasRowIndexes = args.ContainsKey("rowIndexes");
                var hasWhere = args.ContainsKey("where");
                if (!hasRowIndexes && !hasWhere)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows",
                        "Supply at least one of 'rowIndexes' (positional) or 'where' (field-value match).");
                }

                var toggle = args.GetBool("toggle");

                // Positional path: 'the 5th record' → rowIndexes=[5].
                // OpenAPI declares 1-based; convert to 0-based for the grid.
                var fromRowIndexes = Array.Empty<int>();
                if (hasRowIndexes)
                {
                    var oneBasedRaw = args.GetIntArray("rowIndexes");
                    if (oneBasedRaw.Length == 0)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows",
                            "Argument 'rowIndexes' must contain at least one row index when supplied.");
                    }
                    for (var i = 0; i < oneBasedRaw.Length; i++)
                    {
                        if (oneBasedRaw[i] < 1)
                        {
                            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows",
                                $"Argument 'rowIndexes[{i}]' must be a positive 1-based integer. Received: {oneBasedRaw[i]}.");
                        }
                    }
                    fromRowIndexes = oneBasedRaw.Select(x => x - 1).ToArray();
                }

                // Field-value path: 'row whose CustomerID equals TOMSP'. Reflection over
                // the current view records using the public TValue property; case-insensitive by default.
                var fromWhere = Array.Empty<int>();
                if (hasWhere)
                {
                    var view = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
                    fromWhere = ResolveFieldValueMatches(view, args["where"]);
                }

                // Union, dedup, sort. After selecting with the user's 1-based rowIndexes
                // we get 0-based view indexes. Combined lives in 0-based view space.
                var viewRows = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
                var viewSize = viewRows?.Count ?? -1;

                var combined = fromRowIndexes
                    .Concat(fromWhere)
                    .Distinct()
                    .OrderBy(idx => idx)
                    .ToList();

                // Validate against the current view before sending to the selection module —
                // it silently drops out-of-range entries which can mask agent bugs.
                if (viewSize >= 0)
                {
                    var outOfRange = combined.Where(idx => idx < 0 || idx >= viewSize).ToList();
                    if (outOfRange.Count > 0)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows",
                            $"One or more row indexes fall outside the current view [0, {viewSize - 1}]: [{string.Join(", ", outOfRange)}].");
                    }
                }

                // Preserve existing selection when 'toggle' was requested and the user is
                // adding to a prior selection (e.g. "select rows 2 and 5, keep 5 already picked").
                // Otherwise SelectRowsAsync would clear and replace — the documented behaviour
                // of the multi-row API.
                List<int>? finalSelection = null;
                if (toggle)
                {
                    var existing = (await _parent.GetSelectedRowIndexesAsync().ConfigureAwait(true)) ?? new List<int>();
                    finalSelection = existing
                        .Concat(combined)
                        .Distinct()
                        .OrderBy(i => i)
                        .ToList();
                }
                else if (isSingleSelection)
                {
                    // For Single-type, only the first resolved row is honored. Pick the
                    // smallest combined index so positional intents stay predictable.
                    finalSelection = combined.Count > 0 ? new List<int> { combined[0] } : new List<int>();
                }
                else
                {
                    // Default Multiple replacement-mode — full union replaces prior selection.
                    finalSelection = combined;
                }

                if (finalSelection.Count > 0)
                {
                    if (isSingleSelection)
                    {
                        // For Single, route through the single-row API — the multi-row API
                        // ignores excess indexes in Single mode and silently yields the
                        // last item only, which is confusing for agent authors.
                        await _parent.SelectRowAsync((int)finalSelection[0], null, false).ConfigureAwait(true);
                    }
                    else
                    {
                        // SelectRowsAsync internally clears the prior selection
                        // (line 158 of Selection.cs) and selects each row in turn, matching data
                        // rows by either array position (non-virtualization) or `_.Index`
                        // (virtualization). Off-screen selections under virtualization are
                        // tracked on VirtualScrollModule.SelectRowsMethodIndexes.
                        await _parent.SelectRowsAsync(finalSelection.ToArray()).ConfigureAwait(true);
                    }
                }

                return new
                {
                    success = true,
                    count = finalSelection.Count,
                    selectionMode = isSingleSelection ? "Single" : "Multiple",
                    fromRowIndexes = fromRowIndexes,
                    fromWhere = fromWhere,
                    appliedIndexes = finalSelection
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "selectRows",
                    "Failed to select rows", ex);
            }
        }

        /// <summary>
        /// Resolve a "where" argument like <c>{ field:"CustomerID", equals:"TOMSP", limit:1 }</c>
        /// into a list of 0-based indexes from the supplied current-view records.
        /// </summary>
        /// <remarks>
        /// The "where" arg is itself a JSON object, so the raw pre-deserialization
        /// <see cref="object"/> from the args dict is re-serialized through
        /// <see cref="System.Text.Json.JsonSerializer"/> to re-hydrate the nested
        /// properties inside a plain dictionary. This is more permissive than
        /// the schema implies (the agent is allowed to over-specify fields) and
        /// avoids hand-rolling a per-property deserializer.
        /// </remarks>
        private static int[] ResolveFieldValueMatches(System.Collections.Generic.List<TValue> view, object? whereArg)
        {
            if (view is null || view.Count == 0 || whereArg is null) return Array.Empty<int>();

            var whereJson = System.Text.Json.JsonSerializer.Serialize(whereArg);
            var where = System.Text.Json.JsonSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(whereJson);

            if (where is null
                || !where.TryGetValue("field", out var fieldRaw) || fieldRaw is null
                || !where.TryGetValue("equals", out var equalsRaw) || equalsRaw is null)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows",
                    "Argument 'where' requires both 'field' (property name on the grid item) and 'equals' (value to match).");
            }

            var field = fieldRaw.ToString() ?? string.Empty;
            var equals = equalsRaw.ToString() ?? string.Empty;
            var caseSensitive = where.TryGetValue("caseSensitive", out var csRaw) && csRaw is bool csb && csb;
            var limit = 1;
            if (where.TryGetValue("limit", out var limitRaw) && limitRaw is not null)
            {
                try { var parsed = Convert.ToInt32(limitRaw, System.Globalization.CultureInfo.InvariantCulture); limit = parsed < 1 ? 1 : parsed; }
                catch (OverflowException) { throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows", $"Argument 'where.limit' is out of int range ({limitRaw})."); }
                catch (FormatException)      { throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRows", $"Argument 'where.limit' must be a positive integer; received '{limitRaw}'."); }
            }

            var cmp = caseSensitive
                ? (System.Func<string, string, bool>)((a, b) => string.Equals(a, b, StringComparison.Ordinal))
                : (a, b) => string.Equals(a, b, StringComparison.OrdinalIgnoreCase);

            var matches = new List<int>();
            for (var i = 0; i < view.Count && matches.Count < limit; i++)
            {
                var item = view[i];
                if (item is null) continue;

                var prop = item.GetType().GetProperty(field);
                if (prop is null || !prop.CanRead) continue;

                var rawValue = prop.GetValue(item);
                if (rawValue is null) continue;

                if (cmp(rawValue.ToString() ?? string.Empty, equals))
                {
                    matches.Add(i);
                }
            }
            return matches.ToArray();
        }

        private async Task<object> HandleClearSelection(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ClearSelectionAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearSelection",
                    "Failed to clear selection", ex);
            }
        }

        private async Task<object> HandleClearCellSelection(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ClearCellSelectionAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearCellSelection",
                    "Failed to clear cell selection", ex);
            }
        }

        private async Task<object> HandleClearRowSelection(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ClearRowSelectionAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearRowSelection",
                    "Failed to clear row selection", ex);
            }
        }

        private async Task<object> HandleUndo(Dictionary<string, object> args)
        {
            try
            {
                await _parent.UndoAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "undo",
                    "Failed to undo the most recent grid operation", ex);
            }
        }

        private async Task<object> HandleRedo(Dictionary<string, object> args)
        {
            try
            {
                await _parent.RedoAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "redo",
                    "Failed to redo the most recent grid operation", ex);
            }
        }

        private async Task<object> HandleSetCellValue(Dictionary<string, object> args)
        {
            try
            {
                if (args.TryGetValue("cellUpdates", out var updatesRaw) && updatesRaw is not null)
                {
                    var updates = DeserializeCellUpdates(updatesRaw);
                    if (updates.Count == 0)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "setCellValue",
                            "Argument 'cellUpdates' must contain at least one update.");
                    }
                    var primaryKeys = await _parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                    if (primaryKeys.Count == 0)
                    {
                        throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "setCellValue",
                            "Cell updates by key require a GridColumn configured with IsPrimaryKey=true.");
                    }
                    foreach (var update in updates)
                    {
                        update.PrimaryKeyValue = ConvertJsonValue(update.PrimaryKeyValue, GetFieldType(primaryKeys[0]))!;
                        update.NewValue = ConvertJsonValue(update.NewValue, GetFieldType(update.ColumnField));
                    }
                    await _parent.SetCellValueAsync(updates).ConfigureAwait(true);
                    return new { success = true, updatedCount = updates.Count };
                }

                args.ValidateRequired("setCellValue", "key", "fieldName");
                var fieldName = args.GetString("fieldName");
                if (!args.TryGetValue("value", out var value))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "setCellValue",
                        "Argument 'value' is required for a single-cell update; pass null to clear a nullable cell.");
                }
                var primaryFields = await _parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                if (primaryFields.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "setCellValue",
                        "Cell updates by key require a GridColumn configured with IsPrimaryKey=true.");
                }
                var key = args["key"];
                key = ConvertJsonValue(key, GetFieldType(primaryFields[0]))!;
                var convertedValue = ConvertJsonValue(value, GetFieldType(fieldName), "setCellValue", "value");
                await _parent.SetCellValueAsync(key, fieldName, convertedValue!).ConfigureAwait(true);
                return new { success = true, updatedCount = 1 };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "setCellValue",
                    "Failed to set cell value", ex);
            }
        }

        private async Task<object> HandleSetRowData(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("setRowData", "key", "rowData");
                var key = args.TryGetValue("key", out var k2) ? k2 : null;
                var rowDataJson = args.TryGetValue("rowData", out var rd) && rd != null
                    ? System.Text.Json.JsonSerializer.Serialize(rd)
                    : null;

                // Deserialize dictionary to TValue - host app must match schema
                TValue? rowData = !string.IsNullOrEmpty(rowDataJson)
                    ? System.Text.Json.JsonSerializer.Deserialize<TValue>(
                        rowDataJson,
                        _caseInsensitiveJsonSerializerOptions)
                    : default;

                var primaryFields = await _parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                if (primaryFields.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "setRowData",
                        "Updating a row by key requires a GridColumn configured with IsPrimaryKey=true.");
                }
                key = ConvertJsonValue(key, GetFieldType(primaryFields[0]), "setRowData", "key")!;
                await _parent.SetRowDataAsync(key!, rowData!, args.GetBool("preventDataUpdate")).ConfigureAwait(true);

                return new { success = true, preventDataUpdate = args.GetBool("preventDataUpdate") };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "setRowData",
                    "Failed to set row data", ex);
            }
        }

        private async Task<object> HandleCrudLifecycle(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("crudLifecycle", "action");
                var action = args.GetString("action").Trim().ToUpperInvariant();

                switch (action)
                {
                    case "STARTEDIT":
                        await _parent.StartEditAsync().ConfigureAwait(true);
                        break;

                    case "ENDEDIT":
                        await _parent.EndEditAsync().ConfigureAwait(true);
                        break;

                    case "CLOSEEDIT":
                        await _parent.CloseEditAsync().ConfigureAwait(true);
                        break;

                    case "ADDRECORD":
                        if (args.TryGetValue("data", out var rawAddData) && rawAddData is not null)
                        {
                            int? index = null;
                            if (args.TryGetValue("index", out var indexValue) && indexValue is not null)
                            {
                                if (!int.TryParse(indexValue.ToString(), out var parsedIndex) || parsedIndex < 0)
                                {
                                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "crudLifecycle",
                                        "Argument 'index' must be a non-negative integer for addRecord.");
                                }
                                index = parsedIndex;
                            }
                            var addData = DeserializeCrudRecord(rawAddData, "addRecord");
                            await _parent.AddRecordAsync(addData, index).ConfigureAwait(true);
                        }
                        else
                        {
                            if (args.TryGetValue("index", out var suppliedIndex) && suppliedIndex is not null)
                            {
                                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "crudLifecycle",
                                    "Argument 'index' can only be used with record data.");
                            }
                            await _parent.AddRecordAsync().ConfigureAwait(true);
                        }
                        break;

                    case "DELETERECORD":
                        var hasDeleteField = args.TryGetValue("fieldName", out var rawDeleteField) &&
                            rawDeleteField is not null;
                        var hasDeleteData = args.TryGetValue("data", out var rawDeleteData) &&
                            rawDeleteData is not null;
                        if (hasDeleteField != hasDeleteData)
                        {
                            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "crudLifecycle",
                                "For deleteRecord, provide both fieldName and data to delete a specific record, or omit both to delete the selected record.");
                        }
                        if (hasDeleteField)
                        {
                            var fieldName = rawDeleteField!.ToString();
                            if (string.IsNullOrWhiteSpace(fieldName))
                            {
                                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "crudLifecycle",
                                    "Argument 'fieldName' must be a non-empty string.");
                            }
                            var deleteData = DeserializeCrudRecord(rawDeleteData!, "deleteRecord");
                            await _parent.DeleteRecordAsync(fieldName, deleteData).ConfigureAwait(true);
                        }
                        else
                        {
                            await _parent.DeleteRecordAsync().ConfigureAwait(true);
                        }
                        break;

                    default:
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "crudLifecycle",
                            $"Unknown CRUD action '{action}'. Use startEdit, endEdit, closeEdit, addRecord, or deleteRecord.");
                }

                return new
                {
                    success = true,
                    action = action.Trim(),
                    message = $"CRUD action '{action.Trim()}' completed."
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "crudLifecycle",
                    "Failed to execute CRUD lifecycle action", ex);
            }
        }

        private static TValue DeserializeCrudRecord(object rawData, string action)
        {
            var dataJson = System.Text.Json.JsonSerializer.Serialize(rawData);
            var data = System.Text.Json.JsonSerializer.Deserialize<TValue>(dataJson);
            if (data is null)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "crudLifecycle",
                    $"Argument 'data' must contain a valid record object for {action}.");
            }
            return data;
        }

        private async Task<object> HandleDeleteRow(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("deleteRow", "rowIndex");

                if (_parent.EditSettings?.AllowDeleting != true)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "deleteRow",
                        "Deleting a row requires GridEditSettings.AllowDeleting=true.");
                }

                var rowIndex = ParsePositiveIndex(args["rowIndex"], "deleteRow", "rowIndex");
                var primaryKeyFields = await _parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                if (primaryKeyFields.Count != 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "deleteRow",
                        "Deleting by view index requires exactly one primary-key column.");
                }

                var records = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
                if (rowIndex > records.Count)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "deleteRow",
                        $"Argument 'rowIndex' must be between 1 and {records.Count} for the current view.");
                }

                await _parent.DeleteRecordAsync(primaryKeyFields[0], records[rowIndex - 1]).ConfigureAwait(true);
                return new { success = true, rowIndex };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "deleteRow",
                    "Failed to delete the requested current-view row", ex);
            }
        }

        private async Task<object> HandleExcelExport(Dictionary<string, object> args)
        {
            try
            {
                // Optional format — default xlsx. Aliases: "excel"/"xls"→xlsx, "csv"/"text"/"tsv"→csv.
                var formatRaw = args.GetString("format");
                var format = "xlsx";
                if (!string.IsNullOrWhiteSpace(formatRaw))
                {
                    var f = formatRaw.Trim().ToUpperInvariant();
                    if (f is "XLSX" or "XLS" or "EXCEL" or "XLSB") format = "xlsx";
                    else if (f is "CSV" or "TEXT" or "TSV") format = "csv";
                    else throw new WebMcpException(
                        WebMcpErrorCode.InvalidArgument,
                        "excelExport",
                        $"Argument 'format' must be 'xlsx' or 'csv'. Received: '{formatRaw}'.");
                }

                var baseName = args.GetString("fileName");
                if (string.IsNullOrWhiteSpace(baseName)) baseName = "Export";

                // Sanitization: strip path components (Syncfusion uses only the leaf but defence-in-depth),
                // drop invalid filesystem chars. Empty leaf → fallback to "Export".
                var sanitized = SanitizeExportFileName(baseName);
                if (string.IsNullOrWhiteSpace(sanitized)) sanitized = "Export";

                // Always pin the extension — Syncfusion's Workbook.Save() emits Office Open XML
                // (.xlsx) which begins with the ZIP magic bytes (PK\013\010 / 0x50 0x4B 0x03 0x04).
                // Without an explicit extension the browser falls back to content-sniffing,
                // detects ZIP, and saves the file as "<sanitized>.zip" — that is exactly why the
                // user reported "sometimes the export ends up as a zip file". We always override
                // whatever extension the caller supplied and attach the canonical one.
                var resolved = StripTrailingExtension(sanitized, format) + "." + format;

                if (!_parent.AllowExcelExport)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "excelExport",
                        "Spreadsheet export requires AllowExcelExport=true.");
                }

                var exportProperties = new ExcelExportProperties { FileName = resolved };
                var asMemoryStream = args.GetBool("asMemoryStream");
                string? data = null;
                if (asMemoryStream)
                {
                    using var stream = format == "csv"
                        ? await _parent.ExportToCsvAsync(true, exportProperties).ConfigureAwait(true)
                        : await _parent.ExportToExcelAsync(true, exportProperties).ConfigureAwait(true);
                    if (stream is null)
                    {
                        throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "excelExport",
                            "The grid export did not return a memory stream.");
                    }
                    data = Convert.ToBase64String(stream.ToArray());
                }
                else
                {
                    if (format == "csv")
                    {
                        await _parent.ExportToCsvAsync(false, exportProperties).ConfigureAwait(true);
                    }
                    else
                    {
                        await _parent.ExportToExcelAsync(exportProperties).ConfigureAwait(true);
                    }
                }

                return new
                {
                    success = true,
                    requestedFileName = baseName,
                    resolvedFileName = resolved,
                    format = format,
                    contentType = format == "csv" ? "text/csv" : "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    delivery = asMemoryStream ? "base64" : "browserDownload",
                    data = data,
                    note = asMemoryStream
                        ? "The data field contains the exported file bytes encoded as base64."
                        : "The browser receives the export as a download. The .xlsx file is internally a ZIP container of XML parts — that's normal for the Office Open XML format. The filename extension pins the browser to save it as .xlsx/.csv instead of auto-sniffing ZIP."
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "excelExport",
                    "Failed to export to Excel", ex);
            }
        }

        private async Task<object> HandlePdfExport(Dictionary<string, object> args)
        {
            try
            {
                if (!_parent.AllowPdfExport)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "pdfExport",
                        "PDF export requires AllowPdfExport=true.");
                }

                var properties = new PdfExportProperties();
                var exportTypeName = args.GetString("exportType");
                if (!string.IsNullOrWhiteSpace(exportTypeName))
                {
                    if (!Enum.TryParse(exportTypeName, true, out ExportType exportType) ||
                        !Enum.IsDefined(exportType))
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "pdfExport",
                            "Argument 'exportType' must be 'AllPages' or 'CurrentPage'.");
                    }
                    properties.ExportType = exportType;
                }

                SetPdfBooleanOption(args, "includeHeaderRow", value => properties.IncludeHeaderRow = value);
                SetPdfBooleanOption(args, "includeCommandColumn", value => properties.IncludeCommandColumn = value);
                SetPdfBooleanOption(args, "allowHorizontalOverflow", value => properties.AllowHorizontalOverflow = value);
                SetPdfBooleanOption(args, "disableAutoFitWidth", value => properties.DisableAutoFitWidth = value);
                SetPdfBooleanOption(args, "isRepeatHeader", value => properties.IsRepeatHeader = value);
                SetPdfBooleanOption(args, "isThemeEnabled", value => properties.IsThemeEnabled = value);

                await _parent.ExportToPdfAsync(properties).ConfigureAwait(true);
                return new
                {
                    success = true,
                    exportType = properties.ExportType.ToString(),
                    delivery = "browserDownload"
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "pdfExport",
                    "Failed to export to PDF", ex);
            }
        }

        private static void SetPdfBooleanOption(
            Dictionary<string, object> args,
            string argumentName,
            Action<bool> assign)
        {
            if (!args.TryGetValue(argumentName, out var rawValue))
            {
                return;
            }

            if (rawValue is not bool value)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "pdfExport",
                    $"Argument '{argumentName}' must be a boolean.");
            }

            assign(value);
        }

        private Task<object> HandlePreventRender(Dictionary<string, object> args)
        {
            try
            {
                var preventRender = !args.ContainsKey("preventRender") || args.GetBool("preventRender");
                _parent.PreventRender(preventRender);
                return Task.FromResult<object>(new { success = true, preventRender = preventRender });
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "preventRender",
                    "Failed to update the grid render setting", ex);
            }
        }

        /// <summary>
        /// Removes any extension that matches the current <paramref name="format"/> so we
        /// can re-attach the canonical one. Leaves other extensions alone (so an agent that
        /// passes "report.txt" still gets "report.txt.xlsx").
        /// </summary>
        private static string StripTrailingExtension(string name, string format)
        {
            var dot = name.LastIndexOf('.');
            if (dot < 0) return name;
            var ext = name.Substring(dot + 1).ToUpperInvariant();
            if (format == "xlsx" && (ext is "XLSX" or "XLS")) return name.Substring(0, dot);
            if (format == "csv" && (ext is "CSV" or "TSV" or "TXT")) return name.Substring(0, dot);
            return name;
        }

        /// <summary>
        /// Strips directory traversal, control char sequences and characters Windows/macOS
        /// refuse in filenames (<c>\ / : * ? " &lt; &gt; |</c>). Keeps Unicode.
        /// </summary>
        // CA1870: cache the path-separator set so repeated calls don't allocate
        // a fresh char[] per filename.
        private static readonly SearchValues<char> _pathSeparators = SearchValues.Create("/\\");
        private static readonly char[] _invalidFilenameChars =
            { '<', '>', ':', '"', '/', '\\', '|', '?', '*' };

        private static string SanitizeExportFileName(string raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return string.Empty;
            var leaf = raw;
            var slash = leaf.AsSpan().LastIndexOfAny(_pathSeparators);
            if (slash >= 0) leaf = leaf.Substring(slash + 1);
            var trimmed = leaf.Trim().TrimEnd('.', ' ');
            if (trimmed.Length == 0) return string.Empty;
            var buf = trimmed.ToCharArray();
            for (var i = 0; i < buf.Length; i++)
            {
                if (Array.IndexOf(_invalidFilenameChars, buf[i]) >= 0 || buf[i] < 32) buf[i] = '_';
            }
            var cleaned = new string(buf);
            return cleaned.Length > 120 ? cleaned.Substring(0, 120) : cleaned;
        }

        private async Task<object> HandleGroupColumn(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("groupColumn", "columnName");
                var columnName = args.GetString("columnName");
                await _parent.GroupColumnAsync(columnName!).ConfigureAwait(true);

                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "groupColumn",
                    "Failed to group column", ex);
            }
        }

        // PHASE 4 EXPANSION (17): toolbar-style search — filters rows.
        // Write-side (mutates SearchSettings.Key via the SearchAsync plumbing).
        private async Task<object> HandleSearchText(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("searchText", "searchString");
                var searchString = args.GetString("searchString") ?? string.Empty;

                await _parent.SearchAsync(searchString).ConfigureAwait(true);

                return new { success = true, searchString = searchString };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "searchText",
                    "Failed to apply search", ex);
            }
        }

        // PHASE 5 EXPANSION (19/41): multi-column sort (additive)
        private async Task<object> HandleSortColumns(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("sortColumns", "columns");
                var sortColumns = DeserializeSortColumns(args["columns"], "sortColumns");
                await _parent.SortColumnsAsync(sortColumns).ConfigureAwait(true);
                return new { success = true, appliedCount = sortColumns.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "sortColumns",
                    "Failed to sort by multiple columns", ex);
            }
        }

        // PHASE 5 EXPANSION (20/41): multi-column sort with optional clear
        private async Task<object> HandleSortColumnsWithClear(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("sortColumnsWithClear", "columns");
                var sortColumns = DeserializeSortColumns(args["columns"], "sortColumnsWithClear");
                var clearPrevious = args.GetBool("clearPreviousSort");
                await _parent.SortColumnsAsync(sortColumns, clearPrevious).ConfigureAwait(true);
                return new { success = true, cleared = clearPrevious, appliedCount = sortColumns.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "sortColumnsWithClear",
                    "Failed to sort by multiple columns (with clear)", ex);
            }
        }

        // PHASE 5 EXPANSION (21/41): clear all sorts
        private async Task<object> HandleClearSorting(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ClearSortingAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearSorting",
                    "Failed to clear sorting", ex);
            }
        }

        // PHASE 5 EXPANSION (22/41): clear sort on specific field names
        private async Task<object> HandleClearSortingByFields(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("clearSortingByFields", "fieldNames");
                var fieldNames = DeserializeStringList(args["fieldNames"], "clearSortingByFields.fieldNames");
                if (fieldNames.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "clearSortingByFields",
                        "Argument 'fieldNames' must contain at least one field name.");
                }
                await _parent.ClearSortingAsync(fieldNames).ConfigureAwait(true);
                return new { success = true, clearedCount = fieldNames.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearSortingByFields",
                    "Failed to clear sorting for specified fields", ex);
            }
        }

        // PHASE 5 EXPANSION (23/41): clear filtering — all or specific
        private async Task<object> HandleClearFiltering(Dictionary<string, object> args)
        {
            try
            {
                if (args.TryGetValue("fieldNames", out var raw) && raw is not null)
                {
                    var fieldNames = DeserializeStringList(raw, "clearFiltering.fieldNames");
                    if (fieldNames.Count == 0)
                    {
                        // Empty array is treated as "clear all" — same as omitting the arg.
                        await _parent.ClearFilteringAsync().ConfigureAwait(true);
                        return new { success = true, scope = "all", fieldCount = 0 };
                    }
                    await _parent.ClearFilteringAsync(fieldNames).ConfigureAwait(true);
                    return new { success = true, scope = "specific", fieldCount = fieldNames.Count };
                }

                await _parent.ClearFilteringAsync().ConfigureAwait(true);
                return new { success = true, scope = "all", fieldCount = 0 };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearFiltering",
                    "Failed to clear filtering", ex);
            }
        }

        /// <summary>
        /// Deserialize an args-dictionary value (which the JSON layer surfaces as either a
        /// primitive, <see cref="System.Text.Json.JsonElement"/>, or nested dictionary)
        /// into a <see cref="List{SortColumn}"/>. Field and direction are validated; unknown
        /// direction tokens default to <see cref="SortDirection.Ascending"/> with a logged warning.
        /// </summary>
        private static List<SortColumn> DeserializeSortColumns(object raw, string toolName)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(raw);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            if (items is null || items.Count == 0)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                    "Argument 'columns' must contain at least one column definition.");
            }

            var result = new List<SortColumn>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                var entry = items[i];
                if (!entry.TryGetValue("field", out var fieldRaw) || fieldRaw is null || string.IsNullOrWhiteSpace(fieldRaw.ToString()))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                        $"Argument 'columns[{i}].field' is required and must be a non-empty string.");
                }
                var field = fieldRaw.ToString()!;

                var direction = SortDirection.Ascending;
                if (entry.TryGetValue("direction", out var dirRaw) && dirRaw is not null)
                {
                    var token = dirRaw.ToString() ?? string.Empty;
                    if (token.Equals("Descending", StringComparison.OrdinalIgnoreCase))
                    {
                        direction = SortDirection.Descending;
                    }
                    else if (!token.Equals("Ascending", StringComparison.OrdinalIgnoreCase))
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                            $"Argument 'columns[{i}].direction' must be 'Ascending' or 'Descending'. Received: '{token}'.");
                    }
                }
                else
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                        $"Argument 'columns[{i}].direction' is required.");
                }

                result.Add(new SortColumn { Field = field, Direction = direction });
            }
            return result;
        }

        /// <summary>
        /// Deserialize an args-dictionary value into a list of strings. Used by clear* tools
        /// that take an array of column field names.
        /// </summary>
        private static List<string> DeserializeStringList(object raw, string path)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(raw);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<string>>(json);
            if (items is null) return new List<string>();
            // Drop null/whitespace entries defensively — they would never match a column.
            return items
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s!.Trim())
                .ToList();
        }

        // PHASE 5 EXPANSION (24/41): select a single cell
        private async Task<object> HandleSelectCell(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("selectCell", "rowIndex", "cellIndex");
                var rowRaw = args["rowIndex"]?.ToString();
                var cellRaw = args["cellIndex"]?.ToString();
                if (!int.TryParse(rowRaw, out var rowOneBased) || rowOneBased < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCell",
                        $"Argument 'rowIndex' must be a positive 1-based integer. Received: '{rowRaw}'.");
                }
                if (!int.TryParse(cellRaw, out var cellOneBased) || cellOneBased < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCell",
                        $"Argument 'cellIndex' must be a positive 1-based integer. Received: '{cellRaw}'.");
                }

                // 1-based → 0-based for the grid's public API.
                var zeroBased = (row: rowOneBased - 1, cell: cellOneBased - 1);
                var toggle = args.GetBool("toggle");
                await _parent.SelectCellAsync((zeroBased.row, zeroBased.cell), toggle).ConfigureAwait(true);

                return new
                {
                    success = true,
                    applied = zeroBased,
                    toggle = toggle,
                    selectionMode = _parent.SelectionSettings?.Mode.ToString() ?? string.Empty
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "selectCell",
                    "Failed to select cell", ex);
            }
        }

        // PHASE 5 EXPANSION (25/41): select multiple cells
        private async Task<object> HandleSelectCells(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("selectCells", "rowCellIndexes");
                var tuples = DeserializeCellTuples(args["rowCellIndexes"], "selectCells");
                await _parent.SelectCellsAsync(tuples).ConfigureAwait(true);
                return new { success = true, appliedCount = tuples.Length };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "selectCells",
                    "Failed to select cells", ex);
            }
        }

        // PHASE 5 EXPANSION (26/41): select a rectangular range of cells
        private async Task<object> HandleSelectCellsByRange(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("selectCellsByRange", "startRowIndex", "startCellIndex");
                var startRowRaw = args["startRowIndex"]?.ToString();
                var startCellRaw = args["startCellIndex"]?.ToString();
                if (!int.TryParse(startRowRaw, out var sRow) || sRow < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCellsByRange",
                        $"Argument 'startRowIndex' must be a positive 1-based integer. Received: '{startRowRaw}'.");
                }
                if (!int.TryParse(startCellRaw, out var sCell) || sCell < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCellsByRange",
                        $"Argument 'startCellIndex' must be a positive 1-based integer. Received: '{startCellRaw}'.");
                }

                ValueTuple<int, int>? end = null;
                var hasEndRow = args.TryGetValue("endRowIndex", out var endRowRaw) && endRowRaw is not null;
                var hasEndCell = args.TryGetValue("endCellIndex", out var endCellRaw) && endCellRaw is not null;
                if (hasEndRow || hasEndCell)
                {
                    if (!hasEndRow || !hasEndCell)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCellsByRange",
                            "Both 'endRowIndex' and 'endCellIndex' must be supplied together, or both omitted.");
                    }
                    if (!int.TryParse(endRowRaw?.ToString(), out var eRow) || eRow < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCellsByRange",
                            $"Argument 'endRowIndex' must be a positive 1-based integer. Received: '{endRowRaw}'.");
                    }
                    if (!int.TryParse(endCellRaw?.ToString(), out var eCell) || eCell < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectCellsByRange",
                            $"Argument 'endCellIndex' must be a positive 1-based integer. Received: '{endCellRaw}'.");
                    }
                    end = (eRow - 1, eCell - 1);
                }

                var startZero = (row: sRow - 1, cell: sCell - 1);
                await _parent.SelectCellsByRangeAsync(startZero, end).ConfigureAwait(true);

                return new
                {
                    success = true,
                    startIndex = startZero,
                    endIndex = end
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "selectCellsByRange",
                    "Failed to select cells by range", ex);
            }
        }

        /// <summary>
        /// Deserialize an args-dictionary value into an array of (rowIndex, cellIndex) tuples
        /// for the cell-selection tools. All values are returned as 0-based to match the
        /// grid's public API. Validates that rowIndex/cellIndex are positive 1-based integers.
        /// </summary>
        private static ValueTuple<int, int>[] DeserializeCellTuples(object raw, string toolName)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(raw);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            if (items is null || items.Count == 0)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                    "Argument 'rowCellIndexes' must contain at least one { rowIndex, cellIndex } entry.");
            }

            var result = new ValueTuple<int, int>[items.Count];
            for (var i = 0; i < items.Count; i++)
            {
                var entry = items[i];
                if (!entry.TryGetValue("rowIndex", out var rRaw) || rRaw is null
                    || !int.TryParse(rRaw.ToString(), out var rOne) || rOne < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                        $"rowCellIndexes[{i}].rowIndex must be a positive 1-based integer.");
                }
                if (!entry.TryGetValue("cellIndex", out var cRaw) || cRaw is null
                    || !int.TryParse(cRaw.ToString(), out var cOne) || cOne < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                        $"rowCellIndexes[{i}].cellIndex must be a positive 1-based integer.");
                }
                result[i] = (rOne - 1, cOne - 1);
            }
            return result;
        }

        // PHASE 6 EXPANSION (28/41): SelectRowsByRangeAsync
        private async Task<object> HandleSelectRowsByRange(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("selectRowsByRange", "startIndex");
                var startRaw = args["startIndex"]?.ToString();
                if (!int.TryParse(startRaw, out var startOne) || startOne < 1)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRowsByRange",
                        $"Argument 'startIndex' must be a positive 1-based integer. Received: '{startRaw}'.");
                }
                int? endOne = null;
                if (args.TryGetValue("endIndex", out var endRaw) && endRaw is not null)
                {
                    if (!int.TryParse(endRaw.ToString(), out var parsed) || parsed < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "selectRowsByRange",
                            $"Argument 'endIndex' must be a positive 1-based integer. Received: '{endRaw}'.");
                    }
                    endOne = parsed;
                }

                // Convert 1-based to 0-based for the grid API.
                var s0 = startOne - 1;
                int? e0 = endOne.HasValue ? endOne.Value - 1 : (int?)null;
                await _parent.SelectRowsByRangeAsync(s0, e0).ConfigureAwait(true);

                return new
                {
                    success = true,
                    appliedRange = new { start = s0, end = e0 ?? s0 }
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "selectRowsByRange",
                    "Failed to select rows by range", ex);
            }
        }

        // PHASE 6 EXPANSION (29/41): SaveCellAsync
        private async Task<object> HandleSaveCell(Dictionary<string, object> args)
        {
            try
            {
                await _parent.SaveCellAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "saveCell",
                    "Failed to save the cell being edited", ex);
            }
        }

        // PHASE 6 EXPANSION (30/41): EditCellAsync
        private async Task<object> HandleEditCell(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("editCell", "index", "fieldName");
                var idxRaw = args["index"]?.ToString();
                if (!int.TryParse(idxRaw, out var idx) || idx < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "editCell",
                        $"Argument 'index' must be a non-negative integer. Received: '{idxRaw}'.");
                }
                var field = args.GetString("fieldName");
                if (string.IsNullOrWhiteSpace(field))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "editCell",
                        "Argument 'fieldName' must be a non-empty string.");
                }
                await _parent.EditCellAsync(idx, field!).ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "editCell",
                    "Failed to edit cell", ex);
            }
        }

        // PHASE 6 EXPANSION (31/41): UngroupColumnAsync
        private async Task<object> HandleUngroupColumn(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("ungroupColumn", "columnName");
                var columnName = args.GetString("columnName");
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "ungroupColumn",
                        "Argument 'columnName' must be a non-empty string.");
                }
                await _parent.UngroupColumnAsync(columnName!).ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "ungroupColumn",
                    "Failed to ungroup column", ex);
            }
        }

        // PHASE 6 EXPANSION (32/41): ExpandAllGroupAsync
        private async Task<object> HandleExpandAllGroups(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ExpandAllGroupAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "expandAllGroups",
                    "Failed to expand all grouped rows", ex);
            }
        }

        // PHASE 6 EXPANSION (33/41): CollapseAllGroupAsync
        private async Task<object> HandleCollapseAllGroups(Dictionary<string, object> args)
        {
            try
            {
                await _parent.CollapseAllGroupAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "collapseAllGroups",
                    "Failed to collapse all grouped rows", ex);
            }
        }

        // PHASE 6 EXPANSION (34/41): ClearGroupingAsync
        private async Task<object> HandleClearGrouping(Dictionary<string, object> args)
        {
            try
            {
                var snapshot = _parent.GroupSettings?.Columns?.Length ?? 0;
                await _parent.ClearGroupingAsync().ConfigureAwait(true);
                return new { success = true, removedCount = snapshot };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "clearGrouping",
                    "Failed to clear grouping", ex);
            }
        }

        // PHASE 6 EXPANSION (35/41): ShowColumnsAsync
        private async Task<object> HandleShowColumns(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("showColumns", "columnNames");
                var names = DeserializeStringList(args["columnNames"], "showColumns.columnNames");
                if (names.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "showColumns",
                        "Argument 'columnNames' must contain at least one column name.");
                }
                var showBy = args.GetString("showBy");
                if (string.IsNullOrWhiteSpace(showBy)) showBy = "HeaderText";
                if (!showBy.Equals("HeaderText", StringComparison.OrdinalIgnoreCase) && !showBy.Equals("Field", StringComparison.OrdinalIgnoreCase))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "showColumns",
                        $"Argument 'showBy' must be 'HeaderText' or 'Field'. Received: '{showBy}'.");
                }
                await _parent.ShowColumnsAsync(names.ToArray(), showBy).ConfigureAwait(true);
                return new { success = true, showBy = showBy, requestedCount = names.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "showColumns",
                    "Failed to show columns", ex);
            }
        }

        // PHASE 6 EXPANSION (36/41): HideColumnsAsync
        private async Task<object> HandleHideColumns(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("hideColumns", "columnNames");
                var names = DeserializeStringList(args["columnNames"], "hideColumns.columnNames");
                if (names.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "hideColumns",
                        "Argument 'columnNames' must contain at least one column name.");
                }
                var hideBy = args.GetString("hideBy");
                if (string.IsNullOrWhiteSpace(hideBy)) hideBy = "HeaderText";
                if (!hideBy.Equals("HeaderText", StringComparison.OrdinalIgnoreCase) && !hideBy.Equals("Field", StringComparison.OrdinalIgnoreCase))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "hideColumns",
                        $"Argument 'hideBy' must be 'HeaderText' or 'Field'. Received: '{hideBy}'.");
                }
                await _parent.HideColumnsAsync(names.ToArray(), hideBy).ConfigureAwait(true);
                return new { success = true, hideBy = hideBy, requestedCount = names.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "hideColumns",
                    "Failed to hide columns", ex);
            }
        }

        // PHASE 6 EXPANSION (37/41): ReorderColumnAsync (single)
        private async Task<object> HandleReorderColumn(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("reorderColumn", "fromFieldName", "toFieldName");
                var from = args.GetString("fromFieldName");
                var to = args.GetString("toFieldName");
                if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumn",
                        "Arguments 'fromFieldName' and 'toFieldName' must be non-empty strings.");
                }
                await _parent.ReorderColumnAsync(from!, to!).ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "reorderColumn",
                    "Failed to reorder column", ex);
            }
        }

        // PHASE 6 EXPANSION (38/41): ReorderColumnsAsync (multi)
        private async Task<object> HandleReorderColumns(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("reorderColumns", "fromFieldNames", "toFieldName");
                var from = DeserializeStringList(args["fromFieldNames"], "reorderColumns.fromFieldNames");
                var to = args.GetString("toFieldName");
                if (from.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumns",
                        "Argument 'fromFieldNames' must contain at least one column.");
                }
                if (string.IsNullOrWhiteSpace(to))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumns",
                        "Argument 'toFieldName' must be a non-empty string.");
                }
                await _parent.ReorderColumnsAsync(from, to!).ConfigureAwait(true);
                return new { success = true, movedCount = from.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "reorderColumns",
                    "Failed to reorder columns", ex);
            }
        }

        // PHASE 6 EXPANSION (39/41): ReorderColumnByIndexAsync
        private async Task<object> HandleReorderColumnByIndex(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("reorderColumnByIndex", "fromIndex", "toIndex");
                var fromRaw = args["fromIndex"]?.ToString();
                var toRaw = args["toIndex"]?.ToString();
                if (!int.TryParse(fromRaw, out var fromIdx) || fromIdx < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumnByIndex",
                        $"Argument 'fromIndex' must be a non-negative integer. Received: '{fromRaw}'.");
                }
                if (!int.TryParse(toRaw, out var toIdx) || toIdx < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumnByIndex",
                        $"Argument 'toIndex' must be a non-negative integer. Received: '{toRaw}'.");
                }
                await _parent.ReorderColumnByIndexAsync(fromIdx, toIdx).ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "reorderColumnByIndex",
                    "Failed to reorder column by index", ex);
            }
        }

        // PHASE 6 EXPANSION (40/41): ReorderColumnByTargetIndexAsync
        private async Task<object> HandleReorderColumnByTargetIndex(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("reorderColumnByTargetIndex", "fieldName", "toIndex");
                var field = args.GetString("fieldName");
                if (string.IsNullOrWhiteSpace(field))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumnByTargetIndex",
                        "Argument 'fieldName' must be a non-empty string.");
                }
                var toRaw = args["toIndex"]?.ToString();
                if (!int.TryParse(toRaw, out var toIdx) || toIdx < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderColumnByTargetIndex",
                        $"Argument 'toIndex' must be a non-negative integer. Received: '{toRaw}'.");
                }
                await _parent.ReorderColumnByTargetIndexAsync(field!, toIdx).ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "reorderColumnByTargetIndex",
                    "Failed to reorder column to a target index", ex);
            }
        }

        // PHASE 6 EXPANSION (41/41): AutoFitColumnsAsync / AutoFitColumnAsync
        private async Task<object> HandleAutoFitColumns(Dictionary<string, object> args)
        {
            try
            {
                if (args.TryGetValue("fieldNames", out var raw) && raw is not null)
                {
                    var names = DeserializeStringList(raw, "autoFitColumns.fieldNames");
                    if (names.Count == 0)
                    {
                        // Empty array → autofit every column, same as omitting the arg.
                        await _parent.AutoFitColumnsAsync().ConfigureAwait(true);
                        return new { success = true, scope = "all", fittedCount = 0 };
                    }
                    await _parent.AutoFitColumnsAsync(names.ToArray()).ConfigureAwait(true);
                    return new { success = true, scope = "specific", fittedCount = names.Count };
                }

                await _parent.AutoFitColumnsAsync().ConfigureAwait(true);
                return new { success = true, scope = "all", fittedCount = 0 };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "autoFitColumns",
                    "Failed to autofit columns", ex);
            }
        }

        // PHASE 6 EXPANSION (42/41): OpenColumnChooserAsync
        private async Task<object> HandleOpenColumnChooser(Dictionary<string, object> args)
        {
            try
            {
                double? x = null;
                double? y = null;
                var hasX = args.TryGetValue("x", out var xRaw) && xRaw is not null;
                var hasY = args.TryGetValue("y", out var yRaw) && yRaw is not null;
                if (hasX || hasY)
                {
                    if (!hasX || !hasY)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "openColumnChooser",
                            "Both 'x' and 'y' must be supplied together, or both omitted.");
                    }
                    try
                    {
                        x = Convert.ToDouble(xRaw, System.Globalization.CultureInfo.InvariantCulture);
                        y = Convert.ToDouble(yRaw, System.Globalization.CultureInfo.InvariantCulture);
                    }
                    catch (FormatException ex)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "openColumnChooser",
                            $"Arguments 'x' and 'y' must be numeric. {ex.Message}", ex);
                    }
                    catch (OverflowException ex)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "openColumnChooser",
                            $"Arguments 'x' and 'y' are out of range. {ex.Message}", ex);
                    }
                }

                await _parent.OpenColumnChooserAsync(x, y).ConfigureAwait(true);
                return new
                {
                    success = true,
                    position = (x.HasValue && y.HasValue) ? new { x = x, y = y } : null
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "openColumnChooser",
                    "Failed to open column chooser", ex);
            }
        }

        // PHASE 6 EXPANSION (43/46 — read-side lives in HandlersRead.cs)

        // PHASE 6 EXPANSION (44/46): Print / Copy / Paste / Spinner / Refresh / Focus / Scroll / Toolbar
        private async Task<object> HandlePrintGrid(Dictionary<string, object> args)
        {
            try
            {
                await _parent.PrintAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "printGrid",
                    "Failed to print grid", ex);
            }
        }

        private async Task<object> HandleCopyToClipboard(Dictionary<string, object> args)
        {
            try
            {
                bool? withHeader = null;
                if (args.TryGetValue("withHeader", out var raw) && raw is not null)
                {
                    if (raw is bool b) withHeader = b;
                    else if (bool.TryParse(raw.ToString(), out var parsed)) withHeader = parsed;
                }
                await _parent.CopyAsync(withHeader).ConfigureAwait(true);
                return new { success = true, withHeader = withHeader ?? false };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "copyToClipboard",
                    "Failed to copy to clipboard", ex);
            }
        }

        private async Task<object> HandlePasteFromClipboard(Dictionary<string, object> args)
        {
            try
            {
                await _parent.PasteAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "pasteFromClipboard",
                    "Failed to paste from clipboard", ex);
            }
        }

        private async Task<object> HandleShowSpinner(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ShowSpinnerAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "showSpinner",
                    "Failed to show spinner", ex);
            }
        }

        private async Task<object> HandleHideSpinner(Dictionary<string, object> args)
        {
            try
            {
                await _parent.HideSpinnerAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "hideSpinner",
                    "Failed to hide spinner", ex);
            }
        }

        private async Task<object> HandleRefreshGrid(Dictionary<string, object> args)
        {
            try
            {
                var isModel = args.GetBool("isModelRefresh");
                await _parent.Refresh(isModel).ConfigureAwait(true);
                return new { success = true, isModelRefresh = isModel };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "refreshGrid",
                    "Failed to refresh grid", ex);
            }
        }

        private async Task<object> HandleRefreshColumns(Dictionary<string, object> args)
        {
            try
            {
                await _parent.RefreshColumnsAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "refreshColumns",
                    "Failed to refresh columns", ex);
            }
        }

        private async Task<object> HandleRefreshHeader(Dictionary<string, object> args)
        {
            try
            {
                await _parent.RefreshHeaderAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "refreshHeader",
                    "Failed to refresh header", ex);
            }
        }

        private async Task<object> HandleFocusGrid(Dictionary<string, object> args)
        {
            try
            {
                await _parent.FocusAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "focusGrid",
                    "Failed to focus grid", ex);
            }
        }

        private async Task<object> HandleScrollIntoView(Dictionary<string, object> args)
        {
            try
            {
                int colIdx = -1;
                int rowIdx = -1;
                int rowHeight = -1;
                if (args.TryGetValue("columnIndex", out var cRaw) && cRaw is not null)
                {
                    if (!int.TryParse(cRaw.ToString(), out colIdx))
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "scrollIntoView",
                            $"Argument 'columnIndex' must be an integer. Received: '{cRaw}'.");
                    }
                }
                if (args.TryGetValue("rowIndex", out var rRaw) && rRaw is not null)
                {
                    if (!int.TryParse(rRaw.ToString(), out rowIdx))
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "scrollIntoView",
                            $"Argument 'rowIndex' must be an integer. Received: '{rRaw}'.");
                    }
                }
                if (args.TryGetValue("rowHeight", out var hRaw) && hRaw is not null)
                {
                    if (!int.TryParse(hRaw.ToString(), out rowHeight))
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "scrollIntoView",
                            $"Argument 'rowHeight' must be an integer. Received: '{hRaw}'.");
                    }
                }

                await _parent.ScrollIntoViewAsync(colIdx, rowIdx, rowHeight).ConfigureAwait(true);
                return new { success = true, columnIndex = colIdx, rowIndex = rowIdx };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "scrollIntoView",
                    "Failed to scroll into view", ex);
            }
        }

        private async Task<object> HandleEnableToolbarItems(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("enableToolbarItems", "items", "isEnable");
                var items = DeserializeStringList(args["items"], "enableToolbarItems.items");
                if (items.Count == 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "enableToolbarItems",
                        "Argument 'items' must contain at least one toolbar item ID.");
                }
                var isEnable = args.GetBool("isEnable");
                await _parent.EnableToolbarItemsAsync(items, isEnable).ConfigureAwait(true);
                return new { success = true, affectedCount = items.Count, isEnable = isEnable };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "enableToolbarItems",
                    "Failed to enable/disable toolbar items", ex);
            }
        }

        // PHASE 7 EXPANSION (57/64): detail-row toggle by data OR by field/value
        private async Task<object> HandleExpandCollapseDetailRow(Dictionary<string, object> args)
        {
            try
            {
                object? dataRaw = null;
                object? valueRaw = null;
                var hasData = args.TryGetValue("data", out dataRaw) && dataRaw is not null;
                var hasField = args.TryGetValue("fieldName", out var fieldRaw) && !string.IsNullOrWhiteSpace(fieldRaw?.ToString())
                               && args.TryGetValue("value", out valueRaw) && valueRaw is not null;

                if (!hasData && !hasField)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "expandCollapseDetailRow",
                        "Supply either 'data' (TValue reference) or both 'fieldName' and 'value'.");
                }

                if (hasData)
                {
                    var data = DeserializeCrudRecord(dataRaw!, "expandCollapseDetailRow");
                    await _parent.ExpandCollapseDetailRowAsync(data).ConfigureAwait(true);
                    return new { success = true, matchMode = "data" };
                }

                var fieldName = fieldRaw!.ToString()!;
                await _parent.ExpandCollapseDetailRowAsync(fieldName, valueRaw!).ConfigureAwait(true);
                return new { success = true, matchMode = "field" };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "expandCollapseDetailRow",
                    "Failed to toggle detail row", ex);
            }
        }

        // PHASE 7 EXPANSION (58/64): expand all detail rows
        private async Task<object> HandleExpandAllDetailRows(Dictionary<string, object> args)
        {
            try
            {
                await _parent.ExpandAllDetailRowAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "expandAllDetailRows",
                    "Failed to expand all detail rows", ex);
            }
        }

        // PHASE 7 EXPANSION (59/64): collapse all detail rows
        private async Task<object> HandleCollapseAllDetailRows(Dictionary<string, object> args)
        {
            try
            {
                await _parent.CollapseAllDetailRowAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "collapseAllDetailRows",
                    "Failed to collapse all detail rows", ex);
            }
        }

        // PHASE 7 EXPANSION (60/64): merge a single rectangular region
        private async Task<object> HandleMergeCells(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("mergeCells", "rowIndex", "columnIndex");
                if (!TryGetNonNegativeInt(args, "rowIndex", "mergeCells", out var rowIndex))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "mergeCells",
                        "Argument 'rowIndex' must be a non-negative integer.");
                }
                if (!TryGetNonNegativeInt(args, "columnIndex", "mergeCells", out var columnIndex))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "mergeCells",
                        "Argument 'columnIndex' must be a non-negative integer.");
                }

                var rowSpan = 1;
                var colSpan = 1;
                if (args.TryGetValue("rowSpan", out var rsRaw) && rsRaw is not null)
                {
                    if (!int.TryParse(rsRaw.ToString(), out rowSpan) || rowSpan < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "mergeCells",
                            "Argument 'rowSpan' must be a positive integer (>=1).");
                    }
                }
                if (args.TryGetValue("columnSpan", out var csRaw) && csRaw is not null)
                {
                    if (!int.TryParse(csRaw.ToString(), out colSpan) || colSpan < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "mergeCells",
                            "Argument 'columnSpan' must be a positive integer (>=1).");
                    }
                }

                var info = new MergeCellInfo
                {
                    RowIndex = rowIndex,
                    ColumnIndex = columnIndex,
                    RowSpan = rowSpan,
                    ColumnSpan = colSpan
                };
                await _parent.MergeCellsAsync(info).ConfigureAwait(true);
                return new
                {
                    success = true,
                    rowIndex = rowIndex,
                    columnIndex = columnIndex,
                    rowSpan = rowSpan,
                    columnSpan = colSpan
                };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "mergeCells",
                    "Failed to merge cells", ex);
            }
        }

        // PHASE 7 EXPANSION (61/64): merge multiple regions
        private async Task<object> HandleMergeCellsBulk(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("mergeCellsBulk", "infos");
                var infos = DeserializeMergeInfos(args["infos"], "mergeCellsBulk.infos");
                await _parent.MergeCellsAsync(infos).ConfigureAwait(true);
                return new { success = true, appliedCount = infos.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "mergeCellsBulk",
                    "Failed to merge cells in bulk", ex);
            }
        }

        // PHASE 7 EXPANSION (62/64): unmerge a single region
        private async Task<object> HandleUnmergeCells(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("unmergeCells", "rowIndex", "columnIndex");
                if (!TryGetNonNegativeInt(args, "rowIndex", "unmergeCells", out var rowIndex))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "unmergeCells",
                        "Argument 'rowIndex' must be a non-negative integer.");
                }
                if (!TryGetNonNegativeInt(args, "columnIndex", "unmergeCells", out var columnIndex))
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "unmergeCells",
                        "Argument 'columnIndex' must be a non-negative integer.");
                }

                var info = new UnmergeCellInfo
                {
                    RowIndex = rowIndex,
                    ColumnIndex = columnIndex
                };
                await _parent.UnmergeCellsAsync(info).ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "unmergeCells",
                    "Failed to unmerge cells", ex);
            }
        }

        // PHASE 7 EXPANSION (63/64): unmerge multiple regions
        private async Task<object> HandleUnmergeCellsBulk(Dictionary<string, object> args)
        {
            try
            {
                args.ValidateRequired("unmergeCellsBulk", "infos");
                var infos = DeserializeUnmergeInfos(args["infos"], "unmergeCellsBulk.infos");
                await _parent.UnmergeCellsAsync(infos).ConfigureAwait(true);
                return new { success = true, requestedCount = infos.Count };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "unmergeCellsBulk",
                    "Failed to unmerge cells in bulk", ex);
            }
        }

        // PHASE 7 EXPANSION (64/64): unmerge all regions
        private async Task<object> HandleUnmergeAll(Dictionary<string, object> args)
        {
            try
            {
                await _parent.UnmergeAllAsync().ConfigureAwait(true);
                return new { success = true };
            }
            catch (WebMcpException) { throw; }
            catch (Exception ex)
            {
                throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "unmergeAll",
                    "Failed to unmerge all cells", ex);
            }
        }

        /// <summary>
        /// Parse a non-negative integer out of <paramref name="args"/> for the supplied key.
        /// </summary>
        private static bool TryGetNonNegativeInt(Dictionary<string, object> args, string key, string toolName, out int value)
        {
            value = 0;
            if (!args.TryGetValue(key, out var raw) || raw is null) return false;
            if (!int.TryParse(raw.ToString(), out var parsed) || parsed < 0)
            {
                return false;
            }
            value = parsed;
            return true;
        }

        /// <summary>
        /// Deserialize an args-dictionary value into a list of <see cref="MergeCellInfo"/> for
        /// the bulk merge tool. Validates that rowIndex/columnIndex are non-negative and that
        /// rowSpan/columnSpan are positive when supplied.
        /// </summary>
        private static List<MergeCellInfo> DeserializeMergeInfos(object raw, string path)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(raw);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            if (items is null || items.Count == 0)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                    "Argument 'infos' must contain at least one entry.");
            }

            var result = new List<MergeCellInfo>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                var entry = items[i];
                if (!entry.TryGetValue("rowIndex", out var rRaw) || rRaw is null
                    || !int.TryParse(rRaw.ToString(), out var rowIndex) || rowIndex < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                        $"infos[{i}].rowIndex must be a non-negative integer.");
                }
                if (!entry.TryGetValue("columnIndex", out var cRaw) || cRaw is null
                    || !int.TryParse(cRaw.ToString(), out var colIndex) || colIndex < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                        $"infos[{i}].columnIndex must be a non-negative integer.");
                }

                var rowSpan = 1;
                if (entry.TryGetValue("rowSpan", out var rsRaw) && rsRaw is not null)
                {
                    if (!int.TryParse(rsRaw.ToString(), out rowSpan) || rowSpan < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                            $"infos[{i}].rowSpan must be a positive integer (>=1).");
                    }
                }

                var colSpan = 1;
                if (entry.TryGetValue("columnSpan", out var csRaw) && csRaw is not null)
                {
                    if (!int.TryParse(csRaw.ToString(), out colSpan) || colSpan < 1)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                            $"infos[{i}].columnSpan must be a positive integer (>=1).");
                    }
                }

                result.Add(new MergeCellInfo
                {
                    RowIndex = rowIndex,
                    ColumnIndex = colIndex,
                    RowSpan = rowSpan,
                    ColumnSpan = colSpan
                });
            }
            return result;
        }

        /// <summary>
        /// Deserialize an args-dictionary value into a list of <see cref="UnmergeCellInfo"/>
        /// for the bulk unmerge tool. Validates that rowIndex/columnIndex are non-negative.
        /// </summary>
        private static List<UnmergeCellInfo> DeserializeUnmergeInfos(object raw, string path)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(raw);
            var items = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>(json);
            if (items is null || items.Count == 0)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                    "Argument 'infos' must contain at least one entry.");
            }

            var result = new List<UnmergeCellInfo>(items.Count);
            for (var i = 0; i < items.Count; i++)
            {
                var entry = items[i];
                if (!entry.TryGetValue("rowIndex", out var rRaw) || rRaw is null
                    || !int.TryParse(rRaw.ToString(), out var rowIndex) || rowIndex < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                        $"infos[{i}].rowIndex must be a non-negative integer.");
                }
                if (!entry.TryGetValue("columnIndex", out var cRaw) || cRaw is null
                    || !int.TryParse(cRaw.ToString(), out var colIndex) || colIndex < 0)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, path,
                        $"infos[{i}].columnIndex must be a non-negative integer.");
                }
                result.Add(new UnmergeCellInfo { RowIndex = rowIndex, ColumnIndex = colIndex });
        }
        return result;
    }

    private async Task<object> HandleBatchEditWorkflow(Dictionary<string, object> args)
    {
        try
        {
            args.ValidateRequired("batchEditWorkflow", "action");
            EnsureBatchEditMode("batchEditWorkflow");
            var action = args.GetString("action").Trim().ToUpperInvariant();
            switch (action)
            {
                case "INSPECT":
                case "ISDIRTY":
                {
                    var changes = await _parent.GetBatchChangesAsync().ConfigureAwait(true);
                    ValidateBatchChanges(changes);
                    var isDirty = changes.AddedRecords.Count > 0
                        || changes.ChangedRecords.Count > 0
                        || changes.DeletedRecords.Count > 0;
                    return new { success = true, action = action == "INSPECT" ? "inspect" : "isdirty", isDirty, changes };
                }
                case "UPDATEROW":
                {
                    if (_parent.EditSettings?.AllowEditing != true)
                    {
                        throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "batchEditWorkflow",
                            "Updating a staged row requires GridEditSettings.AllowEditing=true.");
                    }
                    args.ValidateRequired("batchEditWorkflow", "rowData");
                    var row = DeserializeCrudRecord(args["rowData"], "batchEditWorkflow.rowData");
                    await _parent.UpdateBatchRowAsync(row).ConfigureAwait(true);
                    return new { success = true, action = "updaterow", isDirty = true };
                }
                case "APPLY":
                {
                    args.ValidateRequired("batchEditWorkflow", "batchChanges");
                    var json = System.Text.Json.JsonSerializer.Serialize(args["batchChanges"]);
                    var changes = System.Text.Json.JsonSerializer.Deserialize<BatchChanges<TValue>>(
                        json,
                        _caseInsensitiveJsonSerializerOptions);
                    if (changes is null)
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "batchEditWorkflow",
                            "Argument 'batchChanges' must be a valid batch changes object.");
                    }
                    ValidateBatchChanges(changes);
                    ValidateBatchEditPermissions(changes);
                    await _parent.ApplyBatchChangesAsync(changes).ConfigureAwait(true);
                    return new
                    {
                        success = true,
                        action = "apply",
                        appliedCounts = new
                        {
                            added = changes.AddedRecords.Count,
                            changed = changes.ChangedRecords.Count,
                            deleted = changes.DeletedRecords.Count
                        }
                    };
                }
                default:
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "batchEditWorkflow",
                        $"Unknown action '{action}'. Use inspect, isDirty, updateRow, or apply.");
            }
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "batchEditWorkflow",
                "Failed to execute the batch-edit workflow", ex);
        }
    }

    private async Task<object> HandleUpdateGridData(Dictionary<string, object> args)
    {
        try
        {
            args.ValidateRequired("updateGridData", "action");

            var action = args.GetString("action").Trim().ToUpperInvariant();
            var rowIndex = ParsePositiveIndex(
                args.TryGetValue("rowIndex", out var rawIndex) && rawIndex is not null ? rawIndex : null!,
                "updateGridData",
                "rowIndex");
            var view = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
            if (view is null || rowIndex > view.Count)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "updateGridData",
                    $"Argument 'rowIndex' ({rowIndex}) is outside the current view.");
            }

            var zeroBasedIndex = rowIndex - 1;
            switch (action)
            {
                case "CELL":
                    args.ValidateRequired("updateGridData", "fieldName");
                    if (!args.TryGetValue("value", out var cellValue))
                    {
                        throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "updateGridData",
                            "Argument 'value' is required for action='cell'; pass null to clear the cell.");
                    }
                    if (_parent.EditSettings?.Mode != EditMode.Batch)
                    {
                        throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "updateGridData",
                            "Updating a cell through this API requires EditMode.Batch.");
                    }
                    await _parent.UpdateCellAsync(
                        zeroBasedIndex,
                        args.GetString("fieldName"),
                        ConvertJsonValue(cellValue, GetFieldType(args.GetString("fieldName")),
                            "updateGridData", "value")!).ConfigureAwait(true);
                    break;
                case "ROW":
                    args.ValidateRequired("updateGridData", "rowData");
                    var row = DeserializeCrudRecord(args["rowData"], "updateGridData.rowData");
                    await _parent.UpdateRowAsync(zeroBasedIndex, row).ConfigureAwait(true);
                    break;
                default:
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "updateGridData",
                        $"Unknown action '{action}'. Use cell or row.");
            }

            return new { success = true, action, rowIndex };
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "updateGridData",
                "Failed to update grid data", ex);
        }
    }

    private async Task<object> HandleReorderRow(Dictionary<string, object> args)
    {
        try
        {
            args.ValidateRequired("reorderRow", "fromIndex", "toIndex");
            var fromIndex = ParsePositiveIndex(args["fromIndex"], "reorderRow", "fromIndex");
            var toIndex = ParsePositiveIndex(args["toIndex"], "reorderRow", "toIndex");
            var view = await _parent.GetCurrentViewRecordsAsync().ConfigureAwait(true);
            if (view is null || fromIndex > view.Count || toIndex > view.Count)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "reorderRow",
                    $"Both indexes must be within the current view (1-{view?.Count ?? 0}).");
            }
            if (_parent.RowReorderModule is null)
            {
                throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "reorderRow",
                    "Row reordering is unavailable. Enable the grid's row drag-and-drop feature.");
            }

            await _parent.ReorderRowAsync(fromIndex - 1, toIndex - 1).ConfigureAwait(true);
            return new { success = true, fromIndex, toIndex };
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "reorderRow",
                "Failed to reorder the row", ex);
        }
    }

    private async Task<object> HandlePersistGridState(Dictionary<string, object> args)
    {
        try
        {
            args.ValidateRequired("persistGridState", "action");
            var action = args.GetString("action").Trim().ToUpperInvariant();
            switch (action)
            {
                case "GET":
                    return new
                    {
                        success = true,
                        action = "get",
                        data = await _parent.GetPersistDataAsync().ConfigureAwait(true)
                    };
                case "SET":
                    args.ValidateRequired("persistGridState", "properties");
                    await _parent.SetPersistDataAsync(args.GetString("properties")).ConfigureAwait(true);
                    return new { success = true, action = "set" };
                case "RESET":
                    await _parent.ResetPersistDataAsync().ConfigureAwait(true);
                    return new { success = true, action = "reset" };
                default:
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "persistGridState",
                        $"Unknown action '{action}'. Use get, set, or reset.");
            }
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "persistGridState",
                "Failed to access persisted grid state", ex);
        }
    }

    private async Task<object> HandleUpdatePageSize(Dictionary<string, object> args)
    {
        try
        {
            args.ValidateRequired("updatePageSize", "action");
            var action = args.GetString("action").Trim().ToUpperInvariant();
            if (action == "FIXED")
            {
                var pageSize = ParsePositiveIndex(
                    args.TryGetValue("pageSize", out var rawPageSize) && rawPageSize is not null ? rawPageSize : null!,
                    "updatePageSize",
                    "pageSize");
                if (!_parent.AllowPaging)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "updatePageSize",
                        "Changing a fixed page size requires AllowPaging=true.");
                }
                await _parent.UpdatePageSizeAsync(pageSize).ConfigureAwait(true);
                return new { success = true, action = "fixed", pageSize = _parent.PageSettings?.PageSize ?? pageSize };
            }
            if (action == "VIEWPORT")
            {
                var height = ParsePositiveIndex(
                    args.TryGetValue("height", out var rawHeight) && rawHeight is not null ? rawHeight : null!,
                    "updatePageSize",
                    "height");
                var rowHeight = ParsePositiveIndex(
                    args.TryGetValue("rowHeight", out var rawRowHeight) && rawRowHeight is not null ? rawRowHeight : null!,
                    "updatePageSize",
                    "rowHeight");
                if (!_parent.EnableVirtualization)
                {
                    throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "updatePageSize",
                        "Viewport-based page-size updates require EnableVirtualization=true.");
                }
                if (height < rowHeight)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "updatePageSize",
                        "Argument 'height' must be at least as large as 'rowHeight'.");
                }
                await _parent.UpdatePageSizeAsync(height, rowHeight).ConfigureAwait(true);
                return new { success = true, action = "viewport", pageSize = _parent.PageSettings?.PageSize ?? 0 };
            }
            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "updatePageSize",
                $"Unknown action '{action}'. Use fixed or viewport.");
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "updatePageSize",
                "Failed to update page size", ex);
        }
    }

    private async Task<object> HandleUpdateExternalMessage(Dictionary<string, object> args)
    {
        try
        {
            if (!args.TryGetValue("message", out var message) || message is null)
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "updateExternalMessage",
                    "Argument 'message' is required; pass an empty string to clear the external message.");
            }
            if (_parent.PageSettings is null)
            {
                throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "updateExternalMessage",
                    "The grid must have GridPageSettings configured to update the external message.");
            }
            await _parent.UpdateExternalMessageAsync(message.ToString() ?? string.Empty).ConfigureAwait(true);
            return new { success = true };
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "updateExternalMessage",
                "Failed to update the pager external message", ex);
        }
    }

    private async Task<object> HandleUndoRedoHistory(Dictionary<string, object> args)
    {
        try
        {
            args.ValidateRequired("undoRedoHistory", "action");
            EnsureUndoRedoEnabled("undoRedoHistory");
            var action = args.GetString("action").Trim().ToUpperInvariant();
            var manager = _parent.UndoRedoManager;
            var undoBefore = manager?.UndoCount ?? 0;
            var redoBefore = manager?.RedoCount ?? 0;
            switch (action)
            {
                case "undoall":
                    await _parent.UndoAllAsync().ConfigureAwait(true);
                    break;
                case "redoall":
                    await _parent.RedoAllAsync().ConfigureAwait(true);
                    break;
                case "clear":
                    await _parent.ClearUndoRedoAsync().ConfigureAwait(true);
                    break;
                default:
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "undoRedoHistory",
                        $"Unknown action '{action}'. Use undoAll, redoAll, or clear.");
            }

            return new
            {
                success = true,
                action,
                undoCount = manager?.UndoCount ?? 0,
                redoCount = manager?.RedoCount ?? 0,
                undoCountBefore = undoBefore,
                redoCountBefore = redoBefore
            };
        }
        catch (WebMcpException) { throw; }
        catch (Exception ex)
        {
            throw new WebMcpException(WebMcpErrorCode.ExecutionFailed, "undoRedoHistory",
                "Failed to update undo/redo history", ex);
        }
    }

    private void EnsureBatchEditMode(string toolName)
    {
        if (_parent.EditSettings?.Mode != EditMode.Batch)
        {
            throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, toolName,
                "This operation requires GridEditSettings.Mode=EditMode.Batch.");
        }
    }

    private void ValidateBatchEditPermissions(BatchChanges<TValue> changes)
    {
        var settings = _parent.EditSettings!;
        if (changes.AddedRecords.Count > 0 && !settings.AllowAdding)
        {
            throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "batchEditWorkflow",
                "Applying added records requires GridEditSettings.AllowAdding=true.");
        }
        if (changes.ChangedRecords.Count > 0 && !settings.AllowEditing)
        {
            throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "batchEditWorkflow",
                "Applying changed records requires GridEditSettings.AllowEditing=true.");
        }
        if (changes.DeletedRecords.Count > 0 && !settings.AllowDeleting)
        {
            throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, "batchEditWorkflow",
                "Applying deleted records requires GridEditSettings.AllowDeleting=true.");
        }
    }

    private static void ValidateBatchChanges(BatchChanges<TValue> changes)
    {
        if (changes.AddedRecords is null || changes.ChangedRecords is null || changes.DeletedRecords is null)
        {
            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "batchEditWorkflow",
                "Batch changes must provide non-null addedRecords, changedRecords, and deletedRecords arrays.");
        }
    }

    private void EnsureUndoRedoEnabled(string toolName)
    {
        if (_parent.EditSettings?.EnableUndoRedo != true || _parent.EditSettings.Mode != EditMode.Batch)
        {
            throw new WebMcpException(WebMcpErrorCode.OperationNotSupported, toolName,
                "Undo/redo history requires EnableUndoRedo=true and EditMode.Batch.");
        }
    }

    private static Type? GetFieldType(string? fieldPath)
    {
        if (string.IsNullOrWhiteSpace(fieldPath))
        {
            return null;
        }

        var currentType = typeof(TValue);
        var segments = fieldPath.Split('.');
        for (var i = 0; i < segments.Length; i++)
        {
            var property = currentType.GetProperty(
                segments[i],
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.Public |
                System.Reflection.BindingFlags.IgnoreCase);
            if (property is null)
            {
                return null;
            }
            currentType = property.PropertyType;
            if (i < segments.Length - 1)
            {
                currentType = Nullable.GetUnderlyingType(currentType) ?? currentType;
            }
        }
        return currentType;
    }

    private static object? ConvertJsonValue(
        object? value,
        Type? targetType = null,
        string toolName = "setCellValue",
        string argumentName = "value")
    {
        if (value is System.Text.Json.JsonElement element)
        {
            if (targetType is not null && targetType != typeof(object))
            {
                try
                {
                    return System.Text.Json.JsonSerializer.Deserialize(
                        element.GetRawText(),
                        targetType,
                        _caseInsensitiveJsonSerializerOptions);
                }
                catch (System.Text.Json.JsonException ex)
                {
                    throw new WebMcpException(WebMcpErrorCode.InvalidArgument, toolName,
                        $"Argument '{argumentName}' is not a valid value for {targetType.Name}.", ex);
                }
            }
        }
        else
        {
            return value;
        }

        switch (element.ValueKind)
        {
            case System.Text.Json.JsonValueKind.Null:
            case System.Text.Json.JsonValueKind.Undefined:
                return null;
            case System.Text.Json.JsonValueKind.String:
                return element.GetString();
            case System.Text.Json.JsonValueKind.True:
                return true;
            case System.Text.Json.JsonValueKind.False:
                return false;
            case System.Text.Json.JsonValueKind.Number:
                if (element.TryGetInt64(out var integer))
                {
                    return integer;
                }
                if (element.TryGetDecimal(out var decimalValue))
                {
                    return decimalValue;
                }
                return element.GetDouble();
            case System.Text.Json.JsonValueKind.Array:
                return element.EnumerateArray().Select(item => ConvertJsonValue(item)).ToList();
            case System.Text.Json.JsonValueKind.Object:
                return element.EnumerateObject().ToDictionary(
                    property => property.Name,
                    property => ConvertJsonValue(property.Value),
                    StringComparer.OrdinalIgnoreCase);
            default:
                return element.Clone();
        }
    }

    private static List<CellUpdate> DeserializeCellUpdates(object raw)
    {
        var json = System.Text.Json.JsonSerializer.Serialize(raw);
        List<CellUpdate>? updates;
        try
        {
            updates = System.Text.Json.JsonSerializer.Deserialize<List<CellUpdate>>(
                json,
                _caseInsensitiveJsonSerializerOptions);
        }
        catch (System.Text.Json.JsonException ex)
        {
            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "setCellValue",
                "Argument 'cellUpdates' must contain valid update objects with primaryKeyValue and columnField.", ex);
        }
        if (updates is null)
        {
            throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "setCellValue",
                "Argument 'cellUpdates' must be an array of cell update objects.");
        }
        for (var i = 0; i < updates.Count; i++)
        {
            var update = updates[i];
            if (update.PrimaryKeyValue is null || string.IsNullOrWhiteSpace(update.ColumnField))
            {
                throw new WebMcpException(WebMcpErrorCode.InvalidArgument, "setCellValue",
                    $"cellUpdates[{i}] requires a non-null primaryKeyValue and non-empty columnField.");
            }
        }
        return updates;
    }

    }
}
