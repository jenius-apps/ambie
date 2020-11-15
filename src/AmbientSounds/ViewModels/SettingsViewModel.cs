using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;

namespace AmbientSounds.ViewModels
{
    public class SettingsViewModel
    {
        private readonly IUserSettings _userSettings;

        public SettingsViewModel(IUserSettings userSettings)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _userSettings = userSettings;
        }

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
        }
    }
}
