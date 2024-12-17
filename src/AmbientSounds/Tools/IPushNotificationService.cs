using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

/// <summary>
/// Main interface for orchestrating push notification registration.
/// </summary>
public interface IPushNotificationService
{
    /// <summary>
    /// Registers the given device for push notifications.
    /// </summary>
    /// <param name="deviceId">A unique ID representing this device. Preferably a GUID.</param>
    /// <param name="primaryLanguageCode">A two-letter ISO language code representing the user's primary language.</param>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> RegisterAsync(string deviceId, string primaryLanguageCode, CancellationToken ct);

    /// <summary>
    /// Unregisters the given device from notifications.
    /// </summary>
    /// <param name="deviceId">A unique ID representing this device. Preferably a GUID.</param>
    /// <param name="ct">A cancellation token.</param>
    Task UnregisterAsync(string deviceId, CancellationToken ct);
}