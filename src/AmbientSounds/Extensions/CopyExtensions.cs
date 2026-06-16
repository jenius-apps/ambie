using AmbientSounds.Models;

namespace AmbientSounds.Extensions;
public static class CopyExtensions
{
    public static Sound DeepCopy(this Sound sound)
    {
        return new Sound()
        {
            Id = sound.Id,
            ImagePath = sound.ImagePath,
            Name = sound.Name,
            Description = sound.Description,
            PreviewFilePath = sound.PreviewFilePath,
            FilePath = sound.FilePath,
            Attribution = sound.Attribution,
            IapId = sound.IapId,
            FileExtension = sound.FileExtension,
            ScreensaverImagePaths = sound.ScreensaverImagePaths,
            IsPremium = sound.IsPremium,
            IapIds = [.. sound.IapIds],
            IsMix = sound.IsMix,
            ColourHex = sound.ColourHex,
            ImagePaths = sound.ImagePaths,
            Localizations = sound.Localizations,
            MetaDataVersion = sound.MetaDataVersion,
            FileVersion = sound.FileVersion,
            AssociatedVideoIds = [.. sound.AssociatedVideoIds],
            Tags = [.. sound.Tags],
            CategoryIds = sound.CategoryIds is null ? null : [.. sound.CategoryIds],
            SoundIds = [.. sound.SoundIds],
            UploadedBy = sound.UploadedBy,
            UploadUsername = sound.UploadUsername,
            SponsorLinks = [.. sound.SponsorLinks],
            PublishState = sound.PublishState,
            SortPosition = sound.SortPosition,
        };
    }

    public static Guide DeepCopy(this Guide guide)
    {
        return new Guide
        {
            Id = guide.Id,
            Name = guide.Name,
            Description = guide.Description,
            MinutesLength = guide.MinutesLength,
            Culture = guide.Culture,
            SuggestedBackgroundSounds = [..guide.SuggestedBackgroundSounds],
            Localizations = guide.Localizations,
            MetaDataVersion = guide.MetaDataVersion,
            FileVersion = guide.FileVersion,
            IapIds = [.. guide.IapIds],
            IsPremium = guide.IsPremium,
            ImagePath = guide.ImagePath,
            IsDownloaded = guide.IsDownloaded,
            DownloadUrl = guide.DownloadUrl,
            FilePath = guide.FilePath,
            Extension = guide.Extension,
            ColourHex = guide.ColourHex,
            CategoryIds = guide.CategoryIds,
        };
    }

    public static Video DeepCopy(this Video video)
    {
        return new Video
        {
            Id = video.Id,
            FilePath = video.FilePath,
            Extension = video.Extension,
            Name = video.Name,
            IapIds = [.. video.IapIds],
            IsPremium = video.IsPremium,
            MegaByteSize = video.MegaByteSize,
            IsDownloaded = video.IsDownloaded,
            DownloadUrl = video.DownloadUrl,
        };
    }
}
