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
            return SystemInformation.Culture.Name;
        }

        /// <inheritdoc/>
        public bool IsTenFoot()
        {
            return App.IsTenFoot;
        }
    }
}
