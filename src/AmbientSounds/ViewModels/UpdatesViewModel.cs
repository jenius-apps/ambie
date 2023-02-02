using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class UpdatesViewModel : ObservableObject
{
    private readonly IUpdateService _updateService;
    private readonly ISoundVmFactory _soundVmFactory;

    public UpdatesViewModel(
        IUpdateService updateService,
        ISoundVmFactory soundVmFactory,
        IOnlineSoundDataProvider onlineSoundDataProvider)
    {
        Guard.IsNotNull(updateService);
        Guard.IsNotNull(soundVmFactory);
        Guard.IsNotNull(onlineSoundDataProvider);

        _updateService = updateService;
        _soundVmFactory = soundVmFactory;
    }

    public ObservableCollection<UpdateSoundViewModel> UpdateList { get; } = new();

    [ObservableProperty]
    private bool _updateAllVisible;

    [RelayCommand]
    private async Task CheckUpdatesAsync()
    {
        UpdateAllVisible = false;
        UpdateList.Clear();
        var availableUpdates = await _updateService.CheckForUpdatesAsync();
        foreach (var s in availableUpdates)
        {
            var vm = _soundVmFactory.GetUpdateViewModel(s);
            UpdateList.Add(vm);
        }
        UpdateAllVisible = UpdateList.Count > 0;
    }

    public void Uninitialize()
    {
        UpdateList.Clear();
        UpdateAllVisible = UpdateList.Count > 0;
    }
}
