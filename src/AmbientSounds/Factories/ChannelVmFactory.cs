using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class ChannelVmFactory(IServiceProvider serviceProvider)
{
    public ChannelViewModel Create(
        Channel channel,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<Channel>? changeChannelCommand = null)
    {
        return new ChannelViewModel(
            channel,
            serviceProvider.GetRequiredService<IAssetLocalizer>(),
            serviceProvider.GetRequiredService<IChannelService>(),
            serviceProvider.GetRequiredService<IDialogService>(),
            serviceProvider.GetRequiredService<IIapService>(),
            viewDetailsCommand,
            changeChannelCommand);
    }
}
