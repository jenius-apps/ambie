namespace AmbientSounds.GamingInformation;

/// <summary>
/// Indicates the type of device that the game is running on.
/// </summary>
/// <remarks>See <see href="https://docs.microsoft.com/windows/win32/api/gamingdeviceinformation/ne-gamingdeviceinformation-gaming_device_device_id"/>.</remarks>
public enum GAMING_DEVICE_DEVICE_ID
{
    /// <summary>
    /// The device is not in the Xbox family.
    /// </summary>
    GAMING_DEVICE_DEVICE_ID_NONE = 0,

    /// <summary>
    /// The device is an Xbox One (original).
    /// </summary>
    GAMING_DEVICE_DEVICE_ID_XBOX_ONE = 0x768BAE26,

    /// <summary>
    /// The device is an Xbox One S.
    /// </summary>
    GAMING_DEVICE_DEVICE_ID_XBOX_ONE_S = 0x2A7361D9,

    /// <summary>
    /// The device is an Xbox One X.
    /// </summary>
    GAMING_DEVICE_DEVICE_ID_XBOX_ONE_X = 0x5AD617C7,

    /// <summary>
    /// The device is an Xbox One X dev kit.
    /// </summary>
    GAMING_DEVICE_DEVICE_ID_XBOX_ONE_X_DEVKIT = 0x10F7CDE3
}
