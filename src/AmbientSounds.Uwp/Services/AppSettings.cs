using Windows.ApplicationModel.Resources;

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
            CatalogueUrl = resourceLoader.GetString(nameof(CatalogueUrl));
            TelemetryApiKey = resourceLoader.GetString(nameof(TelemetryApiKey));
            MsaClientId = resourceLoader.GetString(nameof(MsaClientId));
            CloudSyncFileUrl = resourceLoader.GetString(nameof(CloudSyncFileUrl));
            UploadUrl = resourceLoader.GetString(nameof(UploadUrl));
        }

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
    }
}
