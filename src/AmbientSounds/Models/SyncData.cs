using System;

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
        public string[] InstalledSoundIds { get; set; } = Array.Empty<string>();

        /// <summary>
        /// List of sound mixes.
        /// </summary>
        public Sound[] SoundMixes { get; set; } = Array.Empty<Sound>();
    }
}
