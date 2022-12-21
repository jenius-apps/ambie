using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// An interface for a provider of sound data.
    /// </summary>
    public interface ISoundDataProvider
    {
        /// <summary>
        /// Retrieves list of sound data available.
        /// </summary>
        /// <param name="soundIds">Optional. Fetches sound with given Ids.</param>
        /// <returns>A list of <see cref="Sound"/> instances.</returns>
        Task<IList<Sound>> GetSoundsAsync(string[]? soundIds = null);

        /// <summary>
        /// Retrieves a random sound.
        /// </summary>
        Task<Sound> GetRandomSoundAsync();

        /// <summary>
        /// Updates the given sounds in cache and in storage.
        /// </summary>
        /// <param name="sounds">The sounds to update.</param>
        Task UpdateLocalSoundAsync(IReadOnlyList<Sound> sounds);

        /// <summary>
        /// Refreshes the downloaded metadata with updated metadata from
        /// the online provider. This is used to update things like names
        /// or attribution.
        /// </summary>
        Task RefreshLocalSoundsMetaDataAsync(IList<Sound> latestSoundMetaData);
    }
}
