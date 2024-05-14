using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class XboxShellPageViewModel : ObservableObject
{
    private readonly IIapService _iapService;
    private readonly ITelemetry _telemetry;
    private readonly IDialogService _dialogService;
    private readonly IXboxSlideshowService _xboxSlideshowService;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly IVideoService _videoService;
    private VideoDownloadTriggeredArgs? _activeVideoDownloadInfo;

    public XboxShellPageViewModel(
        IMixMediaPlayerService mixMediaPlayerService,
        IIapService iapService,
        ITelemetry telemetry,
        IDialogService dialogService,
        IXboxSlideshowService xboxSlideshowService,
        IDispatcherQueue dispatcherQueue,
        IVideoService videoService)
    {
        // For xbox, there's no such thing as a custom global volume.
        // We let the user adjust their TV volume for that.
        // So ensure that we always set this to 1 on Xbox.
        mixMediaPlayerService.GlobalVolume = 1;
        _mixMediaPlayerService = mixMediaPlayerService;
        _iapService = iapService;
        _telemetry = telemetry;
        _dialogService = dialogService;
        _xboxSlideshowService = xboxSlideshowService;
        _dispatcherQueue = dispatcherQueue;
        _videoService = videoService;
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SlideshowVisible))]
    [NotifyPropertyChangedFor(nameof(VideoVisible))]
    private SlideshowMode _slideshowMode = SlideshowMode.Images;

    [ObservableProperty]
    private bool _premiumButtonVisible;

    [ObservableProperty]
    private double _videoProgress;

    [ObservableProperty]
    private bool _downloadingMessageVisible;

    [ObservableProperty]
    private string _videoSource = string.Empty;

    public bool SlideshowVisible => SlideshowMode is SlideshowMode.Images;

    public bool VideoVisible => SlideshowMode is SlideshowMode.Video;

    public async Task InitializeAsync()
    {
        _mixMediaPlayerService.SoundAdded += OnSoundAdded;
        _xboxSlideshowService.VideoDownloadTriggered += OnVideoDownloadTriggered;
        _ = UpdatePremiumButtonAsync();
        await UpdateSlideshowModeAsync();
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.SoundAdded -= OnSoundAdded;
        _xboxSlideshowService.VideoDownloadTriggered -= OnVideoDownloadTriggered;

        if (_activeVideoDownloadInfo is { Progress: { } videoProgress })
        {
            videoProgress.ProgressChanged -= OnProgressChanged;
        }
    }

    private void OnSoundAdded(object sender, SoundPlayedArgs e)
    {
        _ = UpdateSlideshowModeAsync(e.Sound.Id, e.Sound.AssociatedVideoIds);
    }

    private async Task UpdateSlideshowModeAsync(string? soundId = null, IReadOnlyList<string>? associatedVideoIds = null)
    {
        if (soundId is null || associatedVideoIds is null)
        {
            (soundId, associatedVideoIds) = await _xboxSlideshowService.GetSlideshowDataAsync(_mixMediaPlayerService);
        }

        SlideshowMode = await _xboxSlideshowService.GetSlideshowModeAsync(soundId, associatedVideoIds);
        if (SlideshowMode is SlideshowMode.Video)
        {
            await LoadAssociatedVideoAsync(associatedVideoIds);
        }

        // Ensure that the UI updates.
        OnPropertyChanged(nameof(SlideshowMode));
    }

    private async Task LoadAssociatedVideoAsync(IReadOnlyList<string> associatedVideoIds)
    {
        if (associatedVideoIds is not [string videoId, ..])
        {
            return;
        }

        var video = await _videoService.GetLocalVideoAsync(videoId);
        if (video is null)
        {
            return;
        }

        if (video.FilePath is { Length: > 0 } path)
        {
            VideoSource = path;
        }
    }

    private void OnVideoDownloadTriggered(object sender, VideoDownloadTriggeredArgs e)
    {
        if (_activeVideoDownloadInfo is not null)
        {
            // Only support one active download at a time
            return;
        }

        _activeVideoDownloadInfo = e;

        Debug.WriteLine("################### download triggered");
        _activeVideoDownloadInfo.Progress.ProgressChanged += OnProgressChanged;

        _dispatcherQueue.TryEnqueue(() =>
        {
            DownloadingMessageVisible = true;
        });
    }

    private void OnProgressChanged(object sender, double progress)
    {
        Debug.WriteLine($"################### progress changed {progress}");

        _dispatcherQueue.TryEnqueue(async () =>
        {
            VideoProgress = progress;
            if (progress >= 100 && _activeVideoDownloadInfo is not null)
            {
                _activeVideoDownloadInfo.Progress.ProgressChanged -= OnProgressChanged;

                if (_mixMediaPlayerService.GetSoundIds(oldestToNewest: false).FirstOrDefault() == _activeVideoDownloadInfo.SoundId)
                {
                    await Task.Delay(300); // required for the video file to be usable
                    _ = UpdateSlideshowModeAsync(_activeVideoDownloadInfo.SoundId, [_activeVideoDownloadInfo.VideoId]);
                }

                _activeVideoDownloadInfo = null;
                DownloadingMessageVisible = false;
            }
        });
    }

    private async Task UpdatePremiumButtonAsync()
    {
        PremiumButtonVisible = await _iapService.CanShowPremiumButtonsAsync();
    }

    [RelayCommand]
    private async Task OpenPremiumAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.ShellPagePremiumClicked);
        await _dialogService.OpenPremiumAsync();
    }
}
