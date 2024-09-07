using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Models;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using INavigator = AmbientSounds.Services.INavigator;

namespace AmbientSounds.ViewModels;

/// <summary>
/// ViewModel for the shell page.
/// </summary>
public partial class ShellPageViewModel : ObservableObject
{
    private const int LastDaysStreak = 7;
    private const int RatingsTimerInterval = 1800000; // 30 minutes
    private readonly IUserSettings _userSettings;
    private readonly ITimerService _ratingTimer;
    private readonly ITelemetry _telemetry;
    private readonly INavigator _navigator;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly IFocusService _focusService;
    private readonly ISoundMixService _soundMixService;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IShareService _shareService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly IGuideService _guideService;
    private readonly ISoundService _soundService;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly ISearchService _searchService;
    private readonly IStatService _statService;
    private readonly ILocalizer _localizer;
    private readonly IAppStoreUpdater _appStoreUpdater;
    private readonly ISystemInfoProvider _systemInfoProvider;

    public ShellPageViewModel(
        IUserSettings userSettings,
        ITimerService timer,
        ITelemetry telemetry,
        ISystemInfoProvider systemInfoProvider,
        INavigator navigator,
        IDialogService dialogService,
        IIapService iapService,
        IFocusService focusService,
        ISoundMixService soundMixService,
        IMixMediaPlayerService mixMediaPlayerService,
        IShareService shareService,
        IGuideService guideService,
        IDispatcherQueue dispatcherQueue,
        ILocalizer localizer,
        ISoundService soundService,
        IAssetLocalizer assetLocalizer,
        ISearchService searchService,
        IStatService statService,
        IAppStoreUpdater appStoreUpdater)
    {
        IsWin11 = systemInfoProvider.IsWin11();
        IsMeditatePageVisible = systemInfoProvider.GetCulture().ToLower().Contains("en");

        _userSettings = userSettings;
        _ratingTimer = timer;
        _telemetry = telemetry;
        _navigator = navigator;
        _dialogService = dialogService;
        _iapService = iapService;
        _focusService = focusService;
        _soundMixService = soundMixService;
        _mixMediaPlayerService = mixMediaPlayerService;
        _shareService = shareService;
        _dispatcherQueue = dispatcherQueue;
        _guideService = guideService;
        _soundService = soundService;
        _assetLocalizer = assetLocalizer;
        _searchService = searchService;
        _statService = statService;
        _localizer = localizer;
        _appStoreUpdater = appStoreUpdater;
        _systemInfoProvider = systemInfoProvider;

        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("Home"), "\uE10F", ContentPageType.Home.ToString(), tooltipSubtitle: localizer.GetString("HomeSubtitle")));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("Catalogue"), "\uEC4F", ContentPageType.Catalogue.ToString(), tooltipSubtitle: localizer.GetString("CatalogueSubtitle")));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("FocusText"), "\uF272", ContentPageType.Focus.ToString(), tooltipSubtitle: localizer.GetString("FocusSubtitle")));
        if (IsMeditatePageVisible) { MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("RelaxText"), "\uEC0A", ContentPageType.Meditate.ToString(), tooltipSubtitle: localizer.GetString("MeditateSubtitle"))); }
        FooterItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("UpdatesText"), "\uE118", ContentPageType.Updates.ToString(), tooltipSubtitle: localizer.GetString("UpdatesSubtitle")));
        FooterItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("SettingsText"), "\uE713", ContentPageType.Settings.ToString(), tooltipSubtitle: localizer.GetString("SettingsSubtitle")));

        var lastDismissDateTime = _userSettings.GetAndDeserialize(UserSettingsConstants.RatingDismissed, AmbieJsonSerializerContext.Default.DateTime);
        var isNotFirstRun = !systemInfoProvider.IsFirstRun();
        var isDesktop = systemInfoProvider.GetDeviceFamily() == "Windows.Desktop";
        var hasNotBeenRated = !_userSettings.Get<bool>(UserSettingsConstants.HasRated);
        var pastlastDismiss = lastDismissDateTime.AddDays(30) <= DateTime.UtcNow;
        if (isNotFirstRun &&
            isDesktop &&
            hasNotBeenRated &&
            pastlastDismiss)
        {
            _ratingTimer.Interval = RatingsTimerInterval;
            _ratingTimer.IntervalElapsed += OnIntervalLapsed;
            _ratingTimer.Start();
        }
    }

    [ObservableProperty]
    private int _streakCount;

    [ObservableProperty]
    private string _streakText = string.Empty;

    [ObservableProperty]
    private bool _showStreak;

    [ObservableProperty]
    private bool _newStreakExperience;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SidePanelMica))]
    private bool _isWin11;

    public bool SidePanelMica => IsWin11;

    [ObservableProperty]
    private bool _guideBannerVisible;

    [ObservableProperty]
    private bool _isRatingMessageVisible;

    [ObservableProperty]
    private bool _isFreeTrialMessageVisible;

    [ObservableProperty]
    private bool _premiumButtonVisible;

    [ObservableProperty]
    private bool _focusTimeBannerVisible;


    [ObservableProperty]
    private bool _isMissingSoundsMessageVisible;

    [ObservableProperty]
    private bool _isMeditatePageVisible;

    [ObservableProperty]
    private bool _updateButtonVisible;

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    public ObservableCollection<MenuItem> FooterItems { get; } = new();

    public ObservableCollection<DayActivityViewModel> RecentActivity { get; } = new();

    [ObservableProperty]
    public IReadOnlyList<AutosuggestSound> _searchAutosuggestItems = Array.Empty<AutosuggestSound>();

    public bool CanSaveMix => _soundMixService.CanSaveCurrentMix();

    public string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage);

    public bool ShowBackgroundImage => !string.IsNullOrWhiteSpace(BackgroundImagePath);

    public void UpdateCanSave()
    {
        OnPropertyChanged(nameof(CanSaveMix));
    }

    public void Navigate(ContentPageType pageType, string? contentPageNavArgs = null)
    {
        _navigator.NavigateTo(pageType, contentPageNavArgs);
        UpdateSelectedMenu(pageType);
        UpdateTimeBannerVisibility();
        UpdateGuideBannerVisibility();
    }

    [RelayCommand]
    private async Task OpenShareAsync()
    {
        if (_mixMediaPlayerService.GetSoundIds() is { Length: > 0 } ids)
        {
            var sorted = ids.OrderBy(x => x).ToArray();
            _telemetry.TrackEvent(TelemetryConstants.ShellPageShareClicked, new Dictionary<string, string>
            {
                { "ids", string.Join(",", sorted) }
            });
            await _dialogService.OpenShareAsync(ids);
        }
    }

    [RelayCommand]
    private async Task OpenPremiumAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.ShellPagePremiumClicked, new Dictionary<string, string>
        {
            { "viaFreeTrialTip", IsFreeTrialMessageVisible.ToString().ToLower()  }
        });

        IsFreeTrialMessageVisible = false;
        await _dialogService.OpenPremiumAsync();
    }

    public void GoToScreensaver()
    {
        _telemetry.TrackEvent(TelemetryConstants.ScreensaverTriggered, new Dictionary<string, string>()
        {
            { "trigger", "mainPage" }
        });

        _navigator.ToScreensaver();
    }

    [RelayCommand]
    private async Task PlayRandomSoundAsync()
    {
        await _mixMediaPlayerService.PlayRandomAsync();
        _telemetry.TrackEvent(TelemetryConstants.ShuffleClicked);
    }

    public async Task InitializeAsync()
    {
        _iapService.ProductPurchased += OnProductPurchased;
        _userSettings.SettingSet += OnSettingSet;
        _focusService.FocusStateChanged += OnFocusStateChanged;
        _navigator.ContentPageChanged += OnContentPageChanged;
        _shareService.ShareFailed += OnShareFailed;
        _guideService.GuideStarted += OnGuideStarted;
        _guideService.GuideStopped += OnGuideStopped;
        _statService.StreakChanged += OnStreakChanged;

        _ = CheckForUpdatesAsync();
        LoadStreak();
        await LoadPremiumContentAsync();
    }

    private async Task CheckForUpdatesAsync()
    {
        var hasUpdates = await _appStoreUpdater.CheckForUpdatesAsync();
        if (!hasUpdates)
        {
            return;
        }

        var downloadSuccessful = await _appStoreUpdater.TrySilentDownloadAsync();
        if (downloadSuccessful)
        {
            UpdateButtonVisible = true;
            _telemetry.TrackEvent(TelemetryConstants.UpdateShown);
        }
    }

    public void Uninitialize()
    {
        _ratingTimer.Stop();
        _ratingTimer.IntervalElapsed -= OnIntervalLapsed;
        _userSettings.SettingSet -= OnSettingSet;
        _iapService.ProductPurchased -= OnProductPurchased;
        _focusService.FocusStateChanged -= OnFocusStateChanged;
        _navigator.ContentPageChanged -= OnContentPageChanged;
        _shareService.ShareFailed -= OnShareFailed;
        _guideService.GuideStarted -= OnGuideStarted;
        _guideService.GuideStopped -= OnGuideStopped;
        _statService.StreakChanged -= OnStreakChanged;
    }

    private void OnStreakChanged(object sender, StreakChangedEventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            LoadStreak(e);
        });
    }

    private void OnShareFailed(object sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsMissingSoundsMessageVisible = true;
        });
    }

    public void LoadStreak(StreakChangedEventArgs? args = null)
    {
        int count = args?.NewStreak ?? _statService.ValidateAndRetrieveStreak();

        StreakText = count == 1
            ? _localizer.GetString("DaySingular")
            : _localizer.GetString("DayPlural", count.ToString());

        StreakCount = count;
        NewStreakExperience = args?.AnimationRecommended ?? false;
        ShowStreak = count > 0;
    }

    public async Task LoadRecentActivityAsync()
    {
        var recent = await _statService.GetRecentActiveHistory(LastDaysStreak);
        DateTime tempDate = DateTime.Now.AddDays((LastDaysStreak - 1) * -1).Date;
        RecentActivity.Clear();
        foreach (var x in recent)
        {
            RecentActivity.Add(new DayActivityViewModel
            {
                Active = x,
                Date = tempDate
            });

            tempDate = tempDate.AddDays(1);
        }
    }

    [RelayCommand]
    private async Task OpenMissingDialogAsync()
    {
        IsMissingSoundsMessageVisible = false;
        await _dialogService.MissingShareSoundsDialogAsync();
    }

    [RelayCommand]
    private void DismissMissingDialog()
    {
        IsMissingSoundsMessageVisible = false;
    }

    private void OnContentPageChanged(object sender, ContentPageType e)
    {
        UpdateSelectedMenu(e);
        UpdateTimeBannerVisibility();
        UpdateGuideBannerVisibility();

        if (e is not ContentPageType.Search)
        {
            _userSettings.Set(UserSettingsConstants.LastUsedContentPageKey, e.ToString());
        }
    }

    private async Task LoadPremiumContentAsync()
    {
        PremiumButtonVisible = await _iapService.CanShowPremiumButtonsAsync();

        _telemetry.TrackEvent(PremiumButtonVisible
            ? TelemetryConstants.LaunchUserFreeTier
            : TelemetryConstants.LaunchUserPremiumTier);

        if (PremiumButtonVisible
            && _ratingTimer.Interval != RatingsTimerInterval // to avoid multiple tips, we check if the ratings tip is scheduled
            && !_systemInfoProvider.IsFirstRun()
            && !_userSettings.Get<bool>(UserSettingsConstants.HasViewedFreeTrialTipKey)
            && _systemInfoProvider.FirstUseDate().AddDays(1) < DateTime.Now)
        {
            await Task.Delay(1000);
            IsFreeTrialMessageVisible = true;
            _telemetry.TrackEvent(TelemetryConstants.FreeTrialTipShown);
            _userSettings.Set(UserSettingsConstants.HasViewedFreeTrialTipKey, true);
        }
    }

    private void OnIntervalLapsed(object sender, TimeSpan e)
    {
        _ratingTimer.Stop();
        _ratingTimer.IntervalElapsed -= OnIntervalLapsed;
        _dispatcherQueue.TryEnqueue(() => { IsRatingMessageVisible = true; });
        _telemetry.TrackEvent(TelemetryConstants.RatingMessageShown);
    }

    private void OnSettingSet(object sender, string settingsKey)
    {
        if (settingsKey == UserSettingsConstants.BackgroundImage)
        {
            OnPropertyChanged(nameof(ShowBackgroundImage));
            OnPropertyChanged(nameof(BackgroundImagePath));
            OnPropertyChanged(nameof(SidePanelMica));
        }
    }

    private void OnProductPurchased(object sender, string iapId)
    {
        if (iapId is IapConstants.MsStoreAmbiePlusId 
            or IapConstants.MsStoreAmbiePlusLifetimeId)
        {
            PremiumButtonVisible = false;
        }
    }

    private void OnFocusStateChanged(object sender, FocusState e)
    {
        _dispatcherQueue.TryEnqueue(UpdateTimeBannerVisibility);
    }

    private void UpdateTimeBannerVisibility()
    {
        FocusTimeBannerVisible =
            _navigator.GetContentPageName() != "FocusPage" &&
            _focusService.CurrentState != FocusState.None;
    }

    private void UpdateGuideBannerVisibility()
    {
        GuideBannerVisible =
            _navigator.GetContentPageName() != "MeditatePage" &&
            _mixMediaPlayerService.CurrentGuideId is { Length: > 0 };
    }

    [RelayCommand]
    private async Task ApplyUpdatesAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.UpdateClicked);
        await _appStoreUpdater.TrySilentDownloadAndInstallAsync();
    }

    [RelayCommand]
    private void HandleTitleBanner()
    {
        if (FocusTimeBannerVisible)
        {
            Navigate(ContentPageType.Focus);
        }
        else if (GuideBannerVisible)
        {
            Navigate(ContentPageType.Meditate);
        }
    }

    private void OnGuideStopped(object sender, string e)
    {
        _dispatcherQueue.TryEnqueue(UpdateGuideBannerVisibility);
    }

    private void OnGuideStarted(object sender, string e)
    {
        _dispatcherQueue.TryEnqueue(UpdateGuideBannerVisibility);
    }

    [RelayCommand]
    private void NavigateToPage(MenuItem? menuItem)
    {
        if (menuItem?.Tag is null)
        {
            return;
        }

        if (Enum.TryParse(menuItem.Tag, out ContentPageType pageType))
        {
            Navigate(pageType);
        }
    }

    private void UpdateSelectedMenu(ContentPageType pageType)
    {
        foreach (var item in MenuItems)
        {
            item.IsSelected = pageType.ToString() == item.Tag;
        }

        foreach (var item in FooterItems)
        {
            item.IsSelected = pageType.ToString() == item.Tag;
        }
    }

    public async Task FilterAutosuggestAsync(string input)
    {
        if (input.Length < 3 || string.IsNullOrWhiteSpace(input))
        {
            SearchAutosuggestItems = Array.Empty<AutosuggestSound>();
            return;
        }

        var sounds = await _soundService.GetLocalSoundsAsync();
        SearchAutosuggestItems = sounds
            .Where(x => _assetLocalizer.LocalNameContains(x, input))
            .Select(x => new AutosuggestSound(_assetLocalizer.GetLocalName(x), x.Id))
            .ToArray();
    }

    public async Task PlayAsync(AutosuggestSound autosuggestSound)
    {
        var sound = await _soundService.GetLocalSoundAsync(autosuggestSound.Id);
        if (sound is not null)
        {
            await _mixMediaPlayerService.ToggleSoundAsync(sound);
            _telemetry.TrackEvent(TelemetryConstants.SearchAutoSuggestClicked, new Dictionary<string, string>
            {
                { "name", sound.Name }
            });
        }
    }

    public void Search(string query)
    {
        _searchService.TriggerSearch(query);
        _telemetry.TrackEvent(TelemetryConstants.SearchQuerySubmitted, new Dictionary<string, string>
        {
            { "query", query }
        });
    }
}

public class AutosuggestSound
{
    public AutosuggestSound(string name, string id)
    {
        Name = name;
        Id = id;
    }

    public string Id { get; init; }

    public string Name { get; init; }

    public override string ToString()
    {
        return Name;
    }
}
