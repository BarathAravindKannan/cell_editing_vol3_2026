using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Represents the change applied to one cell as part of an Undo/Redo action. (Internal use only)
    /// </summary>
    /// <typeparam name="T">The type of data in the row that contains the changed cell.</typeparam>
    /// <remarks>
    /// This type is used internally by the Undo/Redo manager to track cell-level changes for 
    /// <see cref="UndoRedoActionType.CellEdit"/>, <see cref="UndoRedoActionType.Paste"/>, and 
    /// <see cref="UndoRedoActionType.AutoFill"/> actions. Row add and row delete actions track the 
    /// complete row data instead of individual cell changes.
    /// </remarks>
    internal class CellChange<T>
    {
        /// <summary>
        /// Gets or sets the zero-based index of the row that contains the changed cell.
        /// </summary>
        /// <value>
        /// The zero-based row index. The default value is <c>-1</c>.
        /// </value>
        public int RowIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the zero-based index of the column that contains the changed cell.
        /// </summary>
        /// <value>
        /// The zero-based column index within the visible leaf columns. The default value is <c>-1</c>.
        /// </value>
        /// <remarks>
        /// For a <see cref="UndoRedoActionType.CellEdit"/>, this is the column index of the edited cell.
        /// For a <see cref="UndoRedoActionType.Paste"/> or <see cref="UndoRedoActionType.AutoFill"/>
        /// action, each <see cref="CellChange{T}"/> stores the index of one affected cell. This property
        /// is not used by row add or row delete actions because those actions do not create a
        /// <see cref="CellChange{T}"/> instance.
        /// </remarks>
        public int ColumnIndex { get; set; } = -1;

        /// <summary>
        /// Gets or sets the field name of the column that identifies the cell in the data model.
        /// </summary>
        /// <value>
        /// A string matching the <see cref="Syncfusion.Blazor.Grids.GridColumn.Field"/> property.
        /// The default value is <c>null</c>.
        /// </value>
        /// <remarks>
        /// This property is populated for cell edit, paste, and autofill changes. It must be non-empty
        /// for a change to be buffered during a paste or autofill operation.
        /// </remarks>
        public string? FieldName { get; set; }

        /// <summary>
        /// Gets or sets the value of the cell before the change was applied.
        /// </summary>
        /// <value>
        /// The previous cell value. The default value is <c>null</c>.
        /// </value>
        public object? OldValue { get; set; }

        /// <summary>
        /// Gets or sets the value of the cell after the change was applied.
        /// </summary>
        /// <value>
        /// The new cell value. The default value is <c>null</c>.
        /// </value>
        public object? NewValue { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="Syncfusion.Blazor.Grids.GridColumn"/> definition for the changed cell.
        /// </summary>
        /// <value>
        /// The <see cref="Syncfusion.Blazor.Grids.GridColumn"/> associated with the cell. The default
        /// value is <c>null</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// This property is available for cell edit, paste, and autofill changes. Row add and row delete
        /// actions do not have a cell column. The undo/redo event arguments expose only <c>ColumnIndex</c>,
        /// which is <c>-1</c> for row add, row delete, paste, and autofill actions.
        /// </para>
        /// <para>
        /// The captured <see cref="GridColumn"/> reference is a snapshot taken at recording time and is
        /// <strong>not guaranteed to remain valid</strong>. The column may be reordered, hidden, or removed
        /// from <see cref="SfGrid{T}.Columns"/> before the action is undone or redone; do not dereference
        /// this property without a <see langword="null"/> check. The stable identifier for the column is
        /// <see cref="FieldName"/> (matched against <see cref="GridColumn.Field"/>); prefer
        /// <see cref="ColumnIndex"/> or <see cref="FieldName"/> over this reference when resolving the
        /// target column at undo or redo time.
        /// </para>
        /// </remarks>
        public GridColumn? Column { get; set; }
    }

    /// <summary>
    /// Represents a single action recorded in the Undo/Redo history.
    /// </summary>
    /// <typeparam name="T">The type of data in the grid rows that the action targets.</typeparam>
    /// <remarks>
    /// This type is used internally by the Undo/Redo manager to store and replay action history.
    /// Customers interact with undo/redo through public event args classes and API methods, not directly with this type.
    /// </remarks>
    internal class UndoRedoAction<T>
    {
        /// <summary>
        /// Gets the kind of grid mutation this action represents in the Undo/Redo history.
        /// </summary>
        /// <value>
        /// One of the <see cref="UndoRedoActionType"/> enumeration values that identifies the type of grid operation.
        /// The default value is <see cref="UndoRedoActionType.None"/>, indicating an uninitialized or empty action.
        /// </value>
        internal UndoRedoActionType ActionType { get; set; }

        /// <summary>
        /// Gets a monotonically increasing sequence number assigned when the action is recorded.
        /// </summary>
        /// <value>
        /// An integer that uniquely and sequentially identifies this action. The default value is <c>0</c>.
        /// </value>
        internal int SequenceNumber { get; set; }

        /// <summary>
        /// Gets the cell-level change details for a single cell edit action.
        /// </summary>
        /// <value>
        /// A <see cref="CellChange{T}"/> instance for <see cref="UndoRedoActionType.CellEdit"/> actions; <c>null</c> otherwise.
        /// </value>
        internal CellChange<T>? CellChange { get; set; }

        /// <summary>
        /// Gets a snapshot of the row data for add or delete operations.
        /// </summary>
        /// <value>
        /// A <typeparamref name="T"/> instance for <see cref="UndoRedoActionType.RowAdd"/> and <see cref="UndoRedoActionType.RowDelete"/> actions; <c>null</c> otherwise.
        /// </value>
        internal T? RowData { get; set; }

        /// <summary>
        /// Gets the row index for row-level actions.
        /// </summary>
        /// <value>
        /// The zero-based index of the affected row for <see cref="UndoRedoActionType.RowAdd"/> and <see cref="UndoRedoActionType.RowDelete"/> actions; <c>null</c> otherwise.
        /// </value>
        internal int? RowIndex { get; set; }

        /// <summary>
        /// Gets the insertion position for row add actions.
        /// </summary>
        /// <value>
        /// A <see cref="NewRowPosition"/> value for <see cref="UndoRedoActionType.RowAdd"/> actions; <c>null</c> otherwise.
        /// </value>
        internal NewRowPosition? RowPosition { get; set; }

        /// <summary>
        /// Gets the cell changes for multi-cell batch operations.
        /// </summary>
        /// <value>
        /// A list of <see cref="CellChange{T}"/> instances for <see cref="UndoRedoActionType.Paste"/> and <see cref="UndoRedoActionType.AutoFill"/> actions; <c>null</c> otherwise.
        /// </value>
        internal List<CellChange<T>>? PreviousValues { get; set; }

        /// <summary>
        /// Gets the row snapshots for multi-row operations.
        /// </summary>
        /// <value>
        /// A list of <typeparamref name="T"/> instances for multi-row operations; <c>null</c> for single-action operations.
        /// </value>
        internal List<T>? PreviousRows { get; set; }
    }
}
