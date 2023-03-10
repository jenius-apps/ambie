using AmbientSounds.Cache;
using AmbientSounds.Factories;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace AmbientSounds.ViewModels;

/// <summary>
/// ViewModel representing the catalogue page.
/// </summary>
public class CataloguePageViewModel : ObservableObject
{
    private readonly INavigator _navigator;
    private readonly IPageCache _pageCache;
    private readonly CatalogueRowVmFactory _vmFactory;

    public CataloguePageViewModel(
        IPageCache pageCache,
        INavigator navigator,
        CatalogueRowVmFactory catalogueRowVmFactory)
    {
        _pageCache = pageCache;
        _navigator = navigator;
        _vmFactory = catalogueRowVmFactory;
    }

    public ObservableCollection<CatalogueRowViewModel> Rows { get; } = new();

    public void GoBack() => _navigator.GoBack();

    public async Task InitializeAsync()
    {
        List<Task> tasks = new();
        var rows = await _pageCache.GetCatalogueRowsAsync();
        foreach (var row in rows)
        {
            CatalogueRowViewModel vm = _vmFactory.Create(row);
            tasks.Add(vm.LoadAsync());
            Rows.Add(vm);
        }
        await Task.WhenAll(tasks);
    }

    public void Uninitialize()
    {
        foreach (var row in Rows)
        {
            row.Uninitialize();
        }

        Rows.Clear();
    }
}
