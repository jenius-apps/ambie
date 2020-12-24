using AmbientSounds.Constants;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model for a player user control.
    /// </summary>
    public class PlayerViewModel : ObservableObject
    {
        private readonly IMediaPlayerService _player;
        private readonly IUserSettings _userSettings;
        private string? _soundName;

        public PlayerViewModel(IMediaPlayerService player, IUserSettings userSettings)
        {
            Guard.IsNotNull(player, nameof(player));
            Guard.IsNotNull(userSettings, nameof(userSettings));

            _player = player;
            _userSettings = userSettings;
            _player.NewSoundPlayed += NewSoundPlayed;
            _player.PlaybackStateChanged += PlaybackStateChanged;

            TogglePlayStateCommand = new AsyncRelayCommand(TogglePlayStateAsync);
            RandomCommand = new RelayCommand(PlayRandom);
            Volume = userSettings.Get<double>(UserSettingsConstants.Volume);
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for toggling the play state.
        /// </summary>
        public IAsyncRelayCommand TogglePlayStateCommand { get; }

        /// <summary>
        /// Command for playing a random sound.
        /// </summary>
        public IRelayCommand RandomCommand { get; }

        /// <summary>
        /// Flag for if the player is playing or is about to.
        /// </summary>
        public bool IsPlaying => _player.PlaybackState == MediaPlaybackState.Playing || _player.PlaybackState == MediaPlaybackState.Opening;

        /// <summary>
        /// For for if player is paused or otherwise not playing.
        /// </summary>
        public bool IsPaused => !IsPlaying;

        /// <summary>
        /// Name of current sound track.
        /// </summary>
        public string? SoundName
        {
            get => _soundName;
            set => SetProperty(ref _soundName, value);
        }

        /// <summary>
        /// Volume of player. Range of 0 to 100.
        /// </summary>
        public double Volume
        {
            get => _player.Volume;
            set
            {
                _player.Volume = value;
                _userSettings.Set(UserSettingsConstants.Volume, value);
            }
        }

        /// <summary>
        /// Toggles the player's state.
        /// </summary>
        private async Task TogglePlayStateAsync()
        {
            if (IsPlaying) _player.Pause();
            else _player.Play();

            await Task.Delay(100);

            UpdatePlayState();
        }

        private void PlayRandom()
        {
            _player.PlayRandom();
        }

        private void UpdatePlayState()
        {
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
        }

        private void NewSoundPlayed(object sender, string? soundName)
        {
            SoundName = soundName;
        }

        private void PlaybackStateChanged(object sender, MediaPlaybackState state)
        {
            UpdatePlayState();
        }
    }
}
