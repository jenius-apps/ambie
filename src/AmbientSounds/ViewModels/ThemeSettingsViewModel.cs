using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class ThemeSettingsViewModel : ObservableObject
    {
        private const string NoneImageName = "znone.png";
        private readonly IUserSettings _userSettings;
        private readonly IImagePicker _imagePicker;
        private readonly ITelemetry _telemetry;
        private readonly IAssetsReader _assetsReader;

        public ThemeSettingsViewModel(
            IUserSettings userSettings,
            IAssetsReader assetsReader,
            IImagePicker imagePicker,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(userSettings);
            Guard.IsNotNull(assetsReader);
            Guard.IsNotNull(imagePicker);
            Guard.IsNotNull(telemetry);

            _assetsReader = assetsReader;
            _userSettings = userSettings;
            _imagePicker = imagePicker;
            _telemetry = telemetry;

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

        private async Task LoadImagesAsync()
        {
            if (ImagePaths.Count > 0)
            {
                return;
            }

            var paths = await _assetsReader.GetBackgroundsAsync();
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

        private void SelectImage(string? imagePath)
        {
            if (imagePath?.Contains(NoneImageName) == true)
            {
                imagePath = string.Empty;
            }

            _userSettings.Set(UserSettingsConstants.BackgroundImage, imagePath);

            if (imagePath != null)
            {
                _telemetry.TrackEvent(TelemetryConstants.BackgroundChanged, new Dictionary<string, string>
                {
                    { "path", imagePath.Contains("ms-appx") ? imagePath : "custom" }
                });
            }
        }
    }
}