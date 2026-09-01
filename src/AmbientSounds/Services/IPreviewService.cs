namespace AmbientSounds.Services
{
    /// <summary>
    /// Service for managing
    /// and playing sound previews.
    /// </summary>
    public interface IPreviewService
    {
        /// <summary>
        /// Plays the given preview sound file.
        /// </summary>
        /// <param name="onlineUrl">The web url for the preview sound file.</param>
        void Play(string onlineUrl);

        /// <summary>
        /// Stops the current playback.
        /// </summary>
        void Stop();
    }
}
