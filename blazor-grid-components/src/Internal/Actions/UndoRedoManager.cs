

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Manages undo and redo operations for grid edit actions.
    /// Maintains separate stacks for undo and redo with configurable size limits.
    /// Also handles applying undo/redo actions to the grid.
    /// </summary>
    /// <typeparam name="T">The type of data in the grid rows.</typeparam>
    internal class UndoRedoManager<T>
    {
        private LinkedList<UndoRedoAction<T>> undoStack = new LinkedList<UndoRedoAction<T>>();
        private LinkedList<UndoRedoAction<T>> redoStack = new LinkedList<UndoRedoAction<T>>();
        private int maxStackSize = 20;
        private bool isEnabled;
        private bool isStackLimitInitialized;  // Flag to ensure limit is set only once
        private int sequenceCounter;
        private SfGrid<T>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of actions to keep in the undo/redo stacks.
        /// Default value is 20. When the stack exceeds this limit, the oldest action is removed.
        /// Marked internal because no external caller reads or writes it (the only mutator is
        /// <see cref="Enable(int)"/>), keeping the public surface minimal.
        /// </summary>
        internal int MaxStackSize
        {
            get => maxStackSize;
            set => maxStackSize = Math.Max(1, value);
        }

        internal void SetStackLimit(int stackLimit)
        {
            
            MaxStackSize = stackLimit;
            if (!isEnabled)
            {
                return;
            }
            while (undoStack.Count > maxStackSize && undoStack.Last != null)
            {
                undoStack.RemoveLast();
            }
            if (activeBatch != null && activeBatch.Count > maxStackSize)
            {
                var excess = activeBatch.Count - maxStackSize;
                activeBatch.RemoveRange(0, excess);
            }

            TriggerUndoRedoStackChanged();
        }
        internal void InitializeStackLimit()
        {
            
            if (isStackLimitInitialized)
            {
                return;
            }

            if (Parent?.EditSettings != null)
            {
                SetStackLimit(Parent.EditSettings.UndoRedoLimit);
            }
            isStackLimitInitialized = true;
        }

        internal void UpdateMaxStackSize(int newLimit)
        {
            SetStackLimit(newLimit);
        }

        /// <summary>
        /// Gets a value indicating whether undo/redo is currently enabled.
        /// </summary>
        internal bool IsEnabled => isEnabled;

        /// <summary>
        /// Gets the number of actions available in the undo stack.
        /// </summary>
        internal int UndoCount => undoStack.Count;

        /// <summary>
        /// Gets the number of actions available in the redo stack.
        /// </summary>
        internal int RedoCount => redoStack.Count;

        /// <summary>
        /// Returns the most recent undo action without removing it from the stack.
        /// Used to raise Before-events before the actual undo operation executes.
        /// </summary>
        /// <returns>The next action that would be undone, or null if the undo stack is empty.</returns>
        internal UndoRedoAction<T>? PeekUndo() => undoStack.Count > 0 ? undoStack.First?.Value : null;

        /// <summary>
        /// Returns the most recent redo action without removing it from the stack.
        /// Used to raise Before-events before the actual redo operation executes.
        /// </summary>
        /// <returns>The next action that would be redone, or null if the redo stack is empty.</returns>
        internal UndoRedoAction<T>? PeekRedo() => redoStack.Count > 0 ? redoStack.First?.Value : null;

        /// <summary>
        /// Gets a value indicating whether a batch operation (AutoFill or Paste) is currently active.
        /// Used by SaveCell to determine whether to buffer cell changes or record them immediately.
        /// </summary>
        internal bool IsActiveBatchPending => activeBatch != null;

        /// <summary>
        /// Gets the type of the currently active batch (AutoFill or Paste).
        /// Only meaningful when IsActiveBatchPending is true.
        /// </summary>
        internal UndoRedoActionType ActiveBatchType => activeBatchType;

        /// <summary>
        /// Initializes a new instance of the UndoRedoManager class.
        /// </summary>
        /// <param name="parent">The parent SfGrid instance for accessing grid operations (optional).</param>
        public UndoRedoManager(SfGrid<T>? parent = null)
        {
            Parent = parent;
            Enable(parent?.EditSettings?.UndoRedoLimit ?? 20);
        }

        #region Helper Methods for Recording Actions

        /// <summary>
        /// Validates if an undo/redo action should be recorded based on guard conditions.
        /// Centralizes all guard logic in one place for consistency and maintainability.
        /// </summary>
        /// <param name="expectedMode">The expected edit mode for validation. Defaults to Batch.</param>
        /// <returns>True if the action should be recorded, false otherwise.</returns>
        internal bool ShouldRecordUndoRedoAction(EditMode expectedMode = EditMode.Batch)
        {
            if (Parent?.EditSettings?.EnableUndoRedo != true)
                return false;

            if (expectedMode == EditMode.Batch && Parent.EditSettings?.Mode != EditMode.Batch)
                return false;

            if (isEnabled == false)
                return false;

            return true;
        }

        /// <summary>
        /// Records a cell edit action in the undo/redo stack.
        /// Encapsulates the logic for creating and recording cell edit actions.
        /// </summary>
        /// <param name="rowIndex">The index of the row containing the cell.</param>
        /// <param name="columnIndex">The index of the column being edited.</param>
        /// <param name="fieldName">The field name of the cell being edited.</param>
        /// <param name="oldValue">The previous value before editing.</param>
        /// <param name="newValue">The new value after editing.</param>
        /// <param name="column">The GridColumn object associated with the cell.</param>
        internal void RecordCellEditAction(int rowIndex, int columnIndex, string? fieldName, 
            object? oldValue, object? newValue, GridColumn? column)
        {
            if (!ShouldRecordUndoRedoAction(EditMode.Batch))
                return;

            var cellChange = new CellChange<T>
            {
                RowIndex = rowIndex,
                ColumnIndex = columnIndex,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                Column = column
            };

            var action = new UndoRedoAction<T>
            {
                ActionType = UndoRedoActionType.CellEdit,
                CellChange = cellChange
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        /// <summary>
        /// Records or buffers a cell change based on the current batch state.
        /// If a batch is active (Paste/AutoFill), the change is buffered for later atomic recording.
        /// Otherwise, if the values differ, the change is immediately recorded as a CellEdit action.
        /// This method centralizes the decision logic to avoid duplication in GridJSInteropAdaptor.
        /// </summary>
        /// <param name="rowIndex">The index of the row containing the changed cell.</param>
        /// <param name="columnIndex">The index of the column containing the changed cell.</param>
        /// <param name="fieldName">The field name of the changed cell.</param>
        /// <param name="oldValue">The previous value before the change.</param>
        /// <param name="newValue">The new value after the change.</param>
        /// <param name="column">The GridColumn object associated with the cell.</param>
        internal void RecordOrBufferCellChange(int rowIndex, int columnIndex, string? fieldName, 
            object? oldValue, object? newValue, GridColumn? column)
        {
            if (IsBatchActive)
            {
                // During batch operations (Paste/AutoFill), buffer the change for atomic recording
                BufferCellChange(rowIndex, columnIndex, fieldName, oldValue, newValue, column);
            }
            else if (GridUtils.CompareValues<object>(oldValue!, newValue!))
            {
                // Outside batch, only record if values are different
                RecordCellEditAction(rowIndex, columnIndex, fieldName, oldValue, newValue, column);
            }
        }

        /// <summary>
        /// Records a row addition action in the undo/redo stack.
        /// Encapsulates the logic for creating and recording row add actions.
        /// </summary>
        /// <param name="rowData">The data of the newly added row.</param>
        /// <param name="rowIndex">The index at which the row was added.</param>
        /// <param name="newRowPosition">The position where the new row should be added (Top/Bottom).</param>
        internal void RecordRowAddAction(T? rowData, int rowIndex, NewRowPosition newRowPosition)
        {
            if (!ShouldRecordUndoRedoAction())
                return;

            if (rowData == null)
                return;

            var action = new UndoRedoAction<T>
            {
                ActionType = UndoRedoActionType.RowAdd,
                RowData = rowData,
                RowIndex = rowIndex >= 0 ? rowIndex : -1,
                RowPosition = newRowPosition
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        /// <summary>
        /// Records a row deletion action in the undo/redo stack.
        /// Encapsulates the logic for creating and recording row delete actions.
        /// </summary>
        /// <param name="rowData">The data of the deleted row (should contain edited data if available).</param>
        /// <param name="rowIndex">The index of the deleted row.</param>
        internal void RecordRowDeleteAction(T? rowData, int rowIndex)
        {
            if (!ShouldRecordUndoRedoAction())
                return;

            if (rowData == null)
                return;

            var action = new UndoRedoAction<T>
            {
                ActionType = UndoRedoActionType.RowDelete,
                RowData = rowData,
                RowIndex = rowIndex >= 0 ? rowIndex : -1
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        private List<CellChange<T>>? activeBatch = null!;

        private UndoRedoActionType activeBatchType = UndoRedoActionType.None;

        /// <summary>
        /// Begins a multi-cell batch (AutoFill or Paste). While a batch is active,
        /// BufferCellChange accumulates cell deltas instead of pushing per-cell
        /// actions to the undo stack. The batch must be committed or discarded
        /// to keep the undo stack consistent.
        /// </summary>
        /// <param name="batchType">The type of batch being started (AutoFill or Paste).</param>
        internal void BeginBatch(UndoRedoActionType batchType)
        {
            if (batchType != UndoRedoActionType.AutoFill && batchType != UndoRedoActionType.Paste)
            {
                return;
            }
            activeBatch = new List<CellChange<T>>();
            activeBatchType = batchType;
        }

        /// <summary>
        /// Adds a single cell change (oldValue -> newValue) to the currently active batch.
        /// Has no effect when no batch is active, when the action would not be recorded,
        /// or when the value did not actually change.
        /// </summary>
        /// <param name="rowIndex">Row index of the cell.</param>
        /// <param name="columnIndex">Column index of the cell.</param>
        /// <param name="fieldName">Field name of the column.</param>
        /// <param name="oldValue">Value before the cell was changed.</param>
        /// <param name="newValue">Value after the cell was changed.</param>
        /// <param name="column">Column definition for the cell.</param>
        internal void BufferCellChange(int rowIndex, int columnIndex, string? fieldName,
            object? oldValue, object? newValue, GridColumn? column)
        {
            if (activeBatch == null)
            {
                return;
            }

            if (!ShouldRecordUndoRedoAction(EditMode.Batch))
            {
                return;
            }

            if (string.IsNullOrEmpty(fieldName))
            {
                return;
            }
            if (!GridUtils.CompareValues<object>(oldValue!, newValue!))
            {
                return;
            }

            activeBatch.Add(new CellChange<T>
            {
                RowIndex = rowIndex,
                ColumnIndex = columnIndex,
                FieldName = fieldName,
                OldValue = oldValue,
                NewValue = newValue,
                Column = column
            });
        }

        /// <summary>
        /// Commits the currently active batch as a single atomic UndoRedoAction
        /// (AutoFill or Paste) on the undo stack. No-op if the batch is empty
        /// or no batch is active.
        /// </summary>
        internal void CommitBatch()
        {
            var batch = activeBatch;
            var type = activeBatchType;
            activeBatch = null;
            activeBatchType = UndoRedoActionType.None;

            if (batch == null || batch.Count == 0)
            {
                return;
            }

            if (!ShouldRecordUndoRedoAction(EditMode.Batch))
            {
                return;
            }

            var action = new UndoRedoAction<T>
            {
                ActionType = type,
                PreviousValues = batch
            };

            RecordAction(action);
            TriggerUndoRedoStackChanged();
        }

        /// <summary>
        /// Discards the currently active batch without recording anything.
        /// Safe to call when no batch is active.
        /// </summary>
        internal void DiscardBatch()
        {
            activeBatch = null;
            activeBatchType = UndoRedoActionType.None;
        }

        /// <summary>
        /// Returns true if a multi-cell batch is currently in progress.
        /// </summary>
        internal bool IsBatchActive => activeBatch != null;

        /// <summary>
        /// Triggers the UndoRedoStackChanged event to notify subscribers of stack state changes.
        /// Centralizes event triggering logic for consistency.
        /// </summary>
        internal void TriggerUndoRedoStackChanged()
        {
            if (Parent?.EventAggregator != null)
            {
                Parent.EventAggregator.Trigger("UndoRedoStackChanged", null!);
            }
        }

        private static (UndoRedoActionType ActionType, int RowIndex, string? FieldName, object? OldValue, object? NewValue, int ColumnIndex, T? RowData) BuildEventArgsData(UndoRedoAction<T> action)
        {
            int rowIndex = action.RowIndex ?? -1;
            string? fieldName = null;
            object? oldValue = null;
            object? newValue = null;
            int columnIndex = -1;
            T? rowData = default;


            if (action.ActionType == UndoRedoActionType.CellEdit && action.CellChange != null)
            {
                var change = action.CellChange;
                rowIndex = change.RowIndex >= 0 ? change.RowIndex : rowIndex;
                columnIndex = change.ColumnIndex;
                fieldName = change.FieldName;
                oldValue = change.OldValue;
                newValue = change.NewValue;
            }
            else
            {

                oldValue = action.RowData;
                newValue = action.RowData;
                rowData = action.RowData;
            }

            return (action.ActionType, rowIndex, fieldName, oldValue, newValue, columnIndex, rowData);
        }

        /// <summary>
        /// Fires the OnUndo event before an undo operation is executed.
        /// Events are informational only and cannot modify or cancel the operation.
        /// </summary>
        /// <param name="action">The action that is about to be undone.</param>
        /// <returns>A task representing the asynchronous event invocation.</returns>
        internal async Task FireOnUndoEventAsync(UndoRedoAction<T> action)
        {
            if (Parent?.GridEvents?.OnUndo.HasDelegate != true || action == null)
            {
                return;
            }

            var data = BuildEventArgsData(action);
            var args = new OnUndoEventArgs<T>
            {
                ActionType = data.ActionType,
                RowIndex = data.RowIndex,
                FieldName = data.FieldName,
                OldValue = data.OldValue,
                NewValue = data.NewValue,
                ColumnIndex = data.ColumnIndex,
                RowData = data.RowData
            };
            await Parent.GridEvents.OnUndo.InvokeAsync(args).ConfigureAwait(true);
        }

        /// <summary>
        /// Fires the OnUndoCompleted event after an undo operation has completed and the UI has been refreshed.
        /// Events are informational only and cannot modify or cancel the operation.
        /// </summary>
        /// <param name="action">The action that was undone.</param>
        /// <returns>A task representing the asynchronous event invocation.</returns>
        internal async Task FireOnUndoCompletedEventAsync(UndoRedoAction<T> action)
        {
            if (Parent?.GridEvents?.OnUndoCompleted.HasDelegate != true || action == null)
            {
                return;
            }

            var data = BuildEventArgsData(action);
            var args = new OnUndoCompletedEventArgs<T>
            {
                ActionType = data.ActionType,
                RowIndex = data.RowIndex,
                FieldName = data.FieldName,
                OldValue = data.OldValue,
                NewValue = data.NewValue,
                ColumnIndex = data.ColumnIndex,
                RowData = data.RowData
            };
            await Parent.GridEvents.OnUndoCompleted.InvokeAsync(args).ConfigureAwait(true);
        }

        /// <summary>
        /// Fires the OnRedo event before a redo operation is executed.
        /// Events are informational only and cannot modify or cancel the operation.
        /// </summary>
        /// <param name="action">The action that is about to be redone.</param>
        /// <returns>A task representing the asynchronous event invocation.</returns>
        internal async Task FireOnRedoEventAsync(UndoRedoAction<T> action)
        {
            if (Parent?.GridEvents?.OnRedo.HasDelegate != true || action == null)
            {
                return;
            }

            var data = BuildEventArgsData(action);
            var args = new OnRedoEventArgs<T>
            {
                ActionType = data.ActionType,
                RowIndex = data.RowIndex,
                FieldName = data.FieldName,
                OldValue = data.OldValue,
                NewValue = data.NewValue,
                ColumnIndex = data.ColumnIndex,
                RowData = data.RowData
            };
            await Parent.GridEvents.OnRedo.InvokeAsync(args).ConfigureAwait(true);
        }

        /// <summary>
        /// Fires the OnRedoCompleted event after a redo operation has completed and the UI has been refreshed.
        /// Events are informational only and cannot modify or cancel the operation.
        /// </summary>
        /// <param name="action">The action that was redone.</param>
        /// <returns>A task representing the asynchronous event invocation.</returns>
        internal async Task FireOnRedoCompletedEventAsync(UndoRedoAction<T> action)
        {
            if (Parent?.GridEvents?.OnRedoCompleted.HasDelegate != true || action == null)
            {
                return;
            }

            var data = BuildEventArgsData(action);
            var args = new OnRedoCompletedEventArgs<T>
            {
                ActionType = data.ActionType,
                RowIndex = data.RowIndex,
                FieldName = data.FieldName,
                OldValue = data.OldValue,
                NewValue = data.NewValue,
                ColumnIndex = data.ColumnIndex,
                RowData = data.RowData
            };
            await Parent.GridEvents.OnRedoCompleted.InvokeAsync(args).ConfigureAwait(true);
        }

        #endregion

        /// <summary>
        /// Records a new action to the undo stack.
        /// When a new action is recorded, the redo stack is cleared (standard undo/redo behavior).
        /// If the undo stack exceeds MaxStackSize, the oldest action is removed.
        /// </summary>
        /// <param name="action">The action to record.</param>
        internal void RecordAction(UndoRedoAction<T> action)
        {
            if (!isEnabled || action == null)
            {
                return;
            }
            action.SequenceNumber = ++sequenceCounter;
            undoStack.AddFirst(action);
            if (redoStack.Count > 0)
            {
                ClearRedoStack();
              
            }
            while (undoStack.Count > maxStackSize && undoStack.Last != null)
            {
                undoStack.RemoveLast();
              
            }
        }

        /// <summary>
        /// Performs an undo operation by reverting the most recent action.
        /// The undone action is moved to the redo stack for potential redo.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns the undone action, or null if undo stack is empty.</returns>
        internal async Task<UndoRedoAction<T>?> UndoAsync()
        {
            if (!isEnabled || undoStack.Count == 0)
            {
                return await Task.FromResult<UndoRedoAction<T>?>(null).ConfigureAwait(true);
            }

            var action = undoStack.First?.Value;
            if (action != null)
            {
                undoStack.RemoveFirst();
                redoStack.AddFirst(action);
                return await Task.FromResult(action).ConfigureAwait(true);
            }

            return await Task.FromResult<UndoRedoAction<T>?>(null).ConfigureAwait(true);
        }

        /// <summary>
        /// Performs a redo operation by reapplying the most recently undone action.
        /// The redone action is moved back to the undo stack.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns the redone action, or null if redo stack is empty.</returns>
        internal async Task<UndoRedoAction<T>?> RedoAsync()
        {
            if (!isEnabled || redoStack.Count == 0)
            {
                return await Task.FromResult<UndoRedoAction<T>?>(null).ConfigureAwait(true);
            }
            var action = redoStack.First?.Value;
            if (action != null)
            {
                redoStack.RemoveFirst();

                undoStack.AddFirst(action);
                return await Task.FromResult(action).ConfigureAwait(true);
            }

            return await Task.FromResult<UndoRedoAction<T>?>(null).ConfigureAwait(true);
        }

        /// <summary>
        /// Undoes all actions sequentially, moving them all from the undo stack to the redo stack.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task UndoAllAsync()
        {
            if (!isEnabled)
            {
                return;
            }

            int count = 0;
            while (undoStack.Count > 0)
            {
                var undone = await PerformUndoAsync().ConfigureAwait(true);
                if (undone == null)
                {
                    break;
                }
                count++;
            }

            if (count > 0)
            {
                TriggerUndoRedoStackChanged();
            }
        }

        /// <summary>
        /// Redoes all actions sequentially, moving them all from the redo stack to the undo stack.
        /// </summary>
        /// <returns>A task representing the asynchronous operation.</returns>
        internal async Task RedoAllAsync()
        {
            if (!isEnabled)
            {
                return;
            }

            int count = 0;
            while (redoStack.Count > 0)
            {
                var redone = await PerformRedoAsync().ConfigureAwait(true);
                if (redone == null)
                {
                    break;
                }
                count++;
            }

            if (count > 0)
            {
                TriggerUndoRedoStackChanged();
            }
        }

        /// <summary>
        /// Clears only the redo stack.
        /// This is typically called when batch editing is cancelled to discard redo history.
        /// </summary>
        internal void ClearRedoStack()
        {
            redoStack.Clear();
           
        }

        /// <summary>
        /// Clears both the undo and redo stacks, resetting to a clean state.
        /// </summary>
        internal void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            sequenceCounter = 0;
        }

        /// <summary>
        /// Enables undo/redo with an optional stack size limit.
        /// </summary>
        /// <param name="stackLimit">The maximum number of actions to keep in each stack. Defaults to 20.</param>
        internal void Enable(int stackLimit = 20)
        {
            isEnabled = true;
            MaxStackSize = stackLimit;
            Clear();
            ResetBatchEditDirtyState();
        }

        /// <summary>
        /// Disables undo/redo and clears both stacks immediately.
        /// Also clears any existing dirty/edited state on rows and cells so the UI reverts
        /// to the original data values when the feature is toggled off dynamically.
        /// </summary>
        internal void Disable()
        {
            isEnabled = false;
            Clear();
            ResetBatchEditDirtyState();
        }
        
        private void ResetBatchEditDirtyState()
        {
            if (Parent == null)
            {
                return;
            }

            if (Parent.EditModule != null)
            {
                Parent.EditModule.HasBatchChanges = false;
            }

            if (Parent.Rows == null)
            {
                return;
            }

            foreach (var row in Parent.Rows)
            {
                if (row == null)
                {
                    continue;
                }

                row.IsDirty = false;
                row.EditedData = null!;
                row.HasDataChanges = false;
                if (row.Cells != null)
                {
                    foreach (var cell in row.Cells)
                    {
                        if (cell == null)
                        {
                            continue;
                        }

                        cell.IsDirty = false;
                        cell.EditDisabled = false;
                    }
                }
            }
        }

        /// <summary>
        /// Updates the RowData of the most recent RowAdd action if it exists for the given row index.
        /// Used to keep newly added rows' undo data in sync as cells are edited.
        /// Implements EJ2 pattern: "If row already in undo stack, just update rowData"
        /// </summary>
        /// <param name="rowIndex">The index of the row to update</param>
        /// <param name="newRowData">The updated row data to store</param>
        /// <returns>True if an action was found and updated, false otherwise</returns>
        internal bool UpdateLastRowAddAction(int rowIndex, T newRowData)
        {
            if (!isEnabled || undoStack.Count == 0)
            {
                return false;
            }

            // Search from the most recent (First) backwards for a RowAdd action with matching rowIndex
            var currentNode = undoStack.First;
            while (currentNode != null)
            {
                var action = currentNode.Value;
                if (action?.ActionType == UndoRedoActionType.RowAdd && action.RowIndex == rowIndex)
                {
                    // Found it! Update the rowData with current values
                    action.RowData = newRowData;
                   
                    return true;
                }

                currentNode = currentNode.Next;
            }

            return false;  // No matching RowAdd action found
        }

        /// <summary>
        /// Orchestrates the complete undo workflow: peek, fire before-event, pop, apply, fire after-event.
        /// This method encapsulates the undo orchestration logic to avoid duplication in SfGrid.Methods.cs.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns the undone action, or null if undo stack is empty.</returns>
        internal async Task<UndoRedoAction<T>?> PerformUndoAsync()
        {
            if (!isEnabled || undoStack.Count == 0)
            {
                return null;
            }

            // Peek the top of the undo stack without popping to raise the OnUndo event.
            // The event is informational only and fires before any stack mutation occurs.
            var pendingAction = UndoCount > 0 ? PeekUndo() : null;
            if (pendingAction != null)
            {
                await FireOnUndoEventAsync(pendingAction).ConfigureAwait(true);
            }

            // Capture the undone action and apply it to the grid
            var undoneAction = await UndoAsync().ConfigureAwait(true);
            if (undoneAction != null)
            {
                // Trigger point: Apply the undo action to update grid UI (isRedoAction = false)
                await ApplyUndoRedoAction(undoneAction, isRedoAction: false).ConfigureAwait(true);

                // After undo completes and the UI has been refreshed, fire the OnUndoCompleted event.
                await FireOnUndoCompletedEventAsync(undoneAction).ConfigureAwait(true);
                await MoveFocusToCellEditAsync(undoneAction).ConfigureAwait(true);
            }

            return undoneAction;
        }

        /// <summary>
        /// Orchestrates the complete redo workflow: peek, fire before-event, pop, apply, fire after-event.
        /// This method encapsulates the redo orchestration logic to avoid duplication in SfGrid.Methods.cs.
        /// </summary>
        /// <returns>A task representing the asynchronous operation. Returns the redone action, or null if redo stack is empty.</returns>
        internal async Task<UndoRedoAction<T>?> PerformRedoAsync()
        {
            if (!isEnabled || redoStack.Count == 0)
            {
                return null;
            }

            // Peek the top of the redo stack without popping to raise the OnRedo event.
            // The event is informational only and fires before any stack mutation occurs.
            var pendingAction = RedoCount > 0 ? PeekRedo() : null;
            if (pendingAction != null)
            {
                await FireOnRedoEventAsync(pendingAction).ConfigureAwait(true);
            }

            // Capture the redone action and apply it to the grid
            var redoneAction = await RedoAsync().ConfigureAwait(true);
            if (redoneAction != null)
            {
                // Trigger point: Apply the redo action to update grid UI (isRedoAction = true)
                await ApplyUndoRedoAction(redoneAction, isRedoAction: true).ConfigureAwait(true);

                // After redo completes and the UI has been refreshed, fire the OnRedoCompleted event.
                await FireOnRedoCompletedEventAsync(redoneAction).ConfigureAwait(true);
                await MoveFocusToCellEditAsync(redoneAction).ConfigureAwait(true);
            }

            return redoneAction;
        }

        #region Undo/Redo Action Application

        private async Task MoveFocusToCellEditAsync(UndoRedoAction<T> action)
        {
            if (action?.ActionType != UndoRedoActionType.CellEdit || action.CellChange == null)
            {
                return;
            }

            if (Parent?.FocusModule == null || Parent.Rows == null)
            {
                return;
            }

            int rowIndex = action.CellChange.RowIndex;
            int columnIndex = action.CellChange.ColumnIndex;
            if (rowIndex < 0 || rowIndex >= Parent.Rows.Count)
            {
                return;
            }

            var row = Parent.Rows[rowIndex];
            if (row == null || string.IsNullOrEmpty(row.Uid))
            {
                return;
            }
            var visibleDataCells = row.Cells?
                .Where(c => c.CellType == CellType.Data && c.Visible)
                .ToList();

            if (visibleDataCells == null || visibleDataCells.Count == 0)
            {
                return;
            }
            if (columnIndex < 0 || columnIndex >= visibleDataCells.Count)
            {
                return;
            }

            var cell = visibleDataCells[columnIndex];
            if (cell == null || string.IsNullOrEmpty(cell.Uid))
            {
                return;
            }
            int cellColIndex = cell.Column?.Index ?? -1;
            await Parent.FocusModule.Focus(row.Uid, cell.Uid, cellColIndex: cellColIndex+1)
                .ConfigureAwait(true);
        }

        /// <summary>
        /// Applies an undo/redo action to the grid by restoring old or new values based on operation type.
        /// This method is the trigger point that translates UndoRedoAction objects into actual grid updates.
        /// </summary>
        /// <param name="action">The undo/redo action to apply</param>
        /// <param name="isRedoAction">True if this is a redo operation; false if undo. Controls which value (old vs new) is applied.</param>
        /// <returns>Task representing the asynchronous operation</returns>
        internal async Task ApplyUndoRedoAction(UndoRedoAction<T>? action, bool isRedoAction = false)
        {
            if (action == null || Parent == null)
            {
                return;
            }
            switch (action.ActionType)
            {
                case UndoRedoActionType.CellEdit:
                    await ApplyCellEditUndo(action, isRedoAction).ConfigureAwait(true);
                    break;

                case UndoRedoActionType.RowAdd:
                    await ApplyRowAddUndo(action, isRedoAction).ConfigureAwait(true);
                    break;

                case UndoRedoActionType.RowDelete:
                    await ApplyRowDeleteUndo(action, isRedoAction).ConfigureAwait(true);
                    break;

                case UndoRedoActionType.Paste:
                    await ApplyPasteUndo(action, isRedoAction).ConfigureAwait(true);
                    break;

                case UndoRedoActionType.AutoFill:
                    await ApplyAutoFillUndo(action, isRedoAction).ConfigureAwait(true);
                    break;

                case UndoRedoActionType.None:
                default:
                    // An uninitialized action (None) should never reach the dispatcher; ignore defensively
                    // so that a default-constructed UndoRedoAction<T> is a no-op rather than a crash.
                    break;
            }
                Parent.SoftRefresh = true;
                Parent.PreventRender(false);
                Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            
           
        }

        /// <summary>
        /// Applies a single cell edit undo/redo action by restoring the appropriate cell value.
        /// For undo: restores the old value.
        /// For redo: applies the new value (what was edited to).
        /// </summary>
        private async Task ApplyCellEditUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.CellChange == null || Parent == null)
            {
                return;
            }

            var change = action.CellChange;
            var rowIndex = change.RowIndex;
            var fieldName = change.FieldName;

            var valueToApply = isRedoAction ? change.NewValue : change.OldValue;

            if (string.IsNullOrEmpty(fieldName) || valueToApply == null)
            {
                return;
            }
            await Parent.EditModule!.UpdateCell(rowIndex, fieldName, valueToApply, isUndoRedoAction: true).ConfigureAwait(true);

            var operationType = isRedoAction ? "redone" : "undone";
        }

        /// <summary>
        /// Applies a row add undo/redo action.
        /// For undo: removes the added row.
        /// For redo: re-adds the row.
        /// </summary>
        private async Task ApplyRowAddUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.RowIndex == null || action.RowIndex < 0 || Parent == null)
            {
                return;
            }

            var rowIndex = action.RowIndex.Value;

            if (isRedoAction)
            {
                if (action.RowData != null)
                {
                    var newRow = new RowModelGenerator<T>(Parent).GenerateRow(action.RowData, rowIndex);
                    newRow.EditedData = action.RowData;
                    newRow.IsDirty = true;
                    newRow.Action = EditAction.Added;
                    newRow.IsAddedTop = action.RowPosition == NewRowPosition.Top;
                    newRow.IsAddedBottom = action.RowPosition == NewRowPosition.Bottom;
                    newRow.Cells?.ForEach(_ => _.IsDirty = true);

                    var insertIndex = Math.Min(rowIndex, Parent.Rows?.Count ?? 0);
                    if (insertIndex >= 0 && insertIndex <= (Parent.Rows?.Count ?? 0))
                    {
                        Parent.Rows?.Insert(insertIndex, newRow);
                       
                    }
                }
            }
            else
            {
                var row = Parent.Rows?.Find(_ => _.Index == rowIndex);
                if (row != null)
                {
                    Parent.Rows?.Remove(row);
                    
                }
            }
            Parent.EditModule!.RefreshRowIndex();
            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            Parent.EventAggregator.Trigger("ContentStateChanged", null!);

            await Task.CompletedTask.ConfigureAwait(true);
        }


        private async Task ApplyRowDeleteUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.RowIndex == null || action.RowData == null || Parent == null)
            {
                return;
            }

            var rowIndex = action.RowIndex.Value;
            var primaryKeyFields = await Parent.GetPrimaryKeyFieldNamesAsync().ConfigureAwait(true);

            if (isRedoAction)
            {
                Row<object>? row = null;

                if (primaryKeyFields != null && primaryKeyFields.Count > 0)
                {
                    foreach (var currentRow in Parent.Rows ?? Enumerable.Empty<Row<object>>())
                    {
                        var candidateData = currentRow.EditedData ?? currentRow.Data;
                        if (candidateData == null || currentRow.Action == EditAction.Deleted)
                        {
                            continue;
                        }

                        if (await Parent.EditModule!.MatchCompositeKeyAsync(candidateData, action.RowData, primaryKeyFields).ConfigureAwait(true))
                        {
                            row = currentRow;
                            break;
                        }
                    }
                }

                if (row != null)
                {
                    row.Action = EditAction.Deleted;
                    row.IsDirty = true;
                    Parent.SoftRefresh = true;
                    Parent.EventAggregator.Trigger("RowStateChanged", row);
                }
            }
            else
            {
                Row<object>? deletedRow = null;

                if (primaryKeyFields != null && primaryKeyFields.Count > 0)
                {
                    foreach (var currentRow in Parent.Rows ?? Enumerable.Empty<Row<object>>())
                    {
                        var candidateData = currentRow.EditedData ?? currentRow.Data;
                        if (candidateData == null || currentRow.Action != EditAction.Deleted)
                        {
                            continue;
                        }

                        if (await Parent.EditModule!.MatchCompositeKeyAsync(candidateData, action.RowData, primaryKeyFields).ConfigureAwait(true))
                        {
                            deletedRow = currentRow;
                            break;
                        }
                    }
                }

                if (deletedRow != null)
                {
                    var wasAddedRow = deletedRow.IsAddedTop || deletedRow.IsAddedBottom;
                    deletedRow.Action = wasAddedRow ? EditAction.Added : EditAction.None;
               
                    deletedRow.EditedData = action.RowData;
                    var hasEdits = GridUtils.CompareValues(deletedRow.EditedData, action.RowData);
                    deletedRow.IsDirty = hasEdits;
                    deletedRow.Cells?.ForEach(cell =>
                    {
                        if (wasAddedRow)
                        {
                            cell.IsDirty = true;
                        }
                        else if (cell.Column?.Field != null)
                        {
                            var originalValue = Parent.PropHelper?.GetObject(cell.Column.Field, deletedRow.Data!);
                            var restoredValue = Parent.PropHelper?.GetObject(cell.Column.Field, action.RowData);
                            var cellHasEdit = GridUtils.CompareValues(originalValue, restoredValue);
                            cell.IsDirty = cellHasEdit;
                        }
                        else
                        {
                            cell.IsDirty = false;
                        }
                        
                        cell.EditDisabled = false;
                    });
                    Parent.SoftRefresh = true;
                    Parent.EventAggregator.Trigger("RowStateChanged", deletedRow);
                }
                else
                {
                    var newRow = new RowModelGenerator<T>(Parent).GenerateRow(action.RowData, rowIndex);
                    newRow.EditedData = action.RowData;
                    var hasEdits = !GridUtils.CompareValues(newRow.Data, action.RowData);
                    newRow.IsDirty = hasEdits;
                    var insertIndex = Math.Min(rowIndex, Parent.Rows?.Count ?? 0);
                    if (insertIndex >= 0 && insertIndex <= (Parent.Rows?.Count ?? 0))
                    {
                        Parent.Rows?.Insert(insertIndex, newRow);
                        
                    }
                }
            }

            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);
            await Parent.CallStateHasChangedAsync().ConfigureAwait(true);
            await Task.CompletedTask.ConfigureAwait(true);
        }


        private async Task ApplyPasteUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.PreviousValues == null || action.PreviousValues.Count == 0 || Parent == null)
            {
                return;
            }
            int startRowIndex = int.MaxValue;
            int startColIndex = int.MaxValue;
            int endRowIndex = int.MinValue;
            int endColIndex = int.MinValue;
            foreach (var change in action.PreviousValues)
            {
                if (string.IsNullOrEmpty(change.FieldName))
                {
                    continue;
                }
                startRowIndex = Math.Min(startRowIndex, change.RowIndex);
                startColIndex = Math.Min(startColIndex, change.ColumnIndex);
                endRowIndex = Math.Max(endRowIndex, change.RowIndex);
                endColIndex = Math.Max(endColIndex, change.ColumnIndex);

                var valueToApply = isRedoAction ? change.NewValue : change.OldValue;
                if (valueToApply != null)
                {
                    await Parent.EditModule!.UpdateCell(change.RowIndex, change.FieldName, valueToApply, false).ConfigureAwait(true);
                }
            }
            if (startRowIndex <= endRowIndex && startColIndex <= endColIndex)
            {
                await RestorePasteStateAsync(startRowIndex, startColIndex, endRowIndex, endColIndex).ConfigureAwait(true);
            }

            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);

            var operationType = isRedoAction ? "redone" : "undone";
        }
        private async Task ApplyAutoFillUndo(UndoRedoAction<T> action, bool isRedoAction)
        {
            if (action.PreviousValues == null || action.PreviousValues.Count == 0 || Parent == null)
            {
                return;
            }
            int startRowIndex = int.MaxValue;
            int startColIndex = int.MaxValue;
            int endRowIndex = int.MinValue;
            int endColIndex = int.MinValue;

            foreach (var change in action.PreviousValues)
            {
                if (string.IsNullOrEmpty(change.FieldName))
                {
                    continue;
                }
                startRowIndex = Math.Min(startRowIndex, change.RowIndex);
                startColIndex = Math.Min(startColIndex, change.ColumnIndex);
                endRowIndex = Math.Max(endRowIndex, change.RowIndex);
                endColIndex = Math.Max(endColIndex, change.ColumnIndex);

                var valueToApply = isRedoAction ? change.NewValue : change.OldValue;
                if (valueToApply != null)
                {
                    await Parent.EditModule!.UpdateCell(change.RowIndex, change.FieldName, valueToApply, false).ConfigureAwait(true);
                }
            }
            if (startRowIndex <= endRowIndex && startColIndex <= endColIndex)
            {
                await RestoreAutofillStateAsync(startRowIndex, startColIndex, endRowIndex, endColIndex).ConfigureAwait(true);
            }

            Parent.EditModule!.HasBatchChanges = true;
            Parent.EventAggregator.Trigger("ToolbarStateChanged", null!);

            var operationType = isRedoAction ? "redone" : "undone";
        }
        private async Task RestoreAutofillStateAsync(int startRowIndex, int startColIndex, int endRowIndex, int endColIndex)
        {
            if (Parent == null)
            {
                return;
            }
            await Parent.ClearSelectionAsync().ConfigureAwait(true);
        }

        private async Task RestorePasteStateAsync(int startRowIndex, int startColIndex, int endRowIndex, int endColIndex)
        {
            if (Parent == null)
            {
                return;
            }  
            await Parent.ClearSelectionAsync().ConfigureAwait(true);
        }

        #endregion
    }
}
