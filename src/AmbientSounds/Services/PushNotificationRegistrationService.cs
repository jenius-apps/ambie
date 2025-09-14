using AmbientSounds.Constants;
using AmbientSounds.Models;
using JeniusApps.Common.PushNotifications;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class PushNotificationRegistrationService : IPushNotificationRegistrationService
{
    private readonly IUserSettings _userSettings;
    private readonly IPushNotificationService _pushNotificationService; // used in release mode
    private readonly string _culture; // used in release mode

    public PushNotificationRegistrationService(
        IUserSettings userSettings,
        IPushNotificationService pushNotificationService,
        ISystemInfoProvider systemInfoProvider)
    {
        _userSettings = userSettings;
        _pushNotificationService = pushNotificationService;
        _culture = systemInfoProvider.GetCulture();
    }

    /// <inheritdoc/>
    public async Task<bool> TryRegisterPushNotificationsAsync(CancellationToken cancellationToken = default)
    {
        if (!_userSettings.Get<bool>(UserSettingsConstants.Notifications) ||
            _userSettings.Get<string>(UserSettingsConstants.LocalUserIdKey) is not { Length: > 0 } id)
        {
            return false;
        }

        PremiumState lastKnownState = _userSettings.Get<string>(UserSettingsConstants.LastKnownPremiumState) is string state
            && Enum.TryParse(state, out PremiumState premiumState)
            ? premiumState
            : PremiumState.Unknown;

        if (lastKnownState is PremiumState.Unknown)
        {
            return false;
        }

        Dictionary<string, string> deviceData = new()
        {
            { nameof(PremiumState), lastKnownState.ToString() }
        };

        try
        {
#if DEBUG
            // Don't want to needlessly send messages to the notification service
            // when in debug mode.
            await Task.Delay(1);
#else
            return await _pushNotificationService.RegisterAsync(
                id,
                _culture,
                cancellationToken,
                deviceData: deviceData);
#endif
        }
        catch { }

        return false;
    }
}
