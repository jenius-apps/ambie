using AmbientSounds.Models;
using AmbientSounds.Repositories;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class SoundCache : ISoundCache
{
    private readonly ConcurrentDictionary<string, Sound> _installedSounds = new();
    private readonly ConcurrentDictionary<string, Sound> _preinstalled = new();
    private readonly IOfflineSoundRepository _offlineSoundRepo;
    private readonly IAssetsReader _assetsReader;

    public SoundCache(
        IOfflineSoundRepository offlineSoundRepository,
        IAssetsReader assetsReader)
    {
        Guard.IsNotNull(offlineSoundRepository);
        Guard.IsNotNull(assetsReader);
        _offlineSoundRepo = offlineSoundRepository;
        _assetsReader = assetsReader;
    }

    /// <inheritdoc/>
    public int InstallSoundsCount => _installedSounds.Count;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetPreinstalledSoundsAsync()
    {
        if (_preinstalled.Count == 0)
        {
            IReadOnlyList<Sound> videos = await _assetsReader.GetPackagedSoundsAsync();
            foreach (var v in videos)
            {
                _preinstalled.TryAdd(v.Id, v);
            }
        }

        return _preinstalled.Values as IReadOnlyList<Sound> ?? Array.Empty<Sound>();
    }

    /// <inheritdoc/>
    public async Task<Sound?> GetInstalledSoundAsync(string stringId)
    {
        await GetInstalledSoundsAsync();

        return _installedSounds.TryGetValue(stringId, out Sound result) ? result : null;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetInstalledSoundsAsync()
    {
        if (_installedSounds.Count == 0)
        {
            IReadOnlyList<Sound> videos = await _offlineSoundRepo.GetLocalSoundsAsync();
            foreach (var v in videos)
            {
                _installedSounds.TryAdd(v.Id, v);
            }
        }

        return (_installedSounds.Values as IReadOnlyList<Sound>) ?? Array.Empty<Sound>();
    }

    /// <inheritdoc/>
    public async Task AddLocalInstalledSoundAsync(Sound sound)
    {
        await GetInstalledSoundsAsync();

        _installedSounds.TryAdd(sound.Id, sound);
        await _offlineSoundRepo.SaveLocalSoundsAsync(_installedSounds.Values.ToArray());
    }

    /// <inheritdoc/>
    public async Task RemoveLocalInstalledSoundAsync(string videoId)
    {
        await GetInstalledSoundsAsync();

        _installedSounds.TryRemove(videoId, out _);
        await _offlineSoundRepo.SaveLocalSoundsAsync(_installedSounds.Values.ToArray());
    }

    /// <inheritdoc/>
    public async Task SaveCacheAsync()
    {
        await _offlineSoundRepo.SaveLocalSoundsAsync(_installedSounds.Values.ToArray());
    }
}
