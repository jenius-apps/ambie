using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.ViewModels
{
    public class UploadedSoundViewModel
    {
        private readonly Sound _sound;

        public UploadedSoundViewModel(
            Sound s)
        {
            Guard.IsNotNull(s, nameof(s));
            _sound = s;
        }

        /// <summary>
        /// The sound's attribution.
        /// </summary>
        public string? Attribution => _sound.Attribution;

        /// <summary>
        /// Name of the sound.
        /// </summary>
        public string? Name => _sound.Name;

        /// <summary>
        /// Id of the sound.
        /// </summary>
        public string Id => _sound.Id;

        /// <summary>
        /// The path for the image to display for the current sound.
        /// </summary>
        public string? ImagePath => _sound.ImagePath;

        public PublishState PublishState => GetPublishEnum(_sound.PublishState);

        private PublishState GetPublishEnum(string publishState)
        {
            if (string.IsNullOrWhiteSpace(publishState))
            {
                return PublishState.None;
            }

            var success = Enum.TryParse(publishState, out PublishState result);
            return success ? result : PublishState.None;
        }
    }
}
