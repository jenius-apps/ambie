namespace AmbientSounds.Models
{
    /// <summary>
    /// A sound object.
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// Unique Id for this sound.
        /// </summary>
        /// <remarks>
        /// Generally used to retrieve
        /// translations for this sound's
        /// name.
        /// </remarks>
        public string Id { get; set; }

        /// <summary>
        /// Full ms-appx image path.
        /// </summary>
        public string ImagePath { get; set; }
        
        /// <summary>
        /// Name of sound.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Full ms-appx sound file path.
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// Information about where the sound is from
        /// and what is its license.
        /// </summary>
        public string Attribution { get; set; }
    }
}
