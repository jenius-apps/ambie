using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IChannelService
{
    /// <summary>
    /// Raised when a channel is downloaded.
    /// </summary>
    event EventHandler<string>? ChannelDownloaded;

    /// <summary>
    /// Fetches available channels.
    /// </summary>
    Task<IReadOnlyDictionary<string, Channel>> GetChannelsAsync();

    /// <summary>
    /// Determines if the channel's components are fully downloaded.
    /// </summary>
    Task<bool> IsFullyDownloadedAsync(Channel channel);

    /// <summary>
    /// Determines if the channel is owned.
    /// </summary>
    Task<bool> IsOwnedAsync(Channel channel);

    /// <summary>
    /// Performs the necessary UX changes to play the channel.
    /// </summary>
    Task PlayChannelAsync(Channel channel);

    /// <summary>
    /// Queues the channel for download and installation.
    /// </summary>
    Task<bool> QueueInstallChannelAsync(Channel channel, Progress<double>? progress = null);
}