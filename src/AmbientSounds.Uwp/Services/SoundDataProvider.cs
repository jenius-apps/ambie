using AmbientSounds.Models;
using Microsoft.Toolkit.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
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
        public async Task<IList<Sound>> GetSoundsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile dataFile = await assets.GetFileAsync(DataFileName);

            using Stream dataStream = await dataFile.OpenStreamForReadAsync();

            return await JsonSerializer.DeserializeAsync<Sound[]>(dataStream);
        }

        /// <inheritdoc/>
        public async Task AddLocalSoundAsync(Sound s)
        {
            Guard.IsNotNull(s, nameof(s));

            StorageFile localDataFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                LocalDataFileName,
                CreationCollisionOption.OpenIfExists);

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

            localSounds.Add(s);
            string json = JsonSerializer.Serialize(localSounds);
            await FileIO.WriteTextAsync(localDataFile, json);
        }
    }
}
