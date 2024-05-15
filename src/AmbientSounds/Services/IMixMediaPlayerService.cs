using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

namespace AmbientSounds.Services;

/// <summary>
/// Interface for playing multiple
/// sounds simultaneously.
/// </summary>
public interface IMixMediaPlayerService
{
    /// <summary>
    /// Sound is added.
    /// </summary>
    event EventHandler<SoundPlayedArgs>? SoundAdded;

    /// <summary>
    /// Sound was removed.
    /// </summary>
    event EventHandler<SoundPausedArgs>? SoundRemoved;

    /// <summary>
    /// Raised when the sounds were changed.
    /// </summary>
    event EventHandler<SoundChangedEventArgs>? SoundsChanged;

    /// <summary>
    /// Mix was played.
    /// </summary>
    event EventHandler<MixPlayedArgs>? MixPlayed;

    /// <summary>
    /// Raised when playback changes between
    /// playing and paused.
    /// </summary>
    event EventHandler<MediaPlaybackState>? PlaybackStateChanged;

    /// <summary>
    /// Raised when the guide's playback position changed.
    /// </summary>
    event EventHandler<TimeSpan>? GuidePositionChanged;

    /// <summary>
    /// The total duration of the current guide.
    /// </summary>
    TimeSpan GuideDuration { get; }

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
    /// The ID of the current guide being played.
    /// If a guide is not being played, this will be empty.
    /// </summary>
    string CurrentGuideId { get; }

    /// <summary>
    /// Cancels any current playback
    /// and plays a random sound instead.
    /// </summary>
    Task PlayRandomAsync();

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
    /// Plays the the list of sounds.
    /// </summary>
    Task ToggleSoundsAsync(IReadOnlyList<Sound> sounds, string parentMixId = "");

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
    void RemoveSound(string soundId, bool raiseSoundRemoved = true);

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
    /// Plays the given guide.
    /// </summary>
    /// <param name="guide">The guide to play.</param>
    Task PlayGuideAsync(Guide guide);
    Task AddRandomAsync();

    /// <summary>
    /// Retrieves enumerable of active sound IDs.
    /// </summary>
    /// <param name="oldestToNewest">Sorts the list from oldest to newest if true. Otherwise, sorts newest to oldest.</param>
    /// <returns>Sorted list of active sound IDs.</returns>
    IEnumerable<string> GetSoundIds(bool oldestToNewest);
}
