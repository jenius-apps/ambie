using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public interface IStreakHistoryCache
    {
        Task<StreakHistory> GetStreakHistoryAsync();

        Task UpdateStreakHistory(StreakHistory updatedHistory);
    }
}