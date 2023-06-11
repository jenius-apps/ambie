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
    private readonly IOfflineGuideRepository _offlineGuideRepository;
    private readonly ConcurrentDictionary<string, Guide> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);
    private string _cachedCulture = string.Empty;

    public GuideCache(
        IOnlineGuideRepository onlineGuideRepository,
        IOfflineGuideRepository offlineGuideRepository)
    {
        _onlineGuideRepository = onlineGuideRepository;
        _offlineGuideRepository = offlineGuideRepository;
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
            var offlineGuides = await _offlineGuideRepository.GetAsync();
            var guides = await _onlineGuideRepository.GetGuidesAsync(culture);

            // Note: we are only caching based on culture
            // because the likelihood of the user needing quick access to different cultures
            // is low.
            foreach (var g in guides)
            {
                // If the sound is available offline, use the offline data
                // because it would have local storage of image and sounds.
                if (offlineGuides.FirstOrDefault(x => x == g) is Guide offlineGuide)
                {
                    // Specify IsDownloaded here because this is the first point
                    // in time we know where the object came from.
                    offlineGuide.IsDownloaded = true;
                    _cache.TryAdd(offlineGuide.Id, offlineGuide);
                }
                else
                {
                    // Sound isn't offline, so cache the online data.
                    _cache.TryAdd(g.Id, g);
                }
            }
        }

        _cacheLock.Release();
        return _cache.Values.ToArray();
    }

    /// <inheritdoc/>
    public async Task AddOfflineAsync(Guide guide)
    {
        if (string.IsNullOrEmpty(guide.Id))
        {
            return;
        }

        Guide[]? guides = null;
        await _cacheLock.WaitAsync();

        // Replace the previous guide object in the cache with the new guide object
        // because the old one contained outdated file and image location data.
        if (_cache.TryRemove(guide.Id, out _))
        {
            _cache.TryAdd(guide.Id, guide);
            guides = _cache.Values.ToArray();
        }
        _cacheLock.Release();

        if (guides is not null)
        {
            await _offlineGuideRepository.SaveAsync(guides);
        }
    }
}
