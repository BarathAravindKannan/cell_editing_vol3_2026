// <copyright file="GridKeyDispatchTable.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 4.
// Helpers shared by the keyboard entry methods.
// </copyright>

using System;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Static helpers shared by the keyboard entry methods
    /// (<c>ProcessKeyDown</c> in <c>FocusHandler</c> and
    /// <c>ProcessKeyCombination</c>). The action-keyed switch lives in the
    /// entry methods themselves; this class only carries helpers that both
    /// paths need.
    /// </summary>
    internal static class GridKeyDispatchTable
    {
        /// <summary>
        /// Returns true when a legacy token is bound by default to a header-only
        /// action (SortColumn, GroupColumn, ReorderColumnLeft/Right,
        /// ToggleHeaderCheckbox). These tokens have meaning only when the focus
        /// is on a header cell. When a user has rebound them via
        /// <c>KeyboardShortcuts</c>, the resolver resolves them against
        /// the Header zone by default rather than DataCell, so a JS-routed key
        /// (which doesn't carry the (row, cell) context) still finds the user's
        /// binding.
        /// </summary>
        internal static bool IsHeaderOnlyLegacyToken(string? legacyToken)
        {
            if (string.IsNullOrEmpty(legacyToken))
            {
                return false;
            }
            return legacyToken.Equals("Tab", StringComparison.Ordinal)
                || legacyToken.Equals("ShiftTab", StringComparison.Ordinal)
                || legacyToken.Equals("Enter", StringComparison.Ordinal)
                || legacyToken.Equals("CtrlEnter", StringComparison.Ordinal)
                || legacyToken.Equals("ShiftEnter", StringComparison.Ordinal)
                || legacyToken.Equals("CtrlSpace", StringComparison.Ordinal)
                || legacyToken.Equals("Space", StringComparison.Ordinal)
                || legacyToken.Equals("CtrlLeft", StringComparison.Ordinal)
                || legacyToken.Equals("CtrlRight", StringComparison.Ordinal)
                || legacyToken.Equals("AltDown", StringComparison.Ordinal)
                || legacyToken.Equals("AltUp", StringComparison.Ordinal)
                || legacyToken.Equals("CtrlUp", StringComparison.Ordinal)
                || legacyToken.Equals("CtrlDown", StringComparison.Ordinal)
                || legacyToken.Equals("ArrowUp", StringComparison.Ordinal)
                || legacyToken.Equals("ArrowDown", StringComparison.Ordinal)
                || legacyToken.Equals("ArrowLeft", StringComparison.Ordinal)
                || legacyToken.Equals("ArrowRight", StringComparison.Ordinal)
                || legacyToken.Equals("AltW", StringComparison.Ordinal);
        }
    }
}
