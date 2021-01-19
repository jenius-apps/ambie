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
        }

        /// <inheritdoc/>
        public string CatalogueUrl { get; }

        /// <inheritdoc/>
        public string TelemetryApiKey { get; }
    }
}
