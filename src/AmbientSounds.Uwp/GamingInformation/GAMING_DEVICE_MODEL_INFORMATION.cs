namespace AmbientSounds.GamingInformation;

/// <summary>
/// Contains information about the device that the game is running on.
/// </summary>
/// <remarks>See <see href="https://docs.microsoft.com/windows/win32/api/gamingdeviceinformation/ns-gamingdeviceinformation-gaming_device_model_information"/>.</remarks>
public struct GAMING_DEVICE_MODEL_INFORMATION
{
    /// <summary>
    /// The vendor of the device.
    /// </summary>
    public GAMING_DEVICE_VENDOR_ID vendorId;

    /// <summary>
    /// The type of device.
    /// </summary>
    public GAMING_DEVICE_DEVICE_ID deviceId;
}
