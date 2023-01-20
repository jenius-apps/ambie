namespace AmbientSounds.Constants
{
    /// <summary>
    /// List of constants used for
    /// telemetry events.
    /// </summary>
    public class TelemetryConstants
    {
        // shell page
        private const string ShellPage = "shellpage:";
        public const string ShellPagePremiumClicked = ShellPage + "premiumClicked";
        public const string ShellPageShareClicked = ShellPage + "shareClicked";

        // catalogue
        private const string Catalogue = "catalogue:";
        public const string DownloadClicked = Catalogue + "downloadClicked";
        public const string FreeDownloaded = Catalogue + "freeDownloaded";
        public const string MoreSoundsClicked = Catalogue + "moreSoundsClicked";
        public const string CatalogueDeleteClicked = Catalogue + "deleteClicked";
        public const string PreviewClicked = Catalogue + "previewClicked";
        public const string BuyClicked = Catalogue + "buyClicked";
        public const string SubscribeClicked = Catalogue + "subscribeClicked";
        public const string Purchased = Catalogue + "purchased";
        public const string PurchaseCancelled = Catalogue + "purchaseCancelled";

        // gallery
        private const string Gallery = "gallery:";
        public const string SoundClicked = Gallery + "soundClicked";
        public const string SoundReordered = Gallery + "soundReordered";
        public const string DeleteClicked = Gallery + "deleteClicked";
        public const string ReorderClicked = Gallery + "reorderClicked";
        public const string FreeClicked = Gallery + "freeSoundClicked";
        public const string ExpiredClicked = Gallery + "expiredFreeSoundClicked";
        public const string EmptyMessageButtonClicked = Gallery + "emptyMessageButtonClicked";

        // timer
        private const string Timer = "timer:";
        public const string TimeSelected = Timer + "timeSelected";

        // playback
        private const string Playback = "playback:";
        public const string PlaybackRandom = Playback + "randomClicked";
        public const string PlaybackTime = Playback + "time";
        public const string PlaybackAutoResume = Playback + "autoResumeTriggered";

        // pages
        private const string Page = "page:";
        public const string PageNavTo = Page + "navigatedTo";

        // screensaver
        private const string Screensaver = "screensaver:";
        public const string ScreensaverLoaded = Screensaver + "loaded";
        public const string ScreensaverTriggered = Screensaver + "triggered";
        public const string ScreensaverFullscreen = Screensaver + "fullscreen";

        // mix
        private const string Mix = "mix:";
        public const string MixSaved = Mix + "saved";
        public const string MixCleared = Mix + "cleared";
        public const string MixRemoved = Mix + "removed";

        // sharing
        private const string Share = "share:";
        public const string ShareClicked = Share + "clicked";
        public const string ShareReceived = Share + "received";
        public const string SharePlayed = Share + "soundsPlayed";
        public const string ShareSoundsMissingLoaded = Share + "missingSoundsLoaded";
        public const string ShareFailedMessageShown = Share + "failedMessageShown";
        public const string ShareFailedMessageClicked = Share + "failedMessageClicked";
        public const string ShareFailedMessageDismissed = Share + "failedMessageDismissed";

        // Sign in
        private const string Signin = "signin:";
        public const string SignInTriggered = Signin + "triggered";
        public const string SignOutClicked = Signin + "signedOut";
        public const string SilentSuccessful = Signin + "silentSuccessful";
        public const string SignInCompleted = Signin + "signinCompleted";

        // sync
        private const string Sync = "sync:";
        public const string SyncManual = Sync + "manual";
        public const string SyncDown = Sync + "down";
        public const string SyncUp = Sync + "up";

        // upload
        private const string Upload = "upload:";
        public const string UploadClicked = Upload + "clicked";
        public const string UserSoundDeleted = Upload + "soundDeleted";
        public const string UploadTermsOfUseClicked = Upload + "termsClicked";
        public const string UploadRefreshClicked = Upload + "refreshClicked";
        public const string UploadFilePicked = Upload + "filePicked";

        // pin
        private const string Oobe = "oobe:";
        public const string PinnedToTaskbar = Oobe + "pinnedToTaskbar";
        public const string LaunchMessageShown = Oobe + "launchMessageShown";
        public const string RatingMessageShown = Oobe + "ratingMessageShown";
        public const string OobeRateUsClicked = Oobe + "rateUsClicked";
        public const string OobeRateUsDismissed = Oobe + "rateUsDismissed";

        // theme
        private const string Theme = "theme:";
        public const string BackgroundChanged = Theme + "backgroundChanged";

        // settings
        private const string Settings = "settings:";
        public const string SmtcDisabled = Settings + "smtcDisabled";
        public const string SmtcEnabled = Settings + "smtcEnabled";

        // videos
        private const string Videos = "videos:";
        public const string VideoDownloadClicked = Videos + "downloadClicked";
        public const string VideoDeleteClicked = Videos + "deleteClicked";
        public const string VideoPremiumClicked = Videos + "premiumClicked";
        public const string VideoSelected = Videos + "selected";
        public const string VideoMenuOpened = Videos + "menuOpened";

        // shaders
        private const string Shaders = "shaders:";
        public const string ShaderSelected = Shaders + "selected";
        public const string ShaderDeviceLost = Shaders + "deviceLost";

        public const string MissingSoundsDownloaded = "missingSoundsDownloaded";
        public const string MissingSoundsCanceled = "missingSoundsCanceled";

        // focus
        private const string Focus = "focus:";
        public const string FocusStarted = Focus + "started";
        public const string FocusResumed = Focus + "resumed";
        public const string FocusPaused = Focus + "paused";
        public const string FocusCompleted = Focus + "completed";
        public const string FocusSegmentCompleted = Focus + "segmentCompleted";
        public const string FocusReset = Focus + "reset";
        public const string FocusRecentClicked = Focus + "recentClicked";
        public const string FocusHelpClicked = Focus + "helpClicked";
        public const string FocusTutorialStarted = Focus + "tutorialStarted";
        public const string FocusTutorialEnded = Focus + "tutorialEnded";
        public const string FocusInterruptionLogged = Focus + "interruptionLogged";
        public const string FocusHistoryClicked = Focus + "historyClicked";

        // mini
        private const string Mini = "mini:";
        public const string MiniOpenedManually = Mini + "openedManually";
        public const string MiniOpenedAutomatically = Mini + "openedAutomatically";
        public const string MiniAutoEnabled = Mini + "autoEnabled";
        public const string MiniAutoDisabled = Mini + "autoDisabled";
        public const string MiniBack = Mini + "back";
        public const string MiniNavigate = Mini + "navigation";
        //public const string MiniFocusStart = Mini + "focusStart";
        //public const string MiniFocusPause = Mini + "focusPause";
        //public const string MiniFocusStop = Mini + "focusStop";
        //public const string MiniFocusResume = Mini + "focusResume";
        //public const string MiniSoundClicked = Mini + "soundClicked";

        // tasks
        private const string Task = "task:";
        public const string TaskAdded = Task + "added";
        public const string TaskDeleted = Task + "deleted";
        public const string TaskReordered = Task + "reordered";
        public const string TaskEdited = Task + "edited";
        public const string TaskCompleted = Task + "completed";
        public const string TaskReopened = Task + "reopened";
        public const string TaskCompletedInSession = Task + "completedInSession";
        public const string TaskReopenedInSession = Task + "reopenedInSession";
        public const string TaskCompletedExpanded = Task + "completedExpanded";
        public const string TaskCompletedCollapsed = Task + "completedCollapsed";
        public const string TasksLoaded = Task + "loaded";
    }
}
