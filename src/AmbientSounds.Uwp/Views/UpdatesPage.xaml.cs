using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using JeniusApps.Common.Telemetry;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class UpdatesPage : Page
{
    public UpdatesPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<UpdatesViewModel>();
    }

    public UpdatesViewModel ViewModel => (UpdatesViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackEvent(TelemetryConstants.PageNavTo, new Dictionary<string, string>
        {
            { "name", "updates" }
        });

        await ViewModel.CheckUpdatesCommand.ExecuteAsync(null);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Uninitialize();
    }
}
