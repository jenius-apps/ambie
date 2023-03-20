using System;
using System.Collections.Generic;
using System.Text;

namespace AmbientSounds.Models;

public class Guide : Sound
{
    public double MinutesLength { get; set; }

    /// <summary>
    /// List of semicolon-separated strings representing
    /// sound IDs that are suggested as background for this 
    /// meditation guide.
    /// </summary>
    /// <remarks>
    /// E.g. "[ 'soundIdA', 'soundIdB;soundIdC', 'soundIdX' ]"
    /// </remarks>
    public IReadOnlyList<string> SuggestedBackgroundSounds { get; set; } = Array.Empty<string>();
}
