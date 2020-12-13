using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// A provider of sound data.
    /// </summary>
    public sealed class SoundDataProvider : ISoundDataProvider
    {
        private const string DataFileName = "Data.json";
        private const string LocalDataFileName = "localData.json";
        private List<Sound> _localSoundCache;

        /// <inheritdoc/>
        public event EventHandler<Sound> LocalSoundAdded;

        /// <inheritdoc/>
        public event EventHandler<string> LocalSoundDeleted;

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetSoundsAsync()
        {
            var packagedSounds = await GetPackagedSoundsAsync();
            var localSounds = await GetLocalSoundsAsync(refresh: true);
            packagedSounds.AddRange(localSounds);
            return packagedSounds;
        }

        /// <inheritdoc/>
        public async Task DeleteLocalSoundAsync(Sound s)
        {
            if (s == null || !await IsSoundInstalledAsync(s))
            {
                return;
            }

            // Delete from cache
            var soundForDeletion = _localSoundCache.First(x => x.Id == s.Id);
            _localSoundCache.Remove(soundForDeletion);

            // Write changes to file
            StorageFile localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                LocalDataFileName,
                CreationCollisionOption.OpenIfExists);
            string json = JsonSerializer.Serialize(_localSoundCache);
            await FileIO.WriteTextAsync(localDataFile, json);

            // Delete sound file 
            StorageFile soundFile = await StorageFile.GetFileFromPathAsync(s.FilePath);
            await soundFile.DeleteAsync();

            LocalSoundDeleted?.Invoke(this, soundForDeletion.Id);
        }

        /// <inheritdoc/>
        public async Task AddLocalSoundAsync(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));

            bool isAlreadyInstalled = await IsSoundInstalledAsync(s);
            if (isAlreadyInstalled)
            {
                return;
            }

            StorageFile localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                LocalDataFileName,
                CreationCollisionOption.OpenIfExists);

            _localSoundCache.Add(s);
            string json = JsonSerializer.Serialize(_localSoundCache);
            await FileIO.WriteTextAsync(localDataFile, json);
            LocalSoundAdded?.Invoke(this, s);
        }

        /// <inheritdoc/>
        public async Task<bool> IsSoundInstalledAsync(Sound s)
        {
            if (s == null)
            {
                return false;
            }

            IReadOnlyList<Sound> sounds = await GetLocalSoundsAsync();
            return sounds.Any(x => x.Id == s.Id);
        }

        private async Task<IReadOnlyList<Sound>> GetLocalSoundsAsync(
            StorageFile localDataFile = null,
            bool refresh = false)
        {
            if (localDataFile == null)
            {
                localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    LocalDataFileName,
                    CreationCollisionOption.OpenIfExists);
            }

            if (_localSoundCache != null && !refresh)
            {
                return _localSoundCache;
            }

            try
            {
                using (Stream dataStream = await localDataFile.OpenStreamForReadAsync())
                {
                    _localSoundCache = await JsonSerializer.DeserializeAsync<List<Sound>>(dataStream);
                }
            }
            catch (Exception e)
            {
                // TODO log
            }

            if (_localSoundCache == null)
            {
                _localSoundCache = new List<Sound>();
            }

            return _localSoundCache.AsReadOnly();
        }

        private async Task<List<Sound>> GetPackagedSoundsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile dataFile = await assets.GetFileAsync(DataFileName);
            using Stream dataStream = await dataFile.OpenStreamForReadAsync();
            return await JsonSerializer.DeserializeAsync<List<Sound>>(dataStream);
        }
    }
}
