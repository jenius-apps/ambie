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
    private const int RatingsTimerInterval = 1800000; // 30 minutes
    private readonly IUserSettings _userSettings;
    private readonly ITimerService _ratingTimer;
    private readonly ITelemetry _telemetry;
    private readonly INavigator _navigator;
    private readonly IDialogService _dialogService;
    private readonly IIapService _iapService;
    private readonly ISoundMixService _soundMixService;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IShareService _shareService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly ISoundService _soundService;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly ISearchService _searchService;
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
        ISoundMixService soundMixService,
        IMixMediaPlayerService mixMediaPlayerService,
        IShareService shareService,
        IDispatcherQueue dispatcherQueue,
        ILocalizer localizer,
        ISoundService soundService,
        IAssetLocalizer assetLocalizer,
        ISearchService searchService,
        IAppStoreUpdater appStoreUpdater,
        IExperimentationService experimentationService)
    {
        IsWin11 = systemInfoProvider.IsWin11();

        _userSettings = userSettings;
        _ratingTimer = timer;
        _telemetry = telemetry;
        _navigator = navigator;
        _dialogService = dialogService;
        _iapService = iapService;
        _soundMixService = soundMixService;
        _mixMediaPlayerService = mixMediaPlayerService;
        _shareService = shareService;
        _dispatcherQueue = dispatcherQueue;
        _soundService = soundService;
        _assetLocalizer = assetLocalizer;
        _searchService = searchService;
        _appStoreUpdater = appStoreUpdater;
        _systemInfoProvider = systemInfoProvider;

        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("Home"), "\uE10F", ContentPageType.Home.ToString(), tooltipSubtitle: localizer.GetString("HomeSubtitle")));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("Catalogue"), "\uEC4F", ContentPageType.Catalogue.ToString(), tooltipSubtitle: localizer.GetString("CatalogueSubtitle")));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("FocusText"), "\uF272", ContentPageType.Focus.ToString(), tooltipSubtitle: localizer.GetString("FocusSubtitle")));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("ChannelsTitleText"), "\uE8B2", ContentPageType.Channels.ToString(), tooltipSubtitle: localizer.GetString("ChannelsSubtitle")));
        if (experimentationService.IsEnabled(ExperimentConstants.RelaxPageV2)) { MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("RelaxText"), "\uEC0A", ContentPageType.Meditate.ToString(), tooltipSubtitle: localizer.GetString("MeditateSubtitle"))); }
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("StatsTitleText"), "\uEAFC", ContentPageType.Stats.ToString(), tooltipSubtitle: localizer.GetString("StatsSubtitle")));
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
    [NotifyPropertyChangedFor(nameof(SidePanelMica))]
    private bool _isWin11;

    public bool SidePanelMica => IsWin11;

    [ObservableProperty]
    private bool _isRatingMessageVisible;

    [ObservableProperty]
    private bool _isFreeTrialMessageVisible;

    [ObservableProperty]
    private bool _premiumButtonVisible;

    [ObservableProperty]
    private bool _isMissingSoundsMessageVisible;

    [ObservableProperty]
    private bool _updateButtonVisible;

    public ObservableCollection<MenuItem> MenuItems { get; } = [];

    public ObservableCollection<MenuItem> FooterItems { get; } = [];

    public ObservableCollection<DayActivityViewModel> RecentActivity { get; } = [];

    [ObservableProperty]
    public IReadOnlyList<AutosuggestSound> _searchAutosuggestItems = [];

    public bool CanSaveMix => _soundMixService.CanSaveCurrentMix();

    public string BackgroundImagePath => _userSettings.Get<string>(UserSettingsConstants.BackgroundImage) ?? string.Empty;

    public bool ShowBackgroundImage => !string.IsNullOrEmpty(BackgroundImagePath);

    public void UpdateCanSave()
    {
        OnPropertyChanged(nameof(CanSaveMix));
    }

    public void Navigate(ContentPageType pageType, string? contentPageNavArgs = null)
    {
        _navigator.NavigateTo(pageType, contentPageNavArgs);
        UpdateSelectedMenu(pageType);
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
        _navigator.ContentPageChanged += OnContentPageChanged;
        _shareService.ShareFailed += OnShareFailed;

        _ = CheckForUpdatesAsync();
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
        _navigator.ContentPageChanged -= OnContentPageChanged;
        _shareService.ShareFailed -= OnShareFailed;
    }

    private void OnShareFailed(object sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsMissingSoundsMessageVisible = true;
        });

        _telemetry.TrackEvent(TelemetryConstants.MissingSoundsMessageShown);
    }

    [RelayCommand]
    private async Task OpenMissingDialogAsync()
    {
        IsMissingSoundsMessageVisible = false;
        _telemetry.TrackEvent(TelemetryConstants.MissingSoundsMessageClicked);
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

    private async void OnProductPurchased(object sender, string iapId)
    {
        PremiumButtonVisible = await _iapService.CanShowPremiumButtonsAsync();
    }

    [RelayCommand]
    private async Task ApplyUpdatesAsync()
    {
        _telemetry.TrackEvent(TelemetryConstants.UpdateClicked);
        await _appStoreUpdater.TrySilentDownloadAndInstallAsync();
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
