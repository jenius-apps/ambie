using System;
using Windows.Media.Core;
using Windows.Media.Playback;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for playing and managing
    /// the sound previews.
    /// </summary>
    public class PreviewService : IPreviewService
    {
        private readonly MediaPlayer _player;
        private string? _current;

        public PreviewService()
        {
            _player = new MediaPlayer();
        }

        /// <inheritdoc/>
        public void Play(string onlineUrl)
        {
            if (string.IsNullOrWhiteSpace(onlineUrl) ||
                !Uri.IsWellFormedUriString(onlineUrl, UriKind.Absolute))
            {
                _current = "";
                return;
            }

            if (_current == onlineUrl && 
                _player.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                _current = "";
                _player.Pause();
                return;
            }

            var mediaSource = MediaSource.CreateFromUri(new Uri(onlineUrl));
            _player.Source = mediaSource;
            _player.Play();
            _current = onlineUrl;
        }
    }
}
