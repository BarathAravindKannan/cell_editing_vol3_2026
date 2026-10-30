using System;
using System.Diagnostics.CodeAnalysis;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Represents error categories for WebMCP tool execution failures.
    /// Allows agents to programmatically distinguish failure types and adjust retry logic.
    /// </summary>
    /// <remarks>
    /// Top-level internal enum in <c>Syncfusion.Blazor.Grids.Actions</c>
    /// so that <see cref="WebMcpException"/> (also top-level) can carry
    /// it without requiring a generic type parameter, and so that
    /// <see cref="WebMcpAdapter{TValue}"/> handler partials and
    /// <see cref="DictionaryExtensions"/> can reference it by simple name.
    /// </remarks>
    internal enum WebMcpErrorCode
    {
        /// <summary>
        /// Tool name was not found in registry.
        /// Agent should verify the tool name (e.g., case sensitivity, prefix).
        /// </summary>
        UnknownTool,

        /// <summary>
        /// One or more required arguments are missing or have an invalid type.
        /// Agent should inspect the tool's InputSchema and re-submit corrected arguments.
        /// </summary>
        InvalidArgument,

        /// <summary>
        /// Tool exists and arguments are valid, but the underlying Grid operation failed.
        /// Agent should inspect the InnerException details or retry with idempotent semantics.
        /// </summary>
        ExecutionFailed,

        /// <summary>
        /// User cancelled the operation via the OnWebMcpExecute event.
        /// Agent should NOT retry — this is terminal for this user session.
        /// </summary>
        Cancelled,

        /// <summary>
        /// User denied the confirmation dialog for a write operation.
        /// Agent should NOT retry — this is a user choice.
        /// </summary>
        UserDenied,

        /// <summary>
        /// Tool execution exceeded a configured timeout.
        /// Agent may retry with a smaller dataset or different approach.
        /// </summary>
        Timeout,

        /// <summary>
        /// The requested operation is not supported.
        /// Agent should not retry — this is a terminal error.
        /// </summary>
        OperationNotSupported
    }

    /// <summary>
    /// Exception thrown by WebMCP tool execution when a tool fails.
    /// Carries a structured <see cref="WebMcpErrorCode"/> for programmatic handling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Top-level internal class in <c>Syncfusion.Blazor.Grids.Actions</c>.
    /// Lives next to the <see cref="WebMcpAdapter{TValue}"/> partials so
    /// the file family groups the exception with the adapter that raises
    /// it. Was previously nested inside the adapter, but C# disallows
    /// extension methods inside nested static classes, so the exception
    /// was promoted to a top-level type to keep
    /// <see cref="DictionaryExtensions"/> simple.
    /// </para>
    /// <para>
    /// This exception type is intended to be caught by the WebMcpAdapter dispatcher
    /// and converted into a WebMcpToolResponse with IsError=true.
    /// </para>
    /// <para>
    /// External callers (JavaScript interop, testing) should not see this exception
    /// directly — it is an internal contract.
    /// </para>
    /// </remarks>
    [SuppressMessage("Design", "CA1064:ExceptionsShouldBePublic", 
        Justification = "WebMcpException is intentionally internal and never exposed to external callers. It is caught and transformed by the WebMcpAdapter dispatcher into a WebMcpToolResponse before being returned to JavaScript interop layer. The exception serves as an internal contract within the grid action handlers.")]
    internal class WebMcpException : Exception
    {
        /// <summary>
        /// Standard parameterless constructor — required by exception design
        /// guidelines (CA1032). Frameworks and serialization libraries that
        /// expect to be able to construct an empty exception of the target
        /// type rely on this overload.
        /// </summary>
        internal WebMcpException()
        {
        }

        /// <summary>
        /// Standard message constructor — required by exception design
        /// guidelines (CA1032). Equivalent to the base
        /// <see cref="Exception(string)"/> ctor.
        /// </summary>
        /// <param name="message">Human-readable description of the failure.</param>
        internal WebMcpException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Standard message + inner-exception constructor — required by
        /// exception design guidelines (CA1032). Equivalent to the base
        /// <see cref="Exception(string, Exception)"/> ctor.
        /// </summary>
        /// <param name="message">Human-readable description of the failure.</param>
        /// <param name="innerException">The original cause of the failure.</param>
        internal WebMcpException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Gets the structured error code for this failure.
        /// </summary>
        internal WebMcpErrorCode ErrorCode { get; }

        /// <summary>
        /// Gets the name of the tool that failed (e.g., "sortByColumn").
        /// </summary>
        internal string? ToolName { get; }

        /// <summary>
        /// Initializes a new WebMcpException with a code and message.
        /// Framework-internal — the dispatcher no longer uses this overload;
        /// retained in case a future handler needs to raise a tool-agnostic
        /// error.
        /// </summary>
        /// <param name="errorCode">The structured error classification.</param>
        /// <param name="message">Human-readable description of the failure.</param>
        internal WebMcpException(WebMcpErrorCode errorCode, string message)
            : base(message)
        {
            ErrorCode = errorCode;
            ToolName = null;
        }

        /// <summary>
        /// Initializes a new WebMcpException with a code, tool name, and message.
        /// Used internally by tool handlers and the dispatcher.
        /// </summary>
        /// <param name="errorCode">The structured error classification.</param>
        /// <param name="toolName">The name of the tool that failed.</param>
        /// <param name="message">Human-readable description of the failure.</param>
        internal WebMcpException(WebMcpErrorCode errorCode, string toolName, string message)
            : base(message)
        {
            ErrorCode = errorCode;
            ToolName = toolName;
        }

        /// <summary>
        /// Initializes a new WebMcpException that wraps an inner exception.
        /// Framework-internal — used only by the dispatcher when wrapping
        /// unknown exceptions into a structured <c>WebMcpException</c>.
        /// Host callback exceptions are wrapped by the dispatcher.
        /// </summary>
        /// <param name="errorCode">The structured error classification.</param>
        /// <param name="toolName">The name of the tool that failed.</param>
        /// <param name="message">Human-readable description of the failure.</param>
        /// <param name="innerException">The original cause of the failure.</param>
        internal WebMcpException(WebMcpErrorCode errorCode, string toolName, string message, Exception innerException)
            : base(message, innerException)
        {
            ErrorCode = errorCode;
            ToolName = toolName;
        }
    }
}
