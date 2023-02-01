﻿using AmbientSounds.Constants;
using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class OnlineSoundViewModel : ObservableObject
{
    private readonly Sound _sound;
    private readonly IDownloadManager _downloadManager;
    private readonly ISoundService _soundService;
    private readonly ITelemetry _telemetry;
    private readonly IIapService _iapService;
    private readonly IPreviewService _previewService;
    private readonly IDialogService _dialogService;
    private readonly IAssetLocalizer _assetLocalizer;
    private Progress<double> _downloadProgress;

    public OnlineSoundViewModel(
        Sound s, 
        IDownloadManager downloadManager,
        ISoundService soundService,
        ITelemetry telemetry,
        IPreviewService previewService,
        IIapService iapService,
        IDialogService dialogService,
        IAssetLocalizer assetLocalizer)
    {
        Guard.IsNotNull(s);
        Guard.IsNotNull(downloadManager);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(iapService);
        Guard.IsNotNull(previewService);
        Guard.IsNotNull(dialogService);
        Guard.IsNotNull(assetLocalizer);

        _sound = s;
        _downloadManager = downloadManager;
        _previewService = previewService;
        _iapService = iapService;
        _soundService = soundService;
        _telemetry = telemetry;
        _dialogService = dialogService;
        _assetLocalizer = assetLocalizer;

        _downloadProgress = new Progress<double>();
        _downloadProgress.ProgressChanged += OnProgressChanged;
        _soundService.LocalSoundDeleted += OnSoundDeleted;
        _iapService.ProductPurchased += OnProductPurchased;
    }
    
    public event EventHandler? DownloadCompleted;

    /// <summary>
    /// This sound's download progress.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DownloadProgressVisible))]
    [NotifyPropertyChangedFor(nameof(DownloadButtonVisible))]
    private double _downloadProgressValue;

    /// <summary>
    /// True if the item is already installed.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NotInstalled))]
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
    /// The sound's attribution.
    /// </summary>
    public string? Attribution => _sound.Attribution;

    /// <summary>
    /// Name of the sound.
    /// </summary>
    public string? Name => _assetLocalizer.GetLocalName(_sound);

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
        !string.IsNullOrWhiteSpace(_sound.PreviewFilePath) && 
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
    public bool PlusBadgeVisible => _sound.IsPremium && _sound.IapIds.ContainsAmbiePlus() && !_sound.IapIds.ContainsFreeId();

    /// <summary>
    /// Determines if the free badge is visible
    /// </summary>
    public bool FreeBadgeVisible => _sound.IsPremium && _sound.IapIds.ContainsFreeId();

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
        _telemetry.TrackEvent(TelemetryConstants.PreviewClicked, new Dictionary<string, string>
        {
            { "id", _sound.Id ?? "" },
        });

        _previewService.Play(_sound.PreviewFilePath);
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
    }

    [RelayCommand]
    private Task DownloadAsync()
    {
        if (DownloadProgressValue == 0 && NotInstalled)
        {
            _telemetry.TrackEvent(TelemetryConstants.DownloadClicked, new Dictionary<string, string>
            {
                { "id", _sound.Id ?? "" },
                { "location", TelemetryLocation },
                { "name", _sound.Name }
            });

            if (FreeBadgeVisible)
            {
                _telemetry.TrackEvent(TelemetryConstants.FreeDownloaded, new Dictionary<string, string>
                {
                    { "id", _sound.Id ?? "" },
                    { "name", _sound.Name }
                });
            }

            return _downloadManager.QueueAndDownloadAsync(_sound, _downloadProgress);
        }

        return Task.CompletedTask;
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

    /// <inheritdoc/>
    public void Dispose()
    {
        _iapService.ProductPurchased -= OnProductPurchased;
        _downloadProgress.ProgressChanged -= OnProgressChanged;
        _soundService.LocalSoundDeleted -= OnSoundDeleted;
    }
}