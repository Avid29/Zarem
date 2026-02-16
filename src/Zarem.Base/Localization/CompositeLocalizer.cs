// Avishai Dernis 2026

using System.Collections.Generic;

namespace Zarem.Localization;

/// <summary>
/// An <see cref="IStringLocalizer"/> that searches through multiple <see cref="IStringLocalizer"/>s to find the appropriate key.
/// </summary>
public class CompositeLocalizer : IStringLocalizer
{
    private readonly List<IStringLocalizer> _localizers;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeLocalizer"/> class.
    /// </summary>
    public CompositeLocalizer()
    {
        _localizers = [];
    }

    /// <inheritdoc/>
    public string? Namespace => null;

    /// <inheritdoc/>
    public string? this[string key, params object?[] args]
    {
        get
        {
            foreach (var l in _localizers)
            {
                var result = l[key];
                if (result is not null)
                {
                    return string.Format(result, args);
                }
            }

            return null;
        }
    }

    /// <summary>
    /// Registers a new potential string source.
    /// </summary>
    public void Register(IStringLocalizer localizer)
    {
        _localizers.Add(localizer);
    }
}
