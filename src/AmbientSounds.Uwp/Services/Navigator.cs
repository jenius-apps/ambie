using AmbientSounds.Views;
using System;
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
                    GoBackSafely(RootFrame, new SuppressNavigationTransitionInfo());
                    break;
                default:
                    GoBackSafely(Frame);
                    break;
            }
        }

        private void GoBackSafely(object? frame, NavigationTransitionInfo transition = null)
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
        public async void ToCompact()
        {
            if (Frame is Frame f)
            {
                // Ref: https://programmer.group/uwp-use-compact-overlay-mode-to-always-display-on-the-front-end.html
                var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.CompactOverlay);
                preferences.CustomSize = new Windows.Foundation.Size(360, 500);
                await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.CompactOverlay, preferences);
                f.Navigate(typeof(CompactPage), null, new SuppressNavigationTransitionInfo());
            }
        }

        /// <inheritdoc/>
        public void ToUploadPage()
        {
            if (Frame is Frame f)
            {
                f.Navigate(typeof(UploadPage), null, new DrillInNavigationTransitionInfo());
            }
        }
    }
}
