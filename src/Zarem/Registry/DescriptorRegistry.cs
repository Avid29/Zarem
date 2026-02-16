// Avishai Dernis 2026

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Zarem.Descriptors;

namespace Zarem.Registry;

/// <summary>
/// A class for implementing a registry for a given descriptor.
/// </summary>
public class DescriptorRegistry<T>
    where T : class, IDescriptor
{
    private readonly ConcurrentDictionary<string, T> _idTable = [];
    private readonly ConcurrentDictionary<Type, T> _configTable = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorRegistry{T}"/> class.
    /// </summary>
    public DescriptorRegistry()
    {
    }

    /// <summary>
    /// Retrieves a descriptor from the registry.
    /// </summary>
    /// <param name="identifier">The identifier for the descriptor type.</param>
    public T? Get(string identifier)
    {
        if (_idTable.TryGetValue(identifier, out var value))
            return value;

        return null;
    }

    /// <summary>
    /// Retrieves a descriptor from the registry.
    /// </summary>
    /// <param name="configType">The type for for the config associated with the descriptor.</param>
    public T? Get(Type configType)
    {
        if (_configTable.TryGetValue(configType, out var value))
            return value;

        return null;
    }

    /// <summary>
    /// Registers the descriptor in the look up table.
    /// </summary>
    /// <param name="descriptor"></param>
    public void Register(T descriptor)
    {
        _idTable.TryAdd(descriptor.Identifier, descriptor);
        _configTable.TryAdd(descriptor.ConfigType, descriptor);
    }

    /// <summary>
    /// Gets a <see cref="IEnumerable{T}"/> of the descriptors in the regsitry.
    /// </summary>
    public IEnumerable<T> GetDescriptors() => _idTable.Values;
}
