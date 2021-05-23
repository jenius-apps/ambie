using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class UploadedSoundsListViewModel : ObservableObject
    {
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly IAccountManager _accountManager;
        private readonly ISoundVmFactory _soundVmFactory;
        private readonly IUploadService _uploadService;
        private readonly ITelemetry _telemetry;

        public UploadedSoundsListViewModel(
            IOnlineSoundDataProvider onlineSoundDataProvider,
            IAccountManager accountManager,
            ISoundVmFactory soundVmFactory,
            ITelemetry telemetry,
            IUploadService uploadService)
        {
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(uploadService, nameof(uploadService));
            Guard.IsNotNull(telemetry, nameof(telemetry));

            _telemetry = telemetry;
            _onlineSoundDataProvider = onlineSoundDataProvider;
            _accountManager = accountManager;
            _soundVmFactory = soundVmFactory;
            _uploadService = uploadService;

            LoadCommand = new AsyncRelayCommand(LoadAsync);

            _uploadService.SoundUploaded += OnSoundUploaded;
            _uploadService.SoundDeleted += OnSoundDeleted;
            UploadedSounds.CollectionChanged += OnCollectionChanged;
            LoadCommand.PropertyChanged += OnLoadCommandPropChanged;
        }

        public IAsyncRelayCommand LoadCommand { get; }

        public ObservableCollection<UploadedSoundViewModel> UploadedSounds = new();

        public bool IsEmptyPlaceholderVisible => UploadedSounds.Count == 0 && !LoadCommand.IsRunning;

        private async Task LoadAsync()
        {
            UploadedSounds.Clear();

            // fetch user token
            string? token = await _accountManager.GetCatalogueTokenAsync();
            if (token is null)
            {
                return;
            }

            // fetch the user's uploaded sounds
            var sounds = await _onlineSoundDataProvider.GetUserSoundsAsync(token);
            if (sounds is null || sounds.Count == 0)
            {
                return;
            }

            foreach (var s in sounds)
            {
                var vm = _soundVmFactory.GetUploadedSoundVm(s);
                UploadedSounds.Add(vm);
            }
        }

        private async void OnSoundUploaded(object sender, Models.Sound e)
        {
            await LoadAsync();
        }

        private void OnLoadCommandPropChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(LoadCommand.IsRunning))
            {
                OnPropertyChanged(nameof(IsEmptyPlaceholderVisible));
            }
        }

        private void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsEmptyPlaceholderVisible));
        }

        private void OnSoundDeleted(object sender, string soundId)
        {
            var forDeletion = UploadedSounds.FirstOrDefault(x => x.Id == soundId);
            if (forDeletion is not null)
            {
                UploadedSounds.Remove(forDeletion);
                _telemetry.TrackEvent(TelemetryConstants.UserSoundDeleted);
            }
        }

        public void Dispose()
        {
            _uploadService.SoundUploaded -= OnSoundUploaded;
            _uploadService.SoundDeleted -= OnSoundDeleted;
            UploadedSounds.CollectionChanged -= OnCollectionChanged;
            LoadCommand.PropertyChanged -= OnLoadCommandPropChanged;
            UploadedSounds.Clear();
        }
    }
}
