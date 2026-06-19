using AmbientSounds.Models;
using AmbientSounds.ViewModels;

namespace AmbientSounds.Factories;

public interface ICategoryVmFactory
{
    /// <summary>
    /// Creates the viewmodel.
    /// </summary>
    CategoryViewModel Create(Category category);
}