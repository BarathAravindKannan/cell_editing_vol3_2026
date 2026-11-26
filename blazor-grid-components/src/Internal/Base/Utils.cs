using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Util functions for internal usage.
    /// </summary>
    /// <exclude/>
    internal class GridUtils
    {
        internal static double uId;
        internal static IDictionary<CellType, List<string>> CellStaticClasses = new Dictionary<CellType, List<string>>()
        {
            { CellType.Data, new List<string>() { "e-rowcell" } },
            { CellType.Indent, new List<string>() { "e-indentcell" } },
            { CellType.Detail, new List<string>() { } },
            { CellType.DetailIndent, new List<string>() { "e-detailindentcell" } },
            { CellType.Expand, new List<string>() { "e-recordplusexpand" } },
            { CellType.GroupCaption, new List<string>() { "e-groupcaption" } },
            { CellType.CaptionSummary, new List<string>() { "e-summarycell",  "e-templatecell" } },
            { CellType.Summary, new List<string> { "e-summarycell" } },
            { CellType.RowDrag, new List<string>() { "e-rowdragdrop" } },
            { CellType.CommandColumn, new List<string>() { "e-unboundcell" } }
        };

        internal static IDictionary<string, List<string>> RowStaticClasses = new Dictionary<string, List<string>>()
        {
            { "Data", new List<string>() { "e-row" } },
            { "GroupCaption", new List<string>() { } },
            { "DetailRow", new List<string>() { } },
            { "Summary", new List<string>() { "e-summaryrow" } }
        };

        internal static List<string> RequireRefreshProps = new List<string>()
        {
            "DataSource",
            "Columns",
            "AllowGrouping",
            "GroupSettings",
            "AllowSorting",
            "SortSettings",
            "EnableVirtualization",
            "EnableColumnVirtualization",
            "EnableVirtualMaskRow",
            "AllowFiltering",
            "FilterSettings",
            "FrozenRows",
            "FrozenColumns",
            "Locale",
            "AllowPaging",
            "PageSettings",
            "AllowRowDragAndDrop",
            "RowDropSettings",
            "AllowSearching",
            "SearchSettings",
            "RowTemplate",
            "DetailTemplate",
            "Aggregates",
            "CurrentCode",
            "Query",
            "EnableAltRow",
            "EnableUndoRedo",
            "UndoRedoLimit"
        };

        internal static bool IsRefreshable(string name)
            => RequireRefreshProps.Any(p => name.IndexOf(p, StringComparison.Ordinal) > -1);

        internal static List<GridColumn> GetColumns(IGrid Parent = null!, List<GridColumn> columns = null!)
        {
            List<GridColumn> gridColumn = new List<GridColumn>();
            if (columns != null && columns.Count > 0)
            {
                var FrozenColumnsLeft = columns.Where(x => x.Freeze == FreezeDirection.Left && x.IsFrozen).ToList();
                var MovabelColumns = columns.Where(x => !x.IsFrozen || (x.IsFrozen && x.Freeze == FreezeDirection.Fixed)).ToList();
                var FrozenColumnsRight = columns.Where(x => x.Freeze == FreezeDirection.Right && x.IsFrozen).ToList();
                columns = FrozenColumnsLeft.Concat(MovabelColumns).Concat(FrozenColumnsRight).ToList();
                for (int i = 0; i < columns.Count; i++)
                {
#pragma warning disable BL0005
                    columns[i].Index = i;
                }
            }
            UpdateColumnsModel(columns ?? Parent?.Columns ?? gridColumn, ref gridColumn);
            return gridColumn;
        }

        internal static int GetStackedWidth(GridColumn column, FreezeDirection direction = default, int frozenColumns = 0)
        {
            var width = 0;
            foreach (var col in column.Columns!)
            {
                if (frozenColumns > 0)
                {
                    if (col.Columns == null)
                    {
                        width += string.IsNullOrEmpty(col.Width) ? 0 : GetParsedWidth(col.Width);
                    }
                    else
                    {
                        width += GetStackedWidth(col, frozenColumns: frozenColumns);
                    }
                }
                else
                {
                    if (col.Columns == null && col.IsFrozen && col.Freeze.Equals(direction) && col.Visible)
                    {
                        width += string.IsNullOrEmpty(col.Width) ? 0 : GetParsedWidth(col.Width);
                    }
                    else if (col.Columns != null)
                    {
                        width += GetStackedWidth(col, direction, frozenColumns);
                    }
                    else
                    {
                        width += 0;
                    }
                }
            }
            return width;
        }

        internal static RenderFragment GetRawContent(string content) => builder =>
        {
            builder.AddMarkupContent(0, content);
        };

        internal static IDictionary<string, object> GetAttributeValues(IDictionary<string, object> attributes, string styleParam)
        {
            if (string.IsNullOrEmpty(styleParam) && attributes != null)
            {
                attributes.Remove("style");
                attributes.AddOrUpdateItem("data-sf-style", "");
                return attributes;
            }
            IDictionary<string, object> tempAttributes = attributes!.ToDictionary(entry => entry.Key, entry => entry.Value);
            tempAttributes.AddOrUpdateItem("data-sf-style", styleParam);
            return tempAttributes;
        }

        internal static string GetIndentStyle(Row<object> row, Cell<object> cell)
        {
            var styleText = string.Empty;
            var indent = 0;

            for (int i = 0; i < row.Cells.Count; i++)
            {
                if (row.Cells[i] == cell)
                {
                    if (i > 0)
                    {
                        indent = 30 * i;
                    }
                    styleText = $"Left: {indent}px";
                    break;
                }
            }
            if (string.IsNullOrEmpty(styleText))
            {
                styleText = "Left: 0px";
            }
            return styleText;
        }

        internal static string EnsureUniqueStyles(string existingString, string incomingString)
        {
            existingString = existingString.Trim();
            foreach (var text in incomingString.Split(';'))
            {
                if(!existingString.Contains(text.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    existingString += text + ";";
                }
            }
            return existingString;
        }

        internal static string GetStyleAsStringFromObject(IDictionary<string, object> styleObject)
        {
            if (styleObject == null)
                return string.Empty;

            styleObject.TryGetValue("style", out object? styleValueObject);
            if (styleValueObject == null || string.IsNullOrEmpty(styleValueObject.ToString()))
                return string.Empty;

            string styleString = styleValueObject.ToString()!;
            return GetStyleEndsWithSemiColon(styleString);
        }

        internal static string GetStyleEndsWithSemiColon(string styleString)
        {
#if NET11_0_OR_GREATER
            return styleString.EndsWith(';', System.StringComparison.Ordinal) ? styleString : styleString + ";";
#else
            return styleString.EndsWith(';') ? styleString : styleString + ";";
#endif
        }
        internal static List<string> GetUniqueStringList(List<string> existingStringList, string incomingString)
        {
            return existingStringList.Concat(incomingString.Split(' ')).Distinct().ToList();
        }

        internal static bool IsNoneTextAlign(GridColumn column)
        {
            if (column != null && column.TextAlign == TextAlign.None)
            {
                return true;
            }
            return false;
        }

        internal static string? GetTextAlign(GridColumn column)
        {
            return column?.TextAlign.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture);
        }

        internal static IDictionary<string, object> GetStyleAttributes(string style)
        {
            if (string.IsNullOrEmpty(style))
            {
                return null!;
            }

            return new Dictionary<string, object> { { "data-sf-style", style } };
        }

        internal static string GetAlignmentClass(GridColumn column) =>
    column?.TextAlign switch
    {
        TextAlign.Right => "e-rightalign",
        TextAlign.Left  => "e-leftalign",
        TextAlign.Center => "e-centeralign",
        TextAlign.Justify => "e-justifyalign",
        _ => string.Empty
    };

        internal static object? ParseJsonElementToEnum(GridFilterColumn defaultFltrCol, IGrid Parent)
        {
            var field = defaultFltrCol.Field;
            var value = defaultFltrCol.Value;
            var gridColumn = GridUtils.GetColumnByField(field, Parent.Columns!);
            var type =  (gridColumn != null && gridColumn.IsForeignColumn())  ? gridColumn.ActualType : gridColumn?.ValueType;
            var isNullableEnum = type != null && type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>) && type.GetGenericArguments().Length > 0 && type.GetGenericArguments()[0].IsEnum;

            if (type != null && type.GetTypeInfo().IsEnum && Enum.TryParse(type, value?.ToString(), out var enumvalue))
            {
                return enumvalue;
            }
            else if (isNullableEnum && Enum.TryParse(Nullable.GetUnderlyingType(type!)!, value?.ToString(), out var nullableEnumValue))
            {
                return nullableEnumValue;
            }
            else
            {
                return value;
            }
        }

        internal static GridColumn? grabColumnByUidOrField(string Uid, IGrid Parent, string Field = null!)
        {
            using var col = new GridColumn();
            var column = col;
            var columnModel = new List<GridColumn>();
            UpdateColumnsModel(Parent.Columns!, ref columnModel);
            column = columnModel.FirstOrDefault(col => col.Field == Field || col.Uid == Uid);
            return column;
        }

        internal static void UpdateColumnsModel(List<GridColumn> columns, ref List<GridColumn> initColumns)
        {
            if (columns == null) return;

            foreach (GridColumn column in columns)
            {
                if (column.Columns == null || column.Columns.Count == 0)
                {
                    initColumns.Add(column);
                }
                else
                {
                    UpdateColumnsModel(column.Columns, ref initColumns);
                }
            }
        }

        internal static object GetForeignData(GridColumn column, object data, object foreignKeyData, bool isExport = false)
        {
            if (column == null) return null!;

            var field = column.ForeignKeyField ?? column.Field;
            var key = DataUtil.GetObject(column.Field, data);
            var query = new List<WhereFilter>()
            {
                new WhereFilter()
                {
                    Field = field,
                    value = key,
                    IgnoreCase = false,
                    Operator = "equal"
                }
            };

            var result = isExport 
                ? column.GetForeignKeyData((foreignKeyData as IEnumerable<object>)!, query) 
                : column.GetForeignkeyFilteredData((foreignKeyData as IEnumerable<object>)!, query);
            return result;
        }
                
        internal static GridColumn? GetColumnByField(string field, List<GridColumn> columns)
        {
            if (columns == null || field == null) return null!;
            GridColumn? column = null;
            if (columns.Any(col => col.Field == field))
            {
                column = columns.Where(col => col.Field == field).First();
                return column;
            }
            columns.ForEach((col) =>
            {
                if (column == null && col.Columns != null)
                {
                    column = GetColumnByField(field, col.Columns);                   
                }
            });
            return column;
        }

        internal static GridColumn? GetColumnByFColUidOrField(string fColUidOrField, List<GridColumn> foreignColumns, bool isStackedHeader = false)
        {
            if (foreignColumns == null || fColUidOrField == null) return null!;

            if (isStackedHeader)
            {
                var column = FindColumnByUid(fColUidOrField, foreignColumns);
                if (column != null)
                {
                    return column;
                }
            }
            return foreignColumns.FirstOrDefault(foreignColumn => 
                (foreignColumn.Uid ?? foreignColumn.Field) == fColUidOrField);
        }

        private static GridColumn FindColumnByUid(string uid, List<GridColumn> columns)
        {
            if (columns == null || uid == null) return null!;

            foreach (var col in columns)
            {
                if (uid == col.Uid)
                {
                    return col;
                }
                if (col.Columns != null && col.Columns.Count > 0)
                {
                    var foundColumn = FindColumnByUid(uid, col.Columns);
                    if (foundColumn != null)
                    {
                        return foundColumn;
                    }
                }
            }
            return null!;
        }
                
        internal static string FormarUnit(string value)
        {
            var result = value + string.Empty;
            if (result == "auto" || result.Contains('%', StringComparison.Ordinal) || result.Contains("px", StringComparison.Ordinal))
            {
                return result;
            }

            return result + "px";
        }

        internal static double[] ToDoubleArray(object args)
        {
            List<double> result = new List<double>();
            if (args is IEnumerable arr)
            {
                foreach (var item in arr)
                {
                    if (double.TryParse(Convert.ToString(item, CultureInfo.InvariantCulture), 
                        NumberStyles.Any, CultureInfo.InvariantCulture, out double value))
                    {
                        result.Add(value);
                    }
                }
            }
            return result.ToArray();
        }
        internal static double GetDoubleParsedWidth(string width)
        {
            string iWidth = width;
            if (IsNotPixelValue(iWidth))
            {
                return 0;
            }
            iWidth = GetPxWidth(iWidth);

            return Double.Parse(iWidth, CultureInfo.InvariantCulture);
        }

        internal static int GetParsedWidth(string width)
        {
            string iWidth = width;
            if (IsNotPixelValue(iWidth))
            {
                return 0;
            }
            iWidth = GetPxWidth(iWidth);

            return (int)Math.Round(double.Parse(iWidth, CultureInfo.InvariantCulture));
        }

        private static bool IsNotPixelValue(string width)
        {
            return string.IsNullOrEmpty(width) || width.Contains('%', StringComparison.Ordinal) || width.Contains("auto", StringComparison.Ordinal);
        }

        private static string GetPxWidth(string width)
        {
            if (width.Contains("px", StringComparison.Ordinal))
            {
                width = width.Substring(0, width.IndexOf("px", StringComparison.Ordinal));
            }
            return width;
        }

        internal static int ConvertPxToInt(string widthOrHeight)
        {
            string numericPart = widthOrHeight.Replace("px", "", StringComparison.Ordinal).Trim();

            if (double.TryParse(numericPart, NumberStyles.Float, CultureInfo.InvariantCulture, out double value))
            {
                return (int)Math.Ceiling(value);
            }
            return -1;
        }

        internal static string GetUid(string prefix) => $"{prefix}-{++GridUtils.uId}";

        internal static bool CompareValues<T>(T oldValue, T newValue)
            => !EqualityComparer<T>.Default.Equals(oldValue, newValue);

        internal static object GetCellValue(Cell<object> Cell, Row<object> Row)
        {
            if (Cell == null || Row == null) return null!;

            object? value = null;

            if (Cell.Column != null && Cell.Column.IsForeignColumn())
            {
                var foreignKeyData = Cell.IsDirty 
                    ? GridUtils.GetForeignData(Cell.Column, Row.EditedData!, Cell.Column.GetForeignData()! ?? Cell.Column.ColumnData!) 
                    : Cell.ForeignKeyData;
                value = foreignKeyData != null ? DataUtil.GetVal(foreignKeyData as IEnumerable ?? Enumerable.Empty<object>(), 0, Cell.Column.ForeignKeyValue!) : null!;
            }
            else if (!string.IsNullOrEmpty(Cell.Column?.Field))
            {
                var sourceData = Cell.IsDirty ? Row.EditedData : Row.Data;
                if (sourceData != null)
                {
                    value = DataUtil.GetObject(Cell.Column.Field, sourceData);
                }
            }

            if (value is Enum enumValue)
            {
                value = MetadataExtension.GetDisplayName(enumValue);
            }

            string? ValueTypeName = value?.GetTypeName();
            if (Cell.Column?.ConvertEmptyStringToNull == true && value?.ToString()?.Length == 0)
            {
                value = null!;
            }
            if (value == null && Cell.Column?.NullDisplayText != null)
            {
                value = Cell.Column.NullDisplayText;
            }
            else if (value == null && Cell.Column?.Type == ColumnType.Boolean && Cell.Column?.DisplayAsCheckBox == true)
            {
                return value = false;
            }
            else if (value == null)
            {
                return value = string.Empty;
            }

            if (!string.IsNullOrEmpty(Cell.Column?.Format))
            {
                return DataUtil.GetFormattedValue(value, Cell.Column.Format);
            }

            return value;
        }

        internal static GridColumn GetStackedColumnByUid(List<GridColumn> columns, string uid)
        {
            if (columns == null || string.IsNullOrEmpty(uid))
            {
                return null!;
            }
            foreach (GridColumn column in columns)
            {
                if (column.Uid == uid)
                {
                    return column;
                }
                // Check if column has child columns
                bool hasChildColumns = column.Columns != null && column.Columns.Count > 0;
                if (hasChildColumns)
                {
                    // Recursively search in child columns
                    GridColumn nestedColumn = GetStackedColumnByUid(column?.Columns!, uid);
                    if (nestedColumn != null)
                    {
                        return nestedColumn;
                    }
                }
            }
            return null!;
        }

        internal static Cell<object>? ResolveCurrentCell(Row<object>? row, Cell<object>? staleCell)
        {
            if (row == null || staleCell == null)
            {
                return null;
            }
            // Preferred match: same Column.Uid (stable across re-render).
            if (staleCell.Column != null && !string.IsNullOrEmpty(staleCell.Column.Uid))
            {
                var byColumnUid = row.Cells?.FirstOrDefault(c => c.Column != null && c.Column.Uid == staleCell.Column.Uid);
                if (byColumnUid != null)
                {
                    return byColumnUid;
                }
            }
            // Fallback 1: same Column.Field.
            if (staleCell.Column != null && !string.IsNullOrEmpty(staleCell.Column.Field))
            {
                var byField = row.Cells?.FirstOrDefault(c => c.Column != null && c.Column.Field == staleCell.Column.Field);
                if (byField != null)
                {
                    return byField;
                }
            }
            // Fallback 2: same Index within the row.
            if (staleCell.Index.HasValue)
            {
                int idx = staleCell.Index.Value;
                if (row.Cells != null && idx >= 0 && idx < row.Cells.Count)
                {
                    return row.Cells[idx];
                }
            }
            return null;
        }
    }

    internal static class GridUtilExtension
    {
        internal static void AddOrUpdateItem(this IDictionary<string, object> dict, string key, object value)
        {
            if (dict?.ContainsKey(key) == true)
            {
                dict[key] = value;
            }
            else
            {
                dict?.Add(key, value);
            }
        }

        internal static void AddOrUpdateItem(this IDictionary<object, object> dict, object key, object value)
        {
            if (dict?.ContainsKey(key) == true)
            {
                dict[key] = value;
            }
            else
            {
                dict?.Add(key, value);
            }
        }

        internal static void AddOrSkip(this List<string> listValue, string value)
        {
            if (listValue?.IndexOf(value) == -1)
            {
                listValue?.Add(value);
            }
        }

        internal static List<T>? Clone<T>(this List<T> listValue)
        {
            return listValue?.Select(x => x).ToList();
        }

        internal static string GetTypeName(this object value)
        {
            Type? type = value?.GetType();
            if (type == null)
            {
                return null!;
            }

            var typeName = type.GetType().Name;

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                typeName = type.GetGenericArguments()[0].GetType().Name;
            }

            return typeName;
        }
    }

    internal static class GridKeyUtilExtension
    {
        internal static bool IsEnter(this KeyboardEventArgs e) => e.Key == "Enter";

        internal static bool IsShiftEnter(this KeyboardEventArgs e) => e.Key == "Enter" && e.ShiftKey;

        internal static bool IsCtrlEnter(this KeyboardEventArgs e) => e.Key == "Enter" && e.CtrlKey;

        internal static bool IsTab(this KeyboardEventArgs e) => e.Key == "Tab";

        internal static bool IsShiftTab(this KeyboardEventArgs e) => e.IsTab() && e.ShiftKey;

        internal static bool IsUpArrow(this KeyboardEventArgs e) => e.Key == "ArrowUp";

        internal static bool IsDownArrow(this KeyboardEventArgs e) => e.Key == "ArrowDown";

        internal static bool IsLeftArrow(this KeyboardEventArgs e) => e.Key == "ArrowLeft";

        internal static bool IsRightArrow(this KeyboardEventArgs e) => e.Key == "ArrowRight";

        internal static bool IsArrowKey(this KeyboardEventArgs e) => e.IsUpArrow() || e.IsDownArrow()
            || e.IsLeftArrow() || e.IsRightArrow();

        internal static bool IsShiftUp(this KeyboardEventArgs e) => e.IsUpArrow() && e.ShiftKey;

        internal static bool IsShiftDown(this KeyboardEventArgs e) => e.IsDownArrow() && e.ShiftKey;

        internal static bool IsShiftRight(this KeyboardEventArgs e) => e.IsRightArrow() && e.ShiftKey;

        internal static bool IsShiftLeft(this KeyboardEventArgs e) => e.IsLeftArrow() && e.ShiftKey;

        internal static bool IsHome(this KeyboardEventArgs e) => e.Key == "Home";

        internal static bool IsCtrlHome(this KeyboardEventArgs e) => e.IsHome() && e.CtrlKey;

        internal static bool IsEnd(this KeyboardEventArgs e) => e.Key == "End";

        internal static bool IsCtrlEnd(this KeyboardEventArgs e) => e.IsEnd() && e.CtrlKey;

        internal static bool IsEsc(this KeyboardEventArgs e) => e.Key == "Escape";

        internal static bool IsPageUp(this KeyboardEventArgs e) => e.Key == "PageUp";

        internal static bool IsPageDown(this KeyboardEventArgs e) => e.Key == "PageDown";

        // ctrlAltPageUp: 'ctrl+alt+pageup',
        internal static bool IsCtrlAltPageUp(this KeyboardEventArgs e) => e.IsPageUp() && e.CtrlKey && e.AltKey;

        // ctrlAltPageDown: 'ctrl+alt+pagedown',
        internal static bool IsCtrlAltPageDown(this KeyboardEventArgs e) => e.IsPageDown() && e.CtrlKey && e.AltKey;

        //AltW: 'Alt + w'
        internal static bool IsAltW(this KeyboardEventArgs e, bool isMacDevice = false) => e.AltKey && (e.Key == "W" || e.Key == "w" || (isMacDevice && e.Key == "∑"));

        // altPageUp: 'alt+pageup',
        internal static bool IsAltPageUp(this KeyboardEventArgs e) => e.IsPageUp() && e.AltKey;

        // altPageDown: 'alt+pagedown',
        internal static bool IsAltPageDown(this KeyboardEventArgs e) => e.IsPageDown() && e.AltKey;

        // altDownArrow: 'alt+downarrow',
        internal static bool IsAltArrowDown(this KeyboardEventArgs e) => e.IsDownArrow() && e.AltKey;

        // altUpArrow: 'alt+uparrow',
        internal static bool IsAltArrowUp(this KeyboardEventArgs e) => e.IsUpArrow() && e.AltKey;

        internal static bool IsCtrlArrowUp(this KeyboardEventArgs e) => e.IsUpArrow() && e.CtrlKey;

        // ctrlDownArrow: 'ctrl+downarrow',
        internal static bool IsCtrlArrowDown(this KeyboardEventArgs e) => e.IsDownArrow() && e.CtrlKey;

        // ctrlUpArrow: 'ctrl+uparrow',
        internal static bool IsCtrlA(this KeyboardEventArgs e) => e.CtrlKey && (e.Key == "A" || e.Key == "a");

        // ctrlPlusA: 'ctrl+A',
        internal static bool IsCtrlP(this KeyboardEventArgs e) => e.CtrlKey && (e.Key == "P" || e.Key == "p");

        internal static bool IsMetaP(this KeyboardEventArgs e) => e.MetaKey && (e.Key == "P" || e.Key == "p");

        // ctrlPlusP: 'ctrl+P',
        internal static bool IsInsert(this KeyboardEventArgs e, bool isMacDevice = false) => (e.Key == "Insert" || (isMacDevice && e.AltKey && e.Key == "Enter"));

        // insert: 'insert',
        internal static bool IsDelete(this KeyboardEventArgs e) => e.Key == "Delete";

        // delete: 'delete',
        internal static bool IsF2(this KeyboardEventArgs e) => e.Key == "F2";

        // f2: 'f2',
        //    space: 'space',
        internal static bool IsSpace(this KeyboardEventArgs e) => e.Code == "Space";

        /// <summary>
        /// Returns true only when <paramref name="e"/>.Key is exactly the requested function
        /// key token (e.g. "F1", "F3", "F10"). Avoids prefix-collision with "F1" matching
        /// "F10" / "F11" / "F12".
        /// </summary>
        internal static bool IsFunctionKey(this KeyboardEventArgs e, int n)
        {
            if (n < 1 || n > 12)
            {
                return false;
            }

            return e.Key == "F" + n.ToString(CultureInfo.InvariantCulture);
        }

        internal static bool IsF1(this KeyboardEventArgs e) => e.IsFunctionKey(1);

        internal static bool IsF3(this KeyboardEventArgs e) => e.IsFunctionKey(3);

        internal static bool IsF4(this KeyboardEventArgs e) => e.IsFunctionKey(4);

        internal static bool IsF5(this KeyboardEventArgs e) => e.IsFunctionKey(5);

        internal static bool IsF6(this KeyboardEventArgs e) => e.IsFunctionKey(6);

        internal static bool IsF7(this KeyboardEventArgs e) => e.IsFunctionKey(7);

        internal static bool IsF8(this KeyboardEventArgs e) => e.IsFunctionKey(8);

        internal static bool IsF9(this KeyboardEventArgs e) => e.IsFunctionKey(9);

        internal static bool IsF10(this KeyboardEventArgs e) => e.IsFunctionKey(10);

        internal static bool IsF11(this KeyboardEventArgs e) => e.IsFunctionKey(11);

        internal static bool IsF12(this KeyboardEventArgs e) => e.IsFunctionKey(12);

        /// <summary>
        /// Returns true for any of F1..F12. Used by the additive function-key branches in
        /// <see cref="GetKeyCombination"/>. F2 has its own explicit branch and is intentionally
        /// excluded so its hard-coded "F2" token (consumed by IsF2KeyPressed which uses
        /// Contains("F2")) is preserved.
        /// </summary>
        internal static bool IsFn(this KeyboardEventArgs e)
        {
            for (int n = 1; n <= 12; n++)
            {
                if (n == 2)
                {
                    continue;
                }

                if (e.IsFunctionKey(n))
                {
                    return true;
                }
            }

            return false;
        }

        // ctrlPlusC: 'ctrl+C',
        internal static bool IsCtrlC(this KeyboardEventArgs e) => e.CtrlKey && (e.Key == "C" || e.Key == "c");

        // ctrlShiftPlusH: 'ctrl+shift+H',
        internal static bool IsCtrlShiftH(this KeyboardEventArgs e) => e.CtrlKey && e.ShiftKey && (e.Key == "H" || e.Key == "h");

        // ctrlSpace: 'ctrl+space',
        internal static bool IsCtrlSpace(this KeyboardEventArgs e) => e.CtrlKey && e.IsSpace();

        // ctrlLeftArrow: 'ctrl+leftarrow',
        internal static bool IsCtrlLeftArrow(this KeyboardEventArgs e) => e.CtrlKey && e.IsLeftArrow();

        // ctrlRightArrow: 'ctrl+rightarrow'
        internal static bool IsCtrlRightArrow(this KeyboardEventArgs e) => e.CtrlKey && e.IsRightArrow();

        // ctrlZ: 'ctrl+z' - Undo (also supports Cmd+Z on Mac)
        internal static bool IsCtrlZ(this KeyboardEventArgs e) => (e.CtrlKey || e.MetaKey) && (e.Key == "Z" || e.Key == "z") && !e.ShiftKey;

        // ctrlY: 'ctrl+y' - Redo (also supports Cmd+Y on Mac)
        internal static bool IsCtrlY(this KeyboardEventArgs e) => (e.CtrlKey || e.MetaKey) && (e.Key == "Y" || e.Key == "y") && !e.ShiftKey;

        // ctrlShiftZ: 'ctrl+shift+z' - Redo (alternative) (also supports Cmd+Shift+Z on Mac)
        internal static bool IsCtrlShiftZ(this KeyboardEventArgs e) => (e.CtrlKey || e.MetaKey) && e.ShiftKey && (e.Key == "Z" || e.Key == "z");

        internal static bool IsMetaOrUnidentified(this KeyboardEventArgs e) => e.Key == "Meta" || e.Key == "Unidentified";

        public static string GetKeyCombination(this KeyboardEventArgs e, bool? isMacDevice = false, bool isEnableUndoRedo = false)
        {
            string action = null!;

            if (e.IsMetaOrUnidentified())
            {
                return action;
            }

            if (e.IsEnter() && !(e.AltKey && (isMacDevice ?? false)))
            {
                action = EnterHandler(e, action);
            }
            else if (e.IsTab())
            {
                action = "Tab";
                if (e.IsShiftTab())
                {
                    action = "ShiftTab";
                }
            }
            else if (e.IsArrowKey())
            {
                action = ArrowKeyHandler(e, action!);
            }

            else if (e.IsHome())
            {
                action = "Home";
                if (e.IsCtrlHome())
                {
                    action = "CtrlHome";
                }
            }
            else if (e.IsEnd())
            {
                action = "End";
                if (e.IsCtrlEnd())
                {
                    action = "CtrlEnd";
                }
            }
            else if (e.IsSpace())
            {
                action = "Space";
                if (e.IsCtrlSpace())
                {
                    action = "CtrlSpace";
                }
            }
            else if (e.IsEsc())
            {
                action = "Escape";
            }
            else if (e.IsF2())
            {
                action = "F2";
            }
            else if (e.IsF1())
            {
                action = FunctionKeyHandler(e, "F1", isMacDevice ?? false);
            }
            else if (e.IsF3())
            {
                action = FunctionKeyHandler(e, "F3", isMacDevice ?? false);
            }
            else if (e.IsF4())
            {
                action = FunctionKeyHandler(e, "F4", isMacDevice ?? false);
            }
            else if (e.IsF5())
            {
                action = FunctionKeyHandler(e, "F5", isMacDevice ?? false);
            }
            else if (e.IsF6())
            {
                action = FunctionKeyHandler(e, "F6", isMacDevice ?? false);
            }
            else if (e.IsF7())
            {
                action = FunctionKeyHandler(e, "F7", isMacDevice ?? false);
            }
            else if (e.IsF8())
            {
                action = FunctionKeyHandler(e, "F8", isMacDevice ?? false);
            }
            else if (e.IsF9())
            {
                action = FunctionKeyHandler(e, "F9", isMacDevice ?? false);
            }
            else if (e.IsF10())
            {
                action = FunctionKeyHandler(e, "F10", isMacDevice ?? false);
            }
            else if (e.IsF11())
            {
                action = FunctionKeyHandler(e, "F11", isMacDevice ?? false);
            }
            else if (e.IsF12())
            {
                action = FunctionKeyHandler(e, "F12", isMacDevice ?? false);
            }
            else if (e.IsInsert(isMacDevice: isMacDevice ?? false))
            {
                action = "Insert";
            }
            else if (e.IsDelete())
            {
                action = "Delete";
            }
            else if (e.IsPageDown())
            {
                action = "PageDown";

                if (e.IsAltPageDown())
                {
                    action = "AltPageDown";
                }

                if (e.IsCtrlAltPageDown())
                {
                    action = "CtrlAltPageDown";
                }
            }
            else if (e.IsPageUp())
            {
                action = "PageUp";

                if (e.IsAltPageUp())
                {
                    action = "AltPageUp";
                }

                if (e.IsCtrlAltPageUp())
                {
                    action = "CtrlAltPageUp";
                }
            }
            else if (e.IsCtrlA())
            {
                action = "CtrlA";
            }
            else if (e.IsCtrlP())
            {
                action = "CtrlP";
            }
            else if (e.IsMetaP())
            {
                action = "MetaP";
            }
            else if (e.IsAltW(isMacDevice: isMacDevice ?? false))
            {
                action = "AltW";
            }
            else if (isEnableUndoRedo && e.IsCtrlZ())
            {
                action = "CtrlZ";
            }
            else if (isEnableUndoRedo && e.IsCtrlY())
            {
                action = "CtrlY";
            }
            else if (isEnableUndoRedo && e.IsCtrlShiftZ())
            {
                action = "CtrlShiftZ";
            }
            else if (e.Key.Length == 1 && !char.IsControl(e.Key[0]))
            {
                // Generic printable fallback (Mac: Meta -> Ctrl). Modifier order Ctrl > Alt > Shift.
                bool isMac = isMacDevice ?? false;
                bool ctrl = e.CtrlKey || (isMac && e.MetaKey);
                bool alt = e.AltKey;
                bool shift = e.ShiftKey;
                if (ctrl || alt || shift)
                {
                    string prefix = ctrl ? "Ctrl" : string.Empty;
                    if (alt)
                    {
                        prefix += "Alt";
                    }

                    if (shift)
                    {
                        prefix += "Shift";
                    }

                    action = prefix + e.Key.ToUpperInvariant();
                }
            }

            return action;
        }

        private static string EnterHandler(KeyboardEventArgs e, string action)
        {
            if (e.IsShiftEnter())
            {
                return action  = "ShiftEnter";
            }
            else if (e.IsCtrlEnter())
            {
                return action  = "CtrlEnter";
            }
            else
            {
                return action  = "Enter";
            }
        }

        private static string ArrowKeyHandler(KeyboardEventArgs e, string action)
        {
            if (e.IsUpArrow())
            {
                action = "ArrowUp";
                if (e.IsShiftUp())
                {
                    return action = "ShiftUp";
                }

                if (e.IsAltArrowUp())
                {
                    return action = "AltUp";
                }

                if (e.IsCtrlArrowUp())
                {
                    return action  = "CtrlUp";
                }
            }

            if (e.IsDownArrow())
            {
                action = "ArrowDown";
                if (e.IsShiftDown())
                {
                    return action  = "ShiftDown";
                }

                if (e.IsAltArrowDown())
                {
                   return  action = "AltDown";
                }

                if (e.IsCtrlArrowDown())
                {
                    return action = "CtrlDown";
                }
            }

            if (e.IsLeftArrow())
            {
                action = "ArrowLeft";
                if (e.IsShiftLeft())
                {
                    return action  = "ShiftLeft";
                }

                if (e.IsCtrlLeftArrow())
                {
                    return action  = "CtrlLeft";
                }
            }

            if (e.IsRightArrow())
            {
                action = "ArrowRight";
                if (e.IsShiftRight())
                {
                    return action = "ShiftRight";
                }

                if (e.IsCtrlRightArrow())
                {
                    return action  = "CtrlRight";
                }
            }
            return action;
        }

        /// <summary>
        /// Builds a token of the form "F&lt;n&gt;", "CtrlF&lt;n&gt;", "AltF&lt;n&gt;" or
        /// "ShiftF&lt;n&gt;" for the matched function-key branch in
        /// <see cref="GetKeyCombination(KeyboardEventArgs, bool?, bool)"/>.
        /// On Mac, Meta is treated as Ctrl. Modifier precedence (when multiple modifiers
        /// are held) is Ctrl &gt; Alt &gt; Shift, by design — this is the simplest
        /// deterministic mapping for the additive function-key tokens.
        /// </summary>
        private static string FunctionKeyHandler(KeyboardEventArgs e, string functionKey, bool isMacDevice)
        {
            if (string.IsNullOrEmpty(functionKey))
            {
                return functionKey;
            }

            bool ctrl = e.CtrlKey || (isMacDevice && e.MetaKey);
            bool alt = e.AltKey;
            bool shift = e.ShiftKey;
            if (ctrl)
            {
                return "Ctrl" + functionKey;
            }

            if (alt)
            {
                return "Alt" + functionKey;
            }

            if (shift)
            {
                return "Shift" + functionKey;
            }

            return functionKey;
        }

        /// <summary>
        /// Phase 0 — Additive companion to <see cref="GetKeyCombination(KeyboardEventArgs, bool?, bool)"/>.
        /// Returns a layout-independent <see cref="NormalizedKeyBinding"/> built from the physical
        /// <c>e.code</c> value. Used by the custom-keyboard resolver to compare chords without
        /// being affected by US/EU keyboard layouts. The original <see cref="GetKeyCombination(KeyboardEventArgs, bool?, bool)"/>
        /// contract is preserved; this is an additive API.
        /// </summary>
        /// <param name="e">The keyboard event args from Blazor.</param>
        /// <param name="isMac">When true, <c>Ctrl</c> in a binding matches <c>Ctrl</c> OR <c>Meta</c> at runtime.</param>
        public static NormalizedKeyBinding GetNormalizedKey(this KeyboardEventArgs e, bool isMac = false)
        {
            if (e == null)
            {
                return default;
            }

            if (e.IsMetaOrUnidentified())
            {
                return default;
            }

            KeyModifiers mods = KeyModifiers.None;
            if (e.CtrlKey) mods |= KeyModifiers.Ctrl;
            if (e.ShiftKey) mods |= KeyModifiers.Shift;
            if (e.AltKey) mods |= KeyModifiers.Alt;
            if (e.MetaKey) mods |= KeyModifiers.Meta;
            if (isMac) mods |= KeyModifiers.MetaOrCtrl;

            string? code = !string.IsNullOrEmpty(e.Code) ? GridKeyCodeCatalog.NormalizeCode(e.Code) : null;
            string? key = e.Key;
            return new NormalizedKeyBinding(mods, code, key);
        }
    }
}
