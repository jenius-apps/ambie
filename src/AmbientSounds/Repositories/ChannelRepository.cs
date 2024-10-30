using AmbientSounds.Models;
using AmbientSounds.Services;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories;

public class ChannelRepository : IChannelRepository
{
    private readonly HttpClient _client;
    private readonly string _channelsUrl;

    public ChannelRepository(
        HttpClient httpClient,
        IAppSettings appSettings)
    {
        _client = httpClient;
        _channelsUrl = appSettings.ChannelsUrl;
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<Channel>> GetItemsAsync()
    {
        try
        {
            using Stream result = await _client.GetStreamAsync(_channelsUrl);
            var results = await JsonSerializer.DeserializeAsync(
                result,
                AmbieJsonSerializerContext.CaseInsensitive.ChannelArray);
            return results ?? [];
        }
        catch
        {
            return [];
        }
    }
}
