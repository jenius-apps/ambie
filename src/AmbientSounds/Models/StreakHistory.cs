using System.Collections.Generic;

namespace AmbientSounds.Models;

public class StreakHistory
{
    // Year > month > dates active
    // Example: Years["2023"]["12"] = [ 26, 27 ]
    public Dictionary<string, StreakMonthHistory> Years { get; set; } = new();
}

public class StreakMonthHistory
{
    public Dictionary<string, HashSet<int>> Months { get; set; } = new();
}
