using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.QueryBuilder;

namespace Syncfusion.Blazor.Grids.Internal
{
    internal partial class Filter<T>
    {
        #region Internal Properties

        internal AdvancedFilterRule? ActiveAdvancedFilterRule { get; private set; }

        internal bool LastAdvancedFilteredCancelled { get; private set; }

        #endregion

        #region Internal Methods

        internal void RestoreAdvancedFilterRule(AdvancedFilterRule? rule)
        {
            ActiveAdvancedFilterRule = rule;

            var where = rule == null
                ? null
                : AdvancedFilterRuleConverter.ConvertRule(rule, Parent.Columns);
            Parent.AdvancedFilterWhereFilter = where?.Count > 0 ? where : null;
        }

        internal async Task ApplyAdvancedFilter(AdvancedFilterRule rule)
        {
            if (rule == null)
            {
                return;
            }

            ActiveAdvancedFilterRule = rule;
            var begin = CreateAdvancedFilterBeginArgs(rule, isClear: false);
            if (await RaiseAdvancedFilterBeginAsync(begin).ConfigureAwait(true))
            {
                LastAdvancedFilteredCancelled = false;
                return;
            }

            var where = AdvancedFilterRuleConverter.ConvertRule(rule, Parent.Columns);
            if (where.Count > 0)
            {
                Parent.AdvancedFilterWhereFilter = where;
            }

            await Parent.ModelChanged(
                new ActionEventArgs<T> { RequestType = Action.AdvancedFilter },
                requestType: "AdvancedFilter").ConfigureAwait(true);

            await RaiseAdvancedFilterCompleteAsync(CreateAdvancedFilterCompleteArgs(rule, isClear: false)).ConfigureAwait(true);
        }

        internal async Task ClearAdvancedFilter()
        {
            if (ActiveAdvancedFilterRule == null)
            {
                return;
            }

            var begin = CreateAdvancedFilterBeginArgs(ActiveAdvancedFilterRule, isClear: true);
            if (await RaiseAdvancedFilterBeginAsync(begin).ConfigureAwait(true))
            {
                LastAdvancedFilteredCancelled = false;
                return;
            }

            ActiveAdvancedFilterRule = null;
            Parent.AdvancedFilterWhereFilter = null;

            await Parent.ModelChanged(
                new ActionEventArgs<T> { RequestType = Action.ClearAdvancedFilter },
                requestType: "ClearAdvancedFilter").ConfigureAwait(true);

            await RaiseAdvancedFilterCompleteAsync(CreateAdvancedFilterCompleteArgs(rule: null, isClear: true)).ConfigureAwait(true);
        }

        #endregion

        #region Private Helper Methods

        private AdvancedFilteringEventArgs CreateAdvancedFilterBeginArgs(AdvancedFilterRule? rule, bool isClear)
        {
            return new AdvancedFilteringEventArgs
            {
                Filter = rule,
                IsClear = isClear,
                Dialog = Parent.AdvancedFilterDialogInstance,
                QueryBuilder = Parent.AdvancedFilterQueryBuilderInstance
            };
        }

        private AdvancedFilteredEventArgs CreateAdvancedFilterCompleteArgs(AdvancedFilterRule? rule, bool isClear)
        {
            return new AdvancedFilteredEventArgs
            {
                Filter = rule,
                IsClear = isClear,
                Dialog = Parent.AdvancedFilterDialogInstance,
                QueryBuilder = Parent.AdvancedFilterQueryBuilderInstance
            };
        }

        private async Task<bool> RaiseAdvancedFilterBeginAsync(AdvancedFilteringEventArgs begin)
        {
            if (Parent.GridEvents?.AdvancedFiltering.HasDelegate != true)
            {
                return false;
            }

            await Parent.GridEvents.AdvancedFiltering.InvokeAsync(begin).ConfigureAwait(true);
            return begin.Cancel;
        }

        private async Task RaiseAdvancedFilterCompleteAsync(AdvancedFilteredEventArgs complete)
        {
            LastAdvancedFilteredCancelled = false;
            if (Parent.GridEvents?.AdvancedFiltered.HasDelegate != true)
            {
                return;
            }

            await Parent.GridEvents.AdvancedFiltered.InvokeAsync(complete).ConfigureAwait(true);
            LastAdvancedFilteredCancelled = complete.Cancel;
        }

        #endregion
    }

    /// <summary>
    /// Converts a Grid-owned <see cref="AdvancedFilterRule"/> into the
    /// <see cref="List{WhereFilter}"/> shape consumed by the
    /// grid's runtime data pipeline.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The Query Builder package's <c>SfQueryBuilder.GetDataManagerQuery</c>
    /// produces a <c>WhereFilter</c> that does not know about the grid's
    /// column schema — the runtime then fails to coerce the rule's
    /// <c>Value</c> to the column's actual data type, so operators such as
    /// <c>startswith</c> on string columns never match. This converter
    /// walks the rule tree and builds <c>WhereFilter</c> instances with
    /// the right <c>ColumnType</c>, coerced <c>value</c>, and the
    /// canonical operator string the runtime understands.
    /// </para>
    /// <para>
    /// Operator mapping: the Query Builder already uses the lowercase
    /// names that <c>Syncfusion.Blazor.Data</c> understands
    /// (<c>equal</c>, <c>notequal</c>, <c>startswith</c>, <c>endswith</c>,
    /// <c>contains</c>, <c>greaterthan</c>, …, <c>in</c>, <c>notin</c>,
    /// <c>between</c>, <c>notbetween</c>, <c>isnull</c>, <c>isnotnull</c>,
    /// <c>isempty</c>, <c>isnotempty</c>). The mapping is normalised here
    /// so we control what the runtime sees.
    /// </para>
    /// </remarks>
    internal static class AdvancedFilterRuleConverter
    {
        #region Fields

        private static readonly string[] UnaryOperators =
        {
            "isnull", "isnotnull", "isempty", "isnotempty"
        };

        private static readonly string[] ArrayOperators =
        {
            "in", "notin", "between", "notbetween"
        };

        #endregion

        #region Rule Model Conversion

        /// <summary>
        /// Converts a Grid-owned rule to the Query Builder model used by the
        /// Advanced Filter dialog.
        /// </summary>
        internal static RuleModel? ToRuleModel(AdvancedFilterRule? rule)
        {
            if (rule == null)
            {
                return null;
            }

            var node = new RuleModel
            {
                Condition = rule.Condition!,
                Not = rule.IsRuleNegated,
                IsLocked = rule.IsLocked,
                Field = rule.Field!,
                Operator = rule.Operator!,
                Value = rule.Value!
            };

            if (rule.Rules != null && rule.Rules.Count > 0)
            {
                var children = new List<RuleModel>(rule.Rules.Count);
                foreach (var child in rule.Rules)
                {
                    var mapped = ToRuleModel(child);
                    if (mapped != null)
                    {
                        children.Add(mapped);
                    }
                }

                if (children.Count > 0)
                {
                    node.Rules = children;
                }
            }

            return node;
        }

        /// <summary>
        /// Converts a Query Builder model to the Grid-owned rule used by the
        /// Advanced Filter public API.
        /// </summary>
        internal static AdvancedFilterRule? FromRuleModel(RuleModel? rule)
        {
            if (rule == null)
            {
                return null;
            }

            var node = new AdvancedFilterRule
            {
                Condition = rule.Condition,
                IsRuleNegated = rule.Not,
                IsLocked = rule.IsLocked,
                Field = rule.Field,
                Operator = rule.Operator,
                Value = rule.Value
            };

            if (rule.Rules != null && rule.Rules.Count > 0)
            {
                var children = new List<AdvancedFilterRule>(rule.Rules.Count);
                foreach (var child in rule.Rules)
                {
                    var mapped = FromRuleModel(child);
                    if (mapped != null)
                    {
                        children.Add(mapped);
                    }
                }

                if (children.Count > 0)
                {
                    node.Rules = children;
                }
            }

            return node;
        }

        #endregion

        #region WhereFilter Conversion

        /// <summary>
        /// Grid-owned entry point: converts a public
        /// <see cref="AdvancedFilterRule"/> tree into the
        /// <see cref="List{WhereFilter}"/> shape consumed by the grid's
        /// runtime data pipeline.
        /// </summary>
        internal static List<WhereFilter> ConvertRule(AdvancedFilterRule rule,List<GridColumn>? columns)
        {
            var result = new List<WhereFilter>();
            if (rule == null)
            {
                return result;
            }

            Build(rule, columns, result);
            return result;
        }

        private static void Build(AdvancedFilterRule rule,List<GridColumn>? columns,List<WhereFilter> output,bool negate = false)
        {
            negate ^= rule.IsRuleNegated == true;

            // Group node: has child rules joined by a logical Condition.
            if (rule.Rules != null && rule.Rules.Count > 0)
            {
                var children = new List<WhereFilter>();
                foreach (var child in rule.Rules)
                {
                    Build(child, columns, children, negate);
                }

                if (children.Count == 0)
                {
                    return;
                }

                if (children.Count == 1)
                {
                    output.Add(children[0]);
                    return;
                }

                var condition = NormaliseCondition(rule.Condition);
                if (negate)
                {
                    condition = condition == "and" ? "or" : "and";
                }

                var combined = condition == "or"
                    ? WhereFilter.Or(children)
                    : WhereFilter.And(children);
                output.Add(combined);
                return;
            }

            // Leaf node: a single condition.
            if (string.IsNullOrEmpty(rule.Field) || string.IsNullOrEmpty(rule.Operator))
            {
                return;
            }

            var gridColumn = LookupColumn(columns, rule.Field);
            var operatorName = NormaliseOperator(rule.Operator);
            if (negate)
            {
                operatorName = NegateOperator(operatorName);
            }

            var ignoreCase = ShouldIgnoreCase(gridColumn, operatorName);
            var coercedValue = CoerceValue(rule.Value, gridColumn);

            var where = new WhereFilter
            {
                Field = rule.Field,
                Operator = operatorName,
                value = coercedValue,
                IgnoreCase = ignoreCase,
                IsComplex = false,
                Condition = "and"
            };

            if (gridColumn != null)
            {
                where.ColumnType = gridColumn.Type.ToString();
            }

            // Array operators (in / notin / between / notbetween) must carry
            // a List<object> as value; everything else a single object.
            if (Array.IndexOf(ArrayOperators, operatorName) >= 0)
            {
                where.value = EnsureArray(coercedValue);
            }

            // Unary operators carry no value.
            if (Array.IndexOf(UnaryOperators, operatorName) >= 0)
            {
                where.value = null;
            }

            output.Add(where);
        }

        #endregion

        #region Operator Helpers

        private static string NegateOperator(string operatorName)
        {
            return operatorName switch
            {
                "equal" => "notequal",
                "notequal" => "equal",
                "greaterthan" => "lessthanorequal",
                "greaterthanorequal" => "lessthan",
                "lessthan" => "greaterthanorequal",
                "lessthanorequal" => "greaterthan",
                "contains" => "doesnotcontain",
                "doesnotcontain" => "contains",
                "startswith" => "doesnotstartwith",
                "doesnotstartwith" => "startswith",
                "endswith" => "doesnotendwith",
                "doesnotendwith" => "endswith",
                "in" => "notin",
                "notin" => "in",
                "between" => "notbetween",
                "notbetween" => "between",
                "isnull" => "isnotnull",
                "isnotnull" => "isnull",
                "isempty" => "isnotempty",
                "isnotempty" => "isempty",
                _ => throw new NotSupportedException(
                    $"The advanced filter operator '{operatorName}' cannot be negated.")
            };
        }

        private static GridColumn? LookupColumn(List<GridColumn>? columns, string field)
        {
            if (columns == null || string.IsNullOrEmpty(field))
            {
                return null;
            }

            return columns.FirstOrDefault(c => c != null && c.Field == field);
        }

        private static string NormaliseCondition(string? condition)
        {
            if (string.IsNullOrEmpty(condition))
            {
                return "and";
            }

            return condition.Equals("or", StringComparison.OrdinalIgnoreCase) ? "or" : "and";
        }

        private static string NormaliseOperator(string operatorName)
        {
            // Normalise Query Builder operator strings to the canonical lowercase
            // names consumed by Syncfusion.Blazor.Data's WhereFilter pipeline.
            // Recognises the documented canonical names plus a handful of
            // common aliases; anything unrecognised falls through unchanged
            // so unexpected operators surface as data issues rather than
            // silent coercions.
            if (string.IsNullOrEmpty(operatorName))
            {
                return "equal";
            }

            var trimmed = operatorName.Trim();
            return trimmed.ToUpperInvariant() switch
            {
                "EQUAL" or "EQUALS" or "EQ" => "equal",
                "NOTEQUAL" or "NOTEQUALS" or "NEQ" or "NE" => "notequal",
                "GREATERTHAN" or "GT" => "greaterthan",
                "GREATERTHANOREQUAL" or "GE" => "greaterthanorequal",
                "LESSTHAN" or "LT" => "lessthan",
                "LESSTHANOREQUAL" or "LE" => "lessthanorequal",
                "CONTAINS" => "contains",
                "DOESNOTCONTAIN" or "NOTCONTAINS" => "doesnotcontain",
                "STARTSWITH" or "BEGIN" or "BEGINSWITH" => "startswith",
                "ENDSWITH" or "END" => "endswith",
                "DOESNOTSTARTWITH" or "NOTSTARTSWITH" => "doesnotstartwith",
                "DOESNOTENDWITH" or "NOTENDSWITH" => "doesnotendwith",
                "IN" => "in",
                "NOTIN" => "notin",
                "BETWEEN" => "between",
                "NOTBETWEEN" => "notbetween",
                "ISNULL" or "NULL" => "isnull",
                "ISNOTNULL" or "NOTNULL" => "isnotnull",
                "ISEMPTY" or "EMPTY" => "isempty",
                "ISNOTEMPTY" or "NOTEMPTY" => "isnotempty",
                "LIKE" => "like",
                "WILDCARD" => "wildcard",
                _ => trimmed
            };
        }

        private static bool ShouldIgnoreCase(GridColumn? column, string operatorName)
        {
            // Case-insensitive for string operators on string columns by
            // default — matches the FilterBar and FilterMenu behaviour.
            if (column == null)
            {
                return false;
            }

            if (column.Type != ColumnType.String)
            {
                return false;
            }

            return operatorName is
                "startswith" or "endswith" or "contains" or
                "doesnotcontain" or "doesnotstartwith" or "doesnotendwith" or
                "like" or "wildcard" or "equal" or "notequal";
        }

        #endregion

        #region Value Conversion Helpers

        private static object? CoerceValue(object? value, GridColumn? column)
        {
            if (value == null)
            {
                return null;
            }

            // Unwrap JsonElement values produced by rule persistence.
            if (value is JsonElement json)
            {
                value = UnwrapJsonElement(json);
                if (value == null)
                {
                    return null;
                }
            }

            if (value is string s && string.IsNullOrEmpty(s) && column != null && column.Type != ColumnType.String)
            {
                return null;
            }

            if (column == null)
            {
                return value;
            }

            var targetType = column.ValueType;
            if (targetType == null)
            {
                return value;
            }

            if (value is IEnumerable values && value is not string)
            {
                var coercedValues = new List<object?>();
                foreach (var item in values)
                {
                    coercedValues.Add(CoerceValue(item, column));
                }

                return coercedValues;
            }

            // The runtime filter applies ordinal comparison when the
            // value is already the column's type; we coerce here so the
            // values produced by the Query Builder (often strings) match.
            if (value is string raw)
            {
                return ParseString(raw, targetType);
            }

            if (targetType.IsAssignableFrom(value.GetType()))
            {
                return value;
            }

            try
            {
                return Convert.ChangeType(value, Nullable.GetUnderlyingType(targetType) ?? targetType, CultureInfo.InvariantCulture);
            }
            catch
            {
                return value;
            }
        }

        private static object? UnwrapJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    return element.GetString();
                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var l))
                    {
                        return l;
                    }

                    if (element.TryGetDouble(out var d))
                    {
                        return d;
                    }

                    return element.ToString();
                case JsonValueKind.True:
                    return true;
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;
                case JsonValueKind.Array:
                    var values = new List<object?>();
                    foreach (var item in element.EnumerateArray())
                    {
                        values.Add(UnwrapJsonElement(item));
                    }

                    return values;
                case JsonValueKind.Object:
                    return element.ToString();
                default:
                    return element.ToString();
            }
        }

        private static object? ParseString(string raw, Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            try
            {
                if (underlying == typeof(string))
                {
                    return raw;
                }

                if (underlying == typeof(int))
                {
                    return int.Parse(raw, CultureInfo.InvariantCulture);
                }

                if (underlying == typeof(long))
                {
                    return long.Parse(raw, CultureInfo.InvariantCulture);
                }

                if (underlying == typeof(short))
                {
                    return short.Parse(raw, CultureInfo.InvariantCulture);
                }

                if (underlying == typeof(decimal))
                {
                    return decimal.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                if (underlying == typeof(double))
                {
                    return double.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                if (underlying == typeof(float))
                {
                    return float.Parse(raw, NumberStyles.Any, CultureInfo.InvariantCulture);
                }

                if (underlying == typeof(bool))
                {
                    if (bool.TryParse(raw, out var b))
                    {
                        return b;
                    }

                    return raw.Equals("1", StringComparison.Ordinal) || raw.Equals("true", StringComparison.OrdinalIgnoreCase);
                }

                if (underlying == typeof(DateTime))
                {
                    if (DateTime.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var dt))
                    {
                        return dt;
                    }

                    return raw;
                }

                if (underlying == typeof(DateOnly))
                {
                    if (DateOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
                    {
                        return dto;
                    }

                    return raw;
                }

                if (underlying == typeof(TimeOnly))
                {
                    if (TimeOnly.TryParse(raw, CultureInfo.InvariantCulture, DateTimeStyles.None, out var t))
                    {
                        return t;
                    }

                    return raw;
                }

                if (underlying.IsEnum)
                {
                    if (Enum.TryParse(underlying, raw, true, out var e))
                    {
                        return e;
                    }

                    return raw;
                }

                if (underlying == typeof(Guid))
                {
                    if (Guid.TryParse(raw, out var g))
                    {
                        return g;
                    }

                    return raw;
                }
            }
            catch
            {
                // Fall through to the raw string; the runtime filter
                // will evaluate it and either match or produce an empty
                // result, which is the user-visible failure mode for
                // invalid input.
            }

            return raw;
        }

        private static List<object> EnsureArray(object? value)
        {
            if (value is null)
            {
                return new List<object>();
            }

            if (value is IEnumerable e && value is not string)
            {
                var list = new List<object>();
                foreach (var item in e)
                {
                    list.Add(item);
                }

                return list;
            }

            return new List<object> { value };
        }

        #endregion
    }
}
