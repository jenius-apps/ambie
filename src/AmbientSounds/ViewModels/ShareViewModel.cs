using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class ShareViewModel : ObservableObject
{
    private readonly ISoundService _soundService;
    private readonly IShareService _shareService;
    private readonly string _baseShareUrl;

    [ObservableProperty]
    private string _shareText = string.Empty;

    [ObservableProperty]
    private string _shareUrl = string.Empty;

    public ShareViewModel(
        ISoundService soundService,
        IShareService shareService,
        IAppSettings appSettings)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(shareService);
        _soundService = soundService;
        _shareService = shareService;
        _baseShareUrl = appSettings.ShareUrl;
    }

    public async Task InitializeAsync(IReadOnlyList<string> soundIds)
    {
        var sounds = await _soundService.GetLocalSoundsAsync(soundIds);
        ShareText = string.Join(" • ", sounds.Select(x => x.Name));

        ShareDetail? shareDetail = await _shareService.GetShareDetailAsync(soundIds);
        if (shareDetail is { Id: string id })
        {
            ShareUrl = $"{_baseShareUrl}?id={id}";
        }
    }

    public void Uninitialize()
    {
        ShareText = string.Empty;
        ShareUrl = string.Empty;
    }
}
