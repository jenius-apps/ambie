using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class OnlineSoundViewModel : ObservableObject
{
    private readonly SemaphoreSlim _loadingLock = new(1, 1);
    private readonly Sound _sound;
    private readonly IDownloadManager _downloadManager;
    private readonly ISoundService _soundService;
    private readonly ITelemetry _telemetry;
    private readonly IIapService _iapService;
    private readonly IPreviewService _previewService;
    private readonly IDialogService _dialogService;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IUpdateService _updateService;
    private readonly ILocalizer _localizer;
    private Progress<double> _downloadProgress;
    private bool _loading;
    private bool _initialized;

    public OnlineSoundViewModel(
        Sound s, 
        IDownloadManager downloadManager,
        ISoundService soundService,
        ITelemetry telemetry,
        IPreviewService previewService,
        IIapService iapService,
        IDialogService dialogService,
        IAssetLocalizer assetLocalizer,
        IMixMediaPlayerService mixMediaPlayerService,
        IUpdateService updateService,
        ILocalizer localizer,
        IExperimentationService experimentationService)
    {
        _sound = s;
        _downloadManager = downloadManager;
        _previewService = previewService;
        _iapService = iapService;
        _soundService = soundService;
        _telemetry = telemetry;
        _dialogService = dialogService;
        _assetLocalizer = assetLocalizer;
        _mixMediaPlayerService = mixMediaPlayerService;
        _updateService = updateService;
        _localizer = localizer;

        _downloadProgress = new Progress<double>();
    }
    
    public event EventHandler? DownloadCompleted;

    public bool HasSlideshowImages => _sound.ScreensaverImagePaths is { Length: > 0 };

    public bool HasBackgroundVideo => _sound.AssociatedVideoIds is { Count: > 0 };

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    [NotifyPropertyChangedFor(nameof(UpdateAvailable))]
    [NotifyPropertyChangedFor(nameof(UpdateReasonText))]
    private UpdateReason _updateReason;

    public bool UpdateAvailable => UpdateReason != UpdateReason.None;

    public string DownloadProgressPercent => $"{DownloadProgressValue}%";

    public bool CanPlay => UpdateReason == UpdateReason.None 
        && IsInstalled 
        && !DownloadProgressVisible;

    public string UpdateReasonText => UpdateReason switch
    {
        UpdateReason.MetaData => _localizer.GetString("UpdateReasonMetaData"),
        UpdateReason.File => _localizer.GetString("UpdateReasonFile"),
        UpdateReason.MetaDataAndFile => _localizer.GetString("UpdateReasonMetaDataAndFile"),
        _ => string.Empty
    };

    /// <summary>
    /// This sound's download progress.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    [NotifyPropertyChangedFor(nameof(CanPreview))]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    [NotifyPropertyChangedFor(nameof(DownloadProgressPercent))]
    private double _downloadProgressValue;

    /// <summary>
    /// True if the item is already installed.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotInstalled))]
    [NotifyPropertyChangedFor(nameof(CanPlay))]
    private bool _isInstalled;

    /// <summary>
    /// If true, the sound is owned by the user
    /// and can be downloaded. If false, the sound
    /// must be purchased first.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanBuy))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    private bool _isOwned;

    private void OnProductPurchased(object sender, string iapId)
    {
        if (_sound.IsPremium && _sound.IapIds.Contains(iapId))
        {
            IsOwned = true;
        }
    }

    private async void OnSoundDeleted(object sender, string id)
    {
        if (id == _sound.Id)
        {
            IsInstalled = await _soundService.IsSoundInstalledAsync(_sound.Id);
            DownloadProgressValue = 0;

            // Note: a non-premium sound is treated as "owned"
            IsOwned = _sound.IsPremium ? await _iapService.IsAnyOwnedAsync(_sound.IapIds) : true;
        }
    }

    private async void OnProgressChanged(object sender, double e)
    {
        DownloadProgressValue = e;
        if (e >= 100)
        {
            IsInstalled = await _soundService.IsSoundInstalledAsync(_sound.Id ?? "");
            DownloadProgressValue = 0;
            if (IsInstalled)
            {
                DownloadCompleted?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    /// <summary>
    /// Gets or sets the location within the application of this viewmodel.
    /// This is used for telemetry purposes.
    /// </summary>
    public string TelemetryLocation { get; set; } = "catalogue";

    /// <summary>
    /// Name of the sound.
    /// </summary>
    public string Name => _assetLocalizer.GetLocalName(_sound);

    /// <summary>
    /// Localized description of the sound.
    /// </summary>
    public string Description => _assetLocalizer.GetLocalDescription(_sound);

    /// <summary>
    /// Returns true if the description is populated.
    /// </summary>
    public bool HasDescription => !string.IsNullOrEmpty(Description);

    public string ColourHex => _sound.ColourHex;

    /// <summary>
    /// Id of the sound.
    /// </summary>
    public string Id => _sound.Id;

    /// <summary>
    /// The path for the image to display for the current sound.
    /// </summary>
    public string ImagePath => _sound.ImagePath;

    /// <summary>
    /// Array of sponsor links provided by sound author
    /// to be displayed on screen if the links are valid.
    /// </summary>
    public string[] ValidSponsorLinks => AreLinksValid
        ? _sound.SponsorLinks.Where(static x => Uri.IsWellFormedUriString(x, UriKind.Absolute)).ToArray()
        : Array.Empty<string>();

    /// <summary>
    /// Determines if it's safe to display the links.
    /// </summary>
    public bool AreLinksValid => _sound.SponsorLinks is not null && 
        _sound.SponsorLinks.Length > 0 && 
        _sound.SponsorLinks.Any(static x => Uri.IsWellFormedUriString(x, UriKind.Absolute));

    /// <summary>
    /// Determines if the sound can be previewed.
    /// </summary>
    public bool CanPreview =>
        !DownloadProgressVisible &&
        !string.IsNullOrEmpty(_sound.PreviewFilePath) && 
        Uri.IsWellFormedUriString(_sound.PreviewFilePath, UriKind.Absolute);

    /// <summary>
    /// Determines if the download button is visible.
    /// </summary>
    public bool DownloadButtonVisible => IsOwned && !DownloadProgressVisible;

    /// <summary>
    /// True if the sound can be bought.
    /// </summary>
    public bool CanBuy => _sound.IsPremium && !IsOwned;

    /// <summary>
    /// Determines if the plus badge is visible.
    /// </summary>
    public bool PlusBadgeVisible => _sound.IsPremium && _sound.IapIds.ContainsAmbiePlus();

    /// <summary>
    /// True if download progress should be visible.
    /// </summary>
    public bool DownloadProgressVisible => DownloadProgressValue > 0 && DownloadProgressValue < 100;

    /// <summary>
    /// True if the item can be downloaded;
    /// </summary>
    public bool NotInstalled => !IsInstalled;

    [RelayCommand]
    private void Preview()
    {
        _previewService.Play(_sound.PreviewFilePath);
        _telemetry.TrackEvent(TelemetryConstants.PreviewPlayed, new Dictionary<string, string>
        {
            { "name", _sound.Name },
            { "isOwned", IsOwned.ToString() },
        });
    }

    [RelayCommand]
    private async Task PrimaryActionAsync()
    {
        if (DownloadProgressVisible)
        {
            return;
        }

        if (!IsOwned)
        {
            await BuySoundAsync();
            return;
        }

        if (!IsInstalled)
        {
            await DownloadAsync();
            return;
        }

        await PlayAsync();
    }

    [RelayCommand]
    private async Task BuySoundAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.BuyClicked, new Dictionary<string, string>
        {
            { "id", _sound.Id },
            { "name", _sound.Name }
        });

        await _dialogService.OpenPremiumAsync();
    }

    [RelayCommand]
    private async Task DeleteSoundAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.CatalogueDeleteClicked, new Dictionary<string, string>
        {
            { "id", _sound.Id ?? "" },
        });
        await _soundService.DeleteLocalSoundAsync(_sound.Id ?? "");
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (_loading)
        {
            return;
        }

        await _loadingLock.WaitAsync();
        if (_loading)
        {
            return;
        }

        _loading = true;

        if (!_initialized)
        {
            _initialized = true;
            _downloadProgress.ProgressChanged += OnProgressChanged;
            _soundService.LocalSoundDeleted += OnSoundDeleted;
            _iapService.ProductPurchased += OnProductPurchased;
        }

        if (_downloadManager.IsDownloadActive(_sound))
        {
            var progress = _downloadManager.GetProgress(_sound);
            if (progress is not null)
            {
                RegisterProgress(progress);
            }
        }
        else
        {
            IsInstalled = await _soundService.IsSoundInstalledAsync(_sound.Id ?? "");
        }

        // Determine ownership
        bool isOwned;
        if (_sound.IsPremium)
        {
            isOwned = await _iapService.IsAnyOwnedAsync(_sound.IapIds);
        }
        else
        {
            // a non premium sound is treated as "owned"
            isOwned = true;
        }

        IsOwned = isOwned;
        _loading = false;
        _loadingLock.Release();
    }

    [RelayCommand]
    private async Task PlayAsync()
    {
        if (!IsInstalled || _mixMediaPlayerService.IsSoundPlaying(Id))
        {
            return;
        }

        if (!IsOwned)
        {
            await _dialogService.OpenPremiumAsync();
            return;
        }

        var installedVersion = await _soundService.GetLocalSoundAsync(Id);
        if (installedVersion is not null)
        {
            await _mixMediaPlayerService.ToggleSoundAsync(installedVersion);
        }
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (_mixMediaPlayerService.IsSoundPlaying(Id))
        {
            _mixMediaPlayerService.RemoveSound(Id);
        }

        UpdateReason = UpdateReason.None;
        await _updateService.TriggerUpdateAsync(_sound, _downloadProgress);

        _telemetry.TrackEvent(TelemetryConstants.UpdateSoundClicked, new Dictionary<string, string>
        {
            { "id", Id },
            { "name", _sound.Name }
        });
    }

    [RelayCommand]
    private Task DownloadAsync()
    {
        if (DownloadProgressValue == 0 && NotInstalled)
        {
            _telemetry.TrackEvent(TelemetryConstants.DownloadClicked, new Dictionary<string, string>
            {
                { "name", _sound.Name }
            });

            return _downloadManager.QueueAndDownloadAsync(_sound, _downloadProgress);
        }

        return Task.CompletedTask;
    }

    private void RegisterProgress(IProgress<double> progress)
    {
        if (progress is Progress<double> p && p != _downloadProgress)
        {
            if (_downloadProgress is not null)
            {
                _downloadProgress.ProgressChanged -= OnProgressChanged;
            }

            _downloadProgress = p;
            _downloadProgress.ProgressChanged += OnProgressChanged;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _initialized = false;
        _iapService.ProductPurchased -= OnProductPurchased;
        _downloadProgress.ProgressChanged -= OnProgressChanged;
        _soundService.LocalSoundDeleted -= OnSoundDeleted;
    }
}