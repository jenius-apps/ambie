using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IGuideCache
    {
        Task<IReadOnlyList<Guide>> GetGuidesAsync(string culture);
    }
}