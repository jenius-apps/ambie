using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
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
        App.Services.GetRequiredService<ITelemetry>().TrackEvent(
            TelemetryConstants.PageNavTo,
            new Dictionary<string, string>
            {
                { "name", "home" }
            });
    }

    private void OnCatalogueClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        App.Services.GetRequiredService<INavigator>().NavigateTo(ContentPageType.Catalogue);
    }

    private async void OnDismissClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        await HideCatalogeButtonAnimation.StartAsync();
        CatalogueMessageGrid.Visibility = Visibility.Collapsed;
    }
}
