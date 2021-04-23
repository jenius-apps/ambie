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
        public string GetCulture()
        {
            return SystemInformation.Instance.Culture.Name;
        }

        /// <inheritdoc/>
        public bool IsTenFoot()
        {
            return App.IsTenFoot;
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
