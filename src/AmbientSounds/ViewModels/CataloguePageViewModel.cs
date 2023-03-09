using AmbientSounds.Cache;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Mvvm.ComponentModel;
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

    public CataloguePageViewModel(
        IPageCache pageCache,
        INavigator navigator)
    {
        _pageCache = pageCache;
        _navigator = navigator;
    }

    public ObservableCollection<CatalogueRow> Rows { get; } = new();

    public void GoBack() => _navigator.GoBack();

    public async Task InitializeAsync()
    {
        var rows = await _pageCache.GetCatalogueRowsAsync();
        foreach (var row in rows)
        {
            Rows.Add(row);
        }
    }

    public void Uninitialize()
    {
        Rows.Clear();
    }
}
