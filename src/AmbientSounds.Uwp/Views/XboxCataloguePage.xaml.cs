using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        var telemetry = App.Services.GetRequiredService<ITelemetry>();
        telemetry.TrackPageView(nameof(XboxCataloguePage));

        _systemNavigationManager.BackRequested += OnBackRequested;

        await ViewModel.InitializeAsync(null, default);

        var query = ViewModel.Rows.Select(x => new GroupInfoList(x.Sounds) { Key = x.Title });
        ContactsCVS.Source = new ObservableCollection<GroupInfoList>(query);
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

    private async void OnItemClicked(object sender, ItemClickEventArgs e)
    {
        if (e.ClickedItem is OnlineSoundViewModel vm)
        {
            await ViewModel.OpenSoundDialogCommand.ExecuteAsync(vm);
        }
    }
}

public class GroupInfoList(IEnumerable<object> items) : List<object>(items)
{
    public object? Key { get; set; }
}
