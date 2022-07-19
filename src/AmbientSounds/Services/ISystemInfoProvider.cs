using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for getting system information
    /// for the current session.
    /// </summary>
    public interface ISystemInfoProvider
    {
        /// <summary>
        /// Retrieves the culture name
        /// in en-US format.
        /// </summary>
        string GetCulture();

        /// <summary>
        /// Returns true is the current device is a Desktop.
        /// </summary>
        bool IsDesktop();

        /// <summary>
        /// Returns true is the current
        /// device is Xbox or other device
        /// optimized for a 10-foot viewing
        /// distance.
        /// </summary>
        bool IsTenFoot();

        /// <summary>
        /// Checks whether the current device is an Xbox Series S or X device.
        /// </summary>
        /// <returns>Whether the current device is an Xbox Series S or X device.</returns>
        bool IsXboxSeries();

        /// <summary>
        /// Retrieves list of paths for background images
        /// available in the device.
        /// </summary>
        Task<string[]> GetAvailableBackgroundsAsync();

        /// <summary>
        /// Returns true if the current
        /// session is the first time this app
        /// was run since being installed.
        /// </summary>
        bool IsFirstRun();
    }
}
