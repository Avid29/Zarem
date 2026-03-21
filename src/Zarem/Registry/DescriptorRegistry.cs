// Avishai Dernis 2026

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Zarem.Descriptors.Base;

namespace Zarem.Registry;

/// <summary>
/// A class for implementing a registry for a given <see cref="IDescriptor"/>.
/// </summary>
public abstract class DescriptorRegistry<T>
    where T : class, IDescriptor
{
    private readonly ConcurrentDictionary<string, T> _idTable = [];

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
    public abstract T? Get(Type type);

    /// <summary>
    /// Registers the descriptor in the look up table.
    /// </summary>
    public virtual void Register(T descriptor)
    {
        _idTable.TryAdd(descriptor.Identifier, descriptor);
    }

    /// <summary>
    /// Gets a <see cref="IEnumerable{T}"/> of the descriptors in the regsitry.
    /// </summary>
    public IEnumerable<T> GetDescriptors() => _idTable.Values;
}
