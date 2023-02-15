using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;

namespace AmbientSounds.Tools.Uwp;

public class WindowsMediaPlayerFactory : IMediaPlayerFactory
{
    public IMediaPlayer CreatePlayer(bool disableDefaultSystemControls = false)
    {
        return new WindowsMediaPlayer(disableDefaultSystemControls);
    }
}
