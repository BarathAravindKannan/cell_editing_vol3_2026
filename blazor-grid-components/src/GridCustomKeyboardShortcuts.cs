// <copyright file="GridCustomKeyboardShortcuts.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 1 (task 1.2)
// Public POCO for per-zone keyboard bindings.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// Configures custom keyboard bindings for the grid.
    /// <para>
    /// The grid exposes six scope properties — <see cref="Common"/>, <see cref="Header"/>,
    /// <see cref="DataCell"/>, <see cref="EditCell"/>, <see cref="GroupRow"/> and
    /// <see cref="DetailRow"/>. Each scope is a mapping from a
    /// <see cref="GridKeyboardAction"/> to a string of one or more comma-separated
    /// chords.
    /// </para>
    /// <para>
    /// <strong>Value semantics</strong>
    /// <list type="bullet">
    /// <item><description>Override (a non-empty value) <em>replaces</em> the default chords for that action.</description></item>
    /// <item><description>An empty string or <c>null</c> <em>disables</em> the action entirely.</description></item>
    /// <item><description>A leading <c>+</c> (<c>"+Ctrl+J"</c>) is <em>additive</em>: the default chords remain in effect alongside the new chord.</description></item>
    /// <item><description>A missing key in the dictionary means "use the default chord(s) for this action".</description></item>
    /// <item><description>For <see cref="GridKeyboardAction.SortColumn"/>, the default <c>Shift+Enter</c> clear-sort gesture remains available when the action is rebound; an explicit custom binding for that chord takes precedence.</description></item>
    /// </list>
    /// </para>
    /// <para>
    /// Precedence when multiple scopes bind the same chord: a zone-specific binding wins
    /// over the <see cref="Common"/> scope. Within a scope, the first binding for a chord
    /// wins (the validator emits <c>SFGK003</c> on duplicates).
    /// </para>
    /// </summary>
    public class GridCustomKeyboardShortcuts
    {
        /// <summary>
        /// Fallback scope for actions valid in several zones
        /// (e.g. <see cref="GridKeyboardAction.Print"/>). A zone-specific binding for
        /// the same chord wins over <see cref="Common"/>.
        /// </summary>
        public Dictionary<GridKeyboardAction, string>? Common { get; set; }

        /// <summary>
        /// Header-row bindings. Valid actions include <see cref="GridKeyboardAction.SortColumn"/>,
        /// <see cref="GridKeyboardAction.GroupColumn"/>, <see cref="GridKeyboardAction.ReorderColumnLeft"/>,
        /// <see cref="GridKeyboardAction.ReorderColumnRight"/>, <see cref="GridKeyboardAction.ToggleHeaderCheckbox"/>,
        /// <see cref="GridKeyboardAction.NavigateLeft"/>, <see cref="GridKeyboardAction.NavigateRight"/>,
        /// <see cref="GridKeyboardAction.MoveFocusToFirstRow"/>, <see cref="GridKeyboardAction.FocusFilterBar"/>,
        /// <see cref="GridKeyboardAction.FocusAddForm"/>, <see cref="GridKeyboardAction.OpenColumnMenu"/>,
        /// and <see cref="GridKeyboardAction.OpenFilterMenu"/>.
        /// </summary>
        public Dictionary<GridKeyboardAction, string>? Header { get; set; }

        /// <summary>Data-cell bindings. Most navigation and CRUD actions live here.</summary>
        public Dictionary<GridKeyboardAction, string>? DataCell { get; set; }

        /// <summary>Edit-cell bindings. Only the action <see cref="GridKeyboardAction.Escape"/>
        /// is owned by the JS entry path here; other in-cell shortcuts (text input native
        /// editing) are left to the browser.</summary>
        public Dictionary<GridKeyboardAction, string>? EditCell { get; set; }

        /// <summary>Group-caption-row bindings.</summary>
        public Dictionary<GridKeyboardAction, string>? GroupRow { get; set; }

        /// <summary>Detail-row bindings.</summary>
        public Dictionary<GridKeyboardAction, string>? DetailRow { get; set; }

        /// <summary>
        /// Returns the canonical string snapshot of this configuration. Used by the
        /// diff-guard in <c>SfGrid</c> to detect in-place mutations, object recreation
        /// and dictionary reordering without hashing mutable state. See D6 / N5 in
        /// the OpenSpec change for the rationale.
        /// </summary>
        /// <remarks>
        /// Format (deterministic):
        /// <code>
        /// Scope1:Action1:Chord1,Chord2;Action2:Chord3|Scope2:Action4:Chord4
        /// </code>
        /// Scopes are ordered by declaration order; actions within a scope are ordered
        /// by enum value; chords within an action are ordered alphabetically after
        /// modifier normalization.
        /// </remarks>
        internal string ToCanonicalString()
        {
            var sb = new StringBuilder(128);
            AppendScope(sb, "Common", Common);
            AppendScope(sb, "Header", Header);
            AppendScope(sb, "DataCell", DataCell);
            AppendScope(sb, "EditCell", EditCell);
            AppendScope(sb, "GroupRow", GroupRow);
            AppendScope(sb, "DetailRow", DetailRow);
            return sb.ToString();
        }

        private static void AppendScope(StringBuilder sb, string name, Dictionary<GridKeyboardAction, string>? scope)
        {
            if (scope == null || scope.Count == 0)
            {
                return;
            }
            var actions = new List<GridKeyboardAction>(scope.Keys);
            actions.Sort();
            bool any = false;
            foreach (var action in actions)
            {
                var raw = scope[action];
                if (raw == null)
                {
                    continue;
                }
                if (!any)
                {
                    sb.Append(name);
                    sb.Append(':');
                    any = true;
                }
                else
                {
                    sb.Append(';');
                }
                sb.Append(action.ToString());
                sb.Append(':');
                sb.Append(NormalizeChordList(raw));
            }
            if (any)
            {
                sb.Append('|');
            }
        }

        /// <summary>
        /// Normalizes a comma-separated chord list by trimming whitespace, dropping
        /// empty entries, and sorting chords alphabetically. The leading <c>+</c>
        /// additive marker is preserved.
        /// </summary>
        internal static string NormalizeChordList(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return string.Empty;
            }
            var parts = raw.Split(',');
            var trimmed = new List<string>(parts.Length);
            foreach (var p in parts)
            {
                var t = p.Trim();
                if (t.Length == 0)
                {
                    continue;
                }
#if NET11_0_OR_GREATER
                if (t.StartsWith('+', System.StringComparison.Ordinal) && t.Length > 1)
#else
                if (t.StartsWith('+') && t.Length > 1)
#endif
                {
                    // Additive: keep the marker; normalize the rest.
                    var inner = t.Substring(1).Trim();
                    inner = NormalizeChord(inner);
                    trimmed.Add("+" + inner);
                }
                else
                {
                    trimmed.Add(NormalizeChord(t));
                }
            }
            trimmed.Sort(StringComparer.Ordinal);
            return string.Join(",", trimmed);
        }

        /// <summary>
        /// Normalizes a single chord by sorting its modifiers (Alt, Ctrl, Meta, Shift) and
        /// trimming whitespace. Modifiers are case-insensitive.
        /// </summary>
        internal static string NormalizeChord(string chord)
        {
            if (string.IsNullOrEmpty(chord))
            {
                return string.Empty;
            }
            var tokens = chord.Split('+');
            bool hasCode = false;
            string code = string.Empty;
            bool alt = false, ctrl = false, meta = false, shift = false;
            foreach (var raw in tokens)
            {
                var t = raw.Trim();
                if (t.Length == 0)
                {
                    continue;
                }
                if (string.Equals(t, "Alt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t, "Option", StringComparison.OrdinalIgnoreCase))
                {
                    alt = true;
                }
                else if (string.Equals(t, "Ctrl", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(t, "Control", StringComparison.OrdinalIgnoreCase))
                {
                    ctrl = true;
                }
                else if (string.Equals(t, "Cmd", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(t, "Meta", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(t, "Command", StringComparison.OrdinalIgnoreCase))
                {
                    meta = true;
                }
                else if (string.Equals(t, "Shift", StringComparison.OrdinalIgnoreCase))
                {
                    shift = true;
                }
                else
                {
                    hasCode = true;
                    code = t;
                }
            }
            var sb = new StringBuilder();
            if (alt) AppendMod(sb, "Alt");
            if (ctrl) AppendMod(sb, "Ctrl");
            if (meta) AppendMod(sb, "Meta");
            if (shift) AppendMod(sb, "Shift");
            if (hasCode)
            {
                if (sb.Length > 0) sb.Append('+');
                sb.Append(code);
            }
            return sb.ToString();
        }

        private static void AppendMod(StringBuilder sb, string name)
        {
            if (sb.Length > 0) sb.Append('+');
            sb.Append(name);
        }
    }
}
