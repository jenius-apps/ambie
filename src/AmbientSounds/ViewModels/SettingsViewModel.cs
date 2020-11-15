using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;

namespace AmbientSounds.ViewModels
{
    public class SettingsViewModel
    {
        private readonly IUserSettings _userSettings;

        private readonly string _setting;

        public SettingsViewModel(IUserSettings userSettings)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _userSettings = userSettings;
            _setting = _userSettings.Get<string>("themeSetting");
            if (_setting != null)
            {
                switch (_setting)
                {
                    case "default":
                        DefaultRadioIsChecked = true;
                        break;
                    case "light":
                        LightRadioIsChecked = true;
                        break;
                    case "dark":
                        DarkRadioIsChecked = true;
                        break;
                    default:
                        break;
                }
            }
        }

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
        }

        public bool DefaultRadioIsChecked { get; set; }

        public bool DarkRadioIsChecked { get; set; }

        public bool LightRadioIsChecked { get; set; }

        public void DarkThemeRadioClicked()
        {
            _userSettings.Set("themeSetting", "dark");
        }

        public void DefaultThemeRadioClicked()
        {
            _userSettings.Set("themeSetting", "default");
        }

        public void LightThemeRadioClicked()
        {
            _userSettings.Set("themeSetting", "light");
        }
    }
}