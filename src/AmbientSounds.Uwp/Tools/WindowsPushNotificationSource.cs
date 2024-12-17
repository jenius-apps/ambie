using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

public sealed class WindowsPushNotificationSource : IPushNotificationSource
{
    /// <inheritdoc/>
    public async Task<string?> GetNotificationUriAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        PushNotificationChannel? channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
        return channel?.Uri;
    }

    /// <inheritdoc/>
    public Task UnregisterAsync(CancellationToken ct)
    {
        // There's nothing to unregister when it comes to the WNS platform.
        // Unregistration happens by deleting the device data from your own custom storage server.
        return Task.CompletedTask;
    }
}
