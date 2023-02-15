using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Tools;

public interface ISystemMediaControls
{
    event EventHandler<SystemMediaControlsButton>? ButtonPressed;

    bool IsPlayEnabled { get; set; }

    bool IsPauseEnabled { get; set; }

    bool IsNextEnabled { get; set; }

    bool IsPreviousEnabled { get; set; }

    SystemMediaState PlaybackStatus { get; set; }

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