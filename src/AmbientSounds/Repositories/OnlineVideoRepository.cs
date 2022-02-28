using AmbientSounds.Models;
using AmbientSounds.Services;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public class OnlineVideoRepository : IOnlineVideoRepository
    {
        private readonly string _videosUrl;
        private readonly HttpClient _client;

        public OnlineVideoRepository(
            HttpClient httpClient,
            IAppSettings appSettings)
        {
            Guard.IsNotNull(httpClient, nameof(httpClient));
            Guard.IsNotNull(appSettings, nameof(appSettings));
            _client = httpClient;
            _videosUrl = appSettings.VideosUrl;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetVideosAsync()
        {
            try
            {
                using Stream result = await _client.GetStreamAsync(_videosUrl);
                var results = await JsonSerializer.DeserializeAsync<Video[]>(
                    result,
                    new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });

                return results ?? Array.Empty<Video>();
            }
            catch
            {
                return Array.Empty<Video>();
            }
        }
    }
}
