using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface IChannelService
{
    /// <summary>
    /// ID of channel whose details were most recently viewed.
    /// </summary>
    /// <remarks>
    /// In-memory only. Does not persist if the app restarts.
    /// </remarks>
    string? MostRecentChannelDetailsViewed { get; set; }

    /// <summary>
    /// Raised when a channel is downloaded.
    /// </summary>
    event EventHandler<string>? ChannelDownloaded;

    /// <summary>
    /// Deletes the video behind the given channel.
    /// Associated sound is not deleted because those sounds
    /// can be deleted independently from the Home page.
    /// </summary>
    /// <param name="channel">The channel to delete.</param>
    Task DeleteChannelAsync(Channel channel);

    /// <summary>
    /// Fetches available channels.
    /// </summary>
    Task<IReadOnlyList<Channel>> GetChannelsAsync();

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
    Task PlayChannelAsync(Channel channel, bool performNavigation = true);

    /// <summary>
    /// Queues the channel for download and installation.
    /// </summary>
    Task<bool> QueueInstallChannelAsync(Channel channel, Progress<double>? progress = null);

    /// <summary>
    /// Retrieves active progress for given channel.
    /// </summary>
    Progress<double>? TryGetActiveProgress(Channel c);
}