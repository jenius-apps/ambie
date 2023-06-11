using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IOfflineGuideRepository
    {
        Task<IReadOnlyList<Guide>> GetAsync();
        Task SaveAsync(IReadOnlyList<Guide> guides);
    }
}