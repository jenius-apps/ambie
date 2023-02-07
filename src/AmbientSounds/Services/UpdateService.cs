using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class UpdateService : IUpdateService
{
    private readonly ISoundService _soundService;
    private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
    private readonly IDownloadManager _downloadManager;

    public UpdateService(
        ISoundService soundService,
        IOnlineSoundDataProvider onlineSoundDataProvider,
        IDownloadManager downloadManager)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(onlineSoundDataProvider);
        Guard.IsNotNull(downloadManager);

        _soundService = soundService;
        _onlineSoundDataProvider = onlineSoundDataProvider;
        _downloadManager = downloadManager;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> CheckForUpdatesAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();

        var installed = await _soundService.GetLocalSoundsAsync();
        if (installed.Count == 0)
        {
            return Array.Empty<Sound>();
        }

        var installedIds = installed.Select(x => x.Id).ToArray();
        ct.ThrowIfCancellationRequested();
        var onlineSounds = await _onlineSoundDataProvider.GetSoundsAsync(installedIds);
        if (onlineSounds.Count == 0)
        {
            return Array.Empty<Sound>();
        }

        List<Sound> availableUpdates = new();
        foreach (var onlineSound in onlineSounds)
        {
            ct.ThrowIfCancellationRequested();
            var s = await _soundService.GetLocalSoundAsync(onlineSound.Id);
            if (s is null)
            {
                continue;
            }

            if (IsUpdateAvailable(onlineSound, s))
            {
                availableUpdates.Add(onlineSound);
            }
        }

        return availableUpdates;
    }

    /// <inheritdoc/>
    public async Task TriggerUpdateAsync(Sound onlineSound, IProgress<double> progress)
    {
        var installedSound = await _soundService.GetLocalSoundAsync(onlineSound.Id);
        if (installedSound is null || !IsUpdateAvailable(onlineSound, installedSound))
        {
            return;
        }

        await _downloadManager.QueueUpdateAsync(
            onlineSound, 
            progress, 
            installedSound.FileVersion == onlineSound.FileVersion);
    }

    private bool IsUpdateAvailable(Sound onlineSound, Sound installedSound)
    {
        return onlineSound.MetaDataVersion > installedSound.MetaDataVersion ||
            onlineSound.FileVersion > installedSound.FileVersion;
    }
}
