using AmbientSounds.Models;
using AmbientSounds.Services;
using AmbientSounds.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AmbientSounds.Factories;

public sealed class CategoryVmFactory : ICategoryVmFactory
{
    private readonly IServiceProvider _serviceProvider;

    public CategoryVmFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    /// <inheritdoc/>
    public CategoryViewModel Create(Category category)
    {
        return new CategoryViewModel(
            category,
            _serviceProvider.GetRequiredService<IAssetLocalizer>());
    }
}
