namespace AmbientSounds.Models;

/// <summary>
/// The available modes for the timer in the channel viewer page.
/// </summary>
public enum ChannelTimerMode
{
    /// <summary>
    /// Indicates no timer should be visible on channel viewer page.
    /// </summary>
    None,

    /// <summary>
    /// Indicates that the focus timer widget should be visible on the channel viewer page.
    /// </summary>
    Focus,

    /// <summary>
    /// Indicates that the countdown timer should be visible on the channel viewer page.
    /// </summary>
    Countdown,
}
