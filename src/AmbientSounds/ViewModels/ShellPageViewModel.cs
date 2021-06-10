using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// ViewModel for the shell page.
    /// </summary>
    public class ShellPageViewModel : ObservableObject
    {
        private readonly IUserSettings _userSettings;

        public ShellPageViewModel(
            IUserSettings userSettings)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _userSettings = userSettings;
            _userSettings.SettingSet += OnSettingSet;
        }

        /// <summary>
        /// Path to background image.
        /// </summary>
        public string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

        /// <summary>
        /// Determines if the background image should be shown.
        /// </summary>
        public bool ShowBackgroundImage => !string.IsNullOrWhiteSpace(BackgroundImagePath);

        public void Dispose()
        {
            _userSettings.SettingSet -= OnSettingSet;
        }

        private void OnSettingSet(object sender, string settingsKey)
        {
            if (settingsKey == UserSettingsConstants.BackgroundImage)
            {
                OnPropertyChanged(nameof(ShowBackgroundImage));
                OnPropertyChanged(nameof(BackgroundImagePath));
            }
        }
    }
}
