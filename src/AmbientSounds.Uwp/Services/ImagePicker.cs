using System;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;

#nullable enable

namespace AmbientSounds.Services.Uwp
{
    /// <summary>
    /// Class for browing for an image
    /// and caching it and then forwarding
    /// </summary>
    public class ImagePicker : IImagePicker
    {
        private const string CustomImageFolderName = "CustomBackgrounds";
        /// <inheritdoc/>
        public async Task<string?> BrowseAsync()
        {
            var picker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary
            };
            picker.FileTypeFilter.Add(".jpeg");
            picker.FileTypeFilter.Add(".jpg");
            picker.FileTypeFilter.Add(".png");

            var file = await picker.PickSingleFileAsync();
            if (file != null)
            {
                StorageFolder customFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync(
                    CustomImageFolderName,
                    CreationCollisionOption.OpenIfExists);

                StorageFile cached = await file.CopyAsync(
                    customFolder,
                    file.Name,
                    NameCollisionOption.ReplaceExisting);
                return cached.Path;
            }

            return null;
        }
    }
}
