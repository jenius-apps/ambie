namespace AmbientSounds.Models;

/// <summary>
/// Specifies the reason an asset is updated.
/// </summary>
public enum UpdateReason
{
    None,

    /// <summary>
    /// Only the metadata is updated.
    /// </summary>
    MetaData,

    /// <summary>
    /// Only the file is updated.
    /// </summary>
    File,

    /// <summary>
    /// Both the metadata and file is updated.
    /// </summary>
    MetaDataAndFile
}
