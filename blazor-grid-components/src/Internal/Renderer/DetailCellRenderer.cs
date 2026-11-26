using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Partial class containing detail cell rendering logic for GridRow component.
    /// Handles detail cell state management, rendering, and user interactions.
    /// </summary>
    public partial class GridRowBase<TRow>
    {
        #region Detail Cell Rendering

        /// <summary>
        /// Main RenderFragment for rendering a detail cell TD element
        /// </summary>
        /// <param name="cellParameters">Parameters containing cell and row information</param>
        /// <returns>RenderFragment for the detail cell</returns>
        internal RenderFragment RenderDetailCell(GridCellParameters cellParameters) => builder =>
        {
            var sequence = 0;
            var row = cellParameters.Row;
            var cell = cellParameters.Cell;

            if (row == null || cell == null) return;

            builder.OpenElement(sequence++, "td");
            string? expandClass = cell.DetailRowExpand ? "e-detailrowexpand" : "e-detailrowcollapse";
            string? cellClassName = BaseClassName(cell, row);
            string? baseClass = cellClassName.Replace(" e-freezeleftborder", string.Empty, StringComparison.Ordinal).Trim();
            string? classNames = $"{expandClass} {baseClass}";

            if (Parent != null && Parent.FreezeModule!.GetFreezeLeftCount() > 0)
            {
                classNames = string.Concat(classNames, " e-leftfreeze");
            }

            builder.AddAttribute(sequence++, "class", classNames);

            // The toggle cell is the element that owns focus (tabindex), the
            // click handler and the key handler, so it must also be the element
            // that carries the action semantics. Screen readers build their
            // action model from the accessibility tree; when role="button" sits
            // on the inner glyph instead, the focused row has no primary action
            // and Enter does not invoke the toggle.
            var ariaLabel = cell.DetailRowExpand ? "Row Expand" : "Row Collapse";

            // Add attributes
            builder.AddAttribute(sequence++, "role", "button");
            builder.AddAttribute(sequence++, "aria-label", ariaLabel);
            builder.AddAttribute(sequence++, "aria-expanded", cell.DetailRowExpand ? "true" : "false");
            builder.AddAttribute(sequence++, "tabindex", (0).ToString(CultureInfo.InvariantCulture));
            builder.AddAttribute(sequence++, "data-uid", cell.Uid);

            // Add style if frozen
            var styleText = Parent?.DetailRowModule != null ? Parent.DetailRowModule.GetDetailCellStyle(row, cell) : string.Empty;
            if (!string.IsNullOrEmpty(styleText))
            {
                builder.AddAttribute(sequence++, "data-sf-style", styleText);
            }

            // Add click handler
            builder.AddAttribute(sequence++, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, async (args) =>
            {
                await (Parent?.FocusModule?.ClearFocus()!).ConfigureAwait(true);
                await Parent.FocusModule.Focus(row.Uid!, cell.Uid).ConfigureAwait(true);
                if (Parent?.DetailRowModule != null)
                    await Parent.DetailRowModule.DetailCellClickHandlerInternal(args, row, cell).ConfigureAwait(true);
            }));

            // Add keydown handler
            builder.AddAttribute(sequence++, "onkeydown", EventCallback.Factory.Create<KeyboardEventArgs>(this,
                async (e) => await (Parent?.FocusModule?.ProcessKeyDown(e, row, cell)!).ConfigureAwait(true)));
            builder.AddEventPreventDefaultAttribute(sequence++, "onkeydown", true);

            // Render expand/collapse icon. The glyph is decoration: the toggle
            // cell already carries the name and the expanded state, so the glyph
            // is hidden from assistive technology instead of being announced as
            // a second, independent control.
            var iconClass = cell.DetailRowExpand
                ? "e-icons e-dtdiagonaldown e-icon-gdownarrow"
                : "e-icons e-dtdiagonalright e-icon-grightarrow";

            builder.OpenElement(sequence++, "a");
            builder.AddAttribute(sequence++, "aria-hidden", "true");
            builder.AddAttribute(sequence++, "class", iconClass);
            builder.CloseElement(); // </a>

            builder.CloseElement(); // </td>
        };
        #endregion
    }
}