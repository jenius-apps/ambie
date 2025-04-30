using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

#nullable enable

namespace AmbientSounds.Controls;

public sealed partial class PremiumDialog : ContentDialog
{
    public PremiumDialog()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PremiumControlViewModel>();

        // Pausing for now while tariffs are paused. Will return if tariffs continue.
        //if (new GeographicRegion().CodeTwoLetter.Equals("us", System.StringComparison.OrdinalIgnoreCase))
        //{
        //    TariffText.Visibility = Visibility.Visible;
        //}
    }
    
    public PremiumControlViewModel ViewModel { get; }

    public bool LaunchPromoCodeDirectly { get; set; }

    private async void OnOpened(ContentDialog sender, ContentDialogOpenedEventArgs args)
    {
        try
        {
            await ViewModel.InitializeAsync(LaunchPromoCodeDirectly);
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

    private async void OnPromoCodeBoxKeyDown(object sender, KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter)
        {
            e.Handled = true;
            await ViewModel.SubmitCodeCommand.ExecuteAsync(null);
        }
    }
}
