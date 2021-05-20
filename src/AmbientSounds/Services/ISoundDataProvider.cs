using AmbientSounds.Models;
using System;
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
        /// Local sound added.
        /// </summary>
        event EventHandler<Sound> LocalSoundAdded;

        /// <summary>
        /// Local sound deleted. ID parameter is provided.
        /// </summary>
        event EventHandler<string> LocalSoundDeleted;

        /// <summary>
        /// Retrieves list of sound data available.
        /// </summary>
        /// <param name="refresh">Optional. If true, data will be refreshed from local storage.</param>
        /// <param name="soundIds">Optional. Fetches sound with given Ids.</param>
        /// <returns>A list of <see cref="Sound"/> instances.</returns>
        Task<IList<Sound>> GetSoundsAsync(bool refresh = false, string[]? soundIds = null);

        /// <summary>
        /// Retrieves list of cached local sounds.
        /// </summary>
        Task<IList<Sound>> GetLocalSoundsAsync();

        /// <summary>
        /// Adds sound info to local list.
        /// </summary>
        /// <param name="s">The sound info to save.</param>
        Task AddLocalSoundAsync(Sound s);

        /// <summary>
        /// Deletes sound info to local list.
        /// </summary>
        /// <param name="s">The sound info to delete.</param>
        Task DeleteLocalSoundAsync(string id);

        /// <summary>
        /// Updates the given sounds in cache and in storage.
        /// </summary>
        /// <param name="sounds">The sounds to update.</param>
        Task UpdateLocalSoundAsync(IList<Sound> sounds);

        /// <summary>
        /// Checks to see if given sound
        /// is already saved.
        /// </summary>
        /// <param name="s">The sound to check.</param>
        Task<bool> IsSoundInstalledAsync(string id);

        /// <summary>
        /// Refreshes the downloaded metadata with updated metadata from
        /// the online provider. This is used to update things like names
        /// or attribution.
        /// </summary>
        Task RefreshLocalSoundsMetaDataAsync(IList<Sound> latestSoundMetaData);
    }
}
