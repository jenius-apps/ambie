using AmbientSounds.Constants;
using System;
using System.Text.Json.Serialization.Metadata;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for storing
    /// and retrieving user settings.
    /// </summary>
    public interface IUserSettings
    {
        /// <summary>
        /// Raised when a settings is set. Payload
        /// is the setting key.
        /// </summary>
        event EventHandler<string> SettingSet;

        /// <summary>
        /// Saves settings into persistent local
        /// storage.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <param name="value">The value to save.</param>
        void Set<T>(string settingKey, T value);

        /// <summary>
        /// Retrieves the value for the desired settings key.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <returns>The desired value or returns the default value.</returns>
        T Get<T>(string settingKey);

        /// <summary>
        /// Retrieves the value for the desired settings key.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <param name="defaultOverride">The default override to use if the setting has no value.</param>
        /// <returns>The desired value or returns the default override.</returns>
        T Get<T>(string settingKey, T defaultOverride);

        /// <summary>
        /// Retrieves the value for the desired settings key
        /// and performs json deserialization on the stored value.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> instance to deserialize <typeparamref name="T"/> values.</param>
        /// <returns>The desired value or returns the default.</returns>
        T? GetAndDeserialize<T>(string settingKey, JsonTypeInfo<T> jsonTypeInfo);

        /// <summary>
        /// Saves settings into persistent local storage
        /// after serializing the object.
        /// </summary>
        /// <typeparam name="T">Type of the value.</typeparam>
        /// <param name="settingKey">The settings key, generally found in <see cref="UserSettingsConstants"/>.</param>
        /// <param name="value">The value to save.</param>
        /// <param name="jsonTypeInfo">The <see cref="JsonTypeInfo{T}"/> instance to serialize <typeparamref name="T"/> values.</param>
        void SetAndSerialize<T>(string settingKey, T value, JsonTypeInfo<T> jsonTypeInfo);
    }
}
