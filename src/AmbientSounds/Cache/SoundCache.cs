using AmbientSounds.Models;
using AmbientSounds.Repositories;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class SoundCache : ISoundCache
{
    private readonly SemaphoreSlim _onlineSoundsLock = new(1, 1);
    private readonly ConcurrentDictionary<string, Sound> _installedSounds = new();
    private readonly ConcurrentDictionary<string, Sound> _preinstalled = new();
    private readonly ConcurrentDictionary<string, Sound> _online = new();
    private readonly IOfflineSoundRepository _offlineSoundRepo;
    private readonly IOnlineSoundRepository _onlineSoundRepo;
    private readonly IAssetsReader _assetsReader;
    private DateTime _onlineSoundCacheTime;

    public SoundCache(
        IOfflineSoundRepository offlineSoundRepository,
        IOnlineSoundRepository onlineSoundRepository,
        IAssetsReader assetsReader)
    {
        Guard.IsNotNull(offlineSoundRepository);
        Guard.IsNotNull(onlineSoundRepository);
        Guard.IsNotNull(assetsReader);
        _offlineSoundRepo = offlineSoundRepository;
        _onlineSoundRepo = onlineSoundRepository;
        _assetsReader = assetsReader;
    }

    /// <inheritdoc/>
    public int InstallSoundsCount => _installedSounds.Count;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync()
    {
        await _onlineSoundsLock.WaitAsync();
        if (_online.IsEmpty || _onlineSoundCacheTime.AddHours(1) < DateTime.Now)
        {
            IReadOnlyList<Sound> sounds = await _onlineSoundRepo.GetOnlineSoundsAsync();
            foreach (var s in sounds)
            {
                _online[s.Id] = s;
            }

            _onlineSoundCacheTime = DateTime.Now;
        }
        _onlineSoundsLock.Release();

        return _online.Values as IReadOnlyList<Sound> ?? Array.Empty<Sound>();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync(
        IList<string> soundIds,
        string? iapId = null)
    {
        await _onlineSoundsLock.WaitAsync();
        if (_online.IsEmpty || _onlineSoundCacheTime.AddHours(1) < DateTime.Now)
        {
            IReadOnlyList<Sound> sounds = await _onlineSoundRepo.GetOnlineSoundsAsync(
                soundIds,
                iapId);

            foreach (var s in sounds)
            {
                _online[s.Id] = s;
            }

            _onlineSoundCacheTime = DateTime.Now;
        }
        _onlineSoundsLock.Release();

        if (_online.IsEmpty)
        {
            return Array.Empty<Sound>();
        }

        List<Sound> result = new(soundIds.Count);
        foreach (var s in soundIds)
        {
            if (_online.TryGetValue(s, out Sound item))
            {
                result.Add(item);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetPreinstalledSoundsAsync()
    {
        if (_preinstalled.IsEmpty)
        {
            IReadOnlyList<Sound> sounds = await _assetsReader.GetPackagedSoundsAsync();
            foreach (var s in sounds)
            {
                _preinstalled.TryAdd(s.Id, s);
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
        if (_installedSounds.IsEmpty)
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

        _installedSounds.AddOrUpdate(sound.Id, sound, (_, _) => sound);
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
