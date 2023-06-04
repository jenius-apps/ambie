using AmbientSounds.Models;
using AmbientSounds.ViewModels;

namespace AmbientSounds.Factories;

public interface IGuideVmFactory
{
    OnlineGuideViewModel GetOrCreate(Guide guide);
}