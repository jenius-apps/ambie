using System.Collections.Generic;

namespace AmbientSounds.Models;

public class StreakHistory
{
    // Year > month > days active
    // Example: Years[2023][12] = [ 26, 27 ]
    public Dictionary<int, Dictionary<int, int[]>> Years { get; } = new();
}
