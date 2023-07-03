namespace AmbientSounds.Models;

public interface IVersionedAsset : IAsset
{
    public string Id { get; set; }

    /// <summary>
    /// Version of this data file.
    /// </summary>
    public int MetaDataVersion { get; set; }

    /// <summary>
    /// Version of the asset file.
    /// </summary>
    public int FileVersion { get; set; }

    /// <summary>
    /// Path to image file.
    /// </summary>
    public string ImagePath { get; set; }
}
