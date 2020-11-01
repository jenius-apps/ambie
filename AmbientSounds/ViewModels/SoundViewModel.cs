using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model for a sound object.
    /// </summary>
    public class SoundViewModel : ObservableObject
    {
        private readonly Sound _sound;
        private readonly MediaPlayerService _playerService;

        public SoundViewModel(Sound s, MediaPlayerService playerService)
        {
            _sound = s ?? throw new ArgumentNullException(nameof(s));
            _playerService = playerService ?? throw new ArgumentNullException(nameof(playerService));
        }

        /// <summary>
        /// A bitmap image source for this sound.
        /// </summary>
        public ImageSource SoundImageSource => new BitmapImage(new Uri(_sound.ImagePath));

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string Name => _sound.Name ?? _sound.Id;

        /// <summary>
        /// Loads this sound into the player and plays it.
        /// </summary>
        public void Play()
        {
            _playerService.Play(_sound);
        }
    }
}
