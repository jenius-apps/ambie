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

        public PlayerViewModel(IMediaPlayerService player)
        {
            Guard.IsNotNull(player, nameof(player));

            _player = player;
            _player.NewSoundPlayed += NewSoundPlayed;
            _player.PlaybackStateChanged += PlaybackStateChanged;

            TogglePlayStateCommand = new AsyncRelayCommand(TogglePlayStateAsync);
        }

        /// <summary>
        /// The <see cref="IAsyncRelayCommand"/> responsible for toggling the play state.
        /// </summary>
        public IAsyncRelayCommand TogglePlayStateCommand { get; }

        /// <summary>
        /// Flag for if the player is playing or is about to.
        /// </summary>
        public bool IsPlaying => _player.PlaybackState == MediaPlaybackState.Playing || _player.PlaybackState == MediaPlaybackState.Opening;

        /// <summary>
        /// For for if player is paused or otherwise not playing.
        /// </summary>
        public bool IsPaused => !IsPlaying;

        /// <summary>
        /// Automation name announced to users using narrator for the current available sound action.
        /// </summary>
        public string AutomationName => !IsPlaying ? "Play" : "Pause";

        /// <summary>
        /// Name of current sound track.
        /// </summary>
        public string SoundName => _player?.Current?.Name ?? _player?.Current?.Id ?? "None";

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

        private void UpdatePlayState()
        {
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
        }

        private void NewSoundPlayed(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(SoundName));
        }

        private void PlaybackStateChanged(object sender, MediaPlaybackState state)
        {
            UpdatePlayState();
        }
    }
}
