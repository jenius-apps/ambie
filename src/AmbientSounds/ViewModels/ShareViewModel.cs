using AmbientSounds.Constants;
using AmbientSounds.Services;
using AmbientSounds.Tools;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Tools;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ShareViewModel : ObservableObject
{
    private readonly ISoundService _soundService;
    private readonly IShareService _shareService;
    private readonly IClipboard _clipboard;
    private readonly IAssetLocalizer _assetLocalizer;

    [ObservableProperty]
    private string _shareText = string.Empty;

    [ObservableProperty]
    private string _shareUrl = string.Empty;

    [ObservableProperty]
    private bool _loading;

    [ObservableProperty]
    private bool _copySuccessful;

    public ShareViewModel(
        ISoundService soundService,
        IShareService shareService,
        IClipboard clipboard,
        IAssetLocalizer assetLocalizer)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(shareService);
        Guard.IsNotNull(clipboard);
        Guard.IsNotNull(assetLocalizer);

        _soundService = soundService;
        _shareService = shareService;
        _clipboard = clipboard;
        _assetLocalizer = assetLocalizer;
    }

    public async Task InitializeAsync(IReadOnlyList<string> soundIds)
    {
        Loading = true;
        var shareTask = _shareService.GetShareUrlAsync(soundIds);
        var sounds = await _soundService.GetLocalSoundsAsync(soundIds);
        ShareText = string.Join($" {FocusConstants.DotSeparator} ", sounds.Select(_assetLocalizer.GetLocalName));
        ShareUrl = await shareTask;
        Loading = false;
    }

    public void Uninitialize()
    {
        ShareText = string.Empty;
        ShareUrl = string.Empty;
        CopySuccessful = false;
    }

    [RelayCommand]
    private void Copy()
    {
        CopySuccessful = _clipboard.CopyToClipboard(ShareUrl);
    }
}
