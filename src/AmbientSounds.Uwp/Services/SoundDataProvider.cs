using AmbientSounds.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Storage;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// A provider of sound data.
    /// </summary>
    public sealed class SoundDataProvider : ISoundDataProvider
    {
        private const string DataFileName = "Data.json";

        /// <inheritdoc/>
        public async Task<IList<Sound>> GetSoundsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            StorageFile dataFile = await assets.GetFileAsync(DataFileName);

            using Stream dataStream = await dataFile.OpenStreamForReadAsync();

            var resourceLoader = ResourceLoader.GetForCurrentView();
            var sounds = await JsonSerializer.DeserializeAsync<Sound[]>(dataStream);

            foreach (var s in sounds)
            {
                var translatedName = resourceLoader.GetString("Sound-" + s.Id);
                if (!string.IsNullOrWhiteSpace(translatedName))
                {
                    s.Name = translatedName;
                }
            }

            return sounds;
        }
    }
}
