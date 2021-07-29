using AmbientSounds.Events;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services.Xamarin
{
    public class MixMediaPlayer : IMixMediaPlayerService
    {
        public double GlobalVolume { get; set; }
        public string CurrentMixId { get; set; }
        public Dictionary<string, string[]> Screensavers { get; }
        public MediaPlaybackState PlaybackState { get; set; }

        public event EventHandler<SoundPlayedArgs> SoundAdded;
        public event EventHandler<SoundPausedArgs> SoundRemoved;
        public event EventHandler<MixPlayedArgs> MixPlayed;
        public event EventHandler<MediaPlaybackState> PlaybackStateChanged;

        public IList<string> GetActiveIds()
        {
            throw new NotImplementedException();
        }

        public string[] GetSoundIds()
        {
            throw new NotImplementedException();
        }

        public double GetVolume(string soundId)
        {
            throw new NotImplementedException();
        }

        public bool IsSoundPlaying(string soundId)
        {
            throw new NotImplementedException();
        }

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Play()
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            throw new NotImplementedException();
        }

        public void RemoveSound(string soundId)
        {
            throw new NotImplementedException();
        }

        public void SetMixId(string mixId)
        {
            throw new NotImplementedException();
        }

        public void SetVolume(string soundId, double value)
        {
            throw new NotImplementedException();
        }

        public Task ToggleSoundAsync(Sound s, bool keepPaused = false, string parentMixId = "")
        {
            throw new NotImplementedException();
        }
    }
}
