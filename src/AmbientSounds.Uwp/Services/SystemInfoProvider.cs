using AmbientSounds.GamingInformation;
using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Retrieves system information.
    /// </summary>
    public class SystemInfoProvider : ISystemInfoProvider
    {
        /// <inheritdoc/>
        public bool IsFirstRun()
        {
            return SystemInformation.Instance.IsFirstRun;
        }

        /// <inheritdoc/>
        public string GetCulture()
        {
            return SystemInformation.Instance.Culture.Name;
        }

        /// <inheritdoc/>
        public bool IsDesktop()
        {
            return App.IsDesktop;
        }

        /// <inheritdoc/>
        public bool IsTenFoot()
        {
            return App.IsTenFoot;
        }

        /// <inheritdoc/>
        public unsafe bool IsXboxSeries()
        {
            if (App.IsXbox)
            {
                GAMING_DEVICE_MODEL_INFORMATION information = default;

                if (GamingDeviceInformation.GetGamingDeviceModelInformation(&information) == 0)
                {
                    return
                        information.deviceId is
                        GAMING_DEVICE_DEVICE_ID.GAMING_DEVICE_DEVICE_ID_XBOX_SERIES_S or
                        GAMING_DEVICE_DEVICE_ID.GAMING_DEVICE_DEVICE_ID_XBOX_SERIES_X or
                        GAMING_DEVICE_DEVICE_ID.GAMING_DEVICE_DEVICE_ID_XBOX_SERIES_X_DEVKIT;
                }
            }

            return false;
        }

        /// <inheritdoc/>
        public async Task<string[]> GetAvailableBackgroundsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFolder backgrounds = await assets.GetFolderAsync("Backgrounds");
            var images = await backgrounds.GetFilesAsync();
            return images.Select(static x => $"ms-appx:///Assets/Backgrounds/{x.Name}").ToArray();
        }
    }
}
