using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IGuideCache
    {
        Task AddOfflineAsync(Guide guide);
        Guide? GetCachedGuide(string guideId);
        Task<IReadOnlyList<Guide>> GetGuidesAsync(string culture);
        Task<bool> RemoveOfflineAsync(string guideId);
    }
}