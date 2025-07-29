using AmbientSounds.Models;
using AmbientSounds.ViewModels;

namespace AmbientSounds.Factories;

public interface ICatalogueRowVmFactory
{
    CatalogueRowViewModel Create(CatalogueRow row);
}