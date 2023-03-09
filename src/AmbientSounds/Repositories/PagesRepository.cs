using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class PagesRepository : IPagesRepository
{
    private readonly string _pagesUrl;
    private readonly HttpClient _client;

    public PagesRepository(
        HttpClient httpClient,
        IAppSettings appSettings)
    {
        Guard.IsNotNull(httpClient);
        Guard.IsNotNull(appSettings);
        _client = httpClient;
        _pagesUrl = appSettings.PagesUrl;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<CatalogueRow>> GetCataloguePageAsync()
    {
        try
        {
            string url = _pagesUrl + "/catalogue";
            using Stream result = await _client.GetStreamAsync(url);
            var results = await JsonSerializer.DeserializeAsync(
                result,
                AmbieJsonSerializerContext.CaseInsensitive.CatalogueRowArray);
            return results ?? Array.Empty<CatalogueRow>();
        }
        catch
        {
            return Array.Empty<CatalogueRow>();
        }
    }
}
