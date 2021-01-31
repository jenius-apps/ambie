using AmbientSounds.Constants;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;

namespace AmbientSounds.Services
{
    public class MixMediaPlayerService : IMixMediaPlayerService
    {
        private readonly Dictionary<string, MediaPlayer> _activeSounds = new Dictionary<string, MediaPlayer>();
        private readonly int _maxActive;
        private double _globalVolume;
        private MediaPlaybackState _playbackState = MediaPlaybackState.Paused;

        /// <inheritdoc/>
        public event EventHandler<Sound> SoundAdded;

        /// <inheritdoc/>
        public event EventHandler<string> SoundRemoved;

        /// <inheritdoc/>
        public event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        public MixMediaPlayerService(IUserSettings userSettings)
        {
            _maxActive = userSettings?.Get<int>(UserSettingsConstants.MaxActive) ?? 3;
        }

        /// <inheritdoc/>
        public double GlobalVolume
        {
            get => _globalVolume;
            set => UpdateAllVolumes(value);
        }

        /// <inheritdoc/>
        public MediaPlaybackState PlaybackState
        {
            get => _playbackState;
            set
            {
                _playbackState = value;
                PlaybackStateChanged?.Invoke(this, value);
            }
        }

        private void UpdateAllVolumes(double value)
        {
            if (value < 0d || value > 1d)
            {
                return;
            }

            if (value == 0d)
            {
                // prevent volume from being permanently zero.
                value = 0.01d;
            }

            foreach (var soundId in _activeSounds.Keys)
            {
                _activeSounds[soundId].Volume = GetVolume(soundId) * value;
            }

            // Must be set last since GetVolume
            // uses the old global volume.
            _globalVolume = value;
        }

        /// <inheritdoc/>
        public bool IsSoundPlaying(string soundId)
        {
            return !string.IsNullOrWhiteSpace(soundId) && _activeSounds.ContainsKey(soundId);
        }

        /// <inheritdoc/>
        public async Task ToggleSoundAsync(Sound s)
        {
            if (string.IsNullOrWhiteSpace(s?.Id))
            {
                return;
            }

            if (IsSoundPlaying(s.Id))
            {
                RemoveSound(s.Id);
            }
            else if (_activeSounds.Count < _maxActive)
            {
                MediaSource mediaSource = null;
                if (Uri.IsWellFormedUriString(s.FilePath, UriKind.Absolute))
                {
                    // sound path is packaged and can be read as URI.
                    mediaSource = MediaSource.CreateFromUri(new Uri(s.FilePath));
                }
                else if (s.FilePath != null && s.FilePath.Contains(ApplicationData.Current.LocalFolder.Path))
                {
                    // sound path is a file saved in local folder
                    StorageFile file = await StorageFile.GetFileFromPathAsync(s.FilePath);
                    mediaSource = MediaSource.CreateFromStorageFile(file);
                }
                
                if (mediaSource != null)
                {
                    var player = CreateLoopingPlayer();
                    player.Source = mediaSource;
                    _activeSounds.Add(s.Id, player);
                    SoundAdded?.Invoke(this, s);
                    Play();
                }
            }
        }

        /// <inheritdoc/>
        public void SetVolume(string soundId, double value)
        {
            if (IsSoundPlaying(soundId) && value <= 1d && value >= 0d)
            {
                _activeSounds[soundId].Volume = value * _globalVolume;
            }
        }

        /// <inheritdoc/>
        public double GetVolume(string soundId)
        {
            if (IsSoundPlaying(soundId))
            {
                return _activeSounds[soundId].Volume / _globalVolume;
            }

            return 0;
        }

        public void Play()
        {
            PlaybackState = MediaPlaybackState.Playing;
            foreach (var p in _activeSounds.Values)
            {
                p.Play();
            }
        }

        public void Pause()
        {
            PlaybackState = MediaPlaybackState.Paused;
            foreach (var p in _activeSounds.Values)
            {
                p.Pause();
            }
        }

        /// <inheritdoc/>
        public void RemoveSound(string soundId)
        {
            if (string.IsNullOrWhiteSpace(soundId) || !IsSoundPlaying(soundId))
            {
                return;
            }

            var player = _activeSounds[soundId];
            player.Pause();
            player.Dispose();
            _activeSounds[soundId] = null;
            _activeSounds.Remove(soundId);
            SoundRemoved?.Invoke(this, soundId);

            if (_activeSounds.Count == 0)
            {
                Pause();
            }
        }

        private MediaPlayer CreateLoopingPlayer() => new MediaPlayer() { IsLoopingEnabled = true };
    }
}
