using System;

namespace AmbientSounds.Tools;

/// <summary>
/// Interface for the system's media controls.
/// </summary>
public interface ISystemMediaControls
{
    /// <summary>
    /// Raised when a system media button is pressed.
    /// </summary>
    event EventHandler<SystemMediaControlsButton>? ButtonPressed;

    /// <summary>
    /// Determines if the play button is enabled.
    /// </summary>
    bool IsPlayEnabled { get; set; }

    /// <summary>
    /// Determines if the pause button is enabled.
    /// </summary>
    bool IsPauseEnabled { get; set; }

    /// <summary>
    /// Determines if the next button is enabled.
    /// </summary>
    bool IsNextEnabled { get; set; }

    /// <summary>
    /// Determines if the previous button is enabled.
    /// </summary>
    bool IsPreviousEnabled { get; set; }

    /// <summary>
    /// The current state of the media controls.
    /// </summary>
    SystemMediaState PlaybackStatus { get; set; }

    /// <summary>
    /// Updates the display information on the media controls.
    /// </summary>
    /// <param name="title">The title to display.</param>
    /// <param name="artist">The name of the artist to display.</param>
    void UpdateDisplay(string title, string artist);
}

public enum SystemMediaControlsButton
{
    Play,
    Pause,
    Stop,
    Record,
    FastForward,
    Rewind,
    Next,
    Previous,
    ChannelUp,
    ChannelDown
}

public enum SystemMediaState
{
    Closed,
    Changing,
    Stopped,
    Playing,
    Paused
}