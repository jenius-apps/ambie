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

    public int GetActiveStreak()
    {
        return _userSettings.Get<int>(UserSettingsConstants.ActiveStreakKey);
    }

    public async Task LogStreakAsync()
    {
        await Task.Delay(1);
        var now = DateTime.Now;
        var lastUpdatedTicks = _userSettings.Get<long>(UserSettingsConstants.ActiveStreakUpdateDateTicksKey);
        var lastUpdated = new DateTime(lastUpdatedTicks);

        if (lastUpdated.Date >= now.Date)
        {
            return;
        }

        var currentCount = _userSettings.Get<int>(UserSettingsConstants.ActiveStreakKey);
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
}

public class StreakChangedEventArgs : EventArgs
{
    public int NewStreak { get; init; }

    public bool AnimationRecommended { get; init; }
}
