using AmbientSounds.Tools;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public sealed class TimerFactory : ITimerFactory
{
    private readonly IServiceProvider _serviceProvider;

    public TimerFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public ITimerService Create()
    {
        return _serviceProvider.GetRequiredService<ITimerService>();
    }
}
