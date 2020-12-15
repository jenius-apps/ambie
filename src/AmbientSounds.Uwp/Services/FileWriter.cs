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
        private const string _imagesDirName = "images";

        /// <inheritdoc/>
        public Task<string> WriteSoundAsync(Stream stream, string nameWithExt)
        {
            return WriteFileAsync(stream, _soundsDirName, nameWithExt);
        }

        /// <inheritdoc/>
        public Task<string> WriteImageAsync(Stream stream, string nameWithExt)
        {
            return WriteFileAsync(stream, _imagesDirName, nameWithExt);
        }

        private static async Task<string> WriteFileAsync(Stream stream, string localDirName, string nameWithExt)
        {
            StorageFolder dir = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                localDirName,
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
