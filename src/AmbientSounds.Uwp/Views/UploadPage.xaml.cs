using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views
{
    public sealed partial class UploadPage : Page
    {
        public UploadPage()
        {
            this.InitializeComponent();
            this.DataContext = App.Services.GetRequiredService<UploadPageViewModel>();
        }

        public UploadPageViewModel ViewModel => (UploadPageViewModel)this.DataContext;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
            {
                { "name", "uploadPage" },
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MainSplitView.IsPaneOpen = true;
        }

        private void RefreshClicked(object sender, RoutedEventArgs e)
        {
            MySoundsList.Refresh();
            var telemetry = App.Services.GetRequiredService<ITelemetry>();
            telemetry.TrackEvent(TelemetryConstants.UploadRefreshClicked);
        }
    }
}
