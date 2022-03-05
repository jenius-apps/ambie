using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IOnlineVideoRepository
    {
        /// <summary>
        /// Retrieves the download URL link for the given video.
        /// </summary>
        Task<string> GetDownloadUrlAsync(string videoId);

        /// <summary>
        /// Retrives a list of video metadata from
        /// an online source.
        /// </summary>
        Task<IReadOnlyList<Video>> GetVideosAsync();
    }
}
