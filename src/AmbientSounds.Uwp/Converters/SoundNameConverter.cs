using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace AmbientSounds.Converters
{
    /// <summary>
    /// Translates sound id to name
    /// if there isn't already a specific
    /// name for the sound.
    /// </summary>
    public class SoundNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var resourceLoader = ResourceLoader.GetForCurrentView();
            if (value is string soundName)
            {
                var translatedName = resourceLoader.GetString("Sound-" + soundName);
                return string.IsNullOrWhiteSpace(translatedName)
                    ? soundName
                    : translatedName;
            }
            else
            {
                return resourceLoader.GetString("ReadyToPlayText");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
