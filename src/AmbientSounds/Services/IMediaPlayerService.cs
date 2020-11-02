using AmbientSounds.Models;
using System;

namespace AmbientSounds.Services
{
    /// <summary>
    /// An interface for a service that controls a media player.
    /// </summary>
    public interface IMediaPlayerService
    {
        /// <summary>
        /// Raised whenever a new track is being played.
        /// </summary>
        event EventHandler NewSoundPlayed;

        /// <summary>
        /// Raised whenever the current media playback state is changed.
        /// </summary>
        event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        /// <summary>
        /// Current the current sound track.
        /// </summary>
        Sound? Current { get; }

        /// <summary>
        /// Gets the current playback state (e.g. paused, playing, etc.).
        /// </summary>
        MediaPlaybackState PlaybackState { get; }

        /// <summary>
        /// Starts playing a given sound.
        /// </summary>
        /// <param name="sound">The <see cref="Sound"/> instance to play.</param>
        void Play(Sound sound);

        /// <summary>
        /// Plays the current track.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses the current track.
        /// </summary>
        void Pause();
    }

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
