// <copyright file="GridKeyboardReservedKeys.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 2 (task 2.4)
// Three-tier reserved-key classification (FORBIDDEN / RISKY / SAFE).
// </copyright>

using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Classification of a key chord by whether the browser or OS will claim it
    /// before JavaScript receives the event.
    /// </summary>
    internal enum ReservedTier
    {
        /// <summary>Recognized and safe to bind.</summary>
        Safe,

        /// <summary>May be claimed by browser, OS or assistive technology. Bindings warn; <c>preventDefault</c> can suppress in some browsers.</summary>
        Risky,

        /// <summary>Intercepted by the OS or browser before the page sees the key. Binding never fires.</summary>
        Forbidden
    }

    /// <summary>
    /// Reserved-key classification tables. FORBIDDEN keys cannot fire; RISKY keys
    /// warn the developer and can sometimes be suppressed via
    /// <c>e.preventDefault()</c> in JS.
    /// </summary>
    internal static class GridKeyboardReservedKeys
    {
        private static readonly HashSet<string> _forbiddenCodes = new(StringComparer.Ordinal)
        {
            // F-keys intercepted by browser
            "F5",
            // F11 / F12 are commonly used by dev tools; classify as forbidden for grids
            "F11", "F12"
        };

        private static readonly HashSet<string> _forbiddenChords = new(StringComparer.OrdinalIgnoreCase)
        {
            "Ctrl+R", "Ctrl+Shift+R", "Alt+F4", "Cmd+Q", "Meta+Q"
        };

        private static readonly HashSet<string> _riskyCodes = new(StringComparer.Ordinal)
        {
            "Insert", "CapsLock"
        };

        private static readonly HashSet<string> _riskyChords = new(StringComparer.OrdinalIgnoreCase)
        {
            "Ctrl+N", "Ctrl+T", "Ctrl+W", "Ctrl+S", "Ctrl+L", "Ctrl+F", "Ctrl+P",
            "Ctrl+Tab", "Ctrl+Alt+ArrowLeft", "Ctrl+Alt+ArrowRight",
            "Meta+Space", "Alt+Tab", "Cmd+Space"
        };

        /// <summary>
        /// Classifies a chord into one of the three tiers.
        /// </summary>
        public static ReservedTier Classify(NormalizedKeyBinding binding)
        {
            if (binding.Code == null)
            {
                return ReservedTier.Safe;
            }
            if (_forbiddenCodes.Contains(binding.Code))
            {
                return ReservedTier.Forbidden;
            }
            var chord = binding.ToCanonicalString();
            if (_forbiddenChords.Contains(chord))
            {
                return ReservedTier.Forbidden;
            }
            if (_riskyCodes.Contains(binding.Code))
            {
                return ReservedTier.Risky;
            }
            if (_riskyChords.Contains(chord))
            {
                return ReservedTier.Risky;
            }
            return ReservedTier.Safe;
        }

        /// <summary>
        /// Suggests an alternative binding for a reserved chord. Returns <c>null</c> when no
        /// obvious alternative exists.
        /// </summary>
        public static string? SuggestAlternative(string chord)
        {
            return chord switch
            {
                "Ctrl+R"        => "Ctrl+Alt+R",
                "Ctrl+Shift+R"  => "Alt+R",
                "Ctrl+P"        => "Ctrl+Shift+P",
                "Ctrl+N"        => "Ctrl+Shift+N",
                "Ctrl+T"        => "Ctrl+Shift+T",
                "Ctrl+W"        => "Ctrl+Shift+W",
                "Ctrl+S"        => "Ctrl+Shift+S",
                "Ctrl+L"        => "Ctrl+Shift+L",
                "Ctrl+F"        => "Ctrl+Shift+F",
                _ => null
            };
        }
    }
}
