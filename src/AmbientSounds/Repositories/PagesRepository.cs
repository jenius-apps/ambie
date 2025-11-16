using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
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
        string url = _pagesUrl + "/catalogue";
        return await GetCatalogueRowsAsync(url);
    }

    public async Task<IReadOnlyList<CatalogueRow>> GetMeditatePageAsync(CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        string url = _pagesUrl + "/meditate";
        return await GetCatalogueRowsAsync(url);
    }

    private async Task<IReadOnlyList<CatalogueRow>> GetCatalogueRowsAsync(string url, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            using Stream result = await _client.GetStreamAsync(url);
            CatalogueRow[]? results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.CatalogueRowArray, ct);
            return results ?? [];
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch
        {
            return [];
        }
    }
}
