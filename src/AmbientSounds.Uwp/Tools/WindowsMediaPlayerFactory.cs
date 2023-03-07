using AmbientSounds.Services;

namespace AmbientSounds.Tools.Uwp;

public class WindowsMediaPlayerFactory : IMediaPlayerFactory
{
    private readonly IUserSettings _userSettings;

    public WindowsMediaPlayerFactory(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    /// <inheritdoc/>
    public IMediaPlayer CreatePlayer(bool disableDefaultSystemControls = false)
    {
        return new WindowsMediaPlayer(_userSettings, disableDefaultSystemControls);
    }
}
