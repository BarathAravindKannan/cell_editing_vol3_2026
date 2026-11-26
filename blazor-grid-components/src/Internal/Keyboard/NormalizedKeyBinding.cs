// <copyright file="NormalizedKeyBinding.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 0
// Zero-allocation key binding for the resolver hot path.
// </copyright>

using System;
using System.Globalization;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Normalized key binding for the resolver hot path.
    /// Holds physical <see cref="Code"/> and a <see cref="KeyModifiers"/> bit-flag.
    /// Designed for <c>IEquatable</c> comparisons without heap allocation.
    /// </summary>
    internal readonly struct NormalizedKeyBinding : IEquatable<NormalizedKeyBinding>
    {
        /// <summary>
        /// Bit-flag modifier set.
        /// </summary>
        public KeyModifiers Modifiers { get; }

        /// <summary>
        /// Canonical physical code (e.g. <c>KeyA</c>, <c>ArrowUp</c>, <c>F2</c>, <c>Digit1</c>).
        /// May be <c>null</c> when the binding has not been normalized yet.
        /// </summary>
        public string? Code { get; }

        /// <summary>
        /// Canonical logical key (e.g. <c>Enter</c>, <c>ArrowUp</c>). Used for fallback display.
        /// </summary>
        public string? Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NormalizedKeyBinding"/> struct.
        /// </summary>
        /// <param name="modifiers">Bit-flag modifier set.</param>
        /// <param name="code">Physical key code.</param>
        /// <param name="key">Logical key name.</param>
        public NormalizedKeyBinding(KeyModifiers modifiers, string? code, string? key)
        {
            Modifiers = modifiers;
            Code = code;
            Key = key;
        }

        /// <inheritdoc />
        public bool Equals(NormalizedKeyBinding other)
        {
            return Modifiers == other.Modifiers
                && string.Equals(Code, other.Code, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override bool Equals(object? obj) => obj is NormalizedKeyBinding other && Equals(other);

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                int hash = Modifiers.GetHashCode();
                if (Code != null)
                {
                    hash = (hash * 397) ^ StringComparer.Ordinal.GetHashCode(Code);
                }
                return hash;
            }
        }

        /// <summary>
        /// Returns the canonical wire form: modifiers sorted alphabetically followed by code.
        /// Example: <c>Alt+Ctrl+KeyG</c>.
        /// </summary>
        /// <returns>Canonical string.</returns>
        public string ToCanonicalString()
        {
            var sb = new System.Text.StringBuilder(16);
            AppendModifier(sb, Modifiers, KeyModifiers.Alt, "Alt");
            AppendModifier(sb, Modifiers, KeyModifiers.Ctrl, "Ctrl");
            AppendModifier(sb, Modifiers, KeyModifiers.Meta, "Meta");
            AppendModifier(sb, Modifiers, KeyModifiers.Shift, "Shift");
            AppendModifier(sb, Modifiers, KeyModifiers.MetaOrCtrl, "MetaOrCtrl");
            if (sb.Length > 0)
            {
                sb.Append('+');
            }
            sb.Append(Code ?? string.Empty);
            return sb.ToString();
        }

        private static void AppendModifier(System.Text.StringBuilder sb, KeyModifiers all, KeyModifiers flag, string name)
        {
            if ((all & flag) == flag)
            {
                if (sb.Length > 0)
                {
                    sb.Append('+');
                }
                sb.Append(name);
            }
        }

        /// <summary>
        /// True if the binding matches the given pressed code and modifier flags, taking Mac Ctrl/Cmd equivalence into account.
        /// </summary>
        public bool Matches(string pressedCode, KeyModifiers pressedModifiers)
        {
            if (!string.Equals(Code, pressedCode, StringComparison.Ordinal))
            {
                return false;
            }

            // Required modifier bits in the binding must be present in the pressed modifiers.
            var required = Modifiers & ~KeyModifiers.MetaOrCtrl;
            if ((pressedModifiers & required) != required)
            {
                return false;
            }

            // MetaOrCtrl means: at least one of Ctrl or Meta must be pressed (only on Mac).
            if ((Modifiers & KeyModifiers.MetaOrCtrl) == KeyModifiers.MetaOrCtrl)
            {
                bool ctrl = (pressedModifiers & KeyModifiers.Ctrl) == KeyModifiers.Ctrl;
                bool meta = (pressedModifiers & KeyModifiers.Meta) == KeyModifiers.Meta;
                if (!ctrl && !meta)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns the string representation used in error messages and tests.
        /// </summary>
        public override string ToString() => ToCanonicalString();
    }
}
