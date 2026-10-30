using System.Collections.Generic;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Responses partial — owns the shared <see cref="JsonOptions"/> instance
    /// and the four response builders used by the dispatcher pipeline.
    ///
    /// Cancellation responses are flagged <c>IsError = false</c> by design —
    /// the cancellation sentinel prevents agent retry loops and is therefore
    /// a successful, intentional outcome rather than a failure.
    /// </summary>
    public partial class WebMcpAdapter<TValue>
    {
        // ===== Phase 2: Structured JSON serialization with camelCase =====
        // Use a shared JsonSerializerOptions so every response serializes consistently
        // and to avoid allocating new options on every tool call.
        private static readonly System.Text.Json.JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        // RESPONSE BUILDERS

        private static WebMcpToolResponse CreateSuccessResponse(object result)
        {
            return new WebMcpToolResponse
            {
                Content = new List<WebMcpToolResponse.ContentItem>
                {
                    new()
                    {
                        Type = "text",
                        Text = System.Text.Json.JsonSerializer.Serialize(result, JsonOptions)
                    }
                },
                IsError = false
            };
        }

        private static WebMcpToolResponse CreateStructuredErrorResponse(
            WebMcpErrorCode errorCode,
            string? toolName,
            string message)
        {
            // Serialize as JSON object so agents can parse both code and message
            var payload = new
            {
                errorCode = (int)errorCode,
                errorName = errorCode.ToString(),
            };
            return new WebMcpToolResponse
            {
                Content = new List<WebMcpToolResponse.ContentItem>
                {
                    new()
                    {
                        Type = "text",
                        Text = System.Text.Json.JsonSerializer.Serialize(payload, JsonOptions)
                    }
                },
                IsError = true
            };
        }

        private static WebMcpToolResponse CreateCancelledResponse(string message)
        {
            return new WebMcpToolResponse
            {
                Content = new List<WebMcpToolResponse.ContentItem>
                {
                    new() { Type = "text", Text = message }
                },
                IsError = false  // Important: cancellation is not an error
            };
        }
    }
}
