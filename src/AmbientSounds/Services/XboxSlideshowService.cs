using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Models;
using JeniusApps.Common.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class XboxSlideshowService : IXboxSlideshowService
{
    private readonly IUserSettings _userSettings;
    private readonly IVideoService _videoService;
    private readonly IIapService _iapService;
    private readonly ISoundService _soundService;

    /// <inheritdoc/>
    public event EventHandler<VideoDownloadTriggeredArgs>? VideoDownloadTriggered;

    public XboxSlideshowService(
        IUserSettings userSettings,
        IVideoService videoService,
        IIapService iapService,
        ISoundService soundService)
    {
        _userSettings = userSettings;
        _videoService = videoService;
        _iapService = iapService;
        _soundService = soundService;
    }

    /// <inheritdoc/>
    public async Task<(string SoundId, IReadOnlyList<string> AssociatedVideoIds)> GetSlideshowDataAsync(IMixMediaPlayerService mixMediaPlayerService)
    {
        string? soundIdToUse = mixMediaPlayerService.GetSoundIds(false).FirstOrDefault();
        if (soundIdToUse is not { Length: > 0 })
        {
            return (string.Empty, []);
        }

        Sound? sound = await _soundService.GetLocalSoundAsync(soundIdToUse);
        return (sound?.Id ?? string.Empty, sound?.AssociatedVideoIds ?? []);
    }

    /// <inheritdoc/>
    public async Task<SlideshowMode> GetSlideshowModeAsync(string soundId, IReadOnlyList<string> associatedVideoIds)
    {
        var preferredMode = GetPreferredModeFromSettings();
        if (preferredMode is not SlideshowMode.Video)
        {
            return preferredMode;
        }

        return await TryValidateAndLoadVideoAsync(soundId, associatedVideoIds);
    }

    /// <inheritdoc/>
    public SlideshowMode GetPreferredModeFromSettings()
    {
        return Enum.TryParse(_userSettings.Get<string>(UserSettingsConstants.XboxSlideshowModeKey), out SlideshowMode mode)
            ? mode
            : SlideshowMode.Images;
    }

    private async Task TryInstallAsync(string videoId, string soundId)
    {
        var videos = await _videoService.GetVideosAsync(includeOnline: true, includeOffline: false).ConfigureAwait(false);
        var videoToDownload = videos.FirstOrDefault(x => x.Id == videoId);

        if (videoToDownload is null || !await _iapService.IsAnyOwnedAsync(videoToDownload.IapIds).ConfigureAwait(false))
        {
            return;
        }

        Progress<double> progress = new();
        await _videoService.InstallVideoAsync(videoToDownload, progress).ConfigureAwait(false);
        VideoDownloadTriggered?.Invoke(this, new VideoDownloadTriggeredArgs
        {
            Progress = progress,
            VideoId = videoId,
            SoundId = soundId
        });
    }

    private async Task<SlideshowMode> TryValidateAndLoadVideoAsync(
        string soundId,
        IReadOnlyList<string> associatedVideoIds)
    {
        if (associatedVideoIds is not [{ Length: > 0 } videoId, ..])
        {
            return SlideshowMode.Images;
        }

        if (await _videoService.GetLocalVideoAsync(videoId).ConfigureAwait(false) is not { } video)
        {
            _ = TryInstallAsync(videoId, soundId).ConfigureAwait(false);
            return SlideshowMode.Images;
        }

        if (await _iapService.IsAnyOwnedAsync(video.IapIds).ConfigureAwait(false))
        {
            return SlideshowMode.Video;
        }

        return SlideshowMode.Images;
    }
}
