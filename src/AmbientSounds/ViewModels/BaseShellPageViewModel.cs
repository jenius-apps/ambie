using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using JeniusApps.Common.Settings;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public abstract class BaseShellPageViewModel : ObservableObject
{
    protected readonly IUserSettings _userSettings;
    protected readonly IPushNotificationRegistrationService _pushService;

    public BaseShellPageViewModel(
        IUserSettings userSettings,
        IPushNotificationRegistrationService pushService)
    {
        _userSettings = userSettings;
        _pushService = pushService;
    }

    protected async Task UpdateLastKnownPremiumStateAsync(bool premiumButtonVisible)
    {
        // Note: This logic is performed in the shell page because it's the earliest
        // class that figures out the user's premium state. So we are piggy-backing off
        // this check and updating the last known premium state from it.

        // Update last known premium state
        if (_userSettings.Get<string>(UserSettingsConstants.LastKnownPremiumState) is string state
            && Enum.TryParse(state, out PremiumState lastKnownState))
        {
            if (lastKnownState is PremiumState.Unknown
                || (lastKnownState is PremiumState.Free && !premiumButtonVisible)
                || (lastKnownState is PremiumState.Premium && premiumButtonVisible))
            {
                _userSettings.Set(UserSettingsConstants.LastKnownPremiumState, premiumButtonVisible
                    ? PremiumState.Free.ToString()
                    : PremiumState.Premium.ToString());

                // Re-register push notification since the state has changed.
                _ = await _pushService.TryRegisterPushNotificationsAsync().ConfigureAwait(false);
            }
        }
    }
}
