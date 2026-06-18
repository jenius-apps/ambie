using AmbientSounds.Models;
using AmbientSounds.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly HttpClient _httpClient;
    private readonly string _categoryUrl;

    public CategoryRepository(HttpClient httpClient, IAppSettings appSettings)
    {
        _httpClient = httpClient;
        _categoryUrl = appSettings.CategoryUrl;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Category>> GetItemsAsync(CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        try
        {
            using Stream result = await _httpClient.GetStreamAsync(_categoryUrl);
            Category[]? results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.CategoryArray, ct);
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
