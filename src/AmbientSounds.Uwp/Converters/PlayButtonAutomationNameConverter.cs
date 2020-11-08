using System;
using Windows.ApplicationModel.Resources;
using Windows.UI.Xaml.Data;

namespace AmbientSounds.Converters
{
    public class PlayButtonAutomationNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool isPaused)
            {
                var resourceLoader = ResourceLoader.GetForCurrentView();
                return isPaused ? resourceLoader.GetString("PlayerPlayText") : resourceLoader.GetString("PlayerPauseText");
            }
            else
            {
                return "";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
