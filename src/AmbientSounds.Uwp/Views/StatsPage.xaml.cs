using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class StatsPage : Page
{
    private CancellationTokenSource? _cts;

    public StatsPage()
    {
        this.InitializeComponent();
        ViewModel = App.Services.GetRequiredService<StatsPageViewModel>();
    }

    public StatsPageViewModel ViewModel { get; }

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(StatsPage));
        _cts ??= new();

        try
        {
            await ViewModel.InitializeAsync(_cts.Token);
        }
        catch (OperationCanceledException) { }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _cts?.Cancel();
        _cts = null;
    }
}
