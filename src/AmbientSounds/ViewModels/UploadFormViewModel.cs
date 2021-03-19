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
        private string _donateUrl = "";
        private bool _uploading;
        private bool _rule1;
        private bool _rule2;

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
            set
            {
                SetProperty(ref _name, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public string Attribution
        {
            get => _attribution;
            set
            {
                SetProperty(ref _attribution, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set 
            {
                SetProperty(ref _imageUrl, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public string DonateUrl
        {
            get => _donateUrl;
            set => SetProperty(ref _donateUrl, value);
        }

        public bool Uploading
        {
            get => _uploading;
            set
            {
                SetProperty(ref _uploading, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public string SoundPath
        {
            get => _soundPath;
            set
            {
                SetProperty(ref _soundPath, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public bool Rule1
        {
            get => _rule1;
            set
            {
                SetProperty(ref _rule1, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public bool Rule2
        {
            get => _rule2;
            set
            {
                SetProperty(ref _rule2, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
            }
        }

        public bool IsUploadButtonEnabled => !Uploading && CanUpload() && Rule1 && Rule2;

        private async Task SubmitAsync()
        {
            bool isSignedIn = await _accountManager.IsSignedInAsync();
            if (!isSignedIn || !CanUpload() || Uploading)
            {
                return;
            }

            Uploading = true;
            var s = new Sound
            {
                Name = Name,
                Attribution = Attribution,
                ImagePath = ImageUrl,
                FilePath = SoundPath,
                PublishState = PublishState.UnderReview.ToString(),
                SponsorLinks = string.IsNullOrWhiteSpace(DonateUrl) ? new string[0] : new string[] { DonateUrl },
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

            Uploading = false;
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
