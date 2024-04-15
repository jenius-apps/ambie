using AmbientSounds.Models;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IXboxSlideshowService
    {
        event EventHandler<Progress<double>>? VideoDownloadTriggered;

        Task<SlideshowMode> GetSlideshowModeAsync(Sound sound);
    }
}