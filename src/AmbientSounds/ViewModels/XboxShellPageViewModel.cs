using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using System;
using System.Collections.Generic;
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

    public XboxShellPageViewModel(
        IMixMediaPlayerService mixMediaPlayerService,
        IIapService iapService,
        ITelemetry telemetry,
        IDialogService dialogService,
        IXboxSlideshowService xboxSlideshowService)
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
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SlideshowVisible))]
    [NotifyPropertyChangedFor(nameof(VideoVisible))]
    private SlideshowMode _slideshowMode = SlideshowMode.Images;

    [ObservableProperty]
    private bool _premiumButtonVisible;

    public bool SlideshowVisible => SlideshowMode is SlideshowMode.Images;

    public bool VideoVisible => SlideshowMode is SlideshowMode.Video;

    public async Task InitializeAsync()
    {
        _mixMediaPlayerService.SoundAdded += OnSoundAdded;
        await UpdatePremiumButtonAsync();
    }

    public void Uninitialize()
    {
        _mixMediaPlayerService.SoundAdded -= OnSoundAdded;
    }

    private async void OnSoundAdded(object sender, SoundPlayedArgs e)
    {
        SlideshowMode = await _xboxSlideshowService.GetSlideshowModeAsync(e.Sound);
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
