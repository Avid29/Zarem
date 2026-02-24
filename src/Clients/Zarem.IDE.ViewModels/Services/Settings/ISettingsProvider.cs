// Avishai Dernis 2025

using System;
using System.Numerics;

namespace Zarem.Services.Settings;

/// <summary>
/// An interface for a settings collection.
/// </summary>
public interface ISettingsProvider
{
    /// <summary>
    /// Reads a value from the current <see cref="ISettingsProvider"/> instance and returns its casting in the right type.
    /// </summary>
    /// <typeparam name="T">The type of the object to retrieve.</typeparam>
    /// <param name="key">The key associated to the requested object.</param>
    /// <param name="fallback">Whether or not to return a fallback value on failure.</param>
    /// <returns>The value for the settings key.</returns>
    T? GetValue<T>(string key, bool fallback = true);

    /// <summary>
    /// Reads a value from the current <see cref="ISettingsProvider"/> instance and returns its casting in the right type.
    /// </summary>
    /// <typeparam name="T">The type of the object to retrieve.</typeparam>
    /// <param name="key">The key associated to the requested object.</param>
    /// <param name="value">The value found if any.</param>
    /// <returns>Whether or not a value was found.</returns>
    bool TryGetValue<T>(string key, out T? value);

    /// <summary>
    /// Assigns a value to a settings key.
    /// </summary>
    /// <typeparam name="T">The type of the object bound to the key.</typeparam>
    /// <param name="key">The key to check.</param>
    /// <param name="value">The value to assign to the setting key.</param>
    /// <param name="overwrite">Indicates whether or not to overwrite the setting, if it already exists.</param>
    /// <param name="notify">Indicates whether or not to notify the app after the setting has changed.</param>
    /// <returns>Whether or not the settings value changed.</returns>
    bool SetValue<T>(string key, T value, bool overwrite = true, bool notify = false);
}
