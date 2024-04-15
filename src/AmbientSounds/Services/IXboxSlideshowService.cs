using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IXboxSlideshowService
    {
        Task<SlideshowMode> GetSlideshowModeAsync(Sound sound);
    }
}