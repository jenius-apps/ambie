using CommunityToolkit.Diagnostics;

namespace AmbientSounds.Events
{
    /// <summary>
    /// Event arguments for when the sound is paused.
    /// </summary>
    public class SoundPausedArgs
    {
        public SoundPausedArgs(string soundId, string parentMixId)
        {
            Guard.IsNotNull(soundId, nameof(soundId));
            Guard.IsNotNull(parentMixId, nameof(parentMixId));

            SoundId = soundId;
            ParentMixId = parentMixId;
        }

        /// <summary>
        /// The sound ID of the item which was paused.
        /// </summary>
        public string SoundId { get; }

        /// <summary>
        /// The parent mix ID, if the sound item was part
        /// of a sound mix. If this wasn't part
        /// of a sound mix, then this will be an empty string.
        /// </summary>
        public string ParentMixId { get; set; }
    }
}
