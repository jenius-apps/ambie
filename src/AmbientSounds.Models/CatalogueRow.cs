using System;
using System.Collections.Generic;

namespace AmbientSounds.Models;

public class CatalogueRow : IAsset
{
    public string Name { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public IReadOnlyList<string> SoundIds { get; set; } = Array.Empty<string>();

    public IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; } = new Dictionary<string, DisplayInformation>();
}
