using AmbientSounds.Models;
using System.Collections.Generic;
using System.Linq;

#nullable enable

namespace AmbientSounds.Extensions;

/// <summary>
/// Extensions related to <see cref="ShareDetail"/>.
/// </summary>
public static class ShareDetailExtensions
{
    public const char ShareDetailSplitChar = ';';

    /// <summary>
    /// Generates the composite ID based on the given IDs.
    /// </summary>
    /// <returns>The composite ID is returned.</returns>
    public static string SortAndCompose(this IReadOnlyList<string> ids)
    {
        // This method is the same as in the service.
        // Do not change unless also changing service.
        // TODO: Move to nuget shared with service.
        return string.Join(ShareDetailSplitChar.ToString(), ids.OrderBy(x => x));
    }

    /// <summary>
    /// Splits the composite ID to the individual sound IDs.
    /// </summary>
    /// <returns>List of sound IDs derived from the composite ID.</returns>
    public static IReadOnlyList<string> SplitCompositeId(this ShareDetail detail)
    {
        return detail.SoundIdComposite.Split(ShareDetailSplitChar);
    }
}
