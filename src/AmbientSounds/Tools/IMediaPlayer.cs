using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Tools;

public interface IMediaPlayer
{
    double Volume { get; set; }

    void Pause();

    void Play();

    Task<bool> SetSourceAsync(string pathToFile, bool enableGaplessLoop = false);

    bool SetUriSource(Uri uriSource, bool enableGaplessLoop = false);

    void Dispose();
}
