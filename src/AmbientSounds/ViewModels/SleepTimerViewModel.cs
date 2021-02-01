using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;

namespace AmbientSounds.ViewModels
{
    public class SleepTimerViewModel : ObservableObject
    {
        private const int DefaultTimerInterval = 1000;
        private readonly IMixMediaPlayerService _player;
        private readonly ITelemetry _telemetry;
        private readonly ITimerService _timer;

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
            _timer.IntervalElapsed += TimerElapsed;

            TimerStartCommand = new RelayCommand<int>(StartTimer);
            TimerPlayCommand = new RelayCommand(PlayTimer);
            TimerPauseCommand = new RelayCommand(PauseTimer);
            TimerStopCommand = new RelayCommand(StopTimer);

            _player.PlaybackStateChanged += OnPlaybackStateChanged;
        }
        
        private void OnPlaybackStateChanged(object sender, MediaPlaybackState e)
        {
            if (e == MediaPlaybackState.Paused)
                PauseTimer();
            else if (e == MediaPlaybackState.Opening || e == MediaPlaybackState.Playing)
                PlayTimer();
        }

        /// <summary>
        /// Starts the timer with the specified remainder time.
        /// </summary>
        public IRelayCommand<int> TimerStartCommand { get; }

        /// <summary>
        /// Plays the timer if it were paused.
        /// </summary>
        public IRelayCommand TimerPlayCommand { get; }

        /// <summary>
        /// Pauses the timer.
        /// </summary>
        public IRelayCommand TimerPauseCommand { get; }

        /// <summary>
        /// Stops the timer and clears the countdown.
        /// </summary>
        public IRelayCommand TimerStopCommand { get; }

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

        private void StartTimer(int minutes)
        {
            _telemetry.TrackEvent(TelemetryConstants.TimeSelected, new Dictionary<string, string>
            {
                { "length", minutes.ToString() }
            });

            _timer.Remaining = new TimeSpan(0, minutes, 0);
            OnPropertyChanged(nameof(TimeLeft));
            CountdownVisible = true;
            _timer.Start();
        }

        private void PlayTimer()
        {
            if (_timer.Remaining > new TimeSpan(0))
            {
                _timer.Start();
            }
        }

        private void PauseTimer()
        {
            _timer.Stop();
        }

        private void StopTimer()
        {
            _timer.Stop();
            _timer.Remaining = new TimeSpan(0);
            CountdownVisible = false;
        }

        private void TimerElapsed(object sender, int intervalInMs)
        {
            OnPropertyChanged(nameof(TimeLeft));
            if (_timer.Remaining < new TimeSpan(0, 0, 1))
            {
                StopTimer();
                _player.Pause();
            }
        }
    }
}
