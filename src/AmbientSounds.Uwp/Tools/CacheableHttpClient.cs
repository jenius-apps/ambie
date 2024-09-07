using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using Windows.Web.Http;

namespace AmbientSounds.Tools.Uwp;

internal class CacheableHttpClient : ICacheableHttpClient
{
    private readonly HttpClient _client;

    public CacheableHttpClient()
    {
        _client = new HttpClient();
    }

    /// <inheritdoc/>
    public async Task<Stream> GetStreamAsync(string url)
    {
        IInputStream inputStream = await _client.GetInputStreamAsync(new Uri(url)).AsTask();
        return inputStream.AsStreamForRead();
    }

    /// <inheritdoc/>
    public async Task<string> GetStringAsync(string url)
    {
        return await _client.GetStringAsync(new Uri(url)).AsTask();
    }
}
