using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface ICategoryService
{
    /// <summary>
    /// Retrieves available categories.
    /// </summary>
    /// <param name="ct">A cancellation token.</param>
    /// <returns>List of categories.</returns>
    Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default);
}