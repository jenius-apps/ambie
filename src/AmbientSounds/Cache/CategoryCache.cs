using AmbientSounds.Models;
using AmbientSounds.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public sealed class CategoryCache : ICategoryCache
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ConcurrentDictionary<string, Category> _cache = new();

    public CategoryCache(ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetItemsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();
        await _lock.WaitAsync(ct);
        if (_cache.IsEmpty)
        {
            IReadOnlyList<Category> data = await _categoryRepository.GetItemsAsync(ct);
            foreach (Category d in data)
            {
                _ = _cache.TryAdd(d.Id, d);
            }
        }

        _ = _lock.Release();
        return [.. _cache.Values];
    }
}
