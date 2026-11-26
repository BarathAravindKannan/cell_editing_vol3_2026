using Syncfusion.Blazor.Grids;
using System;

namespace Syncfusion.Blazor.Grids
{
    /// <summary>
    /// GridDomVirtualizationSettings component code-behind.
    /// Part of DOM Virtualization feature (OPSX-20261026-introduce-dom-virtualization-feature).
    /// </summary>
    public partial class GridDomVirtualizationSettings<TValue>
    {
        /// <summary>
        /// Validates that MaxPoolSize is not smaller than visible window + RowBuffer.
        /// Issues a development warning if ceiling is too small.
        /// </summary>
        /// <param name="visibleWindowHeight">Height of the visible grid viewport in pixels</param>
        /// <param name="rowHeight">Height of a single row in pixels</param>
        internal void ValidateMaxPoolSize(double visibleWindowHeight, double rowHeight)
        {
            if (MaxPoolSize == null)
                return;

            // Calculate minimum ceiling: visible rows + buffer
            double visibleRowCount = Math.Ceiling(visibleWindowHeight / rowHeight);
            int minimumCeiling = (int)visibleRowCount + (RowBuffer ?? 0);

            if (MaxPoolSize < minimumCeiling)
            {
#if DEBUG
                System.Diagnostics.Debug.WriteLine(
                    $"[DOM Virtualization] MaxPoolSize ({MaxPoolSize}) is smaller than " +
                    $"visible window ({visibleRowCount}) + RowBuffer ({RowBuffer ?? 0}). " +
                    $"Recommended minimum: {minimumCeiling}. " +
                    $"Blank regions may appear during fast scrolling.");
#endif
                // Clamp to minimum without breaking current contracts
                MaxPoolSize = Math.Max(MaxPoolSize.Value, minimumCeiling);
            }
        }

        /// <summary>
        /// Gets effective render buffer (DOM virtualization layer).
        /// This is DISTINCT from OverscanCount (virtual scrolling layer).
        /// </summary>
        internal int GetEffectiveRowBuffer() => RowBuffer ?? 0;

        /// <summary>
        /// Gets effective max pool size (element ceiling).
        /// </summary>
        internal int GetEffectiveMaxPoolSize() => MaxPoolSize ?? 500;
    }
}
