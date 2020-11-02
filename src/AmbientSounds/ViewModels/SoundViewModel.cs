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

        public SoundViewModel(Sound s, IMediaPlayerService playerService)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(playerService, nameof(playerService));

            _sound = s;
            _playerService = playerService;
            _playerService.PlaybackStateChanged += PlayerService_PlaybackStateChanged;
        }


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
        /// Determines the name that represents the purpose of this item most accurately.
        /// </summary>
        /// Idea: First check if we have a file to play, if not, indicate that our sound is not playable.
        /// After that, check if service is playing and the current item is our sound.
        /// If so, we can be paused, otherwise the sound can be played.
        public string AutomationName => string.IsNullOrEmpty(_sound.FilePath) ? Name + ", not playable" :
            (_playerService.PlaybackState == MediaPlaybackState.Playing
                && _playerService.Current == _sound ? "Pause " + Name : "Play " + Name);

        /// <summary>
        /// Loads this sound into the player and plays it.
        /// </summary>
        public void Play()
        {
            _playerService.Play(_sound);
        }

        private void PlayerService_PlaybackStateChanged(object sender, MediaPlaybackState e)
        {
            OnPropertyChanged(nameof(AutomationName));
        }
    }
}
