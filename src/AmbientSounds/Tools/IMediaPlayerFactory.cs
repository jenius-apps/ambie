using JeniusApps.Common.Tools;

namespace AmbientSounds.Tools;

/// <summary>
/// Factory interface that creates new media player objects.
/// </summary>
public interface IMediaPlayerFactory
{
    /// <summary>
    /// Creates new player.
    /// </summary>
    /// <param name="disableDefaultSystemControls">
    /// If true, this will ensure that 
    /// the system controls wouldn't be automatically enabled.
    /// </param>
    /// <returns>Returns wrapper for media player.</returns>
    public IMediaPlayer CreatePlayer(bool disableDefaultSystemControls = false);
}
