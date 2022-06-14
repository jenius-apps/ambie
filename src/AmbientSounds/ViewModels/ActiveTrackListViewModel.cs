using AmbientSounds.Constants;
using AmbientSounds.Events;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
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
        private readonly ITelemetry _telemetry;
        private readonly bool _loadPreviousState;
        private bool _loaded;

        public event EventHandler? TrackListChanged;

        public ActiveTrackListViewModel(
            IMixMediaPlayerService player,
            ISoundVmFactory soundVmFactory,
            IUserSettings userSettings,
            ITelemetry telemetry,
            ISoundDataProvider soundDataProvider,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            Guard.IsNotNull(soundDataProvider, nameof(soundDataProvider));
            Guard.IsNotNull(telemetry, nameof(telemetry));
            Guard.IsNotNull(appSettings, nameof(appSettings));

            _loadPreviousState = appSettings.LoadPreviousState;
            _telemetry = telemetry;
            _soundDataProvider = soundDataProvider;
            _userSettings = userSettings;
            _soundVmFactory = soundVmFactory;
            _player = player;

            RemoveCommand = new RelayCommand<Sound>(RemoveSound);
            ClearCommand = new RelayCommand(ClearAll);
        }

        /// <summary>
        /// Clears the active tracks list.
        /// </summary>
        public IRelayCommand ClearCommand { get; }

        /// <summary>
        /// Removes the sound from active list
        /// and pauses it.
        /// </summary>
        public IRelayCommand<Sound> RemoveCommand { get; }

        /// <summary>
        /// List of active sounds being played.
        /// </summary>
        public ObservableCollection<ActiveTrackViewModel> ActiveTracks { get; } = new();

        /// <summary>
        /// Determines if the clear button is visible.
        /// </summary>
        public bool IsClearVisible => ActiveTracks.Count > 0 && _loaded;

        /// <summary>
        /// Determines if the placeholder is visible.
        /// </summary>
        public bool IsPlaceholderVisible => ActiveTracks.Count == 0 && _loaded;

        /// <summary>
        /// Loads prevoius state of the active track list.
        /// </summary>
        public async Task LoadPreviousStateAsync()
        {
            _player.SoundAdded += OnSoundAdded;
            _player.SoundRemoved += OnSoundRemoved;
            ActiveTracks.CollectionChanged += ActiveTracks_CollectionChanged;

            if (ActiveTracks.Count > 0 || !_loadPreviousState)
            {
                return;
            }

            string[] soundIds = _player.GetSoundIds();
            if (soundIds is { Length: > 0 })
            {
                // This case is when the track list is returning to view because of a page navigation.

                var sounds = await _soundDataProvider.GetSoundsAsync(soundIds: soundIds);
                if (sounds is { Count: > 0 })
                {
                    foreach (var s in sounds)
                    {
                        AddSoundTrack(s);
                    }
                }
            }
            else
            {
                // This case is when the app is being launched.

                var mixId = _userSettings.Get<string>(UserSettingsConstants.ActiveMixId);
                var previousActiveTrackIds = _userSettings.GetAndDeserialize<string[]>(UserSettingsConstants.ActiveTracks);
                var sounds = await _soundDataProvider.GetSoundsAsync(soundIds: previousActiveTrackIds);
                if (sounds is not null && sounds.Count > 0)
                {
                    foreach (var s in sounds)
                    {
                        await _player.ToggleSoundAsync(s, keepPaused: true, parentMixId: mixId);
                    }
                }

                // Since this is when the app is launching, try to resume automatically
                // after populating the track list.
                if (_userSettings.Get<bool>(UserSettingsConstants.ResumeOnLaunchKey))
                {
                    _player.Play();
                    _telemetry.TrackEvent(TelemetryConstants.PlaybackAutoResume);
                }
            }

            _loaded = true;
            OnPropertyChanged(nameof(IsClearVisible));
            OnPropertyChanged(nameof(IsPlaceholderVisible));
        }

        private void ClearAll()
        {
            var count = ActiveTracks.Count;

            if (count > 0)
            {
                ActiveTracks.Clear();
                _player.RemoveAll();
                UpdateStoredState();
            }

            _telemetry.TrackEvent(TelemetryConstants.MixCleared, new Dictionary<string, string>
            {
                { "count", count.ToString() }
            });
        }

        private void UpdateStoredState()
        {
            var ids = ActiveTracks.Select(static x => x.Sound.Id).ToArray();
            _userSettings.SetAndSerialize(UserSettingsConstants.ActiveTracks, ids);
            _userSettings.Set(UserSettingsConstants.ActiveMixId, _player.CurrentMixId);
        }

        private void ActiveTracks_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            TrackListChanged?.Invoke(this, EventArgs.Empty);
            OnPropertyChanged(nameof(IsClearVisible));
            OnPropertyChanged(nameof(IsPlaceholderVisible));
        }

        private void OnSoundRemoved(object sender, SoundPausedArgs args)
        {
            var sound = ActiveTracks.FirstOrDefault(x => x.Sound?.Id == args.SoundId);
            if (sound is not null)
            {
                ActiveTracks.Remove(sound);
                UpdateStoredState();
            }
        }

        private void OnSoundAdded(object sender, SoundPlayedArgs args)
        {
            if (args?.Sound is not null)
            {
                AddSoundTrack(args.Sound);
            }
        }

        private void AddSoundTrack(Sound sound)
        {
            if (!ActiveTracks.Any(x => x.Sound?.Id == sound.Id))
            {
                ActiveTracks.Add(_soundVmFactory.GetActiveTrackVm(sound, RemoveCommand));
                UpdateStoredState();
            }
        }

        private void RemoveSound(Sound? s)
        {
            if (s is not null)
            {
                _player.RemoveSound(s.Id);
                _telemetry.TrackEvent(TelemetryConstants.MixRemoved);
            }
        }

        public void Dispose()
        {
            ActiveTracks.CollectionChanged -= ActiveTracks_CollectionChanged;
            _player.SoundAdded -= OnSoundAdded;
            _player.SoundRemoved -= OnSoundRemoved;
        }
    }
}
