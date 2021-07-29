using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Xamarin.Native
{
    public interface IAudioPlayer
    {
        void Play();

        void Pause();

        void Dispose();

        bool IsLoopingEnabled { get; set; }

        bool SystemIntegratedControlsEnabled { get; set; }

        double Volume { get; set; }

        void SetSource(object source);
    }
}
