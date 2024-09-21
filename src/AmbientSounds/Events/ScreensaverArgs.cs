using AmbientSounds.Models;

namespace AmbientSounds.Events;

/// <summary>
/// Class that holds navigation arguments for the screensaver page.
/// </summary>
public class ScreensaverArgs
{
    /// <summary>
    /// Requested channel type.
    /// </summary>
    public required ChannelType RequestedType { get; init; }

    /// <summary>
    /// Requested video ID, if channel type is <see cref="ChannelType.Videos"/>.
    /// </summary>
    public string? VideoId { get; init; }
}
