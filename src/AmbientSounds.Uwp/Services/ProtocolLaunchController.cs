using Microsoft.QueryStringDotNET;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using AmbientSounds.Constants;
using JeniusApps.Common.Telemetry;
using Windows.ApplicationModel.DataTransfer;
using Windows.System;
using JeniusApps.Common.Tools.Uwp;
using Microsoft.Extensions.DependencyInjection;

#nullable enable

namespace AmbientSounds.Services;

public class ProtocolLaunchController
{
    private readonly IMixMediaPlayerService _player;
    private readonly IShareService _shareService;
    private readonly ITelemetry _telemetry;
    private readonly INavigator _navigator;

    private const string AutoPlayKey = "autoPlay";

    public ProtocolLaunchController(
        IMixMediaPlayerService player,
        IShareService shareService,
        ITelemetry telemetry,
        INavigator navigator)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(telemetry);
        Guard.IsNotNull(navigator);

        _player = player;
        _shareService = shareService;
        _telemetry = telemetry;
        _navigator = navigator;
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
        
        if (arguments.Contains("minimize"))
        {
            minimize = true;
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
