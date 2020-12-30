namespace AmbientSounds.Services
{
    /// <summary>
    /// Service for triggering the screensaver.
    /// </summary>
    public interface IScreensaverService
    {
        /// <summary>
        /// Returns true if screensaver is enabled.
        /// </summary>
        bool IsScreensaverEnabled { get; }

        /// <summary>
        /// Resets the screensaver's trigger timeout.
        /// </summary>
        void ResetScreensaverTimeout();

        /// <summary>
        /// Starts the screensaver's trigger timer.
        /// When the timer counts down, the screensaver
        /// will launch.
        /// </summary>
        void StartTimer();

        /// <summary>
        /// Stops the trigger timer so the screensaver
        /// will no longer launch.
        /// </summary>
        void StopTimer();
    }
}