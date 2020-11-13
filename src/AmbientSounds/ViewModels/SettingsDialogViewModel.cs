using AmbientSounds.Services;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model class for settings dialog.
    /// </summary>
    public class SettingsDialogViewModel
    {
        private readonly IUserSettings _userSettings;

        private readonly string _setting;

        public SettingsDialogViewModel(IUserSettings userSettings)
        {
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