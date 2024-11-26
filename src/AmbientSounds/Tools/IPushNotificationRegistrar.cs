using System.Threading.Tasks;

namespace AmbientSounds.Tools;

/// <summary>
/// Interface for registering or unregistering
/// for store-based pushed notifications.
/// </summary>
public interface IPushNotificationRegistrar
{
    /// <summary>
    /// Registers for push notifications.
    /// </summary>
    Task<bool> RegisterAsync();
    
    /// <summary>
    /// Tries to register if the user has telemetry enabled.
    /// </summary>
    Task<bool> TryRegisterBasedOnUserSettingsAsync();

    /// <summary>
    /// Unregisters for push notifications.
    /// </summary>
    Task UnregiserAsync();
}
