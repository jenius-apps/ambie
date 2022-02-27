using AmbientSounds.Models;
using AmbientSounds.Services;
using JeniusApps.Common.Tools;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
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
        private readonly IVideoService _videoService;
        private Uri _videoSource = new Uri(DefaultVideoSource);
        private bool _settingsButtonVisible;
        private bool _loading;
        private bool _slideshowVisible;

        /// <summary>
        /// Raised when the view model has completed
        /// initialization.
        /// </summary>
        public event EventHandler? Loaded;

        public ScreensaverPageViewModel(
            ILocalizer localizer,
            IVideoService videoService)
        {
            Guard.IsNotNull(localizer, nameof(localizer));
            Guard.IsNotNull(videoService, nameof(videoService));
            _localizer = localizer;
            _videoService = videoService;
        }

        public ObservableCollection<ToggleMenuItem> MenuItems { get; } = new();

        public ToggleMenuItem? CurrentSelection { get; set; }

        public Uri VideoSource
        {
            get => _videoSource;
            set
            {
                SetProperty(ref _videoSource, value);
                OnPropertyChanged(nameof(VideoPlayerVisible));
            }
        }

        public bool VideoPlayerVisible => VideoSource.AbsoluteUri != DefaultVideoSource;

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

            IReadOnlyList<Video> videos = await _videoService.GetVideosAsync();
            var screensaverCommand = new AsyncRelayCommand<string>(ChangeScreensaverTo);
            MenuItems.Add(new ToggleMenuItem(DefaultId, _localizer.GetString(DefaultId), screensaverCommand, DefaultId));

            foreach (var v in videos)
            {
                MenuItems.Add(new ToggleMenuItem(v.Id, v.Name, screensaverCommand, v.Id));
            }

            if (MenuItems.Count > 1)
            {
                // Only show if we have more than the default option.
                SettingsButtonVisible = true;
            }

            await ChangeScreensaverTo(DefaultId);
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

            if (screensaverId == DefaultId)
            {
                VideoSource = new Uri(DefaultVideoSource);
                SlideshowVisible = true;
            }
            else
            {
                var path = await _videoService.GetFilePathAsync(screensaverId);
                if (!string.IsNullOrEmpty(path))
                {
                    SlideshowVisible = false;

                    try
                    {
                        VideoSource = new Uri(path);
                    }
                    catch (UriFormatException)
                    {
                        // TODO log error
                    }
                }
                else
                {
                    // TODO handle scenario where there
                    // was an issue with the path.
                }
            }

            CurrentSelection = MenuItems.FirstOrDefault(x => x.Id == screensaverId);
        }
    }

    public record ToggleMenuItem(string Id, string Text, ICommand Command, object? CommandParameter = null);
}
