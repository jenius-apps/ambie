using AmbientSounds.Models;
using System.Collections.Generic;
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
    /// Retrieves list of guide IDs to be displayed on the guide page.
    /// </summary>
    /// <returns>List of guide IDs for the guide page.</returns>
    Task<IReadOnlyList<string>> GetGuidePageAsync();
}