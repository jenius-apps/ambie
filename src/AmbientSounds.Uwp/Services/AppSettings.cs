using Windows.ApplicationModel.Resources;

#nullable enable

namespace AmbientSounds.Services.Uwp;

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
        VideosUrl = resourceLoader.GetString(nameof(VideosUrl));
        ShareUrl = resourceLoader.GetString(nameof(ShareUrl));
        CatalogueScope = resourceLoader.GetString(nameof(CatalogueScope));
        PresenceUrl = resourceLoader.GetString(nameof(PresenceUrl));
        PagesUrl = resourceLoader.GetString(nameof(PagesUrl));
        GuidesUrl = resourceLoader.GetString(nameof(GuidesUrl));
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
    public string VideosUrl { get; set; }

    /// <inheritdoc/>
    public string ShareUrl { get; set; }

    /// <inheritdoc/>
    public string CatalogueScope { get; set; }

    /// <inheritdoc/>
    public string PresenceUrl { get; set; }

    /// <inheritdoc/>
    public string PagesUrl { get; set; }

    /// <inheritdoc/>
    public string GuidesUrl { get; set; }
}
