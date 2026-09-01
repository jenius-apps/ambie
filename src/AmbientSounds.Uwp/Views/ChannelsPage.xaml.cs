using AmbientSounds.Constants;
using AmbientSounds.ViewModels;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
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
        PreviewVideoPlayer.MediaPlayer?.Pause();

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
            await ManualContentFadeIn.StartAsync();

            var success = TryLoadVideoPreview(ViewModel.SelectedChannel?.Channel.VideoPreviewUrl);
            PreviewButton.Visibility = success ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private bool TryLoadVideoPreview(string? videoUrl)
    {
        if (!App.Services.GetRequiredService<IExperimentationService>().IsEnabled(ExperimentConstants.ChannelPreview))
        {
            return false;
        }

        if (videoUrl is { Length: > 0 } && Uri.TryCreate(videoUrl, UriKind.Absolute, out Uri uri))
        {
            PreviewVideoPlayer.Source = MediaSource.CreateFromUri(uri);
            PreviewVideoPlayer.MediaPlayer.MediaEnded -= OnMediaEnded;
            PreviewVideoPlayer.MediaPlayer.MediaEnded += OnMediaEnded;
            return true;
        }

        return false;
    }

    private void OnMediaEnded(MediaPlayer sender, object args)
    {
        App.Services.GetRequiredService<IDispatcherQueue>().TryEnqueue(() =>
        {
            PreviewButton.Visibility = Visibility.Visible;
            ViewModel.StopPreviewPlaybackCommand.Execute(null);
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
            App.Services.GetRequiredService<ITelemetry>().TrackEvent(TelemetryConstants.ChannelPreviewClicked, new Dictionary<string, string>
            {
                { "name", ViewModel.SelectedChannel?.Name ?? "" }
            },
            logLevel: LogLevel.Critical);
            PreviewButton.Visibility = Visibility.Collapsed;
            PreviewVideoPlayer.Visibility = Visibility.Visible;
            PreviewVideoPlayer.MediaPlayer.Play();
            ViewModel.PreviewSelectedChannelCommand.Execute(null);
        }
    }
}
