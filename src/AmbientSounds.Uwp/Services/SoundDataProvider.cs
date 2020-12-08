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

        /// <inheritdoc/>
        public event EventHandler<Sound> LocalSoundAdded;

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetSoundsAsync()
        {
            var packagedSounds = await GetPackagedSoundsAsync();
            var localSounds = await GetLocalSoundsAsync();
            packagedSounds.AddRange(localSounds);
            return packagedSounds;
        }

        /// <inheritdoc/>
        public async Task AddLocalSoundAsync(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));

            StorageFile localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                LocalDataFileName,
                CreationCollisionOption.OpenIfExists);

            List<Sound> localSounds = await GetLocalSoundsAsync(localDataFile);
            if (localSounds.Any(x => x.FilePath == s.FilePath))
            {
                // prevent duplication
                return;
            }

            localSounds.Add(s);
            string json = JsonSerializer.Serialize(localSounds);
            await FileIO.WriteTextAsync(localDataFile, json);
            LocalSoundAdded?.Invoke(this, s);
        }

        private async Task<List<Sound>> GetLocalSoundsAsync(StorageFile localDataFile = null)
        {
            if (localDataFile == null)
            {
                localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    LocalDataFileName,
                    CreationCollisionOption.OpenIfExists);
            }

            List<Sound> localSounds = null;

            try
            {
                using (Stream dataStream = await localDataFile.OpenStreamForReadAsync())
                {
                    localSounds = await JsonSerializer.DeserializeAsync<List<Sound>>(dataStream);
                }
            }
            catch (Exception e)
            {
                // TODO log
            }

            if (localSounds == null)
            {
                localSounds = new List<Sound>();
            }

            return localSounds;
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
