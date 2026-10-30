using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Represents a collection of <see cref="GridWebMcpTool"/> components
    /// associated with a <see cref="GridWebMcpSettings"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this component to declare one or more WebMCP tool definitions for an
    /// <see cref="SfGrid{TValue}"/> instance. Each child
    /// <see cref="GridWebMcpTool"/> can customize a built-in WebMCP tool or
    /// define a custom tool that can be invoked by compatible AI agents.
    /// </para>
    /// <para>
    /// This component is intended to be declared within a
    /// <see cref="GridWebMcpSettings"/> component and serves as a logical
    /// container for related tool definitions.
    /// </para>
    /// <para>
    /// The order in which tools are declared does not affect tool execution
    /// behavior.
    /// </para>
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// <SfGrid DataSource="@Orders"
    ///         EnableWebMcp="true">
    ///     <GridWebMcpSettings Name="SalesGrid">
    ///         <GridWebMcpTools>
    ///             <GridWebMcpTool
    ///                 Command="GridWebMcpToolCommand.FilterByColumn"
    ///                 Name="filterOrders" />
    ///
    ///             <GridWebMcpTool
    ///                 Name="getTopCustomers"
    ///                 Description="Returns the top customers." />
    ///         </GridWebMcpTools>
    ///     </GridWebMcpSettings>
    /// </SfGrid>
    /// ]]>
    /// </example>
    public partial class GridWebMcpTools : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        /// <summary>
        /// Cascading reference to the parent
        /// <see cref="GridWebMcpSettings"/> block.
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal GridWebMcpSettings? Parent { get; set; }

        /// <summary>
        /// Cascading reference to the grid (base parent alias).
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Optional child content (typically a sequence of
        /// <c>&lt;GridWebMcpTool&gt;</c> entries).
        /// </summary>
        /// <exclude />
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// The collection of <see cref="GridWebMcpTool"/> entries declared
        /// inside this wrapper. Forwarded to the parent
        /// <see cref="GridWebMcpSettings"/> during
        /// <see cref="OnInitializedAsync"/> and stored in the parent
        /// <see cref="GridWebMcpSettings.ChildTools"/> collection through
        /// <see cref="GridWebMcpSettings.AddTools(List{GridWebMcpTool})"/>.
        /// Framework-internal — the wrapper collects child instances and then
        /// hands them to the settings component for final registration.
        /// </summary>
        internal List<GridWebMcpTool> Tools { get; set; } = new List<GridWebMcpTool>();

        /// <summary>
        /// Adds a <see cref="GridWebMcpTool"/> child entry to
        /// <see cref="Tools"/>. Called by
        /// <see cref="GridWebMcpTool.OnInitializedAsync"/>.
        /// Framework-internal — only consumed inside this assembly.
        /// </summary>
        /// <param name="value">The child entry to add.</param>
        /// <returns>The index of the appended entry.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal int UpdateChildProperty(GridWebMcpTool value)
        {
            // If the same component re-initialises (e.g. on a parent re-render),
            // drop the prior instance so we never accumulate duplicates.
            Tools.Remove(value);
            Tools.Add(value);
            return Tools.Count - 1;
        }

        /// <summary>
        /// Removes a child entry that was previously added. Called by the
        /// framework when a <see cref="GridWebMcpTool"/> is dynamically
        /// disposed (e.g. by an <c>@if</c> block).
        /// </summary>
        internal int RemoveChildProperty(GridWebMcpTool value)
        {
            var idx = Tools.IndexOf(value);
            if (idx >= 0)
            {
                Tools.RemoveAt(idx);
                if (BaseParent != null)
                {
                    BaseParent.HasWebMcpSettingsChanges = true;
                }
            }
            return idx;
        }

        /// <summary>
        /// Initializes the wrapper, captures the main parent reference,
        /// forwards the child <see cref="Tools"/> list to the parent
        /// <see cref="GridWebMcpSettings"/> via
        /// <see cref="GridWebMcpSettings.AddTools"/>, and signals the grid
        /// to re-resolve its effective tool list on the next render.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            if (BaseParent is SfBaseComponent baseComponent)
            {
                MainParent = baseComponent;
            }
            Parent?.AddTools(Tools);
            if (BaseParent != null)
            {
                BaseParent.HasWebMcpSettingsChanges = true;
            }
        }
    }
}
