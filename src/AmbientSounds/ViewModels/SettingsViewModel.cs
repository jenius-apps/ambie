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
            InitializeTheme();
        }

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
        }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for default app theme.
        /// </summary>
        public bool DefaultRadioIsChecked { get; set; }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for dark app theme.
        /// </summary>
        public bool DarkRadioIsChecked { get; set; }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for light app theme.
        /// </summary>
        public bool LightRadioIsChecked { get; set; }

        /// <summary>
        /// Event handler for RadioButton (dark theme) click event.
        /// </summary>
        public void DarkThemeRadioClicked()
        {
            _userSettings.Set(UserSettingsConstants.Theme, "dark");
        }

        /// <summary>
        /// Event handler for RadioButton (default theme) click event.
        /// </summary>
        public void DefaultThemeRadioClicked()
        {
            _userSettings.Set(UserSettingsConstants.Theme, "default");
        }

        /// <summary>
        /// Event handler for RadioButton (light theme) click event.
        /// </summary>
        public void LightThemeRadioClicked()
        {
            _userSettings.Set(UserSettingsConstants.Theme, "light");
        }

        /// <summary>
        /// Method for setting appropriate radio button on for each app theme.
        /// </summary>
        private void InitializeTheme()
        {
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
    }
}