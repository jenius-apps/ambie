using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class PremiumDialog : ContentDialog
{
    public PremiumDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PremiumControlViewModel>();
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

    private void CloseClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        this.Hide();
    }

    private void OnImageFailed(object sender, Windows.UI.Xaml.ExceptionRoutedEventArgs e)
    {
        App.Services.GetRequiredService<ITelemetry>().TrackEvent("error:PremiumBannerImageFailed", new Dictionary<string, string>
        {
            { "message", e.ErrorMessage }
        });
    }
}
