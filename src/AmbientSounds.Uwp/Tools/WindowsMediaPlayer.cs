using System;
using Windows.Media.Core;
using Windows.Media.Playback;

namespace AmbientSounds.Tools.Uwp
{
    public class WindowsMediaPlayer : IMediaPlayer
    {
        private readonly MediaPlayer _player = new();

        public void SetSource(string pathToFile)
        {
            try
            {
                _player.Source = MediaSource.CreateFromUri(new Uri(pathToFile));
            }
            catch { }
        }

        public void Play()
        {
            _player.Play();
        }

        public void Pause()
        {
            _player.Pause();
        }
    }
}
