using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.FileProperties;
using Windows.Storage.Pickers;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for picking a file and remembering it
    /// in Future-Access list. 
    /// </summary>
    public class FilePicker : IFilePicker
    {
        private const string UploadFileKey = "soundToUpload";

        /// <inheritdoc/>
        public async Task<string> OpenPickerAsync()
        {
            var file = await OpenPickerAndGetFileAsync();

            return file is null
                ? ""
                : file.Path;
        }

        /// <inheritdoc/>
        public async Task<(string, ulong)> OpenPickerAndGetSizeAsync()
        {
            var file = await OpenPickerAndGetFileAsync();
            if (file is null)
            {
                return ("", 0);
            }

            BasicProperties basicProperties = await file.GetBasicPropertiesAsync();
            return (file.Path, basicProperties.Size);
        }

        private async Task<StorageFile?> OpenPickerAndGetFileAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");

            var file = await picker.PickSingleFileAsync();
            if (file is not null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(UploadFileKey, file);
                return file;
            }

            return null;
        }

        /// <inheritdoc/>
        public async Task<byte[]?> GetCachedBytesAsync(string filePath)
        {
            StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(UploadFileKey);
            if (file is not null && file.Path == filePath)
            {
                byte[] result;
                using Stream stream = await file.OpenStreamForReadAsync();
                using var memoryStream = new MemoryStream();
                stream.CopyTo(memoryStream);
                result = memoryStream.ToArray();
                return result;
            }

            return null;
        }
    }
}
