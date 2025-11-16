using AmbientSounds.Cache;
using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

/// <summary>
/// ViewModel representing the catalogue page.
/// </summary>
public partial class CataloguePageViewModel : ObservableObject
{
    private readonly IPageCache _pageCache;
    private readonly ICatalogueRowVmFactory _vmFactory;
    private readonly IDialogService _dialogService;

    public CataloguePageViewModel(
        IPageCache pageCache,
        ICatalogueRowVmFactory catalogueRowVmFactory,
        IDialogService dialogService)
    {
        _pageCache = pageCache;
        _vmFactory = catalogueRowVmFactory;
        _dialogService = dialogService;
    }

    public ObservableCollection<CatalogueRowViewModel> Rows { get; } = new();

    [ObservableProperty]
    private bool _loading;

    public async Task InitializeAsync(string? launchArgs, CancellationToken ct)
    {
        try
        {
            Loading = true;
            ct.ThrowIfCancellationRequested();
            List<Task> tasks = new();
            await Task.Delay(150, CancellationToken.None); // added to improve nav perf
            ct.ThrowIfCancellationRequested();
            IReadOnlyList<Models.CatalogueRow> rows = await _pageCache.GetCatalogueRowsAsync();
            ct.ThrowIfCancellationRequested();
            foreach (Models.CatalogueRow row in rows)
            {
                ct.ThrowIfCancellationRequested();
                CatalogueRowViewModel vm = _vmFactory.Create(row);
                tasks.Add(vm.LoadAsync(launchArgs, ct));
                Rows.Add(vm);

                if (Loading)
                {
                    Loading = false;
                }
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            Loading = false;
        }
    }

    public void Uninitialize()
    {
        foreach (CatalogueRowViewModel row in Rows)
        {
            row.Uninitialize();
        }

        Rows.Clear();
    }

    [RelayCommand]
    private async Task OpenSoundDialogAsync(OnlineSoundViewModel? vm)
    {
        if (vm is null || vm.DownloadProgressVisible)
        {
            return;
        }

        bool proceed = await _dialogService.OpenSoundDialogAsync(vm);

        if (!proceed)
        {
            return;
        }

        if (vm.CanPlay)
        {
            _ = vm.PlayCommand.ExecuteAsync(null);
        }
        else if (vm.CanBuy)
        {
            await _dialogService.OpenPremiumAsync();
        }
        else if (vm.DownloadButtonVisible)
        {
            _ = vm.DownloadCommand.ExecuteAsync(null);
        }
    }
}
