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
        /// <inheritdoc/>
        public event EventHandler<Sound> SoundAdded;

        /// <inheritdoc/>
        public event EventHandler<string> SoundRemoved;

        private readonly Dictionary<string, MediaPlayer> _activeSounds = new Dictionary<string, MediaPlayer>();

        /// <inheritdoc/>
        public bool IsSoundPlaying(Sound s)
        {
            return !string.IsNullOrWhiteSpace(s?.Id) && _activeSounds.ContainsKey(s.Id);
        }

        /// <inheritdoc/>
        public async Task ToggleSoundAsync(Sound s)
        {
            if (string.IsNullOrWhiteSpace(s?.Id))
            {
                return;
            }

            if (IsSoundPlaying(s))
            {
                RemoveSound(s);
            }
            else
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
                    player.Play();
                    _activeSounds.Add(s.Id, player);
                    SoundAdded?.Invoke(this, s);
                }
            }
        }

        /// <inheritdoc/>
        public void SetVolume(Sound s, double value)
        {
            if (IsSoundPlaying(s) && value <= 1d && value >= 0d)
            {
                _activeSounds[s.Id].Volume = value;
            }
        }

        /// <inheritdoc/>
        public double GetVolume(Sound s)
        {
            if (IsSoundPlaying(s))
            {
                return _activeSounds[s.Id].Volume;
            }

            return 0;
        }

        /// <inheritdoc/>
        public void RemoveSound(Sound s)
        {
            if (string.IsNullOrWhiteSpace(s?.Id) || !IsSoundPlaying(s))
            {
                return;
            }

            var player = _activeSounds[s.Id];
            player.Pause();
            player.Dispose();
            _activeSounds[s.Id] = null;
            _activeSounds.Remove(s.Id);
            SoundRemoved?.Invoke(this, s.Id);
        }

        private MediaPlayer CreateLoopingPlayer() => new MediaPlayer() { IsLoopingEnabled = true };
    }
}
