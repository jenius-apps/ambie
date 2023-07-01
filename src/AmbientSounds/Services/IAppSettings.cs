namespace AmbientSounds.Services;

/// <summary>
/// Interface for app setting properties.
/// </summary>
public interface IAppSettings
{
    /// <summary>
    /// The URL to access the user's uploaded sounds.
    /// </summary>
    string MySoundsUrl { get; }

    /// <summary>
    /// URL to access the catalogue.
    /// </summary>
    string CatalogueUrl { get; }

    /// <summary>
    /// API key for telemetry.
    /// </summary>
    string TelemetryApiKey { get; }

    /// <summary>
    /// Determines if previous state should be loaded.
    /// Default true.
    /// </summary>
    bool LoadPreviousState { get; set; }

    /// <summary>
    /// The Client ID used for logging into a Microsoft Account.
    /// </summary>
    string MsaClientId { get; set; }

    /// <summary>
    /// The URL for the cloud sync file.
    /// </summary>
    string CloudSyncFileUrl { get; set; }

    /// <summary>
    /// The URL for uploading a sound file.
    /// </summary>
    string UploadUrl { get; set; }

    /// <summary>
    /// URL to access the videos list.
    /// </summary>
    string VideosUrl { get; set; }

    /// <summary>
    /// URL to access the guides list.
    /// </summary>
    string GuidesUrl { get; set; }

    /// <summary>
    /// URL to access the share endpoint.
    /// </summary>
    string ShareUrl { get; set; }

    /// <summary>
    /// The API scope for catalogue access.
    /// </summary>
    string CatalogueScope { get; set; }

    /// <summary>
    /// The URL to enable presence features.
    /// </summary>
    string PresenceUrl { get; set; }

    /// <summary>
    /// The URL to access pages data.
    /// </summary>
    string PagesUrl { get; set; }
}
