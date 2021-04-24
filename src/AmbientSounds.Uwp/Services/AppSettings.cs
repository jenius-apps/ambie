using Windows.ApplicationModel.Resources;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for app settings.
    /// </summary>
    public class AppSettings : IAppSettings
    {
        public AppSettings()
        {
            var resourceLoader = ResourceLoader.GetForCurrentView("appsettings");
            MySoundsUrl = resourceLoader.GetString(nameof(MySoundsUrl));
            CatalogueUrl = resourceLoader.GetString(nameof(CatalogueUrl));
            TelemetryApiKey = resourceLoader.GetString(nameof(TelemetryApiKey));
            MsaClientId = resourceLoader.GetString(nameof(MsaClientId));
            CloudSyncFileUrl = resourceLoader.GetString(nameof(CloudSyncFileUrl));
            UploadUrl = resourceLoader.GetString(nameof(UploadUrl));
            CatalogueScope = resourceLoader.GetString(nameof(CatalogueScope));
        }

        /// <inheritdoc/>
        public string MySoundsUrl { get; }

        /// <inheritdoc/>
        public string CatalogueUrl { get; }

        /// <inheritdoc/>
        public string TelemetryApiKey { get; }

        /// <inheritdoc/>
        public bool LoadPreviousState { get; set; } = true;

        /// <inheritdoc/>
        public string MsaClientId { get; set; }

        /// <inheritdoc/>
        public string CloudSyncFileUrl { get; set; }

        /// <inheritdoc/>
        public string UploadUrl { get; set; }

        /// <inheritdoc/>
        public string CatalogueScope { get; set; }
    }
}
