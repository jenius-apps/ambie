using AmbientSounds.Models;
using AmbientSounds.Services;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Repositories
{
    public class OfflineSoundRepository : IOfflineSoundRepository
    {
        private const string LocalDataFileName = "localData.json";
        private readonly IFileWriter _fileWriter;

        public OfflineSoundRepository(IFileWriter fileWriter)
        {
            Guard.IsNotNull(fileWriter);
            _fileWriter = fileWriter;
        }

        /// <inheritdoc/>
        public Task<IReadOnlyList<Sound>> GetLocalSoundsAsync()
        {
            return GetSoundsAsync(LocalDataFileName);
        }

        private async Task<IReadOnlyList<Sound>> GetSoundsAsync(string file)
        {
            string content = await _fileWriter.ReadAsync(file);
            if (string.IsNullOrEmpty(content))
            {
                return Array.Empty<Sound>();
            }

            try
            {
                var result = JsonSerializer.Deserialize(content, AmbieJsonSerializerContext.Default.SoundArray);
                return result ?? Array.Empty<Sound>();
            }
            catch
            {
                return Array.Empty<Sound>();
            }
        }

        /// <inheritdoc/>
        public Task SaveLocalSoundsAsync(IList<Sound> sounds)
        {
            return _fileWriter.WriteStringAsync(
                JsonSerializer.Serialize(sounds, AmbieJsonSerializerContext.Default.IListSound),
                LocalDataFileName);
        }
    }
}
