using AmbientSounds.Models;
using AmbientSounds.ViewModels;

namespace AmbientSounds.Factories;

public interface IAssetRowVmFactory
{
    CatalogueRowViewModel Create(CatalogueRow row);
    ChannelRowViewModel? CreateChannelRowVm(AssetRow row);
}