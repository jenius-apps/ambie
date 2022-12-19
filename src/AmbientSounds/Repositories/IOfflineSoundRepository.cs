using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IOfflineSoundRepository
    {
        Task<IReadOnlyList<Sound>> GetLocalSoundsAsync();
        Task<IReadOnlyList<Sound>> GetPrenstalledSoundsAsync();
        Task SaveLocalSoundsAsync(IList<Sound> sounds);
    }
}