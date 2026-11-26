// <copyright file="GridKeyboardShortcutCompiler.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 3 (task 3.5)
// Compiler + FromLegacy bridge.
// </copyright>

using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Compiles a <see cref="GridCustomKeyboardShortcuts"/> configuration into an
    /// immutable <see cref="CompiledShortcuts"/> snapshot. The compiler is the
    /// single point where defaults, user overrides, and legacy
    /// <c>GridKeySettings</c> bindings merge.
    /// </summary>
    internal static class GridKeyboardShortcutCompiler
    {
        /// <summary>
        /// Compiles a configuration. Returns an empty-but-non-null instance when
        /// <paramref name="shortcuts"/> 
        /// <c>null</c> so default-chord routing still works.
        /// </summary>
        public static CompiledShortcuts Compile(
            GridCustomKeyboardShortcuts? shortcuts,
            IGrid? grid,
            bool isMac,
            Action<KeyboardWarning> warn)
        {
            // 1. Build a per-scope map of overrides. Each entry is a list of chords
            //    the user wants the action to fire on. Empty list = disable; missing
            //    key = defaults; "+chord" = additive.
            var scopeBindings = new Dictionary<GridKeyboardScope, Dictionary<GridKeyboardAction, List<NormalizedKeyBinding>>>();
            var disabled = new HashSet<GridKeyboardAction>();
            var overrides = new HashSet<GridKeyboardAction>();

            void AddOverride(GridKeyboardScope scope, GridKeyboardAction action, List<NormalizedKeyBinding> chords, bool isDisable)
            {
                if (!scopeBindings.TryGetValue(scope, out var map))
                {
                    map = new Dictionary<GridKeyboardAction, List<NormalizedKeyBinding>>();
                    scopeBindings[scope] = map;
                }
                if (isDisable)
                {
                    map[action] = new List<NormalizedKeyBinding>(); // empty list = disable
                    disabled.Add(action);
                    overrides.Add(action);
                    return;
                }
                if (!map.TryGetValue(action, out var existing))
                {
                    existing = new List<NormalizedKeyBinding>();
                    map[action] = existing;
                }
                else
                {
                    // Merge additive: keep the first non-additive entry intact, then append.
                }
                foreach (var c in chords) existing.Add(c);
                if (existing.Count > 0) overrides.Add(action);
            }

            if (shortcuts != null)
            {
                AddScopeOverrides(shortcuts.Common,    GridKeyboardScope.Common,    AddOverride);
                AddScopeOverrides(shortcuts.Header,    GridKeyboardScope.Header,    AddOverride);
                AddScopeOverrides(shortcuts.DataCell,  GridKeyboardScope.DataCell,  AddOverride);
                AddScopeOverrides(shortcuts.EditCell,  GridKeyboardScope.EditCell,  AddOverride);
                AddScopeOverrides(shortcuts.GroupRow,  GridKeyboardScope.GroupRow,  AddOverride);
                AddScopeOverrides(shortcuts.DetailRow, GridKeyboardScope.DetailRow, AddOverride);
            }

            // 2. FromLegacy
            if (grid?.KeySettings is { } legacy)
            {
                ApplyLegacy(legacy, scopeBindings, disabled, overrides);
            }

            // 3. Build immutable snapshot
            var immutableScope = new Dictionary<GridKeyboardScope, IReadOnlyDictionary<GridKeyboardAction, IReadOnlyList<NormalizedKeyBinding>>>(scopeBindings.Count);
            foreach (var kv in scopeBindings)
            {
                var inner = new Dictionary<GridKeyboardAction, IReadOnlyList<NormalizedKeyBinding>>(kv.Value.Count);
                foreach (var innerKv in kv.Value)
                {
                    inner[innerKv.Key] = innerKv.Value;
                }
                immutableScope[kv.Key] = inner;
            }

            var compiled = new CompiledShortcuts(immutableScope, disabled, overrides);

            // 4. Validate
            GridKeyboardShortcutValidator.Validate(compiled, grid, warn);
            return compiled;
        }

        private static void AddScopeOverrides(
            Dictionary<GridKeyboardAction, string>? scope,
            GridKeyboardScope scopeEnum,
            Action<GridKeyboardScope, GridKeyboardAction, List<NormalizedKeyBinding>, bool> add)
        {
            if (scope == null)
            {
                return;
            }
            foreach (var kv in scope)
            {
                if (kv.Value == null)
                {
                    add(scopeEnum, kv.Key, new List<NormalizedKeyBinding>(), true);
                    continue;
                }
                if (kv.Value.Length == 0)
                {
                    add(scopeEnum, kv.Key, new List<NormalizedKeyBinding>(), true);
                    continue;
                }
                var chords = GridKeyResolver.ParseList(kv.Value, out var additive);
                add(scopeEnum, kv.Key, chords, false);
            }
        }

        // ---- 3.5 FromLegacy ----

        /// <summary>
        /// Maps the four legacy navigation strings on <c>GridKeySettings</c> to
        /// the new action model. Comma-split; empty tokens dropped; per-action
        /// precedence is <c>KeyboardShortcuts</c> &gt; legacy &gt; defaults.
        /// </summary>
        private static void ApplyLegacy(
            GridKeySettings legacy,
            Dictionary<GridKeyboardScope, Dictionary<GridKeyboardAction, List<NormalizedKeyBinding>>> scopeBindings,
            HashSet<GridKeyboardAction> disabled,
            HashSet<GridKeyboardAction> overrides)
        {
            // SFGK011 — warn if both legacy and new set the same action. Today
            // we just respect the new binding (it lives in scopeBindings first),
            // so we skip emitting redundant legacy entries that would be
            // suppressed anyway.
            void MaybeAdd(GridKeyboardScope scope, GridKeyboardAction action, string raw)
            {
                if (string.IsNullOrEmpty(raw))
                {
                    return;
                }
                if (overrides.Contains(action))
                {
                    return; // custom wins
                }
                if (!scopeBindings.TryGetValue(scope, out var map))
                {
                    map = new Dictionary<GridKeyboardAction, List<NormalizedKeyBinding>>();
                    scopeBindings[scope] = map;
                }
                if (map.ContainsKey(action))
                {
                    return; // already has a binding in this scope (e.g. from custom)
                }
                var parsed = GridKeyResolver.ParseList(raw, out _);
                if (parsed.Count == 0)
                {
                    return;
                }
                map[action] = parsed;
                overrides.Add(action);
            }

            MaybeAdd(GridKeyboardScope.DataCell,  GridKeyboardAction.NavigateUp,    legacy.MoveUpCell);
            MaybeAdd(GridKeyboardScope.DataCell,  GridKeyboardAction.NavigateDown,  legacy.MoveDownCell);
            MaybeAdd(GridKeyboardScope.DataCell,  GridKeyboardAction.NavigateLeft,  legacy.MoveLeftCell);
            MaybeAdd(GridKeyboardScope.DataCell,  GridKeyboardAction.NavigateRight, legacy.MoveRightCell);
        }
    }
}
