using AmbientSounds.Animations;
using System;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AmbientSounds.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CompactPage : Page
    {
        public CompactPage()
        {
            this.InitializeComponent();
        }

        private async void CloseCompactClicked()
        {
            var preferences = ViewModePreferences.CreateDefault(ApplicationViewMode.Default);
            await ApplicationView.GetForCurrentView().TryEnterViewModeAsync(ApplicationViewMode.Default, preferences);
            App.AppFrame.GoBack();
        }

        private void GridScaleUp(object sender, PointerRoutedEventArgs e) => SoundItemAnimations.ItemScaleUp(sender as UIElement, 1.05f);

        private void GridScaleNormal(object sender, PointerRoutedEventArgs e) => SoundItemAnimations.ItemScaleNormal(sender as UIElement);
    }
}
