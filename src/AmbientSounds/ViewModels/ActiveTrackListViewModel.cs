using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
        private readonly ITelemetry _telemetry;
        private readonly bool _loadPreviousState;
        private bool _loaded;
        private readonly Queue<Sound> _addQueue = new Queue<Sound>();

        public ActiveTrackListViewModel(
            IMixMediaPlayerService player,
            ISoundVmFactory soundVmFactory,
            IUserSettings userSettings,
            ITelemetry telemetry,
            ISoundMixService soundMixService,
            ISoundDataProvider soundDataProvider,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(soundMixService, nameof(soundMixService));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(appSettings, nameof(appSettings));

            _loadPreviousState = appSettings.LoadPreviousState;
            _telemetry = telemetry;
            _soundMixService = soundMixService;
            _soundDataProvider = soundDataProvider;
            _userSettings = userSettings;
            _soundVmFactory = soundVmFactory;
            _player = player;

            _player.SoundAdded += OnSoundAdded;
            _player.SoundRemoved += OnSoundRemoved;

            RemoveCommand = new RelayCommand<Sound>(RemoveSound);
            SaveCommand = new AsyncRelayCommand<string>(SaveAsync);
            ClearCommand = new RelayCommand(ClearAll);

            ActiveTracks.CollectionChanged += ActiveTracks_CollectionChanged;
        }

        /// <summary>
        /// Clears the active tracks list.
        /// </summary>
        public IRelayCommand ClearCommand { get; }

        /// <summary>
        /// Command for saving the sound mix.
        /// </summary>
        public IAsyncRelayCommand<string> SaveCommand { get; }

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
        /// Determines if the item is a sound mix.
        /// </summary>
        public bool IsMix => !string.IsNullOrWhiteSpace(_player.CurrentMixId);

        /// <summary>
        /// List of active sounds being played.
        /// </summary>
        public ObservableCollection<ActiveTrackViewModel> ActiveTracks { get; } = new();

        /// <summary>
        /// Determines if the clear button is visible.
        /// </summary>
        public bool IsClearVisible => ActiveTracks.Count > 0;

        /// <summary>
        /// Determines if the placeholder is visible.
        /// </summary>
        public bool IsPlaceholderVisible => ActiveTracks.Count == 0;

        /// <summary>
        /// Loads prevoius state of the active track list.
        /// </summary>
        public async Task LoadPreviousStateAsync()
        {
            if (_loaded || !_loadPreviousState)
            {
                return;
            }

            var mixId = _userSettings.Get<string>(UserSettingsConstants.ActiveMixId);
            var previousActiveTrackIds = _userSettings.GetAndDeserialize<string[]>(UserSettingsConstants.ActiveTracks);
            var sounds = await _soundDataProvider.GetSoundsAsync(soundIds: previousActiveTrackIds);
            if (sounds != null && sounds.Count > 0)
            {
                foreach (var s in sounds)
                {
                    await _player.ToggleSoundAsync(s, keepPaused: true, parentMixId: mixId);
                }
            }

            _loaded = true;
        }

        private void ClearAll()
        {
            var count = ActiveTracks.Count;

            if (count > 0)
            {
                ActiveTracks.Clear();
                _player.RemoveAll();
                UpdateStoredState();
                UpdateCanSave();
            }

            _telemetry.TrackEvent(TelemetryConstants.MixCleared, new Dictionary<string, string>
            {
                { "count", count.ToString() }
            });
        }

        private async Task SaveAsync(string name)
        {
            if (SaveCommand.IsRunning || !string.IsNullOrWhiteSpace(_player.CurrentMixId))
            {
                return;
            }

            var soundIds = ActiveTracks.Select(x => x.Sound).ToArray();
            var id = await _soundMixService.SaveMixAsync(soundIds, name);
            _player.SetMixId(id);
            UpdateCanSave();

            _telemetry.TrackEvent(TelemetryConstants.MixSaved, new Dictionary<string, string>
            {
                { "count", soundIds.Length.ToString() }
            });
        }

        private void UpdateStoredState()
        {
            var ids = ActiveTracks.Select(x => x.Sound.Id).ToArray();
            _userSettings.SetAndSerialize(UserSettingsConstants.ActiveTracks, ids);
            _userSettings.Set(UserSettingsConstants.ActiveMixId, _player.CurrentMixId);
        }

        private void ActiveTracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(IsClearVisible));
            OnPropertyChanged(nameof(IsPlaceholderVisible));
        }

        private void OnSoundRemoved(object sender, SoundPausedArgs args)
        {
            var sound = ActiveTracks.FirstOrDefault(x => x.Sound?.Id == args.SoundId);
            if (sound != null)
            {
                ActiveTracks.Remove(sound);
                UpdateStoredState();
                UpdateCanSave();
            }
        }

        private async void OnSoundAdded(object sender, SoundPlayedArgs args)
        {
            if (!ActiveTracks.Any(x => x.Sound?.Id == args.Sound.Id))
            {
                ActiveTracks.Add(_soundVmFactory.GetActiveTrackVm(args.Sound, RemoveCommand));
                UpdateStoredState();
                UpdateCanSave();

                // Required to fix animation issue.
                // It seems this allows the UI to update sooner
                // so that if a Mix is being loaded, all sounds
                // are animated asynchronously.
                await Task.Delay(1); 
            }
        }
        private void UpdateCanSave() => OnPropertyChanged(nameof(CanSave));

        private void RemoveSound(Sound s)
        {
            if (s != null)
            {
                _player.RemoveSound(s.Id);
                _telemetry.TrackEvent(TelemetryConstants.MixRemoved);
            }
        }
    }
}
