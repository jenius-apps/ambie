using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

/// <summary>
/// An interface for storing push notification data to a custom storage server.
/// </summary>
public interface IPushNotificationStorage
{
    /// <summary>
    /// Registers the given device data to the storage server.
    /// </summary>
    /// <param name="data">The data to store.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> RegisterDeviceAsync(DeviceRegistrationData data, CancellationToken ct);

    /// <summary>
    /// Deletes the device data associated with the ID.
    /// </summary>
    /// <param name="deviceId">The ID of the device's data that will be removed.</param>
    /// <param name="ct">A cancellation token.</param>
    Task DeleteDeviceRegistrationAsync(string deviceId, CancellationToken ct);
}
