// <copyright file="GridKeyCodeCatalog.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 0 (task 0.4)
// Recognized physical key codes for the resolver.
// </copyright>

using System.Collections.Generic;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Recognized physical key codes for the resolver. Returns the canonical
    /// key name for known <c>e.code</c> values, or <c>null</c> for unknown codes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The catalog is intentionally <em>not</em> an allow-list (it does not prevent binding).
    /// It exists so that <see cref="GridKeyResolver"/> can emit <c>SFGK001</c> when a
    /// developer binds an unrecognized code such as <c>MediaPlayPause</c>.
    /// </para>
    /// <para>
    /// <see cref="NormalizeCode"/> is the single bridge that keeps <em>parse-time</em>
    /// chord strings (developer-authored, free-form — e.g. <c>"Alt+P"</c>,
    /// <c>"Ctrl+5"</c>, <c>"Plus"</c>) in lockstep with <em>run-time</em>
    /// <c>KeyboardEvent.code</c> values (e.g. <c>KeyP</c>, <c>Digit5</c>,
    /// <c>Equal</c>). Both the parser and the dispatcher route through it so a
    /// binding the developer writes as <c>"Alt+P"</c> matches a real
    /// <c>KeyboardEvent</c> with <c>e.code = "KeyP"</c>. Without this
    /// canonicalization, every single-letter or single-digit chord silently
    /// resolves to <c>null</c> at runtime.
    /// </para>
    /// </remarks>
    internal static class GridKeyCodeCatalog
    {
        private static readonly HashSet<string> _known = new(System.StringComparer.Ordinal)
        {
            // Letters
            "KeyA","KeyB","KeyC","KeyD","KeyE","KeyF","KeyG","KeyH","KeyI","KeyJ",
            "KeyK","KeyL","KeyM","KeyN","KeyO","KeyP","KeyQ","KeyR","KeyS","KeyT",
            "KeyU","KeyV","KeyW","KeyX","KeyY","KeyZ",
            // Digits
            "Digit0","Digit1","Digit2","Digit3","Digit4","Digit5","Digit6","Digit7","Digit8","Digit9",
            // Function keys
            "F1","F2","F3","F4","F5","F6","F7","F8","F9","F10","F11","F12",
            // Navigation
            "ArrowUp","ArrowDown","ArrowLeft","ArrowRight","Home","End","PageUp","PageDown",
            // Editing
            "Enter","Tab","Escape","Insert","Delete","Backspace","Space",
            // Symbols (US layout, names match KeyboardEvent.code)
            "Minus","Equal","BracketLeft","BracketRight","Backslash","Semicolon","Quote",
            "Comma","Period","Slash","Backquote","IntlBackslash","Plus"
        };

        // Aliases accepted in developer-authored chord strings. These are NOT
        // produced by the browser — they're for ergonomics. Maps to the
        // canonical code that the browser will actually deliver.
        private static readonly Dictionary<string, string> _aliases =
            new(System.StringComparer.OrdinalIgnoreCase)
            {
                // Single letter / digit shortcuts: "P" -> "KeyP", "5" -> "Digit5".
                // The browser always reports the physical key as KeyX / DigitX
                // via KeyboardEvent.code, so this is what runtime matching needs.
                { "P",     "KeyP"    }, { "Q",     "KeyQ"    }, { "R",     "KeyR"    },
                { "S",     "KeyS"    }, { "T",     "KeyT"    }, { "U",     "KeyU"    },
                { "V",     "KeyV"    }, { "W",     "KeyW"    }, { "X",     "KeyX"    },
                { "Y",     "KeyY"    }, { "Z",     "KeyZ"    }, { "A",     "KeyA"    },
                { "B",     "KeyB"    }, { "C",     "KeyC"    }, { "D",     "KeyD"    },
                { "E",     "KeyE"    }, { "F",     "KeyF"    }, { "G",     "KeyG"    },
                { "H",     "KeyH"    }, { "I",     "KeyI"    }, { "J",     "KeyJ"    },
                { "K",     "KeyK"    }, { "L",     "KeyL"    }, { "M",     "KeyM"    },
                { "N",     "KeyN"    }, { "O",     "KeyO"    },
                { "0",     "Digit0"  }, { "1",     "Digit1"  }, { "2",     "Digit2"  },
                { "3",     "Digit3"  }, { "4",     "Digit4"  }, { "5",     "Digit5"  },
                { "6",     "Digit6"  }, { "7",     "Digit7"  }, { "8",     "Digit8"  },
                { "9",     "Digit9"  },
                // Ergonomic aliases — what the developer reads, mapped to the
                // canonical KeyboardEvent.code value the browser sends.
                { "Esc",          "Escape"        },
                { "Return",       "Enter"         },
                { "Up",           "ArrowUp"       },
                { "Down",         "ArrowDown"     },
                { "Left",         "ArrowLeft"     },
                { "Right",        "ArrowRight"    },
                { "PgUp",         "PageUp"        },
                { "PgDn",         "PageDown"      },
                { "PageUp",       "PageUp"        },
                { "PageDown",     "PageDown"      },
                { "Del",          "Delete"        },
                { "Backquote",    "Backquote"     },
                { "Backtick",     "Backquote"     },
                { "Tilde",        "Backquote"     },
                { "BSlash",       "Backslash"     },
                { "Backslash",    "Backslash"     },
                { "IntlBackslash","IntlBackslash" },
                { "Plus",         "Equal"         }, // US layouts: + sits on Shift+= (Equal)
                { "Minus",        "Minus"         },
                { "Equal",        "Equal"         },
                { "Equals",       "Equal"         },
                { "Space",        "Space"         },
                { "Spacebar",     "Space"         },
            };

        /// <summary>
        /// Returns the canonical key name for a given <c>e.code</c>, or <c>null</c> if unrecognized.
        /// </summary>
        public static string? GetKey(string? code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return null;
            }
            return _known.Contains(code) ? code : null;
        }

        /// <summary>
        /// True when the code is recognized by the catalog.
        /// </summary>
        public static bool IsKnown(string? code) => GetKey(code) != null;

        /// <summary>
        /// Canonicalizes a raw chord token to the <c>KeyboardEvent.code</c> form
        /// the browser will report at runtime. Both the parser (developer
        /// input like <c>"Alt+P"</c>, <c>"Ctrl+5"</c>, <c>"Plus"</c>) and the
        /// dispatcher (<c>e.code</c> from Blazor) route through this so they
        /// produce byte-identical <c>Code</c> values for the same physical key.
        /// </summary>
        /// <remarks>
        /// Examples:
        /// <list type="bullet">
        ///   <item><description><c>"P"</c>  → <c>"KeyP"</c> (so <c>"Alt+P"</c> matches the runtime <c>KeyP</c>)</description></item>
        ///   <item><description><c>"5"</c>  → <c>"Digit5"</c> (so <c>"Ctrl+5"</c> matches <c>Digit5</c>)</description></item>
        ///   <item><description><c>"Plus"</c>  → <c>"Equal"</c> (so <c>"Ctrl+Plus"</c> matches the runtime <c>Equal</c>)</description></item>
        ///   <item><description><c>"KeyP"</c> → <c>"KeyP"</c> (already canonical, returned as-is)</description></item>
        ///   <item><description><c>"MediaPlayPause"</c> → <c>"MediaPlayPause"</c> (returned as-is; <see cref="IsKnown"/> will reject it later as SFGK001)</description></item>
        /// </list>
        /// </remarks>
        public static string NormalizeCode(string raw)
        {
            if (string.IsNullOrEmpty(raw))
            {
                return raw;
            }

            // 1. Direct alias hit (single letter / digit / ergonomic name).
            if (_aliases.TryGetValue(raw, out var aliased))
            {
                return aliased;
            }

            // 2. Already canonical? (e.g. "KeyP", "Digit5", "F4", "ArrowUp").
            //    The browser always sends one of these values, so accepting
            //    them as-is means runtime values round-trip through this
            //    function unchanged and stay byte-equal with stored bindings.
            if (_known.Contains(raw))
            {
                return raw;
            }

            // 3. Last-resort: try to derive a Key? from a single letter / digit
            //    the developer typed without the "Key"/"Digit" prefix and
            //    without a case match in the alias table. This covers inputs
            //    that bypass the alias dictionary due to case sensitivity
            //    edge cases (the dictionary is case-insensitive, but be safe).
            if (raw.Length == 1)
            {
                char c = raw[0];
                if (c >= 'A' && c <= 'Z') return "Key" + c;
                if (c >= 'a' && c <= 'z') return "Key" + char.ToUpperInvariant(c);
                if (c >= '0' && c <= '9') return "Digit" + c;
            }

            // 4. Unknown. Returned unchanged so IsKnown can emit SFGK001.
            return raw;
        }
    }
}
