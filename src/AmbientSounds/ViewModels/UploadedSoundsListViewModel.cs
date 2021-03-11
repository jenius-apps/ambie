using AmbientSounds.Factories;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class UploadedSoundsListViewModel
    {
        private readonly IOnlineSoundDataProvider _onlineSoundDataProvider;
        private readonly IAccountManager _accountManager;
        private readonly ISoundVmFactory _soundVmFactory;
        private readonly IUploadService _uploadService;

        public UploadedSoundsListViewModel(
            IOnlineSoundDataProvider onlineSoundDataProvider,
            IAccountManager accountManager,
            ISoundVmFactory soundVmFactory,
            IUploadService uploadService)
        {
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(uploadService, nameof(uploadService));

            _onlineSoundDataProvider = onlineSoundDataProvider;
            _accountManager = accountManager;
            _soundVmFactory = soundVmFactory;
            _uploadService = uploadService;

            _uploadService.SoundUploaded += OnSoundUploaded;
            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        public IAsyncRelayCommand LoadCommand { get; }

        public ObservableCollection<UploadedSoundViewModel> UploadedSounds = new();

        private async Task LoadAsync()
        {
            UploadedSounds.Clear();

            // fetch user token
            string? token = await _accountManager.GetCatalogueTokenAsync();
            if (token == null)
            {
                return;
            }

            // fetch the user's uploaded sounds
            var sounds = await _onlineSoundDataProvider.GetUserSoundsAsync(token);
            if (sounds == null || sounds.Count == 0)
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
    }
}
