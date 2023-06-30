using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class UpdateService : IUpdateService
{
    private readonly ISoundService _soundService;
    private readonly ICatalogueService _catalogueService;
    private readonly IDownloadManager _downloadManager;
    private readonly IGuideService _guideService;

    public UpdateService(
        ISoundService soundService,
        ICatalogueService catalogueService,
        IDownloadManager downloadManager,
        IGuideService guideService)
    {
        _soundService = soundService;
        _catalogueService = catalogueService;
        _downloadManager = downloadManager;
        _guideService = guideService;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<(IVersionedAsset, UpdateReason)>> CheckForUpdatesAsync(CancellationToken ct)
    {
        List<(IVersionedAsset, UpdateReason)> results = new();
        var soundResults = await CheckForSoundUpdatesAsync(ct);
        results.AddRange(soundResults);

        var guideResults = await CheckForGuideUpdatesAsync(ct);
        results.AddRange(guideResults);

        return results;
    }

    private async Task<IReadOnlyList<(IVersionedAsset, UpdateReason)>> CheckForGuideUpdatesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var installed = await _guideService.GetOfflineGuidesAsync();
        if (installed.Count == 0)
        {
            return Array.Empty<(IVersionedAsset, UpdateReason)>();
        }

        var onlineGuides = await _guideService.GetOnlineGuidesAsync();
        if (onlineGuides.Count == 0)
        {
            return Array.Empty<(IVersionedAsset, UpdateReason)>();
        }

        var dictionary = installed.ToDictionary(x => x.Id);
        List<(IVersionedAsset, UpdateReason)> availableUpdates = new();
        foreach (var onlineGuide in onlineGuides)
        {
            ct.ThrowIfCancellationRequested();
            if (dictionary.TryGetValue(onlineGuide.Id, out Guide offlineGuide) &&
                GetUpdateReason(onlineGuide, offlineGuide) is UpdateReason r &&
                r != UpdateReason.None)
            {
                availableUpdates.Add((onlineGuide, r));
            }
        }

        return availableUpdates;
    }

    private async Task<IReadOnlyList<(IVersionedAsset, UpdateReason)>> CheckForSoundUpdatesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var installed = await _soundService.GetLocalSoundsAsync();
        if (installed.Count == 0)
        {
            return Array.Empty<(IVersionedAsset, UpdateReason)>();
        }

        var installedIds = installed.Select(x => x.Id).ToArray();
        ct.ThrowIfCancellationRequested();
        var onlineSounds = await _catalogueService.GetSoundsAsync(installedIds);
        if (onlineSounds.Count == 0)
        {
            return Array.Empty<(IVersionedAsset, UpdateReason)>();
        }

        List<(IVersionedAsset, UpdateReason)> availableUpdates = new();
        foreach (var onlineSound in onlineSounds)
        {
            ct.ThrowIfCancellationRequested();
            var s = await _soundService.GetLocalSoundAsync(onlineSound.Id);
            if (s is null)
            {
                continue;
            }

            if (GetUpdateReason(onlineSound, s) is UpdateReason r && r != UpdateReason.None)
            {
                availableUpdates.Add((onlineSound, r));
            }
        }

        return availableUpdates;
    }

    /// <inheritdoc/>
    public async Task TriggerUpdateAsync(
        Sound onlineSound,
        IProgress<double> progress)
    {
        var installedSound = await _soundService.GetLocalSoundAsync(onlineSound.Id);
        if (installedSound is null || GetUpdateReason(onlineSound, installedSound) == UpdateReason.None)
        {
            return;
        }

        await _downloadManager.QueueUpdateAsync(
            onlineSound, 
            progress,
            installedSound.ImagePath,
            installedSound.FilePath,
            installedSound.FileVersion == onlineSound.FileVersion);
    }

    public async Task TriggerUpdateAsync(
        IVersionedAsset asset,
        IProgress<double> progress)
    {
        if (asset is Sound sound)
        {
            await TriggerUpdateAsync(sound, progress);
        }
        else if (asset is Guide guide && progress is Progress<double> p)
        {
            var deleted = await _guideService.DeleteAsync(guide.Id);

            if (deleted)
            {
                await _guideService.DownloadAsync(guide, p);
            }
        }
    }

    private static UpdateReason GetUpdateReason(
        IVersionedAsset newAsset,
        IVersionedAsset oldAsset)
    {
        if (newAsset.MetaDataVersion > oldAsset.MetaDataVersion &&
            newAsset.FileVersion > oldAsset.FileVersion)
        {
            return UpdateReason.MetaDataAndFile;
        }
        else if (newAsset.MetaDataVersion > oldAsset.MetaDataVersion)
        {
            return UpdateReason.MetaData;
        }
        else if (newAsset.FileVersion > oldAsset.FileVersion)
        {
            return UpdateReason.File;
        }

        return UpdateReason.None;
    }
}
