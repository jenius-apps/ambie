using Windows.UI.Xaml;

namespace AmbientSounds.Converters
{
    /// <summary>
    /// Converts string to ElementTheme which
    /// can be used for UIElement.RequestedTheme.
    /// </summary>
    public static class ThemeConverter
    {
        /// <summary>
        /// Converts a string theme to ElementTheme.
        /// </summary>
        /// <param name="themeSetting">'light', 'dark', or 'default'.</param>
        public static ElementTheme ToTheme(this string themeSetting)
        {
            switch (themeSetting)
            {
                case "light":
                    return ElementTheme.Light;
                case "dark":
                    return ElementTheme.Dark;
                default:
                    return ElementTheme.Default;
            }
        }
    }
}
