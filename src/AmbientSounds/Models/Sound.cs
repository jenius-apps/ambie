namespace AmbientSounds.Models
{
    /// <summary>
    /// A sound object.
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// Unique Id for this sound (generally used to retrieve translations for this sound's name).
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// The full path for the associated image for this sound.
        /// </summary>
        public string? ImagePath { get; set; }

        /// <summary>
        /// The name of the sound.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// The full path for the sound file.
        /// </summary>
        public string? FilePath { get; set; }

        /// <summary>
        /// Information about where the sound is from and what is its license.
        /// </summary>
        public string? Attribution { get; set; }

        /// <summary>
        /// Information about where the image is from and what is its license.
        /// </summary>
        public string? ImageAttribution { get; set; }
    }
}
