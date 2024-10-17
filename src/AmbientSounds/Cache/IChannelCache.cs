using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public interface IChannelCache
{
    /// <summary>
    /// Fetches the given item.
    /// </summary>
    Task<Channel?> GetItemAsync(string id);

    /// <summary>
    /// Fetches all items.
    /// </summary>
    Task<IReadOnlyDictionary<string, Channel>> GetItemsAsync();

    /// <summary>
    /// Fetches the given items.
    /// </summary>
    Task<IReadOnlyDictionary<string, Channel>> GetItemsAsync(IReadOnlyList<string> ids);
}