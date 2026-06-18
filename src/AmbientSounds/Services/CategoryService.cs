using AmbientSounds.Cache;
using AmbientSounds.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class CategoryService : ICategoryService
{
    private readonly ICategoryCache _cache;

    public CategoryService(ICategoryCache cache)
    {
        _cache = cache;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IReadOnlyList<Category> result = await _cache.GetItemsAsync(ct);
        return [.. result.OrderBy(x => x.Id)];
    }
}
