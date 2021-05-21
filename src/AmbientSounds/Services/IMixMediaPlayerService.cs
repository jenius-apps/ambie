using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for playing multiple
    /// sounds simultaneously.
    /// </summary>
    public interface IMixMediaPlayerService
    {
        /// <summary>
        /// Sound is added.
        /// </summary>
        event EventHandler<SoundPlayedArgs> SoundAdded;

        /// <summary>
        /// Sound was removed.
        /// </summary>
        event EventHandler<SoundPausedArgs> SoundRemoved;

        /// <summary>
        /// Mix was played.
        /// </summary>
        event EventHandler<MixPlayedArgs> MixPlayed;

        /// <summary>
        /// Raised when playback changes between
        /// playing and paused.
        /// </summary>
        event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        /// <summary>
        /// Global volume control. Max = 1. Min = 0.
        /// </summary>
        double GlobalVolume { get; set; }

        /// <summary>
        /// The ID of the current mix being played.
        /// If a mix is not being played, this will be empty.
        /// </summary>
        string CurrentMixId { get; set; }

        /// <summary>
        /// Dictionary of screensavers for the active tracks.
        /// </summary>
        Dictionary<string, string[]> Screensavers { get; }

        /// <summary>
        /// The current global state of the player.
        /// </summary>
        MediaPlaybackState PlaybackState { get; set; }

        /// <summary>
        /// Resumes playback.
        /// </summary>
        void Play();

        /// <summary>
        /// Pauses playback.
        /// </summary>
        void Pause();

        /// <summary>
        /// Returns the sound ids currently paused or playing.
        /// </summary>
        string[] GetSoundIds();

        /// <summary>
        /// If the given sound is playing,
        /// the sound will be paused and removed.
        /// If the sound was paused, the sound
        /// will be played.
        /// </summary>
        /// <param name="s">The sound to toggle.</param>
        /// <param name="keepPaused">Optional. If true, an inserted sound will not be played automatically.</param>
        Task ToggleSoundAsync(Sound s, bool keepPaused = false, string parentMixId = "");

        /// <summary>
        /// Updates the <see cref="CurrentMixId"/>
        /// and raises an event indicating the mix is
        /// now playing.
        /// </summary>
        /// <param name="mixId">Id of sound mix.</param>
        void SetMixId(string mixId);

        /// <summary>
        /// Removes all active tracks.
        /// </summary>
        void RemoveAll();

        /// <summary>
        /// Removes the sound
        /// from being played.
        /// </summary>
        /// <param name="soundId">The sound to pause and remove.</param>
        void RemoveSound(string soundId);

        /// <summary>
        /// Returns true if the sound is active.
        /// </summary>
        /// <param name="soundId">The sound to check.</param>
        bool IsSoundPlaying(string soundId);

        /// <summary>
        /// Retrieves the volume for the given sound.
        /// </summary>
        double GetVolume(string soundId);

        /// <summary>
        /// Sets the volume for the given sound.
        /// </summary>
        void SetVolume(string soundId, double value);

        /// <summary>
        /// Returns list of active sound Ids.
        /// </summary>
        IList<string> GetActiveIds();
    }
}
