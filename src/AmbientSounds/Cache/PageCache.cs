using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class PageCache : IPageCache
{
    private readonly SemaphoreSlim _catalogueRowCacheLock = new(1, 1);
    private readonly List<CatalogueRow> _catalogueRowsCache = new();
    private readonly IPagesRepository _pagesRepository;

    public PageCache(IPagesRepository pagesRepository)
    {
        Guard.IsNotNull(pagesRepository);
        _pagesRepository = pagesRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CatalogueRow>> GetCatalogueRowsAsync()
    {
        await _catalogueRowCacheLock.WaitAsync();
        if (_catalogueRowsCache.Count == 0)
        {
            var rowsData = await _pagesRepository.GetCataloguePageAsync();
            _catalogueRowsCache.AddRange(rowsData);
        }

        _catalogueRowCacheLock.Release();
        return _catalogueRowsCache;
    }
}
