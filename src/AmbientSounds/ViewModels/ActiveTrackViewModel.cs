using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;

namespace AmbientSounds.ViewModels
{
    public class ActiveTrackViewModel : ObservableObject
    {
        private readonly IMixMediaPlayerService _player;
        private readonly IUserSettings _userSettings;

        public ActiveTrackViewModel(
            Sound s,
            IRelayCommand<Sound> removeCommand,
            IMixMediaPlayerService player,
            IUserSettings userSettings)
        {
            Guard.IsNotNull(s, nameof(s));
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(removeCommand, nameof(removeCommand));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _userSettings = userSettings;
            Sound = s;
            _player = player;
            RemoveCommand = removeCommand;
            Volume = _userSettings.Get($"{Sound.Id}:volume", 100d);
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
                _userSettings.Set($"{Sound.Id}:volume", value);
            }
        }

        /// <summary>
        /// The name of the sound.
        /// </summary>
        public string Name => Sound.Name ?? "";

        /// <summary>
        /// Image for the sound.
        /// </summary>
        public string ImagePath => Sound.ImagePath;

        /// <summary>
        /// This command will remove
        /// this sound from the active tracks list
        /// and it will pause it.
        /// </summary>
        public IRelayCommand<Sound> RemoveCommand { get; }

        public override string ToString()
        {
            return Name;
        }
    }
}
