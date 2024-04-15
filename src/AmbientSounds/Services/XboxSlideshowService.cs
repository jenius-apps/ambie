using AmbientSounds.Constants;
using AmbientSounds.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class XboxSlideshowService : IXboxSlideshowService
{
    private readonly IUserSettings _userSettings;
    private readonly IVideoService _videoService;
    private readonly IIapService _iapService;

    /// <inheritdoc/>
    public event EventHandler<Progress<double>>? VideoDownloadTriggered;

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

        if (await _videoService.GetLocalVideoAsync(videoId).ConfigureAwait(false) is not { } video)
        {
            _ = TryInstallAsync(videoId).ConfigureAwait(false);
            return SlideshowMode.Images;
        }

        if (await _iapService.IsAnyOwnedAsync(video.IapIds).ConfigureAwait(false))
        {
            return SlideshowMode.Video;
        }

        return SlideshowMode.Images;
    }

    private async Task TryInstallAsync(string videoId)
    {
        var videos = await _videoService.GetVideosAsync(includeOnline: true, includeOffline: false).ConfigureAwait(false);
        var videoToDownload = videos.FirstOrDefault(x => x.Id == videoId);

        if (videoToDownload is null || !await _iapService.IsAnyOwnedAsync(videoToDownload.IapIds).ConfigureAwait(false))
        {
            return;
        }

        Progress<double> progress = new();
        await _videoService.InstallVideoAsync(videoToDownload, progress).ConfigureAwait(false);
        VideoDownloadTriggered?.Invoke(this, progress);
    }
}
