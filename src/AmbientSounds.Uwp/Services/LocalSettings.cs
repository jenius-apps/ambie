using AmbientSounds.Constants;
using System;
using System.Text.Json;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for storing and retrieving
    /// user settings for application data
    /// settings storage.
    /// </summary>
    public class LocalSettings : IUserSettings
    {
        /// <inheritdoc/>
        public event EventHandler<string>? SettingSet;

        /// <inheritdoc/>
        public T Get<T>(string settingKey)
        {
            object result = ApplicationData.Current.LocalSettings.Values[settingKey];
            return result is null ? (T)UserSettingsConstants.Defaults[settingKey] : (T)result;
        }

        /// <inheritdoc/>
        public void Set<T>(string settingKey, T value)
        {
            ApplicationData.Current.LocalSettings.Values[settingKey] = value;
            SettingSet?.Invoke(this, settingKey);
        }

        /// <inheritdoc/>
        public T? GetAndDeserialize<T>(string settingKey)
        {
            object result = ApplicationData.Current.LocalSettings.Values[settingKey];
            if (result is string serialized)
            {
                return JsonSerializer.Deserialize<T>(serialized);
            }

            return (T)UserSettingsConstants.Defaults[settingKey];
        }

        /// <inheritdoc/>
        public void SetAndSerialize<T>(string settingKey, T value)
        {
            var serialized = JsonSerializer.Serialize(value);
            Set(settingKey, serialized);
        }


        /// <inheritdoc/>
        public T Get<T>(string settingKey, T defaultOverride)
        {
            object result = ApplicationData.Current.LocalSettings.Values[settingKey];
            return result is null ? defaultOverride : (T)result;
        }
    }
}
