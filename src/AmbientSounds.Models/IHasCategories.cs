using System.Collections.Generic;

namespace AmbientSounds.Models;

/// <summary>
/// An interface for objects that contain categories.
/// </summary>
public interface IHasCategories
{
    /// <summary>
    /// A list of categories associated with the asset.
    /// </summary>
    IReadOnlyList<string>? CategoryIds { get; init; }
}