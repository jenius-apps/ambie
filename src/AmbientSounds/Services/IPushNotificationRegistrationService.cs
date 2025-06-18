using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Interface for registering push notifications.
/// </summary>
public interface IPushNotificationRegistrationService
{
    /// <summary>
    /// Registers device for push notifications if the user allows it.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token.</param>
    /// <returns>True if successful, false otherwise.</returns>
    Task<bool> TryRegisterPushNotificationsAsync(CancellationToken cancellationToken = default);
}