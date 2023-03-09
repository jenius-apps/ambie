using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface ICatalogueService
{
    /// <summary>
    /// Retrieves download link for given sound.
    /// </summary>
    Task<string> GetDownloadLinkAsync(Sound s);

    /// <summary>
    /// Gets list of sounds that can be viewed
    /// on the catalogue.
    /// </summary>
    Task<IReadOnlyList<Sound>> GetSoundsAsync();

    /// <summary>
    /// Gets list of sounds based on given IDs.
    /// </summary>
    /// <param name="soundIds">List of sounds to fetch.</param>
    /// <returns>List of sounds.</returns>
    Task<IReadOnlyList<Sound>> GetSoundsAsync(IReadOnlyList<string> soundIds);
}