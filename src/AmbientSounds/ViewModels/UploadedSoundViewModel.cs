using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class UploadedSoundViewModel
    {
        private readonly Sound _sound;
        private readonly IUploadService _uploadService;

        public UploadedSoundViewModel(
            Sound s,
            IUploadService uploadService)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(uploadService, nameof(uploadService));

            _sound = s;
            _uploadService = uploadService;

            DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        }

        public IAsyncRelayCommand DeleteCommand { get; }

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

        public bool IsPublished => PublishState == PublishState.Published;

        public bool IsPending => !IsPublished && !IsRejected;

        public bool IsRejected => PublishState == PublishState.Rejected;

        private Task DeleteAsync()
        {
            return _uploadService.DeleteAsync(Id);
        }

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
