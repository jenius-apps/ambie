using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public class VideoService : IVideoService
    {
        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetVideosAsync()
        {
            await Task.Delay(1);
            List<Video> results = new();

            // Get list of videos from service

            // Get list of offline videos

            // loop through the videos from service and mark them as downloaded and add their local paths

            results.Add(new Video
            {
                IsDownloaded = true,
                FilePath = "ms-appx:///Assets/aaa_beach.mp4",
                MegaByteSize = 118,
                Id = "123",
                Name = "Beach"
            });

            return results;
        }

        /// <inheritdoc/>
        public async Task<string> GetFilePathAsync(string? videoId)
        {
            if (videoId is null)
            {
                return string.Empty;
            }

            await Task.Delay(1);
            return "ms-appx:///Assets/aaa_beach.mp4";
        }
    }
}
