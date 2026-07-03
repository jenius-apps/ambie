using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

/// <summary>
/// Repository to fetch page data online.
/// </summary>
public interface IPagesRepository
{
    /// <summary>
    /// Retrieves data for the catalogue page.
    /// </summary>
    /// <returns>List of catalogue rows that define the page.</returns>
    Task<IReadOnlyList<CatalogueRow>> GetCataloguePageAsync();

    /// <summary>
    /// Retrieves data for channels page.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    Task<IReadOnlyList<AssetRow>> GetChannelsPageAsync(CancellationToken ct = default);

    /// <summary>
    /// Retrieves data for the meditate page.
    /// </summary>
    /// <returns>List of catalogue rows taht define the page.</returns>
    Task<IReadOnlyList<CatalogueRow>> GetMeditatePageAsync(CancellationToken ct);
}