﻿using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Shaders;
using JeniusApps.Common.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JeniusApps.Common.Telemetry;

namespace AmbientSounds.ViewModels
{
    public partial class ScreensaverPageViewModel : ObservableObject
    {
        private const string DefaultId = "default";
        private const string DarkScreenId = "darkscreen";
        private const string VideoDialogId = "videoDialog";
        private const string DefaultVideoSource = "http://localhost";
        private readonly ILocalizer _localizer;
        private readonly IVideoService _videoService;
        private readonly IDialogService _dialogService;
        private readonly IIapService _iapService;
        private readonly ITelemetry _telemetry;
        private readonly Services.ISystemInfoProvider _systemInfoProvider;
        private readonly IUserSettings _userSettings;
        private Uri _videoSource = new Uri(DefaultVideoSource);

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(AnimatedBackgroundVisible))]
        private string? _animatedBackgroundName = null;

        [ObservableProperty]
        private bool _settingsButtonVisible;

        [ObservableProperty]
        private bool _loading;

        [ObservableProperty]
        private bool _slideshowVisible;

        [ObservableProperty]
        private bool _isDarkScreen;

        /// <summary>
        /// Raised when the view model has completed
        /// initialization.
        /// </summary>
        public event EventHandler? Loaded;

        public ScreensaverPageViewModel(
            ILocalizer localizer,
            IVideoService videoService,
            IDialogService dialogService,
            IIapService iapService,
            ITelemetry telemetry,
            Services.ISystemInfoProvider systemInfoProvider,
            IUserSettings userSettings)
        {
            Guard.IsNotNull(localizer, nameof(localizer));
            Guard.IsNotNull(videoService, nameof(videoService));
            Guard.IsNotNull(dialogService, nameof(dialogService));
            Guard.IsNotNull(iapService, nameof(iapService));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(systemInfoProvider, nameof(systemInfoProvider));
            Guard.IsNotNull(userSettings, nameof(userSettings));

            _localizer = localizer;
            _videoService = videoService;
            _dialogService = dialogService;
            _iapService = iapService;
            _telemetry = telemetry;
            _systemInfoProvider = systemInfoProvider;
            _userSettings = userSettings;

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
                if (value is null)
                {
                    return;
                }

                _videoSource = value;
                OnPropertyChanged(nameof(VideoPlayerVisible));

                // Manually raise this event because
                // UI depends on this being raised
                // when navigating to the page.
                OnPropertyChanged(nameof(VideoSource));
            }
        }

        public bool VideoPlayerVisible => VideoSource.AbsoluteUri != DefaultVideoSource;

        /// <summary>
        /// Determines if the animated background should be shown.
        /// </summary>
        public bool AnimatedBackgroundVisible => AnimatedBackgroundName is not null;

        public bool FullScreenVisible => _systemInfoProvider.IsDesktop();

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
            MenuItems.Add(new FlyoutMenuItem(DarkScreenId, _localizer.GetString("SettingsThemeDarkRadio/Content"), screensaverCommand, DarkScreenId, true));

            // Only enable compute shaders on desktop
            //if (_systemInfoProvider.IsDesktop())
            //{
            //    // Animated backgrounds
            //    MenuItems.Add(new FlyoutMenuItem($"[CS]{nameof(ColorfulInfinity)}", _localizer.GetString("ComputeShader/ColoredSmoke"), screensaverCommand, $"[CS]{nameof(ColorfulInfinity)}", true));
            //    MenuItems.Add(new FlyoutMenuItem($"[CS]{nameof(ProteanClouds)}", _localizer.GetString("ComputeShader/Clouds"), screensaverCommand, $"[CS]{nameof(ProteanClouds)}", true));
            //}

            foreach (var v in videos)
            {
                if (_videoService.GetInstallProgress(v) is not null)
                {
                    // If a video is still being downloaded, don't add it to the menu.
                    continue;
                }

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

            if (menuItemId == VideoDialogId)
            {
                _telemetry.TrackEvent(TelemetryConstants.VideoMenuOpened);
                await _dialogService.OpenVideosMenuAsync();
                return;
            }

            var newSelectedItem = MenuItems.FirstOrDefault(x => x.Id == menuItemId);
            if (newSelectedItem is null)
            {
                menuItemId = DefaultId;
                newSelectedItem = MenuItems.FirstOrDefault(x => x.Id == DefaultId);
            }

            if (newSelectedItem?.IsToggle == true)
            {
                CurrentSelection = newSelectedItem;
                _userSettings.Set(UserSettingsConstants.LastUsedScreensaverKey, menuItemId);
            }

            if (menuItemId == DefaultId)
            {
                IsDarkScreen = false;
                AnimatedBackgroundName = null;
                VideoSource = new Uri(DefaultVideoSource);
                SlideshowVisible = true;
            }
            else if (menuItemId == DarkScreenId)
            {
                IsDarkScreen = true;
                AnimatedBackgroundName = null;
                VideoSource = new Uri(DefaultVideoSource);
                SlideshowVisible = false;
            }
            else if (menuItemId?.StartsWith("[CS]") == true)
            {
                string name = menuItemId.Substring("[CS]".Length);

                IsDarkScreen = false;
                AnimatedBackgroundName = name;
                VideoSource = new Uri(DefaultVideoSource);
                SlideshowVisible = false;

                _telemetry.TrackEvent(TelemetryConstants.ShaderSelected, new Dictionary<string, string>()
                {
                    { "id",  menuItemId! },
                    { "name", name }
                });
            }
            else
            {
                IsDarkScreen = false;
                Video? video = await _videoService.GetLocalVideoAsync(menuItemId!);
                var isOwned = await _iapService.IsAnyOwnedAsync(video?.IapIds ?? Array.Empty<string>());
                if (!isOwned)
                {
                    await _dialogService.OpenPremiumAsync();
                    return;
                }

                var path = await _videoService.GetFilePathAsync(menuItemId);
                if (!string.IsNullOrEmpty(path))
                {
                    AnimatedBackgroundName = null;
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

                _telemetry.TrackEvent(TelemetryConstants.VideoSelected, new Dictionary<string, string>()
                {
                    { "id",  menuItemId! },
                    { "name", video?.Name ?? string.Empty }
                });
            }
        }
    }

    public record FlyoutMenuItem(string Id, string Text, ICommand Command, object? CommandParameter = null, bool IsToggle = false);
}
