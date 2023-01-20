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

    [ObservableProperty]
    private string _shareText = string.Empty;

    [ObservableProperty]
    private string _shareUrl = string.Empty;

    public ShareViewModel(
        ISoundService soundService,
        IShareService shareService)
    {
        Guard.IsNotNull(soundService);
        Guard.IsNotNull(shareService);
        _soundService = soundService;
        _shareService = shareService;
    }

    public async Task InitializeAsync(IReadOnlyList<string> soundIds)
    {
        var shareTask = _shareService.GetShareUrlAsync(soundIds);
        var sounds = await _soundService.GetLocalSoundsAsync(soundIds);
        ShareText = string.Join(" • ", sounds.Select(x => x.Name));
        ShareUrl = await shareTask;
    }

    public void Uninitialize()
    {
        ShareText = string.Empty;
        ShareUrl = string.Empty;
    }
}
