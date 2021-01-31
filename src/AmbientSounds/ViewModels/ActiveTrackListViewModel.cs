using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    public class ActiveTrackListViewModel : ObservableObject
    {
        private readonly IMixMediaPlayerService _player;
        private readonly ISoundVmFactory _soundVmFactory;
        private readonly IUserSettings _userSettings;
        private readonly ISoundDataProvider _soundDataProvider;
        private readonly ISoundMixService _soundMixService;

        public ActiveTrackListViewModel(
            IMixMediaPlayerService player,
            ISoundVmFactory soundVmFactory,
            IUserSettings userSettings,
            ISoundMixService soundMixService,
            ISoundDataProvider soundDataProvider)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));

            _soundMixService = soundMixService;
            _soundDataProvider = soundDataProvider;
            _userSettings = userSettings;
            _soundVmFactory = soundVmFactory;
            _player = player;

            _player.SoundAdded += OnSoundAdded;
            _player.SoundRemoved += OnSoundRemoved;

            RemoveCommand = new RelayCommand<Sound>(RemoveSound);
            SaveCommand = new AsyncRelayCommand(SaveAsync);
        }

        public IAsyncRelayCommand SaveCommand { get; }

        /// <summary>
        /// Removes the sound from active list
        /// and pauses it.
        /// </summary>
        public IRelayCommand<Sound> RemoveCommand { get; }

        /// <summary>
        /// Save button is visible if true.
        /// </summary>
        public bool CanSave => string.IsNullOrWhiteSpace(_player.CurrentMixId) && ActiveTracks.Count > 1;

        /// <summary>
        /// List of active sounds being played.
        /// </summary>
        public ObservableCollection<ActiveTrackViewModel> ActiveTracks { get; } = new();

        /// <summary>
        /// Loads prevoius state of the active track list.
        /// </summary>
        public async Task LoadPreviousStateAsync()
        {
            var mixId = _userSettings.Get<string>(UserSettingsConstants.ActiveMixId);
            var previousActiveTrackIds = _userSettings.GetAndDeserialize<string[]>(UserSettingsConstants.ActiveTracks);
            var sounds = await _soundDataProvider.GetSoundsAsync(previousActiveTrackIds);
            if (sounds != null && sounds.Count > 0)
            {
                foreach (var s in sounds)
                {
                    await _player.ToggleSoundAsync(s, keepPaused: true, parentMixId: mixId);
                }
            }
        }

        private async Task SaveAsync()
        {
            if (SaveCommand.IsRunning || !string.IsNullOrWhiteSpace(_player.CurrentMixId))
            {
                return;
            }

            var id = await _soundMixService.SaveMixAsync(ActiveTracks.Select(x => x.Sound).ToArray());
            _player.CurrentMixId = id;
            UpdateCanSave();
        }

        private void UpdateStoredState()
        {
            var ids = ActiveTracks.Select(x => x.Sound.Id).ToArray();
            _userSettings.SetAndSerialize(UserSettingsConstants.ActiveTracks, ids);
            _userSettings.Set(UserSettingsConstants.ActiveMixId, _player.CurrentMixId);
        }

        private void OnSoundRemoved(object sender, string soundId)
        {
            var sound = ActiveTracks.FirstOrDefault(x => x.Sound?.Id == soundId);
            if (sound != null)
            {
                ActiveTracks.Remove(sound);
                UpdateStoredState();
                UpdateCanSave();
            }
        }

        private void OnSoundAdded(object sender, Sound s)
        {
            if (!ActiveTracks.Any(x => x.Sound?.Id == s.Id))
            {
                ActiveTracks.Add(_soundVmFactory.GetActiveTrackVm(s, RemoveCommand));
                UpdateStoredState();
                UpdateCanSave();
            }
        }

        private void UpdateCanSave() => OnPropertyChanged(nameof(CanSave));

        private void RemoveSound(Sound s)
        {
            if (s != null)
            {
                _player.RemoveSound(s.Id);
            }
        }
    }
}
