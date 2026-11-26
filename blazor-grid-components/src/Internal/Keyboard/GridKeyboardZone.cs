// <copyright file="GridKeyboardZone.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 2 (task 2.1)
// Detection vs configuration zones.
// </copyright>

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Runtime <em>detection</em> zone for a keyboard event. The value is computed
    /// per keystroke from the currently focused (row, cell); it is never cached.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="None"/> means "bypass" — events on the filter bar, toolbar, pager,
    /// search box, dialogs, column chooser, footer and any other chrome should
    /// return this value and not be processed by the grid resolver.
    /// </para>
    /// <para>
    /// <see cref="GridKeyboardScope.Common"/> is <strong>not</strong> a detected zone; it is a
    /// configuration-only fallback scope. Detection returns one of the cell-shape
    /// zones. The configuration scope is modelled by
    /// <see cref="GridKeyboardScope"/>.
    /// </para>
    /// </remarks>
    internal enum GridKeyboardZone
    {
        /// <summary>No zone — bypass the grid resolver.</summary>
        None = 0,

        /// <summary>Header row (sort, group, reorder, header checkbox).</summary>
        Header = 1,

        /// <summary>Data cell.</summary>
        DataCell = 2,

        /// <summary>Cell that is currently being edited.</summary>
        EditCell = 3,

        /// <summary>Group caption row.</summary>
        GroupRow = 4,

        /// <summary>Detail (expanded) row.</summary>
        DetailRow = 5
    }

    /// <summary>
    /// Configuration <em>scope</em> on <see cref="GridCustomKeyboardShortcuts"/>.
    /// Includes the <see cref="Common"/> fallback scope which is never detected.
    /// </summary>
    internal enum GridKeyboardScope
    {
        /// <summary>Fallback scope. Never detected; consulted only when no zone-specific binding matches.</summary>
        Common = 0,

        /// <summary>Header-row scope.</summary>
        Header = 1,

        /// <summary>Data-cell scope.</summary>
        DataCell = 2,

        /// <summary>Edit-cell scope.</summary>
        EditCell = 3,

        /// <summary>Group-caption-row scope.</summary>
        GroupRow = 4,

        /// <summary>Detail-row scope.</summary>
        DetailRow = 5
    }
}
