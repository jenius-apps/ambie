using AmbientSounds.Cache;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public class SoundService : ISoundService
{
    private readonly ISoundCache _soundCache;

    public SoundService(ISoundCache soundCache)
    {
        Guard.IsNotNull(soundCache);
        _soundCache = soundCache;
    }

    // TODO move add/delete functionality here from SoundDataProvider.
    public Task<IReadOnlyList<Sound>> GetLocalSoundsAsync()
    {
        return _soundCache.GetInstalledSoundsAsync();
    }

    public async Task PrepopulateSoundsIfEmpty()
    {
        var sounds = await _soundCache.GetInstalledSoundsAsync();
        if (sounds.Count != 0)
        {
            return;
        }

        var preInstalledSounds = await _soundCache.GetPreinstalledSoundsAsync();
        foreach (var s in preInstalledSounds)
        {
            await _soundCache.AddLocalInstalledSoundAsync(s);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> IsSoundInstalledAsync(string id)
    {
        Sound? sound = await _soundCache.GetInstalledSoundAsync(id);
        return sound is not null;
    }
}
