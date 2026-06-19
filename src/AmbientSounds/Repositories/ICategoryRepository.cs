using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public interface ICategoryRepository
{
    /// <summary>
    /// Retreives the given items.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>List of categories from the server.</returns>
    Task<IReadOnlyList<Category>> GetItemsAsync(CancellationToken ct = default);
}