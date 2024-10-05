using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

#nullable enable

namespace AmbientSounds.Views;

public sealed partial class ChannelsPage : Page
{
    private CancellationTokenSource? _cts;

    public ChannelsPage()
    {
        this.InitializeComponent();
        this.DataContext = App.Services.GetRequiredService<ChannelsPageViewModel>();
    }

    public ChannelsPageViewModel ViewModel => (ChannelsPageViewModel)this.DataContext;

    protected override async void OnNavigatedTo(NavigationEventArgs e)
    {
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

    private void OnGridViewLoaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        if (sender is GridView gridView)
        {
            ScrollViewer? s = gridView.FindDescendant<ScrollViewer>();
            if (s is not null)
            {
                s.CanContentRenderOutsideBounds = true;
            }
        }
    }
}
