using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    /// <summary>
    /// Reads and writes video metadata from local storage.
    /// </summary>
    public class OfflineVideoRepository : IOfflineVideoRepository
    {
        private const string LocalVideoDataFile = "localVideos.json";
        private readonly IFileWriter _fileWriter;

        public OfflineVideoRepository(IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter, nameof(fileWriter));
            _fileWriter = fileWriter;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<Video>> GetVideosAsync()
        {
            string content = await _fileWriter.ReadAsync(LocalVideoDataFile);
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<Video>();
            }

            try
            {
                var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.VideoArray);
                return result ?? Array.Empty<Video>();
            }
            catch
            {
                return Array.Empty<Video>();
            }
        }

        /// <inheritdoc/>
        public Task SaveVideosAsync(IList<Video> videos)
        {
            return _fileWriter.WriteStringAsync(JsonSerializer.Serialize(videos, AmbieJsonSerializerContext.Default.IListVideo), LocalVideoDataFile);
        }
    }
}
