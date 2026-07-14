using System.Collections.Concurrent;

namespace AmbientSounds.Tools;

public sealed class RuntimeMemoryStore : IRuntimeMemoryStore
{
    private readonly ConcurrentDictionary<string, object?> _store = new();

    /// <inheritdoc/>
    public T? Get<T>(string key)
    {
        try
        {
            if (_store.TryGetValue(key, out object? value) && value is not null)
            {
                return (T)value;
            }
        }
        catch { }

        return default;
    }

    /// <inheritdoc/>
    public void Set<T>(string key, T value)
    {
        if (_store.ContainsKey(key))
        {
            _store[key] = value;
        }
        else
        {
            _ = _store.TryAdd(key, value);
        }
    }
}
