namespace AmbientSounds.Tools;

/// <summary>
/// Data to be included when registering a device for push notifications.
/// </summary>
public class DeviceRegistrationData
{
    /// <summary>
    /// The action that the server will try to fulfill.
    /// </summary>
    public required string ActionRequested { get; init; }

    /// <summary>
    /// The push notification URI retrieved from the platform.
    /// </summary>
    public required string Uri { get; init; }

    /// <summary>
    /// A unique ID associated with the device. Preferably a GUID.
    /// </summary>
    public required string DeviceId { get; init; }

    /// <summary>
    /// A two-letter ISO language code or fully formatted code for the user's primary language.
    /// E.g. zh, zh-CN, or zh-Hant are all valid.
    /// </summary>
    public required string PrimaryLanguageCode { get; init; }
}
