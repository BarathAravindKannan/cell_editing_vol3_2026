using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures the Advanced Filter (Query Builder) dialog of the grid.
    /// </summary>
    /// <remarks>
    /// Use this component as a child of <see cref="SfGrid{TValue}"/> to
    /// supply an <see cref="InitialRule"/> and read the live rule from
    /// <see cref="CurrentRule"/>.
    /// </remarks>
    /// <example>
    /// <code><![CDATA[
    /// <SfGrid DataSource="@Orders" AllowAdvancedFiltering="true" @ref="Grid">
    ///     <GridAdvancedFilterSettings InitialRule="@InitialRule" />
    /// </SfGrid>
    /// ]]></code>
    /// </example>
    public partial class GridAdvancedFilterSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        /// <summary>
        /// Gets or sets the cascading grid reference.
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        /// <summary>
        /// Gets or sets the cascading base parent reference.
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        internal static async Task<GridAdvancedFilterSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var settings = new GridAdvancedFilterSettings();
            settings.Parent = (IGrid)baseComponent;
            settings.BaseParent = (IGrid)baseComponent;
            await settings.OnInitializedAsync().ConfigureAwait(true);
            return settings;
        }

        /// <summary>
        /// Initializes the component, captures the supplied <see cref="InitialRule"/>
        /// into <see cref="CurrentRule"/> so it is the first value pushed into the
        /// Query Builder, and registers itself with the parent grid so that
        /// <c>SfGrid.AdvancedFilterSettings</c> is non-null and any rule the user
        /// applies from the dialog round-trips back through the grid.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            if (BaseParent is SfBaseComponent baseComponent)
            {
                MainParent = baseComponent;
            }

            Parent?.UpdateChildProperties(nameof(IGrid.AdvancedFilterSettings), this);

            if (CurrentRule == null && InitialRule != null)
            {
                CurrentRule = InitialRule;
            }
        }

        /// <summary>
        /// Defines the child content.
        /// </summary>
        /// <exclude/>
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the initial compound filter rule applied to the
        /// grid on first render. When <c>null</c>, the grid starts with no
        /// advanced filter.
        /// </summary>
        [Parameter]
        [JsonPropertyName("initialRule")]
        [DefaultValue(null)]
        public AdvancedFilterRule? InitialRule { get; set; }

        /// <summary>
        /// Gets the currently configured advanced filter rule. Updated by
        /// the Query Builder when the user applies a new rule.
        /// </summary>
        /// <value>
        /// The live rule, or <c>null</c> when no rule is configured.
        /// </value>
        [JsonPropertyName("currentRule")]
        public AdvancedFilterRule? CurrentRule { get; internal set; }

        /// <summary>
        /// Gets or sets the dialog header text. The default value is
        /// <c>"Advanced Filter"</c>.
        /// </summary>
        [Parameter]
        [DefaultValue("Advanced Filter")]
        [JsonPropertyName("headerText")]
        public string HeaderText { get; set; } = "Advanced Filter";
    }
}

