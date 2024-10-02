using System;

namespace AmbientSounds.Models;

public class FocusHistorySummary
{
    public int Count { get; set; }

    public long[] RecentStartTimeTicks { get; set; } = Array.Empty<long>();

    //public int GoldTrophies { get; set; }

    //public int SilverTrophies { get; set; }

    //public int BronzeTrophies { get; set; }
}
