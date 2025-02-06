using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.ComponentModel;
using System.Threading;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Animation;
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
        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(ChannelsPage));
        ViewModel.PropertyChanged += OnPropertyChanged;
        ViewModel.GridVideoPlayed += OnVideoPlayed;

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
        ViewModel.PropertyChanged -= OnPropertyChanged;
        ViewModel.GridVideoPlayed -= OnVideoPlayed;
        _cts?.Cancel();
        _cts = null;
        ViewModel.Uninitialize();
    }

    private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedChannel) && ViewModel.SelectedChannel is not null)
        {
            await ManualContentFadeIn.StartAsync();
        }
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

    private void OnVideoPlayed(object sender, ChannelViewModel vm)
    {
        var animation = ChannelsGrid.PrepareConnectedAnimation("channelVideoClicked", vm, "ImageRect");
        animation.Configuration = new BasicConnectedAnimationConfiguration();
    }

    private async void OnClosePaneClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
    {
        await PaneFadeOut.StartAsync();
        ViewModel.CloseDetailsCommand.Execute(null);
    }
}
