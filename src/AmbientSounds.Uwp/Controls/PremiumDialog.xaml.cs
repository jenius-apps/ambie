using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.Globalization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class PremiumDialog : ContentDialog
{
    public PremiumDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PremiumControlViewModel>();

        if (new GeographicRegion().CodeTwoLetter.Equals("us", System.StringComparison.OrdinalIgnoreCase))
        {
            TariffText.Visibility = Visibility.Visible;
        }
    }
    
    public PremiumControlViewModel ViewModel { get; }

    private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        try
        {
            await ViewModel.InitializeAsync();
        }
        catch { }
    }

    private void CloseClick(object sender, RoutedEventArgs e)
    {
        this.Hide();
    }

    private void OnImageFailed(object sender, ExceptionRoutedEventArgs e)
    {
        App.Services.GetRequiredService<ITelemetry>().TrackEvent("error:PremiumBannerImageFailed", new Dictionary<string, string>
        {
            { "message", e.ErrorMessage }
        });
    }
}
