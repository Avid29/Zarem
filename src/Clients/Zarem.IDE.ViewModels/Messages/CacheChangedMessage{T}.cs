// Avishai Dernis 2025

namespace Zarem.Messages;

/// <summary>
/// A message sent when a cache value changes.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class CacheChangedMessage<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CacheChangedMessage{T}"/> class.
    /// </summary>
    public CacheChangedMessage(string key, T newValue, T? oldValue = default)
    { 
        Key = key;
        Value = newValue;
    }

    /// <summary>
    /// Gets the key of the changed cache.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the new value of the cache.
    /// </summary>
    public T Value { get; }
}
