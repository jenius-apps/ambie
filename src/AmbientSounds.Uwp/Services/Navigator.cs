using AmbientSounds.Views;
using System;
using System.Threading.Tasks;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Navigates programmatically in a UWP app.
    /// </summary>
    public class Navigator : INavigator
    {
        public object? RootFrame { get; set; }

        /// <inheritdoc/>
        public object? Frame { get; set; }

        /// <inheritdoc/>
        public void GoBack(string? sourcePage = null)
        {
            switch (sourcePage)
            {
                case nameof(ScreensaverPage):
                    // supress transition to avoid implicit animation bug on home page.
                    GoBackSafely(RootFrame, new DrillInNavigationTransitionInfo());
                    break;
                default:
                    GoBackSafely(Frame);
                    break;
            }
        }

        private void GoBackSafely(object? frame, NavigationTransitionInfo? transition = null)
        {
            if (frame is Frame f && f.CanGoBack)
            {
                f.GoBack(transition);
            }
        }

        /// <inheritdoc/>
        public void ToScreensaver()
        {
            if (RootFrame is Frame f && f.CurrentSourcePageType != typeof(ScreensaverPage))
            {
                f.Navigate(typeof(ScreensaverPage), null, new DrillInNavigationTransitionInfo());
            }
        }

        /// <inheritdoc/>
        public void ToCatalogue()
        {
            if (Frame is Frame f)
            {
                f.Navigate(typeof(CataloguePage), null, new SuppressNavigationTransitionInfo());
            }
        }

        /// <inheritdoc/>
        public void ToFocus()
        {
            if (Frame is Frame f)
            {
                f.Navigate(typeof(FocusPage), null, new SuppressNavigationTransitionInfo());
            }
        }

        /// <inheritdoc/>
        public void ToHome()
        {
            if (Frame is Frame f)
            {
                f.Navigate(typeof(HomePage), null, new SuppressNavigationTransitionInfo());
            }
        }

        /// <inheritdoc/>
        public string GetContentPageName()
        {
            if (Frame is Frame f)
            {
                return f.CurrentSourcePageType?.Name ?? string.Empty;
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public async Task ToCompactOverlayAsync(CompactPageType page)
        {
            Type? pageType = page switch
            {
                CompactPageType.FocusTimer => typeof(CompactFocusTimerPage),
                _ => null
            };

            if (pageType is null)
            {
                return;
            }

            if (RootFrame is Frame f)
            {
                // Ref: https://programmer.group/uwp-use-compact-overlay-mode-to-always-display-on-the-front-end.html
                var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                preferences.CustomSize = new Windows.Foundation.Size(360, 500);
                bool success = await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(
                    ApplicationViewMode.CompactOverlay,
                    preferences);

                if (success)
                {
                    f.Navigate(pageType, null, new SuppressNavigationTransitionInfo());
                }
            }
        }
    }
}
