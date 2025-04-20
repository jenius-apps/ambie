using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Services;
using System;
using Xunit;

namespace AmbientSounds.Tests.Services;

public class StatServiceTests
{
    [Fact]
    public void UpdateUsageTimeTest()
    {
        // Arrange
        StreakHistory stats = new();
        double minutes = 15;
        DateTime testDate = new(2025, 4, 19);

        // Act
        StatService.UpdateUsageTime(stats, minutes, testDate);

        // Assert
        double hoursResult = minutes / 60;
        Assert.Equal(hoursResult, stats.TotalHours);
        Assert.Equal(hoursResult, stats.MonthlyHours[testDate.Month - 1]);
        Assert.Equal(hoursResult, stats.WeeklyHours[(int)testDate.DayOfWeek]);
    }

    [Fact]
    public void UpdateFocusUsageTest()
    {
        // Arrange
        StreakHistory stats = new();
        FocusHistory focusHistory = new()
        {
            FocusLength = 5,
            FocusSegmentsCompleted = 3,
            TasksCompleted = 2
        };

        // Act
        StatService.UpdateFocusUsage(stats, focusHistory);

        // Assert
        double hoursResult = focusHistory.GetFocusTimeCompleted() / 60;
        Assert.Equal(hoursResult, stats.TotalFocusHours);
        Assert.Equal(2, stats.TotalTasksCompleted);
    }
}
