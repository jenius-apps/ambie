using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model for a sound object.
    /// </summary>
    public class SoundViewModel : ObservableObject
    {
        private readonly Sound _sound;
        private readonly IMediaPlayerService _playerService;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly ITelemetry _telemetry;

        public SoundViewModel(
            Sound s,
            IMediaPlayerService playerService,
            int index,
            ISoundDataProvider soundDataProvider,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(playerService, nameof(playerService));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            Index = index;
            _sound = s;
            _playerService = playerService;
            _playerService.PlaybackStateChanged += PlayerService_PlaybackStateChanged;
            _soundDataProvider = soundDataProvider;
            _telemetry = telemetry;

            DeleteCommand = new RelayCommand(DeleteSound);
        }
        
        /// <summary>
        /// Index of this sound in the list.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The sound's GUID.
        /// </summary>
        public string? Id => _sound.Id;

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string? Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string? Name => _sound.Name;  

        /// <summary>
        /// The path for the image to display for the current sound.
        /// </summary>
        public string? ImagePath => _sound.ImagePath;

        /// <summary>
        /// If true, item can be deleted from local storage.
        /// </summary>
        public bool CanDelete => !_sound.FilePath?.StartsWith("ms-appx") ?? false;

        /// <summary>
        /// Returns true if the sound cannot be played.
        /// </summary>
        public bool Unplayable => string.IsNullOrWhiteSpace(_sound.FilePath);

        /// <summary>
        /// Command for deleting this sound.
        /// </summary>
        public IRelayCommand DeleteCommand { get; }

        /// <summary>
        /// Returns true if the sound is currently playing.
        /// </summary>
        public bool IsCurrentlyPlaying => _playerService.PlaybackState == MediaPlaybackState.Playing && _playerService.Current == _sound;

        /// <summary>
        /// Loads this sound into the player and plays it.
        /// </summary>
        public void Play()
        {
            _playerService.Play(_sound, Index);
        }

        private async void DeleteSound()
        {
            _telemetry.TrackEvent(TelemetryConstants.DeleteClicked, new Dictionary<string, string>
            {
                { "name", _sound.Name ?? "" },
                { "id", _sound.Id ?? "" }
            });
            await _soundDataProvider.DeleteLocalSoundAsync(_sound.Id ?? "");
        }

        private void PlayerService_PlaybackStateChanged(object sender, MediaPlaybackState e)
        {
            OnPropertyChanged(nameof(IsCurrentlyPlaying));
        }
    }
}
