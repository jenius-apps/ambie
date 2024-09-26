using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IChannelService
    {
        Task<IReadOnlyList<Channel>> GetChannelsAsync();
        Task<bool> IsFullyDownloadedAsync(Channel channel);
        Task<bool> IsOwnedAsync(Channel channel);
        Task<bool> QueueInstallChannelAsync(Channel channel, Progress<double>? progress = null);
    }
}