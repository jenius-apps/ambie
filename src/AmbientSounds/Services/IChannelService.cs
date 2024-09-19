using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IChannelService
    {
        Task<IReadOnlyList<Channel>> GetChannelsAsync();
    }
}