using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.JSInterop;
using System.Threading.Tasks;
using Syncfusion.Blazor.Grids;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Internal;
using Microsoft.AspNetCore.Components.Web;
using System.Globalization;
using Syncfusion.Blazor.Data;

namespace Syncfusion.Blazor.Grids.Internal
{
    internal class GridJSInteropAdaptor<T> : ComponentBase, IDisposable
    {
        #region Initialization & Lifecycle
        public void Init() => _dotnetRef = Create();

        private DotNetObjectReference<GridJSInteropAdaptor<T>>? _dotnetRef { get; set; }

        public DotNetObjectReference<GridJSInteropAdaptor<T>> Create() => DotNetObjectReference.Create<GridJSInteropAdaptor<T>>(this);

        public DotNetObjectReference<GridJSInteropAdaptor<T>> GetRef() => _dotnetRef ?? Create();

        public void Dispose() => _dotnetRef?.Dispose();

        internal SfGrid<T> Parent { get; set; }

        //This property is used for ResizeStarted method json serialize options.
        private JsonSerializerOptions _resizeJsonSettings = new JsonSerializerOptions() {
	    PropertyNameCaseInsensitive = true,
            NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
        };

        //This property is used for GridKeyDown and EndEdit method json serialization options.
        private JsonSerializerOptions _jsonSettings = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true
        };


        public GridJSInteropAdaptor(SfGrid<T> parent)
        {
            Parent = parent;
        }

        #endregion

        #region Column Management & Configuration
        [JSInvokable]
        public async Task SetMediaColumnVisibility(object args)
        {
            var ColumnArgs = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            List<GridColumn> columns = new List<GridColumn>();
            Parent.IsColumnHeaderChange = true;
            foreach (var uid in ColumnArgs?.MediaColVisibility!.Keys!)
            {
                var column = GridUtils.grabColumnByUidOrField(uid, Parent);
                if (column != null)
                {
                    columns.Add(column);
                }
            }

            foreach (var col in columns)
            {
                if (ColumnArgs.MediaColVisibility!.TryGetValue(col.Uid, out bool value))
                {
                    col.SetVisibility(value);
                }
            }

            if (ColumnArgs.InvokedByMedia)
            {
                Parent.ForceUpdate = true;
                await Parent.CallStateHasChangedAsync().ConfigureAwait(true);
            }
            else
            {
                await Parent.DataProcess().ConfigureAwait(true);
            }

            Parent.IsColumnHeaderChange = false;
        }

        [JSInvokable]
        public async ValueTask ColumnReordered(object? args)
        {
            await Parent.ReorderModule!.ColumnReordered(args).ConfigureAwait(true);
        }

        [JSInvokable]
        public void SetColumnIndexes(int StartColumnIndex, int EndColumnIndex)
        {
            if (Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.StartColumnIndex = StartColumnIndex;
                Parent.VirtualScrollModule.EndColumnIndex = EndColumnIndex;
            }
        }

        [JSInvokable]
        public async Task SetPageSizeAndCIndex(object args)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            await Parent.PageSettings!.UpdateProperties("PageSize", (int)action!.PageSize).ConfigureAwait(true);
            if (Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.StartColumnIndex = (int)action?.StartColumnIndex!;
                Parent.VirtualScrollModule.EndColumnIndex = (int)action?.EndColumnIndex!;
                Parent.VirtualScrollModule.SetVTableWidth((int)action?.VTableWidth!);
            }
            if (GridUtils.GetColumns(Parent)?.Where(Col => !string.IsNullOrEmpty(Col.HideAtMedia)).ToList().Count == 0 && !Parent.EnablePersistence)
            {
                await Parent.DataProcess().ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task SetMinWidth(IDictionary<string, string> minWidthValues)
        {
            Parent.MinWidth = minWidthValues;
            await Parent.EventAggregator.NotifyAsync("ColumnWidthStateChange", null!).ConfigureAwait(true);
            await Parent.FreezeModule!.InvokeClientFrozenHeight().ConfigureAwait(true);
        }

        [JSInvokable]
        public void CalculateOffSetWidth(int gridWidth)
        {
            if (Parent.GroupModule != null)
            {
                Parent.GroupModule.GridOffsetWidth = gridWidth;
            }

        }
        #endregion

        #region Column Resizing
        [JSInvokable]
        public void ColumnWidthChanged(object args, bool isResizing = false)
        {
            var settings = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                NumberHandling = JsonNumberHandling.AllowReadingFromString | JsonNumberHandling.WriteAsString
            };
            ActionArgs action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!, settings)!;
            ColumnResizeStop allowStopEvent = JsonSerializer.Deserialize<ColumnResizeStop>(args?.ToString()!, settings);

            if (!string.IsNullOrEmpty(action?.ColumnUid))
            {
                ColumnWidthChanger(action, allowStopEvent, args!, isResizing);
            }
        }

        private async void ColumnWidthChanger(ActionArgs action, ColumnResizeStop columnResizeStop, object args, bool isResizing = false)
        {
            List<GridColumn> columns = GridUtils.GetColumns(Parent);
            GridColumn? column = columns.FirstOrDefault(x => x.Uid == action.ColumnUid);
            column ??= Parent.Columns?.FirstOrDefault(x => x.Uid == action.ColumnUid); // Stacked header resize
            column?.SetWidth(action.Width!.ToString(CultureInfo.InvariantCulture));
            if (columnResizeStop.AllowStopEvent)
            {
                if (Parent.GridEvents?.ResizeStopped.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                {
                    var e = new ResizeArgs() { Column = column!, Parent = Parent };

                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                        await Parent.EventAggregator.NotifyAsync("ResizeStopped", e).ConfigureAwait(true);
                    else
                        await Parent.GridEvents!.ResizeStopped.InvokeAsync(e).ConfigureAwait(true);
                    if (e.Cancel)
                        return;
                }

            }
            if (columnResizeStop.AllowStopEvent && Parent.AllowResizing && Parent.EnablePersistence)
            {
                foreach (var col in Parent.Columns!)
                {
                    col.TableWidth = columnResizeStop.TableWidth;
                    col.LeftFrozenTableWidth = columnResizeStop.LeftFrozenTableWidth;
                    col.RightFrozenTableWidth = columnResizeStop.RightFrozenTableWidth;
                }
                await Parent.SetLocalStorage().ConfigureAwait(true);
            }
            Parent.ForceUpdate = true;
            await Parent.EventAggregator.NotifyAsync("ColumnWidthStateChange", args).ConfigureAwait(true);
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                if (columnResizeStop.AllowStopEvent)
                {
                    if (((IGrid)Parent).GridTemplates?.DetailTemplate != null)
                    {
                        Parent.SoftRefresh = true;
                    }
                    else
                    {
                        if (Parent?.EditSettings?.Mode == EditMode.Batch && Parent?.Rows?.Any(row => row?.Cells?.Any(cell => cell.IsDirty) == true) == true)
                        {
                            Parent.SoftRefresh = true;
                        }
                        Parent?.EventAggregator.Trigger("ContentStateChanged", null!);
                    }
                }
                else if (isResizing)
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.updateResizeCursor", new object[] { Parent.DataId, false }).ConfigureAwait(true);
                }
            }
            Parent!.ForceUpdate = false;
        }

        [JSInvokable]
        public async ValueTask ResizeStarted(object args)
        {
            ActionArgs action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!)!;
            ColumnResizeStop allowStopEvent = JsonSerializer.Deserialize<ColumnResizeStop>(args?.ToString()!, _resizeJsonSettings);
            if (Parent.FreezeModule!.GetFrozenCount() > 0)
            {
                if (Parent.AllowRowDragAndDrop && Parent.RowReorderModule != null)
                {
                    Parent.RowReorderModule.RowReorderIndentWidth = string.Empty;
                }
                if (Parent.AllowGrouping || ((IGrid)Parent).GridTemplates?.DetailTemplate != null)
                {
                    Parent.GroupModule!.IndentWidth = string.Empty;
                }
            }

            if (allowStopEvent.ColumnList?.Count > 0)
            {
                for (var i = 0; i < allowStopEvent.ColumnList.Count; i++)
                {
                    ColumnWidthChanger(allowStopEvent.ColumnList[i], allowStopEvent, args!);
                }
            }

            if (!string.IsNullOrEmpty(action?.ColumnUid))
            {
                List<GridColumn> columns = GridUtils.GetColumns(Parent);
                GridColumn? column = columns.FirstOrDefault(x => x.Uid == action.ColumnUid);
                if (Parent.GridEvents?.OnResizeStart.HasDelegate == true || Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                {
                    var e = new ResizeArgs() { Column = column!, Parent = Parent };

                    if (Parent.IsRenderedFromTreeGrid || Parent.IsRenderedFromPivotTable)
                        await Parent.EventAggregator.NotifyAsync("ResizeStart", e).ConfigureAwait(true);
                    else
                        await (Parent.GridEvents?.OnResizeStart.InvokeAsync(e))!.ConfigureAwait(true)!;

                    if (e.Cancel)
                    {
                        await Parent.InvokeMethod("sfBlazor.Grid.preventResizeAction", new object[] { Parent.DataId, true }).ConfigureAwait(true);
                        return;
                    }
                }
                await Parent.InvokeMethod("sfBlazor.Grid.preventResizeAction", new object[] { Parent.DataId, false }).ConfigureAwait(true);
            }
        }

        private struct ColumnResizeStop
        {
            public List<ActionArgs> ColumnList { get; set; }
            public bool AllowStopEvent { get; set; }
            public string TableWidth { get; set; }
            public string LeftFrozenTableWidth { get; set; }
            public string RightFrozenTableWidth { get; set; }
        }
        #endregion

        #region Frozen Columns & Rows
        [JSInvokable]
        public async Task InvokeFreezeLineMoving(object args)
        {
            await Parent.FreezeModule!.InvokeFreezeLineMoving(args).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task InvokeFreezeLineMoved(object args)
        {
            await Parent.FreezeModule!.InvokeFreezeLineMoved(args).ConfigureAwait(true);

        }

        [JSInvokable]
        public void PreventExtraFrozenCellRendering()
        {
            if (Parent.GroupModule != null)
                Parent.GroupModule.DisableExtraFrozenTd = true;
            Parent.ForceUpdate = true;
            Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            Parent.ForceUpdate = false;
        }
        #endregion

        #region Grouping Operations
        [JSInvokable]
        public async ValueTask GroupColumn(string Field, string Action)
        {
            if (Action == "Group")
            {
                await Parent.GroupModule!.GroupColumn(Field).ConfigureAwait(true);
            }
            else
            {
                await Parent.GroupModule!.UnGroupColumn(Field).ConfigureAwait(true);
            }
        }
        #endregion

        #region Filtering
        [JSInvokable]
        public async Task FilterPopupClose()
        {
            await Parent.FilterModule!.FilterPopupClose().ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task CloseEnhancedOperatorDropdown()
        {
            await Parent.FilterModule!.CloseEnhancedOperatorDropdown().ConfigureAwait(true);
        }

        [JSInvokable]
        public async void FilterMouseOverHandler(string uid, bool showDialog)
        {
            await Parent.FilterModule!.FilterMouseOverHandler(uid, showDialog).ConfigureAwait(true);
        }
        [JSInvokable]
        public void PreventColumnMenuClose(bool IsPrevent)
        {
            Parent.IsColumnMenuFilter = IsPrevent;
        }

        #endregion

        #region Editing & Cell Operations

        [JSInvokable]
        public async void UpdateChanges()
        {
            if(Parent.EditModule!.IsCellEditMode())
            {
                await Parent.EditModule!.EndEdit().ConfigureAwait(true);
                return;
            }
            await Parent.EditModule!.SaveCell().ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task UpdateCell(double rowIndex, string fieldName, string value)
        {

            var targetRow = Parent.Rows?.Find(_ => _.Index == (int)rowIndex);
            var prePasteValue = targetRow != null
                ? Parent.PropHelper?.GetObject(fieldName, targetRow.Data!)
                : null;

            await Parent.EditModule!.UpdateCopyCell(rowIndex, fieldName, value).ConfigureAwait(true);

            if (Parent.UndoRedoManager != null && Parent.EditSettings != null && Parent.EditSettings.EnableUndoRedo == true && Parent.EditSettings.Mode == EditMode.Batch)
            {
                var dataField = await Parent.GetColumnByFieldAsync(fieldName).ConfigureAwait(true);
                if (dataField != null)
                {
                    var columnIndex = targetRow?.Cells
                        ?.Find(_ => _.Column?.Field?.Equals(fieldName, StringComparison.Ordinal) == true)
                        ?.Index ?? -1;
                    Parent.UndoRedoManager.RecordOrBufferCellChange(
                        (int)rowIndex,
                        (int)columnIndex,
                        fieldName,
                        prePasteValue,
                        value,
                        dataField);
                }
            }
        }

        [JSInvokable]
        public async Task UpdateAutofillCell(double rowIndex, string fieldName, string columnName, double valueIndex, string value)
        {
            var Row = Parent.Rows?.FirstOrDefault(_ => _.Index == valueIndex);
            if (Parent.GridEvents?.BeforeAutoFill.HasDelegate == true)
            {
                var args = new BeforeAutoFillEventArgs
                {
                    Cancel = false,
                };
                await Parent.GridEvents.BeforeAutoFill.InvokeAsync(args).ConfigureAwait(true);
                if (args.Cancel)
                {
                    return;
                }
            }
            if (Parent.EditModule != null && (Row?.Data?.GetType() == typeof(System.Dynamic.ExpandoObject) || (Row?.Data is System.Dynamic.DynamicObject)))
            {
                await Parent.EditModule.UpdateCopyCell(rowIndex, fieldName, value, columnName, valueIndex).ConfigureAwait(true);
            }
            else
            {
                await Parent.EditModule!.UpdateAutofillCell(rowIndex, fieldName, columnName, valueIndex).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Begins an autofill batch. Called from JS when the user starts
        /// dragging the autofill handle, BEFORE the first UpdateAutofillCell.
        /// While a batch is active, per-cell changes are buffered rather than
        /// pushed as individual undo actions.
        /// </summary>
        [JSInvokable]
        public async Task BeginAutoFillBatch()
        {
            Parent.UndoRedoManager?.BeginBatch(UndoRedoActionType.AutoFill);
        }

        /// <summary>
        /// Commits the currently buffered autofill batch as a single atomic
        /// UndoRedoAction on the undo stack. Called from JS on the
        /// mouseup that ends the autofill drag. No-op if no batch is active
        /// or the batch is empty.
        /// </summary>
        [JSInvokable]
        public async Task CommitAutoFillBatch()
        {
            Parent.UndoRedoManager?.CommitBatch();
        }

        /// <summary>
        /// Discards the currently buffered autofill batch without recording
        /// anything. Called from JS when the autofill drag is cancelled
        /// (e.g. by Escape, or when BeforeAutoFill is cancelled).
        /// </summary>
        [JSInvokable]
        public async Task DiscardAutoFillBatch()
        {
            Parent.UndoRedoManager?.DiscardBatch();
        }

        /// <summary>
        /// Begins a paste batch. Called from JS BEFORE the first pasted
        /// cell UpdateCell so that the whole paste is coalesced into one
        /// atomic Paste undo action.
        /// </summary>
        [JSInvokable]
        public async Task BeginPasteBatch()
        {
            Parent.UndoRedoManager?.BeginBatch(UndoRedoActionType.Paste);
        }

        /// <summary>
        /// Commits the currently buffered paste batch as a single atomic
        /// UndoRedoAction on the undo stack. Called from JS when the paste
        /// operation ends.
        /// </summary>
        [JSInvokable]
        public async Task CommitPasteBatch()
        {
            Parent.UndoRedoManager?.CommitBatch();
        }

        /// <summary>
        /// Discards the currently buffered paste batch without recording
        /// anything. Called from JS when the paste is cancelled.
        /// </summary>
        [JSInvokable]
        public async Task DiscardPasteBatch()
        {
            Parent.UndoRedoManager?.DiscardBatch();
        }

        [JSInvokable]
        public async Task UpdateAutofillPositions(object positions, string updateFunction)
        {
            if (Parent.SelectionModule != null)
            {
                await Parent.SelectionModule.HandleAutofillPositionUpdate(positions, updateFunction).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task EndEdit(object? e = null)
        {
            if (Parent.IsRenderedFromGantt) return;
            string? KeyCombination = null;
            if (e != null)
            {
                KeyboardEventArgs? action = JsonSerializer.Deserialize<KeyboardEventArgs>(e.ToString()!, _jsonSettings);
                Parent.EditModule!.KeyCode = action?.Code!;
                Parent.EditModule.IsShiftKey = action?.ShiftKey ?? false;
                KeyCombination = action?.GetKeyCombination();
            }
            if (KeyCombination == "Enter")
            {
                Parent.FocusModule!.SelectedCellIndex = await Parent.EditModule!.GetSelectedCellIndex().ConfigureAwait(true);
            }
            await Parent.EditModule!.EndEdit(null!, false, KeyCombination!).ConfigureAwait(true);
            Parent.EditModule.KeyCode = null!;
            Parent.EditModule.IsShiftKey = false;
        }


        [JSInvokable]
        public void ShowValidationPopup(IDictionary<string, string> position, string arrowPosition)
        {
            foreach (var pos in position)
            {
                Parent.EditModule!.position?.AddOrUpdateItem(pos.Key, pos.Value);
            }

            Parent.EditModule!.ArrowPosition = arrowPosition;
            Parent.EventAggregator.Trigger("ShowValidationMessage", null!);
        }

        [JSInvokable]
        public void RemoveValidationPopup()
        {
            Parent.EditModule!.ClearValidationErrors();
        }

        #endregion

        #region Selection & Focus

        [JSInvokable]
        public void GridFocus(object args)
        {
            args = null!;
            if (Parent.FocusModule != null)
                Parent.FocusModule.ProcessGridFocus(null!);
        }

        [JSInvokable]
        public async Task SelectCellByRow(int rowIndex, double cellIndex)
        {
            Row<object>? row = Parent.Rows?.FirstOrDefault(_ => _.Index == rowIndex);
            if (Parent.SelectionModule != null)
                await Parent.SelectionModule.SelectAutofillCell(row!, cellIndex).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task ClearSelection()
        {
            if (Parent.SelectionModule != null)
            {
                await Parent.SelectionModule.ClearCellSelection(autofillSelect: true).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task SelectRow(int index, bool isScrollIntoView = false, int focusColumnIndex = -1)
        {
            if (Parent.SelectionModule != null)
            {
                if (isScrollIntoView)
                {
                    await Parent.SelectionModule.SelectRow(index, isSelectionMethodInvoked: true, isScrollIntoView: isScrollIntoView, focusColumnIndex: focusColumnIndex).ConfigureAwait(true);
                }
                else
                {
                    await Parent.SelectionModule.SelectRow(index).ConfigureAwait(true);
                }
            }
        }

        #endregion

        #region Selection - Drag Operations

        [JSInvokable]
        public async Task DragSelection(int StartIndex, int EndIndex, bool ClearAll, string TargetId = null!, int StartCellIndex = 0, int EndCellIndex = 0)
        {
            await Parent.SelectionModule!.DragSelection(StartIndex, EndIndex, ClearAll, TargetId, StartCellIndex, EndCellIndex).ConfigureAwait(true);
        }

        [JSInvokable]
        public async Task DragSelectionStarted(int RowIndex, int CellIndex)
        {
            if (Parent.SelectionModule != null)
            {
                await Parent.SelectionModule.HandleDragSelectionStarting(RowIndex, CellIndex).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task DragCellSelection(int StartIndex, int StartCellIndex, int RowIndex, int CellIndex, bool ClearAll, string TargetId = null!)
        {
            await Parent.SelectionModule!.DragCellSelection(StartIndex, StartCellIndex, RowIndex, CellIndex, ClearAll, TargetId).ConfigureAwait(true);
        }

        #endregion

        #region Row Reordering & Drag-Drop

        [JSInvokable]
        public async Task ReorderRows(int fromIndex, int toIndex, string action, bool isDragWithinGrid, string targetClass, string targetId, object? targetDimension = null, DotNetObjectReference<GridJSInteropAdaptor<T>>? destInstance = null, bool outsideGrid = false, bool differentTValue = false, string? fromUid = null, string? toUid = null, bool withinBothGrid = false, double clientX = 0, double clientY = 0)
        {
            if (Parent.RowReorderModule != null)
            {
                Parent.RowReorderModule.IsReorderByInteraction = true;
                await Parent.RowReorderModule.ReorderRows(fromIndex, toIndex, action, isDragWithinGrid, targetClass, targetId, targetDimension, destInstance, outsideGrid, differentTValue, fromUid, toUid, withinBothGrid: withinBothGrid, clientX: clientX, clientY: clientY).ConfigureAwait(true);
                Parent.RowReorderModule.IsReorderByInteraction = false;
            }
        }

        [JSInvokable]
        public async Task RowDragStartEvent(int fromIndex, string Uid)
        {
            if (Parent.RowReorderModule != null)
            {
                await Parent.RowReorderModule.RowDragStartEvent(fromIndex, Uid).ConfigureAwait(true);
            }
        }


        [JSInvokable]
        public async Task DisableShowAddForm(string requestType, bool isTargetAddForm, DotNetObjectReference<GridJSInteropAdaptor<T>> destInstance = null!)
        {
            if (Parent.RowReorderModule != null)
            {
                Parent.RowReorderModule.HandleAddFormStateOnRowDrag(requestType, isTargetAddForm, destInstance);
            }
        }
        #endregion

        #region UI State Management
        [JSInvokable]
        public void LastRowBorder(bool IsRequired)
        {
            if (Parent.RequireLastRowBorder != IsRequired)
            {
                Parent.RequireLastRowBorder = IsRequired;
                Parent.EventAggregator.Trigger("ContentStateChanged", null!);
            }
        }

        [JSInvokable]
        public void MaximumVisibleRows(int VisibleRows)
        {
            Parent.MaxVisibleRowsCount = VisibleRows;
        }
        #endregion

        #region KeyDown and MouseDown Events
        [JSInvokable]
        public void MouseDownHandler(string Target, string CellUid, string rowUid, int? cellColIndex)
        {
            var row = new Row<object>();
            var cell = new Cell<object>();
            using var column = new GridColumn();
            if ((!string.IsNullOrEmpty(CellUid) || (rowUid != null && cellColIndex != null)) && Target == "Content")
            {
                row = Parent.Rows?.FirstOrDefault(r => r.Uid == rowUid);
                cell = row?.Cells?.FirstOrDefault(c => c.Index + 1 == cellColIndex);
            }

            if (!string.IsNullOrEmpty(CellUid) && Target == "Header")
            {
                row = Parent.HeaderRows.Concat(Parent.FrozenHeaderRows)
                    .Concat(Parent.FrozenRightHeaderRows)
                    .FirstOrDefault(r => r.Cells?.Any(c => c.Uid == CellUid) == true);
                cell = row?.Cells?.FirstOrDefault(c => c.Uid == CellUid);
            }

            if(Parent.EditSettings != null && Parent.EditSettings.Validator != null && Target == "Edit" && Parent.EditSettings.Mode == EditMode.Normal && !(Parent.EditSettings.ShowAddNewRow))
            {
                Parent.PreventEndEdit = true;
            }

            var arguments = new GridMouseDown() { Column = cell?.Column!, Row = row!, Cell = cell!, Target = Target };
            Parent.EventAggregator.Trigger("MouseDown", arguments);
        }

        [JSInvokable]
        public async ValueTask GridKeyDown(object args, bool value, bool isPagerFocused, bool isToolbarFocused, int? cellIndex, int? rowIndex = null, int? templateCellIndex = null, bool focusColumnTemplate = false, bool isPopUpOpened = false)
        {
            KeyboardEventArgs? action = JsonSerializer.Deserialize<KeyboardEventArgs>(args?.ToString()!, _jsonSettings);
            if (Parent.FocusModule != null)
            {
                Parent.FocusModule.isPopUpOpened = isPopUpOpened;
                await Parent.FocusModule.ProcessGridKeyDown(action!, null!, isPagerFocused, isToolbarFocused, cellIndex, rowIndex, templateCellIndex, focusColumnTemplate).ConfigureAwait(true);
            }
        }
        #endregion

        #region Virtualization & Infinite Scroll
        [JSInvokable]
        public async Task VirtualRefresh(object args, int scrollTop = 0, int selectedRowIndex = -1, bool isScrollIntoView = false, int focusColumnIndex = -1, bool frozenMidScroll = false, bool focusFromPager = false, bool isPreventFocusScroll = false)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            if (Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.NextRowToNavigate = (int)action!.NextRowToNavigate;
                Parent.VirtualScrollModule.ScrollTop = Parent.OverscanCount == 0 ? scrollTop : Parent.VirtualScrollModule.ScrollTop;
                Parent.VirtualScrollModule.RequestType = action?.RequestType!;
            }
            if (action?.RequestType == "virtualscroll" && action?.Axis != "X")
            {
                await Parent.VirtualScrollModule!.HandleVerticalScrollAsync(action!, scrollTop, selectedRowIndex, isScrollIntoView, focusColumnIndex,
                    isPreventFocusScroll).ConfigureAwait(true);
            }
            if (action?.RequestType == "virtualscroll" && action?.Axis == "X" && Parent.VirtualScrollModule != null)
            {
                await Parent.VirtualScrollModule.HandleHorizontalScrollAsync(action, frozenMidScroll, focusFromPager).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task DomVirtualScroll(int scrollTop)
        {
            if (Parent.EnableDomVirtualization)
            {
                await Parent.EventAggregator.NotifyAsync("DomVirtualScroll", scrollTop).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task SetDomVirtualMaxHeight(int maxHeight)
        {
            if (Parent.EnableDomVirtualization)
            {
                await Parent.EventAggregator.NotifyAsync("DomVirtualMaxHeightChanged", maxHeight).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task LoadInfiniteData(object args, bool isBottom = false, bool isTop = false, bool isLazyLoadChild = false, string middleRowUid = null!, string lastRowUid = null!, int middleRowIndex = 0, bool keyInteractionScroll = false)
        {
            ActionArgs? action = JsonSerializer.Deserialize<ActionArgs>(args?.ToString()!);
            if (!isLazyLoadChild && Parent.InfiniteScrollModule != null)
            {
                Parent.InfiniteScrollModule.KeyInteractionScroll = keyInteractionScroll;
                await Parent.InfiniteScrollModule.GenerateInfiniteScrollDatas(action!, isBottom, isTop).ConfigureAwait(true);
                Parent.InfiniteScrollModule.KeyInteractionScroll = false;
            }
            else if (isLazyLoadChild && Parent.InfiniteScrollModule != null)
            {
                Parent.EventAggregator.Trigger("ContentStateChanged", true);
                await Parent.InvokeMethod("sfBlazor.Grid.updateClonedMaskTranslates", new object[] { Parent.DataId }).ConfigureAwait(true);
                await Task.Yield();
                await Parent.InfiniteScrollModule.LoadLazyLoadChildData(action!, middleRowUid, lastRowUid).ConfigureAwait(true);
            }
        }
        #endregion

        #region Clipboard Operations

        [JSInvokable]
        public async Task InvokeCopyPasteAction(object args, string name)
        {
            BeforeCopyPasteEventArgs? copy = JsonSerializer.Deserialize<BeforeCopyPasteEventArgs>(args.ToString()!);
            if (Parent.GridEvents?.BeforeCopyPaste.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                BeforeCopyPasteEventArgs eventArgs = new BeforeCopyPasteEventArgs()
                {
                    Action = name,
                    Cancel = false,
                    ClipboardText = copy?.ClipboardText!
                };
                if (Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("BeforeCopyPaste", eventArgs).ConfigureAwait(true);
                else
                    await (Parent.GridEvents?.BeforeCopyPaste.InvokeAsync(eventArgs))!.ConfigureAwait(true)!;
                await Parent.InvokeMethod("sfBlazor.Grid.preventCopyToClipBoard", new object[] { Parent.DataId, eventArgs.Cancel, eventArgs.ClipboardText, eventArgs.Action }).ConfigureAwait(true);
            }
            else
            {
                await Parent.InvokeMethod("sfBlazor.Grid.preventCopyToClipBoard", new object[] { Parent.DataId, false, copy?.ClipboardText!, name }).ConfigureAwait(true);
            }
        }

        [JSInvokable]
        public async Task<List<PasteCell>> InvokeBatchPasteAction(List<PasteCell> pasteCells)
        {
            var processedCells = new List<PasteCell>();
            if (pasteCells == null || pasteCells.Count == 0 || Parent.EditModule == null)
            {
                return processedCells;
            }
            bool isCellEditMode = Parent.EditSettings?.Mode == EditMode.Cell;
            string? primaryKeyField = null;
            if (isCellEditMode)
            {
                var primaryKeyFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                primaryKeyField = primaryKeyFields?.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(primaryKeyField))
                {
                    return processedCells;
                }
            }
            var cellUpdates = new List<CellUpdate>();
            var pendingCellModeResults = new List<PasteCell>();
            foreach (PasteCell pasteCell in pasteCells)
            {
                if (pasteCell == null)
                {
                    continue;
                }
                int rowIndex = pasteCell.RowIndex;
                int columnIndex = pasteCell.ColumnIndex;
                string columnField = pasteCell.ColumnField ?? string.Empty;
                string cellValue = pasteCell.Value ?? string.Empty;
                if (string.IsNullOrWhiteSpace(columnField))
                {
                    continue;
                }
                Row<object>? row = Parent.Rows?.Find(item => item.Index == rowIndex);
                if (row?.Data == null)
                {
                    continue;
                }
                if (Parent.GridEvents?.BeforeCellPaste.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
                {
                    var eventArgs = new BeforeCellPasteEventArgs<T>
                    {
                        Cancel = false,
                        CellValue = cellValue,
                        RowIndex = rowIndex,
                        ColumnName = columnField,
                        ColumnIndex = columnIndex,
                        Data = (T)row.Data
                    };

                    if (Parent.IsRenderedFromTreeGrid)
                    {
                        await Parent.EventAggregator.NotifyAsync("BeforeCellPaste", eventArgs).ConfigureAwait(true);
                    }
                    else
                    {
                        await Parent.GridEvents!.BeforeCellPaste.InvokeAsync(eventArgs).ConfigureAwait(true);
                    }
                    if (eventArgs.Cancel)
                    {
                        continue;
                    }

                    rowIndex = eventArgs.RowIndex;
                    columnIndex = eventArgs.ColumnIndex;
                    columnField = eventArgs.ColumnName ?? columnField;
                    cellValue = eventArgs.CellValue ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(columnField))
                    {
                        continue;
                    }
                    if (rowIndex != pasteCell.RowIndex)
                    {
                        row = Parent.Rows?.Find(item => item.Index == rowIndex);
                        if (row?.Data == null)
                        {
                            continue;
                        }
                    }
                }

                if (isCellEditMode)
                {
                    object? primaryKeyValue = Parent.PropHelper?.GetObject(primaryKeyField!, row.Data);
                    if (primaryKeyValue == null)
                    {
                        continue;
                    }
                    object? sanitizedValue = await Parent.EditModule.GetCellPasteValueAsync(columnField, cellValue).ConfigureAwait(true);
                    cellUpdates.Add(new CellUpdate
                    {
                        PrimaryKeyValue = primaryKeyValue,
                        ColumnField = columnField,
                        NewValue = sanitizedValue
                    });

                    pendingCellModeResults.Add(
                        new PasteCell
                        {
                            RowIndex = rowIndex,
                            ColumnIndex = columnIndex,
                            ColumnField = columnField,
                            Value = cellValue
                        });
                }
                else
                {
                    await Parent.EditModule.UpdateCopyCell(rowIndex, columnField, cellValue).ConfigureAwait(true);
                    processedCells.Add(new PasteCell
                    {
                        RowIndex = rowIndex,
                        ColumnIndex = columnIndex,
                        ColumnField = columnField,
                        Value = cellValue
                    });
                }
            }
            if (isCellEditMode && cellUpdates.Count > 0)
            {
                await Parent.SetCellValueAsync(cellUpdates).ConfigureAwait(true);
                processedCells.AddRange(pendingCellModeResults);
            }
            return processedCells;
        }

        #endregion

        #region InitModules

        [JSInvokable]
        public async Task SetGridInitInfo(string? width, bool? isRowDragCell, int? rowHeight, bool? isMacDevice)
        {
            if (!string.IsNullOrEmpty(width))
            {
                await Parent.RefreshIndentWidth(width, isRowDragCell ?? false).ConfigureAwait(true);
            }

            if (rowHeight.HasValue && Parent.VirtualScrollModule != null)
            {
                Parent.VirtualScrollModule.RHeight = rowHeight.Value;
                Parent.EventAggregator.Trigger("VirtualComponentUpdate", null!);
            }

            Parent.IsMacDevice = isMacDevice;
        }

        #endregion

        #region WebMCP Bridge (JS -> C# invokable)

        private static readonly JsonSerializerOptions WebMcpJsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /// <summary>
        /// Maximum wire size, in bytes, of a single WebMCP tool response
        /// after UTF-8 serialization. This is the single enforcement
        /// point for the response-size cap — every response that leaves
        /// <see cref="InvokeWebMcpToolExecute"/> is checked against this
        /// constant.
        /// </summary>
        /// <remarks>
        /// <para>
        /// 1 MB is a safe ceiling for the union of transport constraints
        /// the bridge sits behind:
        /// </para>
        /// <list type="bullet">
        /// <item><description>
        /// SignalR's default <c>MaximumReceiveMessageSize</c> is 32 KB;
        /// most production Blazor Server hosts raise this to 1 MB or
        /// higher, so 1 MB is the upper bound that flows without
        /// transport reconfiguration.
        /// </description></item>
        /// <item><description>
        /// Common MCP client SDKs (Anthropic, OpenAI, the official
        /// TypeScript SDK) accept payloads of at least 1 MB by default.
        /// A larger cap would silently fail on some clients.
        /// </description></item>
        /// <item><description>
        /// 1 MB holds roughly 5,000 records of 200 characters each —
        /// well past the point where an LLM can reason over the result
        /// regardless of transport. The cap is a correctness signal, not
        /// just a memory signal.
        /// </description></item>
        /// </list>
        /// <para>
        /// When the cap is exceeded, the bridge throws
        /// <see cref="InvalidOperationException"/> so the failure is
        /// visible at the JS-interop boundary — not silently truncated,
        /// not converted to a vague error. The exception message includes
        /// the actual byte count for diagnostics.
        /// </para>
        /// </remarks>
        private const int MaxResponseBytes = 1_048_576; // 1 MB

        /// <summary>
        /// Serializes <paramref name="response"/> to UTF-8 once and
        /// throws <see cref="InvalidOperationException"/> if the result
        /// exceeds <see cref="MaxResponseBytes"/>.
        /// </summary>
        /// <param name="response">The dispatcher-built response payload.</param>
        /// <returns>The JSON-serialized string ready for JS-interop return.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the serialized UTF-8 payload exceeds
        /// <see cref="MaxResponseBytes"/>.
        /// </exception>
        private static string SerializeAndEnforceCap(WebMcpToolResponse response)
        {
            byte[] payload = JsonSerializer.SerializeToUtf8Bytes(response, WebMcpJsonOptions);
            if (payload.Length > MaxResponseBytes)
            {
                throw new InvalidOperationException(
                    $"WebMCP tool response exceeds the {MaxResponseBytes:N0}-byte limit " +
                    $"(actual: {payload.Length:N0} bytes). ");
            }
            return Encoding.UTF8.GetString(payload);
        }

        /// <summary>
        /// JS-invokable bridge from the browser's <c>document.modelContext.registerTool()</c> handler.
        /// When an AI agent invokes a tool, the browser forwards the call to this method which delegates to
        /// the <see cref="Syncfusion.Blazor.Grids.Actions.WebMcpAdapter{TValue}"/> dispatcher.
        /// </summary>
        /// <param name="command">The fully-prefixed tool name (e.g., <c>SfGrid-123_sortByColumn</c>).</param>
        /// <param name="argsJson">JSON-serialized dictionary of arguments from the agent.</param>
        /// <returns>A JSON-serialized <see cref="WebMcpToolResponse"/> for the agent to consume.</returns>
        /// <remarks>
        /// The JS layer is responsible for:
        /// <list type="bullet">
        /// <item><description>Receiving the agent's call from <c>document.modelContext</c></description></item>
        /// <item><description>Stripping its own prefix and forwarding the base tool name</description></item>
        /// <item><description>JSON-serializing args into a plain dictionary</description></item>
        /// </list>
        /// The dispatcher then handles event firing, validation, cancellation, and routing.
        /// </remarks>
        [JSInvokable]
        public async Task<string> InvokeWebMcpToolExecute(string command, string argsJson)
        {
            // Early-out: if WebMCP is disabled or adapter is null, return a clean error
            var adapter = Parent._webMcpAdapter;
            if (adapter == null)
            {
                var disabledResponse = new WebMcpToolResponse
                {
                    IsError = true,
                    Content = new List<WebMcpToolResponse.ContentItem>
                    {
                        new() { Type = "text",
                            Text = "{\"errorCode\":1002,\"errorName\":\"ExecutionFailed\",\"message\":\"WebMCP is not enabled for this grid (set EnableWebMcp=true).\"}"
                        }
                    }
                };
                return SerializeAndEnforceCap(disabledResponse);
            }

            Dictionary<string, object> args;
            try
            {
                // Deserialize to a Dictionary of JsonElement first so we can
                // walk the tree deterministically and convert each leaf into
                // its native CLR type. Without this pass, every handler
                // would have to call ((JsonElement)args["x"]).GetString() etc.
                Dictionary<string, object>? raw = string.IsNullOrWhiteSpace(argsJson)
                    ? new Dictionary<string, object>()
                    : JsonSerializer.Deserialize<Dictionary<string, object>>(argsJson);
                args = raw == null
                    ? new Dictionary<string, object>()
                    : ConvertArgsDictionary(raw);
            }
            catch (JsonException ex)
            {
                var parseErrorResponse = new WebMcpToolResponse
                {
                    IsError = true,
                    Content = new List<WebMcpToolResponse.ContentItem>
                    {
                        new() { Type = "text",
                            Text = $"{{\"errorCode\":1001,\"errorName\":\"InvalidArgument\",\"message\":\"Failed to parse tool args JSON: {ex.Message}\"}}"
                        }
                    }
                };
                return SerializeAndEnforceCap(parseErrorResponse);
            }

            WebMcpToolResponse response = await adapter.ExecuteHandler(command, args).ConfigureAwait(true);

            // Byte-cap enforcement. When the serialized response exceeds
            // MaxResponseBytes, SerializeAndEnforceCap throws
            // InvalidOperationException so the failure is visible at the
            // JS-interop boundary rather than silently truncated. The
            // exception propagates out of this [JSInvokable] method and
            // surfaces as a Blazor interop error to the calling JavaScript.
            return SerializeAndEnforceCap(response);
        }

        /// <summary>
        /// Walks the deserialized args dictionary and rewrites every leaf
        /// from <see cref="JsonElement"/> to its native CLR equivalent so
        /// user-supplied handlers can read <c>ctx.Args["x"]</c> as a
        /// <see cref="string"/> / <see cref="int"/> / <see cref="bool"/> /
        /// nested <see cref="Dictionary{TKey, TValue}"/> without casting.
        /// </summary>
        /// <remarks>
        /// Number coercion uses <see cref="JsonElement.TryGetInt64(out long)"/>
        /// then <see cref="JsonElement.TryGetDouble(out double)"/> — the former
        /// avoids floating-point drift on whole-number JSON, the latter
        /// preserves the precision of fractional values. Property names are
        /// preserved verbatim (camelCase / PascalCase both round-trip).
        /// </remarks>
        private static Dictionary<string, object> ConvertArgsDictionary(Dictionary<string, object> source)
        {
            var result = new Dictionary<string, object>(source.Count);
            foreach (var kvp in source)
            {
                // CS8601: ConvertJsonValue returns object? (because null JSON
                // values land in the tree). The runtime dictionary is
                // non-nullable; the assignment is safe because the value is
                // either a real CLR object or null which serialises to JSON
                // null and is consumed by the dispatcher as such.
                result[kvp.Key] = ConvertJsonValue(kvp.Value)!;
            }
            return result;
        }

        private static object? ConvertJsonValue(object? value)
        {
            switch (value)
            {
                case null:
                    return null;

                case JsonElement element:
                    return ConvertJsonElement(element);

                case List<object> list:
                    // STJ sometimes deserializes into List<object> already
                    // (e.g. when array elements are heterogeneous objects).
                    var converted = new List<object?>(list.Count);
                    foreach (var item in list)
                    {
                        converted.Add(ConvertJsonValue(item));
                    }
                    return converted;

                // Already a native CLR type — pass through.
                default:
                    return value;
            }
        }

        private static object? ConvertJsonElement(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.String:
                    // Plain strings land here. JSON null inside a string
                    // position is treated as the string "null" by STJ,
                    // matching what the browser sent — we do not second-guess.
                    return element.GetString();

                case JsonValueKind.Number:
                    if (element.TryGetInt64(out var asLong))
                    {
                        return asLong;
                    }
                    return element.GetDouble();

                case JsonValueKind.True:
                    return true;

                case JsonValueKind.False:
                    return false;

                case JsonValueKind.Null:
                case JsonValueKind.Undefined:
                    return null;

                case JsonValueKind.Object:
                    {
                        var nested = new Dictionary<string, object>();
                        foreach (var prop in element.EnumerateObject())
                        {
                            // CS8601: ConvertJsonElement returns object? —
                            // the dictionary value is non-nullable, but the
                            // contract is "absent → null" and consumers read
                            // through Dictionary<string, object> which
                            // already tolerates nulls.
                            nested[prop.Name] = ConvertJsonElement(prop.Value)!;
                        }
                        return nested;
                    }

                case JsonValueKind.Array:
                    {
                        var items = new List<object?>();
                        foreach (var child in element.EnumerateArray())
                        {
                            items.Add(ConvertJsonElement(child));
                        }
                        return items;
                    }

                default:
                    return null;
            }
        }

        #endregion
    }
}