using AmbientSounds.Constants;
using AmbientSounds.Models;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class XboxSlideshowService : IXboxSlideshowService
{
    private readonly IUserSettings _userSettings;
    private readonly IVideoService _videoService;
    private readonly IIapService _iapService;

    public XboxSlideshowService(
        IUserSettings userSettings,
        IVideoService videoService,
        IIapService iapService)
    {
        _userSettings = userSettings;
        _videoService = videoService;
        _iapService = iapService;
    }

    /// <inheritdoc/>
    public async Task<SlideshowMode> GetSlideshowModeAsync(Sound sound)
    {
        // Retrieve the most appropriate mode for the given sound.

        var preferredMode = Enum.TryParse<SlideshowMode>(_userSettings.Get<string>(UserSettingsConstants.XboxSlideshowModeKey), out var mode)
            ? mode
            : SlideshowMode.Images;

        if (preferredMode is not SlideshowMode.Video)
        {
            return preferredMode;
        }

        if (sound.AssociatedVideoIds is not [{ Length: > 0 } videoId, ..])
        {
            return SlideshowMode.Images;
        }

        if (await _videoService.GetLocalVideoAsync(videoId) is not { } video)
        {
            return SlideshowMode.Images;
        }

        if (await _iapService.IsAnyOwnedAsync(video.IapIds))
        {
            return SlideshowMode.Video;
        }

        return SlideshowMode.Images;
    }
}
