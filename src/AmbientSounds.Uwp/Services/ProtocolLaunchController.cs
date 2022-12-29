using Microsoft.QueryStringDotNET;
using CommunityToolkit.Diagnostics;
using System;

#nullable enable

namespace AmbientSounds.Services;

public class ProtocolLaunchController
{
    private readonly IMixMediaPlayerService _player;

    private const string AutoPlayKey = "autoPlay";

    public ProtocolLaunchController(IMixMediaPlayerService player)
    {
        Guard.IsNotNull(player);

        _player = player;
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
}
