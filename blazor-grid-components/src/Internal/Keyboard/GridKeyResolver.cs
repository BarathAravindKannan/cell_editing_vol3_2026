// <copyright file="GridKeyResolver.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 3 (tasks 3.1, 3.2, 3.3)
// Chord parser + five-layer resolver.
// </copyright>

using System;
using System.Collections.Generic;
using System.Text;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Result of a single resolve call. Computed once at the top of each entry
    /// path and passed down. The <see cref="PreventDefault"/> flag drives
    /// <c>e.preventDefault()</c> in JS (when the binding owns <c>consumeDefault</c>)
    /// or sets <c>bf.Cancel = true</c> in C#.
    /// </summary>
    internal readonly struct ResolveResult
    {
        public ResolveResult(GridKeyboardAction? action, bool consumed, bool preventDefault, GridKeyboardAction? suppressedOriginalAction)
        {
            Action = action;
            Consumed = consumed;
            PreventDefault = preventDefault;
            SuppressedOriginalAction = suppressedOriginalAction;
        }

        /// <summary>The action to dispatch, or <c>null</c> if no grid action matches.</summary>
        public GridKeyboardAction? Action { get; }

        /// <summary>True when the chord was consumed by the resolver (a custom binding matched, or a default was suppressed by an override).</summary>
        public bool Consumed { get; }

        /// <summary>True when the matched binding owns <c>consumeDefault</c>; the caller should call <c>preventDefault()</c>.</summary>
        public bool PreventDefault { get; }

        /// <summary>
        /// When a chord is consumed by layer 3 (suppression), this records the action whose
        /// default would have matched. Used by tests and the legacy bridge to report state.
        /// </summary>
        public GridKeyboardAction? SuppressedOriginalAction { get; }
    }

    /// <summary>
    /// Parses chord strings and resolves a pressed key against the compiled configuration.
    /// </summary>
    /// <remarks>
    /// Resolution follows five layers:
    /// <list type="number">
    /// <item><description>Zone-specific custom binding for the detected zone.</description></item>
    /// <item><description><c>Common</c> scope custom binding (fallback).</description></item>
    /// <item><description>Suppression: a chord that is a built-in default of action <em>A</em> is consumed when <em>A</em> has a custom entry (override or disable) in an applicable scope <strong>and</strong> the chord is not itself bound to another action by a custom entry.</description></item>
    /// <item><description>Defaults from <see cref="GridKeyboardActionDefaults"/>.</description></item>
    /// <item><description>Browser — no grid action.</description></item>
    /// </list>
    /// </remarks>
    internal static class GridKeyResolver
    {
        // ---- 3.1 Parser (Parse) ----

        /// <summary>
        /// Parses a single chord string (e.g. <c>"Ctrl+Shift+G"</c>) into a
        /// <see cref="NormalizedKeyBinding"/>. Returns <c>null</c> on invalid input.
        /// </summary>
        public static NormalizedKeyBinding? Parse(string? chord)
        {
            if (string.IsNullOrWhiteSpace(chord))
            {
                return null;
            }
            if (chord.Length > 64)
            {
                return null; // SFGK010 size cap
            }
            for (int i = 0; i < chord.Length; i++)
            {
                // Reject non-ASCII control characters but allow the rest of Unicode.
                if (chord[i] < 0x20)
                {
                    return null;
                }
            }
            var tokens = chord.Split('+');
            KeyModifiers mods = KeyModifiers.None;
            string? code = null;
            for (int i = 0; i < tokens.Length; i++)
            {
                var t = tokens[i].Trim();
                if (t.Length == 0)
                {
                    continue;
                }
                if (string.Equals(t, "Alt", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(t, "Option", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= KeyModifiers.Alt;
                }
                else if (string.Equals(t, "Ctrl", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(t, "Control", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= KeyModifiers.Ctrl;
                }
                else if (string.Equals(t, "Cmd", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(t, "Meta", StringComparison.OrdinalIgnoreCase) ||
                         string.Equals(t, "Command", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= KeyModifiers.Meta;
                }
                else if (string.Equals(t, "Shift", StringComparison.OrdinalIgnoreCase))
                {
                    mods |= KeyModifiers.Shift;
                }
                else if (i == tokens.Length - 1)
                {
                    // The last token is the key code.
                    code = GridKeyCodeCatalog.NormalizeCode(t);
                    if (!GridKeyCodeCatalog.IsKnown(code))
                    {
                        return null; // SFGK001
                    }
                }
                else
                {
                    return null; // Malformed: a non-modifier token in the modifier slot.
                }
            }
            if (code == null)
            {
                return null;
            }
            return new NormalizedKeyBinding(mods, code, code);
        }

        /// <summary>
        /// Parses a comma-separated chord list and returns the parsed bindings. Empty
        /// entries are dropped. A leading <c>+</c> on a chord marks it as additive
        /// (the parser strips the marker; the compiler applies it later).
        /// </summary>
        public static List<NormalizedKeyBinding> ParseList(string? raw, out bool additiveOnly)
        {
            additiveOnly = false;
            var list = new List<NormalizedKeyBinding>();
            if (string.IsNullOrEmpty(raw))
            {
                return list;
            }
            foreach (var rawPart in raw.Split(','))
            {
                var part = rawPart.Trim();
                if (part.Length == 0)
                {
                    continue;
                }
#if NET11_0_OR_GREATER
                if (part.StartsWith('+', System.StringComparison.Ordinal) && part.Length > 1)
#else
                if (part.StartsWith('+') && part.Length > 1)
#endif
                {
                    additiveOnly = true;
                    part = part.Substring(1).Trim();
                }
                var parsed = Parse(part);
                if (parsed.HasValue)
                {
                    list.Add(parsed.Value);
                }
            }
            return list;
        }

        // ---- 3.2 / 3.3 Resolver (ResolveForKeystroke) ----

        /// <summary>
        /// Resolves a single keystroke against the compiled configuration. The
        /// <paramref name="legacyToken"/> is the canonical string from
        /// <c>Utils.GetKeyCombination</c> and is used as a fallback for the
        /// default-binding layer. <paramref name="normalizePressed"/> is the
        /// result of <c>Utils.GetNormalizedKey</c> for layout-independent
        /// matching against custom chords.
        /// </summary>
        public static ResolveResult ResolveForKeystroke(
            CompiledShortcuts compiled,
            NormalizedKeyBinding normalizePressed,
            string? legacyToken,
            GridKeyboardZone zone)
        {
            if (zone == GridKeyboardZone.None)
            {
                return new ResolveResult(null, false, false, null);
            }

            var scope = GridKeyboardActionDefaults.ScopeForZone(zone);

            // Layer 1: zone-specific custom bindings
            if (compiled.ScopeBindings.TryGetValue(scope, out var zoneBindings))
            {
                foreach (var kv in zoneBindings)
                {
                    foreach (var chord in kv.Value)
                    {
                        if (chord.Matches(normalizePressed.Code!, normalizePressed.Modifiers))
                        {
                            // Disabling entry (empty list with the disable marker) is
                            // handled at compile time; we only see chords here.
                            return new ResolveResult(kv.Key, true, true, null);
                        }
                    }
                }
            }

            // Layer 2: Common scope
            if (scope != GridKeyboardScope.Common &&
                compiled.ScopeBindings.TryGetValue(GridKeyboardScope.Common, out var commonBindings))
            {
                foreach (var kv in commonBindings)
                {
                    foreach (var chord in kv.Value)
                    {
                        if (chord.Matches(normalizePressed.Code!, normalizePressed.Modifiers))
                        {
                            return new ResolveResult(kv.Key, true, true, null);
                        }
                    }
                }
            }

            // Layer 3: suppression — if the chord is a default of action A and A is
            // overridden or disabled in an applicable scope, consume it.
            if (!string.IsNullOrEmpty(legacyToken) &&
                GridKeyboardActionDefaults.TryGetAction(legacyToken, scope, out var defaultAction))
            {
                if (compiled.DisabledActions.Contains(defaultAction))
                {
                    return new ResolveResult(null, true, false, defaultAction);
                }
                if (compiled.OverrideActions.Contains(defaultAction) &&
                    !(defaultAction == GridKeyboardAction.SortColumn &&
                      string.Equals(legacyToken, "ShiftEnter", StringComparison.Ordinal)))
                {
                    // The action has an override but the pressed chord isn't one of
                    // the override's chords. Consume the default to prevent leakage.
                    return new ResolveResult(null, true, false, defaultAction);
                }
            }

            // Layer 4: defaults — match by legacy token first (this is the path
            // that existing focus code already exercised; the chord table is the
            // new custom-binding path).
            if (!string.IsNullOrEmpty(legacyToken) &&
                GridKeyboardActionDefaults.TryGetAction(legacyToken, scope, out var matchedAction))
            {
                return new ResolveResult(matchedAction, true, false, null);
            }

            // Layer 5: no grid action.
            return new ResolveResult(null, false, false, null);
        }
    }
}
