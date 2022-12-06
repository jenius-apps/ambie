using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

/// <summary>
/// ViewModel for the shell page.
/// </summary>
public partial class ShellPageViewModel : ObservableObject
{
    private readonly IUserSettings _userSettings;
    private readonly ITimerService _ratingTimer;
    private readonly ITelemetry _telemetry;
    private readonly INavigator _navigator;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly IFocusService _focusService;
    private readonly ISoundMixService _soundMixService;
    private bool _isRatingMessageVisible;
    private bool _premiumButtonVisible;
    private bool _focusTimeBannerVisible;
    private bool _focusDotVisible;

    [ObservableProperty]
    private bool _titleBarVisible;

    [ObservableProperty]
    private int _navMenuIndex = -1;

    public ShellPageViewModel(
        IUserSettings userSettings,
        ITimerService timer,
        ITelemetry telemetry,
        ISystemInfoProvider systemInfoProvider,
        INavigator navigator,
        IDialogService dialogService,
        IIapService iapService,
        IFocusService focusService,
        ISoundMixService soundMixService)
    {
        Guard.IsNotNull(userSettings);
        Guard.IsNotNull(timer);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(navigator);
        Guard.IsNotNull(dialogService);
        Guard.IsNotNull(iapService);
        Guard.IsNotNull(focusService);
        Guard.IsNotNull(soundMixService);

        _userSettings = userSettings;
        _ratingTimer = timer;
        _telemetry = telemetry;
        _navigator = navigator;
        _dialogService = dialogService;
        _iapService = iapService;
        _focusService = focusService;
        _soundMixService = soundMixService;

        var lastDismissDateTime = _userSettings.GetAndDeserialize(UserSettingsConstants.RatingDismissed, AmbieJsonSerializerContext.Default.DateTime);
        if (!systemInfoProvider.IsFirstRun() &&
            !systemInfoProvider.IsTenFoot() &&
            !_userSettings.Get<bool>(UserSettingsConstants.HasRated) &&
            lastDismissDateTime.AddDays(7) <= DateTime.UtcNow)
        {
            _ratingTimer.Interval = 1800000; // 30 minutes
            _ratingTimer.IntervalElapsed += OnIntervalLapsed;
            _ratingTimer.Start();
        }
    }

    /// <summary>
    /// Determines whether or not the focus
    /// dot is visible.
    /// </summary>
    public bool FocusDotVisible
    {
        get => _focusDotVisible;
        set => SetProperty(ref _focusDotVisible, value);
    }

    /// <summary>
    /// Determines if the current mix can be saved or not.
    /// </summary>
    public bool CanSaveMix => _soundMixService.CanSaveCurrentMix();

    /// <summary>
    /// Determines whether or not the focus 
    /// time banner control is visible on the page.
    /// </summary>
    public bool FocusTimeBannerVisibile
    {
        get => _focusTimeBannerVisible;
        set => SetProperty(ref _focusTimeBannerVisible, value);
    }
        
    /// <summary>
    /// Determines if the rating message is visible.
    /// </summary>
    public bool IsRatingMessageVisible
    {
        get => _isRatingMessageVisible;
        set => SetProperty(ref _isRatingMessageVisible, value);
    }

    public bool PremiumButtonVisible
    {
        get => _premiumButtonVisible;
        set => SetProperty(ref _premiumButtonVisible, value);
    }

    /// <summary>
    /// Path to background image.
    /// </summary>
    public string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

    /// <summary>
    /// Determines if the background image should be shown.
    /// </summary>
    public bool ShowBackgroundImage => !string.IsNullOrWhiteSpace(BackgroundImagePath);

    public void UpdateCanSave()
    {
        OnPropertyChanged(nameof(CanSaveMix));
    }

    public void Navigate(ContentPageType pageType)
    {
        if (NavMenuIndex == (int)pageType)
        {
            return;
        }

        if (pageType == ContentPageType.Home)
        {
            _navigator.ToHome();
        }
        else if (pageType == ContentPageType.Catalogue)
        {
            _navigator.ToCatalogue();
        }
        else if (pageType == ContentPageType.Focus)
        {
            _navigator.ToFocus();
        }

        UpdateTimeBannerVisibility();
        UpdateFocusDotVisibility();

        NavMenuIndex = (int)pageType;
    }

    public async void OpenPremiumDialog()
    {
        _telemetry.TrackEvent(TelemetryConstants.ShellPagePremiumClicked);
        await _dialogService.OpenPremiumAsync();
    }

    public async void OpenThemeSettings() => await _dialogService.OpenThemeSettingsAsync();

    public void GoToScreensaver()
    {
        _telemetry.TrackEvent(TelemetryConstants.ScreensaverTriggered, new Dictionary<string, string>()
        {
            { "trigger", "mainPage" }
        });

        _navigator.ToScreensaver();
    }

    public async Task InitializeAsync(ShellPageNavigationArgs? args = null)
    {
        _iapService.ProductPurchased += OnProductPurchased;
        _userSettings.SettingSet += OnSettingSet;
        _focusService.FocusStateChanged += OnFocusStateChanged;
        _navigator.ContentPageChanged += OnContentPageChanged;

        await LoadPremiumButtonAsync();

        if (args is not null)
        {
            TitleBarVisible = !args.IsGameBarWidget;
        }
    }

    private void OnContentPageChanged(object sender, ContentPageType e)
    {
        // This ensures that the nav menu selects the correct
        // index. Previously, it could become out of sync when the content page
        // is changed programmatically (rather than the user selecting the nav item).
        if (NavMenuIndex != (int)e)
        {
            NavMenuIndex = (int)e;
            UpdateTimeBannerVisibility();
            UpdateFocusDotVisibility();
        }
    }

    public void Dispose()
    {
        _userSettings.SettingSet -= OnSettingSet;
        _iapService.ProductPurchased -= OnProductPurchased;
        _focusService.FocusStateChanged -= OnFocusStateChanged;
        _navigator.ContentPageChanged -= OnContentPageChanged;
    }

    private async Task LoadPremiumButtonAsync()
    {
        PremiumButtonVisible = !await _iapService.IsOwnedAsync(IapConstants.MsStoreAmbiePlusId);
    }

    private void OnIntervalLapsed(object sender, TimeSpan e)
    {
        _ratingTimer.Stop();
        _ratingTimer.IntervalElapsed -= OnIntervalLapsed;
        IsRatingMessageVisible = true;
        _telemetry.TrackEvent(TelemetryConstants.RatingMessageShown);
    }

    private void OnSettingSet(object sender, string settingsKey)
    {
        if (settingsKey == UserSettingsConstants.BackgroundImage)
        {
            OnPropertyChanged(nameof(ShowBackgroundImage));
            OnPropertyChanged(nameof(BackgroundImagePath));
        }
    }

    private void OnProductPurchased(object sender, string iapId)
    {
        if (iapId == IapConstants.MsStoreAmbiePlusId)
        {
            PremiumButtonVisible = false;
        }
    }

    private void OnFocusStateChanged(object sender, FocusState e)
    {
        UpdateTimeBannerVisibility();
        UpdateFocusDotVisibility();
    }

    private void UpdateTimeBannerVisibility()
    {
        FocusTimeBannerVisibile =
            _navigator.GetContentPageName() != "FocusPage" &&
            _focusService.CurrentState != FocusState.None;
    }

    private void UpdateFocusDotVisibility()
    {
        FocusDotVisible = _focusService.CurrentState != FocusState.None;
    }
}
