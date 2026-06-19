using AmbientSounds.Cache;
using AmbientSounds.Factories;
using AmbientSounds.Models;
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
    private readonly ICategoryService _categoryService;
    private readonly ICategoryVmFactory _categoryVmFactory;

    public CataloguePageViewModel(
        IPageCache pageCache,
        ICatalogueRowVmFactory catalogueRowVmFactory,
        IDialogService dialogService,
        ICategoryService categoryService,
        ICategoryVmFactory categoryVmFactory)
    {
        _pageCache = pageCache;
        _vmFactory = catalogueRowVmFactory;
        _dialogService = dialogService;
        _categoryService = categoryService;
        _categoryVmFactory = categoryVmFactory;
    }

    public ObservableCollection<CatalogueRowViewModel> Rows { get; } = [];

    public ObservableCollection<CategoryViewModel> CategoryFilters { get; } = [];

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

            IReadOnlyList<Category> categories = await _categoryService.GetCategoriesAsync(ct);
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
    }

    partial void OnSelectedFilterChanged(CategoryViewModel? oldValue, CategoryViewModel? newValue)
    {
        if (oldValue is { })
        {
            oldValue.IsSelected = false;
        }

        if (newValue is { })
        {
            newValue.IsSelected = true;
        }
    }
}
