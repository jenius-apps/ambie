using AmbientSounds.Models;
using System.Linq;

namespace AmbientSounds.Extensions;

public static class GuideExtensions
{
    public static Guide DeepCopy(this Guide guide)
    {
        return new Guide
        {
            Id = guide.Id,
            Name = guide.Name,
            Description = guide.Description,
            MinutesLength = guide.MinutesLength,
            Culture = guide.Culture,
            SuggestedBackgroundSounds = guide.SuggestedBackgroundSounds.ToArray(),
            Localizations = guide.Localizations,
            MetaDataVersion = guide.MetaDataVersion,
            FileVersion = guide.FileVersion,
            IapIds = guide.IapIds.ToArray(),
            IsPremium = guide.IsPremium,
            ImagePath = guide.ImagePath,
            IsDownloaded = guide.IsDownloaded,
            DownloadUrl = guide.DownloadUrl,
            FilePath = guide.FilePath,
            Extension = guide.Extension,
            ColourHex = guide.ColourHex,
        };
    }
}
