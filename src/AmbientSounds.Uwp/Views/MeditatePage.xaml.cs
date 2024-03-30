using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class MeditatePage : Page
{
    public MeditatePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<MeditatePageViewModel>();
    }

    public MeditatePageViewModel ViewModel => (MeditatePageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        await ViewModel.InitializeAsync();

        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(MeditatePage));
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Uninitialize();
    }
}
