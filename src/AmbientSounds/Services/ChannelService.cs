using AmbientSounds.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ChannelService : IChannelService
{
    private readonly ISoundService _soundService;
    private readonly IVideoService _videoService;
    private readonly IIapService _iapService;
    private readonly IDownloadManager _downloadManager;
    private readonly ICatalogueService _cataloqueService;
    private readonly ConcurrentDictionary<string, double> _activeVideoDownloadProgress = new();
    private readonly ConcurrentDictionary<string, double> _activeSoundDownloadProgress = new();

    public ChannelService(
        ISoundService soundService,
        IVideoService videoService,
        IIapService iapService,
        IDownloadManager downloadManager,
        ICatalogueService catalogueService)
    {
        _soundService = soundService;
        _videoService = videoService;
        _iapService = iapService;
        _downloadManager = downloadManager;
        _cataloqueService = catalogueService;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Channel>> GetChannelsAsync()
    {
        await Task.Delay(1);

        return
        [
            new Channel
            {
                Type = ChannelType.DarkScreen,
                ImagePath = "https://getwallpapers.com/wallpaper/full/3/f/f/6072.jpg",
                Localizations = new Dictionary<string, DisplayInformation>()
                {
                    { "en", new DisplayInformation { Name = "Dark screen", Description = "A dark screen that is ideal for sleeping or low stimulus." } }
                },
            },
            new Channel
            {
                Type = ChannelType.Slideshow,
                ImagePath = "https://images.unsplash.com/photo-1531845116688-48819b3b68d9?q=80&w=640&auto=format&fit=crop&ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D",
                Localizations = new Dictionary<string, DisplayInformation>()
                {
                    { "en", new DisplayInformation { Name = "Slideshow", Description = "An animated carousel of images based on the actively playing sounds." } }
                },
            },
            new Channel
            {
                Type = ChannelType.Videos,
                ImagePath = "https://th.bing.com/th/id/R.af2a4af63bd0e46d1d57ea3b5bf5a822?rik=pLtrY6F8U2760g&riu=http%3a%2f%2fsuperiorclay.com%2fwp-content%2fuploads%2f2014%2f05%2fStandard-Fireplace.jpg&ehk=Nvs9OBUfk5ewZ2h7ysFBZyGW6IBZFy2RY%2fRxKhbn5T0%3d&risl=1&pid=ImgRaw&r=0",
                Localizations = new Dictionary<string, DisplayInformation>()
                {
                    { "en", new DisplayInformation { Name = "Fireplace", Description = "Chestnuts roasting in an open fire." } }
                },
                VideoIds = ["59c3b21c-3df1-44d0-a2f7-096bf55728c3"],
                SoundIds = ["b22901eb-2269-4b3f-80ab-af2722e68ff1"],
                IapIds = ["ambieplus"]
            },
        ];
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
            var sounds = await _cataloqueService.GetSoundsAsync([soundId]);
            var soundToDownload = sounds.Count > 0 ? sounds[0] : null;

            if (soundToDownload is not null && !_activeSoundDownloadProgress.ContainsKey(channel.Id))
            {
                _activeSoundDownloadProgress[channel.Id] = 0;
                var soundProgress = new Progress<double>();
                soundProgress.ProgressChanged += OnSoundProgressChanged;
                await _downloadManager.QueueAndDownloadAsync(soundToDownload, soundProgress);
                isSoundQueued = true;

                void OnSoundProgressChanged(object sender, double e)
                {
                    _activeSoundDownloadProgress[channel.Id] = e / 2;
                    var sum = _activeVideoDownloadProgress[channel.Id] + _activeSoundDownloadProgress[channel.Id];
                    channelProgress.Report(sum);

                    if (sum >= 100)
                    {
                        _activeSoundDownloadProgress.TryRemove(channel.Id, out _);
                        _activeVideoDownloadProgress.TryRemove(channel.Id, out _);
                    }
                }
            }
        }

        if (!isVideoInstalled)
        {
            var onlineVideos = await _videoService.GetVideosAsync(includeOffline: false);
            var videoToDownload = onlineVideos.FirstOrDefault(x => x.Id == videoId);

            if (videoToDownload is not null && !_activeVideoDownloadProgress.ContainsKey(channel.Id))
            {
                _activeVideoDownloadProgress[channel.Id] = 0;
                var videoProgress = new Progress<double>();
                videoProgress.ProgressChanged += OnVideoProgressChanged;
                await _videoService.InstallVideoAsync(videoToDownload, videoProgress);
                isVideoQueued = true;

                void OnVideoProgressChanged(object sender, double e)
                {
                    _activeVideoDownloadProgress[channel.Id] = e / 2;
                    var sum = _activeVideoDownloadProgress[channel.Id] + _activeSoundDownloadProgress[channel.Id];
                    channelProgress.Report(sum);

                    if (sum >= 100)
                    {
                        _activeSoundDownloadProgress.TryRemove(channel.Id, out _);
                        _activeVideoDownloadProgress.TryRemove(channel.Id, out _);
                    }
                }
            }
        }

        return isSoundQueued || isVideoQueued;
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
}
