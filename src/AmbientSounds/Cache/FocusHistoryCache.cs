using AmbientSounds.Extensions;
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

        public async Task AddHistoryAsync(FocusHistory focusHistory)
        {
            if (focusHistory is null || _recentCache.ContainsKey(focusHistory.StartUtcTicks))
            {
                return;
            }

            if (_cachedSummary is null)
            {
                _cachedSummary = new FocusHistorySummary();
            }

            var historyAward = focusHistory.GetAward();
            if (historyAward == HistoryAward.Gold)
            {
                _cachedSummary.GoldTrophies += 1;
            }
            else if (historyAward == HistoryAward.Silver)
            {
                _cachedSummary.SilverTrophies += 1;
            }
            else if (historyAward == HistoryAward.Bronze)
            {
                _cachedSummary.BronzeTrophies += 1;
            }

            _recentCache.TryAdd(focusHistory.StartUtcTicks, focusHistory);
            _allHistoryCache.TryAdd(focusHistory.StartUtcTicks, focusHistory);
            var historyTask = _focusHistoryRepository.SaveHistoryAsync(focusHistory);

            _cachedSummary.RecentStartTimeTicks = _recentCache.Keys.OrderByDescending(x => x).Take(5).ToArray();
            _cachedSummary.Count += 1;
            await _focusHistoryRepository.SaveSummaryAsync(_cachedSummary);
            await historyTask;
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
                await Task.Run(async () =>
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
                });
            }

            return _recentCache.Values.ToList().AsReadOnly();
        }
    }
}
