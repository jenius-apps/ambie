using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
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

    public UpdateService(
        ISoundService soundService,
        ICatalogueService catalogueService,
        IDownloadManager downloadManager)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(catalogueService);
        Guard.IsNotNull(downloadManager);

        _soundService = soundService;
        _catalogueService = catalogueService;
        _downloadManager = downloadManager;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<(Sound, UpdateReason)>> CheckForUpdatesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var installed = await _soundService.GetLocalSoundsAsync();
        if (installed.Count == 0)
        {
            return Array.Empty<(Sound, UpdateReason)>();
        }

        var installedIds = installed.Select(x => x.Id).ToArray();
        ct.ThrowIfCancellationRequested();
        var onlineSounds = await _catalogueService.GetSoundsAsync(installedIds);
        if (onlineSounds.Count == 0)
        {
            return Array.Empty<(Sound, UpdateReason)>();
        }

        List<(Sound, UpdateReason)> availableUpdates = new();
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
    public async Task TriggerUpdateAsync(Sound onlineSound, IProgress<double> progress)
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

    public async Task TriggerUpdateAsync(IVersionedAsset asset, IProgress<double> progress)
    {
        if (asset is Sound sound)
        {
            await TriggerUpdateAsync(sound, progress);
        }
    }

    private UpdateReason GetUpdateReason(Sound newSound, Sound oldSound)
    {
        if (newSound.MetaDataVersion > oldSound.MetaDataVersion &&
            newSound.FileVersion > oldSound.FileVersion)
        {
            return UpdateReason.MetaDataAndFile;
        }
        else if (newSound.MetaDataVersion > oldSound.MetaDataVersion)
        {
            return UpdateReason.MetaData;
        }
        else if (newSound.FileVersion > oldSound.FileVersion)
        {
            return UpdateReason.File;
        }

        return UpdateReason.None;
    }
}
