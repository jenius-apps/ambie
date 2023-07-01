using System;

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
    private readonly IMixMediaPlayerService _mixMediaPlayerService;

    public PlaybackModeObserver(
        IFocusService focusService,
        IGuideService guideService,
        IMixMediaPlayerService mixMediaPlayerService)
    {
        _focusService = focusService;
        _guideService = guideService;
        _mixMediaPlayerService = mixMediaPlayerService;

        _focusService.FocusStateChanged += OnFocusStateChanged;
        _mixMediaPlayerService.GuidePositionChanged += OnGuidePositionChanged;
    }

    private void OnGuidePositionChanged(object sender, TimeSpan e)
    {
        if (e > TimeSpan.MinValue &&
            _focusService.CurrentState == FocusState.Active && 
            _mixMediaPlayerService.CurrentGuideId is { Length: > 0 })
        {
            _focusService.StopTimer(pauseSounds: false);
        }
    }

    private void OnFocusStateChanged(object sender, FocusState e)
    {
        if (e == FocusState.Active && _mixMediaPlayerService.CurrentGuideId is { Length: > 0 } guideId)
        {
            _guideService.Stop(guideId);
        }
    }
}
