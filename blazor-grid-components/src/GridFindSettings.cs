using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Represents configuration settings for the Grid Find and Highlight feature.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Configure the behavior and appearance of the Find and Highlight feature, including 
    /// placeholder text, match highlighting, and case-sensitivity options. These settings are 
    /// applied when the <c>Find</c> toolbar item is enabled.
    /// </para>
    /// </remarks>
    public partial class GridFindSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Gets or sets the placeholder text displayed in the Find input field.
        /// </summary>
        /// <value>
        /// The placeholder text displayed when the input field is empty.
        /// The default value is <c>"Find"</c>.
        /// </value>
        /// <remarks>
        /// Use this property to provide custom instructional text for the
        /// Find and Highlight input field.
        /// </remarks>
        [Parameter]
        public string PlaceholderText { get; set; } = "Find";

        private string? _placeholderText { get; set; }


        /// <summary>
        /// Gets or sets the CSS class applied to highlighted matches.
        /// </summary>
        /// <value>
        /// The CSS class name applied to highlighted matches.
        /// The default value is <c>"e-find-highlight"</c>.
        /// </value>
        /// <remarks>
        /// Provide a custom CSS class to modify the appearance of highlighted matches.
        /// The specified CSS class is applied to the HTML <c>mark</c> element that wraps matched text.
        /// </remarks>
        [Parameter]
        public string HighlightCssClass { get; set; } = "e-find-highlight";

        private string? _highlightCssClass { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether Find and Highlight matching is case-sensitive.
        /// </summary>
        /// <value>
        /// <c>true</c> to perform case-sensitive matching; otherwise, <c>false</c>.
        /// The default value is <c>false</c>.
        /// </value>
        /// <remarks>
        /// When set to <c>false</c>, values such as "John", "john", and "JOHN"
        /// are treated as matching text.
        ///
        /// When set to <c>true</c>, only text with identical letter casing is matched.
        /// </remarks>
        [Parameter]
        public bool CaseSensitive { get; set; } = false;

        private bool _caseSensitive { get; set; }


        internal static async Task<GridFindSettings> Initialize(SfDataBoundComponent baseComponent)
        {
            var GridFindSettings = new GridFindSettings();
            GridFindSettings.Parent = (IGrid)baseComponent;
            GridFindSettings.BaseParent = (IGrid)baseComponent;
            await GridFindSettings.OnInitializedAsync().ConfigureAwait(true);
            return GridFindSettings;
        }

        /// <summary>
        /// Invoked during component initialization.
        /// Updates the parent grid with the current find settings and initializes internal state fields.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            MainParent = (SfBaseComponent)BaseParent!;
            Parent?.UpdateChildProperties(nameof(IGrid.FindSettings), this);
            _placeholderText = PlaceholderText;
            _highlightCssClass = HighlightCssClass;
            _caseSensitive = CaseSensitive;
        }

        /// <summary>
        /// Invoked when component parameters are set or updated.
        /// Synchronizes find-related properties and notifies the parent component if any changes occur.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);
            _placeholderText = PlaceholderText;
            _highlightCssClass = HighlightCssClass;
            _caseSensitive = CaseSensitive;
            Parent?.UpdateChildProperties(nameof(IGrid.FindSettings), this);
        }
    }
}
