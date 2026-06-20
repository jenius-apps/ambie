using AmbientSounds.Cache;
using AmbientSounds.Constants;
using AmbientSounds.Factories;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using JeniusApps.Common.Telemetry;
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
    private readonly ICategoryService _categoryService;
    private readonly ICategoryVmFactory _categoryVmFactory;
    private readonly ICatalogueService _catalogueService;
    private readonly ISoundVmFactory _soundVmFactory;
    private readonly ITelemetry _telemetry;
    private readonly SemaphoreSlim _filteredSoundsLock = new(1, 1);

    public CataloguePageViewModel(
        IPageCache pageCache,
        ICatalogueRowVmFactory catalogueRowVmFactory,
        IDialogService dialogService,
        ICategoryService categoryService,
        ICategoryVmFactory categoryVmFactory,
        ICatalogueService catalogueService,
        ISoundVmFactory soundVmFactory,
        ITelemetry telemetry)
    {
        _pageCache = pageCache;
        _vmFactory = catalogueRowVmFactory;
        _dialogService = dialogService;
        _categoryService = categoryService;
        _categoryVmFactory = categoryVmFactory;
        _catalogueService = catalogueService;
        _soundVmFactory = soundVmFactory;
        _telemetry = telemetry;
    }

    public ObservableCollection<CatalogueRowViewModel> Rows { get; } = [];

    public ObservableCollection<CategoryViewModel> CategoryFilters { get; } = [];

    public ObservableCollection<OnlineSoundViewModel> FilteredSounds { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FilteredListVisible))]
    private CategoryViewModel? _selectedFilter;

    [ObservableProperty]
    private bool _loading;

    public bool FilteredListVisible => SelectedFilter is not null;

    public async Task InitializeAsync(string? launchArgs, CancellationToken ct)
    {
        try
        {
            Loading = true;
            ct.ThrowIfCancellationRequested();
            List<Task> tasks = [];
            await Task.Delay(150, CancellationToken.None); // added to improve nav perf
            ct.ThrowIfCancellationRequested();
            IReadOnlyList<CatalogueRow> rows = await _pageCache.GetCatalogueRowsAsync();
            ct.ThrowIfCancellationRequested();
            foreach (CatalogueRow row in rows)
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

            IReadOnlyList<Category> categories = await _categoryService.GetCategoriesAsync([CategorySupportedPage.Catalogue], ct);
            foreach (Category category in categories)
            {
                CategoryFilters.Add(_categoryVmFactory.Create(category));
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
        CategoryFilters.Clear();
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

    [RelayCommand]
    private void ClearFilterSelection()
    {
        SelectedFilter = null;
        _telemetry.TrackEvent(TelemetryConstants.CatalogueFilterCleared);
    }

    async partial void OnSelectedFilterChanged(CategoryViewModel? oldValue, CategoryViewModel? newValue)
    {
        if (oldValue is { })
        {
            oldValue.IsSelected = false;
        }

        if (newValue is { })
        {
            newValue.IsSelected = true;
            await UpdateFilteredSoundsAsync(newValue);
            _telemetry.TrackEvent(TelemetryConstants.CatalogueFilterClicked, new Dictionary<string, string>
            {
                { "filter", newValue.Name }
            });
        }
    }

    private async Task UpdateFilteredSoundsAsync(CategoryViewModel categoryVm)
    {
        await _filteredSoundsLock.WaitAsync();
        FilteredSounds.Clear();
        IReadOnlyList<Sound> newSounds = await _catalogueService.GetSoundsAsync(categoryVm.Model.Id);
        List<Task> tasks = new(newSounds.Count);
        foreach (Sound sound in newSounds)
        {
            OnlineSoundViewModel? soundVm = _soundVmFactory.GetOnlineSoundVm(sound);
            if (soundVm is not null)
            {
                tasks.Add(soundVm.LoadCommand.ExecuteAsync(null));
                FilteredSounds.Add(soundVm);
            }
        }

        await Task.WhenAll(tasks);
        _ = _filteredSoundsLock.Release();
    }
}
