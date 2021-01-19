namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for app setting properties.
    /// </summary>
    public interface IAppSettings
    {
        /// <summary>
        /// URL to access the catalogue.
        /// </summary>
        string CatalogueUrl { get; }

        /// <summary>
        /// API key for telemetry.
        /// </summary>
        string TelemetryApiKey { get; }
    }
}
