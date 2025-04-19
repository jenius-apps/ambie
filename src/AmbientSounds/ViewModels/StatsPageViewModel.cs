using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class StatsPageViewModel : ObservableObject
{
    private const int LastDaysStreak = 7; // # of days to track streak
    private readonly IStatService _statService;
    private readonly ILocalizer _localizer;

    public StatsPageViewModel(
        IStatService statService,
        ILocalizer localizer)
    {
        _statService = statService;
        _localizer = localizer;
    }

    /// <summary>
    /// The current streak count to display on screen.
    /// </summary>
    [ObservableProperty]
    private int _streakCount;

    /// <summary>
    /// Text to display on screen describing the streak.
    /// </summary>
    [ObservableProperty]
    private string _streakText = string.Empty;

    /// <summary>
    /// Total hours of usage.
    /// </summary>
    [ObservableProperty]
    private double _totalHours;

    /// <summary>
    /// Total hours of usage this month.
    /// </summary>
    [ObservableProperty]
    private double _hoursThisMonth;

    /// <summary>
    /// Total hours of usage this week
    /// </summary>
    [ObservableProperty]
    private double _hoursThisWeek;

    /// <summary>
    /// List of recent streak activity to display on screen.
    /// </summary>
    public ObservableCollection<DayActivityViewModel> RecentActivity { get; } = [];

    /// <summary>
    /// Initializes this viewmodel.
    /// </summary>
    public async Task InitializeAsync()
    {
        LoadStreak();
        await LoadRecentActivityAsync();
        await LoadUsageStatsAsync();
    }

    private async Task LoadUsageStatsAsync()
    {
        DateTime now = DateTime.Now;
        StreakHistory history = await _statService.GetStreakHistory();
        TotalHours = history.TotalHours;
        HoursThisMonth = history.MonthlyHours[now.Month - 1];
        HoursThisWeek = history.WeeklyHours[(int)now.DayOfWeek];
    }

    private void LoadStreak(StreakChangedEventArgs? args = null)
    {
        int count = args?.NewStreak ?? _statService.ValidateAndRetrieveStreak();

        StreakText = count == 1
        ? _localizer.GetString("DaySingular")
            : _localizer.GetString("DayPlural", count.ToString());

        StreakCount = count;
    }

    private async Task LoadRecentActivityAsync()
    {
        IReadOnlyList<bool> recent = await _statService.GetRecentActiveHistory(LastDaysStreak);
        DateTime tempDate = DateTime.Now.AddDays((LastDaysStreak - 1) * -1).Date;
        RecentActivity.Clear();
        foreach (bool x in recent)
        {
            RecentActivity.Add(new DayActivityViewModel
            {
                Active = x,
                Date = tempDate
            });

            tempDate = tempDate.AddDays(1);
        }
    }
}
