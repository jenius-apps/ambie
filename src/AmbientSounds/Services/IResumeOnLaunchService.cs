using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Interface for resuming sounds on app launch.
/// </summary>
public interface IResumeOnLaunchService
{
    /// <summary>
    /// Loads sounds from previous session into the media player.
    /// </summary>
    Task LoadSoundsFromPreviousSessionAsync();

    /// <summary>
    /// Tries to resume the playback of the sound on launch
    /// if allowed.
    /// </summary>
    /// <param name="force">Forces playback to resume.</param>
    void TryResumePlayback(bool force = false);
}