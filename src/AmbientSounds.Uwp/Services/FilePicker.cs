using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;

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
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.List,
                SuggestedStartLocation = PickerLocationId.MusicLibrary
            };
            picker.FileTypeFilter.Add(".mp3");
            picker.FileTypeFilter.Add(".wav");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                StorageApplicationPermissions.FutureAccessList.AddOrReplace(UploadFileKey, file);
                return file.Path;
            }

           return "";
        }

        /// <inheritdoc/>
        public async Task<byte[]> GetCachedBytesAsync(string filePath)
        {
            StorageFile file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(UploadFileKey);
            if (file != null && file.Path == filePath)
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
