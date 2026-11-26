namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Represents the details of a selected cell returned by <see cref="SfGrid{TValue}.GetSelectedCellsAsync"/>.
    /// </summary>
    /// <typeparam name="TValue">The type of the grid data.</typeparam>
    public class GridSelectedCell<TValue>
    {
        /// <summary>
        /// Gets the index of the row that contains the selected cell.
        /// </summary>
        public int RowIndex { get; internal set; }

        /// <summary>
        /// Gets the index of the selected cell.
        /// </summary>
        public int CellIndex { get; internal set; }

        /// <summary>
        /// Gets the column that the selected cell belongs to.
        /// </summary>
        public GridColumn? Column { get; internal set; }

        /// <summary>
        /// Gets the field name of the column that the selected cell belongs to.
        /// </summary>
        public string? Field { get; internal set; }

        /// <summary>
        /// Gets the value of the selected cell.
        /// </summary>
        public object? Value { get; internal set; }

        /// <summary>
        /// Gets the foreign key display value of the selected cell. It is <c>null</c> for non foreign key columns.
        /// </summary>
        public string? ForeignKeyValue { get; internal set; }

        /// <summary>
        /// Gets the data of the row that contains the selected cell.
        /// </summary>
        public TValue? Data { get; internal set; }
    }
}
