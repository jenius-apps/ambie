using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for downloading sounds.
    /// </summary>
    public interface IDownloadManager
    {
        /// <summary>
        /// Raised when the download queue is emptied.
        /// </summary>
        event EventHandler? DownloadsCompleted;

        /// <summary>
        /// Adds sound to download queue and starts
        /// download.
        /// </summary>
        /// <param name="s">The sound to download.</param>
        /// <param name="progress">Progress of download.</param>
        Task QueueAndDownloadAsync(Sound s, IProgress<double> progress);

        /// <summary>
        /// Adds video to download queue and starts download.
        /// </summary>
        /// <returns>
        /// Destination path.
        /// </returns>
        Task<string> QueueAndDownloadAsync(Video video, IProgress<double> progress);

        /// <summary>
        /// Adds sound to download queue and starts
        /// download.
        /// </summary>
        /// <param name="s">The ID of sounds to download.</param>
        Task QueueAndDownloadAsync(IList<string> onlineSoundIds);

        /// <summary>
        /// Returns true if a download is active
        /// for the given sound.
        /// </summary>
        bool IsDownloadActive(Sound s);

        /// <summary>
        /// Returns the progress object
        /// for the given sound if one 
        /// exists. Returns null otherwise.
        /// </summary>
        IProgress<double>? GetProgress(Sound s);

        /// <summary>
        /// Returns progress object if given
        /// file path has an active download.
        /// </summary>
        IProgress<double>? GetProgress(string destinationFilePath);

        /// <summary>
        /// Queues an update to be downloaded.
        /// </summary>
        /// <param name="s">The new sound data.</param>
        /// <param name="progress">Progress of download.</param>
        /// <param name="updateDataOnly">If true, the sound file will not be redownloaded.</param>
        Task QueueUpdateAsync(
            Sound s,
            IProgress<double> progress,
            string previousImagePath,
            string previousFilePath, 
            bool updateDataOnly = false);

        /// <summary>
        /// Queues the given guide for download.
        /// </summary>
        /// <param name="guide">The guide to download.</param>
        /// <param name="progress">Progress tracker object.</param>
        /// <returns>Reeturns the destination path of the guide.</returns>
        Task<string> QueueAndDownloadAsync(Guide guide, IProgress<double> progress);
    }
}
