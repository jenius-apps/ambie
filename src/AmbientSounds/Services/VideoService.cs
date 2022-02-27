using AmbientSounds.Cache;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Service for retrieving videos from online
    /// and from offline.
    /// </summary>
    public class VideoService : IVideoService
    {
        private readonly IVideoCache _videoCache;

        public VideoService(IVideoCache videoCache)
        {
            Guard.IsNotNull(videoCache, nameof(videoCache));
            _videoCache = videoCache;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetVideosAsync()
        {
            List<Video> results = new();

            // Get list of videos from service
            var onlineVidsTask = _videoCache.GetOnlineVideosAsync();

            // Get list of offline videos
            var offlineVids = await _videoCache.GetOfflineVideosAsync();
            var onlineVids = await onlineVidsTask;

            foreach (var vid in onlineVids)
            {
                if (offlineVids.ContainsKey(vid.Id))
                {
                    results.Add(offlineVids[vid.Id]);
                }
                else
                {
                    results.Add(vid);
                }
            }

            return results;
        }

        /// <inheritdoc/>
        public async Task<string> GetFilePathAsync(string? videoId)
        {
            if (videoId is null)
            {
                return string.Empty;
            }

            var offlineVids = await _videoCache.GetOfflineVideosAsync();
            return offlineVids.ContainsKey(videoId)
                ? offlineVids[videoId].FilePath
                : string.Empty;
        }
    }
}
