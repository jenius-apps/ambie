using AmbientSounds.Converters;
using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
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

        /// <inheritdoc/>
        public event EventHandler<Sound>? LocalSoundAdded;

        /// <inheritdoc/>
        public event EventHandler<string>? LocalSoundDeleted;

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetSoundsAsync(bool refresh = false, string[]? soundIds = null)
        {
            var packagedSounds = await GetPackagedSoundsAsync();
            var localSounds = await GetLocalSoundsInternalAsync(refresh: refresh);
            packagedSounds.AddRange(localSounds);

            if (soundIds is null) return packagedSounds;
            else return packagedSounds.Where(x => soundIds.Contains(x.Id)).ToArray();
        }

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetLocalSoundsAsync()
        {
            var localSounds = await GetLocalSoundsInternalAsync();
            return localSounds.ToList();
        }

        /// <inheritdoc/>
        public Task UpdateLocalSoundAsync(IList<Sound> sounds)
        {
            if (sounds is null || sounds.Count == 0)
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
        public async Task DeleteLocalSoundAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !await IsSoundInstalledAsync(id))
            {
                return;
            }

            // TODO: throw or just ignore?
            Guard.IsNotNull(_localSoundCache, nameof(_localSoundCache));

            // Delete from cache
            var soundForDeletion = _localSoundCache.First(x => x.Id == id);
            _localSoundCache.Remove(soundForDeletion);

            // Write changes to file
            await WriteCacheAsync();

            // Delete sound file 
            if (!string.IsNullOrWhiteSpace(soundForDeletion.FilePath))
            {
                StorageFile soundFile = await StorageFile.GetFileFromPathAsync(soundForDeletion.FilePath);
                await soundFile.DeleteAsync();
            }

            // Delete image file
            if (!string.IsNullOrWhiteSpace(soundForDeletion.ImagePath))
            {
                StorageFile imageFile = await StorageFile.GetFileFromPathAsync(soundForDeletion.ImagePath);
                await imageFile.DeleteAsync();
            }

            LocalSoundDeleted?.Invoke(this, soundForDeletion.Id);
        }

        /// <inheritdoc/>
        public async Task AddLocalSoundAsync(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));

            bool isAlreadyInstalled = await IsSoundInstalledAsync(s.Id);
            if (isAlreadyInstalled)
            {
                return;
            }

            // TODO: throw or just ignore?
            Guard.IsNotNull(_localSoundCache, nameof(_localSoundCache));

            _localSoundCache.Add(s);
            await WriteCacheAsync();
            LocalSoundAdded?.Invoke(this, s);
        }

        /// <inheritdoc/>
        public async Task<bool> IsSoundInstalledAsync(string id)
        {
            if (id is null)
            {
                return false;
            }

            var packagedSounds = await GetPackagedSoundsAsync();
            if (packagedSounds.Any(x => x.Id == id)) return true;

            IReadOnlyList<Sound> sounds = await GetLocalSoundsInternalAsync();
            return sounds.Any(x => x.Id == id);
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
            string json = JsonSerializer.Serialize(_localSoundCache);
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
                _localSoundCache = await JsonSerializer.DeserializeAsync<List<Sound>>(dataStream);
            }
            catch (Exception e)
            {
                // TODO log
            }

            if (_localSoundCache is null)
            {
                _localSoundCache = new List<Sound>();
            }

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
                var sounds = await JsonSerializer.DeserializeAsync<List<Sound>>(dataStream);

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
