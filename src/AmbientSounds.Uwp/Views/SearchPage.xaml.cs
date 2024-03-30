using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using System.Collections.Generic;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class SearchPage : Page
{
    public SearchPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<SearchPageViewModel>();
    }

    public SearchPageViewModel ViewModel => (SearchPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        ViewModel.Initialize();

        if (e.Parameter is string searchQuery)
        {
            await ViewModel.TriggerSearchAsync(searchQuery);
        }

        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(SearchPage));
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.Uninitialize();
    }

    private void OnListViewLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is ListView list)
        {
            ScrollViewer? s = list.FindDescendant<ScrollViewer>();
            if (s is not null)
            {
                s.CanContentRenderOutsideBounds = true;
            }
        }

    }
}
