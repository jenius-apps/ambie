using System;
using System.Collections.Generic;

#nullable enable

namespace AmbientSounds.Constants;

/// <summary>
/// Class dedicated to holding constants related to experimentation.
/// </summary>
public class ExperimentConstants
{
    /// <summary>
    /// Convenient list of all experimentation key strings.
    /// </summary>
    public static readonly IReadOnlyList<string> AllKeys =
    [
        // add experiment constants here
    ];

    /// <summary>
    /// Key name to track the catalogue previews feature.
    /// </summary>
    [Obsolete("Experiment complete. Successful.")]
    public const string CataloguePreviews = "cataloguePreviews";
}
