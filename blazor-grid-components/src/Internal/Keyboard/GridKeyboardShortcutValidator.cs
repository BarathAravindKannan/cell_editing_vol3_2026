// <copyright file="GridKeyboardShortcutValidator.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 3 (task 3.4)
// Non-throwing validator with stable warning IDs.
// </copyright>

using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// A single validator warning. Emitted through the warn sink fan-out but never
    /// raised as an exception.
    /// </summary>
    internal readonly struct KeyboardWarning
    {
        public KeyboardWarning(string id, string message, GridKeyboardAction? action = null, GridKeyboardScope? scope = null)
        {
            Id = id;
            Message = message;
            Action = action;
            Scope = scope;
        }
        public string Id { get; }
        public string Message { get; }
        public GridKeyboardAction? Action { get; }
        public GridKeyboardScope? Scope { get; }
    }

    /// <summary>
    /// Validator that emits stable warning IDs and never throws. The full list of
    /// stable IDs is documented in the OpenSpec change (tasks.md 3.4).
    /// </summary>
    internal static class GridKeyboardShortcutValidator
    {
        // SFGK002 — zone/scope applicability
        private static readonly Dictionary<GridKeyboardAction, GridKeyboardScope[]> _allowedScopes = new()
        {
            // Navigation actions are valid in any cell-shape zone
            { GridKeyboardAction.NavigateUp,        new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NavigateDown,      new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow, GridKeyboardScope.Header } },
            { GridKeyboardAction.NavigateLeft,      new[] { GridKeyboardScope.DataCell, GridKeyboardScope.Header, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NavigateRight,     new[] { GridKeyboardScope.DataCell, GridKeyboardScope.Header, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NavigateRowStart,  new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NavigateRowEnd,    new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NavigateGridStart, new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NavigateGridEnd,   new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.StartEdit,         new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.AddRecord,         new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.DeleteRecord,      new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.SelectAll,         new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.Escape,            new[] { GridKeyboardScope.EditCell, GridKeyboardScope.DataCell, GridKeyboardScope.Header } },
            { GridKeyboardAction.SortColumn,        new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.GroupColumn,       new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.ReorderColumnLeft, new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.ReorderColumnRight,new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.ToggleHeaderCheckbox, new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.PageUp,            new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.PageDown,          new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.FirstPage,         new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.LastPage,          new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.NextPageGroup,     new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.PreviousPageGroup, new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.SelectToggle,      new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.ToggleRowSelection,new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.Print,             new[] { GridKeyboardScope.Common } },

            // Phases 10 / 13 / 14 / 15 (append-only)
            { GridKeyboardAction.MoveFocusToFirstRow,  new[] { GridKeyboardScope.DataCell, GridKeyboardScope.Header } },
            { GridKeyboardAction.FocusFilterBar,       new[] { GridKeyboardScope.DataCell, GridKeyboardScope.Header } },
            { GridKeyboardAction.FocusAddForm,         new[] { GridKeyboardScope.DataCell, GridKeyboardScope.Header } },
            { GridKeyboardAction.OpenColumnMenu,       new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.OpenFilterMenu,       new[] { GridKeyboardScope.Header } },
            { GridKeyboardAction.ExpandGroup,          new[] { GridKeyboardScope.GroupRow } },
            { GridKeyboardAction.CollapseGroup,        new[] { GridKeyboardScope.GroupRow } },
            { GridKeyboardAction.ExpandDetailRow,      new[] { GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.CollapseDetailRow,    new[] { GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.ExpandAllDetailRows,  new[] { GridKeyboardScope.DetailRow } },
            { GridKeyboardAction.CollapseAllDetailRows,new[] { GridKeyboardScope.DetailRow } },

            // Phases 11 / 12 / 16 / 17 (append-only)
            { GridKeyboardAction.Copy,                  new[] { GridKeyboardScope.Common } },
            { GridKeyboardAction.CopyWithHeaders,       new[] { GridKeyboardScope.Common } },
            { GridKeyboardAction.Paste,                 new[] { GridKeyboardScope.Common } },
            { GridKeyboardAction.Undo,                  new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.Redo,                  new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.SelectRowRange,        new[] { GridKeyboardScope.DataCell, GridKeyboardScope.GroupRow, GridKeyboardScope.DetailRow } },
            // ExtendSelection is mouse-driven only (Shift+Click / Ctrl+Click).
            // We still register an allowed scope for the rare case where a future
            // custom-binding path wants to consume the MouseAndKeyArgs payload,
            // but SFGK011 below rejects any *keyboard* chord bound to it.
            { GridKeyboardAction.ExtendSelection,       new[] { GridKeyboardScope.DataCell } },
            { GridKeyboardAction.EditEndAndTabNext,     new[] { GridKeyboardScope.EditCell } },
            { GridKeyboardAction.EditEndAndShiftTabPrev,new[] { GridKeyboardScope.EditCell } }
        };

        // SFGK011 — NonBindable actions. These actions have no default keyboard
        // binding and must not accept a custom keyboard override either. They are
        // driven by a non-keyboard input path (mouse / programmatic API).
        private static readonly HashSet<GridKeyboardAction> _nonBindable = new()
        {
            GridKeyboardAction.ExtendSelection
        };

        // SFGK009 — feature-off map (P0 reference; not used at runtime today).
        private static readonly HashSet<GridKeyboardAction> _pagingActions = new()
        {
            GridKeyboardAction.PageUp,
            GridKeyboardAction.PageDown,
            GridKeyboardAction.FirstPage,
            GridKeyboardAction.LastPage,
            GridKeyboardAction.NextPageGroup,
            GridKeyboardAction.PreviousPageGroup
        };

        /// <summary>
        /// Validates a compiled configuration and emits warnings. Never throws.
        /// </summary>
        public static void Validate(CompiledShortcuts compiled, IGrid? grid, Action<KeyboardWarning> warn)
        {
            // SFGK002 — zone/scope applicability
            foreach (var kv in compiled.ScopeBindings)
            {
                var scope = kv.Key;
                foreach (var actionKv in kv.Value)
                {
                    if (!_allowedScopes.TryGetValue(actionKv.Key, out var allowed))
                    {
                        continue;
                    }
                    bool ok = false;
                    foreach (var a in allowed)
                    {
                        if (a == scope) { ok = true; break; }
                    }
                    if (!ok)
                    {
                        warn(new KeyboardWarning(
                            "SFGK002",
                            $"[SfGrid][Keyboard] SFGK002 Action '{actionKv.Key}' is not applicable to scope '{scope}'.",
                            actionKv.Key, scope));
                    }
                }
            }

            // SFGK004 / SFGK005 / SFGK006 — reserved tier
            foreach (var kv in compiled.ScopeBindings)
            {
                foreach (var actionKv in kv.Value)
                {
                    foreach (var chord in actionKv.Value)
                    {
                        var tier = GridKeyboardReservedKeys.Classify(chord);
                        if (tier == ReservedTier.Forbidden)
                        {
                            warn(new KeyboardWarning(
                                "SFGK004",
                                $"[SfGrid][Keyboard] SFGK004 Binding '{chord.ToCanonicalString()}' ({actionKv.Key}) is FORBIDDEN — the browser will intercept it before the grid sees the key. The binding will NOT execute.",
                                actionKv.Key, kv.Key));
                        }
                        else if (tier == ReservedTier.Risky)
                        {
                            var alt = GridKeyboardReservedKeys.SuggestAlternative(chord.ToCanonicalString());
                            var msg = $"[SfGrid][Keyboard] SFGK005 Binding '{chord.ToCanonicalString()}' ({actionKv.Key}) is RISKY — the browser, OS, or assistive technology may claim it. JS preventDefault is best-effort.";
                            if (alt != null)
                            {
                                msg += $" Suggested alternative: {alt}.";
                            }
                            warn(new KeyboardWarning("SFGK005", msg, actionKv.Key, kv.Key));
                        }
                    }
                }
            }

            // SFGK003 — duplicate chord (first wins)
            var seen = new HashSet<string>(StringComparer.Ordinal);
            foreach (var kv in compiled.ScopeBindings)
            {
                foreach (var actionKv in kv.Value)
                {
                    foreach (var chord in actionKv.Value)
                    {
                        var key = kv.Key + "|" + chord.ToCanonicalString();
                        if (!seen.Add(key))
                        {
                            warn(new KeyboardWarning(
                                "SFGK003",
                                $"[SfGrid][Keyboard] SFGK003 Duplicate binding for chord '{chord.ToCanonicalString()}' in scope '{kv.Key}'. The first wins.",
                                actionKv.Key, kv.Key));
                        }
                    }
                }
            }

            // SFGK007 — single printable key outside edit zones
            foreach (var kv in compiled.ScopeBindings)
            {
                if (kv.Key == GridKeyboardScope.EditCell) continue;
                foreach (var actionKv in kv.Value)
                {
                    foreach (var chord in actionKv.Value)
                    {
                        if (chord.Modifiers != KeyModifiers.None && chord.Code != null && IsPrintableSingleKey(chord.Code))
                        {
                            // SFGK007: WCAG 2.1.4 — single-character shortcuts should only fire
                            // when focus is in the relevant content (e.g. an edit input).
                            // We surface the warning but do not block.
                            warn(new KeyboardWarning(
                                "SFGK007",
                                $"[SfGrid][Keyboard] SFGK007 Single printable key binding '{chord.ToCanonicalString()}' outside an edit zone may interfere with screen readers (WCAG 2.1.4).",
                                actionKv.Key, kv.Key));
                        }
                    }
                }
            }

            // SFGK008 — Tab removed from all navigation in a scope
            CheckTabRemoved(compiled, GridKeyboardScope.DataCell, warn);
            CheckTabRemoved(compiled, GridKeyboardScope.Header, warn);

            // SFGK011 — NonBindable actions reject keyboard overrides.
            foreach (var kv in compiled.ScopeBindings)
            {
                foreach (var actionKv in kv.Value)
                {
                    if (!_nonBindable.Contains(actionKv.Key))
                    {
                        continue;
                    }
                    warn(new KeyboardWarning(
                        "SFGK011",
                        $"[SfGrid][Keyboard] SFGK011 Action '{actionKv.Key}' is non-bindable (mouse- or programmatic-driven). Keyboard overrides for this action are ignored. Use SelectRowRange for keyboard range selection.",
                        actionKv.Key, kv.Key));
                }
            }

            // SFGK009 — feature-off
            if (grid != null)
            {
                if (grid.AllowSorting == false && HasOverride(compiled, GridKeyboardAction.SortColumn))
                {
                    warn(new KeyboardWarning("SFGK009",
                        "[SfGrid][Keyboard] SFGK009 SortColumn is bound but AllowSorting is false. The binding will not fire.", GridKeyboardAction.SortColumn));
                }
                if (grid.AllowGrouping == false && HasOverride(compiled, GridKeyboardAction.GroupColumn))
                {
                    warn(new KeyboardWarning("SFGK009",
                        "[SfGrid][Keyboard] SFGK009 GroupColumn is bound but AllowGrouping is false. The binding will not fire.", GridKeyboardAction.GroupColumn));
                }
                if (grid.AllowReordering == false && (HasOverride(compiled, GridKeyboardAction.ReorderColumnLeft) || HasOverride(compiled, GridKeyboardAction.ReorderColumnRight)))
                {
                    warn(new KeyboardWarning("SFGK009",
                        "[SfGrid][Keyboard] SFGK009 ReorderColumn is bound but AllowReordering is false. The binding will not fire."));
                }
                if (grid.EditSettings?.AllowEditing == false && HasOverride(compiled, GridKeyboardAction.StartEdit))
                {
                    warn(new KeyboardWarning("SFGK009",
                        "[SfGrid][Keyboard] SFGK009 StartEdit is bound but AllowEditing is false. The binding will not fire.", GridKeyboardAction.StartEdit));
                }
                if (grid.AllowPaging == false)
                {
                    foreach (var action in _pagingActions)
                    {
                        if (HasOverride(compiled, action))
                        {
                            warn(new KeyboardWarning("SFGK009",
                                $"[SfGrid][Keyboard] SFGK009 {action} is bound but AllowPaging is false. The binding will not fire.", action));
                        }
                    }
                }
            }
        }

        private static void CheckTabRemoved(CompiledShortcuts compiled, GridKeyboardScope scope, Action<KeyboardWarning> warn)
        {
            bool hasTab = false;
            if (compiled.ScopeBindings.TryGetValue(scope, out var bindings))
            {
                foreach (var kv in bindings)
                {
                    if (kv.Key == GridKeyboardAction.NavigateLeft || kv.Key == GridKeyboardAction.NavigateRight)
                    {
                        foreach (var c in kv.Value)
                        {
                            if (string.Equals(c.Code, "Tab", StringComparison.Ordinal))
                            {
                                hasTab = true;
                                break;
                            }
                        }
                    }
                }
            }
            // We don't have direct access to original defaults here; if the user
            // bound NavigateLeft/Right to non-Tab chords in a scope, that scope no
            // longer has a Tab exit path. The validator surfaces SFGK008 only when
            // the user explicitly cleared all Tab navigation in the same scope.
            if (!hasTab && HasOverrideInScope(compiled, GridKeyboardAction.NavigateLeft, scope) && HasOverrideInScope(compiled, GridKeyboardAction.NavigateRight, scope))
            {
                warn(new KeyboardWarning(
                    "SFGK008",
                    $"[SfGrid][Keyboard] SFGK008 Both NavigateLeft and NavigateRight are overridden in scope '{scope}' and neither uses Tab/ShiftTab. The grid may become a keyboard trap (WCAG 2.1.2). Consider keeping a Tab path open."));
            }
        }

        private static bool HasOverride(CompiledShortcuts compiled, GridKeyboardAction action)
        {
            return compiled.OverrideActions.Contains(action);
        }

        private static bool HasOverrideInScope(CompiledShortcuts compiled, GridKeyboardAction action, GridKeyboardScope scope)
        {
            return compiled.ScopeBindings.TryGetValue(scope, out var bindings) && bindings.ContainsKey(action);
        }

        private static bool IsPrintableSingleKey(string code)
        {
            // Letter or digit physical codes only. F-keys, navigation, etc. are not "printable".
            if (code.StartsWith("Key", StringComparison.Ordinal) && code.Length == 4)
            {
                return true;
            }
            if (code.StartsWith("Digit", StringComparison.Ordinal) && code.Length == 6)
            {
                return true;
            }
            return false;
        }
    }
}
