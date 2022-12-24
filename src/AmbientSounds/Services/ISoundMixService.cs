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
        /// Returns true if the given mix ID
        /// is currently playing.
        /// </summary>
        bool IsMixPlaying(string mixId);

        /// <summary>
        /// Saves the list of sounds into a new sound mix.
        /// </summary>
        /// <param name="sounds">List of sounds to save.</param>
        /// <returns>The string Id of the new sound mix.</returns>
        Task<string> SaveMixAsync(IReadOnlyList<Sound> sounds, string name = "");

        /// <summary>
        /// Reconstructs the given sound mixes and adds them to
        /// the installed sound list.
        /// </summary>
        /// <param name="dehydratedMixes">List of sound mixes that just have an Id, name, and list of sound Ids populated.</param>
        Task ReconstructMixesAsync(IList<Sound> dehydratedMixes);

        /// <summary>
        /// Loads the given mix to the player.
        /// </summary>
        /// <param name="mix">The mix to play.</param>
        Task<bool> LoadMixAsync(Sound mix);

        /// <summary>
        /// Returns enumerable
        /// of sound IDs that aren't available.
        /// Null or mpty if no unavailable sounds
        /// are found.
        /// </summary>
        Task<IEnumerable<string>> GetUnavailableSoundsAsync(Sound mix);

        /// <summary>
        /// Saves the current active sounds into a mix.
        /// </summary>
        /// <param name="name">Optional name for the mix.</param>
        /// <returns>The string Id of the new sound mix.</returns>
        Task<string> SaveCurrentMixAsync(string name = "");
        
        /// <summary>
        /// Returns true if the current mix can be saved.
        /// </summary>
        bool CanSaveCurrentMix();
    }
}