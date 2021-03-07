using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for retrieving sound data
    /// from an online source.
    /// </summary>
    public interface IOnlineSoundDataProvider
    {
        /// <summary>
        /// Retrieves list of sound data available online.
        /// </summary>
        /// <returns>A list of <see cref="Sound"/> instances.</returns>
        Task<IList<Sound>> GetSoundsAsync();

        /// <summary>
        /// Retrieves list of sound data available online for the
        /// given sound IDs.
        /// </summary>
        /// <param name="soundIds">List of sounds to get.</param>
        Task<IList<Sound>> GetSoundsAsync(IList<string> soundIds);

        /// <summary>
        /// Retrieves list of sounds uploaded by the user to
        /// the Ambie catalogue.
        /// </summary>
        /// <param name="accesstoken">A JWT access token required for the API.</param>
        /// <returns>List of sounds uploaded by the user to the Ambie Catalogue.</returns>
        Task<IList<Sound>> GetUserSoundsAsync(string accesstoken);
    }
}
