using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using Windows.Networking.PushNotifications;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

public sealed class WindowsPushNotificationRegistrar : IPushNotificationRegistrar
{
    private readonly IUserSettings _userSettings;
    private readonly ITelemetry _telemetry;

    public WindowsPushNotificationRegistrar(
     IUserSettings userSettings,
     ITelemetry telemetry)
    {
        _userSettings = userSettings;
        _telemetry = telemetry;
    }

    public string? ChannelUri { get; private set; }

    public async Task<bool> RegisterAsync()
    {
        PushNotificationChannel? channel = await PushNotificationChannelManager.CreatePushNotificationChannelForApplicationAsync();
        if (channel is not { Uri.Length: > 0 })
        {
            return false;
        }

        // upload channel URI to service.
        ChannelUri = channel.Uri;
        return true;
    }

    public async Task<bool> TryRegisterBasedOnUserSettingsAsync()
    {
        if (_userSettings.Get<bool>(UserSettingsConstants.Notifications))
        {
            return await RegisterAsync();
        }

        return false;
    }

    public Task UnregiserAsync()
    {
        return Task.CompletedTask;
    }
}
