using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Dictionary extension methods for safe JSON-serialized argument access.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Lives alongside the <see cref="WebMcpAdapter{TValue}"/> partials
    /// because every read and write handler uses these helpers to coerce
    /// loosely-typed agent arguments into string / bool / int[] values
    /// without swallowing nulls.
    /// </para>
    /// <para>
    /// <see cref="ValidateRequired"/> throws
    /// <see cref="WebMcpException"/> with
    /// <see cref="WebMcpErrorCode.InvalidArgument"/>
    /// so the dispatcher surfaces missing arguments verbatim instead of a
    /// generic <c>ExecutionFailed</c>.
    /// </para>
    /// <para>
    /// Must remain a <b>top-level</b> static class (not nested inside
    /// <c>WebMcpAdapter&lt;TValue&gt;</c>) because C# requires extension
    /// methods to live in a non-nested static class. The file name keeps
    /// it grouped with the adapter's other partials.
    /// </para>
    /// </remarks>
    internal static class DictionaryExtensions
    {
        internal static string GetString(this Dictionary<string, object> dict, string key)
            => dict.TryGetValue(key, out var value) ? value?.ToString() ?? "" : "";

        internal static bool GetBool(this Dictionary<string, object> dict, string key)
            => dict.TryGetValue(key, out var value) && value is bool b && b;

        /// <summary>
        /// Extracts a JSON integer array from the args dictionary. The
        /// dictionary is populated by <c>JsonSerializer.Deserialize&lt;Dictionary&lt;string, object&gt;&gt;</c>
        /// — every JSON value arrives as a <see cref="JsonElement"/> whose
        /// <see cref="JsonElement.ValueKind"/> matches the original JSON shape.
        /// Note that <see cref="JsonElement"/> does <b>not</b> implement
        /// <see cref="System.Collections.IEnumerable"/>, so naïvely type-testing
        /// for <c>IEnumerable</c> silently returns <see cref="Array.Empty{T}"/>()
        /// and any downstream loop sees a zero-length array. We must dispatch
        /// on the <see cref="JsonElement"/> branch before the legacy fallback.
        /// </summary>
        internal static int[] GetIntArray(this Dictionary<string, object> dict, string key)
        {
            if (!dict.TryGetValue(key, out var value) || value is null)
                return Array.Empty<int>();

            // System.Text.Json path — the dictionary value is a JsonElement
            // when the args were deserialised from a JSON payload (the common
            // case for every WebMCP tool call landing on the server).
            if (value is JsonElement je)
            {
                if (je.ValueKind != JsonValueKind.Array)
                {
                    // Non-array JSON (object / null / string / number scalar).
                    // Treat as "absent" rather than throwing — the handler's
                    // zero-length check will then produce a precise message.
                    return Array.Empty<int>();
                }

                var len = je.GetArrayLength();
                var result = new int[len];
                for (var i = 0; i < len; i++)
                {
                    var item = je[i];
                    switch (item.ValueKind)
                    {
                        case JsonValueKind.Number:
                            // GetInt32 throws if the number is too large and we
                            // want a typed signal in that case.
                            result[i] = item.GetInt32();
                            break;
                        case JsonValueKind.String:
                            if (int.TryParse(item.GetString(), out var parsed))
                            {
                                result[i] = parsed;
                            }
                            else
                            {
                                throw new WebMcpException(
                                    WebMcpErrorCode.InvalidArgument,
                                    "(argument)",
                                    $"Element {i} of '{key}' could not be parsed as int. Received: '{item.GetString()}'.");
                            }
                            break;
                        default:
                            throw new WebMcpException(
                                WebMcpErrorCode.InvalidArgument,
                                "(argument)",
                                $"Element {i} of '{key}' must be a JSON number. Received kind: {item.ValueKind}.");
                    }
                }
                return result;
            }

            // Fallback path: a real .NET IEnumerable arrives only from unit
            // tests or C# callers bypassing the JSON layer. Keep the legacy
            // behaviour so those tests still pass.
            if (value is System.Collections.IEnumerable arr)
            {
                return arr.Cast<object>().Select(x => Convert.ToInt32(x, System.Globalization.CultureInfo.InvariantCulture)).ToArray();
            }

            return Array.Empty<int>();
        }

        /// <summary>
        /// Phase 2: Validate that required string arguments are present and non-empty.
        /// </summary>
        /// <param name="dict">Argument dictionary.</param>
        /// <param name="toolName">Tool name for error context.</param>
        /// <param name="keys">Required argument keys to validate.</param>
        /// <exception cref="WebMcpException">Thrown with InvalidArgument code when a required key is missing.</exception>
        internal static void ValidateRequired(this Dictionary<string, object> dict, string toolName, params string[] keys)
        {
            var missing = new List<string>();
            foreach (var key in keys)
            {
                if (!dict.TryGetValue(key, out var value) || value is null ||
                    (value is string s && string.IsNullOrWhiteSpace(s)))
                {
                    missing.Add(key);
                }
            }
            if (missing.Count > 0)
            {
                throw new WebMcpException(
                    WebMcpErrorCode.InvalidArgument,
                    toolName,
                    $"Missing or empty required argument(s) for '{toolName}': {string.Join(", ", missing)}. " +
                    $"Refer to the tool's InputSchema for the expected arguments.");
            }
        }
    }
}
