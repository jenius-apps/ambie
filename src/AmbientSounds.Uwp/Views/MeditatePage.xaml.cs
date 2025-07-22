using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class MeditatePage : Page
{
    private CancellationTokenSource? _cts;

    public MeditatePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<MeditatePageViewModel>();
    }

    public MeditatePageViewModel ViewModel => (MeditatePageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(MeditatePage));

        _cts ??= new();

        try
        {
            await ViewModel.InitializeAsync(_cts.Token);
        }
        catch (OperationCanceledException)
        {

        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _cts?.Cancel();
        _cts = null;
        ViewModel.Uninitialize();
    }
}
