using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class VersionedAssetViewModel : ObservableObject
{
    private readonly IVersionedAsset _versionedAsset;
    private readonly ILocalizer _localizer;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IUpdateService _updateService;
    private readonly ITelemetry _telemetry;
    private readonly IAssetLocalizer _assetLocalizer;
    private Progress<double> _downloadProgress;

    public VersionedAssetViewModel(
        IVersionedAsset versionedAsset,
        UpdateReason updateReason,
        ILocalizer localizer,
        IMixMediaPlayerService mixMediaPlayerService,
        IUpdateService updateService,
        ITelemetry telemetry,
        IAssetLocalizer assetLocalizer)
    {
        _versionedAsset = versionedAsset;
        UpdateReason = updateReason;
        _localizer = localizer;
        _mixMediaPlayerService = mixMediaPlayerService;
        _updateService = updateService;
        _telemetry = telemetry;
        _assetLocalizer = assetLocalizer;
        _downloadProgress = new Progress<double>();

        UpdateReasonText = UpdateReason switch
        {
            UpdateReason.MetaData => _localizer.GetString("UpdateReasonMetaData"),
            UpdateReason.File => _localizer.GetString("UpdateReasonFile"),
            UpdateReason.MetaDataAndFile => _localizer.GetString("UpdateReasonMetaDataAndFile"),
            _ => string.Empty
        };
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdateAvailable))]
    private UpdateReason _updateReason;

    [ObservableProperty]
    private double _downloadProgressValue;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisible))]
    private bool _downloadProgressVisible;

    [ObservableProperty]
    private bool _updateComplete;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ProgressRingVisible))]
    private bool _loading;

    public bool ProgressRingVisible => Loading || DownloadProgressVisible;

    public string UpdateReasonText { get; }

    public bool UpdateAvailable => UpdateReason != UpdateReason.None;

    public string ImagePath => _versionedAsset.ImagePath;

    public string Name => _assetLocalizer.GetLocalName(_versionedAsset);

    public async Task InitializeAsync()
    {
        await Task.Delay(1);
        _downloadProgress.ProgressChanged += OnProgressChanged;
    }

    public void Unitialize()
    {
        _downloadProgress.ProgressChanged -= OnProgressChanged;
    }

    [RelayCommand]
    private async Task UpdateAsync()
    {
        if (_mixMediaPlayerService.IsSoundPlaying(_versionedAsset.Id))
        {
            _mixMediaPlayerService.RemoveSound(_versionedAsset.Id);
        }

        Loading = true;
        UpdateReason = UpdateReason.None;
        await _updateService.TriggerUpdateAsync(_versionedAsset, _downloadProgress);
        _telemetry.TrackEvent(TelemetryConstants.UpdateSoundClicked, new Dictionary<string, string>
        {
            { "id", _versionedAsset.Id },
            { "name", _versionedAsset.Name }
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
            DownloadProgressVisible = false;
            UpdateComplete = true;
        }

        DownloadProgressValue = e;
    }
}
