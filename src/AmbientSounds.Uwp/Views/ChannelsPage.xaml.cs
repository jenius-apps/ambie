using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Toolkit.Uwp.UI;
using System;
using System.ComponentModel;
using System.Threading;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.UI.Xaml;
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
        App.Services.GetRequiredService<ITelemetry>().TrackPageView(nameof(ChannelsPage));
        ViewModel.PropertyChanged += OnPropertyChanged;

        _cts ??= new();

        string? launchArgs = e.Parameter as string;

        try
        {
            await ViewModel.InitializeAsync(launchArgs, _cts.Token);
        }
        catch (OperationCanceledException)
        {

        }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e)
    {
        ViewModel.PropertyChanged -= OnPropertyChanged;
        _cts?.Cancel();
        _cts = null;
        ViewModel.Uninitialize();
    }

    private async void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViewModel.SelectedChannel) && ViewModel.SelectedChannel is { } channel)
        {
            PreviewVideoPlayer.Visibility = Visibility.Collapsed;
            PreviewButton.Visibility = Visibility.Collapsed;
            await ManualContentFadeIn.StartAsync();
            TryPlayPreviewVideo("https://ambiecataloguestorage.blob.core.windows.net/videopreviews/hokkaidoCoast.mp4");
        }
    }

    private void TryPlayPreviewVideo(string videoUrl)
    {
        PreviewVideoPlayer.MediaPlayer.Pause();
        if (videoUrl is { Length: > 0 } && Uri.TryCreate(videoUrl, UriKind.Absolute, out Uri uri))
        {
            PreviewVideoPlayer.Source = MediaSource.CreateFromUri(uri);
            PreviewVideoPlayer.MediaPlayer.MediaEnded -= OnMediaEnded;
            PreviewVideoPlayer.MediaPlayer.MediaEnded += OnMediaEnded;
            PreviewVideoPlayer.MediaPlayer.Play();
            PreviewVideoPlayer.Visibility = Visibility.Visible;
        }
    }

    private void OnMediaEnded(MediaPlayer sender, object args)
    {
        App.Services.GetRequiredService<IDispatcherQueue>().TryEnqueue(() =>
        {
            PreviewButton.Visibility = Visibility.Visible;
        });
    }

    private async void OnClosePaneClicked(object sender, RoutedEventArgs e)
    {
        PreviewVideoPlayer.MediaPlayer.Pause();
        PreviewVideoPlayer.MediaPlayer.MediaEnded -= OnMediaEnded;

        await PaneFadeOut.StartAsync();
        ViewModel.CloseDetailsCommand.Execute(null);
        PreviewVideoPlayer.Visibility = Visibility.Collapsed;
        PreviewButton.Visibility = Visibility.Collapsed;
    }

    private void OnPreviewButtonClicked(object sender, RoutedEventArgs e)
    {
        if (PreviewVideoPlayer.MediaPlayer.PlaybackSession.PlaybackState is MediaPlaybackState.Paused)
        {
            PreviewButton.Visibility = Visibility.Collapsed;
            PreviewVideoPlayer.MediaPlayer.Play();
        }
    }
}
