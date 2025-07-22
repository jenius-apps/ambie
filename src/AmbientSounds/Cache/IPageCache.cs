using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

/// <summary>
/// Cache for page data.
/// </summary>
public interface IPageCache
{
    /// <summary>
    /// Retrieves list of catalogue page data.
    /// </summary>
    /// <returns>List of catalogue rows that define the page.</returns>
    Task<IReadOnlyList<CatalogueRow>> GetCatalogueRowsAsync();

    /// <summary>
    /// Retrieves list of meditate page data.
    /// </summary>
    /// <returns>List of meditate rows that define the page.</returns>
    Task<IReadOnlyList<CatalogueRow>> GetMeditatePageRowsAsync(CancellationToken ct);
}