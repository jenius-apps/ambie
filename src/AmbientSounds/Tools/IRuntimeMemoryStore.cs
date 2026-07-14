namespace AmbientSounds.Tools;

/// <summary>
/// Tool that stores data during runtime.
/// </summary>
public interface IRuntimeMemoryStore
{
    /// <summary>
    /// Retrieves the data based on the given key.
    /// </summary>
    T? Get<T>(string key);

    /// <summary>
    /// Sets the data based on the given key.
    /// </summary>
    void Set<T>(string key, T value);
}