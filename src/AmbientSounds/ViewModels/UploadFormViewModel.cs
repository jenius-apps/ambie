using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using ByteSizeLib;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class UploadFormViewModel : ObservableObject
    {
        private readonly IUploadService _uploadService;
        private readonly IAccountManager _accountManager;
        private readonly IFilePicker _filePicker;
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly ITelemetry _telemetry;
        private string _name = "";
        private string _attribution = "";
        private string _imageUrl = "";
        private string _soundPath = "";
        private string _donateUrl = "";
        private bool _uploading;
        private bool _rule1;
        private bool _fileTooBig;
        private bool _uploadLimitReached;

        public UploadFormViewModel(
            IUploadService uploadService,
            IAccountManager accountManager,
            IFilePicker filePicker,
            ITelemetry telemetry,
            IOnlineSoundDataProvider onlineSoundDataProvider)
        {
            Guard.IsNotNull(uploadService, nameof(uploadService));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(filePicker, nameof(filePicker));
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _telemetry = telemetry;
            _uploadService = uploadService;
            _accountManager = accountManager;
            _filePicker = filePicker;
            _onlineSoundDataProvider = onlineSoundDataProvider;

            SubmitCommand = new AsyncRelayCommand(SubmitAsync);
            PickSoundCommand = new AsyncRelayCommand(PickSoundFileAsync);

            _onlineSoundDataProvider.UserSoundsFetched += CheckUserListcount;
            _uploadService.SoundDeleted += OnUserSoundDeleted;
        }

        private async void OnUserSoundDeleted(object sender, string e)
        {
            var token = await _accountManager.GetCatalogueTokenAsync();
            if (token is not null)
            {
                var sounds = await _onlineSoundDataProvider.GetUserSoundsAsync(token);
                if (sounds is not null)
                {
                    CheckUserListcount(this, sounds.Count);
                }
            }
        }

        public ObservableCollection<ErrorViewModel> Errors { get; } = new();

        public IAsyncRelayCommand SubmitCommand { get; }

        public IAsyncRelayCommand PickSoundCommand { get; }

        public string Name
        {
            get => _name;
            set
            {
                SetProperty(ref _name, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
                OnPropertyChanged(nameof(PreviewVisible));
            }
        }

        public string Attribution
        {
            get => _attribution;
            set
            {
                SetProperty(ref _attribution, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
                OnPropertyChanged(nameof(PreviewVisible));
            }
        }

        public string ImageUrl
        {
            get => _imageUrl;
            set 
            {
                SetProperty(ref _imageUrl, value);
                OnPropertyChanged(nameof(IsUploadButtonEnabled));
                OnPropertyChanged(nameof(PreviewVisible));
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

        public bool PreviewVisible => 
            !string.IsNullOrWhiteSpace(ImageUrl) ||
            !string.IsNullOrWhiteSpace(Attribution) ||
            !string.IsNullOrWhiteSpace(Name);

        public bool IsUploadButtonEnabled => !Uploading && CanUpload() && Rule1;

        public void TermsClicked()
        {
            _telemetry.TrackEvent(TelemetryConstants.UploadTermsOfUseClicked);
        }

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
                SponsorLinks = string.IsNullOrWhiteSpace(DonateUrl) ? Array.Empty<string>() : new string[] { DonateUrl },
                FileExtension = System.IO.Path.GetExtension(SoundPath)
            };

            RemoveError(Errors, ErrorConstants.CustomId);

            try
            {
                await _uploadService.UploadAsync(s);
            }
            catch (Exception e)
            {
                Errors.Add(new ErrorViewModel(
                    ErrorConstants.CustomId,
                    e.Message + Environment.NewLine + e.InnerException?.Message));

                _telemetry.TrackError(e, new Dictionary<string, string>
                {
                    { "soundObject", JsonSerializer.Serialize(s) }
                });
            }

            _telemetry.TrackEvent(TelemetryConstants.UploadClicked);
            Uploading = false;
        }

        private async Task PickSoundFileAsync()
        {
            (string path, ulong sizeInBytes) = await _filePicker.OpenPickerAndGetSizeAsync();
            if (!string.IsNullOrWhiteSpace(path))
            {
                SoundPath = path;
            }

            var size = new ByteSize(sizeInBytes);
            if (size > ByteSize.FromMegaBytes(ErrorConstants.SizeLimit))
            {
                if (!_fileTooBig)
                {
                    // Only add the error if it was not set before.
                    _fileTooBig = true;
                    Errors.Add(new ErrorViewModel(ErrorConstants.BigFileId));
                }
            }
            else
            {
                _fileTooBig = false;
                RemoveError(Errors, ErrorConstants.BigFileId);
            }

            _telemetry.TrackEvent(TelemetryConstants.UploadFilePicked, new Dictionary<string, string>
            {
                { "size", Math.Round(size.MegaBytes).ToString() }
            });
        }

        private void CheckUserListcount(object sender, int userSoundsCount)
        {
            if (userSoundsCount >= ErrorConstants.UploadLimit)
            {
                if (!_uploadLimitReached)
                {
                    // Only add the error if it was not set before.
                    _uploadLimitReached = true;
                    Errors.Add(new ErrorViewModel(ErrorConstants.UploadLimitId));
                }
            }
            else
            {
                _uploadLimitReached = false;
                RemoveError(Errors, ErrorConstants.UploadLimitId);
            }
        }

        private bool CanUpload()
        {
            return !string.IsNullOrWhiteSpace(SoundPath) &&
                !string.IsNullOrWhiteSpace(ImageUrl) &&
                Uri.IsWellFormedUriString(ImageUrl, UriKind.Absolute) &&
                !string.IsNullOrWhiteSpace(Attribution) &&
                !string.IsNullOrWhiteSpace(Name) &&
                !_fileTooBig &&
                !_uploadLimitReached;
        }

        /// <summary>
        /// Helper for removing errors.
        /// </summary>
        private static void RemoveError(ObservableCollection<ErrorViewModel> errors, string id)
        {
            var errorToRemove = errors.FirstOrDefault(x => x.ErrorId == id);
            if (errorToRemove is not null)
            {
                errors.Remove(errorToRemove);
            }
        }

        public void Dispose()
        {
            _onlineSoundDataProvider.UserSoundsFetched -= CheckUserListcount;
            _uploadService.SoundDeleted -= OnUserSoundDeleted;
        }
    }
}
