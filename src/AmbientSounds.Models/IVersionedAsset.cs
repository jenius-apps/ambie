namespace AmbientSounds.Models;

public interface IVersionedAsset : IAsset
{
    string Id { get; set; }

    /// <summary>
    /// Version of this data file.
    /// </summary>
    int MetaDataVersion { get; set; }

    /// <summary>
    /// Version of the asset file.
    /// </summary>
    int FileVersion { get; set; }

    /// <summary>
    /// Path to image file.
    /// </summary>
    string ImagePath { get; set; }
}
