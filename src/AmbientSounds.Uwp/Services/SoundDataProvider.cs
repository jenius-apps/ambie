using AmbientSounds.Converters;
using AmbientSounds.Models;
using CommunityToolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// A provider of sound data.
    /// </summary>
    public sealed class SoundDataProvider : ISoundDataProvider
    {
        private const string DataFileName = "Data.json";
        private const string LocalDataFileName = "localData.json";
        private List<Sound>? _localSoundCache; // cache of non-packaged sounds.
        private List<Sound>? _packagedSoundCache;
        private Random? _random;        

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetSoundsAsync(string[]? soundIds = null)
        {
            var packagedSoundsTask = GetPackagedSoundsAsync();
            var localSounds = await GetLocalSoundsInternalAsync();
            var packagedSounds = await packagedSoundsTask;
            packagedSounds.AddRange(localSounds);

            if (soundIds is null) return packagedSounds;
            else return packagedSounds.Where(x => soundIds.Contains(x.Id)).ToArray();
        }

        /// <inheritdoc/>
        public async Task<Sound> GetRandomSoundAsync()
        {
            _random ??= new Random();
            var sounds = await GetSoundsAsync();
            var index = _random.Next(sounds.Count);
            return sounds[index];
        }

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetLocalSoundsAsync()
        {
            var localSounds = await GetLocalSoundsInternalAsync();
            return localSounds.ToList();
        }

        /// <inheritdoc/>
        public Task UpdateLocalSoundAsync(IReadOnlyList<Sound> sounds)
        {
            if (sounds is null || sounds.Count == 0 || _localSoundCache is null)
            {
                return Task.CompletedTask;
            }

            foreach (var s in sounds)
            {
                if (_localSoundCache.Any(x => x.Id == s.Id))
                {
                    var item = _localSoundCache.First(x => x.Id == s.Id);
                    item.Name = s.Name;
                }
            }

            return WriteCacheAsync();
        }

        /// <inheritdoc/>
        public async Task RefreshLocalSoundsMetaDataAsync(IList<Sound> latestData)
        {
            if (_localSoundCache is not { Count: > 0 })
            {
                return;
            }

            string[] currentSoundIds = _localSoundCache.Select(static x => x.Id).ToArray();
            if (latestData is not { Count: > 0 })
            {
                return;
            }

            foreach (Sound cachedSound in _localSoundCache)
            {
                var updatedSound = latestData.FirstOrDefault(x => x.Id == cachedSound.Id);
                if (updatedSound is not null)
                {
                    cachedSound.Name = updatedSound.Name;
                    cachedSound.ScreensaverImagePaths = updatedSound.ScreensaverImagePaths;
                    cachedSound.Attribution = updatedSound.Attribution;
                    cachedSound.ColourHex = updatedSound.ColourHex;
                }
            }

            // Write changes to file
            await WriteCacheAsync();
        }

        private async Task WriteCacheAsync()
        {
            StorageFile localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                LocalDataFileName,
                CreationCollisionOption.OpenIfExists);
            string json = _localSoundCache is null ? "" : JsonSerializer.Serialize(_localSoundCache, AmbieJsonSerializerContext.Default.ListSound);
            await FileIO.WriteTextAsync(localDataFile, json);
        }

        private async Task<IReadOnlyList<Sound>> GetLocalSoundsInternalAsync(
            StorageFile? localDataFile = null,
            bool refresh = false)
        {
            if (localDataFile is null)
            {
                localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    LocalDataFileName,
                    CreationCollisionOption.OpenIfExists);
            }

            if (_localSoundCache is not null && !refresh)
            {
                return _localSoundCache.AsReadOnly();
            }

            try
            {
                using Stream dataStream = await localDataFile.OpenStreamForReadAsync();
                _localSoundCache = await JsonSerializer.DeserializeAsync(dataStream, AmbieJsonSerializerContext.Default.ListSound);
            }
            catch (Exception)
            {
                // TODO log
            }

            _localSoundCache ??= new List<Sound>();
            return _localSoundCache.AsReadOnly();
        }

        private async Task<List<Sound>> GetPackagedSoundsAsync()
        {
            if (_packagedSoundCache is null)
            {
                StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
                StorageFile dataFile = await assets.GetFileAsync(DataFileName);
                using Stream dataStream = await dataFile.OpenStreamForReadAsync();
                var sounds = await JsonSerializer.DeserializeAsync(dataStream, AmbieJsonSerializerContext.Default.ListSound);

                foreach (var s in sounds!)
                {
                    s.Name = LocalizationConverter.ConvertSoundName(s.Name);
                }

                _packagedSoundCache = sounds;
            }

            return _packagedSoundCache.ToList();
        }
    }
}
