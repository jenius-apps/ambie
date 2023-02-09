using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private readonly ConcurrentDictionary<string, IProgress<double>> _activeDownloads = new();
        private readonly IVideoCache _videoCache;
        private readonly IOnlineVideoRepository _onlineVideoRepository;
        private readonly IDownloadManager _downloadManager;
        private readonly IFileWriter _fileWriter;

        /// <inheritdoc/>
        public event EventHandler<string>? VideoDownloaded;

        /// <inheritdoc/>
        public event EventHandler<string>? VideoDeleted;

        public VideoService(
            IVideoCache videoCache,
            IOnlineVideoRepository onlineVideoRepository,
            IDownloadManager downloadManager,
            IFileWriter fileWriter)
        {
            Guard.IsNotNull(videoCache, nameof(videoCache));
            Guard.IsNotNull(onlineVideoRepository, nameof(onlineVideoRepository));
            Guard.IsNotNull(downloadManager, nameof(downloadManager));
            Guard.IsNotNull(fileWriter, nameof(fileWriter));

            _videoCache = videoCache;
            _onlineVideoRepository = onlineVideoRepository;
            _downloadManager = downloadManager;
            _fileWriter = fileWriter;
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

            if (includeOnline)
            {
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
            }
            else
            {
                results.AddRange(offlineVids.Values);
            }

            return results;
        }

        /// <inheritdoc/>
        public Task<string> GetDownloadUrlAsync(string videoId)
        {
            return _onlineVideoRepository.GetDownloadUrlAsync(videoId);
        }

        /// <inheritdoc/>
        public async Task InstallVideoAsync(Video video, Progress<double>? progress = null)
        {
            if (string.IsNullOrEmpty(video.DownloadUrl))
            {
                video.DownloadUrl = await _onlineVideoRepository.GetDownloadUrlAsync(video.Id);
            }

            if (progress is null)
            {
                progress = new Progress<double>();
            }

            progress.ProgressChanged += OnProgressChanged;
            _activeDownloads.TryAdd(video.Id, progress);

            var destinationPath = await _downloadManager.QueueAndDownloadAsync(video, progress);

            var newVideo = new Video
            {
                Id = video.Id,
                FilePath = destinationPath,
                Extension = video.Extension,
                Name = video.Name,
                IapIds = video.IapIds.ToArray(),
                IsPremium = video.IsPremium,
                MegaByteSize = video.MegaByteSize,
                IsDownloaded = true
            };

            await _videoCache.AddOfflineVideoAsync(newVideo);

            void OnProgressChanged(object sender, double e)
            {
                if (e >= 100)
                {
                    VideoDownloaded?.Invoke(this, video.Id);
                    progress.ProgressChanged -= OnProgressChanged;
                    _activeDownloads.TryRemove(video.Id, out _);
                }
            }
        }

        /// <inheritdoc/>
        public async Task UninstallVideoAsync(Video video)
        {
            await _fileWriter.DeleteFileAsync(video.FilePath);
            await _videoCache.RemoveOfflineVideoAsync(video.Id);
            video.FilePath = string.Empty;
            video.IsDownloaded = false;
            VideoDeleted?.Invoke(this, video.Id);
        }

        /// <inheritdoc/>
        public Progress<double>? GetInstallProgress(Video video)
        {
            if (_downloadManager.GetProgress(video.FilePath) is Progress<double> progress)
            {
                if (_activeDownloads.TryAdd(video.Id, progress))
                {
                    progress.ProgressChanged += OnProgressChanged;

                    void OnProgressChanged(object sender, double e)
                    {
                        if (e >= 100)
                        {
                            progress.ProgressChanged -= OnProgressChanged;
                            _activeDownloads.TryRemove(video.Id, out _);

                            // The progress above must be removed from the active downloads list
                            // before this event is called so that downstream operations
                            // do not think there is still an active download.
                            VideoDownloaded?.Invoke(this, video.Id);
                        }
                    }
                }

                return progress;
            }

            return null;
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

        /// <inheritdoc/>
        public async Task<Video?> GetLocalVideoAsync(string videoId)
        {
            if (string.IsNullOrEmpty(videoId))
            {
                return null;
            }

            var offlineVids = await _videoCache.GetOfflineVideosAsync();
            if (offlineVids.TryGetValue(videoId, out Video value))
            {
                return value;
            }

            return null;
        }
    }
}
