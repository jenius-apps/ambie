using AmbientSounds.Constants;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services
{
    public sealed class RecentFocusService : IRecentFocusService
    {
        private long _minDateTimeTicks = long.MaxValue;
        private ConcurrentDictionary<long, RecentFocusSettings>? _cache;
        private readonly IFocusService _focusService;
        private readonly IUserSettings _userSettings;

        public RecentFocusService(
            IFocusService focusService,
            IUserSettings userSettings)
        {
            Guard.IsNotNull(focusService, nameof(focusService));
            Guard.IsNotNull(userSettings, nameof(userSettings));
            _focusService = focusService;
            _userSettings = userSettings;
        }

        public Task AddRecentAsync(int focusMinutes, int restMinutes, int repeats, DateTime? lastUsed = null)
        {
            if (!_focusService.CanStartSession(focusMinutes, restMinutes))
            {
                return Task.CompletedTask;
            }

            EnsureCacheInitialized();

            var settings = new RecentFocusSettings
            {
                LastUsed = lastUsed ?? DateTime.Now,
                FocusMinutes = focusMinutes,
                RestMinutes = restMinutes,
                Repeats = repeats
            };

            if (_cache!.Values.Contains(settings))
            {
                return Task.CompletedTask;
            }

            if (_cache.Count >= 5 && _cache.TryGetValue(_minDateTimeTicks, out _))
            {
                var success = _cache.TryRemove(_minDateTimeTicks, out _);
                if (success)
                {
                    _minDateTimeTicks = long.MaxValue;
                }
            }

            _cache.TryAdd(settings.LastUsed.Ticks, settings);
            _userSettings.SetAndSerialize(UserSettingsConstants.RecentFocusKey, _cache.Values.ToArray());

            foreach (var key in _cache.Keys)
            {
                if (key < _minDateTimeTicks)
                {
                    _minDateTimeTicks = key;
                }
            }

            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<RecentFocusSettings>> GetRecentAsync()
        {
            EnsureCacheInitialized();
            IReadOnlyList<RecentFocusSettings> list = _cache!.Values.ToList();
            return Task.FromResult(list);
        }

        private void EnsureCacheInitialized()
        {
            if (_cache is null)
            {
                _cache = new();
                var storedFocusSettings = _userSettings.GetAndDeserialize<RecentFocusSettings[]>(UserSettingsConstants.RecentFocusKey)
                    ?? Array.Empty<RecentFocusSettings>();

                foreach (var settings in storedFocusSettings)
                {
                    if (settings.LastUsed.Ticks < _minDateTimeTicks)
                    {
                        _minDateTimeTicks = settings.LastUsed.Ticks;
                    }

                    _cache.TryAdd(settings.LastUsed.Ticks, settings);
                }
            }
        }
    }
}
