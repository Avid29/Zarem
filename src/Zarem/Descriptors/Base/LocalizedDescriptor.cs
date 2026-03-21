// Avishai Dernis 2026

using System;
using Zarem.Localization;

namespace Zarem.Descriptors.Base;

/// <summary>
/// A base class for a <see cref="IConfigDescriptor"/> which implements a lazy localizer.
/// </summary>
public abstract class LocalizedDescriptor<TSelf> : IConfigDescriptor
    where TSelf : LocalizedDescriptor<TSelf>
{
    private readonly Lazy<Localizer> _localizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="LocalizedDescriptor{TSelf}"/> class.
    /// </summary>
    protected LocalizedDescriptor()
    {
        _localizer = new Lazy<Localizer>(() => new Localizer(ResourceNamespace, typeof(TSelf).Assembly));
    }

    /// <inheritdoc/>
    public abstract string Identifier { get; }

    /// <summary>
    /// Gets the namespace would the resources are located.
    /// </summary>
    protected abstract string ResourceNamespace { get; }

    /// <summary>
    /// Gets the descriptor's localizer.
    /// </summary>
    protected Localizer Localizer => _localizer.Value;

    /// <inheritdoc/>
    public abstract Type ConfigType { get; }

}
