using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IVideoCache
    {
        /// <summary>
        /// Retrieves dictionary of offline video data.
        /// If cache is empty, new data will be retrieved.
        /// Otherwise, cache is returned.
        /// </summary>
        Task<IReadOnlyDictionary<string, Video>> GetOfflineVideosAsync();

        /// <summary>
        /// Retrieves dictionary of online video data.
        /// If cache is empty, new data will be retrieved.
        /// Otherwise, cache is returned.
        /// </summary>
        Task<IReadOnlyList<Video>> GetOnlineVideosAsync();

        /// <summary>
        /// Adds the given video data to cache
        /// and writes to storage.
        /// </summary>
        Task AddOfflineVideoAsync(Video video);

        /// <summary>
        /// Removes the given video data from cache
        /// and from metadata storage.
        /// </summary>
        Task RemoveOfflineVideoAsync(string videoId);
    }
}