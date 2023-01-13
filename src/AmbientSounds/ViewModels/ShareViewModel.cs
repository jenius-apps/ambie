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

    [ObservableProperty]
    private string _shareText = string.Empty;

    [ObservableProperty]
    private string _shareUrl = string.Empty;

    public ShareViewModel(ISoundService soundService)
    {
        Guard.IsNotNull(soundService);
        _soundService = soundService;
    }

    public async Task InitializeAsync(IReadOnlyList<string> soundIds)
    {
        var sounds = await _soundService.GetLocalSoundsAsync(soundIds);
        ShareText = string.Join(" • ", sounds.Select(x => x.Name));
    }

    public void Uninitialize()
    {
        ShareText = string.Empty;
        ShareUrl = string.Empty;
    }
}
