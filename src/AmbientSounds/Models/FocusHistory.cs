using System;
using System.Collections.Generic;
using System.Text;

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

        /// <summary>
        /// Key = utc ticks start time.
        /// Value = minutes length of the interruption as given by user.
        /// </summary>
        public Dictionary<long, double> Interruptions { get; set; } = new();
    }

    public enum HistoryAward
    {
        None,
        Bronze,
        Silver,
        Gold
    }
}
