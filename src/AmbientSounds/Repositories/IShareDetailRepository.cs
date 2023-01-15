using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    /// <summary>
    /// Interface used for fetching share detail
    /// from remote storage.
    /// </summary>
    public interface IShareDetailRepository
    {
        /// <summary>
        /// Retrieves the share detail given the string of sound IDs.
        /// </summary>
        /// <param name="soundIds">Sound IDs to use to fetch the share details.</param>
        /// <returns>A share detail object corresponding to the given sound IDs.</returns>
        Task<ShareDetail?> GetShareDetailAsync(IReadOnlyList<string> soundIds);

        /// <summary>
        /// Retrieves the share detail given the share ID.
        /// </summary>
        /// <param name="soundIds">The share ID to use to fetch the share details.</param>
        /// <returns>A share detail object corresponding to the given share ID.</returns>
        Task<ShareDetail?> GetShareDetailAsync(string shareId);
    }
}