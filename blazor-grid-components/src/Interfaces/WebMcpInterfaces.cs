using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Response format for WebMCP tool execution.
    /// Returned as JSON to the calling agent.
    /// </summary>
    /// <remarks>
    /// The type is <c>internal</c> because it is an implementation
    /// detail of the WebMcpAdapter dispatcher — external callers receive
    /// a JSON-serialized payload via the JS-invokable bridge in
    /// <c>GridJSInteropAdaptor.InvokeWebMcpToolExecute</c>, which converts
    /// the result to a string before it leaves the assembly.
    /// </remarks>
    internal class WebMcpToolResponse
    {
        /// <summary>
        /// True if this response represents an error condition.
        /// Important: Cancellations have IsError=false (not an error, user choice).
        /// </summary>
        public bool IsError { get; set; }

        /// <summary>
        /// List of content items in response. The serialised JSON shape is
        /// unchanged when the type is consumed via the JS bridge; only the
        /// C# accessibility is narrowed to <c>internal</c>.
        /// </summary>
        public List<ContentItem>? Content { get; set; }

        /// <summary>
        /// Content item in response. Demoted to <c>internal</c> per CA1034
        /// (do not nest externally-visible types) — the JSON contract is
        /// unchanged, only the C# accessibility is narrower.
        /// </summary>
        public class ContentItem
        {
            /// <summary>
            /// MIME type of content (e.g., "text", "json").
            /// </summary>
            public string? Type { get; set; }

            /// <summary>
            /// Content text (can be JSON-serialized object or message).
            /// </summary>
            public string? Text { get; set; }
        }
    }

    /// <summary>
    /// Internal implementation class for a WebMCP tool definition.
    /// </summary>
    internal class WebMcpTool : IWebMcpTool
    {
        /// <summary>
        /// Unique name of the tool (camelCase).
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable description.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// JSON Schema for input parameters.
        /// </summary>
        public object? InputSchema { get; set; }

        /// <summary>
        /// JSON Schema for output response.
        /// </summary>
        public object? OutputSchema { get; set; }

        /// <summary>
        /// Tool annotations (ReadOnlyHint, etc).
        /// </summary>
        public object? Annotations { get; set; }
    }

    /// <summary>
    /// Provides the public metadata for a WebMCP tool.
    /// </summary>
    public sealed class WebMcpToolInfo : IWebMcpTool
    {
        /// <summary>
        /// Gets the unique name of the tool.
        /// </summary>
        public string Name { get; init; } = string.Empty;

        /// <summary>
        /// Gets the description of the tool.
        /// </summary>
        public string Description { get; init; } = string.Empty;

        /// <summary>
        /// Gets the JSON schema that describes the tool's input arguments.
        /// </summary>
        public object? InputSchema { get; init; }
    }
}
