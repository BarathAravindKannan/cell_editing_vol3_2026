using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using System.Text.Json.Serialization;
using Syncfusion.Blazor.Grids.Internal;
using System.Threading.Tasks;
using Syncfusion.Blazor;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures grid filtering.
    /// </summary>
    public partial class GridFilterSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        private bool isAutoInitialized { get; set; }

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
        /// Specifies the columns to be filtered at initial rendering of the Grid. You can also get the columns that were currently filtered.
        /// </summary>
        /// <remarks>
        /// Use <see cref="Syncfusion.Blazor.Grids.GridFilterColumn"/> component
        /// to define filter criteria.
        /// </remarks>
        [Parameter]
        public List<GridFilterColumn>? Columns { get; set; }

        private List<GridFilterColumn>? _columns { get; set; }

        /// <summary>
        /// If EnableCaseSensitivity is set to true then searches grid records with exact match based on the filter
        /// operator. It will have no effect on number, boolean and Date fields.
        /// </summary>
        [Parameter]
        public bool EnableCaseSensitivity { get; set; }

        private bool _enableCaseSensitivity { get; set; }

        /// <summary>
        /// If ignoreAccent set to true, then filter ignores the diacritic characters or accents while filtering.
        /// </summary>
        [Parameter]
        public bool IgnoreAccent { get; set; }

        private bool _ignoreAccent { get; set; }

        /// <summary>
        /// Defines the time delay (in milliseconds) in filtering records when the Immediate mode of filter bar is set.
        /// </summary>
        [Parameter]
        public int ImmediateModeDelay { get; set; } = 1500;

        private int _immediateModeDelay { get; set; }

        /// <summary>
        /// Defines the filter bar modes. The available options are.
        /// <list type="bullet">
        /// <item>
        /// <term>OnEnter</term>
        /// <description>Initiate filter operation after Enter key is pressed.</description>
        /// </item>
        /// <item>
        /// <term>Immediate</term>
        /// <description>Initiate filter operation after certain time interval. By default time interval is 1500ms.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public FilterBarMode Mode { get; set; } = FilterBarMode.OnEnter;

        private FilterBarMode _mode { get; set; }

        /// <summary>
        /// The Operators is used to override the default operators in filter menu. This should be defined by type wise
        /// (string, number, date and boolean). Based on the column type, this customize operator list will render in filter menu.
        /// </summary>
        [Parameter]
        public object? Operators { get; set; }

        private object? _operators { get; set; }

        /// <summary>
        /// Shows or hides the filtered status message on the pager.
        /// </summary>
        [Parameter]
        public bool ShowFilterBarStatus { get; set; } = true;

        private bool _showFilterBarStatus { get; set; }

        /// <summary>
        /// Defines options for filtering type. The available options are.
        /// <list type="bullet">
        /// <item>
        /// <term>FilterBar</term>
        /// <description>Default. Specifies the filter type as filter bar.</description>
        /// </item>
        /// <item>
        /// <term>Menu</term>
        /// <description>Specifies the filter type as menu.</description>
        /// </item>
        /// <item>
        /// <term>CheckBox</term>
        /// <description>Specifies the filter type as check box.</description>
        /// </item>
        /// <item>
        /// <term>Excel</term>
        /// <description>Specifies the filter type as excel.</description>
        /// </item>
        /// </list>
        /// </summary>
        [Parameter]
        public FilterType Type { get; set; } = FilterType.FilterBar;

        private FilterType _type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the text in the filter checkbox automatically wraps to the next line when it exceeds the width of the filter dialog.
        /// </summary>
        /// <value>
        /// <b>true</b> to enable text wrapping; otherwise, <b>false</b>. The default is <b>false</b>.
        /// </value>
        /// <remarks>
        /// Text wrapping applies to both the <see cref="Syncfusion.Blazor.Grids.FilterType.CheckBox"/> and <see cref="Syncfusion.Blazor.Grids.FilterType.Excel"/>
        /// </remarks>
        [Parameter]
        public bool AllowTextWrap { get; set; }

        private bool _allowTextWrap { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to enable operator dropdown selector 
        /// and type-aware input controls in the FilterBar header row.
        /// </summary>
        /// <value>
        /// true to enable operator dropdowns and type-aware inputs; false to use traditional text-only FilterBar.
        /// Default is false for backward compatibility.
        /// </value>
        /// <remarks>
        /// This feature is fully backward compatible. When disabled (default), existing 
        /// FilterBar behavior is unchanged. Enable only when you want operator selection UI
        /// and type-specific input controls (SfDatePicker, SfNumericTextBox, SfAutoComplete, etc.).
        /// </remarks>
        [Parameter]
        public bool ShowFilterBarOperator { get; set; } = false;

        private bool _showFilterBarOperator { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the checkbox value list
        /// inside the <see cref="FilterType.Excel"/> and <see cref="FilterType.CheckBox"/>
        /// filter dialogs is loaded in chunks as the user scrolls.
        /// </summary>
        /// <value>
        /// <b>true</b> to enable on-demand (chunked) loading of the
        /// filter-dialog checkbox list; otherwise <b>false</b>.
        /// The default is <b>false</b> — existing customers see no change.
        /// </value>
        /// <remarks>
        /// Applies only to <see cref="FilterType.Excel"/> and
        /// <see cref="FilterType.CheckBox"/>. <see cref="FilterType.FilterBar"/>
        /// and <see cref="FilterType.Menu"/> ignore this setting. The chunk
        /// size is controlled by <see cref="FilterDialogItemsCount"/>, and
        /// may be overridden per request by
        /// <c>FilterDialogOpeningEventArgs.FilterChoiceCount</c>. The data
        /// source is the one already supplied for the filter distinct list
        /// (e.g. <c>CheckboxListData</c> in <c>FilterDialogOpeningEventArgs</c>);
        /// no new event is raised for the chunked requests.
        /// </remarks>
        [Parameter]
        public bool EnableFilterDialogInfiniteScrolling { get; set; }

        private bool _enableFilterDialogInfiniteScrolling { get; set; }

        /// <summary>
        /// Gets or sets the chunk size (number of distinct values per
        /// request) used by the filter-dialog checkbox list when
        /// <see cref="EnableFilterDialogInfiniteScrolling"/> is <b>true</b>.
        /// </summary>
        /// <value>
        /// An integer ≥ 1. Values ≤ 0 are clamped to <b>50</b>, matching the
        /// Syncfusion EJ2 default for <c>filterSettings.itemsCount</c>.
        /// </value>
        /// <remarks>
        /// When a positive value is set on
        /// <c>FilterDialogOpeningEventArgs.FilterChoiceCount</c>, it
        /// overrides this value for the whole dialog session
        /// (-AC16 / BLZ-FILT-012a).
        /// </remarks>
        [Parameter]
        public int FilterDialogItemsCount { get; set; } = 50;

        private int _filterDialogItemsCount { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether invalid date values are prevented
        /// from being applied as filter criteria in date filter dialogs.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> indicating whether date filter validation is enabled.
        /// The default value is <see langword="false"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When set to <see langword="true"/>, the Grid validates date values entered
        /// in Menu and Excel filter dialogs before applying the filter.
        /// </para>
        /// <para>
        /// If an invalid date value is entered:
        /// <list type="bullet">
        /// <item>
        /// <description>The Filter button is disabled until a valid date value is provided.</description>
        /// </item>
        /// <item>
        /// <description>A validation message is displayed to indicate that the entered date is invalid.</description>
        /// </item>
        /// <item>
        /// <description>The Clear button remains available to remove the filter value.</description>
        /// </item>
        /// <item>
        /// <description>Invalid date values are not applied as filter predicates.</description>
        /// </item>
        /// </list>
        /// </para>
        /// <para>
        /// This property applies only to Menu and Excel filter types and does not affect
        /// Filter Bar filtering.
        /// </para>
        /// <para>
        /// When set to <see langword="false"/>, the Grid preserves the existing filtering
        /// behavior for backward compatibility.
        /// </para>
        /// </remarks>
        /// <example>
        /// <code><![CDATA[
        /// <SfGrid DataSource="@Orders" AllowFiltering="true">
        /// <GridFilterSettings Type="Syncfusion.Blazor.Grids.FilterType.Excel" EnableTemporalValidation="true"/>
        /// </SfGrid>
        /// ]]></code>
        /// </example>
        [Parameter]
        public bool EnableTemporalValidation { get; set; } = false;

        private bool _EnableTemporalValidation { get; set; }


        /// <summary>
        /// Updates the child properties of GridFilterSettings based on the specified key and value.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void UpdateChildProperties(string key, List<GridFilterColumn> value)
        {
            if (key == nameof(Columns))
            {
                Columns = _columns = value;
            }

            DirectParameters.AddOrUpdateItem(key, value);
        }

        internal static async Task<GridFilterSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var GridFilterSettings = new GridFilterSettings();
            GridFilterSettings.Parent = (IGrid)baseComponent;
            GridFilterSettings.BaseParent = (IGrid)baseComponent;
            GridFilterSettings.isAutoInitialized = true;
            await GridFilterSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridFilterSettings;
        }

        /// <summary>
        /// Initializes the GridFilterSettings component, updates filter settings in the parent grid,
        /// and synchronizes internal state for filtering configuration.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperties(nameof(IGrid.FilterSettings), this);
            _columns = Columns;
            _enableCaseSensitivity = EnableCaseSensitivity;
            _ignoreAccent = IgnoreAccent;
            _immediateModeDelay = ImmediateModeDelay;
            _mode = Mode;
            _operators = Operators;
            _showFilterBarStatus = ShowFilterBarStatus;
            _type = Type;
            _allowTextWrap = AllowTextWrap;
            _showFilterBarOperator = ShowFilterBarOperator;
            _enableFilterDialogInfiniteScrolling = EnableFilterDialogInfiniteScrolling;
            _filterDialogItemsCount = FilterDialogItemsCount;
            _EnableTemporalValidation = EnableTemporalValidation;
            
            if (!isAutoInitialized && BaseParent != null)
            {
                BaseParent.HasFilterColumnChanges = true;
            }
        }

        /// <summary>
        /// Handles parameter updates for GridFilterSettings and dynamically synchronizes filter-related properties.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);

            await UpdateProperties("Columns", Columns!).ConfigureAwait(true); // update the columns property while dynamically filter the column using filterSettings
            EnableCaseSensitivity = _enableCaseSensitivity = await UpdateProperty(nameof(EnableCaseSensitivity), EnableCaseSensitivity, _enableCaseSensitivity).ConfigureAwait(true);
            IgnoreAccent = _ignoreAccent = await UpdateProperty(nameof(IgnoreAccent), IgnoreAccent, _ignoreAccent).ConfigureAwait(true);
            ImmediateModeDelay = _immediateModeDelay = await UpdateProperty(nameof(ImmediateModeDelay), ImmediateModeDelay, _immediateModeDelay).ConfigureAwait(true);
            Mode = _mode = await UpdateProperty(nameof(Mode), Mode, _mode).ConfigureAwait(true);
            Operators = _operators = await UpdateProperty(nameof(Operators), Operators, _operators).ConfigureAwait(true);
            ShowFilterBarStatus = _showFilterBarStatus = await UpdateProperty(nameof(ShowFilterBarStatus), ShowFilterBarStatus, _showFilterBarStatus).ConfigureAwait(true);
            Type = _type = await UpdateProperty(nameof(Type), Type, _type).ConfigureAwait(true);
            AllowTextWrap = _allowTextWrap = await UpdateProperty(nameof(AllowTextWrap), AllowTextWrap, _allowTextWrap).ConfigureAwait(true);
            ShowFilterBarOperator = _showFilterBarOperator = await UpdateProperty(nameof(ShowFilterBarOperator), ShowFilterBarOperator, _showFilterBarOperator).ConfigureAwait(true);
            EnableFilterDialogInfiniteScrolling = _enableFilterDialogInfiniteScrolling = await UpdateProperty(nameof(EnableFilterDialogInfiniteScrolling), EnableFilterDialogInfiniteScrolling, _enableFilterDialogInfiniteScrolling).ConfigureAwait(true);
            FilterDialogItemsCount = _filterDialogItemsCount = await UpdateProperty(nameof(FilterDialogItemsCount), FilterDialogItemsCount, _filterDialogItemsCount).ConfigureAwait(true);
            EnableTemporalValidation = _EnableTemporalValidation = await UpdateProperty(nameof(EnableTemporalValidation), EnableTemporalValidation, _EnableTemporalValidation).ConfigureAwait(true);
            if (ChildContent == null && BaseParent != null)
            {
                BaseParent.HasFilterColumnChanges = false;
            }
            if (PropertyChanges.Count > 0 && BaseParent != null)
            {
                if (PropertyChanges.ContainsKey("ShowFilterBarOperator"))
                {
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd("ShowFilterBarOperator", this.ShowFilterBarOperator);
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.FilterSettings), this);
                }
                if (PropertyChanges.ContainsKey("EnableFilterDialogInfiniteScrolling"))
                {
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd("EnableFilterDialogInfiniteScrolling", this.EnableFilterDialogInfiniteScrolling);
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.FilterSettings), this);
                }
                if (PropertyChanges.ContainsKey("FilterDialogItemsCount"))
                {
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd("FilterDialogItemsCount", this.FilterDialogItemsCount);
                    ((SfBaseComponent)BaseParent).PropertyChanges.TryAdd(nameof(IGrid.FilterSettings), this);
                }
                PropertyChanges.Clear();
                await BaseParent.PropertyChanged().ConfigureAwait(true);
            }
             // Telemetry for filtering
            GridTelemetryHelper.LogTelemetry(true, "Filtering");
        }

        internal async Task UpdateProperties(string key, List<GridFilterColumn> value)
        {
            if (key == nameof(Columns))
            {
                var columns = DirectParameters.TryGetValue("Columns", out object? Value) ? (List<GridFilterColumn>)Value : Columns;
                Columns = _columns = await UpdateProperty(nameof(Columns), columns, value).ConfigureAwait(true);
            }
        }
    }
}
