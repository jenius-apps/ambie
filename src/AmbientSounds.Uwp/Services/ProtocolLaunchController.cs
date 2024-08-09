using Microsoft.QueryStringDotNET;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using JeniusApps.Common.Telemetry;
using Windows.System;

#nullable enable

namespace AmbientSounds.Services;

public class ProtocolLaunchController
{
    private readonly IMixMediaPlayerService _player;
    private readonly IShareService _shareService;
    private readonly ITelemetry _telemetry;
    private readonly INavigator _navigator;
    private readonly ISoundService _soundService;
    private readonly ISoundMixService _soundMixService;

    private const string AutoPlayKey = "autoPlay";

    public ProtocolLaunchController(
        IMixMediaPlayerService player,
        IShareService shareService,
        ITelemetry telemetry,
        INavigator navigator,
        ISoundService soundService,
        ISoundMixService soundMixService)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(navigator);
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(soundMixService);

        _player = player;
        _shareService = shareService;
        _telemetry = telemetry;
        _navigator = navigator;
        _soundService = soundService;
        _soundMixService = soundMixService;
    }

    public void ProcessShareProtocolArguments(string arguments)
    {
        var query = QueryString.Parse(arguments);

        if (query.TryGetValue("id", out var shareId))
        {
            _ =_shareService.ProcessShareRequestAsync(shareId);
        }
    }

    public async void ProcessAutoPlayProtocolArguments(string arguments)
    {
        bool minimize = false;
        string mixID = "";

        if (arguments.Contains("minimize"))
        {
            minimize = true;
        }

        if (arguments.Contains("mix"))
        {
            int mixStart = arguments.IndexOf("mix=") + 4;
            int mixEnd = arguments.IndexOf('&', mixStart);

            if (mixEnd == -1) // If there's no &, take the rest of the string (smart moment here)
            {
                mixID = arguments.Substring(mixStart);
            }
            else
            {
                mixID = arguments.Substring(mixStart, mixEnd - mixStart);
            }
            if (_player != null) {
                _player.SetMixId(mixID);
                _player.RemoveAll();
                await _player?.ToggleSoundsAsync(await _soundService.GetLocalSoundsAsync((await _soundService.GetLocalSoundAsync(mixID)).SoundIds), parentMixId: mixID);
            }
        }

        _player?.Play();

        if (minimize)
        {
            IList<AppDiagnosticInfo> infos = await AppDiagnosticInfo.RequestInfoForAppAsync();
            IList<AppResourceGroupInfo> resourceInfos = infos[0].GetResourceGroups();
            await resourceInfos[0].StartSuspendAsync();
        }
    }
}
