using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.Factories;

public interface IChannelVmFactory
{
    /// <summary>
    /// Creates a viewmodel with the given parameters.
    /// </summary>
    /// <param name="channel">The channel object.</param>
    /// <param name="viewDetailsCommand">A command for when the user wants to view the details of the channel.</param>
    /// <param name="playCommand">A command for when the user wants to play the channel.</param>
    /// <param name="isNew">A property that determines of the channel is to be marked as new.</param>
    /// <returns>The created viewmodel.</returns>
    ChannelViewModel Create(
        Channel channel,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<ChannelViewModel>? playCommand = null,
        bool isNew = false);
}