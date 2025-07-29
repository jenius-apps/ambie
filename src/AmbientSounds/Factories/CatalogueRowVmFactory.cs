using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class CatalogueRowVmFactory(IServiceProvider serviceProvider) : ICatalogueRowVmFactory
{
    public CatalogueRowViewModel Create(CatalogueRow row)
    {
        return new CatalogueRowViewModel(
            row,
            serviceProvider.GetRequiredService<IAssetLocalizer>(),
            serviceProvider.GetRequiredService<ICatalogueService>(),
            serviceProvider.GetRequiredService<ISoundVmFactory>());
    }
}
