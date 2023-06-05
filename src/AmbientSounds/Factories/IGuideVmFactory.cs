using AmbientSounds.Models;
using AmbientSounds.ViewModels;

namespace AmbientSounds.Factories;

public interface IGuideVmFactory
{
    GuideViewModel GetOrCreate(Guide guide);
}