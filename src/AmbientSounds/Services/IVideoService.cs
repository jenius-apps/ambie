using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for retrieving videos.
    /// </summary>
    public interface IVideoService
    {
        /// <summary>
        /// Raised when a video has completed downloading.
        /// The string is the video ID.
        /// </summary>
        event EventHandler<string>? VideoDownloaded;

        /// <summary>
        /// Raised when a video was deleted.
        /// The string is the video ID.
        /// </summary>
        public event EventHandler<string>? VideoDeleted;

        /// <summary>
        /// Retrieves the download URL for the given video.
        /// </summary>
        Task<string> GetDownloadUrlAsync(string videoId);

        /// <summary>
        /// Gets local file path for given video.
        /// Returns empty string if no local path found.
        /// </summary>
        /// <param name="videoId">The <see cref="Video.Id"/> to use.</param>
        /// <returns>Path to video or empty string if no local path found.</returns>
        Task<string> GetFilePathAsync(string? videoId);

        /// <summary>
        /// Retrieves the progress object for the video's active installation.
        /// Returns null of the video doesn't have an active installation.
        /// </summary>
        Progress<double>? GetInstallProgress(Video video);

        /// <summary>
        /// Retrieves metadata for given local video.
        /// </summary>
        Task<Video?> GetLocalVideoAsync(string videoId);

        /// <summary>
        /// Retrieves list of videos that can be displayed
        /// in a list on-screen. 
        /// </summary>
        Task<IReadOnlyList<Video>> GetVideosAsync(
            bool includeOnline = true,
            bool includeOffline = true);

        /// <summary>
        /// Queues the video for downloading
        /// and stores the video metadata to local storage.
        /// </summary>
        Task InstallVideoAsync(Video video, Progress<double>? progress = null);

        /// <summary>
        /// Removes the video file from local storage.
        /// </summary>
        Task UninstallVideoAsync(Video video);
    }
}