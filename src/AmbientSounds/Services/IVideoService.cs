using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for retrieving videos.
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// Gets local file path for given video.
        /// Returns empty string if no local path found.
        /// </summary>
        /// <param name="videoId">The <see cref="Video.Id"/> to use.</param>
        /// <returns>Path to video or empty string if no local path found.</returns>
        Task<string> GetFilePathAsync(string? videoId);

        /// <summary>
        /// Retrieves list of videos that can be displayed
        /// in a list on-screen. 
        /// </summary>
        Task<IReadOnlyList<Video>> GetVideosAsync();
    }
}