using AmbientSounds.Constants;
using System;
using System.Text.Json;
using Xamarin.Essentials;

namespace AmbientSounds.Services.Xamarin
{
    /// <summary>
    /// Implements user settings storage for xamarin
    /// apps.
    /// </summary>
    public class UserSettings : IUserSettings
    {
        /// <inheritdoc/>
        public event EventHandler<string> SettingSet;

        /// <inheritdoc/>
        public T Get<T>(string settingKey)
        {
            object result = null;
            if (typeof(T) == typeof(string))
            {
                result = Preferences.Get(settingKey, default(string));
            }
            else if (typeof(T) == typeof(bool))
            {
                result = Preferences.Get(settingKey, default(bool));
            }
            else if (typeof(T) == typeof(DateTime))
            {
                result = Preferences.Get(settingKey, default(DateTime));
            }
            else if (typeof(T) == typeof(int))
            {
                result = Preferences.Get(settingKey, default(int));
            }
            else if (typeof(T) == typeof(double))
            {
                result = Preferences.Get(settingKey, default(double));
            }
            else if (typeof(T) == typeof(float))
            {
                result = Preferences.Get(settingKey, default(float));
            }
            else if (typeof(T) == typeof(long))
            {
                result = Preferences.Get(settingKey, default(long));
            }

            return result is T cast ? cast : (T)UserSettingsConstants.Defaults[settingKey];
        }

        /// <inheritdoc/>
        public T Get<T>(string settingKey, T defaultOverride)
        {
            object result = null;
            if (typeof(T) == typeof(string))
            {
                result = Preferences.Get(settingKey, default(string));
            }
            else if (typeof(T) == typeof(bool))
            {
                result = Preferences.Get(settingKey, default(bool));
            }
            else if (typeof(T) == typeof(DateTime))
            {
                result = Preferences.Get(settingKey, default(DateTime));
            }
            else if (typeof(T) == typeof(int))
            {
                result = Preferences.Get(settingKey, default(int));
            }
            else if (typeof(T) == typeof(double))
            {
                result = Preferences.Get(settingKey, default(double));
            }
            else if (typeof(T) == typeof(float))
            {
                result = Preferences.Get(settingKey, default(float));
            }
            else if (typeof(T) == typeof(long))
            {
                result = Preferences.Get(settingKey, default(long));
            }

            return result is T cast ? cast : defaultOverride;
        }

        /// <inheritdoc/>
        public T GetAndDeserialize<T>(string settingKey)
        {
            string value = Get<string>(settingKey);
            if (value is string serialized)
            {
                return JsonSerializer.Deserialize<T>(serialized);
            }

            return (T)UserSettingsConstants.Defaults[settingKey];
        }

        /// <inheritdoc/>
        public void Set<T>(string settingKey, T value)
        {
            if (value is float f)
            {
                Preferences.Set(settingKey, f);
            }
            else if (value is double d)
            {
                Preferences.Set(settingKey, d);
            }
            else if (value is int i)
            {
                Preferences.Set(settingKey, i);
            }
            else if (value is bool b)
            {
                Preferences.Set(settingKey, b);
            }
            else if (value is string s)
            {
                Preferences.Set(settingKey, s);
            }
            else if (value is DateTime dt)
            {
                Preferences.Set(settingKey, dt);
            }
            else if (value is long l)
            {
                Preferences.Set(settingKey, l);
            }
            else
            {
                throw new NotSupportedException("Type not supported for xamarin settings storage: " + value.GetType().Name);
            }

            SettingSet?.Invoke(this, settingKey);
        }

        /// <inheritdoc/>
        public void SetAndSerialize<T>(string settingKey, T value)
        {
            string serialized = JsonSerializer.Serialize(value);
            Set(settingKey, serialized);
        }
    }
}
