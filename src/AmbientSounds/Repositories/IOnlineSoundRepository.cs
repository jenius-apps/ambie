using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public interface IOnlineSoundRepository
{
    /// <summary>
    /// Retrieves the sound download link for
    /// the specified sound data.
    /// </summary>
    /// <param name="s">The sound object whose download link will be fetched.</param>
    /// <returns>URL to download the sound file.</returns>
    Task<string> GetDownloadLinkAsync(Sound s);

    /// <summary>
    /// Retrieves list of sound data available online.
    /// </summary>
    /// <returns>A list of <see cref="Sound"/> instances.</returns>
    Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync();

    /// <summary>
    /// Retrieves list of sound data available online
    /// based on given parameters.
    /// </summary>
    /// <param name="soundIds">The IDs of the sounds to fetch.</param>
    /// <param name="iapId">The iap id filter to apply to the query.</param>
    /// <returns>A list of <see cref="Sound"/> instances.</returns>
    Task<IReadOnlyDictionary<string, Sound?>> GetOnlineSoundsAsync(
        IReadOnlyList<string> soundIds,
        string? iapId = null);
}