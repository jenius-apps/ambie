using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using JeniusApps.Common.Telemetry;

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

        private void PremiumControl_CloseRequested(object sender, System.EventArgs e)
        {
            if (sender is FrameworkElement fe &&
                fe.Parent is FlyoutPresenter fp &&
                fp.Parent is Popup p)
            {
                p.IsOpen = false;
            }
        }

        private void Flyout_Opened(object sender, object e)
        {
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.VideoPremiumClicked);
        }
    }
}
