using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// WebMCP adapter for Blazor DataGrid — orchestrator shell.
    /// Manages tool definitions and routes tool execution requests to Grid methods.
    ///
    /// Split for single responsibility across partial files
    /// (matches the project's <c>ClassName.Purpose.cs</c> naming convention,
    /// see <c>docs/code-guidelines/naming-conventions.md</c>):
    /// <list type="bullet">
    /// <item><description><c>WebMcpAdapter.Catalog.cs</c> — built-in tool schemas + <c>InitializeTools</c></description></item>
    /// <item><description><c>WebMcpAdapter.Dispatcher.cs</c> — <c>GetTools</c> + <c>ExecuteHandler</c> pipeline</description></item>
    /// <item><description><c>WebMcpAdapter.HandlersRead.cs</c> — read tool handlers</description></item>
    /// <item><description><c>WebMcpAdapter.HandlersWrite.cs</c> — state-changing tool handlers</description></item>
    /// <item><description><c>WebMcpAdapter.Responses.cs</c> — <c>JsonOptions</c> + response builders</description></item>
    /// <item><description><c>WebMcpAdapter.Exceptions.cs</c> — internal <c>WebMcpException</c> + <c>WebMcpErrorCode</c></description></item>
    /// <item><description><c>WebMcpAdapter.DictionaryExtensions.cs</c> — typed argument coercion helpers</description></item>
    /// </list>
    /// </summary>
    public partial class WebMcpAdapter<TValue>
    {
        // Backing field for the parent SfGrid that owns this adapter instance.
        // Tool handlers reach grid state via this single reference.
        private readonly SfGrid<TValue> _parent;

        // Static registry of every WebMcpTool the grid exposes to AI agents.
        // Populated on first access by the static ctor; lives for the assembly lifetime.
        private static readonly List<WebMcpTool> webMcpTools = new();

        static WebMcpAdapter()
        {
            InitializeTools();
        }

        /// <summary>
        /// Initializes a new instance of the WebMcpAdapter class.
        /// Framework-internal constructor — only <see cref="SfGrid{TValue}"/>
        /// instantiates the adapter.
        /// </summary>
        /// <param name="parent">The parent SfGrid instance that this adapter will execute tools against.</param>
        internal WebMcpAdapter(SfGrid<TValue> parent)
        {
            _parent = parent;
        }

        /// <summary>
        /// Registers WebMCP tools with the browser's document.modelContext API.
        /// </summary>
        internal async Task RegisterWebMcpToolsAsync(string? prefix = null)
        {
            if (!_parent.EnableWebMcp || _parent._webMcpAdapter == null) return;

            // Invalidate the effective-tools cache when the user has mutated
            // the settings tree since the last registration.
            if (_parent.HasWebMcpSettingsChanges)
            {
                WebMcpAdapter<TValue>.InvalidateEffectiveToolsCache(_parent.WebMcpSettings);
                _parent.HasWebMcpSettingsChanges = false;
            }

            var tools = WebMcpAdapter<TValue>.ResolveEffectiveTools(_parent.WebMcpSettings);
            var toolPrefix =
                prefix
                ?? (!string.IsNullOrEmpty(_parent.WebMcpSettings?.Name) ? _parent.WebMcpSettings!.Name
                   : (!string.IsNullOrEmpty(_parent.ID) ? _parent.ID
                      : $"SfGrid-{_parent.DataId}"));
            await _parent.InvokeMethod("sfBlazor.Grid.registerWebMcpTools", new object[] { _parent.DataId, tools, toolPrefix }).ConfigureAwait(true);
        }
    }
}



