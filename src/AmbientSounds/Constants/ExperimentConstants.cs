using System;
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
    public static IReadOnlyList<string> AllKeys { get; } =
    [
        // add experiment constants here
    ];

    /// <summary>
    /// Key name to track the channels experiment.
    /// </summary>
    [Obsolete("Experiment is over. Results were successful.")]
    public const string ChannelsExperiment = "channelsExperiment";
}
