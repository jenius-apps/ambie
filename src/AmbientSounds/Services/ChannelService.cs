﻿using AmbientSounds.Cache;
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
    public async Task<IReadOnlyDictionary<string, Channel>> GetChannelsAsync()
    {
        return await _channelCache.GetItemsAsync();
    }

    public async Task<bool> QueueInstallChannelAsync(Channel channel, Progress<double>? progress = null)
    {
        if (channel is not { Type: ChannelType.Videos, VideoIds: [string videoId, ..], SoundIds: [string soundId, ..] })
        {
            return false;
        }

        var isSoundInstalled = await _soundService.IsSoundInstalledAsync(soundId);
        var isVideoInstalled = await _videoService.IsVideoInstalledAsync(videoId);

        if (isSoundInstalled && isVideoInstalled)
        {
            return false;
        }

        bool isSoundQueued = false;
        bool isVideoQueued = false;
        IProgress<double> channelProgress = progress ?? new Progress<double>();

        if (!isSoundInstalled)
        {
            var sounds = await _catalogueService.GetSoundsAsync([soundId]);
            var soundToDownload = sounds.Count > 0 ? sounds[0] : null;

            if (soundToDownload is not null && !_activeSoundDownloadProgress.ContainsKey(soundId))
            {
                _activeSoundDownloadProgress[soundId] = 0;
                var soundProgress = new Progress<double>();
                soundProgress.ProgressChanged += OnSoundProgressChanged;
                await _downloadManager.QueueAndDownloadAsync(soundToDownload, soundProgress);
                isSoundQueued = true;

                void OnSoundProgressChanged(object sender, double e)
                {
                    var sum = OnAssetProgressChanged(
                        soundId,
                        videoId,
                        _activeSoundDownloadProgress,
                        _activeVideoDownloadProgress,
                        e,
                        channelProgress);

                    if (sum >= 100)
                    {
                        ChannelDownloaded?.Invoke(this, channel.Id);
                    }
                }
            }
        }

        if (!isVideoInstalled)
        {
            var onlineVideos = await _videoService.GetVideosAsync(includeOffline: false);
            var videoToDownload = onlineVideos.FirstOrDefault(x => x.Id == videoId);

            if (videoToDownload is not null && !_activeVideoDownloadProgress.ContainsKey(videoId))
            {
                _activeVideoDownloadProgress[videoId] = 0;
                var videoProgress = new Progress<double>();
                videoProgress.ProgressChanged += OnVideoProgressChanged;
                await _videoService.InstallVideoAsync(videoToDownload, videoProgress);
                isVideoQueued = true;

                void OnVideoProgressChanged(object sender, double e)
                {
                    var sum = OnAssetProgressChanged(
                        videoId,
                        soundId,
                        _activeVideoDownloadProgress,
                        _activeSoundDownloadProgress, 
                        e,
                        channelProgress);

                }
            }
        }

        return isSoundQueued || isVideoQueued;
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
            if (channel.SoundIds is not [string soundId, ..] || channel.VideoIds.Count == 0)
            {
                return;
            }

            if (await _soundService.GetLocalSoundAsync(soundId) is not Sound s)
            {
                return;
            }

            await _player.PlayFeaturedSoundAsync(FeaturedSoundType.Channel, s.Id, s.FilePath, enableGaplessLoop: true);
        }
        else if (channel.Type is ChannelType.DarkScreen or ChannelType.Slideshow)
        {
            if (_player.GetSoundIds().Length == 0)
            {
                await _player.AddRandomAsync();
            }
            else
            {
                _player.Play();
            }
        }

        if (performNavigation)
        {
            var args = new ScreensaverArgs()
            {
                RequestedType = channel.Type,
                VideoId = channel is { Type: ChannelType.Videos, VideoIds: [string videoId, ..] } ? videoId : null
            };

            _navigator.ToScreensaver(args);
        }
    }
}