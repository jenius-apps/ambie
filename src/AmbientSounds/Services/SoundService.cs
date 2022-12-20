using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

public class SoundService : ISoundService
{
    private readonly ISoundCache _soundCache;
    private readonly IFileWriter _fileWriter;
    private readonly IAssetsReader _assetsReader;

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

    public Task<IReadOnlyList<Sound>> GetLocalSoundsAsync()
    {
        return _soundCache.GetInstalledSoundsAsync();
    }

    public async Task PrepopulateSoundsIfEmpty()
    {
        // TODO: in the calling method, make sure it's only called on first run.
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

    /// <inheritdoc/>
    public async Task AddLocalSoundAsync(Sound? s)
    {
        if (s is null || await IsSoundInstalledAsync(s.Id))
        {
            return;
        }

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

        // Delete image
        if (!_assetsReader.IsPathFromPackage(sound.ImagePath))
        {
            await _fileWriter.DeleteFileAsync(sound.ImagePath);
        }

        // Delete sound file
        if (!_assetsReader.IsPathFromPackage(sound.FilePath))
        {
            await _fileWriter.DeleteFileAsync(sound.FilePath);
        }

        // Delete metadata
        await _soundCache.RemoveLocalInstalledSoundAsync(id);

        LocalSoundDeleted?.Invoke(this, sound.Id);
    }
}
