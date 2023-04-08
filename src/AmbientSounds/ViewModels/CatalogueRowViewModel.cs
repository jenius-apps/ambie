using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

public partial class CatalogueRowViewModel : ObservableObject
{
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly ICatalogueService _dataProvider;
    private readonly ISoundVmFactory _soundVmFactory;
    private readonly CatalogueRow _row;

    public CatalogueRowViewModel(
        CatalogueRow row,
        IAssetLocalizer assetLocalizer,
        ICatalogueService dataProvider,
        ISoundVmFactory soundVmFactory)
    {
        Guard.IsNotNull(row);
        Guard.IsNotNull(assetLocalizer);
        Guard.IsNotNull(dataProvider);
        Guard.IsNotNull(soundVmFactory);

        _row = row;
        _dataProvider = dataProvider;
        _soundVmFactory = soundVmFactory;
        _assetLocalizer = assetLocalizer;
    }

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _rowVisible;

    [ObservableProperty]
    private bool _newAnimationVisible;

    public ObservableCollection<OnlineSoundViewModel> Sounds { get; } = new();

    public async Task LoadAsync(string? launchArgs, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        RowVisible = false;

        Title = _assetLocalizer.GetLocalName(_row);
        IReadOnlyList<Sound>? sounds = null;

        try
        {
            sounds = await _dataProvider.GetSoundsAsync(_row.SoundIds);
        }
        catch { }

        ct.ThrowIfCancellationRequested();
        if (sounds is not null)
        {
            Sounds.Clear();
            List<Task> tasks = new(sounds.Count);
            foreach (var sound in sounds)
            {
                ct.ThrowIfCancellationRequested();
                var vm = _soundVmFactory.GetOnlineSoundVm(sound);
                if (vm is not null)
                {
                    tasks.Add(vm.LoadCommand.ExecuteAsync(null));
                    Sounds.Add(vm);
                    RowVisible = true;
                }
            }

            await Task.WhenAll(tasks);
        }

        HandleLaunchArgs(launchArgs);
    }

    private void HandleLaunchArgs(string? launchArgs)
    {
        if (_row.Name.ToLower() == "new" &&
            launchArgs == LaunchConstants.NewSoundArgument)
        {
            NewAnimationVisible = true;
        }
    }

    public void Uninitialize()
    {
        RowVisible = false;

        foreach (var s in Sounds)
        {
            s.Dispose();
        }

        Sounds.Clear();
    }
}
