using System;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Represents a WebMCP tool definition or customization entry associated with an
    /// <see cref="SfGrid{TValue}"/> instance.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A tool can either customize a built-in WebMCP command by specifying a value
    /// from <see cref="GridWebMcpToolCommand"/> through the <see cref="Command"/>
    /// property, or define a fully custom tool by setting
    /// <see cref="Command"/> to <see cref="GridWebMcpToolCommand.Custom"/>.
    /// </para>
    /// <para>
    /// Built-in tools inherit their default behavior from the grid and can be
    /// customized by overriding properties such as name, description, input schema,
    /// and enabled state.
    /// </para>
    /// <para>
    /// Custom tools do not provide built-in execution logic. Applications must handle
    /// tool invocations through the
    /// <see cref="GridEvents{TValue}.OnWebMcpExecute"/> event and provide an
    /// appropriate execution result.
    /// </para>
    /// <para>
    /// Tool names should be unique within a page. When multiple WebMCP-enabled grids
    /// are present, use <c>GridWebMcpSettings.Name</c> to help avoid tool-name
    /// collisions.
    /// </para>
    /// </remarks>
    /// <example>
    /// <![CDATA[
    /// <SfGrid DataSource="@Orders" EnableWebMcp="true" AllowFiltering="true">
    ///     <GridWebMcpSettings Name="SalesGrid">
    ///         <GridWebMcpTools>
    ///             <GridWebMcpTool
    ///                 Command="GridWebMcpToolCommand.FilterByColumn"
    ///                 Name="filterOrders"
    ///                 Description="Filter grid records by a column." />
    ///
    ///             <GridWebMcpTool
    ///                 Command="GridWebMcpToolCommand.Custom"
    ///                 Name="getTopCustomers"
    ///                 Description="Returns the top customers."
    ///                 InputSchema="@(new
    ///                 {
    ///                     type = "object",
    ///                     properties = new { }
    ///                 })" />
    ///         </GridWebMcpTools>
    ///     </GridWebMcpSettings>
    ///
    ///     <GridEvents TValue="OrderData" OnWebMcpExecute="HandleToolExecute" />
    /// </SfGrid>
    ///
    /// @code {
    ///     private Task HandleToolExecute(WebMcpToolExecuteEventArgs args)
    ///     {
    ///         if (args.ToolName == "getTopCustomers")
    ///         {
    ///             args.Result = new
    ///             {
    ///                 Customers = new[] { "Customer A", "Customer B" }
    ///             };
    ///         }
    ///
    ///         return Task.CompletedTask;
    ///     }
    /// }
    /// ]]>
    /// </example>
    public partial class GridWebMcpTool : SfOwningComponentBase
    {
        #region Cascading Context

        /// <summary>
        /// Cascading reference to the parent <see cref="GridWebMcpSettings"/>.
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal GridWebMcpSettings? Parent { get; set; }

        /// <summary>
        /// Cascading reference to the grid that owns this tool entry.
        /// </summary>
        /// <exclude />
        [CascadingParameter]
        internal IGrid? BaseParent { get; set; }

        /// <summary>
        /// Optional child content. Most tool entries do not declare children.
        /// </summary>
        /// <exclude />
        [Parameter]
        [JsonIgnore]
        public RenderFragment? ChildContent { get; set; }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets the WebMCP command associated with this tool configuration.
        /// </summary>
        /// <value>
        /// A <see cref="GridWebMcpToolCommand"/> that identifies the built-in tool
        /// to customize or specifies a custom tool through
        /// <see cref="GridWebMcpToolCommand.Custom"/>. The default value is
        /// <see cref="GridWebMcpToolCommand.None"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// For built-in commands, this property identifies the WebMCP tool whose
        /// metadata, schema, or availability can be customized through the current
        /// <see cref="GridWebMcpTool"/> configuration.
        /// </para>
        /// <para>
        /// Set this property to <see cref="GridWebMcpToolCommand.Custom"/> to register
        /// a custom tool that is not provided by the grid. Custom tools must define
        /// a unique <see cref="Name"/>, a <see cref="Description"/>, and an
        /// <see cref="InputSchema"/>.
        /// </para>
        /// <para>
        /// Custom tool execution is not handled automatically by the grid. Applications
        /// must process tool invocations through the
        /// <see cref="GridEvents{TValue}.OnWebMcpExecute"/> event and provide a result
        /// by setting <see cref="WebMcpToolExecuteEventArgs.Result"/>. A custom tool
        /// cannot be configured unless this event has an assigned handler.
        /// </para>
        /// <para>
        /// The value <see cref="GridWebMcpToolCommand.None"/> is reserved as the default
        /// state and does not represent a valid configured tool. A tool entry should
        /// specify either a built-in command or
        /// <see cref="GridWebMcpToolCommand.Custom"/>.
        /// </para>
        /// </remarks>
        [Parameter]
        public GridWebMcpToolCommand Command { get; set; } = GridWebMcpToolCommand.None;

        /// <summary>
        /// Gets or sets the tool name exposed to browser-side agents.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the public tool name. For built-in
        /// tools, when no value is specified, the default tool name defined by the
        /// grid is used. For custom tools, this property specifies the unique
        /// identifier exposed through WebMCP registration. The default value is
        /// <see langword="null"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// For built-in tools, this property can be used to override the default
        /// tool name exposed to AI agents. Renaming a built-in tool changes only
        /// the exposed name and does not affect the underlying implementation or
        /// execution behavior.
        /// </para>
        /// <para>
        /// For custom tools, this property is required and uniquely identifies the
        /// tool during registration and execution.
        /// </para>
        /// <para>
        /// During tool execution, the configured tool name is available through
        /// <see cref="WebMcpToolExecuteEventArgs.ToolName"/>, allowing applications
        /// to identify the invoked tool and provide an appropriate result.
        /// </para>
        /// <para>
        /// Tool names should be unique within the application. When multiple
        /// WebMCP-enabled grids are present on the same page,
        /// <see cref="GridWebMcpSettings.Name"/> can be used to help avoid naming
        /// conflicts.
        /// </para>
        /// </remarks>
        [Parameter]
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the description exposed to browser-side agents for this tool.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> containing the tool description. This
        /// parameter is optional for built-in tools, which use the catalog
        /// description when it is omitted. Custom tools must supply a
        /// non-empty description.
        /// </value>
        /// <remarks>
        /// The description is included in the registered tool metadata and can be
        /// used by AI agents to understand the purpose and expected behavior of the
        /// tool. A non-empty description is validated when
        /// <see cref="Command"/> is <see cref="GridWebMcpToolCommand.Custom"/>.
        /// </remarks>
        [Parameter]
        public string? Description { get; set; }

        /// <summary>
        /// Gets or sets the JSON Schema that defines the input arguments accepted by
        /// this tool.
        /// </summary>
        /// <value>
        /// An <see cref="object"/> representing a JSON Schema document. For
        /// built-in tools, the supplied schema replaces the default catalog schema.
        /// Custom tools must supply a valid object schema.
        /// </value>
        /// <remarks>
        /// <para>
        /// The schema is exposed during WebMCP tool registration and allows browser
        /// agents to discover the parameters accepted by the tool.
        /// </para>
        /// <para>
        /// For built-in tools, this property overrides the catalog-defined schema.
        /// For custom tools, the schema is required and defines the supported
        /// input arguments and validation rules.
        /// </para>
        /// <para>
        /// Invalid schema objects may prevent tool registration or cause runtime errors.
        /// Applications are responsible for supplying a valid JSON Schema document.
        /// </para>
        /// </remarks>
        [Parameter]
        public object? InputSchema { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the targeted built-in tool is
        /// enabled.
        /// </summary>
        /// <value>
        /// A <see cref="bool"/> indicating whether the tool should be included in the
        /// effective WebMCP tool catalog. The default value is
        /// <see langword="true"/>.
        /// </value>
        /// <remarks>
        /// <para>
        /// When set to <see langword="false"/>, the referenced built-in tool is
        /// excluded from registration and is not exposed to browser-side agents.
        /// </para>
        /// <para>
        /// This property applies only to entries that target a built-in tool
        /// through <see cref="Command"/>. Custom tools are always included.
        /// </para>
        /// <para>
        /// For custom tools, registration is controlled by the presence of a valid
        /// <see cref="Name"/>, <see cref="Description"/>, and
        /// <see cref="InputSchema"/> definition, together with the associated
        /// <see cref="GridEvents{TValue}.OnWebMcpExecute"/> handler.
        /// </para>
        /// <para>
        /// Setting <see cref="Enabled"/> for a custom tool has no effect on tool
        /// registration or execution behavior.
        /// </para>
        /// </remarks>
        [Parameter]
        public bool Enabled { get; set; } = true;

        #endregion

        #region Lifecycle

        /// <summary>
        /// Initializes the tool entry and registers it with the parent
        /// <see cref="GridWebMcpSettings"/>. Sets
        /// <c>BaseParent.HasWebMcpSettingsChanges</c> so the grid invalidates
        /// its cached effective-tools list on the next render.
        /// </summary>
        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync().ConfigureAwait(true);
            ValidateConfiguration();

            Parent?.UpdateChildProperty(this);

            if (BaseParent != null)
            {
                BaseParent.HasWebMcpSettingsChanges = true;
            }
        }

        private string _snapshotName = string.Empty;
        private string _snapshotDescription = string.Empty;
        private string _snapshotInputSchema = string.Empty;
        private bool _snapshotEnabled = true;
        private GridWebMcpToolCommand _snapshotCommand;
        private bool _snapshotInitialized;

        /// <summary>
        /// Detects parameter mutations and flips the parent's
        /// <c>HasWebMcpSettingsChanges</c> flag.
        /// </summary>
        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync().ConfigureAwait(true);
            ValidateConfiguration();

            // Skip the comparison on the very first invocation — OnInitializedAsync
            // already covered that case.
            if (!_snapshotInitialized)
            {
                _snapshotName = Name ?? string.Empty;
                _snapshotDescription = Description ?? string.Empty;
                _snapshotInputSchema = InputSchema?.ToString() ?? string.Empty;
                _snapshotEnabled = Enabled;
                _snapshotCommand = Command;
                _snapshotInitialized = true;
                return;
            }

            var currentSchema = InputSchema?.ToString() ?? string.Empty;
            var hasChanged =
                !string.Equals(_snapshotName, Name ?? string.Empty, StringComparison.Ordinal) ||
                !string.Equals(_snapshotDescription, Description ?? string.Empty, StringComparison.Ordinal) ||
                !string.Equals(_snapshotInputSchema, currentSchema, StringComparison.Ordinal) ||
                _snapshotEnabled != Enabled ||
                _snapshotCommand != Command;

            if (hasChanged && BaseParent != null)
            {
                BaseParent.HasWebMcpSettingsChanges = true;
            }

            _snapshotName = Name ?? string.Empty;
            _snapshotDescription = Description ?? string.Empty;
            _snapshotInputSchema = currentSchema;
            _snapshotEnabled = Enabled;
            _snapshotCommand = Command;
        }

        private void ValidateConfiguration()
        {
            if (!Enum.IsDefined<GridWebMcpToolCommand>(Command))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(Command),
                    Command,
                    "The configured WebMCP tool command is not defined.");
            }

            if (Command == GridWebMcpToolCommand.None)
            {
                throw new ArgumentException(
                    "GridWebMcpTool requires a Command. Use a built-in command or GridWebMcpToolCommand.Custom.",
                    nameof(Command));
            }

            if (Command != GridWebMcpToolCommand.Custom)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(Name))
            {
                throw new ArgumentException(
                    "GridWebMcpTool requires a non-empty Name when Command is Custom.",
                    nameof(Name));
            }

            if (string.IsNullOrWhiteSpace(Description))
            {
                throw new ArgumentException(
                    "GridWebMcpTool requires a non-empty Description when Command is Custom.",
                    nameof(Description));
            }

            if (InputSchema == null)
            {
                throw new ArgumentException(
                    "GridWebMcpTool requires an InputSchema when Command is Custom.",
                    nameof(InputSchema));
            }

        }

        #endregion
    }
}
