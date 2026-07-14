using JeniusApps.Common.Telemetry;
using System;

namespace AmbientSounds.Services;

/// <summary>
/// Tracks telemetry for mix media player.
/// </summary>
public class PlayerTelemetryTracker
{
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private MediaPlaybackState _currentState;

    public PlayerTelemetryTracker(
        IMixMediaPlayerService mixMediaPlayerService,
        ITelemetry telemetry)
    {
        _mixMediaPlayerService = mixMediaPlayerService;

        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackchanged;
    }

    /// <summary>
    /// The last recorded time when 
    /// media playback state
    /// changed to playing.
    /// </summary>
    public DateTimeOffset PlayStart { get; private set; }

    public void HandleNewState(MediaPlaybackState newState)
    {
        if (_currentState != newState)
        {
            if (newState == MediaPlaybackState.Playing)
            {
                PlayStart = DateTimeOffset.Now;
            }
            else if (newState == MediaPlaybackState.Paused)
            {
                PlayStart = default; // reset
            }

            _currentState = newState;
        }
    }

    private void OnPlaybackchanged(object sender, MediaPlaybackState newState)
    {
        HandleNewState(newState);
    }

    public void Dispose()
    {
        _mixMediaPlayerService.PlaybackStateChanged -= OnPlaybackchanged;
    }
}
