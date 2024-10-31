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
        ChannelsExperiment
    ];

    /// <summary>
    /// Key name to track the catalogue previews feature.
    /// </summary>
    [Obsolete("Experiment complete. Successful.")]
    public const string CataloguePreviews = "cataloguePreviews";

    /// <summary>
    /// Key name to track the channels experiment.
    /// </summary>
    public const string ChannelsExperiment = "channelsExperiment";
}
