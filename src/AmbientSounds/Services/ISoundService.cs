using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface ISoundService
    {
        Task<IReadOnlyList<Sound>> GetLocalSoundsAsync();
        Task PrepopulateSoundsIfEmpty();
    }
}