using AmbientSounds.Models;
using System;
using System.Collections.Generic;

#nullable enable

namespace AmbientSounds.Events;

public class SoundChangedEventArgs : EventArgs
{
    /// <summary>
    /// Sounds added.
    /// </summary>
    public required IReadOnlyList<Sound> SoundsAdded { get; init; }

    /// <summary>
    /// Sounds removed.
    /// </summary>
    public required IReadOnlyList<string> SoundIdsRemoved { get; init; }

    /// <summary>
    /// The parent mix id, if available.
    /// </summary>
    public string ParentMixId { get; init; } = string.Empty;
}
