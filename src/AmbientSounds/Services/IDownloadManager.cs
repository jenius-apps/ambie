using AmbientSounds.Models;
using System;
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
    }
}
