using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;

namespace AmbientSounds.ViewModels
{
    public class ActiveTrackListViewModel
    {
        private readonly IMixMediaPlayerService _player;
        private readonly ISoundVmFactory _soundVmFactory;

        public ActiveTrackListViewModel(
            IMixMediaPlayerService player,
            ISoundVmFactory soundVmFactory)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(soundVmFactory, nameof(soundVmFactory));

            _soundVmFactory = soundVmFactory;
            _player = player;

            _player.SoundAdded += OnSoundAdded;
            _player.SoundRemoved += OnSoundRemoved;

            RemoveCommand = new RelayCommand<Sound>(RemoveSound);
        }

        /// <summary>
        /// Removes the sound from active list
        /// and pauses it.
        /// </summary>
        public IRelayCommand<Sound> RemoveCommand { get; }

        private void OnSoundRemoved(object sender, string soundId)
        {
            var sound = ActiveTracks.FirstOrDefault(x => x.Sound?.Id == soundId);
            if (sound != null)
            {
                ActiveTracks.Remove(sound);
            }
        }

        private void OnSoundAdded(object sender, Sound s)
        {
            if (!ActiveTracks.Any(x => x.Sound?.Id == s.Id))
            {
                ActiveTracks.Add(_soundVmFactory.GetActiveTrackVm(s, RemoveCommand));
            }
        }

        private void RemoveSound(Sound s)
        {
            if (s != null)
            {
                _player.RemoveSound(s.Id);
            }
        }

        /// <summary>
        /// List of active sounds being played.
        /// </summary>
        public ObservableCollection<ActiveTrackViewModel> ActiveTracks { get; } = new();
    }
}
