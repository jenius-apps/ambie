using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class ChannelVmFactory(IServiceProvider serviceProvider)
{
    public ChannelViewModel Create(Channel channel)
    {
        return new ChannelViewModel(
            channel,
            serviceProvider.GetRequiredService<IAssetLocalizer>());
    }
}
