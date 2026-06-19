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
    public async Task<IReadOnlyList<Category>> GetCategoriesAsync(
        IReadOnlyList<CategorySupportedPage>? requestedPages = null,
        CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        IEnumerable<Category> categories = await _cache.GetItemsAsync(ct);

        if (requestedPages is { Count: > 0 })
        {
            IEnumerable<string> requestedPageStrings = requestedPages.Select(x => x.ToString());
            categories = categories.Where(x => x.SupportedPages is null || x.SupportedPages.Intersect(requestedPageStrings).Any());
        }

        return [.. categories.OrderBy(x => x.Id)];
    }
}
