using AmbientSounds.Constants;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Tracks telemetry for mix media player.
    /// </summary>
    public class PlayerTelemetryTracker
    {
        private readonly IMixMediaPlayerService _mixMediaPlayerService;
        private readonly ITelemetry _telemetry;
        private MediaPlaybackState _currentState;

        public PlayerTelemetryTracker(
            IMixMediaPlayerService mixMediaPlayerService,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(mixMediaPlayerService, nameof(mixMediaPlayerService));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _mixMediaPlayerService = mixMediaPlayerService;
            _telemetry = telemetry;

            _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackchanged;
        }

        /// <summary>
        /// The last recorded time when 
        /// media playback state
        /// changed to playing.
        /// </summary>
        public DateTimeOffset PlayStart { get; private set; }

        /// <summary>
        /// Converts diff to telemetry-friendly rounded
        /// string.
        /// </summary>
        public static string GetRoundedDiff(TimeSpan diff)
        {
            if (diff > TimeSpan.Zero)
            {
                string roundedDiff;
                if (diff < TimeSpan.FromMinutes(1))
                {
                    roundedDiff = "<1 min";
                }
                else if (diff < TimeSpan.FromMinutes(5))
                {
                    roundedDiff = "<5 min";
                }
                else if (diff < TimeSpan.FromMinutes(10))
                {
                    roundedDiff = "<10 min";
                }
                else if (diff <= TimeSpan.FromHours(1))
                {
                    roundedDiff = $"{((int)Math.Round(diff.TotalMinutes / 10.0)) * 10} min";
                }
                else if (diff < TimeSpan.FromDays(2))
                {
                    roundedDiff = $"{Math.Round(diff.TotalHours)} hrs";
                }
                else
                {
                    roundedDiff = ">48 hrs";
                }

                return roundedDiff;
            }

            return string.Empty;
        }

        /// <summary>
        /// Tracks duration between given pause time
        /// and the internally stored start time.
        /// </summary>
        /// <returns>The string formatted duration that was calculated.</returns>
        public string TrackDuration(DateTimeOffset pauseTime)
        {
            if (pauseTime < PlayStart || PlayStart == default)
            {
                return string.Empty;
            }

            string roundedDiff = GetRoundedDiff(pauseTime - PlayStart);
            string ids = string.Join(",", _mixMediaPlayerService.GetActiveIds().OrderBy(static x => x));

            if (!string.IsNullOrWhiteSpace(roundedDiff))
            {
                _telemetry.TrackEvent(TelemetryConstants.PlaybackTime, new Dictionary<string, string>
                {
                    { "time", roundedDiff },
                    { "ids",  ids }
                });
            }

            return roundedDiff;
        }

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
                    TrackDuration(DateTimeOffset.Now);
                    PlayStart = default; // reset
                }

                _currentState = newState;
            }
        }

        private void OnPlaybackchanged(object sender, MediaPlaybackState newState) => HandleNewState(newState);

        public void Dispose()
        {
            _mixMediaPlayerService.PlaybackStateChanged -= OnPlaybackchanged;
        }
    }
}
