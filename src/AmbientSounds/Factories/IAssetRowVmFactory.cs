using AmbientSounds.Models;
using AmbientSounds.ViewModels;
using CommunityToolkit.Mvvm.Input;

namespace AmbientSounds.Factories;

public interface IAssetRowVmFactory
{
    CatalogueRowViewModel Create(CatalogueRow row);

    /// <summary>
    /// Creates a <see cref="ChannelRowViewModel"/>.
    /// </summary>
    /// <param name="row">The asset row to use.</param>
    /// <param name="viewDetailsCommand">A command for when the user wants to view the details of the channel.</param>
    /// <param name="playCommand">A command for when the user wants to play the channel.</param>
    ChannelRowViewModel? CreateChannelRowVm(
        AssetRow row,
        IRelayCommand<ChannelViewModel>? viewDetailsCommand = null,
        IRelayCommand<ChannelViewModel>? playCommand = null);
}