using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public interface IChannelRepository
{
    /// <summary>
    /// Retrieves items from online source.
    /// </summary>
    public Task<IReadOnlyList<Channel>> GetItemsAsync();
}
