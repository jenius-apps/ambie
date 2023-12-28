using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public interface IStatService
    {
        event EventHandler<StreakChangedEventArgs>? StreakChanged;

        int GetActiveStreak();
        Task LogStreakAsync();
    }
}