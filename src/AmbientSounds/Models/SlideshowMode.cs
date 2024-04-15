namespace AmbientSounds.Models;

/// <summary>
/// Display modes used for the slideshow experience on Xbox.
/// </summary>
public enum SlideshowMode
{
    /// <summary>
    /// Represents displaying images in a slideshow.
    /// </summary>
    Images,

    /// <summary>
    /// Represents the user's desire to display nothing on screen,
    /// preferring a dark screen instead.
    /// </summary>
    DarkScreen,

    /// <summary>
    /// Represents the user's desire to display videos, if available.
    /// </summary>
    Video
}
