using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class CatalogueRowViewModel : ObservableObject
{
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IOnlineSoundDataProvider _dataProvider;
    private readonly ISoundVmFactory _soundVmFactory;
    private bool _loading;

    public CatalogueRowViewModel(
        IAssetLocalizer assetLocalizer,
        IOnlineSoundDataProvider dataProvider,
        ISoundVmFactory soundVmFactory)
    {
        Guard.IsNotNull(assetLocalizer);
        Guard.IsNotNull(dataProvider);
        Guard.IsNotNull(soundVmFactory);

        _dataProvider = dataProvider;
        _soundVmFactory = soundVmFactory;
        _assetLocalizer = assetLocalizer;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

    public async Task LoadAsync(CatalogueRow row)
    {
        Guard.IsNotNull(row);

        if (_loading)
        {
            return;
        }

        _loading = true;
        Title = _assetLocalizer.GetLocalName(row);
        IList<Sound>? sounds = null;

        try
        {
            sounds = await _dataProvider.GetSoundsAsync(row.SoundIds.ToList());
        }
        catch { }

        if (sounds is not null)
        {
            List<Task> tasks = new(sounds.Count);
            foreach (var sound in sounds)
            {
                var vm = _soundVmFactory.GetOnlineSoundVm(sound);
                if (vm is not null)
                {
                    tasks.Add(vm.LoadCommand.ExecuteAsync(null));
                    Sounds.Add(vm);
                }
            }

            await Task.WhenAll(tasks);
        }

        _loading = false;
    }

    public void Uninitialize()
    {
        foreach (var s in Sounds)
        {
            s.Dispose();
        }

        Sounds.Clear();
    }
}
