using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// An interface for a service that controls a media player.
    /// </summary>
    public interface IMediaPlayerService
    {
        /// <summary>
        /// Raised whenever a new track is being played,
        /// where the string is the item of the item being played.
        /// </summary>
        event EventHandler<string?> NewSoundPlayed;

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
        /// Gets or sets the volume of the player. Max 100 and min 0.
        /// </summary>
        double Volume { get; set; }

        /// <summary>
        /// Starts playing a given sound.
        /// </summary>
        /// <param name="sound">The <see cref="Sound"/> instance to play.</param>
        /// <param name="index">Index of sound in playlist.</param>
        void Play(Sound sound, int index);

        /// <summary>
        /// Plays the current track.
        /// </summary>
        void Play();

        /// <summary>
        /// Plays a random sound in the current playlist.
        /// </summary>
        void PlayRandom();

        /// <summary>
        /// Pauses the current track.
        /// </summary>
        void Pause();

        /// <summary>
        /// Initializes playlist to enable gapless playback.
        /// </summary>
        /// <param name="sounds">List of sounds for gapless playback.</param>
        Task Initialize(IList<Sound> sounds);

        /// <summary>
        /// Adds the given sound to the playlist.
        /// </summary>
        /// <param name="s">The sound to add.</param>
        Task AddToPlaylistAsync(Sound s);

        /// <summary>
        /// Removes the sound from in the given index
        /// from the playlist.
        /// </summary>
        /// <param name="index">The index of the item to remove.</param>
        void DeleteFromPlaylist(int index);
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
