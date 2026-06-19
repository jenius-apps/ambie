using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public interface ICategoryCache
{
    /// <summary>
    /// Retrieves list of categories from cache, and populates from server if cache not initialized.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>List of cached categories.</returns>
    Task<IReadOnlyList<Category>> GetItemsAsync(CancellationToken ct = default);
}