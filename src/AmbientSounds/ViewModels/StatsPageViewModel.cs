using AmbientSounds.Constants;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Humanizer;
using Humanizer.Localisation;
using JeniusApps.Common.Settings;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class StatsPageViewModel : ObservableObject
{
    private const double DefaultMaxSoundUsage = 100; // Just used to ensure we don't divide by zero anywhere
    private const int LastDaysStreak = 7; // # of days to track streak
    private readonly IStatService _statService;
    private readonly ILocalizer _localizer;
    private readonly IUserSettings _userSettings;
    private readonly IQuickResumeService _quickResumeService;

    public StatsPageViewModel(
        IStatService statService,
        ILocalizer localizer,
        IUserSettings userSettings,
        IQuickResumeService quickResumeService)
    {
        _statService = statService;
        _localizer = localizer;
        _userSettings = userSettings;
        _quickResumeService = quickResumeService;

        InitializeUserSettings();
    }

    /// <summary>
    /// Determines if quick resume is enabled.
    /// </summary>
    [ObservableProperty]
    private bool _isQuickResumeEnabled;

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
    /// Total hours of usage the past 7 days.
    /// </summary>
    [ObservableProperty]
    private double _hoursThisWeek;

    /// <summary>
    /// Total hours of focus usage.
    /// </summary>
    [ObservableProperty]
    private double _totalFocusHours;

    /// <summary>
    /// Total tasks completed during a focus session.
    /// </summary>
    [ObservableProperty]
    private int _tasksCompleted;

    /// <summary>
    /// Longest consecutive days of listening to sounds.
    /// </summary>
    [ObservableProperty]
    private int _longestStreak;

    /// <summary>
    /// List of recent streak activity to display on screen.
    /// </summary>
    public ObservableCollection<DayActivityViewModel> RecentActivity { get; } = [];

    /// <summary>
    /// List of sound usage.
    /// </summary>
    public ObservableCollection<SoundUsageHistoryViewModel> SoundUsage { get; } = [];

    /// <summary>
    /// Determines if the top played sounds placeholder should be visible.
    /// </summary>
    [ObservableProperty]
    private bool _topPlayedSoundsPlaceholderVisible;

    /// <summary>
    /// Initializes this viewmodel.
    /// </summary>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        LoadStreak();
        await LoadRecentActivityAsync();
        await LoadUsageStatsAsync(cancellationToken);
    }

    [RelayCommand]
    private async Task EnableQuickResumeAsync()
    {
        if (IsQuickResumeEnabled || _userSettings.Get<bool>(UserSettingsConstants.QuickResumeKey))
        {
            // If quick resume already enabled, fast return.
            return;
        }

        bool successful = await _quickResumeService.TryEnableAsync();
        await Task.Delay(300); // delay is to improve UX
        _userSettings.Set(UserSettingsConstants.QuickResumeKey, successful);
        IsQuickResumeEnabled = successful;
    }

    private void InitializeUserSettings()
    {
        IsQuickResumeEnabled = _userSettings.Get<bool>(UserSettingsConstants.QuickResumeKey);
    }

    private async Task LoadUsageStatsAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        DateTime now = DateTime.Now;
        StreakHistory history = await _statService.GetStreakHistory();
        TotalHours = Math.Round(history.TotalHours, 1);
        HoursThisMonth = Math.Round(history.MonthlyHours[now.Month - 1], 1);
        HoursThisWeek = Math.Round(history.WeeklyHours.Sum(), 1);
        TasksCompleted = history.TotalTasksCompleted;
        TotalFocusHours = Math.Round(history.TotalFocusHours, 1);
        LongestStreak = history.LongestStreak;

        double maxUsage = history.SoundUsage.Count > 0
            ? history.SoundUsage.Values.Max(x => x.TotalHours)
            : DefaultMaxSoundUsage;

        // For aesthetic purposes, we set the max to a little bit higher
        // than the real max so the top sound is only roughly 90% filled.
        maxUsage *= 1.05;

        foreach (SoundUsageHistory soundUsage in history.SoundUsage.Values.OrderByDescending(x => x.TotalHours))
        {
            cancellationToken.ThrowIfCancellationRequested();

            SoundUsage.Add(new SoundUsageHistoryViewModel(soundUsage, maxUsage));
        }

        TopPlayedSoundsPlaceholderVisible = SoundUsage.Count == 0;
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

public sealed class SoundUsageHistoryViewModel
{
    public SoundUsageHistoryViewModel(SoundUsageHistory usage, double max)
    {
        UsageValue = usage.TotalHours;
        MaxValue = max;
        UsageTime = TimeSpan.FromHours(usage.TotalHours).Humanize(minUnit: TimeUnit.Minute);
        Name = usage.LocalizedName;
    }

    public double MaxValue { get; }

    public double UsageValue { get; }

    public string UsageTime { get; }

    public string Name { get; }
}
