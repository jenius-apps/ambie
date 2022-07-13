using AmbientSounds.Constants;
using AmbientSounds.Controls;
using AmbientSounds.Converters;
using AmbientSounds.ViewModels;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for opening dialogs.
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly IUserSettings _userSettings;

        public DialogService(IUserSettings userSettings)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));

            _userSettings = userSettings;
        }

        /// <summary>
        /// UWP apps crash if more than one content dialog
        /// is opened at the same time. This flag
        /// will be used to help ensure only one
        /// dialog is open at a time.
        /// </summary>
        public static bool IsDialogOpen;

        /// <inheritdoc/>
        public async Task<bool> MissingSoundsDialogAsync()
        {
            if (IsDialogOpen)
            {
                return false;
            }

            IsDialogOpen = true;
            var dialog = new ContentDialog()
            {
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
                Title = Strings.Resources.MissingSoundsTitle,
                PrimaryButtonText = Strings.Resources.DownloadText,
                CloseButtonText = Strings.Resources.CancelText,
                Content = Strings.Resources.MissingSoundsMessage
            };

            var result = await dialog.ShowAsync();
            IsDialogOpen = false;
            return result == ContentDialogResult.Primary;
        }

        /// <inheritdoc/>
        public async Task OpenSettingsAsync()
        {
            if (IsDialogOpen)
                return;

            IsDialogOpen = true;
            var dialog = new ContentDialog()
            {
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
                Title = Strings.Resources.SettingsText,
                CloseButtonText = Strings.Resources.CloseText,
                Content = new SettingsControl()
            };

            void OnSettingsSet(object sender, string key)
            {
                if (key == UserSettingsConstants.Theme)
                {
                    dialog.RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme();
                }
            }

            _userSettings.SettingSet += OnSettingsSet;
            await dialog.ShowAsync();
            _userSettings.SettingSet -= OnSettingsSet;
            IsDialogOpen = false;

        }

        /// <inheritdoc/>
        public async Task OpenThemeSettingsAsync()
        {
            if (IsDialogOpen)
                return;

            IsDialogOpen = true;
            var dialog = new ContentDialog()
            {
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
                Title = Strings.Resources.ThemeSettings,
                CloseButtonText = Strings.Resources.CloseText,
                Content = new ThemeSettings()
            };

            void OnSettingsSet(object sender, string key)
            {
                if (key == UserSettingsConstants.Theme)
                {
                    dialog.RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme();
                }
            }

            _userSettings.SettingSet += OnSettingsSet;
            await dialog.ShowAsync();
            _userSettings.SettingSet -= OnSettingsSet;
            IsDialogOpen = false;
        }

        /// <inheritdoc/>
        public async Task<string> RenameAsync(string currentName)
        {
            if (IsDialogOpen)
                return currentName;

            IsDialogOpen = true;
            var inputBoxControl = new RenameInputBox() { Input = currentName };
            bool enterClicked = false;
            var dialog = new ContentDialog()
            {
                Title = Strings.Resources.RenameText,
                CloseButtonText = Strings.Resources.CancelText,
                PrimaryButtonText = Strings.Resources.RenameText,
                Content = inputBoxControl
            };
            inputBoxControl.EnterClicked += (s, e) => { dialog.Hide(); enterClicked = true; };

            var result = await dialog.ShowAsync();
            IsDialogOpen = false;

            return result == ContentDialogResult.Primary || enterClicked ? inputBoxControl.Input : currentName;
        }

        /// <inheritdoc/>
        public async Task OpenPremiumAsync()
        {
            if (IsDialogOpen)
            {
                return;
            }

            IsDialogOpen = true;
            var content = new PremiumControl();
            var dialog = new NoPaddingDialog()
            {
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
                Content = content
            };

            content.CloseRequested += (s, e) => dialog.Hide();
            await dialog.ShowAsync();
            IsDialogOpen = false;
        }

        /// <inheritdoc/>
        public async Task OpenVideosMenuAsync()
        {
            if (IsDialogOpen)
            {
                return;
            }

            IsDialogOpen = true;
            var dialog = new ContentDialog()
            {
                Title = Strings.Resources.ScreensaverCatalogue,
                CloseButtonText = Strings.Resources.CloseText,
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
                Content = new VideosMenu()
            };

            await dialog.ShowAsync();
            IsDialogOpen = false;
        }

        /// <inheritdoc/>
        public async Task<(double, string)> OpenInterruptionAsync()
        {
            if (IsDialogOpen)
            {
                return (0, string.Empty);
            }

            IsDialogOpen = true;
            var dialog = new InterruptionDialog()
            {
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
            };

            var result = await dialog.ShowAsync();
            IsDialogOpen = false;

            return result == ContentDialogResult.Primary 
                ? (dialog.MinutesLogged, dialog.InterruptionNotes) 
                : (0, string.Empty);
        }

        /// <inheritdoc/>
        public async Task OpenHistoryDetailsAsync(FocusHistoryViewModel historyViewModel)
        {
            if (historyViewModel is null || IsDialogOpen)
            {
                return;
            }

            IsDialogOpen = true;
            var dialog = new ContentDialog()
            {
                Title = Strings.Resources.History,
                CloseButtonText = Strings.Resources.CloseText,
                RequestedTheme = _userSettings.Get<string>(UserSettingsConstants.Theme).ToTheme(),
                Content = new FocusHistoryDetails(historyViewModel)
            };

            await dialog.ShowAsync();
            IsDialogOpen = false;
        }
    }
}
