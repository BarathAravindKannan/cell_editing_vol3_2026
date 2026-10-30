using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids.Internal;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid edit settings.
    /// </summary>
    public partial class GridEditSettings : SfDataBoundComponent
    {

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// If AllowAdding is set to true, new records can be added to the Grid.
        /// </summary>
        [Parameter]
        public bool AllowAdding { get; set; }

        private bool _allowAdding { get; set; }

        /// <summary>
        /// If AllowDeleting is set to true, existing record can be deleted from the Grid.
        /// </summary>
        [Parameter]
        public bool AllowDeleting { get; set; }

        private bool _allowDeleting { get; set; }

        /// <summary>
        /// If AllowEditOnDblClick is set to false, Grid will not allow editing of a record on double click.
        /// </summary>
        [Parameter]
        public bool AllowEditOnDblClick { get; set; } = true;

        private bool _allowEditOnDblClick { get; set; }

        /// <summary>
        /// Specifies whether a cell enters edit mode on a single mouse click.
        /// </summary>
        /// <value>
        /// <c>true</c> to enable single-click editing; otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When enabled, a cell enters edit mode on a single click. Otherwise, double-click is required.
        /// This property is applicable only when <see cref="GridEditSettings.Mode"/> is set to <see cref="EditMode.Batch"/>.
        /// It has no effect in other editing modes.
        /// </remarks>
        [Parameter]
        public bool AllowEditOnSingleClick { get; set; }

        private bool _allowEditOnSingleClick { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a focused cell enters edit mode
        /// when a printable key is pressed.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> value that determines whether typing directly into a
        /// focused cell automatically starts editing. The default value is
        /// <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// When set to <see langword="true"/>, pressing a printable character key,
        /// such as a letter, digit, or symbol, immediately places the focused cell
        /// into edit mode.
        /// The editor is opened with its current content cleared and the typed
        /// character becomes the initial value.
        /// Pressing the Enter key behaves differently. It opens the editor for the
        /// current cell while retaining and selecting the existing value.
        /// This property is applicable only when <see cref="Mode"/> is set to
        /// <see cref="EditMode.Cell"/> and has no effect in other edit modes.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEditSettings AllowEditing="true"
        ///                       Mode="EditMode.Cell"
        ///                       AllowEditOnKeyPress="true" />
        /// </SfGrid>
        /// ]]></code>
        /// </example>
        [Parameter]
        public bool AllowEditOnKeyPress { get; set; }

        private bool _allowEditOnKeyPress { get; set; }

        /// <summary>
        /// Gets or sets the navigation behavior of the Enter key while editing cells.
        /// </summary>
        /// <value>
        /// An <see cref="EnterKeyDirection"/> value that specifies how focus moves
        /// after a cell value is committed. The default value is
        /// <see cref="EnterKeyDirection.None"/>.
        /// </value>
        /// <remarks>
        /// This property is applicable only when <see cref="Mode"/> is set to
        /// <see cref="EditMode.Cell"/>.
        /// When enabled, pressing the Enter key saves the current cell value and
        /// optionally moves focus according to the configured direction.
        /// For edit modes other than <see cref="EditMode.Cell"/>, the Enter key
        /// follows the default behavior associated with that mode.
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders">
        ///     <GridEditSettings AllowEditing="true"
        ///                       Mode="EditMode.Cell"
        ///                       EnterKeyDirection="EnterKeyDirection.NextColumn" />
        /// </SfGrid>
        /// ]]></code>
        /// </example>
        [Parameter]
        public EnterKeyDirection EnterKeyDirection { get; set; } = EnterKeyDirection.None;

        private EnterKeyDirection _enterKeyDirection { get; set; }

        /// <summary>
        /// If AllowEditing is set to true, values can be updated in the existing record.
        /// </summary>
        [Parameter]
        public bool AllowEditing { get; set; }

        private bool _allowEditing { get; set; }

        /// <summary>
        /// If allowNextRowEdit is set to true, editing is done to next row. By default allowNextRowEdit is set to false.
        /// </summary>
        [Parameter]
        public bool AllowNextRowEdit { get; set; }

        private bool _allowNextRowEdit { get; set; }

        /// <summary>
        /// Defines the dialog params to edit.
        /// </summary>
        [Parameter]
        public DialogSettings? Dialog { get; set; }

        private DialogSettings? _dialog { get; set; }

        /// <summary>
        /// Defines the custom footer for the edit dialog.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? FooterTemplate { get; set; }

        /// <summary>
        /// Defines the custom header for the edit dialog.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? HeaderTemplate { get; set; }

        /// <summary>
        /// Defines the mode to edit. The available editing modes are:
        /// <list type="bullet">
        /// <item>
        /// <term>Normal</term>
        /// <description>Default. Editing is done in an inline form. Edit form is rendered inline as one of the table rows.</description>
        /// </item>
        /// <item>
        /// <term>Dialog</term>
        /// <description>Editing is done in a Dialog/Pop component.</description>
        /// </item>
        /// <item>
        /// <term>Batch</term>
        /// <description>Enables cell editing. Multiple cells can be edited, added or deleted and saved.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public EditMode Mode { get; set; } = EditMode.Normal;

        private EditMode _mode { get; set; }

        /// <summary>
        /// Defines the position of adding a new row. The available position are:
        /// <list type="bullet">
        /// <item>
        /// <term>Top</term>
        /// <description>Default. Add form is placed at the first row of the grid.</description>
        /// </item>
        /// <item>
        /// <term>Bottom</term>
        /// <description>Add form is placed at the last row of the grid</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public NewRowPosition NewRowPosition { get; set; } = NewRowPosition.Top;

        private NewRowPosition _newRowPosition { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to display a new blank row during grid initialization, facilitating the addition of new records.
        /// </summary>
        /// <value>
        /// <c>true</c>, a new blank row is displayed on the grid content. The default value is <c>false</c>.
        /// </value>
        /// <remarks>    
        /// By default, the new blank row is displayed at the top of the grid content. A new blank row can be displayed either at the top or bottom of the corresponding page, depending on the setting of the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.NewRowPosition"/> property. 
        /// However, it's important to note that the <c>ShowAddNewRow</c> property solely pertains to the display of a new blank row in the <see cref="EditMode.Normal"/> editing mode. 
        /// If the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.AllowAdding"/> property is set to false, the new blank row will be disabled.
        /// Additionally, if any of the grid column's <see cref="Syncfusion.Blazor.Grids.GridColumn.AllowAdding"/> properties is set to false, the corresponding column cell will also be disabled.
        /// </remarks>
        [Parameter]
        public bool ShowAddNewRow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the Undo/Redo functionality is enabled for batch editing operations.
        /// </summary>
        /// <value>
        /// <c>true</c> if Undo/Redo is enabled for batch editing; otherwise, <c>false</c>. The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// This property is only effective when the <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> property is set to
        /// <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>. When enabled, the grid records batch edit actions (such as cell edits,
        /// row additions, row deletions, paste, and auto-fill operations) into an undo/redo history that can be navigated using
        /// <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.UndoAsync"/>, <see cref="Syncfusion.Blazor.Grids.SfGrid{TValue}.RedoAsync"/>,
        /// and related APIs.
        /// </remarks>
        [Parameter]
        public bool EnableUndoRedo { get; set; } = false;

        private bool _enableUndoRedo { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of Undo/Redo steps to maintain in memory.
        /// </summary>
        /// <value>
        /// The maximum number of actions to keep in the Undo/Redo history. The default value is <c>20</c>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When the number of recorded actions exceeds this limit, the oldest action is discarded to make room for new ones.
        /// The minimum value applied is <c>1</c>; any value less than <c>1</c> is treated as <c>1</c>. The clamp is silent;
        /// no diagnostic is emitted when it engages.
        /// </para>
        /// <para>
        /// The limit counts <em>actions</em>, not individual cells. A single <see cref="UndoRedoActionType.Paste"/> or
        /// <see cref="UndoRedoActionType.AutoFill"/> action buffers every affected cell in its
        /// internal previous values list, so one action can hold thousands of
        /// cell change entries. Memory usage therefore scales with
        /// <c>UndoRedoLimit × max cells per Paste/AutoFill</c>; size the buffer accordingly for large row counts.
        /// </para>
        /// <para>
        /// This property is only effective when <see cref="EnableUndoRedo"/> is set to <c>true</c> and the
        /// <see cref="Syncfusion.Blazor.Grids.GridEditSettings.Mode"/> is <see cref="Syncfusion.Blazor.Grids.EditMode.Batch"/>.
        /// </para>
        /// </remarks>
        [Parameter]
        public int UndoRedoLimit { get; set; } = 20;

        private int _undoRedoLimit { get; set; }
        
        /// <summary>
        /// If ShowConfirmDialog is set to false, confirm dialog does not show when batch changes are saved or discarded.
        /// </summary>
        [Parameter]
        public bool ShowConfirmDialog { get; set; } = true;

        private bool _showConfirmDialog { get; set; }

        /// <summary>
        /// If ShowDeleteConfirmDialog is set to true, confirm dialog will show delete action. You can also cancel delete command.
        /// </summary>
        [Parameter]
        public bool ShowDeleteConfirmDialog { get; set; }

        private bool _showDeleteConfirmDialog { get; set; }

        /// <summary>
        /// Defines the custom content and edit elements for the edit dialog.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type TValue of the grid.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? Template { get; set; }

        /// <summary>
        /// Defines the custom validator component for the built-in EditForm. Use this to override built-in
        /// validation components.
        /// </summary>
        /// <remarks>
        /// The parameters passed to the templates can be accessed using implicit parameter named <c>context</c>.
        /// The context is of type <see cref="Syncfusion.Blazor.Grids.ValidatorTemplateContext"/>.
        /// </remarks>
        [Parameter]
        public RenderFragment<object>? Validator { get; set; }

        internal static async Task<GridEditSettings> Initialize(SfBaseComponent baseComponent)
        {
            var GridEditSettings = new GridEditSettings();
            GridEditSettings.Parent = (IGrid)baseComponent;
            GridEditSettings.BaseParent = (IGrid)baseComponent;

            // GridEditSettings.IsAutoInitialized = true;
            await GridEditSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridEditSettings;
        }

        /// <summary>
        /// Initializes the GridEditSettings, applies edit configuration, and updates the parent grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            Parent?.UpdateChildProperties(nameof(IGrid.EditSettings), this);
            _allowAdding = AllowAdding;
            _allowDeleting = AllowDeleting;
            _allowEditOnDblClick = AllowEditOnDblClick;
            _allowEditOnSingleClick = AllowEditOnSingleClick;
            _allowEditing = AllowEditing;
            _allowNextRowEdit = AllowNextRowEdit;
            _dialog = Dialog;
            _mode = Mode;
            _newRowPosition = NewRowPosition;
            _showConfirmDialog = ShowConfirmDialog;
            _showDeleteConfirmDialog = ShowDeleteConfirmDialog;
            _allowEditOnKeyPress = AllowEditOnKeyPress;
            _enterKeyDirection = EnterKeyDirection;
            _enableUndoRedo = EnableUndoRedo;
            _undoRedoLimit = UndoRedoLimit;
        }

        /// <summary>
        /// Handles parameter updates for GridEditSettings and synchronizes internal state when values change.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            bool enableUndoRedoChanged = !SfBaseUtils.Equals(EnableUndoRedo, _enableUndoRedo);
            bool undoRedoLimitChanged = !SfBaseUtils.Equals(UndoRedoLimit, _undoRedoLimit);
            if(!SfBaseUtils.Equals(AllowAdding, _allowAdding) || !SfBaseUtils.Equals(AllowDeleting, _allowDeleting)
            || !SfBaseUtils.Equals(AllowEditOnDblClick, _allowEditOnDblClick) || !SfBaseUtils.Equals(AllowEditOnSingleClick, _allowEditOnSingleClick) || !SfBaseUtils.Equals(AllowEditing, _allowEditing)
            || !SfBaseUtils.Equals(AllowNextRowEdit, _allowNextRowEdit) || !SfBaseUtils.Equals(Dialog, _dialog)
            || !SfBaseUtils.Equals(Mode, _mode) || !SfBaseUtils.Equals(NewRowPosition, _newRowPosition)
            || !SfBaseUtils.Equals(ShowConfirmDialog, _showConfirmDialog) || !SfBaseUtils.Equals(ShowDeleteConfirmDialog, _showDeleteConfirmDialog)
            || !SfBaseUtils.Equals(AllowEditOnKeyPress, _allowEditOnKeyPress) || !SfBaseUtils.Equals(EnterKeyDirection, _enterKeyDirection)
            || enableUndoRedoChanged || undoRedoLimitChanged)
            {
                _allowAdding = AllowAdding;
                _allowDeleting = AllowDeleting;
                _allowEditOnDblClick = AllowEditOnDblClick;
                _allowEditOnSingleClick = AllowEditOnSingleClick;
                _allowEditing = AllowEditing;
                _allowNextRowEdit = AllowNextRowEdit;
                _dialog = Dialog;
                _mode = Mode;
                _newRowPosition = NewRowPosition;
                _showConfirmDialog = ShowConfirmDialog;
                _showDeleteConfirmDialog = ShowDeleteConfirmDialog;
                _allowEditOnKeyPress = AllowEditOnKeyPress;
                _enterKeyDirection = EnterKeyDirection;
                _enableUndoRedo = EnableUndoRedo;
                _undoRedoLimit = UndoRedoLimit;
                if ((enableUndoRedoChanged || undoRedoLimitChanged) && BaseParent != null && PropertyChanges != null)
                {
                    if (enableUndoRedoChanged)
                    {
                        PropertyChanges.TryAdd("EnableUndoRedo", EnableUndoRedo);
                        ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd("EnableUndoRedo", EnableUndoRedo);
                    }
                    if (undoRedoLimitChanged)
                    {
                        PropertyChanges.TryAdd("UndoRedoLimit", UndoRedoLimit);
                        ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd("UndoRedoLimit", UndoRedoLimit);
                    }
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.EditSettings), this);
                }
            }
            if (PropertyChanges != null && PropertyChanges.Count > 0 && BaseParent != null)
            {
                PropertyChanges.Clear();
                await BaseParent.PropertyChanged().ConfigureAwait(true);
            }
            // Telemetry for editing modes
            GridTelemetryHelper.LogTelemetry(true, "Editing");
        }
    }
}