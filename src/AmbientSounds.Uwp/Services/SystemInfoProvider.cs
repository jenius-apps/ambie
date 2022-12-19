using Microsoft.Toolkit.Uwp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.ViewManagement;

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
        public async Task<string[]> GetAvailableBackgroundsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFolder backgrounds = await assets.GetFolderAsync("Backgrounds");
            var images = await backgrounds.GetFilesAsync();
            return images.Select(static x => $"ms-appx:///Assets/Backgrounds/{x.Name}").ToArray();
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<string>> GetAvailableSoundEffectsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFolder soundEffects = await assets.GetFolderAsync("SoundEffects");
            var sounds = await soundEffects.GetFilesAsync();
            return sounds.Select(static x => $"ms-appx:///Assets/SoundEffects/{x.Name}").ToArray();
        }

        /// <inheritdoc/>
        public bool IsCompact()
        {
            return ApplicationView.GetForCurrentView().ViewMode == ApplicationViewMode.CompactOverlay;
        }

        /// <inheritdoc/>
        public bool CanUseFluentSystemIcons()
        {
            var result = SystemInformation.Instance.OperatingSystemVersion;
            return result.Build >= 22000;
        }
    }
}
