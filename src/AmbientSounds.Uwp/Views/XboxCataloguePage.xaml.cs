using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class XboxCataloguePage : Page
{
    private readonly SystemNavigationManager _systemNavigationManager;

    public XboxCataloguePage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<CataloguePageViewModel>();
        _systemNavigationManager = SystemNavigationManager.GetForCurrentView();
    }

    public CataloguePageViewModel ViewModel => (CataloguePageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        _systemNavigationManager.BackRequested += OnBackRequested;

        await ViewModel.InitializeAsync(null, default);
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
            root.GoBack();
        }
    }
}
