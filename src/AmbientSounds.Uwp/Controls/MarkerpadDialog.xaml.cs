using AmbientSounds.Constants;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AmbientSounds.Controls;

public sealed partial class MarkerpadDialog : ContentDialog
{
    private readonly ITelemetry _telemetry;

    public MarkerpadDialog()
    {
        this.InitializeComponent();
        _telemetry = App.Services.GetRequiredService<ITelemetry>();
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        Hide();
    }

    private async void OnStoreLinkClicked(object sender, RoutedEventArgs e)
    {
        _telemetry.TrackEvent(TelemetryConstants.MarkerpadDownloadClicked, logLevel: LogLevel.Critical);
        await Launcher.LaunchUriAsync(new Uri("ms-windows-store://pdp?productId=9NH0WPDRK28T&cid=ambie"));
    }

    private void OnClosing(ContentDialog sender, ContentDialogClosingEventArgs args)
    {
        _telemetry.TrackEvent(TelemetryConstants.MarkerpadDialogClosed);
    }

    private void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        _telemetry.TrackEvent(TelemetryConstants.MarkerpadDialogOpened);
    }
}

public class StoreInstallParameters
{
    public int StoreAction { get; set; }

    public string ProductId { get; set; } = string.Empty;

    public bool AutoInstall { get; set; }

    public bool AutoOpenOnInstallComplete { get; set; }
}
