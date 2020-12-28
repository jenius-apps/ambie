﻿using AmbientSounds.Constants;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Interface for storing
    /// and retrieving user settings.
    /// </summary>
    public interface IUserSettings
    {
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
    }
}
