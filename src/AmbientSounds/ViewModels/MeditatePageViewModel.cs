using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public class MeditatePageViewModel : ObservableObject
{
    private readonly IGuideService _guideService;
    private readonly IGuideVmFactory _guideVmFactory;

    public MeditatePageViewModel(
        IGuideService guideService,
        IGuideVmFactory guideVmFactory)
    {
        _guideService = guideService;
        _guideVmFactory = guideVmFactory;
    }

    public ObservableCollection<GuideViewModel> Guides { get; } = new();

    public async Task InitializeAsync()
    {
        if (Guides.Count > 0)
        {
            return;
        }

        var guides = await _guideService.GetGuidesAsync(culture: "en"); // TODO support other languages
        foreach (var guide in guides)
        {
            var vm = _guideVmFactory.GetOrCreate(guide);
            Guides.Add(vm);
        }
    }

    public void Uninitialize()
    {
        Guides.Clear();
    }
}
