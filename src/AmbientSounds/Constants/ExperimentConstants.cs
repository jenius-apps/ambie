using System.Collections.Generic;

namespace AmbientSounds.Constants;

/// <summary>
/// Class dedicated to holding constants related to experimentation.
/// </summary>
public class ExperimentConstants
{
    /// <summary>
    /// Convenient list of all experimentation key strings.
    /// </summary>
    /// <remarks>
    /// Any experiment key inserted into this list means that the experiment is active when the app is launched.
    /// </remarks>
    public static IReadOnlyList<string> AllKeys { get; } =
    [
        // add experiment constants here
        RelaxPageV2,
    ];

    public const string RelaxPageV2 = nameof(RelaxPageV2);
}
