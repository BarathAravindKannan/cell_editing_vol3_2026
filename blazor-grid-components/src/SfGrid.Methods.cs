using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Grids.Internal;
using Syncfusion.Blazor.Grids.Actions;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Collections.Specialized;
using System.IO;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace Syncfusion.Blazor.Grids
{
    public partial class SfGrid<TValue> : SfDataBoundComponent, IGrid, ISfCircularComponent
    {
        /// <summary>
        /// Adds a new record to the Grid at a specific row index.
        /// </summary>
        /// <param name="data">New record to be added. The data should be of the same type as the generic type of the grid.</param>
        /// <param name="index">The index in which the new record is to be added. If no index is provided, the record will be added to the end of the grid.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method adds the row only if <see cref="Syncfusion.Blazor.Grids.GridEditSettings.AllowAdding"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="AddRecord" @onclick="AddItem">AddRecord</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task AddItem()
        ///    {
        ///        var data = new Order() { OrderID = 1000, CustomerID = "ALFKI", OrderDate = new DateTime(1995, 03, 25), Freight = 25.7 * 2) };
        ///        await grid.AddRecordAsync(data, 1); // pass data and index here.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task AddRecordAsync(TValue data, Nullable<int> index = null)
        {
            if (EditSettings != null &&!EditSettings.AllowAdding)
            {
                return;
            }

            await EditModule!.AddRecord(data!, index).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates the Grid component UI with a batch of changes, including new records, edited records, and deleted records.
        /// </summary>
        /// <param name="batchChanges">It contains the collection of records to add, edit, and delete.</param>
        /// <remarks>This method is used to make bulk changes to the grid UI when in <c>EditMode.Batch</c>.
        /// The edited and newly added records will be visually highlighted in the grid UI, and the highlighting will be cleared once the changes are saved or canceled.
        ///</remarks>
        /// <value>
        /// A task representing the asynchronous operation. The task result can indicate the success or failure of the batch update operation.
        /// </value>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetData" @onclick="SetData">Apply Batch Changes</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  <GridEditSettings AllowAdding="true" AllowEditing="true" AllowDeleting="true" Mode="EditMode.Batch"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task SetData()
        ///    {
        ///      var batchChanges = new BatchChanges<Order>()
        ///        {
        ///            AddedRecords = new List<Order>() { new Order() { OrderID = 1, CustomerID = "ANTAR" } }, DeletedRecords = new List<Order>() { new Order() { OrderID = 1002 } }, ChangedRecords = new List<Order>() { new Order() { OrderID = 1001, CustomerID = "VINET" } } 
        ///        }
        ///       await grid.ApplyBatchChangesAsync(batchChanges);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ApplyBatchChangesAsync(BatchChanges<TValue> batchChanges)
        {
            if (!EditSettings!.Mode.Equals(EditMode.Batch) || batchChanges == null)
            {
                return;
            }
            await EditModule!.ApplyBatchChanges(batchChanges).ConfigureAwait(true);
        }

        /// <summary>
        /// Changes the column width to automatically fit its content and ensure that the content is not wrapped or hidden. This method will ignore any hidden columns.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// You can use this method in the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DataBound"/> event or set the <see cref="GridColumn.AutoFit"/> property to autofit the columns at initial rendering.
        /// </remarks>   
        /// <example>
        /// <code><![CDATA[
        /// <button id="AutoFit" @onclick="AutoFit">AutoFit Column</button>
        /// <SfGrid @ref="Grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> Grid;
        ///    private async Task AutoFit()
        ///    {
        ///       await grid.AutoFitColumnsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task AutoFitColumnsAsync()
        {
            IsAutoFitEnabled = true;
            await InvokeMethod("sfBlazor.Grid.autoFitColumns", new object[] { DataId, Columns!, Array.Empty<string>(), true }).ConfigureAwait(true);
        }

        /// <summary>
        /// A new row with input fields is rendered in grid content, for user to fill the fields and then to save the new record.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method opens a form with input fields to add a new record to the grid.
        /// The new record will only be added if <see cref="Syncfusion.Blazor.Grids.GridEditSettings.AllowAdding"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="AddRecord" @onclick="AddRecord">AddItem</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task AddRecord()
        ///    {
        ///      await grid.AddRecordAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task AddRecordAsync()
        {
            if (EditSettings != null && !EditSettings.AllowAdding)
            {
                return;
            }

            await EditModule!.AddRecord(null!, null).ConfigureAwait(true);
        }

        /// <summary>
        /// Automatically adjusts the width of specified columns to fit their content, without wrapping or hiding.       
        /// </summary>
        /// <param name="fieldNames">An array of columns to be auto fitted, identified by their <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the asynchronous operation.</returns>
        /// <remarks>
	/// Hidden columns are ignored for this autofit process.
        /// You can use this method in the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DataBound"/> event or set the <see cref="GridColumn.AutoFit"/>  property to autofit the columns at initial rendering.
        /// If <see cref="EnablePersistence"/> is <c>true</c>, the current autofit state of the columns will be persisted across page refreshes.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="AutoFit" @onclick="AutoFit">AutoFit Column</button>
        /// <SfGrid @ref="Grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> Grid;
        ///    private async Task AutoFit()
        ///    {
        ///       string[] Columns = { "OrderID", "CustomerID", "Freight" };
        ///       await grid.AutoFitColumnsAsync(Columns);
        ///    }
        /// }
        /// ]]>
        /// </code>
		/// </example>
        public async Task AutoFitColumnsAsync(string[] fieldNames)
        {
            if (EnablePersistence)
            {
                _isPersistAutoFit = true;
                Columns!.ForEach(col => col.IsPersistAutoFit = true);
            }
            IsAutoFitEnabled = true;
            await InvokeMethod("sfBlazor.Grid.autoFitColumns", new object[] { DataId, Columns!, fieldNames, true }).ConfigureAwait(true);
        }
        
	/// <summary>
        /// Automatically adjusts the width of a specified column to fit its content, without wrapping.
        /// Hidden columns are ignored.
        /// </summary>
        /// <param name="fieldName">The name of the column to be auto fitted, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// You can use this method in the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DataBound"/> event or set the <see cref="GridColumn.AutoFit"/>  property to autofit the columns at initial rendering.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="AutoFitColumn" @onclick="AutoFitColumn">AutoFitColumn</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task AutoFitColumn()
        ///    {
        ///       string columnField = "OrderID";
        ///       await grid.AutoFitColumnAsync(columnField);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task AutoFitColumnAsync(string fieldName)
            => await AutoFitColumnsAsync(new string[] { fieldName }).ConfigureAwait(true);

	/// <summary>
        /// Clears the selection of all currently selected cells in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method does not clear the selection if <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/> is set as <see cref= "SelectionMode.Row"/>.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ClearCell" @onclick="ClearCell">Clear</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearCell()
        ///    {
        ///       await grid.ClearCellSelectionAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearCellSelectionAsync()
            => await (SelectionModule?.ClearCellSelection())!.ConfigureAwait(true)!; 
	
        /// <summary>
        /// Clears all the columns filtering and refreshes the Grid asynchronously.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous clear operation.</returns>
        /// <remarks>
        /// This method clears the filtering and refreshes the Grid to show the rows.
	/// This method will clears filtering for all the columns.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ClearRecord" @onclick="ClearFilter">Clear</button>
        /// <SfGrid @ref="grid" DataSource="@Orders" AllowFiltering="true">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearFilter()
        ///    {
        ///       await grid.ClearFilteringAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearFilteringAsync()
        {
            Columns!.ToList().ForEach(column => column.FilterClearIcon = string.Empty);
            if (FilterModule != null)
            {
                await FilterModule.ClearFiltering().ConfigureAwait(true);
            }
        }
	    
        /// <summary>
        /// Clears the filtering for the specificed columns and refreshes the Grid asynchronously.
        /// </summary>
        /// <param name="fieldNames">A list of columns to be cleared, identified by their <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method clears the filtering for the specified columns and refreshes the Grid.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ClearRecord" @onclick="ClearFilter">Clear</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    List<string> listItems = new List<string>();
        ///    private async Task ClearFilter()
        ///    {
        ///       listItems.Add("OrderID");
        ///       listItems.Add("CustomerID");
        ///       await grid.ClearFilteringAsync(listItems);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearFilteringAsync(List<string> fieldNames)
            => await (FilterModule?.ClearFiltering(fieldNames))!.ConfigureAwait(true)!;

        /// <summary>
        /// Clears specific column filtering of the Grid based on the specified field name.
        /// </summary>
        /// <param name="fieldName">The name of the column by which the filtering should be cleared, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method clears the filtering for the specified column and refreshes the Grid to show all rows.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ClearRecord" @onclick="ClearFilter">Clear</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearFilter()
        ///    {
        ///       string columnField = "OrderID";
        ///       await grid.ClearFilteringAsync(columnField);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearFilteringAsync(string fieldName) =>
             await (FilterModule?.ClearFiltering(fieldName))!.ConfigureAwait(true)!;
	
        /// <summary>
        /// Clears all the grouped columns of the Grid, returning it to its original un-grouped state.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ClearGrouping" @onclick="ClearGrouping">ClearGrouping</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearGrouping()
        ///    {
        ///       await grid.ClearGroupingAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearGroupingAsync()
        {
            var GCols = GroupSettings!.Columns?.ToList().Clone() ?? new List<string>();
            for (var i = 0; i < GCols.Count; i++)
            {
                if (GroupModule != null)
                {
                    await GroupModule.UnGroupColumn(GCols[i], true).ConfigureAwait(true);
                }
            }
        }
	
        /// <summary>
        /// Clears all the currently selected rows in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
	/// <remarks>
	/// Its removes selection from all the pages too if checkbox selection is enabled.
	/// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ClearSelection" @onclick="ClearSelection">ClearSelection</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearSelection()
        ///    {
        ///       await grid.ClearRowSelectionAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearRowSelectionAsync()
        {
            if (SelectionModule != null)
            {
                await SelectionModule.ClearRowSelection().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Clears all the selected rows and cells in the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method first checks the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/>. If the Grid is in <see cref="Syncfusion.Blazor.Grids.SelectionMode.Row"/>, it calls the <see cref="ClearRowSelectionAsync()"/> otherwise, it calls the <see cref="ClearCellSelectionAsync()"/> to clear the selection.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ClearSelection" @onclick="ClearSelection">ClearSelection</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearSelection()
        ///    {
        ///       await grid.ClearSelectionAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearSelectionAsync()
        {
            if (SelectionModule != null)
            {
                await SelectionModule.ClearSelection().ConfigureAwait(true);
            }
        }
	
        /// <summary>
        /// Clears all the sorted columns of the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is used to clear all the sorted columns in the Grid. 
        /// The sorting of grouped columns is not cleared.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ClearSorting" @onclick="ClearSorting">Clear Sorting</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearSorting()
        ///    {
        ///      await grid.ClearSortingAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearSortingAsync()
        {
            if (SortModule != null)
            {
                await SortModule.ClearSortAsync().ConfigureAwait(true);
            }
        }
	
        /// <summary>
        /// Clears the sorted columns of the Grid based on the specified field names.
        /// </summary>
        /// <param name="fieldNames">The list of sorted columns to be cleared, identified by their <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method removes the specified columns from the sorted columns of the grid.
        /// If a column is not in the list of sorted columns, it is ignored.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ClearSorting" @onclick="ClearSorting">Clear Sorting</button>
        /// <SfGrid @ref="grid" AllowSorting="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    List<string> listItems = new List<string>();
        ///    private async Task ClearSorting()
        ///    {
        ///       listItems.Add("OrderID");
        ///       listItems.Add("CustomerID");
        ///       await grid.ClearSortingAsync(listItems);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ClearSortingAsync(List<string> fieldNames)
        {
            if (SortModule != null)
            {
                await SortModule.ClearSortAsync(fieldNames).ConfigureAwait(true);
            }
        }
	
        /// <summary>
        /// Cancels the edited state of the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method cancels the current edit state of the grid and closes the <see cref="EditMode"/>. 
        /// If <c>EditMode.Batch</c> is used and the user has made <see cref="Syncfusion.Blazor.Grids.BatchChanges{T}"/> to the data, it will display a confirmation dialog before saving the changes.
        /// Any unsaved changes will be discarded.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ClearEdit" @onclick="ClearEdit">Clear Edit State</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowEditing="true"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ClearEdit()
        ///    {
        ///       await grid.CloseEditAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task CloseEditAsync()
        {
            await EditModule!.CloseEdit().ConfigureAwait(true);
        }

        /// <summary>
        /// Copies the selected rows or cells data into the clipboard.
        /// </summary>
        /// <param name="withHeader">A nullable Boolean value that determines whether to copy the data along with the column header names. The default value is <c>null</c>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is used to copy the selected data in the grid to the clipboard. The data can either include the column header names or not, depending on the value of the <c>withHeader</c> parameter.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="copy" @onclick="Copy">Copy</button>
        /// <button id="copyWithHeader" @onclick="CopyWithHeader">Copy With Header</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task Copy()
        ///    {
        ///         await grid.CopyAsync();  // Copies the selected rows or cells
        ///    }
        ///    private async Task CopyWithHeader()
        ///    {
        ///         await grid.CopyAsync(true); // Copies the selected rows or cells with header
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task CopyAsync(Nullable<bool> withHeader = null)
        {
            await InvokeMethod("sfBlazor.Grid.copyToClipBoard", new object[] { DataId, withHeader! }).ConfigureAwait(true);
        }

        /// <summary>
        /// Pastes the current clipboard content into the selected cells of the grid.
        /// </summary>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous paste operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method retrieves the current clipboard content and inserts it into the selected
        /// grid cells. The operation supports both single-value and tabular clipboard data.
        /// </para>
        /// <para>
        /// When a single value is copied and multiple cells are selected, the value is automatically
        /// replicated across all selected cells (Smart Paste). When tabular data is copied from a
        /// spreadsheet or similar application, the content is pasted while preserving its original
        /// row and column structure (Matrix Paste).
        /// </para>
        /// <para>
        /// To perform a paste operation, the grid must meet the following requirements:
        /// </para>
        /// <list type="bullet">
        /// <item>
        /// <description>Cell selection must be enabled.</description>
        /// </item>
        /// <item>
        /// <description>
        /// The cell selection mode must be configured as <c>Box</c> or <c>BoxWithBorder</c>.
        /// </description>
        /// </item>
        /// <item>
        /// <description>The target columns must be editable.</description>
        /// </item>
        /// <item>
        /// <description>
        /// Either batch editing or cell editing must be enabled.
        /// </description>
        /// </item>
        /// </list>
        /// <para>
        /// Clipboard paste operations can be initiated by using the <c>Ctrl + V</c> keyboard shortcut
        /// or by calling this method programmatically.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button @onclick="Paste">Paste</button>
        ///
        /// <SfGrid @ref="Grid" DataSource="@Orders">
        ///     ...
        /// </SfGrid>
        ///
        /// @code {
        ///     private SfGrid<Order> Grid;
        ///
        ///     private async Task Paste()
        ///     {
        ///         await Grid.PasteAsync();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task PasteAsync()
        {
            await InvokeMethod<bool>("sfBlazor.Grid.pasteFromClipboard", false, new object[] { DataId }).ConfigureAwait(true);
        }


        /// <summary>
        /// Exports the grid data to a CSV file.
        /// </summary>
        /// <param name="excelExportProperties">An object of type <see cref="ExcelExportProperties"/>, provides properties to customize the column, data source, theme, etc. for the exported CSV file. If not provided, default values will be used.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <c>AllowExcelExport</c> property must be <c>true</c> to use this feature.
        /// <c>excelExportProperties</c> object can be used to customize the appearance of the exported CSV file.
        /// If not provided, this method will use default values.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ExportToCsv" @onclick="ExportHandler">ExportToCsv</button>
        /// <SfGrid @ref="grid" AllowExcelExport="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExportHandler()
        ///    {
        ///        Syncfusion.Blazor.Grids.ExcelExportProperties ExportProperties = new Syncfusion.Blazor.Grids.ExcelExportProperties();
        ///        ExportProperties.ExportType = Syncfusion.Blazor.Grids.ExportType.CurrentPage; // here we have changed the ExportType from AllPages to CurrentPage, as like same we can change our desire properties.
        ///        await grid.ExportToCsvAsync(ExportProperties);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExportToCsvAsync(ExcelExportProperties excelExportProperties = null!)
        {
            await ExportToCsvAsync(asMemoryStream: false, excelExportProperties).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the Grid CSV file as a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="asMemoryStream">Specifies whether to return the CSV as a memory stream.</param>
        /// <param name="excelExportProperties">Optional. Provides the Excel export properties such as custom columns, data sources, themes, etc.</param>
        /// <returns>
        /// An asynchronous task that provides a <see cref="MemoryStream"/> containing the exported CSV file when <paramref name="asMemoryStream"/> parameter is true;
        /// otherwise, it returns null and exports the CSV file in the browser.
        /// </returns>
        /// <remarks>
        /// This method will only export the csv file if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowExcelExport"/> is set to <c>true</c>.
        /// It exports the Grid sheet to a CSV file (.csv) in the browser by defining <paramref name="asMemoryStream"/> parameter value to false.
        ///</remarks>
        /// Also, see <seealso cref="Syncfusion.Blazor.Grids.ExcelExportProperties"/> for details on configuring export properties.
        /// <example>
        /// <code><![CDATA[
        /// <button id="ExportToExcel<"@onclick="ExportHandler">ExportToCsv<</button>
        /// <SfGrid @ref="grid" AllowExcelExport="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExportHandler()
        ///    {
        ///        Syncfusion.Blazor.Grids.ExcelExportProperties ExportProperties = new Syncfusion.Blazor.Grids.ExcelExportProperties();
        ///        ExportProperties.ExportType = Syncfusion.Blazor.Grids.ExportType.CurrentPage; // here we have changed the ExportType from AllPages to CurrentPage, as like same we can change our desire properties.
        ///        MemoryStream streamDocument = await grid.ExportToCsvAsync(true, ExportProperties);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<MemoryStream> ExportToCsvAsync(bool asMemoryStream, ExcelExportProperties excelExportProperties = null!)
        {
            // Track telemetry for CSV export
            GridTelemetryHelper.LogTelemetry(true, "Exporting");
            
            using GridExcelExport<TValue> GridExcelExport = new GridExcelExport<TValue>();
            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RequestType = "CsvExport";
            }
            return await GridExcelExport.CsvExport(this, excelExportProperties, isMemoryStreamExport: asMemoryStream).ConfigureAwait(true);
        }
	
        /// <summary>
        /// Deletes the currently selected record from the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only delete a record if <see cref="GridEditSettings.AllowDeleting"/> is set to <c>true</c>.
        /// If no records are selected, it will display an alert message with key <c>DeleteAlert</c>.
        /// If <see cref="GridEditSettings.ShowDeleteConfirmDialog"/> is set to <c>true</c>, it will display a confirm dialog before deleting the record with key <c>DeleteConfirmAlert</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="DeleteRecords" @onclick="DeleteRecords">DeleteRecords</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowDeleting="true"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task DeleteRecords()
        ///    {
        ///       await grid.DeleteRecordAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task DeleteRecordAsync()
        {
            if (!EditSettings!.AllowDeleting)
            {
                return;
            }

            if (!EditModule!.ValidateDeleteOperation())
            {
                return;
            }

            await EditModule!.DeleteRecord().ConfigureAwait(true);
        }

        /// <summary>
        /// Deletes a record in the grid by providing a column name and data.
        /// </summary>
        /// <param name="fieldName">The primary key column name of the record to be deleted, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <param name="data">The data of the record to be deleted.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only delete a record if <see cref="GridEditSettings.AllowDeleting"/> is set to <c>true</c>.
        /// If <c>fieldName</c> and <c>data</c> is not provided and no records are selected, then it will display an alert message with key <c>DeleteAlert</c> while calling this method.
        /// If <see cref="GridEditSettings.ShowDeleteConfirmDialog"/> is set to <c>true</c>, it will display a confirm dialog before deleting the record with key <c>DeleteConfirmAlert</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="DeleteRecords" @onclick="DeleteRecords">DeleteRecords</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowDeleting="true"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///     var data = new Order() { OrderID = 1006, CustomerID = "ALFKI", OrderDate = new DateTime(1995, 05, 15), Freight = 25.7 * 2 };
        ///    private async Task DeleteRecords()
        ///    {
        ///       await grid.DeleteRecordAsync("OrderID", data);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task DeleteRecordAsync(string fieldName, TValue data)
        {
            if (EditSettings != null && !EditSettings.AllowDeleting)
            {
                return;
            }

            if (!EditModule!.ValidateDeleteOperation(data!))
            {
                return;
            }

            await EditModule!.DeleteRecord(fieldName, data!).ConfigureAwait(true);
        }

        /// <summary>
        /// Deletes a record in the grid by specifying one or more primary key field names and the corresponding record data.
        /// </summary>
        /// <param name="primaryKeyFields">
        /// A read-only collection of primary key column names, identified by their
        /// <see cref="GridColumn.Field"/> properties, that uniquely identify the record to be deleted.
        /// </param>
        /// <param name="data">
        /// The data object representing the record to be deleted.
        /// </param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method will delete a record only when <see cref="GridEditSettings.AllowDeleting"/>
        /// is set to <see langword="true"/>.
        ///
        /// <para>
        /// The <paramref name="primaryKeyFields"/> parameter is useful when the grid is configured
        /// with multiple primary key columns (composite keys). The specified key fields, together
        /// with the values available in <paramref name="data"/>, are used to identify the target record.
        /// </para>
        ///
        /// <para>
        /// If neither <paramref name="primaryKeyFields"/> nor <paramref name="data"/> is provided
        /// and no records are selected, an alert with the key <c>DeleteAlert</c> is displayed.
        /// </para>
        ///
        /// <para>
        /// If <see cref="GridEditSettings.ShowDeleteConfirmDialog"/> is set to
        /// <see langword="true"/>, a confirmation dialog with the key
        /// <c>DeleteConfirmAlert</c> is displayed before deleting the record.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="DeleteRecords" @onclick="DeleteRecords">DeleteRecords</button>
        ///
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///     <GridEditSettings AllowDeleting="true"></GridEditSettings>
        ///     .......
        /// </SfGrid>
        ///
        /// @code {
        ///     SfGrid<Order> grid;
        ///
        ///     private async Task DeleteRecords()
        ///     {
        ///         var data = new Order()
        ///         {
        ///             OrderID = 1006,
        ///             ProductID = 1,
        ///             CustomerID = "ALFKI"
        ///         };
        ///
        ///         await grid.DeleteRecordAsync(
        ///             new List<string> { "OrderID", "ProductID" },
        ///             data);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public async Task DeleteRecordAsync(IReadOnlyList<string> primaryKeyFields, TValue data)
        {
            if (primaryKeyFields == null || !primaryKeyFields.Any())
            {
                return;
            }

            if (EditSettings != null && !EditSettings.AllowDeleting)
            {
                return;
            }
            if (!EditModule!.ValidateDeleteOperation(data!))
            {
                return;
            }

            await EditModule!.DeleteRecord(primaryKeyFields, data!).ConfigureAwait(true);
        }

        /// <summary>
        /// Delete any visible row by TR element.
        /// </summary>
        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task DeleteRow(object tr)
        {
            await InvokeMethod("deleteRow", null!, tr).ConfigureAwait(true); // old
        }

        internal Dictionary<string, bool> DynamicEnableDisableItems = new Dictionary<string, bool>();
        /// <summary>
        /// Change a particular cell into an edited state by providing the row index and field name in <see cref="EditMode.Batch"/> or <see cref="EditMode.Cell"/> mode.
        /// </summary>
        /// <param name="index">The index of the row to be edited.</param>
        /// <param name="fieldName">The column name of the cell to be edited, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method edits the cell if the <see cref="GridEditSettings.Mode"/> is set to <see cref="EditMode.Batch"/> or <see cref="EditMode.Cell"/> and <see cref="GridEditSettings.AllowEditing"/> is set to <c>true</c>.
        /// It will search for the row and cell based on the provided <c>index</c> and <c>fieldName</c>, and call <c>EditModule.EditCell</c> to edit the cell.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="editCell" @onclick="EditCell">EditCell</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task EditCell()
        ///    {
        ///      await grid.EditCellAsync(3, "CustomerID");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task EditCellAsync(int index, string fieldName)
        {
            await EditModule!.EditCellByIndexAndField(index, fieldName).ConfigureAwait(true);
        }

        /// <summary>
        /// Enables or disables toolbar items by identified their IDs.
        /// </summary>
        /// <param name="items">A list of strings containing the IDs of toolbar items to enable or disable.</param>
        /// <param name="isEnable">Specifies whether to enable (true) or disable (false) the toolbar items.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method enables or disables the specified toolbar items by adding or updating their key-value pairs.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="EnableToolbarItems" @onclick="ToolbarHandler">EnableToolbarItems</button>
        /// <SfGrid @ref="grid" DataSource="@Orders" Toolbar="@(new List<string>() { "Add", "Edit", "Delete", "Update", "Cancel" })">
        /// <GridEditSettings AllowEditing="true" AllowAdding="true" AllowDeleting="true"/>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///	   private async Task ToolbarHandler()
        ///	   {
        ///		await grid.EnableToolbarItemsAsync(new List<string>() { "Add" , "Edit"}, false); // here we disabled the Add and Edit toolbar items.
        ///	   }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task EnableToolbarItemsAsync(List<string> items, bool isEnable)
        {
            if (items != null)
            {
                foreach (var a in items)
                {
                    if (!DynamicEnableDisableItems.TryAdd(a, isEnable))
                    {
                        DynamicEnableDisableItems[a] = isEnable;
                    }
                }
            }

            
        }
	
        /// <summary>
        /// Saves the modified values when the row or cell is in editing state.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// To close the edit state without saving changes, use the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.CloseEditAsync"/> method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="EndEdit" @onclick="EditHandler">EndEdit</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowEditing="true"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task EditHandler()
        ///    {
        ///      await grid.EndEditAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task EndEditAsync()
        {
            //await EditSettings.Dialog.CloseOnEscape
            await EditModule!.EndEdit().ConfigureAwait(true);
        }

        /// <summary>
        /// Exports Grid data to an Excel file(.xlsx).
        /// </summary>
        /// <param name="excelExportProperties">Provides the excel export properties such as custom columns, data sources, themes, etc.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method exports the data from the current Grid instance to an Excel file. The exported file will be in the .xlsx format.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ExportToExcel<"@onclick="ExportHandler">ExportToExcel<</button>
        /// <SfGrid @ref="grid" AllowExcelExport="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExportHandler()
        ///    {
        ///        Syncfusion.Blazor.Grids.ExcelExportProperties ExportProperties = new Syncfusion.Blazor.Grids.ExcelExportProperties();
        ///        ExportProperties.ExportType = Syncfusion.Blazor.Grids.ExportType.CurrentPage; // here we have changed the ExportType from AllPages to CurrentPage, as like same we can change our desire properties.
        ///        await grid.ExportToExcel(ExportProperties);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExportToExcelAsync(ExcelExportProperties excelExportProperties = null!)
        {
            await ExportToExcelAsync(asMemoryStream: false, excelExportProperties).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the Grid Excel sheet as a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="asMemoryStream">Specifies whether to return the Excel worksheet as a memory stream.</param>
        /// <param name="excelExportProperties">Optional. Provides the excel export properties such as custom columns, data sources, themes, etc.</param>
        /// <returns>
        /// An asynchronous task that provides a <see cref="MemoryStream"/> containing the exported Excel data when <c>asMemoryStream</c> parameter is true;
        /// otherwise, it returns null and exports the Excel sheet in browser.
        /// </returns>
        ///<remarks>
        /// It exports the Grid sheet to an Excel file (.xlsx) in the browser by defining <c>asMemoryStream</c> parameter value to false.
        ///</remarks>
        /// Also, see <seealso cref="Syncfusion.Blazor.Grids.ExcelExportProperties"/> for details on configuring export properties.
        /// <example>
        /// <code><![CDATA[
        /// <button id="ExportToExcel<"@onclick="ExportHandler">ExportToExcel<</button>
        /// <SfGrid @ref="grid" AllowExcelExport="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExportHandler()
        ///    {
        ///        Syncfusion.Blazor.Grids.ExcelExportProperties ExportProperties = new Syncfusion.Blazor.Grids.ExcelExportProperties();
        ///        ExportProperties.ExportType = Syncfusion.Blazor.Grids.ExportType.CurrentPage; // here we have changed the ExportType from AllPages to CurrentPage, as like same we can change our desire properties.
        ///        MemoryStream streamDocument = await grid.ExportToExcel(true, ExportProperties);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<MemoryStream> ExportToExcelAsync(bool asMemoryStream, ExcelExportProperties excelExportProperties = null!)
        {
            // Track telemetry for Excel export
            GridTelemetryHelper.LogTelemetry(true, "Exporting");
            
            using GridExcelExport<TValue> GridExcelExport = new GridExcelExport<TValue>();
            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RequestType = "ExcelExport";
            }
            return await GridExcelExport.ExcelExport(this, excelExportProperties, isMemoryStreamExport: asMemoryStream).ConfigureAwait(true);
        }
	
        /// <summary>
        /// Filters the grid row by a specified column with the given options.
        /// </summary>
        /// <param name="fieldName">The name of the column to be filtered, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <param name="filterOperator">The operator to apply to the filter, refer to the operator list in <see cref="Syncfusion.Blazor.Operator"/>.</param>
        /// <param name="filterValue">The value to use for filtering.</param>
        /// <param name="predicate">The predicate is used to generate the filter query to meet the multiple filtering requests. This parameter is optional.</param>
        /// <param name="matchCase">A Boolean value that indicates whether the filter should be case-sensitive. This parameter is optional.</param>
        /// <param name="ignoreAccent">A Boolean value that indicates whether the filter should ignore accents when comparing values. This parameter is optional.</param>
        /// <param name="actualFilterValue">Specifies the actual filter value as defined in the corresponding data type. This parameter is optional.</param>
        /// <param name="actualOperator">Specifies the actual operator to apply to the filter, as defined in the corresponding data type. This parameter is optional.</param>
        /// <param name="columnUid">Selects the appropriate column when multiple foreign key columns have the same <c>fieldName</c>. This parameter is optional.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous filtering operation.</returns>
        /// <example>
        /// <code><![CDATA[
        /// <button id="FilterByColumn" @onclick="FilterHandler">FilterByColumn</button>
        /// <SfGrid @ref="grid" AllowFiltering="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///      SfGrid<Order> grid;
        ///      private async Task FilterHandler()
        ///      {
        ///		await grid.FilterByColumnAsync("CustomerID", "equal", "ANANTR");
        ///      }
        /// ]]>
        /// </code>
        /// </example>
        public async Task FilterByColumnAsync(string fieldName, string filterOperator, object filterValue, string predicate = null!, Nullable<bool> matchCase = null, Nullable<bool> ignoreAccent = null, object actualFilterValue = null!, object actualOperator = null!, string columnUid = null!)
        {
            Operator fOperator = Filter<TValue>.GetOperator(filterOperator?.ToLower(CultureInfo.CurrentCulture)!);
            await (FilterModule?.FilterByColumn(fieldName, fOperator, filterValue, predicate, matchCase, ignoreAccent, actualFilterValue, actualOperator, columnUid))!.ConfigureAwait(true)!;
        }
	
        /// <summary>
        /// Gets the added, edited, and deleted data before bulk save to the data Source in <see cref="EditMode.Batch"/> mode.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task{BatchChanges}"/> representing the asynchronous operation.</returns>
        /// <remarks>This method should be used when making bulk changes to the data source in <c>EditMode.Batch</c>
        /// to allow for review and modification of the changes before they are committed to the data source.
	/// The edited and new added records are highlighted in grid UI. It will cleared once we saved the updated changes in the grid.
	///</remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetData" @onclick="GetData">Get Data</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  <GridEditSettings AllowAdding="true" AllowEditing="true" AllowDeleting="true" Mode="EditMode.Batch"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetData()
        ///    {
        ///      var Data = await grid.GetBatchChangesAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<BatchChanges<TValue>> GetBatchChangesAsync()
        {
            return await Task.FromResult<BatchChanges<TValue>>(EditModule!.GetBatchChanges()).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets a Column details based on the specific column field name.
        /// </summary>
        /// <param name="fieldName">The field name of the column to retrieve, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{GridColumn}"/> representing the asynchronous operation that returns the <see cref="GridColumn"/> with the specified <c>fieldName</c>.</returns>
        /// <remarks>
        /// This method searches through all columns in the grid and returns the first column whose field name matches the specified <c>fieldName</c> parameter.
        /// If no column is found with the specified <c>fieldName</c>, this method returns <c>null</c>. Case-insensitive search is used while comparing the field name.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="getColumns" @onclick="GetColumn">Get Column</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    List<string> listItems = new List<string>();
        ///    private async Task GetColumn()
        ///    {
        ///         var Column = await grid.GetColumnByFieldAsync("CustomerID");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<GridColumn> GetColumnByFieldAsync(string fieldName)
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<GridColumn>(columns.Where(x => fieldName != null && fieldName.Equals(x.Field, StringComparison.Ordinal)).FirstOrDefault()!).ConfigureAwait(true);
        }
	
        /// <summary>
        /// Gets a column by its unique identifier (UID) value.
        /// </summary>
        /// <param name="uid">The unique identifier of the column to retrieve the corresponding column details.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{GridColumn}"/> representing the asynchronous operation that yields the <see cref="GridColumn"/> with the specified <c>uid</c>, or <c>null</c> if no such column exists.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="getColumns" @onclick="GetColumn">Get Column</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumn()
        ///    {
        ///         var Column = await grid.GetColumnByUidAsync("grid-column18");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<GridColumn> GetColumnByUidAsync(string uid)
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<GridColumn>(columns.Where(x => uid.Equals(x.Uid, StringComparison.Ordinal)).FirstOrDefault()!).ConfigureAwait(true);
        }     
	
        /// <summary>
        /// Gets the collection of column field names which are bound in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that returns a list of strings representing the names of the column fields.</returns>
        /// <remarks>
        /// This method use the <see cref="GridUtils.GetColumns"/> method to retrieve a list of column objects, and then extract the <see cref="GridColumn.Field"/> property of each column object and return it as a list of strings.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="getColumns" @onclick="GetColumns">GetColumnFieldNames</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumns()
        ///    {
        ///         var columns = await grid.GetColumnFieldNamesAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<string>> GetColumnFieldNamesAsync()
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<List<string>>(columns.Select(x => x.Field).ToList()).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the index of a column by its field name.
        /// </summary>
        /// <param name="fieldName">A string value representing the field name of the column whose index is to be returned, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{Integer}"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method retrieves the list of columns from the grid using <see cref="GridUtils.GetColumns"/>,
        /// and searches for the first column whose name matches the <c>GridColumn.Field</c> property of the grid and return its index.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetColumnIndex" @onclick="GetColumnIndex">Get ColumnIndex</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumnIndex()
        ///    {
        ///         var ColumnIndex = await grid.GetColumnIndexByFieldAsync("CustomerID");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<int> GetColumnIndexByFieldAsync(string fieldName)
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<int>(columns.Where(x => fieldName.Equals(x.Field, StringComparison.Ordinal)).FirstOrDefault()!.Index).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the index of a column by its unique identifier (UID).
        /// </summary>
        /// <param name="uid">The unique identifier (UID) of the column whose index is to be returned.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{Integer}"/> representing the asynchronous operation that returns the index of the column with the specified UID.</returns>
        /// <remarks>
        /// The method searches for the column by comparing the provided UID to the <see cref="GridColumn.Uid"/> property of each column in the grid using an ordinal comparison.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetColumnIndex" @onclick="GetColumnIndex">Get ColumnIndex</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumnIndex()
        ///    {
        ///         var ColumnIndex = await grid.GetColumnIndexByUidAsync("grid-column18");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<int> GetColumnIndexByUidAsync(string uid)
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<int>(columns.Where(x => uid.Equals(x.Uid, StringComparison.Ordinal)).FirstOrDefault()!.Index).ConfigureAwait(true);
        }
	
        /// <summary>
        /// Gets the list of columns details which are bound in the Grid.
        /// </summary>
        /// <param name="isRefresh">An optional boolean value indicating whether to refresh the grid columns.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task{GridColumn}"/> representing the asynchronous operation that returns the <see cref="GridColumn"/>.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="getColumns" @onclick="GetColumns">Get Columns</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumns()
        ///    {
        ///         var Columns = await grid.GetColumnsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<GridColumn>> GetColumnsAsync(Nullable<bool> isRefresh = null)
        {
            isRefresh = null;
            return await Task.FromResult<List<GridColumn>>(GridUtils.GetColumns(this)).ConfigureAwait(true);
        }
	
        /// <summary>
        /// Gets the records which are currently visible in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetCurrentViewRecords" @onclick="GetDataHandler">Get CurrentViewRecords</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var records = await grid.GetCurrentViewRecordsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<TValue>> GetCurrentViewRecordsAsync()
        {
            return await Task.FromResult<List<TValue>>(Rows?.Where(_ => _.IsDataRow && !_.IsDetailRow)?.Select(x => (TValue)x.Data!)?.ToList()!).ConfigureAwait(true);
        }

        /// <summary>
        /// Get the current Filter operator and field.
        /// </summary>
        /// <returns>System.Threading.Tasks.Task.</returns>
        /// <exclude/>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public async Task<FilterUI> GetFilterUIInfo()
        {
            return await Task.FromResult<FilterUI>(null!).ConfigureAwait(true); // old
        }

        /// <summary>
        /// Get the filtered records details of the DataGrid.
        /// </summary>
        /// <param name="isStrictFiltering">
        /// A boolean value indicating whether strict filtering should be applied. 
        /// By default, its value is <c>false</c>. When set to <c>true</c>, the method ensures that at least one column is filtered before returning records.
        /// When <c>false</c>, it returns all available records without enforcing filtering conditions.
        /// </param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that returns an array of objects for the local dataSource. If the Grid has remote data, it returns a promise object.</returns>
        /// <example>
        /// <remarks>
        /// Any one of the column must be filtered to get the filtered record details while using this method.
        /// If <paramref name="isStrictFiltering"/> is enabled and no columns are filtered, an empty collection is returned.
        /// </remarks>
        /// <code>
        /// <![CDATA[
        /// <button id="GetFilteredRecords" @onclick="FilterHandler ">Get Filtered Records</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task FilterHandler()
        ///    {
        ///         var data = await grid.GetFilteredRecordsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<object> GetFilteredRecordsAsync(bool isStrictFiltering = false)
        {
            if (!isStrictFiltering || (isStrictFiltering && (AllowFiltering && FilterSettings!.Columns?.Count > 0 || SearchSettings!.Key.Length > 0)))
            {
                object result = await DataManager!.ExecuteQuery<TValue>(DataModule?.GenerateQuery(true)!).ConfigureAwait(true);
                return result is DataResult ? (IEnumerable<object>)((DataResult)result).Result! : result;
            }
            else
            {
                return Enumerable.Empty<object>().Cast<TValue>().ToList();
            }
        }

        /// <summary>
        /// Gets the foreign columns detail from the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that returns a list of <see cref="GridColumn"/> objects.</returns>
        /// <remarks>
        /// A foreign key column must be specified using the <c>ForeignKeyValue</c> property and <c>ForeignKeyField</c> property.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetForeignKeyColumns" @onclick="GetColumnHandler ">Get ForeignKeyColumns</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumnHandler()
        ///    {
        ///         var Columns = await grid.GetForeignKeyColumnsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<GridColumn>> GetForeignKeyColumnsAsync()
        {
            return await ForeignKeyModule!.GetForeignKeyColumnsAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the hidden columns from the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that returns a list of <see cref="GridColumn"/> objects.</returns>
        /// <remarks>
        /// This method retrieves all the columns from the Grid using the <see cref="GridUtils.GetColumns"/> and filters the list that are not visible using the <see cref="GridColumn.Visible"/> property.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetHiddenColumns" @onclick="GetColumnHandler ">Get HiddenColumns</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetColumnHandler()
        ///    {
        ///         var Columns = await grid.GetHiddenColumnsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<GridColumn>> GetHiddenColumnsAsync()
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<List<GridColumn>>(columns.Where(x => !x.Visible).ToList()).ConfigureAwait(true);
        }

        /// <summary>
        /// Get the grid properties which are maintained in the persisted state.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// Returns the state of the grid as string value which can be saved and loaded into grid later using
        /// <c>SetPersistData</c> method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetPersistData" @onclick="GetPersistData">Get PersistData</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetPersistData()
        ///    {
        ///         var data = await grid.GetPersistDataAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<string> GetPersistDataAsync()
        {
            return await Task.FromResult<string>(SerializeModel(this)).ConfigureAwait(true);
        }

        /// <summary>
        /// Get the names of the primary key columns of the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that returns a list of strings representing the names of the primary key columns.</returns>
        /// <remarks>
        /// This method retrieves all the columns from the Grid using the <see cref="GridUtils.GetColumns"/> and filters the list of columns based on the <see cref="GridColumn.IsPrimaryKey"/> property. 
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetPrimaryKeyFieldNames" @onclick="GetDataHandler">Get PrimaryKeyFieldNames</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var fieldNames = await grid.GetPrimaryKeyFieldNamesAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<string>> GetPrimaryKeyFieldNamesAsync()
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<List<string>>(columns.Where(x => x.IsPrimaryKey)
                .Select(x => x.Field)
                .ToList()).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the index of a row in the grid using the specified primary key value. By default, the index is retrieved only from the records in the current view.
        /// To search across the entire dataset, set <paramref name="searchAcrossRecords"/> to <c>true</c>.
        /// </summary>
        /// <param name="value">The value of the primary key column for the row whose index is to be found. </param>
        /// <param name="searchAcrossRecords">A boolean value indicating whether to search across all pages (<c>true</c>) or only within the current page (<c>false</c>). </param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation, with the result containing the index of the row, or -1 if the row cannot be found. </returns>
        /// <remarks>A primary key column must be defined using the <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> property.
        /// When <paramref name="searchAcrossRecords"/> is set to <c>true</c>, a request is sent to the server to fetch the entire dataset, and the index is then calculated from that complete data, regardless of the records currently loaded in the grid.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetRowIndexByPrimaryKey" @onclick="GetDataHandler">Get Row Index</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///     ...
        /// </SfGrid>
        ///
        /// @code {
        ///     SfGrid<Order> grid;
        ///
        ///     private async Task GetDataHandler()
        ///     {
        ///         var rowIndex = await grid.GetRowIndexByPrimaryKeyAsync(15, searchAcrossRecords: true);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<int> GetRowIndexByPrimaryKeyAsync(object value, bool searchAcrossRecords = false)
        {
            var pKey = await GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            if (value == null || pKey == null || pKey.Count == 0)
            {
                return -1;
            }
            var singleKeyDict = new Dictionary<string, object> { { pKey[0]!, value } };
            return await GetRowIndexByPrimaryKeyAsync(singleKeyDict, searchAcrossRecords).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the index of a row in the grid using the specified primary key value or values.
        /// By default, the index is retrieved only from the records in the current view.
        /// To search across the entire dataset, set <paramref name="searchAcrossRecords"/> to <c>true</c>.
        /// </summary>
        /// <param name="primaryKeyValues">
        /// A read-only dictionary that maps primary key column field names to their corresponding values.
        /// For grids with a single primary key column, provide a dictionary containing one key-value pair.
        /// For grids with composite primary keys, provide all primary key field names and their corresponding values.
        /// Field names are matched against the grid's primary key columns, and the order of entries in the dictionary is not significant.
        /// </param>
        /// <param name="searchAcrossRecords">
        /// A boolean value indicating whether to search across all records in the data source (<c>true</c>)
        /// or only within the records currently available in the grid view (<c>false</c>).
        /// </param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation, with the result
        /// containing the zero-based index of the matching row, or <c>-1</c> if no matching row is found.
        /// </returns>
        /// <remarks>
        /// A primary key column must be defined using the <see cref="Syncfusion.Blazor.Grids.GridColumn.IsPrimaryKey"/> property.
        /// <para>
        /// For grids with composite primary keys, all primary key field names and values must be provided in
        /// <paramref name="primaryKeyValues"/> to uniquely identify the row.
        /// </para>
        /// <para>
        /// When <paramref name="searchAcrossRecords"/> is set to <c>true</c>, a request is sent to the server
        /// to retrieve the complete dataset. The row index is then calculated from the full set of records,
        /// regardless of the records currently loaded in the grid.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetRowIndexByPrimaryKey" @onclick="GetDataHandler">Get Row Index</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///     ...
        /// </SfGrid>
        ///
        /// @code {
        ///     SfGrid<Order> grid;
        ///
        ///     private async Task GetDataHandler()
        ///     {
        ///         var rowIndex = await grid.GetRowIndexByPrimaryKeyAsync(
        ///             new Dictionary<string, object>
        ///             {
        ///                 { "OrderID", 15 }
        ///             },
        ///             searchAcrossRecords: true);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<int> GetRowIndexByPrimaryKeyAsync(IReadOnlyDictionary<string, object>? primaryKeyValues,bool searchAcrossRecords = false)
        {
            if (Rows.Count == 0 || primaryKeyValues == null || primaryKeyValues.Count == 0)
            {
                return -1;
            }
            var pKey = await GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            if (pKey == null || pKey.Count == 0)
            {
                return -1;
            }
            var values = BuildPositionalKeyValues(primaryKeyValues, pKey);
            if (searchAcrossRecords)
                return await GetPrimaryKeyIndexAcrossPageAsync(values, pKey).ConfigureAwait(true);
            var dataRows = Rows.Where(_ => _.IsDataRow).ToList();
            if (pKey.Count > 1)
            {
                var targetCompositeKey = new CompositeKey(values.ToArray());
                foreach (var dataRow in dataRows)
                {
                    var rowKeyValues = EditModule?.GetPrimaryKeyValueAsObject(dataRow.Data!).GetAwaiter().GetResult();
                    if (rowKeyValues != null && rowKeyValues.Count > 0 &&
                        targetCompositeKey.Equals(new CompositeKey(rowKeyValues.ToArray())))
                    {
                        return await Task.FromResult<int>(Convert.ToInt32(dataRow.Index!.Value)).ConfigureAwait(true);
                    }
                }
                return await Task.FromResult<int>(-1).ConfigureAwait(true);
            }
            else
            {
                var key = pKey[0];
                var row = dataRows.Find(row => !GridUtils.CompareValues(PropHelper?.GetObject(key, row.Data!), values[0]));
                int rowIndex = row != null ? row.Index!.Value : -1;
                return await Task.FromResult<int>(rowIndex).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Gets the collection of selected records.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that result contains a list of the currently selected records.</returns>
        /// <remarks>
	/// While using this method, you can get the collection of record details which are currently selected in the grid.
	/// </remarks>
	/// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="Get SelectedRecords" @onclick="GetDataHandler">GetSelectedRecords</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var selectedRecords  = await grid.GetSelectedRecordsAsync();  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<TValue>> GetSelectedRecordsAsync()
        {
            return await Task.FromResult<List<TValue>>(SelectedRecords).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the collection of cell indexes from the selected row.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that result contains a list of ValueTuple instances representing the selected row and cell indexes.</returns>
        /// <remarks>
        /// If there are no rows or the <see cref="AllowSelection"/> is set to <c>false</c> or selection mode is set to <see cref= "SelectionMode.Row"/>, an empty list is returned.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="Get selectedRowcellIndexes " @onclick="GetDataHandler ">GetSelectedRowcellIndexes </button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var selectedRowcellIndexes = await grid.GetSelectedRowCellIndexesAsync();  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<ValueTuple<int, int>>> GetSelectedRowCellIndexesAsync()
        {
            List<ValueTuple<int, int>> result = new List<ValueTuple<int, int>>();

            if (Rows.Count == 0 || !AllowSelection || (SelectionModule != null && SelectionModule.IsRowMode()))
            {
                return result;
            }

            List<Row<object>> _dataRows = Rows.Where(row => row.IsDataRow).ToList();
            int count = _dataRows.Count;

            for (var i = 0; i < count; i++)
            {
                Row<object> _row = _dataRows[i];
                int rowIndex = -1;
                if (_row.IsSelected)
                {
                    rowIndex = _row.Index!.Value;
                    _row.Cells.ForEach(cell =>
                    {
                        if (cell.IsSelected)
                        {
                            result.Add((rowIndex, cell.Index!.Value));
                        }
                    });
                }
            }

            return await Task.FromResult<List<ValueTuple<int, int>>>(result).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the collection of selected row indexes.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that result contains a list of integer representing the indexes of the selected rows.</returns>
        /// <example>
        /// <remarks>
        /// This method does not retrieve the row indexes if <see cref="GridSelectionSettings.Mode"/> is set to <see cref= "SelectionMode.Cell"/>.
        /// </remarks>
        /// <code>
        /// <![CDATA[
        /// <button id="GetSelectedRowIndexes " @onclick="GetDataHandler">GetSelectedRowIndexes </button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var SelectedRowIndexes = await grid.GetSelectedRowIndexesAsync();  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<int>> GetSelectedRowIndexesAsync()
        {
            return await Task.FromResult<List<int>>(SelectedRowIndexes).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the unique identifier (UID) by its column field name.
        /// </summary>
        /// <param name="fieldName">The field name of the column whose unique identifier (UID) is to be retrieved, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method searches through all columns in the grid and returns the UID of the first column whose field name matches the specified <c>fieldName</c> parameter.
        /// If no column is found, this method returns <c>null</c>. Case-insensitive search is used while comparing the field name.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetUidByColumnField" @onclick="GetDataHandler">GetUidByColumnField</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var Uid = await grid.GetUidByColumnFieldAsync("CustomerID");  //pass column name here
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<string> GetUidByColumnFieldAsync(string fieldName)
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<string>(columns.Where(x => fieldName.Equals(x.Field, StringComparison.Ordinal)).FirstOrDefault()!.Uid).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets a list of all visible columns in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method uses the <see cref="GridUtils.GetColumns"/> method to retrieve all columns in the grid and returns a list of columns where <see cref="GridColumn.Visible"/> property is true.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GetVisibleColumns" @onclick="GetDataHandler">GetVisibleColumns</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GetDataHandler()
        ///    {
        ///         var Columns = await grid.GetVisibleColumnsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<List<GridColumn>> GetVisibleColumnsAsync()
        {
            List<GridColumn> columns = GridUtils.GetColumns(this);
            return await Task.FromResult<List<GridColumn>>(columns.Where(x => x.Visible).ToList()).ConfigureAwait(true);
        }

        /// <summary>
        /// Navigate to the specified target page number asynchronously.
        /// </summary>
        /// <param name="pageNo">Specifies the page number you want to navigate.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowPaging"/> should be set to <c>true</c> in order to use this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GoToPage" @onclick="PagingHandler">GoToPage</button>
        /// <SfGrid @ref="grid" AllowPaging="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task PagingHandler()
        ///    {
        ///         await grid.GoToPageAsync(4); // pass desire page number here.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task GoToPageAsync(int pageNo)
        {
            if (PageModule != null)
            {
                await PageModule.GoToPageAsync(pageNo).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Collapses all the currently expanded grouped rows in the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only Collapse all the grouped rows if <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowGrouping"/> is set to <c>true</c>.
        /// If the <see cref="GridGroupSettings.EnableLazyLoading"/> property is <c>true</c>, this method collapses all groups and refreshes the grid data.
        /// If the <see cref="EnableVirtualization"/> property is <c>true</c>, this method sets the <c>VirtualScrollModule.GeneratedGroupedRows</c> property to an empty list and call the <c>DataProcess</c> method.
        /// If the <see cref="AllowPaging"/> property is <c>true</c>, this method resets the current page to the first page using the <see cref="Syncfusion.Blazor.Grids.GridPageSettings.UpdateProperties(string, object)"/> and call the <c>DataProcess</c> method.
        /// If the <see cref="GridGroupSettings.EnableLazyLoading"/> property is <c>false</c>, this method collapses all groups by setting their visibility to <c>false</c> and setting their <c>IsExpand</c> property to <c>false</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="Collapse" @onclick="CollapseGroup">CollapseGroup</button>
        /// <SfGrid @ref="grid" DataSource="@Orders" AllowGrouping="true">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task CollapseGroup()
        ///    {
        ///      await grid.CollapseAllGroupAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task CollapseAllGroupAsync()
        {
            if (GroupModule != null)
            {
                await GroupModule.CollapseAllGroupsAsync().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Group a column based on the specified field name of the column.
        /// </summary>
        /// <param name="columnName">Specifies the column field name to be grouped.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
	/// <remarks>
	/// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowGrouping"/> must be <c>true</c> to use this method.
	/// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="GroupColumn" @onclick="GroupColumn">GroupColumn</button>
        /// <SfGrid @ref="grid" DataSource="@Orders" AllowGrouping="true">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task GroupColumn()
        ///    {
        ///      await grid.GroupColumnAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task GroupColumnAsync(string columnName)
        {
            if (GroupModule != null)
            {
                await GroupModule.GroupColumn(columnName).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Expand all grouped rows in the Grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only expand grouped rows if <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowGrouping"/> is set to <c>true</c>.
        /// If <c>GroupSettings.EnableLazyLoading</c> is set to <c>true</c>, it will set <c>IsLazyExpandAll</c> to true and call <c>DataProcess</c> method.
        /// Otherwise, it will set <c>IsExpand</c> property to <c>true</c> for all grouped rows and update the grid accordingly.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ExpandAll" @onclick="ExpandAll">ExpandAll</button>
        /// <SfGrid @ref="grid" DataSource="@Orders" AllowGrouping="true">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExpandAll()
        ///    {
        ///      await grid.ExpandAllGroupAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExpandAllGroupAsync()
        {
            if (GroupModule != null)
            {
                await GroupModule.ExpandAllGroupsAsync().ConfigureAwait(true);
            }
        }
        /// <summary>
        /// Hide the spinner which is shown while performing any grid action or grid loading time.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of hiding the spinner element.</returns>
        /// <remarks>
        /// This method checks if <c>_hasSpinner</c> is <c>true</c> and if <c>SpinnerRef</c> is not null before hiding the spinner.
        /// </remarks>
        /// <example>        
        /// <code><![CDATA[
        /// <button id="HideSpinner" @onclick="SpinnerHandler">HideSpinner</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///       .................
        /// </SfGrid>
        /// @code{
        ///      SfGrid<Order> grid;
        ///      private async Task SpinnerHandler()
        ///      {
        ///		await grid.HideSpinnerAsync(); // Hides the spinner.
        ///      }
        /// ]]>
        /// </code>
        /// </example>
        public async Task HideSpinnerAsync()
        {
            if (_hasSpinner && SpinnerRef != null)
            {
                await SpinnerRef.HideAsync().ConfigureAwait(true);
                await Task.CompletedTask.ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Merges a rectangular area of cells in the part of <see cref="SfGrid{TValue}"/> that is currently visible,
        /// starting at the specified top-left cell. The top-left cell shows once with the effective <c>rowspan</c> and/or
        /// <c>colspan</c>, and the other cells in the area are hidden and cannot receive focus.
        /// </summary>
        /// <param name="info">
        /// The merge request that gives the top-left cell (zero-based <see cref="MergeCellInfo.RowIndex"/> and
        /// <see cref="MergeCellInfo.ColumnIndex"/>) and how many rows and columns to include
        /// (<see cref="MergeCellInfo.RowSpan"/> and <see cref="MergeCellInfo.ColumnSpan"/>).
        /// </param>
        /// <remarks>
        /// <para>Behavior:</para>
        /// <list type="bullet">
        ///   <item><description>The top-left cell is the anchor of the merged area. The other cells in the area are not shown and cannot be focused.</description></item>
        ///   <item><description>Selection and keyboard navigation use the anchor cell and respect the combined size.</description></item>
        /// </list>
        /// <para>Scope and rules:</para>
        /// <list type="bullet">
        ///   <item><description>Works only in the current visible view. Results can change after paging, sorting, filtering, grouping changes, column show/hide or reorder, frozen changes, or virtualization updates.</description></item>
        ///   <item><description><see cref="MergeCellInfo.RowIndex"/> refers to data rows only; header, caption, detail, and summary rows are excluded.</description></item>
        ///   <item><description><see cref="MergeCellInfo.ColumnIndex"/> refers to visible leaf data columns only; header-only, command/checkbox/select, and template-only columns cannot be merged.</description></item>
        ///   <item><description>Each merged area must stay inside one frozen section (left, center, or right) and inside the visible virtualization window.</description></item>
        ///   <item><description>Merged areas cannot overlap existing merged areas.</description></item>
        ///   <item><description>If a horizontal and a vertical merge cross each other, their sizes must match so the crossing forms a proper rectangle. Otherwise, the request is rejected.</description></item>
        /// </list>
        /// <para>Performance:</para>
        /// <list type="bullet">
        ///   <item><description>Computed once per view change. For many merges, use the overload that accepts a collection to reduce extra screen updates.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when indices or span sizes are outside valid ranges.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the request breaks the rules, such as crossing view bounds or frozen boundaries, overlapping an existing merge,
        /// going outside the visible virtualization window, targeting non-data rows, targeting non-leaf or hidden columns,
        /// or crossing another span with mismatched size.
        /// </exception>
        /// <example>
        /// The following example merges a 2x3 area starting at row index 1 and column index 1 in the current view.
        /// <code><![CDATA[
        /// grid.MergeCells(new MergeCellInfo
        /// {
        ///     RowIndex = 1,
        ///     ColumnIndex = 1,
        ///     RowSpan = 2,
        ///     ColumnSpan = 3
        /// });
        /// ]]></code>
        /// </example>
        /// <seealso cref="MergeCellInfo"/>
        /// <seealso cref="SfGrid{TValue}.MergeCellsAsync(System.Collections.Generic.IEnumerable{MergeCellInfo})"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeCellsAsync(UnmergeCellInfo)"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeAllAsync()"/>
        public async Task MergeCellsAsync(MergeCellInfo info)
        {
            await MergeModule!.MergeCellsAsync(info).ConfigureAwait(true);
        }

        /// <summary>
        /// Merges several rectangular areas of cells in the part of <see cref="SfGrid{TValue}"/> that is currently shown,
        /// and does all merges together in one combined operation. Each area starts at its own top-left cell.
        /// </summary>
        /// <param name="infos">
        /// The list of merge requests. Each request gives:
        /// <see cref="MergeCellInfo.RowIndex"/> and <see cref="MergeCellInfo.ColumnIndex"/> for the top-left cell (both start at 0),
        /// and <see cref="MergeCellInfo.RowSpan"/> and <see cref="MergeCellInfo.ColumnSpan"/> for how many rows and columns to include.
        /// All requests must be valid and must not overlap with each other or with already merged areas.
        /// </param>
        /// <remarks>
        /// <para>Checks and processing:</para>
        /// <list type="bullet">
        ///   <item><description>All requests are checked first. If any request is invalid, nothing is merged and the whole operation is stopped.</description></item>
        ///     <item><description>Requests that would overlap with each other or with existing merged areas are not allowed.</description></item>
        ///   <item><description>The order of items does not matter. The final layout is the same.</description></item>
        /// </list>
        /// <para>Where and how merges apply (for each request):</para>
        /// <list type="bullet">
        ///   <item><description>Works only on what is currently shown (for example, current page or visible portion). Results can change when the view changes (paging, sorting, filtering, expanding groups, changing column visibility or order, frozen areas, or virtualization updates).</description></item>
        ///   <item><description><see cref="MergeCellInfo.RowIndex"/> points to data rows only. Header, caption, detail, and summary rows are not included.</description></item>
        ///   <item><description><see cref="MergeCellInfo.ColumnIndex"/> points to visible leaf data columns only. Header-only, command, checkbox/select, and template-only columns cannot be merged.</description></item>
        ///   <item><description>Each merged area must fit entirely inside one frozen side (left, center, or right) and inside the visible virtualization window.</description></item>
        ///   <item><description>If a horizontal area and a vertical area cross each other, their sizes must match so that the crossing forms a proper rectangle. If not, the operation is stopped.</description></item>
        /// </list>
        /// <para>Performance:</para>
        /// <list type="bullet">
        ///   <item><description>Performs one set of checks and one screen update for the whole list to reduce extra re-renders.</description></item>
        ///   <item><description>Merged areas are recalculated when the view changes.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="infos"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
        /// <exception cref="InvalidOperationException">
        /// Thrown when any request breaks the rules, crosses view or frozen boundaries, is outside the visible virtualization window,
        /// targets non-leaf or hidden columns, targets non-data rows, or overlaps with another request or an existing merged area.
        /// </exception>
        /// <example>
        /// The following example performs two merges in one combined operation.
        /// <code><![CDATA[
        /// var list = new List<MergeCellInfo>
        /// {
        ///     new() { RowIndex = 0, ColumnIndex = 0, RowSpan = 3, ColumnSpan = 1 },
        ///     new() { RowIndex = 3, ColumnIndex = 2, RowSpan = 2, ColumnSpan = 2 }
        /// };
        /// grid.MergeCells(list);
        /// ]]></code>
        /// </example>
        /// <seealso cref="MergeCellInfo"/>
        /// <seealso cref="SfGrid{TValue}.MergeCellsAsync(MergeCellInfo)"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeCellsAsync(UnmergeCellInfo)"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeAllAsync()"/>

        public async Task MergeCellsAsync(IEnumerable<MergeCellInfo> infos)
        {
            await MergeModule!.MergeCellsAsync(infos).ConfigureAwait(true);
        }

        /// <summary>
        /// Removes the merged area that starts at the specified top-left cell in the current view of <see cref="SfGrid{TValue}"/>.
        /// Restores each covered cell so it is shown again, can receive focus, and can be navigated one by one.
        /// </summary>
        /// <param name="info">
        /// The unmerge request that identifies the anchor cell (top-left of the merged area) using zero-based
        /// <see cref="UnmergeCellInfo.RowIndex"/> and <see cref="UnmergeCellInfo.ColumnIndex"/> in the current view.
        /// </param>
        /// <remarks>
        /// <para>Behavior:</para>
        /// <list type="bullet">
        ///   <item><description>Unmerges the area whose anchor matches <paramref name="info"/>.</description></item>
        ///   <item><description>If the coordinates do not point to the anchor of a merged area, the call leaves the grid unchanged.</description></item>
        ///   <item><description>After unmerge, all previously covered cells are shown, focusable, and individually navigable.</description></item>
        /// </list>
        /// <para>Scope and constraints:</para>
        /// <list type="bullet">
        ///   <item><description>Applies only to the current view (for example, current page or virtual block). Results can change when the view changes.</description></item>
        ///   <item><description>Indices point to data rows and visible data columns only; header, caption, detail, and summary rows are not included.</description></item>
        /// </list>
        /// <para>Performance:</para>
        /// <list type="bullet">
        ///   <item><description>Runs as a single update. For many unmerges, prefer the overload that takes a collection to reduce re-renders.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="info"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when indices are negative or outside the bounds of the current view.</exception>
        /// <example>
        /// The following example removes the merged area anchored at row index 1 and column index 1.
        /// <code><![CDATA[
        /// grid.UnmergeCells(new UnmergeCellInfo
        /// {
        ///     RowIndex = 1,
        ///     ColumnIndex = 1
        /// });
        /// ]]></code>
        /// </example>
        /// <seealso cref="UnmergeCellInfo"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeCellsAsync(System.Collections.Generic.IEnumerable{UnmergeCellInfo})"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeAllAsync()"/>
        public async Task UnmergeCellsAsync(UnmergeCellInfo info)
        {
            await MergeModule!.UnmergeCellsAsync(info).ConfigureAwait(true);
        }

        /// <summary>
        /// Removes several merged cell areas in the rows currently shown by <see cref="SfGrid{TValue}"/> in one combined operation.
        /// The grid updates the screen once after all requests are handled.
        /// </summary>
        /// <param name="infos">
        /// The list of unmerge requests. Each request points to the top-left cell (the “anchor”) of a merged area using
        /// <see cref="UnmergeCellInfo.RowIndex"/> and <see cref="UnmergeCellInfo.ColumnIndex"/>. Indexing starts at 0 (the first row and first column are 0).
        /// </param>
        /// <remarks>
        /// <para>How the operation works:</para>
        /// <list type="bullet">
        ///   <item><description>All anchors are processed together; the order of items does not change the final result.</description></item>
        ///   <item><description>If an anchor does not match any merged area, that entry is skipped without changing the grid.</description></item>
        ///   <item><description>Repeated anchors and <see langword="null"/> entries are skipped; valid entries continue to be processed.</description></item>
        ///   <item><description>When a merged area is removed, the covered cells appear again as separate cells and can be focused and navigated one by one.</description></item>
        /// </list>
        /// <para>Where the operation applies:</para>
        /// <list type="bullet">
        ///   <item><description>Only affects the rows and columns that are currently visible (for example, the current page). The result can change when the view changes.</description></item>
        ///   <item><description>Row and column numbers refer to data rows and visible data columns only. Header, caption, detail, and summary rows are not included.</description></item>
        /// </list>
        /// <para>Performance:</para>
        /// <list type="bullet">
        ///   <item><description>Runs as a single grouped update to reduce the number of screen refreshes.</description></item>
        /// </list>
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="infos"/> is <see langword="null"/>.</exception>
        /// <exception cref="ArgumentException">Thrown when the collection is empty.</exception>
        /// <example>
        /// The following example removes two merged areas in one operation.
        /// <code><![CDATA[
        /// var toUnmerge = new List<UnmergeCellInfo>
        /// {
        ///     new() { RowIndex = 0, ColumnIndex = 0 },
        ///     new() { RowIndex = 3, ColumnIndex = 2 }
        /// };
        /// grid.UnmergeCells(toUnmerge);
        /// ]]></code>
        /// </example>
        /// <seealso cref="UnmergeCellInfo"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeCellsAsync(UnmergeCellInfo)"/>
        /// <seealso cref="SfGrid{TValue}.UnmergeAllAsync()"/>
        public async Task UnmergeCellsAsync(IEnumerable<UnmergeCellInfo> infos)
        {
            await MergeModule!.UnmergeCellsAsync(infos).ConfigureAwait(true);
        }

        /// <summary>
        /// Removes all merged regions in the current view.
        /// </summary>
        /// <remarks>
        /// Use this to reset merges before re-applying based on a new view (e.g., after paging, sorting, or filtering).
        /// </remarks>
        /// <example>
        /// grid.UnmergeAll();
        /// </example>
        public async Task UnmergeAllAsync()
        {
            await MergeModule!.UnmergeAllAsync().ConfigureAwait(true);
        }


        /// <summary>
        /// Determines whether the grid has any batch changes before updating it.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task{Boolean}"/> representing the asynchronous operation.
        /// It returns <c>true</c> if the grid has any batch changes, such as adding new records, edited records, and deleted records; otherwise, returns <c>false</c>.
        /// </returns>
        /// <remarks>
        /// This method will only work if the edit <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set to <see cref="EditMode.Batch"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="UpdateData" @onclick="UpdateData">UpdateData</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  <GridEditSettings AllowAdding="true" AllowEditing="true" AllowDeleting="true" Mode="EditMode.Batch"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task UpdateData()
        ///    {
        ///         bool isDirty = await grid.IsDirtyAsync(); // if the grid has any batch changes, it returns true; otherwise, false.
        ///         if(isDirty)
        ///         {
        ///             // You can customized code here.
        ///         }
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<bool> IsDirtyAsync()
        {
            if (EditSettings != null && !EditSettings.Mode.Equals(EditMode.Batch))
            {
                return await Task.FromResult<bool>(false).ConfigureAwait(true);
            }
            return await Task.FromResult<bool>(EditModule!.IsDirty()).ConfigureAwait(true);
        }

        /// <summary>
        /// Opens the column chooser pop up anywhere in the screen by given position(X and Y axis).
        /// </summary>
        /// <param name="x">Specifies the X axis position.</param>
        /// <param name="y">Specifies the Y axis position.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// You can manually open the column chooser at any position of screen at any desired time if <see cref="ShowColumnChooser"/> property is set to <c>true</c>
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="OpenColumnChooser" @onclick="ColumnHandler">OpenColumnChooser</button>
        /// <SfGrid @ref="grid" ShowColumnChooser="true" DataSource="@Orders">
        ///       .................
        /// </SfGrid>
        /// @code{
        ///      SfGrid<Order> grid;
        ///      private async Task ColumnHandler()
        ///      {
        ///		await grid.OpenColumnChooserAsync(200,50); // pass desire X and Y value
        ///      }
        /// ]]>
        /// </code>
        /// </example>
        public async Task OpenColumnChooserAsync(Nullable<double> x = null, Nullable<double> y = null)
        {
            // Task.Yield added to resolve Column Chooser not opens when we invoke OpenColumnChooserAsync method externally in WASM application
            await Task.Yield();
            Dictionary<string, string> chooserpositions = null!;
            if (x != null || y != null)
            {
                chooserpositions = new Dictionary<string, string>
                {
                    { "x", x != null ? string.Format(CultureInfo.InvariantCulture, "{0}", x) : null! },
                    { "y", y != null ? string.Format(CultureInfo.InvariantCulture, "{0}", y) : null! }
                };
            }

            RenderColumnChooser = !RenderColumnChooser;
            ShowChooser = true;
            EventAggregator.Trigger("ColumnChooserComp", chooserpositions);
            await Task.CompletedTask.ConfigureAwait(true);
        }

        /// <summary>
        /// Export Grid data to PDF document.
        /// </summary>
        /// <param name="pdfExportProperties">Provides the pdf export properties such as custom columns, data sources, themes, etc.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation</returns>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ExportToPdf<"@onclick="ExportHandler">ExportToPdf<</button>
        /// <SfGrid @ref="grid" AllowPdfExport="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExportHandler()
        ///    {
        ///        Syncfusion.Blazor.Grids.PdfExportProperties ExportProperties = new Syncfusion.Blazor.Grids.PdfExportProperties();
        ///        ExportProperties.ExportType = Syncfusion.Blazor.Grids.ExportType.CurrentPage; // here we have changed the ExportType from AllPages to CurrentPage, as like same we can change our desire properties.
        ///        await grid.ExportToPdfAsync(ExportProperties);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExportToPdfAsync(PdfExportProperties pdfExportProperties = null!)
        {
            await ExportToPdfAsync(asMemoryStream: false, pdfExportProperties).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the Grid PDF document as a <see cref="MemoryStream"/>.
        /// </summary>
        /// <param name="asMemoryStream">Specifies whether to return the PDF document as a memory stream.</param>
        /// <param name="pdfExportProperties">Optional. Provides the pdf export properties such as custom columns, data sources, themes, etc.</param>
        /// <returns>
        /// An asynchronous task that provides a <see cref="MemoryStream"/> containing the exported PDF document when <c>asMemoryStream</c> parameter is true;
        /// otherwise, it returns null and exports the PDF document in browser.
        /// </returns>
        /// Also, see <seealso cref="Syncfusion.Blazor.Grids.PdfExportProperties"/> for details on configuring pdf properties.
        /// <remarks>
        /// It exports the Grid data to a PDF document in the browser by defining <c>asMemoryStream</c> parameter value to false.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ExportToPdf<"@onclick="ExportHandler">ExportToPdf<</button>
        /// <SfGrid @ref="grid" AllowPdfExport="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ExportHandler()
        ///    {
        ///        Syncfusion.Blazor.Grids.PdfExportProperties ExportProperties = new Syncfusion.Blazor.Grids.PdfExportProperties();
        ///        ExportProperties.ExportType = Syncfusion.Blazor.Grids.ExportType.CurrentPage; // here we have changed the ExportType from AllPages to CurrentPage, as like same we can change our desire properties.
        ///        MemoryStream streamDocument = await grid.ExportToPdfAsync(true, ExportProperties);
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<MemoryStream> ExportToPdfAsync(bool asMemoryStream, PdfExportProperties pdfExportProperties = null!)
        {
            // Track telemetry for PDF export
            GridTelemetryHelper.LogTelemetry(true, "Exporting");
            
            using GridPdfExport<TValue> GridPDFExport = new GridPdfExport<TValue>();
            if (EnableInfiniteScrolling && InfiniteScrollModule != null)
            {
                InfiniteScrollModule.RequestType = "PDFExport";
            }
            return await GridPDFExport.PdfExport(this, pdfExportProperties, isMemoryStreamExport: asMemoryStream).ConfigureAwait(true);
        }

        /// <summary>
        /// Print all pages of the Grid and it hides the pager.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// You can customize the print options by using the <see cref="Syncfusion.Blazor.Grids.PrintMode"/> property of the Grid component.
        /// </remarks>
        /// <example>        
        /// <code><![CDATA[
        /// <button id="Print" @onclick="PrintHandler">Print</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// ............
        /// </SfGrid>
        /// @code{
        ///      SfGrid<Order> grid;
        ///      private async Task PrintHandler()
        ///      {
        ///		await grid.PrintAsync(); 
        ///      }
        /// ]]>
        /// </code>
        /// </example>
        public async Task PrintAsync()
        {
            IsPrinting = true;
            await CallStateHasChangedAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Refreshes the Grid with column changes.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous refresh operation.</returns>
        /// <remarks>
        /// Refresh the Grid columns when column property values are updated externally.
        /// This method sets the <see cref="ForceUpdate"/> property to <c>true</c> and calls the <see cref="CallStateHasChangedAsync"/> method asynchronously.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="RefreshColumns" @onclick="RefreshHandler">RefreshColumns</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task RefreshHandler()
        ///    {
        ///      await grid.RefreshColumnsAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task RefreshColumnsAsync()
        {
            ForceUpdate = true;
            await CallStateHasChangedAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Refreshes the Grid header.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous refresh operation.</returns>
        /// <remarks>
        /// This method should be called whenever the header of the component needs to be refreshed.
        /// This method sets the <see cref="RefreshColumnHeader"/> and  <see cref="ForceUpdate"/> properties to <c>true</c> and calls the <see cref="CallStateHasChangedAsync"/> method asynchronously.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="RefreshHeader" @onclick="RefreshHandler">RefreshHeader</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task RefreshHandler()
        ///    {
        ///      await grid.RefreshHeaderAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task RefreshHeaderAsync()
        {
            RefreshColumnHeader = true;
            ForceUpdate = true;
            await CallStateHasChangedAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Moves the Grid column positions in the UI from one column index to another based on field index. 
        /// </summary>
        /// <param name="fromIndex">Specifies the current index of the column.</param>
        /// <param name="toIndex">Specifies the destination or drop column index.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks> 
        /// If you invoke <see cref="ReorderColumnByIndexAsync"/> multiple times, then you won't get the same results every time.
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowReordering"/> should be set to <c>true</c> in order to use this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ReorderColumnByIndex" @onclick="ReorderColumn">ReorderColumnByIndex</button>
        /// <SfGrid @ref="grid" AllowReordering="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ReorderColumn()
        ///    {
        ///         await grid.ReorderColumnByIndexAsync(0,3);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ReorderColumnByIndexAsync(int fromIndex, int toIndex)
        {
            await InvokeMethod("sfBlazor.Grid.reorderColumnByIndex", new object[] { DataId, fromIndex, toIndex }).ConfigureAwait(true);
        }

        /// <summary>
        /// Moves a column in the Grid UI from one position to another, based on its field name and destination index.
        /// </summary>
        /// <param name="fieldName">The name of the column to be moved, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <param name="toIndex">The destination or drop index of the column in the Grid UI.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks> 
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowReordering"/> should be set to <c>true</c> in order to use this method.
        /// If you invoke <see cref="ReorderColumnByTargetIndexAsync"/> method multiple times, then you will get the same results every time.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ReorderColumnByIndex" @onclick="ReorderColumn">ReorderColumnByIndex</button>
        /// <SfGrid @ref="grid" AllowReordering="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ReorderColumn()
        ///    {
        ///         await grid.ReorderColumnByTargetIndexAsync("OrderID",3);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ReorderColumnByTargetIndexAsync(string fieldName, int toIndex)
        {
            await InvokeMethod("sfBlazor.Grid.reorderColumnByTargetIndex", new object[] { DataId, fieldName, toIndex }).ConfigureAwait(true);
        }


        /// <summary>
        /// Changes the Grid column positions based on column field names.
        /// </summary>
        /// <param name="fromFieldNames">The list of columns to be moved.</param>
        /// <param name="toFieldName">The destination of the column where the column should be moved.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowReordering"/> should be set to <c>true</c> in order to use this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ReorderColumns" @onclick="ReorderColumn">ReorderColumns</button>
        /// <SfGrid @ref="grid" AllowReordering="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ReorderColumn()
        ///    {
        ///         await grid.ReorderColumnsAsync(new List<string> { "OrderID", "CustomerID" }, "ShipCountry");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ReorderColumnsAsync(List<string> fromFieldNames, string toFieldName)
        {
            if (fromFieldNames != null && fromFieldNames.Count > 0 && toFieldName != null)
            {
                await InvokeMethod("sfBlazor.Grid.reorderColumns", new object[] { DataId, fromFieldNames, toFieldName }).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Changes the Grid column positions based on column field names.
        /// </summary>
        /// <param name="fromFieldNames">An array of strings that specifies the list of columns to be moved.</param>
        /// <param name="toFieldName">The destination of the column where the column should be moved.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowReordering"/> should be set to <c>true</c> in order to use this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ReorderColumns" @onclick="ReorderColumn">ReorderColumns</button>
        /// <SfGrid @ref="grid" AllowReordering="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ReorderColumn()
        ///    {
        ///         string[] Columns={"OrderID", "CustomerID"};
        ///         await grid.ReorderColumnsAsync(columns, "ShipCountry");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ReorderColumnsAsync(string[] fromFieldNames, string toFieldName)
        {
            await InvokeMethod("sfBlazor.Grid.reorderColumns", new object[] { DataId, fromFieldNames, toFieldName }).ConfigureAwait(true);
        }

        /// <summary>
        /// Changes the position of a column in the Grid by its field name.
        /// </summary>
        /// <param name="fromFieldName">The name of the column to be moved.</param>
        /// <param name="toFieldName">The destination of the column where the column should be moved.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowReordering"/> should be set to <c>true</c> in order to use this method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ReorderColumns" @onclick="ReorderColumn">ReorderColumns</button>
        /// <SfGrid @ref="grid" AllowReordering="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ReorderColumn()
        ///    {
        ///         await grid.ReorderColumnAsync("OrderID", "ShipCountry");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ReorderColumnAsync(string fromFieldName, string toFieldName)
        {
            if (!string.IsNullOrWhiteSpace(fromFieldName) && !string.IsNullOrWhiteSpace(toFieldName))
            {
                await InvokeMethod("sfBlazor.Grid.reorderColumns", new object[] { DataId, fromFieldName, toFieldName }).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Changes the Grid row position with given indexes.
        /// </summary>
        /// <param name="fromIndex">The current row index from which the row needs to be moved.</param>
        /// <param name="toIndex">The new row index to which the row needs to be moved.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ReorderRow" @onclick="ReorderRow">ReorderRow</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ReorderRow()
        ///    {
        ///         await grid.ReorderRowAsync(0,5);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ReorderRowAsync(int fromIndex, int toIndex)
        {
            if (RowReorderModule != null)
            {
                await RowReorderModule.ReorderRows((int)fromIndex, (int)toIndex).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Saves the cell that is currently being edited.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method will only save the cell if the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set as <see cref= "EditMode.Batch"/> and <see cref="Syncfusion.Blazor.Grids.GridEditSettings.AllowEditing"/> is set to <c>true</c>.
        /// This method does not save the value to the data source. Updated the value in Grid UI level only.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SaveCell" @onclick="CellHandler">SaveCell</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowEditing="true" Mode="EditMode.Batch"/>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task CellHandler()
        ///    {
        ///         await grid.SaveCellAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SaveCellAsync()
        {
            await EditModule!.SaveCell().ConfigureAwait(true);
        }

        /// <summary>
        /// Selects a cell by the given index.
        /// </summary>
        /// <param name="cellIndex">The row and cell index of the cell to be selected. For example (0, 1).</param>
        /// <param name="isToggle">Determines whether to toggle the selection of the cell or not. Default value is <c>false</c>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method will only select the cell if the<see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/> is set as <see cref= "SelectionMode.Cell"/> and <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSelection"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SelectCell" @onclick="CellHandler">SelectCell</button>
        /// <SfGrid @ref="grid" AllowSelection="true" DataSource="@Orders">
        /// <GridSelectionSettings Mode="SelectionMode.Cell">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task CellHandler()
        ///    {
        ///         await grid.SelectCellAsync((1,3)); // pass row and cell index
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SelectCellAsync(ValueTuple<int, int> cellIndex, Nullable<bool> isToggle = null)
        {
            isToggle = false;
            if (SelectionModule != null)
            {
                await SelectionModule.SelectCell(cellIndex, true).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Selects a collection of cells by row and column indexes.
        /// </summary>
        /// <param name="rowCellIndexes">An array of row and cell indexes that specifies the cells to be selected.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only select the cell if the<see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/> is set as <see cref= "SelectionMode.Cell"/> and <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSelection"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SelectCells" @onclick="CellHandler">SelectCells</button>
        /// <SfGrid @ref="grid" AllowSelection="true" DataSource="@Orders">
        /// <GridSelectionSettings Mode="SelectionMode.Cell">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task CellHandler()
        ///    {
        ///         var rowCellIndexes = new (int Row, int Cell)[]{(1, 3),(2, 2),(3, 1),(4, 2)};
        ///         await grid.SelectCellsAsync(rowCellIndexes);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SelectCellsAsync(ValueTuple<int, int>[] rowCellIndexes)
        {
            if (rowCellIndexes != null && SelectionModule != null)
            {
                await SelectionModule.SelectCells(rowCellIndexes, true).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Selects a range of cells starting from the specified start index and ending at the specified end index.
        /// </summary>
        /// <param name="startIndex">Specifies the value tuple of start index.</param>
        /// <param name="endIndex">Specifies the value tuple of end row cell index.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only select the cell if the <see cref="Syncfusion.Blazor.Grids.GridSelectionSettings.Mode"/> is set as <see cref= "SelectionMode.Cell"/> and <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSelection"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SelectCellsByRange" @onclick="CellHandler">SelectCellsByRange</button>
        /// <SfGrid @ref="grid" AllowSelection="true" DataSource="@Orders">
        /// <GridSelectionSettings Mode="SelectionMode.Cell">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task CellHandler()
        ///    {
        ///         await grid.SelectCellsByRangeAsync((0, 3), (3, 2));
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SelectCellsByRangeAsync(ValueTuple<int, int> startIndex, ValueTuple<int, int>? endIndex = null) =>
            await (SelectionModule?.SelectCellsByRange(startIndex, (ValueTuple<int, int>)endIndex!, true))!.ConfigureAwait(true)!;

        /// <summary>
        /// Selects a row based on the specified index. By default, selection is limited to the current page records. To enable selection across different pages, set the <paramref name="selectAcrossPages"/> property to <c>true</c>.
        /// </summary>
        /// <param name="index">Specifies the row index to select. </param>
        /// <param name="isToggle">Determines whether to toggle the selection of the row.</param>
        /// <param name="selectAcrossPages">Specifies whether selection should span across multiple pages. When set to <c>true</c>, the grid will navigate to the appropriate page to select the row. </param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation. </returns>
        /// <remarks>
        /// This method will only select the row if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSelection"/> is set to <c>true</c>.
        /// The <paramref name="selectAcrossPages"/> property is applicable only when the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowPaging"/> property is enabled in the Grid.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SelectRow" @onclick="SelectionHandler">SelectRow</button>
        /// <SfGrid @ref="grid" AllowSelection="true" AllowPaging="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        ///
        /// @code{
        ///   SfGrid<Order> grid;
        ///    private async Task SelectionHandler()
        ///    {
        ///         This example navigates to the second page and selects the 3rd row, since the row index passed is 15 and the default page size is 12.
        ///         await grid.SelectRowAsync(15, false, true);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SelectRowAsync(int index, Nullable<bool> isToggle = null,  bool selectAcrossPages = false)
        {
            FocusModule!.SelectedRowIndex = (int?)index;
            var dataRows = Rows.Where(e => e.IsDataRow).ToList();
            if (FocusModule.SelectedCellIndex == null || FocusModule.SelectedCellIndex == 0 || IsToolbarInteraction || EditModule!.KeyCode != null
                || FocusModule.IsByKey)
            {
                bool isBatchOrCellEditMode = EditModule?.IsCellOrBatchEditMode() == true && FocusModule.SelectedCellIndex != null && FocusModule.SelectedCellIndex >= 0;
                if (!isBatchOrCellEditMode)
                {
                       FocusModule.SelectedCellIndex = dataRows.Count != 0 ? dataRows.FirstOrDefault()?.Cells.Where(x => x.Visible && x.CellType != CellType.RowDrag).FirstOrDefault()?.Index ?? 0 : 0;
                }
            }

            if (selectAcrossPages && AllowPaging && index >= 0 && index < TotalItemCount)
            {
                bool isLazyGroup = AllowGrouping && GroupSettings!.EnableLazyLoading && GroupSettings.Columns?.Length > 0;
                int pageSize = PageSettings!.PageSize;
                int pageIndex = isLazyGroup ? GroupModule!.CalculatePageIndex(CurrentViewData!, index, pageSize)
                                            : (index / pageSize + 1);
                if (!isLazyGroup)
                {
                    index %= pageSize;
                }
                await (PagerRef?.GoToPageAsync(pageIndex))!.ConfigureAwait(true)!;
            }
            await (SelectionModule?.SelectRow(index, isToggle, true))!.ConfigureAwait(true)!;
        }

        /// <summary>
        /// Selects a collection of rows by their indexes.
        /// </summary>
        /// <param name="rowIndexes">An arrray of the row indexes to be selected.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method will only select the row if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSelection"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SelectRows" @onclick="SelectionHandler">SelectRows</button>
        /// <SfGrid @ref="grid" AllowSelection="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task SelectionHandler()
        ///    {
        ///         await grid.SelectRowsAsync(new int[] { 1, 2, 3 });
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SelectRowsAsync(int[] rowIndexes)
        {
            if(SelectionModule != null)
            await SelectionModule.SelectRows(rowIndexes).ConfigureAwait(true);
        }

        /// <summary>
        /// Selects a range of rows within the specified range.
        /// </summary>
        /// <param name="startIndex">The starting index of the row to be selected.</param>
        /// <param name="endIndex">The ending index of the row to be selected.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method selects a range of rows between startIndex and endIndex within a specified range. 
        /// This method will only select the row if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSelection"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SelectRowsByRange" @onclick="SelectionHandler">SelectRowsByRange</button>
        /// <SfGrid @ref="grid" AllowSelection="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task SelectionHandler()
        ///    {
        ///         await grid.SelectRowsByRangeAsync(3,7);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SelectRowsByRangeAsync(int startIndex, Nullable<int> endIndex = null)
        {
            if (SelectionModule != null)
            {
                SelectionModule.RangeStartIndex = startIndex;
                SelectionModule.RangeEndIndex = endIndex;
                await SelectionModule.SelectRowsByRange(startIndex, endIndex, true).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Updates the value of a particular cell based on the given primary key value.
        /// </summary>
        /// <param name="key">The primary key value of the row.</param>
        /// <param name="fieldName">The name of the column, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <param name="value">The new value for the cell.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method updates the value of the cell in the row with the specified primary key and column value.
        /// The primary key column should be using <see cref="GridColumn.IsPrimaryKey"/> property.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SetCellValue" @onclick="DataHandler">SetCellValue</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task DataHandler()
        ///    {
        ///         await grid.SetCellValueAsync(1005,"CustomerID","ANTON");
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SetCellValueAsync(object key, string fieldName, object value)
        {
            if(key != null)
            {
                await EditModule!.SetCellValue(key, fieldName, value).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Updates one or more cell values in the grid data source.
        /// </summary>
        /// <param name="cellValueUpdates">
        /// A collection of <see cref="CellUpdate"/> objects that specify the rows,
        /// columns, and values to update.
        /// </param>
        /// <returns>
        /// A <see cref="Task"/> that represents the asynchronous update operation.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Each update identifies a row using <see cref="CellUpdate.PrimaryKeyValue"/>,
        /// a column using <see cref="CellUpdate.ColumnField"/>, and the value to assign
        /// using <see cref="CellUpdate.NewValue"/>.
        /// </para>
        /// <para>
        /// The grid must contain a column configured as the primary key by setting
        /// <see cref="GridColumn.IsPrimaryKey"/> to <see langword="true"/>.
        /// </para>
        /// </remarks>
        /// <example>
        /// The following example updates the <c>Freight</c> and <c>CustomerID</c>
        /// fields in two different rows.
        /// <code><![CDATA[
        /// <button @onclick="UpdateCellValues">Update cell values</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///     <GridColumns>
        ///         <GridColumn Field="@nameof(Order.OrderID)" IsPrimaryKey="true" HeaderText="Order ID" />
        ///         <GridColumn Field="@nameof(Order.CustomerID)" HeaderText="Customer ID" />
        ///         <GridColumn Field="@nameof(Order.Freight)" HeaderText="Freight" />
        ///     </GridColumns>
        /// </SfGrid>
        /// @code {
        ///     private SfGrid<Order> grid = default!;
        ///
        ///     private async Task UpdateCellValues()
        ///     {
        ///         var cellValueUpdates = new List<CellUpdate>
        ///         {
        ///             new CellUpdate
        ///             {
        ///                 PrimaryKeyValue = 1005,
        ///                 ColumnField = nameof(Order.Freight),
        ///                 NewValue = 50.5
        ///             },
        ///             new CellUpdate
        ///             {
        ///                 PrimaryKeyValue = 1006,
        ///                 ColumnField = nameof(Order.CustomerID),
        ///                 NewValue = "ALFKI"
        ///             }
        ///         };
        ///
        ///         await grid.SetCellValueAsync(cellValueUpdates);
        ///     }
        /// }
        /// ]]></code>
        /// </example>
        public async Task SetCellValueAsync(IReadOnlyList<CellUpdate> cellValueUpdates)
        {
            await EditModule!.SetCellValue(cellValueUpdates).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates the value of a particular cell based on the specified primary key value or values.
        /// </summary>
        /// <param name="primaryKeyValues">
        /// A read-only dictionary that maps primary key column field names to their corresponding values.
        /// For grids with a single primary key column, provide a dictionary containing one key-value pair.
        /// For grids with composite primary keys, provide all primary key field names and their corresponding values.
        /// Field names are matched against the grid's primary key columns, and the order of entries in the dictionary is not significant.
        /// </param>
        /// <param name="fieldName">
        /// The name of the column to update, identified by its <see cref="GridColumn.Field"/> property.
        /// </param>
        /// <param name="value">
        /// The new value for the cell.
        /// </param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method updates the value of the specified cell in the row identified by the provided primary key value or values.
        /// <para>
        /// A primary key column must be defined using the <see cref="GridColumn.IsPrimaryKey"/> property.
        /// </para>
        /// <para>
        /// For grids with composite primary keys, all primary key field names and values must be provided in
        /// <paramref name="primaryKeyValues"/> to uniquely identify the row.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SetCellValueComposite" @onclick="CompositeHandler">Update Cell Value</button>
        /// <SfGrid @ref="grid" DataSource="@OrderItems">
        ///     ........
        /// </SfGrid>
        ///
        /// @code {
        ///     SfGrid<OrderItem> grid;
        ///
        ///     private async Task CompositeHandler()
        ///     {
        ///         await grid.SetCellValueAsync(
        ///             new Dictionary<string, object>
        ///             {
        ///                 { "OrderID", 10248 },
        ///                 { "ItemID", 2001 }
        ///             },
        ///             "Quantity",
        ///             5);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SetCellValueAsync(IReadOnlyDictionary<string, object>? primaryKeyValues, string fieldName, object value)
        {
            if (primaryKeyValues != null && primaryKeyValues.Count > 0)
            {
                List<CellUpdate> cellUpdates = new List<CellUpdate>
                {
                    new CellUpdate { ColumnField = fieldName, NewValue = value, PrimaryKeyValues = primaryKeyValues }
                };
                await EditModule!.SetCellValue(cellUpdates).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Updates the edited fields in batch mode while editing.
        /// </summary>
        /// <param name="Data">The data to be updated in the <c>batch</c> mode.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous update operation.</returns>
        /// <remarks>
        /// This method updates the cell if the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set to <see cref="EditMode.Batch"/>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="UpdateBatchRow" @onclick="DataHandler">UpdateBatchRow</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowAdding="true" AllowEditing="true" Mode="EditMode.Batch"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task DataHandler()
        ///    {
        ///         var BatchEdit = await this.grid.GetBatchChangesAsync();
        ///         List<Order> ChangedRecord = BatchEdit.ChangedRecords.ToList();
        ///         ChangedRecord[0].Freight = 100;
        ///         await grid.UpdateBatchRowAsync(ChangedRecord[0]);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task UpdateBatchRowAsync(TValue Data)
        {
            await EditModule!.UpdateBatchRow(Data!).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates and refresh the particular row values based on the given primary key value.
        /// </summary>
        /// <param name="primaryKeyValue">The primary key value of the row to be updated.</param>
        /// <param name="rowData">The new data to be used to update the row.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// A primary key column must be specified using the <see cref="GridColumn.IsPrimaryKey"/> property.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SetRowData" @onclick="DataHandler">SetRowData</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task DataHandler()
        ///    {
        ///         var rowData = new Order() { OrderID = 1006, CustomerID = "ALFKI", OrderDate = new DateTime(1995, 03, 25), Freight = 25.7 * 2 };
        ///         await grid.SetRowDataAsync(1006,rowData);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SetRowDataAsync(object primaryKeyValue, TValue rowData)
        {
            var pKey = await GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            if (primaryKeyValue == null || pKey == null || pKey.Count == 0)
            {
                return;
            }
            var singleKeyDict = new Dictionary<string, object> { { pKey[0]!, primaryKeyValue } };
            await SetRowDataAsync(singleKeyDict, rowData).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates and refresh the particular row values based on the given primary key value.
        /// </summary>
        /// <param name="primaryKeyValue">The primary key value of the row to be updated.</param>
        /// <param name="rowData">The new data to be used to update the row.</param>
        /// <param name="preventDataUpdate">Determines whether to update the data source or only refresh the UI.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// A primary key column must be specified using the <see cref="GridColumn.IsPrimaryKey"/> property.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SetRowData" @onclick="DataHandler">SetRowData</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task DataHandler()
        ///    {
        ///         var rowData = new Order() { OrderID = 1006, CustomerID = "ALFKI", OrderDate = new DateTime(1995, 03, 25), Freight = 25.7 * 2 };
        ///         await grid.SetRowDataAsync(1006,rowData,true);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SetRowDataAsync(object primaryKeyValue, TValue rowData, bool preventDataUpdate)
        {
            var pKey = await GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
            if (primaryKeyValue == null || pKey == null || pKey.Count == 0)
            {
                return;
            }
            var singleKeyDict = new Dictionary<string, object> { { pKey[0]!, primaryKeyValue } };
            await SetRowDataAsync(singleKeyDict, rowData, preventDataUpdate).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates and refreshes the values of a particular row based on the specified primary key value or values.
        /// </summary>
        /// <param name="primaryKeyValues">
        /// A read-only dictionary that maps primary key column field names to their corresponding values.
        /// For grids with a single primary key column, provide a dictionary containing one key-value pair.
        /// For grids with composite primary keys, provide all primary key field names and their corresponding values.
        /// Field names are matched against the grid's primary key columns, and the order of entries in the dictionary is not significant.
        /// </param>
        /// <param name="rowData">
        /// The new data object used to update the row.
        /// </param>
        /// <param name="preventDataUpdate">
        /// A boolean value indicating whether to prevent updating the underlying data source.
        /// When set to <see langword="true"/>, only the row UI is refreshed without updating the data source.
        /// When set to <see langword="false"/>, both the data source and the row UI are updated.
        /// It is an optional parameter and the default value is <see langword="false"/>.
        /// </param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.
        /// </returns>
        /// <remarks>
        /// This method updates the row identified by the specified primary key value or values and refreshes
        /// the corresponding row in the grid.
        /// <para>
        /// A primary key column must be specified using the <see cref="GridColumn.IsPrimaryKey"/> property.
        /// </para>
        /// <para>
        /// For grids with composite primary keys, all primary key field names and values must be provided in
        /// <paramref name="primaryKeyValues"/> to uniquely identify the row.
        /// </para>
        /// <para>
        /// When <paramref name="preventDataUpdate"/> is set to <see langword="true"/>, the grid refreshes the
        /// row using the values from <paramref name="rowData"/> without modifying the underlying data source.
        /// This is useful when the UI needs to be updated temporarily without persisting changes to the data collection.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SetRowData" @onclick="DataHandler">SetRowData</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        ///
        /// @code {
        ///     SfGrid<Order> grid;
        ///
        ///     private async Task DataHandler()
        ///     {
        ///         var rowData = new Order()
        ///         {
        ///             OrderID = 1006,
        ///             CustomerID = "ALFKI",
        ///             OrderDate = new DateTime(1995, 03, 25),
        ///             Freight = 25.7 * 2
        ///         };
        ///
        ///         await grid.SetRowDataAsync(
        ///             new Dictionary<string, object>
        ///             {
        ///                 { "OrderID", 1006 }
        ///             },
        ///             rowData,
        ///             true);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SetRowDataAsync(IReadOnlyDictionary<string, object>? primaryKeyValues, TValue rowData, bool preventDataUpdate = false)
        {
            if (primaryKeyValues != null && EditModule != null)
            {
                var pKey = await GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);
                var positional = BuildPositionalKeyValues(primaryKeyValues, pKey);
                await EditModule.SetRowData(positional, rowData!, preventDataUpdate).ConfigureAwait(true);
            }
        }

        internal async Task ShowColumnsOperation(string[] columnNames, string showBy = "HeaderText", bool suppressEvent = false)
        {
            SoftRefresh = false;
            if (columnNames != null && showBy != null)
            {
                var columns = await GetColumns(columnNames, showBy).ConfigureAwait(true);
                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i] == null)
                    {
                        continue;
                    }
                    columns[i].SetVisibility(true);
                    if (!EnableColumnVirtualization && !string.IsNullOrEmpty(columns[i].HideAtMedia))
                    {
                        _mediaColumnsUid.AddOrUpdateItem(columns[i].Uid, columns[i].Visible);
                    }
                }
                IsColumnHideOrShow = true;
            }

            if (!suppressEvent)
            {
                await ShowHideColumnsOperations().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Handle the common show and hide operations.
        /// </summary>
        private async Task ShowHideColumnsOperations(bool suppressEvent = false)
        {

            if (_mediaColumnsUid.Count > 0)
            {
                await InvokeMethod("sfBlazor.Grid.updateMediaColumns", new object[] { DataId, _mediaColumnsUid }).ConfigureAwait(true);
                _mediaColumnsUid.Clear();
            }

            ForceUpdate = true;
            if (GridEvents?.HeaderCellInfo.HasDelegate == true)
            {
                IsColumnHeaderChange = false;
            }

            var columnsChangingEventArgs = new ColumnVisibilityChangingEventArgs() { Cancel = false, VisibleColumns = await GetVisibleColumnsAsync().ConfigureAwait(true), HiddenColumns = await GetHiddenColumnsAsync().ConfigureAwait(true) };
            await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.ColumnState }, suppressEvent: suppressEvent, eventArgs: columnsChangingEventArgs, requestType: "ColumnState").ConfigureAwait(true);
            if (columnsChangingEventArgs.Cancel)
            {
                RenderColumnChooser = false;
                return;
            }
            if (IsRendered && EnablePersistence)
            {
                await SetLocalStorage().ConfigureAwait(true);
            }

            if (ShowHideEvent)
            {
                if (GridEvents?.OnActionComplete.HasDelegate == true || IsRenderedFromTreeGrid)
                {
                    var actionArgs = new ActionEventArgs<TValue>() { RequestType = Grids.Action.ColumnState, Parent = this, VisibleColumns = await GetVisibleColumnsAsync().ConfigureAwait(true) };
                    if(IsRenderedFromTreeGrid)
                        await EventAggregator.NotifyAsync("ActionComplete", actionArgs).ConfigureAwait(true);
                    else
                        await (GridEvents?.OnActionComplete.InvokeAsync(actionArgs))!.ConfigureAwait(true)!;
                }
                List<GridColumn> refCols = Columns!;
                await InvokeViewRefresh(refCols!).ConfigureAwait(true);
                if (GridEvents?.ColumnVisibilityChanged.HasDelegate == true || IsRenderedFromTreeGrid)
                {
                    ColumnVisibilityChangedEventArgs columnsVisibilityEventArgs = new ColumnVisibilityChangedEventArgs() { Parent = this, VisibleColumns = await GetVisibleColumnsAsync().ConfigureAwait(true), HiddenColumns = await GetHiddenColumnsAsync().ConfigureAwait(true) };
                    if(IsRenderedFromTreeGrid)
                        await EventAggregator.NotifyAsync("ColumnsVisibilityChanged", columnsVisibilityEventArgs).ConfigureAwait(true);
                    else
                        await (GridEvents?.ColumnVisibilityChanged.InvokeAsync(columnsVisibilityEventArgs))!.ConfigureAwait(true)!;
                }
            }
            if (AllowResizing)
            {
                await InvokeMethod("sfBlazor.Grid.updateTableWidth", new object[] { DataId, Columns! }).ConfigureAwait(true);
            }

            if (FreezeModule!.GetFrozenCount() > 0)
            {
                HasFreezeDirection = true;
            }
            if (FreezeModule!.GetFrozenCount() > 0 || FrozenRows > 0)
            {
                
                await InvokeMethod("sfBlazor.Grid.frozenHeight", new object[] { DataId, GetClientOption(), null! }).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Show one or more columns in the grid by their <see cref="GridColumn.Field"/> or <see cref="GridColumn.HeaderText"/>.
        /// </summary>
        /// <param name="columnNames">The list of name of the columns to be shown in the grid.</param>
        /// <param name="showBy">Specifies whether the column name is shown by its <c>Field</c> or <c>HeaderText</c>. Default value is <c>HeaderText</c>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of showing the specified columns in the grid.</returns>
        /// <remarks>
        /// You can dynamically show hidden columns in the grid.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ShowColumns" @onclick="ColumnHandler">ShowColumns</button>
        /// <button id="ShowColumnsByField" @onclick="ColumnFieldHandler">ShowColumnsByField</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ColumnHandler()
        ///    {
        ///         var columns = new List<string>() { "Freight", "Ship Country" };
        ///         await grid.ShowColumnsAsync(columns.ToArray());
        ///    }
        ///    private async Task ColumnFieldHandler()
        ///    {
        ///         var columns = new List<string>() { "Freight", "ShipCountry" };
        ///         await grid.ShowColumnsAsync(columns.ToArray(),"Field");   //pass second param as "Field" to show using field name.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ShowColumnsAsync(string[] columnNames, string showBy = "HeaderText")
        {
            await ShowColumnsOperation(columnNames, showBy).ConfigureAwait(true);
        }

        /// <summary>
        /// Show a column in the grid by its <see cref="GridColumn.Field"/> or <see cref="GridColumn.HeaderText"/> property.
        /// </summary>
        /// <param name="columnName">The name of the column to be shown in the grid.</param>
        /// <param name="showBy">Specifies whether the column name is shown by its <c>Field</c> or <c>HeaderText</c>. Default value is <c>HeaderText</c>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of showing specified column in the grid.</returns>
        /// <remarks>
        /// You can dynamically show hidden column in the grid.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ShowColumn" @onclick="ColumnHandler">ShowColumn</button>
        /// <button id="ShowColumnByField" @onclick="ColumnFieldHandler">ShowColumnByField</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ColumnHandler()
        ///    {
        ///         await grid.ShowColumnAsync("Ship Country");
        ///    }
        ///    private async Task ColumnFieldHandler()
        ///    {
        ///         await grid.ShowColumnAsync("ShipCountry","Field");   //pass second param as "Field" to show using field name.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ShowColumnAsync(string columnName, string showBy = "HeaderText")
        {
            string[] keys = new string[] { columnName };
            await ShowColumnsOperation(keys, showBy).ConfigureAwait(true);
        }

        internal async Task HideColumnsOperation(string[] columnNames, string hideBy = "HeaderText", bool suppressEvent = false)
        {
            SoftRefresh = false;
            if (columnNames != null && hideBy != null)
            {
                var columns = await GetColumns(columnNames, hideBy).ConfigureAwait(true);
                for (int i = 0; i < columns.Count; i++)
                {
                    if (columns[i] == null)
                    {
                        continue;
                    }
                    columns[i].SetVisibility(false);
                    if (!EnableColumnVirtualization && !string.IsNullOrEmpty(columns[i].HideAtMedia))
                    {
                        _mediaColumnsUid.AddOrUpdateItem(columns[i].Uid, columns[i].Visible);
                    }
                }
                IsColumnHideOrShow = true;
            }

            await ShowHideColumnsOperations(suppressEvent: suppressEvent).ConfigureAwait(true);

        }

        /// <summary>
        /// Hide one or more columns in the grid by their <see cref="GridColumn.Field"/> or <see cref="GridColumn.HeaderText"/>.
        /// </summary>
        /// <param name="columnNames">An array of column names to be hidden in the grid.</param>
        /// <param name="hideBy">Specifies whether the column name is hide by its <c>Field</c> or <c>HeaderText</c>. Default value is <c>HeaderText</c>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of hiding the specified columns in the grid</returns>
        /// <remarks>
        /// You can dynamically hide showing columns in the grid.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="HideColumns" @onclick="ColumnHandler">HideColumns</button>
        /// <button id="HideColumnsByField" @onclick="ColumnFieldHandler">HideColumnsByField</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ColumnHandler()
        ///    {
        ///         var columns = new List<string>() { "Freight", "Ship Country" };
        ///         await grid.HideColumnsAsync(columns.ToArray());
        ///    }
        ///    private async Task ColumnFieldHandler()
        ///    {
        ///         var columns = new List<string>() { "Freight", "ShipCountry" };
        ///         await grid.HideColumnsAsync(columns.ToArray(),"Field");   //pass second param as "Field" to hide using field name.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task HideColumnsAsync(string[] columnNames, string hideBy = "HeaderText")
        {
            await HideColumnsOperation(columnNames, hideBy).ConfigureAwait(true);
        }

        /// <summary>
        /// Hides a column in the grid by its <see cref="GridColumn.Field"/> or <see cref="GridColumn.HeaderText"/>.
        /// </summary>
        /// <param name="columnName">The name of the column to hide.</param>
        /// <param name="hideBy">Specifies whether the column name is hide by its <c>Field</c> or <c>HeaderText</c>. Default value is <c>HeaderText</c>.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of hiding the specified column in the grid</returns>
        /// <remarks>
        /// You can dynamically hide showing columns in the grid.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="HideColumn" @onclick="ColumnHandler">HideColumn</button>
        /// <button id="HideColumnByField" @onclick="ColumnFieldHandler">HideColumnByField</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ColumnHandler()
        ///    {
        ///         await grid.HideColumnAsync("Ship Country");
        ///    }
        ///    private async Task ColumnFieldHandler()
        ///    {
        ///         await grid.HideColumnAsync("ShipCountry","Field");   //pass second param as "Field" to hide using field name.
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task HideColumnAsync(string columnName, string hideBy = "HeaderText")
        {
            string[] keys = new string[] { columnName };
            await HideColumnsOperation(keys, hideBy).ConfigureAwait(true);       
        }

        internal async Task<List<GridColumn>> GetColumns(string[] keys, string showBy)
        {
            var columns = GridUtils.GetColumns(this);
            var shColumns = new List<GridColumn>();
            for (int i = 0; i < keys.Length; i++)
            {
                if (showBy.Equals("field", StringComparison.OrdinalIgnoreCase))
                {
                    shColumns.AddRange(columns.Where(column => column.Field != null && column.Field.Equals(keys[i], StringComparison.Ordinal)));
                }
                else
                {
                    shColumns.AddRange(columns.Where(column => column.HeaderText != null && column.HeaderText.Equals(keys[i], StringComparison.Ordinal)));
                }
            }

            return await Task.FromResult(shColumns.Distinct().ToList()).ConfigureAwait(true);
        }

        /// <summary>
        /// Shows a spinner on the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// By default, the grid shows the spinner for all its actions. You can use <see cref="ShowSpinnerAsync()"/> method to show the spinner at your desired time.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ShowSpinner" @onclick="ShowSpinner">ShowSpinner</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task ShowSpinner()
        ///    {
        ///         await grid.ShowSpinnerAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ShowSpinnerAsync()
        {
            if (_hasSpinner && SpinnerRef != null)
            {
                await SpinnerRef.ShowAsync().ConfigureAwait(true);
                await Task.CompletedTask.ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Sorts a column with the given options.
        /// </summary>
        /// <param name="columnName">The field name of the column to sort.</param>
        /// <param name="direction">The direction of the sort which is <see cref="SortDirection.Ascending"/> to sort in ascending order, or <see cref="SortDirection.Descending"/> to sort in descending order.</param>
        /// <param name="isMultiSort">Specifies whether it is a multi-sorting operation.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of sorting the column.</returns>
        /// <remarks>
        /// This method will only sort the column if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSorting"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SortColumn" @onclick="SortHandler">SortColumn</button>
        /// <SfGrid @ref="grid" AllowSorting="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task SortHandler()
        ///    {
        ///         await grid.SortColumnAsync("CustomerID", SortDirection.Descending);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SortColumnAsync(string columnName, SortDirection direction, Nullable<bool> isMultiSort = null)
        {
            if (SortModule != null)
            {
                await SortModule.SortColumn(columnName, direction, isMultiSort, invokedByMethod: true).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Clears the previous sorted columns and sorts a list of columns with the given options.
        /// </summary>
        /// <param name="columns">A list of the columns to be sorted.</param>
        /// <param name="clearPreviousSort">A boolean value that specifies whether to clear the previous sort.</param>
        /// <returns><see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of sorting the columns.</returns>
        /// <remarks>
        /// This method will only sort the columns if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSorting"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SortColumns" @onclick="SortHandler">SortColumns</button>
        /// <SfGrid @ref="grid" AllowSorting="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    public List<SortColumn> sortColumns { get; set; } = new List<SortColumn>();
        ///    private async Task SortHandler()
        ///    {
        ///         sortColumns.Add(new() { Field = nameof(Order.Freight), Direction = SortDirection.Descending });
        ///         sortColumns.Add(new() { Field = nameof(Order.ShipCountry), Direction = SortDirection.Ascending});
        ///         await grid.SortColumnsAsync(sortColumns,true);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SortColumnsAsync(List<SortColumn> columns, bool clearPreviousSort)
        {
            if (clearPreviousSort)
            {
                await ClearSortingAsync().ConfigureAwait(true);
            }

            if (SortModule != null && columns != null)
            {
                await SortModule.ProcessColumnsSorting(columns).ConfigureAwait(true);
            }
            await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Sorting, Cancel = false }, eventArgs: new SortingEventArgs() { Cancel = false, SortedColumns = columns!}, requestType:"Sorting").ConfigureAwait(true);
        }

        /// <summary>
        /// Sorts a list of columns with the given options.
        /// </summary>
        /// <param name="columns">A list of the columns to be sorted.</param>
        /// <returns><see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation of sorting the column.</returns>
        /// <remarks>
        /// This method will only sort the column if the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.AllowSorting"/> is set to <c>true</c>.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="SortColumns" @onclick="SortHandler">SortColumns</button>
        /// <SfGrid @ref="grid" AllowSorting="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    public List<SortColumn> sortColumns { get; set; } = new List<SortColumn>();
        ///    private async Task SortHandler()
        ///    {
        ///         sortColumns.Add(new() { Field = nameof(Order.Freight), Direction = SortDirection.Descending });
        ///         sortColumns.Add(new() { Field = nameof(Order.ShipCountry), Direction = SortDirection.Ascending});
        ///         await grid.SortColumnsAsync(sortColumns);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SortColumnsAsync(List<SortColumn> columns)
        {
            if (columns != null && SortModule != null)
            {
                foreach (var col in columns)
                {
                    await SortModule.SortColumn(col.Field!, col.Direction, true, true).ConfigureAwait(true);
                }
            }

            await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Sorting, Cancel = false }).ConfigureAwait(true);
        }

        /// <summary>
        /// Starts editing of the currently selected row in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method can only be invoked if the <see cref="GridEditSettings.AllowEditing"/> is set to true and the <see cref="GridEditSettings.Mode"/> is not set to <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>. 
        /// At least one row must be selected before invoking this method. If no rows are selected, an alert message is displayed.  
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="BeginEdit" @onclick="EditHandler">BeginEdit</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings AllowEditing="true"/>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task EditHandler()
        ///    {
        ///         await grid.StartEditAsync();  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task StartEditAsync()
        {
            if (!EditSettings!.AllowEditing || EditModule!.IsCellOrBatchEditMode())
            {
                return;
            }
            var Row = await EditModule!.GetSelectedRowForEdit().ConfigureAwait(true);
            if (Row == null)
            {
                return;
            }

            await EditModule!.StartEdit(Row).ConfigureAwait(true);
        }

        /// <summary>
        /// Ungroups a previously grouped column by column name.
        /// </summary>
        /// <param name="columnName">The name of the column to ungroup.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous ungrouping operation.</returns>
        /// <remarks>
        /// If the specified <c>columnName</c> is not currently grouped, this method will not do anything.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="UngroupColumn" @onclick="UnGrouping">UngroupColumn</button>
        /// <SfGrid @ref="grid" AllowGrouping="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task UnGrouping()
        ///    {
        ///         await grid.UngroupColumnAsync("ShipCountry");  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task UngroupColumnAsync(string columnName)
        {
            if(GroupModule != null)
            await GroupModule.UnGroupColumn(columnName).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates the specified cell with the given value without changing it into edited state.
        /// </summary>
        /// <param name="rowIndex">The index of the row containing the cell to update.</param>
        /// <param name="field">The name of the column containing the cell to update.</param>
        /// <param name="value">The new value for the cell.</param>
        /// <returns><see cref="System.Threading.Tasks.Task"/>representing the cell updation operation.</returns>
        /// <remarks>
        /// This method will only update the cell if the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set as <see cref= "EditMode.Batch"/>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="UpdateCell" @onclick="CellHandler">UpdateCell</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        /// <GridEditSettings  Mode="EditMode.Batch"></GridEditSettings>
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task CellHandler()
        ///    {
        ///         await grid.UpdateCellAsync(1, "Freight", 25.6);  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task UpdateCellAsync(int rowIndex, string field, object value)
        {
            if (EditSettings != null && EditSettings.Mode.Equals(EditMode.Batch))
            {
                await EditModule!.UpdateCell(rowIndex, field, value, false).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Defines the text of external message.
        /// </summary>
        /// <param name="message">Specifies the externam message.</param>
        /// <returns><see cref="System.Threading.Tasks.Task"/>.</returns>
        public async Task UpdateExternalMessageAsync(string message)
        {
            await PageSettings!.UpdateProperties("ExternalMessage", message).ConfigureAwait(true);
        }

        /// <summary>
        /// Updates the specified row by given values without changing into an edited state.
        /// </summary>
        /// <param name="index">The index of the row to be updated.</param>
        /// <param name="data">The updated data for the specified row.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous updation operation.</returns>
        /// <remarks>The given updated data will replace the target record completely. 
        /// Property value comparison will not be performed to see changed values.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="UpdateRow" @onclick="RowHandler">UpdateRow</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task RowHandler()
        ///    {
        ///         var data = new Order() { OrderID=1007, CustomerID = "BOLID", OrderDate = new DateTime(1995, 05, 15), Freight = 25.7 * 2 };
        ///         await grid.UpdateRowAsync(6, data);  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task UpdateRowAsync(int index, TValue data)
        {
            await EditModule!.UpdateRow(index, data!).ConfigureAwait(true);
        }

        /// <summary>
        /// Expands or collapses the detail row of the Grid with the specified row data.
        /// </summary>
        /// <param name="data">The data of the row by which the detail row will be expanded or collapsed.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> that represents the asynchronous expand or collapse operation.</returns>
        /// <remarks>
        /// The given <c>data</c> will be compared against the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.CurrentViewData"/> and if a matching row is found, its detail row will be expanded or collapsed. 
        /// If the input data and current view data do not have the same reference, use the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.ExpandCollapseDetailRowAsync(string, object)"/> method.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ExpandOrCollapse" @onclick="RowHandler">ExpandOrCollapse</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task RowHandler()
        ///    {
        ///         var data = new EmployeeData() { EmployeeID = 1, FirstName = "Nancy", LastName = "Davolio", Title = "Sales Representative", HireDate = new DateTime(1995, 05, 15), City = "Seattle", Country = "USA" };
        ///         await grid.ExpandCollapseDetailRowAsync(data);  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExpandCollapseDetailRowAsync(TValue data)
        {
            Row<object>? _row = Rows.Find(_ => _.Data != null && _.Data.Equals(data));
            if (_row != null && DetailRowModule != null)
            {
                await DetailRowModule.DetailClick(_row.Index, _row.Uid!).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Expands or collapses the detail row of the Grid with the specified field name and row data value..
        /// </summary>
        /// <param name="fieldName">The name of the column by which the detail row will be expanded or collapsed, identified by its <see cref="GridColumn.Field"/> property.</param>
        /// <param name="value">The value of the row by which the detail row will be expanded or collapsed</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/>representing the asynchronous expand or collapse operation.</returns>
        /// <remarks>
        /// The specified row data <c>value</c> will be compared against the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.CurrentViewData"/> and
        /// if a matching row is found, its detail row will be expanded or collapsed. 
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ExpandOrCollapse" @onclick="RowHandler">ExpandOrCollapse</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task RowHandler()
        ///    {
        ///         await grid.ExpandCollapseDetailRowAsync("FirstName", "Nancy");  
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExpandCollapseDetailRowAsync(string fieldName, object value)
        {
            Row<object> _row = Rows.Find(_ =>
            {
                object? val = PropHelper?.GetObject(fieldName, _.Data!);
                return val != null && val.Equals(value);
            })!;
            if (_row != null && DetailRowModule != null)
            {
                await DetailRowModule.DetailClick(_row.Index, _row.Uid!).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Expands all the detail rows of the Grid including those are currently collapsed.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/>representing the asynchronous expand operation.</returns>
        /// <remarks>
        /// You can use this method in the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.DataBound"/> event to expand all the rows at initial rendering.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="Expand" @onclick="Expand">ExpandAll</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task Expand()
        ///    {
        ///        await grid.ExpandAllDetailRowAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ExpandAllDetailRowAsync()
        {
            if (DetailRowModule != null)
            {
                await DetailRowModule.ExpandOrCollapseAll(true).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Collapses all the detail rows of the Grid including those are currently expanded.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/>representing the asynchronous collapse operation.</returns>
        /// <example>
        /// <code><![CDATA[
        /// <button id="Collapse" @onclick="Collapse">CollapseAll</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task Collapse()
        ///    {
        ///        await grid.CollapseAllDetailRowAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task CollapseAllDetailRowAsync()
        {
            if(DetailRowModule != null)
            await DetailRowModule.ExpandOrCollapseAll(false).ConfigureAwait(true);
        }

        /// <summary>
        /// Searches the Grid records using the given search key.
        /// </summary>
        /// <param name="searchString">Specifies the search key to be used for searching.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous searching operation.</returns>
        /// <remarks>Passing an empty search key or null will clear the searching.</remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="Search" @onclick="Searching">Search</button>
        /// <button id="ClearSearch" @onclick="ClearSearching">ClearSearch</button>
        /// <SfGrid @ref="Grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> Grid;
        ///    private async Task Searching()
        ///    {
        ///        await Grid.SearchAsync("ALFKI");
        ///    }
        ///    private async Task ClearSearching()
        ///    {
        ///        await Grid.SearchAsync(""); // pass empty string or null to clear the searching
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task SearchAsync(string searchString)
        {
            if (SearchModule != null)
            {
                await SearchModule.PerformSearch(searchString).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Highlights all cells containing the specified search key in the grid without filtering rows.
        /// </summary>
        /// <param name="findKey">The text to find and highlight in the grid cells. Pass <see langword="null"/> or an empty string (<c>""</c>) to clear all highlighting and reset the find state without requiring a separate <see cref="ResetFindAsync()"/> call. Whitespace-only strings (e.g., <c>"   "</c>) are treated as valid search text and will attempt to find whitespace matches in grid cells.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// <para>
        /// When <paramref name="findKey"/> is <see langword="null"/> or an empty string (<c>""</c>), 
        /// the method clears all highlighting and resets the find state.
        /// No separate call to <see cref="ResetFindAsync()"/> is required.
        /// </para>
        /// <para>
        /// <b>Whitespace strings:</b> Strings containing only whitespace characters (e.g., <c>"   "</c>) are treated as valid search terms 
        /// and will search for whitespace matches in grid cells. They do not trigger a clear operation.
        /// </para>
        /// <para>
        /// This method highlights text in grid cells using the CSS class specified in
        /// <see cref="GridFindSettings.HighlightCssClass"/>. The match counter reflects all matches in the dataset when virtualized, 
        /// or in the currently visible/filtered data when paginated. When pagination changes, highlighting is
        /// automatically reapplied to the newly visible cells.
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///   <item><description><b>Column templates and command columns</b>: cells rendered through <see cref="GridColumn.Template"/>, <see cref="GridColumn.EditTemplate"/>, or any <see cref="GridCommandColumn"/> declared inside <see cref="GridColumn.Commands"/> are not scanned, highlighted, counted in total matches, or reachable via "Next" / "Previous" navigation; only the plain text content rendered by the default cell pipeline for data columns is eligible for matching.</description></item>
        ///   <item><description><b>Edit-form text</b>: uncommitted text typed into the Add / Edit / Inline form editors (input, numeric, dropdown, date picker, etc.) is not searched, highlighted, counted in total matches, or reachable via navigation; the Find feature operates on the committed, display-time cell value only.</description></item>
        ///   <item><description><b>Column opt-out</b>: columns with <see cref="GridColumn.AllowSearchHighlight"/> set to <see langword="false"/> are excluded from search, match counting, and navigation.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="FindText" @onclick="FindText">Find Text</button>
        /// <button id="ClearFind" @onclick="ClearFind">Clear Find</button>
        /// <SfGrid @ref="grid" Toolbar="@(new List<string> { "Find" })" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        /// @code{
        ///     SfGrid<Order> grid;
        ///     private async Task FindText()
        ///     {
        ///         await grid.FindHighlightAsync("USA");
        ///     }
        ///     private async Task ClearFind()
        ///     {
        ///         await grid.ResetFindAsync();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task FindHighlightAsync(string findKey)
        {
            if(FindHightLightModule == null)
            {
                return;
            }
            FindHightLightModule.IsFromMethodHighLight = true;
            await FindHightLightModule.FindHighlightAsync(findKey, true).ConfigureAwait(true);
        }

        /// <summary>
        /// Clears all highlighting and resets the find state in the grid.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method clears the current find highlighting, resets the match counter.
        /// The grid is refreshed to reflect these changes.
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <button id="ClearFind" @onclick="ClearFind">Clear Find</button>
        /// <SfGrid @ref="grid" Toolbar="@(new List<string> { "Find" })" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        /// @code{
        ///     SfGrid<Order> grid;
        ///     private async Task ClearFind()
        ///     {
        ///         await grid.ResetFindAsync();
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ResetFindAsync()
        {
            if (FindHightLightModule == null)
            {
                return;
            }
            await FindHightLightModule.ClearFindProperties().ConfigureAwait(true);
            await FindHightLightModule.RefreshGrid().ConfigureAwait(true);
        }

        /// <summary>
        /// Moves the active Find and Highlight match to the next match on the <see cref="SfGrid{TValue}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> that completes after the grid updates the active match.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Mirrors the <b>Next</b> button in the Find toolbar and the <c>Enter</c> key in the find input.
        /// </para>
        /// <para>
        /// If no match is active after <see cref="FindHighlightAsync(string)"/>, the first match becomes active. At the last match, navigation wraps to the first match.
        /// </para>
        /// <para>
        /// Columns with <see cref="GridColumn.AllowSearchHighlight"/> set to <see langword="false"/> are skipped.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid @ref="grid" Toolbar="@(new List<string> { "Find" })" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        /// @code{
        ///     SfGrid<Order> grid;
        ///     private async Task FindNext() => await grid.FindNextHighlightAsync();
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task FindNextHighlightAsync()
        {
            if (FindHightLightModule == null) return;
            FindHightLightModule.IsFromMethodHighLight = true;
            _shouldRender = false;
            await FindHightLightModule.GoToNextAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Moves the active Find and Highlight match to the previous match on the <see cref="SfGrid{TValue}"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> that completes after the grid updates the active match.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Mirrors the <b>Previous</b> button in the Find toolbar and the <c>Shift+Enter</c> key in the find input.
        /// </para>
        /// <para>
        /// If no match is active after <see cref="FindHighlightAsync(string)"/>, the last match becomes active. At the first match, navigation wraps to the last match.
        /// </para>
        /// <para>
        /// Columns with <see cref="GridColumn.AllowSearchHighlight"/> set to <see langword="false"/> are skipped.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid @ref="grid" Toolbar="@(new List<string> { "Find" })" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        /// @code{
        ///     SfGrid<Order> grid;
        ///     private async Task FindPrev() => await grid.FindPreviousHighlightAsync();
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task FindPreviousHighlightAsync()
        {
            if (FindHightLightModule == null) return;
            FindHightLightModule.IsFromMethodHighLight = true;
            _shouldRender = false;
            await FindHightLightModule.GoToPreviousAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Activates a specific Find and Highlight match by its one-based index on the <see cref="SfGrid{TValue}"/>.
        /// </summary>
        /// <param name="matchNumber">
        /// A 1-based <see cref="int"/> index of the match to activate. Values below <c>1</c> are clamped to the first match; values above the total match count are clamped to the last match. No exception is thrown for out-of-range values.
        /// </param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> that completes after the grid updates the active match.
        /// </returns>
        /// <remarks>
        /// <para>
        /// Jumps directly to a match produced by <see cref="FindHighlightAsync(string)"/>.
        /// </para>
        /// <para>
        /// Performs no action when no matches are available.
        /// </para>
        /// <para>
        /// Columns with <see cref="GridColumn.AllowSearchHighlight"/> set to <see langword="false"/> are skipped.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code>
        /// <![CDATA[
        /// <SfGrid @ref="grid" Toolbar="@(new List<string> { "Find" })" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        /// @code{
        ///     SfGrid<Order> grid;
        ///     private async Task JumpToMatch()
        ///     {
        ///         await grid.FindHighlightAsync("Smith");
        ///         await grid.FindGoToAsync(3);
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task FindGoToAsync(int matchNumber)
        {
            if (FindHightLightModule == null) return;
            FindHightLightModule.IsFromMethodHighLight = true;
            _shouldRender = false;
            await FindHightLightModule.GoToAsync(matchNumber).ConfigureAwait(true);
        }


        /// <summary>
        /// Refreshes the grid header and content. Passing the additional parameter as <c>true</c> ensures the component is reinitialized with dynamic changes.
        /// </summary>
        /// <param name="isModelRefresh">Defaults to <c>false</c>. When set to <c>true</c>, specifies whether to re-render the grid with updated model property changes. </param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous refresh operation.
        /// </returns>
        /// <remarks>
        /// Calling the refresh method without passing the additional parameter refreshes the UI only. Setting <paramref name="isModelRefresh"/> to <c>true</c> refreshes the grid with updated property changes and reinitializes it if the model has changed.
        /// </remarks>
        /// <example>
        /// The following example demonstrates how to use the <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.Refresh(bool)"/> method
        /// to refresh a grid component in a Blazor application:
        /// <code>
        /// <![CDATA[
        /// <button @onclick="Refresh">Refresh</button>
        /// <SfGrid @ref="Grid" DataSource="@Orders">
        ///     @if (renderColumns)
        ///     {
        ///         <GridColumns> <!-- Define grid columns 5 here --> </GridColumns>
        ///         <GridAggregates> <!-- Define aggregates here --> </GridAggregates>
        ///     }
        ///     else
        ///     {
        ///         <GridColumns> <!-- Define grid columns 3 here --> </GridColumns>
        ///     }
        /// </SfGrid>
        ///
        /// @code {
        ///     private SfGrid<Order> Grid;
        ///     private bool renderColumns = false;
        ///    
        ///     private async Task Refresh()
        ///     {
        ///         renderColumns = true;
        ///         await Grid.Refresh(true); // Refresh the grid with dynamic columns
        ///     }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task Refresh(bool isModelRefresh = false)
        {
            if (isModelRefresh)
            {
                isGridModelRefresh = isModelRefresh;
            }
            else
            {
                RefreshColumnHeader = true;
                ForceUpdate = true;
                VirtualScrollModule!.RefreshByMethod = true;
                await InvokeAsync(async () => await ModelChanged(new ActionEventArgs<TValue>() { RequestType = Action.Refresh }).ConfigureAwait(true)).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Focuses the grid element.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous focus operation.</returns>
        /// <remarks>
        /// This method focuses on the grid element. If the grid element is not rendered yet, the method does nothing.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="Focus" @onclick="Focus">Focus</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task Focus()
        ///    {
        ///        await grid.FocusAsync();
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task FocusAsync()
        {
            if (IsRendered)
            {
                await InvokeMethod("sfBlazor.Grid.gridFocus", new object[] { DataId, true }).ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Updates the <c>PageSize</c> dynamically with the given height and refreshes the virtualization enabled grid based on the updated page size.
        /// </summary>
        /// <param name="height">The height of the parent or grid container.</param>
        /// <param name="rowHeight">The height of the grid row used to calculate the page size.</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous upadte pagsize operation.</returns>
        /// <remarks>
        /// The <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.EnableVirtualization"/> must be set to <c>true</c> to refresh the grid content based on the given height.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button id="UpdatePageSize" @onclick="UpdatePageSize">UpdatePageSize</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task UpdatePageSize()
        ///    {
        ///        await grid.UpdatePageSizeAsync(600,30);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task UpdatePageSizeAsync(int height, int rowHeight = 0)
        {
            if (rowHeight > 0) 
            { 
                _updateVirtualPageSize = true;
                int RHeight = rowHeight != 0 ? rowHeight : VirtualScrollModule!.RHeight;
                int PageSize = Convert.ToInt32(height / RHeight) * 2;
                await PageSettings!.UpdateProperties("PageSize", PageSize).ConfigureAwait(true);
                await InvokeMethod("sfBlazor.Grid.refreshOnDataChange", new object[] { DataId }).ConfigureAwait(true);
                await DataProcess().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Used to update the page size of the Grid to change the number of rows that can be rendered on a current view port.
        /// </summary>
        /// <param name="pageSize">The number of items to be shown on a page. </param>
        /// <returns><see cref="System.Threading.Tasks.Task"/>.</returns>
        /// <remarks>
        /// By changing the page size, the Grid Pager component dynamically updates the total number of pages according to the given page size and updates the UI.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <button @onclick="HandleButtonClick">UpdatePageSize</button>
        /// <SfGrid @ref="grid" AllowPaging="true" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///     SfGrid<Order> grid;
        ///    private async Task HandleButtonClick()
        ///    {
        ///      await grid.UpdatePageSizeAsync(4);// pass the page size here.    
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task UpdatePageSizeAsync(int pageSize)
        {
            if (!AllowPaging)
            {
                return;
            }
            if (pageSize > 0)
            {
                await PageSettings!.UpdateProperties("PageSize", pageSize).ConfigureAwait(true);
                await Refresh().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Update the <c>Width</c> dynamically with given width and refresh the column virtualization enabled grid based on that updated width of the column.
        /// </summary>
        /// <param name="width">Specifies the parent/grid container width.</param>
        /// <returns><see cref="System.Threading.Tasks.Task"/></returns>
        /// <remarks><see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.EnableColumnVirtualization"/>must be true to refresh the grid content based on the given width.
        /// </remarks>
        internal async Task RefreshVirtualGrid(string width)
        {
            Width = width;
            if (VirtualScrollModule != null)
            {
                VirtualScrollModule.VirtualLoadListener(null!);
            }
            await CallStateHasChangedAsync().ConfigureAwait(true);
            await InvokeMethod("sfBlazor.Grid.updateOptions", new object[] { DataId, GetClientOption() }).ConfigureAwait(true);
        }

        /// <summary>
        /// Update the <c>scrollLeft</c> value dynamically and refresh the column virtualization enabled grid based on that updated scrollleft value.
        /// </summary>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation.</returns>
        /// <remarks>
        /// This method is mainly used for while resizing a splitter pane when the grid's width is set to <c>auto</c> and <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.EnableColumnVirtualization"/> is set as true, empty space issue occurs. To prevent this issue, this method updates the scroll left value and refreshes the grid.
        /// </remarks>
        internal async Task RefreshScrollLeftPosition()
        {
            await InvokeMethod("sfBlazor.Grid.refreshScrollLeftPosition", new object[] { DataId }).ConfigureAwait(true);
        }

        /// <summary>
        /// Scroll to specific row or column into view based on the row and column indexes.
        /// </summary>
        /// <param name="columnIndex">The index of the column to be scrolled.</param>
        /// <param name="rowIndex">The index of the row to be scrolled.</param>
        /// <param name="rowHeight">The height of the row that specfies the row <c>offsetHeight</c> and used to calculate the scroll offset values. This parameter is applicable only when row virtualization enabled.</param>
        /// <remarks>
        /// To scroll the grid content horizontally based on the specified column index, set <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.EnableColumnVirtualization"/> property in the Blazor Grid.
        /// To scroll the grid content vertically based on the specified row index, set <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.EnableVirtualization"/> property in the Blazor Grid. 
        /// </remarks>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous scroll operation.</returns>
        /// <example>
        /// <code><![CDATA[
        /// <button id="ScrollIntoView" @onclick="Scroll">ScrollIntoView</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///  ........
        /// </SfGrid>
        /// @code{
        ///    SfGrid<Order> grid;
        ///    private async Task Scroll()
        ///    {
        ///        await grid.ScrollIntoViewAsync(2,3);
        ///    }
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task ScrollIntoViewAsync(int columnIndex = -1, int rowIndex = -1, int rowHeight = -1)
        {
            if (VirtualScrollModule != null)
            {
                VirtualScrollModule.CurrentRowIndex = rowIndex;
            }
            await InvokeMethod("sfBlazor.Grid.scrollIntoView", new object[] { DataId, columnIndex, rowIndex, rowHeight }).ConfigureAwait(true);
        }

        /// <summary>
        /// Gets the value of a cell in the grid based on row and column indices.
        /// </summary>
        /// <param name="rowIndex">The index of the row from which to retrieve the cell value.</param>
        /// <param name="columnIndex">The index of the column from which to retrieve the cell value.</param>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task"/> representing the asynchronous operation that returns the cell value as an object for the specified row and column indices.
        /// </returns>
        /// <remarks>
        /// This method retrieves the formatted value of a cell from the current view data.
        /// When performing grid actions such as sorting, filtering, and grouping, this method returns the current visible data based on the provided indexes
        /// When binding a Foreignkey column in the grid, this method returns the Foreignkey value and not the Foreignkey field.
        /// The column index represents the index of the column in the Grid UI, not the column order defined in the columns.
        /// If the provided row or column index is not valid, null is returned.
        /// </remarks>
        ///<example>
        ///<code>
        ///<![CDATA[ 
        ///<button @onclick="GetCellValue">GetCellValue</button>
        ///<SfGrid @ref="grid" DataSource="@Orders">
        /// ........
        ///</SfGrid>
        ///@code{
        ///      SfGrid<Order> grid;        
        ///      private async Task GetCellValue()        
        ///      {        
        ///         await grid.GetCellValueByIndexAsync(2,3);    
        ///      }      
        ///    }        
        ///]]>   
        ///</code>
        ///</example>
        public async Task<object> GetCellValueByIndexAsync(int rowIndex, int columnIndex)
        {
            object? value = null;
            var dataRows = (GroupSettings!.Columns != null) ? Rows.Where((R => R.RowType == "Data")).ToList() : Rows;

            if (Rows != null && rowIndex < Rows.Count && rowIndex >= 0 && columnIndex >= 0)
            {
                var dataCells = dataRows[rowIndex].Cells.Where(C => C.CellType == CellType.Data).ToList();
                dataCells = dataCells.Where(R => R.Visible == true).ToList();
                value = GridUtils.GetCellValue(dataCells[columnIndex], dataRows[rowIndex]);
            }
            return value!;
        }

        /// <summary>
        /// Gets the current <see cref="EditContext"/> instance used in the Grid editing operations such as "Add" and "Edit".
        /// </summary>
        /// <returns>
        /// A <see cref="System.Threading.Tasks.Task{EditContext}"/> representing the asynchronous operation. 
        /// The task result contains the <see cref="EditContext"/> instance, which provides information about the edit operation 
        /// including modified field states and validation messages. Returns <c>null</c> if editing is not enabled.
        /// </returns>
        /// <remarks>
        /// This method retrieves the current <see cref="EditContext"/> associated with the Grid editing context. 
        /// It is useful when you need to access edit tracking details programmatically, such as performing custom validation 
        /// or integrating with external forms.
        /// </remarks>
        /// <example>
        /// The following example shows how to retrieve the <see cref="EditContext"/> from the Grid:
        /// <code>
        /// <![CDATA[ 
        /// <button @onclick="GetEditContext">GetEditContext</button>
        /// <SfGrid @ref="grid" DataSource="@Orders">
        ///     ........
        /// </SfGrid>
        /// @code {
        ///     SfGrid<Order> grid;        
        ///     
        ///     private async Task GetEditContext()        
        ///     {        
        ///         await grid.GetEditContextAsync();    
        ///     }      
        /// }
        /// ]]>
        /// </code>
        /// </example>
        public async Task<EditContext> GetEditContextAsync()
        {
            return EditModule!.EditContext!;
        }

        /// <summary>
        /// Load the previously saved state of the Grid.
        /// </summary>
        /// <param name="properties">Contains the saved properties as string value.</param>
        /// <returns>Task.</returns>
        /// <remarks>
        /// This method is primarly used to load and refresh the grid with already saved state.
        /// The state can be served from any source such as window.localStorage, DB etc.
        /// </remarks>
        public async Task SetPersistDataAsync(string properties)
        {
            await HandleSetPersistData(properties).ConfigureAwait(true);
        }

        /// <summary>
        /// Resets the state of the Grid.
        /// </summary>
        /// <returns>void.</returns>
        /// <remarks>This method will clear the current state and refreshes the grid with original state given
        /// delcaratively. If EnablePersistence is used then this will clear the state which is stored in window.localStorage too.</remarks>
        public async Task ResetPersistDataAsync()
        {
            await HandleResetPersistData().ConfigureAwait(true);
        }

        /// <summary>
        /// Specifies the grid templates.
        /// </summary>
        GridTemplates? IGrid.GridTemplates { get; set; }

        /// <summary>
        /// Updates the child property of the grid based on the specified key and value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateChildProperties(string key, object value)
        {
            UpdateChildProperty(key, value).ConfigureAwait(false);
        }

        internal async Task UpdateChildProperty(string key, object value)
        {
            switch (key)
            {
                case nameof(Columns):
                    Columns = _columns = (List<GridColumn>)value;
                    break;
                case nameof(FrozenColumns):
                    FrozenColumns = _frozenColumns = (int)value;
                    break;
                case nameof(Aggregates):
                    Aggregates = _aggregates = (List<GridAggregate>)value;
                    break;
                case nameof(FilterSettings):
                    var filterSettings = value == null ? await GridFilterSettings.Initialize(this).ConfigureAwait(true) : (GridFilterSettings)value;
                    FilterSettings = _filterSettings = filterSettings;
                    break;
                case nameof(SortSettings):
                    var sortSettings = value == null ? await GridSortSettings.Initialize(this).ConfigureAwait(true) : (GridSortSettings)value;
                    SortSettings = _sortSettings = sortSettings;
                    break;
                case nameof(GroupSettings):
                    var groupSettings = value == null ? await GridGroupSettings.Initialize(this).ConfigureAwait(true) : (GridGroupSettings)value;
                    GroupSettings = _groupSettings = groupSettings;
                    break;
                case nameof(EditSettings):
                    var editSettings = value == null ? await GridEditSettings.Initialize(this).ConfigureAwait(true) : (GridEditSettings)value;
                    EditSettings = _editSettings = editSettings;
                    break;
                case nameof(PageSettings):
                    var pageSettings = value == null ? await GridPageSettings.Initialize(this).ConfigureAwait(true) : (GridPageSettings)value;
                    PageSettings = _pageSettings = pageSettings;
                    EnsurePagerDropdown();
                    break;
                case nameof(RowDropSettings):
                    var rowDropSettings = value == null ? await GridRowDropSettings.Initialize(this).ConfigureAwait(true) : (GridRowDropSettings)value;
                    RowDropSettings = _rowDropSettings = rowDropSettings;
                    break;
                case nameof(SearchSettings):
                    var searchSettings = value == null ? await GridSearchSettings.Initialize(this).ConfigureAwait(true) : (GridSearchSettings)value;
                    SearchSettings = _searchSettings = searchSettings;
                    break;
                case nameof(SelectionSettings):
                    var selectionSettings = value == null ? await GridSelectionSettings.Initialize(this).ConfigureAwait(true) : (GridSelectionSettings)value;
                    SelectionSettings = _selectionSettings = selectionSettings;
                    break;
                case nameof(TextWrapSettings):
                    var textWrapSettings = value == null ? await GridTextWrapSettings.Initialize(this).ConfigureAwait(true) : (GridTextWrapSettings)value;
                    TextWrapSettings = _textWrapSettings = textWrapSettings;
                    break;
                case nameof(ColumnChooserSettings):
                    var columnChooserSettings = value == null ? await GridColumnChooserSettings.Initialize(this).ConfigureAwait(true) : (GridColumnChooserSettings)value;
                    ColumnChooserSettings = _columnChooserSettings = columnChooserSettings;
                    break;
                case nameof(KeySettings):
                    var keySettings = value == null ? await GridKeySettings.Initialize(this).ConfigureAwait(true) : (GridKeySettings)value;
                    KeySettings = _keySettings = keySettings;
                    break;
                case nameof(InfiniteScrollSettings):
                    var infiniteScrollSettings = value == null ? await GridInfiniteScrollSettings.Initialize(this).ConfigureAwait(true) : (GridInfiniteScrollSettings)value;
                    InfiniteScrollSettings = _infiniteScrollSettings = infiniteScrollSettings;
                    break;
                case nameof(FindSettings):
                    var findSettings = value == null ? await GridFindSettings.Initialize(this).ConfigureAwait(true) : (GridFindSettings)value;
                    FindSettings = _findSettings = findSettings;
                    break;
                case nameof(WebMcpSettings):
                    var webMcpSettings = value == null ? new GridWebMcpSettings() : (GridWebMcpSettings)value;
                    WebMcpSettings = _webMcpSettings = webMcpSettings;
                    HasWebMcpSettingsChanges = true;
                    break;
            }

            DirectParameters.AddOrUpdateItem(key, value!);
        }

        // Determines if the column should be rendered based on visibility, hidden column settings, or grouping rules.
        internal bool ShouldRenderColumn(bool isVisible, string field)
        {
            return ShouldRenderHiddenColumns || isVisible || (AllowGrouping && (GroupSettings?.Columns?.Contains(field) ?? false));
        }

        #region WebMCP Integration Methods

        /// <summary>
        /// Gets the WebMCP tools available for this grid.
        /// </summary>
        /// <param name="toolNames">
        /// A collection of tool names to filter by, or <see langword="null"/> to
        /// return all available WebMCP tools.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> that resolves to an
        /// <see cref="IReadOnlyList{T}"/> of <see cref="IWebMcpTool"/> instances
        /// representing the matching WebMCP tool definitions.
        /// </returns>
        /// <remarks>
        /// <para>
        /// This method returns the effective WebMCP tool catalog associated with the
        /// grid, including built-in and custom tool definitions that have been
        /// registered for this instance.
        /// </para>
        /// <para>
        /// When <paramref name="toolNames"/> is specified, only tools whose names
        /// match the supplied values are returned. When it is
        /// <see langword="null"/>, all available tools are returned.
        /// </para>
        /// <para>
        /// If WebMCP is not initialized, the grid does not support WebMCP, or no
        /// adapter is available, an empty collection is returned.
        /// </para>
        /// </remarks>
        /// <example>
        /// <![CDATA[
        /// <button @onclick="GetToolsAsync">
        ///     Get Available WebMCP Tools
        /// </button>
        ///
        /// <SfGrid TValue="OrderData"
        ///         DataSource="@Orders"
        ///         EnableWebMcp="true"
        ///         @ref="Grid">
        /// </SfGrid>
        ///
        /// @code {
        ///     private SfGrid<OrderData>? Grid;
        ///
        ///     private async Task GetToolsAsync()
        ///     {
        ///         if (Grid == null)
        ///         {
        ///             return;
        ///         }
        ///
        ///         var tools = await Grid.GetWebMcpToolsAsync();
        ///     }
        /// }
        /// ]]>
        /// </example>
        public async Task<IReadOnlyList<IWebMcpTool>> GetWebMcpToolsAsync(IEnumerable<string>? toolNames = null)
        {
            return await Task.FromResult(_webMcpAdapter == null
                ? new List<IWebMcpTool>()
                : WebMcpAdapter<TValue>.GetTools(toolNames)).ConfigureAwait(false);
        }


        #endregion

        #region Undo/Redo Public API Methods

        /// <summary>
        /// Reverts the most recent batch edit operation and updates the grid UI to reflect the previous state.
        /// </summary>
        /// <returns>A task that represents the asynchronous undo operation.</returns>
        /// <remarks>
        /// This API is only effective when the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.EnableUndoRedo"/> property is
        /// <c>true</c> and the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set to
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>. If a cell is currently in edit mode, the in-progress edit is
        /// <b>closed and discarded</b> before the undo is applied — staged batch changes (added rows, dirty cells, edited data)
        /// are preserved and other staged edits are not pushed onto the undo stack. The pending cell value is <b>not</b> committed
        /// and therefore is <b>not</b> recorded on the undo stack. The <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnUndo"/>
        /// event is raised before the operation and the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnUndoCompleted"/>
        /// event is raised after the grid UI has been refreshed.
        /// <para>
        /// This method is a silent <b>no-op</b> when the undo stack is empty or Undo/Redo is disabled. It returns successfully and does
        /// not throw. Callers that need to detect a no-op should observe the absence of <c>OnUndo</c>/<c>OnUndoCompleted</c> events or
        /// inspect <see cref="Syncfusion.Blazor.Grids.GridEditSettings.EnableUndoRedo"/>. Subscribers to the
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBatchCancel"/> event will see one notification fire as a side effect
        /// of closing the active cell editor; that notification is incidental to the undo and should not be used to gate the undo
        /// operation.
        /// </para>
        /// </remarks>
        public async Task UndoAsync()
        {
            if (IsEdit && EditModule != null)
            {
                await EditModule.CloseEdit(true).ConfigureAwait(true);
            }

            if (UndoRedoManager != null)
            {
                await UndoRedoManager.PerformUndoAsync().ConfigureAwait(true);
            }
            EventAggregator?.Trigger("UndoRedoStackChanged", null!);
        }

        /// <summary>
        /// Reapplies the most recently undone batch edit operation and updates the grid UI accordingly.
        /// </summary>
        /// <returns>A task that represents the asynchronous redo operation.</returns>
        /// <remarks>
        /// This API is only effective when the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.EnableUndoRedo"/> property is
        /// <c>true</c> and the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set to
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>. If a cell is currently in edit mode, the in-progress edit is
        /// <b>closed and discarded</b> before the redo is applied — staged batch changes (added rows, dirty cells, edited data)
        /// are preserved and other staged edits are not pushed onto the undo stack. The pending cell value is <b>not</b> committed
        /// and therefore is <b>not</b> recorded on the undo stack. The <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRedo"/>
        /// event is raised before the operation and the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRedoCompleted"/>
        /// event is raised after the grid UI has been refreshed.
        /// <para>
        /// This method is a silent <b>no-op</b> when the redo stack is empty or Undo/Redo is disabled. It returns successfully and does
        /// not throw. Callers that need to detect a no-op should observe the absence of <c>OnRedo</c>/<c>OnRedoCompleted</c> events or
        /// inspect <see cref="Syncfusion.Blazor.Grids.GridEditSettings.EnableUndoRedo"/>. Subscribers to the
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnBatchCancel"/> event will see one notification fire as a side effect
        /// of closing the active cell editor; that notification is incidental to the redo and should not be used to gate the redo
        /// operation.
        /// </para>
        /// </remarks>
        public async Task RedoAsync()
        {
            if (IsEdit && EditModule != null)
            {
                await EditModule.CloseEdit(true).ConfigureAwait(true);
            }

            if (UndoRedoManager != null)
            {
                await UndoRedoManager.PerformRedoAsync().ConfigureAwait(true);
            }
            EventAggregator?.Trigger("UndoRedoStackChanged", null!);
        }

        /// <summary>
        /// Undoes all recorded batch operations sequentially until the undo stack is empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous undo-all operation.</returns>
        /// <remarks>
        /// This API moves every action from the undo stack to the redo stack. The grid UI is updated for each action in sequence.
        /// The <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnUndo"/> and
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnUndoCompleted"/> events fire <b>once per action</b>
        /// consumed by the loop, not once for the bulk call; audit code therefore receives one invocation per undone action.
        /// This API is only effective when the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.EnableUndoRedo"/> property is
        /// <c>true</c> and the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set to
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>.
        /// <para>
        /// This method is a silent <b>no-op</b> when the undo stack is already empty or Undo/Redo is disabled; no events are raised and no
        /// exception is thrown.
        /// </para>
        /// </remarks>
        public async Task UndoAllAsync()
        {
            if (UndoRedoManager != null)
            {
                await UndoRedoManager.UndoAllAsync().ConfigureAwait(true);
            }
            EventAggregator?.Trigger("UndoRedoStackChanged", null!);
        }

        /// <summary>
        /// Redoes all undone batch operations sequentially until the redo stack is empty.
        /// </summary>
        /// <returns>A task that represents the asynchronous redo-all operation.</returns>
        /// <remarks>
        /// This API moves every action from the redo stack back to the undo stack. The grid UI is updated for each action in
        /// sequence. The <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRedo"/> and
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRedoCompleted"/> events fire <b>once per action</b>
        /// consumed by the loop, not once for the bulk call; audit code therefore receives one invocation per redone action.
        /// This API is only effective when the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.EnableUndoRedo"/>
        /// property is <c>true</c> and the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is set to
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>.
        /// <para>
        /// This method is a silent <b>no-op</b> when the redo stack is already empty or Undo/Redo is disabled; no events are raised and no
        /// exception is thrown.
        /// </para>
        /// </remarks>
        public async Task RedoAllAsync()
        {
            if (UndoRedoManager != null)
            {
                await UndoRedoManager.RedoAllAsync().ConfigureAwait(true);
            }
            EventAggregator?.Trigger("UndoRedoStackChanged", null!);
        }

        /// <summary>
        /// Clears both the Undo and Redo stacks and resets the Undo/Redo history to a clean state.
        /// </summary>
        /// <returns>A task that represents the asynchronous clear operation.</returns>
        /// <remarks>
        /// Any pending Undo or Redo operation is discarded.
        /// None of the <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnUndo"/>,
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnUndoCompleted"/>,
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRedo"/>, or
        /// <see cref="Syncfusion.Blazor.Grids.GridEvents{TValue}.OnRedoCompleted"/> events are raised by this call.
        /// <para>
        /// This method is always safe to call: it is a silent <b>no-op</b> when the Undo/Redo manager is not initialized and never
        /// throws.
        /// </para>
        /// </remarks>
        public async Task ClearUndoRedoAsync()
        {
            if (UndoRedoManager != null)
            {
                UndoRedoManager.Clear();
                EventAggregator?.Trigger("UndoRedoStackChanged", null!);
            }
        }

        #endregion

    }
}
