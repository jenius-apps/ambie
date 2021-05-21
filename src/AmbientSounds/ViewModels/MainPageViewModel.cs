using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace AmbientSounds.ViewModels
{
    public class MainPageViewModel : ObservableObject, IDisposable
    {
        private readonly IScreensaverService _screensaverService;
        private readonly IMixMediaPlayerService _mediaPlayerService;
        private readonly INavigator _navigator;
        private bool _maxTeachingTipOpen;

        public MainPageViewModel(
            IScreensaverService screensaverService,
            IMixMediaPlayerService mediaPlayerService,
            INavigator navigator)
        {
            Guard.IsNotNull(screensaverService, nameof(screensaverService));
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(navigator, nameof(navigator));

            _screensaverService = screensaverService;
            _mediaPlayerService = mediaPlayerService;
            _navigator = navigator;

            _mediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;
            _mediaPlayerService.MaxReached += OnMaxReached;
        }


        private void OnMaxReached(object sender, EventArgs e)
        {
            MaxTeachingTipOpen = true;
        }

        /// <summary>
        /// Controls when the teaching tip is visible or not.
        /// </summary>
        public bool MaxTeachingTipOpen
        {
            get => _maxTeachingTipOpen;
            set
            {
                _maxTeachingTipOpen = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Resets the screensaver timer's timout.
        /// </summary>
        public void ResetTime() => _screensaverService.ResetScreensaverTimeout();

        /// <summary>
        /// Starts the screesaver timer if
        /// a sound is playing.
        /// </summary>
        public void StartTimer()
        {
            _screensaverService.StartTimer();
        }

        public void ToCatalogue() => _navigator.ToCatalogue();

        /// <summary>
        /// Stops the screensaver timer.
        /// </summary>
        public void StopTimer() => _screensaverService.StopTimer();

        private void OnPlaybackChanged(object sender, MediaPlaybackState e)
        {
            if (e == MediaPlaybackState.Playing)
            {
                _screensaverService.StartTimer();
            }
            else
            {
                _screensaverService.StopTimer();
            }
        }

        public void Dispose()
        {
            _mediaPlayerService.PlaybackStateChanged -= OnPlaybackChanged;
            _mediaPlayerService.MaxReached -= OnMaxReached;
        }
    }
}
