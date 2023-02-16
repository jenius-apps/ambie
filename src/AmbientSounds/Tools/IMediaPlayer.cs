using System;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

/// <summary>
/// An abstraction over the system media player.
/// </summary>
public interface IMediaPlayer
{
    /// <summary>
    /// Media player's volume.
    /// </summary>
    double Volume { get; set; }

    /// <summary>
    /// Pauses the media player.
    /// </summary>
    void Pause();

    /// <summary>
    /// Plays the media player.
    /// </summary>
    void Play();

    /// <summary>
    /// Sets the media player's source using the given file path.
    /// </summary>
    /// <param name="pathToFile">Path of the source file.</param>
    /// <param name="enableGaplessLoop">If true, the media source will be configured for gapless playback loop.</param>
    /// <returns>True if setting the source was successful. False, otherwise.</returns>
    Task<bool> SetSourceAsync(string pathToFile, bool enableGaplessLoop = false);

    /// <summary>
    /// Sets the media player's source using the given URI.
    /// </summary>
    /// <param name="uriSource">The URI to load into the media player.</param>
    /// <param name="enableGaplessLoop">If true, the media source will be configured for gapless playback loop.</param>
    /// <returns>True if setting the source was successful. False, otherwise.</returns>
    bool SetUriSource(Uri uriSource, bool enableGaplessLoop = false);

    /// <summary>
    /// Releases resources associated with the media player.
    /// </summary>
    void Dispose();
}
