using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using AmbientSounds.Repositories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.ViewModels;

/// <summary>
/// View model for a sound object.
/// </summary>
public partial class SoundViewModel : ObservableObject
{
    private const string FallbackImageUrl = "http://localhost:8000";
    private readonly Sound _sound;
    private readonly IMixMediaPlayerService _playerService;
    private readonly ISoundService _soundService;
    private readonly IOnlineSoundRepository _onlineSoundRepo;
    private readonly ISoundMixService _soundMixService;
    private readonly ITelemetry _telemetry;
    private readonly IRenamer _renamer;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly IDownloadManager _downloadManager;
    private readonly IPresenceService _presenceService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly IAssetLocalizer _assetLocalizer;
    private Progress<double>? _downloadProgress;

    [ObservableProperty]
    private int _position;

    [ObservableProperty]
    private int _setSize;

    [ObservableProperty]
    private bool _downloadActive;

    [ObservableProperty]
    private double _downloadProgressValue;

    [ObservableProperty]
    private double _presenceCount = 0;

    [ObservableProperty]
    private bool _isKeyPadFocused;

    [ObservableProperty]
    private bool _videoIconVisible;

    public SoundViewModel(
        Sound s,
        IMixMediaPlayerService playerService,
        ISoundService soundService,
        ISoundMixService soundMixService,
        ITelemetry telemetry,
        IRenamer renamer,
        IDialogService dialogService,
        IIapService iapService,
        IDownloadManager downloadManager,
        IPresenceService presenceService,
        IDispatcherQueue dispatcherQueue,
        IOnlineSoundRepository onlineSoundRepo,
        IAssetLocalizer assetLocalizer)
    {
        _sound = s;
        _soundMixService = soundMixService;
        _playerService = playerService;
        _soundService = soundService;
        _telemetry = telemetry;
        _renamer = renamer;
        _dialogService = dialogService;
        _iapService = iapService;
        _downloadManager = downloadManager;
        _presenceService = presenceService;
        _dispatcherQueue = dispatcherQueue;
        _onlineSoundRepo = onlineSoundRepo;
        _assetLocalizer = assetLocalizer;
    }

    public IAsyncRelayCommand<IList<string>>? MixUnavailableCommand { get; set; }

    /// <summary>
    /// The sound's GUID.
    /// </summary>
    public string Id => _sound.Id;

    [ObservableProperty]
    private bool _lockIconVisible;

    /// <summary>
    /// Name of the sound.
    /// </summary>
    public string? Name => IsMix ? _sound.Name : _assetLocalizer.GetLocalName(_sound);

    /// <summary>
    /// True if the sound is a mix.
    /// </summary>
    public bool IsMix => _sound.IsMix;

    public bool IsNotMix => !IsMix;

    public string ColourHex => _sound.ColourHex;

    public bool HasSecondImage => IsMix && _sound.ImagePaths.Length == 2;

    public string? SecondImagePath => _sound.ImagePaths is [_, var path, ..] ? path : FallbackImageUrl;

    public bool HasThirdImage => IsMix && _sound.ImagePaths.Length == 3;

    public string? ThirdImagePath => _sound.ImagePaths is [_, _, var path, ..] ? path : FallbackImageUrl;

    /// <summary>
    /// The path for the image to display for the current sound.
    /// </summary>
    public string ImagePath
    {
        get
        {
            var result = _sound.IsMix ? _sound.ImagePaths.FirstOrDefault() : _sound.ImagePath;

            if (string.IsNullOrEmpty(result))
            {
                _telemetry.TrackEvent("invalidSoundImage", new Dictionary<string, string>
                {
                    { "soundId", _sound.Id },
                    { "source", nameof(SoundViewModel) },
                    { "isMix", _sound.IsMix.ToString() },
                    { "path", result },
                });
                return FallbackImageUrl;
            }
            else
            {
                return result;
            }
        }
    }

    /// <summary>
    /// Returns true if the sound is currently playing.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsPresenceVisible))]
    private bool _isCurrentlyPlaying;

    public bool IsPresenceVisible => PresenceCount > 0 && IsCurrentlyPlaying;

    public void Initialize()
    {
        _playerService.SoundRemoved += OnSoundPaused;
        _playerService.SoundAdded += OnSoundPlayed;
        _playerService.MixPlayed += OnMixPlayed;
        _presenceService.SoundPresenceChanged += OnPresenceChanged;
        _presenceService.PresenceDisconnected += OnPresenceDisconnected;
        _iapService.ProductPurchased += OnIapPurchased;

        DownloadActive = _downloadManager.IsDownloadActive(_sound);
        if (DownloadActive)
        {
            var progress = _downloadManager.GetProgress(_sound);
            if (progress is not null)
            {
                RegisterProgress(progress);
            }
        }

        VideoIconVisible = _sound.AssociatedVideoIds.Count > 0;

        UpdateIsCurrentlyPlaying();
        _ = UpdateIsLockVisibleAsync();
    }

    private async void OnIapPurchased(object sender, string e)
    {
        await UpdateIsLockVisibleAsync();
    }

    private async Task UpdateIsLockVisibleAsync()
    {
        bool isPremium = _sound.IapIds.Count > 0
            ? _sound.IsPremium && _sound.IapIds.ContainsAmbiePlus()
#pragma warning disable CS0618
            : _sound.IsPremium && _sound.IapId == IapConstants.MsStoreAmbiePlusId; // backwards compatibility
#pragma warning restore CS0618

        if (!isPremium)
        {
            return;
        }

        var owned = _sound.IapIds.Count > 0
            ? await _iapService.IsAnyOwnedAsync(_sound.IapIds)
#pragma warning disable CS0618
            : await _iapService.IsOwnedAsync(_sound.IapId); // backwards compatibility
#pragma warning restore CS0618

        LockIconVisible = !owned;
    }

    private void RegisterProgress(IProgress<double> progress)
    {
        if (progress is Progress<double> p)
        {
            if (_downloadProgress is not null)
            {
                _downloadProgress.ProgressChanged -= OnProgressChanged;
            }

            _downloadProgress = p;
            _downloadProgress.ProgressChanged += OnProgressChanged;
        }
    }

    private void OnProgressChanged(object sender, double e)
    {
        DownloadProgressValue = e;
        if (e >= 100)
        {
            DownloadActive = false;
            DownloadProgressValue = 0;
        }
    }

    /// <summary>
    /// Loads this sound into the player and plays it.
    /// </summary>
    [RelayCommand]
    private async Task PlayAsync()
    {
        if (DownloadActive)
        {
            return;
        }

        if (IsCurrentlyPlaying)
        {
            _playerService.RemoveSound(_sound.Id);
            return;
        }

        if (_sound.IsPremium)
        {
            var owned = _sound.IapIds.Count > 0
                ? await _iapService.IsAnyOwnedAsync(_sound.IapIds)
#pragma warning disable CS0618
                : await _iapService.IsOwnedAsync(_sound.IapId); // backwards compatibility
#pragma warning restore CS0618

            if (!owned)
            {
                await _dialogService.OpenPremiumAsync();
                return;
            }
        }

        if (!IsMix)
        {
            await _playerService.ToggleSoundAsync(_sound);
        }
        else
        {
            IEnumerable<string> unavailable = await _soundMixService.GetUnavailableSoundsAsync(_sound);
            if (unavailable.Any())
            {
                if (MixUnavailableCommand is not null)
                {
                    await MixUnavailableCommand.ExecuteAsync(unavailable.ToArray());
                }
                return;
            }

            await _soundMixService.LoadMixAsync(_sound);
        }

        _telemetry.TrackEvent(TelemetryConstants.SoundClicked, new Dictionary<string, string>
        {
            { "name", _sound.Name ?? "" },
            { "mix", IsMix.ToString() }
        });
    }

    [RelayCommand]
    private async Task RenameAsync()
    {
        bool renamed = await _renamer.RenameAsync(_sound);

        if (renamed)
        {
            OnPropertyChanged(nameof(Name));
        }
    }

    private void OnMixPlayed(object sender, MixPlayedArgs args)
    {
        if (args.MixId == _sound.Id || args.SoundIds.Contains(_sound.Id))
        {
            UpdateIsCurrentlyPlaying();
            OnPropertyChanged(nameof(IsPresenceVisible));
        }
    }

    private void OnPresenceChanged(object sender, PresenceEventArgs e)
    {
        if (e.SoundId == _sound.Id)
        {
            _dispatcherQueue.TryEnqueue(() =>
            {
                PresenceCount = e.Count;
                OnPropertyChanged(nameof(IsPresenceVisible));
            });
        }
    }

    private void OnPresenceDisconnected(object sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        { 
            PresenceCount = 0;
            OnPropertyChanged(nameof(IsPresenceVisible));
        });
    }

    private void OnSoundPaused(object sender, SoundPausedArgs args)
    {
        UpdateIsCurrentlyPlaying();
        OnPropertyChanged(nameof(IsPresenceVisible));
    }

    private void OnSoundPlayed(object sender, SoundPlayedArgs args)
    {
        if (args.ParentMixId == _sound.Id || args.Sound.Id == _sound.Id)
        {
            UpdateIsCurrentlyPlaying();
            OnPropertyChanged(nameof(IsPresenceVisible));
        }
    }

    [RelayCommand]
    private async Task DeleteSound()
    {
        if (!_sound.IsMix)
        {
            _playerService.RemoveSound(_sound.Id);
        }

        try
        {
            await _soundService.DeleteLocalSoundAsync(_sound.Id ?? "");

            _telemetry.TrackEvent(TelemetryConstants.DeleteClicked, new Dictionary<string, string>
            {
                { "name", _sound.Name ?? "" }
            });
        }
        catch { }
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        IReadOnlyList<string> ids = IsMix ? _sound.SoundIds.OrderBy(x => x).ToArray() : new string[]
        {
            Id
        };

        await _dialogService.OpenShareAsync(ids);
    }

    public void Dispose()
    {
        _playerService.SoundRemoved -= OnSoundPaused;
        _playerService.SoundAdded -= OnSoundPlayed;
        _playerService.MixPlayed -= OnMixPlayed;
        _presenceService.SoundPresenceChanged -= OnPresenceChanged;
        _presenceService.PresenceDisconnected -= OnPresenceDisconnected;

        if (_downloadProgress is not null)
        {
            _downloadProgress.ProgressChanged -= OnProgressChanged;
        }
    }

    private void UpdateIsCurrentlyPlaying()
    {
        IsCurrentlyPlaying = string.IsNullOrWhiteSpace(_playerService.CurrentMixId)
            ? _playerService.IsSoundPlaying(_sound.Id)
            : _soundMixService.IsMixPlaying(_sound.Id);
    }
}
