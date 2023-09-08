using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Models;
using JeniusApps.Common.Telemetry;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using INavigator = AmbientSounds.Services.INavigator;
using ISystemInfoProvider = AmbientSounds.Services.ISystemInfoProvider;

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
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IShareService _shareService;
    private readonly IDispatcherQueue _dispatcherQueue;
    private readonly IGuideService _guideService;
    private readonly ISoundService _soundService;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly ISearchService _searchService;

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
        ISearchService searchService)
    {
        IsWin11 = systemInfoProvider.CanUseFluentSystemIcons();
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

        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("Home"), "\uE10F", ContentPageType.Home.ToString()));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("Catalogue"), "\uEC4F", ContentPageType.Catalogue.ToString()));
        MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("FocusText"), "\uF272", ContentPageType.Focus.ToString()));
        if (IsMeditatePageVisible) { MenuItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("RelaxText"), "\uEC0A", ContentPageType.Meditate.ToString())); }
        FooterItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("UpdatesText"), "\uE118", ContentPageType.Updates.ToString()));
        FooterItems.Add(new MenuItem(NavigateToPageCommand, localizer.GetString("SettingsText"), "\uE713", ContentPageType.Settings.ToString()));

        var lastDismissDateTime = _userSettings.GetAndDeserialize(UserSettingsConstants.RatingDismissed, AmbieJsonSerializerContext.Default.DateTime);
        var isNotFirstRun = !systemInfoProvider.IsFirstRun();
        var isDesktop = systemInfoProvider.IsDesktop();
        var hasNotBeenRated = !_userSettings.Get<bool>(UserSettingsConstants.HasRated);
        var pastlastDismiss = lastDismissDateTime.AddDays(30) <= DateTime.UtcNow;
        if (isNotFirstRun &&
            isDesktop &&
            hasNotBeenRated &&
            pastlastDismiss)
        {
            _ratingTimer.Interval = 1800000; // 30 minutes
            _ratingTimer.IntervalElapsed += OnIntervalLapsed;
            _ratingTimer.Start();
        }
    }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SidePanelMica))]
    private bool _isWin11;

    public bool SidePanelMica => IsWin11 && !ShowBackgroundImage;

    [ObservableProperty]
    private bool _guideBannerVisible;

    [ObservableProperty]
    private bool _isRatingMessageVisible;

    [ObservableProperty]
    private bool _premiumButtonVisible;

    [ObservableProperty]
    private bool _focusTimeBannerVisible;


    [ObservableProperty]
    private bool _isMissingSoundsMessageVisible;

    [ObservableProperty]
    private bool _isMeditatePageVisible;

    public ObservableCollection<MenuItem> MenuItems { get; } = new();

    public ObservableCollection<MenuItem> FooterItems { get; } = new();

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
        _telemetry.TrackEvent(TelemetryConstants.ShellPagePremiumClicked);
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

        await LoadPremiumContentAsync();
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
    }

    private void OnShareFailed(object sender, EventArgs e)
    {
        _dispatcherQueue.TryEnqueue(() =>
        {
            IsMissingSoundsMessageVisible = true;
        });

        _telemetry.TrackEvent(TelemetryConstants.ShareFailedMessageShown);
    }

    [RelayCommand]
    private async Task OpenMissingDialogAsync()
    {
        IsMissingSoundsMessageVisible = false;
        _telemetry.TrackEvent(TelemetryConstants.ShareFailedMessageClicked);
        await _dialogService.MissingShareSoundsDialogAsync();
    }

    [RelayCommand]
    private void DismissMissingDialog()
    {
        _telemetry.TrackEvent(TelemetryConstants.ShareFailedMessageDismissed);
        IsMissingSoundsMessageVisible = false;
    }

    private void OnContentPageChanged(object sender, ContentPageType e)
    {
        UpdateSelectedMenu(e);
        UpdateTimeBannerVisibility();
        UpdateGuideBannerVisibility();
    }

    private async Task LoadPremiumContentAsync()
    {
        PremiumButtonVisible = !await _iapService.IsAnyOwnedAsync(new string[] 
        {
            IapConstants.MsStoreAmbiePlusId,
            IapConstants.MsStoreAmbiePlusLifetimeId
        });
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
    private void HandleTitleBanner()
    {
        if (FocusTimeBannerVisible)
        {
            Navigate(ContentPageType.Focus);
            _telemetry.TrackEvent(TelemetryConstants.FocusTimeBannerClicked);
        }
        else if (GuideBannerVisible)
        {
            Navigate(ContentPageType.Meditate);
            _telemetry.TrackEvent(TelemetryConstants.GuideBannerClicked);
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
