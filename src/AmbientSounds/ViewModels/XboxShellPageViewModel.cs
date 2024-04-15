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
    private Progress<double>? _currentVideoProgress;

    public XboxShellPageViewModel(
        IMixMediaPlayerService mixMediaPlayerService,
        IIapService iapService,
        ITelemetry telemetry,
        IDialogService dialogService,
        IXboxSlideshowService xboxSlideshowService,
        IDispatcherQueue dispatcherQueue)
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

    public bool SlideshowVisible => SlideshowMode is SlideshowMode.Images;

    public bool VideoVisible => SlideshowMode is SlideshowMode.Video;

    public async Task InitializeAsync()
    {
        _mixMediaPlayerService.SoundAdded += OnSoundAdded;
        _xboxSlideshowService.VideoDownloadTriggered += OnVideoDownloadTriggered;
        await UpdatePremiumButtonAsync();
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.SoundAdded -= OnSoundAdded;
        _xboxSlideshowService.VideoDownloadTriggered -= OnVideoDownloadTriggered;

        if (_currentVideoProgress is { } videoProgress)
        {
            videoProgress.ProgressChanged -= OnProgressChanged;
        }
    }

    private async void OnSoundAdded(object sender, SoundPlayedArgs e)
    {
        SlideshowMode = await _xboxSlideshowService.GetSlideshowModeAsync(e.Sound);
    }

    private void OnVideoDownloadTriggered(object sender, Progress<double> e)
    {
        Debug.WriteLine("################### download triggered");
        _currentVideoProgress = e;
        _currentVideoProgress.ProgressChanged += OnProgressChanged;

        _dispatcherQueue.TryEnqueue(() =>
        {
            DownloadingMessageVisible = true;
        });
    }

    private void OnProgressChanged(object sender, double e)
    {
        Debug.WriteLine($"################### progress changed {e}");

        _dispatcherQueue.TryEnqueue(() =>
        {
            VideoProgress = e;
            if (e >= 100 && _currentVideoProgress is { } videoProgress)
            {
                DownloadingMessageVisible = false;
                videoProgress.ProgressChanged -= OnProgressChanged;
                _currentVideoProgress = null;
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
