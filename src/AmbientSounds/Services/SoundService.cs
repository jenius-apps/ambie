using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public class SoundService : ISoundService
{
    private readonly ISoundCache _soundCache;
    private readonly IFileWriter _fileWriter;
    private readonly IAssetsReader _assetsReader;
    private Random? _random;

    /// <inheritdoc/>
    public event EventHandler<Sound>? LocalSoundAdded;

    /// <inheritdoc/>
    public event EventHandler<string>? LocalSoundDeleted;

    public SoundService(
        ISoundCache soundCache,
        IFileWriter fileWriter,
        IAssetsReader assetsReader)
    {
        Guard.IsNotNull(soundCache);
        Guard.IsNotNull(fileWriter);
        Guard.IsNotNull(assetsReader);
        _soundCache = soundCache;
        _fileWriter = fileWriter;
        _assetsReader = assetsReader;
    }

    /// <inheritdoc/>
    public async Task<Sound?> GetLocalSoundAsync(string? soundId)
    {
        if (soundId is null)
        {
            return null;
        }

        return await _soundCache.GetInstalledSoundAsync(soundId);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetLocalSoundsAsync(IReadOnlyList<string>? soundIds = null)
    {
        var localSounds = await _soundCache.GetInstalledSoundsAsync();
        if (soundIds is null)
        {
            return localSounds;
        }

        return localSounds.Where(x => soundIds.Contains(x.Id)).ToArray();
    }

    /// <inheritdoc/>
    public async Task PrepopulateSoundsIfEmpty()
    {
        var preInstalledSounds = await _soundCache.GetPreinstalledSoundsAsync();
        foreach (var s in preInstalledSounds)
        {
            if (!await IsSoundInstalledAsync(s.Id))
            {
                await _soundCache.AddLocalInstalledSoundAsync(s);
            }
        }

        var all = await _soundCache.GetInstalledSoundsAsync();
        int index = 0;
        foreach(var s in all)
        {
            s.SortPosition = index;
            index++;
        }

        await _soundCache.SaveCacheAsync();
    }

    /// <inheritdoc/>
    public async Task<bool> IsSoundInstalledAsync(string id)
    {
        Sound? sound = await _soundCache.GetInstalledSoundAsync(id);
        return sound is not null;
    }

    /// <inheritdoc/>
    public async Task AddLocalSoundAsync(Sound? s)
    {
        if (s is null || await IsSoundInstalledAsync(s.Id))
        {
            return;
        }

        s.SortPosition = _soundCache.InstallSoundsCount;
        await _soundCache.AddLocalInstalledSoundAsync(s);
        LocalSoundAdded?.Invoke(this, s);
    }

    /// <inheritdoc/>
    public async Task DeleteLocalSoundAsync(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return;
        }

        Sound? sound = await _soundCache.GetInstalledSoundAsync(id);
        if (sound is null)
        {
            return;
        }

        // Delete image only if it was downloaded. Images from the package cannot be deleted.
        if (!_assetsReader.IsPathFromPackage(sound.ImagePath))
        {
            await _fileWriter.DeleteFileAsync(sound.ImagePath);
        }

        // Delete sound only if it was downloaded. Sound files from the package cannot be deleted.
        if (!_assetsReader.IsPathFromPackage(sound.FilePath))
        {
            await _fileWriter.DeleteFileAsync(sound.FilePath);
        }

        // Update positions of remaining sounds.
        await UpdatePositionsAsync(sound.Id, sound.SortPosition, -1);

        // Delete metadata
        await _soundCache.RemoveLocalInstalledSoundAsync(id);

        LocalSoundDeleted?.Invoke(this, sound.Id);
    }

    /// <inheritdoc/>
    public async Task<Sound?> GetRandomSoundAsync()
    {
        _random ??= new Random();
        var sounds = await GetLocalSoundsAsync();
        if (sounds.Count <= 1)
        {
            return sounds.FirstOrDefault();
        }
        else
        {
            var index = _random.Next(sounds.Count);
            return sounds[index];
        }
    }

    /// <inheritdoc/>
    public async Task UpdatePositionsAsync(string soundId, int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex || oldIndex < 0 || string.IsNullOrEmpty(soundId))
        {
            return;
        }

        var sounds = await GetLocalSoundsAsync();

        if (newIndex < 0 && oldIndex >= 0)
        {
            // Item was deleted, so only decrement
            // the items that came after the old index.
            foreach (var s in sounds)
            {
                if (s.SortPosition > oldIndex)
                {
                    s.SortPosition--;
                }
            }
        }
        else
        {
            foreach (var s in sounds)
            {
                if (s.Id == soundId)
                {
                    s.SortPosition = newIndex;
                    continue;
                }

                if (oldIndex < newIndex)
                {
                    if (s.SortPosition > oldIndex && s.SortPosition <= newIndex)
                    {
                        s.SortPosition--;
                    }
                }
                else if (oldIndex > newIndex)
                {
                    if (s.SortPosition >= newIndex && s.SortPosition < oldIndex)
                    {
                        s.SortPosition++;
                    }
                }
            }
        }

        await _soundCache.SaveCacheAsync();
    }

    /// <inheritdoc/>
    public async Task UpdateSoundAsync(Sound updatedSound)
    {
        if (await GetLocalSoundAsync(updatedSound.Id) is Sound sound)
        {
            updatedSound.SortPosition = sound.SortPosition;
            await _soundCache.AddLocalInstalledSoundAsync(updatedSound);
        }
    }
}
