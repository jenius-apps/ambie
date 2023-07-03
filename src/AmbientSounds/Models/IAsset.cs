using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace AmbientSounds.Models;

public interface IAsset
{
    /// <summary>
    /// Name of asset.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Description of asset.
    /// </summary>
    string Description { get; set; }

    /// <summary>
    /// Localizations for this asset.
    /// </summary>
    IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; }
}
