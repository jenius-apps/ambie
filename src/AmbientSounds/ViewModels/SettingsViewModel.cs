using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class SettingsViewModel : ObservableObject
    {
        private const string NoneImageName = "znone.png";
        private readonly IUserSettings _userSettings;
        private readonly IStoreNotificationRegistrar _notifications;
        private readonly IScreensaverService _screensaverService;
        private readonly ISystemInfoProvider _systemInfoProvider;
        private readonly IImagePicker _imagePicker;
        private bool _notificationsLoading;

        public SettingsViewModel(
            IUserSettings userSettings,
            IScreensaverService screensaverService,
            ISystemInfoProvider systemInfoProvider,
            IStoreNotificationRegistrar notifications,
            IImagePicker imagePicker)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(notifications, nameof(notifications));
            Guard.IsNotNull(screensaverService, nameof(screensaverService));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));
            Guard.IsNotNull(imagePicker, nameof(imagePicker));
            _systemInfoProvider = systemInfoProvider;
            _screensaverService = screensaverService;
            _userSettings = userSettings;
            _notifications = notifications;
            _imagePicker = imagePicker;

            SelectImageCommand = new RelayCommand<string>(SelectImage);
            LoadImagesCommand = new AsyncRelayCommand(LoadImagesAsync);
            BrowseCommand = new AsyncRelayCommand(BrowseForImageAsync);
        }

        /// <summary>
        /// Command for selecting the background image.
        /// </summary>
        public IRelayCommand<string> SelectImageCommand { get; }

        /// <summary>
        /// Command for loading the background images.
        /// </summary>
        public IAsyncRelayCommand LoadImagesCommand { get; }

        /// <summary>
        /// Command for opening a file picker to select a custom image.
        /// </summary>
        public IAsyncRelayCommand BrowseCommand { get; }

        /// <summary>
        /// Paths to available background images.
        /// </summary>
        public ObservableCollection<string> ImagePaths { get; } = new();

        /// <summary>
        /// Settings flag for telemetry.
        /// </summary>
        public bool TelemetryEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
            set => _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
        }

        /// <summary>
        /// Settings flag for transparency.
        /// </summary>
        public bool TransparencyOn
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.Transparency);
            set => _userSettings.Set(UserSettingsConstants.Transparency, value);
        }

        /// <summary>
        /// Determines if the transparency toggle is enabled.
        /// </summary>
        public bool TransparencyToggleEnabled => string.IsNullOrWhiteSpace(_userSettings.Get<string>(UserSettingsConstants.BackgroundImage));

        /// <summary>
        /// Settings flag for screensaver.
        /// </summary>
        public bool ScreensaverEnabled
        {
            get => _userSettings.Get(UserSettingsConstants.EnableScreenSaver, _systemInfoProvider.IsTenFoot());
            set
            {
                _userSettings.Set(UserSettingsConstants.EnableScreenSaver, value);
                
                if (value)
                {
                    _screensaverService.StartTimer();
                }
                else
                {
                    _screensaverService.StopTimer();
                }
            }
        }

        /// <summary>
        /// Settings flag for dark screensaver.
        /// </summary>
        public bool DarkScreensaverEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.DarkScreensasver);
            set => _userSettings.Set(UserSettingsConstants.DarkScreensasver, value);
        }

        /// <summary>
        /// Settings flag for notifications.
        /// </summary>
        public bool NotificationsEnabled
        {
            get => _userSettings.Get<bool>(UserSettingsConstants.Notifications);
            set => SetNotifications(value);
        }

        /// <summary>
        /// The current theme.
        /// </summary>
        public string CurrentTheme
        {
            get => _userSettings.Get<string>(UserSettingsConstants.Theme);
            set
            {
                _userSettings.Set(UserSettingsConstants.Theme, value);
                OnPropertyChanged(nameof(DefaultRadioIsChecked));
                OnPropertyChanged(nameof(DarkRadioIsChecked));
                OnPropertyChanged(nameof(LightRadioIsChecked));
            }
        }

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for default app theme.
        /// </summary>
        public bool DefaultRadioIsChecked => !DarkRadioIsChecked && !LightRadioIsChecked;

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for dark app theme.
        /// </summary>
        public bool DarkRadioIsChecked => CurrentTheme == "dark";

        /// <summary>
        /// Property for setting IsChecked property of RadioButton for light app theme.
        /// </summary>
        public bool LightRadioIsChecked => CurrentTheme == "light";

        /// <summary>
        /// Event handler for RadioButton (dark theme) click event.
        /// </summary>
        public void DarkThemeRadioClicked()
        {
            CurrentTheme = "dark";
        }

        /// <summary>
        /// Event handler for RadioButton (default theme) click event.
        /// </summary>
        public void DefaultThemeRadioClicked()
        {
            CurrentTheme = "default";
        }

        /// <summary>
        /// Event handler for RadioButton (light theme) click event.
        /// </summary>
        public void LightThemeRadioClicked()
        {
            CurrentTheme = "light";
        }

        private void SelectImage(string? imagePath)
        {
            if (imagePath?.Contains(NoneImageName) == true)
            {
                imagePath = string.Empty;
            }

            _userSettings.Set(UserSettingsConstants.BackgroundImage, imagePath);
            OnPropertyChanged(nameof(TransparencyToggleEnabled));
        }

        private async Task LoadImagesAsync()
        {
            if (ImagePaths.Count > 0)
            {
                return;
            }

            string[] paths = await _systemInfoProvider.GetAvailableBackgroundsAsync();
            foreach (var p in paths)
            {
                ImagePaths.Add(p);
            }
        }

        private async Task BrowseForImageAsync()
        {
            string? imagePath = await _imagePicker.BrowseAsync();
            if (imagePath == null)
            {
                return;
            }

            SelectImage(imagePath);
        }

        private async void SetNotifications(bool value)
        {
            if (value == NotificationsEnabled || _notificationsLoading)
            {
                return;
            }

            _notificationsLoading = true;
            if (value)
            {
                await _notifications.Register();
            }
            else
            {
                await _notifications.Unregiser();
            }
            _userSettings.Set(UserSettingsConstants.Notifications, value);
            _notificationsLoading = false;
        }
    }
}