// <copyright file="GridKeyboardActionDefaults.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 2 (task 2.3)
// Single source of truth for default bindings. Phase 8/9 paging and selection
// entries are included so the parity test can drive them.
// </copyright>

using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Per-zone default bindings. This is the <strong>single source of truth</strong>
    /// for default chords: docs, the parity test and the runtime resolver all
    /// consume this table so they cannot drift.
    /// </summary>
    /// <remarks>
    /// Bindings are encoded as legacy tokens (the same names emitted by
    /// <c>Utils.GetKeyCombination</c>) so the action dispatcher can match them
    /// without going through a separate chord table.
    /// </remarks>
    internal static class GridKeyboardActionDefaults
    {
        // ---- DataCell ----
        private static readonly Dictionary<GridKeyboardAction, string[]> _dataCellDefaults = new()
        {
            { GridKeyboardAction.NavigateUp,        new[] { "ShiftEnter" } },
            { GridKeyboardAction.NavigateDown,      new[] { "Enter", "ArrowDown", "ShiftDown" } },
            { GridKeyboardAction.NavigateLeft,      new[] { "ShiftTab" } },
            { GridKeyboardAction.NavigateRight,     new[] { "Tab", "ArrowRight", "ShiftRight" } },
            { GridKeyboardAction.NavigateRowStart,  new[] { "Home" } },
            { GridKeyboardAction.NavigateRowEnd,    new[] { "End" } },
            { GridKeyboardAction.NavigateGridStart, new[] { "CtrlHome" } },
            { GridKeyboardAction.NavigateGridEnd,   new[] { "CtrlEnd" } },
            { GridKeyboardAction.StartEdit,         new[] { "F2" } },
            { GridKeyboardAction.AddRecord,         new[] { "Insert" } },
            { GridKeyboardAction.DeleteRecord,      new[] { "Delete" } },
            { GridKeyboardAction.SelectAll,         new[] { "CtrlA" } },
            { GridKeyboardAction.PageUp,            new[] { "PageUp" } },
            { GridKeyboardAction.PageDown,          new[] { "PageDown" } },
            { GridKeyboardAction.FirstPage,         new[] { "CtrlAltPageUp" } },
            { GridKeyboardAction.LastPage,          new[] { "CtrlAltPageDown" } },
            { GridKeyboardAction.NextPageGroup,     new[] { "AltPageDown" } },
            { GridKeyboardAction.PreviousPageGroup, new[] { "AltPageUp" } },
            { GridKeyboardAction.SelectToggle,      new[] { "Space" } },
            { GridKeyboardAction.ToggleRowSelection, new[] { "CtrlSpace" } },
            // Phase 15
            { GridKeyboardAction.MoveFocusToFirstRow, new[] { "AltW" } },
            { GridKeyboardAction.FocusFilterBar,      new[] { "ArrowUp" } },
            { GridKeyboardAction.FocusAddForm,        new[] { "ArrowDown" } },
            // Phase 12 — undo/redo (four-gate alignment: JS gate,
            // Utils.GetKeyCombination, HandleUndo, GridRow.CellRenderer.cs).
            { GridKeyboardAction.Undo, new[] { "CtrlZ", "MetaZ" } },
            { GridKeyboardAction.Redo, new[] { "CtrlY", "MetaShiftZ" } },
            // Phase 16 — selection extensions.
            { GridKeyboardAction.SelectRowRange, new[] { "ShiftUp", "ShiftDown", "ShiftLeft", "ShiftRight" } }
            // ExtendSelection has NO default keyboard binding: it is driven by the
            // legacy MouseAndKeyArgs path (Shift+Click / Ctrl+Click) and is
            // marked [NonBindable] in the validator. Users who want keyboard
            // range selection should bind SelectRowRange instead.
        };

        // ---- Header ----
        private static readonly Dictionary<GridKeyboardAction, string[]> _headerDefaults = new()
        {
            { GridKeyboardAction.NavigateLeft,          new[] { "ShiftTab" } },
            { GridKeyboardAction.NavigateRight,         new[] { "Tab" } },
            { GridKeyboardAction.SortColumn,            new[] { "Enter", "CtrlEnter", "ShiftEnter" } },
            { GridKeyboardAction.GroupColumn,           new[] { "CtrlSpace" } },
            { GridKeyboardAction.ReorderColumnLeft,     new[] { "CtrlLeft" } },
            { GridKeyboardAction.ReorderColumnRight,    new[] { "CtrlRight" } },
            { GridKeyboardAction.ToggleHeaderCheckbox,  new[] { "Space" } },
            { GridKeyboardAction.MoveFocusToFirstRow,   new[] { "AltW" } },
            { GridKeyboardAction.FocusFilterBar,        new[] { "ArrowDown" } },
            { GridKeyboardAction.FocusAddForm,          new[] { "ArrowUp" } },
            // OpenColumnMenu / OpenFilterMenu intentionally have NO defaults.
            // Both would bind to AltDown in the Header zone; AltDown is also
            // dispatched by the legacy inline null-branch based on context
            // (ShowColumnMenu vs AllowFiltering). Registering a default here
            // would shadow the legacy branch and break the filter-only case.
            // Users who want named-action behavior for AltDown should rebind
            // via KeyboardShortcuts.
        };

        // ---- EditCell ----
        private static readonly Dictionary<GridKeyboardAction, string[]> _editCellDefaults = new()
        {
            { GridKeyboardAction.Escape, new[] { "Escape" } },
            // Phase 17 — edit-end tab transitions. The EditCell-zone match
            // wins over the DataCell NavigateLeft/NavigateRight bindings, so
            // Tab/ShiftTab inside a Cell-mode editor end the edit and move to
            // the next/previous editable cell rather than navigating out of
            // the grid (HandleTabNavigationInCellMode owns the default case).
            { GridKeyboardAction.EditEndAndTabNext,      new[] { "Tab" } },
            { GridKeyboardAction.EditEndAndShiftTabPrev, new[] { "ShiftTab" } }
        };

        // ---- GroupRow ----
        // Order matters: resolver picks the first-registered action per token
        // (Dictionary iteration is insertion-ordered). ExpandGroup/CollapseGroup
        // are placed BEFORE NavigateRight/Left so the default ArrowRight/ArrowLeft
        // on a caption row expands/collapses rather than navigates.
        // ExpandAllGroup/CollapseAllGroup are placed BEFORE NavigateDown/Up so
        // CtrlDown/CtrlUp expands/collapses all groups rather than navigating.
        private static readonly Dictionary<GridKeyboardAction, string[]> _groupRowDefaults = new()
        {
            { GridKeyboardAction.ExpandGroup,       new[] { "ArrowRight" } },
            { GridKeyboardAction.CollapseGroup,     new[] { "ArrowLeft" } },
            { GridKeyboardAction.ExpandAllGroup,    new[] { "CtrlDown" } },
            { GridKeyboardAction.CollapseAllGroup,  new[] { "CtrlUp" } },
            { GridKeyboardAction.NavigateDown,      new[] { "ArrowDown" } },
            { GridKeyboardAction.NavigateUp,        new[] { "ArrowUp" } },
            { GridKeyboardAction.NavigateLeft,      new[] { "ArrowLeft" } },
            { GridKeyboardAction.NavigateRight,     new[] { "ArrowRight" } }
        };

        // ---- DetailRow ----
        // Same ordering rule: ExpandDetailRow / CollapseDetailRow are placed
        // before NavigateDown so the default Enter on a detail row toggles
        // expansion rather than navigating. (CtrlDown/CtrlUp are unique to
        // detail rows so position does not matter.)
        private static readonly Dictionary<GridKeyboardAction, string[]> _detailRowDefaults = new()
        {
            { GridKeyboardAction.ExpandDetailRow,   new[] { "Enter", "AltDown" } },
            { GridKeyboardAction.CollapseDetailRow, new[] { "Enter", "AltUp" } },
            { GridKeyboardAction.ExpandAllDetailRows,   new[] { "CtrlDown" } },
            { GridKeyboardAction.CollapseAllDetailRows, new[] { "CtrlUp" } },
            { GridKeyboardAction.NavigateDown,      new[] { "ArrowDown" } },
            { GridKeyboardAction.NavigateUp,        new[] { "ArrowUp" } },
            { GridKeyboardAction.NavigateLeft,      new[] { "ArrowLeft" } },
            { GridKeyboardAction.NavigateRight,     new[] { "ArrowRight" } }
        };

        // ---- Common ----
        private static readonly Dictionary<GridKeyboardAction, string[]> _commonDefaults = new()
        {
            { GridKeyboardAction.Print,           new[] { "CtrlP", "MetaP" } },
            // Phase 11 — clipboard. Cut excluded per D7.
            // The JS gate (scripts/clipboard.ts) intercepts the default chord
            // before the C# dispatcher sees it; the Common-scope default here
            // is consumed by the resolver for documentation/parity tests and
            // for any custom-bound chord the developer wires up.
            { GridKeyboardAction.Copy,            new[] { "CtrlC", "MetaC" } },
            { GridKeyboardAction.CopyWithHeaders, new[] { "CtrlShiftH" } },
            { GridKeyboardAction.Paste,           new[] { "CtrlV", "MetaV" } }
        };

        /// <summary>
        /// Returns the scope that should be consulted for a given detected zone.
        /// </summary>
        public static GridKeyboardScope ScopeForZone(GridKeyboardZone zone)
        {
            return zone switch
            {
                GridKeyboardZone.Header    => GridKeyboardScope.Header,
                GridKeyboardZone.DataCell  => GridKeyboardScope.DataCell,
                GridKeyboardZone.EditCell  => GridKeyboardScope.EditCell,
                GridKeyboardZone.GroupRow  => GridKeyboardScope.GroupRow,
                GridKeyboardZone.DetailRow => GridKeyboardScope.DetailRow,
                _ => GridKeyboardScope.Common
            };
        }

        /// <summary>
        /// Maps a legacy token to the action that owns it for a given zone. Returns
        /// <c>true</c> if a default binding exists.
        /// </summary>
        /// <remarks>
        /// When <paramref name="scope"/> is not <see cref="GridKeyboardScope.Common"/>,
        /// the per-zone dictionary is consulted first; on a miss the
        /// <c>_commonDefaults</c> table is searched as a fallback. This makes actions
        /// whose default bindings are only registered in the Common scope (for
        /// example <see cref="GridKeyboardAction.Print"/> → <c>CtrlP</c>/<c>MetaP</c>)
        /// discoverable from any zone, which is required for both default-chord
        /// dispatch and the override-suppression layer in
        /// <c>GridKeyResolver.ResolveForKeystroke</c>.
        /// </remarks>
        public static bool TryGetAction(string legacyToken, GridKeyboardScope scope, out GridKeyboardAction action)
        {
            action = default;
            if (string.IsNullOrEmpty(legacyToken))
            {
                return false;
            }
            foreach (var kv in GetDictionary(scope))
            {
                foreach (var t in kv.Value)
                {
                    if (string.Equals(t, legacyToken, StringComparison.Ordinal))
                    {
                        action = kv.Key;
                        return true;
                    }
                }
            }
            if (scope != GridKeyboardScope.Common)
            {
                foreach (var kv in _commonDefaults)
                {
                    foreach (var t in kv.Value)
                    {
                        if (string.Equals(t, legacyToken, StringComparison.Ordinal))
                        {
                            action = kv.Key;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private static Dictionary<GridKeyboardAction, string[]> GetDictionary(GridKeyboardScope scope)
        {
            return scope switch
            {
                GridKeyboardScope.Common    => _commonDefaults,
                GridKeyboardScope.Header    => _headerDefaults,
                GridKeyboardScope.DataCell  => _dataCellDefaults,
                GridKeyboardScope.EditCell  => _editCellDefaults,
                GridKeyboardScope.GroupRow  => _groupRowDefaults,
                GridKeyboardScope.DetailRow => _detailRowDefaults,
                _ => _dataCellDefaults
            };
        }

    }
}
