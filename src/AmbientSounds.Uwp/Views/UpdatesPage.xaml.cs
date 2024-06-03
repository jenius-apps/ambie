using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class UpdatesPage : Page
{
    private readonly SystemNavigationManager _systemNavigationManager;

    public UpdatesPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<UpdatesViewModel>();
        _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
    }

    public UpdatesViewModel ViewModel => (UpdatesViewModel)this.DataContext;

    private bool IsTenFoot => App.IsTenFoot;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (IsTenFoot)
        {
            _systemNavigationManager.BackRequested += OnBackRequested;
        }

        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackPageView(nameof(UpdatesPage));

        await ViewModel.CheckUpdatesCommand.ExecuteAsync(null);
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        if (IsTenFoot)
        {
            _systemNavigationManager.BackRequested -= OnBackRequested;
        }

        ViewModel.Uninitialize();
    }

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        if (IsTenFoot && App.Services.GetRequiredService<INavigator>().RootFrame is Frame root)
        {
            e.Handled = true;
            root.GoBack();
        }
    }
}
