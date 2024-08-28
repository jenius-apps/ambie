namespace AmbientSounds.Constants;

/// <summary>
/// List of constants used for
/// telemetry events.
/// </summary>
public class TelemetryConstants
{
    // search
    private const string Search = "search:";
    public const string SearchQuerySubmitted = Search + "querySubmitted";
    public const string SearchEmptyResult = Search + "emptyResult";
    public const string SearchAutoSuggestClicked = Search + "autosuggestClicked";

    // launch
    private const string Launch = "launch:";
    public const string LaunchViaToast = Launch + "toast";
    public const string LaunchUserPremiumTier = Launch + "premiumTierUser";
    public const string LaunchUserFreeTier = Launch + "freeTierUser";

    // shell page
    private const string ShellPage = "shellpage:";
    public const string ShellPagePremiumClicked = ShellPage + "premiumClicked";
    public const string ShellPageShareClicked = ShellPage + "shareClicked";
    public const string ShuffleClicked = ShellPage + "shuffleClicked";
    public const string UpdateShown = ShellPage + "updateShown";
    public const string UpdateClicked = ShellPage + "updateClicked";
    public const string FreeTrialTipShown = ShellPage + "freeTrialTipShown";

    private const string HomePage = "homepage:";
    public const string DownloadMessageShown = HomePage + "downloadMessageShown";
    public const string DownloadMessageClicked = HomePage + "downloadMessageClicked";
    public const string DownloadMessageDismissed = HomePage + "downloadMessageDismissed";

    // updates
    private const string Updates = "updates:";
    public const string UpdateAllClicked = Updates + "updateAllButtonClicked";
    public const string UpdateSoundClicked = Updates + "updateSoundClicked";

    // catalogue
    private const string Catalogue = "catalogue:";
    public const string DownloadClicked = Catalogue + "downloadClicked";
    public const string CatalogueDeleteClicked = Catalogue + "deleteClicked";
    public const string BuyClicked = Catalogue + "buyClicked";
    public const string SubscribeClicked = Catalogue + "subscribeClicked";
    public const string Purchased = Catalogue + "purchased";
    public const string PurchaseCancelled = Catalogue + "purchaseCancelled";
    public const string BuyDurableClicked = Catalogue + "buyDurableClicked";
    public const string BuyDurablePurchased = Catalogue + "buyDurablePurchased";
    public const string BuyDurableCanceled = Catalogue + "buyDurableCanceled";
    public const string PreviewPlayed = Catalogue + "previewPlayed";

    // premium
    private const string Premium = "premium:";
    public const string LifetimeClicked = Premium + "lifetimeClicked";
    public const string LifetimePurchased = Premium + "lifetimePurchased";
    public const string LifetimeCanceled = Premium + "lifetimeCanceled";

    // gallery
    private const string Gallery = "gallery:";
    public const string SoundClicked = Gallery + "soundClicked";
    public const string SoundReordered = Gallery + "soundReordered";
    public const string DeleteClicked = Gallery + "deleteClicked";
    public const string ReorderClicked = Gallery + "reorderClicked";
    public const string EmptyMessageButtonClicked = Gallery + "emptyMessageButtonClicked";

    // timer
    private const string Timer = "timer:";
    public const string TimeSelected = Timer + "timeSelected";

    // playback
    private const string Playback = "playback:";
    public const string PlaybackRandom = Playback + "randomClicked";
    public const string PlaybackTime = Playback + "time";
    public const string PlaybackAutoResume = Playback + "autoResumeTriggered";

    // screensaver
    private const string Screensaver = "screensaver:";
    public const string ScreensaverLoaded = Screensaver + "loaded";
    public const string ScreensaverTriggered = Screensaver + "triggered";
    public const string ScreensaverFullscreen = Screensaver + "fullscreen";

    // mix
    private const string Mix = "mix:";
    public const string MixSaved = Mix + "saved";

    // sharing
    private const string Share = "share:";
    public const string SharePlayed = Share + "soundsPlayed";

    // pin
    private const string Oobe = "oobe:";
    public const string PinnedToTaskbar = Oobe + "pinnedToTaskbar";
    public const string LaunchMessageShown = Oobe + "launchMessageShown";
    public const string RatingMessageShown = Oobe + "ratingMessageShown";
    public const string OobeRateUsClicked = Oobe + "rateUsClicked";
    public const string OobeRateUsDismissed = Oobe + "rateUsDismissed";

    // settings
    private const string Settings = "settings:";
    public const string SettingsModifySubscriptionClicked = Settings + "modifySubscriptionClicked";
    public const string FeedbackClicked = Settings + "feedbackClicked";
    public const string CheckForUpdatesClicked = Settings + "checkForUpdatesClicked";

    // videos
    private const string Videos = "videos:";
    public const string VideoDownloadClicked = Videos + "downloadClicked";
    public const string VideoDeleteClicked = Videos + "deleteClicked";
    public const string VideoPremiumClicked = Videos + "premiumClicked";
    public const string VideoSelected = Videos + "selected";
    public const string VideoMenuOpened = Videos + "menuOpened";

    // focus
    private const string Focus = "focus:";
    public const string FocusStarted = Focus + "started";
    public const string FocusCompleted = Focus + "completed";
    public const string FocusSegmentCompleted = Focus + "segmentCompleted";
    public const string FocusRecentClicked = Focus + "recentClicked";
    public const string FocusInterruptionLogged = Focus + "interruptionLogged";
    public const string FocusHistoryClicked = Focus + "historyClicked";
    public const string FocusSkipClicked = Focus + "skipClicked";

    // mini
    private const string Mini = "mini:";
    public const string MiniBack = Mini + "back";
    public const string MiniNavigate = Mini + "navigation";

    // tasks
    private const string Task = "task:";
    public const string TaskAdded = Task + "added";
    public const string TaskDeleted = Task + "deleted";
    public const string TaskReordered = Task + "reordered";
    public const string TaskEdited = Task + "edited";
    public const string TaskCompleted = Task + "completed";
    public const string TasksLoaded = Task + "loaded";

    // guide
    private const string Guide = "guide:";
    public const string GuideDownloaded = Guide + "downloaded";
    public const string GuidePlayed = Guide + "played";
    public const string GuideStopped = Guide + "stopped";
    public const string GuideDeleted = Guide + "deleted";
    public const string GuidePurchaseClicked = Guide + "purchaseClicked";

    // xbox
    private const string Xbox = "xbox:";
    public const string XboxUnlockVideoClicked = $"{Xbox}unlockVideoClicked";
    public const string XboxUnlockVideoShown = $"{Xbox}unlockVideoShown";
    public const string XboxSlideshowModeChanged = $"{Xbox}slideShowModeChanged";
}
