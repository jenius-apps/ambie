namespace AmbientSounds.Services
{
    /// <summary>
    /// An enum representing possible media playback states.
    /// </summary>
    public enum MediaPlaybackState
    {
        /// <summary>
        /// A file is currently being opened or buffered.
        /// </summary>
        Opening,

        /// <summary>
        /// A track is currently being played.
        /// </summary>
        Playing,

        /// <summary>
        /// A track is opened but currently paused.
        /// </summary>
        Paused,

        /// <summary>
        /// The media playback is currently stopped.
        /// </summary>
        Stopped
    }
}
