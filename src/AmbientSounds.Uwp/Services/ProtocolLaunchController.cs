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

    public void ProcessShareProtocolArguments(string arguments)
    {
        var query = QueryString.Parse(arguments);

        if (query.TryGetValue("id", out var shareId))
        {
            _ =_shareService.ProcessShareRequestAsync(shareId);
        }
    }
}
