using Microsoft.Toolkit.Uwp.Helpers;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Retrieves system information.
    /// </summary>
    public class SystemInfoProvider : ISystemInfoProvider
    {
        /// <inheritdoc/>
        public string GetCulture()
        {
            return SystemInformation.Instance.Culture.Name;
        }
    }
}
