using System;
using System.IO;
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
        private const string _soundsDirName = "sounds";
        private const string _imagesDirName = "images";

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
