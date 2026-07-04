using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class AssetRowVmFactory(IServiceProvider serviceProvider) : IAssetRowVmFactory
{
    public CatalogueRowViewModel Create(CatalogueRow row)
    {
        return new CatalogueRowViewModel(
            row,
            serviceProvider.GetRequiredService<IAssetLocalizer>(),
            serviceProvider.GetRequiredService<ICatalogueService>(),
            serviceProvider.GetRequiredService<ISoundVmFactory>());
    }

    public ChannelRowViewModel? CreateChannelRowVm(
        AssetRow row,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<ChannelViewModel>? playCommand = null)
    {
        return Enum.TryParse(row.AssetType, out AssetRowType rowType) && rowType is AssetRowType.Channel
            ? new ChannelRowViewModel(
                row,
                viewDetailsCommand,
                playCommand,
                serviceProvider.GetRequiredService<IAssetLocalizer>(),
                serviceProvider.GetRequiredService<IChannelService>(),
                serviceProvider.GetRequiredService<IChannelVmFactory>())
            : null;
    }
}
