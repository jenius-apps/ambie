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

        public ActiveTrackListViewModel(
            IMixMediaPlayerService player)
        {
            Guard.IsNotNull(player, nameof(player));
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

        private void OnSoundAdded(object sender, Sound e)
        {
            if (!ActiveTracks.Any(x => x.Sound?.Id == e.Id))
            {
                ActiveTracks.Add(new ActiveTrackViewModel(e, RemoveCommand, _player));
            }
        }

        private void RemoveSound(Sound s)
        {
            if (s != null)
            {
                _player.RemoveSound(s);
            }
        }

        /// <summary>
        /// List of active sounds being played.
        /// </summary>
        public ObservableCollection<ActiveTrackViewModel> ActiveTracks { get; } = new();
    }
}
