// Avishai Dernis 2025

namespace Zarem.Messages;

/// <summary>
/// A message sent when a setting value changes.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
public class SettingChangedMessage<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SettingChangedMessage{T}"/> class.
    /// </summary>
    public SettingChangedMessage(string key, T newValue, T? oldValue = default)
    { 
        Key = key;
        NewValue = newValue;
        OldValue = oldValue;
    }

    /// <summary>
    /// Gets the key of the changed setting.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the new value of the setting.
    /// </summary>
    public T NewValue { get; }

    /// <summary>
    /// Gets the old value of the setting.
    /// </summary>
    public T? OldValue { get; }
}
