using System.Collections.Generic;

namespace AmbientSounds.Models;

public class StreakHistory
{
    // Year > month > dates active
    // Example: Years["2023"]["12"] = [ 26, 27 ]
    public Dictionary<string, Dictionary<string, HashSet<int>>> Years { get; set; } = new();
}
