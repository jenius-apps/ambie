using AmbientSounds.Constants;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace AmbientSounds.ViewModels
{
    public partial class SleepTimerViewModel : ObservableObject
    {
        private const int DefaultTimerInterval = 1000; // ms
        private readonly IMixMediaPlayerService _player;
        private readonly ITelemetry _telemetry;
        private readonly ITimerService _timer;
        private bool _canPlay;
        private bool _canStop;

        public SleepTimerViewModel(
            IMixMediaPlayerService player,
            ITimerService timer,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(timer, nameof(timer));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _player = player;
            _timer = timer;
            _telemetry = telemetry;
            _timer.Interval = DefaultTimerInterval;
        }
        
        private void OnPlaybackStateChanged(object sender, MediaPlaybackState e)
        {
            if (e == MediaPlaybackState.Paused)
                PauseTimer();
            else if (e == MediaPlaybackState.Opening || e == MediaPlaybackState.Playing)
                PlayTimer();
        }

        public bool StopVisible
        {
            get => _canStop;
            set => SetProperty(ref _canStop, value);
        }

        public bool PlayVisible
        {
            get => _canPlay;
            set => SetProperty(ref _canPlay, value);
        }

        /// <summary>
        /// Determines if the sleep timer's countdown
        /// is visible.
        /// </summary>
        public bool CountdownVisible
        {
            get => _countdownVisible;
            set => SetProperty(ref _countdownVisible, value);
        }
        private bool _countdownVisible;

        /// <summary>
        /// String representation of time remaining.
        /// E.g. 0:59:12 for 59 minutes and 12 seconds left.
        /// </summary>
        public string TimeLeft => _timer.Remaining.ToString("g");

        [RelayCommand]
        private void StartTimer(int minutes)
        {
            _telemetry.TrackEvent(TelemetryConstants.TimeSelected, new Dictionary<string, string>
            {
                { "length", minutes.ToString() }
            });

            _timer.Remaining = TimeSpan.FromMinutes(minutes);
            OnPropertyChanged(nameof(TimeLeft));
            CountdownVisible = true;
            _timer.Start();
            StopVisible = true;
            PlayVisible = false;
        }

        [RelayCommand]
        private void PlayTimer()
        {
            if (_timer.Remaining > TimeSpan.Zero)
            {
                _timer.Start();
                StopVisible = true;
                PlayVisible = false;
            }
        }

        [RelayCommand]
        private void PauseTimer()
        {
            _timer.Stop();
            StopVisible = false;

            if (_timer.Remaining > TimeSpan.Zero)
            {
                PlayVisible = true;
            }
        }

        [RelayCommand]
        private void StopTimer()
        {
            _timer.Stop();
            _timer.Remaining = TimeSpan.Zero;
            OnPropertyChanged(nameof(TimeLeft));
            CountdownVisible = false;
            StopVisible = false;
            PlayVisible = false;
        }

        private void TimerElapsed(object sender, TimeSpan remaining)
        {
            OnPropertyChanged(nameof(TimeLeft));
            if (remaining < TimeSpan.FromSeconds(1))
            {
                StopTimer();
                _player.Pause();
            }
        }

        public void Initialize()
        {
            _timer.IntervalElapsed += TimerElapsed;
            _player.PlaybackStateChanged += OnPlaybackStateChanged;
        }

        public void Dispose()
        {
            _timer.IntervalElapsed -= TimerElapsed;
            _player.PlaybackStateChanged -= OnPlaybackStateChanged;
        }
    }
}
