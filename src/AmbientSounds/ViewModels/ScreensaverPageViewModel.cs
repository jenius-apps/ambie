using JeniusApps.Common.Tools;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AmbientSounds.ViewModels
{
    public class ScreensaverPageViewModel : ObservableObject
    {
        private const string DefaultId = "default";
        private const string DefaultVideoSource = "http://localhost";
        private readonly ILocalizer _localizer;
        private string _videoSource = DefaultVideoSource;
        private bool _settingsButtonVisible;
        private bool _loading;
        private bool _slideshowVisible;

        /// <summary>
        /// Raised when the view model has completed
        /// initialization.
        /// </summary>
        public event EventHandler? Loaded;

        public ScreensaverPageViewModel(ILocalizer localizer)
        {
            Guard.IsNotNull(localizer, nameof(localizer));
            _localizer = localizer;
        }

        public ObservableCollection<ToggleMenuItem> MenuItems { get; } = new();

        public ToggleMenuItem? CurrentSelection { get; set; }

        public string VideoSource
        {
            get => _videoSource;
            set
            {
                SetProperty(ref _videoSource, value);
                OnPropertyChanged(nameof(VideoPlayerVisible));
            }
        }

        public bool VideoPlayerVisible => !string.IsNullOrEmpty(VideoSource) && VideoSource != DefaultVideoSource;

        public bool SlideshowVisible
        {
            get => _slideshowVisible;
            set => SetProperty(ref _slideshowVisible, value);
        }

        public bool SettingsButtonVisible
        {
            get => _settingsButtonVisible;
            set => SetProperty(ref _settingsButtonVisible, value);
        }

        public bool Loading
        {
            get => _loading;
            set => SetProperty(ref _loading, value);
        }

        public async Task InitializeAsync()
        {
            Loading = true;
            await Task.Delay(1);

            // TODO Get list of available videos

            var screensaverCommand = new AsyncRelayCommand<string>(ChangeScreensaverTo);
            MenuItems.Add(new ToggleMenuItem(DefaultId, _localizer.GetString(DefaultId), screensaverCommand, DefaultId));

            if (MenuItems.Count > 1)
            {
                // Ensure CurrentSelection is set properly
                await ChangeScreensaverTo(DefaultId);
                SettingsButtonVisible = true;
            }

            Loading = false;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private async Task ChangeScreensaverTo(string? screensaverId)
        {
            if (screensaverId is null)
            {
                return;
            }

            await Task.Delay(1);

            if (screensaverId == "default")
            {
                VideoSource = DefaultVideoSource;
                SlideshowVisible = true;
            }
            else
            {
                SlideshowVisible = false;
                // VideoSource = await _videoService.GetPathAsync(screensaverId);
            }

            CurrentSelection = MenuItems.FirstOrDefault(x => x.Id == screensaverId);
        }
    }

    public record ToggleMenuItem(string Id, string Text, ICommand Command, object? CommandParameter = null);
}
