using Microsoft.QueryStringDotNET;
using CommunityToolkit.Diagnostics;
using System;
using AmbientSounds.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

#nullable enable

namespace AmbientSounds.Services;

public class ProtocolLaunchController
{
    private readonly IMixMediaPlayerService _player;
    private readonly IShareService _shareService;
    private readonly ISoundService _soundService;

    private const string AutoPlayKey = "autoPlay";

    public ProtocolLaunchController(
        IMixMediaPlayerService player,
        IShareService shareService,
        ISoundService soundService)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(soundService);

        _player = player;
        _shareService = shareService;
        _soundService = soundService;
    }

    public void ProcessLaunchProtocolArguments(string arguments)
    {
        var query = QueryString.Parse(arguments);
        query.TryGetValue(AutoPlayKey, out var isAutoPlay);

        if (!string.IsNullOrEmpty(isAutoPlay) && Convert.ToBoolean(isAutoPlay))
        {
            // Auto play music.
            _player.Play();
        }
    }

    public async Task ProcessShareProtocolArgumentsAsync(string arguments)
    {
        var query = QueryString.Parse(arguments);

        if (query.TryGetValue("id", out var shareId))
        {
            IReadOnlyList<string> soundIds = await _shareService.GetSoundIdsAsync(shareId);
            if (soundIds.Count == 0)
            {
                return;
            }

            IReadOnlyList<Sound> sounds = await _soundService.GetLocalSoundsAsync(soundIds);
            if (sounds.Count != soundIds.Count)
            {
                // open missing sounds 
            }
            else
            {
                _player.RemoveAll();

                foreach (var s in sounds)
                {
                    await _player.ToggleSoundAsync(s);

                    // For some reason the third item on the list is always muted. Need to remove and re-add to get it to play.
                    // The delay below is a workaround that seems to let the third item play normally.
                    // A delay of 1ms doesn't work, so 300ms was chosen to allow for the workaround
                    // and to have a smooth-looking transition.
                    await Task.Delay(300);
                }
            }
        }
    }
}
