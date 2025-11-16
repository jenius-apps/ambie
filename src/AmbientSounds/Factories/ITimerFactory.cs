using AmbientSounds.Tools;

namespace AmbientSounds.Factories;

public interface ITimerFactory
{
    ITimerService Create();
}