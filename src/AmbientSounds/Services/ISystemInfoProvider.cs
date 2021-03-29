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
        /// Returns true is the current
        /// device is Xbox or other device
        /// optimized for a 10-foot viewing
        /// distance.
        /// </summary>
        bool IsTenFoot();

        /// <summary>
        /// Retrieves list of paths for background images
        /// available in the device.
        /// </summary>
        Task<string[]> GetAvailableBackgroundsAsync();
    }
}
