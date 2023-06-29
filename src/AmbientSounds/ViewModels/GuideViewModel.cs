using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Tools;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class GuideViewModel : ObservableObject
{
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IDispatcherQueue _dispatcherQueue;

    public GuideViewModel(
        Guide onlineGuide,
        IAsyncRelayCommand<GuideViewModel?> download,
        IAsyncRelayCommand<GuideViewModel?> delete,
        IAsyncRelayCommand<GuideViewModel?> play,
        IRelayCommand<GuideViewModel?> stop,
        IAsyncRelayCommand purchase,
        IAssetLocalizer assetLocalizer,
        IMixMediaPlayerService mixMediaPlayerService,
        IDispatcherQueue dispatcherQueue,
        Progress<double>? progress = null)
    {
        OnlineGuide = onlineGuide;
        DownloadCommand = download;
        DeleteCommand = delete;
        PlayCommand = play;
        StopCommand = stop;
        PurchaseCommand = purchase;
        Name = assetLocalizer.GetLocalName(onlineGuide);
        PreviewText = $"{onlineGuide.MinutesLength}m {FocusConstants.DotSeparator} {assetLocalizer.GetLocalDescription(onlineGuide)}";
        ImagePath = onlineGuide.ImagePath;
        ColourHex = onlineGuide.ColourHex;
        _mixMediaPlayerService = mixMediaPlayerService;
        _dispatcherQueue = dispatcherQueue;

        DownloadProgress = progress ?? new();
    }

    [ObservableProperty]
    private double _downloadProgressValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisible))]
    private bool _loading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisible))]
    private bool _downloadProgressVisible;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlaybackButtonsVisible))]
    private bool _isDownloaded;

    [ObservableProperty]
    private bool _isPlaying;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(BuyButtonVisible))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(PlaybackButtonsVisible))]
    private bool _isOwned;

    [ObservableProperty]
    private double _guidePlaybackProgress;

    public bool ProgressRingVisible => Loading || DownloadProgressVisible;

    public Guide OnlineGuide { get; }

    public Progress<double> DownloadProgress { get; }

    public string Name { get; }

    public string PreviewText { get; }

    public string ImagePath { get; }

    public string ColourHex { get; }

    public bool BuyButtonVisible => !IsOwned;

    public bool DownloadButtonVisible => IsOwned && !IsDownloaded;

    public bool PlaybackButtonsVisible => IsOwned && IsDownloaded;

    public IAsyncRelayCommand<GuideViewModel?> DownloadCommand { get; }

    public IAsyncRelayCommand<GuideViewModel?> DeleteCommand { get; }

    public IAsyncRelayCommand<GuideViewModel?> PlayCommand { get; }

    public IRelayCommand<GuideViewModel?> StopCommand { get; }

    public IAsyncRelayCommand PurchaseCommand { get; }

    public void Initialize()
    {
        _mixMediaPlayerService.GuidePositionChanged += OnGuidePositionChanged;
        DownloadProgress.ProgressChanged += OnProgressChanged;
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.GuidePositionChanged -= OnGuidePositionChanged;
        DownloadProgress.ProgressChanged -= OnProgressChanged;
    }

    private void OnGuidePositionChanged(object sender, TimeSpan e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            if (_mixMediaPlayerService.CurrentGuideId == OnlineGuide.Id)
            {
                GuidePlaybackProgress = e.TotalSeconds / _mixMediaPlayerService.GuideDuration.TotalSeconds * 100;
            }
            else
            {
                GuidePlaybackProgress = 0;
            }
        });
    }

    private void OnProgressChanged(object sender, double e)
    {
        if (e <= 0)
        {
            Loading = true;
            DownloadProgressVisible = false;
        }
        else if (e >= 1 && e < 100)
        {
            Loading = false;
            DownloadProgressVisible = true;
        }
        else if (e >= 100)
        {
            Loading = false;
            IsDownloaded = true;
            DownloadProgressVisible = false;
        }

        DownloadProgressValue = e;
    }

    [RelayCommand]
    private async Task ToggleAsync()
    {
        if (IsPlaying)
        {
            GuidePlaybackProgress = 0;
            StopCommand.Execute(this);
        }
        else
        {
            await PlayCommand.ExecuteAsync(this);
        }
    }
}
