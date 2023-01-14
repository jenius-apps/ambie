using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class ShareService : IShareService
{
    private readonly HttpClient _client;
    private readonly ConcurrentDictionary<string, ShareDetail> _soundIdCache = new();
    private readonly ConcurrentDictionary<string, ShareDetail> _shareIdCache = new();
    private readonly string _baseUrl;

    public ShareService(
        HttpClient httpClient,
        IAppSettings appSettings)
    {
        Guard.IsNotNull(httpClient);
        Guard.IsNotNull(appSettings.ShareUrl);

        _client = httpClient;
        _baseUrl = appSettings.ShareUrl;
    }

    public async Task<ShareDetail?> GetShareDetailAsync(IReadOnlyList<string> soundIds)
    {
        if (soundIds.Count == 0)
        {
            return null;
        }

        string key = SortAndCompose(soundIds);
        if (_soundIdCache.TryGetValue(key, out ShareDetail result))
        {
            return result;
        }

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
            ShareDetail? shareDetail = await JsonSerializer.DeserializeAsync(content, AmbieJsonSerializerContext.CaseInsensitive.ShareDetail);

            if (shareDetail is not null)
            {
                _soundIdCache.TryAdd(key, shareDetail);
                _shareIdCache.TryAdd(shareDetail.Id, shareDetail);
            }

            return shareDetail;
        }

        return null;
    }

    public async Task<ShareDetail?> GetShareDetailAsync(string shareId)
    {
        if (string.IsNullOrEmpty(shareId))
        {
            return null;
        }

        if (_shareIdCache.TryGetValue(shareId, out ShareDetail result))
        {
            return result;
        }

        var url = $"{_baseUrl}/detail?shareId={shareId}";
        HttpRequestMessage message = new(HttpMethod.Get, url);
        HttpResponseMessage response = await _client.SendAsync(message);
        if (response.IsSuccessStatusCode)
        {
            using Stream content = await response.Content.ReadAsStreamAsync();
            ShareDetail? shareDetail = await JsonSerializer.DeserializeAsync(content, AmbieJsonSerializerContext.CaseInsensitive.ShareDetail);

            if (shareDetail is not null)
            {
                _soundIdCache.TryAdd(shareDetail.SoundIdComposite, shareDetail);
                _shareIdCache.TryAdd(shareId, shareDetail);
            }

            return shareDetail;
        }

        return null;
    }

    public async Task<IReadOnlyList<string>> GetSoundIdsAsync(string shareId)
    {
        ShareDetail? detail = await GetShareDetailAsync(shareId);
        if (string.IsNullOrEmpty(detail?.SoundIdComposite))
        {
            return Array.Empty<string>();
        }

        return detail!.SoundIdComposite.Split(';');
    }

    private string SortAndCompose(IReadOnlyList<string> ids)
    {
        // This method is the same as in the service.
        // Do not change unless also changing service.
        // TODO: Move to nuget shared with service.
        return string.Join(";", ids.OrderBy(x => x));
    }
}
