using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Streams;

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Writes ambie data to local directory.
    /// </summary>
    public class FileWriter : IFileWriter
    {
        private const string _soundsDirName = "sounds";

        /// <inheritdoc/>
        public async Task<string> WriteSoundAsync(Stream stream, string nameWithExt)
        {
            StorageFolder dir = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                _soundsDirName,
                CreationCollisionOption.OpenIfExists);
            StorageFile storageFile = await dir.CreateFileAsync(
                nameWithExt,
                CreationCollisionOption.ReplaceExisting);

            using IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            await stream.CopyToAsync(fileStream.AsStreamForWrite());
            await fileStream.FlushAsync();
            return storageFile.Path;
        }
    }
}
