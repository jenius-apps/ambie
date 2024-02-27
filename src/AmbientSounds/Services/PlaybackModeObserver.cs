namespace AmbientSounds.Services;

/// <summary>
/// Responsible for watching when user enables
/// Ambie's various modes and ensuring only one
/// mode is active at a time.
/// </summary>
public class PlaybackModeObserver
{
    private readonly IFocusService _focusService;
    private readonly IGuideService _guideService;
    private readonly ISleepTimerService _sleepTimerService;

    public PlaybackModeObserver(
        IFocusService focusService,
        IGuideService guideService,
        ISleepTimerService sleepTimerService)
    {
        _focusService = focusService;
        _guideService = guideService;
        _sleepTimerService = sleepTimerService;

        _focusService.FocusStateChanged += OnFocusStateChanged;
        _guideService.GuideStarted += OnGuideStarted;
        _sleepTimerService.StateChanged += OnSleepTimerStateChanged;
    }

    private void OnSleepTimerStateChanged(object sender, SleepTimerState e)
    {
        if (e is SleepTimerState.Running)
        {
            _focusService.StopTimer(pauseSounds: false);
        }
    }

    private void OnGuideStarted(object sender, string e)
    {
        if (_focusService.CurrentState == FocusState.Active)
        {
            _focusService.StopTimer(pauseSounds: false);
        }
    }

    private void OnFocusStateChanged(object sender, FocusState e)
    {
        if (e == FocusState.Active)
        {
            _guideService.Stop();
            _sleepTimerService.StopTimer();
        }
    }
}
