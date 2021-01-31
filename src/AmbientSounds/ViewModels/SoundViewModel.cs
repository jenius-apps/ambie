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
        private readonly IMixMediaPlayerService _playerService;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly ISoundMixService _soundMixService;
        private readonly ITelemetry _telemetry;

        public SoundViewModel(
            Sound s,
            IMixMediaPlayerService playerService,
            int index,
            ISoundDataProvider soundDataProvider,
            ISoundMixService soundMixService,
            ITelemetry telemetry)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(playerService, nameof(playerService));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));

            Index = index;
            _sound = s;
            _soundMixService = soundMixService;
            _playerService = playerService;
            _soundDataProvider = soundDataProvider;
            _telemetry = telemetry;

            _playerService.SoundRemoved += OnSoundPaused;
            _playerService.SoundAdded += OnSoundPlayed;

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
        public string? ImagePath => _sound.IsMix ? _sound.ImagePaths[0] : _sound.ImagePath;

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
        public bool IsCurrentlyPlaying => _playerService.IsSoundPlaying(_sound.Id);

        /// <summary>
        /// Loads this sound into the player and plays it.
        /// </summary>
        public async void Play()
        {
            if (!_sound.IsMix)
            {
                await _playerService.ToggleSoundAsync(_sound);
            }
            else
            {
                await _soundMixService.LoadMixAsync(_sound);
            }
        }

        private void OnSoundPaused(object sender, string soundId)
        {
            if (Id == soundId)
            {
                OnPropertyChanged(nameof(IsCurrentlyPlaying));
            }
        }

        private void OnSoundPlayed(object sender, Sound s)
        {
            if (s?.Id == _sound.Id)
            {
                OnPropertyChanged(nameof(IsCurrentlyPlaying));
            }
        }

        private async void DeleteSound()
        {
            _playerService.RemoveSound(_sound.Id);
            _telemetry.TrackEvent(TelemetryConstants.DeleteClicked, new Dictionary<string, string>
            {
                { "name", _sound.Name ?? "" },
                { "id", _sound.Id ?? "" }
            });
            await _soundDataProvider.DeleteLocalSoundAsync(_sound.Id ?? "");
        }
    }
}
