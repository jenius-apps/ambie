using AmbientSounds.Views;
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
    }
}
