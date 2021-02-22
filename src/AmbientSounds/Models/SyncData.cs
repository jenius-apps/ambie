namespace AmbientSounds.Models
{
    /// <summary>
    /// Represents data that is synchronized across devices.
    /// </summary>
    public class SyncData
    {
        /// <summary>
        /// IDs of installed sounds. This includes sound mixes.
        /// </summary>
        public string[] InstalledSoundIds { get; set; } = new string[0];

        /// <summary>
        /// List of sound mixes.
        /// </summary>
        public Sound[] SoundMixes { get; set; } = new Sound[0];
    }
}
