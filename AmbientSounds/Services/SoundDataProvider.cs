using AmbientSounds.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

namespace AmbientSounds.Services
{
    /// <summary>
    /// Retrieves list of sound data
    /// available to the app.
    /// </summary>
    public class SoundDataProvider
    {
        private const string DataFileName = "Data.json";

        /// <summary>
        /// Retrieves list of sound data available to the app.
        /// </summary>
        /// <returns>List of <see cref="Sound"/>.</returns>
        public static async Task<IList<Sound>> GetSoundsAsync()
        {
            StorageFolder appInstalledFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync("Assets");
            var dataFile = await assets.GetFileAsync(DataFileName);
            string text = await FileIO.ReadTextAsync(dataFile);
            return JsonConvert.DeserializeObject<Sound[]>(text);
        }
    }
}
