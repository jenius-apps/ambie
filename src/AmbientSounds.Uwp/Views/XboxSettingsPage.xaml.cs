using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class XboxSettingsPage : Page
{
    private readonly SystemNavigationManager _systemNavigationManager;

    public XboxSettingsPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<SettingsViewModel>();
        _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
    }

    public SettingsViewModel ViewModel => (SettingsViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _systemNavigationManager.BackRequested += OnBackRequested;
        await ViewModel.InitializeAsync();
        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackPageView(nameof(XboxSettingsPage));
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        _systemNavigationManager.BackRequested -= OnBackRequested;
        ViewModel.Uninitialize();
    }

    private void OnBackRequested(object sender, BackRequestedEventArgs e)
    {
        if (App.Services.GetRequiredService<INavigator>().RootFrame is Frame root)
        {
            e.Handled = true;
            root.GoBack();
        }
    }
}
