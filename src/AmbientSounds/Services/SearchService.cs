using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class SearchService : ISearchService
{
    private readonly ICatalogueService _soundService;
    private readonly IAssetLocalizer _assetLocalizer;
    private readonly INavigator _navigator;

    /// <inheritdoc/>
    public event EventHandler<string>? ModifyCurrentSearchRequested;

    public SearchService(
        ICatalogueService soundService,
        IAssetLocalizer assetLocalizer,
        INavigator navigator)
    {
        _soundService = soundService;
        _assetLocalizer = assetLocalizer;
        _navigator = navigator;
    }

    public string CurrentQuery { get; private set; } = string.Empty;

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> SearchByNameAsync(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return Array.Empty<Sound>();
        }

        var sounds = await _soundService.GetSoundsAsync();
        return sounds.Where(x => _assetLocalizer.LocalNameContains(x, name)).ToArray();
    }

    /// <inheritdoc/>
    public void TriggerSearch(string name)
    {
        name = name.Trim();

        if (_navigator.GetContentPageName() is "SearchPage")
        {
            if (!name.Equals(CurrentQuery, StringComparison.OrdinalIgnoreCase))
            {
                // Only fire the signal if the new query is different from the current query.
                ModifyCurrentSearchRequested?.Invoke(this, name);
                CurrentQuery = name;
            }
        }
        else
        {
            _navigator.NavigateTo(ContentPageType.Search, name);
            CurrentQuery = name;
        }
    }
}
