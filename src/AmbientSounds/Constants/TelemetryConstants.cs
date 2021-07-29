namespace AmbientSounds.Constants
{
    /// <summary>
    /// List of constants used for
    /// telemetry events.
    /// </summary>
    public class TelemetryConstants
    {
        // catalogue
        private const string Catalogue = "catalogue:";
        public const string DownloadClicked = Catalogue + "downloadClicked";
        public const string MoreSoundsClicked = Catalogue + "moreSoundsClicked";
        public const string CatalogueDeleteClicked = Catalogue + "deleteClicked";
        public const string PreviewClicked = Catalogue + "previewClicked";
        public const string BuyClicked = Catalogue + "buyClicked";
        public const string SubscribeClicked = Catalogue + "subscribeClicked";

        // gallery
        private const string Gallery = "gallery:";
        public const string SoundClicked = Gallery + "soundClicked";
        public const string DeleteClicked = Gallery + "deleteClicked";

        // timer
        private const string Timer = "timer:";
        public const string TimeSelected = Timer + "timeSelected";

        // playback
        private const string Playback = "playback:";
        public const string PlaybackRandom = Playback + "randomClicked";
        public const string PlaybackTime = Playback + "time";

        // pages
        private const string Page = "page:";
        public const string PageNavTo = Page + "navigatedTo";

        // screensaver
        private const string Screensaver = "screensaver:";
        public const string ScreensaverLoaded = Screensaver + "loaded";
        public const string ScreensaverTriggered = Screensaver + "triggered";

        // mix
        private const string Mix = "mix:";
        public const string MixSaved = Mix + "saved";
        public const string MixCleared = Mix + "cleared";
        public const string MixRemoved = Mix + "removed";

        // sharing
        private const string Share = "share:";
        public const string ShareClicked = Share + "clicked";
        public const string ShareReceived = Share + "received";

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

        // theme
        private const string Theme = "theme:";
        public const string BackgroundChanged = Theme + "backgroundChanged";
    }
}
