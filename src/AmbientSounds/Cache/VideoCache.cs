using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class VideoCache : IVideoCache
{
    private readonly ConcurrentDictionary<string, Video> _onlineVideos = new();
    private readonly ConcurrentDictionary<string, Video> _offlineVideos = new();
    private readonly IOnlineVideoRepository _onlineVideoRepo;
    private readonly IOfflineVideoRepository _offlineVideoRepo;
    private readonly SemaphoreSlim _onlineCacheLock = new(1, 1);
    private readonly SemaphoreSlim _offlineCacheLock = new(1, 1);

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
        await _onlineCacheLock.WaitAsync();

        if (_onlineVideos.IsEmpty)
        {
            IReadOnlyList<Video> videos = await _onlineVideoRepo.GetVideosAsync();
            foreach (var v in videos)
            {
                _onlineVideos.TryAdd(v.Id, v);
            }
        }

        _onlineCacheLock.Release();

        return [.. _onlineVideos.Values];
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, Video>> GetOfflineVideosAsync()
    {
        await EnsureOfflineCacheInitializedAsync();
        return _offlineVideos;
    }

    private async Task EnsureOfflineCacheInitializedAsync()
    {
        await _offlineCacheLock.WaitAsync();

        if (_offlineVideos.IsEmpty)
        {
            IReadOnlyList<Video> videos = await _offlineVideoRepo.GetVideosAsync();
            foreach (var v in videos)
            {
                v.IsDownloaded = true;
                _offlineVideos.TryAdd(v.Id, v);
            }
        }

        _offlineCacheLock.Release();
    }

    /// <inheritdoc/>
    public async Task AddOfflineVideoAsync(Video video)
    {
        await EnsureOfflineCacheInitializedAsync();

        _offlineVideos.TryAdd(video.Id, video);
        await _offlineVideoRepo.SaveVideosAsync([.. _offlineVideos.Values]);
    }

    /// <inheritdoc/>
    public async Task RemoveOfflineVideoAsync(string videoId)
    {
        await EnsureOfflineCacheInitializedAsync();

        _offlineVideos.TryRemove(videoId, out _);
        await _offlineVideoRepo.SaveVideosAsync([.. _offlineVideos.Values]);
    }

    /// <inheritdoc/>
    public async Task<Video?> GetOfflineVideoAsync(string videoId)
    {
        if (string.IsNullOrEmpty(videoId))
        {
            return null;
        }

        await EnsureOfflineCacheInitializedAsync();
        if (_offlineVideos.TryGetValue(videoId, out Video result))
        {
            return result;
        }

        return null;
    }
}
