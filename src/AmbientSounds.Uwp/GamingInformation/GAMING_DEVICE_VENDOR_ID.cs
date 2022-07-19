namespace AmbientSounds.GamingInformation;

/// <summary>
/// Indicates the vendor of the console that the game is running on.
/// </summary>
/// <remarks>See <see href="https://docs.microsoft.com/windows/win32/api/gamingdeviceinformation/ne-gamingdeviceinformation-gaming_device_vendor_id"/>.</remarks>
public enum GAMING_DEVICE_VENDOR_ID
{
    /// <summary>
    /// The vendor of the device is not known.
    /// </summary>
    GAMING_DEVICE_VENDOR_ID_NONE = 0,

    /// <summary>
    /// The vendor of the device is Microsoft.
    /// </summary>
    GAMING_DEVICE_VENDOR_ID_MICROSOFT = unchecked((int)0xC2EC5032)
}
