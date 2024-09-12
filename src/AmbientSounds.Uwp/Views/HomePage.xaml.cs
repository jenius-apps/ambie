using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AmbientSounds.Views;

public sealed partial class HomePage : Page
{
    public HomePage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(HomePage));

        if (App.Services.GetRequiredService<IUserSettings>().Get<bool>(UserSettingsConstants.ShowHomePageDownloadMessageKey))
        {
            CatalogueMessageGrid.Visibility = Visibility.Visible;
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.DownloadMessageShown);
        }
    }

    private void OnCatalogueClicked(object sender, RoutedEventArgs e)
    {
        App.Services.GetRequiredService<INavigator>().NavigateTo(ContentPageType.Catalogue);
        App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.DownloadMessageClicked);
    }

    private async void OnDismissClicked(object sender, RoutedEventArgs e)
    {
        await HideCatalogueButtonAnimation.StartAsync();
        CatalogueMessageGrid.Visibility = Visibility.Collapsed;

        App.Services.GetRequiredService<IUserSettings>().Set(
            UserSettingsConstants.ShowHomePageDownloadMessageKey,
            false);

        App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.DownloadMessageDismissed);
    }
}
