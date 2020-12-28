namespace AmbientSounds.Constants
{
    /// <summary>
    /// List of constants used for
    /// telemetry events.
    /// </summary>
    public class TelemetryConstants
    {
        // catalogue
        private const string Catalogue = "catalogue";
        public const string DownloadClicked = Catalogue + ":downloadClicked";
        public const string MoreSoundsClicked = Catalogue + ":moreSoundsClicked";
        public const string CatalogueDeleteClicked = Catalogue + ":deleteClicked";

        // gallery
        private const string Gallery = "gallery";
        public const string SoundClicked = Gallery + ":soundClicked";
        public const string DeleteClicked = Gallery + ":deleteClicked";

        // timer
        private const string Timer = "timer";
        public const string TimeSelected = Timer + ":timeSelected";
        public const string TimerStateChanged = Timer + ":timerStateChanged";

        // mode
        private const string ViewModes = "viewModes";
        public const string Compact = ViewModes + ":compact";
        public const string ScreensaverOpened = ViewModes + ":screensaverOpened";

        // playback
        private const string Playback = "playback";
        public const string PlaybackStateChanged = Playback + ":stateChanged";
        public const string PlaybackRandom = Playback + ":randomClicked";
    }
}
