using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class SearchService : ISearchService
{
    private readonly ICatalogueService _soundService;
    private readonly IAssetLocalizer _assetLocalizer;

    public SearchService(
        ICatalogueService soundService,
        IAssetLocalizer assetLocalizer)
    {
        _soundService = soundService;
        _assetLocalizer = assetLocalizer;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Array.Empty<Sound>();
        }

        var sounds = await _soundService.GetSoundsAsync();
        return sounds.Where(x => _assetLocalizer.LocalNameContains(x, name)).ToArray();
    }
}
