using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing cell rendering logic for GridRow component.
    /// Handles cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Group Caption Properties
        /// <summary>
        /// Gets or sets the caption template context data used for rendering group caption rows.
        /// </summary>
        private CaptionTemplateContext? templateData { get; set; }
        /// <summary>
        /// Gets and sets the column associated with the current group caption; used to obtain header text
        /// and foreign-key lookup values when rendering the group caption.
        /// </summary>
        private GridColumn? column { get; set; }

        internal readonly PropertyInfoHelper propertyHelper = new PropertyInfoHelper();
        private int? Colspan { get; set; }
        private string groupCaptionFocusClass { get; set; } = string.Empty;
        #endregion

        #region Group Caption RenderFragment
        /// <summary>
        /// Main RenderFragment for rendering a cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the cell</returns>
        internal RenderFragment RenderGroupCaptionCell(GridCellParameters cellParameters) => builder =>
        {
            var cell = cellParameters?.Cell!;
            var row = cellParameters?.Row!;

            var sequence = 0;
            var captionValue = GetCaption(row);
            var captionText = captionValue?.ToString() ?? string.Empty;
            string txtClass = GridUtils.GetAlignmentClass(cell?.Column!);

            // Main caption TD
            builder.OpenElement(sequence++, "td");
            builder.AddAttribute(sequence++, "class", $"{GetClass(row, cell!, true)} {txtClass}");
            builder.AddAttribute(sequence++, "data-sf-style", GetStyle(row, cell!));
            builder.AddAttribute(sequence++, "colspan", cell?.ColSpan);
            builder.AddAttribute(sequence++, "tabindex", cell?.TabIndex.ToString(CultureInfo.InvariantCulture));
            builder.AddAttribute(sequence++, "aria-label", $"{captionText} {Parent!.Localizer?.GetText(GridLocaleKeys.GroupCaption)}");
            builder.AddAttribute(sequence++, "data-uid", cell?.Uid);
            builder.AddAttribute(sequence++, "title", captionText);

            // Event handlers
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, 
                async (e) => await OnGroupCaptionClick(e, row, cell!).ConfigureAwait(true)));
            builder.AddAttribute(sequence++, "onfocus", EventCallback.Factory.Create<FocusEventArgs>(this,
                (e) => OnGroupCaptionFocus(e)));
            builder.AddAttribute(sequence++, "onblur", EventCallback.Factory.Create<FocusEventArgs>(this,
                async (e) => await OnGroupCaptionBlur(e, row, cell!).ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(sequence++, "onkeydown", !IsLastCellInternal(row, cell!));
            builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) => await OnGroupCaptionKeyDown(e, row, cell!).ConfigureAwait(true)));

            // Render content
            // Render content
            if (Parent.GroupSettings!.CaptionTemplate != null)
            {
                builder.AddContent(sequence++, Parent.GroupSettings.CaptionTemplate(templateData!));
            }
            else
            {
                var searchKey = Parent.FindHightLightModule?.SearchHighlightKey;
                var shouldHighlightCaption = Parent.IsFindToolbarEnabled() &&
                    Parent.FindHightLightModule != null &&
                    !string.IsNullOrEmpty(searchKey);

                if (shouldHighlightCaption && Parent.FindHightLightModule != null)
                {
                    var regularCssClass = "e-find-highlight";
                    var activeCssClass = "e-find-highlight-active";
                    var (_, highlightedHtml, updatedCounter, _) = Parent.FindHightLightModule.HighlightMatchesServerSideActive(
                        captionText,
                        searchKey!,
                        regularCssClass,
                        activeCssClass,
                        Parent.FindHightLightModule.CurrentMatchIndex,
                        Parent.FindHightLightModule.GlobalMatchCounter,
                        out _,
                        Parent.FindSettings?.CaseSensitive ?? false);

                    Parent.FindHightLightModule.GlobalMatchCounter = updatedCounter;
                    builder.AddContent(sequence++, new MarkupString(highlightedHtml));
                }
                else if (column?.DisableHtmlEncode == true)
                {
                    builder.AddContent(sequence++, captionText);
                }
                else
                {
                    if (column?.EnableSanitization == false)
                    {
                        builder.AddContent(sequence++, (MarkupString)captionText);
                    }
                    else
                    {
                        builder.AddContent(sequence++, GridUtils.GetRawContent(SanitizeHtmlHelper.Sanitize(captionText)));
                    }
                }
            }

            builder.CloseElement(); // </td>

            // Extra frozen TD if needed
            if (Parent.GroupModule != null && !Parent.GroupModule.DisableExtraFrozenTd && !hasGroupCaptionTemplate())
            {
                if (Parent.FreezeModule!.GetFreezeLeftCount() > 0 || (Parent.FreezeModule!.GetFrozenCount() > 0 && Parent.FreezeModule!.GetFreezeRightCount() < 1) && Colspan > 0)
                {
                    builder.OpenElement(sequence++, "td");
                    builder.AddAttribute(sequence++, "class", $"{GetClass(row, cell!, false)} {txtClass}");
                    builder.AddAttribute(sequence++, "colspan", Colspan);
                    builder.AddAttribute(sequence++, "tabindex", cell?.TabIndex.ToString(CultureInfo.InvariantCulture));
                    builder.AddAttribute(sequence++, "title", captionText);
                    builder.CloseElement();
                }
            }
        };

        #endregion

        #region Helper Methods
        private async Task OnGroupCaptionClick(MouseEventArgs e, Row<object> groupCaptionRow, Cell<object> groupCaptionCell)
        {
            if (groupCaptionRow != null && groupCaptionCell != null && Parent?.FocusModule != null)
            {
                await Parent.FocusModule.CellClickHandler((groupCaptionRow, groupCaptionCell), e).ConfigureAwait(true);
                await Parent.FocusModule.Refresh(groupCaptionRow, groupCaptionCell, isCtrlOrShiftKeyPressed: e.CtrlKey || e.ShiftKey).ConfigureAwait(true);
            }
        }

        private void OnGroupCaptionFocus(FocusEventArgs e)
        {
            groupCaptionFocusClass = "e-focus e-focused";
            StateHasChanged();
        }

        private async Task OnGroupCaptionBlur(FocusEventArgs e, Row<object> groupCaptionRow, Cell<object> groupCaptionCell)
        {
            groupCaptionFocusClass = string.Empty;
            if (groupCaptionRow != null && groupCaptionCell != null && Parent?.FocusModule != null)
            {
                await Parent.FocusModule.ClearFocus(groupCaptionRow, groupCaptionCell).ConfigureAwait(true);
            }
            StateHasChanged();
        }

        private async Task OnGroupCaptionKeyDown(KeyboardEventArgs e, Row<object> groupCaptionRow, Cell<object> groupCaptionCell)
        {
            if (groupCaptionRow != null && groupCaptionCell != null && Parent?.FocusModule != null)
            {
                await Parent.FocusModule.ProcessKeyDown(e, groupCaptionRow, groupCaptionCell).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Gets the caption text for a group row, including aggregate values and localized item count.
        /// </summary>
        /// <param name="Row">The group row for which to generate the caption.</param>
        private string GetCaption(Row<object> Row)
        {
            var groupedData = Row?.Data as Group<TRow>;
            var groupAggregate = new AggregateTemplateContext();

            if (Parent!.GroupSettings!.CaptionTemplate != null && hasGroupCaptionTemplate())
            {
                GridAggregateColumn? firstAggregateColumn = GetFirstAggregate();

                if (firstAggregateColumn != null && groupedData != null)
                {
                    var aggregate = (groupedData.Aggregates as IDictionary<string, object>)!.FirstOrDefault(a =>
                        a.Key.Split(' ')[0] == firstAggregateColumn.Field);
                    var appliedFormatValue = DataUtil.GetFormattedValue(aggregate.Value, firstAggregateColumn.Format!);
                    groupAggregate.Field = firstAggregateColumn.Field;

                    switch (firstAggregateColumn.Type)
                    {
                        case AggregateType.Sum:
                            groupAggregate.Sum = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.Average:
                            groupAggregate.Average = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.Max:
                            groupAggregate.Max = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.Min:
                            groupAggregate.Min = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.Count:
                            groupAggregate.Count = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.TrueCount:
                            groupAggregate.TrueCount = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.FalseCount:
                            groupAggregate.FalseCount = appliedFormatValue?.ToString()!;
                            break;
                        case AggregateType.Custom:
                            groupAggregate.Custom = appliedFormatValue?.ToString()!;
                            break;
                    }
                }
            }

            IReadOnlyList<GroupInfo> parentGroupSnapshots = Array.Empty<GroupInfo>();
            if (Parent.GroupSettings.CaptionTemplate != null)
            {
                var snapshots = new List<GroupInfo>(Row?.ParentGroupSnapshots ?? new List<GroupInfo>());
                parentGroupSnapshots = snapshots;
            }

            IEnumerable childRecords = Array.Empty<object>();
            IReadOnlyList<GroupInfo> childGroups = Array.Empty<GroupInfo>();
            if (Parent.GroupSettings.CaptionTemplate != null && groupedData?.Items != null)
            {
                childGroups = CreateChildGroupSnapshots(groupedData.Items);
                childRecords = GetGroupRecords(groupedData.Items);
            }

            templateData = new CaptionTemplateContext()
            {
                Field = groupedData!.Field,
                Count = groupedData.CountItems,
                GroupGuid = groupedData!.GroupGuid,
                Key = groupedData?.Key?.ToString()!,
                Level = groupedData!.Level,
                GroupAggregates = groupAggregate,
                ParentGroups = parentGroupSnapshots,
                ChildRecords = childRecords,
                ChildGroups = childGroups
            };
            column = GridUtils.GetColumnByField(groupedData.Field!, (List<GridColumn>)Parent.Columns!)!;

            if (column.IsForeignColumn())
            {
                var foreignColumnData = column.ColumnData;
                var query = new List<WhereFilter>()
            {
                new WhereFilter()
                {
                    Field = column.ForeignKeyField ?? column.Field,
                    value = "null".Equals(groupedData.Key?.ToString(), StringComparison.Ordinal) ? null : groupedData.Key,
                    IgnoreCase = false,
                    Operator = "equal"
                }
            };
                templateData.ForeignKeyValue = column.ForeignKeyValue;
                var foreignData = column.GetForeignkeyFilteredData((foreignColumnData as IEnumerable<object>)!, query);

                foreach (var val in foreignData)
                {
                    templateData.ForeignKey = propertyHelper.GetObject(column.ForeignKeyValue!, val)?.ToString()!;
                }
            }

            var colName = column?.HeaderText ?? column?.Field;
            templateData.HeaderText = colName!;
            var colValue = column!.IsForeignColumn() ? templateData.ForeignKey : groupedData.Key;
            var count = (int)groupedData.CountItems;
            var strItems = Parent.Localizer!.GetText(count > 1 ? GridLocaleKeys.Items : GridLocaleKeys.Item);

            return $"{colName}: {colValue} - {count} {strItems}";
        }

        private string GetStyle(Row<object> row, Cell<object> cell)
        {
            var styleText = string.Empty;

            if (Parent!.GroupModule!.DisableExtraFrozenTd)
            {
                return styleText;
            }

            var indent = row?.Cells?.IndexOf(cell) ?? 0;
            indent = Math.Max(indent, 0) * 30;
            var isGroupCaptionTemplate = Parent.GroupModule.IsGroupCaptionTemplate();

            if (hasGroupCaptionTemplate() && row?.Cells != null)
            {
                return $"Left: {indent}px";
            }

            styleText = $"Left: {indent}px";
            if (Parent!.GroupSettings!.EnableLazyLoading && Parent.IsColumnHideOrShow && !isGroupCaptionTemplate && row?.Cells != null)
            {
                List<GridColumn> gridColumns = GridUtils.GetColumns(Parent);
                List<string> originalGroupedColumns = Parent!.GroupSettings!.Columns!
                    .Where(gcol => gridColumns.Any(e => e.Field == gcol)).ToList();
                int groupedLen = originalGroupedColumns.Count;
                int visibleColumnsLen = gridColumns.Where(col => col.Visible).Count();
                int colSpan = visibleColumnsLen + groupedLen +
                    ((((IGrid)Parent).GridTemplates != null && ((IGrid)Parent).GridTemplates.DetailTemplate != null) ? 1 : 0) -
                    row.Indent + (visibleColumnsLen > 0 ? -1 : 0);
                foreach (var rowCell in row.Cells)
                {
                    rowCell.ColSpan = colSpan;
                }
            }

            List<GridColumn> columns = GridUtils.GetColumns(Parent)
                .Where(gridColumn => gridColumn.Visible)
                .ToList();

            if (Parent.FreezeModule!.GetFreezeLeftCount() > 0 || (Parent.FreezeModule!.GetFrozenCount() > 0 && Parent.FreezeModule!.GetFreezeRightCount() < 1))
            {
                if (cell.CellType == CellType.GroupCaption)
                {
                    int remainingIndentColumns = Math.Max(
                        (Parent.GroupSettings?.Columns?.Length ?? 0) - (row?.Indent ?? 0) - 1,
                        0);
                    int frozenLeftColumns = columns.Count(column =>
                        column.Index < Parent.FrozenColumns ||
                        (column.IsFrozen && column.Freeze == FreezeDirection.Left));
                    cell.ColSpan = remainingIndentColumns + frozenLeftColumns;
                    Colspan = columns.Count - frozenLeftColumns;
                }
                else
                {
                    cell.ColSpan = GridUtils.GetColumns(Parent).Count / 2;
                    Colspan = Math.Max(columns.Count - (cell.ColSpan ?? 0), 0);
                }
            }
            else
            {
                Colspan = Math.Max(columns.Count - (cell.ColSpan ?? 0), 0);
            }
            return styleText;
        }

        private bool HasFrozenLeftPane()
        {
            return Parent!.FrozenColumns > 0 || Parent.FreezeModule!.GetFreezeLeftCount() > 0;
        }

        private string GetClass(Row<object> row, Cell<object> cell, bool isFirstFrozenTd = true)
        {
            var cellClassName = BaseClassName(cell, row);
            string classNames = cellClassName.Replace(" e-freezeleftborder", string.Empty, StringComparison.Ordinal).Trim();

            if (isFirstFrozenTd)
            {
                if (HasFrozenLeftPane())
                {
                    classNames += " e-leftfreeze";
                }
            }
            else
            {
                classNames = classNames.Replace(" e-leftfreeze", string.Empty, StringComparison.Ordinal).Trim();
            }

            if (!Parent!.GroupModule!.DisableExtraFrozenTd)
            {
                classNames = classNames.Replace(" e-focus e-focused", string.Empty, StringComparison.Ordinal).Trim();
            }
            if (!string.IsNullOrEmpty(groupCaptionFocusClass))
            {
                classNames += $" {groupCaptionFocusClass}";
            }
            return classNames;
        }

        private GridAggregateColumn? GetFirstAggregate()
        {
            foreach (var aggregate in Parent?.Aggregates!)
            {
                var gridColumn = GridUtils.GetColumns(this.Parent).Where(_ => _.Visible).ToList();
                foreach (var col in gridColumn)
                {
                    if (col == gridColumn.FirstOrDefault() &&
                        (col.Field == aggregate.Columns![0].Field || col.Field == aggregate.Columns[0].ColumnName))
                    {
                        return aggregate.Columns.FirstOrDefault();
                    }
                }
            }
            return null!;
        }

        private bool hasGroupCaptionTemplate()
        {
            return Parent!.Aggregates?.Any(aggregate => aggregate.Columns != null &&
                aggregate.Columns.Any(column => column.GroupCaptionTemplate != null)) ?? false;
        }

        /// <summary>
        /// Creates snapshots of the direct sub-groups contained in a group's items.
        /// </summary>
        private List<GroupInfo> CreateChildGroupSnapshots(IEnumerable items)
        {
            var childGroups = new List<GroupInfo>();
            foreach (var item in items)
            {
                if (item is not Group<TRow> childGroup)
                {
                    return childGroups;
                }

                var field = childGroup.Field;
                childGroups.Add(new GroupInfo
                {
                    Field = field,
                    Key = childGroup.Key?.ToString(),
                    Count = childGroup.CountItems,
                    Level = childGroup.Level - 1,
                    HeaderText = field != null ? GridUtils.GetColumnByField(field, (List<GridColumn>)Parent!.Columns!)?.HeaderText ?? field : null
                });
            }

            return childGroups;
        }

        /// <summary>
        /// Lazily yields the data records of a group, descending into nested sub-groups.
        /// </summary>
        private static IEnumerable<object> GetGroupRecords(IEnumerable items)
        {
            foreach (var item in items)
            {
                if (item is Group<TRow> childGroup)
                {
                    if (childGroup.Items == null)
                    {
                        continue;
                    }

                    foreach (var record in GetGroupRecords(childGroup.Items))
                    {
                        yield return record;
                    }
                }
                else if (item != null)
                {
                    yield return item;
                }
            }
        }

        #endregion
    }
}
