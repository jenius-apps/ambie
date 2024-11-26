using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Microsoft.WindowsAzure.Messaging;
using System;
using System.Threading.Tasks;
using Windows.Networking.PushNotifications;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

public sealed class AzurePushNotificationRegistrar : IPushNotificationRegistrar
{
    private readonly IUserSettings _userSettings;
    private readonly ITelemetry _telemetry;
    private readonly string _notificationHubName;
    private readonly string _notificationHubConnectionString;

    public AzurePushNotificationRegistrar(
        IUserSettings userSettings,
        ITelemetry telemetry,
        IAppSettings appSettings)
    {
        _userSettings = userSettings;
        _telemetry = telemetry;
        _notificationHubConnectionString = appSettings.NotificationHubConnectionString;
        _notificationHubName = appSettings.NotificationHubName;
    }

    /// <inheritdoc/>
    public async Task<bool> TryRegisterBasedOnUserSettingsAsync()
    {
        if (_userSettings.Get<bool>(UserSettingsConstants.Notifications))
        {
            return await RegisterAsync();
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task<bool> RegisterAsync()
    {
        try
        {
            var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
            var hub = new NotificationHub(_notificationHubName, _notificationHubConnectionString);
            Registration result = await hub.RegisterNativeAsync(channel.Uri);
            return result?.RegistrationId is { Length: > 0 };
        }
        catch (Exception e)
        {
            _telemetry.TrackError(e);
        }

        return false;
    }

    /// <inheritdoc/>
    public async Task UnregiserAsync()
    {
        var channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
        var hub = new NotificationHub(_notificationHubName, _notificationHubConnectionString);
        await hub.UnregisterAllAsync(channel.Uri);
    }
}
