namespace AmbientSounds.Services.Xamarin
{
    /// <summary>
    /// Mobile doesn't support screensavers,
    /// so this class provides implementation
    /// with no operation.
    /// </summary>
    public class ScreensaverService : IScreensaverService
    {
        /// <inheritdoc/>
        public bool IsScreensaverEnabled { get; }

        /// <inheritdoc/>
        public void ResetScreensaverTimeout() { }

        /// <inheritdoc/>
        public void StartTimer() { }

        /// <inheritdoc/>
        public void StopTimer() { }
    }
}
