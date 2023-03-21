using AmbientSounds.Cache;
using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class MeditatePageViewModel : ObservableObject
{
    private readonly IPageCache _pageCache;
    private readonly IGuideService _guideService;
    private readonly ISoundVmFactory _soundVmFactory;

    public MeditatePageViewModel(
        IPageCache pageCache,
        IGuideService guideService,
        ISoundVmFactory soundVmFactory)
    {
        _pageCache = pageCache;
        _guideService = guideService;
        _soundVmFactory = soundVmFactory;
    }

    public ObservableCollection<OnlineSoundViewModel> Guides { get; } = new();

    public async Task InitializeAsync()
    {
        var guideIds = await _pageCache.GetGuidePageAsync();
        var guides = await _guideService.GetGuidesAsync(guideIds);
        foreach (var s in guides)
        {
            var vm = _soundVmFactory.GetOnlineSoundVm(s);
            // TODO initialize vm
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        Guides.Clear();
    }
}
