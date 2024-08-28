using CommunityToolkit.Diagnostics;
using Microsoft.QueryStringDotNET;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.System;

#nullable enable

namespace AmbientSounds.Services;

public class ProtocolLaunchController
{
    private readonly IMixMediaPlayerService _player;
    private readonly IShareService _shareService;

    public ProtocolLaunchController(
        IMixMediaPlayerService player,
        IShareService shareService)
    {
        Guard.IsNotNull(player);
        Guard.IsNotNull(shareService);

        _player = player;
        _shareService = shareService;
    }

    public void ProcessShareProtocolArguments(string arguments)
    {
        var query = QueryString.Parse(arguments);

        if (query.TryGetValue("id", out var shareId))
        {
            _ =_shareService.ProcessShareRequestAsync(shareId);
        }
    }

    public async Task ProcessAutoPlayProtocolArgumentsAsync(string arguments)
    {
        _player.Play();

        if (arguments.Contains("minimize", StringComparison.OrdinalIgnoreCase))
        {
            IList<AppDiagnosticInfo> infos = await AppDiagnosticInfo.RequestInfoForAppAsync();
            IList<AppResourceGroupInfo> resourceInfos = infos[0].GetResourceGroups();
            await resourceInfos[0].StartSuspendAsync();
        }
    }
}
