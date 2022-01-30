using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Shaders;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels
{
    public class ThemeSettingsViewModel : ObservableObject
    {
        private const string NoneImageName = "znone.png";
        private readonly IUserSettings _userSettings;
        private readonly IImagePicker _imagePicker;
        private readonly ITelemetry _telemetry;
        private readonly ISystemInfoProvider _systemInfoProvider;

        public ThemeSettingsViewModel(
            IUserSettings userSettings,
            ISystemInfoProvider systemInfoProvider,
            IImagePicker imagePicker,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));
            Guard.IsNotNull(imagePicker, nameof(imagePicker));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _systemInfoProvider = systemInfoProvider;
            _userSettings = userSettings;
            _imagePicker = imagePicker;
            _telemetry = telemetry;

            SelectBackgroundItemCommand = new RelayCommand<object>(SelectBackgroundItem);
            LoadBackgroundItemsCommand = new AsyncRelayCommand(LoadBackgroundItemsAsync);
            BrowseCommand = new AsyncRelayCommand(BrowseForImageAsync);
        }

        /// <summary>
        /// Command for selecting the background image.
        /// </summary>
        public IRelayCommand<string> SelectBackgroundItemCommand { get; }

        /// <summary>
        /// Command for loading the background images.
        /// </summary>
        public IAsyncRelayCommand LoadBackgroundItemsCommand { get; }

        /// <summary>
        /// Command for opening a file picker to select a custom image.
        /// </summary>
        public IAsyncRelayCommand BrowseCommand { get; }

        /// <summary>
        /// Paths to available background images.
        /// </summary>
        public ObservableCollection<object> BackgroundItems { get; } = new();

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

        private async Task LoadBackgroundItemsAsync()
        {
            if (BackgroundItems.Count > 0)
            {
                return;
            }

            string[] paths = await _systemInfoProvider.GetAvailableBackgroundsAsync();

            // Images
            foreach (var p in paths.Where(path => !path.Contains(NoneImageName)))
            {
                BackgroundItems.Add(p);
            }

            if (_systemInfoProvider.IsDesktop())
            {
                // Animated backgrounds
                BackgroundItems.Add(typeof(ColorfulInfinity));
                BackgroundItems.Add(typeof(Octagrams));
                BackgroundItems.Add(typeof(ProteanClouds));
            }

            // Empty image
            BackgroundItems.Add(paths.Single(path => path.Contains(NoneImageName)));
        }

        private async Task BrowseForImageAsync()
        {
            string? imagePath = await _imagePicker.BrowseAsync();
            if (imagePath == null)
            {
                return;
            }

            SelectBackgroundItem(imagePath);
        }

        private void SelectBackgroundItem(object? selectedItem)
        {
            if (selectedItem is string imagePath)
            {
                if (imagePath.Contains(NoneImageName) == true)
                {
                    imagePath = string.Empty;
                }

                _userSettings.Set(UserSettingsConstants.AnimatedBackgroundType, string.Empty);
                _userSettings.Set(UserSettingsConstants.BackgroundImage, imagePath);

                _telemetry.TrackEvent(TelemetryConstants.BackgroundChanged, new Dictionary<string, string>
                {
                    { "path", imagePath.Contains("ms-appx") ? imagePath : "custom" }
                });
            }
            else if (selectedItem is Type shaderType)
            {
                _userSettings.Set(UserSettingsConstants.AnimatedBackgroundType, shaderType.Name);
                _userSettings.Set(UserSettingsConstants.BackgroundImage, string.Empty);

                _telemetry.TrackEvent(TelemetryConstants.BackgroundChanged, new Dictionary<string, string>
                {
                    { "type", shaderType.FullName }
                });
            }
        }
    }
}
