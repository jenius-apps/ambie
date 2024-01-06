using AmbientSounds.Models;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public interface IStreakHistoryRepository
    {
        Task<StreakHistory?> GetStreakHistoryAsync();
        Task UpdateStreakHistoryAsync(StreakHistory history);
    }
}