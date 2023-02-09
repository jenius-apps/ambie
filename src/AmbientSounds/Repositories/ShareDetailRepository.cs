using AmbientSounds.Extensions;
using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class ShareDetailRepository : IShareDetailRepository
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    public ShareDetailRepository(
        HttpClient httpClient,
        IAppSettings appSettings)
    {
        Guard.IsNotNull(httpClient);
        Guard.IsNotNull(appSettings.ShareUrl);

        _client = httpClient;
        _baseUrl = appSettings.ShareUrl;
    }

    /// <inheritdoc/>
    public async Task<ShareDetail?> GetShareDetailAsync(IReadOnlyList<string> soundIds)
    {
        if (soundIds.Count == 0)
        {
            return null;
        }

        string key = soundIds.SortAndCompose();

        var url = $"{_baseUrl}/detail?{string.Join("&", soundIds.Select(x => "soundIds=" + x))}";
        HttpRequestMessage message = new(HttpMethod.Get, url);
        HttpResponseMessage response = await _client.SendAsync(message);
        if (!response.IsSuccessStatusCode)
        {
            response.Dispose();
            message.Dispose();
            message = new(HttpMethod.Post, url);
            response = await _client.SendAsync(message);
        }

        if (response.IsSuccessStatusCode)
        {
            using Stream content = await response.Content.ReadAsStreamAsync();
            ShareDetail? shareDetail = await JsonSerializer.DeserializeAsync(
                content,
                AmbieJsonSerializerContext.CaseInsensitive.ShareDetail);

            return shareDetail;
        }

        return null;
    }

    /// <inheritdoc/>
    public async Task<ShareDetail?> GetShareDetailAsync(string shareId)
    {
        if (string.IsNullOrEmpty(shareId))
        {
            return null;
        }

        var url = $"{_baseUrl}/detail?shareId={shareId}";
        HttpRequestMessage message = new(HttpMethod.Get, url);
        HttpResponseMessage response = await _client.SendAsync(message);
        if (response.IsSuccessStatusCode)
        {
            using Stream content = await response.Content.ReadAsStreamAsync();
            ShareDetail? shareDetail = await JsonSerializer.DeserializeAsync(
                content,
                AmbieJsonSerializerContext.CaseInsensitive.ShareDetail);
            return shareDetail;
        }

        return null;
    }
}
