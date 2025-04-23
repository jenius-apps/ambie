﻿using System.Collections.Generic;

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
        PromoCodeExperiment,
        StatsPageExperiment,
    ];

    /// <summary>
    /// Experiment key for the promo code feature.
    /// </summary>
    public const string PromoCodeExperiment = nameof(PromoCodeExperiment);

    /// <summary>
    /// Experiment key for the stats page feature.
    /// </summary>
    public const string StatsPageExperiment = nameof(StatsPageExperiment);
}
