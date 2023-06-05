using AmbientSounds.Models;
using AmbientSounds.Repositories;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class GuideCache : IGuideCache
{
    private readonly IOnlineGuideRepository _onlineGuideRepository;
    private readonly ConcurrentDictionary<string, Guide> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private string _cachedCulture = string.Empty;

    public GuideCache(
        IOnlineGuideRepository onlineGuideRepository)
    {
        _onlineGuideRepository = onlineGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guide>> GetGuidesAsync(string culture)
    {
        if (culture is not { Length: > 0 })
        {
            return Array.Empty<Guide>();
        }

        await _cacheLock.WaitAsync();

        if (_cachedCulture != culture)
        {
            // Clear cache because the user switched languages,
            // so there's no point to retain guide caches for a different language.
            _cache.Clear();
            _cachedCulture = culture;
        }

        if (_cache.IsEmpty)
        {
            // TODO get offline data too
            var guides = await _onlineGuideRepository.GetGuidesAsync(culture);
            foreach (var g in guides)
            {
                _cache.TryAdd(g.Id, g);
            }
        }

        _cacheLock.Release();
        return _cache.Values.ToArray();
    }
}
