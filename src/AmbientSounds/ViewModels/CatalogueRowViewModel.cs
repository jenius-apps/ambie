using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class CatalogueRowViewModel : ObservableObject
{
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly IOnlineSoundDataProvider _dataProvider;
    private readonly ISoundVmFactory _soundVmFactory;

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

        Title = _assetLocalizer.GetLocalName(row);

        await Task.Delay(1);

        //IList<Sound> sounds;

        //try
        //{
        //    sounds = await _dataProvider.GetSoundsAsync();
        //}
        //catch 
        //{
        //    return;
        //}

        //List<Task> tasks = new(sounds.Count);
        //foreach (var sound in sounds)
        //{
        //    var vm = _soundVmFactory.GetOnlineSoundVm(sound);
        //    if (vm is not null)
        //    {
        //        tasks.Add(vm.LoadCommand.ExecuteAsync(null));
        //        Sounds.Add(vm);
        //    }
        //}

        //await Task.WhenAll(tasks);
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
