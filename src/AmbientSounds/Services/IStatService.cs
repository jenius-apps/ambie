using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IStatService
    {
        event EventHandler<StreakChangedEventArgs>? StreakChanged;

        int ValidateAndRetrieveStreak();
        Task LogStreakAsync();
        Task<IReadOnlyList<bool>> GetRecentActiveHistory(int days);
    }
}