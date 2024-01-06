using AmbientSounds.Models;
using AmbientSounds.Repositories;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Cache;

public class StreakHistoryCache : IStreakHistoryCache
{
    private readonly IStreakHistoryRepository _streakHistoryRepository;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private StreakHistory? _history;

    public StreakHistoryCache(IStreakHistoryRepository streakHistoryRepository)
    {
        _streakHistoryRepository = streakHistoryRepository;
    }

    /// <inheritdoc />
    public async Task<StreakHistory> GetStreakHistoryAsync()
    {
        await _lock.WaitAsync();
        _history ??= await _streakHistoryRepository.GetStreakHistoryAsync();
        _lock.Release();

        return _history ?? new StreakHistory();
    }

    /// <inheritdoc />
    public async Task UpdateStreakHistory(StreakHistory updatedHistory)
    {
        await _lock.WaitAsync();
        _history = updatedHistory;
        await _streakHistoryRepository.UpdateStreakHistoryAsync(updatedHistory);
        _lock.Release();
    }
}
