namespace AmbientSounds.Models
{
    /// <summary>
    /// A sound object.
    /// </summary>
    public class Sound
    {
        /// <summary>
        /// GUID.
        /// </summary>
        public string Id { get; set; } = "";

        /// <summary>
        /// Path to image file.
        /// </summary>
        public string ImagePath { get; set; } = "";

        /// <summary>
        /// Name of sound.
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Path of sound file.
        /// </summary>
        public string FilePath { get; set; } = "";

        /// <summary>
        /// Path of preview sound file.
        /// </summary>
        public string PreviewFilePath { get; set; } = "";

        /// <summary>
        /// Extension of sound file.
        /// </summary>
        public string FileExtension { get; set; } = "";

        /// <summary>
        /// Copyright or attribution text.
        /// </summary>
        public string Attribution { get; set; } = "";

        /// <summary>
        /// The list of image paths to be used
        /// for the screensaver.
        /// </summary>
        public string[]? ScreensaverImagePaths { get; set; }

        /// <summary>
        /// True if sound is premium.
        /// </summary>
        public bool IsPremium { get; set; }

        /// <summary>
        /// Id used to identify the IAP
        /// associated with this sound.
        /// </summary>
        public string IapId { get; set; } = "";

        /// <summary>
        /// If true, this sound is a custom mix.
        /// </summary>
        public bool IsMix { get; set; }

        /// <summary>
        /// The list of image paths to be used
        /// for the mix.
        /// </summary>
        public string[] ImagePaths { get; set; } = new string[0];

        /// <summary>
        /// List of sound Ids for this mix.
        /// </summary>
        public string[] SoundIds { get; set; } = new string[0];
    }
}
