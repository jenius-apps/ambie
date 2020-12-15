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
        /// <returns>A list of <see cref="Sound"/> instances.</returns>
        Task<IList<Sound>> GetSoundsAsync();

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
        /// Checks to see if given sound
        /// is already saved.
        /// </summary>
        /// <param name="s">The sound to check.</param>
        Task<bool> IsSoundInstalledAsync(string id);
    }
}
