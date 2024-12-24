using AmbientSounds.Cache;
using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class ChannelService : IChannelService
{
    private readonly IChannelCache _channelCache;
    private readonly ISoundService _soundService;
    private readonly IVideoService _videoService;
    private readonly IIapService _iapService;
    private readonly IDownloadManager _downloadManager;
    private readonly ICatalogueService _catalogueService;
    private readonly INavigator _navigator;
    private readonly IMixMediaPlayerService _player;
    private readonly ConcurrentDictionary<string, double> _activeVideoDownloadProgress = new();
    private readonly ConcurrentDictionary<string, double> _activeSoundDownloadProgress = new();
    private readonly ConcurrentDictionary<string, IProgress<double>> _activeChannelProgress = new();

    /// <inheritdoc/>
    public event EventHandler<string>? ChannelDownloaded;

    public ChannelService(
        IChannelCache channelCache,
        ISoundService soundService,
        IVideoService videoService,
        IIapService iapService,
        IDownloadManager downloadManager,
        ICatalogueService catalogueService,
        INavigator navigator,
        IMixMediaPlayerService mixMediaPlayerService)
    {
        _channelCache = channelCache;
        _soundService = soundService;
        _videoService = videoService;
        _iapService = iapService;
        _downloadManager = downloadManager;
        _catalogueService = catalogueService;
        _navigator = navigator;
        _player = mixMediaPlayerService;
    }

    /// <inheritdoc/>
    public string? MostRecentChannelDetailsViewed { get; set; }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Channel>> GetChannelsAsync()
    {
        var cache = await _channelCache.GetItemsAsync().ConfigureAwait(false);
        List<Channel> result = [];
        Channel? darkscreen = null;
        Channel? slideshow = null;

        foreach (var c in cache.OrderBy(x => x.Key))
        {
            if (c.Value.Type is ChannelType.DarkScreen)
            {
                darkscreen = c.Value;
            }
            else if (c.Value.Type is ChannelType.Slideshow)
            {
                slideshow = c.Value;
            }
            else
            {
                result.Add(c.Value);
            }
        }

        if (darkscreen is not null)
        {
            result.Insert(0, darkscreen);
        }

        if (slideshow is not null)
        {
            result.Insert(0, slideshow);
        }

        return result;
    }

    /// <inheritdoc/>
    public Progress<double>? TryGetActiveProgress(Channel c)
    {
        if (_activeChannelProgress.TryGetValue(c.Id, out IProgress<double> activeProgress) &&
            activeProgress is Progress<double> result)
        {
            return result;
        }

        return null;
    }

    public async Task<bool> QueueInstallChannelAsync(Channel channel, Progress<double>? progress = null)
    {
        bool containsAssociatedSound = channel is { SoundIds.Count: > 0 };
        string soundId = containsAssociatedSound
            ? channel.SoundIds[0]
            : string.Empty;

        if (channel is not { Type: ChannelType.Videos, VideoIds: [string videoId, ..] })
        {
            return false;
        }

        var isSoundInstalled = false;
        var isVideoInstalled = await _videoService.IsVideoInstalledAsync(videoId);

        if (containsAssociatedSound)
        {
            isSoundInstalled = await _soundService.IsSoundInstalledAsync(soundId);
            if (isSoundInstalled && isVideoInstalled)
            {
                return false;
            }
        }
        else
        {
            if (isVideoInstalled)
            {
                return false;
            }
        }

        bool isSoundQueued = false;
        bool isVideoQueued = false;
        Progress<double> channelProgress = progress ?? new();
        channelProgress.ProgressChanged += OnChannelProgressChanged;

        if (containsAssociatedSound && !isSoundInstalled)
        {
            var sounds = await _catalogueService.GetSoundsAsync([soundId]);
            var soundToDownload = sounds.Count > 0 ? sounds[0] : null;

            if (soundToDownload is not null && !_activeSoundDownloadProgress.ContainsKey(soundId))
            {
                // Check if sound is already being downloaded outside of the channel serivce.
                IProgress<double>? activeExternalProgress = _downloadManager.GetProgress(soundToDownload);
                bool shouldQueueNewSound = activeExternalProgress is null;
                Progress<double> soundProgress = activeExternalProgress as Progress<double> ?? new();

                _activeSoundDownloadProgress[soundId] = 0;
                soundProgress.ProgressChanged += OnSoundProgressChanged;

                if (shouldQueueNewSound)
                {
                    // Only queue if the sound isn't already being downloaded.
                    await _downloadManager.QueueAndDownloadAsync(soundToDownload, soundProgress);
                }

                isSoundQueued = true;

                void OnSoundProgressChanged(object sender, double e)
                {
                    OnAssetProgressChanged(
                        soundId,
                        videoId,
                        _activeSoundDownloadProgress,
                        _activeVideoDownloadProgress,
                        e,
                        channelProgress);
                }
            }
        }

        if (!isVideoInstalled)
        {
            var onlineVideos = await _videoService.GetVideosAsync(includeOffline: false);
            var videoToDownload = onlineVideos.FirstOrDefault(x => x.Id == videoId);

            if (videoToDownload is not null && !_activeVideoDownloadProgress.ContainsKey(videoId))
            {
                // TODO expand if statement to check if there's an active sound download from
                // outside the channel service. Need to check the downloader itself.

                _activeVideoDownloadProgress[videoId] = 0;
                var videoProgress = new Progress<double>();
                videoProgress.ProgressChanged += OnVideoProgressChanged;
                await _videoService.InstallVideoAsync(videoToDownload, videoProgress);
                isVideoQueued = true;

                void OnVideoProgressChanged(object sender, double e)
                {
                    OnAssetProgressChanged(
                        videoId,
                        soundId,
                        _activeVideoDownloadProgress,
                        _activeSoundDownloadProgress, 
                        e,
                        channelProgress);
                }
            }
        }

        bool queueSuccess = isSoundQueued || isVideoQueued;
        if (queueSuccess)
        {
            _activeChannelProgress[channel.Id] = channelProgress;
        }
        else
        {
            channelProgress.ProgressChanged -= OnChannelProgressChanged;
        }

        void OnChannelProgressChanged(object sender, double e)
        {
            if (e >= 100)
            {
                ChannelDownloaded?.Invoke(this, channel.Id);
                if (_activeChannelProgress.TryRemove(channel.Id, out var removedValue) && 
                    removedValue is Progress<double> completedProgress)
                {
                    completedProgress.ProgressChanged -= OnChannelProgressChanged;
                }
            }
        }

        return queueSuccess;
    }

    private static double OnAssetProgressChanged(
        string thisAssetId,
        string otherAssetId,
        ConcurrentDictionary<string, double> thisAssetDictionary,
        ConcurrentDictionary<string, double> otherAssetDictionary,
        double thisRawProgress,
        IProgress<double> channelProgress)
    {
        var currentProgress = otherAssetDictionary.ContainsKey(otherAssetId) ? thisRawProgress / 2 : thisRawProgress;
        thisAssetDictionary[thisAssetId] = currentProgress;

        var sum = otherAssetDictionary.TryGetValue(otherAssetId, out double soundValue)
            ? currentProgress + soundValue
            : currentProgress;

        channelProgress.Report(sum);
        return sum;
    }

    /// <inheritdoc/>
    public async Task<bool> IsFullyDownloadedAsync(Channel channel)
    {
        if (channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            // Dark screen and slideshow work without downloading anything,
            // so they're always fully downloaded.
            return true;
        }

        if (channel is not { VideoIds: [string videoId, ..], SoundIds: [string soundId, ..] })
        {
            // If the video channel is misconfigured so it is missing sounds or videos,
            // always say it's not fully downloaded.
            return false;
        }

        bool isSoundInstalled = await _soundService.IsSoundInstalledAsync(soundId);
        bool isVideoInstalled = await _videoService.IsVideoInstalledAsync(videoId);
        return isSoundInstalled && isVideoInstalled;
    }

    /// <inheritdoc/>
    public async Task<bool> IsOwnedAsync(Channel channel)
    {
        if (channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            // Dark screen and slideshow work without downloading anything,
            // so they're always owned.
            return true;
        }

        var isOwned = await _iapService.IsAnyOwnedAsync(channel.IapIds);
        return isOwned;
    }

    /// <inheritdoc/>
    public async Task PlayChannelAsync(Channel channel, bool performNavigation = true)
    {
        if (channel.Type is ChannelType.Videos)
        {
            if (channel.VideoIds.Count == 0)
            {
                return;
            }

            if (channel.SoundIds is [string soundId, ..] && await _soundService.GetLocalSoundAsync(soundId) is Sound s)
            {
                await _player.PlayFeaturedSoundAsync(FeaturedSoundType.Channel, s.Id, s.FilePath, enableGaplessLoop: true);
            }
            else
            {
                await PlayCurrentSoundsOrRandomAsync(_player);
            }
        }
        else if (channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            await PlayCurrentSoundsOrRandomAsync(_player);
        }

        if (performNavigation)
        {
            var args = new ScreensaverArgs()
            {
                RequestedType = channel.Type,
                VideoId = channel is { Type: ChannelType.Videos, VideoIds: [string videoId, ..] } ? videoId : null,
                VideoImagePreviewUrl = channel.Type is ChannelType.Videos ? channel.ImagePath : null
            };

            _navigator.ToScreensaver(args);
        }
    }

    private static async Task PlayCurrentSoundsOrRandomAsync(IMixMediaPlayerService mixMediaPlayerService)
    {
        if (mixMediaPlayerService.GetSoundIds().Length == 0)
        {
            await mixMediaPlayerService.AddRandomAsync();
        }
        else
        {
            mixMediaPlayerService.Play();
        }
    }

    /// <inheritdoc/>
    public async Task DeleteChannelAsync(Channel channel)
    {
        if (channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow ||
            channel.VideoIds is not [string videoId, ..])
        {
            return;
        }

        var video = await _videoService.GetLocalVideoAsync(videoId);
        if (video is null)
        {
            return;
        }

        await _videoService.UninstallVideoAsync(video);
    }
}
