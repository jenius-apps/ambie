using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;

namespace AmbientSounds.ViewModels
{
    public class MainPageViewModel : ObservableObject
    {
        private readonly IScreensaverService _screensaverService;
        private readonly IMixMediaPlayerService _mediaPlayerService;
        private readonly INavigator _navigator;
        private readonly IDialogService _dialogService;

        public MainPageViewModel(
            IScreensaverService screensaverService,
            IMixMediaPlayerService mediaPlayerService,
            INavigator navigator,
            IDialogService dialogService)
        {
            Guard.IsNotNull(screensaverService, nameof(screensaverService));
            Guard.IsNotNull(mediaPlayerService, nameof(mediaPlayerService));
            Guard.IsNotNull(navigator, nameof(navigator));
            Guard.IsNotNull(dialogService, nameof(dialogService));

            _screensaverService = screensaverService;
            _mediaPlayerService = mediaPlayerService;
            _navigator = navigator;
            _dialogService = dialogService;
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

        public void Initialize()
        {
            _mediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;
        }

        public void Dispose()
        {
            _mediaPlayerService.PlaybackStateChanged -= OnPlaybackChanged;
        }

        public async void OpenThemeSettings() => await _dialogService.OpenThemeSettingsAsync();
    }
}
