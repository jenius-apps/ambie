using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IMeditateService
    {
        Task PlayAsync(Guide guide);
    }
}