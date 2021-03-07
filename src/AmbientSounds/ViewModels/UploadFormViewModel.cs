using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class UploadFormViewModel : ObservableObject
    {
        private readonly IUploadService _uploadService;
        private readonly IAccountManager _accountManager;
        private readonly IFilePicker _filePicker;
        private string _name = "";
        private string _attribution = "";
        private string _imageUrl = "";
        private string _soundPath = "";

        public UploadFormViewModel(
            IUploadService uploadService,
            IAccountManager accountManager,
            IFilePicker filePicker)
        {
            Guard.IsNotNull(uploadService, nameof(uploadService));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(filePicker, nameof(filePicker));

            _uploadService = uploadService;
            _accountManager = accountManager;
            _filePicker = filePicker;

            SubmitCommand = new AsyncRelayCommand(SubmitAsync);
            PickSoundCommand = new AsyncRelayCommand(PickSoundFileAsync);
        }

        public IAsyncRelayCommand SubmitCommand { get; }

        public IAsyncRelayCommand PickSoundCommand { get; }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Attribution
        {
            get => _attribution;
            set => SetProperty(ref _attribution, value);
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set => SetProperty(ref _imageUrl, value);
        }

        public string SoundPath
        {
            get => _soundPath;
            set => SetProperty(ref _soundPath, value);
        }

        private async Task SubmitAsync()
        {
            bool isSignedIn = await _accountManager.IsSignedInAsync();
            if (!isSignedIn || !CanUpload())
            {
                return;
            }

            var s = new Sound
            {
                Name = Name,
                Attribution = Attribution,
                ImagePath = ImageUrl,
                FilePath = SoundPath,
                FileExtension = System.IO.Path.GetExtension(SoundPath)
            };

            try
            {
                await _uploadService.UploadAsync(s);
            }
            catch (Exception e)
            {
                // TODO handle in UI
            }
        }

        private async Task PickSoundFileAsync()
        {
            var result = await _filePicker.OpenPickerAsync();
            if (!string.IsNullOrWhiteSpace(result))
            {
                SoundPath = result;
            }
        }

        private bool CanUpload()
        {
            return !string.IsNullOrWhiteSpace(SoundPath) &&
                !string.IsNullOrWhiteSpace(ImageUrl) &&
                Uri.IsWellFormedUriString(ImageUrl, UriKind.Absolute) &&
                !string.IsNullOrWhiteSpace(Attribution) &&
                !string.IsNullOrWhiteSpace(Name);
        }
    }
}
