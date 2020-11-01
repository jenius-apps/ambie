using AmbientSounds.Services;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace AmbientSounds.ViewModels
{
    /// <summary>
    /// View model for a player user control.
    /// </summary>
    public class PlayerViewModel : ObservableObject
    {
        private readonly MediaPlayerService _player;

        public PlayerViewModel(MediaPlayerService player)
        {
            _player = player ?? throw new ArgumentNullException(nameof(player));
            _player.NewSoundPlayed += NewSoundPlayed;
        }

        /// <summary>
        /// Flag for if the player is playing or is about to.
        /// </summary>
        public bool IsPlaying => _player.PlayBackState == MediaPlaybackState.Playing || _player.PlayBackState == MediaPlaybackState.Opening;

        /// <summary>
        /// For for if player is paused or otherwise not playing.
        /// </summary>
        public bool IsPaused => !IsPlaying;

        /// <summary>
        /// Name of current sound track.
        /// </summary>
        public string SoundName => _player?.Current?.Name ?? _player?.Current?.Id ?? "None";

        /// <summary>
        /// Toggles the player's state.
        /// </summary>
        public async void TogglePlayState()
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
            UpdatePlayState();
        }
    }
}
