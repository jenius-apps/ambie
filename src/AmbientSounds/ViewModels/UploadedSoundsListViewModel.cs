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

        public UploadedSoundsListViewModel(
            IOnlineSoundDataProvider onlineSoundDataProvider,
            IAccountManager accountManager,
            ISoundVmFactory soundVmFactory)
        {
            Guard.IsNotNull(onlineSoundDataProvider, nameof(onlineSoundDataProvider));
            Guard.IsNotNull(accountManager, nameof(accountManager));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));

            _onlineSoundDataProvider = onlineSoundDataProvider;
            _accountManager = accountManager;
            _soundVmFactory = soundVmFactory;

            LoadCommand = new AsyncRelayCommand(LoadAsync);
        }

        public IAsyncRelayCommand LoadCommand { get; }

        public ObservableCollection<UploadedSoundViewModel> UploadedSounds = new();

        private async Task LoadAsync()
        {
            UploadedSounds.Clear();

            // fetch user token
            string token = await _accountManager.GetTokenAsync() ?? "foobar";
            if (string.IsNullOrWhiteSpace(token))
            {
                // TODO return once we are properly able to get token.
                //return;
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
    }
}
