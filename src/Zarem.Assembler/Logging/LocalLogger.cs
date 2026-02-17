// Avishai Dernis 2026

using System.Reflection;
using Zarem.Assembler.Logging.Interfaces;
using Zarem.Localization;

namespace Zarem.Assembler.Logging;

/// <summary>
/// A base class for a logger specific to an assembly
/// </summary>
public class LocalLogger
{
    /// <summary>
    /// Initializes a new instance of the <see cref="LocalLogger"/> class.
    /// </summary>
    public LocalLogger(ILogger parent, string @namespace, Assembly assembly)
    {
        Localizer = new Localizer(@namespace, assembly);
        Parent = parent;
    }

    /// <summary>
    /// Gets the parent logger.
    /// </summary>
    public ILogger Parent { get; }

    /// <summary>
    /// Gets the localizer for the logger.
    /// </summary>
    protected Localizer Localizer { get; }
}
