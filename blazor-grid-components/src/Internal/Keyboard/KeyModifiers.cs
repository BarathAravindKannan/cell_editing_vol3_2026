// <copyright file="KeyModifiers.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 0
// Bit-flag modifier representation for zero-allocation per-keystroke comparisons.
// </copyright>

using System;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Bit-flag modifier representation for normalized key bindings.
    /// Used in <see cref="NormalizedKeyBinding"/> so that key comparisons allocate no heap.
    /// </summary>
    [Flags]
    internal enum KeyModifiers
    {
        /// <summary>No modifiers pressed.</summary>
        None = 0,

        /// <summary>Ctrl (Control) key pressed.</summary>
        Ctrl = 1 << 0,

        /// <summary>Shift key pressed.</summary>
        Shift = 1 << 1,

        /// <summary>Alt (Option on Mac) key pressed.</summary>
        Alt = 1 << 2,

        /// <summary>Meta (Cmd on Mac, Windows key) pressed.</summary>
        Meta = 1 << 3,

        /// <summary>Internal flag — on Mac, Ctrl in user config means Ctrl OR Meta at runtime.</summary>
        MetaOrCtrl = 1 << 4
    }
}
