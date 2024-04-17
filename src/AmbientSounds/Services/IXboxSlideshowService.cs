using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IXboxSlideshowService
    {
        /// <summary>
        /// Raised when a video download was triggered.
        /// </summary>
        event EventHandler<VideoDownloadTriggeredArgs>? VideoDownloadTriggered;

        /// <summary>
        /// Determines the most appropriate slideshow mode
        /// for the given sound.
        /// </summary>
        /// <param name="soundId">The sound ID to use.</param>
        /// <param name="associatedVideoIds">Video IDs associated with the sound.</param>
        /// <returns>An enum that represents what slideshow mode to display in the UI.</returns>
        Task<SlideshowMode> GetSlideshowModeAsync(string soundId, IReadOnlyList<string> associatedVideoIds);
    }
}