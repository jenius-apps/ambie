using AmbientSounds.Constants;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Tracks telemetry for mix media player.
    /// </summary>
    public class PlayerTelemetryTracker
    {
        private readonly IMixMediaPlayerService _mixMediaPlayerService;
        private readonly ITelemetry _telemetry;
        private DateTimeOffset _playStart;
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
                else if (diff <= TimeSpan.FromHours(8))
                {
                    roundedDiff = $"{Math.Round(diff.TotalHours)} hrs";
                }
                else if (diff < TimeSpan.FromDays(1))
                {
                    roundedDiff = "<24 hrs";
                }
                else
                {
                    roundedDiff = ">24 hrs";
                }

                return roundedDiff;
            }

            return string.Empty;
        }

        /// <summary>
        /// Tracks duration between given pause time
        /// and the internally stored start time.
        /// </summary>
        public void TrackDuration(DateTimeOffset pauseTime)
        {
            if (pauseTime < _playStart)
            {
                return;
            }

            string roundedDiff = GetRoundedDiff(pauseTime - _playStart);

            if (!string.IsNullOrWhiteSpace(roundedDiff))
            {
                _telemetry.TrackEvent(TelemetryConstants.PlaybackTime, new Dictionary<string, string>
                {
                    { "time", roundedDiff }
                });
            }
        }

        private void OnPlaybackchanged(object sender, MediaPlaybackState newState)
        {
            if (_currentState != newState)
            {
                if (newState == MediaPlaybackState.Playing)
                {
                    _playStart = DateTimeOffset.Now;
                }
                else if (newState == MediaPlaybackState.Paused)
                {
                    TrackDuration(DateTimeOffset.Now);
                }

                _currentState = newState;
            }
        }
    }
}
