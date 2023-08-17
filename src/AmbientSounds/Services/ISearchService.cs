using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public interface ISearchService
{
    /// <summary>
    /// Raised when a search is requested while the
    /// current content page is already on the search page.
    /// </summary>
    event EventHandler<string>? ModifyCurrentSearchRequested;

    /// <summary>
    /// Returns list of sounds that contain the given name string.
    /// </summary>
    /// <param name="name">The name to query.</param>
    /// <returns>List of sounds that contain the query.</returns>
    Task<IReadOnlyList<Sound>> SearchByNameAsync(string name);

    /// <summary>
    /// Performs page navigation if required and performs the search.
    /// </summary>
    /// <param name="name">The name to query.</param>
    void TriggerSearch(string name);
}