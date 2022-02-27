using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IOfflineVideoRepository
    {
        /// <summary>
        /// Retrives a list of video metadata from
        /// an offline source.
        /// </summary>
        Task<IReadOnlyList<Video>> GetVideosAsync();

        /// <summary>
        /// Overwrites the local storage with the given list of video
        /// metadata.
        /// </summary>
        /// <param name="videos">List of videos to that will overwrite the storage file.</param>
        Task SaveVideosAsync(IList<Video> videos);
    }
}
