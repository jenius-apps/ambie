using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls
{
    public sealed partial class VideosMenu : UserControl
    {
        public VideosMenu()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<VideosMenuViewModel>();
        }

        public VideosMenuViewModel ViewModel => (VideosMenuViewModel)this.DataContext;

        private async void OnLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            await ViewModel.InitializeAsync();
        }
    }
}
