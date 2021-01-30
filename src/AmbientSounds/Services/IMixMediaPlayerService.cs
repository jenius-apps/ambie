using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Services
{
    public interface IMixMediaPlayerService
    {
        event EventHandler<Sound> SoundAdded;

        event EventHandler<Sound> SoundRemoved;

        void AddSound(Sound s);
    }
}
