using System.Collections.Generic;

namespace AmbientSounds.Models;

/// <summary>
/// Interface that standardizes classes that can contain localizations.
/// </summary>
public interface IHasLocalizations
{
    /// <summary>
    /// Localizations for this asset.
    /// </summary>
    IReadOnlyDictionary<string, DisplayInformation> Localizations { get; set; }
}