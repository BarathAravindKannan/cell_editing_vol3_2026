// <copyright file="KeyboardOwner.cs" company="Syncfusion">
// Custom Keyboard Shortcuts - Phase 0.5
// Owning entry path for an action (D2 / N3).
// </copyright>

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Owning entry path for a <see cref="GridKeyboardAction"/>. The owner is
    /// fixed per action; the two paths never both run for the same keystroke
    /// (D2 / N3 / N4).
    /// </summary>
    internal enum KeyboardOwner
    {
        /// <summary>
        /// Action is dispatched from the JS gate (<c>keyDownHandler</c>) via
        /// the <c>GridKeyDown</c> interop. The Blazor <c>@onkeydown</c> path
        /// does not own this action.
        /// </summary>
        Js,

        /// <summary>
        /// Action is dispatched from a Blazor <c>@onkeydown</c> handler. The
        /// JS gate must not forward it; the C# resolver sees it through
        /// <c>FocusHandler.ProcessKeyDown</c>.
        /// </summary>
        Blazor
    }
}
