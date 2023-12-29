using AmbientSounds.Constants;
using System;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public sealed class StatService : IStatService
{
    private readonly IUserSettings _userSettings;
    private readonly IMixMediaPlayerService _mixMediaPlayerService;

    public event EventHandler<StreakChangedEventArgs>? StreakChanged;

    public StatService(
        IUserSettings userSettings,
        IMixMediaPlayerService mixMediaPlayerService)
    {
        _userSettings = userSettings;
        _mixMediaPlayerService = mixMediaPlayerService;

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

        await Task.Delay(1);
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

        StreakChanged?.Invoke(this, new StreakChangedEventArgs
        {
            NewStreak = currentCount
        });
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
