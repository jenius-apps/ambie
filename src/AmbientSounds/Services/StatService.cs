using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Extensions;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Tools;
using JeniusApps.Common.Settings;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class StatService : IStatService
{
    private readonly IUserSettings _userSettings;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private readonly IStreakHistoryCache _streakHistoryCache;
    private readonly ITimerService _dayTimer;
    private readonly ITimerService _usageTimer;
    private readonly IFocusHistoryService _focusHistoryService;
    private readonly ISoundCache _soundCache;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly TimeSpan _usageInterval = TimeSpan.FromMinutes(15);

    public event EventHandler<StreakChangedEventArgs>? StreakChanged;

    public StatService(
        IUserSettings userSettings,
        IMixMediaPlayerService mixMediaPlayerService,
        IStreakHistoryCache streakHistoryCache,
        ITimerFactory timerFactory,
        IFocusHistoryService focusHistoryService,
        ISoundCache soundCache,
        IAssetLocalizer assetLocalizer)
    {
        _userSettings = userSettings;
        _mixMediaPlayerService = mixMediaPlayerService;
        _streakHistoryCache = streakHistoryCache;
        _focusHistoryService = focusHistoryService;
        _soundCache = soundCache;
        _assetLocalizer = assetLocalizer;

        _dayTimer = timerFactory.Create();
        _dayTimer.Interval = 86400000; // 24 hours
        _dayTimer.IntervalElapsed += OnDayElapsed;

        _usageTimer = timerFactory.Create();
        _usageTimer.Interval = (int)_usageInterval.TotalMilliseconds; // 15 minutes
        _usageTimer.IntervalElapsed += OnUsageIntervalElapsed;

        _mixMediaPlayerService.PlaybackStateChanged += OnPlaybackChanged;
        _focusHistoryService.HistoryAdded += OnFocusHistoryAdded;
    }

    /// <inheritdoc/>
    public int ValidateAndRetrieveStreak()
    {
        DateTime now = DateTime.Now;
        DateTime lastUpdated = StreakLastUpdated();

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

    /// <inheritdoc/>
    public async Task LogStreakAsync()
    {
        DateTime now = DateTime.Now;
        DateTime lastUpdated = StreakLastUpdated();

        if (lastUpdated.Date >= now.Date)
        {
            return;
        }

        int currentCount = ValidateAndRetrieveStreak();
        currentCount++;
        _userSettings.Set(UserSettingsConstants.ActiveStreakKey, currentCount);
        _userSettings.Set(UserSettingsConstants.ActiveStreakUpdateDateTicksKey, now.Ticks);
        await LogActivityAsync(now, currentCount);

        StreakChanged?.Invoke(this, new StreakChangedEventArgs
        {
            NewStreak = currentCount,
            AnimationRecommended = true,
        });
    }

    /// <inheritdoc/>
    public Task<StreakHistory> GetStreakHistory()
    {
        return _streakHistoryCache.GetStreakHistoryAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<bool>> GetRecentActiveHistory(int days)
    {
        if (days <= 0)
        {
            return [];
        }

        DateTime tempDate = DateTime.Now.AddDays((days - 1) * -1);
        StreakHistory history = await _streakHistoryCache.GetStreakHistoryAsync();
        List<bool> recentHistory = [];
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

    private async Task LogActivityAsync(DateTime date, int latestStreak)
    {
        StreakHistory streakHistory = await _streakHistoryCache.GetStreakHistoryAsync();
        string year = date.Year.ToString();
        string month = date.Month.ToString();

        if (!streakHistory.Years.ContainsKey(year))
        {
            streakHistory.Years.Add(year, []);
        }

        if (!streakHistory.Years[year].ContainsKey(month))
        {
            streakHistory.Years[year].Add(month, []);
        }

        _ = streakHistory.Years[year][month].Add(date.Day);

        if (latestStreak > streakHistory.LongestStreak)
        {
            streakHistory.LongestStreak = latestStreak;
        }

        await _streakHistoryCache.UpdateStreakHistory(streakHistory);
    }

    private async void OnPlaybackChanged(object sender, MediaPlaybackState e)
    {
        if (e is MediaPlaybackState.Opening or MediaPlaybackState.Playing)
        {
            await LogStreakAsync();
            _dayTimer.Start();
            _usageTimer.Start();
        }
        else
        {
            _dayTimer.Stop();
            _usageTimer.Stop();
        }
    }

    private async void OnDayElapsed(object sender, TimeSpan e)
    {
        await LogStreakAsync();
    }

    private DateTime StreakLastUpdated()
    {
        long lastUpdatedTicks = _userSettings.Get<long>(UserSettingsConstants.ActiveStreakUpdateDateTicksKey);
        return new DateTime(lastUpdatedTicks);
    }

    private async void OnUsageIntervalElapsed(object sender, TimeSpan e)
    {
        await LogUsageTimeAsync(minutes: _usageInterval.TotalMinutes, _mixMediaPlayerService.GetSoundIds());
    }

    private async Task LogUsageTimeAsync(double minutes, IReadOnlyList<string> soundIds)
    {
        StreakHistory streakHistory = await _streakHistoryCache.GetStreakHistoryAsync();

        List<(string, string)> soundList = [];
        foreach (var id in soundIds)
        {
            if (await _soundCache.GetInstalledSoundAsync(id) is Sound s)
            {
                soundList.Add((s.Id, _assetLocalizer.GetLocalName(s)));
            }
        }

        UpdateUsageTime(streakHistory, minutes, DateTime.Now, soundList);
        await _streakHistoryCache.UpdateStreakHistory(streakHistory);
    }

    private async void OnFocusHistoryAdded(object sender, FocusHistory? history)
    {
        await LogFocusUsageAsync(history).ConfigureAwait(false);
    }

    private async Task LogFocusUsageAsync(FocusHistory? history)
    {
        if (history is null)
        {
            return;
        }

        StreakHistory streakHistory = await _streakHistoryCache.GetStreakHistoryAsync().ConfigureAwait(false);
        UpdateFocusUsage(streakHistory, history);
        await _streakHistoryCache.UpdateStreakHistory(streakHistory).ConfigureAwait(false);
    }

    /// <summary>
    /// Updates the given streak history object with new usage minutes.
    /// </summary>
    /// <remarks>
    /// This public static class was exposed like this primarily to make unit testing a bit easier.
    /// This is not intended to be used in real scenarios outside of this class.
    /// Callers should only rely on the methods exposed via <see cref="IStatService"/>.
    /// </remarks>
    /// <param name="streakHistory">The object to update.</param>
    /// <param name="minutes">The new usage minutes.</param>
    /// <param name="currentDate">The given date when the usage was recorded.</param>
    public static void UpdateUsageTime(StreakHistory streakHistory, double minutes, DateTime currentDate, IReadOnlyList<(string Id, string Name)> sounds)
    {
        double hourValue = minutes / 60;
        streakHistory.TotalHours += hourValue;
        streakHistory.MonthlyHours[currentDate.Month - 1] += hourValue;
        streakHistory.WeeklyHours[(int)currentDate.DayOfWeek] += hourValue;

        foreach (var (Id, Name) in sounds)
        {
            if (streakHistory.SoundUsage.ContainsKey(Id))
            {
                streakHistory.SoundUsage[Id].TotalHours += hourValue;
            }
            else
            {
                SoundUsageHistory newEntry = new()
                {
                    Id = Id,
                    LocalizedName = Name,
                    TotalHours = hourValue
                };
                streakHistory.SoundUsage.Add(Id, newEntry);
            }
        }
    }

    /// <summary>
    /// Updates the given streak history object with new focus usage.
    /// </summary>
    /// <remarks>
    /// This public static class was exposed like this primarily to make unit testing a bit easier.
    /// This is not intended to be used in real scenarios outside of this class.
    /// Callers should only rely on the methods exposed via <see cref="IStatService"/>.
    /// </remarks>
    /// <param name="streakHistory">The object to update.</param>
    /// <param name="focusHistory">The new focus history object..</param>
    public static void UpdateFocusUsage(StreakHistory streakHistory, FocusHistory focusHistory)
    {
        streakHistory.TotalFocusHours += focusHistory.GetFocusTimeCompleted() / 60;
        streakHistory.TotalTasksCompleted += focusHistory.TasksCompleted;
    }
}

public class StreakChangedEventArgs : EventArgs
{
    public int NewStreak { get; init; }

    public bool AnimationRecommended { get; init; }
}
