// Avishai Dernis 2026

using System;
using System.Collections.Concurrent;
using Zarem.Descriptors.Base;

namespace Zarem.Registry;

/// <summary>
/// A class for implementing a registry for a given <see cref="IConfigDescriptor"/>.
/// </summary>
public class TypeDescriptorRegistry<T> : DescriptorRegistry<T>
    where T : class, ITypeDescriptor
{
    private readonly ConcurrentDictionary<Type, T> _typeTable = [];

    /// <inheritdoc/>
    public override T? Get(Type configType)
    {
        if (_typeTable.TryGetValue(configType, out var value))
            return value;

        return null;
    }

    /// <summary>
    /// Registers the descriptor in the look up table.
    /// </summary>
    public override void Register(T descriptor)
    {
        base.Register(descriptor);
        _typeTable.TryAdd(descriptor.Type, descriptor);
    }
}
