using AmbientSounds.Xamarin.Native;
using AVFoundation;
using Foundation;
using Xamarin.Forms;

[assembly: Dependency(typeof(IAudioPlayer))]

namespace AmbientSounds.Xamarin.iOS
{
    public class AudioPlayer : IAudioPlayer
    {
        private AVAudioPlayer _player;

        public AudioPlayer()
        {
        }

        public bool IsLoopingEnabled
        {
            get => !(_player?.NumberOfLoops is null) && _player.NumberOfLoops < 0;
            set
            {
                if (_player is null)
                {
                    return;
                }

                // negative number represents infinite loops.
                // ref: https://stackoverflow.com/a/6804267
                _player.NumberOfLoops = value ? -1 : 0;
            }
        }

        public bool SystemIntegratedControlsEnabled { get; set; }

        public double Volume
        {
            get => _player?.Volume ?? 0d;
            set
            {
                if (_player != null)
                {
                    _player.Volume = (float)value;
                }
            }
        }

        public void Dispose()
        {
            _player?.Dispose();
            _player = null;
        }

        public void Pause()
        {
            _player?.Pause();
        }

        public void Play()
        {
            _player?.Play();
        }

        public void SetSource(object source)
        {
            if (source is string s)
            {
                _player = AVAudioPlayer.FromUrl(NSUrl.FromString(s));
            }
        }
    }
}