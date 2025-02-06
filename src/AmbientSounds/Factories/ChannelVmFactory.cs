using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class ChannelVmFactory(IServiceProvider serviceProvider)
{
    public ChannelViewModel Create(
        Channel channel,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<ChannelViewModel>? playCommand = null)
    {
        return new ChannelViewModel(
            channel,
            serviceProvider.GetRequiredService<IAssetLocalizer>(),
            serviceProvider.GetRequiredService<IChannelService>(),
            serviceProvider.GetRequiredService<IDialogService>(),
            serviceProvider.GetRequiredService<IIapService>(),
            serviceProvider.GetRequiredService<ITelemetry>(),
            viewDetailsCommand,
            playCommand);
    }
}
