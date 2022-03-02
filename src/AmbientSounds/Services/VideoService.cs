using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.Repositories;
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
        private readonly IOnlineVideoRepository _onlineVideoRepository;

        public VideoService(
            IVideoCache videoCache,
            IOnlineVideoRepository onlineVideoRepository)
        {
            Guard.IsNotNull(videoCache, nameof(videoCache));
            Guard.IsNotNull(onlineVideoRepository, nameof(onlineVideoRepository));

            _videoCache = videoCache;
            _onlineVideoRepository = onlineVideoRepository;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetVideosAsync(
            bool includeOnline = true,
            bool includeOffline = true)
        {
            List<Video> results = new();

            // Get list of videos from service
            Task<IReadOnlyList<Video>> onlineVidsTask = includeOnline
                ? _videoCache.GetOnlineVideosAsync()
                : Task.FromResult(Array.Empty<Video>() as IReadOnlyList<Video>);

            // Get list of offline videos
            IReadOnlyDictionary<string, Video> offlineVids = includeOffline
                ? await _videoCache.GetOfflineVideosAsync()
                : new Dictionary<string, Video>();

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
        public Task<string> GetDownloadUrlAsync(string videoId)
        {
            return _onlineVideoRepository.GetDownloadUrlAsync(videoId);
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
