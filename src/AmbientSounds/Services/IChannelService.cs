using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IChannelService
    {
        Task<IReadOnlyList<Channel>> GetChannelsAsync();
        Task<bool> IsFullyDownloadedAsync(Channel channel);
        Task<bool> IsOwnedAsync(Channel channel);
    }
}