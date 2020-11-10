using AmbientSounds.Animations;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            if (App.IsTenFoot)
            {
                // Ref: https://docs.microsoft.com/en-us/windows/uwp/xbox-apps/turn-off-overscan
                ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);
            }
        }

        private void GridScaleUp(object sender, PointerRoutedEventArgs e) => SoundItemAnimations.ItemScaleUp(sender as UIElement, 1.1f);

        private void GridScaleNormal(object sender, PointerRoutedEventArgs e) => SoundItemAnimations.ItemScaleNormal(sender as UIElement);
    }
}
