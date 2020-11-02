using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using Windows.Media.Core;
using Windows.Media.Playback;
using UwpMediaPlaybackState = Windows.Media.Playback.MediaPlaybackState;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Service that controls a media player.
    /// </summary>
    public sealed class MediaPlayerService : IMediaPlayerService
    {
        /// <inheritdoc/>
        public event EventHandler NewSoundPlayed;

        /// <inheritdoc/>
        public event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        private readonly MediaPlayer _player;

        public MediaPlayerService()
        {
            _player = new MediaPlayer { IsLoopingEnabled = true };

            _player.PlaybackSession.PlaybackStateChanged += (s, e) => PlaybackStateChanged?.Invoke(this, PlaybackState);
        }

        /// <inheritdoc/>
        public Sound Current { get; private set; }

        /// <inheritdoc/>
        public MediaPlaybackState PlaybackState
        {
            get
            {
                return _player.PlaybackSession.PlaybackState switch
                {
                    UwpMediaPlaybackState.None => MediaPlaybackState.Stopped,
                    UwpMediaPlaybackState.Opening => MediaPlaybackState.Opening,
                    UwpMediaPlaybackState.Buffering => MediaPlaybackState.Opening,
                    UwpMediaPlaybackState.Playing => MediaPlaybackState.Playing,
                    UwpMediaPlaybackState.Paused => MediaPlaybackState.Paused,
                    _ => ThrowHelper.ThrowArgumentException<MediaPlaybackState>("Invalid playback state")
                };
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Play()
        {
            _player.Play();
        }

        /// <inheritdoc/>
        public void Pause()
        {
            _player.Pause();
        }
    }
}
