using System.Runtime.InteropServices;

namespace AmbientSounds.GamingInformation;

/// <summary>
/// APIs form the <c>GamingDeviceInformation.h</c> header.
/// </summary>
/// <remarks>See <see href="https://docs.microsoft.com/windows/win32/api/_gamingdvcinfo/"/>.</remarks>
public static class GamingDeviceInformation
{
    /// <summary>
    /// Gets information about the device that the game is running on.
    /// </summary>
    /// <param name="information">A structure containing information about the device that the game is running on.</param>
    /// <returns>An <c>HRESULT</c> for the operation.</returns>
    /// <remarks>See <see href="https://docs.microsoft.com/windows/win32/api/gamingdeviceinformation/nf-gamingdeviceinformation-getgamingdevicemodelinformation"/>.</remarks>
    [DllImport("api-ms-win-gaming-deviceinformation-l1-1-0", ExactSpelling = true, CallingConvention = CallingConvention.StdCall)]
    public static extern unsafe int GetGamingDeviceModelInformation(GAMING_DEVICE_MODEL_INFORMATION* information);
}
