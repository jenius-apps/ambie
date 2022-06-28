using AmbientSounds.Models;
using AmbientSounds.Repositories;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Cache
{
    public class FocusHistoryCache : IFocusHistoryCache
    {
        private readonly ConcurrentDictionary<long, FocusHistory> _recentCache = new();
        private readonly ConcurrentDictionary<long, FocusHistory> _allHistoryCache = new();
        private readonly IFocusHistoryRepository _focusHistoryRepository;
        private FocusHistorySummary? _cachedSummary;

        public FocusHistoryCache(IFocusHistoryRepository focusHistoryRepository)
        {
            Guard.IsNotNull(focusHistoryRepository, nameof(focusHistoryRepository));

            _focusHistoryRepository = focusHistoryRepository;
        }

        public async Task<FocusHistory?> GetHistoryAsync(long startTimeTicks)
        {
            if (_allHistoryCache.TryGetValue(startTimeTicks, out FocusHistory value))
            {
                return value;
            }

            var history = await _focusHistoryRepository.GetHistoryAsync(startTimeTicks);
            if (history is not null)
            {
                _allHistoryCache.TryAdd(startTimeTicks, history);
            }

            return history;
        }

        public async Task<IReadOnlyList<FocusHistory>> GetRecentAsync()
        {
            if (_recentCache.Count == 0)
            {
                var summary = _cachedSummary ?? await _focusHistoryRepository.GetSummaryAsync();
                foreach (var tick in summary.RecentStartTimeTicks)
                {
                    var history = await GetHistoryAsync(tick);
                    if (history is null)
                    {
                        continue;
                    }

                    _recentCache.TryRemove(tick, out _);
                    _recentCache.TryAdd(tick, history);
                }
            }

            return _recentCache.Values.ToList().AsReadOnly();
        }
    }
}
