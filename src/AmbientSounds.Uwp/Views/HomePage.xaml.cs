using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.System;
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

    private async void OnKeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
    {
        if (e.Key is VirtualKey.Enter or VirtualKey.Space)
        {
            if (e.OriginalSource is FrameworkElement fe && fe.DataContext is SoundViewModel vm)
            {
                await vm.PlayCommand.ExecuteAsync(null);
            }
        }
    }
}
