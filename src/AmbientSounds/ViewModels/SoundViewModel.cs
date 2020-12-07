using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model for a sound object.
    /// </summary>
    public class SoundViewModel : ObservableObject
    {
        private readonly Sound _sound;
        private readonly IMediaPlayerService _playerService;
        private readonly int _index;

        public SoundViewModel(Sound s, IMediaPlayerService playerService, int index)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(playerService, nameof(playerService));

            _index = index;
            _sound = s;
            _playerService = playerService;
            _playerService.PlaybackStateChanged += PlayerService_PlaybackStateChanged;
        }

        /// <summary>
        /// The sound's Id.
        /// </summary>
        public string Id => _sound.Id;

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string Name => _sound.Name ?? _sound.Id;

        /// <summary>
        /// The path for the image to display for the current sound.
        /// </summary>
        public string ImagePath => _sound.ImagePath;

        /// <summary>
        /// Returns true if the sound cannot be played.
        /// </summary>
        public bool Unplayable => string.IsNullOrWhiteSpace(_sound.FilePath);

        /// <summary>
        /// Returns true if the sound is currently playing.
        /// </summary>
        public bool IsCurrentlyPlaying => _playerService.PlaybackState == MediaPlaybackState.Playing && _playerService.Current == _sound;

        /// <summary>
        /// Loads this sound into the player and plays it.
        /// </summary>
        public void Play()
        {
            _playerService.Play(_sound, _index);
        }

        private void PlayerService_PlaybackStateChanged(object sender, MediaPlaybackState e)
        {
            OnPropertyChanged(nameof(IsCurrentlyPlaying));
        }
    }
}
