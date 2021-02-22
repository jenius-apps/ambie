using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for managing the sync between cloud
    /// storage and local.
    /// </summary>
    public interface ISyncEngine
    {
        /// <summary>
        /// Raised when a sync is completed.
        /// </summary>
        event EventHandler? SyncCompleted;

        /// <summary>
        /// Raised when a sync is started.
        /// </summary>
        event EventHandler? SyncStarted;

        /// <summary>
        /// Uploads the current local state to the cloud.
        /// </summary>
        Task SyncUp();

        /// <summary>
        /// Downloads and loads the saved state from the cloud.
        /// </summary>
        Task SyncDown();
    }
}