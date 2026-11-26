// <copyright file="CompiledShortcuts.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 3 (task 3.5)
// Immutable per-grid compiled bindings.
// </copyright>

using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Immutable per-grid compiled bindings. Constructed by
    /// <see cref="GridKeyboardShortcutCompiler"/>; the only place a fresh
    /// instance is published is at compile time.
    /// </summary>
    internal sealed class CompiledShortcuts
    {
        /// <summary>Per-scope action → list of chords.</summary>
        public IReadOnlyDictionary<GridKeyboardScope, IReadOnlyDictionary<GridKeyboardAction, IReadOnlyList<NormalizedKeyBinding>>> ScopeBindings { get; }

        /// <summary>Actions that have been explicitly disabled in any scope (empty list with the disable marker).</summary>
        public IReadOnlySet<GridKeyboardAction> DisabledActions { get; }

        /// <summary>Actions that have at least one non-empty override binding in any scope.</summary>
        public IReadOnlySet<GridKeyboardAction> OverrideActions { get; }

        public CompiledShortcuts(
            IReadOnlyDictionary<GridKeyboardScope, IReadOnlyDictionary<GridKeyboardAction, IReadOnlyList<NormalizedKeyBinding>>> scopeBindings,
            IReadOnlySet<GridKeyboardAction> disabledActions,
            IReadOnlySet<GridKeyboardAction> overrideActions)
        {
            ScopeBindings = scopeBindings;
            DisabledActions = disabledActions;
            OverrideActions = overrideActions;
        }
    }
}
