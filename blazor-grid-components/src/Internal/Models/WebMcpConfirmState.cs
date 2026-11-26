using System;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Manages the state of a pending WebMCP tool confirmation dialog.
    /// </summary>
    internal class WebMcpConfirmState
    {
        /// <summary>
        /// Gets or sets the name of the tool being confirmed.
        /// </summary>
        public string ToolName { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets a value indicating whether the confirmation dialog is visible.
        /// </summary>
        public bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the task completion source that resolves when the user responds.
        /// </summary>
        public TaskCompletionSource<bool>? PendingTaskCompletionSource { get; set; }
    }
}