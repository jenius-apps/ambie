using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

/// <summary>
/// Interface for retrieving data from Assets folder
/// inside the install location.
/// </summary>
public interface IAssetsReader
{
    /// <summary>
    /// Retrieves list of paths for background images in Assets folder.
    /// </summary>
    Task<IReadOnlyList<string>> GetBackgroundsAsync();

    /// <summary>
    /// Retrieves list of paths for sound effects in Assets folder.
    /// </summary>
    Task<IReadOnlyList<string>> GetSoundEffectsAsync();

    /// <summary>
    /// Retrieves list of sounds pre-installed in assets folder.
    /// </summary>
    Task<IReadOnlyList<Sound>> GetPackagedSoundsAsync();

    /// <summary>
    /// Returns true if the file path is contained inside the app's
    /// package/install location.
    /// </summary>
    bool IsPathFromPackage(string filePath);
}
