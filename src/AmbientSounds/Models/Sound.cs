namespace AmbientSounds.Models
{
    /// <summary>
    /// A sound object.
    /// </summary>
    /// <param name="Id">Unique Id for this sound (generally used to retrieve translations for this sound's name).</param>
    /// <param name="ImagePath">The full path for the associated image for this sound.</param>
    /// <param name="Name">The name of the sound.</param>
    /// <param name="FilePath">The full path for the sound file.</param>
    /// <param name="Attribution">Information about where the sound is from and what is its license.</param>
    public sealed record Sound(
        string Id,
        string ImagePath,
        string Name,
        string FilePath,
        string Attribution);
}
