using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using JeniusApps.Common.Telemetry;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class CataloguePage : Page
{
    private CancellationTokenSource? _cts;

    public CataloguePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<CataloguePageViewModel>();
    }

    public CataloguePageViewModel ViewModel => (CataloguePageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        App.Services.GetRequiredService<IUserSettings>().Set(
            UserSettingsConstants.ShowHomePageDownloadMessageKey,
            false);

        App.Services.GetRequiredService<ITelemetry>().TrackEvent(
            TelemetryConstants.PageNavTo,
            new Dictionary<string, string>
            {
                { "name", "catalogue" }
            });

        string? navArgs = e.Parameter is string s ? s : null;

        try
        {
            _cts = new CancellationTokenSource();
            await ViewModel.InitializeAsync(navArgs, _cts.Token);
        }
        catch (OperationCanceledException)
        {
            _cts?.Dispose();
            _cts = null;
            return;
        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _cts?.Cancel();
        ViewModel.Uninitialize();
    }
}
