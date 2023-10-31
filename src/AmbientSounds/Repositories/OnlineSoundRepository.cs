using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using JeniusApps.Common.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class OnlineSoundRepository : IOnlineSoundRepository
{
    private readonly ISystemInfoProvider _systemInfoProvider;
    private readonly HttpClient _client;
    private readonly string _url;

    public OnlineSoundRepository(
        HttpClient httpClient,
        ISystemInfoProvider systemInfoProvider,
        IAppSettings appSettings)
    {
        Guard.IsNotNull(systemInfoProvider);
        Guard.IsNotNull(httpClient);
        _systemInfoProvider = systemInfoProvider;
        _client = httpClient;
        _url = appSettings.CatalogueUrl;
    }

    /// <inheritdoc/>
    public async Task<string> GetDownloadLinkAsync(Sound s)
    {
        Guard.IsNotNull(s);

        var url = $"{_url}/{s.Id}/file?userId={s.UploadedBy}";

        try
        {
            return await _client.GetStringAsync(url);
        }
        catch
        {
            return "";
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync()
    {
        var url = _url + $"?culture={_systemInfoProvider.GetCulture()}&premium=true";
        try
        {
            using Stream result = await _client.GetStreamAsync(url);
            var results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.SoundArray);
            return results ?? Array.Empty<Sound>();
        }
        catch
        {
            return Array.Empty<Sound>();
        }
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyDictionary<string, Sound?>> GetOnlineSoundsAsync(
        IReadOnlyList<string> soundIds,
        string? iapId = null)
    {
        if (soundIds.Count == 0)
        {
            return new Dictionary<string, Sound?>();
        }

        var url = _url + $"/sounds?{string.Join("&", soundIds.Select(x => "ids=" + x))}";
        if (!string.IsNullOrEmpty(iapId))
        {
            url += $"&iapId={iapId}";
        }

        var dict = new Dictionary<string, Sound?>();
        foreach (var id in soundIds)
        {
            dict.Add(id, null);
        }

        try
        {
            using Stream result = await _client.GetStreamAsync(url);
            var results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.SoundArray);
            if (results is not null)
            {
                foreach (var s in results)
                {
                    if (dict.ContainsKey(s.Id))
                    {
                        dict[s.Id] = s;
                    }
                }
            }
        }
        catch { }

        return dict;
    }
}
