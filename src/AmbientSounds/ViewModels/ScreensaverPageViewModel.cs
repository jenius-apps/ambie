using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AmbientSounds.ViewModels;

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
    private readonly ISystemInfoProvider _systemInfoProvider;
    private readonly IUserSettings _userSettings;
    private readonly IChannelService _channelService;
    private readonly IFocusService _focusService;
    private readonly ChannelVmFactory _channelFactory;
    private Uri _videoSource = new(DefaultVideoSource);
    private string _activeScreensaverId = string.Empty;

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
        ISystemInfoProvider systemInfoProvider,
        IUserSettings userSettings,
        IChannelService channelService,
        ChannelVmFactory channelVmFactory,
        IExperimentationService experimentationService,
        IFocusService focusService)
    {
        _localizer = localizer;
        _videoService = videoService;
        _dialogService = dialogService;
        _iapService = iapService;
        _telemetry = telemetry;
        _systemInfoProvider = systemInfoProvider;
        _userSettings = userSettings;
        _channelService = channelService;
        _channelFactory = channelVmFactory;
        _focusService = focusService;

        _videoService.VideoDeleted += OnVideoDeleted;

        ChannelSwitcherVisible = !_userSettings.Get<bool>(UserSettingsConstants.ChannelSwitcherHidden);
    }

    [ObservableProperty]
    private bool _focusTimerVisible;

    /// <summary>
    /// A11y text to use for channel switcher toggle button.
    /// </summary>
    public string ToggleChannelSwitcherText => ChannelSwitcherVisible
        ? _localizer.GetString("ChannelSwitcherHide")
        : _localizer.GetString("ChannelSwitcherShow");

    /// <summary>
    /// Determines if the channel switcher is visible.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ToggleChannelSwitcherText))]
    private bool _channelSwitcherVisible;

    [ObservableProperty]
    private bool _clockVisible;

    [ObservableProperty]
    private bool _settingsButtonVisible;

    [ObservableProperty]
    private bool _loading;

    [ObservableProperty]
    private bool _slideshowVisible;

    [ObservableProperty]
    private bool _isDarkScreen;

    [ObservableProperty]
    private bool _dialogOpen;

    [ObservableProperty]
    private string _videoPlaceholderImageUrl = "http://localhost";

    public ObservableCollection<FlyoutMenuItem> MenuItems { get; } = [];

    public ObservableCollection<ChannelViewModel> Channels { get; } = [];

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

    public bool FullScreenVisible => _systemInfoProvider.GetDeviceFamily() == "Windows.Desktop";

    public Task InitializeAsync(ScreensaverArgs args)
    {
        VideoPlaceholderImageUrl = args.VideoImagePreviewUrl ?? "http://localhost";

        string screensaverId = args.RequestedType switch
        {
            ChannelType.DarkScreen => DarkScreenId,
            ChannelType.Slideshow => DefaultId,
            ChannelType.Videos => args.VideoId ?? string.Empty,
            _ => string.Empty
        };

        _activeScreensaverId = screensaverId;
        return InitializeAsync(screensaverId);
    }

    public async Task InitializeAsync(string? screensaverToSelect = "")
    {
        if (Loading)
        {
            return;
        }

        Loading = true;

        UpdateClockSettings();

        var channelsTask = InitializeChannelsAsync(default);

        MenuItems.Clear();
        IReadOnlyList<Video> videos = await _videoService.GetVideosAsync(includeOnline: false);
        MenuItems.Add(new FlyoutMenuItem(DefaultId, _localizer.GetString(DefaultId), ChangeScreensaverToCommand, DefaultId, true));
        MenuItems.Add(new FlyoutMenuItem(DarkScreenId, _localizer.GetString("SettingsThemeDarkRadio/Content"), ChangeScreensaverToCommand, DarkScreenId, true));

        foreach (var v in videos)
        {
            if (_videoService.GetInstallProgress(v) is not null)
            {
                // If a video is still being downloaded, don't add it to the menu.
                continue;
            }

            MenuItems.Add(new FlyoutMenuItem(v.Id, v.Name, ChangeScreensaverToCommand, v.Id, true));
        }

        MenuItems.Add(new FlyoutMenuItem(VideoDialogId, _localizer.GetString("MoreScreensavers"), ChangeScreensaverToCommand, VideoDialogId));

        if (MenuItems.Count > 1)
        {
            // Only show if we have more than the default option.
            SettingsButtonVisible = true;
        }

        await ChangeScreensaverTo(string.IsNullOrEmpty(screensaverToSelect) ? DefaultId : screensaverToSelect);

        await channelsTask;
        Loading = false;
        Loaded?.Invoke(this, EventArgs.Empty);
    }

    public async Task InitializeChannelsAsync(CancellationToken ct)
    {
        Channels.Clear();
        ct.ThrowIfCancellationRequested();

        var channels = await _channelService.GetChannelsAsync();

        ct.ThrowIfCancellationRequested();
        foreach (var c in channels)
        {
            ct.ThrowIfCancellationRequested();

            if (_channelFactory.Create(c, playCommand: PlayChannelCommand) is { } vm)
            {
                await vm.InitializeAsync();
                Channels.Add(vm);
            }
        }
    }

    public void Uninitialize()
    {
        foreach (var channel in Channels)
        {
            channel.Uninitialize();
        }

        Channels.Clear();
    }

    private async void OnVideoDeleted(object sender, string deletedVideoId)
    {
        await InitializeAsync(deletedVideoId == CurrentSelection?.Id
            ? DefaultId
            : CurrentSelection?.Id);
    }

    [RelayCommand]
    private async Task PageSettingsAsync()
    {
        DialogOpen = true;
        _telemetry.TrackEvent(TelemetryConstants.ChannelViewerSettingsClicked);
        await _dialogService.OpenChannelPageSettingsAsync();
        DialogOpen = false;

        UpdateClockSettings();
    }

    [RelayCommand]
    private void ToggleChannelSwitcher()
    {
        ChannelSwitcherVisible = !ChannelSwitcherVisible;
        _userSettings.Set(UserSettingsConstants.ChannelSwitcherHidden, !ChannelSwitcherVisible);
    }

    private void UpdateClockSettings()
    {
        string? channelTimerModeString = _userSettings.Get<string>(UserSettingsConstants.ChannelTimerModeKey);
        ClockVisible = _userSettings.Get<bool>(UserSettingsConstants.ChannelClockEnabledKey) ||
            channelTimerModeString == ChannelTimerMode.Countdown.ToString();

        FocusTimerVisible = _focusService.CurrentState is not FocusState.None || channelTimerModeString == ChannelTimerMode.Focus.ToString();
    }


    [RelayCommand]
    private async Task PlayChannelAsync(ChannelViewModel? channelViewModel)
    {
        if (channelViewModel?.Channel is not Channel channel)
        {
            return;
        }

        string? menuItemId = null;
        if (channel.Type is ChannelType.DarkScreen)
        {
            menuItemId = DarkScreenId;
        }
        else if (channel.Type is ChannelType.Slideshow)
        {
            menuItemId = DefaultId;
        }
        else if (channel is { Type: ChannelType.Videos, VideoIds: [string videoId, ..] })
        {
            menuItemId = videoId;
        }

        if (menuItemId is not { Length: > 0 } || menuItemId == _activeScreensaverId)
        {
            return;
        }

        _activeScreensaverId = menuItemId;
        await _channelService.PlayChannelAsync(channel, performNavigation: false);
        await ChangeScreensaverTo(menuItemId);
    }

    [RelayCommand]
    private async Task ChangeScreensaverTo(string? menuItemId)
    {
        if (menuItemId is null)
        {
            return;
        }

        if (menuItemId == VideoDialogId)
        {
            _telemetry.TrackEvent(TelemetryConstants.VideoMenuOpened);
            DialogOpen = true;
            await _dialogService.OpenVideosMenuAsync();
            DialogOpen = false;
            return;
        }

        if (menuItemId == DefaultId)
        {
            IsDarkScreen = false;
            VideoSource = new Uri(DefaultVideoSource);
            SlideshowVisible = true;
        }
        else if (menuItemId == DarkScreenId)
        {
            IsDarkScreen = true;
            VideoSource = new Uri(DefaultVideoSource);
            SlideshowVisible = false;
        }
        else
        {
            IsDarkScreen = false;
            Video? video = await _videoService.GetLocalVideoAsync(menuItemId!);
            var isOwned = await _iapService.IsAnyOwnedAsync(video?.IapIds ?? Array.Empty<string>());
            if (!isOwned)
            {
                DialogOpen = true;
                await _dialogService.OpenPremiumAsync();
                DialogOpen = false;
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

            _telemetry.TrackEvent(TelemetryConstants.VideoSelected, new Dictionary<string, string>()
            {
                { "id",  menuItemId! },
                { "name", video?.Name ?? string.Empty }
            });
        }

        _userSettings.Set(UserSettingsConstants.LastUsedChannelKey, menuItemId);
    }
}

public record FlyoutMenuItem(string Id, string Text, ICommand Command, object? CommandParameter = null, bool IsToggle = false);
