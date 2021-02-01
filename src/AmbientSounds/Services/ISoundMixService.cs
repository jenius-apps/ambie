using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for performing sound mix operations.
    /// </summary>
    public interface ISoundMixService
    {
        /// <summary>
        /// Saves the list of sounds into a new sound mix.
        /// </summary>
        /// <param name="sounds">List of sounds to save.</param>
        /// <returns>The string Id of the new sound mix.</returns>
        Task<string> SaveMixAsync(IList<Sound> sounds, string name = "");

        /// <summary>
        /// Loads the given mix to the player.
        /// </summary>
        /// <param name="mix">The mix to play.</param>
        Task LoadMixAsync(Sound mix);
    }
}