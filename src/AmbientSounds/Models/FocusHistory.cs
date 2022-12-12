using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace AmbientSounds.Models
{
    public class FocusHistory
    {
        public long StartUtcTicks { get; set; }

        public long EndUtcTicks { get; set; }

        public int FocusLength { get; set; }

        public int RestLength { get; set; }

        public int Repetitions { get; set; }

        public int FocusSegmentsCompleted { get; set; }

        public int RestSegmentsCompleted { get; set; }

        /// <summary>
        /// If a session was cancelled before reaching 100% completion,
        /// this value is the amount of timeSpan ticks remaining on that segment
        /// that was cancelled. 
        /// </summary>
        public long PartialSegmentTicks { get; set; }

        [JsonConverter(typeof(JsonStringEnumConverter))]
        public SessionType PartialSegmentType { get; set; }

        public List<FocusInterruption> Interruptions { get; set; } = new();

        public int TasksCompleted { get; set; }

        public int Pauses { get; set; }
    }

    public enum HistoryAward
    {
        None,
        Bronze,
        Silver,
        Gold
    }

    public enum SessionType
    {
        None,
        Focus,
        Rest
    }
}
