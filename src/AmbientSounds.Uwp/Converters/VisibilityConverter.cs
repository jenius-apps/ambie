using Windows.UI.Xaml;

#nullable enable

namespace AmbientSounds.Converters
{
    /// <summary>
    /// Extensions for converting visiblity.
    /// </summary>
    public static class VisibilityConverter
    {
        /// <summary>
        /// Inverts the bool.
        /// </summary>
        public static bool Invert(this bool value) => !value;

        /// <summary>
        /// Inverts the given bool to a visibility.
        /// </summary>
        public static Visibility InvertBool(this bool value)
        {
            return InvertVisibility(value ? Visibility.Visible : Visibility.Collapsed);
        }

        /// <summary>
        /// Inverts the given visibility.
        /// </summary>
        public static Visibility InvertVisibility(this Visibility value)
        {
            return value == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        /// <summary>
        /// Returns visible if string is not empty. 
        /// False, otherwise.
        /// </summary>
        public static Visibility IfStringNotEmpty(this string? value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? Visibility.Collapsed
                : Visibility.Visible;
        }
    }
}
