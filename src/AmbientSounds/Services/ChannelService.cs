using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ChannelService : IChannelService
{
    private readonly ISoundService _soundService;
    private readonly IVideoService _videoService;
    private readonly IIapService _iapService;

    public ChannelService(
        ISoundService soundService,
        IVideoService videoService,
        IIapService iapService)
    {
        _soundService = soundService;
        _videoService = videoService;
        _iapService = iapService;
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
            },
        ];
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
        Video? video = await _videoService.GetLocalVideoAsync(videoId);
        return isSoundInstalled && video is not null;
    }

    /// <inheritdoc/>
    public async Task<bool> IsOwnedAsync(Channel channel)
    {
        var isOwned = await _iapService.IsAnyOwnedAsync(channel.IapIds);
        return isOwned;
    }
}
