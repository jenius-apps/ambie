using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IXboxSlideshowService
{
    /// <summary>
    /// Raised when a video download was triggered.
    /// </summary>
    event EventHandler<VideoDownloadTriggeredArgs>? VideoDownloadTriggered;

    /// <summary>
    /// Retrieves data that is used for slideshows.
    /// </summary>
    /// <param name="mixMediaPlayerService">The instance of the media player.</param>
    /// <returns>The soundId and list of video IDs to use for a slideshow based on the current state of the media player.</returns>
    Task<(string SoundId, IReadOnlyList<string> AssociatedVideoIds)> GetSlideshowDataAsync(IMixMediaPlayerService mixMediaPlayerService);

    /// <summary>
    /// Determines the most appropriate slideshow mode
    /// for the given sound.
    /// </summary>
    /// <param name="soundId">The sound ID to use.</param>
    /// <param name="associatedVideoIds">Video IDs associated with the sound.</param>
    /// <returns>An enum that represents what slideshow mode to display in the UI.</returns>
    Task<SlideshowMode> GetSlideshowModeAsync(string soundId, IReadOnlyList<string> associatedVideoIds);

    /// <summary>
    /// Returns the preferred mode by the user directly from settings.
    /// </summary>
    SlideshowMode GetPreferredModeFromSettings();
}