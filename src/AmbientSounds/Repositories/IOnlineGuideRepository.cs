using AmbientSounds.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IOnlineGuideRepository
    {
        Task<string> GetDownloadUrlAsync(string guideId);
        Task<IReadOnlyList<Guide>> GetGuidesAsync(string culture);
    }
}