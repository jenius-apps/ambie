using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public class VideoCache : IVideoCache
    {
        private readonly ConcurrentDictionary<string, Video> _onlineVideos = new();
        private readonly ConcurrentDictionary<string, Video> _offlineVideos = new();
        private readonly IOnlineVideoRepository _onlineVideoRepo;
        private readonly IOfflineVideoRepository _offlineVideoRepo;

        public VideoCache(
            IOnlineVideoRepository onlineVideoRepository,
            IOfflineVideoRepository offlineVideoRepository)
        {
            Guard.IsNotNull(onlineVideoRepository, nameof(onlineVideoRepository));
            Guard.IsNotNull(offlineVideoRepository, nameof(offlineVideoRepository));
            _onlineVideoRepo = onlineVideoRepository;
            _offlineVideoRepo = offlineVideoRepository;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetOnlineVideosAsync()
        {
            if (_onlineVideos.Count == 0)
            {
                IReadOnlyList<Video> videos = await _onlineVideoRepo.GetVideosAsync();
                foreach (var v in videos)
                {
                    _onlineVideos.TryAdd(v.Id, v);
                }
            }

            return _onlineVideos.Values.ToList();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyDictionary<string, Video>> GetOfflineVideosAsync()
        {
            if (_offlineVideos.Count == 0)
            {
                IReadOnlyList<Video> videos = await _offlineVideoRepo.GetVideosAsync();
                foreach (var v in videos)
                {
                    v.IsDownloaded = true;
                    _offlineVideos.TryAdd(v.Id, v);
                }
            }

            return _offlineVideos;
        }

        /// <inheritdoc/>
        public async Task AddOfflineVideoAsync(Video video)
        {
            await GetOfflineVideosAsync();

            _offlineVideos.TryAdd(video.Id, video);
            await _offlineVideoRepo.SaveVideosAsync(_offlineVideos.Values.ToArray());
        }

        /// <inheritdoc/>
        public async Task RemoveOfflineVideoAsync(string videoId)
        {
            await GetOfflineVideosAsync();

            _offlineVideos.TryRemove(videoId, out _);
            await _offlineVideoRepo.SaveVideosAsync(_offlineVideos.Values.ToArray());
        }
    }
}
