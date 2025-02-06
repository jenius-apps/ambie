using JeniusApps.Common.Settings;

namespace AmbientSounds.Services;

public sealed class SoundVolumeService : ISoundVolumeService
{
    private readonly IUserSettings _userSettings;

    public SoundVolumeService(IUserSettings userSettings)
    {
        _userSettings = userSettings;
    }

    /// <inheritdoc/>
    public double GetVolume(string soundId, string? mixId = null)
    {
        if (mixId is not { Length: > 0 } mix)
        {
            // If mix volume not requested, then immediately return 
            // the base volume.
            return _userSettings.Get(BaseKey(soundId), 100d);
        }

        // try mix volume first
        double mixVolume = _userSettings.Get(MixKey(soundId, mix), -1d);
        if (mixVolume > -1)
        {
            // If mix volume exists, then return it.
            return mixVolume;
        }

        // If mix volume didn't exist, then return the base volume.
        return _userSettings.Get(BaseKey(soundId), 100d);
    }

    /// <inheritdoc/>
    public void SetVolume(double volume, string soundId, string? mixId = null)
    {
        string key = mixId is not { Length: > 0 } mix
            ? BaseKey(soundId)
            : MixKey(soundId, mix);

        _userSettings.Set(key, volume);
    }

    private static string BaseKey(string soundId) => $"{soundId}:volume";

    private static string MixKey(string soundId, string mixId) => $"{mixId}:{soundId}:volume";
}
