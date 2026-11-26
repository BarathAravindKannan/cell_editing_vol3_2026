using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Syncfusion.Blazor.Internal;

namespace Syncfusion.Blazor.Grids.Internal
{
    /// <summary>
    /// Code-behind class for GridFindInput component.
    /// Provides UI for text finding and highlighting in grid cells without filtering rows.
    /// </summary>
    public partial class GridFindInput<TValue> : ComponentBase, IDisposable
    {
        /// <summary>
        /// Reference to the parent SfGrid component
        /// </summary>
        [CascadingParameter]
        public SfGrid<TValue>? Grid { get; set; }

        #region Private Properties

        /// <summary>
        /// Total number of matches found in visible/filtered data
        /// </summary>
        private int TotalMatches { get; set; }

        /// <summary>
        /// Flag to track whether the component has been disposed
        /// </summary>
        private bool disposed { get; set; }


        /// <summary>
        /// Reference to the find input field for focus management
        /// Used to refocus the input after keyboard navigation (Enter/Shift+Enter)
        /// </summary>
        private ElementReference _findInputRef;

        /// <summary>
        /// CSS class applied to the find bar while its input has focus.
        /// </summary>
        private string InputFocus { get; set; } = string.Empty;

        #endregion

        #region Life Cycle Hooks
        /// <summary>
        /// Lifecycle hook - initialize component and get settings from parent grid
        /// </summary>
        protected override void OnInitialized()
        {
            Grid?.EventAggregator.Add("RefreshSearchHighlight", RefreshFindAndHighlightInput);
            Grid?.EventAggregator.Add("ClearSearchHighlight", ClearFindOnDataChange);
            if (Grid != null && Grid.FindHightLightModule != null)
            {
                TotalMatches = Grid.FindHightLightModule.TotalMatches;
            }
        }

        /// <summary>
        /// Lifecycle hook - sync component state when parameters change
        /// </summary>
        protected override void OnParametersSet()
        {
            if (Grid != null && Grid.FindHightLightModule != null)
            {
                TotalMatches = Grid.FindHightLightModule.TotalMatches;
            }
        }

        private void RefreshFindAndHighlightInput(object args)
        {
            StateHasChanged();
        }

        /// <summary>
        /// Re-apply find highlighting when data changes (sort, filter, page, group)
        /// If search key matches in new data, highlighting continues
        /// If no matches found, display "No records" message but keep the search key
        /// </summary>
        private async void ClearFindOnDataChange(object args)
        {
            if (!string.IsNullOrEmpty(Grid?.FindHightLightModule?.SearchHighlightKey) &&
                Grid.IsFindToolbarEnabled() && Grid.FindHightLightModule != null)
            {
                await Grid.FindHightLightModule.FindHighlightAsync(Grid.FindHightLightModule.SearchHighlightKey).ConfigureAwait(true);
                TotalMatches = Grid.FindHightLightModule.TotalMatches;
                if (TotalMatches == 0)
                {
                    Grid.FindHightLightModule.CurrentMatchIndex = 0;
                    Grid.FindHightLightModule.HasNavigated = false;
                }                    
                StateHasChanged();
            }
        }
        #endregion

        #region Focus Handling

        private void OnFocus()
        {
            InputFocus = "e-input-focus";
        }

        private void OnBlur()
        {
            InputFocus = string.Empty;
        }

        #endregion

        #region Keyboard Event Handling & Navigation Handling

        /// <summary>
        /// Handle keyboard events (Enter for next, Shift+Enter for previous)
        /// Refocuses input after keyboard navigation to prevent focus from shifting to grid body
        /// </summary>
        private async Task OnKeyDown(KeyboardEventArgs e)
        {
            if (e.Key == "Enter")
            {
                if (e.ShiftKey)
                {
                    await GoToPrevious().ConfigureAwait(true);
                }
                else
                {
                    await GoToNext().ConfigureAwait(true);
                }
                // Prevents focus from shifting to grid body/row after scroll operations
                // Only for keyboard actions (Enter/Shift+Enter), not for button clicks
                try
                {
                    if (_findInputRef.Id != null)
                    {
                        await _findInputRef.FocusAsync().ConfigureAwait(true);
                    }
                }
                catch (TaskCanceledException)
                {
                    
                }
            }
            else if (e.Key == "Escape")
            {
                await OnClearFind().ConfigureAwait(true);
            }
        }

        /// <summary>
        /// Navigate to next match. Delegates to <c>FindHighlight.GoToNextAsync</c> so the
        /// toolbar button, the <c>Enter</c> keyboard shortcut, and the public
        /// <c>SfGrid.FindNextHighlightAsync()</c> API all share one implementation.
        /// </summary>
        private async Task GoToNext()
        {
            if (TotalMatches == 0 || Grid?.FindHightLightModule == null) return;
            await Grid.FindHightLightModule.GoToNextAsync().ConfigureAwait(true);
            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
        }

        /// <summary>
        /// Navigate to previous match. Delegates to <c>FindHighlight.GoToPreviousAsync</c> so the
        /// toolbar button, the <c>Shift+Enter</c> keyboard shortcut, and the public
        /// <c>SfGrid.FindPreviousHighlightAsync()</c> API all share one implementation.
        /// </summary>
        private async Task GoToPrevious()
        {
            if (TotalMatches == 0 || Grid?.FindHightLightModule == null) return;

            await Grid.FindHightLightModule.GoToPreviousAsync().ConfigureAwait(true);
            await InvokeAsync(StateHasChanged).ConfigureAwait(true);
        }
        #endregion

        #region Disposal
        /// <summary>
        /// Properly disposes resources used by the GridFindInput component.
        /// Unsubscribes from event aggregator events to prevent memory leaks.
        /// </summary>

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose unmanaged resources in the RowParameters.Row component.
        /// </summary>
        /// <param name="disposing">Boolean value to dispose the object.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            if (disposing)
            {
                if (Grid?.EventAggregator != null)
                {
                    Grid.EventAggregator.Remove("RefreshSearchHighlight", RefreshFindAndHighlightInput);
                    Grid.EventAggregator.Remove("ClearSearchHighlight", ClearFindOnDataChange);
                }
            }
            disposed = true;
        }
       
        #endregion

        #region Helper methods
        /// <summary>
        /// Clear find highlighting and reset state
        /// </summary>
        private async Task OnClearFind()
        {
            if(Grid == null || Grid.FindHightLightModule == null)
            {
                return;
            }

            await Grid.FindHightLightModule.ClearFindProperties().ConfigureAwait(true);
            Grid.FindHightLightModule.HasNavigated = false;
            await Grid.ResetFindAsync().ConfigureAwait(true);
        }

        /// <summary>
        /// Get tooltip text for counter
        /// </summary>
        private string GetCounterTooltip()
        {
            if (!HasSearchHighlight || TotalMatches == 0)
            {
                return GetLocalizedText(GridLocaleKeys.NoResult, "No matches found");
            }

            var currentMatchIndex = Intl.GetNumericFormat<int>(Math.Max(Grid?.FindHightLightModule?.CurrentMatchIndex ?? 0, 0), "n0");
            var totalMatches = Intl.GetNumericFormat<int>(Math.Max(Grid?.FindHightLightModule?.TotalMatches ?? 0, 0), "n0");
            var matchText = GetLocalizedText("Grid_Match", "Match");
            var ofText = GetLocalizedText("Grid_Of", "of");
            return $"{matchText} {currentMatchIndex} {ofText} {totalMatches}";
        }

        private string GetFindLabel()
        {
            return GetLocalizedText(GridLocaleKeys.FindHighLightARIA, "Find");
        }

        private string GetClearLabel()
        {
            return GetLocalizedText(GridLocaleKeys.ClearButton, "Clear search");
        }

        private string GetNextLabel()
        {
            return GetLocalizedText(GridLocaleKeys.NextMatch, "Next match");
        }

        private string GetPreviousLabel()
        {
            return GetLocalizedText(GridLocaleKeys.PreviousMatch, "Previous match");
        }

        private string GetLocalizedText(string key, string fallback)
        {
            string? localizedText = Grid?.Localizer?.GetText(key);
            return string.IsNullOrEmpty(localizedText) ? fallback : localizedText;
        }

        private string GetFindInputId()
        {
            return $"{Grid?.ID}_find_input";
        }

        private string GetFindStatusId()
        {
            return $"{Grid?.ID}_find_status";
        }

        private string GetFindPlaceholderText()
        {
            if(Grid == null || (Grid.FindSettings == null))
            {
                return GetFindLabel();
            }
            var placeholderText = Grid.FindSettings.PlaceholderText;
            if (placeholderText != null && !string.Equals(placeholderText, GetFindLabel(), StringComparison.Ordinal))
            {
                return placeholderText; 
            }
            return GetFindLabel();
        }

        private bool HasSearchHighlight =>
            !string.IsNullOrEmpty(Grid?.FindHightLightModule?.SearchHighlightKey);

        #endregion
    }
}
