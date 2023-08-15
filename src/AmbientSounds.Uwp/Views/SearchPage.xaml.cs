using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class SearchPage : Page
{
    private CancellationTokenSource? _cts;

    public SearchPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<SearchPageViewModel>();
    }

    public SearchPageViewModel ViewModel => (SearchPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
        if (e.Parameter is string searchQuery)
        {
            _cts?.Cancel();
            _cts = new();
            await ViewModel.SearchAsync(searchQuery, _cts.Token);
        }
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
