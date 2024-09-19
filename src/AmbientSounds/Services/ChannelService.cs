using AmbientSounds.Cache;
using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ChannelService : IChannelService
{
    private readonly ISoundCache _soundCache;

    public ChannelService(ISoundCache soundCache)
    {
        _soundCache = soundCache;
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
            }
        ];
    }
}
