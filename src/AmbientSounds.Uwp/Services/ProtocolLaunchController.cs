using Microsoft.QueryStringDotNET;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using AmbientSounds.Constants;
using JeniusApps.Common.Telemetry;

#nullable enable

namespace AmbientSounds.Services;

public class ProtocolLaunchController
{
    private readonly IMixMediaPlayerService _player;
    private readonly IShareService _shareService;
    private readonly ITelemetry _telemetry;

    private const string AutoPlayKey = "autoPlay";

    public ProtocolLaunchController(
        IMixMediaPlayerService player,
        IShareService shareService,
        ITelemetry telemetry)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(telemetry);

        _player = player;
        _shareService = shareService;
        _telemetry = telemetry;
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
