namespace AmbientSounds.Services;

public interface ISoundVolumeService
{
    /// <summary>
    /// Retrieves stored volume for the sound.
    /// </summary>
    /// <param name="soundId">The sound's ID.</param>
    /// <param name="mixId">If provided, retrieves the sound's volume within the mix.</param>
    /// <returns>The sound's stored volume, min 0 and max 100.</returns>
    double GetVolume(string soundId, string? mixId = null);

    /// <summary>
    /// Saves the volume.
    /// </summary>
    /// <param name="volume">The sound's stored volume, min 0 and max 100.</param>
    /// <param name="soundId">The sound's ID.</param>
    /// <param name="mixId">If provided, stores the sound's volume within the mix.</param>
    void SetVolume(double volume, string soundId, string? mixId = null);
}