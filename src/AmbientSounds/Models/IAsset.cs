using System;
using System.Collections.Generic;
using System.Text;

#nullable enable

namespace AmbientSounds.Models;

public interface IAsset
{
    /// <summary>
    /// Name of sound.
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// Localizations for this sound.
    /// </summary>
    IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; }
}
