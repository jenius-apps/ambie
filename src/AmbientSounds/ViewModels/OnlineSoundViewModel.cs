using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace AmbientSounds.ViewModels
{
    public class OnlineSoundViewModel : ObservableObject
    {
        private readonly Sound _sound;

        public OnlineSoundViewModel(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            _sound = s;
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
    }
}
