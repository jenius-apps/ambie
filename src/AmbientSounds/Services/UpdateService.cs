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

    public UpdateService(
        ISoundService soundService,
        IOnlineSoundDataProvider onlineSoundDataProvider)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(onlineSoundDataProvider);

        _soundService = soundService;
        _onlineSoundDataProvider = onlineSoundDataProvider;
    }

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

            if (onlineSound.MetaDataVersion > s.MetaDataVersion ||
                onlineSound.FileVersion > s.FileVersion)
            {
                availableUpdates.Add(onlineSound);
            }
        }

        return availableUpdates;
    }
}
