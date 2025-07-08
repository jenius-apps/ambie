using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.PushNotifications;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using IAssetsReader = AmbientSounds.Tools.IAssetsReader;

namespace AmbientSounds.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private const string NoneImageName = "znone.png";
    private readonly IImagePicker _imagePicker;
    private readonly IAssetsReader _assetsReader;
    private readonly IUserSettings _userSettings;
    private readonly IPushNotificationService _notifications; // used in release mode, don't remove
    private readonly ITelemetry _telemetry;
    private readonly IAppStoreRatings _appStoreRatings;
    private readonly IQuickResumeService _quickResumeService;
    private readonly IBackgroundTaskService _backgroundTaskService;
    private readonly IIapService _iapService;
    private readonly IUriLauncher _uriLauncher;
    private readonly IAppStoreUpdater _storeUpdater;
    private readonly ISystemInfoProvider _systemInfoProvider; // used in release mode, don't remove
    private readonly IDialogService _dialogService;
    private bool _notificationsLoading;

    public SettingsViewModel(
        IUserSettings userSettings,
        IPushNotificationService notifications,
        ITelemetry telemetry,
        IAssetsReader assetsReader,
        IImagePicker imagePicker,
        IAppStoreRatings appStoreRatings,
        IQuickResumeService quickResumeService,
        IBackgroundTaskService backgroundTaskService,
        ISystemInfoProvider systemInfoProvider,
        ILocalizer localizer,
        IIapService iapService,
        IUriLauncher uriLauncher,
        IAppStoreUpdater appStoreUpdater,
        IDialogService dialogService)
    {
        _userSettings = userSettings;
        _notifications = notifications;
        _telemetry = telemetry;
        _assetsReader = assetsReader;
        _imagePicker = imagePicker;
        _appStoreRatings = appStoreRatings;
        _quickResumeService = quickResumeService;
        _backgroundTaskService = backgroundTaskService;
        _iapService = iapService;
        _uriLauncher = uriLauncher;
        _storeUpdater = appStoreUpdater;
        _systemInfoProvider = systemInfoProvider;
        _dialogService = dialogService;

        if (systemInfoProvider.IsOnBatterySaver())
        {
            BackgroundImageDescription = "🥰 " + localizer.GetString("SettingsBackgroundDescription");
        }

        _xboxDisplayModeSelectedIndex = (int)GetEnum(UserSettingsConstants.XboxSlideshowModeKey, SlideshowMode.Images);
        _channelTimerModeIndex = (int)GetEnum(UserSettingsConstants.ChannelTimerModeKey, ChannelTimerMode.None);

        DeviceId = _userSettings.Get<string>(UserSettingsConstants.LocalUserIdKey) ?? string.Empty;
    }

    [ObservableProperty]
    private string _deviceId = string.Empty;

    [ObservableProperty]
    private bool _updateBarVisible;

    [ObservableProperty]
    private bool _manageSubscriptionVisible;

    [ObservableProperty]
    private string _backgroundImageDescription = string.Empty;

    [ObservableProperty]
    private int _xboxDisplayModeSelectedIndex;

    [ObservableProperty]
    private bool _promoCodeVisible;

    /// <summary>
    /// Determines the selected index for the channel timer mode.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ChannelCountdownEnabled))]
    [NotifyPropertyChangedFor(nameof(ChannelFocusEnabled))]
    private int _channelTimerModeIndex;

    /// <summary>
    /// Paths to available background images.
    /// </summary>
    public ObservableCollection<string> ImagePaths { get; } = [];

    /// <summary>
    /// The current theme.
    /// </summary>
    public string CurrentTheme
    {
        get => _userSettings.Get<string>(UserSettingsConstants.Theme) ?? string.Empty;
        set
        {
            _userSettings.Set(UserSettingsConstants.Theme, value);
            OnPropertyChanged(nameof(CurrentThemeIndex));
        }
    }

    public int CurrentThemeIndex => CurrentTheme switch
    {
        "default" => 0,
        "dark" => 1,
        "light" => 2,
        _ => 0
    };

    /// <summary>
    /// Settings flag for telemetry.
    /// </summary>
    public bool TelemetryEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.TelemetryOn);
        set
        {
            _userSettings.Set(UserSettingsConstants.TelemetryOn, value);
            _telemetry.SetEnabled(value);
        }
    }

    /// <summary>
    /// Settings flag for resume on launch.
    /// </summary>
    public bool ResumeOnLaunchEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.ResumeOnLaunchKey);
        set => _userSettings.Set(UserSettingsConstants.ResumeOnLaunchKey, value);
    }

    /// <summary>
    /// Settings flag for resume on launch.
    /// </summary>
    public bool AmbieMiniEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.CompactOnFocusKey);
        set => _userSettings.Set(UserSettingsConstants.CompactOnFocusKey, value);
    }

    /// <summary>
    /// Determines if the countdown timer on the channel viewer page is enabled.
    /// </summary>
    public bool ChannelCountdownEnabled => ((ChannelTimerMode)ChannelTimerModeIndex) is ChannelTimerMode.Countdown;

    /// <summary>
    /// Determines if the focus timer on the channel viewer page is enabled.
    /// </summary>
    public bool ChannelFocusEnabled => ((ChannelTimerMode)ChannelTimerModeIndex) is ChannelTimerMode.Focus;

    public bool ChannelClockEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.ChannelClockEnabledKey);
        set
        {
            _userSettings.Set(UserSettingsConstants.ChannelClockEnabledKey, value);
            OnPropertyChanged();
            _telemetry.TrackEvent(value
                ? TelemetryConstants.ChannelViewerClockEnabled
                : TelemetryConstants.ChannelViewerClockDisabled);
        }
    }

    public string ChannelClockPreview => DateTime.Now.ToShortTimeString();

    public bool StreaksReminderEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.StreaksReminderEnabledKey);
        set
        {
            _userSettings.Set(UserSettingsConstants.StreaksReminderEnabledKey, value);
            OnStreaksReminderToggled(value);
        }
    }

    private async void OnStreaksReminderToggled(bool value)
    {
        if (value)
        {
            var permissionGranted = await _backgroundTaskService.RequestPermissionAsync();
            if (!permissionGranted)
            {
                StreaksReminderEnabled = false;
                OnPropertyChanged(nameof(StreaksReminderEnabled));
            }
            else
            {
                _backgroundTaskService.ToggleStreakReminderTask(true);
            }
        }
        else
        {
            _backgroundTaskService.ToggleStreakReminderTask(false);
        }
    }

    public bool QuickResumeEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.QuickResumeKey);
        set
        {
            _userSettings.Set(UserSettingsConstants.QuickResumeKey, value);
            OnQuickResumeToggled(value);
        }
    }

    private async void OnQuickResumeToggled(bool value)
    {
        if (value)
        {
            var enabled = await _quickResumeService.TryEnableAsync();
            if (!enabled)
            {
                QuickResumeEnabled = false;
                OnPropertyChanged(nameof(QuickResumeEnabled));
                // TODO: this experience isn't great.
                // it will only be hit when the user explicitly disables the
                // background task settings in ambie's os settings.
                // This means when the user turned off bg task, this checkbox toggle
                // doesn't work at all. So we shouldn't even try to show it.
                // We should set the IsEnabled to false when the user doesn't provide the permissions
                // Then we should add a hyperlink underneath saying enable background task
            }
        }
        else
        {
            _quickResumeService.Disable();
        }
    }

    public bool PlayAfterFocusEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.PlayAfterFocusKey);
        set => _userSettings.Set(UserSettingsConstants.PlayAfterFocusKey, value);
    }

    /// <summary>
    /// Settings flag for notifications.
    /// </summary>
    public bool NotificationsEnabled
    {
        get => _userSettings.Get<bool>(UserSettingsConstants.Notifications);
        set => SetNotifications(value);
    }

    public async Task InitializeAsync()
    {
        ManageSubscriptionVisible = await _iapService.IsSubscriptionOwnedAsync();
        PromoCodeVisible = await _iapService.CanShowPremiumButtonsAsync();
    }

    private TEnum GetEnum<TEnum>(string settingsKey, TEnum defaultValue) where TEnum : struct
    {
        string? rawStringValue = _userSettings.Get<string>(settingsKey);
        return Enum.TryParse(rawStringValue, out TEnum result) ? result : defaultValue;
    }

    public void Uninitialize()
    {
    }

    private async void SetNotifications(bool value)
    {
        if (value == NotificationsEnabled || _notificationsLoading)
        {
            return;
        }

        _notificationsLoading = true;

        string? deviceId = _userSettings.Get<string>(UserSettingsConstants.LocalUserIdKey);

        if (deviceId is { Length: > 0 } id)
        {
#if DEBUG
            await Task.Delay(1);
#else
            if (value)
            {
                _backgroundTaskService.TogglePushNotificationRenewalTask(true);
                await _notifications.RegisterAsync(id, _systemInfoProvider.GetCulture(), default);
            }
            else
            {
                _backgroundTaskService.TogglePushNotificationRenewalTask(false);
                await _notifications.UnregisterAsync(id, default);
            }
#endif
            _userSettings.Set(UserSettingsConstants.Notifications, value);
        }
        _notificationsLoading = false;
    }

    public void UpdateTheme(string newTheme)
    {
        if (newTheme is "default" or "dark" or "light")
        {
            CurrentTheme = newTheme;
        }
    }

    [RelayCommand]
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

    [RelayCommand]
    private async Task BrowseAsync()
    {
        string? imagePath = await _imagePicker.BrowseAsync();
        if (imagePath == null)
        {
            return;
        }

        SelectImage(imagePath);
    }

    [RelayCommand]
    private void SelectImage(string? imagePath)
    {
        if (imagePath?.Contains(NoneImageName) == true)
        {
            imagePath = string.Empty;
        }

        _userSettings.Set(UserSettingsConstants.BackgroundImage, imagePath);
    }

    [RelayCommand]
    private async Task RequestRatingAsync()
    {
        bool result = await _appStoreRatings.RequestInAppRatingsAsync();
        if (result)
        {
            _userSettings.Set(UserSettingsConstants.HasRated, true);
        }

        _telemetry.TrackEvent(TelemetryConstants.SettingsRateUsClicked);
    }

    [RelayCommand]
    private async Task RedirectToMsAccountAsync()
    {
        await _uriLauncher.LaunchUriAsync(new Uri("https://account.microsoft.com/services"));
        _telemetry.TrackEvent(TelemetryConstants.SettingsModifySubscriptionClicked);
    }

    [RelayCommand]
    private async Task RedirectToFeedbackFormAsync()
    {
        await _uriLauncher.LaunchUriAsync(new Uri("https://forms.office.com/r/VaPaEQWvx6"));
        _telemetry.TrackEvent(TelemetryConstants.FeedbackClicked);
    }

    [RelayCommand]
    private async Task TryUpdateAsync()
    {
        if (UpdateBarVisible)
        {
            return;
        }

        _telemetry.TrackEvent(TelemetryConstants.CheckForUpdatesClicked);

        var updateAvailable = await _storeUpdater.CheckForUpdatesAsync();

        if (!updateAvailable)
        {
            return;
        }

        UpdateBarVisible = true;
        await _storeUpdater.TryApplyUpdatesAsync();
        UpdateBarVisible = false;
    }

    [RelayCommand]
    private async Task EnterPromoCodeAsync()
    {
        await _dialogService.OpenPremiumAsync(launchPromoCodeDirectly: true);
        PromoCodeVisible = await _iapService.CanShowPremiumButtonsAsync();
    }

    partial void OnXboxDisplayModeSelectedIndexChanged(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex)
        {
            return;
        }

        if (Enum.GetNames(typeof(SlideshowMode)).Length > newIndex)
        {
            var mode = (SlideshowMode)newIndex;
            _userSettings.Set(UserSettingsConstants.XboxSlideshowModeKey, mode.ToString());

            _telemetry.TrackEvent(TelemetryConstants.XboxSlideshowModeChanged, new Dictionary<string, string>
            {
                { "mode", mode.ToString() }
            });
        }
    }

    partial void OnChannelTimerModeIndexChanged(int oldIndex, int newIndex)
    {
        if (oldIndex == newIndex)
        {
            return;
        }

        if (Enum.GetNames(typeof(ChannelTimerMode)).Length > newIndex && newIndex >= 0)
        {
            var mode = (ChannelTimerMode)newIndex;
            _userSettings.Set(UserSettingsConstants.ChannelTimerModeKey, mode.ToString());

            //_telemetry.TrackEvent(TelemetryConstants.XboxSlideshowModeChanged, new Dictionary<string, string>
            //{
            //    { "mode", mode.ToString() }
            //});
        }
    }
}