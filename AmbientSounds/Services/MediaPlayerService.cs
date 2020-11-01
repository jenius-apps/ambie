using AmbientSounds.Models;
using System;
using Windows.Foundation;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Service that controls a media player.
    /// </summary>
    public class MediaPlayerService
    {
        /// <summary>
        /// Triggers when a new track is opened.
        /// </summary>
        public event EventHandler NewSoundPlayed;
        public event TypedEventHandler<MediaPlaybackSession, object> PlaybackStateChanged;
        private readonly MediaPlayer _player;

        public MediaPlayerService()
        {
            _player = new MediaPlayer
            {
                IsLoopingEnabled = true
            };

            _player.PlaybackSession.PlaybackStateChanged += (s, e) => PlaybackStateChanged?.Invoke(s, e);
        }

        /// <summary>
        /// Current sound track.
        /// </summary>
        public Sound Current { get; private set; }

        /// <summary>
        /// The current playback state (e.g. paused, playing, etc.).
        /// </summary>
        public MediaPlaybackState PlaybackState => _player.PlaybackSession.PlaybackState;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        public void Play(Sound s)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.FilePath) || !Uri.IsWellFormedUriString(s.FilePath, UriKind.Absolute))
            {
                return;
            }

            if (s == Current)
            {
                if (PlaybackState == MediaPlaybackState.Playing || PlaybackState == MediaPlaybackState.Opening) _player.Pause();
                else _player.Play();
            }
            else
            {
                _player.Pause();
                _player.Source = MediaSource.CreateFromUri(new Uri(s.FilePath));
                _player.Play();

                Current = s;
                NewSoundPlayed?.Invoke(this, new EventArgs());
            }
        }

        /// <summary>
        /// Plays the current track.
        /// </summary>
        public void Play()
        {
            _player.Play();
        }

        /// <summary>
        /// Pauses the current track.
        /// </summary>
        public void Pause()
        {
            _player.Pause();
        }
    }
}
