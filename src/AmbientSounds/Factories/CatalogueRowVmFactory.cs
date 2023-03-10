using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public class CatalogueRowVmFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CatalogueRowVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public CatalogueRowViewModel Create(CatalogueRow row)
    {
        return new CatalogueRowViewModel(
            row,
            _serviceProvider.GetRequiredService<IAssetLocalizer>(),
            _serviceProvider.GetRequiredService<ICatalogueService>(),
            _serviceProvider.GetRequiredService<ISoundVmFactory>());
    }
}
