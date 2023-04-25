using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    protected override void OnNavigatedTo(NavigationEventArgs e)
    {
        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
        {
            { "name", "settings" }
        });
    }
}
