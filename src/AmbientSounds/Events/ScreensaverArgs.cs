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

    /// <summary>
    /// A URL to an image to be used as placeholder for the video as it loads.
    /// </summary>
    public string? VideoImagePreviewUrl { get; init; }
}
