using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IShareDetailCache
    {
        /// <summary>
        /// Retrieves the given share detail from cache. Retrieves
        /// from storage if not found in cache.
        /// </summary>
        /// <param name="soundIds">The soundIds associated with the share detail.</param>
        /// <returns>Share detail if found, null otherwise.</returns>
        Task<ShareDetail?> GetShareDetailAsync(IReadOnlyList<string> soundIds);

        /// <summary>
        /// Retrieves the given share detail from cache. Retrieves
        /// from storage if not found in cache.
        /// </summary>
        /// <param name="soundIds">The ID of the share detail.</param>
        /// <returns>Share detail if found, null otherwise.</returns>
        Task<ShareDetail?> GetShareDetailAsync(string shareId);
    }
}