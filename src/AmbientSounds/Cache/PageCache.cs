using AmbientSounds.Models;
using AmbientSounds.Repositories;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class PageCache : IPageCache
{
    private readonly SemaphoreSlim _catalogueRowCacheLock = new(1, 1);
    private readonly List<CatalogueRow> _catalogueRowsCache = [];
    private readonly SemaphoreSlim _meditatePageCacheLock = new(1, 1);
    private readonly List<CatalogueRow> _meditatePageRowsCache = [];
    private readonly IPagesRepository _pagesRepository;

    public PageCache(IPagesRepository pagesRepository)
    {
        _pagesRepository = pagesRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CatalogueRow>> GetCatalogueRowsAsync()
    {
        await _catalogueRowCacheLock.WaitAsync();
        if (_catalogueRowsCache.Count == 0)
        {
            IReadOnlyList<CatalogueRow> rowsData = await _pagesRepository.GetCataloguePageAsync();
            _catalogueRowsCache.AddRange(rowsData);
        }

        _ = _catalogueRowCacheLock.Release();
        return _catalogueRowsCache;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CatalogueRow>> GetMeditatePageRowsAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        await _meditatePageCacheLock.WaitAsync(ct);
        if (_meditatePageRowsCache.Count == 0)
        {
            IReadOnlyList<CatalogueRow> rowsData = await _pagesRepository.GetMeditatePageAsync(ct);
            _meditatePageRowsCache.AddRange(rowsData);
        }

        _ = _meditatePageCacheLock.Release();
        return _meditatePageRowsCache;
    }
}
