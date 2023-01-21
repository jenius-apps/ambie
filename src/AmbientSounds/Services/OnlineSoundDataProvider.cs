using AmbientSounds.Cache;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

/// <summary>
/// Retrieves sound data from an online source.
/// </summary>
public class OnlineSoundDataProvider : IOnlineSoundDataProvider
{
    private readonly ISystemInfoProvider _systemInfoProvider;
    private readonly ISoundCache _assetsReader;
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly string _mySoundsUrl;

    /// <inheritdoc/>
    public event EventHandler<int>? UserSoundsFetched;

    public OnlineSoundDataProvider(
        HttpClient httpClient,
        ISystemInfoProvider systemInfoProvider,
        IAppSettings appSettings,
        ISoundCache assetsReader)
    {
        Guard.IsNotNull(systemInfoProvider);
        Guard.IsNotNull(httpClient);
        Guard.IsNotNull(assetsReader);
        _systemInfoProvider = systemInfoProvider;
        _client = httpClient;
        _assetsReader = assetsReader;
        _url = appSettings.CatalogueUrl;
        _mySoundsUrl = appSettings.MySoundsUrl;
    }

    /// <inheritdoc/>
    public async Task<string> GetDownloadLinkAsync(Sound s)
    {
        if (s is null)
        {
            return "";
        }

        var url = $"{_url}/{s.Id}/file?userId={s.UploadedBy}";

        try
        {
            var result = await _client.GetStringAsync(url);
            return result;
        }
        catch
        {
            return "";
        }
    }

    /// <inheritdoc/>
    public async Task<IList<Sound>> GetSoundsAsync()
    {
        if (string.IsNullOrWhiteSpace(_url))
        {
            return new List<Sound>();
        }

        var url = _url + $"?culture={_systemInfoProvider.GetCulture()}&premium=true";
        using Stream result = await _client.GetStreamAsync(url);
        var results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.SoundArray);

        var packagedSounds = await _assetsReader.GetPreinstalledSoundsAsync();

        var list = new List<Sound>(packagedSounds.OrderBy(x => x.Id));
        if (results is not null)
        {
            list.AddRange(results);
        }

        return list;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Sound>> GetOnlineSoundsAsync(
        IReadOnlyList<string> soundIds,
        string? iapId = null)
    {
        if (soundIds is null || soundIds.Count == 0)
        {
            return Array.Empty<Sound>();
        }

        var url = _url + $"/sounds?{string.Join("&", soundIds.Select(x => "ids=" + x))}";
        if (!string.IsNullOrEmpty(iapId))
        {
            url += $"&iapId={iapId}";
        }

        using Stream result = await _client.GetStreamAsync(url);
        var results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.SoundArray);

        return results ?? Array.Empty<Sound>();
    }

    /// <inheritdoc/>
    public async Task<IList<Sound>> GetSoundsAsync(IList<string> soundIds)
    {
        if (soundIds is null || soundIds.Count == 0)
        {
            return Array.Empty<Sound>();
        }

        var sounds = await GetSoundsAsync();
        return sounds?.Where(x => x.Id is not null && soundIds.Contains(x.Id)).ToArray()
            ?? Array.Empty<Sound>();
    }

    /// <inheritdoc/>
    public async Task<IList<Sound>> GetUserSoundsAsync(string accesstoken)
    {
        if (string.IsNullOrWhiteSpace(accesstoken))
        {
            return Array.Empty<Sound>();
        }

        using var msg = new HttpRequestMessage(HttpMethod.Get, _mySoundsUrl);
        msg.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accesstoken);
        var response = await _client.SendAsync(msg);
        using Stream result = await response.Content.ReadAsStreamAsync();

        try
        {
            var results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.SoundArray);

            UserSoundsFetched?.Invoke(this, results?.Length ?? 0);
            return results ?? Array.Empty<Sound>();
        }
        catch
        {
            UserSoundsFetched?.Invoke(this, 0);
            return Array.Empty<Sound>();
        }
    }
}
