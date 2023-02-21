using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
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
    private readonly ICatalogueService _catalogueService;

    public CataloguePageViewModel(
        INavigator navigator,
        ICatalogueService catalogueService)
    {
        Guard.IsNotNull(navigator);
        Guard.IsNotNull(catalogueService);

        _navigator = navigator;
        _catalogueService = catalogueService;
    }

    public ObservableCollection<CatalogueRow> Rows { get; } = new();

    public void GoBack() => _navigator.GoBack();

    public async Task InitializeAsync()
    {
        var rows = await _catalogueService.GetCatalogueRowsAsync();
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
