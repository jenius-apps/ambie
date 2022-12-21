using AmbientSounds.Models;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for renaming sounds.
    /// </summary>
    public interface IRenamer
    {
        /// <summary>
        /// Pops a dialog to rename the given sound. Sound cannot be 
        /// a packaged sound.
        /// </summary>
        /// <param name="sound">Local sound to rename.</param>
        /// <returns>True if the rename was successful.</returns>
        Task<bool> RenameAsync(Sound sound);
    }
}