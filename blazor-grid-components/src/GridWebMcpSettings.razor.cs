using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Represents the WebMCP configuration for an <see cref="SfGrid{TValue}"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this component to customize the WebMCP tools exposed by a grid to
    /// compatible AI agents. Built-in tools can be disabled, customized, or
    /// replaced, and additional custom tools can be registered through
    /// <see cref="GridWebMcpTool"/> child components.
    /// </para>
    /// <para>
    /// Custom tool execution is handled through the
    /// <see cref="GridEvents{TValue}.OnWebMcpExecute"/> event.
    /// </para>
    /// <para>
    /// This component is effective only when
    /// <see cref="SfGrid{TValue}.EnableWebMcp"/> is <see langword="true"/> and
    /// the hosting environment supports the browser's
    /// <c>document.modelContext</c> API.
    /// </para>
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// <SfGrid DataSource="@Orders" EnableWebMcp="true">
    /// <GridWebMcpSettings Name="SalesGrid">
    ///     <GridWebMcpTools>
    ///         <GridWebMcpTool Command="GridWebMcpToolCommand.FilterByColumn" Name="filterOrders" />
    ///     </GridWebMcpTools>
    /// </GridWebMcpSettings>
    /// </SfGrid>
    /// ]]>
    /// </example>
    public partial class GridWebMcpSettings : SfDataBoundComponent
    {
        /// <summary>
        /// Defines the parent component.
        /// </summary>
        /// <exclude />
        protected override SfBaseComponent? MainParent { get; set; }

        /// <summary>
        /// Cascading reference to the grid that owns this settings block.
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal IGrid? Parent { get; set; }

        /// <summary>
        /// Cascading reference to the grid (base parent alias).
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Optional child content. Typically a
        /// <c>&lt;GridWebMcpTools&gt;</c> wrapper containing one or more
        /// <c>&lt;GridWebMcpTool&gt;</c> entries, or a flat list of
        /// <c>&lt;GridWebMcpTool&gt;</c> entries.
        /// </summary>
        /// <exclude />
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        /// <summary>
        /// Gets or sets the prefix used when generating browser-side WebMCP tool names
        /// for this grid instance.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> that defines the prefix applied to all tool names
        /// registered by this grid. When <see langword="null"/> or empty, the grid
        /// falls back to its configured <c>ID</c> and then to an internally generated
        /// identifier. The default value is <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When specified, this value takes precedence over the grid's <c>ID</c>
        /// when generating browser-side tool names during WebMCP registration.
        /// </para>
        /// <para>
        /// Tool names must be unique within the browser-side WebMCP tool registry.
        /// When multiple WebMCP-enabled grids are rendered on the same page,
        /// configure a unique <c>Name</c> value for each grid to avoid tool
        /// registration conflicts.
        /// </para>
        /// <para>
        /// This prefix is combined with the configured tool name to produce the
        /// fully qualified tool identifier exposed through
        /// <see cref="WebMcpToolExecuteEventArgs.ToolName"/>.
        /// </para>
        /// <para>
        /// This property is not serialized and must be supplied directly through
        /// the component instance.
        /// </para>
        /// </remarks>
        [Parameter]
        [JsonIgnore]
        public string? Name { get; set; }

        /// <summary>
        /// The collection of <see cref="GridWebMcpTool"/> entries declared
        /// inside this settings block. The collection is flat — entries
        /// supplied directly as <c>&lt;GridWebMcpTool&gt;</c> children of
        /// <c>&lt;GridWebMcpSettings&gt;</c> (or inside a nested
        /// <c>&lt;GridWebMcpTools&gt;</c> wrapper) all end up in this list.
        /// Framework-internal: mutated by <see cref="UpdateChildProperty(GridWebMcpTool)"/>
        /// and <see cref="AddTools"/> as children initialize, then read by
        /// the WebMCP adapter to produce the merged per-instance catalog.
        /// </summary>
        internal List<GridWebMcpTool> ChildTools { get; set; } = new List<GridWebMcpTool>();

        /// <summary>
        /// Adds a <see cref="GridWebMcpTool"/> child entry to
        /// <see cref="ChildTools"/>. Called by
        /// <see cref="GridWebMcpTool.OnInitializedAsync"/> and by the
        /// <c>&lt;GridWebMcpTools&gt;</c> wrapper.
        /// Framework-internal — only consumed inside this assembly.
        /// </summary>
        /// <param name="value">The child entry to add.</param>
        /// <returns>The index of the appended entry.</returns>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal int UpdateChildProperty(GridWebMcpTool value)
        {
            // If the same component re-initialises (e.g. on a parent re-render),
            // drop the prior instance so we never accumulate duplicates.
            ChildTools.Remove(value);
            ChildTools.Add(value);
            return ChildTools.Count - 1;
        }

        /// <summary>
        /// Merges the supplied list of tool entries into
        /// <see cref="ChildTools"/>. Called by the <c>&lt;GridWebMcpTools&gt;</c>
        /// wrapper component when its children finish initializing.
        /// Framework-internal — only consumed inside this assembly.
        /// </summary>
        /// <param name="wrapperTools">The list forwarded by the wrapper.</param>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void AddTools(List<GridWebMcpTool> wrapperTools)
        {
            if (wrapperTools == null) return;
            foreach (var tool in wrapperTools)
            {
                if (tool == null) continue;
                ChildTools.Remove(tool);
                ChildTools.Add(tool);
            }
        }

        /// <summary>
        /// Updates the child property collection keyed by name. Mirrors the
        /// pattern used by every other <c>GridXxxSettings</c> component so the
        /// grid's <c>DirectParameters</c> pipeline keeps working.
        /// Framework-internal — only consumed inside this assembly.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        internal void UpdateChildProperties(string key, List<GridWebMcpTool> value)
        {
            DirectParameters.AddOrUpdateItem(key, value);
        }

        /// <summary>
        /// Initializes the settings component, hooks it into the parent
        /// grid, and signals that the tool registry needs to be re-resolved
        /// on the next render via
        /// <c>BaseParent.HasWebMcpSettingsChanges</c>.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            if (BaseParent is SfBaseComponent baseComponent)
            {
                MainParent = baseComponent;
            }
            Parent?.UpdateChildProperties(nameof(IGrid.WebMcpSettings), this);
            if (BaseParent != null)
            {
                BaseParent.HasWebMcpSettingsChanges = true;
            }
        }

        /// <summary>
        /// Validates custom-tool execution configuration after all grid child
        /// components have initialized, independent of their markup order.
        /// </summary>
        private static bool ToolsEquals(
            IEnumerable<GridWebMcpToolCommand>? left,
            IEnumerable<GridWebMcpToolCommand>? right)
        {
            if (ReferenceEquals(left, right)) return true;
            if (left == null || right == null) return false;
            var leftSet = new HashSet<GridWebMcpToolCommand>(left);
            var rightSet = new HashSet<GridWebMcpToolCommand>(right);
            return leftSet.SetEquals(rightSet);
        }

        /// <summary>
        /// Detects mutations on the allowlist and flips
        /// <c>BaseParent.HasWebMcpSettingsChanges</c> so the next
        /// registration call re-resolves the effective tool catalog.
        /// Mirrors the pattern used by
        /// <see cref="GridWebMcpTool.OnParametersSetAsync"/>.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);
            if (BaseParent != null)
            {
                BaseParent.HasWebMcpSettingsChanges = true;
            }
        }

        /// <summary>
        /// Removes a child entry that was previously added. Called by the
        /// framework when a <see cref="GridWebMcpTool"/> is dynamically
        /// disposed (e.g. by an <c>@if</c> block).
        /// </summary>
        /// <param name="value">The child entry to remove.</param>
        /// <returns>The index of the removed entry, or <c>-1</c> if not found.</returns>
        internal int RemoveChildProperty(GridWebMcpTool value)
        {
            var idx = ChildTools.IndexOf(value);
            if (idx >= 0)
            {
                ChildTools.RemoveAt(idx);
                if (BaseParent != null)
                {
                    BaseParent.HasWebMcpSettingsChanges = true;
                }
            }
            return idx;
        }
    }
}
