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

    /// <inheritdoc/>
    public async Task<IReadOnlyList<string>> GetGuidePageAsync()
    {
        await Task.Delay(1);
        return new string[]
        {
            "582f9db1-efdc-4276-8cee-99c8a33ae125",
            "b1fb26ca-ba24-4beb-abc8-530b510ca0db",
            "43e66adc-4762-47a7-9b08-5c3e96bde0ab",
            "3b0ed907-5e93-405d-8059-eca1ceba0573",
            "5bac41f6-3c00-4250-b5d5-2e115409cb6e"
        };
    }
}
