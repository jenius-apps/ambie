using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Tools
{
    public interface IMediaPlayer
    {
        void Pause();
        void Play();
        void SetSource(string pathToFile);
    }
}
