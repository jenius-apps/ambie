using AmbientSounds.Views;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Navigates programmatically in a UWP app.
    /// </summary>
    public class Navigator : INavigator
    {
        /// <inheritdoc/>
        public object Frame { get; set; }

        /// <inheritdoc/>
        public void ToScreensaver()
        {
            if (Frame is Frame f)
            {
                f.Navigate(typeof(ScreensaverPage), null, new DrillInNavigationTransitionInfo());
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
