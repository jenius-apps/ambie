using AmbientSounds.Services;
using Microsoft.Extensions.DependencyInjection;
using JeniusApps.Common.Telemetry;
using System;

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
        return new OutputControlledMediaPlayer(
            _serviceProvider.GetRequiredService<IUserSettings>(),
            _serviceProvider.GetRequiredService<ITelemetry>(),
            disableDefaultSystemControls);
    }
}
