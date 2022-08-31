using CommunityToolkit.Diagnostics;

namespace AmbientSounds.Events
{
    /// <summary>
    /// Event arguments for when a mix is played.
    /// </summary>
    public class MixPlayedArgs
    {
        public MixPlayedArgs(string mixId, string[] soundIds)
        {
            Guard.IsNotNull(mixId, nameof(mixId));
            Guard.IsNotNull(soundIds, nameof(soundIds));

            MixId = mixId;
            SoundIds = soundIds;
        }

        /// <summary>
        /// Id of mix being played.
        /// </summary>
        public string MixId { get; }

        /// <summary>
        /// Ids of sounds inside mix.
        /// </summary>
        public string[] SoundIds { get; }
    }
}
