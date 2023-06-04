using System;
using JeniusApps.Common.Tools.Uwp;
using JeniusApps.Common.Tools;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

public class WindowsMediaPlayerFactory : IMediaPlayerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public WindowsMediaPlayerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public IMediaPlayer CreatePlayer(bool disableDefaultSystemControls = false)
    {
        return new WindowsMediaPlayer(
            disableDefaultSystemControls);
    }
}
