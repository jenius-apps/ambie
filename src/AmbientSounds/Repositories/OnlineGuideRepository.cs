using AmbientSounds.Models;
using AmbientSounds.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class OnlineGuideRepository : IOnlineGuideRepository
{
    private readonly string _guidesUrl;
    private readonly HttpClient _client;

    public OnlineGuideRepository(
        HttpClient httpClient,
        IAppSettings appSettings)
    {
        _client = httpClient;
        _guidesUrl = appSettings.GuidesUrl;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Guide>> GetGuidesAsync(string culture)
    {
        try
        {
            culture = string.IsNullOrEmpty(culture) ? "en-us" : culture;
            var url = _guidesUrl + $"?culture={culture}";
            using Stream result = await _client.GetStreamAsync(url);
            var results = await JsonSerializer.DeserializeAsync(
                result,
                AmbieJsonSerializerContext.CaseInsensitive.GuideArray);
            return results ?? Array.Empty<Guide>();
        }
        catch
        {
            return Array.Empty<Guide>();
        }
    }

    /// <inheritdoc/>
    public async Task<string> GetDownloadUrlAsync(string guideId)
    {
        try
        {
            var result = await _client.GetAsync($"{_guidesUrl}/{guideId}?withDownloadUrl=true");
            if (result.IsSuccessStatusCode)
            {
                var json = await result.Content.ReadAsStringAsync();
                var guide = JsonSerializer.Deserialize(json, AmbieJsonSerializerContext.CaseInsensitive.Guide);
                return guide?.DownloadUrl ?? string.Empty;
            }
        }
        catch { }

        return string.Empty;
    }
}
