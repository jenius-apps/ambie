using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;

namespace AmbientSounds.Events
{
    /// <summary>
    /// Event arguments for when the sound is played.
    /// </summary>
    public class SoundPlayedArgs
    {
        public SoundPlayedArgs(Sound sound, string parentMixId)
        {
            Guard.IsNotNull(sound, nameof(sound));
            Guard.IsNotNull(parentMixId, nameof(parentMixId));

            Sound = sound;
            ParentMixId = parentMixId;
        }

        /// <summary>
        /// Reference to the sound which was played.
        /// </summary>
        public Sound Sound { get; }

        /// <summary>
        /// The parent mix ID, if the sound item was part
        /// of a sound mix. If this wasn't part
        /// of a sound mix, then this will be an empty string.
        /// </summary>
        public string ParentMixId { get; }
    }
}
