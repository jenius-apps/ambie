using AmbientSounds.Cache;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class SoundService : ISoundService
{
    private readonly ISoundCache _soundCache;

    public SoundService(ISoundCache soundCache)
    {
        Guard.IsNotNull(soundCache);
        _soundCache = soundCache;
    }

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
            await _soundCache.AddLocalInstalledSoundAsync(s.Value);
        }
    }
}
