using System;

#nullable enable

namespace AmbientSounds.Tools.Uwp;

internal class WindowsSystemMediaControlsFactory : ISystemMediaControlsFactory
{
    public ISystemMediaControls Create()
    {
        return new WindowsSystemMediaControls();
    }
}
