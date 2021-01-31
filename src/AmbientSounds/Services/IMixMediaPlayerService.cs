using AmbientSounds.Models;
using System;
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
        event EventHandler<Sound> SoundAdded;

        /// <summary>
        /// Sound was removed. String is
        /// the sound's ID.
        /// </summary>
        event EventHandler<string> SoundRemoved;

        /// <summary>
        /// If the given sound is playing,
        /// the sound will be paused and removed.
        /// If the sound was paused, the sound
        /// will be played.
        /// </summary>
        /// <param name="s">The sound to toggle.</param>
        Task ToggleSoundAsync(Sound s);

        /// <summary>
        /// Removes the sound
        /// from being played.
        /// </summary>
        /// <param name="s">The sound to pause and remove.</param>
        void RemoveSound(Sound s);

        /// <summary>
        /// Returns true if the sound is active.
        /// </summary>
        /// <param name="s">The sound to check.</param>
        bool IsSoundPlaying(Sound s);

        /// <summary>
        /// Retrieves the volume for the given sound.
        /// </summary>
        double GetVolume(Sound s);

        /// <summary>
        /// Sets the volume for the given sound.
        /// </summary>
        void SetVolume(Sound s, double value);
    }
}
