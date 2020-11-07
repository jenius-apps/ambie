using AmbientSounds.Constants;
using Windows.Storage;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Class for storing and retrieving
    /// user settings for application data
    /// settings storage.
    /// </summary>
    public class LocalSettings : IUserSettings
    {
        /// <inheritdoc/>
        public T Get<T>(string settingKey)
        {
            object result = ApplicationData.Current.LocalSettings.Values[settingKey];
            return result == null ? (T)UserSettingsConstants.Defaults[settingKey] : (T)result;
        }

        /// <inheritdoc/>
        public void Set<T>(string settingKey, T value)
        {
            ApplicationData.Current.LocalSettings.Values[settingKey] = value;
        }
    }
}
