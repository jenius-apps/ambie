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
        private const string VideoDialogId = "videoDialog";
        private const string DefaultVideoSource = "http://localhost";
        private readonly ILocalizer _localizer;
        private readonly IVideoService _videoService;
        private readonly IDialogService _dialogService;
        private readonly IIapService _iapService;
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
            IVideoService videoService,
            IDialogService dialogService,
            IIapService iapService)
        {
            Guard.IsNotNull(localizer, nameof(localizer));
            Guard.IsNotNull(videoService, nameof(videoService));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(iapService, nameof(iapService));

            _localizer = localizer;
            _videoService = videoService;
            _dialogService = dialogService;
            _iapService = iapService;

            _videoService.VideoDownloaded += OnVideoDownloaded;
            _videoService.VideoDeleted += OnVideoDeleted;
        }

        public ObservableCollection<FlyoutMenuItem> MenuItems { get; } = new();

        public FlyoutMenuItem? CurrentSelection { get; set; }

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

        public async Task InitializeAsync(string? screensaverToSelect = "")
        {
            if (Loading)
            {
                return;
            }

            Loading = true;

            MenuItems.Clear();
            IReadOnlyList<Video> videos = await _videoService.GetVideosAsync(includeOnline: false);
            var screensaverCommand = new AsyncRelayCommand<string>(ChangeScreensaverTo);
            MenuItems.Add(new FlyoutMenuItem(DefaultId, _localizer.GetString(DefaultId), screensaverCommand, DefaultId, true));

            foreach (var v in videos)
            {
                MenuItems.Add(new FlyoutMenuItem(v.Id, v.Name, screensaverCommand, v.Id, true));
            }

            MenuItems.Add(new FlyoutMenuItem(VideoDialogId, _localizer.GetString("MoreScreensavers"), screensaverCommand, VideoDialogId));

            if (MenuItems.Count > 1)
            {
                // Only show if we have more than the default option.
                SettingsButtonVisible = true;
            }

            await ChangeScreensaverTo(string.IsNullOrEmpty(screensaverToSelect) ? DefaultId : screensaverToSelect);

            Loading = false;
            Loaded?.Invoke(this, EventArgs.Empty);
        }

        private async void OnVideoDeleted(object sender, string deletedVideoId)
        {
            await InitializeAsync(deletedVideoId == CurrentSelection?.Id
                ? DefaultId
                : CurrentSelection?.Id);
        }

        private async void OnVideoDownloaded(object sender, string e)
        {
            await InitializeAsync(screensaverToSelect: CurrentSelection?.Id);
        }

        private async Task ChangeScreensaverTo(string? menuItemId)
        {
            if (menuItemId is null)
            {
                return;
            }

            if (menuItemId == DefaultId)
            {
                VideoSource = new Uri(DefaultVideoSource);
                SlideshowVisible = true;
            }
            else if (menuItemId == VideoDialogId)
            {
                await _dialogService.OpenVideosMenuAsync();
            }
            else
            {
                Video? video = await _videoService.GetLocalVideoAsync(menuItemId);
                var isOwned = await _iapService.IsAnyOwnedAsync(video?.IapIds ?? Array.Empty<string>());
                if (!isOwned)
                {
                    await _dialogService.OpenPremiumAsync();
                    return;
                }

                var path = await _videoService.GetFilePathAsync(menuItemId);
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

            var newSelectedItem = MenuItems.FirstOrDefault(x => x.Id == menuItemId);
            if (newSelectedItem?.IsToggle == true)
            {
                CurrentSelection = newSelectedItem;
            }
        }
    }

    public record FlyoutMenuItem(string Id, string Text, ICommand Command, object? CommandParameter = null, bool IsToggle = false);
}
