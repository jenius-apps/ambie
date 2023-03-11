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
    private readonly Dictionary<string, Sound> _online = new();
    private readonly IOfflineSoundRepository _offlineSoundRepo;
    private readonly IOnlineSoundRepository _onlineSoundRepo;
    private readonly IAssetsReader _assetsReader;
    private DateTime _globalOnlineSoundCacheTime;

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
        if (_online.Count == 0 || _globalOnlineSoundCacheTime.AddHours(1) < DateTime.Now)
        {
            IReadOnlyList<Sound> sounds = await _onlineSoundRepo.GetOnlineSoundsAsync();
            foreach (var s in sounds)
            {
                _online[s.Id] = s;
            }

            _globalOnlineSoundCacheTime = DateTime.Now;
        }
        _onlineSoundsLock.Release();

        return _online.Values.ToArray();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync(IReadOnlyList<string> soundIds)
    {
        if (soundIds.Count == 0)
        {
            return Array.Empty<Sound>();
        }

        Sound[] orderedResults = new Sound[soundIds.Count];
        var notCachedSoundIds = new Dictionary<string, int>();
        await _onlineSoundsLock.WaitAsync();

        int index = 0;
        foreach (var id in soundIds)
        {
            if (_online.TryGetValue(id, out var sound))
            {
                orderedResults[index] = sound;
            }
            else
            {
                notCachedSoundIds.Add(id, index);
            }

            index++;
        }

        if (notCachedSoundIds.Count > 0)
        {
            var sounds = await _onlineSoundRepo.GetOnlineSoundsAsync(notCachedSoundIds.Keys.ToArray());
            
            foreach (var s in sounds)
            {
                _online[s.Id] = s;
                orderedResults[notCachedSoundIds[s.Id]] = s;
            }
            // note: we are purposefully not updating
            // the cache date here because that date represents
            // a global cache fetch, not a specific sound fetch.
        }

        _onlineSoundsLock.Release();
        return orderedResults.Where(x => x is not null).ToArray();
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
