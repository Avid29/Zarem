// Avishai Dernis 2025

using CommunityToolkit.Diagnostics;
using CommunityToolkit.Mvvm.Messaging;
using Zarem.Messages;
using Zarem.Services.Settings;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Text.Json;
using Windows.Foundation.Collections;

namespace Zarem.IDE.Services.Settings;

/// <summary>
/// An implementation of the <see cref="ISettingsProvider"/> that handles storing settings in a given folder.
/// </summary>
public class SettingsProvider : ISettingsProvider
{
    private readonly IMessenger _messenger;
    private readonly IPropertySet _storage;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsProvider"/> class.
    /// </summary>
    public SettingsProvider(IMessenger messenger, IPropertySet storage)
    {
        _messenger = messenger;
        _storage = storage;
    }

    /// <inheritdoc/>
    public T? GetValue<T>(string key, bool fallback = true)
    {
        if (TryGetValue(key, out T? value))
            return value;

        if (fallback)
            return default;

        return ThrowHelper.ThrowInvalidOperationException<T?>($"The setting {key} was requested, but not found.");
    }

    public bool TryGetValue<T>(string key, out T? value)
    {
        bool status = _storage.TryGetValue(key, out var raw);

        // Attempt deserialization
        var type = typeof(T);
        value = type switch
        {
            _ when type.IsEnum || type.IsPrimitive || type == typeof(string) => (T?)(raw ?? default(T)),
            _ when type.IsClass => (string?)raw is not null ? JsonSerializer.Deserialize<T>((string?)raw ?? string.Empty) : default,
            _ => ThrowHelper.ThrowArgumentException<T?>(nameof(raw)),
        };

        return status;
    }

    /// <inheritdoc/>
    public bool SetValue<T>(string key, T value, bool overwrite = true, bool notify = false)
    {
        // Attempt serialization
        var type = typeof(T);
        var serializable = type switch
        {
            _ when type.IsEnum => Convert.ChangeType(value, Enum.GetUnderlyingType(type)),
            _ when type.IsPrimitive || type == typeof(string) => value,
            _ when type.IsClass => JsonSerializer.Serialize(value),
            _ => ThrowHelper.ThrowArgumentException<object?>(nameof(value)),
        };

        // Get old value if notifying
        T? old = default;
        if (notify)
        {
            TryGetValue(key, out old);
        }

        // Save new value
        if (!_storage.ContainsKey(key))
        {
            _storage.Add(key, serializable);
        }
        else if (overwrite)
        {
            _storage[key] = serializable;
        }

        // Send an update message
        if (notify)
        {
            _messenger.Send(new SettingChangedMessage<T>(key, value, old));
        }

        if (value is null)
            return false;

        return !value.Equals(old);
    }
}
