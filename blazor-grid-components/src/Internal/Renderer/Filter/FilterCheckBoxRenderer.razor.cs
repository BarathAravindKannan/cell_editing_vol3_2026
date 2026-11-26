using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Data;
using Syncfusion.Blazor.Internal;
using Syncfusion.Blazor.Popups;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Syncfusion.Blazor.Tests, PublicKey=00240000048000009400000006020000002400005253413100040000010001002382fcb1069523ce72d849497a557a445c151eaf4007aa79adef551a8204ca7f728e5378607d85695b16f129ec35bf4af15dcf6d3581deb8bb0debb239c33e7f1271a37c7f60f1044ae417730f5082abee5f9ec568a8a4cef04074394755706376e982dc6f9d15430faaad385ae8f00a77ef1c97517f1a1517004ce78028b9ce")]
namespace Syncfusion.Blazor.Grids.Internal
{

    /// <summary>
    /// Represents a renderer for filter checkboxes in a grid.
    /// </summary>
    public partial class FilterCheckBoxRenderer<TContent>
    {
        private SfDialog? ExcelDialog { get; set; }

        /// <summary>
        /// Gets or sets the dialog animation effects for the filter checkbox.
        /// </summary>
        public DialogEffect Effects { get; set; } = DialogEffect.None;

        private bool VisibleProperty { get; set; }

        private ExcelBase<TContent>? ExcelBaseRef { get; set; }

        /// <summary>
        /// Gets or sets the parent grid.
        /// </summary>
        [CascadingParameter]
        public SfGrid<TContent>? Parent { get; set; }

        /// <summary>
        /// Gets or sets the horizontal position value for the filter dialog.
        /// </summary>
        public string Xvalue { get; set; } = "right";

        /// <summary>
        /// Gets or sets the vertical position value for the filter dialog.
        /// </summary>
        public string Yvalue { get; set; } = "bottom";

        /// <summary>
        /// Gets or sets the column associated with the filter checkbox.
        /// </summary>
        [Parameter]
        public GridColumn? Column { get; set; }

        private object? InputValue { get; set; }

        private bool SelectAllChk { get; set; }

        private bool IsCurrentSelectionChecked { get; set; }

        private bool FullDataEmpty { get; set; }

        private string InputFocus { get; set; } = string.Empty;

        private string XPosition { get; set; } = "center";

        private string YPosition { get; set; } = "center";

        private List<CheckBoxModel> FilteredData { get; set; } = new List<CheckBoxModel>();

        private List<CheckBoxModel> AddPredicate { get; set; } = new List<CheckBoxModel>();

        private Dictionary<string,List<CheckBoxModel>> DistinctPredicate { get; set; } = new Dictionary<string, List<CheckBoxModel>>();

        private bool Intermediate { get; set; }

        private bool isCurrentSelecitonFilter { get; set; }

        private PropertyInfoHelper PropertyHelper { get; set; } = new PropertyInfoHelper();
        private ISyncfusionStringLocalizer? Localizer;

        private Dictionary<string, object> CheckedData { get; set; } = new Dictionary<string, object>();
        private List<KeyValuePair<string, object>> _checkboxVirtualizationData { get; set; } = new List<KeyValuePair<string, object>>();

        private IEnumerable<object>? CheckboxListData { get; set; }

        private Dictionary<string, object> InitialBindedData { get; set; } = new Dictionary<string, object>();

        private HashSet<object?>? AdvancedFilteredValues { get; set; }

        private bool FilterDialogInitialRendering { get; set; }

        private bool IsClearIconPressed { get; set; }


        private bool IsOkButtonDisabled { get; set; }

        private int FilterChoiceCount { get; set; }

        private string? NullKey { get; set; }

        private object? NullData { get; set; }

        private bool NullChecked { get; set; }

        private bool isBlank { get; set; }

        private bool isSearch { get; set; }

        private string CancelIcon { get; set; } = string.Empty;

        private string SearchValue { get; set; } = string.Empty;

        private string excelSearchOperator { get; set; } = "none";

        private List<PredicateModel<object>> predicateModels { get; set; } = new List<PredicateModel<object>>();

        /// <summary>
        /// Tracks whether the dialog was toggled closed by clicking the filter icon.
        /// Used to implement toggle behavior: first click opens, second click closes.
        /// </summary>
        private bool _isDialogToggledClosed { get; set; }

        // ----- On-demand loading (BLZ-FILT-007..019) -----
        // Private state for chunked loading of the checkbox value list inside
        // FilterType.Excel / FilterType.CheckBox dialogs. These fields are
        // owned by the renderer (per-dialog lifetime) and reset on each
        // OpenFilterDialog via CancelChunkRequestAsync → fresh _chunkCts.
        private int _loadedDistinctCount { get; set; }
        private int _nextSourceOffset { get; set; }
        private int _initialDistinctCount { get; set; }
        private int _chunkSize { get; set; } = 50;
        private int _virtualWindowStart { get; set; }
        private int _virtualWindowEnd { get; set; }
        private int _virtualRowHeight { get; set; } = 32;
        private int _virtualFirstVisibleIndex { get; set; }
        private int _virtualVisibleCount { get; set; } = 10;
        private bool _isLoadingNextChunk { get; set; }
        private CancellationTokenSource? _chunkCts { get; set; }
        private int _chunkGeneration { get; set; }
        private bool _scrollListenerBound { get; set; }
        private List<object>? _customCheckboxDataSource { get; set; }
        private List<object>? _customSearchCheckboxDataSource { get; set; }
        private int _legacyFilterChoiceCount { get; set; }
        private bool _lastChunkedLoadingActive { get; set; }
        private bool _bindChunkListenerAfterRender { get; set; }
        private int _announceToken { get; set; }
        // Client-side dedup window across server-paged chunks: a column value
        // that appears in chunk 0 and chunk 1 (e.g. "US") would otherwise
        // show twice. We keep the first occurrence and drop later ones
        // (Option B from the design discussion). Per-dialog lifetime; reset
        // on each OpenFilterDialog via ResolveChunkSizeForDialog.
        private HashSet<string> _seenDistinctKeys { get; set; } = new HashSet<string>(StringComparer.Ordinal);
        // True while the server still has more rows to page through. With
        // server paging, CheckboxListData only holds the current page so the
        // chunk-end guard (_loadedDistinctCount >= CheckboxListData.Count())
        // no longer reflects end-of-data — we set this from the page size
        // and (when available) the DataResult.Count total.
        private bool _hasMoreChunks { get; set; } = true;

        // NEW: Throttle scroll events to prevent infinite triggering during momentum scroll
        // Track the last index that triggered a load to prevent re-triggering for the same position
        private int _lastTriggeredChunkIndex { get; set; } = -1;

        private const int DefaultFilterDialogChunkSize = 50;
        private const int DefaultFilterDialogVirtualWindowSize = 20;
        private const int VirtualWindowOverscanCount = 5;


        //AdaptiveDialog Section Started
        private SfDialog? CheckBoxAdaptiveDialog { get; set; }

        private bool IsContextMenuItemSelected { get; set; } = true;
        private bool IsBackButtonTriggered { get; set; }        
      

        private Dictionary<string, object> MaxHeight = new Dictionary<string, object>()
     {
        { "data-sf-style", "max-height:100%"}
    };
       
        private async Task AdaptiveRendered()
        {
            VisibleProperty = true;
            // Reset toggle state when adaptive dialog is rendered
            Parent!.FilterModule!.ResetColumnDialogState(Column!.Uid);
            await OpenFilterDialog(XPosition, YPosition).ConfigureAwait(true);
            Parent.FilterModule.FilterDialogInstance = CheckBoxAdaptiveDialog;
            await Parent.InvokeMethod("sfBlazor.Grid.customFilterDialog", new object[] { Parent.DataId, $"{Column?.Uid}customcheckboxfilter", Parent.FilterModule.GetFilterType(Column!) == "Excel" }).ConfigureAwait(true);
            VisibleProperty = false;
        }

        internal void CloseHandler()
        {
            // Reset toggle state on adaptive dialog close
            Parent!.FilterModule!.ResetColumnDialogState(Column!.Uid);
            _isDialogToggledClosed = false;

            IsBackButtonTriggered = true;
            if(Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = false;
            }
                      
            Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
        }

        internal string GetContentDialogClassNames() 
        {
            string styleText = "e-filter-popup";

            if (Parent?.FilterModule != null && Parent.FilterModule.GetFilterType(Column!) == "Excel")
            {
                styleText = string.Concat(styleText, " e-excelfilter");
            }
            else
            {
                styleText = string.Concat(styleText, " e-checkboxfilter");
            }

            return styleText;
        }

        internal string GetAdaptiveDialogClassNames() 
        {
            string styleText = " e-resfilterdiv e-bigger e-responsive-dialog  e-resfilter e-row-responsive-filter e-dlg-fullscreen";

            if (Parent!.FilterModule != null &&  Parent.FilterModule.GetFilterType(Column!) == "Excel")
            {
                styleText = string.Concat(styleText, " e-excelfilter");
            }
            else
            {
                styleText = string.Concat(styleText, " e-bigger e-checkboxfilter");
            }
            return styleText;
        }

        private void OpenCheckboxDialog(object args)
        {
            IsContextMenuItemSelected = true;
            StateHasChanged();
        }

        private void CloseCheckboxDialog(object args)
        {
            IsContextMenuItemSelected = false;
            StateHasChanged();
        }

        //AdaptiveDialog Section Ended

        /// <summary>
        /// Initializes the component and registers event handlers for checkbox dialogs.
        /// </summary>
        protected override void OnInitialized()
        {
            Localizer = Parent!.Localizer;
            CheckedData?.Clear();
            _lastChunkedLoadingActive = IsChunkedLoadingActive();
            Parent.EventAggregator.Add("CloseAdaptiveCheckBoxDialog", CloseCheckboxDialog);
            Parent.EventAggregator.Add("OpenAdaptiveCheckBoxDialog", OpenCheckboxDialog);
        }

        /// <inheritdoc />
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            var chunkedLoadingActive = IsChunkedLoadingActive();
            if (chunkedLoadingActive == _lastChunkedLoadingActive)
            {
                return;
            }

            _lastChunkedLoadingActive = chunkedLoadingActive;
            if (Parent?.FilterModule == null || Column == null ||
                !Parent.FilterModule.IsFilterDialogOpen(Column.Uid))
            {
                return;
            }

            if (!chunkedLoadingActive)
            {
                var legacyCustomData = _customCheckboxDataSource;
                await CancelChunkRequestAsync().ConfigureAwait(true);
                FilterChoiceCount = _legacyFilterChoiceCount;
                await UpdateDataSource(false, legacyCustomData).ConfigureAwait(true);
                await InvokeAsync(StateHasChanged).ConfigureAwait(true);
                return;
            }

            _legacyFilterChoiceCount = FilterChoiceCount;
            _customCheckboxDataSource = CheckboxListData?.ToList();
            _chunkCts = new CancellationTokenSource();
            ResolveChunkSizeForDialog();
            FilterChoiceCount = _chunkSize;
            CheckedData.Clear();
            FilteredData.Clear();
            CheckboxListData = null;
            await UpdateDataSource(false, _customCheckboxDataSource, _chunkSize).ConfigureAwait(true);
            await LoadInitialChunkAsync(_chunkCts.Token).ConfigureAwait(true);
            _bindChunkListenerAfterRender = true;
        }

        /// <inheritdoc />
        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            await base.OnAfterRenderAsync(firstRender).ConfigureAwait(true);
            if (!_bindChunkListenerAfterRender)
            {
                return;
            }

            _bindChunkListenerAfterRender = false;
            if (IsChunkedLoadingActive() && _chunkCts != null)
            {
                await BindScrollListenerIfNeeded(_chunkCts.Token).ConfigureAwait(true);
            }
        }

        private static Operator GetOperator(string value)
        {
            return Filter<object>.GetOperator(value);
        }
        private async Task FilterBtnClick()
        {
            if(Parent!.FilterModule != null)
            {
                Parent.FilterModule.ExcelDialog = true;
            }      
            var CheckedCount = GetCheckedDataCount();
            if(Parent.FilterModule != null)
            {
                // CRITICAL FIX: Reset toggle state BEFORE setting FilterIconIsClicked to false
                // and BEFORE hiding the dialog. If we don't do this here, the parent component
                // (FilterType.razor) will destroy FilterCheckBoxRenderer before Closed() event fires,
                // preventing proper state cleanup.
                Parent.FilterModule.ResetColumnDialogState(Column!.Uid);
                Parent.FilterModule.FilterIconIsClicked = false;
            }          
            if (!Parent.EnableAdaptiveUI)
            {
                await (ExcelDialog?.HideAsync()!).ConfigureAwait(true);
            }
            else
            {
                IsBackButtonTriggered = true;
                Parent.EventAggregator.Trigger("CloseAdaptiveDialog", null!);
                Parent.EventAggregator.Trigger("CloseColumnMenuAdaptiveDialog", null!);
            }
            if( Parent.SelectionModule != null &&!string.IsNullOrEmpty(Parent.SelectionModule.IsSelectFilteredField))
            {
                Parent.SelectionModule.IsSelectFilteredField = string.Empty;
                Parent.SelectionModule.IsHeaderCheckboxChecked = false;
            }
            if (CheckedCount == FilteredData?.Count && Parent.FilterModule != null)
            {
                await Parent.FilterModule.RemoveFilterColumnByField(Column?.Field!).ConfigureAwait(true);
            }
            else
            {
                var isNotEqual = FilteredData?.Count != CheckedCount && FilteredData?.Count - CheckedCount < CheckedCount;
                if (isNotEqual)
                {
                    var unCheckCount = GetUnCheckedDataCount();
                    await GeneratePredicate("notequal", unCheckCount).ConfigureAwait(true);
                    await Parent.ModelChanged(new ActionEventArgs<TContent>() { Columns = predicateModels, CurrentFilteringColumn = Column!.Field, RequestType = Grids.Action.Filtering }, requestType: "Filtering", eventArgs: new FilteringEventArgs() { FilterPredicates = predicateModels, ColumnName = Column.Field }).ConfigureAwait(true);
                }
                else
                {
                    await GeneratePredicate("equal", CheckedCount).ConfigureAwait(true);
                    await Parent.ModelChanged(new ActionEventArgs<TContent>() { Columns = predicateModels, CurrentFilteringColumn = Column!.Field, RequestType = Grids.Action.Filtering }, requestType: "Filtering", eventArgs: new FilteringEventArgs() { FilterPredicates = predicateModels, ColumnName = Column.Field }).ConfigureAwait(true);
                }
            }

            if (Parent.ColumnMenuInstance != null && Parent.FilterModule != null)
            {
                Parent.FilterModule.HideColumnMenuPopup();
            }

            Parent.FilterModule!.ExcelDialog = false;
        }

        internal object? GetActualValue(object value)
        {
            if (Column?.ValueType?.Namespace != "System")
                return value?.ToString();
            switch (Column?.Type)
            {
                case ColumnType.Integer:
                case ColumnType.Double:
                case ColumnType.Long:
                case ColumnType.Decimal:
                    var IsInteger = Parent!.FilterModule!.IntConvertedList.ContainsKey(Column.ValueType);
                    var isLong = Parent.FilterModule.LongConvertedList.ContainsKey(Column.ValueType);
                    if (IsInteger)
                    {
                        if (int.TryParse(value?.ToString(), out int temp) == true)
                        {
                            value = SfBaseUtils.ChangeType(value, Column?.ValueType!);
                        }
                    }
                    else if (isLong)
                    {
                        if (long.TryParse(value?.ToString(), out long temp) == true)
                        {
                            value = SfBaseUtils.ChangeType(value, Column?.ValueType!);
                        }
                    }
                    else if (double.TryParse(value?.ToString(), out double temp) == true)
                    {
                        value = SfBaseUtils.ChangeType(value, Column?.ValueType!);
                    }
                    else
                    {
                        value = "grd-search-notvalid";
                        isSearch = true;
                        CheckedData?.Clear();
                    }
                    break;
                case ColumnType.Date:
                case ColumnType.DateTime:
                    if (DateTime.TryParseExact(value?.ToString(), Column?.Format, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime Temp) == true)
                    {
                        value = Temp;
                    }
                    else if(value != null && !Parent!.DataManager!.DataAdaptor!.IsRemote())
                    {
                        value = value.ToString()!;
                    }
                    else
                    {
                        value = "grd-search-notvalid";
                        CheckedData?.Clear();
                        isSearch = true;
                    }
                    break;
                case ColumnType.Boolean:
                    if (bool.TryParse((string)value, out bool b))
                    {
                        value = SfBaseUtils.ChangeType(value, Column?.ValueType!);
                    }
                    else
                    {
                        value = "grd-search-notvalid";
                        CheckedData?.Clear();
                        isSearch = true;
                    }
                    break;
                case ColumnType.None:
                case ColumnType.String:
                    value = SfBaseUtils.ChangeType(value, Column?.ValueType!);
                    break;
            }
            return value;
        }

        private async Task GeneratePredicate(string Operator, int checkcount = 0)
        {
            // Use a local non-null column reference to avoid redundant null-condition checks
            var column = Column;
            if (column == null)
            {
                // If Column is not available, nothing to generate
                return;
            }

            var checkCount = checkcount;
            var Data = GetCheckChangeValues(Operator);
            var predicate = Operator == "notequal" ? "and" : "or";
            PredicateModel<object> predicateModel = new PredicateModel<object>();
#pragma warning disable BL0005
            if (Parent!.FilterSettings!.Columns == null)
            {
                Parent.FilterSettings.Columns = new List<GridFilterColumn>();
            }
#pragma warning restore BL0005
            if (IsCurrentSelectionChecked)
            {
                var values = DistinctPredicate[column.Uid];
                foreach (var v in Data)
                {
                    var temp = values?.Find(x => x.Value != null && x.Value.Equals(v.Value));
                    if (temp != null)
                    {
                        values?.Remove(temp);
                        values?.Add(v);
                    }
                }

                var unCheckDatas = GetUnCheckChangeValues();
                foreach (var v in unCheckDatas)
                {
                    var temp = values?.Find(x => x.Value?.Equals(v.Value) == true);
                    if (temp != null)
                    {
                        values?.Remove(temp);
                        values?.Add(v);
                    }
                }

                DistinctPredicate?.Remove(column.Uid);
                DistinctPredicate?.Add(column.Uid, values!);
                RemoveExistingPredicate();

                if (DistinctPredicate?.TryGetValue(column.Uid, out var value) == true)
                {
                    foreach (var val in DistinctPredicate[column.Uid])
                    {
                        var operators = val.isChecked ? "equal" : "notequal";
                        var predict = val.isChecked ? "or" : "and";
                        if (val != null && val.isChecked && (val.FUid == column.Uid))
                        {
                            using var filterColumn = new GridFilterColumn()
                            {
#pragma warning disable BL0005
                                Field = column.IsForeignColumn() == true ? column.ForeignKeyValue! : column.Field!,
                                Operator = Filter<TContent>.GetOperator(operators),
                                Value = val.Value,
                                MatchCase = true,
                                IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                                Uid = column.Uid!,
                                Predicate = "or",
                                ColumnType = Filter<TContent>.GetColumnType(column.Type)!
#pragma warning restore BL0005
                            };

                            predicateModel = new PredicateModel<object>()
                            {
                                Field = (column.IsForeignColumn() == true ? column.ForeignKeyValue : column.Field)!,
                                Operator = Filter<TContent>.GetOperator(operators),
                                Value = val.Value,
                                MatchCase = true,
                                IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                                Uid = column.Uid!,
                                Predicate = "or"
                            };
                            
                            predicateModels?.Add(predicateModel);
                            Parent.FilterSettings.Columns?.Add(filterColumn);
                        }
                    }
                }
            }     
            else
            {
                RemoveExistingPredicate();

                foreach (var Fval in Data)
                {
                    if (Fval != null && Fval.Value != null && Fval.Value.ToString() != "null" && !string.IsNullOrEmpty(Fval.Value.ToString()))
                    {
                        using var filterColumn = new GridFilterColumn()
                        {
#pragma warning disable BL0005
                            Field = (column.IsForeignColumn() == true ? column.ForeignKeyValue : column.Field)!,
                            Operator = Filter<TContent>.GetOperator(Operator),
                            Value = Fval.Value,
                            MatchCase = true,
                            IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                            Uid = column.Uid!,
                            Predicate = predicate,
                            ColumnType = Filter<TContent>.GetColumnType(column.Type)!
#pragma warning restore BL0005
                        };
                        predicateModel = new PredicateModel<object>()
                        {
                            Field = (column.IsForeignColumn() == true ? column.ForeignKeyValue : column.Field)!,
                            Operator = Filter<TContent>.GetOperator(Operator),
                            Value = Fval.Value,
                            MatchCase = true,
                            IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                            Uid = column.Uid!,
                            Predicate = predicate
                        };                        
                        predicateModels?.Add(predicateModel);
                        Parent.FilterSettings.Columns?.Add(filterColumn);
                    }
                    else
                    {
                        NullValueFiltering(Operator, column.ValueType!);
                    }
                }
            }
            
            await Parent.FilterSettings.UpdateProperties("Columns", Parent.FilterSettings.Columns!).ConfigureAwait(true);
        }

        internal List<CheckBoxModel> GetCheckChangeValues(string Operator)
        {
            bool equal = Operator == "notequal" ? false : true;
            List<CheckBoxModel> Model = new List<CheckBoxModel>();
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (data?.isChecked == equal)
                {
                    Model.Add(data);
                }
            }
            return Model;
        }

        internal List<CheckBoxModel> GetUnCheckChangeValues()
        {
            List<CheckBoxModel> Model = new List<CheckBoxModel>();
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (!data.isChecked)
                {
                    Model.Add(data);
                }
            }
            return Model;
        }

        internal void NullValueFiltering(string opr, Type valueType)
        {
            object[] Values = new object[] { null! };
            int nullCount = 1;
            var predicate = opr == "notequal" ? "and" : "or";
            if (Column!.Type == ColumnType.String)
            {
                Values = new object[] { null!, string.Empty };
                nullCount = 2;
            }

            PredicateModel<object> predicateModel = new PredicateModel<object>();
            for (var n = 0; n < nullCount; n++)
            {
                using var filterColumn = new GridFilterColumn()
                {
#pragma warning disable BL0005
                    Field = (Column?.IsForeignColumn() == true ? Column.ForeignKeyValue : Column?.Field)!,
                    Operator = Filter<TContent>.GetOperator(opr),
                    Value = Values[n],
                    MatchCase = true,
                    IgnoreAccent = Parent!.FilterSettings!.IgnoreAccent,
                    Uid = Column?.Uid!,
                    Predicate = predicate
#pragma warning restore BL0005
                };
                predicateModel = new PredicateModel<object>()
                {
                    Field = (Column?.IsForeignColumn() == true ? Column.ForeignKeyValue : Column?.Field)!,
                    Operator = Filter<TContent>.GetOperator(opr),
                    Value = Values[n],
                    MatchCase = true,
                    IgnoreAccent = Parent.FilterSettings.IgnoreAccent,
                    Uid = Column?.Uid!,
                    Predicate = predicate
                };
                predicateModels?.Add(predicateModel);
                Parent.FilterSettings.Columns?.Add(filterColumn);
            }
        }

        internal void RemoveExistingPredicate()
        {
            if (Parent!.FilterSettings!.Columns != null)
            {
                int Colcount = Parent.FilterSettings.Columns.Count;
                for (var i = Colcount - 1; i >= 0; i--)
                {
                    if (Parent.FilterSettings.Columns[i]?.Uid == Column?.Uid)
                    {
                        Parent.FilterSettings.Columns.RemoveAt(i);
                    }
                }
            }
        }

        private async Task ClearBtnClick()
        {
            Parent!.FilterModule!.ExcelDialog = true;
            Parent.FilterModule.FilterIconIsClicked = false;
            if (!Parent.EnableAdaptiveUI)
            {
                if (ExcelDialog != null)
                {
                    await ExcelDialog.HideAsync().ConfigureAwait(true);
                }                
            }
            else
            {
                IsBackButtonTriggered = true;
                Parent.EventAggregator.Trigger("CloseAdaptiveDialog", null!);
            }
            if (Parent.ColumnMenuInstance != null)
            {
                Parent.FilterModule.HideColumnMenuPopup();
            }

            var filterType = Parent.FilterModule.GetFilterType(Column!);
            if (filterType == "Excel" || filterType == "CheckBox")
            {
                if (filterType == "Excel")
                {
                    Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                }
                Parent.FilterModule.ExcelDialog = false;
            }
            if (filterType != "Excel" || Parent.EnableAdaptiveUI)
            {
                await Parent.FilterModule.RemoveFilterColumnByField(Column?.Field!, Column?.Uid!, foreginKeyFieldName: Column!.ForeignKeyValue!).ConfigureAwait(true);
            }
        }

        private async Task DialogCreated()
        {
            if (Parent!.FocusModule != null)
            {
                Parent.FocusModule.IsSelectAllClicked = false;
            }
            VisibleProperty = true;
            if (Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = true;
                Parent.FilterModule.FilterDialogInstance = ExcelDialog;
            }

            if (Parent.FilterModule != null && Parent.FilterModule.FilterIconIsClicked && Parent.FilterModule.FilterIconColumn != null)
            {
                if (Parent.FilterModule.IsColumnMenuFilter)
                {
                    var dlgID = Column?.Uid + "_excelDlg";
                    string[] positions = await Parent.InvokeMethod<string[]>("sfBlazor.Grid.filterPopupRender", false, new object[] { Parent.DataId, dlgID, Column?.Uid!, "excel", Parent.FilterModule.IsColumnMenuFilter }).ConfigureAwait(true);
                    if (positions != null)
                    {
                        XPosition = positions[0];
                        YPosition = positions[1];
                        await OpenFilterDialog(XPosition, YPosition).ConfigureAwait(true);
                    }
                    VisibleProperty = false;
                }
                else
                {
                    await OpenFilterDialog().ConfigureAwait(true);
                    VisibleProperty = false;
                }
            }
        }
        
        /// <summary>
        /// Toggle the filter dialog open/closed based on current state.
        /// Called by filter icon click handler to implement toggle behavior.
        /// </summary>
        internal async Task OnFilterIconClickAsync()
        {
            bool isAlreadyOpen = Parent!.FilterModule!.IsFilterDialogOpen(Column!.Uid);
            
            if (isAlreadyOpen)
            {
                // Toggle close: hide dialog
                _isDialogToggledClosed = true;
                if (ExcelDialog != null)
                    await ExcelDialog.HideAsync().ConfigureAwait(true);
                if (CheckBoxAdaptiveDialog != null)
                    await CheckBoxAdaptiveDialog.HideAsync().ConfigureAwait(true);
            }
            else
            {
                // Open dialog and track state
                _isDialogToggledClosed = false;
                Parent.FilterModule.SetFilterDialogOpen(Column!.Uid);
                await OpenFilterDialog().ConfigureAwait(true);
            }
        }
        
        internal async Task Closed()
        {
            // CRITICAL: Reset toggle state on ANY dialog close
            Parent!.FilterModule!.ResetColumnDialogState(Column!.Uid);
            _isDialogToggledClosed = false;

            // Mirror OncloseDialog — cancel any in-flight chunk request
            // when the dialog fully closes (BLZ-FILT-011).
            await CancelChunkRequestAsync().ConfigureAwait(true);

            if (Parent.FilterModule != null && Parent.FocusModule != null && !Parent.FilterModule.IsSubMenuClick)
            {
                Parent.FilterModule.FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                Parent.FocusModule.Focus("", "", headerUid: Column?.Uid).GetAwaiter();
            }
        }

        internal async Task OncloseDialog(BeforeCloseEventArgs args)
        {
            // Reset toggle state when dialog closes
            Parent!.FilterModule!.ResetColumnDialogState(Column!.Uid);
            _isDialogToggledClosed = false;

            if(args?.ClosedBy == "Escape" && Parent.FilterSettings!.Type == FilterType.Excel)
            {
                ExcelBaseRef?.Dispose();
            }

            // Cancel any in-flight chunk request when the dialog is closing.
            // BLZ-FILT-011 / BLZ-FILT-018: cancellation is the expected path
            // here, never an error.
            await CancelChunkRequestAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Opens the filter dialog at the specified position and triggers related events before rendering.
        /// </summary>
        public async Task OpenFilterDialog(string XPosition = "", string YPosition = "")
        {
            FilterDialogInitialRendering = true;
            var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterBeforeOpen, Cancel = false, ColumnName = Column?.Field!, Parent = Parent, CheckboxListData = null!};
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent!.GridEvents?.OnActionBegin!, actionArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
            FilterDialogOpeningEventArgs dialogOpeningArgs = new FilterDialogOpeningEventArgs()
            {
                ColumnName = this.Column?.Field!,
                Parent = Parent,
                FilterChoiceCount = FilterChoiceCount,
            };
            if ((Parent.GridEvents != null && Parent.GridEvents!.FilterDialogOpening.HasDelegate) || Parent.IsRenderedFromTreeGrid)
            {
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("FilterDialogOpening", dialogOpeningArgs).ConfigureAwait(true);
                else if(Parent.GridEvents != null)
                    await Parent.GridEvents.FilterDialogOpening.InvokeAsync(dialogOpeningArgs).ConfigureAwait(true);
                if (dialogOpeningArgs.Cancel)
                {
                    Parent.FilterModule!.FilterIconIsClicked = false;
                    // CRITICAL: Reset toggle state to prevent stale OpenFilterColumnUid
                    // from causing the next click to be misinterpreted as a toggle-close
                    if (Column != null && !string.IsNullOrEmpty(Column.Uid))
                    {
                        Parent.FilterModule!.ResetColumnDialogState(Column.Uid);
                    }
                    Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                    return;
                }
            }
            if (dialogOpeningArgs.Cancel)
            {
                Parent.FilterModule!.FilterIconIsClicked = false;
                // CRITICAL: Reset toggle state to prevent stale OpenFilterColumnUid
                // from causing the next click to be misinterpreted as a toggle-close
                if (Column != null && !string.IsNullOrEmpty(Column.Uid))
                {
                    Parent.FilterModule!.ResetColumnDialogState(Column.Uid);
                }
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                return;
            }
            if (actionArgs.Cancel)
            {
                Parent.FilterModule!.FilterIconIsClicked = false;
                // CRITICAL: Reset toggle state to prevent stale OpenFilterColumnUid
                // from causing the next click to be misinterpreted as a toggle-close
                if (Column != null && !string.IsNullOrEmpty(Column.Uid))
                {
                    Parent.FilterModule!.ResetColumnDialogState(Column.Uid);
                }
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                return;
            }

            if (Parent.FilterModule!.IsColumnMenuFilter)
            {
                Xvalue = XPosition;
                Yvalue = YPosition;
            }

            if (IsChunkedLoadingActive())
            {
                await CancelChunkRequestAsync().ConfigureAwait(true);
                _legacyFilterChoiceCount = actionArgs.FilterChoiceCount > 0
                    ? actionArgs.FilterChoiceCount
                    : dialogOpeningArgs.FilterChoiceCount;
                _chunkCts = new CancellationTokenSource();
                ResolveChunkSizeForDialog(dialogOpeningArgs.FilterChoiceCount);
                FilterChoiceCount = _chunkSize;
                CheckboxListData = null;
                FilteredData.Clear();
            }
            if (CheckBoxAdaptiveDialog != null && Parent.EnableAdaptiveUI && ((Parent.AdaptiveUIMode.Equals(AdaptiveMode.Both)) || (Parent.SyncfusionService.IsDeviceMode && Parent.AdaptiveUIMode.Equals(AdaptiveMode.Mobile)) || (Parent.AdaptiveUIMode.Equals(AdaptiveMode.Desktop) && Parent.SyncfusionService?.IsDeviceMode != true) ))
            {
                await CheckBoxAdaptiveDialog.ShowAsync().ConfigureAwait(true);
            }
            else
            {
                StateHasChanged();
                if (ExcelDialog != null)
                {
                    await ExcelDialog.ShowAsync().ConfigureAwait(true);
                }
            }
            await UpdateDataSource(false, actionArgs.CheckboxListData ?? dialogOpeningArgs.CheckboxListData, IsChunkedLoadingActive() ? _chunkSize : dialogOpeningArgs.FilterChoiceCount).ConfigureAwait(true);
            if (IsChunkedLoadingActive())
            {
                await LoadInitialChunkAsync(_chunkCts!.Token).ConfigureAwait(true);
            }
            StateHasChanged();
            if (IsChunkedLoadingActive() && _chunkCts != null)
            {
                await BindScrollListenerIfNeeded(_chunkCts.Token).ConfigureAwait(true);
            }
            
            if (Parent.FilterModule != null && Column != null && Parent.FilterModule.GetFilterType(Column) == "Excel")
            {
                await Parent.InvokeMethod("sfBlazor.Grid.focusExcelInput", new object[] { Parent.DataId, Column.Uid}).ConfigureAwait(true);
            }

            var CompleteArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterAfterOpen, Cancel = false, ColumnName = Column?.Field!, Parent = Parent };
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent.GridEvents?.OnActionComplete!, CompleteArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionComplete", CompleteArgs).ConfigureAwait(true);
            if (actionArgs.Cancel)
            {
                Parent.FilterModule!.FilterIconIsClicked = false;
                Parent.EventAggregator.Trigger("FilterComponentUpdate", null!);
                return;
            }
            if ((Parent.GridEvents != null && Parent.GridEvents.FilterDialogOpened.HasDelegate) || Parent.IsRenderedFromTreeGrid)
            {
                var dialogOpenedArgs = new FilterDialogOpenedEventArgs() 
                { 
                    ColumnName = this.Column?.Field!, 
                    Parent = Parent, 
                    FilterChoiceCount = dialogOpeningArgs.FilterChoiceCount, 
                    CheckboxListData = dialogOpeningArgs.CheckboxListData 
                };
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("FilterDialogOpened", dialogOpenedArgs).ConfigureAwait(true);
                else
                    await Parent.GridEvents!.FilterDialogOpened.InvokeAsync(dialogOpenedArgs).ConfigureAwait(true);
            }
            if(Parent.FilterModule != null)
            {
                Parent.FilterModule.FilterIconIsClicked = true;
            }
            FilterDialogInitialRendering = false;
        }

        private async void CurrentSelectionClickHandler(MouseEventArgs args)
        {
            Parent!.FocusModule!.ClickedCheckBoxId = "addCurrentSelection";
            IsCurrentSelectionChecked = !IsCurrentSelectionChecked;
            if (IsCurrentSelectionChecked)
            {
                IsOkButtonDisabled = false;
            }
            else if (!IsCurrentSelectionChecked && !(SelectAllChk || Intermediate))
            {
                IsOkButtonDisabled = true;
            }
                    
        }

        private async void CheckBoxClickHandler(MouseEventArgs args, string Key)
        {
            Parent!.FocusModule!.IsSelectAllClicked = false;
            var argument = args;
            var data = (CheckBoxModel)CheckedData?.GetValueOrDefault(Key)!;
            var CheckValue = data?.Value?.ToString();
            bool isNull = string.IsNullOrEmpty(CheckValue) || CheckValue == "null";
            Parent.FocusModule.ClickedCheckBoxId = Key;
            if (isNull || Column?.IsForeignColumn() == true)
            {
                foreach (var cData in CheckedData!)
                {
                    var nullData = (CheckBoxModel)cData.Value;
                    var nullCheckValue = nullData?.Value?.ToString();
                    bool isDataNull = string.IsNullOrEmpty(nullCheckValue) || nullCheckValue == "null";
                    if (isDataNull && isNull && nullData != null)
                    {
                        nullData.isChecked = !nullData.isChecked;
                    }
                    else if (!isDataNull && CheckValue == nullData?.Value?.ToString() && nullData != null)
                    {
                        nullData.isChecked = !nullData.isChecked;
                    }
                }
            }
            else
            {
                if(data != null)
                {
                    data.isChecked = !data.isChecked;
                }
            }

            SelectAllHandler();
        }

        internal List<WhereFilter> getPredicate(List<GridFilterColumn> columns)
        {
            var cols = DataGenerator<TContent>.Distinct(columns, "Uid", true);
            List<GridFilterColumn> collection;
            List<WhereFilter> predicate = new List<WhereFilter>();
            List<WhereFilter> foreignPred = new List<WhereFilter>();
            List<WhereFilter> predicateList = new List<WhereFilter>();
            foreach (var col in cols)
            {
                if (col?.Uid != null && col.Uid != Column?.Uid)
                {
                    var ForeignKeyFCol = GridUtils.GetColumnByFColUidOrField(col.Uid,  Parent!.IsStackedHeader ? Parent.GetColumnsAsync().Result : (List<GridColumn>)Parent.Columns!);
                    if (ForeignKeyFCol?.IsForeignColumn() == true)
                    {
                        if (col != null && Parent.DataModule != null)
                        {
                            predicateList = Parent.ForeignKeyModule!.ForeignKeyPredicates(ForeignKeyFCol, predicateList);
                        }

                        predicate.Add(new WhereFilter() { Condition = "and", IsComplex = true, predicates = predicateList });
                    }
                    else
                    {
                        collection = columns.Where(c => c.Uid == col.Uid).ToList();
                        if (collection.Count != 0)
                        {
                            predicate.Add(DataGenerator<TContent>.GeneratePredicate(collection));
                        }
                    }
                }
            }

            return predicate;
        }

        internal async Task<Query> GenerateWherePredicate(bool isForeignDataEmpty)
        {
            var isForeignKeyDataEmpty = isForeignDataEmpty;
            Query query = new Query();
            Query Foreignquery = new Query();

            Query ForeignDataquery = new Query();
            query.Distincts = new List<string>() { Column?.Field!};
            List<WhereFilter> WhereFilters = new List<WhereFilter>();
            List<WhereFilter> ForeignFilters = new List<WhereFilter>();
            List<WhereFilter> InputFilters = new List<WhereFilter>();
            FullDataEmpty = false;
            if (Parent!.FilterSettings!.Columns != null)
            {
                List<WhereFilter> CheckBoxPredicate = getPredicate(Parent.FilterSettings.Columns);
                if (CheckBoxPredicate.Count > 0)
                {
                    WhereFilters.Add(new WhereFilter() { Condition = "and", IsComplex = true, predicates = CheckBoxPredicate });
                }
            }

            if (InputValue != null && !string.IsNullOrEmpty(InputValue.ToString()))
            {
                var Field = Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field;
                if (Column?.IsForeignColumn() == true)
                {
                    ForeignFilters.Add(new WhereFilter() { Field = Field, Operator = excelSearchOperator != "none" ? excelSearchOperator : Column?.FilterSettings?.Operator?.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "contains", value = InputValue, IgnoreCase = true, Condition = "and" });
                    ForeignDataquery.Where(new WhereFilter() { Condition = "or", IsComplex = true, predicates = ForeignFilters });
                    await GenerateForeignPredicate(ForeignDataquery, Foreignquery).ConfigureAwait(true);
                    if (Foreignquery.Queries?.Where != null)
                    {
                        InputFilters.Add(new WhereFilter() { Condition = "and", IsComplex = true, predicates = Foreignquery.Queries.Where });
                    }
                    else
                    {
                        FullDataEmpty = true;
                    }
                }
                else
                {
                    if ((Parent.DataManager?.Adaptor == Adaptors.ODataV4Adaptor || Parent.DataManager?.Adaptor == Adaptors.ODataAdaptor) && Column?.ValueType != typeof(string))
                    {
                        InputFilters.Add(new WhereFilter() { Field = Field, Operator = excelSearchOperator != "none" ? excelSearchOperator : Column?.FilterSettings?.Operator?.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "equal", value = InputValue, IgnoreCase = true, Condition = "and" });
                    }
                    else
                    {
                        InputFilters.Add(new WhereFilter() { Field = Field, Operator = excelSearchOperator != "none" ? excelSearchOperator : Column?.FilterSettings?.Operator?.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture) ?? "contains", value = InputValue, IgnoreCase = true, Condition = "and" });
                    }
                }
            }

            if (WhereFilters.Count != 0 && InputFilters.Count == 0)
            {
                var condition = isSearch ? "or" : "and";
                query.Where(new WhereFilter() { Condition = condition, IsComplex = true, predicates = WhereFilters });
            }
            else if (WhereFilters.Count != 0 || InputFilters.Count != 0)
            {
                if (WhereFilters.Count != 0)
                {
                    InputFilters.Add(new WhereFilter() { Condition = "or", IsComplex = true, predicates = WhereFilters });
                }

                query.Where(new WhereFilter() { Condition = "and", IsComplex = true, predicates = InputFilters });
            }

            return query;
        }

        /// <summary>
        /// Casts the consumer-populated <see cref="GridLoadEventArgs  {T}.Result"/> back to the
        /// renderer's <typeparamref name="TPayload"/> while preserving non-generic shape.
        /// </summary>
        private static List<object>? CastCheckboxResult<TPayload>(IEnumerable? result)
        {
            if (result == null)
            {
                return null;
            }
            return result.OfType<TPayload>().Cast<object>().ToList();
        }

        private async Task UpdateDataSource(bool isInputSearch = false, IEnumerable<object>? customFilterData = null, int filterChoiceCount = 0, bool appendChunk = false, CancellationToken cancellationToken = default)
        {
            ThrowIfChunkRequestCanceled(cancellationToken);
            isSearch = isInputSearch;
            isCurrentSelecitonFilter = true;
            AdvancedFilteredValues = null;
            IEnumerable<object>? CheckboxData = null;
            bool isForeignDataEmpty = false;
            bool isChunkedPageRequest = false;
            int sourcePageCount = 0;
            bool hasMoreChunkPages = false;
            var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterChoiceRequest, ColumnName = Column!.Field, Parent = Parent };
            var filterSearchingArgs = new CheckboxFilterSearchingEventArgs() { SearchText = SearchValue, ColumnName = Column.Field, Parent = Parent, CheckboxListData = null! };

            if (!isInputSearch && !appendChunk)
            {

                isCurrentSelecitonFilter = false;

                if ((Parent!.GridEvents != null && Parent.GridEvents.OnActionBegin.HasDelegate) || Parent.IsRenderedFromTreeGrid)
                {
                    if(Parent.IsRenderedFromTreeGrid)
                        await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
                    else
                        await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent.GridEvents!.OnActionBegin, actionArgs).ConfigureAwait(true);
                }
                FilterChoiceCount = (int)actionArgs.FilterChoiceCount > 0 ? (int)actionArgs.FilterChoiceCount : filterChoiceCount;
                customFilterData = actionArgs.CheckboxListData ?? customFilterData;
                if (IsChunkedLoadingActive() && actionArgs.FilterChoiceCount > 0)
                {
                    _legacyFilterChoiceCount = actionArgs.FilterChoiceCount;
                }
                if (IsChunkedLoadingActive() && FilterChoiceCount > 0)
                {
                    _chunkSize = FilterChoiceCount;
                }

            }

            if (IsChunkedLoadingActive() && !isInputSearch && !appendChunk)
            {
                _customCheckboxDataSource = customFilterData?.ToList();
                _nextSourceOffset = 0;
            }
            else if (IsChunkedLoadingActive() && isInputSearch &&
                string.IsNullOrEmpty(SearchValue) && customFilterData != null)
            {
                _customCheckboxDataSource = customFilterData.ToList();
            }

            var customData = customFilterData?.ToList();

            if (!(InputValue != null && !string.IsNullOrEmpty(InputValue.ToString()))) 
            {
                IsCurrentSelectionChecked = false;
                isCurrentSelecitonFilter = false;
            }

            if (customData == null)
            {
                Query Foreignquery = new Query();
                object? FColDataSource = null;
                if (Column?.IsForeignColumn() == true)
                {
                    FColDataSource = await (Column?.GetData(Foreignquery.Queries)!).ConfigureAwait(true);
                    ThrowIfChunkRequestCanceled(cancellationToken);
                    isForeignDataEmpty = !((IEnumerable<object>)Column.ColumnData!).Any();
                }

                Query DataManagerQuery = await GenerateWherePredicate(isForeignDataEmpty).ConfigureAwait(true);
                ThrowIfChunkRequestCanceled(cancellationToken);
                isChunkedPageRequest = IsChunkedLoadingActive();
                var hasAdvancedFilter = Parent!.AllowAdvancedFiltering && Parent.AdvancedFilterWhereFilter?.Count > 0;
                Query GridQuery = Column?.IsForeignColumn() == true && isForeignDataEmpty
                    ? new Query()
                    : Parent!.DataModule!.GenerateQuery(hasAdvancedFilter).RequiresCount();
                var SortField = Column?.IsForeignColumn() == true ? Column?.IsForeignKeyField() == true ? Column?.Field : Column?.ForeignKeyField : Column?.Field;
                DataManagerQuery.Sort(SortField!, "ascending");

                var needSort = GridQuery?.Queries?.Sorted?.FindAll(x => x?.Name == SortField);
                if (needSort == null || needSort.Count == 0)
                {
                    GridQuery?.Sort(SortField!, "ascending");
                }

                DataManagerQuery.Queries.Search = GridQuery?.Queries?.Search;
                DataManagerQuery.Queries.RequiresCounts = true;
                if (Parent!.Query != null && Parent.Query.Queries.Where != null)
                {
                    if (DataManagerQuery.Queries.Where == null)
                    {
                        DataManagerQuery.Queries.Where = Parent.Query.Queries.Where;
                    }
                    else
                    {
                        foreach (var where in Parent.Query.Queries.Where)
                        {
                            DataManagerQuery.Queries.Where.Add(where);
                        }
                    }
                }
                DataManagerQuery.Queries.Params = GridQuery?.Queries?.Params;
                DataManagerQuery.Queries.Expand = GridQuery?.Queries?.Expand;
                GridQuery!.Queries.Select = DataManagerQuery.Queries.Select = new List<string>() { SortField! };
                GridQuery.Queries.RequiresCounts = true;
                // Server-side paging for the filter-dialog distinct query (BLZ-FILT-007 / GRID-DATA-005):
                // when chunked loading is active, request only one page worth of rows from the
                // underlying DataManager. ODataV4Adaptor translates this to $skip/$top; WebApiAdaptor
                // translates to skip/take; BlazorAdaptor applies LINQ Skip/Take client-side anyway.
                // UrlAdaptor behavior depends on the consumer's URL template.
                if (isChunkedPageRequest)
                {
                    GridQuery!.Skip(_nextSourceOffset);
                    GridQuery!.Take(_chunkSize);
                    DataManagerQuery.Skip(_nextSourceOffset);
                    DataManagerQuery.Take(_chunkSize);
                }
                // Event-driven data source: invoke OnLoadData when subscribed for the full sample,
                // otherwise fall back to executing the DataManager query directly.
                IEnumerable<object>? tmpFullData = null;
                if (Parent.GridEvents?.OnDataLoad.HasDelegate == true)
                {
                    var fullEventArgs = await EventDrivenDataHandler<TContent>.InvokeDataRequestAsync(DataManagerQuery).ConfigureAwait(true);
                    ThrowIfChunkRequestCanceled(cancellationToken);
                    await InvokeAsync( () => Parent.GridEvents.OnDataLoad.InvokeAsync(fullEventArgs)).ConfigureAwait(true);
                    ThrowIfChunkRequestCanceled(cancellationToken);
                    tmpFullData = CastCheckboxResult<TContent>(fullEventArgs?.Result);
                }
                else
                {
                    IEnumerable tmpFullDataRaw = ((DataResult)await (Parent?.DataManager?.ExecuteQuery<TContent>(DataManagerQuery)!).ConfigureAwait(true))?.Result!;
                    ThrowIfChunkRequestCanceled(cancellationToken);
                    tmpFullData = tmpFullDataRaw?.OfType<TContent>().Cast<object>().ToList()!;
                }
                var fullData = (IEnumerable<object>)tmpFullData! ?? (IEnumerable<object>)new List<TContent>();
                if (isChunkedPageRequest)
                {
                    sourcePageCount = fullData.Count();
                    if (sourcePageCount > _chunkSize)
                    {
                        sourcePageCount = _chunkSize;
                    }
                    hasMoreChunkPages = sourcePageCount >= _chunkSize;
                }
                if (DataManagerQuery.Queries.Distinct != null)
                {
                    CheckboxData = fullData;
                    if (isChunkedPageRequest)
                    {
                        CheckboxListData = CheckboxData.ToList();
                        CheckboxData = CheckboxListData.Take(_chunkSize).ToList();
                    }
                }
                else
                {
                    object? result = null;
                    if (Parent.DataManager?.Adaptor != Adaptors.BlazorAdaptor)
                    {
#pragma warning disable BL0005
                        using DataManager customDataManager = new SfDataManager() { Json = fullData, Adaptor = Adaptors.BlazorAdaptor };
#pragma warning restore BL0005
                        result = await customDataManager.ExecuteQuery<TContent>(GridQuery).ConfigureAwait(true);
                    }
                    else
                    {
                        result = await Parent.DataManager.ExecuteQuery<TContent>(GridQuery).ConfigureAwait(true);
                    }
                    ThrowIfChunkRequestCanceled(cancellationToken);

                    IEnumerable<object>? Data = result is DataResult ? (IEnumerable<object>)((DataResult)result).Result! : (IEnumerable<object>)result;
                    Data = ForeignKey<TContent>.GetForeignKeyDataSource(Column!, Data) as IEnumerable<object>;
                    if (hasAdvancedFilter && Data != null)
                    {
                        var filterField = Column?.IsForeignColumn() == true ? Column.ForeignKeyValue : Column?.Field;
                        if (!string.IsNullOrEmpty(filterField))
                        {
                            AdvancedFilteredValues = new HashSet<object?>(
                                Data.Select(item => PropertyHelper.GetObject(filterField, item)));
                        }
                    }

                    IEnumerable<object>? FullData = FullDataEmpty ? new List<IEnumerable<object>>() : fullData;
                    FullData = ForeignKey<TContent>.GetForeignKeyDataSource(Column!, FullData) as IEnumerable<object>;

                    var propertyName = Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field;
                    var DistinctData = (Data != null && propertyName != null) ? DataUtil.GetDistinct(Data, propertyName) : Enumerable.Empty<object>();
                    var DistinctFullData = FullData != null && propertyName != null && ((FullData.Count() >= DistinctData?.Cast<object>().Count()) || isSearch) ? DataUtil.GetDistinct(FullData, propertyName) : DistinctData;
                    if (isChunkedPageRequest)
                    {
                        // Server-paged chunked mode: DistinctData / DistinctFullData reflect only
                        // the current page (Data had Skip/Take applied upstream). Apply the
                        // cross-chunk dedup window (Option B) so a column value that appears in
                        // chunk 0 and chunk 1 (e.g. "US") is shown only once. End-of-data is
                        // derived from the page size: a short page means no more rows.
                        var pageDistinct = DistinctFullData?.Cast<object>().ToList() ?? new List<object>();
                        var deduped = FilterPageBySeenDistinctKeys(pageDistinct, propertyName, Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field);
                        CheckboxListData = deduped;
                        CheckboxData = deduped;
                    }
                    else
                    {
                        DistinctFullData = DistinctFullData?.Take(FilterChoiceCount == 0 ? 1000 : FilterChoiceCount);
                        CheckboxData = DistinctFullData;
                        if (CheckboxListData == null)
                        {
                            CheckboxListData = CheckboxData;
                        }
                    }
                }
            }
            else
            {
                if (appendChunk)
                {
                    // Server-paged chunked append: the consumer (parent)
                    // handed us a fresh page via customFilterData; dedup
                    // against the cross-chunk window and update the
                    // end-of-data flag.
                    var raw = customData.ToList();
                    var propName = Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field;
                    CheckboxData = FilterPageBySeenDistinctKeys(raw, propName, propName);
                    sourcePageCount = raw.Count;
                    hasMoreChunkPages = raw.Count >= _chunkSize;
                    isChunkedPageRequest = true;
                }
                else if (IsChunkedLoadingActive() && isInputSearch && !string.IsNullOrEmpty(SearchValue))
                {
                    IEnumerable<object> searchData = customData;
                    if (Column?.FilterItemTemplate == null)
                    {
                        var searchText = SearchValue.ToLower(CultureInfo.CurrentCulture);
                        searchData = customData.Where(val =>
                        {
                            var value = Parent!.PropHelper?.GetValue(val, Column?.Field!)?.ToString()?.ToLower(CultureInfo.CurrentCulture);
                            return value != null && value.Contains(searchText, StringComparison.Ordinal);
                        });
                    }

                    _customSearchCheckboxDataSource = searchData.ToList();
                    var searchPage = _customSearchCheckboxDataSource
                        .Skip(_nextSourceOffset)
                        .Take(_chunkSize)
                        .ToList();
                    var searchField = Column?.IsForeignColumn() == true ? Column.ForeignKeyValue : Column?.Field;
                    var distinctSearchPage = FilterPageBySeenDistinctKeys(searchPage, searchField, searchField);
                    CheckboxData = distinctSearchPage;
                    CheckboxListData = distinctSearchPage;
                    sourcePageCount = searchPage.Count;
                    hasMoreChunkPages = _customSearchCheckboxDataSource.Count > _nextSourceOffset + sourcePageCount;
                    isChunkedPageRequest = true;
                }
                else if (isInputSearch && !string.IsNullOrEmpty(SearchValue) && Column?.FilterItemTemplate == null)
                {
                    // Starting a new search resets the dedup window so
                    // the user sees the full search hit-set fresh.
                    _seenDistinctKeys.Clear();
                    _hasMoreChunks = false;
                    CheckboxData = customData
                    .Where(val =>
                    {
                        var value = Parent!.PropHelper?.GetValue(val, Column?.Field!)?.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        var searchText = SearchValue?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        return value != null && searchText != null && value.Contains(searchText, StringComparison.Ordinal);
                    })
                    ?.ToList();
                    CheckboxListData = CheckboxData;
                }
                else if (IsChunkedLoadingActive() && (!isInputSearch || string.IsNullOrEmpty(SearchValue)))
                {
                    var propName = Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue : Column?.Field;
                    var rawPage = appendChunk
                        ? customData
                        : customData.Take(_chunkSize).ToList();
                    var deduped = FilterPageBySeenDistinctKeys(rawPage, propName, propName);
                    CheckboxListData = deduped;
                    CheckboxData = deduped;
                    sourcePageCount = rawPage.Count;
                    hasMoreChunkPages = appendChunk
                        ? customData.Count >= _chunkSize
                        : customData.Count > rawPage.Count;
                    isChunkedPageRequest = true;
                }
                else{
                    CheckboxData = customData.Take(FilterChoiceCount == 0 ? 1000 : FilterChoiceCount); // bind the custom datasource from searchbegin event
                }
                if (CheckboxListData == null)
                {
                    CheckboxListData = CheckboxData;
                }
            }

            //For Blanks input search
            if (CheckboxListData != null && isInputSearch && customFilterData == null)
            {
                var blanksText = Localizer?.GetText(GridLocaleKeys.Blanks)?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                var distinctCheckboxData = CheckboxListData
                    .Where(val =>
                    {
                        var value = Parent!.PropHelper?.GetValue(val, Column?.Field!);
                        var searchText = SearchValue?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        return value == null && !string.IsNullOrEmpty(searchText) && blanksText?.Contains(searchText, StringComparison.Ordinal) == true;
                    })
                    ?.ToList();

                if (distinctCheckboxData?.Count > 0)
                {
                    CheckboxData ??= Enumerable.Empty<object>();
                    CheckboxData = distinctCheckboxData.Concat(CheckboxData.Except(distinctCheckboxData)).AsQueryable();
                    CheckboxData = CheckboxData.Where(list => CheckboxListData.Contains(list));
                }
            }

            if (!appendChunk)
            {
                CheckedData?.Clear();
                FilteredData.Clear();
                _checkboxVirtualizationData.Clear();
                _virtualWindowStart = 0;
                _virtualWindowEnd = DefaultFilterDialogVirtualWindowSize;
            }

            if (CheckboxData != null && !CheckboxData.Any() && Column?.Format != null && (!Column.Format.Contains('C', StringComparison.Ordinal) || !Column.Format.Contains('N', StringComparison.Ordinal)))
            {
                var FilteredValues = InitialBindedData?.Values?.Where(e => ((CheckBoxModel)e)?.FormattedValue?.ToString()?.Contains(SearchValue, StringComparison.Ordinal) == true)?.ToList();

                foreach (var data in FilteredValues!)
                {
                    var val = (CheckBoxModel)data;
                    var uniqueId = "cbox" + Guid.NewGuid().ToString();
                    var FilterValue = PropertyHelper?.GetObject(Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue!: Column?.Field!, val?.Data!);

                    var checkBoxModelData = new CheckBoxModel()
                    {
                        Data = val?.Data!,
                        Value = val?.Value!,
                        FormattedValue = val?.FormattedValue!,
                        isChecked = isForeignDataEmpty ? false : UpdateResult(FilterValue!),
                        guid = uniqueId,
                        FUid = Column?.Uid!
                    };
                    FilteredData?.Add(checkBoxModelData);
                    AddCheckboxDataItem(uniqueId, checkBoxModelData);
                }
            }
            else
            {
                UpdateCheckBoxDataItems(CheckboxData!, isForeignDataEmpty, isInputSearch);
            }
            SelectAllHandler(CheckboxData);
            if (!appendChunk)
            {
                await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent!.GridEvents?.OnActionComplete!, actionArgs).ConfigureAwait(true);
                ThrowIfChunkRequestCanceled(cancellationToken);
                await Parent!.EventAggregator.NotifyAsync("ActionComplete", actionArgs).ConfigureAwait(true);
                ThrowIfChunkRequestCanceled(cancellationToken);
            }
            if (isChunkedPageRequest)
            {
                _nextSourceOffset += sourcePageCount;
                _hasMoreChunks = hasMoreChunkPages;
            }
            if (IsChunkedLoadingActive() && !appendChunk)
            {
                _loadedDistinctCount = FilteredData?.Count ?? 0;
                _initialDistinctCount = _loadedDistinctCount;
            }
        }

        private void UpdateCheckBoxDataItems(IEnumerable<object> CheckboxData, bool isForeignDataEmpty, bool isInputSearch = false)
        {
            foreach (var value in CheckboxData)
            {
                var FilterValue = PropertyHelper?.GetObject(Column?.IsForeignColumn() == true ? Column?.ForeignKeyValue! : Column?.Field!, value);
                var uniqueId = "cbox" + Guid.NewGuid().ToString();
                var val = Column?.IsForeignColumn() == true ? FilterValue : Parent!.PropHelper?.GetValue(value, Column?.Field!);
                object? CheckBoxValue = null;
                var currentCulture = CultureInfo.CurrentCulture?.Name;

                if (val != null && val.ToString() != "null" && !string.IsNullOrEmpty(val.ToString()))
                {
                    if ((Column?.Type == ColumnType.Date || Column?.Type == ColumnType.DateTime) || Column?.Type == ColumnType.DateOnly || Column?.Type == ColumnType.TimeOnly)
                    {
                        if(val.GetType() == typeof(string))
                        { 
                            CheckBoxValue = val;
                        }
                        else if (val is DateTimeOffset)
                        {
                            CheckBoxValue = ((DateTimeOffset)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        }
                        else if (val is DateOnly)
                        {
                            CheckBoxValue = ((DateOnly)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        }
                        else if (val is TimeOnly)
                        {
                            CheckBoxValue = ((TimeOnly)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        }
                        else
                        {
                            CheckBoxValue = ((DateTime)val).ToString(Column?.Format, CultureInfo.CurrentCulture);
                        } 
                    }
                    if (Column?.Type == ColumnType.Integer || Column?.Type == ColumnType.Double || Column?.Type == ColumnType.Long || Column?.Type == ColumnType.Decimal)
                    {
                        CheckBoxValue = Convert.ToDouble(val, CultureInfo.CurrentCulture).ToString(Column?.Format, CultureInfo.CurrentCulture);
                    }
                    if (Column?.Type == ColumnType.Boolean)
                    {
                        CheckBoxValue = (bool)val ? Localizer?.GetText(GridLocaleKeys.FilterTrue) : Localizer?.GetText(GridLocaleKeys.FilterFalse);
                    }
                    if ((Column?.Type != ColumnType.Integer && Column?.Type != ColumnType.Double && Column?.Type != ColumnType.Long && Column?.Type != ColumnType.Decimal) && Column?.Type != ColumnType.Date && Column?.Type != ColumnType.DateTime && Column?.Type != ColumnType.Boolean && Column?.Type != ColumnType.DateOnly && Column?.Type != ColumnType.TimeOnly)
                    {
                        CheckBoxValue = val;
                    }
                }
                var data = new CheckBoxModel()
                {
                    Data = value,
                    Value = FilterValue!,
                    FormattedValue = CheckBoxValue!,
                    isChecked = isForeignDataEmpty ? false : UpdateResult(FilterValue!, isInputSearch),
                    guid = uniqueId,
                    FUid = Column?.Uid!
                };
                FilteredData?.Add(data);

                if (!isInputSearch)
                {
                    CheckBoxModel tempModel = AddPredicate?.Find(e => e?.Value != null && e.Value.Equals(data.Value))!;

                    if (tempModel != null)
                    {
                        AddPredicate?.Remove(tempModel);
                    }
                    AddPredicate?.Add(data);
                    if (DistinctPredicate?.ContainsKey(Column?.Uid!) != true)
                    {
                        DistinctPredicate?.Add(Column?.Uid!, AddPredicate!);
                    }
                    else if (DistinctPredicate?.ContainsKey(Column?.Uid!) == true)
                    {
                        DistinctPredicate[Column?.Uid!] = AddPredicate!;
                    }
                }

                var checkBoxVal = CheckBoxValue?.ToString();
                if ((Column?.Type != ColumnType.Integer && Column?.Type != ColumnType.Double && Column?.Type != ColumnType.Long && Column?.Type != ColumnType.Decimal) || IsClearIconPressed || (Parent!.FilterModule!.FilterIconIsClicked && SearchValue?.Length == 0))
                {
                    if (FilterDialogInitialRendering)
                    {
                        InitialBindedData?.AddOrUpdateItem(uniqueId, data);
                    }
                    AddCheckboxDataItem(uniqueId, data);
                }
                else if (CheckBoxValue != null &&
                    (Column?.FilterItemTemplate != null || ((currentCulture == "en-US" || currentCulture == "zh" || currentCulture == "ar") && (checkBoxVal?.Replace(",", "", StringComparison.Ordinal)?.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    || (currentCulture == "de" && (checkBoxVal?.Replace(".", "", StringComparison.Ordinal)?.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    || (currentCulture == "fr" && (string.Concat(checkBoxVal!.Where(c => !char.IsWhiteSpace(c))!.ToArray())!.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    || (currentCulture != "en-US" && (checkBoxVal?.Contains(SearchValue!, StringComparison.Ordinal) == true))
                    ) || CheckBoxValue == null)
                {
                    AddCheckboxDataItem(uniqueId, data);
                }
            }
        }

        internal bool UpdateResult(object fValue, bool isInputSearch = false)
        {
            if (AdvancedFilteredValues != null && !AdvancedFilteredValues.Contains(fValue))
            {
                return false;
            }

            bool isTrue = true;
            bool isFiltered = false;
            if (Parent!.FilterSettings!.Columns != null)
            {
                foreach (GridFilterColumn FCol in Parent.FilterSettings.Columns)
                {
                    if (FCol?.Uid == Column?.Uid)
                    {
                        if (Parent.FilterSettings.Columns?.Where(col => col?.Uid == Column?.Uid)?.Count() == 2)
                        {
                            var primaryFilter = Parent.FilterSettings.Columns?.Where(col => col?.Uid == Column?.Uid)?.ToList()[0];
                            var secondaryFilter = Parent.FilterSettings.Columns?.Where(col => col?.Uid == Column?.Uid)?.ToList()[1];

                            if (primaryFilter?.Operator == secondaryFilter?.Operator && primaryFilter?.Operator == Operator.NotEqual && primaryFilter?.Predicate == secondaryFilter?.Predicate && (primaryFilter?.Predicate?.Equals("or", StringComparison.Ordinal) == true))
                            {
                                return true;
                            }
                            else if (primaryFilter?.Operator == secondaryFilter?.Operator && primaryFilter?.Operator == Operator.Equal && primaryFilter?.Predicate == secondaryFilter?.Predicate && (primaryFilter?.Predicate?.Equals("and", StringComparison.Ordinal) == true))
                            {
                                return false;
                            }
                        }

                        isFiltered = true;
#pragma warning disable BL0005
                        FCol!.Value = FCol.Value?.GetType()?.Name == "JsonElement" ? SfBaseUtils.ChangeType(FCol.Value, Column?.ValueType!) : FCol.Value!;
#pragma warning restore BL0005
                        var CurrentFilterValue = FCol.Value == null ? "null" : FCol.Value;
                        fValue = fValue == null ? "null" : fValue;
                        var actualString = fValue;
                        var filterString = CurrentFilterValue;
                        if(actualString.ToString() == "null" || filterString?.ToString() == "null")
                        {
                            actualString = actualString.ToString();
                            filterString = filterString?.ToString();
                        }
                        else
                        {
                            var (actualValue, fitlerValue) = GetActualAndFilterValue(fValue, CurrentFilterValue);
                            actualString = actualValue;
                            filterString = fitlerValue;
                        }
                        if (!Parent.FilterSettings.EnableCaseSensitivity && !FCol.MatchCase && fValue is string && CurrentFilterValue is string)
                        {
                            filterString = CurrentFilterValue?.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                            actualString = fValue.ToString()?.ToLower(System.Globalization.CultureInfo.CurrentCulture);
                        }
                        IComparable comparableActualValue = (IComparable)actualString!;
                        IComparable comparableCurrentFilterValue = (IComparable)filterString!;
                        int integerValue = 0;
                        if (FCol.Value != null && IsNumericValue(filterString!) && double.TryParse(FCol.Value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double result) && actualString != null && IsNumericValue(actualString) && double.TryParse(actualString.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out double resultValue))
                        {
                            integerValue = (double.Parse(actualString.ToString()!, CultureInfo.InvariantCulture)).CompareTo(double.Parse(filterString?.ToString()!, CultureInfo.InvariantCulture));
                        }
                        else
                        {
                            integerValue = comparableActualValue!.CompareTo(comparableCurrentFilterValue);
                        }
                        if (integerValue != 0 && integerValue != 1 && integerValue != -1)
                        {
                            if (Convert.ToInt32(actualString, CultureInfo.InvariantCulture) < Convert.ToInt16(filterString, CultureInfo.InvariantCulture))
                            {
                                integerValue = -1;
                            }
                            else
                            {
                                integerValue = 1;
                            }
                        }

                        if (FCol.Operator != Operator.Equal && FCol.Operator != Operator.NotEqual)
                        {
                            if (Parent.FilterSettings.Columns?.IndexOf(FCol) == 0 && string.Equals(FCol?.Predicate, "or", StringComparison.OrdinalIgnoreCase))
                            {
                                isTrue = false;
                            }
                            isFiltered = false;
                            isTrue = ColOperator(isTrue, FCol!, integerValue, actualString?.ToString()!, filterString?.ToString()!);
                        }
                        else
                        {
                            isTrue = FCol.Operator == Operator.NotEqual ? false : true;
                            if (actualString is DateTime actual && filterString is DateTime filter)
                            {
                                if (actualString is DateTime)
                                {
                                    actualString = actual.ToString(Column?.Format, CultureInfo.CurrentCulture);
                                }
                                if (filterString is DateTime)
                                {
                                    filterString = filter.ToString(Column?.Format, CultureInfo.CurrentCulture);
                                }
                            }
                            if (actualString?.ToString() == filterString?.ToString() && !isInputSearch)
                            {
                                return isTrue;
                            }
                            else if (actualString?.ToString() == filterString?.ToString() && isInputSearch && !(InputValue != null && !string.IsNullOrEmpty(InputValue.ToString())))
                            {
                                return isTrue;
                            }

                        }
                    }
                }
                if (isInputSearch && (InputValue != null && !string.IsNullOrEmpty(InputValue.ToString())))
                {
                    return true;
                }

                return isFiltered ? !isTrue : isTrue;
            }
            return isTrue;
        }
        private static bool IsNumericValue(object filterValue)
        {
            TypeCode filterTypeCode = Type.GetTypeCode(Nullable.GetUnderlyingType(filterValue.GetType()) ?? filterValue.GetType());
            return filterTypeCode >= TypeCode.SByte && filterTypeCode <= TypeCode.Decimal;
        }
        internal (object,object) GetActualAndFilterValue(object fValue, object CurrentFilterValue)
        {
            var actualString = fValue;
            var filterString = CurrentFilterValue;
            if (fValue is int)
            {
                filterString = Convert.ToInt32(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is long)
            {
                filterString = Convert.ToInt64(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is decimal)
            {
                filterString = Convert.ToDecimal(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is float)
            {
                filterString = Convert.ToSingle(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if(fValue is short)
            {
                filterString = Convert.ToInt16(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is ushort)
            {
                filterString = Convert.ToUInt16(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is uint)
            {
                filterString = Convert.ToUInt32(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is ulong)
            {
                filterString = Convert.ToUInt64(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is byte)
            {
                filterString = Convert.ToByte(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (fValue is sbyte)
            {
                filterString = Convert.ToSByte(CurrentFilterValue, CultureInfo.InvariantCulture);
            }
            else if (Column?.Type == ColumnType.Date || Column?.Type == ColumnType.DateTime)
            {
                if (fValue is DateTimeOffset)
                {
                    actualString = GetDateTimeFromDateTimeOffset(fValue);
                }
                if (CurrentFilterValue is DateTimeOffset)
                {
                    filterString = GetDateTimeFromDateTimeOffset(CurrentFilterValue);
                }
            }
            else
            {
                actualString = fValue?.ToString();
                filterString = CurrentFilterValue?.ToString();
            }
            return (actualString!, filterString!);
        }

        private static DateTime GetDateTimeFromDateTimeOffset(object value)
        {
            DateTimeOffset fOffset = (DateTimeOffset)value;
            DateTime actualDateTime = fOffset.DateTime;
            return actualDateTime;
        }
        private static bool ColOperator(bool isTrue, GridFilterColumn FCol, int integerValue, string actualString, string filterString)
        {
            bool operatorResult;
            switch (FCol?.Operator)
            {
                case Operator.LessThan:
                    operatorResult = integerValue < 0;
                    break;
                case Operator.LessThanOrEqual:
                    operatorResult = integerValue <= 0;
                    break;
                case Operator.GreaterThan:
                    operatorResult = integerValue > 0;
                    break;
                case Operator.GreaterThanOrEqual:
                    operatorResult = integerValue >= 0;
                    break;
                case Operator.Contains:
                    operatorResult = actualString.Contains(filterString, StringComparison.Ordinal);
                    break;
                case Operator.DoesNotContain:
                    operatorResult = !actualString.Contains(filterString, StringComparison.Ordinal);
                    break;
                case Operator.StartsWith:
                    operatorResult = actualString.StartsWith(filterString, StringComparison.Ordinal);;
                    break;
                case Operator.DoesNotStartWith:
                    operatorResult = !actualString.StartsWith(filterString, StringComparison.Ordinal);
                    break;
                case Operator.EndsWith:
                    operatorResult = actualString.EndsWith(filterString, StringComparison.Ordinal);
                    break;
                case Operator.DoesNotEndWith:
                    operatorResult = !actualString.EndsWith(filterString, StringComparison.Ordinal);
                    break;
                case Operator.IsNull:
                    operatorResult = actualString == null || actualString == "null";
                    break;
                case Operator.IsNotNull:
                    operatorResult = !(actualString == null || actualString == "null");
                    break;
                case Operator.IsEmpty:
                    operatorResult = actualString != null && actualString.Length == 0;
                    break;
                case Operator.IsNotEmpty:
                    operatorResult = !(actualString == null || actualString == "null" || actualString.Length == 0);
                    break;
                default:
                    operatorResult = true;
                    break;
            }
            return string.Equals(FCol?.Predicate, "or", StringComparison.OrdinalIgnoreCase) ? (isTrue || operatorResult) : isTrue && operatorResult;
        }

        internal async Task GenerateForeignPredicate(Query DataManagerQuery, Query Foreignquery)
        {
            DataManagerQuery.Queries.RequiresCounts = true;
            IEnumerable<object> ForeignData = (IEnumerable<object>)((DataResult)await (Column?.DataManager?.ExecuteQuery<TContent>(DataManagerQuery))!.ConfigureAwait(true))?.Result!;
            Foreignquery.Distincts = new List<string>() { Column?.IsForeignColumn() == true ? Column?.ForeignKeyField! : Column?.Field! };
            Foreignquery.Queries.RequiresCounts = true;
            List<WhereFilter> WhereFilters = new List<WhereFilter>();
            foreach (var data in ForeignData!)
            {
                var FKeyValue = PropertyHelper?.GetObject(Column?.IsForeignColumn() == true ? Column?.ForeignKeyField! : Column?.Field!, data);
                WhereFilters.Add(new WhereFilter() { Field = Column?.Field, Operator = "equal", Condition = "or", value = FKeyValue, IgnoreCase = true, IgnoreAccent = true });
            }

            if (WhereFilters.Count > 0)
            {
                Foreignquery.Where(new WhereFilter() { Condition = "or", IsComplex = true, predicates = WhereFilters });
            }
        }

        internal object? GetForeignKeyValue(object data, object fdata)
        {
            var FData = GridUtils.GetForeignData(Column!, data, fdata);
            object? KeyName = null;
            foreach (var val in (List<object>)FData)
            {
                KeyName = PropertyHelper?.GetObject(Column?.ForeignKeyValue!, val);
            }

            return KeyName;
        }

        internal void SelectAllHandler(IEnumerable<object>? checkboxData = null)
        {
            int CheckCount = GetCheckedDataCount();
            int UnCheckCount = GetUnCheckedDataCount();
            
            SelectAllChk = CheckedData?.Count == CheckCount ? true : false;
            Intermediate = CheckedData?.Count != CheckCount && CheckedData?.Count != UnCheckCount ? true : false;

            if (Intermediate || SelectAllChk || IsCurrentSelectionChecked)
            {
                IsOkButtonDisabled = false;
            }
            else
            {
                IsOkButtonDisabled = !IsOkButtonDisabled;
            }

            if (checkboxData != null && !checkboxData.Any())
            {
                IsOkButtonDisabled = true;
            }
        }

        internal async void SelectAllClickHandler(string uid)
        {
            if(Parent!.FocusModule != null)
            {
                Parent.FocusModule.ClickedCheckBoxId = string.Empty;
                Parent.FocusModule.IsSelectAllClicked = true;
            }
            if (Intermediate || SelectAllChk)
            {
                IsOkButtonDisabled = true;
                SelectAllChk = false;
                Intermediate = false;
            }
            else
            {
                IsOkButtonDisabled = false;
                SelectAllChk = true;
            }

            foreach (var data in CheckedData)
            {
                var CheckData = (CheckBoxModel)data.Value;
                CheckData.isChecked = SelectAllChk;
            }
        }

        internal int GetUnCheckedDataCount()
        {
            var checkcount = 0;
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (!data.isChecked)
                {
                    checkcount++;
                }
            }

            return checkcount;
        }

        internal int GetCheckedDataCount()
        {
            var checkcount = 0;
            foreach (var val in CheckedData)
            {
                var data = (CheckBoxModel)val.Value;
                if (data.isChecked)
                {
                    checkcount++;
                }
            }

            return checkcount;
        }

        private async Task InputArgs(Microsoft.AspNetCore.Components.ChangeEventArgs args)
        {
            SearchValue = args?.Value?.ToString()!;
            var requestToken = CancellationToken.None;
            if (IsChunkedLoadingActive())
            {
                RestartChunkRequestForSearch();
                requestToken = _chunkCts!.Token;
            }
            var searchGeneration = _chunkGeneration;
            VisibleProperty = true;
            var actionArgs = new ActionEventArgs<TContent>() { RequestType = Grids.Action.FilterSearchBegin, SearchString = SearchValue, ColumnName = Column?.Field!, Parent = Parent, CheckboxListData = null! };
            await SfBaseUtils.InvokeEvent<ActionEventArgs<TContent>>(Parent!.GridEvents?.OnActionBegin!, actionArgs).ConfigureAwait(true);
            await Parent.EventAggregator.NotifyAsync("ActionBegin", actionArgs).ConfigureAwait(true);
            var filterSearchingArgs = new CheckboxFilterSearchingEventArgs() { SearchText = SearchValue!, ColumnName = Column?.Field!, Parent = Parent, CheckboxListData = null! };
            if ((Parent.GridEvents?.CheckboxFilterSearching.HasDelegate == true) || Parent.IsRenderedFromTreeGrid)
            {
                if(Parent.IsRenderedFromTreeGrid)
                    await Parent.EventAggregator.NotifyAsync("CheckboxFilterSearching", filterSearchingArgs).ConfigureAwait(true);
                else
                    await SfBaseUtils.InvokeEvent<CheckboxFilterSearchingEventArgs>(Parent.GridEvents?.CheckboxFilterSearching!, filterSearchingArgs).ConfigureAwait(true);
                
                excelSearchOperator = actionArgs.ExcelSearchOperator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture);
                if(!SearchValue!.Equals(filterSearchingArgs.SearchText, StringComparison.Ordinal))
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.searchClear", new object[] { Parent.DataId, $"{Parent.ID}_SearchBox", filterSearchingArgs.SearchText}).ConfigureAwait(true);
                }
                SearchValue = filterSearchingArgs.SearchText;
            }
            if (Parent.GridEvents?.OnActionBegin.HasDelegate == true || Parent.IsRenderedFromTreeGrid)
            {
                if (!SearchValue.Equals(actionArgs.SearchString, StringComparison.Ordinal))
                {
                    await Parent.InvokeMethod("sfBlazor.Grid.searchClear", new object[] { Parent.DataId, $"{Parent.ID}_SearchBox", actionArgs.SearchString }).ConfigureAwait(true);
                }
                SearchValue = actionArgs.SearchString;
            }
            excelSearchOperator = actionArgs.ExcelSearchOperator.ToString().ToLower(System.Globalization.CultureInfo.CurrentCulture);
            SearchValue = ((Column?.ValueType == typeof(Guid) || Column?.ValueType == typeof(Guid?)) && !Guid.TryParse(SearchValue, out var newguid)) ? null! : SearchValue;
            InputValue = (!string.IsNullOrEmpty(SearchValue) && Column?.ValueType != null) ? GetActualValue(SearchValue)! : null!;
            if (string.IsNullOrEmpty(SearchValue))
            {
                CancelIcon = string.Empty;
                // Clearing the search returns the user to the full
                // distinct set; reset the cross-chunk dedup window,
                // end-of-data flag, AND _loadedDistinctCount so the next
                // page-fetch starts from offset 0 rather than skipping
                // past items already shown (GRID-DATA-005).
                _seenDistinctKeys.Clear();
                _hasMoreChunks = true;
                _loadedDistinctCount = 0;
                _nextSourceOffset = 0;
                SelectAllHandler();
            }
            else if (Parent.DataManager!.DataAdaptor!.IsRemote())
            {

            }
            else
            {
                CancelIcon = "e-chkcancel-icon";
                SelectAllHandler();
            }

            if (InputValue == null || InputValue.ToString() != "grd-search-notvalid" || (actionArgs.CheckboxListData != null && actionArgs.CheckboxListData.Any()) || (filterSearchingArgs.CheckboxListData != null && filterSearchingArgs.CheckboxListData.Any()) )
            {
                await UpdateDataSource(
                    true,
                    actionArgs.CheckboxListData ?? filterSearchingArgs.CheckboxListData,
                    cancellationToken: requestToken).ConfigureAwait(true);
            }
            if (!IsChunkedLoadingActive() || searchGeneration == _chunkGeneration)
            {
                VisibleProperty = false;
            }
        }

        private string GetClassName()
        {
            var filterType = Parent!.FilterModule!.GetFilterType(Column!);
            if (filterType == "Excel")
            {
                return "e-filter-popup e-excelfilter";
            }
            else
            {
                return "e-filter-popup e-checkboxfilter";
            }
        }

        private string GetLabelClassName()
        {
            return Parent!.FilterSettings!.AllowTextWrap ? "e-fltrcheck e-wrapfilter" : "e-fltrcheck";
        }

        private void OnBlur()
        {
            InputFocus = string.Empty;
        }

        private void OnFocus()
        {
            InputFocus = "e-input-focus";
        }

        private async Task CancelIconClick(MouseEventArgs e)
        {
            IsClearIconPressed = true;
            var eventArgs = e;
            CancelIcon = string.Empty;
            InputValue = null!;
            SearchValue = string.Empty;
            VisibleProperty = true;
            await UpdateDataSource().ConfigureAwait(true);
            await Parent!.InvokeMethod("sfBlazor.Grid.searchClear", new object[] { Parent.DataId, $"{Parent.ID}_SearchBox" }).ConfigureAwait(true);
            VisibleProperty = false;
            IsClearIconPressed = false;
        }

        internal class CheckBoxModel
        {
            public object? Value { get; set; }

            public object? Data { get; set; }

            public object? FormattedValue { get; set; }

            public bool isChecked { get; set; } = true;

            public string? guid { get; set; }

            public string? FUid { get; set; }
        }

        // ----- On-demand loading methods (BLZ-FILT-007..019) -----

        /// <summary>
        /// Resolves the effective chunk size for this dialog session and
        /// resets all per-dialog on-demand loading state. Idempotent; safe
        /// to call before <see cref="OpenFilterDialog"/> returns.
        /// </summary>
        private void ResolveChunkSizeForDialog(int filterChoiceCount = 0)
        {
            // BLZ-FILT-012a / -AC16: FilterChoiceCount > 0 wins for the whole session.
            int resolved = filterChoiceCount > 0
                ? filterChoiceCount
                : (Parent?.FilterSettings?.FilterDialogItemsCount ?? 0);
            if (resolved < 1)
            {
                resolved = DefaultFilterDialogChunkSize;
            }
            _chunkSize = resolved;
            _loadedDistinctCount = 0;
            _nextSourceOffset = 0;
            _initialDistinctCount = 0;
            _virtualWindowStart = 0;
            _virtualWindowEnd = DefaultFilterDialogVirtualWindowSize;
            _virtualRowHeight = 32;
            _virtualFirstVisibleIndex = 0;
            _virtualVisibleCount = 10;
            _checkboxVirtualizationData.Clear();
            _customSearchCheckboxDataSource = null;
            _isLoadingNextChunk = false;
            _announceToken = 0;
            FilterDialogAnnounceText = string.Empty;
            // Server paging + cross-chunk dedup window: reset for the new
            // dialog session so a column value already shown in the previous
            // dialog is treated as "new" again.
            _seenDistinctKeys.Clear();
            _hasMoreChunks = true;
            // Reset scroll throttle for new dialog session
            _lastTriggeredChunkIndex = -1;
        }

        private void RestartChunkRequestForSearch()
        {
            _chunkGeneration++;
            var previousCts = _chunkCts;
            _chunkCts = new CancellationTokenSource();
            if (previousCts != null)
            {
                previousCts.Cancel();
                previousCts.Dispose();
            }
            _isLoadingNextChunk = false;
            _loadedDistinctCount = 0;
            _nextSourceOffset = 0;
            _seenDistinctKeys.Clear();
            _customSearchCheckboxDataSource = null;
            _hasMoreChunks = true;
            _initialDistinctCount = 0;
            _lastTriggeredChunkIndex = -1;
        }

        private void ThrowIfChunkRequestCanceled(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested ||
                (cancellationToken.CanBeCanceled && !IsChunkedLoadingActive()))
            {
                throw new OperationCanceledException(cancellationToken);
            }
        }

        /// <summary>
        /// Records the first chunk of the checkbox distinct list. Called
        /// once from <see cref="OpenFilterDialog"/> when
        /// <c>GridFilterSettings.EnableFilterDialogInfiniteScrolling</c> is
        /// true and the current <see cref="FilterType"/> is <c>Excel</c> or
        /// <c>CheckBox</c>.
        /// </summary>
        internal Task LoadInitialChunkAsync(CancellationToken ct)
        {
            if (ct.IsCancellationRequested || !IsChunkedLoadingActive()) { return Task.CompletedTask; }

            _loadedDistinctCount = FilteredData.Count;
            _initialDistinctCount = _loadedDistinctCount;
            return Task.CompletedTask;
        }

        /// <summary>
        /// Attaches a single passive scroll listener via the existing
        /// <c>scripts/sf-grid.ts</c> module. Called from
        /// <see cref="OpenFilterDialog"/> after the dialog has rendered.
        /// </summary>
        private async Task BindScrollListenerIfNeeded(CancellationToken ct)
        {
            if (ct.IsCancellationRequested || Parent == null || Column == null) { return; }
            if (!IsChunkedLoadingActive()) { return; }
            if (_chunkCts == null || _chunkCts.IsCancellationRequested) { return; }

            var dotnetRef = Microsoft.JSInterop.DotNetObjectReference.Create(this);
            try
            {
                await Parent.InvokeMethod(
                    "sfBlazor.Grid.bindFilterDialogScroll",
                    new object[] { Parent.DataId, Column.Uid, dotnetRef }).ConfigureAwait(true);
                _chunkDotNetRef = dotnetRef;
                _scrollListenerBound = true;
                if (ct.IsCancellationRequested || !IsChunkedLoadingActive())
                {
                    await CancelChunkRequestAsync().ConfigureAwait(true);
                    return;
                }
            }
            catch (Microsoft.JSInterop.JSException)
            {
                // JS not yet available (e.g. prerender); the listener will
                // be attached on the next DialogCreated after render.
                dotnetRef.Dispose();
            }
            catch (System.OperationCanceledException)
            {
                dotnetRef.Dispose();
                throw;
            }
        }

        // Tracks the DotNetObjectReference handed to JS so DisposeAsync
        // can release it. Set in BindScrollListenerIfNeeded, cleared in
        // CancelChunkRequestAsync.
        private Microsoft.JSInterop.DotNetObjectReference<FilterCheckBoxRenderer<TContent>>? _chunkDotNetRef;

        /// <summary>
        /// Invoked by the JS scroll listener with the first visible
        /// checkbox's logical data index. Requests the next chunk when the
        /// viewport approaches the end of the loaded list.
        /// </summary>
        /// <remarks>
        /// The <see cref="CancellationToken"/> is sourced from the renderer's
        /// per-dialog <see cref="_chunkCts"/> — <c>[JSInvokable]</c> callbacks
        /// do not receive a token over JS interop; the C# lifecycle owns it
        /// (see <see cref="CancelChunkRequestAsync"/>). The token is observed at
        /// every async yield so a dialog close cancels an in-flight chunk.
        /// </remarks>
        [Microsoft.JSInterop.JSInvokable]
        public async Task OnCheckboxListScrollAsync(int firstVisibleDataIndex, int visibleCount, int rowHeight)
        {
            if (!IsChunkedLoadingActive()) { return; }
            if (_chunkCts == null || _chunkCts.IsCancellationRequested) { return; }
            if (firstVisibleDataIndex < 0 || visibleCount <= 0) { return; }
            _virtualFirstVisibleIndex = firstVisibleDataIndex;
            _virtualVisibleCount = visibleCount;
            if (rowHeight > 0)
            {
                _virtualRowHeight = rowHeight;
            }

            var windowChanged = UpdateVirtualWindow(firstVisibleDataIndex, visibleCount);

            if (windowChanged && !_isLoadingNextChunk)
            {
                await InvokeAsync(StateHasChanged).ConfigureAwait(true);
            }

            if (CheckedData == null || _loadedDistinctCount <= 0) { return; }
            // Server-paged chunked mode: the previous client-side guard
            // (_loadedDistinctCount >= CheckboxListData.Count()) no longer
            // reflects end-of-data because CheckboxListData now holds only
            // the current page. Use _hasMoreChunks, which is updated after
            // every page fetch based on the returned page size.
            if (!_hasMoreChunks) { return; }

            // NEW: Throttle scroll events by tracking which "chunk boundary" triggered the load.
            // Calculate which chunk the user is currently in (0, 1, 2, ...).
            // Only trigger load when crossing into a NEW chunk to prevent repeated triggers
            // during momentum scrolling (BLZ-FILT-INFINITE-SCROLL fix).
            int currentChunkIndex = (firstVisibleDataIndex + visibleCount) / _chunkSize;
            
            // Trigger when we're getting close to the end of loaded data.
            // Specifically: when bottom of visible window is within one chunk of loaded end.
            // This ensures we load the NEXT chunk BEFORE user runs out of items to scroll.
            var prefetchThreshold = Math.Min(
                _chunkSize,
                visibleCount + VirtualWindowOverscanCount);
            if (firstVisibleDataIndex + visibleCount < _loadedDistinctCount - prefetchThreshold) { return; }
            
            // Throttle: only trigger if we've crossed into a NEW chunk boundary (not seen before)
            if (currentChunkIndex <= _lastTriggeredChunkIndex) { return; }
            
            _lastTriggeredChunkIndex = currentChunkIndex;
            await LoadNextChunkAsync(_chunkCts.Token).ConfigureAwait(true);
        }

        /// <summary>
        /// Appends the next slice of the cached distinct-value source to the
        /// already-rendered checkbox list.
        /// </summary>
        private async Task LoadNextChunkAsync(CancellationToken ct)
        {
            if (!IsChunkedLoadingActive()) { return; }
            if (ct.IsCancellationRequested || _isLoadingNextChunk) { return; }
            if (Parent == null || Column == null) { return; }

            var generation = _chunkGeneration;
            _isLoadingNextChunk = true;
            VisibleProperty = true;
            try
            {
                var previousLoadedCount = _loadedDistinctCount;
                // Pass null customFilterData so UpdateDataSource takes the
                // server path (customFilterData == null) and applies
                // .Skip(_nextSourceOffset).Take(_chunkSize) to the
                // DataManagerQuery / GridQuery. Passing CheckboxListData
                // would route to the local-slicing else branch which
                // would Take() from the previously-returned page and
                // yield zero rows (GRID-DATA-005 / BLZ-FILT-018).
                var customSource = string.IsNullOrEmpty(SearchValue)
                    ? _customCheckboxDataSource
                    : _customSearchCheckboxDataSource;
                var nextCustomPage = customSource?
                    .Skip(_nextSourceOffset)
                    .Take(_chunkSize)
                    .ToList();
                await UpdateDataSource(
                    false,
                    nextCustomPage,
                    _chunkSize,
                    appendChunk: true,
                    cancellationToken: ct).ConfigureAwait(true);
                ThrowIfChunkRequestCanceled(ct);
                if (generation != _chunkGeneration || !IsChunkedLoadingActive()) { return; }
                _loadedDistinctCount = FilteredData.Count;
                UpdateVirtualWindow(
                    _virtualFirstVisibleIndex,
                    _virtualVisibleCount);
                VisibleProperty = false;
                if (_loadedDistinctCount > previousLoadedCount && _loadedDistinctCount > _initialDistinctCount)
                {
                    AnnounceLoadedCountOnce(_loadedDistinctCount);
                }
                await InvokeAsync(StateHasChanged).ConfigureAwait(true);
            }
            catch (System.OperationCanceledException)
            {
                // Closing or restarting the opt-in session cancels pending work.
                if (generation == _chunkGeneration)
                {
                    VisibleProperty = false;
                }
            }
            catch (Exception)
            {
                // Non-cancellation: surface by toggling spinner off and
                // re-throw — the harness's outer gates catch this; we do
                // not silently swallow real errors (BLZ-FILT-018).
                if (generation == _chunkGeneration)
                {
                    VisibleProperty = false;
                    await InvokeAsync(StateHasChanged).ConfigureAwait(true);
                }
                throw;
            }
            finally
            {
                if (generation == _chunkGeneration)
                {
                    _isLoadingNextChunk = false;
                }
            }
        }

        /// <summary>
        /// Cancels any in-flight chunk request and disposes the per-dialog
        /// <see cref="CancellationTokenSource"/> and
        /// <see cref="Microsoft.JSInterop.DotNetObjectReference{T}"/>.
        /// Idempotent. Called from <see cref="OncloseDialog"/>,
        /// <see cref="Closed"/>, and the renderer dispose path.
        /// </summary>
        internal async Task CancelChunkRequestAsync()
        {
            _chunkGeneration++;
            var chunkCts = _chunkCts;
            _chunkCts = null;
            var hadChunkSession = chunkCts != null || _scrollListenerBound || _chunkDotNetRef != null;
            try
            {
                if (chunkCts != null)
                {
                    await chunkCts.CancelAsync().ConfigureAwait(true);
                }
            }
            catch (ObjectDisposedException)
            {
                // Already disposed — nothing to do.
            }
            finally
            {
                chunkCts?.Dispose();
            }
            if (hadChunkSession)
            {
                _isLoadingNextChunk = false;
                _loadedDistinctCount = 0;
                _nextSourceOffset = 0;
                VisibleProperty = false;
                _customCheckboxDataSource = null;
                _customSearchCheckboxDataSource = null;
            }
            if (_scrollListenerBound && Parent != null && Column != null)
            {
                _scrollListenerBound = false;
                try
                {
                    await Parent.InvokeMethod(
                        "sfBlazor.Grid.unbindFilterDialogScroll",
                        new object[] { Parent.DataId, Column.Uid }).ConfigureAwait(true);
                }
                catch (Microsoft.JSInterop.JSDisconnectedException)
                {
                    // A disconnected Blazor circuit cannot run DOM cleanup.
                }
                catch (Microsoft.JSInterop.JSException)
                {
                    // The dialog may already be detached from the browser DOM.
                }
                catch (OperationCanceledException)
                {
                    // Disposal may race with an interop cancellation.
                }
            }

            _chunkDotNetRef?.Dispose();
            _chunkDotNetRef = null;
        }

        /// <summary>
        /// Writes the new total loaded count to the polite live region
        /// <strong>once per dialog session</strong>, on the first growth
        /// past the initial chunk (BLZ-FILT-A11Y-005).
        /// </summary>
        internal void AnnounceLoadedCountOnce(int totalLoaded)
        {
            if (_announceToken != 0) { return; } // already announced
            if (totalLoaded <= 0) { return; }
            _announceToken = 1; // mark announced
            // Literal English fallback. A localized "N items loaded" key is
            // intentionally NOT added to GridLocaleKeys here because that
            // would require synchronized updates to the Syncfusion.Blazor
            // satellite resources (owned by a separate package). The polite
            // live region fires at most once per dialog session
            // (BLZ-FILT-A11Y-005), so the user-visible impact is minimal.
            FilterDialogAnnounceText = string.Format(
                System.Globalization.CultureInfo.CurrentCulture,
                "{0} items loaded",
                totalLoaded);
        }

        /// <summary>
        /// Text content rendered into the polite live region. Set by
        /// <see cref="AnnounceLoadedCountOnce"/>; read by the
        /// <c>.razor</c> markup. Empty by default (live region renders no
        /// text on dialog open until the first chunk appends).
        /// </summary>
        internal string FilterDialogAnnounceText { get; set; } = string.Empty;

        /// <summary>
        /// True when the on-demand loading state machine should be active
        /// for the current dialog: both filter dialog types that support a
        /// checkbox list AND the opt-in flag is set.
        /// </summary>
        internal bool IsChunkedLoadingActive()
        {
            if (Parent == null || Column == null) { return false; }
            if (Parent.FilterSettings == null) { return false; }
            if (!Parent.FilterSettings.EnableFilterDialogInfiniteScrolling) { return false; }
            var filterType = Parent.FilterModule?.GetFilterType(Column);
            return filterType == "Excel" || filterType == "CheckBox";
        }

        internal bool IsCheckboxListVirtualized()
            => IsChunkedLoadingActive();

        internal IEnumerable<(KeyValuePair<string, object> Data, int Index)> GetCheckboxDataForRender()
        {
            if (!IsCheckboxListVirtualized())
            {
                return CheckedData.Select((item, index) => (item, index));
            }

            if (_checkboxVirtualizationData.Count != CheckedData.Count)
            {
                _checkboxVirtualizationData = CheckedData.ToList();
            }

            var start = Math.Clamp(_virtualWindowStart, 0, _checkboxVirtualizationData.Count);
            var count = Math.Clamp(
                _virtualWindowEnd - start,
                0,
                _checkboxVirtualizationData.Count - start);
            return _checkboxVirtualizationData
                .GetRange(start, count)
                .Select((item, index) => (item, start + index));
        }

        private void AddCheckboxDataItem(string key, CheckBoxModel value)
        {
            CheckedData?.AddOrUpdateItem(key, value);
            if (IsChunkedLoadingActive())
            {
                _checkboxVirtualizationData.Add(new KeyValuePair<string, object>(key, value));
            }
        }

        internal IReadOnlyDictionary<string, object> GetCheckboxListVirtualizationAttributes()
        {
            if (!IsCheckboxListVirtualized())
            {
                return new Dictionary<string, object>();
            }

            return new Dictionary<string, object>
            {
                ["style"] = "overflow-anchor:none;",
                ["data-virtual-start"] = _virtualWindowStart,
                ["data-virtual-end"] = Math.Min(_virtualWindowEnd, CheckedData.Count),
                ["data-total-count"] = CheckedData.Count,
                ["data-leading-count"] = GetCheckboxListLeadingRowCount(),
                ["data-has-more"] = _hasMoreChunks && string.IsNullOrEmpty(SearchValue) ? "true" : "false"
            };
        }

        internal int GetVirtualTopSpacerHeight()
            => IsCheckboxListVirtualized() ? _virtualWindowStart * _virtualRowHeight : 0;

        internal int GetVirtualBottomSpacerHeight()
            => IsCheckboxListVirtualized()
                ? Math.Max(0, CheckedData.Count - _virtualWindowEnd) * _virtualRowHeight
                : 0;

        internal static bool IsVirtualBlankValue(CheckBoxModel value, bool requireValue = false)
        {
            var displayValue = value.FormattedValue ?? value.Value;
            var displayText = displayValue?.ToString();
            return (requireValue && (value.Value == null || string.IsNullOrEmpty(value.Value.ToString()) || value.Value.ToString() == "null"))
                || displayValue == null
                || string.IsNullOrEmpty(displayText)
                || displayText == "null";
        }

        private int GetCheckboxListLeadingRowCount()
        {
            if (CheckedData.Count == 0) { return 0; }
            return 1 + (isCurrentSelecitonFilter ? 1 : 0);
        }

        private bool UpdateVirtualWindow(int firstVisibleDataIndex, int visibleCount)
        {
            var start = Math.Min(
                CheckedData.Count,
                Math.Max(0, firstVisibleDataIndex - VirtualWindowOverscanCount));
            var end = Math.Min(
                CheckedData.Count,
                Math.Max(start, firstVisibleDataIndex + visibleCount + VirtualWindowOverscanCount));
            if (start == _virtualWindowStart && end == _virtualWindowEnd) { return false; }
            _virtualWindowStart = start;
            _virtualWindowEnd = end;
            return true;
        }

        /// <summary>
        /// Cross-chunk dedup window (Option B). Filters the current page of
        /// distinct rows so that any column value already shown in a
        /// previous page is dropped, and records the keys we are about to
        /// render so subsequent chunks can dedup against us.
        /// </summary>
        /// <param name="page">Rows returned for the current page (already
        /// distinct within the page, but possibly duplicated across pages
        /// when the server does not know about the prior filter).</param>
        /// <param name="keyPropertyName">Column field whose value identifies
        /// the distinct row.</param>
        /// <param name="displayPropertyName">For foreign columns, the
        /// underlying display value (e.g. "ShipCountryName") that the
        /// checkbox label uses. Falls back to <paramref name="keyPropertyName"/>
        /// when null.</param>
        private List<object> FilterPageBySeenDistinctKeys(
            List<object> page,
            string? keyPropertyName,
            string? displayPropertyName)
        {
            if (page == null || page.Count == 0) { return page ?? new List<object>(); }
            var column = Column;
            var parent = Parent;
            if (column == null || parent == null) { return page; }
            var display = displayPropertyName ?? keyPropertyName ?? column.Field;
            if (string.IsNullOrEmpty(display)) { return page; }

            var result = new List<object>(page.Count);
            foreach (var row in page)
            {
                // Resolve the value that the user actually sees in the
                // checkbox label. For non-foreign columns this is the
                // column field value; for foreign columns it is the
                // foreign key display value (same as the rest of the
                // rendering pipeline uses).
                object? displayValue = column.IsForeignColumn() == true
                    ? (parent.PropHelper?.GetValue(row, display!) ?? DataUtil.GetObject(display!, row!))
                    : (parent.PropHelper?.GetValue(row, display!) ?? row);
                string key = displayValue?.ToString() ?? "null";
                if (_seenDistinctKeys.Add(key))
                {
                    result.Add(row);
                }
            }
            return result;
        }

        /// <summary>
        /// Returns the id of the sentinel element rendered after the
        /// checkbox list. JS uses it to locate the list for scroll events.
        /// </summary>
        internal string GetScrollSentinelId()
            => $"{Parent?.DataId}{Column?.Uid}_chkScrollSentinel";

        /// <summary>
        /// Returns the id of the polite live region element rendered inside
        /// the dialog. Reads the element's <c>textContent</c> to announce
        /// the new loaded total once per dialog session
        /// (BLZ-FILT-A11Y-005).
        /// </summary>
        internal string GetAnnounceRegionId()
            => $"{Parent?.ID}{Column?.Uid}_chkAnnounce";

        /// <summary>
        /// Disposes the renderer's per-dialog state. Mirrors the
        /// disposal-required pattern mandated by BLZ-030 / BLZ-031 / BLZ-018
        /// and the <c>javascript-interop</c> skill.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await CancelChunkRequestAsync().ConfigureAwait(true);
            GC.SuppressFinalize(this);
        }
    }
}