using AmbientSounds.Cache;
using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
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

    public ObservableCollection<OnlineGuideViewModel> Guides { get; } = new();

    public async Task InitializeAsync()
    {
        var guideIds = await _pageCache.GetGuidePageAsync();
        var guides = await _guideService.GetGuidesAsync(guideIds);
        foreach (var s in guides)
        {
            var vm = _soundVmFactory.GetOnlineGuideVm(s);
            await vm.LoadCommand.ExecuteAsync(null);
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        foreach (var g in Guides)
        {
            g.Dispose();
        }

        Guides.Clear();
    }
}
