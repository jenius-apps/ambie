using AmbientSounds.Converters;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.ApplicationModel;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

/// <summary>
/// Class with functions that read data
/// from the install location's Assets folder.
/// </summary>
public class AssetsReader : IAssetsReader
{
    private const string DataFileName = "Data.json";

    /// <inheritdoc/>
    public bool IsPathFromPackage(string filePath)
    {
        return filePath.StartsWith("ms-appx:///Assets");
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetBackgroundsAsync()
    {
        StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
        StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
        StorageFolder backgrounds = await assets.GetFolderAsync("Backgrounds");
        var images = await backgrounds.GetFilesAsync();
        return images.Select(static x => $"ms-appx:///Assets/Backgrounds/{x.Name}").ToArray();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetSoundEffectsAsync()
    {
        StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
        StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
        StorageFolder soundEffects = await assets.GetFolderAsync("SoundEffects");
        var sounds = await soundEffects.GetFilesAsync();
        return sounds.Select(static x => $"ms-appx:///Assets/SoundEffects/{x.Name}").ToArray();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetPackagedSoundsAsync()
    {
        StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
        StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
        StorageFile dataFile = await assets.GetFileAsync(DataFileName);
        using Stream dataStream = await dataFile.OpenStreamForReadAsync();
        var sounds = await JsonSerializer.DeserializeAsync(dataStream, AmbieJsonSerializerContext.Default.ListSound);
        if (sounds is null)
        {
            return Array.Empty<Sound>();
        }

        return sounds;
    }
}
