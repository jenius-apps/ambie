using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Writes ambie data to local directory.
    /// </summary>
    public class FileWriter : IFileWriter
    {
        private static readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();
        private const string _soundsDirName = "sounds";
        private const string _imagesDirName = "images";

        /// <inheritdoc/>
        public async Task<string> ReadAsync(string relativeLocalPath, CancellationToken ct = default)
        {
            ct.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(relativeLocalPath))
            {
                return string.Empty;
            }

            IStorageItem targetLocation = await ApplicationData.Current.LocalFolder.TryGetItemAsync(relativeLocalPath);
            ct.ThrowIfCancellationRequested();

            if (targetLocation is StorageFile file)
            {
                return await FileIO.ReadTextAsync(file);
            }

            return string.Empty;
        }

        /// <inheritdoc/>
        public async Task<bool> DeleteFileAsync(string absolutePathInLocalStorage)
        {
            if (string.IsNullOrEmpty(absolutePathInLocalStorage))
            {
                return false;
            }

            try
            {
                var file = await StorageFile.GetFileFromPathAsync(absolutePathInLocalStorage);
                await file.DeleteAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <inheritdoc/>
        public async Task WriteStringAsync(string content, string relativeLocalPath)
        {
            if (string.IsNullOrEmpty(relativeLocalPath))
            {
                return;
            }

            var semaphore = _semaphores.GetOrAdd(relativeLocalPath, new SemaphoreSlim(1, 1));
            await semaphore.WaitAsync();
            try
            {
                StorageFile targetLocation = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                    relativeLocalPath,
                    CreationCollisionOption.OpenIfExists);

                if (targetLocation != null)
                {
                    await FileIO.WriteTextAsync(targetLocation, content);
                }
            }
            finally
            {
                semaphore.Release();
            }
        }


        /// <inheritdoc/>
        public Task<string> WriteSoundAsync(Stream stream, string nameWithExt)
        {
            return WriteFileAsync(stream, nameWithExt, _soundsDirName);
        }

        /// <inheritdoc/>
        public Task<string> WriteImageAsync(Stream stream, string nameWithExt)
        {
            return WriteFileAsync(stream, nameWithExt, _imagesDirName);
        }

        /// <inheritdoc/>
        public async Task<string> WriteFileAsync(Stream stream, string nameWithExt, string? localDirName = null)
        {
            StorageFolder dir;

            if (string.IsNullOrWhiteSpace(localDirName))
            {
                dir = ApplicationData.Current.LocalFolder;
            }
            else
            {
                dir = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                    localDirName,
                    CreationCollisionOption.OpenIfExists);
            }

            StorageFile storageFile = await dir.CreateFileAsync(
                nameWithExt,
                CreationCollisionOption.ReplaceExisting);

            using IRandomAccessStream fileStream = await storageFile.OpenAsync(FileAccessMode.ReadWrite);
            await stream.CopyToAsync(fileStream.AsStreamForWrite());
            await fileStream.FlushAsync();
            return storageFile.Path;
        }

        /// <inheritdoc/>
        public async Task<string> WriteBitmapAsync(Stream stream, string nameWithExt)
        {
            StorageFile storageFile = await ApplicationData.Current.LocalFolder.CreateFileAsync(
                nameWithExt,
                CreationCollisionOption.ReplaceExisting);

            // ref: https://codedocu.com/Details?d=1592&a=9&f=181&l=0&v=d
            using (IRandomAccessStream s = stream.AsRandomAccessStream())
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(s);

                // Get the SoftwareBitmap representation of the file
                var softwareBitmap = await decoder.GetSoftwareBitmapAsync();

                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, await storageFile.OpenAsync(FileAccessMode.ReadWrite));

                encoder.SetSoftwareBitmap(softwareBitmap);

                await encoder.FlushAsync();

                return storageFile.Path;
            }
        }
    }
}
