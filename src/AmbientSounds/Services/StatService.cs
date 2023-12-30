using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class StatService : IStatService
{
    private readonly IUserSettings _userSettings;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IStreakHistoryCache _streakHistoryCache;

    public event EventHandler<StreakChangedEventArgs>? StreakChanged;

    public StatService(
        IUserSettings userSettings,
        IMixMediaPlayerService mixMediaPlayerService,
        IStreakHistoryCache streakHistoryCache)
    {
        _userSettings = userSettings;
        _mixMediaPlayerService = mixMediaPlayerService;
        _streakHistoryCache = streakHistoryCache;

        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;
    }

    public int ValidateAndRetrieveStreak()
    {
        var now = DateTime.Now;
        var lastUpdated = StreakLastUpdated();

        if (now.Date > lastUpdated.AddDays(1).Date)
        {
            // if over 24 hours have passed since last streak update, then
            // streak is broken. So we reset.
            _userSettings.Set(UserSettingsConstants.ActiveStreakKey, 0);

            return 0;
        }
        else
        {
            return _userSettings.Get<int>(UserSettingsConstants.ActiveStreakKey);
        }
    }

    public async Task LogStreakAsync()
    {
        if (!_userSettings.Get<bool>(UserSettingsConstants.StreaksEnabledKey))
        {
            return;
        }

        var now = DateTime.Now;
        var lastUpdated = StreakLastUpdated();

        if (lastUpdated.Date >= now.Date)
        {
            return;
        }

        var currentCount = ValidateAndRetrieveStreak();
        currentCount++;
        _userSettings.Set(UserSettingsConstants.ActiveStreakKey, currentCount);
        _userSettings.Set(UserSettingsConstants.ActiveStreakUpdateDateTicksKey, now.Ticks);
        await LogActivityAsync(now);

        StreakChanged?.Invoke(this, new StreakChangedEventArgs
        {
            NewStreak = currentCount
        });
    }

    public Task<StreakHistory> GetStreakHistory()
    {
        return _streakHistoryCache.GetStreakHistoryAsync();
    }

    public async Task<IReadOnlyList<bool>> GetRecentActiveHistory(int days)
    {
        if (days <= 0)
        {
            return Array.Empty<bool>();
        }

        var tempDate = DateTime.Now.AddDays((days - 1) * -1);
        var history = await _streakHistoryCache.GetStreakHistoryAsync();
        List<bool> recentHistory = new();
        for (int i = 0; i < days; i++)
        {
            if (history.Years.TryGetValue(tempDate.Year.ToString(), out var monthly) &&
                monthly.TryGetValue(tempDate.Month.ToString(), out var dayList) &&
                dayList.Contains(tempDate.Day))
            {
                recentHistory.Add(true);
            }
            else
            {
                recentHistory.Add(false);
            }

            tempDate = tempDate.AddDays(1);
        }

        return recentHistory;
    }
    
    private async Task LogActivityAsync(DateTime date)
    {
        var streakHistory = await _streakHistoryCache.GetStreakHistoryAsync();
        var year = date.Year.ToString();
        var month = date.Month.ToString();

        if (!streakHistory.Years.ContainsKey(year))
        {
            streakHistory.Years.Add(year, new());
        }

        if (!streakHistory.Years[year].ContainsKey(month))
        {
            streakHistory.Years[year].Add(month, new());
        }

        streakHistory.Years[year][month].Add(date.Day);
        await _streakHistoryCache.UpdateStreakHistory(streakHistory);
    }

    private async void OnPlaybackChanged(object sender, MediaPlaybackState e)
    {
        if (e is MediaPlaybackState.Opening or MediaPlaybackState.Playing)
        {
            await LogStreakAsync();
        }
    }

    private DateTime StreakLastUpdated()
    {
        var lastUpdatedTicks = _userSettings.Get<long>(UserSettingsConstants.ActiveStreakUpdateDateTicksKey);
        return new DateTime(lastUpdatedTicks);
    }
}

public class StreakChangedEventArgs : EventArgs
{
    public int NewStreak { get; init; }

    public bool AnimationRecommended { get; init; }
}
