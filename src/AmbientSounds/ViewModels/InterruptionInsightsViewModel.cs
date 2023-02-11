using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public class InterruptionInsightsViewModel : ObservableObject
{
    private readonly IFocusHistoryService _focusHistoryService;

    public InterruptionInsightsViewModel(IFocusHistoryService focusHistoryService)
    {
        Guard.IsNotNull(focusHistoryService);

        _focusHistoryService = focusHistoryService;
    }

    public ObservableCollection<InterruptionViewModel> Interruptions { get; } = new();

    public async Task InitializeAsync()
    {
        Interruptions.Clear();

        var interruptions = await _focusHistoryService.GetRecentInterruptionsAsync();
        foreach (var i in interruptions)
        {
            Interruptions.Add(new InterruptionViewModel(i));
        }
    }

    public void Uninitialize()
    {
        Interruptions.Clear();
    }
}
