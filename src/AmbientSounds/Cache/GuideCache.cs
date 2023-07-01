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
    private readonly ConcurrentDictionary<string, Guide> _offlineCache = new();
    private readonly ConcurrentDictionary<string, Guide> _onlineCache = new();
    private readonly SemaphoreSlim _onlineCacheLock = new(1, 1);
    private readonly SemaphoreSlim _offlineCacheLock = new(1, 1);
    private string _cachedOnlineCulture = string.Empty;

    public GuideCache(
        IOnlineGuideRepository onlineGuideRepository,
        IOfflineGuideRepository offlineGuideRepository)
    {
        _onlineGuideRepository = onlineGuideRepository;
        _offlineGuideRepository = offlineGuideRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guide>> GetOnlineGuidesAsync(string culture)
    {
        if (culture is not { Length: > 0 })
        {
            return Array.Empty<Guide>();
        }

        await _onlineCacheLock.WaitAsync();

        if (_cachedOnlineCulture != culture)
        {
            // Clear cache because the user switched languages,
            // so there's no point to retain guide caches for a different language.
            _onlineCache.Clear();
            _cachedOnlineCulture = culture;
        }

        if (_onlineCache.IsEmpty)
        {
            var onlineGuides = await _onlineGuideRepository.GetGuidesAsync(culture);

            // Note: we are only caching based on culture
            // because the likelihood of the user needing quick access to different cultures
            // is low.
            foreach (var onlineGuide in onlineGuides)
            {
                _onlineCache.TryAdd(onlineGuide.Id, onlineGuide);
            }
        }

        _onlineCacheLock.Release();
        return _onlineCache.Values.ToArray();
    }

    public async Task<IReadOnlyList<Guide>> GetOfflineGuidesAsync()
    {
        await _offlineCacheLock.WaitAsync();
        if (_offlineCache.IsEmpty)
        {
            var offlineGuides = await _offlineGuideRepository.GetAsync();
            foreach (var offlineGuide in offlineGuides)
            {
                // This is the first time the offline guide is retrieved from storage,
                // so set IsDownloaded to true so downstream processes know it's available offline.
                offlineGuide.IsDownloaded = true;
                _offlineCache.TryAdd(offlineGuide.Id, offlineGuide);
            }
        }
        _offlineCacheLock.Release();
        return _offlineCache.Values.ToArray();
    }

    /// <inheritdoc/>
    public async Task<Guide?> GetOfflineGuideAsync(string guideId)
    {
        // Ensure offline cache is initialized.
        await GetOfflineGuidesAsync();

        if (_offlineCache.TryGetValue(guideId, out Guide result))
        {
            return result;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task AddOfflineAsync(Guide guide)
    {
        if (string.IsNullOrEmpty(guide.Id) || _offlineCache.ContainsKey(guide.Id))
        {
            return;
        }

        Guide[]? guides = null;
        await _offlineCacheLock.WaitAsync();

        if (_offlineCache.TryAdd(guide.Id, guide))
        {
            guides = _offlineCache.Values.ToArray();
        }
        _offlineCacheLock.Release();

        if (guides is not null)
        {
            await _offlineGuideRepository.SaveAsync(guides);
        }
    }

    /// <inheritdoc/>
    public async Task<bool> RemoveOfflineAsync(string guideId)
    {
        if (string.IsNullOrEmpty(guideId))
        {
            return false;
        }

        bool result = false;
        await _offlineCacheLock.WaitAsync();

        if (_offlineCache.TryRemove(guideId, out _))
        {
            await _offlineGuideRepository.SaveAsync(_offlineCache.Values.ToArray());
            result = true;
        }

        _offlineCacheLock.Release();
        return result;
    }
}
