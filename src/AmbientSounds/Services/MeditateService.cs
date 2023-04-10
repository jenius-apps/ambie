using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class MeditateService : IMeditateService
{
    private readonly IMixMediaPlayerService _mixMediaPlayerService;
    private FocusState _meditationState = FocusState.None;

    public event EventHandler<FocusState>? MeditationStateChanged;

    public MeditateService(
        IMixMediaPlayerService mixMediaPlayerService)
    {
        _mixMediaPlayerService = mixMediaPlayerService;
    }

    /// <inheritdoc/>
    public async Task PlayAsync(Guide guide)
    {
        await _mixMediaPlayerService.ToggleSoundAsync(guide, isLeadingSound: true);
    }
}
