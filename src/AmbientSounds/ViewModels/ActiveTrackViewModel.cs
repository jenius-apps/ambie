using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels
{
    public class ActiveTrackViewModel
    {
        private readonly IMixMediaPlayerService _player;

        public ActiveTrackViewModel(
            Sound s,
            IRelayCommand<Sound> removeCommand,
            IMixMediaPlayerService player)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(removeCommand, nameof(removeCommand));
            Sound = s;
            _player = player;
            RemoveCommand = removeCommand;
        }

        /// <summary>
        /// The <see cref="Sound"/>
        /// for this view model.
        /// </summary>
        public Sound Sound { get; }

        /// <summary>
        /// The volume of the sound.
        /// </summary>
        public double Volume
        {
            get => _player.GetVolume(Sound.Id) * 100;
            set
            {
                _player.SetVolume(Sound.Id, value / 100d);
            }
        }

        /// <summary>
        /// The name of the sound.
        /// </summary>
        public string Name => Sound.Name ?? "";

        /// <summary>
        /// This command will remove
        /// this sound from the active tracks list
        /// and it will pause it.
        /// </summary>
        public IRelayCommand<Sound> RemoveCommand { get; }
    }
}
