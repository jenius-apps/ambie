using AmbientSounds.Models;
using AmbientSounds.Repositories;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public sealed class ChannelCache : IChannelCache
{
    private readonly IChannelRepository _channelRepository;
    private readonly ConcurrentDictionary<string, Channel> _cache = new();
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public ChannelCache(IChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, Channel>> GetItemsAsync()
    {
        await EnsureInitializedAsync().ConfigureAwait(false);
        return _cache;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, Channel>> GetItemsAsync(IReadOnlyList<string> ids)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        Dictionary<string, Channel> result = [];
        foreach (var id in ids)
        {
            if (_cache.TryGetValue(id, out Channel value))
            {
                result.Add(id, value);
            }
        }

        return result;
    }

    /// <inheritdoc/>
    public async Task<Channel?> GetItemAsync(string id)
    {
        await EnsureInitializedAsync().ConfigureAwait(false);

        if (_cache.TryGetValue(id, out Channel channel))
        {
            return channel;
        }

        return null;
    }

    private async Task EnsureInitializedAsync()
    {
        await _cacheLock.WaitAsync();

        if (!_cache.IsEmpty)
        {
            _cacheLock.Release();
            return;
        }

        var channels = await _channelRepository.GetItemsAsync().ConfigureAwait(false);
        foreach (var c in channels)
        {
            _cache[c.Id] = c;
        }

        _cacheLock.Release();
    }
}
