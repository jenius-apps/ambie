using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Repositories;
using CommunityToolkit.Diagnostics;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class ShareDetailCache : IShareDetailCache
{
    private readonly ConcurrentDictionary<string, ShareDetail> _soundIdCache = new();
    private readonly ConcurrentDictionary<string, ShareDetail> _shareIdCache = new();
    private readonly IShareDetailRepository _shareDetailRepository;

    public ShareDetailCache(
        IShareDetailRepository shareDetailRepository)
    {
        Guard.IsNotNull(shareDetailRepository);

        _shareDetailRepository = shareDetailRepository;
    }

    /// <inheritdoc/>
    public async Task<ShareDetail?> GetShareDetailAsync(IReadOnlyList<string> soundIds)
    {
        string key = soundIds.SortAndCompose();
        if (_soundIdCache.TryGetValue(key, out ShareDetail result))
        {
            return result;
        }

        ShareDetail? shareDetail = await _shareDetailRepository.GetShareDetailAsync(soundIds);
        TryAddToCache(shareDetail);

        return shareDetail;
    }

    /// <inheritdoc/>
    public async Task<ShareDetail?> GetShareDetailAsync(string shareId)
    {
        if (string.IsNullOrEmpty(shareId))
        {
            return null;
        }

        if (_shareIdCache.TryGetValue(shareId, out ShareDetail result))
        {
            return result;
        }

        ShareDetail? shareDetail = await _shareDetailRepository.GetShareDetailAsync(shareId);
        TryAddToCache(shareDetail);

        return shareDetail;
    }

    private bool TryAddToCache(ShareDetail? shareDetail)
    {
        if (shareDetail is { SoundIdComposite.Length: > 0, Id.Length: > 0 })
        {
            _soundIdCache.TryAdd(shareDetail.SoundIdComposite, shareDetail);
            _shareIdCache.TryAdd(shareDetail.Id, shareDetail);

            return true;
        }

        return false;
    }
}
