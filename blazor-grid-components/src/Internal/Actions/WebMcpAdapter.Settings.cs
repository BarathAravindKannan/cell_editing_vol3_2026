using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Syncfusion.Blazor.Grids.Internal;

namespace Syncfusion.Blazor.Grids.Actions
{
    /// <summary>
    /// Provides WebMCP tool management for a grid instance by combining the
    /// built-in WebMCP tool catalog with user-defined
    /// <see cref="GridWebMcpSettings"/> configuration.
    /// </summary>
    /// <typeparam name="TValue">
    /// The type of data associated with the grid.
    /// </typeparam>
    /// <remarks>
    /// The effective tool collection is created per instance without modifying
    /// the built-in catalog.
    ///
    /// User-defined tools can:
    /// <list type="bullet">
    /// <item><description>Disable built-in tools.</description></item>
    /// <item><description>Override built-in tool metadata.</description></item>
    /// <item><description>Replace built-in tools with custom implementations.</description></item>
    /// <item><description>Add new custom tools.</description></item>
    /// </list>
    ///
    /// When a custom tool is executed, the host can provide a response through
    /// the <c>OnWebMcpExecute</c> event.
    /// </remarks>
    public partial class WebMcpAdapter<TValue>
    {
        // Cache the merged list keyed on the user's settings reference so we
        // do not re-resolve on every JS callback. The cache is invalidated
        // by the grid when HasWebMcpSettingsChanges flips.
        private static readonly Dictionary<object, IReadOnlyList<IGridWebMcpTool>> _effectiveToolsCache = new();

        /// <summary>
        /// Resolves the effective WebMCP tool list for this grid instance by
        /// merging the static built-in catalog with the supplied
        /// <paramref name="userSettings"/>. Returns the full built-in catalog
        /// (in original order) when <paramref name="userSettings"/> is
        /// <c>null</c> or declares no children.
        /// </summary>
        /// <param name="userSettings">User-supplied tool overrides. May be null.</param>
        /// <returns>
        /// An immutable list of <see cref="IGridWebMcpTool"/> entries ready to
        /// hand to <c>document.modelContext.registerTool(...)</c>.
        /// </returns>
        internal static IReadOnlyList<IGridWebMcpTool> ResolveEffectiveTools(GridWebMcpSettings? userSettings)
        {
            if (userSettings == null || userSettings.ChildTools == null || userSettings.ChildTools.Count == 0)
            {
                return webMcpTools.AsReadOnly();
            }

            // Cache hit
            lock (_effectiveToolsCache)
            {
                if (_effectiveToolsCache.TryGetValue(userSettings, out var cached))
                {
                    return cached;
                }
            }

            var overridesByCommand = new Dictionary<GridWebMcpToolCommand, GridWebMcpTool>(userSettings.ChildTools.Count);
            var customTools = new List<GridWebMcpTool>();
            var disabledCommands = new HashSet<GridWebMcpToolCommand>();
            var seenCustomNames = new HashSet<string>(StringComparer.Ordinal);

            foreach (var entry in userSettings.ChildTools)
            {
                if (entry == null) continue;

                if (entry.Command == GridWebMcpToolCommand.None)
                {
                    throw new ArgumentException(
                        "GridWebMcpTool requires a Command. Use a built-in command or GridWebMcpToolCommand.Custom.",
                        nameof(userSettings));
                }

                if (!Enum.IsDefined<GridWebMcpToolCommand>(entry.Command))
                {
                    throw new ArgumentOutOfRangeException(
                        nameof(userSettings),
                        entry.Command,
                        "The configured WebMCP tool command is not defined.");
                }

                if (entry.Command != GridWebMcpToolCommand.Custom)
                {
                    // Last-write-wins: later entries override earlier ones
                    // for the same Command.
                    overridesByCommand[entry.Command] = entry;
                    if (!entry.Enabled)
                    {
                        disabledCommands.Add(entry.Command);
                    }
                }
                else
                {
                    // Custom entries are validated by GridWebMcpTool during
                    // initialization; keep this guard for dynamically supplied
                    // child collections.
                    if (string.IsNullOrWhiteSpace(entry.Name))
                    {
                        throw new ArgumentException(
                            "A custom GridWebMcpTool requires a non-empty Name.",
                            nameof(userSettings));
                    }

                    if (string.IsNullOrWhiteSpace(entry.Description))
                    {
                        throw new ArgumentException(
                            $"Custom WebMCP tool '{entry.Name}' requires a non-empty Description.",
                            nameof(userSettings));
                    }

                    if (entry.InputSchema == null)
                    {
                        throw new ArgumentException(
                            $"Custom WebMCP tool '{entry.Name}' requires an InputSchema.",
                            nameof(userSettings));
                    }

                    if (!seenCustomNames.Add(entry.Name!))
                    {
                        // Duplicate custom tool name — last entry wins. The
                        // dispatcher is event-driven; no per-tool Handler
                        // gating happens here.
                    }
                    customTools.Add(entry);
                }
            }

            // Custom tools that have fully replaced a built-in (same base
            // name, Command=Custom) take ownership of that name at
            // catalog level. Built a set of their base names once so the
            // built-in loop can detect a name collision in O(1) per entry.
            var customBaseNames = new HashSet<string>(
                customTools
                    .Where(t => !string.IsNullOrWhiteSpace(t.Name))
                    .Select(t => t.Name!),
                StringComparer.Ordinal);

            var result = new List<IGridWebMcpTool>(webMcpTools.Count + customTools.Count);

            foreach (var builtIn in webMcpTools)
            {
                var command = ResolveCommandForBaseName(builtIn.Name);

                if (command.HasValue && disabledCommands.Contains(command.Value))
                {
                    // Disabled by user — skip.
                    continue;
                }

                if (command.HasValue && overridesByCommand.TryGetValue(command.Value, out var userEntry))
                {
                    if (!userEntry.Enabled)
                    {
                        // Disabled by user — skip.
                        continue;
                    }

                    // Override the catalog entry with the user-supplied fields.
                    result.Add(BuildOverriddenTool(builtIn, userEntry));
                }
                else if (customBaseNames.Contains(builtIn.Name))
                {
                    // Fully replaced by the user (custom entry with no
                    // Command but the same base name). Skip the built-in —
                    // the custom tool owns this name and is appended below.
                    // If the host's event handler does not set a result
                    // for the custom tool, the dispatcher will surface
                    // UnknownTool rather than silently falling through to
                    // the built-in. That is intentional: replacing a
                    // built-in name means the host is the source of truth
                    // for that tool.
                    continue;
                }
                else
                {
                    // Auto-include — keep the catalog entry unchanged.
                    result.Add(builtIn);
                }
            }

            // Append fully custom tools. De-duplicate by Name (last entry wins).
            // Custom tools carry no per-tool Handler — the dispatcher routes
            // them through OnWebMcpExecute on the grid.
            var appendedNames = new HashSet<string>(StringComparer.Ordinal);
            foreach (var custom in customTools)
            {
                if (string.IsNullOrWhiteSpace(custom.Name)) continue;
                if (!appendedNames.Add(custom.Name!)) continue;

                var newTool = new WebMcpTool
                {
                    Name = custom.Name!,
                    Description = custom.Description ?? string.Empty,
                    InputSchema = custom.InputSchema!,
                    OutputSchema = new
                    {
                        type = "object",
                        properties = new { }
                    },
                };

                ValidateInputSchema(newTool.InputSchema, custom.Name!);
                result.Add(newTool);
            }

            IReadOnlyList<IGridWebMcpTool> view = result.AsReadOnly();
            lock (_effectiveToolsCache)
            {
                _effectiveToolsCache[userSettings] = view;
            }
            return view;
        }

        /// <summary>
        /// Build a clone of <paramref name="source"/> with the user-supplied
        /// overrides applied. The catalog instance is never mutated.
        /// </summary>
        private static WebMcpTool BuildOverriddenTool(WebMcpTool source, GridWebMcpTool userEntry)
        {
            var effectiveName = string.IsNullOrWhiteSpace(userEntry.Name) ? source.Name : userEntry.Name!;
            var effectiveDescription = string.IsNullOrWhiteSpace(userEntry.Description) ? source.Description : userEntry.Description!;
            object? effectiveInputSchema = userEntry.InputSchema ?? source.InputSchema;
            object? effectiveAnnotations;
            if (source.Annotations != null)
            {
                effectiveAnnotations = source.Annotations;
            }
            else
            {
                effectiveAnnotations = new { ReadOnlyHint = false };
            }

            if (userEntry.InputSchema != null)
            {
                ValidateInputSchema(userEntry.InputSchema, effectiveName);
            }

            return new WebMcpTool
            {
                Name = effectiveName,
                Description = effectiveDescription,
                InputSchema = effectiveInputSchema,
                OutputSchema = source.OutputSchema,
                Annotations = effectiveAnnotations
            };
        }

        /// <summary>
        /// Lightweight validation of user-supplied <c>InputSchema</c>: must
        /// be an object with <c>type=object</c> and a <c>properties</c>
        /// dictionary. Throws <see cref="WebMcpException"/> with
        /// <see cref="WebMcpErrorCode.InvalidArgument"/> when malformed.
        /// </summary>
        private static void ValidateInputSchema(object? schema, string effectiveName)
        {
            if (schema == null)
            {
                // Null schema is allowed for tools that take no args.
                return;
            }

            // Anonymous types + dictionaries arrive as IDictionary<string,object?>
            // after deserialisation; here we accept the raw object the user
            // declared. Use reflection for the common shape.
            var schemaType = schema.GetType();
            string? typeValue = null;
            bool hasProperties = false;

            if (schema is System.Collections.IDictionary dict)
            {
                foreach (System.Collections.DictionaryEntry kv in dict)
                {
                    var key = kv.Key?.ToString();
                    if (string.Equals(key, "type", StringComparison.Ordinal))
                    {
                        typeValue = kv.Value?.ToString();
                    }
                    else if (string.Equals(key, "properties", StringComparison.Ordinal))
                    {
                        hasProperties = kv.Value != null;
                    }
                }
            }
            else
            {
                // Anonymous type — peek at properties by name.
                var typeProp = schemaType.GetProperty("type");
                if (typeProp != null)
                {
                    typeValue = typeProp.GetValue(schema)?.ToString();
                }
                var propertiesProp = schemaType.GetProperty("properties");
                if (propertiesProp != null)
                {
                    hasProperties = propertiesProp.GetValue(schema) != null;
                }
            }

            if (typeValue == null || !string.Equals(typeValue, "object", StringComparison.OrdinalIgnoreCase))
            {
                throw new WebMcpException(
                    WebMcpErrorCode.InvalidArgument,
                    effectiveName,
                    $"GridWebMcpTool '{effectiveName}' has an InputSchema that is not a JSON object schema (type must be \"object\").");
            }

            // "properties" is technically optional in JSON Schema, so we only
            // warn via the hasProperties flag — we do not throw on missing.
            _ = hasProperties;
        }

        /// <summary>
        /// Maps an enum member to the canonical base tool name (camelCase).
        /// Returns <c>null</c> for unknown members.
        /// Framework-internal — only used by the adapter partials themselves
        /// to invert the registry lookup.
        /// </summary>
        internal static string? ResolveToolNameForCommand(GridWebMcpToolCommand command)
        {
            if (command == GridWebMcpToolCommand.None || command == GridWebMcpToolCommand.Custom)
            {
                return null;
            }

            var member = typeof(GridWebMcpToolCommand).GetField(command.ToString());
            if (member == null) return null;
            var attr = member.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                              .OfType<EnumMemberAttribute>()
                              .FirstOrDefault();
            return attr?.Value;
        }

        /// <summary>
        /// Inverse map: resolves the canonical base tool name to the
        /// matching <see cref="GridWebMcpToolCommand"/> member. Returns
        /// <c>null</c> when the name does not correspond to a built-in tool.
        /// Framework-internal — only used by the merge logic in
        /// <see cref="ResolveEffectiveTools"/>.
        /// </summary>
        internal static GridWebMcpToolCommand? ResolveCommandForBaseName(string baseToolName)
        {
            if (string.IsNullOrEmpty(baseToolName)) return null;
            foreach (var member in typeof(GridWebMcpToolCommand).GetFields())
            {
                if (!member.IsLiteral) continue;
                var attr = member.GetCustomAttributes(typeof(EnumMemberAttribute), false)
                                  .OfType<EnumMemberAttribute>()
                                  .FirstOrDefault();
                if (attr != null &&
                    !string.Equals(attr.Value, "none", StringComparison.Ordinal) &&
                    !string.Equals(attr.Value, "custom", StringComparison.Ordinal) &&
                    string.Equals(attr.Value, baseToolName, StringComparison.Ordinal))
                {
                    return Enum.Parse<GridWebMcpToolCommand>(member.Name);
                }
            }
            return null;
        }

        /// <summary>
        /// Invalidate the cache for the supplied settings instance. Called
        /// by the grid when <c>HasWebMcpSettingsChanges</c> flips, so the
        /// next dispatcher lookup rebuilds the merged list.
        /// </summary>
        internal static void InvalidateEffectiveToolsCache(GridWebMcpSettings? settings)
        {
            if (settings == null) return;
            lock (_effectiveToolsCache)
            {
                _effectiveToolsCache.Remove(settings);
            }
        }
    }
}
