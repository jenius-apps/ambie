using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
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

                var results = await JsonSerializer.DeserializeAsync(result, AmbieJsonSerializerContext.CaseInsensitive.VideoArray);

                return results ?? Array.Empty<Video>();
            }
            catch
            {
                return Array.Empty<Video>();
            }
        }

        /// <inheritdoc/>
        public async Task<string> GetDownloadUrlAsync(string videoId)
        {
            try
            {
                var result = await _client.GetAsync($"{_videosUrl}/{videoId}?withDownloadUrl=true");
                if (result.IsSuccessStatusCode)
                {
                    var json = await result.Content.ReadAsStringAsync();
                    var video = JsonSerializer.Deserialize(json, AmbieJsonSerializerContext.CaseInsensitive.Video);
                    return video?.DownloadUrl ?? string.Empty;
                }
            }
            catch { }

            return string.Empty;
        }
    }
}
